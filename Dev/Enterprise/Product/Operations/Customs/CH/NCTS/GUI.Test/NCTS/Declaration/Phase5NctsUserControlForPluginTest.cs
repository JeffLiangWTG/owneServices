using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.CH.NCTS.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.CH.NCTS.GUI.Testing;

[TestedType(typeof(Phase5NctsUserControlForPlugin))]
sealed class Phase5NctsUserControlForPluginTest : TestCaseWithFactory
{
	public void TestGetNctsArrivalUserControl()
	{
		var header = Factory.New<NctsHeader>();
		var userControlForPlugin = new Phase5NctsUserControlForPluginForTest(header);
		var userControl = userControlForPlugin.GetNctsArrivalUserControlExposed();
		AssertType<Phase5ArrivalNotificationTabUserControl>(userControl);
		userControl.Dispose();
		userControlForPlugin.Dispose();
	}

	sealed class Phase5NctsUserControlForPluginForTest : Phase5NctsUserControlForPlugin
	{
		public Phase5NctsUserControlForPluginForTest(NctsHeader nctsMovement) : base(nctsMovement)
		{
		}

		public ZUserControl GetNctsArrivalUserControlExposed() => GetNctsArrivalUserControl();
	}
}
