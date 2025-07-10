using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GB.GUI.JobDeclarationForms.Testing
{
	public class MessageUserControlTest : TestCaseWithFactory
	{
		public void TestChiefEDIMenuForEntriesVisibility()
		{
			var customsInterface = new LocalCountryCustomsInterface();
			customsInterface.RecipientID = "RecipientID";
			customsInterface.SubmissionType = Customs.Business.DeclarationApplicationCodeListForRegistry.Codes.BothBuiltInDefaulted;
			using (CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, customsInterface))
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Interfaced;
				declaration.ActiveEntryHeaders.AddNew();

				using (var form = new ZForm(declaration))
				using (var control = new MessageUserControl())
				{
					form.Controls.Add(control);
					form.Show();
					control.SetMenusEntryOnPopup(null, null);
					AssertEquals(false, control.gbChiefEDIMenuForEntries.Visible);
				}
			}
		}

		public void TestLRN()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = GB.Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			declaration.ActiveEntryHeaders.AddNew();

			using (var form = new ZForm(declaration))
			using (var control = new MessageUserControl())
			{
				form.Controls.Add(control);
				form.Show();

				CombineAssertions(() =>
				{
					var lrnColumn = control.EntriesBoundGrid.GetColumnStyle(CusEntryHeader.Schema.LRN);
					AssertNotNull("User control should have EntryTypeFriendlyName column for CDS", lrnColumn);
					AssertEquals("EntryTypeFriendlyName column is visible for CDS", true, lrnColumn.IsVisible);
					AssertEquals("EntryTypeFriendlyName column is avaialble for CDS", false, lrnColumn.IsUnavailable);
				});
			}
		}

		public void TestEntryLineColumns()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.ActiveEntryHeaders.AddNew().AllEntryLines.AddNew();
			using (var form = new ZForm(declaration))
			using (var control = new MessageUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				var procColumn = control.FindSingle<ZGrid>("EntryLineGrid").GetColumnStyle("ProcedureCodeWithoutConcession");
				AssertNotNull(procColumn);
				Assert(procColumn.IsVisible);
			}
		}

		public void TestSetupEntryGridContextMenuAndColumns()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.ActiveEntryHeaders.AddNew();

			using (var form = new ZForm(declaration))
			using (var control = new MessageUserControl())
			{
				form.Controls.Add(control);
				form.Show();

				CombineAssertions(() =>
				{
					var entryTypeColumn = control.EntriesBoundGrid.GetColumnStyle(CusEntryHeader.Schema.EntryTypeFriendlyName);
					AssertNotNull("User control should have EntryTypeFriendlyName column", entryTypeColumn);
					AssertEquals("EntryTypeFriendlyName should be visible", true, entryTypeColumn.IsVisible);

					AssertNotNull("User control should have CH_MasterUCR column", control.EntriesBoundGrid.Columns[CusEntryHeader.Schema.CH_MasterUCR]);
					AssertNotNull("User control should have IsCancelledWithCustoms column", control.EntriesBoundGrid.GetColumnStyle(CusEntryHeader.Schema.IsCancelledWithCustoms));
					AssertNotNull("User control should have ImportClearanceStatusICS column", control.EntriesBoundGrid.GetColumnStyle(CusEntryHeader.Schema.ImportClearanceStatusICS));
					AssertNotNull("User control should have RouteOfEntry column", control.EntriesBoundGrid.GetColumnStyle(CusEntryHeader.Schema.RouteOfEntry));
					AssertNotNull("User control should have IrcInventoryReturnCode column", control.EntriesBoundGrid.GetColumnStyle(CusEntryHeader.Schema.IrcInventoryReturnCode));
					AssertNotNull("User control should have StyleOfEntrySOE column", control.EntriesBoundGrid.GetColumnStyle(CusEntryHeader.Schema.StyleOfEntrySOE));
				});
			}
		}

		public void TestDeclarationMessagesTabPage()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			declaration.ZG_Gateway = GatewayList.Codes.MCP_CUSDECOnly;

			using var form = new ZForm(declaration);
			using var userControl = new MessageUserControl();
			form.Controls.Add(userControl);
			form.Show();
			CombineAssertions(() =>
			{
				userControl.OnShown();
				var tabPage = userControl.DeclarationMessagesTabPage;
				AssertNull("EXP/MCP DeclarationMessagesTabPage", tabPage);

				declaration.JE_MessageType = MessageTypeList.Codes.Import;
				userControl.OnShown();
				tabPage = userControl.DeclarationMessagesTabPage;
				AssertNotNull("IMP/MCP DeclarationMessagesTabPage", tabPage);
				AssertEquals("CaptionResourceString.Caption", "Declaration Messages", tabPage.CaptionResourceString.Caption);
				AssertEquals("Index", 4, tabPage.TabIndex);
				AssertEquals("IMP/MCP TabRelevant", expected: true, tabPage.TabRelevant);

				declaration.ZG_Gateway = GatewayList.Codes.CDS;
				userControl.OnShown();
				AssertEquals("IMP/CDS TabRelevant", expected: true, tabPage.TabRelevant);

				declaration.JE_MessageType = MessageTypeList.Codes.Export;
				userControl.OnShown();
				AssertEquals("IMP/CDS TabRelevant", expected: false, tabPage.TabRelevant);
			});
		}
	}
}
