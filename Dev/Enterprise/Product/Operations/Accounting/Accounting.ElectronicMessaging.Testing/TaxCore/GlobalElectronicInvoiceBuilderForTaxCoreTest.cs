using System;
using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.ElectronicMessaging.Common.GlobalElectronicInvoice;
using Enterprise.Accounting.ElectronicMessaging.Common.Testing;
using Enterprise.Accounting.Export.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.ElectronicMessaging.TaxCore.Testing
{
	[TestedType(typeof(GlobalElectronicInvoiceBuilderForTaxCore))]
	public abstract class GlobalElectronicInvoiceBuilderForTaxCoreTest : TestCaseWithFactory
	{
		[TestDate(2019, 7, 16, 15, 26, 32)]
		public void TestGlobalElectronicInvoiceMessageForTransactionBatchIsCreatedCorrectly()
		{
			const string SamplePayloadTaxCoreLegacy = """{"DateAndTimeOfIssue":"2019-07-16T15:26:00Z","BD":"12345678798","IT":0,"TT":0,"PaymentType":0,"InvoiceNumber":"00001000","Options":{"OmitQRCodeGen":"1","OmitTextualRepresentation":"1"},"Hash":"z5Ge0oGqAYpfzObK1gYEHw==","Items":[{"Name":"Charge Code 1","Quantity":1,"UnitPrice":100.0000,"Labels":["A"],"TotalAmount":100.0000}]}""";

			AssertGlobalElectronicInvoiceMessageForTransactionBatchIsCreatedCorrectly(ZDate.Today.AddDays(1), SamplePayloadTaxCoreLegacy);
		}

		[TestDate(2019, 7, 16, 15, 26, 32)]
		public void TestGlobalElectronicInvoiceMessageForTransactionBatchIsCreatedCorrectlyV3()
		{
			const string SamplePayloadTaxCoreV3 = """{"dateAndTimeOfIssue":"2019-07-16T15:26:00Z","buyerId":"12345678798","invoiceType":0,"transactionType":0,"payment":[{"amount":100.0000,"paymentType":0}],"invoiceNumber":"00001000","options":{"omitQRCodeGen":"1","omitTextualRepresentation":"1"},"items":[{"name":"Charge Code 1","quantity":1,"unitPrice":100.0000,"labels":["A"],"totalAmount":100.0000}]}""";

			AssertGlobalElectronicInvoiceMessageForTransactionBatchIsCreatedCorrectly(ZDate.Today.AddDays(-1), SamplePayloadTaxCoreV3);
		}

		void AssertGlobalElectronicInvoiceMessageForTransactionBatchIsCreatedCorrectly(ZDate newSchemaComplianceDate, string expectedPayload)
		{
			var branch = CreateBranch();

			using (AccountingMasterFilesRegistry.Instance.EReportingComplianceDateNewSchema.SetTemporaryValue(branch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, newSchemaComplianceDate.ToDateTime()))
			using (var credentialCreator = new TestEInvoicingCertificateCredentialCreator(branch, ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(7)))
			{
				var taxMsg = ObjectCreator.CreateTaxMsg("TX", "TaxCore Demo Tax Msg", "TaxCore Demo Tax Msg", "TaxCore Demo Tax Msg");
				taxMsg.A9_RN_NKCountryCode = CountryCode;
				taxMsg.A9_TaxGroupCode = ((CodeDescriptionBoolRelatedItem)CountryComplianceFactory.GetITaxMessageGroupProvider(CountryCode)?.GetTaxMessageGroup().First()).Code;

				ObjectCreator.SetCustomsCodeForOrgHeader(ObjectCreator.Debtor, OrgCusCode.CodeTypes.TaxFileCode, CountryCode, "12345678798");

				var certificateCredential = credentialCreator.CreateCertificateCredential();

				Factory.Save();

				AccEInvoicingBatch batch = null;
				InvoicingBase arInvoice = null;
				using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, branch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
				{
					batch = ObjectCreator.CreateEInvoicingBatch(100, EInvoicingBatchState.Ready, GlbCompany.CurrentCompany);
					arInvoice = ObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "001", ObjectCreator.AUD, 1.0m, 100.00m, 10.00m, 100.00m, 10.00m, ObjectCreator.Debtor, ObjectCreator.CC1.PK);
					arInvoice.Lines[0].AL_A9_VATClass = taxMsg.PK;
					ObjectCreator.CreateEInvoicingTransactionPivot(batch, arInvoice, EInvoicingPivotState.Batched);
					Factory.Save();
				}

				var dataAccess = new BatchExportDataAccess(((IDbConnectionInternals)TestConnection).ADOConnection, ((IDbConnectionInternals)TestConnection).ADOTransaction);
				var exporter = new TransactionBatchExporter(dataAccess);
				var transactionBatch = exporter.CreateTransactionBatch(batch);
				var (einvoice, validationErrors, validationWarnings) = new GlobalElectronicInvoiceBuilderForTaxCore(batch.AIB_BatchNumber.ToString(), transactionBatch, arInvoice, TaxCoreEInvoicingObjectFactory.GetICountryEInvoicingObjectFactory(CountryCode)).Create();

				AssertNullOrEmpty("ValidationErrors", validationErrors.ToString());
				AssertNullOrEmpty("ValidationWarnings", validationWarnings.ToString());
				AssertEquals("MessagingSystem", InvoiceSystemName, einvoice.Header.ElectronicInvoiceBatchRequest.MessagingSystem);
				AssertEquals("MessageType", "REQ", einvoice.Header.ElectronicInvoiceBatchRequest.MessageType);
				AssertEquals("BatchNumber", batch.AIB_BatchNumber.ToString(), einvoice.Header.ElectronicInvoiceBatchRequest.BatchNumber);
				AssertEquals("FileName", string.Empty, einvoice.Header.ElectronicInvoiceBatchRequest.FileName);
				AssertEquals("Certificate", certificateCredential.GP_Certificate, Convert.FromBase64String(einvoice.Header.ElectronicInvoiceBatchRequest.Certificate));
				var actualPasswordString = EInvoicingTestHelper.DecodePassword(einvoice.Header.ElectronicInvoiceBatchRequest.Password);
				AssertEquals("Password", certificateCredential.CurrentDecryptedCertificatePassphrase, actualPasswordString);
				AssertEquals("Payload", expectedPayload, einvoice.Payload.ToUTF8FromBase64());
				AssertEquals("No Error", string.Empty, validationErrors.ToString());
				AssertEquals("Credentials", 1, einvoice.Header.ElectronicInvoiceBatchRequest.Credentials.Count);
				AssertEquals("Credentials", "PAC", einvoice.Header.ElectronicInvoiceBatchRequest.Credentials[0].Key);
				AssertEquals("Credentials", certificateCredential.GP_MailBoxID, einvoice.Header.ElectronicInvoiceBatchRequest.Credentials[0].Value.Value);
			}
		}

		[TestDate(2019, 7, 16, 15, 26, 32)]
		public void TestGlobalElectronicInvoiceMessage_AddsValidationError_WhenUnsupportedTaxMessage()
		{
			const string SamplePayloadUnsupportedTaxMessageLegacy = """{"DateAndTimeOfIssue":"2019-07-16T15:26:00Z","BD":"12345678798","IT":0,"TT":0,"PaymentType":0,"InvoiceNumber":"00001000","Options":{"OmitQRCodeGen":"1","OmitTextualRepresentation":"1"},"Hash":"1TKiGYBnFWWzDRK/7tcr0A==","Items":[{"Name":"Charge Code 1","Quantity":1,"UnitPrice":100.0000,"Labels":["NO!"],"TotalAmount":100.0000}]}""";

			AssertGlobalElectronicInvoiceMessage_AddsValidationError_WhenUnsupportedTaxMessage(ZDate.Today.AddDays(1), SamplePayloadUnsupportedTaxMessageLegacy);
		}

		void AssertGlobalElectronicInvoiceMessage_AddsValidationError_WhenUnsupportedTaxMessage(ZDate newSchemaComplianceDate, string expectedPayload)
		{
			var branch = CreateBranch();

			using (AccountingMasterFilesRegistry.Instance.EReportingComplianceDateNewSchema.SetTemporaryValue(branch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, newSchemaComplianceDate.ToDateTime()))
			using (var credentialCreator = new TestEInvoicingCertificateCredentialCreator(branch, ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(7)))
			{
				var taxMsg = ObjectCreator.CreateTaxMsg("TX", "TaxCore Demo Tax Msg", "TaxCore Demo Tax Msg", "TaxCore Demo Tax Msg");
				taxMsg.A9_RN_NKCountryCode = CountryCode;
				taxMsg.A9_TaxGroupCode = "NO!";

				ObjectCreator.SetCustomsCodeForOrgHeader(ObjectCreator.Debtor, OrgCusCode.CodeTypes.TaxFileCode, CountryCode, "12345678798");

				var certificateCredential = credentialCreator.CreateCertificateCredential();

				Factory.Save();

				AccEInvoicingBatch batch = null;
				InvoicingBase arInvoice = null;
				using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, branch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
				{
					batch = ObjectCreator.CreateEInvoicingBatch(100, EInvoicingBatchState.Ready, GlbCompany.CurrentCompany);
					arInvoice = ObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "001", ObjectCreator.AUD, 1.0m, 100.00m, 10.00m, 100.00m, 10.00m, ObjectCreator.Debtor, ObjectCreator.CC1.PK);
					arInvoice.Lines[0].AL_A9_VATClass = taxMsg.PK;
					ObjectCreator.CreateEInvoicingTransactionPivot(batch, arInvoice, EInvoicingPivotState.Batched);
					Factory.Save();
				}

				var dataAccess = new BatchExportDataAccess(((IDbConnectionInternals)TestConnection).ADOConnection, ((IDbConnectionInternals)TestConnection).ADOTransaction);
				var exporter = new TransactionBatchExporter(dataAccess);
				var transactionBatch = exporter.CreateTransactionBatch(batch);
				var (einvoice, validationErrors, validationWarnings) = new GlobalElectronicInvoiceBuilderForTaxCore(batch.AIB_BatchNumber.ToString(), transactionBatch, arInvoice, TaxCoreEInvoicingObjectFactory.GetICountryEInvoicingObjectFactory(CountryCode)).Create();

				AssertEquals("Unknown tax code should generate JSON Schema validation error", "/Items/items/Labels/items: Value should match one of the values specified by the enum", validationErrors.ToString());
				AssertNullOrEmpty("ValidationWarnings", validationWarnings.ToString());
				AssertEquals("Payload", expectedPayload, einvoice.Payload.ToUTF8FromBase64());
			}
		}

		public void TestTaxCoreEInvoiceSchemaCompatibilityWithTaxCoreTaxMessageGroupCodes()
		{
			var fijiTaxMessageGroupCodes = ReflectionExtensions.GetConstantValues(typeof(FijiComplianceInfo.TaxMessageGroupCodes));
			var samoaTaxMessageGroupCodes = ReflectionExtensions.GetConstantValues(typeof(SamoaComplianceInfo.TaxMessageGroupCodes));
			var schema = JsonSchemaLoader.Load("Enterprise.Accounting.ElectronicMessaging.TaxCore.EInvoice.TaxCoreEInvoiceSchema.json");
			var notifications = new Logger();

			foreach (var taxMessageGroupCode in fijiTaxMessageGroupCodes.Concat(samoaTaxMessageGroupCodes))
			{
				var payload = $$"""{"DateAndTimeOfIssue":"2019-07-16T15:26:00Z","BD":"12345678798","IT":0,"TT":0,"PaymentType":0,"InvoiceNumber":"00001000","Options":{"OmitQRCodeGen":"1","OmitTextualRepresentation":"1"},"Hash":"z5Ge0oGqAYpfzObK1gYEHw==","Items":[{"Name":"Charge Code 1","Quantity":1,"UnitPrice":100.0000,"Labels":["{{taxMessageGroupCode}}"],"TotalAmount":100.0000}]}""";
				Assert($"JSON Validation failed for tax message group: {taxMessageGroupCode}, update the TaxCoreEInvoiceSchema.json to include new TaxMessageGroup code", schema.ValidateJSON(payload, notifications));
			}
		}

		public void TestGEIMessageIsProductionSystem()
		{
			var branch = CreateBranch();
			using (var credentialCreator = new TestEInvoicingCertificateCredentialCreator(branch, ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(7)))
			{
				// Arrange
				var defaultRegistryValue = AccountingMasterFilesRegistry.Instance.EReportingGEIMessageSystemType.Value;
				AssertEquals("Precondition", AccountingMasterFilesConstants.EReportingGEIMessageSystemTypeCodes.Default, defaultRegistryValue);

				var taxMsg = ObjectCreator.CreateTaxMsg("TX", "TaxCore Demo Tax Msg", "TaxCore Demo Tax Msg", "TaxCore Demo Tax Msg");
				taxMsg.A9_RN_NKCountryCode = CountryCode;
				taxMsg.A9_TaxGroupCode = ((CodeDescriptionBoolRelatedItem)CountryComplianceFactory.GetITaxMessageGroupProvider(CountryCode)?.GetTaxMessageGroup().First()).Code;

				ObjectCreator.SetCustomsCodeForOrgHeader(ObjectCreator.Debtor, OrgCusCode.CodeTypes.TaxFileCode, CountryCode, "12345678798");

				var certificateCredential = credentialCreator.CreateCertificateCredential();

				Factory.Save();

				AccEInvoicingBatch batch = null;
				InvoicingBase arInvoice = null;
				using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, branch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
				{
					batch = ObjectCreator.CreateEInvoicingBatch(100, EInvoicingBatchState.Ready, GlbCompany.CurrentCompany);
					arInvoice = ObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "001", ObjectCreator.AUD, 1.0m, 100.00m, 10.00m, 100.00m, 10.00m, ObjectCreator.Debtor, ObjectCreator.CC1.PK);
					arInvoice.Lines[0].AL_A9_VATClass = taxMsg.PK;
					ObjectCreator.CreateEInvoicingTransactionPivot(batch, arInvoice, EInvoicingPivotState.Batched);
					Factory.Save();
				}

				var dataAccess = new BatchExportDataAccess(((IDbConnectionInternals)TestConnection).ADOConnection, ((IDbConnectionInternals)TestConnection).ADOTransaction);
				var exporter = new TransactionBatchExporter(dataAccess);
				var transactionBatch = exporter.CreateTransactionBatch(batch);

				var globalBuilder = new GlobalElectronicInvoiceBuilderForTaxCore("12345", transactionBatch, arInvoice, TaxCoreEInvoicingObjectFactory.GetICountryEInvoicingObjectFactory(CountryCode));

				// Assert
				ExecuteWithTemporaryRegistryValue(batch.Company.PK.ToGuid(), AccountingMasterFilesConstants.EReportingGEIMessageSystemTypeCodes.AlwaysProductionSystem, DatabaseTypes.Codes.Training, expectedValue: true, globalBuilder);
				ExecuteWithTemporaryRegistryValue(batch.Company.PK.ToGuid(), AccountingMasterFilesConstants.EReportingGEIMessageSystemTypeCodes.AlwaysTestSystem, DatabaseTypes.Codes.Production, expectedValue: false, globalBuilder);
				ExecuteWithTemporaryRegistryValue(batch.Company.PK.ToGuid(), AccountingMasterFilesConstants.EReportingGEIMessageSystemTypeCodes.Default, DatabaseTypes.Codes.Production, expectedValue: true, globalBuilder);
				ExecuteWithTemporaryRegistryValue(batch.Company.PK.ToGuid(), AccountingMasterFilesConstants.EReportingGEIMessageSystemTypeCodes.Default, DatabaseTypes.Codes.Test, expectedValue: false, globalBuilder);

				static void ExecuteWithTemporaryRegistryValue(Guid companyPk, string registryCode, string licenceType, bool expectedValue, IGlobalElectronicInvoiceBuilder globalBuilder)
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

		GlbBranch CreateBranch()
		{
			var company = ObjectCreator.CreateCompanyAndBranch("FJBXL");
			Factory.Save();
			return company.Branches[0];
		}

		protected abstract ZString CountryCode { get; }

		protected abstract ZString InvoiceSystemName { get; }

		protected override void SetUp()
		{
			base.SetUp();
			Helper.SetupControlAccounts();
		}

		protected EInvoicingTestHelper Helper
		{
			get { return helper ??= new EInvoicingTestHelper(ObjectCreator); }
		}
		EInvoicingTestHelper helper;

		protected TestObjectCreator ObjectCreator
		{
			get { return testObjectCreator ??= new TestObjectCreator(Factory); }
		}
		TestObjectCreator testObjectCreator;
	}
}
