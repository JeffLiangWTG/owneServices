using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.EU.GUI.PlugIn.Testing
{
	public class EcsMessageMenuProviderTest : TestCaseWithFactory
	{
		public virtual void TestCreateArrivalMessages()
		{
			Assert("CreateArrivalMessages", !EcsMessageMenuProvider.New(Factory.New<CusExitControlHeader>()).CreateArrivalMessages());
		}

		public virtual void TestCreateDepartureMessages()
		{
			Assert("CreateArrivalMessages", !EcsMessageMenuProvider.New(Factory.New<CusExitControlHeader>()).CreateDepartureMessages());
		}

		public virtual void TestCreateMenuItems()
		{
			var menuItems = EcsMessageMenuProvider.New(Factory.New<CusExitControlHeader>()).CreateMenuItems();

			AssertEquals("CreateMenuItems", 3, menuItems.Count());
			Assert(menuItems.Any(c => c.Text == "Arrive at Exit Location"));
			Assert(menuItems.Any(c => c.Text == "Depart from Exit Location"));
			Assert(menuItems.Any(c => c.Text == "Capture MRNs"));
		}

		protected virtual Type ExpectEcsMessageMenuProviderType => typeof(EcsMessageMenuProvider);

		public void TestNewEcsMessageMenuProvider()
		{
			AssertType(ExpectEcsMessageMenuProviderType, EcsMessageMenuProvider.New(Factory.New<CusExitControlHeader>()));
		}
	}
}
