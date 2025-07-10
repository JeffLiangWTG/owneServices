using System;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Interfaces;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CusHAWBMessageReferencePopulationTest : TestCaseWithFactory
	{
		[TestDate(2005, 11, 01, 01, 01, 01)]
		public void TestOrphanedCARSTAttached()
		{
			AUCustomsDataRegistry.Instance.AttachOrphanedCARSTsWhenCusHAWBCreated.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			var mAWB = Factory.New<CusMAWB>();
			mAWB.CM_MAWB = "40691102981";
			Factory.Save();

			var message1 = Factory.New<CMRCARSTMessage>();
			message1.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+34:::CARST+2BDG H3H4 0F1F:1+8'
DTM+9:20051102140706724348:ZZZ'
DTM+132:20051030:102'
FTX+AHN+++CONSOLIDATED STATUS:DCLALLOWED'
FTX+AHN+++CARGO REPORT SAC:NO'
TDT+20+6901++6+5X::3'
LOC+12+AUSYD::6'
NAD+MR+FGE743G::95'
NAD+UD+28078835604::95'
RFF+ABO:S00001843/BNE1::1'
RFF+MWB:40691102981'
RFF+HWB:ATL001437XXXXXXXXXXXX'
DOC+1'
PAC+0000003'
UNT+16+000001'".Replace("\r\n", "");
			message1.SetEM_LinkedObject();
			message1.EM_MessageNum = "1";
			Assert("Message1 attached to master", mAWB.Messages.Contains(message1));

			var message2 = Factory.New<CMRCARSTMessage>();
			message2.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+34:::CARST+2BDG H3H4 0F1F:1+8'
DTM+9:20051102140706724348:ZZZ'
DTM+132:20051030:102'
FTX+AHN+++CONSOLIDATED STATUS:DCLALLOWED'
FTX+AHN+++CARGO REPORT SAC:NO'
TDT+20+6901++6+5X::3'
LOC+12+AUSYD::6'
NAD+MR+FGE743G::95'
NAD+UD+28078835604::95'
RFF+ABO:S00001843/BNE1::1'
RFF+MWB:40691102981'
RFF+HWB:ATL001438'
DOC+1'
PAC+0000003'
UNT+16+000001'".Replace("\r\n", "");
			message2.EM_MessageNum = "2";
			message2.SetEM_LinkedObject();
			Factory.Save();
			Assert("Message2 attached to master", mAWB.Messages.Contains(message2));

			var newHAWB1 = mAWB.ChildBills.AddNew();
			newHAWB1.CS_HAWB = "ATL001437XXXXXXXXXXXX";
			newHAWB1.IsGettingUnattachedHouseCARSTSQueryFromLocalCacheForTest = true;
			Factory.Save();
			Assert("New house now contains message", newHAWB1.Messages.Contains(message1));
			Assert("Detached from master", !mAWB.Messages.Contains(message1));

			AUCustomsDataRegistry.Instance.AttachOrphanedCARSTsWhenCusHAWBCreated.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
			var newHAWB2 = mAWB.ChildBills.AddNew();
			newHAWB2.CS_HAWB = "ATL001438";
			Factory.Save();
			Assert("New house does not contains message", !newHAWB2.Messages.Contains(message2));
			Assert("Still attached to master", mAWB.Messages.Contains(message2));

			using (AUCustomsDataRegistry.Instance.AttachOrphanedCARSTsWhenCusHAWBCreated.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true))
			{
				var message3 = Factory.New<CMRCARSTMessage>();
				message3.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+34:::CARST+2BDG H3H4 0F1F:1+8'
DTM+9:20051102140706724348:ZZZ'
DTM+132:20051030:102'
FTX+AHN+++CONSOLIDATED STATUS:DCLALLOWED'
FTX+AHN+++CARGO REPORT SAC:NO'
TDT+20+6901++6+5X::3'
LOC+12+AUSYD::6'
NAD+MR+FGE743G::95'
NAD+UD+28078835604::95'
RFF+ABO:S00001843/BNE1::1'
RFF+MWB:40691102981'
RFF+HWB:ATL001439'
DOC+1'
PAC+0000003'
UNT+16+000001'".Replace("\r\n", "");
				message3.EM_MessageNum = "2";
				message3.SetEM_LinkedObject();
				Factory.Save();
				Assert("Message3 attached to master", mAWB.Messages.Contains(message3));

				var newHAWB3 = mAWB.ChildBills.AddNew();
				newHAWB3.CS_HAWB = "ATL001439";
				newHAWB3.CS_IsHVLV = true;
				Factory.Save();
				Assert("For HVLV, New house does not contains message", !newHAWB3.Messages.Contains(message3));
				Assert("Still attached to master", mAWB.Messages.Contains(message3));
			}
		}

		public void TestAddShipmentMessagingEventIfRequiredForStandardShipment()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var mawb = Factory.New<CusMAWB>();
			var hawb = mawb.ChildBills.AddNew();
			hawb.CS_JS = shipment.PK;
			AssertEquals("no log on non-hvlv", 0, new LogsForNominatedEvent(shipment.GetLogs(), Events.HVLVReady).Count);

			hawb.CS_MsgStatus = CMRBaseStatuses.Codes.AwaitingResponseToOriginal;
			AssertEquals("No log on non-hvlv", 0, new LogsForNominatedEvent(shipment.GetLogs(), Events.HVLVReady).Count);
		}

		public void TestAddShipmentMessagingEventIfRequiredForHLVShipment_HLSOnVersion1()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.HighVolumeLowValueLegacy;

			var mawb = Factory.New<CusMAWB>();
			var hawb = mawb.ChildBills.AddNew();
			hawb.CS_JS = shipment.PK;
			var nominatedEvent = new LogsForNominatedEvent(shipment.GetLogs(), Events.HVLVReady);
			AssertEquals("no log on non-hvlv", 0, nominatedEvent.Count);

			hawb.CS_MsgStatus = CMRBaseStatuses.Codes.AwaitingResponseToOriginal;
			nominatedEvent = new LogsForNominatedEvent(shipment.GetLogs(), Events.HVLVReady);
			AssertEquals("One log now", 1, nominatedEvent.Count);
			AssertEquals(true, nominatedEvent[0].SL_Reference.EndsWith("|RES=Cargo Reporting", StringComparison.OrdinalIgnoreCase));

			hawb.CS_MsgStatus = CMRBaseStatuses.Codes.OriginalRejected;
			hawb.CS_MsgStatus = CMRBaseStatuses.Codes.AwaitingResponseToOriginal;
			nominatedEvent = new LogsForNominatedEvent(shipment.GetLogs(), Events.HVLVReady);
			AssertEquals("Two logs now", 2, nominatedEvent.Count);
			AssertEquals(true, nominatedEvent[0].SL_Reference.EndsWith("|RES=Cargo Reporting", StringComparison.OrdinalIgnoreCase));
			AssertEquals(true, nominatedEvent[1].SL_Reference.EndsWith("|RES=Cargo Reporting", StringComparison.OrdinalIgnoreCase));
		}

		public void TestDoNotAddShipmentMessagingEvent_ForNonHLSShipments()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.HighVolumeLowValue;

			var mawb = Factory.New<CusMAWB>();
			var hawb = mawb.ChildBills.AddNew();
			hawb.CS_JS = shipment.PK;
			var nominatedEvent = new LogsForNominatedEvent(shipment.GetLogs(), Events.HVLVReady);
			AssertEquals("Precondition: No HLR logs on the HVLV shipment", 0, nominatedEvent.Count);

			hawb.CS_MsgStatus = CMRBaseStatuses.Codes.AwaitingResponseToOriginal;
			nominatedEvent = new LogsForNominatedEvent(shipment.GetLogs(), Events.HVLVReady);
			AssertEquals("There should be no HLR event log for HVLV shipment if the HAWB's message status changes", 0, nominatedEvent.Count);

			hawb.CS_MsgStatus = CMRBaseStatuses.Codes.OriginalRejected;
			hawb.CS_MsgStatus = CMRBaseStatuses.Codes.AwaitingResponseToOriginal;
			nominatedEvent = new LogsForNominatedEvent(shipment.GetLogs(), Events.HVLVReady);
			AssertEquals("There should be no HLR event log for HVLV shipment if the HAWB's message status changes", 0, nominatedEvent.Count);
		}

		public void TestIDocManagerSupportIncudingRelatedObjectsMembers()
		{
			var hawb = Factory.New<CusHAWB>();
			var underbond = hawb.AllUnderbonds.AddNew();
			var supporter = hawb as IDocManagerSupportIncudingRelatedObjects;
			AssertEquals("Self reference", hawb, supporter.SelfReference);
			AssertEquals(1, supporter.GetRelatedBusinessObjects().Count());
			Assert(supporter.GetRelatedBusinessObjects().Contains(underbond));
			AssertType(typeof(DocManagerIncludingRelatedObjectsInfo), ((IDocManagerSupport)hawb).DocManagerInfo);
		}

		public void TestIDataExportCSVFileNameProviderMembers()
		{
			var hawb = Factory.New<CusHAWB>();
			var fileNameProvider = hawb as IDataExportCSVFileNameProvider;
			hawb.CS_HAWB = "123456";
			AssertEquals("File name suffix", "123456", fileNameProvider.FileNameSuffix);
		}

		public void TestUsesTranshipmentPortOnUnderbond()
		{
			var hawb = Factory.New<CusHAWB>();
			AssertEquals(true, ((ICusUnderbondDependentCollectionParent)hawb).UsesTranshipmentPortOnUnderbond);
			AssertEquals(ZString.Empty, ((ICusUnderbondDependentCollectionParent)hawb).DefaultTranshipmentPort);
			hawb.CS_RL_NKDestination = "NZAKL";
			AssertEquals("NZAKL", ((ICusUnderbondDependentCollectionParent)hawb).DefaultTranshipmentPort);

			var underbond = hawb.Underbonds.AddNew();
			hawb.CS_RL_NKDestination = "AUSYD";
			underbond.C4_MovementReason = CMRUnderbondRequestCodes.Codes.Transshipment;
			AssertEquals(ZString.Empty, underbond.C4_RL_NKTranshipDestPort);
			underbond.C4_MovementReason = CMRUnderbondRequestCodes.Codes.OtherMovement;
			AssertEquals(ZString.Empty, underbond.C4_RL_NKTranshipDestPort);
			hawb.CS_RL_NKDestination = "NZAKL";
			underbond.C4_MovementReason = CMRUnderbondRequestCodes.Codes.Transshipment;
			AssertEquals("NZAKL", underbond.C4_RL_NKTranshipDestPort);
			underbond.C4_RL_NKTranshipDestPort = ZString.Empty;
			underbond.C4_MovementReason = CMRUnderbondRequestCodes.Codes.OtherMovement;
			AssertEquals(ZString.Empty, underbond.C4_RL_NKTranshipDestPort);
		}

		public void TestEnsureShipmentHasUniqueConsignRefBeforePopulatingMessageReference()
		{
			var mawb = Factory.New<CusMAWB>();
			var hawb = Factory.New<TestHAWB>();
			mawb.ChildBills.Add(hawb);
			var shipment = Factory.New<TestShipment>();
			shipment.hawbForTest = hawb;
			hawb.CS_JS = shipment.PK;

			Factory.Save();

			AssertEquals("precondition", false, shipment.JS_UniqueConsignRef.IsEmpty);
			AssertEquals(false, hawb.CS_MessageReference.IsEmpty);
		}

		public void TestLongShipmentNumberDoesNotCauseExceptionOnSaving()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				var mawb = Factory.New<CusMAWB>();
				var hawb = Factory.New<CusHAWB>();
				mawb.ChildBills.Add(hawb);
				var shipment = Factory.New<CommonShipment>();
				shipment.JS_UniqueConsignRef = "ALONGSHIPMENTNUMBER";
				hawb.CS_JS = shipment.PK;
				Factory.Save();
				AssertEquals("ALONGSHIPMENTNUMBER", hawb.CS_MessageReference);
			}
		}

		public void TestHVLVStatusMapping()
		{
			var hawb = Factory.New<TestHAWB>();
			AssertEquals(CargoWise.EventReference.Constants.EventReferenceTypeTypes.Codes.Cleared, hawb.GetHVLVStatusMappingForTest(CMRConsolidatedCargoStatuses.Codes.ClearCargoIsFreeOfAnyImpedimentsAndMayBeReleased));
			AssertEquals(CargoWise.EventReference.Constants.EventReferenceTypeTypes.Codes.Cleared, hawb.GetHVLVStatusMappingForTest(CMRConsolidatedCargoStatuses.Codes.ClearhrmCargoIsClearButIsIdentifiedAsHighRiskMovement));
			AssertEquals(CargoWise.EventReference.Constants.EventReferenceTypeTypes.Codes.Held, hawb.GetHVLVStatusMappingForTest(CMRConsolidatedCargoStatuses.Codes.HeldCargoIsHeldUnderCustomsControl));
			AssertEquals(CargoWise.EventReference.Constants.EventReferenceTypeTypes.Codes.Held, hawb.GetHVLVStatusMappingForTest(CMRConsolidatedCargoStatuses.Codes.AcsseizedCargoIsSeizedByCustoms));
			AssertEquals(CargoWise.EventReference.Constants.EventReferenceTypeTypes.Codes.Held, hawb.GetHVLVStatusMappingForTest(CMRConsolidatedCargoStatuses.Codes.WithdrawnCargoReportHadBeenWithdrawn));
			AssertEquals(CargoWise.EventReference.Constants.EventReferenceTypeTypes.Codes.GovernmentAgencyRequirements, hawb.GetHVLVStatusMappingForTest(CMRConsolidatedCargoStatuses.Codes.CondclearCargoCanBeReleasedIntoHomeConsumptionSubjectToConditionSTheseConditionsAreProvidedInSupplementaryInformation));
			AssertEquals(CargoWise.EventReference.Constants.EventReferenceTypeTypes.Codes.GovernmentAgencyRequirements, hawb.GetHVLVStatusMappingForTest(CMRConsolidatedCargoStatuses.Codes.AqisseizedCargoIsSeizedByQuarantine));
			AssertEquals(CargoWise.EventReference.Constants.EventReferenceTypeTypes.Codes.Transshipment, hawb.GetHVLVStatusMappingForTest(CMRConsolidatedCargoStatuses.Codes.TranshipCargoIsForTranshipmentATranshipmentNumberWillBeGeneratedAndTransmittedWithStatus));
			AssertEquals(CargoWise.EventReference.Constants.EventReferenceTypeTypes.Codes.Transshipment, hawb.GetHVLVStatusMappingForTest(CMRConsolidatedCargoStatuses.Codes.TranshphrmTranshipmentCargoIsClearButIsIdentifiedAsHighRiskMovement));
		}

		sealed class TestShipment : ForwardingShipment
		{
			public TestShipment(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public TestHAWB hawbForTest;

			public override void OnSaving()
			{
				if (!onSavingCalled)
				{
					onSavingCalled = true;
					hawbForTest.OnSaving();

					base.OnSaving();
				}
			}
			bool onSavingCalled;
		}

		sealed class TestHAWB : CusHAWB
		{
			public TestHAWB(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public override void OnSaving()
			{
				if (!onSavingCalled)
				{
					onSavingCalled = true;
					base.OnSaving();
				}
			}
			bool onSavingCalled;

			public ZString GetHVLVStatusMappingForTest(ZString status)
			{
				return GetHVLVStatusMapping(status);
			}
		}
	}
}
