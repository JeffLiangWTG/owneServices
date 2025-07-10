using System;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace CargoWise.Bi.Product.Module.Controller.Testing
{
	[TestedType(typeof(PowerBiAnalyticsReportsController))]
	// [RequiresSoftware(RequiredSoftware.IsVM | RequiredSoftware.PowerBi)]
	public class PowerBiAnalyticsReportsControllerBasherTest : ZPopupControllerBasherTest
	{
		protected override void SetUp()
		{
			base.SetUp();
			if (TestingState.IsRunningOnDAT)
			{
				Name = "SkipTest";
			}
		}

		public void SkipTest()
		{
			Assert(true);
		}

		public void TestModuleID()
		{
			AssertEquals(null, new PowerBiAnalyticsReportsController().ModuleID);
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.PowerBiAnalyticsReports;
		}

		public override Type ControllerToBashType
		{
			get { return typeof(PowerBiAnalyticsReportsController); }
		}
	}
}
