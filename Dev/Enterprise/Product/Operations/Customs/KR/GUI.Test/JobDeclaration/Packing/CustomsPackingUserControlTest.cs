using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.KR;
using Enterprise.Customs.KR.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI.Testing
{
	sealed class CustomsPackingUserControlTest : TestCaseWithFactory
	{
		public void TestHouseBillsGrid()
		{
			using var testForm = new JobDeclarationFormForTest(jobDeclaration);
			testForm.Show();
			var brokerageControl = testForm.CustomsBrokerageUserControl as CustomsBrokerageUserControlForTest;
			brokerageControl.MainTabControl.SelectedTab = brokerageControl.PackingTabPage;

			using var control = (ImportCustomsPackingUserControl)brokerageControl.Packing;
			var grid = control.HouseBillsGrid;
			var index = 0;
			AssertEquals(grid.Columns[index++].ColumnName, Bill.Schema.CU_BillType);
			AssertEquals(grid.Columns[index++].ColumnName, Bill.Schema.CU_BillNum);
			AssertEquals(grid.Columns[index++].ColumnName, Bill.Schema.CU_IssueDate);
			AssertEquals(grid.Columns[index++].ColumnName, Bill.Schema.CU_ParentBillUniqueCode);
			AssertEquals(grid.Columns[index++].ColumnName, Bill.Schema.CU_HBSplitDecInd);
			AssertEquals(grid.Columns[index++].ColumnName, Bill.Schema.CU_HBSplitDecReasonCode);
			AssertEquals(grid.Columns[index++].ColumnName, nameof(Bill.HBSplitDecReasonRemark));
		}

		public void TestPackingDetailsIsNotShown()
		{
			using var testForm = new JobDeclarationFormForTest(jobDeclaration);
			testForm.Show();
			var brokerageControl = testForm.CustomsBrokerageUserControl as CustomsBrokerageUserControlForTest;
			brokerageControl.MainTabControl.SelectedTab = brokerageControl.PackingTabPage;

			using var control = (ImportCustomsPackingUserControl)brokerageControl.Packing;
			var panel = control.FindSingle<ZPanel>("PackingDetailsPanel");
			Assert(!panel.Visible);
		}

		protected override void SetUp()
		{
			base.SetUp();
			jobDeclaration = Factory.New<JobDeclaration>();
			jobDeclaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;
		}

		JobDeclaration jobDeclaration;
	}
}
