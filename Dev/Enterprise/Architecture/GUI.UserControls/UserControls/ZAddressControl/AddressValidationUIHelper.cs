using System.Drawing;
using CargoWise.Windows.UI;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using ResString = Enterprise.ZArchitecture.GUI.UserControls.ResString;

namespace Enterprise.ZArchitecture.GUI
{
	public static class AddressValidationUIHelper
	{
		public static void SetButtonValidationStatus(ZButton button, string validationStatus, bool isErrorSuppressed)
		{
			if (!button.IsDesignMode() && !string.IsNullOrEmpty(validationStatus))
			{
				switch (validationStatus)
				{
					case (AddressValidationStatus.Verified):
						button.Image = Properties.Resources.AddressValid;
						button.ReadOnly = false;
						button.ToolTipCaption = ResString.GetMultilingualString("46EB837F-1AB2-4B1E-843B-0AA5B27372EE", "This address is verified.");
						break;
					case (AddressValidationStatus.ManuallyVerified):
						button.Image = Properties.Resources.AddressPendingValidation;
						button.ReadOnly = false;
						button.ToolTipCaption = ResString.GetMultilingualString("EB15F543-797A-445F-B29E-362A876839BA", "This address is manually verified.");
						break;
					case (AddressValidationStatus.VerifiedToStreet):
						button.Image = Properties.Resources.AddressValid;
						button.ReadOnly = false;
						button.ToolTipCaption = ResString.GetMultilingualString("35066C6E-A042-4418-96EA-540BDEA8C0E8", "This address is verified to street number.");
						break;
					case (AddressValidationStatus.ToBeVerified):
					case (AddressValidationStatus.ExcludeBackgroundValidation):
						button.Image = Properties.Resources.AddressPendingValidation;
						button.ReadOnly = false;
						button.ToolTipCaption = ResString.GetMultilingualString("38CDE799-057C-4133-A510-FF9CA3F79786", "This address needs to be verified.");
						break;
					case (AddressValidationStatus.Unverifiable):
					case (AddressValidationStatus.Invalid):
						button.Image = Properties.Resources.AddressInvalid;
						button.ReadOnly = false;

						var mainMessage = ResString.GetMultilingualString("92972EF4-9667-4B2E-8AD7-2C00882BF90A", "This address has a verification status of Invalid.");
						var additionalMessage = ResString.GetMultilingualString("9088B806-6E60-4551-AC53-76428BD7ABD1", "(Invalid error suppressed for this address.)");

						var shouldSuppressError = isErrorSuppressed && OrganisationsDataRegistry.Instance.IsErrorSuppressionEnabled;

						button.ToolTipCaption = (shouldSuppressError)
							? MultilingualString.Join(" ", mainMessage, additionalMessage)
							: mainMessage;

						break;
					case (AddressValidationStatus.CountryNotAvailable):
						button.Image = Properties.Resources.AddressOriginal;
						button.ReadOnly = true;
						button.ToolTipCaption = ResString.GetMultilingualString("5C16E27E-0180-4697-B6CA-03450410087A", "It is not yet possible to verify addresses for this country/region.");
						break;
				}
				button.Invalidate();
			}
		}

		public static void ResetAddressFieldState(
			ZTextBox address1Control,
			ZTextBox address2Control,
			ZTextBox cityControl,
			ZTextBox postcodeControl,
			ZDropEdit stateControl,
			ZCodeFindBox countryControl)
		{
			address1Control.ResetBackColor();
			address2Control.ResetBackColor();
			cityControl.ResetBackColor();
			postcodeControl.ResetBackColor();
			stateControl.CodeBox.ResetBackColor();
			countryControl.CodeBox.ResetBackColor();
		}

		public static void SetAddressFieldValidationStatus(
			ZTextBox address1Control,
			ZTextBox address2Control,
			ZTextBox cityControl,
			ZTextBox postcodeControl,
			ZDropEdit stateControl,
			ZCodeFindBox countryControl,
			string validationStatus)
		{
			var color = default(Color);

			var changeField = false;

			if (!address1Control.IsDesignMode() && !string.IsNullOrEmpty(validationStatus))
			{
				changeField = true;
			}

			if (changeField)
			{
				color = GetValidationColor(validationStatus);
			}

			address1Control.BackColor = color;
			address2Control.BackColor = color;
			cityControl.BackColor = color;
			postcodeControl.BackColor = color;
			stateControl.CodeBox.BackColor = color;
			countryControl.CodeBox.BackColor = color;
		}

		public static void ResetAddressDropEditState(ZDropEdit addressEdit)
		{
			if (!addressEdit.IsDesignMode())
			{
				addressEdit.CodeBox.ResetBackColor();
			}
		}

		public static void SetAddressDropEditValidationStatus(ZDropEdit addressEdit, string validationStatus)
		{
			if (!addressEdit.IsDesignMode() && !string.IsNullOrEmpty(validationStatus))
			{
				addressEdit.CodeBox.BackColor = GetValidationColor(validationStatus);
			}
		}

		static Color GetValidationColor(string validationStatus)
		{
			var color = default(Color);
			switch (validationStatus)
			{
				case (AddressValidationStatus.ManuallyVerified):
				case (AddressValidationStatus.Verified):
				case (AddressValidationStatus.VerifiedToStreet):
					color = colorValid;
					break;
				case (AddressValidationStatus.ToBeVerified):
				case (AddressValidationStatus.ExcludeBackgroundValidation):
					color = colorUnknown;
					break;
				case (AddressValidationStatus.Unverifiable):
				case (AddressValidationStatus.Invalid):
					color = colorInvalid;
					break;
				case (AddressValidationStatus.CountryNotAvailable):
					color = default(Color);
					break;
			}
			return color;
		}
		internal static readonly Color colorValid = Color.FromArgb(198, 236, 198);
		internal static readonly Color colorUnknown = Color.FromArgb(255, 224, 179);
		internal static readonly Color colorInvalid = Color.FromArgb(255, 179, 179);
	}
}
