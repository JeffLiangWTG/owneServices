using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.SWL.Business;
using Enterprise.Client.SWL.Business.Testing;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.SWL.GUI.Testing
{
	public class ShipnetSetupPlugInTest : ShipnetTestCase
	{
		public void TestLicenceCheckPoint()
		{
			using (MockShipnetSetupPlugIn testPlugIn = new MockShipnetSetupPlugIn(Org))
			{
				AssertNull("PlugIn LicenceCheckPoint", testPlugIn.LicenceCheckPoint);
			}
		}

		public void TestName()
		{
			using (ShipnetSetupPlugIn testPlugIn = new ShipnetSetupPlugIn(Org))
			{
				AssertEquals("PlugIn Name", "Setup Shipnet Data", testPlugIn.Name);
			}
		}

		public void TestHasUserControl()
		{
			using (MockShipnetSetupPlugIn testPlugIn = new MockShipnetSetupPlugIn(Org))
			{
				AssertEquals("PlugIn HasUserControl", true, testPlugIn.HasUserControl);
			}
		}

		public void TestUserControl()
		{
			using (MockShipnetSetupPlugIn testPlugIn = new MockShipnetSetupPlugIn(Org))
			{
				AssertEquals("Type of PlugIn UserControl", typeof(ShipnetSetupUserControl), testPlugIn.UserControl.GetType());
				AssertEquals("PlugIn UserControl is Docked to Fill", DockStyle.Fill, testPlugIn.UserControl.Dock);
			}
		}

		public void TestBusinessEntity()
		{
			using (ShipnetSetupPlugIn testPlugIn = new ShipnetSetupPlugIn(Org))
			{
				AssertEquals("Type of PlugIn BusinessEntity", typeof(ShipnetSetupBusinessObject), testPlugIn.BusinessEntity.GetType());
			}
		}

		public void TestSaveBusinessEntity()
		{
			using (ShipnetSetupPlugIn testPlugIn = new ShipnetSetupPlugIn(Org))
			{
				ShipnetSetupBusinessObject plugInBizObj = (ShipnetSetupBusinessObject)testPlugIn.BusinessEntity;
				plugInBizObj.IsShipnetCarrier = true;
				plugInBizObj.CommunicationMode.EK_CommunicationsTransport = ShipnetExportCommunicationsTransportMappingList.Codes.File;
				plugInBizObj.CommunicationMode.EK_Filename = "TESTFILE";
				plugInBizObj.CommunicationMode.EK_FileFormat = "TXT";
				plugInBizObj.CommunicationMode.EK_Destination = "DUMMYDIRECTORY";
				plugInBizObj.CommunicationMode.EK_Module = EDICommunicationsMode.Modules.Shipnet;
				plugInBizObj.DebtorControlCode = "DebtorCode";
				plugInBizObj.CreditorControlCode = "CreditorCode";
				plugInBizObj.ChargeGroups.RemoveAndDeleteAll();
				ShipnetChargeGroup group = plugInBizObj.ChargeGroups.AddNew();
				group.ChargeGroupCode = "Group Code";
				group.ChargeGroupDescription = "Group Description";
				ShipnetCharge charge = group.Charges.AddNew();
				charge.ChargePK = new ZGuid("22222222-2222-2222-2222-222222222222");
				Org.Factory.Save();
				ShipnetSetupBusinessObject registryValue = (ShipnetSetupBusinessObject)SWLDataRegistry.Instance.GetShipnetSetupBusinessObject(Org.CompanyData.PK);
				AssertEquals("CommunicationMode", plugInBizObj.CommunicationMode.PK, registryValue.CommunicationMode.PK);
				AssertEquals("Number of Charge Groups", 1, registryValue.ChargeGroups.Count);
				ShipnetChargeGroup registryChargeGroup = registryValue.ChargeGroups[0];
				AssertEquals("Group Code", "Group Code", registryChargeGroup.ChargeGroupCode);
				AssertEquals("Group Description", "Group Description", registryChargeGroup.ChargeGroupDescription);
				AssertEquals("Number of charges", 1, registryChargeGroup.Charges.Count);
				ShipnetCharge registryCharge = registryChargeGroup.Charges[0];
				AssertEquals("ChargePK", "22222222-2222-2222-2222-222222222222", registryCharge.ChargePK.ToString());
			}
		}

		public void TestLoadBusinessEntity()
		{
			ShipnetSetupBusinessObject bizObj = new ShipnetSetupBusinessObject(Factory);
			EDICommunicationsMode communicationsMode = Factory.New<EDICommunicationsMode>();
			communicationsMode.EK_Module = EDICommunicationsMode.Modules.Shipnet;
			bizObj.IsShipnetCarrier = true;
			bizObj.fCommunicationPK = communicationsMode.PK;
			bizObj.DebtorControlCode = "DebtorCode";
			bizObj.CreditorControlCode = "CreditorCode";
			ShipnetChargeGroup group = bizObj.ChargeGroups.AddNew();
			group.ChargeGroupCode = "Group Code";
			group.ChargeGroupDescription = "Group Description";
			ShipnetCharge charge = group.Charges.AddNew();
			charge.ChargePK = new ZGuid("22222222-2222-2222-2222-222222222222");
			SWLDataRegistry.Instance.SetOrDeleteShipnetSetupBusinessObject(Org.CompanyData.PK, bizObj);
			Factory.Save();
			using (ShipnetSetupPlugIn testPlugIn = new ShipnetSetupPlugIn(Org))
			{
				ShipnetSetupBusinessObject plugInBizObj = (ShipnetSetupBusinessObject)testPlugIn.BusinessEntity;
				AssertEquals("CommunicationPK", communicationsMode.PK, plugInBizObj.fCommunicationPK);
				AssertEquals("Number of Charge Groups", 1, plugInBizObj.ChargeGroups.Count);
				ShipnetChargeGroup registryChargeGroup = plugInBizObj.ChargeGroups[0];
				AssertEquals("Group Code", "Group Code", registryChargeGroup.ChargeGroupCode);
				AssertEquals("Group Description", "Group Description", registryChargeGroup.ChargeGroupDescription);
				AssertEquals("Number of charges", 1, registryChargeGroup.Charges.Count);
				ShipnetCharge registryCharge = registryChargeGroup.Charges[0];
				AssertEquals("ChargePK", "22222222-2222-2222-2222-222222222222", registryCharge.ChargePK.ToString());
			}
		}

		#region Implementation
		protected OrgHeader Org
		{
			get
			{
				return org ?? (org = Factory.LoadTop1<OrgHeader>(new ZQuery()));
			}
		}

		OrgHeader org;
		class MockShipnetSetupPlugIn : ShipnetSetupPlugIn
		{
			public MockShipnetSetupPlugIn(IBusiness hostBusinessEntity) : base(hostBusinessEntity)
			{
			}

			public new LicenceCheckpoint LicenceCheckPoint
			{
				get
				{
					return base.LicenceCheckPoint;
				}
			}

			public new ZBool HasUserControl
			{
				get
				{
					return base.HasUserControl;
				}
			}
		}
		#endregion
	}
}
