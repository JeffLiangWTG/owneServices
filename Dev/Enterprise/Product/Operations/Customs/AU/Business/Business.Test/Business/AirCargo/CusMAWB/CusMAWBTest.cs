using System;
using System.Collections;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Interfaces;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CusMAWB))]
	sealed class CusMAWBTest : CusMAWBBaseAbstractTest
	{
		public void TestGetOutturnsReconciliationMessage() => CombineAssertions(() =>
		{
			var mawb = Factory.New<CusMAWB>();
			AssertEquals("No House Bills", ZString.Empty, mawb.GetOutturnsReconciliationMessage());
			var houseBill1 = mawb.FilteredChildBills.AddNew();
			houseBill1.CS_PiecesManifested = 2;
			houseBill1.CS_HAWB = "WI00152454H1";
			var houseBill2 = mawb.FilteredChildBills.AddNew();
			houseBill2.CS_PiecesManifested = 5;
			houseBill2.CS_HAWB = "WI00152454H2";
			AssertEquals("No underbond", "HouseBill WI00152454H1 (packages manifested 2) doesn't match packages outturned in Underbond", mawb.GetOutturnsReconciliationMessage());

			var underbond1 = mawb.AllUnderbonds.AddNew();
			underbond1.C4_SendersMessageReference = "U00013598";
			var outturn1 = underbond1.Outturns.AddNew();
			outturn1.C5_PackagesOutturned = 2;
			outturn1.C5_ParentID = houseBill1.PK;
			var outturn2 = underbond1.Outturns.AddNew();
			outturn2.C5_PackagesOutturned = 3;
			outturn2.C5_ParentID = houseBill2.PK;
			AssertEquals("houseBill2 has different #packages outturned in underbond", "HouseBill WI00152454H2 (packages manifested 5) doesn't match packages outturned in Underbond", mawb.GetOutturnsReconciliationMessage());
			outturn2.C5_PackagesOutturned = 5;
			AssertEquals("houseBill2 reconciled", ZString.Empty, mawb.GetOutturnsReconciliationMessage());

			var houseBill3 = mawb.FilteredChildBills.AddNew();
			houseBill3.CS_PiecesManifested = 10;
			houseBill3.CS_HAWB = "WI00152454H3";
			AssertEquals("No underbond for WI00152454H3", "HouseBill WI00152454H3 (packages manifested 10) doesn't match packages outturned in Underbond", mawb.GetOutturnsReconciliationMessage());

			var underbond2 = mawb.AllUnderbonds.AddNew();
			underbond2.C4_SendersMessageReference = "U00014000";
			var outturn3 = underbond2.Outturns.AddNew();
			outturn3.C5_PackagesOutturned = 10;
			outturn3.C5_ParentID = houseBill3.PK;
			AssertEquals("houseBill3 reconciled", ZString.Empty, mawb.GetOutturnsReconciliationMessage());
		});

		public void TestGetOutturnsReadyForSendingMessage() => CombineAssertions(() =>
		{
			var mawb = Factory.New<CusMAWB>();
			AssertEquals("No House Bills", "There are no House Bills", mawb.GetOutturnsReadyForSendingMessage());
			var houseBill1 = mawb.FilteredChildBills.AddNew();
			houseBill1.CS_PiecesManifested = 2;
			houseBill1.CS_HAWB = "WI00152454H1";
			var houseBill2 = mawb.FilteredChildBills.AddNew();
			houseBill2.CS_PiecesManifested = 5;
			houseBill2.CS_HAWB = "WI00152454H2";
			AssertEquals("No underbond", "HouseBill WI00152454H1 doesn't have matching outturn record in Underbond", mawb.GetOutturnsReadyForSendingMessage());

			var underbond1 = mawb.AllUnderbonds.AddNew();
			underbond1.C4_SendersMessageReference = "U00013598";
			var outturn1 = underbond1.Outturns.AddNew();
			outturn1.C5_PackagesOutturned = 2;
			outturn1.C5_ParentID = houseBill1.PK;
			var outturn2 = underbond1.Outturns.AddNew();
			outturn2.C5_PackagesOutturned = 3;
			outturn2.C5_ParentID = houseBill2.PK;
			AssertEquals("No error event though houseBill2 has different #packages outturned in underbond", ZString.Empty, mawb.GetOutturnsReadyForSendingMessage());

			var houseBill3 = mawb.FilteredChildBills.AddNew();
			houseBill3.CS_PiecesManifested = 10;
			houseBill3.CS_HAWB = "WI00152454H3";
			AssertEquals("No underbond for WI00152454H3", "HouseBill WI00152454H3 doesn't have matching outturn record in Underbond", mawb.GetOutturnsReadyForSendingMessage());

			var underbond2 = mawb.AllUnderbonds.AddNew();
			underbond2.C4_SendersMessageReference = "U00014000";
			var outturn3 = underbond2.Outturns.AddNew();
			outturn3.C5_PackagesOutturned = 2;
			outturn3.C5_ParentID = houseBill3.PK;
			AssertEquals("No error", ZString.Empty, mawb.GetOutturnsReadyForSendingMessage());
		});

		public void TestSynchroniseData()
		{
			var mawb = Factory.New<CusMAWB>();
			var consol = Factory.New<ForwardingConsol>();
			mawb.CM_JK = consol.PK;
			consol.JK_RL_NKLoadPort = "USLAX";
			consol.JK_RL_NKDischargePort = "AUSYD";
			consol.Transports[0].JW_ETD = ZDateTime.Today;

			mawb.SynchroniseData();
			AssertEquals(ZDateTime.Today, mawb.CM_DepartureDate);

			consol.SetAirCargoSynchroniserRetriever(() => new MAWBToConsolBridge(mawb));
			consol.Transports[0].JW_ETD = ZDateTime.Today.AddDays(1);
			AssertEquals(ZDateTime.Today.AddDays(1), mawb.CM_DepartureDate);
			consol.Transports[0].JW_ATD = ZDateTime.Today.AddDays(-1);
			AssertEquals(ZDateTime.Today.AddDays(-1), mawb.CM_DepartureDate);
		}

		public void TestIScanMasterBillProvider()
		{
			var mawb = Factory.New<CusMAWB>();
			mawb.CM_MAWB = "MB1";
			mawb.CM_MasterHouseBill = "MHB1";
			var consol = Factory.New<ForwardingConsol>();
			mawb.CM_JK = consol.PK;
			mawb.ChildBills.AddNew();
			mawb.ChildBills.AddNew();
			mawb.Underbonds.AddNew();

			var iScan = (IScanMasterBillProvider)mawb;
			AssertEquals("MB1", iScan.MasterBill);
			AssertEquals("MHB1", iScan.MasterHouseBill);
			Assert(!iScan.IsStandAlone);
			AssertEquals(2, iScan.GetChildBills(null).Count());
			AssertEquals(1, iScan.Underbonds.Count());
		}

		public void TestIsAltPartShipModelActive()
		{
			AUCustomsDataRegistry.Instance.ActivateAlternatePartShipmentModel.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
			var mawb1 = Factory.New<CusMAWB>();
			Assert(!mawb1.IsAltPartShipModelActive);
			Assert(!mawb1.CM_fUseAltPartShipModel);
			AUCustomsDataRegistry.Instance.ActivateAlternatePartShipmentModel.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			var mawb2 = Factory.New<CusMAWB>();
			Assert(mawb2.IsAltPartShipModelActive);
			Assert(mawb2.CM_fUseAltPartShipModel);
		}

		public void TestDeConsolidatorOrgHeader()
		{
			var org1 = Factory.New<OrgHeader>();
			org1.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "1", Core.Constants.CountryCodes.Australia);
			var org2 = Factory.New<OrgHeader>();
			org2.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "2", Core.Constants.CountryCodes.Australia);
			var org3 = Factory.New<OrgHeader>();
			org3.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "3", Core.Constants.CountryCodes.Australia);
			var org4 = Factory.New<OrgHeader>();
			org4.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "4", Core.Constants.CountryCodes.Australia);

			var collection = new DefaultDestinationPremiseIDCollection(Factory);
			var item1 = collection.AddNew();
			item1.AirlineCode = "QF";
			item1.PortOfDischarge = "AUSYD";
			item1.PremiseID = "1";

			AUCustomsDataRegistry.Instance.DefaultDestinationPremiseIDs = collection;

			var mawb = Factory.New<CusMAWB>();
			mawb.CM_FlightNo = "QF111";
			mawb.CM_RL_NKDischargePort = "AUMEL";
			var hawb = mawb.ChildBills.AddNew();
			hawb.CS_RL_NKDestination = "AUSYD";
			AssertEquals(org1, mawb.DeConsolidatorOrgHeader);

			var item2 = collection.AddNew();
			item2.AirlineCode = "QF";
			item2.PortOfDischarge = "AUMEL";
			item2.PremiseID = "2";
			item2.UseDischargePort = true;
			AUCustomsDataRegistry.Instance.DefaultDestinationPremiseIDs = collection;
			AssertEquals(org2, mawb.DeConsolidatorOrgHeader);

			mawb.CM_OA_UnpackDepotAddress = org3.MainAddress.PK;
			AssertEquals(org3, mawb.DeConsolidatorOrgHeader);

			var underbond = mawb.Underbonds.AddNew();
			underbond.C4_MovementReason = CMRUnderbondRequestCodes.Codes.UnpackLclAtDestination;
			underbond.C4_DestinationPremiseID = "4";
			AssertEquals(org4, mawb.DeConsolidatorOrgHeader);
		}

		[ExpectNoExceptions]
		public void TestIWorkflowTriggerEventSourceDoesNotThrowIfNoConsol()
		{
			IWorkflowTriggerEventSource mawb1 = Factory.New<CusMAWB>();
			_ = mawb1.JobHeaderCompany;
			_ = mawb1.ParentWorkflowProviders;
			var consol = Factory.New<ForwardingConsol>();
			var mawb2 = Factory.New<CusMAWB>();
			mawb2.CM_JK = consol.PK;
			_ = ((IWorkflowTriggerEventSource)mawb2).JobHeaderCompany;
		}

		public void TestParentWorkflowProviders()
		{
			IWorkflowTriggerEventSource mawb1 = Factory.New<CusMAWB>();
			var prov = mawb1.ParentWorkflowProviders;
			AssertNotNull(prov);
			AssertEquals(0, prov.Count);

			var consol = Factory.New<ForwardingConsol>();
			var mawb2 = Factory.New<CusMAWB>();
			mawb2.CM_JK = consol.PK;
			prov = ((IWorkflowTriggerEventSource)mawb2).ParentWorkflowProviders;
			AssertNotNull(prov);
			AssertEquals(1, prov.Count);
			AssertEquals(consol, prov[0]);
		}

		public void TestIsDCLOutturnComplete()
		{
			var mawb = Factory.New<CusMAWB>();
			Assert("MAWB with no houses is NOT complete", !mawb.IsDCLOutturnComplete);
			mawb.ResetDCLUnderbondCacheForTest();

			var hawb1 = mawb.ChildBills.AddNew();
			var hawb2 = mawb.ChildBills.AddNew();
			Assert("No underbonds, so is NOT complete", !mawb.IsDCLOutturnComplete);
			hawb1.ResetRemainingDCLOutturnQuantityCacheForTesting();
			hawb2.ResetRemainingDCLOutturnQuantityCacheForTesting();

			var underbond = mawb.Underbonds.AddNew();
			underbond.C4_MovementReason = CMRUnderbondRequestCodes.Codes.UnpackLclAtDestination;
			Assert("No outturns, so is NOT complete", !mawb.IsDCLOutturnComplete);
			hawb1.ResetRemainingDCLOutturnQuantityCacheForTesting();
			hawb2.ResetRemainingDCLOutturnQuantityCacheForTesting();

			var outturn1 = underbond.Outturns.AddNew();
			outturn1.C5_PackagesOutturned = 20;
			outturn1.C5_LastMessageDate = ZDateTime.Now;
			outturn1.C5_OutturnResultType = CMROutturnResultType.Codes.NilDiscrepancy;
			outturn1.C5_ParentID = hawb1.PK;
			underbond.OutturnStatus.Code = CMRBaseStatuses.Codes.OriginalAccepted;
			Assert("One house not outturn yet, so is NOT complete", !mawb.IsDCLOutturnComplete);
			hawb1.ResetRemainingDCLOutturnQuantityCacheForTesting();
			hawb2.ResetRemainingDCLOutturnQuantityCacheForTesting();

			var outturn2 = underbond.Outturns.AddNew();
			outturn2.C5_PackagesOutturned = 0;
			outturn2.C5_LastMessageDate = ZDateTime.Now;
			outturn2.C5_OutturnResultType = CMROutturnResultType.Codes.ShortLanded;
			outturn2.C5_ParentID = hawb2.PK;
			Assert("One house is short, so is NOT incomplete", !mawb.IsDCLOutturnComplete);
			hawb1.ResetRemainingDCLOutturnQuantityCacheForTesting();
			hawb2.ResetRemainingDCLOutturnQuantityCacheForTesting();

			outturn2.C5_OutturnResultType = CMROutturnResultType.Codes.SurplusPackages;
			Assert("Is now incomplete", mawb.IsDCLOutturnComplete);
			hawb1.ResetRemainingDCLOutturnQuantityCacheForTesting();
			hawb2.ResetRemainingDCLOutturnQuantityCacheForTesting();
		}

		public void TestHumanReadableName()
		{
			var cusMAWB = Factory.New<CusMAWB>();
			AssertEquals("AirCargo Report", cusMAWB.HumanReadableName);
			cusMAWB.CM_MAWB = "08111111111";
			AssertEquals("AirCargo Report (MAWB: 081-11111111)", cusMAWB.HumanReadableName);
			cusMAWB.CM_MasterHouseBill = "X1234";
			AssertEquals("AirCargo Report (MAWB: 081-11111111 MHB: X1234)", cusMAWB.HumanReadableName);
			cusMAWB.CM_MAWB = "";
			AssertEquals("AirCargo Report (MHB: X1234)", cusMAWB.HumanReadableName);
		}

		public void TestIsValidFlagOnCusMAWB()
		{
			var cusMAWB = Factory.New<CusMAWB>();
			cusMAWB.CM_MAWB = "08111111111";
			cusMAWB.CM_MasterHouseBill = "X1234";
			cusMAWB.CM_RL_NKLoadPort = "GBLHR";
			cusMAWB.CM_RL_NKDischargePort = "AUSYD";
			cusMAWB.CM_FlightNo = "QF5";
			cusMAWB.CM_ArrivalDate = ZDateTime.Now;
			cusMAWB.RunPreSaveValidation();
			Factory.Save();
			var factory2 = new BusinessObjectFactory();
			factory2.RefreshEnabled = false;
			var cusMAWBinOtherFactory = factory2.Load<CusMAWB>(cusMAWB.PK);
			Assert("Should be valid", cusMAWBinOtherFactory.LightValidationIsValid);

			cusMAWB.CM_ArrivalDate = ZDateTime.Empty;
			cusMAWB.RunPreSaveValidation();
			Assert("Message error on date of arrival", cusMAWB.CM_ArrivalDateInfo.HasMessageErrors());
			Factory.Save();

			var factory3 = new BusinessObjectFactory();
			factory3.RefreshEnabled = false;
			var cusMAWBinAnotherFactory = factory3.Load<CusMAWB>(cusMAWB.PK);
			Assert("Should not be valid", !cusMAWBinAnotherFactory.LightValidationIsValid);
		}

		public void TestGetShipmentsNotReferenceByHAWB()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			consol.JK_RL_NKDischargePort = "AUSYD";
			var shipment1 = consol.Shipments.AddNew();
			var shipment2 = consol.Shipments.AddNew();
			var mawb = Factory.New<CusMAWB>();
			mawb.CM_JK = consol.PK;
			Factory.Save();
			var shipments = mawb.GetShipmentsNotReferenceByHAWB();
			AssertEquals(2, shipments.Length);
			AssertCollectionContains(shipment1, shipments);
			AssertCollectionContains(shipment2, shipments);

			var hawb = mawb.ChildBills.AddNew();
			Factory.Save();
			shipments = mawb.GetShipmentsNotReferenceByHAWB();
			AssertEquals(2, shipments.Length);
			AssertCollectionContains(shipment1, shipments);
			AssertCollectionContains(shipment2, shipments);

			hawb.CS_JS = shipment2.PK;
			Factory.Save();
			shipments = mawb.GetShipmentsNotReferenceByHAWB();
			AssertEquals(1, shipments.Length);
			AssertEquals(shipment1, shipments[0]);
		}

		public void TestIMessageManageableBizObj()
		{
			var mAWB = (CusMAWB)GetNewBusinessObject();
			Customs.Business.IMessageManageableBizObj mawb = mAWB;

			AssertEquals("MessageManager", typeof(CusMAWBMessageManager), mawb.GetMessageManagerForAmendmentDetection().GetType());
		}

		public void TestUsesTranshipmentPortOnUnderbond()
		{
			var mAWB = (CusMAWB)GetNewBusinessObject();
			mAWB.CM_RL_NKDischargePort = ZString.Empty;
			AssertEquals(true, ((ICusUnderbondDependentCollectionParent)mAWB).UsesTranshipmentPortOnUnderbond);
			AssertEquals(ZString.Empty, ((ICusUnderbondDependentCollectionParent)mAWB).DefaultTranshipmentPort);
			mAWB.CM_RL_NKDischargePort = "NZAKL";
			AssertEquals("NZAKL", ((ICusUnderbondDependentCollectionParent)mAWB).DefaultTranshipmentPort);

			var underbond = mAWB.Underbonds.AddNew();
			mAWB.CM_RL_NKDischargePort = "AUSYD";
			underbond.C4_MovementReason = CMRUnderbondRequestCodes.Codes.Transshipment;
			AssertEquals(ZString.Empty, underbond.C4_RL_NKTranshipDestPort);
			underbond.C4_MovementReason = CMRUnderbondRequestCodes.Codes.OtherMovement;
			AssertEquals(ZString.Empty, underbond.C4_RL_NKTranshipDestPort);
			mAWB.CM_RL_NKDischargePort = "NZAKL";
			underbond.C4_MovementReason = CMRUnderbondRequestCodes.Codes.Transshipment;
			AssertEquals("NZAKL", underbond.C4_RL_NKTranshipDestPort);
			underbond.C4_RL_NKTranshipDestPort = ZString.Empty;
			underbond.C4_MovementReason = CMRUnderbondRequestCodes.Codes.OtherMovement;
			AssertEquals(ZString.Empty, underbond.C4_RL_NKTranshipDestPort);
		}

		public void TestCM_OH_ResponsibleParty()
		{
			var mAWB = Factory.New<CusMAWB>();
			mAWB.CM_OH_ResponsibleParty = ZGuid.Empty;
			Assert("CM_ResponsiblePartyID should be empty", mAWB.CM_ResponsiblePartyID.IsEmpty);
			Assert("CM_ResponsiblePartyID should not be read only", !mAWB.CM_ResponsiblePartyIDInfo.ReadOnly);
			mAWB.CM_OH_ResponsibleParty = GlbCompany.CurrentCompany.OrgProxy.PK;
			AssertEquals("CM_ResponsiblePartyID should be OrgProxy's abn", GlbCompany.CurrentCompany.OrgProxy.PrimaryRegistrationNumber.Number.Replace(" ", ""), mAWB.CM_ResponsiblePartyID);
			Assert("CM_ResponsiblePartyID should be read only", mAWB.CM_ResponsiblePartyIDInfo.ReadOnly);
			var responsibleParty = Factory.New<OrgHeader>();
			responsibleParty.OH_FullName = "QANTAS CUCKOO SQUEAKERS";
			var code = responsibleParty.CustomsCodes.AddNew();
			code.OK_CodeType = OrgCusCode.CodeTypes.CustomsClientID;
			code.OK_CustomsRegNo = "8553P";
			mAWB.CM_OH_ResponsibleParty = responsibleParty.PK;
			AssertEquals("8553P", mAWB.CM_ResponsiblePartyID);
			code.OK_CustomsRegNo = "8553P--------->x";
			mAWB.CM_OH_ResponsibleParty = responsibleParty.PK;
			AssertEquals("8553P--------->", mAWB.CM_ResponsiblePartyID);
			code.OK_OA_PremisesAddress = Factory.New<OrgAddress>().PK;
			mAWB.CM_OH_ResponsibleParty = responsibleParty.PK;
			AssertEquals(ZString.Empty, mAWB.CM_ResponsiblePartyID);
		}

		public void TestTransport()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;

			var transport1 = consol.Transports[0];
			transport1.JW_VoyageFlight = "snth";

			var transport2 = consol.Transports.AddNew();
			transport2.JW_VoyageFlight = "aoeu";

			var mAWB = Factory.New<CusMAWB>();
			mAWB.CM_JK = consol.PK;

			mAWB.CM_FlightNo = "snth";
			AssertEquals(transport1, mAWB.Transport);

			mAWB.CM_FlightNo = "aoeu";
			AssertEquals(transport2, mAWB.Transport);

			mAWB.CM_FlightNo = "";
			AssertEquals(transport1, mAWB.Transport);
		}

		public void TestCanSendWithoutDelay()
		{
			var mAWB = Factory.New<CusMAWB>();
			Assert("Should be delayed because No CARSTS were found", !((ICusUnderbondDependentCollectionParent)mAWB).CanSendWithoutDelay);
			var message = Factory.New<CMRCARSTMessage>();
			message.EM_MessageType = CMRMessage.CMRMessageTypes.CARST;
			mAWB.Messages.Add(message);
			Assert("Should not be delayed due to carst.", ((ICusUnderbondDependentCollectionParent)mAWB).CanSendWithoutDelay);
		}

		[TestDate(2007, 03, 20, 10, 10, 0)]
		public void TestNilUnderbond()
		{
			var mAWB = Factory.New<CusMAWB>();

			var hAWB1 = mAWB.ChildBills.AddNew();
			hAWB1.CS_HAWB = "111";
			hAWB1.CS_GoodsDescription = "Cuckoo Squeakers";
			hAWB1.CS_PiecesManifested = 3;

			var hAWB2 = mAWB.ChildBills.AddNew();
			hAWB2.CS_HAWB = "222";
			hAWB2.CS_GoodsDescription = "Gibbiceps";
			hAWB2.CS_PiecesManifested = 6;

			var hAWB3 = mAWB.ChildBills.AddNew();
			hAWB3.CS_HAWB = "333";
			hAWB3.CS_GoodsDescription = "Ninjas";
			hAWB3.CS_PiecesManifested = 9;

			var underbond1 = mAWB.Underbonds.AddNew();
			var underbond2 = mAWB.Underbonds.AddNew();

			AssertEquals(0, underbond1.Outturns.Count);
			AssertEquals(0, underbond2.Outturns.Count);

			((ICusUnderbondNilUnderbondPerformer)mAWB).PerformNilUnderbond(underbond1);
			AssertEquals(3, underbond1.Outturns.Count);
			AssertEquals("HouseBill 111", underbond1.Outturns[0].ParentStringRepresentation);
			AssertEquals(3, underbond1.Outturns[0].C5_PackagesOutturned);
			AssertEquals(3, underbond1.Outturns[0].C5_OuterPacks);
			AssertEquals("Cuckoo Squeakers", underbond1.Outturns[0].C5_GoodsDescription);
			AssertEquals("C5_OutturnResultType should be NilDiscrepancy", CMROutturnResultType.Codes.NilDiscrepancy, underbond1.Outturns[0].C5_OutturnResultType);

			AssertEquals("HouseBill 222", underbond1.Outturns[1].ParentStringRepresentation);
			AssertEquals(6, underbond1.Outturns[1].C5_PackagesOutturned);
			AssertEquals(6, underbond1.Outturns[1].C5_OuterPacks);
			AssertEquals("Gibbiceps", underbond1.Outturns[1].C5_GoodsDescription);
			AssertEquals("C5_OutturnResultType should be NilDiscrepancy", CMROutturnResultType.Codes.NilDiscrepancy, underbond1.Outturns[1].C5_OutturnResultType);

			AssertEquals("HouseBill 333", underbond1.Outturns[2].ParentStringRepresentation);
			AssertEquals(9, underbond1.Outturns[2].C5_PackagesOutturned);
			AssertEquals(9, underbond1.Outturns[2].C5_OuterPacks);
			AssertEquals("Ninjas", underbond1.Outturns[2].C5_GoodsDescription);
			AssertEquals("C5_OutturnResultType should be NilDiscrepancy", CMROutturnResultType.Codes.NilDiscrepancy, underbond1.Outturns[2].C5_OutturnResultType);

			AssertEquals(0, underbond2.Outturns.Count);

			AssertEquals(new ZDateTime(2007, 03, 20, 10, 10, 0), underbond1.C4_Outurned);
			AssertEquals(ZDateTime.Empty, underbond2.C4_Outurned);
		}

		public void TestNilUnderbondWithExistingOutturnLine()
		{
			var mAWB = Factory.New<CusMAWB>();

			var hAWB1 = mAWB.ChildBills.AddNew();
			hAWB1.CS_HAWB = "111";
			hAWB1.CS_GoodsDescription = "Cuckoo Squeakers";
			hAWB1.CS_PiecesManifested = 3;

			var underbond1 = mAWB.Underbonds.AddNew();

			((ICusUnderbondNilUnderbondPerformer)mAWB).PerformNilUnderbond(underbond1);
			AssertEquals(1, underbond1.Outturns.Count);

			underbond1.Outturns[0].C5_PackagesOutturned = 2;
			underbond1.Outturns[0].C5_OutturnResultType = CMROutturnResultType.Codes.ShortLanded;

			var hAWB2 = mAWB.ChildBills.AddNew();
			hAWB2.CS_HAWB = "222";
			hAWB2.CS_GoodsDescription = "Gibbiceps";
			hAWB2.CS_PiecesManifested = 6;

			var hAWB3 = mAWB.ChildBills.AddNew();
			hAWB3.CS_HAWB = "333";
			hAWB3.CS_GoodsDescription = "Ninjas";
			hAWB3.CS_PiecesManifested = 9;

			((ICusUnderbondNilUnderbondPerformer)mAWB).PerformNilUnderbond(underbond1);

			AssertEquals(3, underbond1.Outturns.Count);
			AssertEquals("HouseBill 111", underbond1.Outturns[0].ParentStringRepresentation);
			AssertEquals(3, underbond1.Outturns[0].C5_OuterPacks);
			AssertEquals(2, underbond1.Outturns[0].C5_PackagesOutturned);
			AssertEquals(CMROutturnResultType.Codes.ShortLanded, underbond1.Outturns[0].C5_OutturnResultType);
			AssertEquals("Cuckoo Squeakers", underbond1.Outturns[0].C5_GoodsDescription);

			AssertEquals("HouseBill 222", underbond1.Outturns[1].ParentStringRepresentation);
			AssertEquals(6, underbond1.Outturns[1].C5_OuterPacks);
			AssertEquals(6, underbond1.Outturns[1].C5_PackagesOutturned);
			AssertEquals(CMROutturnResultType.Codes.NilDiscrepancy, underbond1.Outturns[1].C5_OutturnResultType);
			AssertEquals("Gibbiceps", underbond1.Outturns[1].C5_GoodsDescription);

			AssertEquals("HouseBill 333", underbond1.Outturns[2].ParentStringRepresentation);
			AssertEquals(9, underbond1.Outturns[2].C5_OuterPacks);
			AssertEquals(9, underbond1.Outturns[2].C5_PackagesOutturned);
			AssertEquals(CMROutturnResultType.Codes.NilDiscrepancy, underbond1.Outturns[2].C5_OutturnResultType);
			AssertEquals("Ninjas", underbond1.Outturns[2].C5_GoodsDescription);
		}

		[ExpectNoExceptions()]
		public void TestNilUnderbondWithDeleteObject()
		{
			var mAWB = Factory.New<CusMAWB>();
			var hAWB1 = mAWB.ChildBills.AddNew();
			hAWB1.CS_HAWB = "111";
			hAWB1.CS_GoodsDescription = "Cuckoo Squeakers";
			hAWB1.CS_PiecesManifested = 3;
			var underbond1 = mAWB.Underbonds.AddNew();
			underbond1.Delete();
			((ICusUnderbondNilUnderbondPerformer)mAWB).PerformNilUnderbond(underbond1);
		}

		public void TestUnderbondStatusOnMAWB()
		{
			var mAWB = Factory.New<CusMAWB>();
			Assert("Should default to empty", mAWB.UnderbondStatus.IsEmpty);
			var underbond = mAWB.Underbonds.AddNew();
			underbond.C4_ParentID = mAWB.PK;
			underbond.UnderbondStatus.Code = "WTO";
			mAWB.AllUnderbonds.Load();
			Factory.Save();
			AssertEquals("Awaiting Response to Original", mAWB.UnderbondStatus);
			var underbond2 = mAWB.Underbonds.AddNew();
			underbond2.C4_ParentID = mAWB.PK;
			mAWB.Underbonds.Add(underbond2);
			mAWB.AllUnderbonds.Load();
			Factory.Save();
			AssertEquals("More than one underbond", mAWB.UnderbondStatus);
		}

		public void TestStatusCalcualtorsDoNotRemoveDefaultStatusCusEntryNum()
		{
			GlbCompany.GetCurrentCompany(Factory).OrgProxy.Addresses[0].LocalControlledPremisesID = "9914N";
			var mAWB = Factory.New<CusMAWB>();
			var underbond = mAWB.Underbonds.AddNew();
			underbond.C4_DestinationPremiseID = "9914N";
			Factory.Save();
			AssertEquals("2 CusEntryStatus records created on saving", 2, underbond.CusEntryNumbers.Length);
			var cusEntryNumber1 = underbond.CusEntryNumbers[0];
			var cusEntryNumber2 = underbond.CusEntryNumbers[1];
			AssertEquals("Underbond status", CusEntryNumber.EntryType.UnderbondStatus, cusEntryNumber1.CE_EntryType);
			AssertEquals("Outturn status", CusEntryNumber.EntryType.OutturnStatus, cusEntryNumber2.CE_EntryType);
			var cusEntryNumber1PK = cusEntryNumber1.PK;
			var cusEntryNumber2PK = cusEntryNumber2.PK;
			Factory.Save();
			Assert("CusEntryNumber1 not deleted", !cusEntryNumber1.IsDeleted);
			Assert("CusEntryNumber2 not deleted", !cusEntryNumber2.IsDeleted);
			underbond.Calculator.DeriveStatusNow();
			underbond.OutturnStatusCalculator.DeriveStatusNow();
			Assert("CusEntryNumber1 still not deleted", !cusEntryNumber1.IsDeleted);
			Assert("CusEntryNumber2 still not deleted", !cusEntryNumber2.IsDeleted);
		}

		public void TestSynchroniserDoesNotOverride()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_MasterBillNum = "081";

			var transport = consol.Transports[0];
			transport.JW_VoyageFlight = "QF1";

			var mAWB = Factory.New<CusMAWB>();
			mAWB.CM_JK = consol.PK;
			mAWB.SynchroniseData();
			AssertEquals("081", mAWB.CM_MAWB);
			AssertEquals("QF1", mAWB.CM_FlightNo);
			mAWB.CM_MAWB = "081111111";
			mAWB.CM_FlightNo = "QF2";
			Factory.Save();
			AssertEquals("081111111", mAWB.CM_MAWB);
			AssertEquals("QF2", mAWB.CM_FlightNo);
		}

		public void TestDefaultValuesResponsibleParty()
		{
			var currentRegNo = GlbCompany.CurrentCompany.OrgProxy.PrimaryRegistrationNumber.Number;
			var currentCompany = Factory.Load<OrgHeader>(GlbCompany.CurrentCompany.OrgProxy.PK);

			try
			{
				currentCompany.PrimaryRegistrationNumber.Number = "21 003 980 130 123";
				Factory.Save();

				Assert("Pre-condition, registry value on by default", AUCustomsDataRegistry.Instance.DefaultAirConsolResponsibleParty.Value);
				AssertEquals("Pre-condition", "21 003 980 130 123", GlbCompany.CurrentCompany.OrgProxy.PrimaryRegistrationNumber.Number);

				var mAWB = Factory.New<CusMAWB>();

				Assert("Should default to false on a new MAWB.", !mAWB.ChildBillsIsLoaded);
				AssertEquals("Default Responsible Party", GlbCompany.CurrentCompany.OrgProxy.PK, mAWB.CM_OH_ResponsibleParty);
				AssertEquals("Default Responsible Party ID", "21003980130123", mAWB.CM_ResponsiblePartyID);

				using (AUCustomsDataRegistry.Instance.DefaultAirConsolResponsibleParty.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false))
				{
					var mAWB2 = Factory.New<CusMAWB>();
					AssertEquals("Should not default", ZGuid.Empty, mAWB2.CM_OH_ResponsibleParty);
				}
			}
			finally
			{
				currentCompany.PrimaryRegistrationNumber.Number = currentRegNo;
				Factory.Save();
			}
		}

		public void TestSetDefaultValuesFromMAWB()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_AgentType = Core.Constants.AgentType.Agent;
			consol.JK_MasterBillNum = "081002393248";
			consol.JK_AgentType = "AGT";
			consol.JK_BookingReference = "H90909";

			var ctoOrg = Factory.NewWithValidTestData<OrgHeader>();
			ctoOrg.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "3337C", Core.Constants.CountryCodes.Australia);

			var cfsOrg = Factory.NewWithValidTestData<OrgHeader>();
			cfsOrg.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "9912J", Core.Constants.CountryCodes.Australia);
			consol.JK_OA_ArrivalCTOAddress = ctoOrg.MainAddress.PK;

			var mAWB = Factory.New<CusMAWB>();
			mAWB.CM_JK = consol.PK;
			mAWB.CM_FlightNo = "QF123";
			mAWB.CM_OA_UnpackDepotAddress = cfsOrg.MainAddress.PK;
			var hAWB1 = mAWB.ChildBills.AddNew();
			hAWB1.CS_PiecesManifested = 125;
			hAWB1.CS_HAWB = "Squeaker";
			var hAWB2 = mAWB.ChildBills.AddNew();
			hAWB2.CS_PiecesManifested = 25;
			var hAWB3 = mAWB.ChildBills.AddNew();
			hAWB3.CS_PiecesManifested = 72;
			hAWB3.CS_MasterHouseBill = hAWB1.CS_HAWB;
			var testUnderbond = mAWB.Underbonds.AddNew();
			testUnderbond.C4_ParentID = mAWB.PK;
			testUnderbond.C4_ParentTableCode = "CM";
			testUnderbond.SetDefaultValuesFromParent();
			AssertEquals("QF123", testUnderbond.C4_FlightNo);
			AssertEquals(150u, testUnderbond.C4_PiecesManifested);
			AssertEquals("C4_OriginPremiseID defaults from Consol Arrival CTO controlled premise ID (CCP Code)", "3337C", testUnderbond.C4_OriginPremiseID);
			AssertEquals("C4_DestinationPremiseID defaults from CFS controlled premise ID (CCP Code)", "9912J", testUnderbond.C4_DestinationPremiseID);

			var emptyCfsOrg = Factory.NewWithValidTestData<OrgHeader>();
			mAWB.CM_OA_UnpackDepotAddress = emptyCfsOrg.MainAddress.PK;
			testUnderbond = mAWB.Underbonds.AddNew();
			testUnderbond.C4_ParentID = mAWB.PK;
			testUnderbond.SetDefaultValuesFromParent();
			AssertEquals("C4_DestinationPremiseID fallbacks to Company's Org Proxy when CCP code of CFS is absent", GlbCompany.CurrentCompany.OrgProxy.MainAddress.LocalControlledPremisesID, testUnderbond.C4_DestinationPremiseID);
		}

		public void TestSetDefaultValuesFromMAWBMaxLengthExceeded()
		{
			var mAWB = Factory.New<CusMAWB>();
			var hAWB1 = mAWB.ChildBills.AddNew();
			hAWB1.CS_PiecesManifested = 32000;
			var hAWB2 = mAWB.ChildBills.AddNew();
			hAWB2.CS_PiecesManifested = 32000;
			var testUnderbond = mAWB.Underbonds.AddNew();
			testUnderbond.C4_ParentID = mAWB.PK;
			testUnderbond.C4_ParentTableCode = "CM";
			testUnderbond.SetDefaultValuesFromParent();
			AssertEquals(0u, testUnderbond.C4_PiecesManifested);
		}

		public void TestReadOnlyDoesNotSetToTrueWhenLoaded()
		{
			var mAWB = Factory.New<CusMAWB>();
			mAWB.CM_HouseMessageIsSent = true;
			Factory.Save();

			var mAWBLoaded = Factory.Load<CusMAWB>(mAWB.PK);
			AssertEquals("MAWB loaded should not be readonly", false, mAWBLoaded.ReadOnly);

			mAWBLoaded.RegisterHouseBillsAsEditableChildren();
			AssertEquals("House bill collection should not be readonly either", false, mAWBLoaded.ChildBills.ReadOnly);
		}

		public void TestReadOnlyDoesntPropagate()
		{
			var mAWB = Factory.New<CusMAWB>();
			var hAWB = mAWB.ChildBills.AddNew();
			hAWB.ReadOnly = false;
			mAWB.SetReadOnly(true);
			mAWB.UnregisterHouseBillsAsEditableChildren();
			mAWB.RegisterHouseBillsAsEditableChildren();
			AssertEquals("Readonly should have propagated down to child", true, hAWB.ReadOnly);
			hAWB.ReadOnly = false;
			mAWB.SetReadOnly(false);
			mAWB.UnregisterHouseBillsAsEditableChildren();
			mAWB.RegisterHouseBillsAsEditableChildren();
			AssertEquals("Readonly should NOT have propagated down to child", false, hAWB.ReadOnly);
		}

		public void TestPropertyReadOnly()
		{
			var mAWB = Factory.New<CusMAWB>();
			AssertEquals("MAWB", false, mAWB.CM_MAWBInfo.ReadOnly);
			AssertEquals("Flight No", false, mAWB.CM_FlightNoInfo.ReadOnly);
			AssertEquals("Arrival date", false, mAWB.CM_ArrivalDateInfo.ReadOnly);
			AssertEquals("Load port", false, mAWB.CM_RL_NKLoadPortInfo.ReadOnly);
			AssertEquals("Discharge port", false, mAWB.CM_RL_NKDischargePortInfo.ReadOnly);
			AssertEquals("Master House", false, mAWB.CM_MasterHouseBillInfo.ReadOnly);

			mAWB.CM_HouseMessageIsSent = true;
			AssertEquals("MAWB", true, mAWB.CM_MAWBInfo.ReadOnly);
			AssertEquals("Flight No", true, mAWB.CM_FlightNoInfo.ReadOnly);
			AssertEquals("Arrival date", true, mAWB.CM_ArrivalDateInfo.ReadOnly);
			AssertEquals("Load port", true, mAWB.CM_RL_NKLoadPortInfo.ReadOnly);
			AssertEquals("Discharge port", true, mAWB.CM_RL_NKDischargePortInfo.ReadOnly);
			AssertEquals("Master House", true, mAWB.CM_MasterHouseBillInfo.ReadOnly);
		}

		public void TestSynchroniseDataForMasterHouse_ForCoLoadConsol()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_AgentType = Core.Constants.AgentType.CoLoad;
			AssertEquals("IsCoLoad: CLD", true, consol.IsCoLoad);

			consol.JK_CoLoadMasterBill = "H90909";

			var mAWB = Factory.New<CusMAWB>();
			mAWB.CM_JK = consol.PK;

			mAWB.SynchroniseData();
			AssertEquals("Master House number", "H90909", mAWB.CM_MasterHouseBill);

			consol.JK_AgentType = "AGT";
			AssertEquals("Is not CoLoad: AGT", false, consol.IsCoLoad);
			consol.JK_BookingReference = "H90909";
			mAWB.CM_MasterHouseBill = "";
			mAWB.SynchroniseData();
			AssertEquals("Master House number", "", mAWB.CM_MasterHouseBill);
			var org = Factory.New<OrgHeader>();
			consol.JK_OA_UnpackDepotAddress = org.MainAddress.PK;
			mAWB.SynchroniseData();
			AssertEquals(org.MainAddress.PK, mAWB.CM_OA_UnpackDepotAddress);
		}

		public void TestSynchroniseDataForMasterHouse_ForGatewayCoLoadConsol()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_AgentType = Core.Constants.AgentType.CoLoad;
			consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;
			AssertEquals("Consol should be Gateway Co-Load", true, consol.IsCoLoad && consol.IsSendingOrReceivingForwarderGateway);

			consol.JK_CoLoadMasterBill = "H90909";

			var mAWB = Factory.New<CusMAWB>();
			mAWB.CM_JK = consol.PK;

			mAWB.SynchroniseData();
			AssertEquals("Master House number", "H90909", mAWB.CM_MasterHouseBill);
		}

		public void TestICusMAWBProvider()
		{
			var mAWB = Factory.New<CusMAWB>();
			AssertEquals("ICusMAWBProvider", mAWB, ((ICusMAWBProvider)mAWB).MAWB);
		}

		[ExpectNoExceptions]
		public void TestDeletedMAWBNotSynchronised()
		{
			var consol = Factory.New<ForwardingConsol>();
			var mAWB = Factory.New<CusMAWB>();
			mAWB.CM_JK = consol.PK;

			mAWB.Delete();
			AssertEquals("Deleted", true, mAWB.IsDeleted);

			mAWB.SynchroniseData();
		}

		public void TestGetAllPossibleCollectionProviderss()
		{
			var masterBill = Factory.New<CusMAWB>();
			var houseBill = masterBill.ChildBills.AddNew();
			var partShip = houseBill.PartShips.AddNew();
			ICusUnderbondUnionCollectionParent masterBillUnion = masterBill;

			AssertNotNull("GetAllPossibleCollectionProviders should not return null", masterBillUnion.GetAllPossibleCollectionProviders());
			var blek = new ArrayList(masterBillUnion.GetAllPossibleCollectionProviders());
			AssertEquals("Length of AllPossibleCollectionProviders is 1", 1, blek.Count);
			Assert("The MasterBill should be in AllPossibleCollectionProviders", blek.Contains(masterBill));
		}

		public void TestDefaultCurrentBranchToDischargePort()
		{
			var masterBill = Factory.New<CusMAWB>();
			AssertEquals("Discharge port is defaulted", GlbBranch.CurrentBranch.GB_RL_NKHomePort, masterBill.CM_RL_NKDischargePort);
		}

		public void TestMessages()
		{
			var masterBill = Factory.New<CusMAWB>();
			AssertNotNull(masterBill.Messages);
		}

		public void TestDBMasterBillNumChanged()
		{
			var masterBill = Factory.New<CusMAWB>();
			masterBill.CM_MAWB = "08100000000";
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var loadedMAWB = newFactory.Load<CusMAWB>(masterBill.PK);
			Assert("MAWB still same", !loadedMAWB.MasterBillNumChanged);

			loadedMAWB.CM_MAWB = "08122222222";
			Assert("MAWB still different", loadedMAWB.MasterBillNumChanged);

			loadedMAWB.CM_MAWB = "08100000000";
			Assert("MAWB still same", !loadedMAWB.MasterBillNumChanged);
		}

		public void TestDeleteMAWBAfterDeletingChildren()
		{
			var masterBill = Factory.New<CusMAWB>();
			masterBill.CM_MAWB = "08100000000";
			var houseBill1 = masterBill.ChildBills.AddNew();
			var houseBill2 = masterBill.ChildBills.AddNew();
			Factory.Save();

			masterBill.Delete();
			Assert("Master bill deleted", masterBill.IsDeleted);
			Assert("House bill 1 deleted", houseBill1.IsDeleted);
			Assert("HouseBill 2 deleted", houseBill2.IsDeleted);
		}

		public void TestDontSynchroniseDataWhenHouseMessageIsSent()
		{
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "AUMEL";
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_MasterBillNum = "08100000000";
			consol.JK_RL_NKDischargePort = "AUSYD";
			var masterBill = Factory.New<CusMAWB>();
			masterBill.CM_JK = consol.PK;
			masterBill.CM_MAWB = "00000000081";
			var houseBill = masterBill.ChildBills.AddNew();
			houseBill.CS_MsgStatus = CMRBaseStatuses.Codes.OriginalAccepted;
			Factory.Save();
			masterBill.SynchroniseData();
			Assert("MAWB shouldnt be updated", masterBill.CM_MAWB != consol.JK_MasterBillNum);
			AssertEquals("AUMEL", masterBill.CM_RL_NKDischargePort);
		}

		public void TestFlagHouseMessageIsSent_DefaultValue()
		{
			var masterBill = Factory.New<CusMAWB>();
			Assert("CM_HouseMessageIsSent is set to false", !masterBill.CM_HouseMessageIsSent);

			masterBill.CM_HouseMessageIsSent = true;
			masterBill.CM_MAWB = "081-0000 0000";
			Factory.Save();

			var loadedMasterBill = Factory.Load<CusMAWB>(masterBill.PK);
			Assert("Loaded Master bill should have HouseMessageIsSent set to be true", masterBill.CM_HouseMessageIsSent);
		}

		public void TestMAWBReadOnly()
		{
			var masterBill = Factory.New<CusMAWB>();
			masterBill.CM_HouseMessageIsSent = true;
			masterBill.CM_MAWB = "081-0000 0000";
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var loadedMasterBill = newFactory.Load<CusMAWB>(masterBill.PK);
			Assert("LoadedMasterBill's HouseMessageIsSent", loadedMasterBill.CM_HouseMessageIsSent);

			Assert("MAWB readonly", loadedMasterBill.CM_MAWBInfo.ReadOnly);
			Assert("MAWB readonly", loadedMasterBill.CM_ArrivalDateInfo.ReadOnly);
			Assert("MAWB readonly", loadedMasterBill.CM_RL_NKDischargePortInfo.ReadOnly);
			Assert("MAWB readonly", loadedMasterBill.CM_RL_NKLoadPortInfo.ReadOnly);
			Assert("MAWB readonly", loadedMasterBill.CM_FlightNoInfo.ReadOnly);
		}

		public void TestHasMAWBChangesOnly()
		{
			var masterBill = Factory.New<CusMAWB>();
			var houseBill = masterBill.ChildBills.AddNew();
			houseBill.CS_HAWB = "Changed";
			Assert("HasMAWBChangesOnly doesnt take Children's HasChange", !masterBill.HasMAWBChangesOnly);

			masterBill.CM_MAWB = "MAWBChanged";
			Assert("HasMAWBChangesOnly MAWB Changed", masterBill.HasMAWBChangesOnly);
		}

		public void TestDescriptionOfMasterBill()
		{
			var masterBill = Factory.New<CusMAWB>();
			masterBill.CM_FlightNo = "QF1";
			masterBill.CM_ArrivalDate = new ZDateTime(2004, 2, 24);
			AssertEquals("Description of Master bill", "QF1 : 24-Feb-04", masterBill.CM_Description);
		}

		public void TestPortOfDischargeBeingDomestic()
		{
			var masterBill = (CusMAWB)GetNewBusinessObject();
			masterBill.CM_FlightNo = "QF1";
			masterBill.CM_RL_NKDischargePort = "NZAKL";
			Assert("The discharge port should be a domestic one", masterBill.CM_RL_NKDischargePortInfo.HasMessageErrors());

			masterBill.CM_RL_NKDischargePort = "AUSYD";
			Assert("The discharge port should be a domestic one", !masterBill.CM_RL_NKDischargePortInfo.HasMessageErrors());
		}

		public void TestCurrentHouseBillsLoadWhenNewCurrentHouseBillSet()
		{
			var masterBill = Factory.New<CusMAWB>();
			var houseBill1 = masterBill.ChildBills.AddNew();
			masterBill.CurrentHouseBill = houseBill1;
			AssertEquals("Current House Bills has one element", 1, masterBill.CurrentHouseBills.Count);
			AssertEquals("Current House Bills has House bill 1", houseBill1, masterBill.CurrentHouseBills[0]);

			var houseBill2 = masterBill.ChildBills.AddNew();
			masterBill.CurrentHouseBill = houseBill2;
			AssertEquals("Current House Bills has one element", 1, masterBill.CurrentHouseBills.Count);
			AssertEquals("Current House Bills has House bill 2", houseBill2, masterBill.CurrentHouseBills[0]);
		}

		public void TestCanDeleteFlag()
		{
			var masterBill = Factory.New<CusMAWB>();
			Assert("Certainly can delete", masterBill.CanDelete);
		}

		public void TestCannotDeleteAsHouseMessageIsSent()
		{
			var masterBill = Factory.New<CusMAWB>();
			masterBill.CM_HouseMessageIsSent = true;
			Assert("Cannot delete as there is a message sent for a house bill", !masterBill.CanDelete);
			AssertEquals("Cannot delete reason", "This Cargo Report Master may not be deleted because messages have been sent, or it has child bills for which messages have been sent.", masterBill.ReasonForNotAbleToDelete);
		}

		public void TestCreateNew()
		{
			var consol = Factory.New<ForwardingConsol>();
			AssertNotNull("CusMAWB", CusMAWB.CreateNew(consol));
		}

		public void TestLoad()
		{
			var consol = Factory.New<ForwardingConsol>();
			CusMAWB.CreateNew(consol);
			AssertNotNull(CusMAWB.Load(consol));
		}

		public void TestValidationObject()
		{
			var masterBill = Factory.New<CusMAWB>();
			AssertEquals("ValidationType", typeof(CMRCusMAWBValidation), masterBill.Validation.GetType());
		}

		public void TestDetails()
		{
			var masterBill = Factory.New<CusMAWB>();
			masterBill.CM_MAWB = "123";
			masterBill.CM_RL_NKLoadPort = "NZAKL";
			masterBill.CM_RL_NKDischargePort = "AUSYD";
			AssertEquals("Details", @"MASTER BILL DETAILS:
MAWB: 123
Load Port: NZAKL
Discharge Port: AUSYD
", masterBill.Details);
		}

		public override void TestIsStandAlone()
		{
			var mAWB = Factory.New<CusMAWB>();
			AssertEquals("IsStandAlone", true, mAWB.IsStandAlone);

			var consol = Factory.New<ForwardingConsol>();
			mAWB.CM_JK = consol.PK;
			AssertEquals("Plugged in", false, mAWB.IsStandAlone);
		}

		public void TestPiecesManifestedForAllHAWBs()
		{
			var mAWB = Factory.New<CusMAWB>();
			mAWB.ChildBills.AddNew().CS_PiecesManifested = 2;
			mAWB.ChildBills.AddNew().CS_PiecesManifested = 7;
			AssertEquals("PiecesManifestedForAllHAWBs", 9, mAWB.PiecesManifestedForAllHAWBs);
		}

		public void TestUnderbonds()
		{
			var masterBill = Factory.New<CusMAWB>();
			ICusUnderbondDependentCollectionParent masterBillUnder = masterBill;
			AssertNotNull(masterBillUnder.Underbonds);
			Assert(masterBill.IsRegisteredEditableChildObject(masterBillUnder.Underbonds));
		}

		public override void TestAllUnderbonds()
		{
			var masterBill = Factory.New<CusMAWB>();
			masterBill.ChildBills.AddNew();
			ICusUnderbondUnionCollectionParent masterBillUnder = masterBill;
			AssertNotNull(masterBillUnder.AllUnderbonds);
		}

		public void TestOutturnableLines()
		{
			var mAWB = Factory.New<CusMAWB>();
			var hAWB = mAWB.ChildBills.AddNew();
			var result = ((ICusUnderbondDependentCollectionParent)mAWB).OutturnableLines;
			AssertEquals("Length", 2, result.Length);
			AssertEquals("Result[0]", mAWB, result[0]);
			AssertEquals("Result[1]", hAWB, result[1]);
		}

		public void TestIAirOutturnReportHeaderInformationProvider_GetHeader()
		{
			var underbond = Factory.New<CusUnderbond>();
			var mAWB = Factory.New<CusMAWB>();
			AssertNotNull((mAWB as IAirOutturnReportHeaderInformationProvider).GetHeader(underbond));
		}

		public void TestIsBureau()
		{
			var underbond = Factory.New<CusUnderbond>();
			var masterBill = Factory.New<CusMAWB>();
			var header = ((IUnderbondMovementRequestHeaderProvider)masterBill).GetHeader(underbond);
			masterBill.CM_IsBureau = false;
			AssertEquals("IsBureau", false, header.IsBureau);
			masterBill.CM_IsBureau = true;
			AssertEquals("IsBureau", true, header.IsBureau);
		}

		public void TestCM_IsBureauDefaults()
		{
			var mAWB = Factory.New<CusMAWB>();
			mAWB.CM_IsBureau = true;
			var hAWB = mAWB.ChildBills.AddNew();
			hAWB.CS_HAWB = "BILL 1";
			var masterUnderbond = (CusUnderbond)((ICusUnderbondDependentCollectionParent)mAWB).Underbonds.AddNew();
			mAWB.AllUnderbonds.Load();
			AssertEquals("Master Underbond Default Is Bureau", true, masterUnderbond.C4_IsBureau);
			mAWB.CM_IsBureau = false;
			AssertEquals("Master Underbond MAWB Changed Is Bureau", false, masterUnderbond.C4_IsBureau);
			mAWB.CM_IsBureau = true;
			AssertEquals("Master Underbond MAWB Changed Is Bureau", true, masterUnderbond.C4_IsBureau);

			masterUnderbond.C4_MessageStatus = CMRBaseStatuses.Codes.OriginalAccepted;

			mAWB.CM_IsBureau = false;
			AssertEquals("Master Underbond MAWB Changed Is Bureau", true, masterUnderbond.C4_IsBureau);
		}

		public override void TestFilteredChildBills()
		{
			var mawb = Factory.New<CusMAWB>();

			var clearCount = 0;
			foreach (CodeDescriptionPair pair in new CMRConsolidatedCargoStatuses())
			{
				var hawb = mawb.ChildBills.AddNew();
				hawb.CS_CustomsStatus = pair.Code;
				if (CMRConsolidatedCargoStatuses.AllClearStatus.ContainsCode(pair.Code))
				{
					clearCount++;
				}
			}

			mawb.InvalidBillsOnlyFilter = true;
			mawb.FilteredChildBills.Rebuild();
			AssertEquals("All house bills should have errors", mawb.ChildBills.Count, mawb.FilteredChildBills.Count);

			mawb.InvalidBillsOnlyFilter = false;

			mawb.CustomsCargoStatusFilter = CMRConsolidatedCargoStatuses.Filter.Codes.NotClear;
			mawb.FilteredChildBills.Rebuild();
			AssertEquals("Not clear bills only", mawb.ChildBills.Count - clearCount, mawb.FilteredChildBills.Count);

			mawb.CustomsCargoStatusFilter = CMRConsolidatedCargoStatuses.Filter.Codes.ClearOrConditional;
			mawb.FilteredChildBills.Rebuild();
			AssertEquals("Clear or conditional bills only", 2, mawb.FilteredChildBills.Count);

			mawb.CustomsCargoStatusFilter = CMRConsolidatedCargoStatuses.Filter.Codes.HeldOrConditional;
			mawb.FilteredChildBills.Rebuild();
			AssertEquals("Held or conditional bills only", 2, mawb.FilteredChildBills.Count);
		}

		public void TestAreHouseBillsEditableChild()
		{
			var mAWB = Factory.New<CusMAWB>();

			AssertEquals("Business does not initiate Registration", true, mAWB.IsRegisteredEditableChildObject(mAWB.ChildBills));

			mAWB.UnregisterHouseBillsAsEditableChildren();
			AssertEquals("Unregistered", false, mAWB.IsRegisteredEditableChildObject(mAWB.ChildBills));

			mAWB.RegisterHouseBillsAsEditableChildren();
			AssertEquals("Registered", true, mAWB.IsRegisteredEditableChildObject(mAWB.ChildBills));
		}

		public void TestCanBeDeleted()
		{
			var mAWB = Factory.New<CusMAWB>();
			AssertEquals(true, mAWB.CanDelete);

			var house = mAWB.ChildBills.AddNew();
			AssertEquals(true, mAWB.CanDelete);

			house.CS_MsgStatus = ZString.Empty;
			AssertEquals(true, mAWB.CanDelete);

			house.CS_MsgStatus = CMRBaseStatuses.Codes.NotSent;
			AssertEquals(true, mAWB.CanDelete);

			house.CS_MsgStatus = CMRBaseStatuses.Codes.AwaitingResponseToOriginal;
			AssertEquals(false, mAWB.CanDelete);
		}

		public void TestABNSpacesStripped()
		{
			var masterBill = Factory.New<CusMAWB>();
			masterBill.CM_ResponsiblePartyID = ABNNumberWithSpaces;
			AssertEquals("Responsible Party ID", ABNNumberWithOutSpaces, masterBill.CM_ResponsiblePartyID);
		}

		public override void TestEffectiveResponsibleParty()
		{
			var orgHeader1 = Factory.New<OrgHeader>();
			var orgHeader2 = Factory.New<OrgHeader>();
			var orgCusCode2 = Factory.New<OrgCusCode>();
			orgCusCode2.OK_CodeType = OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber;
			orgCusCode2.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Australia;
			orgCusCode2.OK_CustomsRegNo = "41065894724";
			orgCusCode2.OK_OH = orgHeader2.PK;
			var mawb = Factory.New<CusMAWB>();

			AssertNull("Responsible party is not specified", mawb.EffectiveResponsiblePartyOrgHeader);
			AssertEquals("Responsible party is not specified", mawb.ReasonEffectiveResponsiblePartyIsUnavailable);

			mawb.CM_OH_ResponsibleParty = orgHeader1.PK;
			AssertEquals("Responsible party", orgHeader1, mawb.EffectiveResponsiblePartyOrgHeader);
			AssertEquals(ZString.Empty, mawb.ReasonEffectiveResponsiblePartyIsUnavailable);

			mawb.CM_OH_ResponsibleParty = ZGuid.Empty;
			mawb.CM_ResponsiblePartyID = "41065894724";
			AssertEquals("Responsible party", orgHeader2, mawb.EffectiveResponsiblePartyOrgHeader);
			AssertEquals(ZString.Empty, mawb.ReasonEffectiveResponsiblePartyIsUnavailable);

			var orgHeader3 = Factory.New<OrgHeader>();
			var orgCusCode3 = Factory.New<OrgCusCode>();
			orgCusCode3.OK_CodeType = OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber;
			orgCusCode3.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Australia;
			orgCusCode3.OK_CustomsRegNo = "41065894724";
			orgCusCode3.OK_OH = orgHeader3.PK;

			AssertNull("Ambiguous responsible party ID", mawb.EffectiveResponsiblePartyOrgHeader);
			AssertEquals("There are multiple organizations with the same ABN [41065894724]", mawb.ReasonEffectiveResponsiblePartyIsUnavailable);

			mawb.CM_ResponsiblePartyID = "XXXXXXXX";

			AssertNull("Incorrect responsible party ID", mawb.EffectiveResponsiblePartyOrgHeader);
			AssertEquals("Cannot find any organization with ABN [XXXXXXXX]", mawb.ReasonEffectiveResponsiblePartyIsUnavailable);
		}

		public void TestBulkAllocateReferenceNumbersForChildBills()
		{
			var mawb = Factory.New<CusMAWB>();

			var hawb1 = mawb.ChildBills.AddNew();
			var hawb2 = mawb.ChildBills.AddNew();

			var shipment = CommonShipment.New(Factory);
			hawb2.CS_JS = shipment.PK;

			mawb.BulkAllocateReferenceNumbersForChildBills();

			AssertNotNullOrEmpty(hawb1.CS_MessageReference);
			AssertNullOrEmpty(hawb2.CS_MessageReference);
		}

		public override void TestGetNewCusMAWBProcessTaskCollection()
		{
			var mawb = Factory.New<CusMAWB>();
			AssertType("WorkflowItems's type should be ProcessTaskCollection<CusMAWBProcessTask, CusMAWB>", typeof(ProcessTaskCollection<CusMAWBProcessTask, CusMAWB>), ((IWorkflowProvider)mawb).WorkflowItems);
		}

		public void TestJobHeaderIsNotDeactivatedWhenFactoryHasInvoicingPlugInGUIBusinessContext()
		{
			AssertJobHeaderDeactivation(true);
		}

		public void TestJobHeaderIsDeactivatedWhenFactoryDoesNotHaveInvoicingPlugInGUIBusinessContext()
		{
			AssertJobHeaderDeactivation(false);
		}

		public void TestAuditSecurity()
		{
			AssertEquals("AuditSecurity", Env.Security.ACAMasterImportAuditBilling, ((IJobInvoicingPlugIn)Factory.New<CusMAWB>()).InvoicingSupporter.AuditSecurity);
		}

		public void TestEditSecurity()
		{
			IJobInvoicingPlugIn testJob = Factory.New<CusMAWB>();
			AssertEquals("EditSecurityCheckPoint should be Empty", Env.Security.None, testJob.InvoicingSupporter.EditSecurityCheckpoint);
			AssertEquals("EditSecuritytMessage should be Empty", ZString.Empty, testJob.InvoicingSupporter.EditSecurityMessage);
			AssertEquals("EditSecurityLock should be False", ZBool.False, testJob.InvoicingSupporter.EditSecurityLock);
		}

		public void TestDefaultChargeGroup()
		{
			IJobInvoicingPlugIn testJob = Factory.New<CusMAWB>();
			AssertEquals("DefaultChargeGroup should be Empty", ZString.Empty, testJob.InvoicingSupporter.DefaultChargeGroup);
		}

		public void TestIJobHeaderParent_AllowInvoiceDeletion()
		{
			IJobHeaderParent cusMAWB = Factory.New<CusMAWB>();
			Assert(cusMAWB.AllowInvoiceDeletion);
		}

		protected override void TestBusinessObjectsWithRelatedEventsCore(CusMAWBBase mAWBBase)
		{
			var mAWB = mAWBBase as CusMAWB;

			AssertNotNull("MAWBBase should be a CusMAWB", mAWB);

			var origBizOWithRelatedLogCount = mAWB.BusinessObjectsWithRelatedEvents.Length;

			var underbond1 = (CusUnderbond)((ICusUnderbondDependentCollectionParent)mAWB).Underbonds.AddNew();
			var underbond2 = (CusUnderbond)((ICusUnderbondDependentCollectionParent)mAWB).Underbonds.AddNew();
			mAWB.AllUnderbonds.Load();

			var hB1 = mAWB.ChildBills.AddNew();
			var hB2 = mAWB.ChildBills.AddNew();

			AssertEquals("BusinessObjectWithRelatedLogs should contain the 2 additional Housebills", 4, mAWB.BusinessObjectsWithRelatedEvents.Length - origBizOWithRelatedLogCount);
			AssertCollectionContains("Housebill1 should be in BusinessObjectWithRelatedLogs", hB1, mAWB.BusinessObjectsWithRelatedEvents);
			AssertCollectionContains("Housebill2 should be in BusinessObjectWithRelatedLogs", hB2, mAWB.BusinessObjectsWithRelatedEvents);
			AssertCollectionContains("Underbond1 should be in BusinessObjectWithRelatedLogs", underbond1, mAWB.BusinessObjectsWithRelatedEvents);
			AssertCollectionContains("Underbond2 should be in BusinessObjectWithRelatedLogs", underbond2, mAWB.BusinessObjectsWithRelatedEvents);
		}

		void AssertJobHeaderDeactivation(bool hasInvoicingPlugInGUIContext)
		{
			var mawb = Factory.New<CusMAWB>();
			var jobLoader = new JobHeader.Loader(mawb);
			var job = jobLoader.TryCreate();
			Factory.Save();

			mawb.IsCancelled = true;
			if (hasInvoicingPlugInGUIContext)
			{
				mawb.SetContext(BusinessContext.InvoicingPlugInGUI);
			}

			var assertionMessage1 = string.Format("mawb {0} have InvoicingPluginGUI Business Context", hasInvoicingPlugInGUIContext ? "should" : "should not");
			var assertionMessage2 = string.Format("Job {0} be deactivated by OnSaving method", hasInvoicingPlugInGUIContext ? "should not" : "should");

			Assert("Deactivating mawb, IsCancelled flag should be set to true", mawb.IsCancelled);
			Assert("Deactivating mawb, IsCancelledInfo should have changes", mawb.IsCancelledHasChanged);
			AssertEquals(assertionMessage1, hasInvoicingPlugInGUIContext, mawb.HasContext(BusinessContext.InvoicingPlugInGUI));
			Assert("Job is not yet deactivated", !job.IsCancelled);

			Factory.Save();

			AssertEquals(assertionMessage2, !hasInvoicingPlugInGUIContext, job.IsCancelled);
		}

		const string ABNNumberWithSpaces = "75 006 687 958";
		const string ABNNumberWithOutSpaces = "75006687958";
	}
}
