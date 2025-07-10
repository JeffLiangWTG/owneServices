using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.Customs.BE.MessageContracts.MessageProviders;
using CargoWise.Customs.BE.MessageDefinitions.NCTSVersion51_8_2.ctypes;
using CargoWise.Types;
using Enterprise.Customs.BE.Business;
using Enterprise.Customs.BE.Business.Testing;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.BE.NCTS.Business.Testing
{
	[TestedType(typeof(CC025CMessageProcessor))]
	sealed class CC025CMessageProcessorTest : MessageProcessorTestCase<CC025CMessageProcessor, ICC025CDataProvider>
	{
		public void TestCESEvent()
		{
			var releaseDate = ZDateTime.Now;
			mockProvider.Setup(x => x.ReleaseDate).Returns(releaseDate.ToDateTime());
			mockProvider.Setup(x => x.ReleaseIndicator).Returns(Constants.ReleaseIndicator.FullRelease);
			var header = ProcessBasicMessage();
			CombineAssertions(() =>
			{
				var movementHeader = header.ArrivalMovementHeader;
				var eventLog = movementHeader.Logs.GetAllLogs().Cast<StmALog>().FirstOrDefault(l => l.SL_SE_NKEvent == "CES" && (l.SL_Reference == "CL1"));
				AssertNotNull("event should be created", eventLog);
				AssertEquals("date is correct", $"{ZDateTime.Now.Year}.{ZDateTime.Now.Month}.{ZDateTime.Now.Day}", $"{eventLog.SL_EventTime.Year}.{eventLog.SL_EventTime.Month}.{eventLog.SL_EventTime.Day}");
				AssertEquals("branch", "BNE", eventLog.SL_GB_NKBranch);
				AssertEquals("department", "BRN", eventLog.SL_GE_NKDepartment);
				AssertEquals("reference", "CL1", eventLog.SL_Reference);
				AssertEquals("Only 1 CES-event should have been created", 1, movementHeader.Logs.Find(x => x.SL_SE_NKEvent == Events.CustomsEntryStatus.Code && x.SL_Reference == movementHeader.BM_CustomsStatus).Count());
			});
		}

		public void TestLockEventCreated()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType("A");
			mockProvider.Setup(x => x.HouseConsignments).Returns(new ReadOnlyCollection<HouseConsignmentXmlProvider>
				(new List<HouseConsignmentXmlProvider>()
					{ HouseConsignmentXmlProvider.New(
						new HouseConsignmentType02
							{ ConsignmentItem = new Collection<ConsignmentItemType02>(new List<ConsignmentItemType02>()
								{ new ConsignmentItemType02 { Packaging = new Collection<PackagingType02> { new PackagingType02() } } } ) }) }));

			mockProvider.Setup(x => x.ReleaseIndicator).Returns(Constants.ReleaseIndicator.FullRelease);

			CombineAssertions(() =>
			{
				var declarationConfig = new DeclarationLockConfig() { DeclarationType = EUJobMessageTypeList.Codes.NctsArrivalUnloadingRemarks, };
				var tabInfo = declarationConfig.TabInfos.AddNew();
				tabInfo.TabPage = tabInfo.Lookups.TabPageList.GetAllCodes().First();
				var eventInfo = declarationConfig.EventInfos.AddNew();
				eventInfo.EventReference = NCTS5ArrivalCustomsStatusList.Codes.ClosedFullRelease;
				eventInfo.EventType = Events.CustomsEntryStatusCode;
				eventInfo.EventSource = Core.Constants.Customs.EventLockSourceTypes.Codes.NctsHeader;
				incomingMessage.EM_LinkedObject = header;
				incomingMessage.EM_Status = "PPS";
				using (CustomsDataRegistry.Instance.DeclarationLockForEdit.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, new DeclarationLockConfigCollection() { declarationConfig }))
				{
					Processor.ProcessMessage(incomingMessage);
					Factory.Save();
				}
				var lckEventlog = header.Logs.Find(c => c.SL_SE_NKEvent == AutoEvents.LockForEditCode).First();
				AssertEquals("EventReference", "The tab 'Unloading Remarks' and its depending tabs were locked when message 'Goods Release Notification' was received.", lckEventlog.SL_Reference);
			});
		}

		NctsHeader ProcessBasicMessage(NctsHeader header = null)
		{
			var nctsHeader = header ?? Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			nctsHeader.ArrivalMovementHeader.BM_CustomsStatus = NCTS5ArrivalCustomsStatusList.Codes.UnloadPermissionGranted;
			Extensions.CreateMovementReferenceNumber(nctsHeader, "22BE000000000012J1");
			Factory.Save();

			mockProvider.Setup(x => x.HouseConsignments).Returns(new ReadOnlyCollection<HouseConsignmentXmlProvider>
				(new List<HouseConsignmentXmlProvider>()
					{ HouseConsignmentXmlProvider.New(
						new HouseConsignmentType02
							{ ConsignmentItem = new Collection<ConsignmentItemType02>(new List<ConsignmentItemType02>()
								{ new ConsignmentItemType02 { Packaging = new Collection<PackagingType02> { new PackagingType02() } } } ) }) }));
			mockProvider.Setup(x => x.MRN).Returns("22BE000000000012J1");

			Processor.PreProcessMessage(incomingMessage);
			Processor.ProcessMessage(incomingMessage);
			Factory.Save();

			return nctsHeader;
		}

		public void TestMessageTypesToInclude()
		{
			AssertSequencesEqual(new ZString[] { BEIncomingMessageTypes.Codes.CC025C }, processor.MessageTypesToInclude);
		}

		#region TestValidateCC025CMessageDeclaration

		public void TestPreProcessCC025C_NonMatchingMRN()
		{
			mockProvider.Setup(x => x.MRN).Returns("FAKE");
			AssertPreProcess(EDIMessageStatusList.Codes.Failed);
			AssertEquals("The processing of the message with interchange failed because the message could not be linked to a NCTS declaration.", incomingMessage.Notes.FindByDescription(Constants.MessageProcessingNotes.ProcessingLog).Single(x => x.ST_IsCustomDescription).ST_NoteText);
		}

		public void TestPreProcessCC025C_MatchingMRN()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Arrival);
			var movementHeader = nctsHeader.ArrivalMovementHeader;
			movementHeader.BM_SubApplicationCode = NctsMoveHeaderType.Codes.Arrival;
			Extensions.CreateMovementReferenceNumber(nctsHeader, "22BE000000000012J1");
			mockProvider.Setup(x => x.MRN).Returns("22BE000000000012J1");
			Factory.Save();
			AssertPreProcess(EDIMessageStatusList.Codes.PreProcessedOK);
		}

		public void TestPreProcessCC025C_MatchingMRNWrongSubApplicationCode()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Arrival);
			var movementHeader = nctsHeader.ArrivalMovementHeader;
			movementHeader.BM_SubApplicationCode = NctsMoveHeaderType.Codes.Departure;
			Extensions.CreateMovementReferenceNumber(nctsHeader, "22BE000000000012J1");
			mockProvider.Setup(x => x.MRN).Returns("22BE000000000012J1");
			Factory.Save();
			AssertPreProcess(EDIMessageStatusList.Codes.Failed);
		}

		void AssertPreProcess(string expectedStatus)
		{
			processor.PreProcessMessage(incomingMessage);
			AssertEquals("EDIMEssage Status Should BE " + expectedStatus, expectedStatus, incomingMessage.EM_Status);
		}

		#endregion

		public void TestProcessing()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Arrival);
			var movementHeader = nctsHeader.ArrivalMovementHeader;
			movementHeader.BM_SubApplicationCode = NctsMoveHeaderType.Codes.Arrival;
			Extensions.CreateMovementReferenceNumber(nctsHeader, "22BE000000000012J1");
			Factory.Save();
			nctsHeader.Bills.AddNew().ArrivalGoodsItems.AddNew().BY_DeclarationGoodsItemNumber = 1;
			var secondBill = nctsHeader.Bills.AddNew();
			secondBill.MovementDetail.B9_SeqNo = "2";
			secondBill.ArrivalGoodsItems.AddNew().BY_DeclarationGoodsItemNumber = 2;
			var secondGoodsItemOnSecondBill = secondBill.ArrivalGoodsItems.AddNew();
			secondGoodsItemOnSecondBill.BY_DeclarationGoodsItemNumber = 3;
			var firstPackage = secondGoodsItemOnSecondBill.Packages.AddNew();
			firstPackage.B5_SequenceNumber = 1;
			var secondPackage = secondGoodsItemOnSecondBill.Packages.AddNew();
			secondPackage.B5_SequenceNumber = 2;

			var releaseDate = ZDateTime.Now;

			var houseConsignmentProvider = HouseConsignmentXmlProvider.New(new HouseConsignmentType02
			{
				SequenceNumber = "2",
				ConsignmentItem = new Collection<ConsignmentItemType02>(new List<ConsignmentItemType02>() { new ConsignmentItemType02 {
					DeclarationGoodsItemNumber = "3",
					Packaging = new Collection<PackagingType02> { new PackagingType02
					{
						NumberOfPackages = "5",
						SequenceNumber = "2",
					}, },
			} }),
			});

			mockProvider.Setup(x => x.HouseConsignments).Returns(new ReadOnlyCollection<HouseConsignmentXmlProvider>(new List<HouseConsignmentXmlProvider>() { houseConsignmentProvider }));
			mockProvider.Setup(x => x.MRN).Returns("22BE000000000012J1");
			mockProvider.Setup(x => x.ReleaseDate).Returns(releaseDate.ToDateTime());
			mockProvider.Setup(x => x.ReleaseIndicator).Returns(Constants.ReleaseIndicator.FullRelease);

			Processor.PreProcessMessage(incomingMessage);
			Processor.ProcessMessage(incomingMessage);
			CombineAssertions(() =>
			{
				AssertEquals("EM_Status", EDIMessage.Status.ProcessedOK, incomingMessage.EM_Status);
				AssertEquals("CE_IssueDate", releaseDate, nctsHeader.MovementReferenceEntryNumber.CE_IssueDate);
				AssertEquals("EffectiveMessageStatus", LogicalStatusList.Codes.Accepted, nctsHeader.EffectiveMessageStatus);
				AssertEquals("BM_CustomsStatus", NCTS5ArrivalCustomsStatusList.Codes.ClosedFullRelease, movementHeader.BM_CustomsStatus);
				AssertEquals("B5_UnitsReleased for first package", ZShort.Zero, firstPackage.B5_UnitsReleased);
				AssertEquals("B5_UnitsReleased for second package", new ZShort(5), secondPackage.B5_UnitsReleased);
			});
		}

		protected override string ExpectedMessageFriendlyName => BEIncomingMessageTypes.Descriptions.CC025C;

		protected override CC025CMessageProcessor Processor => processor;

		protected override Type ExpectedMessageInterpreterType => typeof(CC025CMessageInterpreter);

		protected override void SetUp()
		{
			base.SetUp();
			mockProvider = new Mock<ICC025CDataProvider>();
			mockProvider.CallBase = true;
			mockProcessor = new Mock<CC025CMessageProcessor>(new BatchProcessor.LoggingInformation());
			mockProcessor.CallBase = true;
			provider = mockProvider.Object;
			mockProcessor.Setup(m => m.GetMessageDataProvider(It.IsAny<BEMessage>())).Returns(provider);
			processor = mockProcessor.Object;
			incomingMessage = CreateIncomingMessage(Factory);
			Factory.Save();
		}

		Mock<ICC025CDataProvider> mockProvider;
		Mock<CC025CMessageProcessor> mockProcessor;
		CC025CMessageProcessor processor;
		ICC025CDataProvider provider;
		BEMessage incomingMessage;
	}
}
