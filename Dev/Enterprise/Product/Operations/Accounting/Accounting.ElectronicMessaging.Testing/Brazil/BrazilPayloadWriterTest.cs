using System;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Reversing;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing.Testing;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.ZArchitecture.Schema;
using Newtonsoft.Json;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.ElectronicMessaging.Brazil.Testing
{
	class BrazilPayloadWriterTest : TransactionBatchToPayloadWriterBaseTest
	{
		protected override TransactionBatchToPayloadWriterBase GetTestWriter() => new BrazilPayloadWriter();

		public void TestUnknownMessageType_Throws()
		{
			var accBatch = Factory.NewWithValidTestData<AccEInvoicingBatch>();
			AssertExceptionThrown<ArgumentException>(()
				=> ((ITransactionBatchToPayloadWriter)new BrazilPayloadWriter()).WritePayloadToStream(new TransactionBatch(), new MemoryStream(), "ZZZ", accBatch, new Common.Logger(), new Common.Logger())
			);
		}

		#region GEN Message Type

		public void TestMessageTypeGEN_WithPSTAllocationMethod_WritesNoPayload()
		{
			using (var stream = new MemoryStream())
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Brazil))
			using (AccountingMasterFilesRegistry.Instance.ComplianceDocumentNumberAllocation_Receivables.SetTemporaryValue(
					Env.CurrentCompanyPK, Guid.Empty, Guid.Empty,
					AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post))
			{
				var errors = new Common.Logger();
				var warnings = new Common.Logger();
				var accBatch = SetUpInvoiceBatch();

				((ITransactionBatchToPayloadWriter)new BrazilPayloadWriter()).WritePayloadToStream(new TransactionBatch(), stream, BrazilEInvoiceAPICommandList.Codes.GenerateInvoiceRequest, accBatch, errors, warnings);

				AssertNullOrEmpty(errors.ToString());
				AssertNullOrEmpty(warnings.ToString());
				var actualJson = Encoding.UTF8.GetString(stream.ToArray());
				AssertNullOrEmpty(actualJson);
			}
		}

		public void TestMessageTypeGEN_AllocationMethodAppliesToBranch_WritesNoPayload()
		{
			var nonCurrentBranchPK = GlbCompany.CurrentCompany.ActiveBranches.First(x => x.PK != Env.CurrentBranchPK).PK;

			var collection = new ComplianceSubTypeAllocationOverrideConfigurationCollection(null, Factory, CountryCodes.Brazil);
			var nfeAndBranch = collection.AddNew();
			nfeAndBranch.SubType = BrazilComplianceInfo.ComplianceSubTypeCodes.NFS;
			nfeAndBranch.BranchPK = nonCurrentBranchPK;
			nfeAndBranch.AllocationMethod = AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post;

			using (var stream = new MemoryStream())
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Brazil))
			using (AccountingMasterFilesRegistry.Instance.ComplianceDocumentNumberAllocationOverride_Receivables.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, collection))
			using (AccountingMasterFilesRegistry.Instance.ComplianceDocumentNumberAllocation_Receivables.SetTemporaryValue(
					Env.CurrentCompanyPK, Guid.Empty, Guid.Empty,
					AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.GovermentNumberAllocate))
			{
				var errors = new Common.Logger();
				var warnings = new Common.Logger();
				var accBatch = SetUpInvoiceBatch(branchPK: nonCurrentBranchPK);

				((ITransactionBatchToPayloadWriter)new BrazilPayloadWriter()).WritePayloadToStream(new TransactionBatch(), stream, BrazilEInvoiceAPICommandList.Codes.GenerateInvoiceRequest, accBatch, errors, warnings);

				AssertNullOrEmpty(errors.ToString());
				AssertNullOrEmpty(warnings.ToString());
				var actualJson = Encoding.UTF8.GetString(stream.ToArray());
				AssertNullOrEmpty(actualJson);
			}
		}

		public void TestMessageTypeGEN_WithoutComplianceBook_WithGVTAllocationMethod_AddsNotification()
		{
			using (var stream = new MemoryStream())
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Brazil))
			using (AccountingMasterFilesRegistry.Instance.ComplianceDocumentNumberAllocation_Receivables.SetTemporaryValue(
					Env.CurrentCompanyPK, Guid.Empty, Guid.Empty,
					AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.GovermentNumberAllocate))
			{
				var errors = new Common.Logger();
				var warnings = new Common.Logger();
				var accBatch = SetUpInvoiceBatch(createComplianceBook: false);

				((ITransactionBatchToPayloadWriter)new BrazilPayloadWriter()).WritePayloadToStream(new TransactionBatch(), stream, BrazilEInvoiceAPICommandList.Codes.GenerateInvoiceRequest, accBatch, errors, warnings);

				AssertEquals(0L, stream.Length);
				AssertEquals("No Compliance Sequence was found for the AR INV transaction using GVT Compliance Document Number Allocation Method.", errors.ToString());
				AssertNullOrEmpty(warnings.ToString());
			}
		}

		public void TestMessageTypeGEN_WithoutComplianceSequenceForeignKey_WithGVTAllocationMethod_WritesJSONPayload()
		{
			using (var stream = new MemoryStream())
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Brazil))
			using (AccountingMasterFilesRegistry.Instance.ComplianceDocumentNumberAllocation_Receivables.SetTemporaryValue(
					Env.CurrentCompanyPK, Guid.Empty, Guid.Empty,
					AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.GovermentNumberAllocate))
			{
				var errors = new Common.Logger();
				var warnings = new Common.Logger();
				var accBatch = SetUpInvoiceBatch(compliancePrefix: "12", createComplianceBook: true, setInvoiceComplianceBook: false);

				((ITransactionBatchToPayloadWriter)new BrazilPayloadWriter()).WritePayloadToStream(new TransactionBatch(), stream, BrazilEInvoiceAPICommandList.Codes.GenerateInvoiceRequest, accBatch, errors, warnings);

				AssertNullOrEmpty(errors.ToString());
				AssertNullOrEmpty(warnings.ToString());
				var actualJson = Encoding.UTF8.GetString(stream.ToArray());
				var actualObject = JsonConvert.DeserializeObject<dynamic>(actualJson);
				AssertEquals("12", actualObject.ComplianceBookPrefix.ToString());
			}
		}

		public void TestMessageTypeGEN_WithComplianceBookAndBlankPrefixAndGVTAllocationMethod_WritesJSONPayload()
		{
			using (var stream = new MemoryStream())
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Brazil))
			using (AccountingMasterFilesRegistry.Instance.ComplianceDocumentNumberAllocation_Receivables.SetTemporaryValue(
					Env.CurrentCompanyPK, Guid.Empty, Guid.Empty,
					AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.GovermentNumberAllocate))
			{
				var errors = new Common.Logger();
				var warnings = new Common.Logger();
				var accBatch = SetUpInvoiceBatch(compliancePrefix: "", createComplianceBook: true, setInvoiceComplianceBook: true);

				((ITransactionBatchToPayloadWriter)new BrazilPayloadWriter()).WritePayloadToStream(new TransactionBatch(), stream, BrazilEInvoiceAPICommandList.Codes.GenerateInvoiceRequest, accBatch, errors, warnings);

				AssertNullOrEmpty(errors.ToString());
				AssertNullOrEmpty(warnings.ToString());
				var actualJson = Encoding.UTF8.GetString(stream.ToArray());
				var actualObject = JsonConvert.DeserializeObject<dynamic>(actualJson);
				AssertEquals("", actualObject.ComplianceBookPrefix.ToString());
			}
		}

		public void TestMessageTypeGEN_WithComplianceBookAndPrefixAndGVTAllocationMethod_WritesJSONPayload()
		{
			using (var stream = new MemoryStream())
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Brazil))
			using (AccountingMasterFilesRegistry.Instance.ComplianceDocumentNumberAllocation_Receivables.SetTemporaryValue(
					Env.CurrentCompanyPK, Guid.Empty, Guid.Empty,
					AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.GovermentNumberAllocate))
			{
				var errors = new Common.Logger();
				var warnings = new Common.Logger();
				var accBatch = SetUpInvoiceBatch(compliancePrefix: "42", createComplianceBook: true, setInvoiceComplianceBook: true);

				((ITransactionBatchToPayloadWriter)new BrazilPayloadWriter()).WritePayloadToStream(new TransactionBatch(), stream, BrazilEInvoiceAPICommandList.Codes.GenerateInvoiceRequest, accBatch, errors, warnings);

				AssertNullOrEmpty(errors.ToString());
				AssertNullOrEmpty(warnings.ToString());
				var actualJson = Encoding.UTF8.GetString(stream.ToArray());
				var actualObject = JsonConvert.DeserializeObject<dynamic>(actualJson);
				AssertEquals("42", actualObject.ComplianceBookPrefix.ToString());
			}
		}

		#endregion

		#region CAN Message Type

		public void TestMessageTypeCAN_WithoutGovernmentAllocatedNumber_AddsNotification()
		{
			using (var stream = new MemoryStream())
			{
				var errors = new Common.Logger();
				var warnings = new Common.Logger();
				var accBatch = Factory.NewWithValidTestData<AccEInvoicingBatch>();

				((ITransactionBatchToPayloadWriter)new BrazilPayloadWriter()).WritePayloadToStream(new TransactionBatch(), stream, BrazilEInvoiceAPICommandList.Codes.GenerateCancellationRequest, accBatch, errors, warnings);

				AssertEquals(0L, stream.Length);
				AssertEquals("No government allocated number (NF-e number) was found for original AR INV transaction.", errors.ToString());
				AssertNullOrEmpty(warnings.ToString());
			}
		}

		public void TestMessageTypeCAN_WithoutInvoiceVerificationCode_NoNotifications()
		{
			using (var stream = new MemoryStream())
			{
				var errors = new Common.Logger();
				var warnings = new Common.Logger();
				var accBatch = SetUpInvoiceReversalBatch("63432");

				((ITransactionBatchToPayloadWriter)new BrazilPayloadWriter()).WritePayloadToStream(new TransactionBatch(), stream, BrazilEInvoiceAPICommandList.Codes.GenerateCancellationRequest, accBatch, errors, warnings);

				AssertEquals(68L, stream.Length);
				AssertNullOrEmpty(errors.ToString());
				AssertNullOrEmpty(warnings.ToString());
			}
		}

		public void TestMessageTypeCAN_WithGovernmentAllocatedNumberAndInvoiceVerificationCode_WritesJSONPayload()
		{
			using (var stream = new MemoryStream())
			{
				var errors = new Common.Logger();
				var warnings = new Common.Logger();
				var accBatch = SetUpInvoiceReversalBatch("63432", "1098765");

				((ITransactionBatchToPayloadWriter)new BrazilPayloadWriter()).WritePayloadToStream(new TransactionBatch(), stream, BrazilEInvoiceAPICommandList.Codes.GenerateCancellationRequest, accBatch, errors, warnings);

				AssertNullOrEmpty(errors.ToString());
				AssertNullOrEmpty(warnings.ToString());
				var actualJson = Encoding.UTF8.GetString(stream.ToArray());
				var actualObject = JsonConvert.DeserializeObject<dynamic>(actualJson);
				AssertEquals("63432", actualObject.GovernmentAllocatedNumber.ToString());
				AssertEquals("1098765", actualObject.InvoiceVerificationCode.ToString());
			}
		}

		public void TestMessageTypeCAN_WithGovernmentAllocatedNumberAndInvoiceVerificationCodeAsBlobs_WritesJSONPayload()
		{
			using (var stream = new MemoryStream())
			{
				var errors = new Common.Logger();
				var warnings = new Common.Logger();
				var accBatch = SetUpInvoiceReversalBatch("aGY4NzQzaGZpZXdydWhmZ2ZkZ2VlNHRnNGU=", "SW52b2ljZVZlcmlmaWNhdGlvbkNvZGU=");

				((ITransactionBatchToPayloadWriter)new BrazilPayloadWriter()).WritePayloadToStream(new TransactionBatch(), stream, BrazilEInvoiceAPICommandList.Codes.GenerateCancellationRequest, accBatch, errors, warnings);

				AssertNullOrEmpty(errors.ToString());
				AssertNullOrEmpty(warnings.ToString());
				var actualJson = Encoding.UTF8.GetString(stream.ToArray());
				var actualObject = JsonConvert.DeserializeObject<dynamic>(actualJson);
				AssertEquals("aGY4NzQzaGZpZXdydWhmZ2ZkZ2VlNHRnNGU=", actualObject.GovernmentAllocatedNumber.ToString());
				AssertEquals("SW52b2ljZVZlcmlmaWNhdGlvbkNvZGU=", actualObject.InvoiceVerificationCode.ToString());
			}
		}

		public void TestMessageTypeCAN_WithGovernmentAllocatedGuid_WritesJSONPayload()
		{
			using (var stream = new MemoryStream())
			{
				var errors = new Common.Logger();
				var warnings = new Common.Logger();
				var accBatch = SetUpInvoiceReversalBatch("97f7e928-9a4c-4555-a9e1-8fd6e10d854f", "8f127783-336c-45c1-b54d-d5f10c0461ef");

				((ITransactionBatchToPayloadWriter)new BrazilPayloadWriter()).WritePayloadToStream(new TransactionBatch(), stream, BrazilEInvoiceAPICommandList.Codes.GenerateCancellationRequest, accBatch, errors, warnings);

				AssertNullOrEmpty(errors.ToString());
				AssertNullOrEmpty(warnings.ToString());
				var actualJson = Encoding.UTF8.GetString(stream.ToArray());
				var actualObject = JsonConvert.DeserializeObject<dynamic>(actualJson);
				AssertEquals("97f7e928-9a4c-4555-a9e1-8fd6e10d854f", actualObject.GovernmentAllocatedNumber.ToString());
				AssertEquals("8f127783-336c-45c1-b54d-d5f10c0461ef", actualObject.InvoiceVerificationCode.ToString());
			}
		}

		#endregion

		#region Implementation

		AccEInvoicingBatch SetUpInvoiceBatch(string compliancePrefix = "", bool createComplianceBook = true, bool setInvoiceComplianceBook = true, ZGuid? branchPK = null)
		{
			var creator = new TestObjectCreator(Factory);

			var arInvoice = creator.CreateInvoiceWithLine(typeof(ARInvoice), "INV0001", creator.AUD, 1m, 100m, 0m, 100m, 0m, creator.AALSHI, creator.CC1.PK);
			if (branchPK != null)
			{
				arInvoice.AH_GB = branchPK.Value;
			}
			arInvoice.AH_ComplianceSubType = BrazilComplianceInfo.ComplianceSubTypeCodes.NFS;

			if (createComplianceBook)
			{
				var sequence = creator.SetupComplianceSequence(ZGuid.Empty, BrazilComplianceInfo.ComplianceSubTypeCodes.NFS, compliancePrefix, 1, 100, 1, allocationLevel: "BRN");
				if (setInvoiceComplianceBook)
				{
					arInvoice.AH_XD_ComplianceBook = sequence.PK;
				}
			}

			var pivot = creator.CreateEInvoicingTransactionPivot(arInvoice);
			var result = creator.CreateEInvoicingBatchForPivot(pivot, 10, Constants.EInvoicingBatchState.Ready);
			Factory.Save();

			return result;
		}

		AccEInvoicingBatch SetUpInvoiceReversalBatch(string governmentAllocatedNumber, string invoiceVerificationCode = null)
		{
			var creator = new TestObjectCreator(Factory);
			var arInvoiceToReverse = creator.CreateInvoiceWithLine(typeof(ARInvoice), "INV0001", creator.AUD, 1m, 100m, 0m, 100m, 0m, creator.AALSHI, creator.CC1.PK);
			Factory.Save();

			if (invoiceVerificationCode != null)
			{
				var authorisation = Factory.New<AccTransactionHeaderAuthorisationRecord>();
				authorisation.AHF_ParentId = arInvoiceToReverse.PK;
				authorisation.AHF_ParentTableCode = AccTransactionHeaderSchema.Constants.Prefix;
				authorisation.AHF_Number = invoiceVerificationCode;
				// Other required propertues
				authorisation.AHF_DateTime = DateTime.Now;
				authorisation.AHF_IDType = "GVT";
				authorisation.AHF_Counter = "1";
				authorisation.AHF_RecordType = AccTransactionHeaderAuthorisationRecordTypes.Brazil;
			}

			var reverser = new ReversingFactory().NewReversing(arInvoiceToReverse);
			reverser.Reverse();
			var reversingCreditNote = arInvoiceToReverse.ReverseInvoice;
			reversingCreditNote.AH_TransactionNum = "REV0001";
			reversingCreditNote.AH_ReceiptType = "IDE";

			var creditNotePivot = creator.CreateEInvoicingTransactionPivot(reversingCreditNote);
			var result = creator.CreateEInvoicingBatchForPivot(creditNotePivot, 10, Constants.EInvoicingBatchState.Ready);
			result.AIB_GovernmentAllocatedNumber = governmentAllocatedNumber;
			Factory.Save();

			return result;
		}

		#endregion
	}
}
