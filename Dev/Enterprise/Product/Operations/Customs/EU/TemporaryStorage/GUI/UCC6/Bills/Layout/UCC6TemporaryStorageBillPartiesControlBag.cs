using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.TemporaryStorage.GUI
{
	public class UCC6TemporaryStorageBillPartiesControlBag : ControlBag
	{
		protected UCC6TemporaryStorageBillPartiesControlBag()
		{
			ShipperSeparatorUserControl = RegisterControl(nameof(UCC6TemporaryStorageBillPartiesControl.ShipperSeparatorUserControl));
			ShipperAddressControl = RegisterControl(nameof(UCC6TemporaryStorageBillPartiesControl.ShipperAddressControl));
			ShipperNameTextBox = RegisterControl(nameof(UCC6TemporaryStorageBillPartiesControl.ShipperNameTextBox));
			ShipperStreet1TextBox = RegisterControl(nameof(UCC6TemporaryStorageBillPartiesControl.ShipperStreet1TextBox));
			ShipperStreet2TextBox = RegisterControl(nameof(UCC6TemporaryStorageBillPartiesControl.ShipperStreet2TextBox));
			ShipperCityTextBox = RegisterControl(nameof(UCC6TemporaryStorageBillPartiesControl.ShipperCityTextBox));
			ShipperCountryCodeFindBox = RegisterControl(nameof(UCC6TemporaryStorageBillPartiesControl.ShipperCountryCodeFindBox));
			ShipperStateDropEdit = RegisterControl(nameof(UCC6TemporaryStorageBillPartiesControl.ShipperStateDropEdit));
			ShipperPhoneTextBox = RegisterControl(nameof(UCC6TemporaryStorageBillPartiesControl.ShipperPhoneTextBox));
			ShipperPostCodeTextBox = RegisterControl(nameof(UCC6TemporaryStorageBillPartiesControl.ShipperPostCodeTextBox));
			ShipperRegNoTypeDropEdit = RegisterControl(nameof(UCC6TemporaryStorageBillPartiesControl.ShipperRegNoTypeDropEdit));
			ShipperRegNoTextBox = RegisterControl(nameof(UCC6TemporaryStorageBillPartiesControl.ShipperRegNoTextBox));

			ConsigneeSeparatorUserControl = RegisterControl(nameof(UCC6TemporaryStorageBillPartiesControl.ConsigneeSeparatorUserControl));
			ConsigneeAddressControl = RegisterControl(nameof(UCC6TemporaryStorageBillPartiesControl.ConsigneeAddressControl));
			ConsigneeNameTextBox = RegisterControl(nameof(UCC6TemporaryStorageBillPartiesControl.ConsigneeNameTextBox));
			ConsigneeStreet1TextBox = RegisterControl(nameof(UCC6TemporaryStorageBillPartiesControl.ConsigneeStreet1TextBox));
			ConsigneeStreet2TextBox = RegisterControl(nameof(UCC6TemporaryStorageBillPartiesControl.ConsigneeStreet2TextBox));
			ConsigneeCityTextBox = RegisterControl(nameof(UCC6TemporaryStorageBillPartiesControl.ConsigneeCityTextBox));
			ConigneeCountryCodeFindBox = RegisterControl(nameof(UCC6TemporaryStorageBillPartiesControl.ConigneeCountryCodeFindBox));
			ConsigneeStateDropEdit = RegisterControl(nameof(UCC6TemporaryStorageBillPartiesControl.ConsigneeStateDropEdit));
			ConsigneePostcodeTextBox = RegisterControl(nameof(UCC6TemporaryStorageBillPartiesControl.ConsigneePostcodeTextBox));
			ConsigneePhoneTextBox = RegisterControl(nameof(UCC6TemporaryStorageBillPartiesControl.ConsigneePhoneTextBox));
			ConsigneeRegNoTypeDropEdit = RegisterControl(nameof(UCC6TemporaryStorageBillPartiesControl.ConsigneeRegNoTypeDropEdit));
			ConsigneeRegoNoTextBox = RegisterControl(nameof(UCC6TemporaryStorageBillPartiesControl.ConsigneeRegoNoTextBox));

			NotifyPartySeparatorUserControl = RegisterControl(nameof(UCC6TemporaryStorageBillPartiesControl.NotifyPartySeparatorUserControl));
			NotifyPartyAddressControl = RegisterControl(nameof(UCC6TemporaryStorageBillPartiesControl.NotifyPartyAddressControl));
			NotifyPartyNameTextBox = RegisterControl(nameof(UCC6TemporaryStorageBillPartiesControl.NotifyPartyNameTextBox));
			NotifyPartyStreet1TextBox = RegisterControl(nameof(UCC6TemporaryStorageBillPartiesControl.NotifyPartyStreet1TextBox));
			NotifyPartyStreet2TextBox = RegisterControl(nameof(UCC6TemporaryStorageBillPartiesControl.NotifyPartyStreet2TextBox));
			NotifyPartyCityTextBox = RegisterControl(nameof(UCC6TemporaryStorageBillPartiesControl.NotifyPartyCityTextBox));
			NotifyPartyCountryCodeFindBox = RegisterControl(nameof(UCC6TemporaryStorageBillPartiesControl.NotifyPartyCountryCodeFindBox));
			NotifyPartyStateDropEdit = RegisterControl(nameof(UCC6TemporaryStorageBillPartiesControl.NotifyPartyStateDropEdit));
			NotifyPartyPostcodeTextBox = RegisterControl(nameof(UCC6TemporaryStorageBillPartiesControl.NotifyPartyPostcodeTextBox));
			NotifyPartyPhoneTextBox = RegisterControl(nameof(UCC6TemporaryStorageBillPartiesControl.NotifyPartyPhoneTextBox));
			NotifyPartyRegNoTypeDropEdit = RegisterControl(nameof(UCC6TemporaryStorageBillPartiesControl.NotifyPartyRegNoTypeDropEdit));
			NotifyPartyRegNoTextBox = RegisterControl(nameof(UCC6TemporaryStorageBillPartiesControl.NotifyPartyRegNoTextBox));
		}

		public static UCC6TemporaryStorageBillPartiesControlBag Instance => uCC6TemporaryStorageBillPartiesControlBag.Value;

		[WTG.StaticAnalysis.Annotation.ThreadSafe]
		static readonly Lazy<UCC6TemporaryStorageBillPartiesControlBag> uCC6TemporaryStorageBillPartiesControlBag = new Lazy<UCC6TemporaryStorageBillPartiesControlBag>(() => new UCC6TemporaryStorageBillPartiesControlBag());

		protected override Control CreateTemplate() => new UCC6TemporaryStorageBillPartiesControl();

		public ControlReference ShipperSeparatorUserControl { get; }
		public ControlReference ShipperAddressControl { get; }
		public ControlReference ShipperNameTextBox { get; }
		public ControlReference ShipperStreet1TextBox { get; }
		public ControlReference ShipperStreet2TextBox { get; }
		public ControlReference ShipperCityTextBox { get; }
		public ControlReference ShipperCountryCodeFindBox { get; }
		public ControlReference ShipperStateDropEdit { get; }
		public ControlReference ShipperPostCodeTextBox { get; }
		public ControlReference ShipperPhoneTextBox { get; }
		public ControlReference ShipperRegNoTextBox { get; }
		public ControlReference ShipperRegNoTypeDropEdit { get; }

		public ControlReference ConsigneeSeparatorUserControl { get; }
		public ControlReference ConsigneeAddressControl { get; }
		public ControlReference ConsigneeNameTextBox { get; }
		public ControlReference ConsigneeStreet1TextBox { get; }
		public ControlReference ConsigneeStreet2TextBox { get; }
		public ControlReference ConsigneeCityTextBox { get; }
		public ControlReference ConigneeCountryCodeFindBox { get; }
		public ControlReference ConsigneeStateDropEdit { get; }
		public ControlReference ConsigneePostcodeTextBox { get; }
		public ControlReference ConsigneePhoneTextBox { get; }
		public ControlReference ConsigneeRegoNoTextBox { get; }
		public ControlReference ConsigneeRegNoTypeDropEdit { get; }

		public ControlReference NotifyPartySeparatorUserControl { get; }
		public ControlReference NotifyPartyAddressControl { get; }
		public ControlReference NotifyPartyNameTextBox { get; }
		public ControlReference NotifyPartyStreet1TextBox { get; }
		public ControlReference NotifyPartyStreet2TextBox { get; }
		public ControlReference NotifyPartyCityTextBox { get; }
		public ControlReference NotifyPartyCountryCodeFindBox { get; }
		public ControlReference NotifyPartyStateDropEdit { get; }
		public ControlReference NotifyPartyPostcodeTextBox { get; }
		public ControlReference NotifyPartyPhoneTextBox { get; }
		public ControlReference NotifyPartyRegNoTextBox { get; }
		public ControlReference NotifyPartyRegNoTypeDropEdit { get; }
	}
}
