using System.Linq;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.IT.Business.Testing;
using Enterprise.Customs.IT.GUI.PlugIn;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;
using CusEntryHeader = Enterprise.Customs.IT.Business.Declaration.CusEntryHeader;

namespace Enterprise.Customs.IT.GUI.Testing;

[TestedType(typeof(ManualReleaseGridContextMenuItemComponent))]
sealed class ManualReleaseGridContextMenuItemComponentTest : GridContextMenuItemComponentAbstractTest<CusEntryHeader>
{
	public void TestManualReleaseMenuItemVisible()
	{
		using (var control = new MessageUserControl())
		{
			var entriesBoundGrid = (ZGrid)control.Controls.Find("EntriesBoundGrid", true).SingleOrDefault();
			AssertNotNull(entriesBoundGrid);

			control.Show();
			var menu = entriesBoundGrid.ContextMenu;

			var menuItem = menu.MenuItems.FindByText("Manual Release", true);
			AssertNotNull(menuItem);
			AssertEquals("ManualRelease Menu item must be visible", true, menuItem.Visible);
		}
	}

	public void TestManualReleaseMenuItemClick_WithNoRegistrationNumberNorMRN()
	{
		using (var form = new ZForm(declaration))
		using (var control = new MessageUserControl())
		{
			form.Controls.Add(control);
			form.SetDataBinding(declaration, ".");
			form.Show();

			var entriesBoundGrid = (ZGrid)control.Controls.Find("EntriesBoundGrid", true).SingleOrDefault();
			AssertNotNull(entriesBoundGrid);

			entriesBoundGrid.Select(0);

			var menu = entriesBoundGrid.ContextMenu;
			var manualReleaseMenuItem = menu.MenuItems.FindByText("Manual Release", true);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			manualReleaseMenuItem.PerformClick();

			AssertContains("This Entry has no Registration number nor MRN, it is not possible to insert the Release code.", UnitTestUserNotification.Instance.LastMessage.Text);
		}
	}

	public void TestManualReleaseMenuItemClick_WhenEntryHasMRN()
	{
		entryHeader.MovementReferenceNumberSetter("123");

		using (var form = new ZForm(declaration))
		using (var control = new MessageUserControl())
		{
			form.Controls.Add(control);
			form.SetDataBinding(declaration, ".");
			form.Show();

			var entriesBoundGrid = (ZGrid)control.Controls.Find("EntriesBoundGrid", true).SingleOrDefault();
			AssertNotNull(entriesBoundGrid);

			entriesBoundGrid.Select(0);

			var menu = entriesBoundGrid.ContextMenu;
			var manualReleaseMenuItem = menu.MenuItems.FindByText("Manual Release", true);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			manualReleaseMenuItem.PerformClick();

			AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals(typeof(EntryManualReleaseForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
		}
	}

	public void TestManualReleaseMenuItemClick_WhenEntryHasRegistrationNumber()
	{
		Factory.NewCusEntryNumber(entryHeader, entryType: "REG", entryNum: "4 T-2343G", issueDate: null);

		using (var form = new ZForm(declaration))
		using (var control = new MessageUserControl())
		{
			form.Controls.Add(control);
			form.SetDataBinding(declaration, ".");
			form.Show();

			var entriesBoundGrid = (ZGrid)control.Controls.Find("EntriesBoundGrid", true).SingleOrDefault();
			AssertNotNull(entriesBoundGrid);

			entriesBoundGrid.Select(0);

			var menu = entriesBoundGrid.ContextMenu;
			var manualReleaseMenuItem = menu.MenuItems.FindByText("Manual Release", true);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			manualReleaseMenuItem.PerformClick();

			AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals(typeof(EntryManualReleaseForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
		}
	}

	public void TestManualReleaseMenuItemClick_WithExistingSystemGeneratedReleaseCode()
	{
		Factory.NewCusEntryNumber(entryHeader, entryType: "CLR", entryNum: "4 T-2343G", issueDate: null);

		using (var form = new ZForm(declaration))
		using (var control = new MessageUserControl())
		{
			form.Controls.Add(control);
			form.SetDataBinding(declaration, ".");
			form.Show();

			var entriesBoundGrid = (ZGrid)control.Controls.Find("EntriesBoundGrid", true).SingleOrDefault();
			AssertNotNull(entriesBoundGrid);

			entriesBoundGrid.Select(0);

			var menu = entriesBoundGrid.ContextMenu;
			var manualReleaseMenuItem = menu.MenuItems.FindByText("Manual Release", true);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			manualReleaseMenuItem.PerformClick();

			AssertContains("This Entry has a System generated Release Code, it is not possible to change it", UnitTestUserNotification.Instance.LastMessage.Text);
		}
	}

	public void TestManualReleaseMenuItemClick_WithExistingNotSystemGeneratedReleaseCode()
	{
		entryHeader.MovementReferenceNumberSetter("123");
		var entryNum = Factory.NewCusEntryNumber(entryHeader, entryType: "CLR", entryNum: "4 T-2343G", issueDate: null);
		entryNum.CE_EntryIsSystemGenerated = false;

		using (var form = new ZForm(declaration))
		using (var control = new MessageUserControl())
		{
			form.Controls.Add(control);
			form.SetDataBinding(declaration, ".");
			form.Show();

			var entriesBoundGrid = (ZGrid)control.Controls.Find("EntriesBoundGrid", true).SingleOrDefault();
			AssertNotNull(entriesBoundGrid);

			entriesBoundGrid.Select(0);

			var menu = entriesBoundGrid.ContextMenu;
			var manualReleaseMenuItem = menu.MenuItems.FindByText("Manual Release", true);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			manualReleaseMenuItem.PerformClick();

			AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals(typeof(EntryManualReleaseForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
		}
	}

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.NewWithValidTestData<JobDeclaration>();
		declaration.JE_MessageType = "EXP";

		entryHeader = declaration.CustomsEntryHeaders.AddNew();
	}

	protected override GridContextMenuItemComponent<CusEntryHeader> GetNewContextMenuItemComponent(ZGrid grid)
	{
		return new ManualReleaseGridContextMenuItemComponent(grid);
	}

	JobDeclaration declaration;
	CusEntryHeader entryHeader;

	protected override string MenuItemText => "Manual Release";
}
