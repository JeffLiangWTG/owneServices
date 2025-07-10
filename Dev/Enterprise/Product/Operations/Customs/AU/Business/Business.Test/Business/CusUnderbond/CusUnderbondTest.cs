using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Interfaces;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Environment;
using Enterprise.Freight.CFS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CusUnderbond))]
	sealed class CusUnderbondTest : CusUnderbondTest<CusUnderbond>
	{
		public void TestGetNewCusUnderbondProcessTaskCollection()
		{
			AssertType("WorkflowItems's type should be ProcessTaskCollection<CusUnderbondProcessTask, CusUnderbond>", typeof(ProcessTaskCollection<CusUnderbondProcessTask, CusUnderbond>), ((IWorkflowProvider)HAWBUnderbond).WorkflowItems);
		}

		public void TestICusUnderbondCorrectlySetup()
		{
			HAWBUnderbond.C4_MovementReason = CMRUnderbondRequestCodes.Codes.Transshipment;
			Factory.Save();
			AssertEquals(typeof(CusUnderbond), new BusinessObjectFactory().Load<Customs.Business.CusUnderbond>(HAWBUnderbond.PK).GetType());
			AssertEquals(typeof(CusUnderbond), new BusinessObjectFactory().Load<Integration.Customs.AU.ICusUnderbond>(HAWBUnderbond.PK).GetType());
		}

		public void TestAcceptedOutturnAddsRLDEvent()
		{
			var mawb = Factory.New<CusMAWB>();
			mawb.CM_MAWB = "MB1";
			var hawb1 = mawb.ChildBills.AddNew();
			hawb1.CS_CustomsStatus = CMRConsolidatedCargoStatuses.Codes.HeldCargoIsHeldUnderCustomsControl;
			hawb1.CS_HAWB = "HB1";
			var hawb2 = mawb.ChildBills.AddNew();
			hawb2.CS_CustomsStatus = CMRConsolidatedCargoStatuses.Codes.HeldCargoIsHeldUnderCustomsControl;
			hawb2.CS_HAWB = "HB2";
			var hawb3 = mawb.ChildBills.AddNew();
			hawb3.CS_CustomsStatus = CMRConsolidatedCargoStatuses.Codes.ClearCargoIsFreeOfAnyImpedimentsAndMayBeReleased;
			hawb3.CS_HAWB = "HB3";
			hawb3.CS_CargoReceivedDate = ZDateTime.Now;
			var hawb4 = mawb.ChildBills.AddNew();
			hawb4.CS_CustomsStatus = CMRConsolidatedCargoStatuses.Codes.HeldCargoIsHeldUnderCustomsControl;
			hawb4.CS_HAWB = "HB4";
			var underbond = mawb.Underbonds.AddNew();
			var outturn1 = underbond.Outturns.AddNew();
			outturn1.C5_PackagesOutturned = 1;
			outturn1.C5_ParentID = hawb1.PK;
			outturn1.C5_ParentTableCode = CusHAWBSchema.Constants.Prefix;
			outturn1.C5_OutturnResultType = CMROutturnResultType.Codes.ShortLanded;
			var outturn2 = underbond.Outturns.AddNew();
			outturn2.C5_PackagesOutturned = 2;
			outturn2.C5_ParentID = hawb2.PK;
			outturn2.C5_ParentTableCode = CusHAWBSchema.Constants.Prefix;
			var outturn3 = underbond.Outturns.AddNew();
			outturn3.C5_PackagesOutturned = 0;
			outturn3.C5_ParentID = hawb3.PK;
			outturn3.C5_ParentTableCode = CusHAWBSchema.Constants.Prefix;
			outturn3.C5_OutturnResultType = CMROutturnResultType.Codes.ShortLanded;
			var outturn4 = underbond.Outturns.AddNew();
			outturn4.C5_PackagesOutturned = 0;
			outturn4.C5_ParentID = hawb4.PK;
			outturn4.C5_ParentTableCode = CusHAWBSchema.Constants.Prefix;
			outturn4.C5_OutturnResultType = CMROutturnResultType.Codes.NilDiscrepancy;

			underbond.C4_SendersMessageReference = "U00020203";
			var outgoingMessage = underbond.Messages.AddNew(typeof(CMRAIROUTMessage));
			outgoingMessage.EM_MessageSubType = CMRMessage.MessageSubTypes.Original;
			outgoingMessage.EM_ApplicationCode = EDIInterchange.ApplicationCodes.CMR;
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_Status = "SNT";
			outgoingMessage.EM_MessageText = "UNH+1+CUSCAR:D:99B:UN'BGM+263:::AIROUT+U00001799/CMT1:1+9'DTM+570:20120503:102'DTM+570:0812:401'NAD+VW+41065894724::95'TDT+20+442++6+QF::3'LOC+59+9914N::95'DTM+132:20120503:102'CNI++:::I'RFF+ACU:NIL'GID+1'RFF+MWB:08102998329'GID+1'RFF+HWB:HB1'GID+1'PAC+10'FTX+AAA+++LINE 1'CNI++:::I'RFF+ACU:NIL'GID+1'RFF+MWB:08102998329'GID+1'RFF+HWB:HB2'GID+1'PAC+20'FTX+AAA+++LINE 2'CNI++:::I'RFF+ACU:NIL'GID+1'RFF+MWB:08102998329'GID+1'RFF+HWB:HB3'GID+1'PAC+30'FTX+AAA+++LINE 3'UNT+36+1'";
			Factory.Save();
			AssertNull(hawb1.Logs.MostRecentLogByEventTime(Events.ReadyForLocalDelivery));
			Assert(!hawb1.CS_IsHeldAtOutturn);
			hawb1.CS_CustomsStatus = CMRConsolidatedCargoStatuses.Codes.ClearCargoIsFreeOfAnyImpedimentsAndMayBeReleased;
			Factory.Save();
			AssertNull(hawb1.Logs.MostRecentLogByEventTime(Events.ReadyForLocalDelivery));
			Assert(!hawb1.CS_IsHeldAtOutturn);

			var newFactory = new BusinessObjectFactory();
			var airOutMessage = newFactory.New<CMRAIROUTRMessage>();
			airOutMessage.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+961:::AIROUTR+1I9B 2DHC 97F6:001+11'
NAD+MR+FGF697C::95'
RFF+ACW:AIROUT'
RFF+AFM:9'
RFF+ABO:U00020203/BNE1::001'
DTM+310:20060115083841:204'
ERP+1'
ERC+ADVICE:80:95'
ERC+MS5203:6:95'
FTX+AAO+++THIS TRANSACTION WAS ACCEPTED WITHOUT ERRORS AND WARNINGS'
CNT+55:000'
UNT+13+000001'".Replace("\r\n", "");
			airOutMessage.SetEM_LinkedObject();
			airOutMessage.EM_Status = EDIMessage.Status.Received;
			newFactory.Save();

			AssertNotNull("hawb1", hawb1.Logs.MostRecentLogByEventTime(Events.ReadyForLocalDelivery));
			Assert(!hawb1.CS_IsHeldAtOutturn);
			AssertNull("hawb2", hawb2.Logs.MostRecentLogByEventTime(Events.ReadyForLocalDelivery));
			Assert(hawb2.CS_IsHeldAtOutturn);
			AssertNull("hawb3", hawb3.Logs.MostRecentLogByEventTime(Events.ReadyForLocalDelivery));
			Assert(!hawb3.CS_IsHeldAtOutturn);
			AssertNull("hawb4", hawb4.Logs.MostRecentLogByEventTime(Events.ReadyForLocalDelivery));
			Assert(hawb4.CS_IsHeldAtOutturn);

			var hawb1CadLog = hawb1.Logs.MostRecentLogByEventTime(Events.CargoReceivedAtDepot);
			AssertNotNull("hawb1 cadLog", hawb1CadLog);
			AssertEquals("hawb1 CargoReceivedDate", hawb1CadLog.SL_EventTime, hawb1.CS_CargoReceivedDate);

			var hawb2CadLog = hawb2.Logs.MostRecentLogByEventTime(Events.CargoReceivedAtDepot);
			AssertNotNull("hawb2 cadLog", hawb2CadLog);
			AssertEquals("hawb2 CargoReceivedDate", hawb2CadLog.SL_EventTime, hawb2.CS_CargoReceivedDate);

			AssertNull("hawb3 cadLog", hawb3.Logs.MostRecentLogByEventTime(Events.CargoReceivedAtDepot));
			AssertEquals("hawb3 CargoReceivedDate", ZDateTime.Empty, hawb3.CS_CargoReceivedDate);

			var hawb4CadLog = hawb4.Logs.MostRecentLogByEventTime(Events.CargoReceivedAtDepot);
			AssertNotNull("hawb4 cadLog", hawb4CadLog);
			AssertEquals("hawb4 CargoReceivedDate", hawb4CadLog.SL_EventTime, hawb4.CS_CargoReceivedDate);

			hawb2.CS_CustomsStatus = CMRConsolidatedCargoStatuses.Codes.ClearCargoIsFreeOfAnyImpedimentsAndMayBeReleased;
			outturn1.C5_PackagesOutturned = 0;
			outturn3.C5_OutturnResultType = CMROutturnResultType.Codes.NilDiscrepancy;
			underbond.OutturnStatus.Code = CMRBaseStatuses.Codes.AwaitingResponseToAmendment;
			Factory.Save();

			airOutMessage = newFactory.New<CMRAIROUTRMessage>();
			airOutMessage.EM_MessageText = @"UNH+000002+CUSRES:D:99B:UN'
BGM+961:::AIROUTR+1I9B 2DHC 97F6:001+11'
NAD+MR+FGF697C::95'
RFF+ACW:AIROUT'
RFF+AFM:9'
RFF+ABO:U00020203/BNE1::001'
DTM+310:20060115083841:204'
ERP+1'
ERC+ADVICE:80:95'
ERC+MS5203:6:95'
FTX+AAO+++THIS TRANSACTION WAS ACCEPTED WITHOUT ERRORS AND WARNINGS'
CNT+55:000'
UNT+13+000002'".Replace("\r\n", "");
			airOutMessage.SetEM_LinkedObject();
			airOutMessage.EM_Status = EDIMessage.Status.Received;
			newFactory.Save();

			AssertNotNull("hawb1", hawb1.Logs.MostRecentLogByEventTime(Events.ReadyForLocalDelivery));
			Assert(!hawb1.CS_IsHeldAtOutturn);
			AssertNotNull("hawb2", hawb2.Logs.MostRecentLogByEventTime(Events.ReadyForLocalDelivery));
			Assert(!hawb2.CS_IsHeldAtOutturn);
			AssertNotNull("hawb3", hawb3.Logs.MostRecentLogByEventTime(Events.ReadyForLocalDelivery));
			Assert(!hawb3.CS_IsHeldAtOutturn);
			AssertNull("hawb4", hawb4.Logs.MostRecentLogByEventTime(Events.ReadyForLocalDelivery));
			Assert(hawb4.CS_IsHeldAtOutturn);
		}

		public override void TestTranshipmentPortVisible()
		{
			var container = SCAOcean.Containers.AddNew();
			var underbond = (CusUnderbond)((ICusUnderbondDependentCollectionParent)container).Underbonds.AddNew();

			AssertEquals(false, ((ICusUnderbondDependentCollectionParent)underbond).UsesTranshipmentPortOnUnderbond);
		}

		public void TestTranshipmentVisibility()
		{
			var container = SCAOcean.Containers.AddNew();
			var underbond = (CusUnderbond)((ICusUnderbondDependentCollectionParent)container).Underbonds.AddNew();

			AssertEquals(false, underbond.TranshipmentPortVisible);

			underbond.C4_MovementReason = CMRUnderbondRequestCodes.Codes.Transshipment;
			AssertEquals(true, underbond.TranshipmentPortVisible);

			underbond.C4_MovementReason = CMRUnderbondRequestCodes.Codes.OtherMovement;
			AssertEquals(false, underbond.TranshipmentPortVisible);
		}

		public void TestIsChangingUniqueIdentifier()
		{
			var underbond = Factory.New<CusUnderbond>();
			AssertEquals("pre-condition expected status", CMRBaseStatuses.Codes.NotSent, underbond.OutturnStatus.Code);
			AssertEquals("pre-condition expected value", false, underbond.IsChangingUniqueIdentifier);

			underbond.C4_MAWB = "081-12345678";
			Factory.Save();
			AssertEquals("IsChangingUniqueIdentifier should be false when no messages have been sent", false, underbond.IsChangingUniqueIdentifier);

			underbond.OutturnStatus.Code = CMRBaseStatuses.Codes.WithdrawalAccepted;
			underbond.C4_MAWB = "081-12345687";
			Factory.Save();
			AssertEquals("IsChangingUniqueIdentifier should be false when in Withdrawn status", false, underbond.IsChangingUniqueIdentifier);

			underbond.OutturnStatus.Code = CMRBaseStatuses.Codes.AmendmentAccepted;
			underbond.C4_MAWB = "081-3333333";
			AssertEquals("Expected C4_MAWBInfo.HasChanges to be true", true, underbond.C4_MAWBInfo.HasChanges);
			AssertEquals("IsChangingUniqueIdentifier should be true when in any other status and MAWB is changed", true, underbond.IsChangingUniqueIdentifier);
		}

		public void TestLinkedObjectAsCusHAWB()
		{
			AssertEquals("LinkedObject", HAWB, HAWBUnderbond.LinkedObject);
		}

		public void TestMessageCollections()
		{
			var underbond = Factory.New<CusUnderbond>();
			var cARSTMessage = Factory.New<CMRCARSTMessage>();
			cARSTMessage.EM_MessageText = EDIMessage.MessageNumberPlaceHolder;
			underbond.Messages.Add(cARSTMessage);
			var aIROUTMessage = Factory.New<CMRAIROUTMessage>();
			aIROUTMessage.EM_MessageText = EDIMessage.MessageNumberPlaceHolder;
			aIROUTMessage.EM_MessageType = CMRMessage.CMRMessageTypes.AIROUT;
			underbond.Messages.Add(aIROUTMessage);
			var uBMREQRMessage = Factory.New<CMRUBMREQRMessage>();
			uBMREQRMessage.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+961:::UBMREQR+2C4G E33F GJ0F:1+32'
FTX+AAH+++CJM436P20191'
TDT+20+156++11++++7619422::11'
TDT+1++ROA'
LOC+5+8136B::95'
LOC+4+8139A::95'
NAD+MR+CJM436P::95'
RFF+ANX:EXPECTED CARGO ARRIVAL ADVICE'
RFF+ACD:MOV'
DOC+1'
PAC+++LCL:67:95'
PAC+0000020++CT:185:95'
RFF+AAQ:HALE2525260'
RFF+MB:OCEAN20050613DD'
RFF+BH:HOUSE45454545'
UNT+17+000001'".Replace("\r\n", "");
			underbond.Messages.Add(uBMREQRMessage);
			Factory.Save();
			AssertCollectionContains(cARSTMessage, underbond.Messages);
			AssertCollectionContains(aIROUTMessage, underbond.Messages);
			AssertCollectionNotContains("CARST message should NOT be in the Outturn collection", cARSTMessage, underbond.OutturnMessages);
			AssertCollectionContains("CARST message should be in the carst collection", cARSTMessage, underbond.CargoMessages);
			AssertCollectionNotContains("Outturn message should NOT be in the carst Collection", aIROUTMessage, underbond.CargoMessages);
			AssertCollectionContains("UBMREQR should be in cargo collection", uBMREQRMessage, underbond.CargoMessages);
			AssertCollectionNotContains("UBMREQR should not be in outturn collection", uBMREQRMessage, underbond.OutturnMessages);
			AssertEquals(1, underbond.OutturnMessages.Count);
			AssertEquals(CMRMessage.CMRMessageTypes.AIROUT, underbond.OutturnMessages[0].EM_MessageType);
		}

		[TestDate(2005, 12, 18)]
		public void TestAMIIssue00013006()
		{
			GlbCompany.CurrentCompany.OrgProxy.MainAddress.LocalControlledPremisesID = "EM18N";
			GlbCompany.CurrentCompany.OrgProxy.OH_IsUnpackDepot = true;
			var mAWB = Factory.New<CusMAWB>();
			mAWB.CM_MAWB = "08165225436";
			mAWB.CM_FlightNo = "QF002";
			mAWB.CM_ArrivalDate = new ZDateTime(2005, 12, 12);
			var underbond1 = (CusUnderbond)((ICusUnderbondDependentCollectionParent)mAWB).Underbonds.AddNew();
			underbond1.C4_SendersMessageReference = "U00003023";
			underbond1.C4_OriginPremiseID = "9532M";
			underbond1.C4_DestinationPremiseID = "DP41B";
			underbond1.C4_FlightNo = "QF002";
			underbond1.C4_ArrivalDate = new ZDateTime(2005, 12, 12);
			var underbond2 = (CusUnderbond)((ICusUnderbondDependentCollectionParent)mAWB).Underbonds.AddNew();
			underbond2.C4_SendersMessageReference = "U00003024";
			underbond2.C4_OriginPremiseID = "9532M";
			underbond2.C4_DestinationPremiseID = "DP41B";
			underbond2.C4_FlightNo = "QF010";
			underbond2.C4_ArrivalDate = new ZDateTime(2005, 12, 12);

			var logger = new BatchProcessor.LoggingInformation();
			var processor = new UBMREQRMessageProcessor(logger);
			Factory.Save();

			var message = Factory.New<CMRUBMREQRMessage>();
			message.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+961:::UBMREQR+2JBA B6EB 23BF:1+32'
DTM+9:20051213075015260513:ZZZ'
DTM+132:20051212:102'
FTX+AAH+++FGE743GU00003023/SYD1'
TDT+20+002++6+QF::3'
TDT+1++ROA'
LOC+5+9532M::95'
LOC+4+DP41B::95'
NAD+MR+FGE743G::95'
NAD+UD+28078835604::95'
RFF+ABO:U00003023/SYD1::1'
RFF+ANX:UNDERBOND APPROVAL'
RFF+ACD:DCL'
DOC+1'
PAC+++AIR:67:95'
PAC+0000110'
RFF+MWB:08165225436'
UNT+19+000001'".Replace("\r\n", "");
			processor.ProcessMessage(message);

			var message2 = Factory.New<CMRUBMREQRMessage>();
			message2.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+961:::UBMREQR+349J 5AII C3BF:1+32'
DTM+9:20051213075005164468:ZZZ'
DTM+132:20051212:102'
FTX+AAH+++FGE743GU00003024/SYD1'
TDT+20+010++6+QF::3'
TDT+1++ROA'
LOC+5+9532M::95'
LOC+4+DP41B::95'
NAD+MR+FGE743G::95'
NAD+UD+28078835604::95'
RFF+ABO:U00003024/SYD1::1'
RFF+ANX:UNDERBOND APPROVAL'
RFF+ACD:DCL'
DOC+1'
PAC+++AIR:67:95'
PAC+0000001'
RFF+MWB:08165225436'
UNT+19+000001'".Replace("\r\n", "");
			message2.SetEM_LinkedObject();

			var message3 = Factory.New<CMRUBMREQRMessage>();
			message3.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+961:::UBMREQR+2D48 F7JB 23BF:1+32'
DTM+9:20051213075015299870:ZZZ'
DTM+132:20051212:102'
FTX+AAH+++FGE743GU00003023/SYD1'
TDT+20+002++6+QF::3'
TDT+1++ROA'
LOC+5+9532M::95'
LOC+4+DP41B::95'
NAD+MR+FGE743G::95'
NAD+UD+28078835604::95'
RFF+ABO:U00003023/SYD1::1'
RFF+ANX:EXPECTED CARGO ARRIVAL ADVICE'
RFF+ACD:DCL'
DOC+1'
PAC+++AIR:67:95'
PAC+0000110'
RFF+MWB:08165225436'
UNT+19+000001'".Replace("\r\n", "");
			message3.SetEM_LinkedObject();

			var message4 = Factory.New<CMRUBMREQRMessage>();
			message4.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+961:::UBMREQR+4C9D 16B8 C3BF:1+32'
DTM+9:20051213075005221150:ZZZ'
DTM+132:20051212:102'
FTX+AAH+++FGE743GU00003024/SYD1'
TDT+20+010++6+QF::3'
TDT+1++ROA'
LOC+5+9532M::95'
LOC+4+DP41B::95'
NAD+MR+FGE743G::95'
NAD+UD+28078835604::95'
RFF+ABO:U00003024/SYD1::1'
RFF+ANX:EXPECTED CARGO ARRIVAL ADVICE'
RFF+ACD:DCL'
DOC+1'
PAC+++AIR:67:95'
PAC+0000001'
RFF+MWB:08165225436'
UNT+19+000001'".Replace("\r\n", "");
			message4.SetEM_LinkedObject();

			AssertEquals("CusUnderbond", message.EM_LinkTable);
			AssertEquals("CusUnderbond", message2.EM_LinkTable);
			AssertEquals("CusUnderbond", message3.EM_LinkTable);
			AssertEquals("CusUnderbond", message4.EM_LinkTable);
			AssertEquals("Message should have been assigned to a newly created underbond", 2, mAWB.AllUnderbonds[0].Messages.Count);
			AssertEquals("Message should have been assigned to a newly created underbond", 2, mAWB.AllUnderbonds[1].Messages.Count);
		}

		public void TestLinkedObjectAsCusMAWB()
		{
			AssertEquals("LinkedObject", MAWB, MAWBUnderbond.LinkedObject);
		}

		public void TestLinkedObjectAsCTOCusMAWB()
		{
			AssertEquals("LinkedObject", CTOMAWB, CTOMAWBUnderbond.LinkedObject);
		}

		public void TestLinkedObjectAsCTOCusHAWB()
		{
			AssertEquals("LinkedObject", CTOHAWB, CTOHAWBUnderbond.LinkedObject);
		}

		public void TestLinkedObjectAsCusPartShip()
		{
			AssertEquals("LinkedObject", PartShip, PartShipUnderbond.LinkedObject);
		}

		public void TestCanSendWithoutDelay()
		{
			var underbond = Factory.New<CusUnderbond>();
			Assert(((ICusUnderbondDependentCollectionParent)underbond).CanSendWithoutDelay);
		}

		public void TestIsAirCargo()
		{
			var underbond = Factory.New<CusUnderbond>();
			underbond.C4_ParentTableCode = CusSCADepotHouseSchema.Constants.Prefix;
			AssertEquals(false, underbond.IsAirCargo);
			underbond.C4_ParentTableCode = CusHAWBSchema.Constants.Prefix;
			AssertEquals(true, underbond.IsAirCargo);
			underbond.C4_ParentTableCode = CusSCAContainerSchema.Constants.Prefix;
			AssertEquals(false, underbond.IsAirCargo);
			underbond.C4_ParentTableCode = CusMAWBSchema.Constants.Prefix;
			AssertEquals(true, underbond.IsAirCargo);
		}

		public void TestParentDetailsForCusHAWB()
		{
			HAWBUnderbond.C4_ModeOfMovement = ZString.Empty;
			AssertEquals("Details", HAWB.Details, HAWBUnderbond.Details);
		}

		public void TestLookups()
		{
			AssertEquals("LookupsType", typeof(CusUnderbondLookups), HAWBUnderbond.Lookups.GetType());
		}

		public void TestDefaultValues()
		{
			var testUnderbond = Factory.New<CusUnderbond>();
			AssertEquals(CMRUnderbondModeOfMovement.Codes.Road, testUnderbond.C4_ModeOfMovement);
			AssertEquals(CMRUnderbondRequestCodes.Codes.UnpackLclAtDestination, testUnderbond.C4_MovementReason);
		}

		public void TestDeleteNoLongerThrowException_00827659()
		{
			var testUnderbond = Factory.New<CusUnderbond>();
			testUnderbond.UnderbondStatus.Code = CMRBaseStatuses.Codes.AwaitingResponseToOriginal;
			Assert(!testUnderbond.CanDelete);

			testUnderbond.UnderbondStatus.Code = CMRBaseStatuses.Codes.NotSent;
			Assert(testUnderbond.CanDelete);
		}

		public void TestSetDefaultValuesFromMAWB()
		{
			GlbCompany.GetCurrentCompany(Factory).OrgProxy.PrimaryRegistrationNumber.Number = "35234";
			var mAWB = Factory.New<CusMAWB>();
			mAWB.CM_FlightNo = "QF123";
			mAWB.CM_ResponsiblePartyID = "55555555555";
			var hAWB1 = mAWB.ChildBills.AddNew();
			hAWB1.CS_PiecesManifested = 125;
			var hAWB2 = mAWB.ChildBills.AddNew();
			hAWB2.CS_PiecesManifested = 25;
			var testUnderbond = mAWB.Underbonds.AddNew();
			testUnderbond.C4_ParentID = mAWB.PK;
			testUnderbond.C4_ParentTableCode = "CM";
			testUnderbond.SetDefaultValuesFromParent();
			AssertEquals("QF123", testUnderbond.C4_FlightNo);
			AssertEquals(150u, testUnderbond.C4_PiecesManifested);
			AssertEquals("35234", testUnderbond.C4_ResponsiblePartyID);
			mAWB.CM_ResponsiblePartyID = "";
			var testUnderbond2 = mAWB.Underbonds.AddNew();
			AssertEquals("35234", testUnderbond2.C4_ResponsiblePartyID);
		}

		public override void TestVoyageAndVesselDetailsVisible()
		{
			HAWBUnderbond.C4_ModeOfMovement = CMRUnderbondModeOfMovement.Codes.SeaDomesticVessel;
			AssertEquals("VoyageAndVesselDetailsVisible", false, HAWBUnderbond.VoyageAndVesselDetailsVisible);
			HAWBUnderbond.C4_ModeOfMovement = CMRUnderbondModeOfMovement.Codes.SeaInternationalVessel;
			AssertEquals("VoyageAndVesselDetailsVisible", true, HAWBUnderbond.VoyageAndVesselDetailsVisible);
			HAWBUnderbond.C4_ModeOfMovement = CMRUnderbondModeOfMovement.Codes.Road;
			AssertEquals("VoyageAndVesselDetailsVisible", false, HAWBUnderbond.VoyageAndVesselDetailsVisible);
		}

		public void TestValidation()
		{
			AssertEquals("Validation Type", typeof(CusUnderbondValidation), HAWBUnderbond.Validation.GetType());
		}

		public void TestLookup()
		{
			AssertEquals("Lookup Type", typeof(CusUnderbondLookups), HAWBUnderbond.Lookups.GetType());
		}

		public void TestUnderbondBySeaVesselID()
		{
			AssertEquals("Underbond by see Vessel ID", "", HAWBUnderbond.UnderbondBySeaVesselID);

			var vessel = Factory.LoadTop1<RefVessel>(new ZQuery());
			vessel.RV_LloydsNumber = "9x9x9x9";
			HAWBUnderbond.C4_UnderbondBySeaVessel = vessel.RV_Code;
			AssertEquals("Underbond by see Vessel ID", vessel.RV_LloydsNumber, HAWBUnderbond.UnderbondBySeaVesselID);
		}

		public void TestCalculator()
		{
			AssertNotNull(HAWBUnderbond.Calculator);
		}

		public override void TestDefaultUnderbondStatus()
		{
			AssertEquals("DefaultUnderbondStatus", CMRBaseStatuses.Codes.NotSent, HAWBUnderbond.UnderbondStatus.Status.CE_EntryStatus);
		}

		public override void TestDefaultOutturnStatus()
		{
			AssertEquals("DefaultOutturnStatus", CMRBaseStatuses.Codes.NotSent, HAWBUnderbond.OutturnStatus.Status.CE_EntryStatus);
		}

		public void TestOutturnStatusCalculator()
		{
			AssertNotNull("OutturnStatusCalculator", HAWBUnderbond.OutturnStatusCalculator);
		}

		public void TestCanDoOutturnChangedFirredOnDestinationAddressSet()
		{
			HAWBUnderbond.CanDoOutturnChanged += new EventHandler(HAWBUnderbond_CanDoOutturnChanged);
			AssertEquals("HAWBUnderbond_CanDoOutturnChanged", 0, canDoOutturnChangedCount);
			HAWBUnderbond.C4_OA_DestinationAddress = ZGuid.NewZGuid();
			AssertEquals("HAWBUnderbond_CanDoOutturnChanged", 1, canDoOutturnChangedCount);
		}

		public void TestCanDoOutturnChangedFirredOnDestinationEstIDSet()
		{
			HAWBUnderbond.CanDoOutturnChanged += new EventHandler(HAWBUnderbond_CanDoOutturnChanged);
			AssertEquals("HAWBUnderbond_CanDoOutturnChanged", 0, canDoOutturnChangedCount);
			HAWBUnderbond.C4_DestinationPremiseID = "12332132";
			AssertEquals("HAWBUnderbond_CanDoOutturnChanged", 1, canDoOutturnChangedCount);
		}

		public new void TestGetCanDoOutturn()
		{
			HAWBUnderbond.C4_DestinationPremiseID = "AA33N";
			AssertEquals("GetCanDoOutturn", false, HAWBUnderbond.CanDoOutturn);
			var codeDescriptionPairList = new CodeDescriptionPairList();
			codeDescriptionPairList.AddPair("AA33N", "67094168242");
			FreightDataRegistry.Instance.OuturnResponsiblePartyIDOverride.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, codeDescriptionPairList);
			AssertEquals("GetCanDoOutturn", true, HAWBUnderbond.CanDoOutturn);
			HAWBUnderbond.C4_DestinationPremiseID = "";
			AssertEquals("GetCanDoOutturn", false, HAWBUnderbond.CanDoOutturn);
			GlbCompany.GetCurrentCompany(Factory).OrgProxy.Addresses[0].LocalControlledPremisesID = "12345";
			AssertEquals("GetCanDoOutturn", false, HAWBUnderbond.CanDoOutturn);
			HAWBUnderbond.C4_DestinationPremiseID = "12345";
			AssertEquals("GetCanDoOutturn", true, HAWBUnderbond.CanDoOutturn);
			HAWBUnderbond.C4_DestinationPremiseID = "24680";
			AssertEquals("GetCanDoOutturn", false, HAWBUnderbond.CanDoOutturn);
			GlbCompany.GetCurrentCompany(Factory).OrgProxy.Addresses[0].LocalControlledPremisesID = "9914n";
			HAWBUnderbond.C4_DestinationPremiseID = "9914N";
			AssertEquals("GetCanDoOutturn", true, HAWBUnderbond.CanDoOutturn);
			HAWBUnderbond.Delete();
			AssertEquals("GetCanDoOutturn", false, HAWBUnderbond.CanDoOutturn);
		}

		public void TestGetCanDoOutturnForMultipleBranches()
		{
			GlbCompany.GetCurrentCompany(Factory).OrgProxy.Addresses[0].LocalControlledPremisesID = "12345";
			var newBranch = GlbCompany.GetCurrentCompany(Factory).Branches.AddNew();
			var newProxy = newBranch.Factory.New<OrgHeader>();
			newBranch.GB_OH_OrgProxy = newProxy.PK;
			var address1 = newBranch.OrgProxy.Addresses.AddNew();
			address1.OA_Address1 = "Cuckoo Squeaker St";
			address1.OA_Address2 = "SqueakerVille";
			address1.LocalControlledPremisesID = "Squeaker";
			AssertEquals("GetCanDoOutturn", false, HAWBUnderbond.CanDoOutturn);
			HAWBUnderbond.C4_DestinationPremiseID = "Squeaker";
			AssertEquals("GetCanDoOutturn", true, HAWBUnderbond.CanDoOutturn);
			HAWBUnderbond.C4_DestinationPremiseID = "Gibbiceps";
			AssertEquals("GetCanDoOutturn", false, HAWBUnderbond.CanDoOutturn);
			HAWBUnderbond.C4_DestinationPremiseID = "12345";
			AssertEquals("GetCanDoOutturn", true, HAWBUnderbond.CanDoOutturn);
		}

		public void TestGetCanDoOutturnFromBranches()
		{
			GlbBranch.GetCurrentBranch(Factory).OrgProxy.Addresses[0].LocalControlledPremisesID = "T949G";
			HAWBUnderbond.C4_DestinationPremiseID = "T949G";
			AssertEquals("GetCanDoOutturn", true, HAWBUnderbond.CanDoOutturn);
			HAWBUnderbond.C4_DestinationPremiseID = "30293";
			AssertEquals("GetCanDoOutturn", false, HAWBUnderbond.CanDoOutturn);
			GlbBranch.GetCurrentBranch(Factory).OrgProxy.Addresses[0].LocalControlledPremisesID = "J493O";
			HAWBUnderbond.C4_DestinationPremiseID = "J493O";
			AssertEquals("GetCanDoOutturn", true, HAWBUnderbond.CanDoOutturn);
		}

		public void TestGetCanDoOutturnFromCFSOrg() => CombineAssertions(() =>
		{
			var cfsOrg = Factory.New<OrgHeader>();
			cfsOrg.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "1234R", "AU");
			var serviceRelatedParty = cfsOrg.AllRelatedParties.AddNew();
			serviceRelatedParty.PR_PartyType = RelatedPartyTypeList.Codes.AuthorizedCargoReporter;
			serviceRelatedParty.PR_OH_RelatedParty = GlbCompany.GetCurrentCompany(Factory).GC_OH_OrgProxy;
			MAWB.CM_OA_UnpackDepotAddress = cfsOrg.MainAddress.PK;
			HAWBUnderbond.C4_DestinationPremiseID = "1234R";
			AssertEquals("Underbond is destined to CFS org", true, HAWBUnderbond.CanDoOutturn);

			HAWBUnderbond.C4_DestinationPremiseID = "9999";
			AssertEquals("CCP doesn't match", false, HAWBUnderbond.CanDoOutturn);

			HAWBUnderbond.C4_DestinationPremiseID = "1234R";
			serviceRelatedParty.PR_OH_RelatedParty = Factory.New<OrgHeader>().PK;
			AssertEquals("Service related party doesn't match", false, HAWBUnderbond.CanDoOutturn);

			cfsOrg.ServiceRelatedParties.RemoveAndDeleteAll();
			AssertEquals("No service related party", false, HAWBUnderbond.CanDoOutturn);
		});

		public void TestGetIsUnderbondForSeaShipment()
		{
			var underbond = Factory.New<CusUnderbond>();
			AssertEquals(false, underbond.IsUnderbondForSeaShipment);
			underbond.LinkedObject = Factory.New<CusSCAPivot>();
			AssertEquals(true, underbond.IsUnderbondForSeaShipment);
			underbond.LinkedObject = Factory.New<CusSCAContainer>();
			AssertEquals(true, underbond.IsUnderbondForSeaShipment);
			underbond.LinkedObject = Factory.New<CusSeaManOBLDetail>();
			AssertEquals(true, underbond.IsUnderbondForSeaShipment);
			underbond.LinkedObject = Factory.New<CusHAWB>();
			AssertEquals(false, underbond.IsUnderbondForSeaShipment);
		}

		public void TestGetIsUnderbondForDepotSeaShipment()
		{
			var underbond = Factory.New<CusUnderbond>();
			AssertEquals(false, underbond.IsUnderbondForSeaShipment);
			var consol = Factory.New<CFSLoadListConsol>();
			var container = consol.Containers.AddNew();
			underbond.LinkedObject = CFSContainerWrapper.Load(container);
			AssertEquals(true, underbond.IsUnderbondForSeaShipment);
			var shipment = consol.Shipments.AddNew();
			underbond.LinkedObject = CFSShipmentWrapper.Load(shipment);
			AssertEquals(true, underbond.IsUnderbondForSeaShipment);
			underbond.LinkedObject = CFSContainerWrapper.Load(container);
			AssertEquals(true, underbond.IsUnderbondForSeaShipment);
			underbond.LinkedObject = null;
			AssertEquals(false, underbond.IsUnderbondForSeaShipment);
			underbond.LinkedObject = CFSContainerWrapper.Load(container);
			AssertEquals(true, underbond.IsUnderbondForSeaShipment);
		}

		public void TestIDataExportCSVFileNameProviderMembers()
		{
			var oceanBill = Factory.New<CusSCAOceanBill>();
			var container = oceanBill.Containers.AddNew();
			container.CN_ContainerNumber = "CONTAINER1";
			var underBond = Factory.New<CusUnderbond>();
			underBond.C4_SendersMessageReference = "U111111";
			underBond.LinkedObject = container;
			var fileNameProvider = underBond as IDataExportCSVFileNameProvider;

			AssertEquals("file name suffix", "U111111_Container CONTAINER1", fileNameProvider.FileNameSuffix);
		}

		public void TestLinkedObjectName()
		{
			var oceanBill = Factory.New<CusSCAOceanBill>();
			var container1 = oceanBill.Containers.AddNew();
			container1.CN_ContainerNumber = "CONTAINER1";
			var container2 = oceanBill.Containers.AddNew();
			container2.CN_ContainerNumber = "CONTAINER2";
			var house1 = oceanBill.HouseBills.AddNew();
			house1.CA_HouseBill = "HOUSEBILL1";
			var house2 = oceanBill.HouseBills.AddNew();
			house2.CA_HouseBill = "HOUSEBILL2";

			CusSCAPivot pack1 = house1.Pivot.AddNew();
			pack1.CV_CN = container1.PK;

			CusSCAPivot pack2 = house2.Pivot.AddNew();
			pack2.CV_CN = container1.PK;

			//CusUnderbond Underbond = (CusUnderbond)OceanBill.Underbonds.AddNew();
			var underbond = Factory.New<CusUnderbond>();

			AssertEquals("Underbonds should not be linked by default", "", underbond.LinkedObjectName);
			AssertEquals("Underbonds should not be linked by default", null, underbond.LinkedObject);

			underbond.LinkedObject = container1;
			AssertEquals("Container CONTAINER1", underbond.LinkedObjectName);
			AssertEquals(container1, underbond.LinkedObject);

			AssertEquals("Underbond should be linked to OceanBill", 4, underbond.Lookups.AllUnderbondForList.Count);

			var underbondCode = underbond.Lookups.AllUnderbondForList[2].Code;
			var expectedPack = oceanBill.Pivots.Cast<ICusUnderbondDependentCollectionParent>().First(c => c.UnderbondHumanReadableName == underbondCode);
			underbond.LinkedObjectName = underbondCode;
			AssertEquals(expectedPack.UnderbondHumanReadableName, underbond.LinkedObjectName);
			AssertEquals(expectedPack, underbond.LinkedObject);

			underbond.LinkedObjectName = "";
			AssertEquals("", underbond.LinkedObjectName);
			AssertEquals(null, underbond.LinkedObject);
		}

		const string ABN = "36103224237";
		public void TestSetDefaultValues()
		{
			var currentCompanyProxy = GlbCompany.GetCurrentCompany(Factory).OrgProxy;
			currentCompanyProxy.PrimaryRegistrationNumber.Number = ABN;
			var newUnderbond = Factory.New<CusUnderbond>();
			AssertEquals("Responsible Party ID Default", ABN, newUnderbond.C4_ResponsiblePartyID);
			AssertEquals("Our Party ID for destination", currentCompanyProxy.MainAddress.LocalControlledPremisesID, newUnderbond.C4_DestinationPremiseID);
			AssertEquals("C4_ApplicationCode", Customs.Business.CusUnderbondApplicationCodeList.Codes.AUUnderbond, newUnderbond.C4_ApplicationCode);
		}

		const string ABNNumberWithSpaces = "75 006 687 958";
		const string ABNNumberWithOutSpaces = "75006687958";
		public void TestABNSpacesStripped()
		{
			var underbond = Factory.New<CusUnderbond>();
			underbond.C4_ResponsiblePartyID = ABNNumberWithSpaces;
			AssertEquals("Responsible Party ID", ABNNumberWithOutSpaces, underbond.C4_ResponsiblePartyID);
		}

		public void TestVesselNameDefaultingFromLloyds()
		{
			const string vesselName = "Vessel001";
			AssertEquals("Precondition: There are no Vessels named " + vesselName, 0, Factory.Load<RefVessel>(new ZQuery(RefVesselSchema.RV_Code, vesselName)).Length);
			const string lloydsNumber = "112233";
			AssertEquals("Precondition: There are no Vessels with Lloyds Number " + lloydsNumber, 0, Factory.Load<RefVessel>(new ZQuery(RefVesselSchema.RV_LloydsNumber, lloydsNumber)).Length);

			var newUnderbond1 = Factory.New<CusUnderbond>();
			newUnderbond1.C4_UnderbondBySeaLloydsIMONum = lloydsNumber;
			AssertEquals("Vessel Name does not update when Lloyds does not match any vessels", ZString.Empty, newUnderbond1.C4_UnderbondBySeaVessel);

			var vesselA = Factory.New<RefVessel>();
			vesselA.RV_Code = vesselName;
			vesselA.RV_LloydsNumber = lloydsNumber;

			var newUnderbond2 = Factory.New<CusUnderbond>();
			newUnderbond2.C4_UnderbondBySeaLloydsIMONum = lloydsNumber;
			AssertEquals("Vessel Name updates when Lloyds matches a vessel", vesselA.RV_Code, newUnderbond2.C4_UnderbondBySeaVessel);
		}

		public void TestLloydsNumberDefaulting()
		{
			const string LloydsNum1 = "7705934";
			const string LloydsNum2 = "9121273";
			var vessel1 = Factory.LoadTop1<RefVessel>(new ZQuery(RefVesselSchema.RV_LloydsNumber, LloydsNum1));
			var vessel2 = Factory.LoadTop1<RefVessel>(new ZQuery(RefVesselSchema.RV_LloydsNumber, LloydsNum2));
			var newUnderbond = Factory.New<CusUnderbond>();
			newUnderbond.C4_UnderbondBySeaVessel = vessel1.RV_Code;
			AssertEquals("Underbond Lloyds Number", LloydsNum1, newUnderbond.C4_UnderbondBySeaLloydsIMONum);

			newUnderbond.C4_UnderbondBySeaVessel = ZString.Empty;
			newUnderbond.C4_UnderbondBySeaLloydsIMONum = LloydsNum2;
			AssertEquals("Underbond Lloyds Number", vessel2.RV_Code, newUnderbond.C4_UnderbondBySeaVessel);
		}

		public void TestLloydsNumberDefaulting_MultipleVessels()
		{
			const string vesselName = "Vessel001";
			AssertEquals("Precondition: There are no Vessels named " + vesselName, 0, Factory.Load<RefVessel>(new ZQuery(RefVesselSchema.RV_Code, vesselName)).Length);

			var newUnderbond1 = Factory.New<CusUnderbond>();
			newUnderbond1.C4_UnderbondBySeaVessel = vesselName;
			AssertEquals("Lloyds does not update when RV_Code does not match any vessels", ZString.Empty, newUnderbond1.C4_UnderbondBySeaLloydsIMONum);

			var vesselA = Factory.New<RefVessel>();
			vesselA.RV_Code = vesselName;
			vesselA.RV_LloydsNumber = "112233";

			var newUnderbond2 = Factory.New<CusUnderbond>();
			newUnderbond2.C4_UnderbondBySeaVessel = vesselName;
			AssertEquals("Lloyds updates when RV_Code matches a single vessel", vesselA.RV_LloydsNumber, newUnderbond2.C4_UnderbondBySeaLloydsIMONum);

			var vesselB = Factory.New<RefVessel>();
			vesselB.RV_Code = vesselName;
			vesselB.RV_LloydsNumber = "998877";

			var newUnderbond3 = Factory.New<CusUnderbond>();
			newUnderbond3.C4_UnderbondBySeaVessel = vesselName;
			AssertEquals("Lloyds does not update when RV_Code matches multiple vessels", ZString.Empty, newUnderbond3.C4_UnderbondBySeaLloydsIMONum);
		}

		public void TestSetUnderbondBySeaVessel()
		{
			var vessel = Factory.New<RefVessel>();
			vessel.RV_Code = "Vessel001";
			vessel.RV_LloydsNumber = "112233";

			var underbond = Factory.New<CusUnderbond>();
			underbond.SetUnderbondBySeaVessel(vessel);
			AssertEquals(vessel.RV_Code, underbond.C4_UnderbondBySeaVessel);
			AssertEquals(vessel.RV_LloydsNumber, underbond.C4_UnderbondBySeaLloydsIMONum);
		}

		public void TestDefaultC4_IsMoveFromDischarge()
		{
			var underbond = Factory.New<CusUnderbond>();
			AssertEquals("Default value for C4_IsMoveFromDischarge", true, underbond.C4_IsMoveFromDischarge);
		}

		public void TestC4_Status()
		{
			var underbond = Factory.New<CusUnderbond>();
			underbond.C4_Status = CMRUnderbondStatuses.Codes.ExpectedCargoArrivalAdviceReceived;
			AssertEquals("Underbond Status Description ExpectedCargoArrivalAdvice", CMRUnderbondStatuses.Descriptions.ExpectedCargoArrivalAdviceReceived, underbond.ApprovalStatus);
			underbond.C4_Status = CMRUnderbondStatuses.Codes.UnderbondApprovalAdviceReceived;
			AssertEquals("Underbond Status Description ExpectedCargoArrivalAdvice", CMRUnderbondStatuses.Descriptions.UnderbondApprovalAdviceReceived, underbond.ApprovalStatus);
			underbond.C4_Status = CMRUnderbondStatuses.Codes.UnderbondApprovalRescindAdviceReceived;
			AssertEquals("Underbond Status Description ExpectedCargoArrivalAdvice", CMRUnderbondStatuses.Descriptions.UnderbondApprovalRescindAdviceReceived, underbond.ApprovalStatus);
			underbond.C4_Status = CMRUnderbondStatuses.Codes.ExpectedCargoArrivalRescindAdviceReceived;
			AssertEquals("Underbond Status Description ExpectedCargoArrivalAdvice", CMRUnderbondStatuses.Descriptions.ExpectedCargoArrivalRescindAdviceReceived, underbond.ApprovalStatus);
		}

		public void TestShortDescription()
		{
			var oceanBill = Factory.New<CusSCAOceanBill>();
			oceanBill.CB_OceanBill = "12345";
			var container1 = oceanBill.Containers.AddNew();
			container1.CN_ContainerNumber = "CONTAINER1";
			var house1 = oceanBill.HouseBills.AddNew();
			house1.CA_HouseBill = "HOUSEBILL1";

			var pack1 = house1.Pivot.AddNew();
			pack1.CV_CN = container1.PK;

			var underbond = pack1.Underbonds.AddNew();

			AssertEquals("Container CONTAINER1 - Housebill HOUSEBILL1", underbond.LinkedObjectName);
			AssertEquals("ShortDescription", "Ocean Bill: 12345", underbond.ShortDescription);
		}

		public void TestEqualsByAirKeyFields()
		{
			var underbond1 = Factory.New<CusUnderbond>();
			var underbond2 = Factory.New<CusUnderbond>();

			underbond1.C4_ParentTableCode = CusMAWBSchema.Constants.Prefix;
			underbond2.C4_ParentTableCode = CusMAWBSchema.Constants.Prefix;

			AssertEquals(false, underbond1.EqualsByAirKeyFields(underbond2));

			underbond1.C4_FlightNo = "QF123";
			underbond2.C4_FlightNo = "QF123";

			underbond1.C4_DestinationPremiseID = "9914N";
			underbond2.C4_DestinationPremiseID = "9914N";

			underbond1.C4_ArrivalDate = ZDateTime.BrettsBirthday;
			underbond2.C4_ArrivalDate = ZDateTime.BrettsBirthday;

			underbond1.C4_Outurned = new ZDateTime(2007, 03, 10);
			underbond2.C4_Outurned = new ZDateTime(2007, 03, 10);

			AssertEquals(true, underbond1.EqualsByAirKeyFields(underbond2));

			AssertEqualByAirKeyFields(underbond1, underbond2, underbond1.C4_FlightNoInfo, (ZString)"foo");
			AssertEqualByAirKeyFields(underbond1, underbond2, underbond1.C4_DestinationPremiseIDInfo, (ZString)"bar");
			AssertEqualByAirKeyFields(underbond1, underbond2, underbond1.C4_ArrivalDateInfo, new ZDateTime(2006, 2, 3));
			AssertEqualByAirKeyFields(underbond1, underbond2, underbond1.C4_OuturnedInfo, new ZDateTime(2006, 2, 3));
			AssertEqualByAirKeyFields(underbond1, underbond2, underbond1.C4_ParentTableCodeInfo, ZString.Empty);

			AssertEquals("postcondition: we haven't actually messed up in our helper method", true, underbond1.EqualsByAirKeyFields(underbond2));
		}

		#region IJobInvoicingPlugIn Members

		public void TestAuditSecurity()
		{
			AssertEquals("AuditSecurity", Env.Security.ACAOutturnBillsAuditBilling, ((IJobInvoicingPlugIn)Factory.New<CusUnderbond>()).InvoicingSupporter.AuditSecurity);
		}

		public void TestEditSecurity()
		{
			IJobInvoicingPlugIn testJob = Factory.New<CusUnderbond>();
			AssertEquals("EditSecurityCheckPoint should be Empty", Env.Security.None, testJob.InvoicingSupporter.EditSecurityCheckpoint);
			AssertEquals("EditSecuritytMessage should be Empty", ZString.Empty, testJob.InvoicingSupporter.EditSecurityMessage);
			AssertEquals("EditSecurityLock should be False", ZBool.False, testJob.InvoicingSupporter.EditSecurityLock);
		}

		public void TestDefaultChargeGroup()
		{
			IJobInvoicingPlugIn testJob = Factory.New<CusUnderbond>();
			AssertEquals("DefaultChargeGroup should be Empty", ZString.Empty, testJob.InvoicingSupporter.DefaultChargeGroup);
		}

		#endregion

		#region IJobHeaderParent Members

		public void TestIJobHeaderParent_AllowInvoiceDeletion()
		{
			IJobHeaderParent cusUnderbond = Factory.New<CusUnderbond>();
			Assert(cusUnderbond.AllowInvoiceDeletion);
		}

		#endregion

		void AssertEqualByAirKeyFields(CusUnderbond underbond1, CusUnderbond underbond2, ZPropertyInfo info, IZType testValue)
		{
			var original = info.Value;
			info.Value = testValue;
			AssertEquals(false, underbond1.EqualsByAirKeyFields(underbond2));
			info.Value = original;
		}

		void HAWBUnderbond_CanDoOutturnChanged(object sender, EventArgs e)
		{
			canDoOutturnChangedCount++;
		}

		int canDoOutturnChangedCount;

		#region Implementation

		CusSCAOceanBill SCAOcean
		{
			get
			{
				if (fSCAOcean == null)
				{
					fSCAOcean = Factory.New<CusSCAOceanBill>();
				}
				return fSCAOcean;
			}
		}
		CusSCAOceanBill fSCAOcean;

		CusUnderbond fPartShipUnderbond;
		CusUnderbond PartShipUnderbond
		{
			get
			{
				if (fPartShipUnderbond == null)
				{
					fPartShipUnderbond = (CusUnderbond)((ICusUnderbondDependentCollectionParent)PartShip).Underbonds.AddNew();
				}
				return fPartShipUnderbond;
			}
		}

		CusPartShip fPartShip;
		CusPartShip PartShip
		{
			get
			{
				if (fPartShip == null)
				{
					fPartShip = CTOHAWB.PartShips.AddNew();
				}
				return fPartShip;
			}
		}

		CusUnderbond HAWBUnderbond
		{
			get
			{
				if (fHAWBUnderbond == null)
				{
					fHAWBUnderbond = (CusUnderbond)((ICusUnderbondDependentCollectionParent)HAWB).Underbonds.AddNew();
					fHAWBUnderbond.C4_DestinationPremiseID = "";
				}
				return fHAWBUnderbond;
			}
		}
		CusUnderbond fHAWBUnderbond;

		CusHAWB fHAWB;
		CusHAWB HAWB
		{
			get
			{
				if (fHAWB == null)
				{
					fHAWB = MAWB.ChildBills.AddNew();
				}
				return fHAWB;
			}
		}

		CusUnderbond fMAWBUnderbond;
		CusUnderbond MAWBUnderbond
		{
			get
			{
				if (fMAWBUnderbond == null)
				{
					fMAWBUnderbond = (CusUnderbond)((ICusUnderbondDependentCollectionParent)MAWB).Underbonds.AddNew();
				}
				return fMAWBUnderbond;
			}
		}

		CusMAWB fMAWB;
		CusMAWB MAWB
		{
			get
			{
				if (fMAWB == null)
				{
					fMAWB = Factory.New<CusMAWB>();
				}
				return fMAWB;
			}
		}

		CusUnderbond fCTOMAWBUnderbond;
		CusUnderbond CTOMAWBUnderbond
		{
			get
			{
				if (fCTOMAWBUnderbond == null)
				{
					ICusUnderbondDependentCollectionParent parent = CTOMAWB;
					fCTOMAWBUnderbond = Factory.New<CusUnderbond>();
					fCTOMAWBUnderbond.LinkedObject = parent;
				}
				return fCTOMAWBUnderbond;
			}
		}

		CusUnderbond fCTOHAWBUnderbond;
		CusUnderbond CTOHAWBUnderbond
		{
			get
			{
				if (fCTOHAWBUnderbond == null)
				{
					ICusUnderbondDependentCollectionParent parent = CTOHAWB;
					fCTOHAWBUnderbond = Factory.New<CusUnderbond>();
					fCTOHAWBUnderbond.LinkedObject = parent;
				}
				return fCTOHAWBUnderbond;
			}
		}

		CTOCusMAWB fCTOMAWB;
		CTOCusMAWB CTOMAWB
		{
			get
			{
				if (fCTOMAWB == null)
				{
					fCTOMAWB = Factory.New<CTOCusMAWB>();
				}
				return fCTOMAWB;
			}
		}

		CTOCusHAWB fCTOHAWB;
		CTOCusHAWB CTOHAWB
		{
			get
			{
				if (fCTOHAWB == null)
				{
					fCTOHAWB = CTOMAWB.ChildBills.AddNew();
				}
				return fCTOHAWB;
			}
		}

		#endregion

	}
}
