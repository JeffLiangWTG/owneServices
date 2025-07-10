using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business.Interfaces;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	class VoyageManifestExpectedArrivalCusUnderbondFactory_Test : TestCaseWithFactory
	{
		public void TestLoadOrCreateUnderbondParentForContainerMovementWhereContainerExists()
		{
			CusSeaManTranHead tranHead = Factory.New<CusSeaManTranHead>();
			tranHead.BT_VoyageNum = "156";
			tranHead.BT_VesselName = "ANRO TEMASEK";
			CusSeaManOBLHeader oceanBill = tranHead.OceanBills.AddNew();
			CusSeaManOBLDetail container = oceanBill.Details.AddNew();
			container.BD_ContainerNumber = "RRRS0000001";
			container.BD_LineCargoType = Core.Constants.ContainerModes.FCL;
			ICusUnderbondDependentCollectionParent result = UnderbondFactory.LoadOrCreateUnderbondParentInternal(ContainerApprovalArrivalMessage, 1);
			AssertEquals("Result", container, result);
		}

		public void TestCreateOrLoadUnderbondLoad()
		{
			CusSeaManTranHead tranHead = Factory.New<CusSeaManTranHead>();
			tranHead.BT_VoyageNum = "156";
			tranHead.BT_VesselName = "ANRO TEMASEK";
			CusSeaManOBLHeader oceanBill = tranHead.OceanBills.AddNew();
			CusSeaManOBLDetail container = oceanBill.Details.AddNew();
			container.BD_ContainerNumber = "RRRS0000001";
			container.BD_LineCargoType = Core.Constants.ContainerModes.FCL;
			CusUnderbond result = UnderbondFactory.CreateOrLoadUnderbondInternal(ContainerApprovalArrivalMessage, 1);

			AssertEquals("Movement Reason", "MOV", result.C4_MovementReason);
			AssertEquals("Mode of movement", "IVS", result.C4_ModeOfMovement);
			AssertEquals("destination premise", "8135H", result.C4_DestinationPremiseID);
			AssertEquals("origin premise", "KR15S", result.C4_OriginPremiseID);
			AssertEquals("C4_ParentID", container.PK, result.C4_ParentID);

			AssertEquals("same underbond", result, UnderbondFactory.CreateOrLoadUnderbondInternal((CMRUBMREQRMessage)ContainerApprovalArrivalMessage.Clone(), 1));

			AssertEquals("Movement Reason", "MOV", result.C4_MovementReason);
			AssertEquals("Mode of movement", "IVS", result.C4_ModeOfMovement);
			AssertEquals("Origin premise", "8135H", result.C4_DestinationPremiseID);
			AssertEquals("destination premise", "KR15S", result.C4_OriginPremiseID);
		}

		public void TestLoadOrCreateUnderbondParentForContainerMovementWhereContainerDoesntExist()
		{
			ICusUnderbondDependentCollectionParent result = UnderbondFactory.LoadOrCreateUnderbondParentInternal(ContainerApprovalArrivalMessage, 1);
			AssertEquals("Result", null, result);
		}

		public void TestIsInterestedInUBMREQR()
		{
			GlbBranch.CurrentBranch.OrgProxy.MainAddress.LocalControlledPremisesID = "KR15S";
			GlbBranch.CurrentBranch.OrgProxy.OH_IsSeaCTO = true;
			AssertEquals("IsInterestedInUBMREQR", false, UnderbondFactory.IsInterestedInUBMREQRInternal(HouseApprovalArrivalMessage));
			AssertEquals("IsInterestedInUBMREQR", true, UnderbondFactory.IsInterestedInUBMREQRInternal(ContainerApprovalArrivalMessage));
			AssertEquals("IsInterestedInUBMREQR", false, UnderbondFactory.IsInterestedInUBMREQRInternal(ContainerExpectedArrivalMessage));
		}

		public void TestExpectedArrivalNotProcessedTwice()
		{
			CusSeaManTranHead tranHead = Factory.New<CusSeaManTranHead>();
			tranHead.BT_VoyageNum = "156";
			tranHead.BT_VesselName = "ANRO TEMASEK";
			CusSeaManOBLHeader oceanBill = tranHead.OceanBills.AddNew();
			CusSeaManOBLDetail container = oceanBill.Details.AddNew();
			container.BD_ContainerNumber = "RRRS0000001";
			container.BD_LineCargoType = Core.Constants.ContainerModes.FCL;

			ContainerApprovalArrivalMessage.SetEM_LinkedObject();

			ZQuery filter = new ZQuery(CusUnderbondSchema.C4_ParentTableCode, "BD");
			filter.AddToFilter(CusUnderbondSchema.C4_ParentID, container.PK);
			CusUnderbond[] underbonds = (CusUnderbond[])Factory.Load(typeof(CusUnderbond), filter);
			AssertEquals("Only one underbond exists for this container", 1, underbonds.Length);

			CMRUBMREQRMessage copyOfMessage = (CMRUBMREQRMessage)ContainerApprovalArrivalMessage.Clone();
			copyOfMessage.SetEM_LinkedObject();

			underbonds = (CusUnderbond[])Factory.Load(typeof(CusUnderbond), filter);
			AssertEquals("Still only one underbond exists for this container", 1, underbonds.Length);
		}

		public void TestVoyageManifestUnderbondReturned()
		{
			ICusUnderbondDependentCollectionParent ocean = Factory.New<CusSeaManOBLDetail>();
			((CusSeaManOBLDetail)ocean).BD_ContainerNumber = "RRRS0000001";
			CusUnderbond underbond = (CusUnderbond)ocean.Underbonds.AddNew();
			underbond.C4_OriginPremiseID = "KR15S";
			underbond.C4_DestinationPremiseID = "8135H";
			underbond.C4_SendersMessageReference = "U00000500";

			ContainerApprovalArrivalMessage.SetEM_LinkedObject();
			ZQuery underbondFilter = new ZQuery(CusUnderbondSchema.C4_OriginPremiseID, "KR15S");
			underbondFilter.AddToFilter(CusUnderbondSchema.C4_DestinationPremiseID, "8135H");

			CusUnderbond[] underbonds = (CusUnderbond[])Factory.Load(typeof(CusUnderbond), underbondFilter);
			AssertEquals("One Underbonds should have been returned", 1, underbonds.Length);
		}

		#region Implementation

		VoyageManifestExpectedArrivalCusUnderbondFactory fUnderbondFactory;
		VoyageManifestExpectedArrivalCusUnderbondFactory UnderbondFactory
		{
			get
			{
				if (fUnderbondFactory == null)
				{
					fUnderbondFactory = new VoyageManifestExpectedArrivalCusUnderbondFactory();
				}
				return fUnderbondFactory;
			}
		}

		CMRUBMREQRMessage fHouseApprovalArrivalMessage;
		CMRUBMREQRMessage HouseApprovalArrivalMessage
		{
			get
			{
				if (fHouseApprovalArrivalMessage == null)
				{
					fHouseApprovalArrivalMessage = Factory.New<CMRUBMREQRMessage>();
					fHouseApprovalArrivalMessage.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+961:::UBMREQR+2C4G E33F GJ0F:1+32'
FTX+AAH+++CJM436P20191'
TDT+20+156++11++++7619422::11'
TDT+1++ROA'
LOC+5+8136B::95'
LOC+4+8139A::95'
NAD+MR+CJM436P::95'
RFF+ANX:UNDERBOND APPROVAL'
RFF+ACD:MOV'
DOC+1'
PAC+++LCL:67:95'
PAC+0000020++CT:185:95'
RFF+AAQ:HALE2525260'
RFF+MB:OCEAN20050613DD'
RFF+BH:HOUSE45454545'
UNT+17+000001'".Replace("\r\n", "");
				}
				return fHouseApprovalArrivalMessage;
			}
		}

		#region Container Approval Arrival Message

		CMRUBMREQRMessage fContainerApprovalMessage;
		CMRUBMREQRMessage ContainerApprovalArrivalMessage
		{
			get
			{
				if (fContainerApprovalMessage == null)
				{
					fContainerApprovalMessage = Factory.New<CMRUBMREQRMessage>();
					fContainerApprovalMessage.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+961:::UBMREQR+9538 A830 45F:1+32'
FTX+AAH+++CJM436P20417'
TDT+20+156++11++++7619422::11'
TDT+1++IVS'
TDT+1+37++11++++7038678::11'
LOC+5+KR15S::95'
LOC+4+8135H::95'
NAD+MR+CJN977M::95'
RFF+ABO:U00000500/SYD1::1'
RFF+ANX:UNDERBOND APPROVAL'
RFF+ACD:MOV'
DOC+1'
PAC+++FCL:67:95'
PAC+0000200++CT:185:95'
RFF+AAQ:RRRS0000001'
DOC+1'
PAC+++FCL:67:95'
PAC+0000200++CT:185:95'
RFF+AAQ:RRRS0000002'
DOC+1'
PAC+++FCL:67:95'
PAC+0000200++CT:185:95'
RFF+AAQ:RRRS0000003'
DOC+1'
PAC+++FCL:67:95'
PAC+0000200++CT:185:95'
RFF+AAQ:RRRS0000004'
DOC+1'
PAC+++FCL:67:95'
PAC+0000200++CT:185:95'
RFF+AAQ:RRRS0000005'
DOC+1'
PAC+++FCL:67:95'
PAC+0000200++CT:185:95'
RFF+AAQ:RRRS0000006'
DOC+1'
PAC+++FCL:67:95'
PAC+0000200++CT:185:95'
RFF+AAQ:RRRS0000007'
DOC+1'
PAC+++FCL:67:95'
PAC+0000200++CT:185:95'
RFF+AAQ:RRRS0000008'
DOC+1'
PAC+++FCL:67:95'
PAC+0000200++CT:185:95'
RFF+AAQ:RRRS0000009'
DOC+1'
PAC+++FCL:67:95'
PAC+0000200++CT:185:95'
RFF+AAQ:RRRS0000010'
UNT+52+000001'".Replace("\r\n", "");
				}
				return fContainerApprovalMessage;
			}
		}

		#endregion

		#region Container Expected Arrival Message

		CMRUBMREQRMessage fContainerExpectedArrivalMessage;
		CMRUBMREQRMessage ContainerExpectedArrivalMessage
		{
			get
			{
				if (fContainerExpectedArrivalMessage == null)
				{
					fContainerExpectedArrivalMessage = Factory.New<CMRUBMREQRMessage>();
					fContainerExpectedArrivalMessage.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+961:::UBMREQR+3FH1 CGE9 601F:1+32'
DTM+9:20051104182030648117:ZZZ'
FTX+AAH+++AAA374MU00000584/SYD1'
TDT+20+224++11++++8811924::11'
TDT+1++ROA'
LOC+5+KR15S::95'
LOC+4+8135H::95'
NAD+MR+AAA374M::95'
NAD+UD+41065894724::95'
RFF+ABO:U00000584/SYD1::1'
RFF+ANX:EXPECTED CARGO ARRIVAL ADVICE'
RFF+ACD:MOV'
DOC+1'
PAC+++FCL:67:95'
PAC+0000030++PK:185:95'
RFF+AAQ:ARUE3049589'
PCI+28+THINGS'
UNT+19+000001'".Replace("\r\n", "");
				}
				return fContainerExpectedArrivalMessage;
			}
		}

		#endregion

		#endregion

	}
}
