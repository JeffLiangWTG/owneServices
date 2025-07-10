using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Customs.CH.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CH.GUI.Testing;

class NC123MessageSendingDetailsUserControlTest : TestCaseWithFactory
{
	public void TestControlBinding() => CombineAssertions(() =>
	{
		var bo = new ExportDeclarationMessageSendingObjectParent(Factory.New<JobDeclaration>());
		using (var form = new ZForm(bo))
		using (var userControl = new NC123MessageSendingDetailsUserControl())
		{
			form.Controls.Add(userControl);
			form.Show();
			AssertBindTo(userControl.IdentificationNumberTextBox, nameof(ExportDeclarationMessageSendingObjectParent.IdentificationNumber));
			AssertBindTo(userControl.ContactNameTextBox, nameof(ExportDeclarationMessageSendingObjectParent.ContactName));
			AssertBindTo(userControl.PhoneNumberTextBox, nameof(ExportDeclarationMessageSendingObjectParent.PhoneNumber));
			AssertBindTo(userControl.EmailAddressTextBox, nameof(ExportDeclarationMessageSendingObjectParent.EmailAddress));
			AssertBindTo(userControl.DeclarationLanguageDropEdit, nameof(ExportDeclarationMessageSendingObjectParent.SendingDeclaration) + "." + nameof(ExportDeclarationMessageSendingObjectParent.SendingDeclaration.JE_DeclarationLanguage));
			AssertBindTo(userControl.LocationOfGoodsDropEdit, nameof(ExportDeclarationMessageSendingObjectParent.SendingDeclaration) + "." + nameof(ExportDeclarationMessageSendingObjectParent.SendingDeclaration.JE_LocationOfGoods));
			AssertEquals("TransportDynamicLayoutPanel", nameof(ExportDeclarationMessageSendingObjectParent.SendingDeclaration), userControl.TransportDynamicLayoutPanel.GetBindingMember());
			AssertBindTo(userControl.NextProcedureDropEdit, $"{nameof(ExportDeclarationMessageSendingObjectParent.SendingObjectsCollection)}.{nameof(ExportDeclarationMessageSendingObject.NextProcedure)}");
		}

		static void AssertBindTo(Control control, string bindTo)
		{
			AssertEquals($"{control.Name}.BindTo", bindTo, (control as IBindTo).BindTo);
		}
	});
}
