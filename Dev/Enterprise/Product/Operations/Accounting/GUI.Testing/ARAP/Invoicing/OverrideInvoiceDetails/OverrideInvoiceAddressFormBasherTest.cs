using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Core.Forms;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.ARAP.Invoicing.Testing
{
	[TestedType(typeof(OverrideInvoiceAddressContactForm))]
	public class OverrideInvoiceAddressFormBasherTest : OverrideInvoiceDetailsFormTest
	{
		#region Implementation

		protected override Form GetFormToBashCore()
		{
			return new OverrideInvoiceAddressContactForm(new OverrideInvoiceAddressHelper(Factory, null, ZGuid.Empty, false, Factory.New<ARInvoice>().PK));
		}

		protected override void AssertFormDisplayMode(OverrideInvoiceDetailsForm testForm)
		{
			AssertNotEquals("DisplayMode is not savedNew", ODisplayMode.NewSaved, testForm.DisplayMode);
			AssertEquals("DisplayMode is Undefined", ODisplayMode.Undefined, testForm.DisplayMode);
		}

		protected override void AssertButtonVisibility(ZButton closeButton, ZButton continueButton, ZPostingButtonsUserControl postingUserControl)
		{
			Assert(closeButton.Visible);
			Assert(continueButton.Visible);

			Assert(!postingUserControl.Visible);
		}

		#endregion
	}
}
