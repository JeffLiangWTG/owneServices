using System;
using Enterprise.Customs.IE.GUI;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Module.Testing
{
	[TestedType(typeof(CustomsAndExciseReportsController))]
	sealed class CustomsAndExciseReportsControllerTest : ZArchitecture.Modules.Testing.ZControllerBasherTest
	{
		public override Type ControllerToBashType => typeof(CustomsAndExciseReportsController);

		protected override ControllerID GetControllerID() => ControllerIDs.Customs.IE.CustomsAndExciseReports;
		public void TestGetForm()
		{
			var controller = new CustomsAndExciseReportsController();
			using (var form = controller.ShowNewForm())
			{
				AssertType<CustomsAndExciseReportsForm>("GetForm", form);
			}
		}
	}
}
