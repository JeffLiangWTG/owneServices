using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI.WebAddressValidation;
using Enterprise.ZArchitecture.GUI;
using WTG.AddressCleansing.Common;

namespace Enterprise.Customs.EU.GUI
{
	public class CusGoodsLocationWebAddressValidationExtension : ILayoutExtension
	{
		IControlHost controlHost;
		CancellationTokenSource cancellationToken;
		CusGoodsLocationWebValidationControlFormLayout validationControl;

		#region ILayoutBehavior

		void ILayoutExtension.Initialize(IControlHost host)
		{
			controlHost = host;
			cancellationToken = new CancellationTokenSource();
			validationControl = new CusGoodsLocationWebValidationControlFormLayout(controlHost);
			RegisterEventsForAddressControls();
		}

		void ILayoutExtension.Cleanup()
		{
			if (validationControl is ISupportWebAddressValidationControl addressValidationControl)
			{
				if (addressValidationControl.ValidateButton is ZButton validateButton)
				{
					validateButton.Click -= ValidateAddress_OnClick;
				}

				var addressForValidation = addressValidationControl.AddressForValidation;
				if (addressForValidation != null)
				{
					addressForValidation.AddressValidationStatusChanged -= AddressForValidation_AddressValidationStatusChanged;
				}
			}
			validationControl?.Dispose();
		}

		#endregion

		void RegisterEventsForAddressControls()
		{
			if (validationControl is ISupportWebAddressValidationControl addressValidationControl)
			{
				SupportWebAddressValidationControlHelper.HookISupportWebAddressValidationControlChangeFocusEvents(addressValidationControl, AddressValidationControlGotFocus, AddressValidationControlLostFocus);
				if (addressValidationControl.ValidateButton is ZButton validateButton)
				{
					validateButton.Click += ValidateAddress_OnClick;
				}

				var addressValidation = addressValidationControl.AddressForValidation;
				if (addressValidation != null)
				{
					addressValidation.AddressValidationStatusChanged += AddressForValidation_AddressValidationStatusChanged;
					AddressForValidation_AddressValidationStatusChanged(null, EventArgs.Empty);
				}
			}
		}

		async void ValidateAddress_OnClick(object sender, EventArgs e)
		{
			if (validationControl is ISupportWebAddressValidationControl addressValidationControl)
			{
				addressValidationControl.ValidationJustForced = true;
				await ValidateAddress(addressValidationControl.AddressForValidation);
			}
		}

		void AddressValidationControlLostFocus(object sender, EventArgs e)
		{
			if (validationControl is ISupportWebAddressValidationControl addressValidationControl)
			{
				SupportWebAddressValidationControlHelper.ShowSuggestionControlsUponGotFocus(sender, addressValidationControl.AddressForValidation, validationControl);
			}
		}

		void AddressValidationControlGotFocus(object sender, EventArgs e)
		{
			SupportWebAddressValidationControlHelper.HideSuggestionControlsUponLostFocus(validationControl);
		}

		async Task ValidateAddress(ISupportWebAddressValidation address)
		{
			if (address != null && validationControl is ISupportWebAddressValidationControl addressValidationControl && AddressValidationService.IsAddressNeedValidation(address))
			{
				CleanseAction cleanseAction;
				var controlCollection = validationControl.Controls;

				if (!addressValidationControl.ValidationJustForced)
				{
					AddressSuggestionControlHelper.CloseSuggestionForm(controlCollection);
					cleanseAction = CleanseAction.QuickValidate;
				}
				else
				{
					cleanseAction = CleanseAction.ValidateAndSuggest;
					addressValidationControl.ValidationJustForced = false;
				}

				var form = ((Control)controlHost).Parent.FindForm();
				await AddressSuggestionControlHelper.ValidateAddressAsync(cancellationToken, address, validationControl, form, controlCollection, () => RefreshValidationStatus(address), null, 0, cleanseAction);
			}
		}

		void RefreshValidationStatus(ISupportWebAddressValidation address)
		{
			if (address != null && validationControl is ISupportWebAddressValidationControl addressValidationControl)
			{
				AddressValidationUIHelper.SetButtonValidationStatus(addressValidationControl.ValidateButton, address.ValidationStatus, address.IsErrorSuppressed);
				AddressValidationUIHelper.SetAddressFieldValidationStatus(addressValidationControl.Address1Control,
					addressValidationControl.Address2Control,
					addressValidationControl.CityControl,
					addressValidationControl.PostcodeControl,
					addressValidationControl.StateControl,
					addressValidationControl.CountryControl,
					address.ValidationStatus);

				((Control)controlHost).Invalidate();
			}
		}

		void AddressForValidation_AddressValidationStatusChanged(object sender, EventArgs e)
		{
			if (validationControl is ISupportWebAddressValidationControl addressValidationControl)
			{
				RefreshValidationStatus(addressValidationControl.AddressForValidation);
			}
		}
	}
}
