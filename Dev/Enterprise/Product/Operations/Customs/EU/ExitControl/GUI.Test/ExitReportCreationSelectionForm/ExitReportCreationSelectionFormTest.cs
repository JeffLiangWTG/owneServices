using System.Windows.Forms;
using Enterprise.Customs.EU.ExitControl.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.ExitControl.GUI.Testing
{
	[TestedType(typeof(ExitReportCreationSelectionForm))]
	class ExitReportCreationSelectionFormTest : ZFormBasherTest
	{
		public void TestClickOK()
		{
			var exitHeder = Factory.New<CusExitHeader>();
			var consignment = exitHeder.CusExitConsignments.AddNew();
			consignment.CXC_MovementReference = "AH";
			var consignmentItem = consignment.CusExitConsignmentItems.AddNew();
			consignmentItem.CCI_LineNumber = 1;
			var consignmentPackagePivot = consignmentItem.CusExitConsignmentPackagePivots.AddNew();
			var package = consignmentPackagePivot.Package;
			package.CXP_Calc_ShouldReportItem = true;
			using (var form = new ExitReportCreationSelectionForm(consignment, null))
			{
				form.Show();
				var okButton = form.FindSingleOrDefault<ZButton>(c => c.Name == "OKButton");
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				consignmentItem.CCI_Calc_ShouldReportItem = true;
				consignmentItem.CCI_Calc_ReportGrossMass = -1m;
				okButton.PerformClick();
				AssertEquals(DialogResult.None, form.DialogResult);
				AssertEquals("Error There are errors - can't save.", UnitTestUserNotification.Instance.LastMessage.ToString());

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				consignmentItem.CCI_Calc_ReportGrossMass = 2m;
				okButton.PerformClick();
				AssertEquals(DialogResult.OK, form.DialogResult);
			}
		}

		public void TestMinimizeBox()
		{
			using (var form = new ExitReportCreationSelectionForm(null, null))
			{
				AssertEquals(false, form.MinimizeBox);
			}
		}

		public void TestSelectAllAndClearAll()
		{
			var exitHeder = Factory.NewWithValidTestData<CusExitHeader>();
			var consignment = exitHeder.CusExitConsignments.AddNew();
			var consignmentItem1 = consignment.CusExitConsignmentItems.AddNew();
			consignmentItem1.CCI_LineNumber = 1;
			var consignmentItem2 = consignment.CusExitConsignmentItems.AddNew();
			consignmentItem2.CCI_LineNumber = 2;
			var consignmentPackagePivot1 = consignmentItem1.CusExitConsignmentPackagePivots.AddNew();
			var consignmentPackagePivot2 = consignmentItem2.CusExitConsignmentPackagePivots.AddNew();
			var consignmentPackagePivot3 = consignmentItem2.CusExitConsignmentPackagePivots.AddNew();
			var package1 = consignmentPackagePivot1.Package;
			var package2 = consignmentPackagePivot2.Package;
			var package3 = consignmentPackagePivot3.Package;
			Factory.Save();
			using (var form = new ExitReportCreationSelectionForm(consignment, null))
			{
				form.Show();
				CombineAssertions(() =>
				{
					AssertEquals(false, consignmentItem1.CCI_Calc_ShouldReportItem);
					AssertEquals(false, consignmentItem2.CCI_Calc_ShouldReportItem);
					AssertEquals(false, package1.CXP_Calc_ShouldReportItem);
					AssertEquals(false, package2.CXP_Calc_ShouldReportItem);
					AssertEquals(false, package3.CXP_Calc_ShouldReportItem);
				});

				var selectAllButton = form.FindSingleOrDefault<ZButton>(c => c.Name == "SelectAllButton");
				selectAllButton.PerformClick();
				CombineAssertions(() =>
				{
					AssertEquals(true, consignmentItem1.CCI_Calc_ShouldReportItem);
					AssertEquals(true, consignmentItem2.CCI_Calc_ShouldReportItem);
					AssertEquals(true, package1.CXP_Calc_ShouldReportItem);
					AssertEquals(true, package2.CXP_Calc_ShouldReportItem);
					AssertEquals(true, package3.CXP_Calc_ShouldReportItem);
				});

				var clearAllButton = form.FindSingleOrDefault<ZButton>(c => c.Name == "ClearAllButton");
				clearAllButton.PerformClick();
				CombineAssertions(() =>
				{
					AssertEquals(false, consignmentItem1.CCI_Calc_ShouldReportItem);
					AssertEquals(false, consignmentItem2.CCI_Calc_ShouldReportItem);
					AssertEquals(false, package1.CXP_Calc_ShouldReportItem);
					AssertEquals(false, package2.CXP_Calc_ShouldReportItem);
					AssertEquals(false, package3.CXP_Calc_ShouldReportItem);
				});
			}
		}

		protected override Form GetFormToBashCore()
		{
			var exitHeder = Factory.New<CusExitHeader>();
			var consignment = exitHeder.CusExitConsignments.AddNew();
			consignment.HasChanges = false;
			return new ExitReportCreationSelectionForm(consignment, null);
		}
	}
}
