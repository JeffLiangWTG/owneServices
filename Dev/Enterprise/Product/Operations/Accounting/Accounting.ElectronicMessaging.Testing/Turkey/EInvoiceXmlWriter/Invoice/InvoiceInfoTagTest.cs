using System;
using System.Linq;
using System.Xml;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.DataTransfer.Universal;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.ElectronicMessaging.efatura.uyumsoft.com.tr;
using Enterprise.Accounting.Export.Business;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.MasterFiles.Business.CountryCompliance.TurkeyComplianceInfo;
using static Enterprise.MasterFiles.Business.TurkeyOrgCusCodeInfo;

namespace Enterprise.Accounting.ElectronicMessaging.Turkey.Testing
{
	public class InvoiceInfoTagTest : TestCaseWithFactory
	{
		readonly TurkeyEInvoiceTestHelper Helper = new TurkeyEInvoiceTestHelper();

		[TestDate(2020, 1, 29, 23, 8, 32)]
		public void TestInvoiceInfo_EIN() => TestInvoiceInfo(ComplianceSubTypeCodes.EIN, "TEMELFATURA");

		[TestDate(2020, 1, 29, 23, 8, 32)]
		public void TestInvoiceInfo_EAR() => TestInvoiceInfo(ComplianceSubTypeCodes.EAR, "EARSIVFATURA");

		void TestInvoiceInfo(string complianceSubType, string profileID)
		{
			using (TestObjectCreator.SetTemporaryControlAccounts())
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Turkey))
			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, Helper.TurkeyBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				var arInvoice = Helper.CreateARInvoiceWithShipment(Helper.TestObjectCreator.CreditorTR, Helper.TestObjectCreator.EUR, "AR001", complianceSubType);
				var invoicingBatch = Helper.CreateInvoiceBatch(arInvoice);
				var exporter = new TransactionBatchExporter(DataAccess, PopulateOptionalXUTFieldsSetting.AllTrue());
				using (var transactionBatch = exporter.CreateTransactionBatch(invoicingBatch, SchemaVersionManager.Current.Namespace))
				{
					var eInvoice = Helper.EInvoiceInitializer(transactionBatch, true);

					AssertEquals("ABC2020000000001", eInvoice.LocalDocumentId);
					AssertEquals(efatura.uyumsoft.com.tr.InvoiceScenarioChoosen.Automated, eInvoice.Scenario);
					if (complianceSubType == ComplianceSubTypeCodes.EIN)
					{
						AssertNotNull(eInvoice.TargetCustomer);
						AssertEquals("12345678901", eInvoice.TargetCustomer.VknTckn);
						AssertEquals("urn:mail:debtor@testmailaddress.com", eInvoice.TargetCustomer.Alias);
						AssertEquals("Istanbul Debtor Test Company", eInvoice.TargetCustomer.Title);
						AssertNull(eInvoice.EArchiveInvoiceInfo);
						AssertNull(eInvoice.Notification);
					}
					else
					{
						AssertNull(eInvoice.TargetCustomer);
						AssertEquals(efatura.uyumsoft.com.tr.InvoiceDeliveryType.Paper, eInvoice.EArchiveInvoiceInfo.DeliveryType);
						AssertNotNull(eInvoice.Notification);
						AssertNotNull(eInvoice.Notification.Mailing);
						var mailingInformation = eInvoice.Notification.Mailing.ToList();
						AssertEquals(1, mailingInformation.Count);
						AssertEquals("debtor@testmailaddress.com", mailingInformation[0].To);
						AssertEquals(true, mailingInformation[0].EnableNotification);
						AssertEquals("e-Fatura Bilgileri", mailingInformation[0].Subject);
						AssertNotNull(mailingInformation[0].Attachment);
						var attachment = mailingInformation[0].Attachment;
						AssertEquals(false, attachment.Html);
						AssertEquals(true, attachment.Pdf);
						AssertEquals(true, attachment.Xml);
					}

					var invoiceNode = eInvoice.Invoice;
					AssertEquals("2.1", invoiceNode.UBLVersionID.Value);
					AssertEquals("TR1.2", invoiceNode.CustomizationID.Value);
					AssertEquals(profileID, invoiceNode.ProfileID.Value);
					AssertEquals("ABC2020000000001", invoiceNode.ID.Value);
					AssertEquals(false, invoiceNode.CopyIndicator.Value);
					AssertEquals("", invoiceNode.UUID.Value);  // In new invoice always is empty.
					AssertEquals(Convert.ToDateTime("29/01/2020"), invoiceNode.IssueDate.Value);
					AssertNull("We do not populate IssueTime due to an added time zone offset depending on current computer", invoiceNode.IssueTime);
					AssertEquals("ISTISNA", invoiceNode.InvoiceTypeCode.Value);
					AssertEquals("EUR", invoiceNode.DocumentCurrencyCode.Value);
					AssertEquals("Satış", invoiceNode.AccountingCost.Value); // Turkish Language
					AssertEquals(3m, invoiceNode.LineCountNumeric.Value);
					AssertEquals("00001000", invoiceNode.AdditionalDocumentReference[0].ID.Value);
				}
			}
		}

		[TestDate(2023, 11, 01, 23, 8, 32)]
		public void TestInvoiceReferenceDataNodeBehaviourForOrderReference_HasOrderItems_NoAttachecOrders() =>
			AssertInvoiceReferenceDataNodeBehaviourForOrderRefereceValue(expectedOrders: "ABC0001,DBC0002,EDB0003", expectedOrderDate: Convert.ToDateTime("01/11/2023 00:00:00.0000000"), setOrderItems: true, attachOrder: false);

		[TestDate(2023, 11, 01, 23, 8, 32)]
		public void TestInvoiceReferenceDataNodeBehaviourForOrderReference_HasOrderItems_SingleOrder() =>
			AssertInvoiceReferenceDataNodeBehaviourForOrderRefereceValue(expectedOrders: "ABC0001", expectedOrderDate: Convert.ToDateTime("01/11/2023 00:00:00.0000000"), setOrderItems: true, orderItems: "ABC0001", attachOrder: false);

		[TestDate(2023, 11, 01, 23, 8, 32)]
		public void TestInvoiceReferenceDataNodeBehaviourForOrderReference_HasOrderItems_HasAttachedOrder() =>
			AssertInvoiceReferenceDataNodeBehaviourForOrderRefereceValue(expectedOrders: "ABC0001,DBC0002,EDB0003,ORDER001", expectedOrderDate: Convert.ToDateTime("01/11/2023 00:00:00.0000000"), setOrderItems: true, attachOrder: true);

		[TestDate(2023, 11, 01, 23, 8, 32)]
		public void TestInvoiceReferenceDataNodeBehaviourForOrderReference_NoOrderItems_HasAttachedOrder() =>
			AssertInvoiceReferenceDataNodeBehaviourForOrderRefereceValue(expectedOrders: "ORDER001", expectedOrderDate: Convert.ToDateTime("01/10/2023 00:00:00.0000000"), setOrderItems: false, attachOrder: true);

		[TestDate(2023, 11, 01, 23, 8, 32)]
		public void TestInvoiceReferenceDataNodeBehaviourForOrderReference_NoOrderItems_NoAttachedOrder() =>
			AssertInvoiceReferenceDataNodeBehaviourForOrderRefereceValue(expectedOrders: "", expectedOrderDate: new DateTime());

		void AssertInvoiceReferenceDataNodeBehaviourForOrderRefereceValue(string expectedOrders, DateTime expectedOrderDate, bool setOrderItems = false, string orderItems = "ABC0001,DBC0002,EDB0003", bool attachOrder = false)
		{
			string complianceSubType = ComplianceSubTypeCodes.EIN;

			using (TestObjectCreator.SetTemporaryControlAccounts())
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Turkey))
			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, Helper.TurkeyBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				var arInvoice = Helper.CreateARInvoiceWithShipment(Helper.TestObjectCreator.CreditorTR, Helper.TestObjectCreator.EUR, "AR001", complianceSubType, setOrderItems: setOrderItems, orderItems: orderItems, attachOrder: attachOrder);
				var invoicingBatch = Helper.CreateInvoiceBatch(arInvoice);
				var exporter = new TransactionBatchExporter(DataAccess, PopulateOptionalXUTFieldsSetting.AllTrue());
				using (var transactionBatch = exporter.CreateTransactionBatch(invoicingBatch, SchemaVersionManager.Current.Namespace))
				{
					var eInvoice = Helper.EInvoiceInitializer(transactionBatch, true, false, true, "", null);
					var invoiceNode = eInvoice.Invoice;

					AssertNotNull(invoiceNode);
					AssertEquals(!string.IsNullOrEmpty(expectedOrders), invoiceNode.OrderReference != null);
					if (invoiceNode.OrderReference != null)
					{
						AssertEquals(expectedOrders, invoiceNode.OrderReference.ID.Value);
						AssertEquals(expectedOrderDate, invoiceNode.OrderReference.IssueDate.Value);
					}
				}
			}
		}

		[TestDate(2020, 1, 29, 23, 8, 32)]
		public void TestARInvoiceExemptionInvoiceInfoTag()
		{
			using (TestObjectCreator.SetTemporaryControlAccounts())
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Turkey))
			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, Helper.TurkeyBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				var invoicingBatch = Helper.CreateInvoiceWithExemption(Helper.TestObjectCreator.TRY);
				var exporter = new TransactionBatchExporter(DataAccess, PopulateOptionalXUTFieldsSetting.AllTrue());
				AssertInvoiceInfoTag(exporter, invoicingBatch, "Test Company Name", EInvoiceProfileTypes.TEMELFATURA, EInvoiceInfoTypes.TaxExemption, Helper.TestObjectCreator.TRY.Code, 1m);
			}
		}

		[TestDate(2020, 1, 29, 23, 8, 32)]
		public void TestARInvoiceWithWithholdingInvoiceInfoTag()
		{
			using (TestObjectCreator.SetTemporaryControlAccounts())
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Turkey))
			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, Helper.TurkeyBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				var arInvoice = Helper.CreateForeignCurrencyInvoiceWithWithholding(Helper.TestObjectCreator.EUR, 1.5m);
				var invoicingBatch = Helper.CreateInvoiceBatch(arInvoice);
				var exporter = new TransactionBatchExporter(DataAccess, PopulateOptionalXUTFieldsSetting.AllTrue());
				AssertInvoiceInfoTag(exporter, invoicingBatch, "Test Company Name", EInvoiceProfileTypes.TEMELFATURA, EInvoiceInfoTypes.WithHoldingTax, Helper.TestObjectCreator.EUR.Code, 1m);
			}
		}

		[TestDate(2020, 1, 29, 23, 8, 32)]
		public void TestARInvoiceWithExemptionInvoiceInfoTag()
		{
			using (TestObjectCreator.SetTemporaryControlAccounts())
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Turkey))
			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, Helper.TurkeyBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				var arInvoice = Helper.CreateForeignCurrencyInvoiceWithExemption(Helper.TestObjectCreator.EUR, 1.5m);
				var invoicingBatch = Helper.CreateInvoiceBatch(arInvoice);
				var exporter = new TransactionBatchExporter(DataAccess, PopulateOptionalXUTFieldsSetting.AllTrue());
				AssertInvoiceInfoTag(exporter, invoicingBatch, "Test Company Name", EInvoiceProfileTypes.TEMELFATURA, EInvoiceInfoTypes.TaxExemption, Helper.TestObjectCreator.EUR.Code, 1m);
			}
		}

		[TestDate(2020, 1, 29, 23, 8, 32)]
		public void TestARInvoiceSatisInvoiceInfoTag_EIN() => TestARInvoiceSatisInvoiceInfoTag(ComplianceSubTypeCodes.EIN, debtorOrgCusCode: null, EInvoiceProfileTypes.TEMELFATURA);

		[TestDate(2020, 1, 29, 23, 8, 32)]
		public void TestARInvoiceSatisInvoiceInfoTag_EAR() => TestARInvoiceSatisInvoiceInfoTag(ComplianceSubTypeCodes.EAR, debtorOrgCusCode: null, EInvoiceProfileTypes.EARSIVFATURA);

		[TestDate(2020, 1, 29, 23, 8, 32)]
		public void TestARInvoiceReturnInvoiceInfoTag_CCN_Basic() => TestARInvoiceSatisInvoiceInfoTag(ComplianceSubTypeCodes.CCN, OrgCusCodes.VTE, EInvoiceProfileTypes.TEMELFATURA);

		[TestDate(2020, 1, 29, 23, 8, 32)]
		public void TestARInvoiceReturnInvoiceInfoTag_CCN_Commercial() => TestARInvoiceSatisInvoiceInfoTag(ComplianceSubTypeCodes.CCN, OrgCusCodes.VTC, EInvoiceProfileTypes.TICARIFATURA);

		[TestDate(2020, 1, 29, 23, 8, 32)]
		public void TestARInvoiceReturnInvoiceInfoTag_CCN_eArchive() => TestARInvoiceSatisInvoiceInfoTag(ComplianceSubTypeCodes.CCN, debtorOrgCusCode: null, EInvoiceProfileTypes.EARSIVFATURA);

		void TestARInvoiceSatisInvoiceInfoTag(string complianceSubType, string debtorOrgCusCode, string xmlEInvoiceType)
		{
			using (TestObjectCreator.SetTemporaryControlAccounts())
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Turkey))
			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, Helper.TurkeyBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				var arInvoice = Helper.CreateARInvoiceWithTax(Helper.TestObjectCreator.CreditorTR, Helper.TestObjectCreator.EUR, "AR001", complianceSubType, createVTE: (debtorOrgCusCode == OrgCusCodes.VTE), createVTC: (debtorOrgCusCode == OrgCusCodes.VTC));
				var invoicingBatch = Helper.CreateInvoiceBatch(arInvoice);
				var exporter = new TransactionBatchExporter(DataAccess, PopulateOptionalXUTFieldsSetting.AllTrue());
				AssertInvoiceInfoTag(exporter, invoicingBatch, "Istanbul Debtor Test Company", xmlEInvoiceType, EInvoiceInfoTypes.ARInvoice, Helper.TestObjectCreator.EUR.Code, 2m);
			}
		}

		[TestDate(2020, 1, 29, 23, 8, 32)]
		public void TestAPCreditNoteInfoTag_DIN() => TestAPCreditNoteInfoTag(ComplianceSubTypeCodes.DIN, debtorOrgCusCode: null, EInvoiceProfileTypes.TEMELFATURA);

		[TestDate(2020, 1, 29, 23, 8, 32)]
		public void TestAPCreditNoteInfoTag_DAR() => TestAPCreditNoteInfoTag(ComplianceSubTypeCodes.DAR, debtorOrgCusCode: null, EInvoiceProfileTypes.EARSIVFATURA);

		[TestDate(2020, 1, 29, 23, 8, 32)]
		public void TestAPCreditNoteInfoTag_DIN_Basic() => TestAPCreditNoteInfoTag(ComplianceSubTypeCodes.DIN, OrgCusCodes.VTE, EInvoiceProfileTypes.TEMELFATURA);

		[TestDate(2020, 1, 29, 23, 8, 32)]
		public void TestAPCreditNoteInfoTag_DIN_Commercial() => TestAPCreditNoteInfoTag(ComplianceSubTypeCodes.DIN, OrgCusCodes.VTC, EInvoiceProfileTypes.TEMELFATURA);

		[TestDate(2020, 1, 29, 23, 8, 32)]
		public void TestAPCreditNoteInfoTag_DAR_eArchive() => TestAPCreditNoteInfoTag(ComplianceSubTypeCodes.DAR, debtorOrgCusCode: null, EInvoiceProfileTypes.EARSIVFATURA);

		void TestAPCreditNoteInfoTag(string complianceSubType, string debtorOrgCusCode, string expectedXmlEInvoiceType)
		{
			using (TestObjectCreator.SetTemporaryControlAccounts())
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Turkey))
			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, Helper.TurkeyBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				var apCreditNote = Helper.CreateTestAPCreditNote(Helper.TestObjectCreator.CC14, "return", Helper.TestObjectCreator.KDV18.PK, "AP001", 99.9m, Helper.TestObjectCreator.TRY, complianceSubType, createVTE: (debtorOrgCusCode == OrgCusCodes.VTE), createVTC: (debtorOrgCusCode == OrgCusCodes.VTC));
				var invoicingBatch = Helper.CreateInvoiceBatch(apCreditNote);
				var exporter = new TransactionBatchExporter(DataAccess, PopulateOptionalXUTFieldsSetting.AllTrue());
				AssertInvoiceInfoTag(exporter, invoicingBatch, "Istanbul Debtor Test Company", expectedXmlEInvoiceType, EInvoiceInfoTypes.Return, Helper.TestObjectCreator.TRY.Code, 1m);
			}
		}

		void AssertInvoiceInfoTag(TransactionBatchExporter exporter, AccEInvoicingBatch invoicingBatch, string targetCustomerTitle, string profileType, string invoiceTypeTag, string currencyCode, decimal lineCount)
		{
			using (var transactionBatch = exporter.CreateTransactionBatch(invoicingBatch, SchemaVersionManager.Current.Namespace))
			{
				var eInvoice = Helper.EInvoiceInitializer(transactionBatch);

				AssertEquals("ABC2020000000001", eInvoice.LocalDocumentId);
				if (profileType == EInvoiceProfileTypes.EARSIVFATURA)
				{
					AssertNotNull("EArchiveInvoiceInfo", eInvoice.EArchiveInvoiceInfo);
					AssertEquals(InvoiceDeliveryType.Paper, eInvoice.EArchiveInvoiceInfo.DeliveryType);
				}
				else
				{
					AssertEquals(targetCustomerTitle, eInvoice.TargetCustomer.Title);
				}
				AssertEquals(InvoiceScenarioChoosen.Automated, eInvoice.Scenario);

				var invoiceNode = eInvoice.Invoice;
				AssertEquals("2.1", invoiceNode.UBLVersionID.Value);
				AssertEquals("TR1.2", invoiceNode.CustomizationID.Value);
				AssertEquals(profileType, invoiceNode.ProfileID.Value);
				AssertEquals(false, invoiceNode.CopyIndicator.Value);
				AssertEquals("", invoiceNode.UUID.Value);  // In new invoice always is empty.
				AssertEquals(Convert.ToDateTime("29/01/2020"), invoiceNode.IssueDate.Value);
				AssertNull("We do not populate IssueTime due to an added time zone offset depending on current computer", invoiceNode.IssueTime);
				AssertEquals(invoiceTypeTag, invoiceNode.InvoiceTypeCode.Value);
				AssertEquals(currencyCode, invoiceNode.DocumentCurrencyCode.Value);
				AssertEquals("Satış", invoiceNode.AccountingCost.Value); // Turkish Language
				AssertEquals(lineCount, invoiceNode.LineCountNumeric.Value);

				AssertBillingReference(invoiceNode);
			}
		}

		void AssertBillingReference(InvoiceType invoiceNode)
		{
			if (invoiceNode.BillingReference != null)
			{
				Assert(invoiceNode.BillingReference.Any());

				var billingReference = invoiceNode.BillingReference.First();
				AssertNotNull(billingReference.InvoiceDocumentReference);

				var document = billingReference.InvoiceDocumentReference;
				AssertNotNull(document.ID);
				AssertNotNull(document.IssueDate);
				AssertNotNull(document.DocumentType);
				AssertNotNull(document.DocumentTypeCode);

				AssertEquals("ORGAP001", document.ID.Value);
				AssertEquals(ZDateTime.Now.AddDays(-5).Date, document.IssueDate.Value);
				AssertEquals("FATURA", document.DocumentType.Value);
				AssertEquals("IADE", document.DocumentTypeCode.Value);
			}
		}

		public void TestVknTcknIsCorrectForTargetCustomer()
		{
			using (TestObjectCreator.SetTemporaryControlAccounts())
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Turkey))
			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, Helper.TurkeyBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				var arInvoice = Helper.CreateARInvoiceWithShipment(Helper.TestObjectCreator.CreditorTR, ComplianceSubTypeCodes.EIN);
				var invoicingBatch = Helper.CreateInvoiceBatch(arInvoice);
				var exporter = new TransactionBatchExporter(DataAccess, PopulateOptionalXUTFieldsSetting.AllTrue());
				using (var transactionBatch = exporter.CreateTransactionBatch(invoicingBatch, SchemaVersionManager.Current.Namespace))
				{
					Helper.TestObjectCreator.DebtorTR.OH_Category = OrgConstants.Category.Business;
					Helper.TestObjectCreator.DebtorTR.Factory.Save();
					var eInvoice = Helper.EInvoiceInitializer(transactionBatch);
					AssertEquals("34567890123", eInvoice.TargetCustomer.VknTckn);
					AssertEquals("urn:mail:debtor@testmailaddress.com", eInvoice.TargetCustomer.Alias);
					AssertEquals("Istanbul Debtor Test Company", eInvoice.TargetCustomer.Title);
				}

				var arInvoice1 = Helper.CreateARInvoiceWithShipment(Helper.TestObjectCreator.CreditorTR, "AR002", ComplianceSubTypeCodes.EIN, true, "C0002");
				var invoicingBatch1 = Helper.CreateInvoiceBatch(arInvoice1, 2);
				using (var transactionBatch = exporter.CreateTransactionBatch(invoicingBatch1, SchemaVersionManager.Current.Namespace))
				{
					Helper.TestObjectCreator.DebtorTR.OH_Category = OrgConstants.Category.NaturalPersonIndividual;
					Helper.TestObjectCreator.DebtorTR.Factory.Save();
					var eInvoice = Helper.EInvoiceInitializer(transactionBatch);
					AssertEquals("12345678901", eInvoice.TargetCustomer.VknTckn);
					AssertEquals("urn:mail:debtor@testmailaddress.com", eInvoice.TargetCustomer.Alias);
					AssertEquals("Istanbul Debtor Test Company", eInvoice.TargetCustomer.Title);
				}
			}
		}

		public void TestVknTcknIsCorrectForTargetCustomer_EmptyCustomCodes()
		{
			using (TestObjectCreator.SetTemporaryControlAccounts())
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Turkey))
			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, Helper.TurkeyBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				var arInvoice = Helper.CreateARInvoiceWithShipment(Helper.TestObjectCreator.CreditorTR, ComplianceSubTypeCodes.EIN, createDebtorCustomeCodes: false);
				var invoicingBatch = Helper.CreateInvoiceBatch(arInvoice);
				var exporter = new TransactionBatchExporter(DataAccess, PopulateOptionalXUTFieldsSetting.AllTrue());
				using (var transactionBatch = exporter.CreateTransactionBatch(invoicingBatch, SchemaVersionManager.Current.Namespace))
				{
					var eInvoice = Helper.EInvoiceInitializer(transactionBatch);
					AssertEquals(string.Empty, eInvoice.TargetCustomer.VknTckn);
					AssertEquals(string.Empty, eInvoice.TargetCustomer.Alias);
					AssertEquals("Istanbul Debtor Test Company", eInvoice.TargetCustomer.Title);
				}
			}
		}

		public void TestInvoiceOrderReference_XUTCustomsDeclaration()
		{
			var currency = Factory.LoadTop1<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, "USD"));
			var helper = new RefCurrencyTestHelper(Factory);
			helper.CreateRefLanguageText("RX_SubUnitName", currency.PK, "TR-TR", "RX", "sent");
			Factory.Save();
			var invoice = BuildInvoiceFromXUT(UniversalTransaction_CustomsDeclaration);

			AssertNotNull(invoice.OrderReference);
			AssertEquals("OrderRef12345", invoice.OrderReference.ID.Value);
			AssertEquals("17/03/2025 12:00:00 AM", invoice.OrderReference.IssueDate.Value.ToString());
		}

		string UniversalTransaction_CustomsDeclaration => TurkeyEInvoiceTestHelper.GetEmbeddedResourceAsString($"{nameof(UniversalTransaction_CustomsDeclaration)}.xml");

		InvoiceType BuildInvoiceFromXUT(string xutContent, bool createCommentCharge = false)
		{
			var currency = Factory.LoadTop1<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, "TRY"));
			var currencyHelper = new RefCurrencyTestHelper(Factory);
			currencyHelper.CreateRefLanguageText("RX_SubUnitName", currency.PK, "TR-TR", "RX", "kuruş");
			Factory.Save();
			using (TestObjectCreator.SetTemporaryControlAccounts())
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Turkey))
			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, Helper.TurkeyBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				if (createCommentCharge)
				{
					_ = Helper.TestObjectCreator.CommentChargeCode;
					Helper.Factory.Save();
				}

				var xmlDocumentFile = new XmlDocument();
				xmlDocumentFile.LoadXml(xutContent);
				var xutXmlAsText = xmlDocumentFile.OuterXml;
				var importer = new TransactionImporter();
				var transaction = importer.ImportUniversalTransactionFromXml(xutXmlAsText, Helper.Factory, false);
				var uInvoice = (TransactionInfo)transaction.Item1;
				uInvoice.TransactionReference = "TESTREFERENCE";
				var invoiceBatch = Helper.CreateInvoiceBatch(Helper.TestObjectCreator.CreateInvoice(typeof(ARInvoice), "INV001", Helper.TestObjectCreator.AUD, 1M, Helper.TestObjectCreator.ABIGAS));
				var exporter = new TransactionBatchExporter(DataAccess, PopulateOptionalXUTFieldsSetting.AllTrue());
				using (var transactionBatch = exporter.CreateTransactionBatch(invoiceBatch, SchemaVersionManager.Current.Namespace))
				{
					var eInvoice = Helper.SetTransaction(transactionBatch, uInvoice);
					return eInvoice.Invoice;
				}
			}
		}

		protected override void SetUp()
		{
			base.SetUp();

			var connection = ((IDbConnectionInternals)base.TestConnection).ADOConnection;
			var transaction = ((IDbConnectionInternals)base.TestConnection).ADOTransaction;
			DataAccess = new BatchExportDataAccess(connection, transaction);
		}

		BatchExportDataAccess DataAccess;
	}
}
