using System;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Windows.UI;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI.WebAddressValidation;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI
{
	sealed class CusGoodsLocationWebValidationControlFormLayout : ContainerControl, ISupportWebAddressValidationControl
	{
		public CusGoodsLocationWebValidationControlFormLayout(IControlHost controlHost)
		{
			this.controlHost = Argument.NotNull(controlHost, nameof(controlHost));
		}
		readonly IControlHost controlHost;

		#region ISupportWebAddressValidationControl

		bool ISupportWebAddressValidationControl.ProcessManuallyVerifyShortCutKey(ref Message msg, Keys keyData)
		{
			return SupportWebAddressValidationControlHelper.ProcessCommandKey(keyData, ParentControl.GetReadOnly(), this, ((ISupportWebAddressValidationControl)this).AddressForValidation);
		}

		ZTextBox ISupportWebAddressValidationControl.AddressCodeControl => null;

		ZTextBox ISupportWebAddressValidationControl.Address1Control => address1Control ?? (address1Control = GetControlFromLayout<StreetAndNumberWithAddressValidationControl>(nameof(CusGoodsLocationUserControl.StreetAndNumberWithAddressValidationUserControl)).StreetAndNumberTextBox);
		ZTextBox address1Control;

		ZTextBox ISupportWebAddressValidationControl.Address2Control => ((ISupportWebAddressValidationControl)this).Address1Control;

		ZTextBox ISupportWebAddressValidationControl.CityControl => cityControl ?? (cityControl = GetControlFromLayout<ZTextBox>(nameof(CusGoodsLocationUserControl.CityTextBox)));
		ZTextBox cityControl;

		ZTextBox ISupportWebAddressValidationControl.PostcodeControl => postcodeControl ?? (postcodeControl = GetControlFromLayout<ZTextBox>(nameof(CusGoodsLocationUserControl.PostcodeTextBox)));
		ZTextBox postcodeControl;

		ZDropEdit ISupportWebAddressValidationControl.StateControl => stateControl ?? (stateControl = new ZDropEdit());
		ZDropEdit stateControl;

		ZCodeFindBox ISupportWebAddressValidationControl.CountryControl => countryControl ?? (countryControl = GetControlFromLayout<ZCodeFindBox>(nameof(CusGoodsLocationUserControl.CountryCodeFindBox)));
		ZCodeFindBox countryControl;

		ZTextBox ISupportWebAddressValidationControl.AdditionalAddressInformationControl => null;

		ZButton ISupportWebAddressValidationControl.ValidateButton => validateAddressButton ?? (validateAddressButton = GetControlFromLayout<StreetAndNumberWithAddressValidationControl>(nameof(CusGoodsLocationUserControl.StreetAndNumberWithAddressValidationUserControl)).ValidateAddressButton);
		ZButton validateAddressButton;

		bool ISupportWebAddressValidationControl.ValidationJustForced { get; set; }

		Control ISupportWebAddressValidationControl.SuggestionWindowParentControl => ParentControl;

		ISupportWebAddressValidation ISupportWebAddressValidationControl.AddressForValidation
		{
			get
			{
				var cusGoodsLocation = controlHost.GetCurrentDataItem() as CusGoodsLocation;
				return cusGoodsLocation?.Address;
			}
		}

		ZButton ISupportWebAddressValidationControl.ClearAddressFieldsButton => null;

		Func<Keys, bool> ISupportWebAddressValidationControl.AddressValidationProcessCmdKey { get; set; }

		#endregion

		Control ParentControl => (Control)controlHost;

		protected override ControlCollection CreateControlsInstance()
		{
#if WINZOR
			if (ParentControl is null)
			{
				return base.CreateControlsInstance();
			}
#endif
			return ParentControl?.Controls;
		}

		TControl GetControlFromLayout<TControl>(string controlName) where TControl : Control => (TControl)controlHost.GetControlByName(controlName);

		protected override void Dispose(bool disposing)
		{
#if WINZOR
			if (Controls != null)
			{
				base.Dispose(disposing);
			}
#else
			base.Dispose(disposing);
#endif
			stateControl?.Dispose();
		}
	}
}
