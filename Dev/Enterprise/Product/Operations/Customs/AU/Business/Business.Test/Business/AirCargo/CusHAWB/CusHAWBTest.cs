using System;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Interfaces;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CusHAWB))]
	sealed class CusHAWBTest : CusHAWBBaseAbstractTest
	{
		[ExpectNoExceptions]
		public void TestIWorkflowTriggerEventSourceDoesNotThrowIfNoConsol()
		{
			var mawb1 = Factory.New<CusMAWB>();
			IWorkflowTriggerEventSource hawb1 = mawb1.ChildBills.AddNew();
			object obj = hawb1.JobHeaderCompany;
			obj = hawb1.ParentWorkflowProviders;
			var consol = Factory.New<ForwardingConsol>();
			var mawb2 = Factory.New<CusMAWB>();
			mawb2.CM_JK = consol.PK;
			var shipment = Factory.New<ForwardingShipment>();
			var hawb2 = mawb2.ChildBills.AddNew();
			hawb2.CS_JS = shipment.PK;
			obj = ((IWorkflowTriggerEventSource)hawb2).JobHeaderCompany;
		}

		public void TestParentWorkflowProviders()
		{
			var mawb1 = Factory.New<CusMAWB>();
			IWorkflowTriggerEventSource hawb1 = mawb1.ChildBills.AddNew();
			var prov = hawb1.ParentWorkflowProviders;
			AssertNotNull(prov);
			AssertEquals(0, prov.Count);

			var consol = Factory.New<ForwardingConsol>();
			var mawb2 = Factory.New<CusMAWB>();
			mawb2.CM_JK = consol.PK;
			var shipment = Factory.New<ForwardingShipment>();
			var hawb2 = mawb2.ChildBills.AddNew();
			hawb2.CS_JS = shipment.PK;
			prov = ((IWorkflowTriggerEventSource)hawb2).ParentWorkflowProviders;
			AssertNotNull(prov);
			AssertEquals(1, prov.Count);
			AssertEquals(shipment, prov[0]);
		}

		public override void TestGetNewCusHAWBProcessTaskCollection()
		{
			var mawb = Factory.NewWithValidTestData<CusMAWB>();
			var hawb = mawb.ChildBills.AddNew();
			AssertType("WorkflowItems's type should be ProcessTaskCollection<CusHAWBProcessTask,CusHAWB>", typeof(ProcessTaskCollection<CusHAWBProcessTask, CusHAWB>), ((IWorkflowProvider)hawb).WorkflowItems);
		}

		public void TestStatusChangeFiresUniversalEvent_WithACSAQIS()
		{
			const string CARSTHeldMessage = @"UNH+000001+CUSRES:D:99B:UN'
BGM+34:::CARST+1GAG D03A D06F:1+8'
DTM+9:20051110003152681932:ZZZ'
DTM+132:20051110:102'
FTX+AHN+++CONSOLIDATED STATUS:CONDCLEAR'
FTX+AHN+++ACS/AQIS IMPEDIMENT DETAILS:CONDITIONAL RELEASE'
FTX+AHN+++ACS/AQIS IMPEDIMENT DETAILS:TEST'
TDT +20+006++6+QF::3'
LOC+12+AUSYD::6'
LOC+4+8553P::95'
NAD+MR+FGH939C::95'
NAD+UD+83003926181::95'
RFF+ABO:321/PRD1::1'
RFF+MWB:08145322174'
RFF+HWB:V0014102928'
UNT+34+000001'
";
			var mawb = Factory.NewWithValidTestData<CusMAWB>();
			var hawb = mawb.ChildBills.AddNew();
			var shipment = Factory.New<ForwardingShipment>();
			hawb.CS_JS = shipment.PK;
			shipment.JS_ShipmentType = Enterprise.Core.Constants.ShipmentTypes.HighVolumeLowValue;
			hawb.CS_IsHVLV = true;
			hawb.CS_CustomsStatus = "CCL";

			var message = Factory.New<CMRCARSTMessage>();
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageText = CARSTHeldMessage.Replace("\r\n", "");
			Factory.Save();
			hawb.Messages.AddFromDatabase(message.PK);
			Factory.Save();

			hawb.CS_CustomsStatus = "CLR";
			hawb.Factory.Save();

			var query = new ZQuery(StmALogSchema.SL_Parent, hawb.PK);
			query.AddToFilter(StmALogSchema.SL_Reference, SQLComparisonOperator.StartsWith, "|LOC");
			query.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.CustomsEntryStatus.Code);

			var logs = Factory.Load<StmALog>(query);
			AssertEquals("One universal event should be created", 1, logs.Length);
			AssertEquals("Free text should be properly set", "|LOC=AUBNE|RES=CONDITIONAL RELEASE TEST|SER=PCS|TYP=CLR", logs[0].SL_Reference);
		}

		public void TestStatusChangeFiresUniversalEvent_WithoutACSAQIS()
		{
			const string CARSTHeldMessage = @"UNH+000001+CUSRES:D:99B:UN'
BGM+34:::CARST+1GAG D03A D06F:1+8'
DTM+9:20051110003152681932:ZZZ'
DTM+132:20051110:102'
FTX+AHN+++CONSOLIDATED STATUS:CONDCLEAR'
TDT +20+006++6+QF::3'
LOC+12+AUSYD::6'
LOC+4+8553P::95'
NAD+MR+FGH939C::95'
NAD+UD+83003926181::95'
RFF+ABO:321/PRD1::1'
RFF+MWB:08145322174'
RFF+HWB:V0014102928'
UNT+34+000001'
";
			var mawb = Factory.NewWithValidTestData<CusMAWB>();
			var hawb = mawb.ChildBills.AddNew();
			var shipment = Factory.New<ForwardingShipment>();
			hawb.CS_JS = shipment.PK;
			shipment.JS_ShipmentType = Enterprise.Core.Constants.ShipmentTypes.HighVolumeLowValue;
			var org = Factory.NewWithValidTestData<OrgHeader>();
			shipment.HouseBillIssuingPartyDocumentaryAddress.OrganisationPK = org.PK;
			hawb.CS_IsHVLV = true;
			hawb.CS_CustomsStatus = "CCL";

			var message = Factory.New<CMRCARSTMessage>();
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageText = CARSTHeldMessage.Replace("\r\n", "");
			Factory.Save();
			hawb.Messages.AddFromDatabase(message.PK);
			Factory.Save();

			hawb.CS_CustomsStatus = "CLR";
			hawb.Factory.Save();

			var query = new ZQuery(StmALogSchema.SL_Parent, hawb.PK);
			query.AddToFilter(StmALogSchema.SL_Reference, SQLComparisonOperator.StartsWith, "|LOC");
			query.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.CustomsEntryStatus.Code);

			var logs = Factory.Load<StmALog>(query);
			AssertEquals("One universal event should be created", 1, logs.Length);
			AssertEquals("Free text should be properly set", "|LOC=AUBNE|SER=PCS|TYP=CLR", logs[0].SL_Reference);
		}

		public void TestStatusChangeFiresUniversalEvent_StatusNotRelevant()
		{
			var mawb = Factory.NewWithValidTestData<CusMAWB>();
			var hawb = mawb.ChildBills.AddNew();
			var shipment = Factory.New<ForwardingShipment>();
			hawb.CS_JS = shipment.PK;
			shipment.JS_ShipmentType = Enterprise.Core.Constants.ShipmentTypes.HighVolumeLowValue;
			hawb.CS_IsHVLV = true;
			hawb.CS_CustomsStatus = "HLD";
			hawb.Factory.Save();

			hawb.CS_CustomsStatus = "ABC";
			hawb.Factory.Save();

			var query = new ZQuery(StmALogSchema.SL_Parent, hawb.PK);
			query.AddToFilter(StmALogSchema.SL_Reference, SQLComparisonOperator.StartsWith, "|LOC");
			query.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.CustomsEntryStatus.Code);
			var logs = Factory.Load<StmALog>(query);
			AssertEquals("0 log published", 0, logs.Length);
		}

		public void TestStatusChangeFiresUniversalEvent_NoHVLV()
		{
			var mawb = Factory.NewWithValidTestData<CusMAWB>();
			var hawb = mawb.ChildBills.AddNew();
			var shipment = Factory.New<ForwardingShipment>();
			hawb.CS_JS = shipment.PK;
			shipment.JS_ShipmentType = Enterprise.Core.Constants.ShipmentTypes.HighVolumeLowValue;
			hawb.CS_IsHVLV = false;
			hawb.CS_CustomsStatus = "HLD";
			hawb.Factory.Save();

			hawb.CS_CustomsStatus = "CLR";
			hawb.Factory.Save();

			var query = new ZQuery(StmALogSchema.SL_Parent, hawb.PK);
			query.AddToFilter(StmALogSchema.SL_Reference, SQLComparisonOperator.StartsWith, "|LOC");
			query.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.CustomsEntryStatus.Code);
			var logs = Factory.Load<StmALog>(query);
			AssertEquals("0 log published", 0, logs.Length);
		}

		public void TestStatusChangeFiresUniversalEvent_NoNewStatus()
		{
			var mawb = Factory.NewWithValidTestData<CusMAWB>();
			var hawb = mawb.ChildBills.AddNew();
			var shipment = Factory.New<ForwardingShipment>();
			hawb.CS_JS = shipment.PK;
			shipment.JS_ShipmentType = Enterprise.Core.Constants.ShipmentTypes.HighVolumeLowValue;
			hawb.CS_IsHVLV = true;
			hawb.CS_CustomsStatus = "HLD";
			hawb.Factory.Save();

			var query = new ZQuery(StmALogSchema.SL_Parent, hawb.PK);
			query.AddToFilter(StmALogSchema.SL_Reference, SQLComparisonOperator.StartsWith, "|LOC");
			query.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.CustomsEntryStatus.Code);
			var logs = Factory.Load<StmALog>(query);
			AssertEquals("0 log published", 0, logs.Length);
		}

		public void TestStatusChangeFiresUniversalEvent_EventPublished()
		{
			TestCaseHelper.ClearTable(EDIMessageSchema.Constants.TableName);

			var mawb = Factory.NewWithValidTestData<CusMAWB>();
			var hawb = mawb.ChildBills.AddNew();
			var shipment = Factory.New<ForwardingShipment>();
			hawb.CS_JS = shipment.PK;
			shipment.JS_ShipmentType = Enterprise.Core.Constants.ShipmentTypes.HighVolumeLowValue;

			hawb.CS_HAWB = "HB1";
			hawb.CS_IsHVLV = true;
			hawb.CS_CustomsStatus = "CCL";
			hawb.Factory.Save();

			hawb.CS_CustomsStatus = "CLR";
			hawb.Factory.Save();

			var query = new ZQuery(StmALogSchema.SL_Parent, hawb.PK);
			query.AddToFilter(StmALogSchema.SL_Reference, SQLComparisonOperator.StartsWith, "|LOC");
			query.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.CustomsEntryStatus.Code);

			var logs = Factory.Load<StmALog>(query);
			AssertEquals("One universal event should be created", 1, logs.Length);
			AssertEquals("Free text should be properly set", "|LOC=AUBNE|SER=PCS|TYP=CLR", logs[0].SL_Reference);

			var msgQuery = new ZQuery(EDIMessageSchema.EM_ApplicationCode, EDIMessage.ApplicationCodes.UniversalDataMessaging);
			msgQuery.AddToFilter(EDIMessageSchema.EM_MessageType, EDIMessageTypeList.Codes.XDC);
			msgQuery.AddToFilter(EDIMessageSchema.EM_MessageSubType, EDIMessageSubTypeList.Codes.XmlUniversalEvent);

			var messagesCount = Factory.GetDatabaseCount(typeof(EDIMessage), msgQuery);
			AssertEquals("Universal event should not be published for a performance optimization, the upper message will be processed by CusHAWBCustomsStatusLogSubscriber.", 0, messagesCount);
		}

		public void TestIScanHouseBillProvider()
		{
			var hawb = Factory.New<CusHAWB>();
			var shipment = Factory.New<ForwardingShipment>();
			hawb.CS_JS = shipment.PK;
			shipment.JS_ShipmentType = Enterprise.Core.Constants.ShipmentTypes.StandardHouse;
			hawb.CS_HAWB = "HB1";
			hawb.CS_ConsigneeName = "CONSIGNEE NAME";
			hawb.CS_CustomsStatus = "CLR";
			hawb.CS_GoodsDescription = "GOODS DESCRIPTION";
			hawb.CS_PiecesManifested = 50;

			var iScan = (IScanHouseBillProvider)hawb;
			AssertEquals(Enterprise.Core.Constants.ShipmentTypes.StandardHouse, iScan.ShipmentType);
			AssertEquals("HB1", iScan.HouseBill);
			AssertEquals("CONSIGNEE NAME", iScan.ConsigneeName);
			var manifestInfo = iScan.GetManifestInformation(null);
			AssertEquals("CLR", manifestInfo.CustomsStatus);
			AssertEquals("GOODS DESCRIPTION", manifestInfo.GoodsDescription);
			AssertEquals(50, manifestInfo.Quantity);
			AssertEquals(CusHAWBSchema.Constants.Prefix, manifestInfo.TablePrefix);
			AssertEquals(hawb.PK, manifestInfo.PK);
		}

		public void TestIsCargoStatusClear()
		{
			var hawb = Factory.New<CusHAWB>();
			AssertEquals(false, hawb.IsCargoStatusClear);
			hawb.CS_CustomsStatus = CMRConsolidatedCargoStatuses.Codes.ClearCargoIsFreeOfAnyImpedimentsAndMayBeReleased;
			AssertEquals(true, hawb.IsCargoStatusClear);
			hawb.CS_CustomsStatus = CMRConsolidatedCargoStatuses.Codes.ClearhrmCargoIsClearButIsIdentifiedAsHighRiskMovement;
			AssertEquals(true, hawb.IsCargoStatusClear);
			hawb.CS_CustomsStatus = CMRConsolidatedCargoStatuses.Codes.CondclearCargoCanBeReleasedIntoHomeConsumptionSubjectToConditionSTheseConditionsAreProvidedInSupplementaryInformation;
			AssertEquals(false, hawb.IsCargoStatusClear);
			hawb.CS_CustomsStatus = CMRConsolidatedCargoStatuses.Codes.HeldCargoIsHeldUnderCustomsControl;
			AssertEquals(false, hawb.IsCargoStatusClear);
		}

		public void TestLogReadyForLocalDeliveryIfIsCargoStatusClear()
		{
			var hawb = Factory.New<CusHAWB>();
			hawb.CS_IsHeldAtOutturn = true;
			AssertNull("No ReadyForLocalDelivery Event", hawb.Logs.MostRecentLogByEventTime(Events.ReadyForLocalDelivery));
			AssertEquals("IsCleared", false, CMRConsolidatedCargoStatuses.IsCargoStatusClearFor(hawb.CS_CustomsStatus));
			hawb.LogReadyForLocalDeliveryIfIsCargoStatusClear();
			AssertNull("No ReadyForLocalDelivery Event was added as hawb was not cleared", hawb.Logs.MostRecentLogByEventTime(Events.ReadyForLocalDelivery));
			Assert("IsHeldAtOutturn not reset", hawb.CS_IsHeldAtOutturn);

			hawb.CS_CustomsStatus = CMRConsolidatedCargoStatuses.Codes.ClearCargoIsFreeOfAnyImpedimentsAndMayBeReleased;
			AssertEquals("IsCleared", true, CMRConsolidatedCargoStatuses.IsCargoStatusClearFor(hawb.CS_CustomsStatus));
			hawb.LogReadyForLocalDeliveryIfIsCargoStatusClear();
			AssertNotNull("ReadyForLocalDelivery Event was added as hawb was cleared", hawb.Logs.MostRecentLogByEventTime(Events.ReadyForLocalDelivery));
			Assert("IsHeldAtOutturn is reset", !hawb.CS_IsHeldAtOutturn);

			hawb.CS_IsHeldAtOutturn = true;
			hawb.LogReadyForLocalDeliveryIfIsCargoStatusClear();
			var query = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.ReadyForLocalDeliveryCode);
			query.AddToFilter(StmALogSchema.SL_Parent, hawb.PK);
			AssertEquals("No new ReadyForLocalDelivery Event needs to be added", 1, Factory.Load<StmALog>(query).Length);
			Assert("IsHeldAtOutturn is reset", !hawb.CS_IsHeldAtOutturn);
		}

		public void TestHVLVAndRemailReadOnlyAttribute()
		{
			var hawb = Factory.New<CusHAWB>();
			foreach (var msgStatus in new[] { CMRBaseStatuses.Codes.NotSent, CMRBaseStatuses.Codes.OriginalRejected, CMRBaseStatuses.Codes.WithdrawalAccepted, string.Empty })
			{
				hawb.CS_MsgStatus = msgStatus;
				Assert(!hawb.CS_IsRemailReporterInfo.ReadOnly);
				Assert(!hawb.CS_IsSpecialReporterInfo.ReadOnly);
			}
			hawb.CS_MsgStatus = CMRBaseStatuses.Codes.OriginalAccepted;
			Assert(hawb.CS_IsRemailReporterInfo.ReadOnly);
			Assert(hawb.CS_IsSpecialReporterInfo.ReadOnly);
		}

		public void TestHumanReadableName()
		{
			var cusMAWB = Factory.New<CusMAWB>();
			var cusHAWB = cusMAWB.ChildBills.AddNew();
			AssertEquals("Air Cargo House", cusHAWB.HumanReadableName);
			cusHAWB.CS_HAWB = "HB23423";
			AssertEquals("Air Cargo House (HAWB: HB23423)", cusHAWB.HumanReadableName);
			cusHAWB.CS_MasterHouseBill = "X1234";
			AssertEquals("Air Cargo House (HAWB: HB23423 MHB: X1234)", cusHAWB.HumanReadableName);
			cusHAWB.CS_HAWB = "";
			AssertEquals("Air Cargo House (MHB: X1234)", cusHAWB.HumanReadableName);
		}

		public void TestRemainingDCLOutturnQuantity()
		{
			var mawb = Factory.New<CusMAWB>();
			var hawb = mawb.ChildBills.AddNew();
			hawb.CS_PiecesManifested = 50;
			AssertEquals("No underbonds returns manifested", 50, hawb.RemainingDCLOutturnQuantity);
			hawb.ResetRemainingDCLOutturnQuantityCacheForTesting();

			var nonDCLUnderbond = mawb.Underbonds.AddNew();
			nonDCLUnderbond.C4_MovementReason = CMRUnderbondRequestCodes.Codes.OtherMovement;
			var nonDCLUnderbondOutturn = nonDCLUnderbond.Outturns.AddNew();
			nonDCLUnderbondOutturn.C5_PackagesOutturned = 20;
			nonDCLUnderbondOutturn.C5_LastMessageDate = ZDateTime.Now;
			nonDCLUnderbondOutturn.C5_OutturnResultType = CMROutturnResultType.Codes.ShortLanded;
			nonDCLUnderbond.OutturnStatus.Code = CMRBaseStatuses.Codes.OriginalAccepted;
			AssertEquals("Non-DCL underbonds are ignored", 50, hawb.RemainingDCLOutturnQuantity);
			hawb.ResetRemainingDCLOutturnQuantityCacheForTesting();

			var dclUnderbond1 = mawb.Underbonds.AddNew();
			dclUnderbond1.C4_MovementReason = CMRUnderbondRequestCodes.Codes.UnpackLclAtDestination;
			AssertEquals("No outturns returns manifested", 50, hawb.RemainingDCLOutturnQuantity);
			hawb.ResetRemainingDCLOutturnQuantityCacheForTesting();

			var dclUnderbond1Outturn = dclUnderbond1.Outturns.AddNew();
			dclUnderbond1Outturn.C5_PackagesOutturned = 20;
			dclUnderbond1Outturn.C5_LastMessageDate = ZDateTime.Now;
			dclUnderbond1Outturn.C5_OutturnResultType = CMROutturnResultType.Codes.ShortLanded;
			dclUnderbond1Outturn.C5_ParentID = hawb.PK;
			dclUnderbond1.OutturnStatus.Code = CMRBaseStatuses.Codes.OriginalAccepted;
			AssertEquals("Short landed on first underbond", 30, hawb.RemainingDCLOutturnQuantity);
			hawb.ResetRemainingDCLOutturnQuantityCacheForTesting();

			dclUnderbond1.OutturnStatus.Code = CMRBaseStatuses.Codes.WithdrawalAccepted;
			AssertEquals("Withdrawn returns manifested", 50, hawb.RemainingDCLOutturnQuantity);
			dclUnderbond1.OutturnStatus.Code = CMRBaseStatuses.Codes.OriginalAccepted;
			hawb.ResetRemainingDCLOutturnQuantityCacheForTesting();

			var dclUnderbond2 = mawb.Underbonds.AddNew();
			dclUnderbond2.C4_MovementReason = CMRUnderbondRequestCodes.Codes.UnpackLclAtDestination;
			var dclUnderbond2Outturn = dclUnderbond2.Outturns.AddNew();
			dclUnderbond2Outturn.C5_PackagesOutturned = 5;
			dclUnderbond2Outturn.C5_LastMessageDate = ZDateTime.Now;
			dclUnderbond2Outturn.C5_OutturnResultType = CMROutturnResultType.Codes.ShortLanded;
			dclUnderbond2Outturn.C5_ParentID = hawb.PK;
			dclUnderbond2.OutturnStatus.Code = CMRBaseStatuses.Codes.OriginalAccepted;
			AssertEquals("Short landed on second underbond", 25, hawb.RemainingDCLOutturnQuantity);
			hawb.ResetRemainingDCLOutturnQuantityCacheForTesting();

			dclUnderbond2Outturn.C5_PackagesOutturned = 100;
			AssertEquals("Short over-commitment returns 1", 1, hawb.RemainingDCLOutturnQuantity);
			hawb.ResetRemainingDCLOutturnQuantityCacheForTesting();
			dclUnderbond2Outturn.C5_OutturnResultType = CMROutturnResultType.Codes.NilDiscrepancy;
			AssertEquals("Nill descrepancy returns completed", 0, hawb.RemainingDCLOutturnQuantity);
			hawb.ResetRemainingDCLOutturnQuantityCacheForTesting();
			dclUnderbond2Outturn.C5_OutturnResultType = CMROutturnResultType.Codes.SurplusPackages;
			AssertEquals("Surplus returns completed", 0, hawb.RemainingDCLOutturnQuantity);
			hawb.ResetRemainingDCLOutturnQuantityCacheForTesting();
		}

		public void TestJobConShipLinkDeleteCheckAndCannotDeleteExceptionNotCaught()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;

			var shipment = consol.Shipments.AddNew();

			var mawb = Factory.New<CusMAWB>();
			mawb.CM_JK = consol.PK;

			var hawb = mawb.ChildBills.AddNew();
			hawb.CS_JS = shipment.PK;

			new CusHAWBAIRCRMessageManager(hawb).GenerateOriginalMessages(hawb);
			AssertEquals(1, hawb.Messages.Count);
			hawb.CS_MsgStatus = CMRBaseStatuses.Codes.OriginalRejected;

			AssertNoExceptionThrown(() => consol.Shipments.Remove(shipment));
			AssertNoExceptionThrown(() => hawb.Delete());
		}

		public void TestHasActiveDCLOutturnLine()
		{
			var mawb = Factory.New<CusMAWB>();
			var hawb = mawb.ChildBills.AddNew();
			AssertEquals(false, hawb.HasActiveDCLOutturnLine);

			var underbond1 = mawb.Underbonds.AddNew();
			underbond1.C4_MovementReason = CMRUnderbondRequestCodes.Codes.OtherMovement;
			underbond1.Outturns.AddNew();
			AssertEquals(false, hawb.HasActiveDCLOutturnLine);

			var underbond2 = mawb.Underbonds.AddNew();
			underbond2.C4_MovementReason = CMRUnderbondRequestCodes.Codes.UnpackLclAtDestination;
			var outturn2 = underbond2.Outturns.AddNew();
			outturn2.C5_ParentID = hawb.PK;
			AssertEquals(true, hawb.HasActiveDCLOutturnLine);
		}

		public void TestLoadDetachedCusHAWB()
		{
			var shipment = Factory.New<ForwardingShipment>();

			var cusHAWB = Factory.New<CusHAWB>();
			cusHAWB.CS_JS = shipment.PK;
			cusHAWB.CS_ApplicationCode = "CMR";

			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			var cusHAWBs = CusHAWB.Load(factory2, new ZGuid[] { shipment.PK });
			AssertEquals(1, cusHAWBs.Length);
			AssertEquals(cusHAWB.PK, cusHAWBs[0].PK);
		}

		public void TestCS_CustomsStatus_DoesntLookAtCS_IsResponsePending_WhenSetToCMR()
		{
			HAWB.MAWB.CM_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;

			HAWB.CS_CustomsStatus = CMRBaseStatuses.Codes.AwaitingResponseToAmendment;
			HAWB.CS_IsResponsePending = true;
			AssertEquals("Should not revert to WAIT for CMR", CMRBaseStatuses.Codes.AwaitingResponseToAmendment, HAWB.CS_CustomsStatus);
		}

		public void TestIMessageManageableBizObj()
		{
			Customs.Business.IMessageManageableBizObj houseBill = HAWB;

			AssertEquals("MessageManager", typeof(CusHAWBMessageManager), houseBill.GetMessageManagerForAmendmentDetection().GetType());
		}

		public void TestEntryStatusChangedLogged()
		{
			var mAWB = Factory.New<CusMAWB>();
			CusHAWB hAWB = mAWB.ChildBills.AddNew();
			Factory.Save();

			hAWB.CS_CustomsStatus = Events.CustomsCommenced.Code;
			Factory.Save();

			ZQuery filter = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsEntryStatus.Code);
			filter.AddToFilter(StmALogSchema.SL_Parent, hAWB.PK);
			AssertEquals("Event Created", 1, Factory.GetDatabaseCount(typeof(StmALog), filter));
		}

		public void TestEntryStatusChangeLoggedInCorrectBranch()
		{
			var melbourneBranch = Factory.New<GlbBranch>();
			melbourneBranch.GB_GC = GlbCompany.CurrentCompany.PK;
			melbourneBranch.GB_Code = "TML";
			melbourneBranch.GB_RL_NKHomePort = "AUMEL";
			var perthBranch = Factory.New<GlbBranch>();
			perthBranch.GB_GC = GlbCompany.CurrentCompany.PK;
			perthBranch.GB_Code = "TPR";
			perthBranch.GB_RL_NKHomePort = "AUPER";
			Factory.Save();

			using (DisposableEnvironment.ForBranch(melbourneBranch.PK.ToGuid()))
			{
				var mawb = Factory.New<CusMAWB>();
				mawb.CM_MAWB = "61877774830";
				var hawb = mawb.ChildBills.AddNew();
				hawb.CS_MessageReference = "A26536447";
				hawb.CS_ClearanceDate = ZDateTime.Empty;
				Factory.Save();

				var melbourneBranchTime = ZDateTime.Now;
				using (DisposableEnvironment.ForBranch(perthBranch.PK.ToGuid()))
				{
					AssertLessThan("No longer in East Coast timezone", ZDateTime.Now, melbourneBranchTime);
					hawb.CS_CustomsStatus = CMRConsolidatedCargoStatuses.Codes.ClearCargoIsFreeOfAnyImpedimentsAndMayBeReleased;
					Factory.Save();
				}

				var log = hawb.Logs.MostRecentLogByEventTime(Events.CustomsEntryStatus);
				AssertEquals("CLR", log.SL_Reference);
				AssertGreaterThanOrEqualTo("log should have Melbourne timestamp even though logged in Perth timezone", log.SL_EventTime, melbourneBranchTime);
			}
		}

		public void TestICusMAWBProvider()
		{
			AssertEquals("ICusMAWBProvider", MAWB, ((ICusMAWBProvider)HAWB).MAWB);
		}

		public void TestHeading()
		{
			var hAWB = Factory.New<CusHAWB>();
			AssertEquals("Heading", hAWB.UnderbondHumanReadableName, ((IDetailsTabPageHeadingProvider)hAWB).Heading);
		}

		public void TestSetDefaultValuesFromHAWB()
		{
			var mAWB = Factory.New<CusMAWB>();
			mAWB.CM_FlightNo = "QF123";
			CusHAWB hAWB = mAWB.ChildBills.AddNew();
			hAWB.CS_PiecesManifested = 125;
			CusUnderbond testUnderbond = hAWB.Underbonds.AddNew();
			testUnderbond.C4_ParentID = hAWB.PK;
			testUnderbond.C4_ParentTableCode = "CS";
			testUnderbond.SetDefaultValuesFromParent();
			AssertEquals("QF123", testUnderbond.C4_FlightNo);
			AssertEquals(125u, testUnderbond.C4_PiecesManifested);
		}

		public void TestIsMasterHouseNumDifferent()
		{
			HAWB.CS_MasterHouseBill = "";
			HAWB.MAWB.CM_MasterHouseBill = "ABC";
			Factory.Save();

			HAWB.CS_MasterHouseBill = "ABC";
			AssertEquals("Master House is not different", false, HAWB.IsMasterHouseBillDifferent);
			Factory.Save();

			HAWB.CS_MasterHouseBill = "";
			AssertEquals("Master House is not different", false, HAWB.IsMasterHouseBillDifferent);
			Factory.Save();

			HAWB.CS_MasterHouseBill = "DEF";
			AssertEquals("Master House is different", true, HAWB.IsMasterHouseBillDifferent);
		}

		public void TestSetMessageReferenceOnSavingWithShipment()
		{
			HAWB.CS_MessageReference = "";
			AssertEquals("Reference is empty", true, HAWB.CS_MessageReference.IsEmpty);
			AssertEquals("Not in database yet", false, HAWB.IsInDatabase);

			CommonShipment shipment = CommonShipment.New(Factory);
			HAWB.CS_JS = shipment.PK;

			Factory.Save();
			AssertEquals("Reference is not empty", false, HAWB.CS_MessageReference.IsEmpty);
			AssertEquals("Using shipment reference number", shipment.JS_UniqueConsignRef, HAWB.CS_MessageReference);
		}

		[ExpectException(typeof(AirCargoException))]
		public void TestCheckPrealertConstraintIfPrealerted()
		{
			HAWB.CS_IsPrealerted = true;
			HAWB.CheckPreAlertConstraint();
		}

		[ExpectException(typeof(AirCargoException))]
		public void TestCheckPrealertConstraintIfPrealertHeld()
		{
			HAWB.CS_IsPrealertHeldByUser = true;
			HAWB.CheckPreAlertConstraint();
		}

		[ExpectNoExceptions()]
		public void TestCheckPrealertConstraint()
		{
			HAWB.CS_IsPrealerted = false;
			HAWB.CS_IsPrealertHeldByUser = false;
			HAWB.CheckPreAlertConstraint();
		}

		public new void TestOutturnableLines()
		{
			AssertEquals(1, ((ICusUnderbondDependentCollectionParent)HAWB).OutturnableLines.Length);
			AssertEquals(HAWB, ((ICusUnderbondDependentCollectionParent)HAWB).OutturnableLines[0]);
		}

		public void TestRelatedOrgMatchApprovalsDeletedOnDelete()
		{
			OrgMatchApproval.Loader loader = new OrgMatchApproval.Loader(Factory);
			OrgMatchApproval matchApproval = loader.LoadOrCreate(HAWB.PK, OrgMatchApprovalType.AirCargoConsignee);
			HAWB.Delete();
			AssertEquals("Deleting the CusHAWB should cascade delete the related OrgMatchApproval object", true, matchApproval.IsDeleted);
		}

		public void TestOnLoaded()
		{
			CusHAWB cusHAWB = Factory.New<CusHAWB>();
			cusHAWB.CS_CM = Factory.New<CusMAWB>().PK;
			Factory.Save();

			BusinessObjectFactory clearFactory = new BusinessObjectFactory();
			cusHAWB = clearFactory.Load<CusHAWB>(cusHAWB.PK);
			AssertEquals(false, cusHAWB.HasChanges);
		}

		public void TestTablePrefix()
		{
			AssertEquals("Constant has recently been changed, TablePrefix needs to be updated", CusHAWBSchema.Constants.Prefix, HAWB.TablePrefix);
		}

		public void TestCurrentQueue()
		{
			AssertEquals(ProcessQueueParentHelper.CurrentQueue.PK, HAWB.CurrentQueue.PK);
			AssertEquals("Default type should be base ProcessQueue", typeof(ProcessQueue), HAWB.CurrentQueue.GetType());
			Assert("Has to be a registered editable child object", HAWB.IsRegisteredEditableChildObject(HAWB.CurrentQueue));
		}

		public void TestActiveProcessQueueForBinding()
		{
			AssertEquals("The collection should have 1 child", 1, HAWB.ActiveProcessQueueForBinding.Count);
			AssertEquals("Child should be the CurrentQueue", HAWB.CurrentQueue, HAWB.ActiveProcessQueueForBinding[0].ProcessQueue);
		}

		public void TestProcessQueueDeletedOnHouseBillDelete()
		{
			ProcessQueue processQueue = HAWB.CurrentQueue;
			Assert("Record should not be deleted", !processQueue.IsDeleted);
			HAWB.Delete();
			Assert("Record should be deleted", processQueue.IsDeleted);
		}

		public void TestAttachingShipmentWithNoDeclaration()
		{
			CommonShipment shipment = CommonShipment.New(Factory);
			BusinessObject declaration = (BusinessObject)Factory.New<Integration.Customs.IBaseJobDeclaration>();
			HAWB.CS_JE_CustomsFormalEntry = declaration.PK;
			Factory.Save();
			AssertEquals("PreCondition: CS_JS should be empty", ZGuid.Empty, HAWB.CS_JS);
			AssertEquals("PreCondition: CS_JE_CustomsFormalEntry should be same as Declaration.PK", declaration.PK, HAWB.CS_JE_CustomsFormalEntry);
			AssertNull("PreCondition: HouseBill.Shipment should be null", HAWB.Shipment);
			HAWB.CS_JS = shipment.PK;
			AssertEquals("CS_JS should be same as Shipment.PK", shipment.PK, HAWB.CS_JS);
			AssertNotNull("HouseBill.Shipment should not be null", HAWB.Shipment);
			AssertEquals("CS_JE_CustomsFormalEntry should be empty as Shipment is not linked to a Declaration", ZGuid.Empty, HAWB.CS_JE_CustomsFormalEntry);

			((IBusinessObjectInternals)HAWB).IsCopying = true;
			try
			{
				HAWB.CS_JE_CustomsFormalEntry = declaration.PK;
				HAWB.CS_JS = ZGuid.Empty;
				AssertEquals("PreCondition: CS_JS should be empty", ZGuid.Empty, HAWB.CS_JS);
				AssertEquals("PreCondition: CS_JE_CustomsFormalEntry should be same as Declaration.PK", declaration.PK, HAWB.CS_JE_CustomsFormalEntry);
				AssertNull("PreCondition: HouseBill.Shipment should be null", HAWB.Shipment);
				HAWB.CS_JS = shipment.PK;
				AssertEquals("CS_JS should be same as Shipment.PK", shipment.PK, HAWB.CS_JS);
				AssertNotNull("HouseBill.Shipment should not be null", HAWB.Shipment);
				AssertEquals("CS_JE_CustomsFormalEntry should remain the same as Declaration.PK", declaration.PK, HAWB.CS_JE_CustomsFormalEntry);
			}
			finally
			{
				((IBusinessObjectInternals)HAWB).IsCopying = false;
			}
		}

		[TestDateIncremental(0, 0, 1, 0)]
		public void TestAttachingShipmentWithDeclarations()
		{
			var shipment = CommonShipment.New(Factory);
			var declaration1 = Factory.New<Integration.Customs.IBaseJobDeclaration>();
			declaration1.JE_JS = shipment.PK;
			var branch = Factory.NewWithValidTestData<GlbCompany>().Branches.AddNew();
			var declaration2 = Factory.New<Integration.Customs.IBaseJobDeclaration>();
			declaration2.JE_GB = branch.PK;
			declaration2.JE_JS = shipment.PK;
			Factory.Save();
			AssertEquals("PreCondition: CS_JS should be empty", ZGuid.Empty, HAWB.CS_JS);
			AssertEquals("PreCondition: CS_JE_CustomsFormalEntry should be empty", ZGuid.Empty, HAWB.CS_JE_CustomsFormalEntry);
			AssertNull("PreCondition: HouseBill.Shipment should be null", HAWB.Shipment);
			StmALog declaration1AddEvent = ((BusinessObject)declaration1).GetLogs().MostRecentLogByEventTime(Events.AddedARecordToTheSystem);
			StmALog declaration2AddEvent = ((BusinessObject)declaration2).GetLogs().MostRecentLogByEventTime(Events.AddedARecordToTheSystem);
			Assert("PreCondition: Declaration1 Add Event Time should be less than Declaration2 Add Event Time", declaration2AddEvent.SL_EventTime > declaration1AddEvent.SL_EventTime);
			HAWB.CS_JS = shipment.PK;
			AssertEquals("CS_JS should be same as Shipment.PK", shipment.PK, HAWB.CS_JS);
			AssertNotNull("HouseBill.Shipment should not be null", HAWB.Shipment);
			AssertEquals("CS_JE_CustomsFormalEntry should be same as Declaration2.PK", declaration2.PK, HAWB.CS_JE_CustomsFormalEntry);
		}

		public void TestAttachingDeclarationWithNoShipment()
		{
			CommonShipment shipment = CommonShipment.New(Factory);
			BusinessObject declaration = (BusinessObject)Factory.New<Integration.Customs.IBaseJobDeclaration>();
			HAWB.CS_JS = shipment.PK;
			Factory.Save();
			AssertEquals("PreCondition: CS_JE_CustomsFormalEntry should be empty", ZGuid.Empty, HAWB.CS_JE_CustomsFormalEntry);
			AssertNull("PreCondition: HouseBill.Declaration should be null", HAWB.Declaration);
			AssertEquals("PreCondition: CS_JS should be same as Shipment1.PK", shipment.PK, HAWB.CS_JS);
			HAWB.CS_JE_CustomsFormalEntry = declaration.PK;
			AssertEquals("CS_JE_CustomsFormalEntry should be same as Declaration.PK", declaration.PK, HAWB.CS_JE_CustomsFormalEntry);
			AssertNotNull("HouseBill.Declaration should not be null", HAWB.Declaration);
			AssertEquals("CS_JS should be empty as Declaration is not linked to a Shipment", ZGuid.Empty, HAWB.CS_JS);

			((IBusinessObjectInternals)HAWB).IsCopying = true;
			try
			{
				HAWB.CS_JE_CustomsFormalEntry = ZGuid.Empty;
				HAWB.CS_JS = shipment.PK;
				AssertEquals("PreCondition: CS_JE_CustomsFormalEntry should be empty", ZGuid.Empty, HAWB.CS_JE_CustomsFormalEntry);
				AssertNull("PreCondition: HouseBill.Declaration should be null", HAWB.Declaration);
				AssertEquals("PreCondition: CS_JS should be same as Shipment1.PK", shipment.PK, HAWB.CS_JS);
				HAWB.CS_JE_CustomsFormalEntry = declaration.PK;
				AssertEquals("CS_JE_CustomsFormalEntry should be same as Declaration.PK", declaration.PK, HAWB.CS_JE_CustomsFormalEntry);
				AssertNotNull("HouseBill.Declaration should not be null", HAWB.Declaration);
				AssertEquals("CS_JS should remain the same as Shipment.PK", shipment.PK, HAWB.CS_JS);
			}
			finally
			{
				((IBusinessObjectInternals)HAWB).IsCopying = false;
			}
		}

		public void TestAttachingDeclarationWithShipment()
		{
			CommonShipment shipment1 = CommonShipment.New(Factory);
			CommonShipment shipment2 = CommonShipment.New(Factory);
			BusinessObject declaration = (BusinessObject)Factory.New<Integration.Customs.IBaseJobDeclaration>();
			declaration[JobDeclarationSchema.JE_JS.Name] = shipment2.PK;
			HAWB.CS_JS = shipment1.PK;
			Factory.Save();
			AssertEquals("PreCondition: CS_JE_CustomsFormalEntry should be empty", ZGuid.Empty, HAWB.CS_JE_CustomsFormalEntry);
			AssertNull("PreCondition: HouseBill.Declaration should be null", HAWB.Declaration);
			AssertEquals("PreCondition: CS_JS should be same as Shipment1.PK", shipment1.PK, HAWB.CS_JS);
			HAWB.CS_JE_CustomsFormalEntry = declaration.PK;
			AssertEquals("CS_JE_CustomsFormalEntry should be same as Declaration.PK", declaration.PK, HAWB.CS_JE_CustomsFormalEntry);
			AssertNotNull("HouseBill.Declaration should not be null", HAWB.Declaration);
			AssertEquals("CS_JS should same as Shipment2.PK", shipment2.PK, HAWB.CS_JS);

			((IBusinessObjectInternals)HAWB).IsCopying = true;
			try
			{
				HAWB.CS_JE_CustomsFormalEntry = ZGuid.Empty;
				HAWB.CS_JS = shipment1.PK;
				AssertEquals("PreCondition: CS_JE_CustomsFormalEntry should be empty", ZGuid.Empty, HAWB.CS_JE_CustomsFormalEntry);
				AssertNull("PreCondition: HouseBill.Declaration should be null", HAWB.Declaration);
				AssertEquals("PreCondition: CS_JS should be same as Shipment1.PK", shipment1.PK, HAWB.CS_JS);
				HAWB.CS_JE_CustomsFormalEntry = declaration.PK;
				AssertEquals("CS_JE_CustomsFormalEntry should be same as Declaration.PK", declaration.PK, HAWB.CS_JE_CustomsFormalEntry);
				AssertNotNull("HouseBill.Declaration should not be null", HAWB.Declaration);
				AssertEquals("CS_JS should remain the same as Shipment.PK", shipment1.PK, HAWB.CS_JS);
			}
			finally
			{
				((IBusinessObjectInternals)HAWB).IsCopying = false;
			}
		}

		public void TestDocManagerInfo()
		{
			DocManagerInfo docManagerInfo = ((IDocManagerSupport)HAWB).DocManagerInfo;
			AssertEquals("ACH", docManagerInfo.DocManagerCode);
		}

		public void TestDeleteIsNotAllowed()
		{
			var mawb = Factory.New<CusMAWB>();
			var underbond = mawb.Underbonds.AddNew();
			underbond.C4_MovementReason = CMRUnderbondRequestCodes.Codes.UnpackLclAtDestination;
			var hawb = mawb.ChildBills.AddNew();
			hawb.CS_MsgStatus = CMRBaseStatuses.Codes.OriginalAccepted;
			Assert("Should be non deletable", !hawb.CanDelete);
			AssertEquals("Wrong 'NotAbleToDelete' Message", CusHAWBBase.HAWBCannotBeDeleted, hawb.ReasonForNotAbleToDelete);
			hawb.CS_MsgStatus = CMRBaseStatuses.Codes.NotSent;
			Assert("Should be deletable", hawb.CanDelete);
			var outturn = underbond.Outturns.AddNew();
			outturn.C5_ParentID = hawb.PK;
			Assert("Should be non deletable", !hawb.CanDelete);
			AssertEquals("Wrong 'NotAbleToDelete' Message", CusHAWB.HAWBCannotBeDeletedDueToOutturn, hawb.ReasonForNotAbleToDelete);
			outturn.Delete();
			mawb.CM_JK = Factory.New<ForwardingConsol>().PK;
			Assert("Should not be deletable", !hawb.CanDelete);
			AssertEquals("Wrong 'NotAbleToDelete' Message", CusHAWB.CannotDeleteHAWBsOnConsol, hawb.ReasonForNotAbleToDelete);
			mawb.CM_JK = ZGuid.Empty;
			Assert("Should be deletable", hawb.CanDelete);
			hawb.Delete();
			Assert(hawb.IsDeleted);
		}

		public void TestAllUnderbonds()
		{
			var masterBill = Factory.New<CusMAWB>();
			CusHAWB houseBill = masterBill.ChildBills.AddNew();

			AssertNotNull("AllUnderbonds is not null", houseBill.AllUnderbonds);
			houseBill.Underbonds.AddNew();
			houseBill.AllUnderbonds.Load();
			AssertEquals("AllUnderbonds Count is 1", 1, houseBill.AllUnderbonds.Count);
		}

		public void TestUnderbondHumanReadableName()
		{
			var masterBill = Factory.New<CusMAWB>();
			CusHAWB houseBill = masterBill.ChildBills.AddNew();
			ICusUnderbondDependentCollectionParent houseBillUnder = houseBill;
			AssertEquals("Housebill UnderbondHumanReadableName is HouseBill", "HouseBill", houseBillUnder.UnderbondHumanReadableName);
			houseBill.CS_HAWB = "123";
			AssertEquals("Housebill UnderbondHumanReadableName is HouseBill", "HouseBill 123", houseBillUnder.UnderbondHumanReadableName);
		}

		public void TestUnderbondDetails()
		{
			var masterBill = Factory.New<CusMAWB>();
			CusHAWB houseBill = masterBill.ChildBills.AddNew();
			ICusUnderbondDependentCollectionParent houseBillUnder = houseBill;
			AssertEquals("Underbond details match HouseBill details", houseBill.Details, houseBillUnder.Details);
		}

		public void TestGetAllPossibleCollectionProviderss()
		{
			var masterBill = Factory.New<CusMAWB>();
			CusHAWB houseBill = masterBill.ChildBills.AddNew();
			ICusUnderbondUnionCollectionParent houseBillUnion = houseBill;

			AssertNotNull("GetAllPossibleCollectionProviders should not return null", houseBillUnion.GetAllPossibleCollectionProviders());
			AssertEquals("Length of AllPossibleCollectionProviders is 1", 1, houseBillUnion.GetAllPossibleCollectionProviders().Length);
			AssertEquals("HouseBill", houseBill, houseBillUnion.GetAllPossibleCollectionProviders()[0]);
		}

		public void TestGetAllPossibleCollectionProvidersIncludesPartShips()
		{
			var masterBill = Factory.New<CusMAWB>();
			CusHAWB houseBill = masterBill.ChildBills.AddNew();
			CusPartShip partShip = houseBill.PartShips.AddNew();
			ICusUnderbondUnionCollectionParent houseBillUnion = houseBill;
			AssertEquals("2nd Provider", partShip, houseBillUnion.GetAllPossibleCollectionProviders()[1]);
		}

		public void TestDontInitiateRegisterMAWBInBusinessLayer()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.Shipments.AddNew();
			var mawb = Factory.New<CusMAWB>();
			mawb.CM_JK = consol.PK;
			var hawb = mawb.ChildBills.AddNew();
			AssertEquals("Business does not initiate RegisterEditableChild", false, hawb.IsRegisteredEditableChildObject(mawb));
		}

		public void TestLooksUpType()
		{
			var mAWB = Factory.New<CusMAWB>();
			CusHAWB hAWB = mAWB.ChildBills.AddNew();
			AssertEquals("Lookups", typeof(CusHAWBLookups), hAWB.Lookups.GetType());
		}

		public void TestIsMasterDefaultedFromIsColoadFromShipment()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.CoLoadMaster;

			var mAWB = Factory.New<CusMAWB>();
			CusHAWB hAWB = mAWB.ChildBills.AddNew();
			hAWB.CS_JS = shipment.PK;

			AssertEquals("CS_IsMaterHouse", false, hAWB.CS_IsMasterHouse);
			hAWB.SynchroniseData();
			AssertEquals("CS_IsMasterHouse is synchronised", true, hAWB.CS_IsMasterHouse);
		}

		[ExpectNoExceptions]
		public void TestMaxLengthOfJobDocAddressFieldsSynchronisingToHAWBDoesntBlowUp()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();

			shipment.ConsigneeDocumentaryAddress.E2_CompanyName = ZString.Replicate('x', shipment.ConsigneeDocumentaryAddress.E2_CompanyNameInfo.MaxLength);
			shipment.ConsigneeDocumentaryAddress.E2_Contact = ZString.Replicate('x', shipment.ConsigneeDocumentaryAddress.E2_ContactInfo.MaxLength);
			shipment.ConsigneeDocumentaryAddress.E2_City = ZString.Replicate('x', shipment.ConsigneeDocumentaryAddress.E2_CityInfo.MaxLength);
			shipment.ConsigneeDocumentaryAddress.E2_Phone = ZString.Replicate('x', shipment.ConsigneeDocumentaryAddress.E2_PhoneInfo.MaxLength);
			shipment.ConsigneeDocumentaryAddress.E2_Postcode = ZString.Replicate('x', shipment.ConsigneeDocumentaryAddress.E2_PostcodeInfo.MaxLength);
			shipment.ConsigneeDocumentaryAddress.E2_State = ZString.Replicate('x', shipment.ConsigneeDocumentaryAddress.E2_StateInfo.MaxLength);
			shipment.ConsigneeDocumentaryAddress.E2_Address1 = ZString.Replicate('x', shipment.ConsigneeDocumentaryAddress.E2_Address1Info.MaxLength);
			shipment.ConsigneeDocumentaryAddress.E2_Address2 = ZString.Replicate('x', shipment.ConsigneeDocumentaryAddress.E2_Address2Info.MaxLength);

			shipment.ConsignorDocumentaryAddress.E2_CompanyName = ZString.Replicate('x', shipment.ConsignorDocumentaryAddress.E2_CompanyNameInfo.MaxLength);
			shipment.ConsignorDocumentaryAddress.E2_Contact = ZString.Replicate('x', shipment.ConsignorDocumentaryAddress.E2_ContactInfo.MaxLength);
			shipment.ConsignorDocumentaryAddress.E2_City = ZString.Replicate('x', shipment.ConsignorDocumentaryAddress.E2_CityInfo.MaxLength);
			shipment.ConsignorDocumentaryAddress.E2_Phone = ZString.Replicate('x', shipment.ConsignorDocumentaryAddress.E2_PhoneInfo.MaxLength);
			shipment.ConsignorDocumentaryAddress.E2_Postcode = ZString.Replicate('x', shipment.ConsignorDocumentaryAddress.E2_PostcodeInfo.MaxLength);
			shipment.ConsignorDocumentaryAddress.E2_State = ZString.Replicate('x', shipment.ConsignorDocumentaryAddress.E2_StateInfo.MaxLength);
			shipment.ConsignorDocumentaryAddress.E2_Address1 = ZString.Replicate('x', shipment.ConsignorDocumentaryAddress.E2_Address1Info.MaxLength);
			shipment.ConsignorDocumentaryAddress.E2_Address2 = ZString.Replicate('x', shipment.ConsignorDocumentaryAddress.E2_Address2Info.MaxLength);

			CusMAWB mawb = Factory.New<CusMAWB>();
			CusHAWB hawb = mawb.ChildBills.AddNew();
			hawb.CS_JS = shipment.PK;

			hawb.SynchroniseData();
		}

		public void TestDefaultPaymentTermForCusHAWB()
		{
			Env.Registry.SetConsolPaymentTerm("PPD");
			var mAWB = Factory.New<CusMAWB>();
			CusHAWB hAWB = mAWB.ChildBills.AddNew();
			AssertEquals("PrePaid Collect should be defaulted from the registry item", "PO", hAWB.CS_FreightPrepaidCollect);
		}

		public void TestIsMasterFlagOrMasterHouseBillNumberDifferent()
		{
			var masterBill = Factory.New<CusMAWB>();
			CusHAWB houseBill = masterBill.ChildBills.AddNew();
			Factory.Save();

			houseBill.CS_IsMasterHouse = true;
			AssertEquals("IsMasterFlagOrMasterHouseBillNumberDifferent", true, houseBill.IsMasterFlagOrMasterHouseBillDifferent);

			houseBill.CS_IsMasterHouse = false;
			houseBill.CS_MasterHouseBill = "123456";
			AssertEquals("IsMasterFlagOrMasterHouseBillNumberDifferent", true, houseBill.IsMasterFlagOrMasterHouseBillDifferent);

			houseBill.CS_MasterHouseBill = "";
			AssertEquals("IsMasterFlagOrMasterHouseBillNumberDifferent", false, houseBill.IsMasterFlagOrMasterHouseBillDifferent);
		}

		[ExpectNoExceptions]
		public void TestPreAlertSubShipmentBeforeCoLoadMaster()
		{
			var masterBill = Factory.New<CusMAWB>();
			CusHAWB coLoadHAWB = masterBill.ChildBills.AddNew();
			coLoadHAWB.CS_HAWB = "111";
			coLoadHAWB.CS_IsMasterHouse = true;
			CusHAWB hAWB = masterBill.ChildBills.AddNew();
			hAWB.CS_MasterHouseBill = "111";

			hAWB.CheckPreAlertConstraint();//Constraint removed
		}

		public void TestManifestedPiecesNeverReadOnly()
		{
			var masterBill = Factory.New<CusMAWB>();
			CusHAWB houseBill = masterBill.ChildBills.AddNew();
			Factory.Save();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			CusHAWB loadedHouseBill = newFactory.Load<CusHAWB>(houseBill.PK);
			AssertEquals("PiecesManifested is editible", false, loadedHouseBill.CS_PiecesManifestedInfo.ReadOnly);

			loadedHouseBill.CS_IsPrealerted = true;
			newFactory.Save();

			BusinessObjectFactory newFactory2 = new BusinessObjectFactory();
			CusHAWB loadedHouseBill2 = newFactory2.Load<CusHAWB>(houseBill.PK);
			AssertEquals("PiecesManifested is readonly", false, loadedHouseBill2.CS_PiecesManifestedInfo.ReadOnly);
		}

		public void TestLandingToSendMessageOnWhenLoaded()
		{
			var masterBill = Factory.New<CusMAWB>();
			CusHAWB hAWB = masterBill.ChildBills.AddNew();
			Factory.Save();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			CusHAWB hAWBLoaded = newFactory.Load<CusHAWB>(hAWB.PK);
			AssertEquals("LandingToSendMessageOn is set OnLoaded", hAWBLoaded, hAWBLoaded.LandingToSendMessageOn);
		}

		public void TestIsMasterFlagDifferent()
		{
			var mAWB = Factory.New<CusMAWB>();
			mAWB.CM_MAWB = "12345678901";
			CusHAWB hAWB = mAWB.ChildBills.AddNew();
			hAWB.CS_IsMasterHouse = true;

			AssertEquals("Not different to DB version, not yet saved", false, hAWB.IsMasterFlagDifferent);
			Factory.Save();
			AssertEquals("Not different to DB version", false, hAWB.IsMasterFlagDifferent);
			hAWB.CS_IsMasterHouse = false;
			AssertEquals("Different to DB version", true, hAWB.IsMasterFlagDifferent);
		}

		public void TestIsMasterFlagDifferentWithNoChangesI4076()
		{
			var mAWB = Factory.New<CusMAWB>();
			mAWB.CM_MAWB = "12345678901";
			CusHAWB hAWB = mAWB.ChildBills.AddNew();
			DataRow newHawbRow = ((INeedRow)hAWB).Row.Table.NewRow();
			CusHAWB hAWB2 = new CusHAWB(Factory, newHawbRow);
			AssertEquals("Detached Row quering Original Value", false, hAWB2.IsMasterFlagDifferent);
			AssertEquals("Detached Row quering Original Value", false, hAWB2.IsMasterHouseBillDifferent);
		}

		public void TestIsMasterHouseNumDifferent2()
		{
			var mAWB = Factory.New<CusMAWB>();
			mAWB.CM_MAWB = "12345678901";
			CusHAWB hAWB = mAWB.ChildBills.AddNew();
			hAWB.CS_MasterHouseBill = "1";

			AssertEquals("Not different to DB version, not yet saved", false, hAWB.IsMasterHouseBillDifferent);
			Factory.Save();
			AssertEquals("Not different to DB version", false, hAWB.IsMasterHouseBillDifferent);
			hAWB.CS_MasterHouseBill = "2";
			AssertEquals("Different to DB version", true, hAWB.IsMasterHouseBillDifferent);
		}

		public void TestHouseBillChanged()
		{
			var mAWB = Factory.New<CusMAWB>();
			mAWB.CM_MAWB = "08190905656";
			CusHAWB hAWB = mAWB.ChildBills.AddNew();
			hAWB.CS_HAWB = "123";
			Factory.Save();

			hAWB.CS_HAWB = "321";
			Assert("HouseBill number changed", hAWB.HouseBillNumberChanged);

			hAWB.CS_HAWB = "123";
			Assert("HouseBill number not changed", !hAWB.HouseBillNumberChanged);
		}

		public void TestMakeHouseBillNumberEditibleWhenZeroLanded()
		{
			var mAWB = Factory.New<CusMAWB>();
			CusHAWB hAWB = mAWB.ChildBills.AddNew();
			hAWB.CS_IsPrealerted = true;
			hAWB.CS_CustomsStatus = AirCargoMessage.NewStatus.Zero_landed;
			Factory.Save();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			CusHAWB loadedHAWB = newFactory.Load<CusHAWB>(hAWB.PK);
			Assert("HouseBill is editible", !loadedHAWB.CS_HAWBInfo.ReadOnly);
		}

		public void TestDontRefreshDataWhenAwaitingResponse()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_GoodsDescription = "ShipmentDescription";

			var mAWB = Factory.New<CusMAWB>();
			CusHAWB houseBill = mAWB.ChildBills.AddNew();
			houseBill.CS_JS = shipment.PK;
			houseBill.CS_GoodsDescription = "HouseBillDescription";

			houseBill.CS_MsgStatus = CMRBaseStatuses.Codes.OriginalAccepted;
			houseBill.SynchroniseData();
			Assert("Goods Description shouldnt be udpated", shipment.JS_GoodsDescription != houseBill.CS_GoodsDescription);
		}

		public void TestIsWeightDifferent()
		{
			var masterBill = Factory.New<CusMAWB>();
			CusHAWB houseBill = masterBill.ChildBills.AddNew();
			houseBill.CS_Weight = 20;
			houseBill.CS_WeightUQ = "KG";
			Factory.Save();

			houseBill.CS_WeightUQ = "LB";
			Assert("Weight is different", houseBill.IsWeightDifferent);
		}

		public void TestIsPaymentMethodDifferent()
		{
			var masterBill = Factory.New<CusMAWB>();
			CusHAWB houseBill = masterBill.ChildBills.AddNew();
			houseBill.CS_FreightPrepaidCollect = "PPD";
			Factory.Save();

			houseBill.CS_FreightPrepaidCollect = "CCX";
			Assert("Payment method is different", houseBill.IsPaymentMethodDifferent);
		}

		public void TestIsMoneyValueDifferent()
		{
			var masterBill = Factory.New<CusMAWB>();
			CusHAWB houseBill = masterBill.ChildBills.AddNew();
			houseBill.CS_RX_NKGoodsCurrency = "USD";
			Factory.Save();

			houseBill.CS_RX_NKGoodsCurrency = "EUR";
			Assert("Goods value is different", houseBill.IsMoneyValueDifferent);

			houseBill.CS_RX_NKGoodsCurrency = "USD";
			Assert("Goods value is not different", !houseBill.IsMoneyValueDifferent);
		}

		public void TestIsDescriptionDifferent()
		{
			var masterBill = Factory.New<CusMAWB>();
			CusHAWB houseBill = masterBill.ChildBills.AddNew();
			houseBill.CS_GoodsDescription = "Original";
			Factory.Save();

			houseBill.CS_GoodsDescription = "Changed";
			Assert("Goods description is different", houseBill.IsDescriptionDifferent);
		}

		public void TestIsPortDifferent()
		{
			var masterBill = Factory.New<CusMAWB>();
			CusHAWB houseBill = masterBill.ChildBills.AddNew();
			houseBill.CS_RL_NKOrigin = "NZAKL";
			Factory.Save();

			houseBill.CS_RL_NKOrigin = "NAXXX";
			Assert("Origin is different", houseBill.IsPortDifferent("10"));
		}

		public void TestIsPiecesManifestedDifferent()
		{
			var masterBill = Factory.New<CusMAWB>();
			CusHAWB houseBill = masterBill.ChildBills.AddNew();
			houseBill.CS_PiecesManifested = 10;
			Factory.Save();

			houseBill.CS_PiecesManifested = 20;
			Assert("Manifested is different", houseBill.IsManifestedPiecesDifferent);
		}

		public void TestIsMasterFlagOrMasterHouseBillDifferent()
		{
			var masterBill = Factory.New<CusMAWB>();
			CusHAWB houseBill = masterBill.ChildBills.AddNew();
			houseBill.CS_IsMasterHouse = true;
			Factory.Save();

			houseBill.CS_IsMasterHouse = false;
			Assert("IsMaster Different", houseBill.IsMasterFlagOrMasterHouseBillDifferent);
		}

		public void TestSynchroniseDataForConsignor()
		{
			var consol = Factory.New<ForwardingConsol>();
			var shipment = consol.Shipments.AddNew();
			var orgHeader = Factory.New<OrgHeader>();

			orgHeader.OH_FullName = "foo";
			orgHeader.MainAddress.OA_City = "bar";
			orgHeader.MainAddress.OA_Phone = "321";
			orgHeader.MainAddress.OA_PostCode = "123";
			orgHeader.MainAddress.OA_State = "blop";
			orgHeader.MainAddress.OA_Address1 = "addr1";
			orgHeader.MainAddress.OA_Address2 = "addr2";
			orgHeader.MainAddress.OA_RL_NKRelatedPortCode = "NZAKL";
			orgHeader.Contacts.AddNew();
			orgHeader.Contacts[0].OC_ContactName = "monkey";
			orgHeader.Contacts[0].Documents.AddNew();
			orgHeader.Contacts[0].Documents[0].OD_DocumentGroup = ContactType.Consignor.Code;
			orgHeader.Contacts[0].Documents[0].OD_DefaultContact = true;

			var orgCusCode1 = orgHeader.CustomsCodes.AddNew();
			orgCusCode1.OK_CodeType = OrgCusCode.CodeTypes.CustomsClientID;
			orgCusCode1.OK_CustomsRegNo = "AAA111";
			orgCusCode1.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Australia;

			shipment.ConsignorPK = orgHeader.PK;
			shipment.JS_OuterPacks = 10;

			var masterBill = Factory.New<CusMAWB>();
			masterBill.CM_JK = consol.PK;
			var houseBill = masterBill.ChildBills.AddNew();
			houseBill.CS_JS = shipment.PK;
			houseBill.SynchroniseData();

			AssertEquals("OuterPacks", (short)10, houseBill.CS_PiecesManifested);
			AssertEquals(shipment.Consignor.MainAddress.PK, houseBill.CS_OA_ConsignorAddress);
			AssertEquals(shipment.Consignor.OH_FullName, houseBill.CS_ConsignorName);
			AssertEquals(shipment.Consignor.MainAddress.OA_City, houseBill.CS_ConsignorCity);
			AssertEquals(shipment.Consignor.MainAddress.OA_Phone, houseBill.CS_ConsignorPhone);
			AssertEquals(shipment.Consignor.MainAddress.OA_PostCode, houseBill.CS_ConsignorPostcode);
			AssertEquals(shipment.Consignor.MainAddress.OA_State, houseBill.CS_ConsignorState);
			AssertEquals(shipment.Consignor.MainAddress.OA_Address1, houseBill.CS_ConsignorStreet);
			AssertEquals(shipment.Consignor.MainAddress.OA_Address2, houseBill.CS_ConsignorStreet2);
			AssertEquals(shipment.Consignor.Contacts[0].OC_ContactName, houseBill.CS_ConsignorContactName);
			AssertEquals("NZ", houseBill.CS_RN_NKConsignorCountry);
			AssertEquals("CID for AU", "AAA111", houseBill.CS_ConsignorIdentifier);
		}

		public void TestSynchroniseConsigneeAddressFromShipmentAddress()
		{
			var consol = Factory.New<ForwardingConsol>();
			var shipment = consol.Shipments.AddNew();
			var consigneeOrg = Factory.New<OrgHeader>();
			consigneeOrg.OH_FullName = "Test Consignee";
			var deliveryOrg = Factory.New<OrgHeader>();
			deliveryOrg.OH_FullName = "Test Delivery";
			var consigneeAddressPK = consigneeOrg.MainAddress.PK;
			var deliveryAddressPK = deliveryOrg.MainAddress.PK;
			shipment.ConsigneeDocumentaryAddress.E2_OA_Address = consigneeAddressPK;
			shipment.ConsigneeDeliveryAddress.E2_OA_Address = deliveryOrg.MainAddress.PK;
			var masterBill = Factory.New<CusMAWB>();
			masterBill.CM_JK = consol.PK;
			var houseBill = masterBill.ChildBills.AddNew();
			houseBill.CS_JS = shipment.PK;

			consigneeOrg.MiscServ.OM_IMAirCargoReportDefaultConsignee = ConsigneeDefaultOptionList.Codes.Consignee;
			houseBill.SynchroniseData();
			AssertEquals("Default to shipment Consignee address", consigneeAddressPK, houseBill.CS_OA_ConsigneeAddress);

			consigneeOrg.MiscServ.OM_IMAirCargoReportDefaultConsignee = ConsigneeDefaultOptionList.Codes.DeliverTo;
			houseBill.SynchroniseData();
			AssertEquals("Default to shipment Delivery address", deliveryAddressPK, houseBill.CS_OA_ConsigneeAddress);

			houseBill.CS_OA_ConsigneeAddress = ZGuid.Empty;
			consigneeOrg.MiscServ.OM_IMAirCargoReportDefaultConsignee = ConsigneeDefaultOptionList.Codes.None;
			houseBill.SynchroniseData();
			AssertEquals("Doesn't default address", ZGuid.Empty, houseBill.CS_OA_ConsigneeAddress);

			consigneeOrg.MiscServ.OM_IMAirCargoReportDefaultConsignee = ConsigneeDefaultOptionList.Codes.Default;
			using (AUCustomsDataRegistry.Instance.DefaultAirCargoConsignee.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Enterprise.Core.Constants.AUCustoms.DefaultConsigneeOption.Consignee))
			{
				houseBill.SynchroniseData();
				AssertEquals("Default to shipment Consignee address based on registry setting", consigneeAddressPK, houseBill.CS_OA_ConsigneeAddress);
			}

			using (AUCustomsDataRegistry.Instance.DefaultAirCargoConsignee.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Enterprise.Core.Constants.AUCustoms.DefaultConsigneeOption.DeliverTo))
			{
				houseBill.SynchroniseData();
				AssertEquals("Default to shipment Delivery address based on registry setting", deliveryAddressPK, houseBill.CS_OA_ConsigneeAddress);
			}

			using (AUCustomsDataRegistry.Instance.DefaultAirCargoConsignee.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Enterprise.Core.Constants.AUCustoms.DefaultConsigneeOption.None))
			{
				houseBill.CS_OA_ConsigneeAddress = ZGuid.Empty;
				houseBill.SynchroniseData();
				AssertEquals("Doesn't default address based on registry setting", ZGuid.Empty, houseBill.CS_OA_ConsigneeAddress);
			}
		}

		public void TestSynchroniseConsigneeAddressFromShipmentOverrideAddress()
		{
			var consol = Factory.New<ForwardingConsol>();
			var shipment = consol.Shipments.AddNew();
			shipment.ConsigneeDeliveryAddress.E2_AddressOverride = true;
			shipment.ConsigneeDeliveryAddress.E2_CompanyName = "NAME OVERRIDE1";
			shipment.ConsigneeDeliveryAddress.E2_City = "CITY OVERRIDE1";
			shipment.ConsigneeDeliveryAddress.E2_Phone = "PHONE OVERRIDE1";
			shipment.ConsigneeDeliveryAddress.E2_Postcode = "PC OVR1";
			shipment.ConsigneeDeliveryAddress.E2_State = "STATE OVRRIDE1";
			shipment.ConsigneeDeliveryAddress.E2_Address1 = "ADDRESS1 OVERRIDE1";
			shipment.ConsigneeDeliveryAddress.E2_Address2 = "ADDRESS2 OVERRIDE1";
			shipment.ConsigneeDeliveryAddress.E2_Contact = "CONTACT OVERRIDE1";
			shipment.ConsigneeDocumentaryAddress.E2_AddressOverride = true;
			shipment.ConsigneeDocumentaryAddress.E2_CompanyName = "NAME OVERRIDE2";
			shipment.ConsigneeDocumentaryAddress.E2_City = "CITY OVERRIDE2";
			shipment.ConsigneeDocumentaryAddress.E2_Phone = "PHONE OVERRIDE2";
			shipment.ConsigneeDocumentaryAddress.E2_Postcode = "PC OVR2";
			shipment.ConsigneeDocumentaryAddress.E2_State = "STATE OVRRIDE2";
			shipment.ConsigneeDocumentaryAddress.E2_Address1 = "ADDRESS1 OVERRIDE2";
			shipment.ConsigneeDocumentaryAddress.E2_Address2 = "ADDRESS2 OVERRIDE2";
			shipment.ConsigneeDocumentaryAddress.E2_Contact = "CONTACT OVERRIDE2";
			var masterBill = Factory.New<CusMAWB>();
			masterBill.CM_JK = consol.PK;
			var houseBill = masterBill.ChildBills.AddNew();
			houseBill.CS_JS = shipment.PK;

			using (AUCustomsDataRegistry.Instance.DefaultAirCargoConsignee.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Enterprise.Core.Constants.AUCustoms.DefaultConsigneeOption.None))
			{
				houseBill.SynchroniseData();
				CombineAssertions("Doesn't default address based on registry setting", () =>
				{
					AssertEquals(ZGuid.Empty, houseBill.CS_OA_ConsigneeAddress);
					AssertEquals(ZString.Empty, houseBill.CS_ConsigneeName);
					AssertEquals(ZString.Empty, houseBill.CS_ConsigneeCity);
					AssertEquals(ZString.Empty, houseBill.CS_ConsigneePhone);
					AssertEquals(ZString.Empty, houseBill.CS_ConsigneePostcode);
					AssertEquals(ZString.Empty, houseBill.CS_ConsigneeState);
					AssertEquals(ZString.Empty, houseBill.CS_ConsigneeStreet);
					AssertEquals(ZString.Empty, houseBill.CS_ConsigneeStreet2);
					AssertEquals(ZString.Empty, houseBill.CS_ConsigneeContactName);
				});
			}

			using (AUCustomsDataRegistry.Instance.DefaultAirCargoConsignee.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Enterprise.Core.Constants.AUCustoms.DefaultConsigneeOption.DeliverTo))
			{
				houseBill.SynchroniseData();
				CombineAssertions("Default to shipment Consignee address based on registry setting", () =>
				{
					AssertEquals(ZGuid.Empty, houseBill.CS_OA_ConsigneeAddress);
					AssertEquals("NAME OVERRIDE1", houseBill.CS_ConsigneeName);
					AssertEquals("CITY OVERRIDE1", houseBill.CS_ConsigneeCity);
					AssertEquals("PHONE OVERRIDE1", houseBill.CS_ConsigneePhone);
					AssertEquals("PC OVR1", houseBill.CS_ConsigneePostcode);
					AssertEquals("STATE OVRRIDE1", houseBill.CS_ConsigneeState);
					AssertEquals("ADDRESS1 OVERRIDE1", houseBill.CS_ConsigneeStreet);
					AssertEquals("ADDRESS2 OVERRIDE1", houseBill.CS_ConsigneeStreet2);
					AssertEquals("CONTACT OVERRIDE1", houseBill.CS_ConsigneeContactName);
				});
			}

			using (AUCustomsDataRegistry.Instance.DefaultAirCargoConsignee.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Enterprise.Core.Constants.AUCustoms.DefaultConsigneeOption.Consignee))
			{
				houseBill.SynchroniseData();
				CombineAssertions("Default to shipment Delivery address based on registry setting", () =>
				{
					AssertEquals(ZGuid.Empty, houseBill.CS_OA_ConsigneeAddress);
					AssertEquals("NAME OVERRIDE2", houseBill.CS_ConsigneeName);
					AssertEquals("CITY OVERRIDE2", houseBill.CS_ConsigneeCity);
					AssertEquals("PHONE OVERRIDE2", houseBill.CS_ConsigneePhone);
					AssertEquals("PC OVR2", houseBill.CS_ConsigneePostcode);
					AssertEquals("STATE OVRRIDE2", houseBill.CS_ConsigneeState);
					AssertEquals("ADDRESS1 OVERRIDE2", houseBill.CS_ConsigneeStreet);
					AssertEquals("ADDRESS2 OVERRIDE2", houseBill.CS_ConsigneeStreet2);
					AssertEquals("CONTACT OVERRIDE2", houseBill.CS_ConsigneeContactName);
				});
			}
		}

		public void TestSynchroniseConsignorWithShipment()
		{
			var consol = Factory.New<ForwardingConsol>();
			CommonShipment shipment = consol.Shipments.AddNew();
			OrgHeader consignor = Factory.New<OrgHeader>();
			consignor.OH_FullName = "Test Consignor";
			consignor.Contacts.AddNew();
			consignor.Contacts[0].OC_ContactName = "Test Consignor Contact";
			consignor.Contacts[0].Documents.AddNew();
			consignor.Contacts[0].Documents[0].OD_DocumentGroup = ContactType.Consignor.Code;
			consignor.Contacts[0].Documents[0].OD_DefaultContact = true;
			shipment.ConsignorPK = consignor.PK;

			var masterBill = Factory.New<CusMAWB>();
			masterBill.CM_JK = consol.PK;
			CusHAWB houseBill = masterBill.ChildBills.AddNew();
			houseBill.CS_JS = shipment.PK;
			houseBill.SynchroniseData();

			AssertEquals(consignor.MainAddress.PK, houseBill.CS_OA_ConsignorAddress);
			AssertEquals(consignor.OH_FullName, houseBill.CS_ConsignorName);
			AssertEquals(shipment.Consignor.Contacts[0].OC_ContactName, houseBill.CS_ConsignorContactName);
		}

		public void TestSynchroniseDataForConsignorAddressOverride()
		{
			var consol = Factory.New<ForwardingConsol>();
			CommonShipment shipment = consol.Shipments.AddNew();
			shipment.ConsignorDocumentaryAddress.E2_AddressOverride = true;
			shipment.ConsignorDocumentaryAddress.E2_CompanyName = "NAME OVERRIDE";
			shipment.ConsignorDocumentaryAddress.E2_City = "CITY OVERRIDE";
			shipment.ConsignorDocumentaryAddress.E2_Phone = "PHONE OVERRIDE";
			shipment.ConsignorDocumentaryAddress.E2_Postcode = "PC OVR";
			shipment.ConsignorDocumentaryAddress.E2_State = "STATE OVRRIDE";
			shipment.ConsignorDocumentaryAddress.E2_Address1 = "ADDRESS1 OVERRIDE";
			shipment.ConsignorDocumentaryAddress.E2_Address2 = "ADDRESS2 OVERRIDE";
			shipment.ConsignorDocumentaryAddress.E2_Contact = "CONTACT OVERRIDE";
			var masterBill = Factory.New<CusMAWB>();
			masterBill.CM_JK = consol.PK;
			CusHAWB houseBill = masterBill.ChildBills.AddNew();
			houseBill.CS_JS = shipment.PK;
			houseBill.SynchroniseData();
			AssertEquals(ZGuid.Empty, houseBill.CS_OA_ConsignorAddress);
			AssertEquals("NAME OVERRIDE", houseBill.CS_ConsignorName);
			AssertEquals("CITY OVERRIDE", houseBill.CS_ConsignorCity);
			AssertEquals("PHONE OVERRIDE", houseBill.CS_ConsignorPhone);
			AssertEquals("PC OVR", houseBill.CS_ConsignorPostcode);
			AssertEquals("STATE OVRRIDE", houseBill.CS_ConsignorState);
			AssertEquals("ADDRESS1 OVERRIDE", houseBill.CS_ConsignorStreet);
			AssertEquals("ADDRESS2 OVERRIDE", houseBill.CS_ConsignorStreet2);
			AssertEquals("CONTACT OVERRIDE", houseBill.CS_ConsignorContactName);
		}

		public void TestSynchroniseDataForPaymentType()
		{
			var consol = Factory.New<ForwardingConsol>();
			CommonShipment shipment = consol.Shipments.AddNew();
			shipment.ConsigneePK = Factory.LoadTop1<OrgHeader>(new ZQuery()).PK;
			shipment.JS_INCO = Core.Constants.IncoTerms.FreeOnBoard;
			shipment.JS_E_ARV = new ZDateTime(2006, 12, 12);

			var masterBill = Factory.New<CusMAWB>();
			masterBill.CM_JK = consol.PK;
			masterBill.CM_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			CusHAWB houseBill = masterBill.ChildBills.AddNew();
			houseBill.CS_JS = shipment.PK;
			houseBill.SynchroniseData();

			AssertEquals("Prepaid/collect", CMRMethodsOfPayment.Codes.Collect, houseBill.CS_FreightPrepaidCollect);

			shipment.JS_INCO = Core.Constants.IncoTerms.CostAndFreight;
			houseBill.SynchroniseData();
			AssertEquals("Prepaid/collect", CMRMethodsOfPayment.Codes.PrepaidOnly, houseBill.CS_FreightPrepaidCollect);
		}

		public void TestConsolETDUsedWhenATDIsEmpty()
		{
			var consol = Factory.New<ForwardingConsol>();
			Transport transport = consol.Transports[0];
			DateTime testDate = DateTime.Today;
			transport.JW_ETA = testDate;

			var masterBill = Factory.New<CusMAWB>();
			masterBill.CM_JK = consol.PK;
			masterBill.SynchroniseData();

			AssertEquals("Master Bill Arrival Date Synchirnised", testDate, masterBill.CM_ArrivalDate);
		}

		public void TestConsolATDUsedWhenATDIsNotEmpty()
		{
			var consol = Factory.New<ForwardingConsol>();
			Transport transport = consol.Transports[0];
			DateTime testDate = DateTime.Today;
			transport.JW_ETA = testDate.AddDays(-1);
			transport.JW_ATA = testDate;

			var masterBill = Factory.New<CusMAWB>();
			masterBill.CM_JK = consol.PK;
			masterBill.SynchroniseData();

			AssertEquals("Master Bill Arrival Date Synchirnised", testDate, masterBill.CM_ArrivalDate);
		}

		public void TestReadOnly()
		{
			var master = Factory.New<CusMAWB>();
			CusHAWB houseBill1 = master.ChildBills.AddNew();
			CusHAWB houseBill2 = master.ChildBills.AddNew();
			houseBill1.CS_IsResponsePending = true;
			master.CM_HouseMessageIsSent = true;//will be set when the above flag is set by message builder
			Factory.Save();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			CusHAWB loadedHAWB = newFactory.Load<CusHAWB>(houseBill1.PK);
			Assert("Loaded HouseBill 1 is readonly as it is waiting for the message", loadedHAWB.ReadOnly);
			Assert("MAWB readonly", loadedHAWB.MAWB.CM_MAWBInfo.ReadOnly);
			Assert("MAWB readonly", loadedHAWB.MAWB.CM_ArrivalDateInfo.ReadOnly);
			Assert("MAWB readonly", loadedHAWB.MAWB.CM_RL_NKDischargePortInfo.ReadOnly);
			Assert("MAWB readonly", loadedHAWB.MAWB.CM_RL_NKLoadPortInfo.ReadOnly);
			Assert("MAWB readonly", loadedHAWB.MAWB.CM_FlightNoInfo.ReadOnly);

			loadedHAWB = newFactory.Load<CusHAWB>(houseBill2.PK);
			Assert(!loadedHAWB.ReadOnly);
			Assert("MAWB readonly", loadedHAWB.MAWB.CM_MAWBInfo.ReadOnly);
			Assert("MAWB readonly", loadedHAWB.MAWB.CM_ArrivalDateInfo.ReadOnly);
			Assert("MAWB readonly", loadedHAWB.MAWB.CM_RL_NKDischargePortInfo.ReadOnly);
			Assert("MAWB readonly", loadedHAWB.MAWB.CM_RL_NKLoadPortInfo.ReadOnly);
			Assert("MAWB readonly", loadedHAWB.MAWB.CM_FlightNoInfo.ReadOnly);
		}

		public void TestSynchronisingDataConsol()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Enterprise.Core.Constants.TransportModes.Air;

			Transport transport = consol.Transports[0];
			transport.JW_ATA = ZDateTime.Today;
			transport.JW_VoyageFlight = "QF1";
			transport.JW_RL_NKDiscPort = "AUSYD";
			transport.JW_RL_NKLoadPort = "NZAKL";
			consol.JK_MasterBillNum = "081-0000 0000";

			var masterBill = Factory.New<CusMAWB>();
			masterBill.CM_JK = consol.PK;
			masterBill.CM_RL_NKDischargePort = "";
			masterBill.SynchroniseData();

			AssertEquals("Arrival Date", consol.JK_ArrivalForLastImportTransport, masterBill.CM_ArrivalDate);
			AssertEquals("FlightNo", consol.JK_VoyageFlightForLastImportTransport, masterBill.CM_FlightNo);
			AssertEquals("Discharge Port", consol.JK_RL_NKDiscForLastImportTransport, masterBill.CM_RL_NKDischargePort);
			AssertEquals("Load Port", consol.JK_RL_NKLoadForFirstImportTransport, masterBill.CM_RL_NKLoadPort);
			AssertEquals("Master Bill number", consol.JK_MasterBillNum, masterBill.CM_MAWB);
		}

		public void TestCustomsStatus()
		{
			var mAWB = Factory.New<CusMAWB>();
			CusHAWB houseBill = mAWB.ChildBills.AddNew();
			houseBill.CS_CustomsStatus = "H600";
			Assert("CS_CustomsStatus.StartsWith('H600')", houseBill.CS_CustomsStatus.StartsWith("H600"));
		}

		public void TestMakeMasterBillReadOnlyWhenOneHouseBillWaitingForResponse()
		{
			var masterBill = Factory.New<CusMAWB>();
			CusHAWB houseBill1 = masterBill.ChildBills.AddNew();
			houseBill1.CS_HAWB = "1";
			CusHAWB houseBill2 = masterBill.ChildBills.AddNew();
			houseBill2.CS_HAWB = "2";

			houseBill1.CS_IsResponsePending = true;
			masterBill.CM_HouseMessageIsSent = true;
			Factory.Save();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			CusHAWB loadedHouse1 = newFactory.Load<CusHAWB>(houseBill1.PK);
			Assert("LoadedHouse1 is readonly", loadedHouse1.ReadOnly);

			Assert("MAWB readonly", loadedHouse1.MAWB.CM_MAWBInfo.ReadOnly);
			Assert("MAWB readonly", loadedHouse1.MAWB.CM_ArrivalDateInfo.ReadOnly);
			Assert("MAWB readonly", loadedHouse1.MAWB.CM_RL_NKDischargePortInfo.ReadOnly);
			Assert("MAWB readonly", loadedHouse1.MAWB.CM_RL_NKLoadPortInfo.ReadOnly);
			Assert("MAWB readonly", loadedHouse1.MAWB.CM_FlightNoInfo.ReadOnly);

			newFactory = new BusinessObjectFactory();
			CusHAWB loadedHouse2 = newFactory.Load<CusHAWB>(houseBill2.PK);
			Assert("LoadedHouse2 is not readonly", !loadedHouse2.ReadOnly);
			Assert("MAWB readonly", loadedHouse2.MAWB.CM_MAWBInfo.ReadOnly);
			Assert("MAWB readonly", loadedHouse2.MAWB.CM_ArrivalDateInfo.ReadOnly);
			Assert("MAWB readonly", loadedHouse2.MAWB.CM_RL_NKDischargePortInfo.ReadOnly);
			Assert("MAWB readonly", loadedHouse2.MAWB.CM_RL_NKLoadPortInfo.ReadOnly);
			Assert("MAWB readonly", loadedHouse2.MAWB.CM_FlightNoInfo.ReadOnly);
		}

		public void TestHasMessageChangesFlag()
		{
			CusHAWB houseBill = new ZTestHelper(Factory).CreateTestHouseBill();
			Factory.Save();

			houseBill.CS_IsPrealerted = true;
			houseBill.MAWB.CM_FlightNo = "QF1";
			Assert("HouseBill HasChanges", houseBill.HasChanges);
			Assert("HouseBill Message Changes excludes MasterBill Changes", !houseBill.HasMessageChanges);

			Factory.Save();

			houseBill.CS_FolioReference = "XXX";
			Assert("HouseBill HasChanges", houseBill.HasChanges);
			Assert("HouseBill Message Changes excludes Folio Changes", !houseBill.HasMessageChanges);

			Factory.Save();
			houseBill.CS_WarehouseLocation = "WARE";
			Assert("HouseBill HasChanges", houseBill.HasChanges);
			Assert("HouseBill Message Changes excludes Warehouse Location Changes", !houseBill.HasMessageChanges);

			Factory.Save();
			houseBill.CS_ShipmentType = "ZZZ";
			Assert("HouseBill HasChanges", houseBill.HasChanges);
			Assert("HouseBill Message Changes excludes Warehouse Location Changes", !houseBill.HasMessageChanges);

			Factory.Save();
			houseBill.CS_GoodsDescription = "Other Description";
			Assert("HouseBill Message Changes", houseBill.HasMessageChanges);
		}

		public void TestMakeReadonlyOnceWithdrawn()
		{
			var masterBill = Factory.New<CusMAWB>();
			CusHAWB houseBill = masterBill.ChildBills.AddNew();

			houseBill.CS_IsPrealerted = false;
			houseBill.CS_IsResponsePending = false;
			Factory.Save();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			CusHAWB loadedHouseBill1 = newFactory.Load<CusHAWB>(houseBill.PK);
			CusMAWB loadedMaster = loadedHouseBill1.MAWB;

			Assert("", !loadedHouseBill1.ReadOnly);
			Assert("Zero-Landing sent therefore, MasterDetails is editible", !loadedMaster.ReadOnly);
		}

		public void TestDontRefreshDataWhenMasterBillReadonly()
		{
			var consol = Factory.New<ForwardingConsol>();
			Transport transport = consol.Transports[0];
			transport.JW_VoyageFlight = "QF1";
			transport.JW_RL_NKDiscPort = "AUSYD";

			CommonShipment shipment = consol.Shipments.AddNew();
			shipment.JS_OuterPacks = 2;

			var masterBill = Factory.New<CusMAWB>();
			masterBill.CM_JK = consol.PK;

			CusHAWB houseBill = masterBill.ChildBills.AddNew();
			houseBill.CS_JS = shipment.PK;

			houseBill.SynchroniseData();
			masterBill.SynchroniseData();

			Assert("It should have updated", houseBill.CS_PiecesManifested == shipment.JS_OuterPacks);
			Assert("it should have updated the master", houseBill.MAWB.CM_FlightNo == consol.JK_VoyageFlightForLastImportTransport);

			transport.JW_VoyageFlight = "QF2";
			masterBill.CM_HouseMessageIsSent = true;//Shouldn;t update the master details
			masterBill.SynchroniseData();
			Assert("It should not update the master", houseBill.MAWB.CM_FlightNo != consol.JK_VoyageFlightForLastImportTransport);
		}

		public void TestGetMasterHouseBill()
		{
			var masterBill = Factory.New<CusMAWB>();
			CusHAWB coLoad = masterBill.ChildBills.AddNew();
			CusHAWB master = masterBill.ChildBills.AddNew();

			master.CS_HAWB = "M123456";
			coLoad.CS_HAWB = "H123456";
			coLoad.CS_MasterHouseBill = master.CS_HAWB;
			Factory.Save();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			CusHAWB loadedCoLoad = newFactory.Load<CusHAWB>(coLoad.PK);
			AssertEquals("Master Shipment", master.PK, coLoad.MasterHouseBill.PK);
		}

		public void TestMessageStatusFromCustomsStatus()
		{
			var masterBill = Factory.New<CusMAWB>();
			CusHAWB houseBill = masterBill.ChildBills.AddNew();
			AssertEquals("HouseBill status", CusHAWB.MessageStatus.NotSent, houseBill.CS_MessageStatus);

			houseBill.CS_IsResponsePending = true;
			AssertEquals("House Bill status", AirCargoMessage.WaitingDescription, houseBill.CS_MessageStatus);

			houseBill.CS_IsResponsePending = false;
			houseBill.CS_CustomsStatus = "C100";
			Assert("House Bill status", houseBill.CS_MessageStatus.StartsWith("C100"));

			houseBill.CS_CustomsStatus = "REJ";
			Assert("House Bill status", houseBill.CS_MessageStatus.StartsWith("REJ"));
		}

		public void TestDontRefreshPiecesManifestedWhenPiecesDiscrepancy()
		{
			var shipment = Factory.New<ForwardingShipment>();

			var masterBill = Factory.New<CusMAWB>();
			CusHAWB houseBill = masterBill.ChildBills.AddNew();
			houseBill.CS_JS = shipment.PK;
			houseBill.CS_PiecesManifested = 10;

			houseBill.CS_IsPrealerted = true;

			shipment.JS_OuterPacks = 20;
			houseBill.SynchroniseData();
			AssertEquals("Pieces Manifested is not updated as pieces discrepancy is logged", (short)10, houseBill.CS_PiecesManifested);

			houseBill.CS_IsPrealerted = false;
			houseBill.SynchroniseData();
			AssertEquals("Pieces Manifested is updated as pieces discrepancy is cancelled", (short)20, houseBill.CS_PiecesManifested);
		}

		[ExpectNoExceptions]
		public void TestLongOrgNameDontBlowOut()
		{
			var mAWB = Factory.New<CusMAWB>();
			CusHAWB houseBill = mAWB.ChildBills.AddNew();
			var partyWithLongName = Factory.New<OrgHeader>();
			partyWithLongName.OH_FullName = "This is a very very very very very very long name";

			houseBill.CS_OA_ConsigneeAddress = partyWithLongName.PK;
		}

		public void TestMessageReferenceFromShipment()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			shipment.JS_HouseBill = "H1234";

			ForwardingConsol consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = Enterprise.Core.Constants.TransportModes.Air;

			Transport transport = consol.Transports[0];
			transport.JW_RL_NKDiscPort = "AUSYD";

			Factory.Save();

			var masterBill = Factory.New<CusMAWB>();
			CusHAWB houseBill = masterBill.ChildBills.AddNew();
			houseBill.CS_JS = shipment.PK;

			Factory.Save();
			Assert("Reference Number of HouseBill", !houseBill.CS_MessageReference.IsEmpty);
			Assert("Reference Number of HouseBill", houseBill.CS_MessageReference.StartsWith("S"));
		}

		public void TestAirPortType()
		{
			var mAWB = Factory.New<CusMAWB>();
			CusHAWB houseBill = mAWB.ChildBills.AddNew();

			var airPort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_HasAirport, "Y"));
			var seaPort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_HasAirport, "N"));
			AssertNotNull("PreCondition: Airport", airPort);
			houseBill.CS_RL_NKDestination = airPort.RL_Code;
			Assert("No warning", !houseBill.CS_RL_NKDestinationInfo.HasWarnings());

			houseBill.CS_RL_NKDestination = seaPort.RL_Code;
			Assert("No Air port has been warned", houseBill.CS_RL_NKDestinationInfo.HasWarnings());
		}

		public void TestSynchronisedHouseBillRetainsHyphenSlash()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_HouseBill = "012-123/$?'454";

			var masterBill = Factory.New<CusMAWB>();
			CusHAWB houseBill = masterBill.ChildBills.AddNew();
			houseBill.CS_JS = shipment.PK;
			houseBill.SynchroniseData();
			AssertEquals("House bill number synchronised has - and / but no other non-alpha", "012-123/454", houseBill.CS_HAWB);
		}

		public void TestLongShipmentNumberDoesNotCauseExceptionWhenSyncronising()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				CommonShipment shipment = Factory.New<CommonShipment>();
				shipment.JS_UniqueConsignRef = "ALONGSHIPMENTNUMBER";
				var masterBill = Factory.New<CusMAWB>();
				CusHAWB houseBill = masterBill.ChildBills.AddNew();
				houseBill.CS_JS = shipment.PK;
				houseBill.SynchroniseData();
				AssertEquals("ALONGSHIPMENTNUMBER", houseBill.CS_MessageReference);
			}
		}

		public void TestPiecesLandedSameAsManifestBeforeShortLanding()
		{
			var mAWB = Factory.New<CusMAWB>();
			CusHAWB houseBill = mAWB.ChildBills.AddNew();
			houseBill.CS_PiecesManifested = 100;
			AssertEquals("Landed pieces", (short)100, houseBill.CS_PiecesLanded);
		}

		public void TestUnderbondRequestBranchDiffersFromThisBranch_Flag()
		{
			var masterBill = Factory.New<CusMAWB>();
			CusHAWB houseBill = masterBill.ChildBills.AddNew();
			var differentBranch = Factory.LoadTop1<GlbBranch>(new ZQuery(GlbBranchSchema.PK, SQLComparisonOperator.NotEqual, GlbBranch.CurrentBranch.PK));
			AssertNotNull("Precondition:More than one branch", differentBranch);

			var underbondMessage = Factory.New<AirCargoMessage>();
			underbondMessage.EM_LinkUniqueID = houseBill.PK;
			underbondMessage.EM_LinkTable = CusHAWB.Schema.TableName;
			underbondMessage.EM_GB = differentBranch.PK;
			underbondMessage.EM_MessageSubType = MessageSubTypes.UnderbondRequest.Code;

			Assert("UnderbondRequestBranch differs from this branch", houseBill.UnderbondRequestBranchDiffersFromThisBranch);
		}

		public void TestNeedToClearMessages()
		{
			ZTestHelper helper = new ZTestHelper(Factory);
			var masterBill = Factory.New<CusMAWB>();
			masterBill.CM_ArrivalDate = DateTime.Today;
			masterBill.CM_FlightNo = "QF2";
			masterBill.CM_MAWB = "081-1324 5676";
			masterBill.CM_RL_NKDischargePort = "AUSYD";
			masterBill.CM_RL_NKLoadPort = "";
			masterBill.CM_ResponsiblePartyID = "87003014042";

			AssertEquals("Load port has a message error", true, masterBill.CM_RL_NKLoadPortInfo.HasMessageErrors());

			masterBill.RegisterHouseBillsAsEditableChildren();
			AssertEquals("PreCondition: House bills are registered", true, masterBill.IsRegisteredEditableChildObject(masterBill.ChildBills));

			CusHAWB houseBill = helper.CreateTestHouseBill(masterBill);
			houseBill.RunPreSaveValidation();
			AssertEquals("House bill has no message error", false, houseBill.HasMessageErrors);
			AssertEquals("NeedToClearMessageErrors should consider Master Bill Message Error too", true, houseBill.NeedToClearMessageErrors);
		}

		public void TestHouseBillNumberChanged()
		{
			var masterBill = Factory.New<CusMAWB>();
			CusHAWB houseBill = masterBill.ChildBills.AddNew();
			houseBill.CS_HAWB = "1";
			Assert("House bill number not changed as there is no DB version of this house bill", !houseBill.HouseBillNumberChanged);
			Factory.Save();

			houseBill.CS_HAWB = "2";
			Assert("House bill number changed", houseBill.HouseBillNumberChanged);
			Factory.Save();

			houseBill.CS_HAWB = "2";
			Assert("House bill number changed", !houseBill.HouseBillNumberChanged);
		}

		public void TestDefaultFromFreight()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var masterBill = Factory.New<CusMAWB>();
			CusHAWB houseBill = masterBill.ChildBills.AddNew();
			houseBill.DefaultFromFreight(shipment);
			AssertEquals("HouseBill.CS_CM", masterBill.PK, houseBill.CS_CM);
			AssertEquals("HouseBill.CS_JS", shipment.PK, houseBill.CS_JS);
		}

		public void TestCreateNew()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var masterBill = Factory.New<CusMAWB>();
			CusHAWB houseBill = CusHAWB.CreateNew(masterBill, shipment);
			AssertEquals("HouseBill.CS_JS", shipment.PK, houseBill.CS_JS);
			AssertEquals("HouseBill.CS_CM", masterBill.PK, houseBill.CS_CM);
			AssertEquals("MasterBill.HouseBills.Count", 1, masterBill.ChildBills.Count);
		}

		public void TestLoadFromShipment()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var masterBill = Factory.New<CusMAWB>();
			CusHAWB.CreateNew(masterBill, shipment);
			AssertNotNull(CusHAWB.Load(shipment));
		}

		public void TestLoadFromShipmentWithMultipleMAWBs()
		{
			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			factory2.RefreshEnabled = false;
			ForwardingShipment shipment = factory2.New<ForwardingShipment>();
			Customs.Business.CusMAWB cusMAWB = factory2.New<CusMAWB>();
			cusMAWB.CM_ApplicationCode = Enterprise.Core.Constants.Customs.ExpressApplicationCodes.NZ.ECIWriteOff;
			Customs.Business.CusHAWB cusHAWBNZ = cusMAWB.ChildBills.AddNew();
			cusHAWBNZ.CS_JS = shipment.PK;
			factory2.Save();
			ForwardingShipment shipment2 = Factory.Load<ForwardingShipment>(shipment.PK);
			var masterBill = Factory.New<CusMAWB>();
			CusHAWB.CreateNew(masterBill, shipment2);
			CusHAWB cusHAWBAU = CusHAWB.Load(shipment2);
			AssertNotNull(cusHAWBAU);
			AssertNotEquals(cusHAWBAU.PK, cusHAWBNZ.PK);
			AssertEquals(typeof(CusHAWB), cusHAWBAU.GetType());
		}

		public void TestLoadFromShipmentWithNZCusHAWB()
		{
			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			factory2.RefreshEnabled = false;
			ForwardingShipment shipment = factory2.New<ForwardingShipment>();
			Customs.Business.CusMAWB cusMAWB = factory2.New<CusMAWB>();
			cusMAWB.CM_ApplicationCode = Enterprise.Core.Constants.Customs.ExpressApplicationCodes.NZ.ECIWriteOff;
			Customs.Business.CusHAWB cusHAWB = cusMAWB.ChildBills.AddNew();
			cusHAWB.CS_JS = shipment.PK;
			factory2.Save();
			ForwardingShipment shipment2 = Factory.Load<ForwardingShipment>(shipment.PK);
			AssertNull(CusHAWB.Load(shipment2));
		}

		public void TestValidationObject()
		{
			var masterBill = Factory.New<CusMAWB>();
			CusHAWB houseBill = masterBill.ChildBills.AddNew();
			AssertEquals("Validation", typeof(CMRCusHAWBValidation), houseBill.Validation.GetType());
		}

		public void TestDetails()
		{
			var masterBill = Factory.New<CusMAWB>();
			CusHAWB houseBill = masterBill.ChildBills.AddNew();
			masterBill.CM_MAWB = "123";
			masterBill.CM_RL_NKLoadPort = "NZAKL";
			masterBill.CM_RL_NKDischargePort = "AUSYD";
			houseBill.CS_MessageReference = "321";
			houseBill.CS_HAWB = "246";
			houseBill.CS_RL_NKOrigin = "NZAKL";
			houseBill.CS_RL_NKDestination = "AUSYD";
			AssertEquals("Details", @"MASTER BILL DETAILS:
MAWB: 123
Load Port: NZAKL
Discharge Port: AUSYD

HOUSE BILL DETAILS:
Message Reference: 321
HAWB: 246
Origin: NZAKL
Destination: AUSYD
", houseBill.Details);
		}

		[TestDate(2006, 10, 10)]
		public void TestGetCargoReportBuilderForCMR()
		{
			var masterBill = Factory.New<CusMAWB>();
			CusHAWB houseBill = masterBill.ChildBills.AddNew();

			masterBill.CM_ArrivalDate = ZDateTime.Today;
			AssertEquals("BuilderType", typeof(AIRCRMessageBuilder), houseBill.GetCargoReportBuilder().GetType());
		}

		public void TestLoadCusHAWB()
		{
			TestHelperHAWBInformation info = new TestHelperHAWBInformation();
			info.ArrivalDate = ZDateTime.Now;
			info.MAWB = "1";
			info.HAWB = "1";
			info.FlightNumber = "QF123";
			AssertNull(CusHAWB.Load(Factory, info));
			CreateAndSaveHAWB(info);
			AssertNotNull(CusHAWB.Load(Factory, info));
		}

		public void TestLoadCusHAWBWithNullDate()
		{
			TestHelperHAWBInformation info = new TestHelperHAWBInformation();
			info.ArrivalDate = ZDateTime.Empty;
			AssertNull(CusHAWB.Load(Factory, info));
		}

		public void TestLoadCusHAWBWithInvalidDate()
		{
			TestHelperHAWBInformation info = new TestHelperHAWBInformation();
			info.ArrivalDate = ZDateTime.Invalid;
			AssertNull(CusHAWB.Load(Factory, info));
		}

		public void TestLoadCusHAWBDeveloperError()
		{
			TestHelperHAWBInformation info = new TestHelperHAWBInformation();
			info.ArrivalDate = ZDateTime.Now;
			info.MAWB = "1";
			info.HAWB = "1";
			info.FlightNumber = "QF123";
			CreateAndSaveHAWB(info);
			AssertNotNull(CusHAWB.Load(Factory, info));
			CreateAndSaveHAWB(info);
			AssertNotNull(CusHAWB.Load(Factory, info));
			AssertNotNull("ErrorReported", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestIAirOutturnReportHeaderInformationProvider_GetHeader()
		{
			var underbond = Factory.New<CusUnderbond>();
			var masterBill = Factory.New<CusMAWB>();
			CusHAWB houseBill = masterBill.ChildBills.AddNew();
			AssertNotNull((houseBill as IAirOutturnReportHeaderInformationProvider).GetHeader(underbond));
		}

		public void TestIsBureau()
		{
			var underbond = Factory.New<CusUnderbond>();
			var masterBill = Factory.New<CusMAWB>();
			CusHAWB houseBill = masterBill.ChildBills.AddNew();
			IUnderbondMovementRequestHeader header = ((IUnderbondMovementRequestHeaderProvider)houseBill).GetHeader(underbond);
			masterBill.CM_IsBureau = false;
			AssertEquals("IsBureau", false, header.IsBureau);
			masterBill.CM_IsBureau = true;
			AssertEquals("IsBureau", true, header.IsBureau);
		}

		public void TestABNSpacesStripped()
		{
			var masterBill = Factory.New<CusMAWB>();
			CusHAWB houseBill = masterBill.ChildBills.AddNew();
			houseBill.CS_ResponsiblePartyID = ABNNumberWithSpaces;
			AssertEquals("Responsible Party ID", ABNNumberWithOutSpaces, houseBill.CS_ResponsiblePartyID);
		}

		public void TestReportedToCustomsPreFlight()
		{
			var mawb = Factory.New<CusMAWB>();
			var hawb1 = mawb.ChildBills.AddNew();
			hawb1.CS_MsgStatus = CMRBaseStatuses.Codes.AwaitingResponseToOriginal;
			var hawb2 = mawb.ChildBills.AddNew();

			Func<StmALog, bool> isCRCLog = log => log.SL_SE_NKEvent == Events.ReportedToCustomsPreFlight.Code;

			AssertEquals("Is not reported to customs (Pre-flight)", false, mawb.Logs.Find(isCRCLog).Any());
			AssertEquals("Is not reported to customs (Pre-flight)", false, hawb1.Logs.Find(isCRCLog).Any());
			AssertEquals("Is not reported to customs (Pre-flight)", false, hawb2.Logs.Find(isCRCLog).Any());
			hawb1.CS_MsgStatus = CMRBaseStatuses.Codes.OriginalAccepted;
			Assert("Is reported to customs (Pre-flight)", mawb.Logs.Find(isCRCLog).Count() == 1);
			hawb2.CS_MsgStatus = CMRBaseStatuses.Codes.OriginalAccepted;
			Assert("Is reported to customs (Pre-flight) only once", mawb.Logs.Find(isCRCLog).Count() == 1);
			hawb2.CS_MsgStatus = CMRBaseStatuses.Codes.AwaitingResponseToOriginal;
			AssertEquals("Is not reported to customs (Pre-flight)", false, hawb1.Logs.Find(isCRCLog).Any());
			Assert("Is reported to customs (Pre-flight) first time", hawb2.Logs.Find(isCRCLog).Count() == 1);
			hawb2.CS_MsgStatus = CMRBaseStatuses.Codes.AwaitingResponseToWithdrawal;
			Assert("Is reported to customs (Pre-flight) second time", hawb2.Logs.Find(isCRCLog).Count() == 2);
			hawb2.CS_MsgStatus = CMRBaseStatuses.Codes.AwaitingResponseToAmendment;
			Assert("Is reported to customs (Pre-flight) third time", hawb2.Logs.Find(isCRCLog).Count() == 3);
		}

		public void TestIsHeldAtOutturn()
		{
			var hawb = Factory.New<CusHAWB>();
			hawb.CS_IsHeldAtOutturn = true;
			AssertEquals(true, ((ICargoDepotEventParent)hawb).IsHeldAtOutturn);

			((ICargoDepotEventParent)hawb).IsHeldAtOutturn = false;
			AssertEquals(false, hawb.CS_IsHeldAtOutturn);
		}

		[TestDate(2019, 10, 30)]
		public void TestClearanceDateIsSetWhenCustomsEntryStatusIsClear() // CES
		{
			var mawb = Factory.New<CusMAWB>();
			mawb.CM_MAWB = "MB1";
			var hawb = mawb.ChildBills.AddNew();
			hawb.CS_HAWB = "HB1";
			hawb.CS_CustomsStatus = CMRConsolidatedCargoStatuses.Codes.HeldCargoIsHeldUnderCustomsControl;
			Factory.Save();
			AssertEquals(ZDateTime.Empty, hawb.CS_ClearanceDate);

			hawb.CS_CustomsStatus = CMRConsolidatedCargoStatuses.Codes.ClearCargoIsFreeOfAnyImpedimentsAndMayBeReleased;
			Factory.Save();
			AssertEquals(ZDateTime.Now, hawb.CS_ClearanceDate);
		}

		protected override Type GetExpectedParentObjectType() => typeof(CusMAWB);

		protected override BusinessObject GetNewBusinessObject() => GetHAWBToTest();

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetHAWBToTest();

		protected override CusHAWBBase GetHAWBToTest(BusinessObjectFactory factory) => (factory != Factory) ? factory.New<CusMAWB>().ChildBills.AddNew() : GetNewHAWBToTest();

		CusMAWB mawb;
		CusMAWB MAWB => mawb ?? (mawb = Factory.New<CusMAWB>());

		CusHAWB hawb;
		CusHAWB HAWB => hawb ?? (hawb = GetNewHAWBToTest());

		CusHAWB GetNewHAWBToTest() => MAWB.ChildBills.AddNew();

		ProcessQueueParentHelper processQueueParentHelper;
		ProcessQueueParentHelper ProcessQueueParentHelper => processQueueParentHelper ?? (processQueueParentHelper = new ProcessQueueParentHelper(HAWB));

		void CreateAndSaveHAWB(ICusHAWBInformationProvider hAWBInformation)
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			CusMAWB mAWB = (CusMAWB)new CusMAWBBase.Loader(factory).FindFirstMatchingMAWB(hAWBInformation.MAWB)
				?? factory.New<CusMAWB>();

			CusHAWB hAWB = mAWB.ChildBills.AddNew();
			mAWB.CM_MAWB = hAWBInformation.MAWB;
			hAWB.CS_HAWB = hAWBInformation.HAWB;
			mAWB.CM_FlightNo = hAWBInformation.FlightNumber;
			mAWB.CM_ArrivalDate = hAWBInformation.ArrivalDate;
			factory.Save();
		}

		const string ABNNumberWithSpaces = "75 006 687 958";
		const string ABNNumberWithOutSpaces = "75006687958";

		sealed class TestHelperHAWBInformation : ICusHAWBInformationProvider
		{
			ZString mawb;
			public ZString MAWB
			{
				get => mawb;
				set => mawb = value;
			}

			ZString hawb;
			public ZString HAWB
			{
				get => hawb;
				set => hawb = value;
			}

			ZDateTime arrivalDate;
			public ZDateTime ArrivalDate
			{
				get => arrivalDate;
				set => arrivalDate = value;
			}

			ZString flightNumber;
			public ZString FlightNumber
			{
				get => flightNumber;
				set => flightNumber = value;
			}
		}
	}
}
