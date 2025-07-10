using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.TS305;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.TS315V;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.TS316;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.TS333;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using AISUCC5InterchangeProcessorTestHelper = Enterprise.Customs.IE.Messaging.UCC5.Testing.AISUCC5InterchangeProcessorTestHelper;

namespace Enterprise.Customs.IE.Business.CusTempStorage.Testing
{
	[TestedType(typeof(TemporaryStorageHeader))]
	sealed class TemporaryStorageHeaderTest : EU.Business.CusTempStorage.Testing.TemporaryStorageHeaderAbstractTest<TemporaryStorageHeader>
	{
		public void TestAMA_TransportMeans_Caption()
		{
			AssertEquals("Border Transport Type", Factory.New<TemporaryStorageHeader>().AMA_TransportMeansInfo.GetAttribute<ResourceStringDataAttribute>().Caption);
		}

		public void TestAMA_VesselName_Caption()
		{
			AssertEquals("Border Transport ID", Factory.New<TemporaryStorageHeader>().AMA_VesselNameInfo.GetAttribute<ResourceStringDataAttribute>().Caption);
		}

		public void TestAMA_AgentType_Caption()
		{
			this.AssertDataBoundResourceStringsWithMultipleResourceKey(Factory.New<TemporaryStorageHeader>().AMA_AgentTypeInfo, string.Empty, "Rep. Status", fullDescription: "Representative Status");
		}

		public void TestAMA_AgentType_DefaultValue()
		{
			AssertEquals(ZString.Empty, Factory.New<TemporaryStorageHeader>().AMA_AgentType);
		}

		public void TestIsUCC5()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			header.AMA_ManifestType = ImportDeclarationApplicationCodeList.Codes.V1;
			AssertEquals("IsUCC5 should be true when AMA_ManifestType = V1", true, header.IsUCC5);
			header.AMA_ManifestType = ImportDeclarationApplicationCodeList.Codes.V2;
			AssertEquals("IsUCC5 should be false when AMA_ManifestType is any value other than V1", false, header.IsUCC5);
		}

		public void TestCustomsStatusDescription()
		{
			var storageHeader = Factory.New<TemporaryStorageHeader>();

			AssertEquals("Empty code", ZString.Empty, storageHeader.CustomsStatusDescription);

			storageHeader.CustomsStatus = AISEntryStatusList.Codes.Accepted;
			AssertEquals("Valid code", AISEntryStatusList.Descriptions.Accepted, storageHeader.CustomsStatusDescription);

			storageHeader.CustomsStatus = "@@@";
			AssertEquals("Invalid code", ZString.Empty, storageHeader.CustomsStatusDescription);
		}

		public void TestBills()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			AssertType<EU.Business.CusTempStorage.TemporaryStorageBillCollection<TemporaryStorageBill, TemporaryStorageHeader>>(header.Bills);
		}

		public void TestMasterBill()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			AssertType<TemporaryStorageBill>(header.MasterBill);
		}

		public void TestGetBillType()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			AssertEquals(typeof(TemporaryStorageBill), header.GetBillType());
		}

		public void TestIMessageAttacheeMembers()
		{
			var branch = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK).Branches.AddNew();
			var header = Factory.New<TemporaryStorageHeader>();
			header.AMA_GB = branch.PK;
			var customsAgent = Factory.New<GlbStaff>();
			customsAgent.GS_Code = "!2#";
			header.AMA_GS_NKCustomsAgent = "!2#";
			header.AMA_MessageStatus = "MS1";
			header.CustomsStatus = "ES2";
			header.MRN = "MRN001";
			var message = header.Messages.AddNew();
			message.EM_MessageNum = "MN1";
			IMessageAttachee messageAttachee = header;
			AssertEquals("PK", header.PK, messageAttachee.PK);
			AssertEquals("TableName", AsycudaManifestHeaderSchema.Constants.TableName, messageAttachee.TableName);
			AssertEquals("TablePrefix", AsycudaManifestHeaderSchema.Constants.Prefix, messageAttachee.TablePrefix);
			AssertEquals("Branch", branch, messageAttachee.Branch);
			AssertEquals("CustomsAgent", customsAgent, messageAttachee.CustomsAgent);
			AssertEquals("RelatedJob", header, messageAttachee.RelatedJob);
			AssertEquals("Factory", Factory, messageAttachee.Factory);
			AssertEquals("LogicalStatus", "MS1", messageAttachee.LogicalStatus);
			AssertEquals("EntryStatus", "ES2", messageAttachee.EntryStatus);
			AssertEquals("MovementReferenceNumber", "MRN001", messageAttachee.MovementReferenceNumber);
			AssertEquals("Messages Count", 1, messageAttachee.Messages.Count());
			AssertEquals("EM_MessageNum", "MN1", messageAttachee.Messages.First().EM_MessageNum);
		}

		public void TestIRelatedJobMembers()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			header.AMA_JobReference = "JN1";
			header.CustomsStatus = "CS1";
			IRelatedJob relatedJob = header;
			AssertEquals("JobNumber", "JN1", relatedJob.JobNumber);
			AssertEquals("JobDescription", "JN1", relatedJob.JobDescription);
			AssertEquals("CustomsStatus", "CS1", relatedJob.JobStatus);
			AssertEquals("ControllerID", ControllerIDs.Customs.EU.UCC6TemporaryStorage, relatedJob.ControllerID);
			AssertEquals("BusinessObjectPK", header.PK, relatedJob.BusinessObjectPK);
		}

		public void TestAMA_ManifestType_Caption()
		{
			AssertEquals("Message Version", DataBoundResourceStrings.GetDataForProperty(typeof(TemporaryStorageHeader), nameof(TemporaryStorageHeader.AMA_ManifestType)).Caption);
		}

		public void TestCustomsOfficeOfLodgementCaptions()
		{
			AssertEquals("CustomsOfficeOfLodgement", "Customs Office of Lodgement", DataBoundResourceStrings.GetDataForProperty(typeof(TemporaryStorageHeader), nameof(TemporaryStorageHeader.CustomsOfficeOfLodgement)).Caption);
		}

		public void TestCustomsOfficeOfLodgement()
		{
			var storageHeader = Factory.New<TemporaryStorageHeader>();
			AssertEquals(ZString.Empty, storageHeader.CustomsOfficeOfLodgement);

			var codeData = storageHeader.CustomsOfficeCodeOfLodgement;
			AssertEquals(ZString.Empty, storageHeader.CustomsOfficeOfLodgement);

			codeData.CY_Data = "7758258";
			AssertEquals("7758258", storageHeader.CustomsOfficeOfLodgement);
		}

		public void TestCustomsOfficeCodeOfLodgement()
		{
			var storageHeader = Factory.New<TemporaryStorageHeader>();
			storageHeader.AMA_RN_NKCountry = Core.Constants.CountryCodes.France;
			CombineAssertions(() =>
			{
				AssertEquals("PK", storageHeader.PK, storageHeader.CustomsOfficeCodeOfLodgement.CY_ParentID);
				AssertEquals("CSI_ParentTableCode", AsycudaManifestHeaderSchema.Constants.Prefix, storageHeader.CustomsOfficeCodeOfLodgement.CY_ParentTableCode);
				AssertEquals("CSI_Type", "EUO", storageHeader.CustomsOfficeCodeOfLodgement.CY_Type);
				AssertEquals("CSI_SubType", "LOD", storageHeader.CustomsOfficeCodeOfLodgement.CY_Code);
			});
		}

		static GlbCompany CreateCompany(BusinessObjectFactory factory, ZString countryCode, ZString code)
		{
			var company = factory.New<GlbCompany>();
			company.GC_Code = code;
			company.GC_Name = $"TEST {code} COMP";
			company.GC_OH_OrgProxy = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
			company.GC_RN_NKCountryCode = countryCode;
			return company;
		}

		static GlbBranch CreateBranch(GlbCompany company, ZString countryCode, ZString code)
		{
			var branch = company.Branches.AddNew();
			branch.GB_Code = code;
			branch.GB_BranchName = $"TEST {code} BRANCH";
			branch.GB_OH_OrgProxy = GlbBranch.CurrentBranch.GB_OH_OrgProxy;
			branch.GB_RN_NKCountryCode = countryCode;
			return branch;
		}

		[TestDate(2024, 5, 21)]
		public void TestUpdateLrnWhenRejected()
		{
			var company = CreateCompany(Factory, Core.Constants.CountryCodes.Ireland, "CO1");
			var branch = CreateBranch(company, Core.Constants.CountryCodes.Ireland, "BRX");
			var prefix = "TS" + GlbCompany.CurrentCompany.LicenceServerID;

			var temporaryStorageHeader = Factory.NewWithValidTestData<TemporaryStorageHeaderForTesting>();
			temporaryStorageHeader.AMA_GB = branch.PK;
			temporaryStorageHeader.FireFactorySaving();

			var originalLRN = $"{prefix}BRX24000000001V01";
			var mrn = "21IEDUB11A782454R2";
			AssertEquals("To make sure LRN is updated after message processing.", originalLRN, temporaryStorageHeader.LRN);

			var transactionId = "A16A0F24-4C2A-4B04-8B2A-C5AA2DA10AC0";
			var outgoingMessage = Factory.New<AISUCC5OutboundEDIMessage>();
			outgoingMessage.EM_ApplicationReference = transactionId;
			outgoingMessage.EM_Status = EDIMessage.Status.Acknowledged;
			outgoingMessage.EM_GB = GlbBranch.CurrentBranch.PK;
			outgoingMessage.EM_SystemCreateUser = GlbStaff.CurrentUser.GS_Code;
			temporaryStorageHeader.Messages.Add(outgoingMessage);

			var incomingTS305Message = Factory.New<AISUCC5InboundEDIMessage>();
			incomingTS305Message.EM_ApplicationReference = transactionId;
			incomingTS305Message.EM_MessageType = AISInterchangeTypeList.Codes.TS305;
			incomingTS305Message.EM_MessageText = InterchangeProcessorTestHelper.GetMailboxItemText(
				transactionId,
				AISUCC5InterchangeProcessorTestHelper.GetTS305Text(mrn, new DateTime(2024, 5, 16), "Reason TS305"),
				includeResponseWrap: false
			);
			incomingTS305Message.EM_SystemCreateTimeUtc = ZDateTime.UtcNow;
			var logger = new BatchProcessor.LoggingInformation();
			var ts305Processor = new AIS.UCC5.TS305Processor(logger, typeof(Ts305));
			ts305Processor.PreProcessMessage(incomingTS305Message);
			ts305Processor.ProcessMessage(incomingTS305Message);
			temporaryStorageHeader.FireFactorySaving();
			AssertEquals("To make sure that the message has been successfully processed.", EDIMessage.Status.ProcessedOK, incomingTS305Message.EM_Status);
			AssertEquals("Not update LRN by non-rejection message.", originalLRN, temporaryStorageHeader.LRN);

			var incomingTS333Message = Factory.New<AISUCC5InboundEDIMessage>();
			incomingTS333Message.EM_ApplicationReference = transactionId;
			incomingTS333Message.EM_MessageType = AISInterchangeTypeList.Codes.TS333;
			incomingTS333Message.EM_MessageText = InterchangeProcessorTestHelper.GetMailboxItemText(
				transactionId,
				AISUCC5InterchangeProcessorTestHelper.GetTS333Text(mrn, new DateTime(2024, 5, 17), "Reason TS333"),
				includeResponseWrap: false
			);
			incomingTS333Message.EM_SystemCreateTimeUtc = ZDateTime.UtcNow;
			var ts333Processor = new AIS.UCC5.TS333Processor(logger, typeof(Ts333));
			ts333Processor.PreProcessMessage(incomingTS333Message);
			ts333Processor.ProcessMessage(incomingTS333Message);
			temporaryStorageHeader.FireFactorySaving();
			AssertEquals("To make sure that the message has been successfully processed.", EDIMessage.Status.ProcessedOK, incomingTS333Message.EM_Status);
			AssertEquals("TS333(rejection), update LRN.", $"{prefix}BRX24000000001V02", temporaryStorageHeader.LRN);

			var incomingTS316Message = Factory.New<AISUCC5InboundEDIMessage>();
			incomingTS316Message.EM_ApplicationReference = transactionId;
			incomingTS316Message.EM_MessageType = AISInterchangeTypeList.Codes.TS316;
			incomingTS316Message.EM_MessageText = InterchangeProcessorTestHelper.GetMailboxItemText(
				transactionId,
				AISUCC5InterchangeProcessorTestHelper.GetTS316Text(originalLRN, new DateTime(2024, 5, 18)),
				includeResponseWrap: false
			);
			incomingTS316Message.EM_SystemCreateTimeUtc = ZDateTime.UtcNow;
			var ts316Processor = new AIS.UCC5.TS316Processor(logger, typeof(Ts316));
			ts316Processor.PreProcessMessage(incomingTS316Message);
			ts316Processor.ProcessMessage(incomingTS316Message);
			temporaryStorageHeader.FireFactorySaving();
			AssertEquals("To make sure that the message has been successfully processed.", EDIMessage.Status.ProcessedOK, incomingTS316Message.EM_Status);
			AssertEquals("TS316(rejection), update LRN.", $"{prefix}BRX24000000001V03", temporaryStorageHeader.LRN);

			var incomingTS315VMessage = Factory.New<AISUCC5InboundEDIMessage>();
			incomingTS315VMessage.EM_ApplicationReference = transactionId;
			incomingTS315VMessage.EM_MessageType = AISInterchangeTypeList.Codes.TS315V;
			incomingTS315VMessage.EM_MessageText = InterchangeProcessorTestHelper.GetMailboxItemText(
				transactionId,
				AISUCC5InterchangeProcessorTestHelper.GetTS315VText(mrn, originalLRN, new DateTime(2024, 5, 19)),
				includeResponseWrap: false
			);
			incomingTS315VMessage.EM_SystemCreateTimeUtc = ZDateTime.UtcNow;
			var ts315VProcessor = new AIS.UCC5.TS315VProcessor(logger, typeof(Ts315V));
			ts315VProcessor.PreProcessMessage(incomingTS315VMessage);
			ts315VProcessor.ProcessMessage(incomingTS315VMessage);
			temporaryStorageHeader.FireFactorySaving();
			AssertEquals("To make sure that the message has been successfully processed.", EDIMessage.Status.ProcessedOK, incomingTS315VMessage.EM_Status);
			AssertEquals("TS315V(acceptance), not update LRN.", $"{prefix}BRX24000000001V03", temporaryStorageHeader.LRN);

			var temporaryStorageHeader2 = Factory.NewWithValidTestData<TemporaryStorageHeaderForTesting>();
			temporaryStorageHeader2.AMA_GB = branch.PK;
			temporaryStorageHeader2.FireFactorySaving();
			AssertEquals("Another Job, start a new sequence.", $"{prefix}BRX24000000002V01", temporaryStorageHeader2.LRN);
		}

		public void TestCustomsOfficeOfFirstEntryCaptions()
		{
			var data = DataBoundResourceStrings.GetDataForProperty(typeof(TemporaryStorageHeader), nameof(TemporaryStorageHeader.CustomsOfficeOfFirstEntry));
			AssertEquals("CustomsOfficeCodeOfFirstEntry", "Customs Office of First Entry", data.Caption);
			AssertEquals("CustomsOfficeCodeOfFirstEntry", "5/24 Customs Office of First Entry", data.FullDescription);
		}

		public void TestCustomsOfficeOfFirstEntry()
		{
			var storageHeader = Factory.New<TemporaryStorageHeader>();
			AssertEquals(ZString.Empty, storageHeader.CustomsOfficeOfFirstEntry);

			var codeData = storageHeader.CustomsOfficeCodeOfFirstEntry;
			AssertEquals(ZString.Empty, storageHeader.CustomsOfficeOfFirstEntry);

			codeData.CY_Data = "7758258";
			AssertEquals("7758258", storageHeader.CustomsOfficeOfFirstEntry);
		}

		public void TestCustomsOfficeCodeOfFirstEntry()
		{
			var storageHeader = Factory.New<TemporaryStorageHeader>();
			storageHeader.AMA_RN_NKCountry = Core.Constants.CountryCodes.France;
			CombineAssertions(() =>
			{
				AssertEquals("PK", storageHeader.PK, storageHeader.CustomsOfficeCodeOfFirstEntry.CY_ParentID);
				AssertEquals("CSI_ParentTableCode", AsycudaManifestHeaderSchema.Constants.Prefix, storageHeader.CustomsOfficeCodeOfFirstEntry.CY_ParentTableCode);
				AssertEquals("CSI_Type", "EUO", storageHeader.CustomsOfficeCodeOfFirstEntry.CY_Type);
				AssertEquals("CSI_SubType", "ENT", storageHeader.CustomsOfficeCodeOfFirstEntry.CY_Code);
			});
		}

		public void TestDefaultAMA_ManifestType()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			AssertEquals("V1", header.AMA_ManifestType);
		}

		public void TestCusGoodsLocationProvider_ProviderKey()
		{
			var storageHeader = Factory.New<TemporaryStorageHeader>() as ICusGoodsLocationProvider;
			AssertEquals("ProviderKey", "IEDECL", storageHeader.ProviderKey);
		}

		public void TestCusGoodsLocationProvider_UCCVersionProperty()
		{
			var storageHeader = Factory.New<TemporaryStorageHeader>();
			AssertEquals("UCCVersionDependingOnPropertyInfo returns AMA_ManifestTypeInfo", storageHeader.AMA_ManifestTypeInfo, ((ICusGoodsLocationProviderWithUCCVersion)storageHeader).IsUCC5Info);
		}

		public void TestCusGoodsLocationProvider_UCCVersion()
		{
			var storageHeader = Factory.New<TemporaryStorageHeader>();
			storageHeader.AMA_ManifestType = ImportDeclarationApplicationCodeList.Codes.V1;
			AssertEquals("IsUCC5 returns true for V1", true, ((ICusGoodsLocationProviderWithUCCVersion)storageHeader).IsUCC5);
			AssertEquals("IsUCC6 returns true for V2", false, ((ICusGoodsLocationProviderWithUCCVersion)storageHeader).IsUCC6);

			storageHeader.AMA_ManifestType = ImportDeclarationApplicationCodeList.Codes.V2;
			AssertEquals("IsUCC5 returns false for V1", false, ((ICusGoodsLocationProviderWithUCCVersion)storageHeader).IsUCC5);
			AssertEquals("IsUCC6 returns true for V2", true, ((ICusGoodsLocationProviderWithUCCVersion)storageHeader).IsUCC6);
		}

		protected override Type ExpectedTypeOfContainer => typeof(ManifestBase.AsycudaContainerCollection<EU.Business.CusTempStorage.TemporaryStorageContainer, EU.Business.CusTempStorage.TemporaryStorageHeader>);

		protected override LightValidationTester GetNewLightValidationTester(BusinessObject bizObjToTest)
		{
			return new TemporaryStorageHeaderLightValidationTester(bizObjToTest);
		}

		sealed class TemporaryStorageHeaderLightValidationTester : LightValidationTester
		{
			public TemporaryStorageHeaderLightValidationTester(BusinessObject bo) : base(bo)
			{
			}

			protected override bool ShouldTestProperty(ZPropertyInfo info)
			{
				return base.ShouldTestProperty(info) && info.Name != TemporaryStorageBill.Schema.ABL_RL_NKPortOfDischarge;
			}
		}

		public override List<ZString> GetExcludedColumns_OnlySomeSubclassesAreSetDefaultValues()
		{
			var result = base.GetExcludedColumns_OnlySomeSubclassesAreSetDefaultValues();
			result.Add(TemporaryStorageHeader.Schema.AMA_OA_Declarant);
			return result;
		}

		class TemporaryStorageHeaderForTesting : TemporaryStorageHeader
		{
			public TemporaryStorageHeaderForTesting(BusinessObjectFactory factory, System.Data.DataRow row) : base(factory, row)
			{
			}

			public void FireFactorySaving()
			{
				OnFactorySaving();
			}
		}
	}
}
