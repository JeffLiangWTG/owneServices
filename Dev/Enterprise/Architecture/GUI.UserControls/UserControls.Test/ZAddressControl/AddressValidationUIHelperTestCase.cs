using System;
using System.Drawing;
using System.IO;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class AddressValidationUIHelperTestCase : TransactionedTestCase
	{
		public void TestSetButtonValidationStatus()
		{
			using (var form = new ZForm())
			using (ZButton button = new ZButton())
			{
				form.Controls.Add(button);
				form.Show();

				AssertButtonValidationStatus(button, AddressValidationStatus.Verified, Properties.Resources.AddressValid, false, "This address is verified.");
				AssertButtonValidationStatus(button, AddressValidationStatus.ManuallyVerified, Properties.Resources.AddressPendingValidation, false, "This address is manually verified.");
				AssertButtonValidationStatus(button, AddressValidationStatus.VerifiedToStreet, Properties.Resources.AddressValid, false, "This address is verified to street number.");
				AssertButtonValidationStatus(button, AddressValidationStatus.ToBeVerified, Properties.Resources.AddressPendingValidation, false, "This address needs to be verified.");
				AssertButtonValidationStatus(button, AddressValidationStatus.ExcludeBackgroundValidation, Properties.Resources.AddressPendingValidation, false, "This address needs to be verified.");
				AssertButtonValidationStatus(button, AddressValidationStatus.Unverifiable, Properties.Resources.AddressInvalid, false, "This address has a verification status of Invalid.");
				AssertButtonValidationStatus(button, AddressValidationStatus.Invalid, Properties.Resources.AddressInvalid, false, "This address has a verification status of Invalid.");
				AssertButtonValidationStatus(button, AddressValidationStatus.CountryNotAvailable, Properties.Resources.AddressOriginal, true, "It is not yet possible to verify addresses for this country/region.");
			}
		}

		public void TestSetButtonValidationStatus_WhenEnablingSuppressErrorOnRegistryAndFlag_ShouldIncludeAdditionalWarningInTooltip()
		{
			// Arrange.

			using (var form = new ZForm())
			using (var button = new ZButton())
			{
				form.Controls.Add(button);
				form.Show();

				OrganisationsDataRegistry.Instance.IsErrorSuppressionEnabled = true;

				// Act

				AddressValidationUIHelper.SetButtonValidationStatus(button, AddressValidationStatus.Invalid, true);

				// Assert.

				AssertEquals(
					"This address has a verification status of Invalid. (Invalid error suppressed for this address.)",
					button.ToolTipCaption.GetUnresolvedValue());
			}
		}

		public void TestSetButtonValidationStatus_WhenEnablingSuppressErrorOnRegistryButNotOnFlag_ShouldNotIncludeAdditionalWarningInTooltip()
		{
			// Arrange.

			using (var form = new ZForm())
			using (var button = new ZButton())
			{
				form.Controls.Add(button);
				form.Show();

				OrganisationsDataRegistry.Instance.IsErrorSuppressionEnabled = true;

				// Act

				AddressValidationUIHelper.SetButtonValidationStatus(button, AddressValidationStatus.Invalid, false);

				// Assert.

				AssertEquals(
					"This address has a verification status of Invalid.",
					button.ToolTipCaption.GetUnresolvedValue());
			}
		}

		public void TestSetButtonValidationStatus_WhenEnablingSuppressErrorOnFlagButNotOnRegistry_ShouldNotIncludeAdditionalWarningInTooltip()
		{
			// Arrange.

			using (var form = new ZForm())
			using (var button = new ZButton())
			{
				form.Controls.Add(button);
				form.Show();

				OrganisationsDataRegistry.Instance.IsErrorSuppressionEnabled = false;

				// Act

				AddressValidationUIHelper.SetButtonValidationStatus(button, AddressValidationStatus.Invalid, true);

				// Assert.

				AssertEquals(
					"This address has a verification status of Invalid.",
					button.ToolTipCaption.GetUnresolvedValue());
			}
		}

		void AssertButtonValidationStatus(ZButton button, string status, Image expectedImage, bool isReadOnly, string expectedToolTip)
		{
			AddressValidationUIHelper.SetButtonValidationStatus(button, status, false);
			AssertImageBitsEquals(expectedImage, button.Image);
			AssertEquals(isReadOnly, button.ReadOnly);
			AssertEquals(expectedToolTip, button.ToolTipCaption.GetUnresolvedValue());
		}

		void AssertImageBitsEquals(Image b1, Image b2)
		{
			var bits1 = ConvertBitMapToByteArray(b1);
			var bits2 = ConvertBitMapToByteArray(b2);
			AssertEquals(bits1.Length, bits2.Length);

			for (int i = 0; i < bits1.Length; i++)
			{
				AssertEquals(bits1[0], bits2[0]);
			}
		}

		byte[] ConvertBitMapToByteArray(Image bitmap)
		{
			byte[] result = null;
			if (bitmap != null)
			{
				MemoryStream stream = new MemoryStream();
				bitmap.Save(stream, bitmap.RawFormat);
				result = stream.ToArray();
			}
			return result;
		}

		public void TestSetAddressDropEditValidationStatus()
		{
			using (var dropEdit = new ZDropEdit())
			{
				AssertDropEditValidationStatus(dropEdit, AddressValidationStatus.Verified, AddressValidationUIHelper.colorValid);
				AssertDropEditValidationStatus(dropEdit, AddressValidationStatus.ManuallyVerified, AddressValidationUIHelper.colorValid);
				AssertDropEditValidationStatus(dropEdit, AddressValidationStatus.VerifiedToStreet, AddressValidationUIHelper.colorValid);
				AssertDropEditValidationStatus(dropEdit, AddressValidationStatus.ToBeVerified, AddressValidationUIHelper.colorUnknown);
				AssertDropEditValidationStatus(dropEdit, AddressValidationStatus.ExcludeBackgroundValidation, AddressValidationUIHelper.colorUnknown);
				AssertDropEditValidationStatus(dropEdit, AddressValidationStatus.Unverifiable, AddressValidationUIHelper.colorInvalid);
				AssertDropEditValidationStatus(dropEdit, AddressValidationStatus.Invalid, AddressValidationUIHelper.colorInvalid);
			}
		}

		void AssertDropEditValidationStatus(ZDropEdit dropEdit, string status, Color expectedColor)
		{
			AddressValidationUIHelper.SetAddressDropEditValidationStatus(dropEdit, status);
			AssertEquals(expectedColor, dropEdit.CodeBox.BackColor);
		}

		public void TestSetAddressFieldValidationStatus()
		{
			string validationStatus = AddressValidationStatus.ToBeVerified;
			var color = Color.FromArgb(255, 224, 179);

			using (var form = new ZForm())
			using (IDisposable address1Control = new ZTextBox(), address2Control = new ZTextBox(), cityControl = new ZTextBox(),
				postcodeControl = new ZTextBox(), stateControl = new ZDropEdit(), countryControl = new ZCodeFindBox(), testControl = new ZTextBox())
			{
				form.Controls.Add(address1Control as ZTextBox);
				form.Controls.Add(address2Control as ZTextBox);
				form.Controls.Add(cityControl as ZTextBox);
				form.Controls.Add(postcodeControl as ZTextBox);
				form.Controls.Add(stateControl as ZDropEdit);
				form.Controls.Add(countryControl as ZCodeFindBox);
				form.Controls.Add(testControl as ZTextBox);
				form.Show();

				(testControl as ZTextBox).Focus();

				AddressValidationUIHelper.SetAddressFieldValidationStatus(address1Control as ZTextBox, address2Control as ZTextBox, cityControl as ZTextBox,
					postcodeControl as ZTextBox, stateControl as ZDropEdit, countryControl as ZCodeFindBox, validationStatus);

				AssertEquals(color, (address1Control as ZTextBox).BackColor);
				AssertEquals(color, (address2Control as ZTextBox).BackColor);
				AssertEquals(color, (cityControl as ZTextBox).BackColor);
				AssertEquals(color, (postcodeControl as ZTextBox).BackColor);
				AssertEquals(color, (stateControl as ZDropEdit).CodeBox.BackColor);
				AssertEquals(color, (countryControl as ZCodeFindBox).CodeBox.BackColor);
			}
		}
	}
}
