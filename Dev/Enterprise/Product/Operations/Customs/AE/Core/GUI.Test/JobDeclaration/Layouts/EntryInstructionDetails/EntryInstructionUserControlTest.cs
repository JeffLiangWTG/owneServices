using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.Customs.AE.Business;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AE.GUI.Testing;

sealed class EntryInstructionUserControlTest : TestCaseWithFactory
{
	public void TestEntryInstructionGridColumnOrder()
	{
		using var userControl = new EntryInstructionUserControl();
		var controlGrid = (ZGrid)userControl.Controls.Find("EntryInstructionsGrid", searchAllChildren: true).Single();
		var columnsOrder = controlGrid.ColumnStyles.Cast<ZGridColumnInfo>().Select(x => x.ColumnName).ToArray();
		var expectedColumnOrder = new[]
		{
			CusEntryInstruction.Schema.CEI_Style,
			CusEntryInstruction.Schema.CEI_Description,
			CusEntryInstruction.Schema.CEI_DateForDuty,
			CusEntryInstruction.Schema.CEI_DeclarationPurpose,
			CusEntryInstruction.Schema.CEI_DeclarationPurposeDetails,
		};
		AssertArrayEqualsByElements(expectedColumnOrder, columnsOrder);
	}

	public void TestOnLoad()
	{
		using var form = new ZForm();
		using var control = new EntryInstructionUserControl();
		control.JobDeclaration = Declaration;
		form.Controls.Add(control);
		form.Show();

		AssertEquals(typeof(LayoutEntryInstructionDetailBasicUserControl), control.FindSingle<ZDynamicControlCreationUserControl>("DetailsUserControl").UserControlType);
		AssertEquals(typeof(DocumentAvailabilityUserControl), control.FindSingle<ZDynamicControlCreationUserControl>("DocumentAvailabilityUserControl").UserControlType);
	}

	public void TestDocumentAvailabilityTabPageCaption()
	{
		using var form = new ZForm();
		using var control = new EntryInstructionUserControl();
		control.JobDeclaration = Declaration;
		form.Controls.Add(control);
		form.Show();

		AssertEquals("Document Availability", control.FindSingle<ZTabPage>("EntryInstructionDocumentAvailabilityTabPage").CaptionResourceString.Caption);
	}

	public void TestTabPagesOrder()
	{
		var tabOrderThatMustBeMaintained = new string[] { "EntryInstructionDetailsTabPage", "EntryInstructionDocumentAvailabilityTabPage" };
		using var form = new ZForm();
		using var control = new EntryInstructionUserControl();
		control.JobDeclaration = Declaration;
		form.Controls.Add(control);
		form.Show();

		var instructionTabControl = control.FindSingle<ZTabControl>("EntryInstructionTabControl");
		var tabPagesOrder = instructionTabControl.AllTabPages.OfType<ZTabPage>().Where(tp => tp.TabVisible).Select(tp => tp.Name).ToArray();

		AssertArrayEqualsByElements(tabOrderThatMustBeMaintained, tabPagesOrder);
	}

	JobDeclaration Declaration => declaration ??= Factory.New<JobDeclaration>();
	JobDeclaration declaration;
}
