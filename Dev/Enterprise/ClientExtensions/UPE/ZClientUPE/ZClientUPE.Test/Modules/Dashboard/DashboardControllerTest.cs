using CargoWise.EntityFramework;
using Enterprise.Client.UPE.Business;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Module
{
	[TestedType(typeof(DashboardController))]
	public class DashboardControllerTest : ZSingletonControllerBasherTest
	{
		public void TestCheckPointForView()
		{
			AssertEquals(Env.Security.None, new DashboardController().InternalCheckPointForView);
		}

		public void TestCheckPointForEdit()
		{
			AssertEquals(Env.Security.None, new DashboardController().InternalCheckPointForEdit);
		}

		public void TestCheckPointForNew()
		{
			AssertEquals(Env.Security.None, new DashboardController().InternalCheckPointForNew);
		}

		public void TestCheckPointForDelete()
		{
			AssertEquals(Env.Security.None, new DashboardController().InternalCheckPointForDelete);
		}

		protected override ControllerID GetControllerID()
		{
			return ClientControllerRegistration.Dashboard;
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			return new Dashboard(new BusinessObjectFactory());
		}

		protected override void SetUp()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			base.SetUp();
		}
	}
}
