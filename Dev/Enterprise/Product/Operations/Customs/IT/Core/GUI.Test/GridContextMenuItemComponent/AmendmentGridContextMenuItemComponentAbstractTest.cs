using System.Linq;
using System.Windows.Forms;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.IT.GUI.PlugIn;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.GUI.Testing;

abstract class AmendmentGridContextMenuItemComponentAbstractTest : GridContextMenuItemComponentAbstractTest<CusEntryHeader>
{
	public void TestSetAsAmendment()
	{
		declaration.JE_MessageType = GetMessageType();

		using (var form = new ZForm(declaration))
		using (var control = new MessageUserControl())
		{
			entryHeader.CH_EntryStatus = "REG";
			form.Controls.Add(control);
			form.SetDataBinding(declaration, ".");
			form.Show();

			var entriesBoundGrid = (ZGrid)control.Controls.Find("EntriesBoundGrid", true).SingleOrDefault();
			AssertNotNull(entriesBoundGrid);

			var menu = entriesBoundGrid.ContextMenu;
			var menuItem = menu.MenuItems.FindByText("Set Entry as Amendment", true);
			entriesBoundGrid.Select(0);

			AssertNotNull("Set Entry as Amendment menu item must be available", menuItem);
			AssertEquals("Set Entry as Amendment Menu item must be visible", true, menuItem.Visible);
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
			menuItem.PerformClick();

			AssertEquals("After Entry has been set to Amending, feedback message is shown to user", "One Entry was set to Amending", UnitTestUserNotification.Instance.LastMessage.Text);
			Assert("A log entry related to AMG must be found", entryHeader.Logs.HasLogWith(x => x.SL_SE_NKEvent == "CSO" && x.SL_Reference.Contains("Entry set to AMG, original status: [REG | ]")));
		}
	}

	public void TestSetAsAmendment_NoMRN()
	{
		declaration.JE_MessageType = GetMessageType();

		using (var form = new ZForm(declaration))
		using (var control = new MessageUserControl())
		{
			declaration.CustomsEntryHeaders.RemoveAndDeleteAll();
			entryHeader = declaration.CustomsEntryHeaders.AddNew();
			form.Controls.Add(control);
			form.SetDataBinding(declaration, ".");
			form.Show();

			var entriesBoundGrid = control.FindSingleOrDefault<ZGrid>("EntriesBoundGrid");
			var menu = entriesBoundGrid.ContextMenu;
			var menuItem = menu.MenuItems.FindByText("Set Entry as Amendment", true);
			entriesBoundGrid.Select(0);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

			ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(form =>
			{
				var entryAmendmentForm = (EntryAmendmentForm)form;
				entryAmendmentForm.BusinessEntity.MovementReferenceNumber = "24ITQYG08AAB1956J4";
			});

			menuItem.PerformClick();

			AssertEquals("After Entry has been set to Amending, feedback message is shown to user", "One Entry was set to Amending", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals("MRN was added", "24ITQYG08AAB1956J4", entryHeader.MovementReferenceNumber);
		}
	}

	public void TestSetAsAmendment_CancelOperation()
	{
		declaration.JE_MessageType = GetMessageType();

		using (var form = new ZForm(declaration))
		using (var control = new MessageUserControl())
		{
			entryHeader.CH_EntryStatus = "REG";
			form.Controls.Add(control);
			form.SetDataBinding(declaration, ".");
			form.Show();

			var entriesBoundGrid = (ZGrid)control.Controls.Find("EntriesBoundGrid", true).SingleOrDefault();
			AssertNotNull(entriesBoundGrid);

			var menu = entriesBoundGrid.ContextMenu;
			var menuItem = menu.MenuItems.FindByText("Set Entry as Amendment", true);
			entriesBoundGrid.Select(0);

			AssertNotNull("Set Entry as Amendment menu item must be available", menuItem);
			AssertEquals("Set Entry as Amendment Menu item must be visible", true, menuItem.Visible);
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
			menuItem.PerformClick();

			AssertEquals("If user presses Cancel, no feedback message is shown to user", "Are you sure you want to set this Entry as AMENDING?", UnitTestUserNotification.Instance.LastMessage.Text);
			Assert("No log entry related to AMG must be found", !entryHeader.Logs.HasLogWith(x => x.SL_SE_NKEvent == "CSO" && x.SL_Reference.Contains("Entry set to AMG, original status:")));
		}
	}

	public void TestSetAsAmendment_NoMRN_CancelOperation()
	{
		declaration.JE_MessageType = GetMessageType();

		using (var form = new ZForm(declaration))
		using (var control = new MessageUserControl())
		{
			declaration.CustomsEntryHeaders.RemoveAndDeleteAll();
			entryHeader = declaration.CustomsEntryHeaders.AddNew();
			form.Controls.Add(control);
			form.SetDataBinding(declaration, ".");
			form.Show();

			var entriesBoundGrid = control.FindSingleOrDefault<ZGrid>("EntriesBoundGrid");
			var menu = entriesBoundGrid.ContextMenu;
			var menuItem = menu.MenuItems.FindByText("Set Entry as Amendment", true);
			entriesBoundGrid.Select(0);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
			menuItem.PerformClick();

			AssertEquals(typeof(EntryAmendmentForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
			AssertNull("If user presses Cancel, no feedback message is shown to user", UnitTestUserNotification.Instance.LastMessage.Text);
		}
	}

	protected void AssertMenuItemVisible(ZString entryStatus, bool expectedValue)
	{
		entryHeader.CH_EntryStatus = entryStatus;

		using (var form = new ZForm(declaration))
		using (var control = new MessageUserControl())
		{
			form.Controls.Add(control);
			form.SetDataBinding(declaration, ".");
			form.Show();

			var entriesBoundGrid = (ZGrid)control.Controls.Find("EntriesBoundGrid", true).SingleOrDefault();
			AssertNotNull(entriesBoundGrid);

			control.Show();
			entriesBoundGrid.Select(0);
			var menu = entriesBoundGrid.ContextMenu;
			menu.DoPopup();

			var menuItem = menu.MenuItems.FindByText("Set Entry as Amendment", true);
			AssertNotNull("Set Entry as Amendment menu item must be available", menuItem);
			AssertEquals($"When Entry Header status is {entryStatus}", expectedValue, menuItem.Visible);
		}
	}

	protected abstract ZString GetMessageType();

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.NewWithValidTestData<JobDeclaration>();
		declaration.JE_MessageType = GetMessageType();
		declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();

		entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryHeader.MovementReferenceNumberSetter("23ITMRN00001");
	}

	protected override GridContextMenuItemComponent<CusEntryHeader> GetNewContextMenuItemComponent(ZGrid grid)
		=> new AmendmentGridContextMenuItemComponent(grid);

	protected override string MenuItemText => "Set Entry as Amendment";

	protected JobDeclaration declaration;
	CusEntryHeader entryHeader;
}
