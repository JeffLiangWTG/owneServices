using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Logging;
using Enterprise.Customs.Common;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.FR.Business.EdiMessages;
using Enterprise.Customs.FR.Business.MasterFiles;
using Enterprise.Customs.FR.Business.Testing;
using Enterprise.Customs.FR.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Customs.FR.Business.MessageProcessors.Testing
{
	abstract class FRAutoDeadLineExtensionMessageSenderTest : TestCaseWithFactory
	{
		public void AssertAutomationProcessorWorkingCorrectly()
		{
			GlbBranch branch;
			OrgHeader declarant, importer, supplier;
			GenerateImporterDeclarantAndBranch(out branch, out declarant, out importer, out supplier);
			Factory.Save();

			var automaticAdvanceDate = new AutomatedModification();
			automaticAdvanceDate.EnableAutomatedModification = true;
			automaticAdvanceDate.TimeByDefault = new ZDateTime(2023, 1, 1, 11, 30, 00);
			var branchPK = branch.PK;

			var entryHeader1 = CreateEntryHeaderForTesting(branchPK, "19212081312", ZDateTime.Today.AddDays(-60), CandidateEntriesRequiredStatus, declarant.MainAddress.PK, importer.PK, supplier.PK, DeltaMode);
			var entryHeader2 = CreateEntryHeaderForTesting(branchPK, "19212081313", ZDateTime.Today.AddDays(-61), CandidateEntriesRequiredStatus, declarant.MainAddress.PK, importer.PK, supplier.PK, DeltaMode);
			var entryHeader3 = CreateEntryHeaderForTesting(branchPK, "19212081314", ZDateTime.Today.AddDays(-15), UnsuitableEntryStatus, declarant.MainAddress.PK, importer.PK, supplier.PK, DeltaMode);
			var entryHeader4 = CreateEntryHeaderForTesting(branchPK, "19212081315", ZDateTime.Today.AddDays(-60), CandidateEntriesRequiredStatus, declarant.MainAddress.PK, importer.PK, supplier.PK, DeltaMode);
			entryHeader4.CH_CEI_Instruction = ZGuid.Empty;

			Factory.Save();

			using (FRCustomsDataRegistry.Instance.FRAutomatedModification.SetTemporaryValue(Guid.Empty, branchPK.ToGuid(), Guid.Empty, automaticAdvanceDate))
			{
				var logger = new TestServiceLogger();
				var processor = new FRAutoDeadLineExtensionMessageSenderForTest(new LoggerWrapper(logger), DeltaMode, MessageType, CandidateEntriesRequiredStatus, IsUCC6);
				processor.Process(GlbCompany.CurrentCompany);

				AssertEquals($"A {MessageType} message should have been automatically sent.", 1, entryHeader1.Messages.Count);
				AssertEquals($"A {MessageType} message should not have been automatically sent because it's issue date exceeds 60 days.", 0, entryHeader2.Messages.Count);
				AssertEquals($"A {MessageType} message should not have been automatically sent because it's status doesn't allow for it.", 0, entryHeader3.Messages.Count);
				AssertEquals($"A {MessageType} message should not have been automatically sent because it has no entryInstruction.", 0, entryHeader4.Messages.Count);

				var message = entryHeader1.Messages[0];

				CombineAssertions($"Entry status, sequence number and auto sent {MessageType} message content", () =>
				{
					AssertEquals("CH_Status", MessageStatusCodeList.Codes.AWR, entryHeader1.CH_Status);
					AssertEquals("CH_SequenceNumber", 1, entryHeader1.CH_SequenceNumber);
					if (!IsUCC6)
					{
						AssertContains("Action code in EM_MessageText", $"<codact>{DeltaActionCode}</codact>", message.EM_MessageText);
						AssertContains("Entry number in EM_MessageText", "<refdos>19212081312</refdos>", message.EM_MessageText);
					}
					AssertEquals("EM_ApplicationCode", FREDIMessage.ApplicationCodes.FRCustomsMessage, message.EM_ApplicationCode);
					AssertEquals("EM_Status", EDIMessage.Status.Queued, message.EM_Status);
					AssertEquals("EM_ReceiveTransmit", EDIMessage.Direction.Transmit, message.EM_ReceiveTransmit);
					AssertEquals("EM_MessageSubType", MessageType, message.EM_MessageSubType);
					AssertEquals("Date for duty", ZDateTime.Today.AddDays(1), entryHeader1.EntryInstruction.CEI_DateForDuty);
				});
			}

			using (FRCustomsDataRegistry.Instance.FRAutomatedModification.SetTemporaryValue(Guid.Empty, branchPK.ToGuid(), Guid.Empty, automaticAdvanceDate))
			{
				var logger = new TestServiceLogger();
				var processor = new FRAutoDeadLineExtensionMessageSender(new LoggerWrapper(logger), DeltaMode);
				processor.Process(GlbCompany.CurrentCompany);

				AssertEquals("No new message should have been sent as all entries have been previously processed.", 1, entryHeader1.Messages.Count);
			}
		}

		void GenerateImporterDeclarantAndBranch(out GlbBranch branch, out OrgHeader declarant, out OrgHeader importer, out OrgHeader supplier)
		{
			var company = GlbCompany.CurrentCompany;
			branch = company.Branches.AddNew();
			branch.GB_Code = "FR1";
			company.Factory.Save();

			declarant = Factory.NewWithValidTestData<OrgHeader>();
			declarant.MainAddress.FillWithValidTestData();
			if (IsUCC6)
			{
				declarant.SetupAccount(OrgCusAccountCodeList.Codes.DEC, OrgCusAccountDeltaIETypeList.Codes.DCN, "DIE001", ZString.Empty, ZString.Empty, "B26F06FF");
			}
			else
			{
				declarant.SetupAccount(OrgCusAccountCodeList.Codes.DGI, OrgCusAccountDeltaGTypeList.Codes.G1, "DGI001", ZString.Empty, ZString.Empty, "C0E1C9EB");
				declarant.SetupAccount(OrgCusAccountCodeList.Codes.DGI, OrgCusAccountDeltaGTypeList.Codes.G2, "DGI002", ZString.Empty, ZString.Empty, "C0E1C9EC");
			}
			declarant.CustomsCodes.AddNew(OrgCusCode.FranceCodeTypes.Siret, "12345678", Core.Constants.CountryCodes.France);
			declarant.CustomsCodes.AddNew(OrgCusCode.CodeTypes.BrokerageRegistration, "12345679", Core.Constants.CountryCodes.France);
			declarant.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "FR12345679", Core.Constants.CountryCodes.France);
			var frOrgImpAddInfo2 = FROrgImpAddInfo.Get(declarant);
			frOrgImpAddInfo2.ZO_DeltaG1SubProcedure = DeltaG1SubProcedureList.Codes.C;

			importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.MainAddress.OA_PostCode = "123456";
			if (IsUCC6)
			{
				importer.SetupAccount(OrgCusAccountCodeList.Codes.DEC, OrgCusAccountDeltaIETypeList.Codes.DCN, "DEC001", ZString.Empty, ZString.Empty, "B26F06FF");
			}
			else
			{
				importer.SetupAccount(OrgCusAccountCodeList.Codes.DGI, OrgCusAccountDeltaGTypeList.Codes.G1, "DGI003", ZString.Empty, ZString.Empty, "C0E1C9EB");
				importer.SetupAccount(OrgCusAccountCodeList.Codes.DGI, OrgCusAccountDeltaGTypeList.Codes.G2, "DGI004", ZString.Empty, ZString.Empty, "C0E1C9EC");
			}
			importer.CustomsCodes.AddNew(OrgCusCode.FranceCodeTypes.TVA, "12345678", Core.Constants.CountryCodes.France);
			importer.CustomsCodes.AddNew(OrgCusCode.FranceCodeTypes.Siret, "12345679", Core.Constants.CountryCodes.France);
			importer.CustomsCodes.AddNew(OrgCusCode.CodeTypes.BrokerageRegistration, "12345679", Core.Constants.CountryCodes.France);
			var frOrgImpAddInfo = FROrgImpAddInfo.Get(importer);
			frOrgImpAddInfo.ZO_DeltaG1SubProcedure = DeltaG1SubProcedureList.Codes.C;

			supplier = Factory.NewWithValidTestData<OrgHeader>();
			var frOrgImpAddInfo3 = FROrgImpAddInfo.Get(supplier);
			frOrgImpAddInfo3.ZO_DeltaG1SubProcedure = DeltaG1SubProcedureList.Codes.C;
		}

		Declaration.CusEntryHeader CreateEntryHeaderForTesting(ZGuid branchPK, ZString entryReferenceNumber, ZDateTime entryNumberDate, ZString jeEntryStatus, ZGuid declarantAddress, ZGuid importer, ZGuid supplier, ZString deltaMode)
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.CustomsEntryInstructions.RemoveAndDeleteAll();

			declaration.JE_OH_Importer = importer;
			declaration.JE_OH_Supplier = supplier;
			declaration.JE_MessageType = "IMP";
			declaration.JE_ApplicationCode = "BLT";
			declaration.JE_MasterBill = "UnitTest";
			declaration.JE_EntryStatus = jeEntryStatus;
			declaration.JE_GB = branchPK;
			declaration.JE_GC = GlbCompany.CurrentCompany.PK;
			declaration.JE_OA_DeclarantAddress = declarantAddress;
			if (IsUCC6)
			{
				declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
				declaration.JE_CustomsProfile = "DIE001";
			}
			else
			{
				declaration.JE_DeltaMode = deltaMode;
				declaration.JE_CustomsProfile = deltaMode == OrgCusAccountDeltaGTypeList.Codes.G2 ? "DGI002" : "DGI001";
			}

			var cei = declaration.CustomsEntryInstructions.AddNew();

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();

			invoiceLine.JI_Tariff = "2203001010";
			invoiceLine.JI_Description = "Unit Test";
			invoiceLine.JI_LinePrice = 50;
			invoiceLine.JI_CEI = cei.PK;
			invoiceLine.JI_CustomsQuantity = 1m;
			invoiceLine.JI_NetWeightUQ = "KG";
			invoiceLine.JI_NetWeight = 1m;
			invoiceLine.JI_Weight = 2m;
			invoiceLine.JI_WeightUQ = "KG";

			cei.CEI_DateForDuty = ZDateTime.Today;

			Assert("Merge failed", declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer()));

			foreach (SupportingDocument item in declaration.SupportingDocuments)
			{
				if (item.CSI_DateOfIssue == ZDateTime.Empty)
				{
					item.CSI_DateOfIssue = ZDateTime.Today;
				}
			}

			var entryHeader = declaration.CustomsEntryHeaders[0];
			entryHeader.CH_EntryStatus = jeEntryStatus;
			entryHeader.CH_BGMReference = entryReferenceNumber;

			var cusNum = Factory.NewWithValidTestData<CusEntryNumber>();
			cusNum.CE_ParentID = entryHeader.PK;
			cusNum.CE_Category = "CUS";
			cusNum.CE_EntryType = "IMP";
			cusNum.CE_ParentTable = CusEntryHeaderSchema.Constants.TableName;
			cusNum.CE_RN_NKCountryCode = Core.Constants.CountryCodes.France;
			cusNum.CE_EntryNum = "1111";
			cusNum.CE_IssueDate = entryNumberDate;

			return entryHeader;
		}

		protected ZString DeltaMode { get; set; }

		protected int DeltaActionCode { get; set; }

		protected abstract ZString MessageType { get; }

		protected abstract ZString CandidateEntriesRequiredStatus { get; }

		protected abstract ZString UnsuitableEntryStatus { get; }

		protected abstract bool IsUCC6 { get; }
	}

	class FRAutoDeadLineExtensionMessageSenderForTest : FRAutoDeadLineExtensionMessageSender
	{
		public FRAutoDeadLineExtensionMessageSenderForTest(ICommonLogger logger, ZString deltaMode, ZString messageType, ZString candidateEntriesRequiredStatus, bool isUCC6) : base(logger, deltaMode)
		{
			this.messageType = messageType;
			this.candidateEntriesRequiredStatus = candidateEntriesRequiredStatus;
			this.isUCC6 = isUCC6;
		}

		protected override ZString MessageType => messageType;
		readonly ZString messageType;

		protected override ZString CandidateEntriesRequiredStatus => candidateEntriesRequiredStatus;
		readonly ZString candidateEntriesRequiredStatus;

		protected override bool IsUCC6 => isUCC6;
		readonly bool isUCC6;
	}
}
