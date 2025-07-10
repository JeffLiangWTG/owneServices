using System;
using System.Windows.Forms;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.ARAP.Testing
{
	public class TestPaymentAddressWithContactControl : TestCase
	{
		public void TestPaymentAddressWithContactControlTest()
		{
			using (var control = new PaymentAddressWithContactControl())
			{
				Func<string, Control> getControl = (controlName) =>
				{
					var controls = control.Controls.Find(controlName, true);
					AssertEquals(1, controls.Length);
					return controls[0];
				};

				AssertEquals(false, control.ContactInfoTabVisible);
				AssertEquals(false, getControl("AddressesLink").Visible);
				AssertEquals(false, getControl("ContactsLink").Visible);
				AssertEquals(DockStyle.Fill, getControl("GroupBox").Dock);
				AssertEquals(DockStyle.Fill, getControl("DetailsTabControl").Dock);
			}
		}
	}
}
