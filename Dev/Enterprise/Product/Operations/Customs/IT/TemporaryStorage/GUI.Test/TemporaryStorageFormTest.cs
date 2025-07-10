using System.Linq;
using System.Windows.Forms;
using Enterprise.Customs.IT.TemporaryStorage.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.IT.TemporaryStorage.GUI.Testing;

[TestedType(typeof(TemporaryStorageForm))]
sealed class TemporaryStorageFormTest : ZTemplateFormTest
{
	public void TestGetNewMessagingMenu()
	{
		using var form = GetFormToBash();
		var menuTypes = form.Menu.MenuItems.Cast<ZMenuItem>().Select(x => x.GetType());
		AssertEquals("Contains IT TemporaryStorageMessagesMenu", true, menuTypes.Contains(typeof(TemporaryStorageMessagesMenu)));
	}

	protected override Form GetFormToBashCore()
	{
		var header = Factory.NewWithValidTestData<TemporaryStorageHeader>();
		var bill = header.Bills.AddNew();
		bill.FillWithValidTestData();
		bill.PackedItems.AddNew().FillWithValidTestData();
		_ = header.PresentationCustomsOfficeCode;
		Factory.Save();
		var form = new TemporaryStorageForm(header);
		form.ControllerID = ControllerIDs.Customs.EU.UCC6TemporaryStorage;
		return form;
	}
}
