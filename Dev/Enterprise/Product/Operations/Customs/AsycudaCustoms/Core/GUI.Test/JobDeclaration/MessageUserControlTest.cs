using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.AsycudaCustoms.Business;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using CusEntryHeader = Enterprise.Customs.AsycudaCustoms.Business.CusEntryHeader;

namespace Enterprise.Customs.AsycudaCustoms.GUI.Testing
{
	class MessageUserControlTest : TestCaseWithFactory
	{
		public void TestSetupEntryHeaderColumns()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.CustomsEntryHeaders.AddNew();
			using (var form = new ZForm(declaration))
			using (var userControl = new MessageUserControl())
			{
				form.Controls.Add(userControl);
				form.Show();
				var grid = userControl.EntriesBoundGrid;
				CombineAssertions(() =>
				{
					AssertNotNull("User control should have CH_BondValidToDate column", grid.GetColumnStyle(CusEntryHeader.Schema.CH_BondValidToDate));
					AssertNotNull("User control should have CH_BondAcquittedDate column", grid.GetColumnStyle(CusEntryHeader.Schema.CH_BondAcquittedDate));

					AssertEquals("CH_EntryReleaseDate Column is NOT readonly", false, grid.GetColumnStyle(CusEntryHeader.Schema.CH_EntryReleaseDate).IsReadOnly);

					var entryNumberColumn = grid.GetColumnStyle(CusEntryHeader.Schema.EntryNumber);
					AssertNotNull("User control should have EntryNumber column", entryNumberColumn);
					AssertEquals("EntryNumber Column is NOT readonly", false, entryNumberColumn.IsReadOnly);
					AssertEquals("EntryNumber Column is Uppercase", CharacterCasing.Upper, entryNumberColumn.CharacterCasing);

					var entryHeaderStatusDescriptionColumn = grid.GetColumnStyle(CusEntryHeader.Schema.EntryHeaderStatusDescription);
					AssertNotNull("User control should have EntryHeaderStatusDescription column", entryHeaderStatusDescriptionColumn);
					AssertEquals("EntryHeaderStatusDescription Column is readonly", true, entryHeaderStatusDescriptionColumn.IsReadOnly);
					AssertEquals("EntryHeaderStatusDescription Column is Uppercase", CharacterCasing.Upper, entryHeaderStatusDescriptionColumn.CharacterCasing);

					var statusColumn = grid.GetColumnStyle(CusEntryHeader.Schema.CH_EntryStatus);
					AssertNotNull("User control should have CH_EntryStatus column", statusColumn);
					AssertEquals("CH_Status Column is readonly", false, statusColumn.IsReadOnly);
					AssertEquals("CH_Status Column is ZDropEditColumnStyleInfo", typeof(ZDropEditColumnStyleInfo), statusColumn.GetType());

					AssertEquals("Warehouse Status Column is readonly", true, grid.GetColumnStyle(CusEntryHeader.Schema.CH_WarehouseTransactionStatus).IsReadOnly);
					AssertEquals("CustomsValue Column is readonly", true, grid.GetColumnStyle(CusEntryHeader.Schema.CustomsValue).IsReadOnly);
					AssertEquals("ValueForVAT Column is readonly", true, grid.GetColumnStyle(CusEntryHeader.Schema.ValueForVAT).IsReadOnly);

					AssertEquals("Has Manual Warehouse Update Column is readonly", true, grid.GetColumnStyle(CusEntryHeader.Schema.CH_HasManualWhsUpdate).IsReadOnly);
				});
			}
		}

		public void TestEntryNumberIsReadOnly()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CustomsEntryHeaders.AddNew();

			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();

				var messagesTabPage = ((CustomsBrokerageUserControl)form.CustomsBrokerageUserControl).MessagesTabPage;
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = messagesTabPage;
				var grid = messagesTabPage.FindSingle<MessageUserControl>().EntriesBoundGrid;
				AssertEquals(true, grid.Visible);

				grid.Focus();
				grid.ListManager.Position = 0;
				AssertEquals(false, grid.GetColumnStyle(CusEntryHeader.Schema.EntryNumber).IsReadOnly);
			}
		}

		public void TestAddEntryLineColumns()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.CustomsEntryHeaders.AddNew();
			using (var form = new ZForm(declaration))
			using (var userControl = new MessageUserControl())
			{
				form.Controls.Add(userControl);
				form.Show();
				CombineAssertions(() =>
				{
					var grid = userControl.FindSingle<ZGrid>("EntryLineGrid");
					AssertEquals("CL_CustomsValue is readonly", true, grid.GetColumnStyle(Business.CusEntryLine.Schema.CL_CustomsValue).IsReadOnly);
					AssertEquals("CL_ValueForVAT is readonly", true, grid.GetColumnStyle(Business.CusEntryLine.Schema.CL_ValueForVAT).IsReadOnly);
				});
			}
		}
	}
}
