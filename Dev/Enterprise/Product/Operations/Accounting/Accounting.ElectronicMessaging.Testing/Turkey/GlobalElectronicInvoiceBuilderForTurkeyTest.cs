using System;
using System.Linq;
using System.Xml;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.ElectronicMessaging.Common.GlobalElectronicInvoice;
using Enterprise.Accounting.ElectronicMessaging.Common.Testing;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using Enterprise.Accounting.Export.Business;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using static Enterprise.MasterFiles.Business.CountryCompliance.TurkeyComplianceInfo;
using UniversalTransactionBatch = Enterprise.UniversalDataBuss.DataObjects.Accounting.TransactionBatch;

namespace Enterprise.Accounting.ElectronicMessaging.Turkey.Testing
{
	sealed class GlobalElectronicInvoiceBuilderForTurkeyTest : TestCaseWithFactory
	{
		ICountryEInvoicingObjectFactory GetTestCountryFactory() => new TurkeyEInvoicingObjectFactory();

		[TestDate(2020, 1, 29, 15, 26, 32)]
		public void TestGlobalElectronicEInvoiceMessageForTransactionBatchMissingCompanyCredential()
		{
			using (TestObjectCreator.SetTemporaryControlAccounts())
			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, Helper.TurkeyBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				var arInvoice = Helper.CreateARInvoiceWithShipment(Helper.TestObjectCreator.DebtorTR, Helper.TestObjectCreator.TRY, "AR001", ComplianceSubTypeCodes.EAR);
				var invoicingBatch = Helper.CreateInvoiceBatch(arInvoice);

				using (var converter = new TransactionBatchToGEIConverterForTurkey(GetTestCountryFactory()))
				{
					var (eInvoice, validationErrors, validationWarnings) = converter.Convert(invoicingBatch);

					var expectedError = "Credential required for sending invoice is missing for company: " + Helper.TurkeyBranch.Company.GC_Code;
					AssertNull(eInvoice);
					AssertEquals("Error", expectedError, validationErrors.ToString());
				}
			}
		}

		[TestDate(2020, 1, 29, 15, 28, 00, 000)]
		public void TestErrorWhenComplianceSubTypeEmtiedAfterARInvoiceIssued()
		{
			using (AccountingMasterFilesRegistry.Instance.EnableNewTurkeyARComplianceFeaturesFrom.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDate.Today.AddDays(-1).ToDateTime()))
			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, Helper.TurkeyBranch.Company.FirstActiveBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
			using (Helper.EnableEInvoicingFunctionalityForCompany(Helper.TurkeyBranch.Company))
			using (TestObjectCreator.SetTemporaryControlAccounts())
			using (Helper.SetEReportingComplianceDateForCompany(Helper.TurkeyBranch.Company, ZDateTime.Now.ToDateTime()))
			{
				Helper.CommonHelper.AddCustomsCodeForCountryIfMissing(Helper.TurkeyBranch.Company.OrgProxy, CountryCodes.Turkey, OrgCusCode.CodeTypes.VATCode, "11112222");
				Helper.CreateCompanySignatureCredential(Helper.TurkeyBranch.Company);

				var arInvoice = Helper.CreateARINVTransactions(Helper.TurkeyBranch, Helper.TestObjectCreator.KDV18);
				var transactionHeader = Factory.LoadTop1<AccTransactionHeader>(new ZQuery(AccTransactionHeaderSchema.PK, arInvoice.PK));

				transactionHeader.AH_ComplianceSubType = ZString.Empty;
				Factory.Save();

				var invoicingBatch = Helper.CreateInvoiceBatch(arInvoice);

				using (var converter = new TransactionBatchToGEIConverterForTurkey(GetTestCountryFactory()))
				{
					var expectedError = "Value cannot be null.\r\nParameter name: ComplianceSubType";
					AssertExceptionThrown(typeof(ArgumentException), expectedError, () => converter.Convert(invoicingBatch));
				}
			}
		}

		[TestDate(2020, 1, 29, 15, 28, 00, 000)]
		public void TestErrorForInvalidComplianceSubType()
		{
			using (AccountingMasterFilesRegistry.Instance.EnableNewTurkeyARComplianceFeaturesFrom.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDate.Today.AddDays(-1).ToDateTime()))
			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, Helper.TurkeyBranch.Company.FirstActiveBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
			using (Helper.EnableEInvoicingFunctionalityForCompany(Helper.TurkeyBranch.Company))
			using (TestObjectCreator.SetTemporaryControlAccounts())
			using (Helper.SetEReportingComplianceDateForCompany(Helper.TurkeyBranch.Company, ZDateTime.Now.ToDateTime()))
			{
				Helper.CommonHelper.AddCustomsCodeForCountryIfMissing(Helper.TurkeyBranch.Company.OrgProxy, CountryCodes.Turkey, OrgCusCode.CodeTypes.VATCode, "11112222");
				Helper.CreateCompanySignatureCredential(Helper.TurkeyBranch.Company);

				var arInvoice = Helper.CreateARINVTransactions(Helper.TurkeyBranch, Helper.TestObjectCreator.KDV18);
				var transactionHeader = Factory.LoadTop1<AccTransactionHeader>(new ZQuery(AccTransactionHeaderSchema.PK, arInvoice.PK));

				transactionHeader.AH_ComplianceSubType = "XXX";
				Factory.Save();

				var invoicingBatch = Helper.CreateInvoiceBatch(arInvoice);

				using (var converter = new TransactionBatchToGEIConverterForTurkey(GetTestCountryFactory()))
				{
					var expectedError = "Invalid Compliance Sub Type.";
					AssertExceptionThrown(typeof(ArgumentException), expectedError, () => converter.Convert(invoicingBatch));
				}
			}
		}

		[TestDate(2020, 1, 29, 23, 08, 00)]
		public void TestRINGlobalElectroniceInvoiceMessageForTransactionBatchIsCreatedCorrectly()
		{
			var currency = Factory.LoadTop1<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, "TRY"));
			var helper = new RefCurrencyTestHelper(Factory);
			helper.CreateRefLanguageText("RX_SubUnitName", currency.PK, "TR-TR", "RX", "kuruş");
			Factory.Save();
			using (AccountingMasterFilesRegistry.Instance.EnableNewTurkeyARComplianceFeaturesFrom.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDate.Today.AddDays(-1).ToDateTime()))
			using (TestObjectCreator.SetTemporaryControlAccounts())
			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, Helper.TurkeyBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				var signatureCredential = Helper.CreateCompanySignatureCredential(Helper.TurkeyBranch.Company);
				var arInvoice = Helper.CreateARInvoiceWithShipment(Helper.TestObjectCreator.CreditorTR, Helper.TestObjectCreator.TRY, "AR001", ComplianceSubTypeCodes.EIN);
				var invoicingBatch = Helper.CreateInvoiceBatch(arInvoice);
				var exporter = new TransactionBatchExporter(DataAccess, PopulateOptionalXUTFieldsSetting.AllTrue());

				using (var transactionBatch = exporter.CreateTransactionBatch(invoicingBatch, SchemaVersionManager.Current.Namespace))
				{
					var uInvoice = transactionBatch.TransactionCollection.First();
					Helper.SetTransactionShipment1(uInvoice);
					Helper.SetTransactionShipment2(uInvoice);
					var (eInvoice, validationErrors, validationWarnings) = new RINGlobalElectronicInvoiceBuilderForTurkey(invoicingBatch.AIB_BatchNumber.ToString(), transactionBatch).Create();
					var batchRequest = eInvoice.Header.ElectronicInvoiceBatchRequest;

					AssertFieldsForInvoiceTypesAreCreatedCorrectly(batchRequest, TurkeyEInvoiceAPICommandList.Codes.SendReceivablesInvoice, invoicingBatch, signatureCredential, validationErrors);

					var xmlDocumentExpected = new XmlDocument();
					xmlDocumentExpected.LoadXml(TurkeyEInvoiceExportedXmlLocalCurrency);
					var xmlDocumentPayLoad = new XmlDocument();
					xmlDocumentPayLoad.LoadXml(eInvoice.Payload.ToUTF8FromBase64());
					AssertEquals("Payload", xmlDocumentExpected.OuterXml, xmlDocumentPayLoad.OuterXml);
				}
			}
		}

		[TestDate(2020, 1, 29, 23, 08, 00)]
		public void TestRINGlobalElectroniceInvoiceMessageForTransactionBatchIsCreatedCorrectly_EAR()
		{
			var currency = Factory.LoadTop1<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, "TRY"));
			var helper = new RefCurrencyTestHelper(Factory);
			helper.CreateRefLanguageText("RX_SubUnitName", currency.PK, "TR-TR", "RX", "kuruş");
			Factory.Save();
			using (AccountingMasterFilesRegistry.Instance.EnableNewTurkeyARComplianceFeaturesFrom.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDate.Today.AddDays(-1).ToDateTime()))
			using (TestObjectCreator.SetTemporaryControlAccounts())
			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, Helper.TurkeyBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				var signatureCredential = Helper.CreateCompanySignatureCredential(Helper.TurkeyBranch.Company);
				var arInvoice = Helper.CreateARInvoiceWithShipment(Helper.TestObjectCreator.CreditorTR, Helper.TestObjectCreator.TRY, "AR001", ComplianceSubTypeCodes.EAR);
				var invoicingBatch = Helper.CreateInvoiceBatch(arInvoice);
				var exporter = new TransactionBatchExporter(DataAccess, PopulateOptionalXUTFieldsSetting.AllTrue());

				using (var transactionBatch = exporter.CreateTransactionBatch(invoicingBatch, SchemaVersionManager.Current.Namespace))
				{
					var uInvoice = transactionBatch.TransactionCollection.First();
					Helper.SetTransactionShipment1(uInvoice);
					Helper.SetTransactionShipment2(uInvoice);
					var (eInvoice, validationErrors, validationWarnings) = new RINGlobalElectronicInvoiceBuilderForTurkey(invoicingBatch.AIB_BatchNumber.ToString(), transactionBatch).Create();
					var batchRequest = eInvoice.Header.ElectronicInvoiceBatchRequest;
					AssertEquals("MessagingSystem", "Turkey E-Invoicing System", batchRequest.MessagingSystem);
					AssertEquals("MessageType", TurkeyEInvoiceAPICommandList.Codes.SendReceivablesInvoice, batchRequest.MessageType);
					AssertEquals("BatchNumber", invoicingBatch.AIB_BatchNumber.ToString().PadLeft(5, '0'), batchRequest.BatchNumber);
					AssertEquals("FileName", string.Empty, batchRequest.FileName);
					AssertEquals("UserID", signatureCredential.GP_UserID, batchRequest.Certificate);
					var actualPasswordString = EInvoicingTestHelper.DecodePassword(batchRequest.Password);
					AssertEquals("Password", signatureCredential.CurrentDecryptedPassword, actualPasswordString);
					AssertEquals("No Error", string.Empty, validationErrors.ToString());

					var xmlDocumentExpected = new XmlDocument();
					xmlDocumentExpected.LoadXml(TurkeyEInvoiceExportedXmlLocalCurrency_EAR);
					var xmlDocumentPayLoad = new XmlDocument();
					xmlDocumentPayLoad.LoadXml(eInvoice.Payload.ToUTF8FromBase64());
					AssertEquals("Payload", xmlDocumentExpected.OuterXml, xmlDocumentPayLoad.OuterXml);
				}
			}
		}

		[TestDate(2020, 1, 29, 15, 26, 32)]
		public void TestRINGlobalElectronicInvoiceMessageForTransactionBatchMissingTransaction()
		{
			using (AccountingMasterFilesRegistry.Instance.EnableNewTurkeyARComplianceFeaturesFrom.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDate.Today.AddDays(-1).ToDateTime()))
			using (TestObjectCreator.SetTemporaryControlAccounts())
			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, Helper.TurkeyBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				var signatureCredential = Helper.CreateCompanySignatureCredential(Helper.TurkeyBranch.Company);
				var arInvoice = Helper.CreateARInvoiceWithShipment(Helper.TestObjectCreator.CreditorTR, Helper.TestObjectCreator.TRY, "AR001", ComplianceSubTypeCodes.EAR);
				var invoicingBatch = Helper.CreateInvoiceBatch(arInvoice);

				using (var converter = new MockTransactionBatchToGEIConverterForTurkey())
				{
					var expectedError = "Each transaction batch can have only one transaction in order to generate a Turkey Electronic Invoice.";
					AssertExceptionThrown(typeof(ArgumentException), expectedError, () => converter.Convert(invoicingBatch));
				}
			}
		}

		[TestDate(2020, 1, 29, 15, 26, 32)]
		public void TestGlobalElectroniceInvoiceMessageForTransactionBatchIsCreatedCorrectly()
		{
			using (AccountingMasterFilesRegistry.Instance.EnableNewTurkeyARComplianceFeaturesFrom.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDate.Today.AddDays(-1).ToDateTime()))
			using (TestObjectCreator.SetTemporaryControlAccounts())
			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, Helper.TurkeyBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				var signatureCredential = Helper.CreateCompanySignatureCredential(Helper.TurkeyBranch.Company);
				var arInvoice = Helper.CreateARInvoiceWithShipment(Helper.TestObjectCreator.CreditorTR, Helper.TestObjectCreator.TRY, "AR001", ComplianceSubTypeCodes.EAR);
				var batch = Helper.CreateEInvoicingBatch(1, EInvoicingBatchState.Ready, arInvoice, "1234", EInvoicingPivotActionType.DocumentAction, EInvoicingPivotState.Queued);

				var (eInvoice, validationErrors, validationWarnings) = new GlobalElectronicInvoiceRequestByIdBuilderForTurkey(batch.AIB_BatchNumber.ToString(), TurkeyEInvoiceAPICommandList.Codes.RequestPDFofReceivablesInvoice, batch.Company, batch.AIB_GovernmentAllocatedNumber).Create();
				var batchRequest = eInvoice.Header.ElectronicInvoiceBatchRequest;

				AssertFieldsForInvoiceTypesAreCreatedCorrectly(batchRequest, TurkeyEInvoiceAPICommandList.Codes.RequestPDFofReceivablesInvoice, batch, signatureCredential, validationErrors);

				AssertEquals("Payload", batch.AIB_GovernmentAllocatedNumber, eInvoice.Payload.ToUTF8FromBase64());

				//Same test for production environment
				var arInvoiceForProduction = Helper.CreateARInvoiceWithShipment(Helper.TestObjectCreator.CreditorTR, Helper.TestObjectCreator.TRY, "AR002", ComplianceSubTypeCodes.EAR, true, true, false, "C0002");
				var batchForProduction = Helper.CreateEInvoicingBatch(2, EInvoicingBatchState.Ready, arInvoiceForProduction, "5678", EInvoicingPivotActionType.DocumentAction, EInvoicingPivotState.Queued);
				LicenceTypeChanger.SetSystemLicence(DatabaseTypes.Codes.Production);
				Assert("Pre-condition: Should be Production system", Env.Instance.IsProductionSystem);
				var (eInvoiceForProduction, validationErrorsForProduction, validationWarningsForProduction) = new GlobalElectronicInvoiceRequestByIdBuilderForTurkey(batchForProduction.AIB_BatchNumber.ToString(), TurkeyEInvoiceAPICommandList.Codes.RequestPDFofReceivablesInvoice, batchForProduction.Company, batchForProduction.AIB_GovernmentAllocatedNumber).Create();
				var batchRequestForProduction = eInvoiceForProduction.Header.ElectronicInvoiceBatchRequest;
				AssertFieldsForInvoiceTypesAreCreatedCorrectly(batchRequestForProduction, TurkeyEInvoiceAPICommandList.Codes.RequestPDFofReceivablesInvoice, batchForProduction, signatureCredential, validationErrorsForProduction, true);
			}
		}

		public void TestGlobalElectroniceInvoiceMessageForMissingInputs()
		{
			using (AccountingMasterFilesRegistry.Instance.EnableNewTurkeyARComplianceFeaturesFrom.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDate.Today.AddDays(-1).ToDateTime()))
			using (TestObjectCreator.SetTemporaryControlAccounts())
			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, Helper.TurkeyBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				var signatureCredential = Helper.CreateCompanySignatureCredential(Helper.TurkeyBranch.Company);
				var arInvoice = Helper.CreateARInvoiceWithShipment(Helper.TestObjectCreator.CreditorTR, Helper.TestObjectCreator.TRY, "AR001", ComplianceSubTypeCodes.EAR);
				var batch = Helper.CreateEInvoicingBatch(1, Core.Constants.EInvoicingBatchState.Ready, arInvoice, "1234", EInvoicingPivotActionType.DocumentAction, EInvoicingPivotState.Queued);
				var expectedError = "Value cannot be null.\r\nParameter name: company";
				AssertExceptionThrown(typeof(ArgumentException), expectedError, () => new GlobalElectronicInvoiceRequestByIdBuilderForTurkey(batch.AIB_BatchNumber.ToString(), TurkeyEInvoiceAPICommandList.Codes.RequestPDFofReceivablesInvoice, null, batch.AIB_GovernmentAllocatedNumber).Create(), true);
				expectedError = "Value cannot be empty string (\"\").\r\nParameter name: messageType";
				AssertExceptionThrown(typeof(ArgumentException), expectedError, () => new GlobalElectronicInvoiceRequestByIdBuilderForTurkey(batch.AIB_BatchNumber.ToString(), ZString.Empty, batch.Company, batch.AIB_GovernmentAllocatedNumber).Create(), true);
				expectedError = "Value cannot be empty string (\"\").\r\nParameter name: batchNumber";
				AssertExceptionThrown(typeof(ArgumentException), expectedError, () => new GlobalElectronicInvoiceRequestByIdBuilderForTurkey(ZString.Empty, TurkeyEInvoiceAPICommandList.Codes.CancelReceivablesInvoice, batch.Company, batch.AIB_GovernmentAllocatedNumber).Create(), true);
				expectedError = "Value cannot be empty string (\"\").\r\nParameter name: invoiceId";
				AssertExceptionThrown(typeof(ArgumentException), expectedError, () => new GlobalElectronicInvoiceRequestByIdBuilderForTurkey(batch.AIB_BatchNumber.ToString(), TurkeyEInvoiceAPICommandList.Codes.CancelReceivablesInvoice, batch.Company, ZString.Empty).Create(), true);
				expectedError = "Value cannot be null.\r\nParameter name: reversalCreditNotePk";
				AssertExceptionThrown(typeof(ArgumentException), expectedError, () => new RCNGlobalElectronicInvoiceBuilderForTurkey(batch.AIB_BatchNumber.ToString(), batch.Company, batch.AIB_GovernmentAllocatedNumber, ZGuid.Empty).Create(), true);
			}
		}

		[TestTimeZoneUNLOCO("TRIST")]
		[TestDate(2020, 1, 29, 15, 28, 00, 000)]
		public void TestRCNGlobalElectroniceInvoiceMessageForTransactionBatchIsCreatedCorrectly()
		{
			using (AccountingMasterFilesRegistry.Instance.EnableNewTurkeyARComplianceFeaturesFrom.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDate.Today.AddDays(-1).ToDateTime()))
			using (TestObjectCreator.SetTemporaryControlAccounts())
			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, Helper.TurkeyBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				var signatureCredential = Helper.CreateCompanySignatureCredential(Helper.TurkeyBranch.Company);
				var arInvoice = Helper.CreateARInvoiceWithShipment(Helper.TestObjectCreator.CreditorTR, Helper.TestObjectCreator.TRY, "AR001", ComplianceSubTypeCodes.EAR);
				var arCreditNote = Helper.CreateReverseTransaction(arInvoice);
				var invoicingBatch = Helper.CreateInvoiceBatch(arCreditNote);
				invoicingBatch.AIB_GovernmentAllocatedNumber = Helper.GovermentAllocatedNumberForTest;
				var (eInvoice, validationErrors, validationWarnings) = new RCNGlobalElectronicInvoiceBuilderForTurkey(invoicingBatch.AIB_BatchNumber.ToString(), invoicingBatch.Company, invoicingBatch.AIB_GovernmentAllocatedNumber, arCreditNote.PK).Create();
				var batchRequest = eInvoice.Header.ElectronicInvoiceBatchRequest;

				AssertFieldsForInvoiceTypesAreCreatedCorrectly(batchRequest, TurkeyEInvoiceAPICommandList.Codes.CancelReceivablesInvoice, invoicingBatch, signatureCredential, validationErrors);

				var xmlDocumentExpected = new XmlDocument();
				xmlDocumentExpected.LoadXml(TurkeyEInvoiceRCNGEIMessageXml);
				var xmlDocumentPayLoad = new XmlDocument();
				xmlDocumentPayLoad.LoadXml(eInvoice.Payload.ToUTF8FromBase64());
				var payloadXml = xmlDocumentExpected.ChildNodes[0].LastChild;
				var payload = payloadXml.InnerXml.ToUTF8FromBase64();
				AssertEquals("Payload", payloadXml.InnerXml, eInvoice.Payload);
			}
		}

		void AssertFieldsForInvoiceTypesAreCreatedCorrectly(GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequest batchRequest, string messageType, AccEInvoicingBatch invoicingBatch
			, GlbCompanySignatureCredential signatureCredential, INotifications validationErrors, bool isProductionSystem = false)
		{
			AssertEquals("MessagingSystem", "Turkey E-Invoicing System", batchRequest.MessagingSystem);
			AssertEquals("MessageType", messageType, batchRequest.MessageType);
			AssertEquals("BatchNumber", invoicingBatch.AIB_BatchNumber.ToString().PadLeft(5, '0'), batchRequest.BatchNumber);
			AssertEquals("FileName", string.Empty, batchRequest.FileName);
			AssertEquals("UserID", signatureCredential.GP_UserID, batchRequest.Certificate);
			var actualPasswordString = EInvoicingTestHelper.DecodePassword(batchRequest.Password);
			AssertEquals("Password", signatureCredential.CurrentDecryptedPassword, actualPasswordString);
			AssertEquals("No Error", string.Empty, validationErrors.ToString());
			AssertNotNull(batchRequest.IsProductionSystemSpecified);
			AssertEquals("IsProductionSystemSpecified", true, batchRequest.IsProductionSystemSpecified);
			AssertNotNull(batchRequest.IsProductionSystem);
			AssertEquals("IsProductionSystem", isProductionSystem, batchRequest.IsProductionSystem);
		}

		[TestDate(2020, 1, 29, 23, 08, 00)]
		public void TestPILGEIMessageIsCreatedCorrectly()
		{
			using (AccountingMasterFilesRegistry.Instance.EnableNewTurkeyAPComplianceFeaturesFrom.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDate.Today.AddDays(-1).ToDateTime()))
			using (TestObjectCreator.SetTemporaryControlAccounts())
			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, Helper.TurkeyBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				var signatureCredential = Helper.CreateCompanySignatureCredential(Helper.TurkeyBranch.Company);
				var batch = Helper.CreateBatch();
				var exporter = new TransactionBatchExporter(DataAccess, PopulateOptionalXUTFieldsSetting.AllTrue());

				using (var transactionBatch = exporter.CreateTransactionBatch(batch, SchemaVersionManager.Current.Namespace))
				{
					var (eInvoice, validationErrors, validationWarnings) = new APGEIBuilderForTurkey(batch.AIB_BatchNumber.ToString(), TurkeyEInvoiceAPICommandList.Codes.GetInboxInvoiceList, batch.Company, "").Create();

					var expectedXml = new XmlDocument();
					expectedXml.LoadXml(TurkeyPILGEIMessageXml);
					var xmlDocument = new XmlDocument();
					xmlDocument.LoadXml(eInvoice.Payload.ToUTF8FromBase64());
					AssertEquals("Payload", expectedXml.OuterXml, xmlDocument.OuterXml);
					AssertEquals("Pagesize value should be taken from registry", AccountingConfigurationRegistry.Instance.SizeOfRequestedAPTransactionList.Value, Convert.ToInt32(xmlDocument.ChildNodes[0].FirstChild.Attributes["PageSize"].Value));
				}
			}
		}

		public void TestPAPGEIMessagePayloadIsCreatedCorrectly() => AssertGEIPayloadIsCreatedCorrectly(TurkeyEInvoiceAPICommandList.Codes.SendApproveDocumentResponse, TurkeyPAPGEIMessagePayload);

		public void TestPRJGEIMessagePayloadIsCreatedCorrectly() => AssertGEIPayloadIsCreatedCorrectly(TurkeyEInvoiceAPICommandList.Codes.SendRejectDocumentResponse, TurkeyPRJGEIMessagePayload);

		void AssertGEIPayloadIsCreatedCorrectly(string apiCommand, string expectedPayload)
		{
			using (AccountingMasterFilesRegistry.Instance.EnableNewTurkeyAPComplianceFeaturesFrom.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDate.Today.AddDays(-1).ToDateTime()))
			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, Helper.TurkeyBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				var factory = Helper.TestObjectCreator.Factory;
				var signatureCredential = Helper.CreateCompanySignatureCredential(Helper.TurkeyBranch.Company);
				var invoicingBatch = Helper.TestObjectCreator.CreateEInvoicingBatch(batchNumber: 2, batchState: EInvoicingBatchState.Ready, GlbCompany.CurrentCompany, governmentAllocatedNumber: Helper.GovermentAllocatedNumberForTest);
				var (eInvoice, validationErrors, validationWarnings) = new APGEIBuilderForTurkey(invoicingBatch.AIB_BatchNumber.ToString(), apiCommand, invoicingBatch.Company, invoicingBatch.AIB_GovernmentAllocatedNumber).Create();
				var batchRequest = eInvoice.Header.ElectronicInvoiceBatchRequest;

				AssertFieldsForInvoiceTypesAreCreatedCorrectly(batchRequest, apiCommand, invoicingBatch, signatureCredential, validationErrors);

				var xmlDocumentPayLoad = new XmlDocument();
				xmlDocumentPayLoad.LoadXml(eInvoice.Payload.ToUTF8FromBase64());
				var xmlDocumentExpected = new XmlDocument();
				xmlDocumentExpected.LoadXml(expectedPayload);
				AssertEquals("Payload", xmlDocumentExpected.InnerXml, xmlDocumentPayLoad.InnerXml);
			}
		}

		public void TestGEIMessageIsProductionSystem()
		{
			using (TestObjectCreator.SetTemporaryControlAccounts())
			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, Helper.TurkeyBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				// Arrange
				var defaultRegistryValue = AccountingMasterFilesRegistry.Instance.EReportingGEIMessageSystemType.Value;
				AssertEquals("Precondition", AccountingMasterFilesConstants.EReportingGEIMessageSystemTypeCodes.Default, defaultRegistryValue);

				var transactionBatch = new UniversalTransactionBatch(DefaultDataObjectWriterStrategy.TestInstance);
				transactionBatch.TransactionCollection.Add(new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance));
				var company = Helper.TurkeyBranch.Company.PK.ToGuid();

				var signatureCredentials = Helper.CreateCompanySignatureCredential(Helper.TurkeyBranch.Company);

				var arInvoiceForProduction = Helper.CreateARInvoiceWithShipment(Helper.TestObjectCreator.CreditorTR, Helper.TestObjectCreator.TRY, "AR002", ComplianceSubTypeCodes.EAR, true, true, false, "C0002");
				var batchForProduction = Helper.CreateEInvoicingBatch(2, EInvoicingBatchState.Ready, arInvoiceForProduction, "5678", EInvoicingPivotActionType.DocumentAction, EInvoicingPivotState.Queued);

				var globalBuilder = new GlobalElectronicInvoiceRequestByIdBuilderForTurkey(batchForProduction.AIB_BatchNumber.ToString(), TurkeyEInvoiceAPICommandList.Codes.RequestPDFofReceivablesInvoice, Helper.TurkeyBranch.Company, batchForProduction.AIB_GovernmentAllocatedNumber);

				// Assert
				ExecuteWithTemporaryRegistryValue(company, AccountingMasterFilesConstants.EReportingGEIMessageSystemTypeCodes.AlwaysProductionSystem, DatabaseTypes.Codes.Training, true, globalBuilder);
				ExecuteWithTemporaryRegistryValue(company, AccountingMasterFilesConstants.EReportingGEIMessageSystemTypeCodes.AlwaysTestSystem, DatabaseTypes.Codes.Production, false, globalBuilder);
				ExecuteWithTemporaryRegistryValue(company, AccountingMasterFilesConstants.EReportingGEIMessageSystemTypeCodes.Default, DatabaseTypes.Codes.Production, true, globalBuilder);
				ExecuteWithTemporaryRegistryValue(company, AccountingMasterFilesConstants.EReportingGEIMessageSystemTypeCodes.Default, DatabaseTypes.Codes.Test, false, globalBuilder);

				void ExecuteWithTemporaryRegistryValue(Guid companyPk, string registryCode, string licenceType, bool expectedValue, IGlobalElectronicInvoiceBuilder globalBuilder)
				{
					LicenceTypeChanger.SetSystemLicence(licenceType);
					using (AccountingMasterFilesRegistry.Instance.EReportingGEIMessageSystemType.SetTemporaryValue(companyPk, Guid.Empty, Guid.Empty, registryCode))
					{
						var (eInvoice, _, _) = globalBuilder.Create();
						AssertEquals(expectedValue, eInvoice.Header.ElectronicInvoiceBatchRequest.IsProductionSystem);
					}
				}
			}
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			var connection = ((IDbConnectionInternals)base.TestConnection).ADOConnection;
			var transaction = ((IDbConnectionInternals)base.TestConnection).ADOTransaction;
			DataAccess = new BatchExportDataAccess(connection, transaction);
		}

		BatchExportDataAccess DataAccess;

		TurkeyEInvoiceTestHelper Helper => helper ?? (helper = new TurkeyEInvoiceTestHelper());
		TurkeyEInvoiceTestHelper helper;

		string TurkeyEInvoiceExportedXmlLocalCurrency => TurkeyEInvoiceTestHelper.GetEmbeddedResourceAsString("TurkeyEInvoiceExportedXml.xml");

		string TurkeyEInvoiceExportedXmlLocalCurrency_EAR => TurkeyEInvoiceTestHelper.GetEmbeddedResourceAsString("TurkeyEInvoiceExportedXml_EAR.xml");

		string TurkeyEInvoiceRCNGEIMessageXml => TurkeyEInvoiceTestHelper.GetEmbeddedResourceAsString("TurkeyEInvoiceRCNGEIMessageXml.xml");

		string TurkeyPILGEIMessageXml => TurkeyEInvoiceTestHelper.GetEmbeddedResourceAsString("TurkeyEInvoicePILGEIMessageXml.xml");

		string TurkeyPAPGEIMessagePayload => TurkeyEInvoiceTestHelper.GetEmbeddedResourceAsString("TurkeyEInvoicePAPGEIMessagePayload.xml");

		string TurkeyPRJGEIMessagePayload => TurkeyEInvoiceTestHelper.GetEmbeddedResourceAsString("TurkeyEInvoicePRJGEIMessagePayload.xml");

		#endregion

		#region Inner Classes

		internal class MockGlobalElectronicInvoiceBuilderForTurkey : IGlobalElectronicInvoiceBuilder
		{
			public MockGlobalElectronicInvoiceBuilderForTurkey(UniversalTransactionBatch universalTransactionBatch)
			{
				Batch = universalTransactionBatch;

				if (Batch?.TransactionCollection?.Count != 1)
				{
					throw new ArgumentException("Each transaction batch can have only one transaction in order to generate a Turkey Electronic Invoice.");
				}
			}

			UniversalTransactionBatch Batch { get; }

			public (GlobalElectronicInvoicing EInvoice, INotifications ValidationErrors, INotifications ValidationWarnings) Create()
			{
				return (new GlobalElectronicInvoicing(), new Logger(), new Logger());
			}
		}

		internal class MockTransactionBatchToGEIConverterForTurkey : TransactionBatchToGEIConverter
		{
			protected override IGlobalElectronicInvoiceBuilder GetGEIBuilder(string batchNumber)
			{
				return new MockGlobalElectronicInvoiceBuilderForTurkey(UniversalBatch);
			}

			public new (GlobalElectronicInvoicing EInvoice, ZString ValidationErrors, ZString ValidationWarnings) Convert(AccEInvoicingBatch batch)
			{
				var builder = GetGEIBuilder(batch.AIB_BatchNumber.ToString());
				var (eInvoice, validationErrors, validationWarnings) = builder.Create();
				return (eInvoice, validationErrors.ToString(), validationWarnings.ToString());
			}
		}
	}

	#endregion
}
