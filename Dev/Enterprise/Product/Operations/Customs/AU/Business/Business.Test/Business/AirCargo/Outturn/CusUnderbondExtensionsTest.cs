using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CusUnderbondExtensionsTest : TestCaseWithFactory
	{
		public void TestGetOutturnStatus()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();

			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_HouseBill = "HouseBill1";
			shipment1.JS_ShipmentType = Core.Constants.ShipmentTypes.StandardHouse;

			var consolCusMAWB = Factory.New<CusMAWB>();
			consolCusMAWB.CM_MAWB = "Master1";
			consolCusMAWB.CM_JK = consol.PK;

			var underbond1 = consolCusMAWB.Underbonds.AddNew();
			underbond1.OutturnStatus.Code = "";
			AssertEquals(OutturnStatus.ReadyForScanning, underbond1.GetOutturnStatus());

			underbond1.OutturnStatus.Code = CMRBaseStatuses.Codes.NotSent;
			AssertEquals(OutturnStatus.ReadyForScanning, underbond1.GetOutturnStatus());
			underbond1.OutturnStatus.Code = CMRBaseStatuses.Codes.WithdrawalAccepted;
			AssertEquals(OutturnStatus.ReadyForScanning, underbond1.GetOutturnStatus());
			underbond1.OutturnStatus.Code = CMRBaseStatuses.Codes.OriginalRejected;
			AssertEquals(OutturnStatus.ReadyForScanning, underbond1.GetOutturnStatus());

			underbond1.OutturnStatus.Code = CMRBaseStatuses.Codes.AwaitingResponseToAmendment;
			AssertEquals(OutturnStatus.AwaitingResponseFromCustoms, underbond1.GetOutturnStatus());

			underbond1.OutturnStatus.Code = CMRBaseStatuses.Codes.AwaitingResponseToOriginal;
			AssertEquals(OutturnStatus.AwaitingResponseFromCustoms, underbond1.GetOutturnStatus());

			underbond1.OutturnStatus.Code = CMRBaseStatuses.Codes.AwaitingResponseToWithdrawal;
			AssertEquals(OutturnStatus.AwaitingResponseFromCustoms, underbond1.GetOutturnStatus());

			underbond1.OutturnStatus.Code = CMRBaseStatuses.Codes.OriginalAccepted;
			AssertEquals(OutturnStatus.Sent, underbond1.GetOutturnStatus());

			underbond1.OutturnStatus.Code = CMRBaseStatuses.Codes.AmendmentAccepted;
			AssertEquals(OutturnStatus.Sent, underbond1.GetOutturnStatus());
		}

		public void TestGetUnderbondStatus()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();

			var consolCusMAWB = Factory.New<CusMAWB>();
			consolCusMAWB.CM_MAWB = "Master1";
			consolCusMAWB.CM_JK = consol.PK;

			var unserbond1 = consolCusMAWB.Underbonds.AddNew();

			AssertEquals("PRECONDITION", 0, unserbond1.Outturns.Count);

			unserbond1.OutturnStatus.Code = "";
			AssertEquals(UnderbondStatus.NotSend, unserbond1.GetUnderbondStatus());

			unserbond1.OutturnStatus.Code = CMRBaseStatuses.Codes.AmendmentAccepted;
			AssertEquals(UnderbondStatus.NotSend, unserbond1.GetUnderbondStatus());

			unserbond1.OutturnStatus.Code = CMRBaseStatuses.Codes.OriginalRejected;
			AssertEquals(UnderbondStatus.NotSend, unserbond1.GetUnderbondStatus());

			unserbond1.OutturnStatus.Code = CMRBaseStatuses.Codes.AwaitingResponseToAmendment;
			AssertEquals(UnderbondStatus.WaitingCustomsResponse, unserbond1.GetUnderbondStatus());

			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_HouseBill = "HouseBill1";
			shipment1.JS_ShipmentType = Core.Constants.ShipmentTypes.StandardHouse;

			var shipment2 = consol.Shipments.AddNew();
			shipment2.JS_HouseBill = "HouseBill1";
			shipment2.JS_ShipmentType = Core.Constants.ShipmentTypes.StandardHouse;

			var shipment3 = consol.Shipments.AddNew();
			shipment3.JS_HouseBill = "HouseBill2";
			shipment3.JS_ShipmentType = Core.Constants.ShipmentTypes.StandardHouse;

			var cusHAWB1 = CusHAWB.CreateNew(consolCusMAWB, shipment1);
			var cusHAWB2 = CusHAWB.CreateNew(consolCusMAWB, shipment2);
			var cusHAWB3 = CusHAWB.CreateNew(consolCusMAWB, shipment3);

			consolCusMAWB.ChildBills.Add(cusHAWB1);
			consolCusMAWB.ChildBills.Add(cusHAWB2);
			consolCusMAWB.ChildBills.Add(cusHAWB3);

			unserbond1.OutturnStatus.Code = CMRBaseStatuses.Codes.AmendmentAccepted;
			AssertEquals(UnderbondStatus.NotSend, unserbond1.GetUnderbondStatus());

			unserbond1.OutturnStatus.Code = CMRBaseStatuses.Codes.AwaitingResponseToAmendment;
			AssertEquals(UnderbondStatus.WaitingCustomsResponse, unserbond1.GetUnderbondStatus());

			unserbond1.OutturnStatus.Code = CMRBaseStatuses.Codes.OriginalRejected;
			AssertEquals(UnderbondStatus.NotSend, unserbond1.GetUnderbondStatus());

			var outturn1 = unserbond1.Outturns.AddNew();
			outturn1.C5_ParentID = cusHAWB1.PK;
			outturn1.C5_OutturnResultType = CMROutturnResultType.Codes.NilDiscrepancy;
			outturn1.C5_LastMessageDate = ZDateTime.Now;
			var outturn2 = unserbond1.Outturns.AddNew();
			outturn2.C5_ParentID = cusHAWB2.PK;
			outturn2.C5_OutturnResultType = CMROutturnResultType.Codes.NilDiscrepancy;
			outturn2.C5_LastMessageDate = ZDateTime.Now;
			var outturn3 = unserbond1.Outturns.AddNew();
			outturn3.C5_ParentID = cusHAWB3.PK;
			outturn3.C5_OutturnResultType = CMROutturnResultType.Codes.NilDiscrepancy;
			outturn3.C5_LastMessageDate = ZDateTime.Now;

			unserbond1.OutturnStatus.Code = CMRBaseStatuses.Codes.AmendmentAccepted;
			AssertEquals(UnderbondStatus.FullySent, unserbond1.GetUnderbondStatus());

			unserbond1.OutturnStatus.Code = CMRBaseStatuses.Codes.AwaitingResponseToAmendment;
			AssertEquals(UnderbondStatus.WaitingCustomsResponse, unserbond1.GetUnderbondStatus());

			unserbond1.OutturnStatus.Code = CMRBaseStatuses.Codes.OriginalRejected;
			AssertEquals(UnderbondStatus.NotSend, unserbond1.GetUnderbondStatus());

			unserbond1.OutturnStatus.Code = CMRBaseStatuses.Codes.OriginalAccepted;
			AssertEquals(UnderbondStatus.FullySent, unserbond1.GetUnderbondStatus());

			var unserbond2 = consolCusMAWB.Underbonds.AddNew();
			cusHAWB1.ResetRemainingDCLOutturnQuantityCacheForTesting();
			cusHAWB2.ResetRemainingDCLOutturnQuantityCacheForTesting();
			cusHAWB3.ResetRemainingDCLOutturnQuantityCacheForTesting();
			AssertEquals(UnderbondStatus.CompletedOnEarlierUnderbond, unserbond2.GetUnderbondStatus());
		}

		public void TestMergeUnderbondStatuses()
		{
			var statusToMerge = new List<UnderbondStatus>() { UnderbondStatus.FullySent, UnderbondStatus.NotSend, UnderbondStatus.WaitingCustomsResponse };
			AssertEquals(UnderbondStatus.WaitingCustomsResponse, CusUnderbondExtensions.MergeUnderbondStatuses(statusToMerge));

			statusToMerge = new List<UnderbondStatus>() { UnderbondStatus.NotSend, UnderbondStatus.NotSend, UnderbondStatus.NotSend };
			AssertEquals(UnderbondStatus.NotSend, CusUnderbondExtensions.MergeUnderbondStatuses(statusToMerge));

			statusToMerge = new List<UnderbondStatus>() { UnderbondStatus.FullySent, UnderbondStatus.FullySent, UnderbondStatus.PartiallySent };
			AssertEquals(UnderbondStatus.FullySent, CusUnderbondExtensions.MergeUnderbondStatuses(statusToMerge));

			statusToMerge = new List<UnderbondStatus>() { UnderbondStatus.FullySent, UnderbondStatus.FullySent, UnderbondStatus.NotSend };
			AssertEquals(UnderbondStatus.PartiallySent, CusUnderbondExtensions.MergeUnderbondStatuses(statusToMerge));
		}

		public void TestGetAccumulatedUnderbondStatusForStandAlone()
		{
			var standAloneHouseBill = Factory.New<CusMAWB>();
			standAloneHouseBill.CM_MAWB = "Master1";
			standAloneHouseBill.CM_MasterHouseBill = "HouseBill1";

			var underbond = standAloneHouseBill.Underbonds.AddNew();
			underbond.Outturns.AddNew();
			underbond.OutturnStatus.Code = CMRBaseStatuses.Codes.AmendmentAccepted;
			AssertEquals(UnderbondStatus.FullySent, underbond.GetAccumulatedUnderbondStatus());
		}

		public void TestGetAccumulatedUnderbondStatusForConsol()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();

			var consolCusMAWB = Factory.New<CusMAWB>();
			consolCusMAWB.CM_MAWB = "Master1";
			consolCusMAWB.CM_JK = consol.PK;

			var underbond1 = consolCusMAWB.Underbonds.AddNew();
			underbond1.OutturnStatus.Code = CMRBaseStatuses.Codes.AmendmentAccepted;
			underbond1.Outturns.AddNew();

			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_HouseBill = "HouseBill1";
			shipment1.JS_ShipmentType = Core.Constants.ShipmentTypes.StandardHouse;

			var shipment2 = consol.Shipments.AddNew();
			shipment2.JS_HouseBill = "HouseBill1";
			shipment2.JS_ShipmentType = Core.Constants.ShipmentTypes.HighVolumeLowValueLegacy;

			var shipment3 = consol.Shipments.AddNew();
			shipment3.JS_HouseBill = "HouseBill2";
			shipment3.JS_ShipmentType = Core.Constants.ShipmentTypes.HighVolumeLowValueLegacy;

			var cusHAWB1 = CusHAWB.CreateNew(consolCusMAWB, shipment1);
			var cusHAWB2 = CusHAWB.CreateNew(consolCusMAWB, shipment2);
			var cusHAWB3 = CusHAWB.CreateNew(consolCusMAWB, shipment3);

			consolCusMAWB.ChildBills.Add(cusHAWB1);
			consolCusMAWB.ChildBills.Add(cusHAWB2);
			consolCusMAWB.ChildBills.Add(cusHAWB3);

			var standAloneHouseBill2 = Factory.New<CusMAWB>();
			standAloneHouseBill2.CM_MAWB = "Master1";
			standAloneHouseBill2.CM_MasterHouseBill = "HouseBill1";

			var standAloneHouseBill3 = Factory.New<CusMAWB>();
			standAloneHouseBill3.CM_MAWB = "Master1";
			standAloneHouseBill3.CM_MasterHouseBill = "HouseBill2";

			var standAlone2Underbond = standAloneHouseBill2.Underbonds.AddNew();
			var standAlone3Underbond = standAloneHouseBill3.Underbonds.AddNew();

			standAlone2Underbond.OutturnStatus.Code = CMRBaseStatuses.Codes.AmendmentAccepted;
			standAlone3Underbond.OutturnStatus.Code = CMRBaseStatuses.Codes.AmendmentAccepted;

			AssertEquals(UnderbondStatus.PartiallySent, underbond1.GetAccumulatedUnderbondStatus());

			standAlone2Underbond.OutturnStatus.Code = CMRBaseStatuses.Codes.AwaitingResponseToOriginal;
			AssertEquals(UnderbondStatus.WaitingCustomsResponse, underbond1.GetAccumulatedUnderbondStatus());

			standAlone2Underbond.OutturnStatus.Code = CMRBaseStatuses.Codes.AmendmentRejected;
			AssertEquals(UnderbondStatus.PartiallySent, underbond1.GetAccumulatedUnderbondStatus());
		}
	}
}
