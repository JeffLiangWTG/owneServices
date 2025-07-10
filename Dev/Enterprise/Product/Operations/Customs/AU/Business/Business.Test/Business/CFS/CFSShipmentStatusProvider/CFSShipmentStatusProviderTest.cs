using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Freight.CFS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Moq;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public class CFSShipmentStatusProviderTest : Freight.CFS.Business.Testing.CFSShipmentStatusProviderTest
	{
		#region InternalTests

		public void TestStaticNew()
		{
			var provider = new CFSShipmentStatusProvider(Shipment);
			AssertEquals(typeof(CFSShipmentStatusProvider), provider.GetType());
		}

		public void TestShipmentWrapper()
		{
			var provider = new CFSShipmentStatusProviderForTest(Shipment);
			AssertNotNull(provider.ShipmentWrapperExposed);
			AssertEquals(Shipment, provider.ShipmentWrapperExposed.Shipment);
			AssertEquals(typeof(CFSShipmentWrapper), provider.ShipmentWrapperExposed.GetType());
		}

		public void TestGatePassStatuses()
		{
			var provider = new CFSShipmentStatusProviderForTest(Shipment);
			AssertNotNull(provider.GatePassStatusesExposed);
			Assert(provider.GatePassStatusesExposed.ContainsCode(CMRUnderbondStatuses.Codes.ExpectedCargoArrivalAdviceReceived));
			Assert(provider.GatePassStatusesExposed.ContainsCode(CMRConsolidatedCargoStatuses.Codes.AcsseizedCargoIsSeizedByCustoms));
			Assert("GatePassStatuses should be cached", ReferenceEquals(CMRConsolidatedCargoAndUnderbondStatuses.GetStatuses(Shipment.Factory), provider.GatePassStatusesExposed));
		}

		public void TestOutturnMatchesShipment()
		{
			CusOutturnHeader outturnHeader = Factory.New<CusOutturnHeader>();
			DepotCusOutturn outturn = outturnHeader.Outturns.AddNew();

			outturn.C5_HouseBill = "foo";
			outturn.C5_CustomsStatus = "HLD";
			Shipment.JS_HouseBill = "foo";

			var provider = new CFSShipmentStatusProviderForTest(Shipment);
			AssertEquals(true, provider.OutturnMatchesShipmentExposed(outturn));

			outturn.C5_CustomsStatus = ZString.Empty;
			AssertEquals(false, provider.OutturnMatchesShipmentExposed(outturn));

			outturn.C5_CustomsStatus = "HLD";
			AssertEquals(true, provider.OutturnMatchesShipmentExposed(outturn));

			outturn.C5_HouseBill = "bar";
			AssertEquals(false, provider.OutturnMatchesShipmentExposed(outturn));

			outturn.C5_HouseBill = "foo";
			Shipment.JS_HouseBill = "bar";
			AssertEquals(false, provider.OutturnMatchesShipmentExposed(outturn));

			outturn.C5_HouseBill = "bar";
			AssertEquals(true, provider.OutturnMatchesShipmentExposed(outturn));
		}

		#endregion

		#region CanSaveAndPrint

		public void TestCanSaveAndPrint()
		{
			const string notClearMessage = "This shipment has not been cleared by customs.  Continue with Contingency Release?";
			const string acsSeizedMessage = "This shipment has been seized by customs and may not be gate passed.";
			const string aqisSeizedMessage = "This shipment has been seized by Quarantine and may not be gate passed";
			const string hrmWarningMessage = "This shipment is marked as high risk";
			const string condClearMessage = "Confirm conditional clearance actions have been completed.\r\nHave the conditional clearance requirements been met?";
			const string submovMessage = "This shipment is clear to be moved underbond but may not be delivered for home consumption";
			const string errorsMessage = "Please clear all errors before saving.";
			const string underbondMessage = "This shipment is clear to be moved underbond.";

			var provider = new CFSShipmentStatusProviderForTest(Shipment);
			AssertEquals("precondition", false, Shipment.HasErrors);

			var mock = new Mock<ISaveAndPrintUI>();

			provider.ShortStatusForTest = "";
			mock.Setup(m => m.Ask(notClearMessage)).Returns(false);
			AssertEquals(false, provider.CanSaveAndPrint(mock.Object));
			mock.VerifyAll();

			provider.ShortStatusForTest = "";
			mock.Setup(m => m.Ask(notClearMessage)).Returns(true);
			AssertEquals(true, provider.CanSaveAndPrint(mock.Object));
			mock.VerifyAll();

			provider.ShortStatusForTest = CMRConsolidatedCargoStatuses.Codes.AcsseizedCargoIsSeizedByCustoms;
			mock.Setup(m => m.ShowError(acsSeizedMessage));
			AssertEquals(false, provider.CanSaveAndPrint(mock.Object));
			mock.VerifyAll();

			provider.ShortStatusForTest = CMRConsolidatedCargoStatuses.Codes.AqisseizedCargoIsSeizedByQuarantine;
			mock.Setup(m => m.ShowError(aqisSeizedMessage));
			AssertEquals(false, provider.CanSaveAndPrint(mock.Object));
			mock.VerifyAll();

			provider.ShortStatusForTest = CMRConsolidatedCargoStatuses.Codes.ClearCargoIsFreeOfAnyImpedimentsAndMayBeReleased;
			AssertEquals(true, provider.CanSaveAndPrint(mock.Object));
			mock.VerifyAll();

			provider.ShortStatusForTest = CMRConsolidatedCargoStatuses.Codes.ClearhrmCargoIsClearButIsIdentifiedAsHighRiskMovement;
			mock.Setup(m => m.ShowWarning(hrmWarningMessage));
			AssertEquals(true, provider.CanSaveAndPrint(mock.Object));
			mock.VerifyAll();

			provider.ShortStatusForTest = CMRConsolidatedCargoStatuses.Codes.CondclearCargoCanBeReleasedIntoHomeConsumptionSubjectToConditionSTheseConditionsAreProvidedInSupplementaryInformation;
			mock.Setup(m => m.Ask(condClearMessage)).Returns(false);
			AssertEquals(false, provider.CanSaveAndPrint(mock.Object));
			mock.VerifyAll();

			provider.ShortStatusForTest = CMRConsolidatedCargoStatuses.Codes.CondclearCargoCanBeReleasedIntoHomeConsumptionSubjectToConditionSTheseConditionsAreProvidedInSupplementaryInformation;
			mock.Setup(m => m.Ask(condClearMessage)).Returns(true);
			AssertEquals(true, provider.CanSaveAndPrint(mock.Object));
			mock.VerifyAll();

			provider.ShortStatusForTest = CMRConsolidatedCargoStatuses.Codes.HeldCargoIsHeldUnderCustomsControl;
			mock.Setup(m => m.Ask(notClearMessage)).Returns(false);
			AssertEquals(false, provider.CanSaveAndPrint(mock.Object));
			mock.VerifyAll();

			provider.ShortStatusForTest = CMRConsolidatedCargoStatuses.Codes.HeldCargoIsHeldUnderCustomsControl;
			mock.Setup(m => m.Ask(notClearMessage)).Returns(true);
			AssertEquals(true, provider.CanSaveAndPrint(mock.Object));
			mock.VerifyAll();

			provider.ShortStatusForTest = CMRConsolidatedCargoStatuses.Codes.SububmovCargoCannotBeReleasedIntoHomeConsumptionButUnderbondMovementIsAllowed;
			mock.Setup(m => m.ShowWarning(submovMessage));
			AssertEquals(true, provider.CanSaveAndPrint(mock.Object));
			mock.VerifyAll();

			provider.ShortStatusForTest = CMRConsolidatedCargoStatuses.Codes.TranshipCargoIsForTranshipmentATranshipmentNumberWillBeGeneratedAndTransmittedWithStatus;
			AssertEquals(true, provider.CanSaveAndPrint(mock.Object));
			mock.VerifyAll();

			provider.ShortStatusForTest = CMRConsolidatedCargoStatuses.Codes.TranshphrmTranshipmentCargoIsClearButIsIdentifiedAsHighRiskMovement;
			mock.Setup(m => m.ShowWarning(hrmWarningMessage));
			AssertEquals(true, provider.CanSaveAndPrint(mock.Object));
			mock.VerifyAll();

			provider.ShortStatusForTest = CMRConsolidatedCargoStatuses.Codes.TransitCargoIsTransitCargoThisValueWillOnlyBeViewableFromAnInteractiveFunction;
			AssertEquals(true, provider.CanSaveAndPrint(mock.Object));
			mock.VerifyAll();

			provider.ShortStatusForTest = CMRConsolidatedCargoStatuses.Codes.WithdrawnCargoReportHadBeenWithdrawn;
			mock.Setup(m => m.Ask(notClearMessage)).Returns(false);
			AssertEquals(false, provider.CanSaveAndPrint(mock.Object));
			mock.VerifyAll();

			provider.ShortStatusForTest = CMRConsolidatedCargoStatuses.Codes.WithdrawnCargoReportHadBeenWithdrawn;
			mock.Setup(m => m.Ask(notClearMessage)).Returns(true);
			AssertEquals(true, provider.CanSaveAndPrint(mock.Object));
			mock.VerifyAll();

			provider.ShortStatusForTest = CMRUnderbondStatuses.Codes.UnderbondApprovalAdviceReceived;
			mock.Setup(m => m.ShowWarning(underbondMessage));
			AssertEquals(true, provider.CanSaveAndPrint(mock.Object));
			mock.VerifyAll();

			provider.ShortStatusForTest = CMRConsolidatedCargoStatuses.Codes.ClearCargoIsFreeOfAnyImpedimentsAndMayBeReleased;
			Shipment.RunPreSaveValidation();
			AssertEquals("precondition: shipment has errors", true, Shipment.HasErrors);
			mock.Setup(m => m.ShowError(errorsMessage));
			AssertEquals(false, provider.CanSaveAndPrint(mock.Object));
			mock.VerifyAll();
		}

		#endregion

		#region Status

		public void TestStatusForStandAloneAirFreight()
		{
			const string mawbNum = "15798113606";
			const string hawbNum = "AIR-4072016";
			const string msgFlightNum = "PW407";
			const string consolFlightNum = "PW0417";

			Shipment.JS_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			Shipment.JS_HouseBill = hawbNum;
			CFSLoadListConsol consol = Shipment.Consols.AddNew();
			consol.JK_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			consol.JK_MasterBillNum = mawbNum;
			consol.MainTransport.JW_ETA = new ZDateTime(2016, 07, 04, 11, 11, 0);
			consol.MainTransport.JW_VoyageFlight = consolFlightNum;

			var masterBill = Factory.New<CusMAWB>();
			masterBill.CM_MAWB = mawbNum;
			var houseBill = masterBill.ChildBills.AddNew();
			houseBill.CS_JS = Shipment.PK;
			houseBill.CS_HAWB = hawbNum;

			var underbond = Factory.New<CusUnderbond>();
			underbond.C4_ArrivalDate = new ZDateTime(2016, 07, 08, 15, 35, 0);
			underbond.C4_MAWB = mawbNum;
			underbond.C4_FlightNo = msgFlightNum;
			var outturn = Factory.New<DepotCusOutturn>();
			outturn.C5_C4_Underbond = underbond.PK;
			outturn.C5_HouseBill = hawbNum;

			string messageText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+34:::CARST+410B CHHG 4AG:1+8'
DTM+9:20160707142509685351:ZZZ'
DTM+132:20160707:102'
FTX+AHN+++CONSOLIDATED STATUS:HELD'
FTX+AHN+++CARGO REPORT SAC:NO'
TDT+20+407++6+PW::3'
LOC+12+AUSYD::6'
LOC+4+9914N::95'
NAD+MR+AAA374M::95'
NAD+UD+41065894724::95'
RFF+ABO:2I1E B8FI EAG::1'
RFF+MWB:15798113606'
RFF+HWB:AIR-4072016'
DOC+1
PAC+0000001
UNT+17+000001'".Replace("\r\n", "");

			var message = Factory.New<CMRCARSTMessage>();
			message.EM_MessageText = messageText;
			message.EM_LinkUniqueID = outturn.PK;
			message.EM_LinkTable = CusOutturn.Schema.TableName;
			Factory.Save();

			var gatePass = Factory.Load<GatePassShipment>(Shipment.PK);
			AssertEquals("Confirm status comes from Outturn as housebill status will be NOT", CMRConsolidatedCargoStatuses.Descriptions.HeldCargoIsHeldUnderCustomsControl, gatePass.JS_GatePassStatus);
			AssertEquals("NOT", houseBill.CS_CustomsStatus);

			houseBill.CS_CustomsStatus = ZString.Empty;
			AssertEquals("Confirm status comes from Outturn as housebill status is empty", CMRConsolidatedCargoStatuses.Descriptions.HeldCargoIsHeldUnderCustomsControl, gatePass.JS_GatePassStatus);

			houseBill.CS_CustomsStatus = CMRConsolidatedCargoStatuses.Codes.ClearCargoIsFreeOfAnyImpedimentsAndMayBeReleased;
			AssertEquals("Confirm Status comes from Housebill and does not fallback to outturn", CMRConsolidatedCargoStatuses.Descriptions.ClearCargoIsFreeOfAnyImpedimentsAndMayBeReleased, gatePass.JS_GatePassStatus);
		}

		public void TestStatus()
		{
			#region Init

			var orgProxy = Factory.Load<OrgHeader>(GlbCompany.CurrentCompany.OrgProxy.PK);
			orgProxy.MainAddress.LocalControlledPremisesID = "1111A";
			orgProxy.OH_IsUnpackDepot = true;

			Factory.Save();

			#endregion

			#region FCL Expected Arrival Message

			string fCLExpectedArrivalMessageText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+961:::UBMREQR+2C4G E33F GJ0F:1+32'
FTX+AAH+++CJM436P20191'
TDT+20+222++11++++7654321::11'
TDT+1++ROA'
LOC+5+9999B::95'
LOC+4+1111A::95'
NAD+MR+CJM436P::95'
RFF+ANX:EXPECTED CARGO ARRIVAL ADVICE'
RFF+ACD:MOV'
DOC+1'
PAC+++FCL:67:95'
PAC+0000020++CT:185:95'
RFF+AAQ:AAAA1111117'
UNT+15+000001'".Replace("\r\n", "");

			CMRUBMREQRMessage message = Factory.New<CMRUBMREQRMessage>();
			message.EM_MessageText = fCLExpectedArrivalMessageText;
			message.SetEM_LinkedObject();

			#endregion

			#region Verifying Outturn Header and Outturn have been created correctly

			ZQuery filter = new ZQuery(CusOutturnHeaderSchema.C6_VoyageNum, "222");
			filter.AddToFilter(CusOutturnHeaderSchema.C6_OutturningPremiseID, "1111A");
			filter.AddToFilter(CusOutturnHeaderSchema.C6_LloydsIMO, "7654321");
			CusOutturnHeader[] headers = (CusOutturnHeader[])Factory.Load(typeof(CusOutturnHeader), filter);
			AssertEquals("Only one header found.", 1, headers.Length);

			CusOutturnHeader header = headers[0];

			AssertNotNull("CusOutturnHeader has been created.");
			AssertEquals("Only one Outturn created.", 1, header.Outturns.Count);

			DepotCusOutturn fCLOutturn = header.Outturns[0];
			AssertEquals("Container number on outturn is correct", "AAAA1111117", fCLOutturn.C5_ContainerNumber);
			AssertEquals("Customs status is blank.", ZString.Empty, fCLOutturn.C5_CustomsStatus);

			#endregion

			#region Creating a CFS load list and container for the outturn line

			CFSContainerCreator fCLCreator = new CFSContainerCreator(fCLOutturn, Factory);
			CFSLoadListConsol consol = fCLCreator.Consol;

			OrgHeader forwarder = GetNewForwarderOrg();
			consol.JK_OH_Forwarder = forwarder.PK;
			AssertEquals("Consol always has one transport", 1, consol.Transports.Count);
			Freight.Business.Transport transport = consol.Transports.MostInterestingTransport;
			transport.JW_ETA = ZDateTime.Now;
			consol.JK_RL_NKLoadPort = "NZAKL";
			consol.JK_RL_NKDischargePort = "AUSYD";

			CFSContainer container = fCLCreator.Container;
			container.JC_OH_CFSClient = forwarder.PK;
			container.JC_RC = RefContainer.New(Factory).PK;

			AssertEquals("only one container on the consol", 1, consol.Containers.Count);
			AssertEquals("Container on consol is the one we have.", container, consol.Containers[0]);

			consol.RunPreSaveValidation();
			AssertEquals(false, consol.HasErrors);

			#endregion

			#region LCL CARST Status Message

			string lCLCARSTMessageText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+34:::CARST+3JFD 9FB6 D7B5:1+8'
DTM+9:20050726162613590090:ZZZ'
FTX+AHN+++CONSOLIDATED STATUS:HELD'
FTX+AHN+++DEPARTURE FROM LAST OVERSEAS PORT:YES'
FTX+AHN+++QUOTED MASTER / OCEAN BILL EXISTS:YES'
FTX+AHN+++IAR ACS CLEARED:YES'
FTX+AHN+++COMPLETE UNDERBOND SERIES APPROVED:YES'
FTX+AHN+++LCL UNDERBOND SATISFIED:YES'
FTX+AHN+++CARGO NOT A CONSOLIDATION:YES'
FTX+AHN+++RELEASE PREMISE IN DESTINATION:YES'
FTX+AHN+++CARGO REPORT ACS EVALUATED:YES'
FTX+AHN+++IAR AQIS CLEARED:YES'
FTX+AHN+++CARGO REPORT AQIS EVALUATED:YES'
FTX+AHN+++IMPORT DECLARATIONS MATCHED:N/A'
FTX+AHN+++IMPORT DECLARATION ACS EVALUATED:N/A'
FTX+AHN+++IMPORT DECLARATION AQIS EVALUATED:N/A'
FTX+AHN+++ACS EVALUATION COMPLETE:YES'
FTX+AHN+++AQIS CARGO REPORT EVALUATION COMPLETE:YES'
FTX+AHN+++ACS IMPORT DECLARATION EVALUATION COMPLETE:N/A'
FTX+AHN+++AQIS IMPORT DECLARATION EVALUATION COMPLETE:N/A'
FTX+AHN+++IMPORT DECLARATION PAID:N/A'
FTX+AHN+++CARGO REPORT SAC:NO'
TDT+20+222++11++++7654321::11'
LOC+12+AUSYD::6'
LOC+4+1111A::95'
NAD+MR+AAA374M::95'
NAD+UD+41065894724::95'
RFF+MB:OB5000'
RFF+BH:HB4000'
RFF+AAQ:AAAA1111117'
DOC+1'
PAC+++LCL:67:95'
PAC+123'
FTX+AAA+++GOODS LINE DESCRIPTION'
FTX+MKS+++GOODS LINE MARKS'
UNT+37+000001'".Replace("\r\n", "");

			CMRCARSTMessage lCLCARSTMessage = Factory.New<CMRCARSTMessage>();
			lCLCARSTMessage.EM_MessageText = lCLCARSTMessageText;
			lCLCARSTMessage.SetEM_LinkedObject();

			#endregion

			#region Verifying new Outturn and Load List Lines created correctly

			header.Outturns.Load();
			AssertEquals("Second outturn has been created.", 2, header.Outturns.Count);

			DepotCusOutturn lCLOutturn = header.Outturns[1];
			if (lCLOutturn == fCLOutturn)
			{
				lCLOutturn = header.Outturns[0]; // make sure we get the LCL one.
			}

			Assert("Container mode is LCL", lCLOutturn.IsLCL);
			AssertEquals("Container number", "AAAA1111117", lCLOutturn.C5_ContainerNumber);
			AssertEquals("Ocean bill number", "OB5000", lCLOutturn.C5_MasterBill);
			AssertEquals("House bill number", "HB4000", lCLOutturn.C5_HouseBill);
			AssertEquals("Status is HELD", "HLD", lCLOutturn.CombinedStatus.Code);
			AssertEquals("Description", "GOODS LINE DESCRIPTION", lCLOutturn.C5_GoodsDescription);
			AssertEquals("Marks", "GOODS LINE MARKS", lCLOutturn.C5_MarksAndNumbers);

			var shipment = lCLOutturn.Shipment;
			AssertNotNull("CFS Shipment created", shipment);
			AssertEquals("Shipnment packs", 123, shipment.JS_OuterPacks);
			AssertEquals("Shipnment pack type", "PKG", shipment.JS_F3_NKPackType);
			AssertEquals("Shipnment mode", "LCL", shipment.JS_PackingMode);
			AssertEquals("Shipnment house bill", "HB4000", shipment.JS_HouseBill);
			AssertEquals("Shipnment goods description", "GOODS LINE DESCRIPTION", shipment.JS_GoodsDescription);
			AssertEquals("Shipnment marks", "GOODS LINE MARKS", shipment.JS_MarksAndNumbers);

			#endregion

			#region Random Additional Checks

			//AssertEquals(0, Header.Underbonds.Count); // Should we link the things to the header?
			AssertNotNull(lCLOutturn.Underbond);
			AssertEquals(1, lCLOutturn.Underbond.Outturns.Count);
			AssertEquals("JS", lCLOutturn.Underbond.C4_ParentTableCode);
			AssertNotNull(fCLOutturn.Underbond);
			AssertEquals(1, lCLOutturn.Underbond.Outturns.Count);
			AssertEquals("JC", fCLOutturn.Underbond.C4_ParentTableCode);
			Assert(lCLOutturn != fCLOutturn);

			AssertEquals(typeof(CFSContainerWrapper), fCLOutturn.Parent.GetType());
			AssertEquals(typeof(CFSShipmentWrapper), lCLOutturn.Parent.GetType());

			#endregion

			#region Creating a CFS Shipment from the LCL line

			CFSShipmentCreator lCLCreator = new CFSShipmentCreator(lCLOutturn, consol, Factory);
			CFSShipment shipment1 = lCLCreator.Shipment;

			OrgHeader consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_IsConsignor = true;
			consignor.OH_Code = "CONSIGNOR";

			OrgHeader consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_Code = "CONSE";
			consignee.OH_IsConsignee = true;

			shipment1.JS_GoodsDescription = "Stuff";
			container.RefContainer.RC_Code = "foo";
			container.RefContainer.RC_ContainerType = "bar";
			shipment1.ConsigneePK = consignee.PK;
			shipment1.ConsignorPK = consignor.PK;

			AssertEquals("Containers on consol", 1, consol.Containers.Count);
			AssertEquals("Containers on shipment", 1, shipment1.Containers.Count());
			AssertEquals("Same container on shipment and consol", consol.Containers[0], shipment1.Containers.First());
			AssertEquals("Same container as our container", container, consol.Containers[0]);

			consol.RunPreSaveValidation();
			AssertEquals(false, consol.HasErrors);

			#endregion

			#region Checking status on the Gate Pass Shipment

			Factory.Save();

			AssertEquals("HLD", lCLOutturn.C5_CustomsStatus);
			AssertEquals("HLD", lCLOutturn.CombinedStatus.Code);

			var gatePass = Factory.Load<GatePassShipment>(shipment1.PK);

			AssertEquals(gatePass.PK, lCLOutturn.C5_ParentID);
			AssertEquals(JobShipmentSchema.Constants.Prefix, lCLOutturn.C5_ParentTableCode);

			AssertEquals("HLD", gatePass.JS_GatePassStatusShort);

			#endregion

			#region LCL Underbond Approval Message

			string lCLUnderbondApprovalMessageText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+961:::UBMREQR+2C4G E33F GJ0F:1+32'
FTX+AAH+++CJM436P20191'
TDT+20+222++11++++7654321::11'
TDT+1++ROA'
LOC+5+1111A::95'
LOC+4+2222O::95'
NAD+MR+CJM436P::95'
RFF+ANX:UNDERBOND APPROVAL'
RFF+ACD:MOV'
DOC+1'
PAC+++LCL:67:95'
PAC+0000020++CT:185:95'
RFF+AAQ:AAAA1111117'
RFF+MB:OB5000'
RFF+BH:HB4000'
UNT+17+000001'".Replace("\r\n", "");

			CMRUBMREQRMessage lCLUnderbondApprovalMessage = Factory.New<CMRUBMREQRMessage>();
			lCLUnderbondApprovalMessage.EM_MessageText = lCLUnderbondApprovalMessageText;
			lCLUnderbondApprovalMessage.SetEM_LinkedObject();

			#endregion

			#region UserFriendlyStatus

			ZString expectedResult = @"CONSOLIDATED STATUS : HELD
IMPORT DECLARATIONS MATCHED : N/A
IMPORT DECLARATION ACS EVALUATED : N/A
IMPORT DECLARATION AQIS EVALUATED : N/A
ACS IMPORT DECLARATION EVALUATION COMPLETE : N/A
AQIS IMPORT DECLARATION EVALUATION COMPLETE : N/A
IMPORT DECLARATION PAID : N/A
CARGO REPORT SAC : NO

========================================
Warning: Cargo is not a consolidation.
========================================";
			AssertMultilineASCIIEquals("Details should come from CARST Message", expectedResult, gatePass.UserFriendlyStatus);

			#endregion

			#region Verifying Underbond Approval matches correctly against the line

			lCLOutturn.Messages.Load();
			AssertEquals("LCL Outturn has two messages", 2, lCLOutturn.Messages.Count);
			lCLOutturn.StatusCalculator.DeriveStatusNow();
			lCLOutturn.MessageStatusCalculator.DeriveStatusNow();
			AssertEquals("APP", lCLOutturn.CombinedStatus.Code);

			#endregion

			#region Checking the new status on the gate pass shipment

			AssertEquals("APP", gatePass.JS_GatePassStatusShort);

			#endregion

			#region Another LCL CARST Status Message

			string lCLCARSTMessageText2 = @"UNH+000001+CUSRES:D:99B:UN'
BGM+34:::CARST+3JFD 9FB6 D7B5:1+8'
DTM+9:20050726162613590090:ZZZ'
FTX+AHN+++CONSOLIDATED STATUS:CLEAR'
FTX+AHN+++CARGO REPORT SAC:NO'
TDT+20+222++11++++7654321::11'
LOC+12+AUSYD::6'
LOC+4+1111A::95'
NAD+MR+AAA374M::95'
NAD+UD+41065894724::95'
RFF+MB:OB5000'
RFF+BH:HB4000'
RFF+AAQ:AAAA1111117'
DOC+1'
PAC+++LCL:67:95'
PAC+123'
FTX+AAA+++GOODS LINE DESCRIPTION AMENDED'
FTX+MKS+++GOODS LINE MARKS AMENDED'
UNT+19+000001'".Replace("\r\n", "");

			shipment.JS_GoodsDescription = ZString.Empty;
			shipment.JS_MarksAndNumbers = ZString.Empty;

			var lCLCARSTMessage2 = Factory.New<CMRCARSTMessage>();
			lCLCARSTMessage2.EM_MessageText = lCLCARSTMessageText2;
			lCLCARSTMessage2.SetEM_LinkedObject();

			#endregion

			#region Verifying amended details on Load List Line

			AssertEquals("Shipnment goods description", "GOODS LINE DESCRIPTION AMENDED", shipment.JS_GoodsDescription);
			AssertEquals("Shipnment marks", "GOODS LINE MARKS AMENDED", shipment.JS_MarksAndNumbers);

			#endregion

			#region An LCL SEI Message

			string sEIMessage = @"UNH+000001+CUSRES:D:99B:UN'
BGM+961:::SEI+1G79 7IAF 71F9:1++11'
DTM+9:20090317154645464587:ZZZ'
TDT+20+222++11++++7654321::11'
TDT+1++ROA'
NAD+MR+AAA374M::95'
NAD+VW+41065894724::95'
RFF+ABO:O00000007/DAT8::8'
DOC+1'
PAC+++LCL:67:95'
PAC+30++BX:185:95'
RFF+MB:OB5000'
RFF+BH:HB4000'
RFF+AAQ:AAAA1111117'
PCI+28+MARKS1'
FTX+AAA+++GOODS DESCRIPTION AMENDED2'
MEA+AAE+G+KG:0000000007000.00'
MEA+AAE+AAL+KG:0000000007001.00'
MEA+AAE+ABJ+CU:0000000000007.00'
NAD+CN++CONSIGNEE 2'
UNT+21+000001'".Replace("O00000007", header.C6_SendersMessageReference).Replace("\r\n", "");

			shipment.JS_GoodsDescription = ZString.Empty;
			shipment.JS_MarksAndNumbers = ZString.Empty;

			var interchange = Factory.New<EDIInterchange>();
			var lCLSEIMessage = Factory.New<CMRSEIMessage>();
			lCLSEIMessage.EM_EI = interchange.PK;
			lCLSEIMessage.EM_MessageText = sEIMessage;
			lCLSEIMessage.SetEM_LinkedObject();

			#endregion

			#region Verifying amended details on Load List Line

			AssertEquals("Shipnment goods description", "GOODS DESCRIPTION AMENDED2", shipment.JS_GoodsDescription);
			AssertEquals("Shipnment marks", "MARKS1", shipment.JS_MarksAndNumbers);
			AssertEquals("Shipnment weight", 7000m, shipment.JS_ActualWeight);
			AssertEquals("Shipnment weight units", "KG", shipment.JS_UnitOfWeight);
			AssertEquals("Shipnment volume", 7m, shipment.JS_ActualVolume);
			AssertEquals("Shipnment volume units", "M3", shipment.JS_UnitOfVolume);

			#endregion
		}

		public void TestGatePassStatusForAirShipment()
		{
			const string mawbNum = "08177770000";
			const string hawbNum = "H1234";
			const string msgFlightNum = "QF2";
			const string consolFlightNum = "QF02";

			Shipment.JS_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			Shipment.JS_HouseBill = hawbNum;
			CFSLoadListConsol consol = Shipment.Consols.AddNew();
			consol.JK_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			consol.JK_MasterBillNum = mawbNum;
			consol.MainTransport.JW_ETA = new ZDateTime(2010, 3, 20, 11, 11, 0);
			consol.MainTransport.JW_VoyageFlight = consolFlightNum;

			CusUnderbond underbond = Factory.New<CusUnderbond>();
			underbond.C4_ArrivalDate = new ZDateTime(2010, 3, 20, 9, 35, 0);
			underbond.C4_MAWB = mawbNum;
			underbond.C4_FlightNo = msgFlightNum;
			DepotCusOutturn outturn = Factory.New<DepotCusOutturn>();
			outturn.C5_C4_Underbond = underbond.PK;
			outturn.C5_HouseBill = hawbNum;

			string messageText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+34:::CARST+3JFD 9FB6 D7B5:1+8'
DTM+9:20100320162613590090:ZZZ'
FTX+AHN+++CONSOLIDATED STATUS:HELD'
FTX+AHN+++DEPARTURE FROM LAST OVERSEAS PORT:YES'
FTX+AHN+++QUOTED MASTER / OCEAN BILL EXISTS:YES'
FTX+AHN+++IAR ACS CLEARED:YES'
FTX+AHN+++COMPLETE UNDERBOND SERIES APPROVED:YES'
FTX+AHN+++LCL UNDERBOND SATISFIED:YES'
FTX+AHN+++CARGO NOT A CONSOLIDATION:YES'
FTX+AHN+++RELEASE PREMISE IN DESTINATION:YES'
FTX+AHN+++CARGO REPORT ACS EVALUATED:YES'
FTX+AHN+++IAR AQIS CLEARED:YES'
FTX+AHN+++CARGO REPORT AQIS EVALUATED:YES'
FTX+AHN+++IMPORT DECLARATIONS MATCHED:N/A'
FTX+AHN+++IMPORT DECLARATION ACS EVALUATED:N/A'
FTX+AHN+++IMPORT DECLARATION AQIS EVALUATED:N/A'
FTX+AHN+++ACS EVALUATION COMPLETE:YES'
FTX+AHN+++AQIS CARGO REPORT EVALUATION COMPLETE:YES'
FTX+AHN+++ACS IMPORT DECLARATION EVALUATION COMPLETE:N/A'
FTX+AHN+++AQIS IMPORT DECLARATION EVALUATION COMPLETE:N/A'
FTX+AHN+++IMPORT DECLARATION PAID:N/A'
FTX+AHN+++CARGO REPORT SAC:NO'
TDT+20+2++6+QF::3'
LOC+12+AUSYD::6'
LOC+4+9920A::95'
NAD+MR+AAA374M::95'
NAD+UD+41065894724::95'
RFF+ABO:14BD 797H 2FA::1'
RFF+MWB:08177770000'
RFF+HWB:H1234'
UNT+34+000001'".Replace("\r\n", "");

			CMRCARSTMessage message = Factory.New<CMRCARSTMessage>();
			message.EM_MessageText = messageText;
			message.EM_LinkUniqueID = outturn.PK;
			message.EM_LinkTable = CusOutturn.Schema.TableName;
			Factory.Save();

			GatePassShipment gatePass = Factory.Load<GatePassShipment>(Shipment.PK);
			AssertEquals("Status is from Outturn", CMRConsolidatedCargoStatuses.Descriptions.HeldCargoIsHeldUnderCustomsControl, gatePass.JS_GatePassStatus);

			CusMAWB masterBill = Factory.New<CusMAWB>();
			CusHAWB houseBill = masterBill.ChildBills.AddNew();
			houseBill.CS_JS = Shipment.PK;
			houseBill.CS_CustomsStatus = CMRConsolidatedCargoStatuses.Codes.SububmovCargoCannotBeReleasedIntoHomeConsumptionButUnderbondMovementIsAllowed;
			AssertEquals("Status is from HAWB", CMRConsolidatedCargoStatuses.Descriptions.SububmovCargoCannotBeReleasedIntoHomeConsumptionButUnderbondMovementIsAllowed, gatePass.JS_GatePassStatus);
		}

		public void TestGatePassUseLatestOutturnStatus_SeaShipment()
		{
			var houseBillNum = "H1234";
			Shipment.JS_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			Shipment.JS_HouseBill = houseBillNum;
			var outturn1 = Factory.New<DepotCusOutturn>();
			outturn1.C5_ParentID = Shipment.PK;
			outturn1.C5_ParentTableCode = "JS";
			outturn1.C5_HouseBill = houseBillNum;
			outturn1.C5_CustomsStatus = CMRConsolidatedCargoStatuses.Codes.HeldCargoIsHeldUnderCustomsControl;
			Factory.Save();

			var outturn2 = Factory.New<DepotCusOutturn>();
			outturn2.C5_ParentID = Shipment.PK;
			outturn2.C5_ParentTableCode = "JS";
			outturn2.C5_HouseBill = houseBillNum;
			outturn2.C5_CustomsStatus = CMRConsolidatedCargoStatuses.Codes.ClearCargoIsFreeOfAnyImpedimentsAndMayBeReleased;
			Factory.Save();

			var gatePass = Factory.Load<GatePassShipment>(Shipment.PK);
			AssertEquals("The latest outturn's status should be used for Gate pass", CMRConsolidatedCargoStatuses.Descriptions.ClearCargoIsFreeOfAnyImpedimentsAndMayBeReleased, gatePass.JS_GatePassStatus);
		}

		public void TestGatePassUseLatestOutturnStatus_AirShipment()
		{
			var mawbNum = "08177770000";
			var hawbNum = "H1234";

			Shipment.JS_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			Shipment.JS_HouseBill = hawbNum;
			var consol = Shipment.Consols.AddNew();
			consol.JK_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			consol.JK_MasterBillNum = mawbNum;

			var underbond = Factory.New<CusUnderbond>();
			underbond.C4_MAWB = mawbNum;

			var outturn1 = Factory.New<DepotCusOutturn>();
			outturn1.C5_C4_Underbond = underbond.PK;
			outturn1.C5_HouseBill = hawbNum;
			outturn1.C5_CustomsStatus = CMRConsolidatedCargoStatuses.Codes.HeldCargoIsHeldUnderCustomsControl;
			Factory.Save();

			var outturn2 = Factory.New<DepotCusOutturn>();
			outturn2.C5_C4_Underbond = underbond.PK;
			outturn2.C5_HouseBill = hawbNum;
			outturn2.C5_CustomsStatus = CMRConsolidatedCargoStatuses.Codes.ClearCargoIsFreeOfAnyImpedimentsAndMayBeReleased;
			Factory.Save();

			var gatePass = Factory.Load<GatePassShipment>(Shipment.PK);
			AssertEquals("The latest outturn's status should be used for Gate pass", CMRConsolidatedCargoStatuses.Descriptions.ClearCargoIsFreeOfAnyImpedimentsAndMayBeReleased, gatePass.JS_GatePassStatus);
		}

		#region Helper Methods

		#region GetNewForwarderOrg

		OrgHeader GetNewForwarderOrg()
		{
			OrgHeader forwarderOrg = Factory.NewWithValidTestData<OrgHeader>();
			forwarderOrg.OH_IsForwarder = true;
			forwarderOrg.OH_IsDebtor = true;
			return forwarderOrg;
		}

		#endregion

		#endregion

		#endregion

		#region Status Class

		public void TestStatusClass()
		{
			GatePassShipment shipment = Factory.New<GatePassShipment>();
			CFSShipmentStatusProvider provider = CFSShipmentStatusProvider.New(shipment);
			CFSShipmentWrapper wrapper = CFSShipmentWrapper.Load(shipment);
			DepotCusOutturn outturn = wrapper.Outturns.AddNew();

			AssertEquals(StatusClass.Held, provider.StatusClass);

			outturn.C5_CustomsStatus = CMRConsolidatedCargoStatuses.Codes.ClearCargoIsFreeOfAnyImpedimentsAndMayBeReleased;
			AssertEquals(StatusClass.Clear, provider.StatusClass);

			outturn.C5_CustomsStatus = CMRConsolidatedCargoStatuses.Codes.TranshipCargoIsForTranshipmentATranshipmentNumberWillBeGeneratedAndTransmittedWithStatus;
			AssertEquals(StatusClass.Clear, provider.StatusClass);

			outturn.C5_CustomsStatus = CMRConsolidatedCargoStatuses.Codes.TransitCargoIsTransitCargoThisValueWillOnlyBeViewableFromAnInteractiveFunction;
			AssertEquals(StatusClass.Clear, provider.StatusClass);

			outturn.C5_CustomsStatus = CMRConsolidatedCargoStatuses.Codes.CondclearCargoCanBeReleasedIntoHomeConsumptionSubjectToConditionSTheseConditionsAreProvidedInSupplementaryInformation;
			AssertEquals(StatusClass.Warning, provider.StatusClass);

			outturn.C5_CustomsStatus = CMRConsolidatedCargoStatuses.Codes.ClearhrmCargoIsClearButIsIdentifiedAsHighRiskMovement;
			AssertEquals(StatusClass.Warning, provider.StatusClass);

			outturn.C5_CustomsStatus = CMRConsolidatedCargoStatuses.Codes.TranshphrmTranshipmentCargoIsClearButIsIdentifiedAsHighRiskMovement;
			AssertEquals(StatusClass.Warning, provider.StatusClass);

			outturn.C5_CustomsStatus = CMRConsolidatedCargoStatuses.Codes.HeldCargoIsHeldUnderCustomsControl;
			AssertEquals(StatusClass.Held, provider.StatusClass);

			outturn.C5_CustomsStatus = CMRConsolidatedCargoStatuses.Codes.AcsseizedCargoIsSeizedByCustoms;
			AssertEquals(StatusClass.Held, provider.StatusClass);

			outturn.C5_CustomsStatus = CMRConsolidatedCargoStatuses.Codes.AqisseizedCargoIsSeizedByQuarantine;
			AssertEquals(StatusClass.Held, provider.StatusClass);

			outturn.C5_CustomsStatus = CMRConsolidatedCargoStatuses.Codes.SububmovCargoCannotBeReleasedIntoHomeConsumptionButUnderbondMovementIsAllowed;
			AssertEquals(StatusClass.Held, provider.StatusClass);

			outturn.C5_CustomsStatus = CMRConsolidatedCargoStatuses.Codes.WithdrawnCargoReportHadBeenWithdrawn;
			AssertEquals(StatusClass.Held, provider.StatusClass);

			outturn.C5_CustomsStatus = ZString.Empty;
			outturn.C5_MessageStatus = CMRUnderbondStatuses.Codes.UnderbondApprovalAdviceReceived;
			AssertEquals(StatusClass.Underbonded, provider.StatusClass);

			outturn.C5_MessageStatus = CMRUnderbondStatuses.Codes.UnderbondApprovalRescindAdviceReceived;
			AssertEquals(StatusClass.Held, provider.StatusClass);

			outturn.C5_MessageStatus = CMRUnderbondStatuses.Codes.ExpectedCargoArrivalAdviceReceived;
			AssertEquals(StatusClass.Held, provider.StatusClass);

			outturn.C5_MessageStatus = CMRUnderbondStatuses.Codes.ExpectedCargoArrivalRescindAdviceReceived;
			AssertEquals(StatusClass.Held, provider.StatusClass);
		}

		#endregion

		#region Implementation

		class CFSShipmentStatusProviderForTest : CFSShipmentStatusProvider
		{
			public CFSShipmentStatusProviderForTest(GatePassShipment shipment)
				: base(shipment)
			{
			}

			public CFSShipmentWrapper ShipmentWrapperExposed => ShipmentWrapper;
			public CodeDescriptionPairList GatePassStatusesExposed => GatePassStatuses;
			public bool OutturnMatchesShipmentExposed(DepotCusOutturn outturn) => OutturnMatchesShipment(outturn);

			protected override ZString ShortStatusCore() => ShortStatusForTest;
			public ZString ShortStatusForTest;
		}

		protected override string GetCountryCode()
		{
			return Enterprise.Core.Constants.CountryCodes.Australia;
		}

		#endregion
	}
}
