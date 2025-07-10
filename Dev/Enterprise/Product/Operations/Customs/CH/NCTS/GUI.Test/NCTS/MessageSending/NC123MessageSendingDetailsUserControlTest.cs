using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Customs.CH.NCTS.Business;

namespace Enterprise.Customs.CH.NCTS.GUI.Testing;

class NC123MessageSendingDetailsUserControlTest : TestCaseWithFactory
{
	public void TestControlBinding() => CombineAssertions(() =>
	{
		using (var userControl = new NC123MessageSendingDetailsUserControl())
		{
			AssertBindTo(userControl.IdentificationNumberTextBox, nameof(NctsHeaderDepartureMessageSendingObject.IdentificationNumber));
			AssertBindTo(userControl.ContactNameTextBox, nameof(NctsHeaderDepartureMessageSendingObject.ContactName));
			AssertBindTo(userControl.PhoneNumberTextBox, nameof(NctsHeaderDepartureMessageSendingObject.PhoneNumber));
			AssertBindTo(userControl.EmailAddressTextBox, nameof(NctsHeaderDepartureMessageSendingObject.EmailAddress));
			AssertBindTo(userControl.CommunicationLanguageDropEdit, nameof(NctsHeaderDepartureMessageSendingObject.CommunicationLanguage));
		}

		static void AssertBindTo(Control control, string bindTo)
		{
			AssertEquals($"{control.Name}.BindTo", bindTo, (control as IBindTo).BindTo);
		}
	});

	public void TestApprovedLocation()
	{
		using (var userControl = new NC123MessageSendingDetailsUserControl())
		{
			AssertEquals("BindingMember", ".", userControl.ApprovedLocationOfGoodsUserControl.GetBindingMember());
		}
	}
}
