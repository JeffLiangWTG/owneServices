using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Customs.IE.Business.CusTempStorage;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.IE.GUI.Testing
{
	[TestedType(typeof(TemporaryStorageForm))]
	sealed class TemporaryStorageFormTest : ZTemplateFormTest
	{
		public void TestGetNewMessagingMenu()
		{
			var storageHeader = Factory.NewWithValidTestData<TemporaryStorageHeader>();
			using (var form = new TemporaryStorageForm(storageHeader))
			{
				AssertType<TemporaryStorageMessagesMenu>("GetNewMessagingMenu should have been overridden", ((IFileMenuItemsProvider)form).MainMenu.MenuItems.FindByName(nameof(TemporaryStorageMessagesMenu)));
			}
		}

		protected override Form GetFormToBashCore()
		{
			var storageHeader = Factory.NewWithValidTestData<TemporaryStorageHeader>();
			var bill = storageHeader.Bills.AddNew();
			bill.FillWithValidTestData();
			bill.PackedItems.AddNew().FillWithValidTestData();
			var presentationCustomsOfficeCode = storageHeader.PresentationCustomsOfficeCode;
			var customsOfficeOfLodgement = storageHeader.CustomsOfficeOfLodgement;
			var customsOfficeCodeOfFirstEntry = storageHeader.CustomsOfficeCodeOfFirstEntry;
			Factory.Save();
			var form = new TemporaryStorageForm(storageHeader);
			form.ControllerID = ControllerIDs.Customs.EU.UCC6TemporaryStorage;
			return form;
		}
	}
}
