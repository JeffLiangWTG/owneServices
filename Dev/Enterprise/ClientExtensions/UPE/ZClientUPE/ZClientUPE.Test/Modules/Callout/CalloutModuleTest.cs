using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.UPE.Business;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client.UPE.Module.Testing
{
	internal class CalloutModuleTest : TestCaseWithDummy
	{
		public void TestLicenceAndSecurityCheckpoint()
		{
			AssertEquals(Env.Licence.AlwaysAllow, Module.LicenceCheckPoint);
			AssertEquals(Env.Security.ACAHouse, Module.SecurityCheckpoint);
		}

		public void TestModuleID()
		{
			AssertEquals(ClientModuleRegistration.Callout, Module.ID);
		}

		public void TestAllowNewAndDelete()
		{
			AssertEquals(false, Module.AllowNew);
			AssertEquals(false, Module.AllowDelete);
		}

		public void TestGetNewGridCollection()
		{
			AssertEquals("Grid collection MUST be Callout so that items in the grid are shown as Callout appropriately", typeof(CalloutCollection), Module.GetNewGridCollection().GetType());
		}

		public void TestGetNewController()
		{
			AssertEquals(typeof(CalloutController), Module.GetNewController(Dummy).GetType());
		}

		public void TestGetNewFilterControl()
		{
			using (CalloutFilterControl filterControl = Module.GetNewFilterControl() as CalloutFilterControl)
			{
				AssertNotNull("Type should be CalloutFilterControl", filterControl);
			}
		}

		public void TestGetNewFilterBusinessObject()
		{
			AssertEquals(typeof(CalloutFilterBusinessObject), Module.GetNewFilterBusinessObject().GetType());
		}

		protected override void TearDown()
		{
			Module.Dispose();
			base.TearDown();
		}

		protected override void SetUp()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			base.SetUp();
		}

		#region CalloutModuleForTest
		CalloutModuleForTest Module
		{
			get
			{
				if (fModule == null)
				{
					fModule = new CalloutModuleForTest();
				}

				return fModule;
			}
		}

		CalloutModuleForTest fModule;
		class CalloutModuleForTest : CalloutModule
		{
			public new IBusinessObjectCollection GetNewGridCollection()
			{
				return base.GetNewGridCollection();
			}

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

			public new BusinessObjectFactory Factory
			{
				get
				{
					return base.Factory;
				}
			}
		}
		#endregion
	}
}
