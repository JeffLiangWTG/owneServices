using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Interfaces;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	class SeaCargoForwarderUnderbondApprovalFactoryTestCase : TestCaseWithFactory
	{
		public void TestSeaCargoForwarderRecordExistsDoesntMatchOnVoyageManifest()
		{
			CusSeaManTranHead tranHead = Factory.New<CusSeaManTranHead>();
			CusSeaManOBLHeader oceanBill = tranHead.OceanBills.AddNew();
			CusSeaManOBLDetail detail = oceanBill.Details.AddNew();

			CusUnderbond underbond = (CusUnderbond)((ICusUnderbondDependentCollectionParent)detail).Underbonds.AddNew();
			underbond.C4_SendersMessageReference = "U00000109";

			AssertEquals("Voyage Manifest container is not a sea cargo forwarder record.", false, UnderbondFactory.SeaCargoForwarderUnderbondRequestExists(ContainerUnderbondApprovalMessage));
		}

		public void TestContainerUBMREQRDoesNotCreateANewContainer()
		{
			CusSCAOceanBill oceanBill = CreateOceanBill("ANRO TEMASEK", "156");
			AssertNull(UnderbondFactory.LoadOrCreateUnderbondParentInternal(ContainerUnderbondApprovalMessage, 1));
		}

		public void TestVoyageManifestUnderbondReturned()
		{
			CusSCAOceanBill ocean = Factory.New<CusSCAOceanBill>();
			CusSCAContainer container = ocean.Containers.AddNew();
			container.CN_ContainerNumber = "FRCU3920390";
			CusUnderbond underbond = container.Underbonds.AddNew();
			underbond.C4_OriginPremiseID = "9122P";
			underbond.C4_DestinationPremiseID = "9914N";
			underbond.C4_SendersMessageReference = "U00000109";

			UnderbondFactory.ProcessIncomingUBMREQR(ContainerUnderbondApprovalMessage);
			ZQuery underbondFilter = new ZQuery(CusUnderbondSchema.C4_OriginPremiseID, "9122P");
			underbondFilter.AddToFilter(CusUnderbondSchema.C4_DestinationPremiseID, "9914N");
			AssertEquals("Status should be app", CMRUnderbondStatuses.Codes.UnderbondApprovalAdviceReceived, underbond.C4_Status);
		}

		public void TestExpectedCargoArrivalMessageIgnored()
		{
			CusSCAOceanBill ocean = Factory.New<CusSCAOceanBill>();
			CusSCAContainer container = ocean.Containers.AddNew();
			container.CN_ContainerNumber = ContainerNumber;
			CusUnderbond underbond = container.Underbonds.AddNew();
			underbond.C4_OriginPremiseID = OriginID;
			underbond.C4_DestinationPremiseID = DestinationID;
			underbond.C4_SendersMessageReference = "U00000109";

			UnderbondFactory.ProcessIncomingUBMREQR(ContainerExpectedArrivalMessage);

			ZQuery underbondFilter = new ZQuery(EDIMessageSchema.EM_LinkUniqueID, underbond.PK);
			AssertNull("Underbond should not have been linked", Factory.LoadTop1<CMRUBMREQRMessage>(underbondFilter));
		}

		public void TestUnderbondApprovalMatchedUnderbondRequest()
		{
			CusSCAOceanBill ocean = Factory.New<CusSCAOceanBill>();
			CusSCAContainer container = ocean.Containers.AddNew();
			container.CN_ContainerNumber = ContainerNumber;
			CusUnderbond underbond = container.Underbonds.AddNew();
			underbond.C4_OriginPremiseID = OriginID;
			underbond.C4_DestinationPremiseID = DestinationID;
			underbond.C4_SendersMessageReference = "U00000109";

			UnderbondFactory.ProcessIncomingUBMREQR(ContainerUnderbondApprovalMessage);
			ZQuery underbondFilter = new ZQuery(EDIMessageSchema.EM_LinkUniqueID, underbond.PK);
			AssertNotNull("Underbond should not have been linked", Factory.LoadTop1<CMRUBMREQRMessage>(underbondFilter));
			AssertEquals("Underbond Status should now be Approved", CMRUnderbondStatuses.Descriptions.UnderbondApprovalAdviceReceived, underbond.ApprovalStatus);
		}

		public void TestUnderbondApprovalIgnoredIfUnderbondRequestMissing()
		{
			CusSCAOceanBill ocean = Factory.New<CusSCAOceanBill>();
			CusSCAContainer container = ocean.Containers.AddNew();
			container.CN_ContainerNumber = ContainerNumber;

			UnderbondFactory.ProcessIncomingUBMREQR(ContainerUnderbondApprovalMessage);
			AssertEquals("No new underbonds should be added to the container", 0, container.Underbonds.Count);
		}

		public void TestUnderbondApprovalIgnoredIfShipmentDetailsDoNotMatch()
		{
			CusSCAOceanBill ocean = Factory.New<CusSCAOceanBill>();
			CusSCAContainer container = ocean.Containers.AddNew();
			container.CN_ContainerNumber = DifferentContainerNumber;
			CusUnderbond underbond = container.Underbonds.AddNew();
			underbond.C4_OriginPremiseID = OriginID;
			underbond.C4_DestinationPremiseID = DestinationID;
			underbond.C4_SendersMessageReference = "U00000109";

			UnderbondFactory.ProcessIncomingUBMREQR(ContainerUnderbondApprovalMessage);
			ZQuery underbondFilter = new ZQuery(EDIMessageSchema.EM_LinkUniqueID, underbond.PK);
			AssertNull("Underbond should not have been linked", Factory.LoadTop1<CMRUBMREQRMessage>(underbondFilter));
		}

		public void TestHouseUnderbondApprovalMatchesExistingUnderbond()
		{
			CusSCAOceanBill ocean = Factory.New<CusSCAOceanBill>();
			ocean.CB_OceanBill = MasterBill;
			CusSCAContainer container = ocean.Containers.AddNew();
			container.CN_ContainerNumber = HouseContainer;
			CusSCAHouse house = ocean.HouseBills.AddNew();
			house.CA_HouseBill = HouseBill;
			CusUnderbond underbond = container.Underbonds.AddNew();
			underbond.C4_OriginPremiseID = OriginID;
			underbond.C4_DestinationPremiseID = DestinationID;
			underbond.C4_SendersMessageReference = "U00000663";

			UnderbondFactory.ProcessIncomingUBMREQR(HouseUnderbondApprovalMessage);
			ZQuery underbondFilter = new ZQuery(EDIMessageSchema.EM_LinkUniqueID, underbond.PK);
			AssertNotNull("Underbond should have been linked", Factory.LoadTop1<CMRUBMREQRMessage>(underbondFilter));
			AssertEquals("Underbond Status should now be Approved", CMRUnderbondStatuses.Descriptions.UnderbondApprovalAdviceReceived, underbond.ApprovalStatus);
		}

		public void TestHouseUnderbondApprovalIgnoredIfUnderbondRequestMissing()
		{
			CusSCAOceanBill ocean = Factory.New<CusSCAOceanBill>();
			ocean.CB_OceanBill = MasterBill;
			CusSCAContainer container = ocean.Containers.AddNew();
			container.CN_ContainerNumber = HouseContainer;
			CusSCAHouse house = ocean.HouseBills.AddNew();
			house.CA_HouseBill = HouseBill;
			house.Pivot.Add(container.Pivots.AddNew());
			UnderbondFactory.ProcessIncomingUBMREQR(HouseUnderbondApprovalMessage);
			AssertEquals("No new underbonds should be added to the container", 0, house.Pivot[0].Underbonds.Count);
		}

		public void TestHouseUnderbondApprovalIgnoredIfDetailsIncorrect()
		{
			CusSCAOceanBill ocean = Factory.New<CusSCAOceanBill>();
			ocean.CB_OceanBill = MasterBill;
			CusSCAContainer container = ocean.Containers.AddNew();
			container.CN_ContainerNumber = HouseContainer;
			CusSCAHouse house = ocean.HouseBills.AddNew();
			house.CA_HouseBill = WrongHouseBill;
			CusUnderbond underbond = container.Underbonds.AddNew();
			underbond.C4_OriginPremiseID = OriginID;
			underbond.C4_DestinationPremiseID = DestinationID;
			underbond.C4_SendersMessageReference = "U00000109";

			UnderbondFactory.ProcessIncomingUBMREQR(HouseUnderbondApprovalMessage);
			ZQuery underbondFilter = new ZQuery(EDIMessageSchema.EM_LinkUniqueID, underbond.PK);
			AssertNull("Underbond should not have been linked", Factory.LoadTop1<CMRUBMREQRMessage>(underbondFilter));
		}

		#region Implementation
		#region CusSCA Creation methods

		CusSCAOceanBill CreateOceanBill(ZString vesselName, ZString voyageNumber)
		{
			CusSCAOceanBill oceanBill = Factory.New<CusSCAOceanBill>();
			oceanBill.CB_VesselName = vesselName;
			oceanBill.CB_Voyage = voyageNumber;
			return oceanBill;
		}

		#endregion

		const string DifferentContainerNumber = "UGTU4483829";
		const string ContainerNumber = "FRCU3920390";
		const string OriginID = "9921P";
		const string DestinationID = "9914N";

		#region ContainerUnderbondApprovalMessage

		CMRUBMREQRMessage ContainerUnderbondApprovalMessage
		{
			get
			{
				if (fContainerUnderbondApprovalMessage == null)
				{
					fContainerUnderbondApprovalMessage = Factory.New<CMRUBMREQRMessage>();
					fContainerUnderbondApprovalMessage.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+961:::UBMREQR+2A5F 0G2H 3365:1+32'
DTM+9:20050823141330549979:ZZZ'
FTX+AAH+++AAA374MU00000109/SYD3'
TDT+20+8547++11++++8610033::11'
TDT+1++ROA'
LOC+5+9122P::95'
LOC+4+9914N::95'
NAD+MR+AAA374M::95'
NAD+UD+41065894724::95'
RFF+ABO:U00000109/SYD3::1'
RFF+ANX:UNDERBOND APPROVAL'
RFF+ACD:DCL'
DOC+1'
PAC+++FCL:67:95'
PAC+0000168++PK:185:95'
RFF+AAQ:FRCU3920390'
UNT+18+000001'".Replace("\r\n", "");
				}
				return fContainerUnderbondApprovalMessage;
			}
		}
		CMRUBMREQRMessage fContainerUnderbondApprovalMessage;

		#endregion

		#region ContainerExpectedArrivalMessage

		CMRUBMREQRMessage ContainerExpectedArrivalMessage
		{
			get
			{
				if (fContainerExpectedArrivalMessage == null)
				{
					fContainerExpectedArrivalMessage = Factory.New<CMRUBMREQRMessage>();
					fContainerExpectedArrivalMessage.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+961:::UBMREQR+3I1B 9E57 3365:1+32'
DTM+9:20050823141330626423:ZZZ'
FTX+AAH+++AAA374MU00000109/SYD3'
TDT+20+8547++11++++8610033::11'
TDT+1++ROA'
LOC+5+9122P::95'
LOC+4+9914N::95'
NAD+MR+AAA374M::95'
NAD+UD+41065894724::95'
RFF+ABO:U00000109/SYD3::1'
RFF+ANX:EXPECTED CARGO ARRIVAL ADVICE'
RFF+ACD:DCL'
DOC+1'
PAC+++FCL:67:95'
PAC+0000168++PK:185:95'
RFF+AAQ:FRCU3920390'
UNT+18+000001'".Replace("\r\n", "");
				}
				return fContainerExpectedArrivalMessage;
			}
		}
		CMRUBMREQRMessage fContainerExpectedArrivalMessage;

		#endregion
		const string HouseBill = "HB143LCL103";
		const string WrongHouseBill = "HB147LCL002";
		const string HouseContainer = "LCLU2011828";
		const string MasterBill = "OBL143LCL1";

		#region HouseUnderbondApprovalMessage

		CMRUBMREQRMessage HouseUnderbondApprovalMessage
		{
			get
			{
				if (fHouseUnderbondApprovalMessage == null)
				{
					fHouseUnderbondApprovalMessage = Factory.New<CMRUBMREQRMessage>();
					fHouseUnderbondApprovalMessage.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+961:::UBMREQR+JE4B 8JF9 2GF:1+32'
DTM+9:20051109172550548540:ZZZ'
FTX+AAH+++AAA374MU00000663/SYD2'
TDT+20+143++11++++7817103::11'
TDT+1++ROA'
LOC+5+9914N::95'
LOC+4+EF79D::95'
NAD+MR+AAA374M::95'
NAD+UD+41065894724::95'
RFF+ABO:U00000663/SYD2::1'
RFF+ANX:UNDERBOND APPROVAL'
RFF+ACD:DEL'
DOC+1'
PAC+++LCL:67:95'
PAC+0000004++YC:185:95'
RFF+AAQ:LCLU2011828'
RFF+MB:OBL143LCL1'
RFF+BH:HB143LCL103'
PCI+28+NIL MARKS'
UNT+21+000001'".Replace("\r\n", "");
				}
				return fHouseUnderbondApprovalMessage;
			}
		}
		CMRUBMREQRMessage fHouseUnderbondApprovalMessage;

		#endregion

		SeaCargoForwarderUnderbondApprovalFactory fUnderbondFactory;
		SeaCargoForwarderUnderbondApprovalFactory UnderbondFactory
		{
			get
			{
				if (fUnderbondFactory == null)
				{
					fUnderbondFactory = new SeaCargoForwarderUnderbondApprovalFactory();
				}
				return fUnderbondFactory;
			}
		}

		protected override void SetUp()
		{
			Env.Registry.AUCustomsImportsMessagingMode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			base.SetUp();
		}

		#endregion
	}
}
