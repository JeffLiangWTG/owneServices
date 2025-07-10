using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Module.Testing
{
	internal class UPEAUCustomsAirCargoModuleTest : TransactionedTestCase
	{
		public void TestSupportWorkflow()
		{
			Assert(Module.SupportsWorkflow);
		}

		public void TestModuleID()
		{
			AssertEquals(ClientModuleRegistration.HouseAirCargo, Module.ID);
		}

		public void TestCheckPoints()
		{
			AssertEquals(Env.Licence.AlwaysAllow, Module.LicenceCheckPoint);
			AssertEquals(Env.Security.ACAHouse, Module.SecurityCheckpoint);
		}

		public void TestGetNewFilterControl()
		{
			using (UPEAirCargoFilterControl filterControl = (UPEAirCargoFilterControl)Module.GetNewFilterControl())
			{
				AssertNotNull(filterControl);
			}
		}

		public void TestGetNewFilterBusinessObject()
		{
			AssertEquals(typeof(UPEAirCargoFilterBusinessObject), Module.GetNewFilterBusinessObject().GetType());
		}

		public void TestGetNewController()
		{
			AssertEquals(typeof(UPEAirCargoController), Module.GetNewController(null).GetType());
		}

		public void TestLevel1LoadMenuItem()
		{
			MenuAssertion.AssertHasMenu("Level 1 load menu item should exist", Module.FormActionMenu, "&Actions", "D&ata Transfer", "Import Level 1");
		}

		#region Implementation
		protected override void SetUp()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			base.SetUp();
			Module = new UPEAUCustomsAirCargoModuleForTest();
		}

		protected override void TearDown()
		{
			Module.Dispose();
			base.TearDown();
		}

		UPEAUCustomsAirCargoModuleForTest Module;
		class UPEAUCustomsAirCargoModuleForTest : UPEAirCargoModule
		{
			public new ZController GetNewController(BusinessObject selectedBusinessObject)
			{
				return base.GetNewController(selectedBusinessObject);
			}

			public new IFilterControl GetNewFilterControl()
			{
				return base.GetNewFilterControl();
			}

			public new FilterBusinessObject GetNewFilterBusinessObject()
			{
				return base.GetNewFilterBusinessObject();
			}

			public new FilterModuleMenuItemDescriptorCollection ImportMenuItems
			{
				get
				{
					return base.ImportMenuItems;
				}
			}

			public new FilterModuleMenuItemDescriptorCollection ExportMenuItems
			{
				get
				{
					return base.ExportMenuItems;
				}
			}
		}
		#endregion
	}
}
