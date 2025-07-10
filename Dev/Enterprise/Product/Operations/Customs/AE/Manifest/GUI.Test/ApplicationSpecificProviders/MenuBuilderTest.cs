using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Customs.AE.Manifest.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.AE.Manifest.GUI.Testing;

[TestedType(typeof(MenuBuilder))]
sealed class MenuBuilderTest : TestCaseWithFactory
{
	public void TestMenuCaption()
	{
		var header = Factory.New<AsycudaManifestHeader>();
		using var form = new ZForm(header);
		AssertEquals("AE Manifest", new MenuBuilder(header, form).MenuCaption.EnglishText);
	}

	public void TestSendManifestMenu() => CombineAssertions(() =>
	{
		var header = Factory.New<AsycudaManifestHeader>();
		using var form = new ZForm(header);
		var menuBuilder = new MenuBuilder(header, form);
		var menuItems = menuBuilder.BuildMenu();
		AssertEquals("pre-condition: IsValidForMessage", false, header.AMA_RN_NKCountryInfo.HasNotifications());
		AssertNoExceptionThrown(() =>
		{
			menuItems.Single(x => x.Caption == "Send &Manifest").PerformClick();
		});
	});

	public void TestSendingSingle() => CombineAssertions(() =>
	{
		var header = CreateNewManifest();
		var bill = header.Bills.AddNew();
		bill.ABL_BillNumber = "B0001";

		using var menu = new ASYCUDA.GUI.Testing.AsycudaMenuForTest(header);
		using var form = new ZForm(header);
		form.Menu.MenuItems.Add(menu);
		form.Show();
		menu.OnPopup(EventArgs.Empty);
		var sendMenu = menu.MenuItems.FindByText("Send &Manifest");
		AssertNotNull("Send Manifest menu item not found", sendMenu);

		UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
		UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
		ZFormModaliser.ShowDialogsInTest = true;
		ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
		ZFormModaliser.SetDelegateToCallOnFormClosing(obj =>
		{
			var dialog = (BillsSelectionDialog)obj;
			dialog.SelectAll();
		});

		sendMenu.PerformClick();
		AssertEquals("1 CUSCAR Manifest Message(s) Created.", UnitTestUserNotification.Instance.LastMessage.Text);
	});

	public void TestSendingMultiple() => CombineAssertions(() =>
	{
		var header = CreateNewManifest();
		var billFirst = header.Bills.AddNew();
		billFirst.ABL_BillNumber = "B0001";

		var billSecond = header.Bills.AddNew();
		billSecond.ABL_BillNumber = "B0002";

		using var menu = new ASYCUDA.GUI.Testing.AsycudaMenuForTest(header);
		using var form = new ZForm(header);
		form.Menu.MenuItems.Add(menu);
		form.Show();
		menu.OnPopup(EventArgs.Empty);
		var sendMenu = menu.MenuItems.FindByText("Send &Manifest");
		AssertNotNull("Send Manifest menu item not found", sendMenu);

		UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
		UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
		ZFormModaliser.ShowDialogsInTest = true;
		ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
		ZFormModaliser.SetDelegateToCallOnFormClosing(obj =>
		{
			var dialog = (BillsSelectionDialog)obj;
			dialog.SelectAll();
		});

		sendMenu.PerformClick();
		AssertEquals("2 CUSCAR Manifest Message(s) Created.", UnitTestUserNotification.Instance.LastMessage.Text);
	});

	AsycudaManifestHeader CreateNewManifest()
	{
		var header = Factory.New<AsycudaManifestHeader>();
		header.AMA_ManifestType = AEManifestTypes.Codes.ACI;
		header.AMA_RL_NKPortOfLoading = "AEABU";

		return header;
	}
}
