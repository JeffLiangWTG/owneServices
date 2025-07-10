using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.GUI.Plugin;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GB.GUI.JobDeclarationForms.Testing
{
	class EntryInstructionDetailsUserControlTest : TestCaseWithFactory
	{
		public void TestDetailsUserControlType()
		{
			AssertEquals(typeof(EntryInstructionDetailBasicUserControl), control.FindSingle<ZDynamicControlCreationUserControl>("DetailsUserControl").UserControlType);
		}

		public void TestGridUserControlType()
		{
			AssertEquals(typeof(EntryInstructionGridUserControl), control.FindSingle<ZDynamicControlCreationUserControl>("EntryInstructionGridUserControl").UserControlType);
		}

		public void TestAdditionalInfoTabPage()
		{
			AssertEquals("[UCC 2/2] Additional Info", control.FindSingle<ZTabControl>("EntryInstructionTabControl").GetTabPage("AdditionalInfoTabPage").CaptionResourceString.Caption);
		}

		public void TestAdditionalInfosUserControl()
		{
			var additionalInfoTab = control.FindSingle<ZTabPage>("AdditionalInfoTabPage");
			control.EntryInstructionTabControl.SelectedTab = additionalInfoTab;

			var additionalInfoUserControl = control.FindSingle<ZDynamicControlCreationUserControl>("AdditionalInfoUserControl");
			AssertEquals(DockStyle.Fill, additionalInfoUserControl.Dock);
			AssertEquals(typeof(GBAdditionalInfosUserControl), additionalInfoUserControl.UserControlType);
		}

		public void TestAdditionalInfosGrid()
		{
			control.FindSingle<ZTabControl>("EntryInstructionTabControl").SelectTab("AdditionalInfoTabPage");
			var grid = control.FindSingle<ZGrid>("AdditionalInfosGrid");
			CombineAssertions(() =>
			{
				AssertEquals("DataMember", "CustomsEntryInstructions.AdditionalInfos", grid.DataMember);
				AssertEquals("ColumnLayoutContext", "DEC", grid.ColumnLayoutContext);
			});
		}

		public void TestFiscalReferencesTabPage()
		{
			AssertEquals("Fiscal References", control.FindSingle<ZTabControl>("EntryInstructionTabControl").GetTabPage("FiscalReferencesTabPage").CaptionResourceString.Caption);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var declaration = Factory.New<JobDeclaration>();
			declaration.CustomsEntryInstructions.AddNew();
			form = new ZForm(declaration);
			control = new EntryInstructionDetailsUserControl();
			form.Controls.Add(control);
			form.Show();
		}

		protected override void TearDown()
		{
			base.TearDown();
			control?.Dispose();
			form?.Dispose();
		}

		EntryInstructionDetailsUserControl control;
		ZForm form;
	}
}
