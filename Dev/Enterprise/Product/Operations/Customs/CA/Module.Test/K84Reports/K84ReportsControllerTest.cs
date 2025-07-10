using System;
using Enterprise.Customs.CA.GUI;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Module.Testing
{
	[TestedType(typeof(K84ReportsController))]
	sealed class K84ReportsControllerTest : ZControllerBasherTest
	{
		public void TestCheckPoints()
		{
			var message = Factory.New<Business.EDIMessage>();
			var controller = new K84ReportsController();
			AssertEquals(Env.Security.CAK84ReportsView, controller.GetCheckPointForView(message));
		}

		public void TestCorrectForm()
		{
			AssertType<EDIMessageWithDocumentsForm>(Controller.ShowNewForm());
		}

		public override Type ControllerToBashType => typeof(K84ReportsController);

		protected override ControllerID GetControllerID() => ControllerIDs.Customs.CA.K84Reports;
	}
}
