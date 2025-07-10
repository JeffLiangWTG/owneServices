using System;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.ElectronicMessaging.Common.Testing;
using Enterprise.Accounting.Export.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using UniversalTransactionBatch = Enterprise.UniversalDataBuss.DataObjects.Accounting.TransactionBatch;
using UniversalTransactionInfo = Enterprise.UniversalDataBuss.DataObjects.Accounting.TransactionInfo;

namespace Enterprise.Accounting.ElectronicMessaging.TaxCore.Testing
{
	public class TaxCoreEInvoiceCreatorTest : TestCaseWithFactory
	{
		[TestDate(2019, 7, 16, 15, 26, 32)]
		public void TestTaxCoreEInvoiceCreation_Invoice()
		{
			var branch = CreateBranch();
			using (var credentialCreator = new TestEInvoicingCertificateCredentialCreator(branch, ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(7)))
			{
				ObjectCreator.SetCustomsCodeForOrgHeader(ObjectCreator.Debtor, CountryFactory.TaxFileCode, CountryCode, "12345678798");
				var certificateCredential = credentialCreator.CreateCertificateCredential();

				AccEInvoicingBatch eInvoiceBatch = null;
				using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, branch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
				{
					var arInvoice = ObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "001", ObjectCreator.AUD, 1.0m, 100.00m, 10.00m, 100.00m, 10.00m, ObjectCreator.Debtor, ObjectCreator.CC1.PK);
					eInvoiceBatch = CreateEInvoicingBatch(arInvoice);
					Factory.Save();
				}

				var (eInvoice, notifications) = CreateTaxCoreEInvoice(eInvoiceBatch, certificateCredential, branch);
				AssertNotNull("EInvoice", eInvoice);
				AssertEquals("DateAndTimeOfIssue", "2019-07-16T15:26:00Z", eInvoice.UTCCreateTime);
				AssertEquals("Cashier", null, eInvoice.CashierTFN);
				AssertEquals("BD", "12345678798", eInvoice.TaxIdentificationNumber);
				AssertEquals("BuyerCostCenterId", null, eInvoice.BuyerCostCenterId);
				AssertEquals("InvoiceType", TaxCoreInvoiceTypes.Normal, eInvoice.InvoiceType);
				AssertEquals("TransactionType", TaxCoreTransactionTypes.Sale, eInvoice.TransactionType);
				AssertEquals("PaymentType", TaxCorePaymentTypes.Other, eInvoice.PaymentType);
				AssertEquals("PaymentType", "00001000", eInvoice.InvoiceNumber);
				AssertEquals("ReferentDocumentNumber", null, eInvoice.ReferentDocumentNumber);
				AssertEquals("ReferentDocumentDateAndTime", null, eInvoice.ReferentDocumentDateAndTime);
				AssertEquals("OmitQRCodeGen", TaxCoreOptionTypes.Omit, eInvoice.QROptions.OmitQRCodeGen);
				AssertEquals("OmitTextualRepresentation", TaxCoreOptionTypes.Omit, eInvoice.QROptions.OmitTextualRepresentation);
				AssertEquals("PAC", null, eInvoice.PAC);
				AssertEquals("Hash", "Uwo1p6U349/QntcBkIvaUg==", eInvoice.Hash);
				AssertEquals("Lines", 1, eInvoice.Lines.Count);
				AssertEquals("GTIN", null, eInvoice.Lines[0].GTIN);
				AssertEquals("Name", "Charge Code 1", eInvoice.Lines[0].Name);
				AssertEquals("Quantity", 1, eInvoice.Lines[0].Quantity);
				AssertEquals("Discount", 0M, eInvoice.Lines[0].Discount);
				AssertEquals("UnitPrice", 100M, eInvoice.Lines[0].UnitPrice);
				AssertEquals("Labels", null, eInvoice.Lines[0].Labels);
				AssertEquals("TotalAmount", 100M, eInvoice.Lines[0].TotalAmount);
			}
		}

		[TestDate(2019, 7, 16, 15, 26, 32)]
		public void TestTaxCoreEInvoiceCreation_CreditNote()
		{
			var branch = CreateBranch();
			using (var credentialCreator = new TestEInvoicingCertificateCredentialCreator(branch, ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(7)))
			{
				ObjectCreator.SetCustomsCodeForOrgHeader(ObjectCreator.Debtor, OrgCusCode.CodeTypes.TaxFileCode, CountryCode, "12345678798");
				var certificateCredential = credentialCreator.CreateCertificateCredential();

				AccEInvoicingBatch eInvoiceBatch = null;
				ARInvoice arInvoice = null;
				ARCreditNote arCreditNote = null;
				using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, branch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
				{
					arInvoice = (ARInvoice)ObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "001", ObjectCreator.AUD, 1.0m, 100.00m, 10.00m, 100.00m, 10.00m, ObjectCreator.Debtor, ObjectCreator.CC1.PK);
					arInvoice.AH_TransactionReference = "REF001";
					Factory.Save();

					var job = ObjectCreator.CreateJob(ObjectCreator.LocalClient, 1.0M, ObjectCreator.Agent, 1.0M);
					arCreditNote = ObjectCreator.CreateARCreditNoteWithLine("ARCRD001", ObjectCreator.Debtor, ObjectCreator.AUD, 1.0m, "Credit Note", job, ObjectCreator.CC1, 100.00m, ZDateTime.Today, false);
					arCreditNote.AH_TransactionBelongsToGroup = arInvoice.PK;
					(arCreditNote as IAmending).FlagAsCreatedAmending();
					eInvoiceBatch = CreateEInvoicingBatch(arCreditNote);
					Factory.Save();
				}

				var (eInvoice, notifications) = CreateTaxCoreEInvoice(eInvoiceBatch, certificateCredential, branch, createAdditionalTransactionInfo: () => new AdditionalTransactionInfoTaxCoreEInvoice() { OriginalTransactionGovtReferenceNumber = arInvoice.AH_TransactionReference, OriginalTransactionCreationDate = arInvoice.AH_SystemCreateTimeUtc });
				AssertNotNull("EInvoice", eInvoice);
				AssertEquals("DateAndTimeOfIssue", "2019-07-16T15:26:00Z", eInvoice.UTCCreateTime);
				AssertEquals("Cashier", null, eInvoice.CashierTFN);
				AssertEquals("BD", "12345678798", eInvoice.TaxIdentificationNumber);
				AssertEquals("BuyerCostCenterId", null, eInvoice.BuyerCostCenterId);
				AssertEquals("InvoiceType", TaxCoreInvoiceTypes.Normal, eInvoice.InvoiceType);
				AssertEquals("TransactionType", TaxCoreTransactionTypes.Refund, eInvoice.TransactionType);
				AssertEquals("PaymentType", TaxCorePaymentTypes.Other, eInvoice.PaymentType);
				AssertEquals("PaymentType", arCreditNote.AH_TransactionNum, eInvoice.InvoiceNumber);
				AssertEquals("ReferentDocumentNumber", arInvoice.AH_TransactionReference, eInvoice.ReferentDocumentNumber);
				AssertEquals("ReferentDocumentDateAndTime", "2019-07-16T15:26:32Z", eInvoice.ReferentDocumentDateAndTime);
				AssertEquals("OmitQRCodeGen", TaxCoreOptionTypes.Omit, eInvoice.QROptions.OmitQRCodeGen);
				AssertEquals("OmitTextualRepresentation", TaxCoreOptionTypes.Omit, eInvoice.QROptions.OmitTextualRepresentation);
				AssertEquals("PAC", null, eInvoice.PAC);
				AssertEquals("Hash", "QnoKKc88CuMLYn47sJMo6g==", eInvoice.Hash);
				AssertEquals("Lines", 1, eInvoice.Lines.Count);
				AssertEquals("GTIN", null, eInvoice.Lines[0].GTIN);
				AssertEquals("Name", "Credit Note", eInvoice.Lines[0].Name);
				AssertEquals("Quantity", 1, eInvoice.Lines[0].Quantity);
				AssertEquals("Discount", 0M, eInvoice.Lines[0].Discount);
				AssertEquals("UnitPrice", 100M, eInvoice.Lines[0].UnitPrice);
				AssertEquals("Labels", null, eInvoice.Lines[0].Labels);
				AssertEquals("TotalAmount", 100M, eInvoice.Lines[0].TotalAmount);
			}
		}

		[TestDate(2019, 7, 16, 15, 26, 32)]
		public void TestTaxCoreEInvoiceCreation_Properties()
		{
			var branch = CreateBranch();
			using (var credentialCreator = new TestEInvoicingCertificateCredentialCreator(branch, ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(7)))
			{
				//Data setup
				var taxMsg = ObjectCreator.CreateTaxMsg("TX", "TaxCore Demo Tax Msg", "TaxCore Demo Tax Msg", "TaxCore Demo Tax Msg");
				taxMsg.A9_RN_NKCountryCode = CountryCode;
				taxMsg.A9_TaxGroupCode = FijiComplianceInfo.TaxMessageGroupCodes.A;

				ObjectCreator.SetCustomsCodeForOrgHeader(ObjectCreator.Debtor, CountryFactory.TaxFileCode, CountryCode, "12345678798");

				var certificateCredential = credentialCreator.CreateCertificateCredential();
				certificateCredential.GP_MailBoxID = "A25963";

				var staff = Factory.Load<GlbStaff>(GlbStaff.CurrentUser.PK);
				var staffCertificate = staff.Certificates.AddNew();
				staffCertificate.XZ_Type = CertificateTypePairList.Codes.TFN;
				staffCertificate.XZ_RefNumber = "225369147";
				staffCertificate.XZ_ParentID = staff.PK;
				staffCertificate.XZ_ParentTableCode = GlbStaffSchema.Constants.Prefix;

				Factory.Save();

				//Transaction data creation
				AccEInvoicingBatch eInvoiceBatch = null;
				using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, branch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
				{
					var arInvoice = ObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "001", ObjectCreator.AUD, 1.0m, 100.00m, 10.00m, 100.00m, 10.00m, ObjectCreator.Debtor, ObjectCreator.CC1.PK);
					arInvoice.Lines[0].AL_A9_VATClass = taxMsg.PK;
					eInvoiceBatch = CreateEInvoicingBatch(arInvoice);
					Factory.Save();
				}

				var paymentMethodMapping = new[]
				{
					new { AgreedPaymentMethod = OrgConstants.CreditAgreedPaymentMethods.Code.CashAndBankCheck, TaxCoreEInvoicePaymentMethod = TaxCorePaymentTypes.Cash },
					new { AgreedPaymentMethod = OrgConstants.CreditAgreedPaymentMethods.Code.DebitCard, TaxCoreEInvoicePaymentMethod = TaxCorePaymentTypes.Card },
					new { AgreedPaymentMethod = OrgConstants.CreditAgreedPaymentMethods.Code.CreditCard, TaxCoreEInvoicePaymentMethod = TaxCorePaymentTypes.Card },
				};

				foreach (var pmMapping in paymentMethodMapping)
				{
					Action<UniversalTransactionInfo> modifyTransactionInfo = (transactionInfo) => transactionInfo.AgreedPaymentMethod = pmMapping.AgreedPaymentMethod;
					var (eInvoice, notifications) = CreateTaxCoreEInvoice(eInvoiceBatch, certificateCredential, branch, modifyTransactionInfo);
					AssertNotNull("EInvoice", eInvoice);
					AssertEquals("DateAndTimeOfIssue", "2019-07-16T15:26:00Z", eInvoice.UTCCreateTime);
					AssertEquals("Cashier", "225369147", eInvoice.CashierTFN);
					AssertEquals("BD", "12345678798", eInvoice.TaxIdentificationNumber);
					AssertEquals("BuyerCostCenterId", null, eInvoice.BuyerCostCenterId);
					AssertEquals("InvoiceType", TaxCoreInvoiceTypes.Normal, eInvoice.InvoiceType);
					AssertEquals("TransactionType", TaxCoreTransactionTypes.Sale, eInvoice.TransactionType);
					AssertEquals("PaymentType", pmMapping.TaxCoreEInvoicePaymentMethod, eInvoice.PaymentType);
					AssertEquals("PaymentType", "00001000", eInvoice.InvoiceNumber);
					AssertEquals("ReferentDocumentNumber", null, eInvoice.ReferentDocumentNumber);
					AssertEquals("ReferentDocumentDateAndTime", null, eInvoice.ReferentDocumentDateAndTime);
					AssertEquals("OmitQRCodeGen", TaxCoreOptionTypes.Omit, eInvoice.QROptions.OmitQRCodeGen);
					AssertEquals("OmitTextualRepresentation", TaxCoreOptionTypes.Omit, eInvoice.QROptions.OmitTextualRepresentation);
					AssertEquals("PAC", "A25963", eInvoice.PAC);
					AssertEquals("Lines", 1, eInvoice.Lines.Count);
					AssertEquals("GTIN", null, eInvoice.Lines[0].GTIN);
					AssertEquals("Name", "Charge Code 1", eInvoice.Lines[0].Name);
					AssertEquals("Quantity", 1, eInvoice.Lines[0].Quantity);
					AssertEquals("Discount", 0M, eInvoice.Lines[0].Discount);
					AssertEquals("UnitPrice", 100M, eInvoice.Lines[0].UnitPrice);
					AssertEquals("Labels", "A", eInvoice.Lines[0].Labels[0]);
					AssertEquals("TotalAmount", 100M, eInvoice.Lines[0].TotalAmount);
				}
			}
		}

		[TestDate(2019, 7, 16, 15, 26, 32)]
		public void TestTaxCoreEInvoiceCreation_Validation()
		{
			var branch = CreateBranch();
			using (var credentialCreator = new TestEInvoicingCertificateCredentialCreator(branch, ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(7)))
			{
				ObjectCreator.SetCustomsCodeForOrgHeader(ObjectCreator.Debtor, CountryFactory.TaxFileCode, CountryCode, "12345678798");
				var certificateCredential = credentialCreator.CreateCertificateCredential();

				AccEInvoicingBatch eInvoiceBatch = null;
				ARInvoice arInvoice = null;
				ARCreditNote arCreditNote = null;
				using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, branch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
				{
					arInvoice = (ARInvoice)ObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "001", ObjectCreator.AUD, 1.0m, 100.00m, 10.00m, 100.00m, 10.00m, ObjectCreator.Debtor, ObjectCreator.CC1.PK);
					arInvoice.AH_TransactionReference = "REF001";
					Factory.Save();

					var job = ObjectCreator.CreateJob(ObjectCreator.LocalClient, 1.0M, ObjectCreator.Agent, 1.0M);
					arCreditNote = ObjectCreator.CreateARCreditNoteWithLine("ARCRD001", ObjectCreator.Debtor, ObjectCreator.AUD, 1.0m, "Credit Note", job, ObjectCreator.CC1, 100.00m, ZDateTime.Today, false);
					arCreditNote.AH_TransactionBelongsToGroup = arInvoice.PK;
					(arCreditNote as IAmending).FlagAsCreatedAmending();
					eInvoiceBatch = CreateEInvoicingBatch(arCreditNote);
					Factory.Save();
				}

				var expectedErrorForMissingRefTransactionData = FormattableString.Invariant($"'Referent Document Number' or 'Referent Document Date' is missing for refund transaction {arCreditNote.AH_TransactionNum}");
				var expectedErrorForMissingLabel = FormattableString.Invariant($"{arCreditNote.AH_TransactionNum} has one or more line(s) with non zero tax amount but empty tax message");
				var validationErrorMapping = new[]
				{
					new { TransactionInfoModificationAction = new Action<UniversalTransactionInfo>((transactionInfo) => transactionInfo.PostingJournalCollection[0].OSGSTVATAmount = 25M), AdditionInfoCreationAction = new Func<IAdditionalTransactionInfoForTaxCoreEInvoice>(() => null), Error = expectedErrorForMissingLabel },
					new { TransactionInfoModificationAction = new Action<UniversalTransactionInfo>((transactionInfo) => transactionInfo.PostingJournalCollection[0].OSGSTVATAmount = 0M), AdditionInfoCreationAction = new Func<IAdditionalTransactionInfoForTaxCoreEInvoice>(() => new AdditionalTransactionInfoTaxCoreEInvoice() { OriginalTransactionGovtReferenceNumber = "REF001", OriginalTransactionCreationDate = ZDateTime.Today }), Error = string.Empty },
					new { TransactionInfoModificationAction = new Action<UniversalTransactionInfo>((transactionInfo) => { }), AdditionInfoCreationAction = new Func<IAdditionalTransactionInfoForTaxCoreEInvoice>(() => new AdditionalTransactionInfoTaxCoreEInvoice() { OriginalTransactionGovtReferenceNumber = "REF001", OriginalTransactionCreationDate = ZDateTime.Today }), Error = string.Empty },
					new { TransactionInfoModificationAction = new Action<UniversalTransactionInfo>((transactionInfo) => { }), AdditionInfoCreationAction = new Func<IAdditionalTransactionInfoForTaxCoreEInvoice>(() => new AdditionalTransactionInfoTaxCoreEInvoice() { OriginalTransactionGovtReferenceNumber = "", OriginalTransactionCreationDate = ZDateTime.Today }), Error = expectedErrorForMissingRefTransactionData },
					new { TransactionInfoModificationAction = new Action<UniversalTransactionInfo>((transactionInfo) => { }), AdditionInfoCreationAction = new Func<IAdditionalTransactionInfoForTaxCoreEInvoice>(() => new AdditionalTransactionInfoTaxCoreEInvoice() { OriginalTransactionGovtReferenceNumber = "REF0001", OriginalTransactionCreationDate = ZDateTime.Invalid }), Error = expectedErrorForMissingRefTransactionData }
				};

				foreach (var item in validationErrorMapping)
				{
					var (eInvoice, notifications) = CreateTaxCoreEInvoice(eInvoiceBatch, certificateCredential, branch, item.TransactionInfoModificationAction, item.AdditionInfoCreationAction);
					AssertContains(item.Error, notifications.ToString());
					if (string.IsNullOrEmpty(item.Error))
					{
						AssertNotNull("EInvoice", eInvoice);
					}
					else
					{
						AssertNull("EInvoice", eInvoice);
					}
				}
			}
		}

		(ITaxCoreEInvoice EInvoice, INotifications ValidationErrors) CreateTaxCoreEInvoice(AccEInvoicingBatch eInvoiceBatch, EInvoicingCertificateCredential certificateCredential, GlbBranch branch, Action<UniversalTransactionInfo> modifyTransactionInfo = null, Func<IAdditionalTransactionInfoForTaxCoreEInvoice> createAdditionalTransactionInfo = null)
		{
			var universalTransactionBatch = CreateUniversalTransactionBatch(eInvoiceBatch);
			var universalTransaction = universalTransactionBatch.TransactionCollection[0];
			modifyTransactionInfo?.Invoke(universalTransaction);
			var staff = Factory.LoadTop1<GlbStaff>(new ZQuery(GlbStaffSchema.GS_Code, universalTransaction.CreateUser.Code));
			var notifications = new Logger();
			var eInvoiceCreator = new TaxCoreEInvoiceCreator();
			var eInvoice = eInvoiceCreator.Create(new TaxCoreEInvoiceCreatorParameter
			{
				AdditionalTransactionInfo = createAdditionalTransactionInfo?.Invoke(),
				CertificateCredential = certificateCredential,
				CreateUser = staff,
				Notifications = notifications,
				UniversalTransaction = universalTransaction,
				TaxFileCode = CountryFactory.TaxFileCode,
				TFNCode = CountryFactory.TFNCode,
				CountryCode = CountryCode,
				Branch = branch
			});
			return (eInvoice, notifications);
		}

		AccEInvoicingBatch CreateEInvoicingBatch(InvoicingBase invoice)
		{
			var batch = ObjectCreator.CreateEInvoicingBatch(100, EInvoicingBatchState.Ready, GlbCompany.CurrentCompany);
			ObjectCreator.CreateEInvoicingTransactionPivot(batch, invoice, EInvoicingPivotState.Batched);
			return batch;
		}

		UniversalTransactionBatch CreateUniversalTransactionBatch(AccEInvoicingBatch batch)
		{
			var dataAccess = new BatchExportDataAccess(((IDbConnectionInternals)TestConnection).ADOConnection, ((IDbConnectionInternals)TestConnection).ADOTransaction);
			var exportor = new TransactionBatchExporter(dataAccess);
			var transactionBatch = exportor.CreateTransactionBatch(batch);
			return transactionBatch;
		}

		protected ITaxCoreCountryEInvoicingObjectFactory CountryFactory => TaxCoreEInvoicingObjectFactory.GetICountryEInvoicingObjectFactory(CountryCode);

		GlbBranch CreateBranch()
		{
			var company = ObjectCreator.CreateCompanyAndBranch("FJBXL");
			Factory.Save();
			return company.Branches[0];
		}

		ZString CountryCode => CountryCodes.Fiji;

		protected override void SetUp()
		{
			base.SetUp();
			Helper.SetupControlAccounts();
		}

		EInvoicingTestHelper Helper
		{
			get { return helper ?? (helper = new EInvoicingTestHelper(ObjectCreator)); }
		}
		EInvoicingTestHelper helper;

		TestObjectCreator ObjectCreator
		{
			get { return testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory)); }
		}
		TestObjectCreator testObjectCreator;
	}
}
