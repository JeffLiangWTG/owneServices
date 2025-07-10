using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.JP.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.JP.GUI.Testing
{
	[TestedType(typeof(MessagesTabUserControl))]
	sealed class MessagesTabUserControlTest : TestCaseWithFactory
	{
		public void TestResetDefaultOrderAndVisibleColumnsForMessagesBoundGrid()
		{
			var declaration = Factory.New<JobDeclaration>();

			using var form = new JobDeclarationForm(declaration);
			var messagesTabPage = form.CustomsBrokerageUserControl.FindSingle<ZTabPage>("MessagesTabPage");
			form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = messagesTabPage;
			form.Show();

			var messagesTabUserControl = messagesTabPage.FindSingle<MessagesTabUserControl>("MessagesUserControl");
			var grid = messagesTabUserControl.FindSingle<ZGrid>("MessagesBoundGrid");
			var columnIndex = 0;
			grid.ResetColumns();
			CombineAssertions("", () =>
			{
				AssertEquals(17, grid.ColumnStyles.Count);
				AssertColumnDefaultOrderAndVisible(grid, columnIndex++, Common.EDIMessage.Schema.EM_Calc_ProcedureCode, true);
				AssertColumnDefaultOrderAndVisible(grid, columnIndex++, Common.EDIMessage.Schema.EM_Calc_ProcedureName, true);
				AssertColumnDefaultOrderAndVisible(grid, columnIndex++, Common.EDIMessage.Schema.EM_Calc_OutputInformationCode, true);
				AssertColumnDefaultOrderAndVisible(grid, columnIndex++, Common.EDIMessage.Schema.EM_Calc_OutputInformation, true);
				AssertColumnDefaultOrderAndVisible(grid, columnIndex++, Common.EDIMessage.Schema.EM_MessageNum, true);
				AssertColumnDefaultOrderAndVisible(grid, columnIndex++, Common.EDIMessage.Schema.EM_MessageType, true);
				AssertColumnDefaultOrderAndVisible(grid, columnIndex++, Common.EDIMessage.Schema.EM_MessageSubType, true);
				AssertColumnDefaultOrderAndVisible(grid, columnIndex++, Common.EDIMessage.Schema.EM_MessageDateTime, true);
				AssertColumnDefaultOrderAndVisible(grid, columnIndex++, Common.EDIMessage.Schema.EM_SystemCreateTimeUtc, true);
				AssertColumnDefaultOrderAndVisible(grid, columnIndex++, Common.EDIMessage.Schema.EM_InterchangeNumber, true);
				AssertColumnDefaultOrderAndVisible(grid, columnIndex++, Common.EDIMessage.Schema.EM_DateTimeInterchangeSent, true);
				AssertColumnDefaultOrderAndVisible(grid, columnIndex++, Common.EDIMessage.Schema.EM_User, true);
				AssertColumnDefaultOrderAndVisible(grid, columnIndex++, Common.EDIMessage.Schema.EM_Status, true);
				AssertColumnDefaultOrderAndVisible(grid, columnIndex++, Common.EDIMessage.Schema.EM_ReceiveTransmit, true);
				AssertColumnDefaultOrderAndVisible(grid, columnIndex++, Common.EDIMessage.Schema.EM_MessageSubTypeDescription, true);
				AssertColumnDefaultOrderAndVisible(grid, columnIndex++, Common.EDIMessage.Schema.EM_ApplicationReference, false);
				AssertColumnDefaultOrderAndVisible(grid, columnIndex++, Common.EDIMessage.Schema.EM_InterchangeStatus, false);
			});
		}

		public void TestBindingSourceDataSourceType()
		{
			AssertEquals(typeof(Common.EDIMessageCollection), userControl.BindingSource.DataSourceType);
		}

		protected override void SetUp()
		{
			base.SetUp();
			userControl = new MessagesTabUserControl();
		}
		MessagesTabUserControl userControl;

		protected override void TearDown()
		{
			base.TearDown();
			userControl.Dispose();
		}

		void AssertColumnDefaultOrderAndVisible(ZGrid grid, ZInt columnIndex, string expectedColumnName, bool expectedVisibility)
		{
			var column = grid.Columns[columnIndex];
			AssertEquals(expectedColumnName, column.ColumnName);
			AssertEquals(expectedVisibility, column.IsVisible);
		}
	}
}
