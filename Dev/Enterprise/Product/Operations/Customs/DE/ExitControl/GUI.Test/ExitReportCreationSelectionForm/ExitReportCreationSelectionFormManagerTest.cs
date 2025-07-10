using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.DE.ExitControl.Business;
using Enterprise.Customs.EU.ExitControl.GUI;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.DE.ExitControl.GUI.Testing
{
	sealed class ExitReportCreationSelectionFormManagerTest : TestCaseWithFactory
	{
		public void TestShowExitReportCreationSelectionForm_NotShownCXC_StatusLessThan310()
		{
			consignment.CXC_Status = "123";

			UnitTestUserNotification.Instance.ClearMessages();
			ExitReportCreationSelectionFormManager.ShowExitReportCreationSelectionForm(consignment, null);
			AssertNullOrEmpty(UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestShowExitReportCreationSelectionForm_ShownCXC_StatusGreaterThan310()
		{
			consignment.CXC_Status = "410";

			UnitTestUserNotification.Instance.ClearMessages();
			ExitReportCreationSelectionFormManager.ShowExitReportCreationSelectionForm(consignment, null);
			AssertEquals("A consignment should have at least one item.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		protected override void SetUp()
		{
			base.SetUp();

			var exitHeader = Factory.New<CusExitHeader>();
			consignment = exitHeader.CusExitConsignments.AddNew();
		}

		CusExitConsignment consignment;
	}
}
