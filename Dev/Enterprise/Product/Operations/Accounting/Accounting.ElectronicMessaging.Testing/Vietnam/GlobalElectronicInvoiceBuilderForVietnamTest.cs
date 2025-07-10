using System;
using CargoWise.Data;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.ElectronicMessaging.Common.GlobalElectronicInvoice;
using Enterprise.Accounting.ElectronicMessaging.Common.Testing;
using Enterprise.Accounting.ElectronicMessaging.Testing.Vietnam;
using Enterprise.Accounting.ElectronicMessaging.Vietnam.EInvoice;
using Enterprise.Accounting.Export.Business;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using Newtonsoft.Json;
using NUnit.Framework;
using MessageTypes = Enterprise.Accounting.ElectronicMessaging.Vietnam.VietnamEInvoiceAPICommandList.Codes;

namespace Enterprise.Accounting.ElectronicMessaging.Vietnam.Testing
{
	sealed class GlobalElectronicInvoiceBuilderForVietnamTest : TestCaseWithFactory
	{
		[TestDate(2020, 08, 24, 08, 36, 00)]
		public void TestRINGlobalElectronicInvoiceMessageForTransactionBatchIsCreatedCorrectly()
		{
			var branch = CreateVietnamBranch();
			Factory.Save();

			AccEInvoicingBatch batch;
			InvoicingBase arInvoice;
			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, branch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				Helper.AddCustomsCodeForCountryIfMissing(branch.OrgProxy, Core.Constants.CountryCodes.VietNam, "VAT", "0100233488");

				batch = TestObjectCreator.CreateEInvoicingBatch(100, Core.Constants.EInvoicingBatchState.Ready, GlbCompany.CurrentCompany);
				arInvoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "001", TestObjectCreator.AUD, 1.0m, 100.00m, 10.00m, 100.00m, 10.00m, TestObjectCreator.Debtor, TestObjectCreator.CC1.PK);
				arInvoice.AH_GB = branch.PK;

				TestObjectCreator.CreateEInvoicingTransactionPivot(batch, arInvoice, Core.Constants.EInvoicingPivotState.Batched);
				arInvoice.AH_TransactionReference = "AP/19E1";
				Factory.Save();
			}

			var dataAccess = new BatchExportDataAccess(((IDbConnectionInternals)TestConnection).ADOConnection, ((IDbConnectionInternals)TestConnection).ADOTransaction);
			var exporter = new TransactionBatchExporter(dataAccess, PopulateOptionalXUTFieldsSetting.AllTrue());
			var transactionBatch = exporter.CreateTransactionBatch(batch);

			var additionalTransactionInfo = new AdditionalTransactionInfoForVietnamEInvoice()
			{
				OriginalTransactionPK = arInvoice.PK,
				OriginalTransactionReference = arInvoice.AH_TransactionReference,
				CompanyPK = arInvoice.Company.PK,
				BranchPK = arInvoice.Branch.PK
			};

			var (eInvoice, _, _) = new RINGlobalElectronicInvoiceBuilderForVietnam(batch.AIB_BatchNumber.ToString(), transactionBatch, additionalTransactionInfo).Create();

			AssertGEIMessage(eInvoice, batch, branch, MessageTypes.SendReceivablesInvoice);
		}

		[TestDate(2020, 08, 24, 08, 36, 00)]
		public void TestRDNGlobalElectronicInvoiceMessageForTransactionBatchIsCreatedCorrectly()
		{
			var branch = CreateVietnamBranch();
			Factory.Save();

			AccEInvoicingBatch batch;
			InvoicingBase arInvoice;

			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, branch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				Helper.AddCustomsCodeForCountryIfMissing(branch.OrgProxy, Core.Constants.CountryCodes.VietNam, "VAT", "0100233488");

				batch = TestObjectCreator.CreateEInvoicingBatch(100, Core.Constants.EInvoicingBatchState.Ready, GlbCompany.CurrentCompany);

				arInvoice = Factory.NewWithPrimaryKey<ARInvoice>(new Guid("8447058a-27ce-4102-93fa-5af847458849"));
				arInvoice.AH_GC = branch.GB_GC;
				arInvoice.AH_OH = TestObjectCreator.Debtor.PK;
				arInvoice.AH_Desc = "TEST";
				arInvoice.AH_Ledger = LedgerTypes.AccountsReceivable;
				arInvoice.AH_TransactionType = TransactionTypes.Invoice;
				arInvoice.AH_TransactionNum = TestObjectCreator.GetRandomString(5);
				arInvoice.AH_GB = branch.PK;
				arInvoice.AH_GE = GlbDepartment.CurrentDepartment.PK;
				arInvoice.AH_InvoiceDate = ZDateTime.Today;
				arInvoice.AH_PostDate = ZDateTime.Today;
				arInvoice.AH_ComplianceSubType = VietnamComplianceInfo.ComplianceSubTypeCodes.TXI;
				arInvoice.AH_TransactionReference = "XI123456";
				TestObjectCreator.CreateInvoiceLine(arInvoice, TestObjectCreator.EUR, 1m, 10m, 0m, 0m, 10m, 0m, 0m);
				arInvoice.Lines[0].GenericCharge = TestObjectCreator.CC1.PK;

				TestObjectCreator.CreateEInvoicingTransactionPivot(batch, arInvoice, Core.Constants.EInvoicingPivotState.Batched);
				arInvoice.AH_TransactionReference = "AP/19E1";
				Factory.Save();
			}

			var dataAccess = new BatchExportDataAccess(((IDbConnectionInternals)TestConnection).ADOConnection, ((IDbConnectionInternals)TestConnection).ADOTransaction);
			var exporter = new TransactionBatchExporter(dataAccess, PopulateOptionalXUTFieldsSetting.AllTrue());
			exporter.CreateTransactionBatch(batch);

			var (eInvoice, _, _) = new RDNGlobalElectronicInvoiceBuilderForVietnam(batch.AIB_BatchNumber.ToString(), new AdditionalTransactionInfoForVietnamEInvoice() { OriginalTransactionPK = arInvoice.PK, CompanyPK = arInvoice.Company.PK, BranchPK = arInvoice.Branch.PK }).Create();

			AssertGEIMessage(eInvoice, batch, branch, MessageTypes.RequestDocumentForInvoice);

			var expectPayload = VietnamEInvoiceTestHelper.GetEmbeddedResourceAsString(VietnamEInvoiceRDNPayloadJson);

			var eInvoiceDocument = JsonConvert.DeserializeObject<VietnamEInvoiceDocument>(eInvoice.Payload.ToUTF8FromBase64());
			var actualPayload = JsonConvert.SerializeObject(eInvoiceDocument, Formatting.Indented);
			AssertEquals(expectPayload, actualPayload);
		}

		[TestDate(2020, 08, 24, 08, 36, 00)]
		public void TestRCNGlobalElectronicInvoiceMessageForTransactionBatchIsCreatedCorrectly()
		{
			var branch = CreateVietnamBranch();
			Factory.Save();

			AccEInvoicingBatch batch;
			InvoicingBase arInvoice;
			InvoicingBase arCreditNote;
			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, branch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				Helper.AddCustomsCodeForCountryIfMissing(branch.OrgProxy, Core.Constants.CountryCodes.VietNam, "VAT", "0100233488");

				arInvoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "001", TestObjectCreator.AUD, 1.0m, 100.00m, 10.00m, 100.00m, 10.00m, TestObjectCreator.Debtor, TestObjectCreator.CC1.PK);
				arInvoice.AH_GB = branch.PK;
				arInvoice.AH_XD_ComplianceBook = sequence.PK;
				arInvoice.AH_TransactionReference = "PREFIX00000001";

				arCreditNote = testObjectCreator.CreateARCreditNote("CRD001", testObjectCreator.ABIGAS, testObjectCreator.AUD, 1m, "Incorrect amount");
				arCreditNote.AH_TransactionBelongsToGroup = arInvoice.PK;
				arCreditNote.SupportingDocumentNumber = "This is a supporting document number";

				batch = TestObjectCreator.CreateEInvoicingBatch(100, Core.Constants.EInvoicingBatchState.Ready, GlbCompany.CurrentCompany);
				testObjectCreator.CreateEInvoicingTransactionPivot(batch, arCreditNote, Core.Constants.EInvoicingPivotState.Queued, Core.Constants.EInvoicingPivotActionType.Cancel);

				Factory.Save();
			}

			var additionalTransactionInfo = new AdditionalTransactionInfoForVietnamEInvoice()
			{
				OriginalTransactionPK = arInvoice.PK,
				OriginalTransactionReference = arInvoice.AH_TransactionReference,
				CompanyPK = arInvoice.Company.PK,
				BranchPK = arInvoice.Branch.PK,
				RectifyDate = arCreditNote.PostDate.Date.ToString("yyyy-MM-dd HH:mm"),
				RectifyReason = arCreditNote.AH_Desc,
				RectifySupportingDocumentNumber = arCreditNote.SupportingDocumentNumber,
				ComplianceSequenceInfo = new AdditionalComplianceSequenceInfo
				{
					OriginalSeriesPrefix = Prefix,
					OriginalIsComplianceNumberFormatDefault = true,
				}
			};

			var (eInvoice, _, _) = new RCNGlobalElectronicInvoiceBuilderForVietnam(batch.AIB_BatchNumber.ToString(), additionalTransactionInfo).Create();

			AssertGEIMessage(eInvoice, batch, branch, MessageTypes.CancelReceivablesInvoice);

			var expectedRawPayload = VietnamEInvoiceTestHelper.GetEmbeddedResourceAsString(VietnamEInvoiceRCNPayloadJson);
			var actualRawPayload = eInvoice.Payload.ToUTF8FromBase64();

			var expectedCancellation = JsonConvert.DeserializeObject<VietnamEInvoiceCancellation>(expectedRawPayload);
			var expectedPayload = JsonConvert.SerializeObject(expectedCancellation, Formatting.Indented);

			var actualCancellation = JsonConvert.DeserializeObject<VietnamEInvoiceCancellation>(actualRawPayload);
			var actualPayload = JsonConvert.SerializeObject(actualCancellation, Formatting.Indented);
			AssertEquals(expectedPayload, actualPayload);
		}

		[TestDate(2020, 08, 24, 08, 36, 00)]
		public void TestADJGlobalElectronicInvoiceMessageForTransactionBatchIsCreatedCorrectly()
		{
			var branch = CreateVietnamBranch();
			Factory.Save();

			AccEInvoicingBatch batch;
			InvoicingBase invoice;
			InvoicingBase creditNote;
			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, branch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				Helper.AddCustomsCodeForCountryIfMissing(branch.OrgProxy, Core.Constants.CountryCodes.VietNam, "VAT", "0100233488");

				invoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "001", TestObjectCreator.AUD, 1.0m, 100.00m, 10.00m, 100.00m, 10.00m, TestObjectCreator.Debtor, TestObjectCreator.CC1.PK);
				invoice.AH_GB = branch.PK;
				invoice.AH_TransactionReference = "AP/19E1";
				Factory.Save();

				creditNote = TestObjectCreator.CreateARCreditNoteWithLine("CRD001", TestObjectCreator.ABIGAS, TestObjectCreator.AUD, 1m, "Incorrect amount", null, TestObjectCreator.CC1, 60m, ZDateTime.UtcNow, false);
				creditNote.AH_TransactionBelongsToGroup = invoice.PK;
				creditNote.AH_TransactionReference = "AP/19E2";
				creditNote.SupportingDocumentNumber = "This is a supporting document number";

				batch = TestObjectCreator.CreateEInvoicingBatch(100, Core.Constants.EInvoicingBatchState.Ready, GlbCompany.CurrentCompany);
				testObjectCreator.CreateEInvoicingTransactionPivot(batch, creditNote, Core.Constants.EInvoicingPivotState.Batched, Core.Constants.EInvoicingPivotActionType.Adjustment);

				Factory.Save();
			}

			var dataAccess = new BatchExportDataAccess(((IDbConnectionInternals)TestConnection).ADOConnection, ((IDbConnectionInternals)TestConnection).ADOTransaction);
			var exporter = new TransactionBatchExporter(dataAccess, PopulateOptionalXUTFieldsSetting.AllTrue());
			var transactionBatch = exporter.CreateTransactionBatch(batch);

			var additionalTransactionInfo = new AdditionalTransactionInfoForVietnamEInvoice()
			{
				OriginalTransactionPK = invoice.PK,
				OriginalTransactionReference = invoice.AH_TransactionReference,
				TransactionReference = creditNote.AH_TransactionReference,
				CompanyPK = invoice.Company.PK,
				BranchPK = invoice.Branch.PK,
				RectifyDate = creditNote.PostDate.Date.ToString("yyyy-MM-dd HH:mm"),
				RectifyReason = creditNote.AH_Desc,
				RectifySupportingDocumentNumber = creditNote.SupportingDocumentNumber,
				ComplianceSequenceInfo = new AdditionalComplianceSequenceInfo
				{
					SeriesPrefix = Prefix,
					IsComplianceNumberFormatDefault = true,
				},
			};

			var (eInvoice, _, _) = new RAJGlobalElectronicInvoiceBuilderForVietnam(batch.AIB_BatchNumber.ToString(), transactionBatch, additionalTransactionInfo).Create();

			AssertGEIMessage(eInvoice, batch, branch, MessageTypes.AdjustReceivablesInvoice);

			var expectedRawPayload = VietnamEInvoiceTestHelper.GetEmbeddedResourceAsString(VietnamEInvoiceRAJPayloadJson);
			var actualRawPayload = eInvoice.Payload.ToUTF8FromBase64();

			var expectedAdjustment = JsonConvert.DeserializeObject<VietnamEInvoiceCancellation>(expectedRawPayload);
			var expectedPayload = JsonConvert.SerializeObject(expectedAdjustment, Formatting.Indented);

			var actualAdjustment = JsonConvert.DeserializeObject<VietnamEInvoiceCancellation>(actualRawPayload);
			var actualPayload = JsonConvert.SerializeObject(actualAdjustment, Formatting.Indented);
			AssertEquals(expectedPayload, actualPayload);
		}

		[TestDate(2020, 08, 24, 08, 36, 00)]
		public void TestAPRGlobalElectronicInvoiceMessageForTransactionBatchIsCreatedCorrectly()
		{
			var branch = CreateVietnamBranch();
			Factory.Save();

			AccEInvoicingBatch batch;
			InvoicingBase invoice;
			InvoicingBase creditNote;
			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, branch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				Helper.AddCustomsCodeForCountryIfMissing(branch.OrgProxy, Core.Constants.CountryCodes.VietNam, "VAT", "0100233488");

				invoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "001", TestObjectCreator.AUD, 1.0m, 100.00m, 10.00m, 100.00m, 10.00m, TestObjectCreator.Debtor, TestObjectCreator.CC1.PK);
				invoice.AH_GB = branch.PK;
				invoice.AH_TransactionReference = "AP/19E1";
				Factory.Save();

				creditNote = TestObjectCreator.CreateARCreditNoteWithLine("CRD001", TestObjectCreator.ABIGAS, TestObjectCreator.AUD, 1m, "Incorrect amount", null, TestObjectCreator.CC1, 60m, ZDateTime.UtcNow, false);
				creditNote.AH_TransactionBelongsToGroup = invoice.PK;
				creditNote.SupportingDocumentNumber = "This is a supporting document number";
				creditNote.AH_TransactionReference = "APCRD001";

				batch = TestObjectCreator.CreateEInvoicingBatch(100, Core.Constants.EInvoicingBatchState.Ready, GlbCompany.CurrentCompany);
				testObjectCreator.CreateEInvoicingTransactionPivot(batch, creditNote, Core.Constants.EInvoicingPivotState.Batched, Core.Constants.EInvoicingPivotActionType.Adjustment);

				Factory.Save();
			}

			var additionalTransactionInfo = new AdditionalTransactionInfoForVietnamEInvoice()
			{
				TransactionPK = creditNote.PK,
				ComplianceSequenceInfo = new AdditionalComplianceSequenceInfo
				{
					SeriesPrefix = Prefix,
					IsComplianceNumberFormatDefault = true,
				},
				CompanyPK = creditNote.Company.PK,
				BranchPK = creditNote.Branch.PK,
				TransactionReference = creditNote.AH_TransactionReference
			};

			var (eInvoice, _, _) = new RAPGlobalElectronicInvoiceBuilderForVietnam(batch.AIB_BatchNumber.ToString(), additionalTransactionInfo).Create();

			AssertGEIMessage(eInvoice, batch, branch, MessageTypes.ApproveReceivablesInvoice);

			var expectedRawPayload = VietnamEInvoiceTestHelper.GetEmbeddedResourceAsString(VietnamEInvoiceRAPPayloadJson);
			var actualRawPayload = eInvoice.Payload.ToUTF8FromBase64();

			var expectedAdjustment = JsonConvert.DeserializeObject<VietnamEInvoiceApprove>(expectedRawPayload);
			var expectedPayload = JsonConvert.SerializeObject(expectedAdjustment, Formatting.Indented);
			expectedPayload = expectedPayload.Replace("{sid}", creditNote.PK.ToString());

			var actualAdjustment = JsonConvert.DeserializeObject<VietnamEInvoiceApprove>(actualRawPayload);
			var actualPayload = JsonConvert.SerializeObject(actualAdjustment, Formatting.Indented);
			AssertEquals(expectedPayload, actualPayload);
		}

		void AssertGEIMessage(GlobalElectronicInvoicing eInvoice, AccEInvoicingBatch batch, GlbBranch branch, string messageType)
		{
			AssertEquals("MessagingSystem", "Vietnam electronic invoicing system", eInvoice.Header.ElectronicInvoiceBatchRequest.MessagingSystem);
			AssertEquals("MessageType", messageType, eInvoice.Header.ElectronicInvoiceBatchRequest.MessageType);
			AssertEquals("BatchNumber", batch.AIB_BatchNumber.ToString(), eInvoice.Header.ElectronicInvoiceBatchRequest.BatchNumber);
			AssertEquals("branch", branch.GB_Code, eInvoice.Header.ElectronicInvoiceBatchRequest.BranchCode);
			AssertEquals("Company", branch.Company.GC_Code, eInvoice.Header.ElectronicInvoiceBatchRequest.CompanyCode);
			AssertEquals("IsProductionSystemSpecified", true, eInvoice.Header.ElectronicInvoiceBatchRequest.IsProductionSystemSpecified);

			AssertEquals("Credentials", 2, eInvoice.Header.ElectronicInvoiceBatchRequest.Credentials.Count);
			AssertEquals("Credentials_username_key", nameof(Username), eInvoice.Header.ElectronicInvoiceBatchRequest.Credentials[0].Key);
			var username = eInvoice.Header.ElectronicInvoiceBatchRequest.Credentials[0].Value.Value;
			AssertEquals("Credentials_username_value", Username, username);

			AssertEquals("Credentials_password_key", nameof(Password), eInvoice.Header.ElectronicInvoiceBatchRequest.Credentials[1].Key);
			var password = eInvoice.Header.ElectronicInvoiceBatchRequest.Credentials[1].Value;
			Assert("Credentials_password_value", password.Encrypted);
			AssertEquals("Credentials_password_value", Password, EInvoicingTestHelper.DecodePassword(password.Value));
		}

		[TestDate(2019, 7, 16, 15, 26, 32)]
		public void TestGEIMessageForTransactionBatch_ProductionLicenceElement()
		{
			var branch = CreateVietnamBranch();
			Factory.Save();

			AccEInvoicingBatch batch;
			InvoicingBase arInvoice;
			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, branch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				Helper.AddCustomsCodeForCountryIfMissing(branch.OrgProxy, Core.Constants.CountryCodes.VietNam, "VAT", "12345675");

				batch = TestObjectCreator.CreateEInvoicingBatch(100, Core.Constants.EInvoicingBatchState.Ready, GlbCompany.CurrentCompany);
				arInvoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "001", TestObjectCreator.AUD, 1.0m, 100.00m, 10.00m, 100.00m, 10.00m, TestObjectCreator.Debtor, TestObjectCreator.CC1.PK);
				arInvoice.AH_GB = branch.PK;
				TestObjectCreator.CreateEInvoicingTransactionPivot(batch, arInvoice, Core.Constants.EInvoicingPivotState.Batched);
				arInvoice.AH_TransactionReference = "AP/19E1";
				Factory.Save();
			}

			var dataAccess = new BatchExportDataAccess(((IDbConnectionInternals)TestConnection).ADOConnection, ((IDbConnectionInternals)TestConnection).ADOTransaction);
			var exporter = new TransactionBatchExporter(dataAccess, PopulateOptionalXUTFieldsSetting.AllTrue());
			var transactionBatch = exporter.CreateTransactionBatch(batch);

			var additionalTransactionInfo = new AdditionalTransactionInfoForVietnamEInvoice()
			{
				OriginalTransactionPK = arInvoice.PK,
				OriginalTransactionReference = arInvoice.AH_TransactionReference,
				CompanyPK = arInvoice.Company.PK,
				BranchPK = arInvoice.Branch.PK
			};

			var (eInvoice, validationErrors, _) = new RINGlobalElectronicInvoiceBuilderForVietnam(batch.AIB_BatchNumber.ToString(), transactionBatch, additionalTransactionInfo).Create();

			Assert("Pre-condition: Should be Test system", !Env.Instance.IsProductionSystem);
			AssertEquals("Error", string.Empty, validationErrors.ToString());

			AssertEquals("IsProductionSystem", false, eInvoice.Header.ElectronicInvoiceBatchRequest.IsProductionSystem);

			LicenceTypeChanger.SetSystemLicence(DatabaseTypes.Codes.Production);
			Assert("Pre-condition: Should be Production system", Env.Instance.IsProductionSystem);
			(eInvoice, validationErrors, _) = new RINGlobalElectronicInvoiceBuilderForVietnam(batch.AIB_BatchNumber.ToString(), transactionBatch, additionalTransactionInfo).Create();
			AssertEquals("Error", string.Empty, validationErrors.ToString());
			AssertEquals("IsProductionSystem", true, eInvoice.Header.ElectronicInvoiceBatchRequest.IsProductionSystem);
		}

		[TestDate(2019, 7, 16, 15, 26, 32)]
		public void TestGEIMessageForTransactionBatch_IsProductionSystem()
		{
			var branch = CreateVietnamBranch();
			Factory.Save();

			AccEInvoicingBatch batch;
			InvoicingBase arInvoice;
			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, branch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				Helper.AddCustomsCodeForCountryIfMissing(branch.OrgProxy, Core.Constants.CountryCodes.VietNam, "VAT", "12345675");

				batch = TestObjectCreator.CreateEInvoicingBatch(100, Core.Constants.EInvoicingBatchState.Ready, GlbCompany.CurrentCompany);
				arInvoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "001", TestObjectCreator.AUD, 1.0m, 100.00m, 10.00m, 100.00m, 10.00m, TestObjectCreator.Debtor, TestObjectCreator.CC1.PK);
				arInvoice.AH_GB = branch.PK;
				TestObjectCreator.CreateEInvoicingTransactionPivot(batch, arInvoice, Core.Constants.EInvoicingPivotState.Batched);
				arInvoice.AH_TransactionReference = "AP/19E1";
				Factory.Save();
			}

			var dataAccess = new BatchExportDataAccess(((IDbConnectionInternals)TestConnection).ADOConnection, ((IDbConnectionInternals)TestConnection).ADOTransaction);
			var exporter = new TransactionBatchExporter(dataAccess, PopulateOptionalXUTFieldsSetting.AllTrue());
			var transactionBatch = exporter.CreateTransactionBatch(batch);

			var additionalTransactionInfo = new AdditionalTransactionInfoForVietnamEInvoice()
			{
				OriginalTransactionPK = arInvoice.PK,
				OriginalTransactionReference = arInvoice.AH_TransactionReference,
				CompanyPK = arInvoice.Company.PK,
				BranchPK = arInvoice.Branch.PK
			};

			AssertEReportingGEIMessageSystemType(arInvoice.Company.PK.ToGuid(), AccountingMasterFilesConstants.EReportingGEIMessageSystemTypeCodes.Default, Env.Instance.IsProductionSystem);
			AssertEReportingGEIMessageSystemType(arInvoice.Company.PK.ToGuid(), AccountingMasterFilesConstants.EReportingGEIMessageSystemTypeCodes.AlwaysProductionSystem, true);
			AssertEReportingGEIMessageSystemType(arInvoice.Company.PK.ToGuid(), AccountingMasterFilesConstants.EReportingGEIMessageSystemTypeCodes.AlwaysTestSystem, false);

			void AssertEReportingGEIMessageSystemType(Guid companyPk, string code, bool expectValue)
			{
				using (AccountingMasterFilesRegistry.Instance.EReportingGEIMessageSystemType.SetTemporaryValue(companyPk, Guid.Empty, Guid.Empty, code))
				{
					var (eInvoice, validationErrors, _) = new RINGlobalElectronicInvoiceBuilderForVietnam(batch.AIB_BatchNumber.ToString(), transactionBatch, additionalTransactionInfo).Create();
					AssertEquals("IsProductionSystem", expectValue, eInvoice.Header.ElectronicInvoiceBatchRequest.IsProductionSystem);
				}
			}
		}

		GlbBranch CreateVietnamBranch()
		{
			var company = TestObjectCreator.CreateCompanyAndBranch("VNHNA");
			var proxyOrg = TestObjectCreator.CreateOrgHeader("PXY", true, true, "VNHNA");
			var branch = company.Branches[0];
			branch.GB_OH_OrgProxy = proxyOrg.PK;
			branch.GB_RN_NKCountryCode = Core.Constants.CountryCodes.VietNam;
			Factory.Save();
			return branch;
		}

		const string Username = "admin";
		const string Password = "123456";
		const string Prefix = "PREFIX";
		const string VietnamEInvoiceRCNPayloadJson = "VietnamEInvoiceRCNPayload.json";
		const string VietnamEInvoiceRDNPayloadJson = "VietnamEInvoiceRDNPayload.json";
		const string VietnamEInvoiceRAJPayloadJson = "VietnamEInvoiceRAJPayload.json";
		const string VietnamEInvoiceRAPPayloadJson = "VietnamEInvoiceRAPPayload.json";
		AccComplianceSequence sequence;

		protected override void SetUp()
		{
			base.SetUp();
			Helper.SetupControlAccounts();

			AccountingConfigurationRegistry.Instance.VietnamEInvoicingUserName.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Username);
			AccountingConfigurationRegistry.Instance.VietnamEInvoicingPassword.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Password);
			AccountingConfigurationRegistry.Instance.VietnamEInvoicingFormNumber.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "world_peace");

			sequence = Factory.NewWithValidTestData<AccComplianceSequence>();
			sequence.XD_GC_Company = GlbCompany.CurrentCompany.PK;
			sequence.XD_GB_BranchOwner = GlbBranch.CurrentBranch.PK;
			sequence.XD_Code = "AAA";
			sequence.XD_SequenceClass = VietnamComplianceInfo.ComplianceSubTypeCodes.TXI;
			sequence.XD_Prefix = Prefix;
			sequence.XD_NumberFormat = AccComplianceSequenceLookups.ComplianceNumberFormatDefault.Code;
			sequence.XD_IsActive = true;

			Factory.Save();
		}

		TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;

		EInvoicingTestHelper Helper => helper ?? (helper = new EInvoicingTestHelper(TestObjectCreator));
		EInvoicingTestHelper helper;
	}
}
