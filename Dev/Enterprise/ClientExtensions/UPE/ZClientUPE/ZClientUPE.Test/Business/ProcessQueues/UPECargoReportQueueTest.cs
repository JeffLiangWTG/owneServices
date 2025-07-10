using System;
using CargoWise.Types;
using Enterprise.Client.UPE.Business.Testing;
using Enterprise.Client.UPE.Testing;
using Enterprise.Customs.AU.Declaration.Business.Testing;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Business.ProcessQueues.Testing
{
	[TestedType(typeof(UPECargoReportQueue))]
	internal class UPECargoReportQueueTest : UPEProcessQueueTestCase
	{
		public override void TestReferenceCode()
		{
			Assert("COM Reference code", Queue.ReferenceCode == "COM");
		}

		public void TestGetFinalisedCusHAWBs_UPECusHAWBIsNull()
		{
			AssertEquals("Should be an empty array if UPECusHAWB is null", 0, Queue.GetFinalisedCusHAWBs().Length);
		}

		public void TestGetFinalisedCusHAWBs()
		{
			TaxOrFeeTestHelper.SetUp();
			TaxOrFeeTestHelper.SetDeminimus(Factory, 100m);

			TestHelper.HouseBill.CS_RN_NKConsigneeCountry = Core.Constants.CountryCodes.Australia;

			Queue.Parent = TestHelper.HouseBill;
			TestHelper.HouseBill.CS_GoodsValue = 0;
			AssertEquals("Sanity check", false, TestHelper.HouseBill.IsFormalDecRequired);

			Queue.P4_CustomsQueue = CargoReportQueueCodeDescriptionPairList.Codes.AwaitingDeclaration;
			Queue.P4_QueueName = CommercialQueueCodeDescriptionPairList.Codes.Completed;
			AssertEquals("Customs Queue is not yet completed", 0, Queue.GetFinalisedCusHAWBs().Length);

			Queue.P4_CustomsQueue = CargoReportQueueCodeDescriptionPairList.Codes.Completed;
			UPECusHAWB[] finalisedCusHAWBs = Queue.GetFinalisedCusHAWBs();
			AssertEquals(1, finalisedCusHAWBs.Length);
			AssertEquals(TestHelper.HouseBill, finalisedCusHAWBs[0]);

			TestHelper.HouseBill.CS_GoodsValue = 101;
			AssertEquals("Sanity check", true, TestHelper.HouseBill.IsFormalDecRequired);
			AssertEquals("Declaration Queue is not completed", 0, Queue.GetFinalisedCusHAWBs().Length);

			TestHelper.HouseBill.CS_JE_CustomsFormalEntry = Factory.New(typeof(UPEJobDeclaration)).PK;
			TestHelper.HouseBill.Declaration.CurrentQueue.P4_CustomsQueue = DeclarationQueueCodeDescriptionPairList.Codes.Compiling;
			AssertEquals("Declaration Queue is not completed", 0, Queue.GetFinalisedCusHAWBs().Length);

			TestHelper.HouseBill.Declaration.CurrentQueue.P4_CustomsQueue = DeclarationQueueCodeDescriptionPairList.Codes.CustomsBonding;
			finalisedCusHAWBs = Queue.GetFinalisedCusHAWBs();
			AssertEquals(1, finalisedCusHAWBs.Length);
			AssertEquals(TestHelper.HouseBill, finalisedCusHAWBs[0]);

			Queue.P4_QueueName = CommercialQueueCodeDescriptionPairList.Codes.Finance;
			AssertEquals("Commercial Queue is not completed", 0, Queue.GetFinalisedCusHAWBs().Length);

			Queue.P4_QueueName = CommercialQueueCodeDescriptionPairList.Codes.Chase;
			finalisedCusHAWBs = Queue.GetFinalisedCusHAWBs();
			AssertEquals(1, finalisedCusHAWBs.Length);
			AssertEquals(TestHelper.HouseBill, finalisedCusHAWBs[0]);
		}

		public void TestAddResolutionCodeLog()
		{
			Queue.Parent = TestHelper.HouseBill;

			Queue.P4_CustomsStatus = CMRConsolidatedCargoStatuses.Codes.WithdrawnCargoReportHadBeenWithdrawn;
			EDIMessage message = Queue.FirstUPECusHAWB.Messages.AddNew();
			message.EM_MessageType = UPECalloutQueue.EdiMessageTypes.Withdrawn;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			Queue.AddResolutionCodeLog();
			AssertEquals("Default release code is DA", ZString.Empty, Queue.ResolutionCode);

			message.EM_MessageType = ZString.Empty;
			Queue.AddResolutionCodeLog();
			AssertEquals("witheld shipment therefore set to empty", ResolutionCodeDescriptionPairList.Codes.DA_Released, Queue.ResolutionCode);

			Queue.P4_QueueName = CommercialQueueCodeDescriptionPairList.Codes.Rebill;
			Queue.P4_Status = ReasonCodeDescriptionPairList.Codes._R1_Abandon;
			Queue.AddResolutionCodeLog();
			AssertEquals("Should create a new log even if a resolution log already exist", ResolutionCodeDescriptionPairList.Codes.BZ_Abandoned, Queue.ResolutionCode);

			Queue.CustomsQueueLogs.RemoveAndDeleteAll();
			Queue.ResetResolutionCodeForTest();
			Queue.AddResolutionCodeLog();
			AssertEquals(ResolutionCodeDescriptionPairList.Codes.BZ_Abandoned, Queue.ResolutionCode);

			Queue.CustomsQueueLogs.RemoveAndDeleteAll();
			Queue.ResetResolutionCodeForTest();
			Queue.P4_QueueName = string.Empty;
			Queue.Parent = TestHelper.HouseBill;
			Queue.FirstUPECusHAWB.CS_JE_CustomsFormalEntry = Factory.New(typeof(UPEJobDeclaration)).PK;
			Queue.FirstUPECusHAWB.Declaration.CurrentQueue.P4_CustomsQueue = DeclarationQueueCodeDescriptionPairList.Codes.CustomsBonding;
			Queue.AddResolutionCodeLog();
			AssertEquals("Sanity check", true, Queue.FirstUPECusHAWB.Declaration.IsInCustomsBondingQueue);
			AssertEquals(ResolutionCodeDescriptionPairList.Codes.B7_SeizedByCustoms, Queue.ResolutionCode);

			Queue.CustomsQueueLogs.RemoveAndDeleteAll();
			Queue.ResetResolutionCodeForTest();
			Queue.FirstUPECusHAWB.Declaration.CurrentQueue.P4_CustomsQueue = string.Empty;
			Queue.FirstUPECusHAWB.Declaration.JE_OH_Importer = Factory.New(typeof(OrgHeader)).PK;
			Queue.FirstUPECusHAWB.Declaration.Importer.SetRelatedParty(Factory.New<OrgHeader>(), RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyDirectionList.Codes.Delivery, Core.Constants.TransportModes.Air, ZString.Empty);
			Queue.AddResolutionCodeLog();
			AssertEquals("Sanity check", true, Queue.FirstUPECusHAWB.Declaration.HasAlternateBroker);
			AssertEquals(ResolutionCodeDescriptionPairList.Codes.BU_ReleasedByAlternateBroker, Queue.ResolutionCode);
		}

		protected override Type ExpectedParentBusinessObjectType
		{
			get { return typeof(UPECusHAWB); }
		}

		protected override Type ExpectedLookupsType
		{
			get { return typeof(UPECargoReportQueueLookups); }
		}

		protected override Type ExpectedValidationType
		{
			get { return typeof(UPECargoReportQueueValidation); }
		}

		protected new UPECargoReportQueue Queue
		{
			get { return (UPECargoReportQueue)base.Queue; }
		}

		UPETestHelper TestHelper
		{
			get { return testHelper ?? (testHelper = new UPETestHelper(Factory)); }
		}
		UPETestHelper testHelper;
	}
}
