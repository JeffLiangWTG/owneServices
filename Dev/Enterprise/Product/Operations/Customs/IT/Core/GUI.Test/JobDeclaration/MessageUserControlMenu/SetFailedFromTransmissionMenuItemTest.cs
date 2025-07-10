using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.IT.Business;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.GUI.Testing;

sealed class SetFailedFromTransmissionMenuItemTest : TestCaseWithFactory
{
	public void TestSetEntryAsFailedFromTransmission()
	{
		using (var form = new ZForm(declaration))
		using (var messageUserControl = new MessageUserControl())
		{
			entryHeader.CH_BGMReference = "12345";
			form.Controls.Add(messageUserControl);
			form.Show();

			var entriesGrid = messageUserControl.FindSingle<ZGrid>("EntriesBoundGrid");
			var menuItem = entriesGrid.ContextMenu.MenuItems.FindByText("Set Entry as Failed from Transmission");
			entriesGrid.Select(0);
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);

			menuItem.PerformClick();

			AssertEquals("When Entry Status is empty, it should be possibile to set FFT", "One Entry was set to Failed from Transmission", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals("When user presses OK, CH_Status must be set to FFT", "FFT", entryHeader.CH_Status);
			Assert("JobDeclaration should have Set log to FFT log", entryHeader.Logs.HasLogWith(x => x.SL_SE_NKEvent == "CSO" && x.SL_Reference.Contains("Entry set to FFT, original status:")));
		}
	}

	public void TestSetEntryAsFailedFromTransmission_CancelOperation()
	{
		using (var form = new ZForm(declaration))
		using (var messageUserControl = new MessageUserControl())
		{
			entryHeader.CH_BGMReference = "12345";
			form.Controls.Add(messageUserControl);
			form.Show();

			var entriesGrid = messageUserControl.FindSingle<ZGrid>("EntriesBoundGrid");
			var menuItem = entriesGrid.ContextMenu.MenuItems.FindByText("Set Entry as Failed from Transmission");
			entriesGrid.Select(0);
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);

			menuItem.PerformClick();

			AssertEquals("When the user press cancel, the CH_Status shouldn't change", "", entryHeader.CH_Status);
			AssertEquals("JobDeclaration should have Set log to FFT log", false, entryHeader.Logs.HasLogWith(x => x.SL_SE_NKEvent == "CSO" && x.SL_Reference.Contains("Entry set to FFT, original status:")));
		}
	}

	public void TestSetEntryAsFailedFromTransmission_EmptySelection()
	{
		using (var form = new ZForm(declaration))
		using (var messageUserControl = new MessageUserControl())
		{
			form.Controls.Add(messageUserControl);
			form.Show();

			var entriesGrid = messageUserControl.FindSingle<ZGrid>("EntriesBoundGrid");
			var menuItem = entriesGrid.ContextMenu.MenuItems.FindByText("Set Entry as Failed from Transmission");
			menuItem.PerformClick();

			AssertContains("At least one row should be selected", "Please select a row first", UnitTestUserNotification.Instance.LastMessage.Text);
		}
	}

	public void TestSetEntryAsFailedFromTransmission_HigherPriority()
	{
		entryHeader.CH_EntryStatus = ITEntryStatusList.Codes.Registered;
		using (var form = new ZForm(declaration))
		using (var messageUserControl = new MessageUserControl())
		{
			form.Controls.Add(messageUserControl);
			form.Show();

			var entriesGrid = messageUserControl.FindSingle<ZGrid>("EntriesBoundGrid");
			var menuItem = entriesGrid.ContextMenu.MenuItems.FindByText("Set Entry as Failed from Transmission");
			entriesGrid.Select(0);
			menuItem.PerformClick();

			AssertContains("When Entry Status is REG or a status with higher priority, it shouldn't be possibile to set FFT", "You cannot set an Entry with entry status", UnitTestUserNotification.Instance.LastMessage.Text);
		}
	}

	public void TestSetEntryAsFailedFromTransmission_OlderIdoc()
	{
		var message = entryHeader.Messages.AddNew();
		message.EM_MessageType = SADConstants.CustomsInterchangeType.IdocR;
		message.EM_SystemCreateTimeUtc = ZDateTime.Now.AddDays(-1);

		using (var form = new ZForm(declaration))
		using (var messageUserControl = new MessageUserControl())
		{
			form.Controls.Add(messageUserControl);
			form.Show();

			var entriesGrid = messageUserControl.FindSingle<ZGrid>("EntriesBoundGrid");
			var menuItem = entriesGrid.ContextMenu.MenuItems.FindByText("Set Entry as Failed from Transmission");
			entriesGrid.Select(0);
			menuItem.PerformClick();

			AssertContains("When exists an Idoc that has not been created less than one hour before, it should be possible to set FFT", "One Entry was set to Failed from Transmission", UnitTestUserNotification.Instance.LastMessage.Text);
		}
	}

	public void TestSetEntryAsFailedFromTransmission_RecentIdoc()
	{
		var message = entryHeader.Messages.AddNew();
		message.EM_MessageType = SADConstants.CustomsInterchangeType.IdocR;
		message.EM_SystemCreateTimeUtc = ZDateTime.Now;

		using (var form = new ZForm(declaration))
		using (var messageUserControl = new MessageUserControl())
		{
			form.Controls.Add(messageUserControl);
			form.Show();

			var entriesGrid = messageUserControl.FindSingle<ZGrid>("EntriesBoundGrid");
			var menuItem = entriesGrid.ContextMenu.MenuItems.FindByText("Set Entry as Failed from Transmission");
			entriesGrid.Select(0);
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
			menuItem.PerformClick();

			AssertContains("When exists an Idoc that has been created less than one hour before, it should be possible to set FFT", "One Entry was set to Failed from Transmission", UnitTestUserNotification.Instance.LastMessage.Text);
		}
	}

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
		declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
		entryHeader = declaration.CustomsEntryHeaders.AddNew();
		Factory.Save();
	}

	JobDeclaration declaration;
	CusEntryHeader entryHeader;
}
