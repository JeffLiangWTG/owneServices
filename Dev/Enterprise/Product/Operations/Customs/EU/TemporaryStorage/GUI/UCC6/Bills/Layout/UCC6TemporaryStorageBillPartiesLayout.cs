using System;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.TemporaryStorage.GUI
{
	public class UCC6TemporaryStorageBillPartiesLayout : IPanelLayoutWithGridProvider
	{
		PanelLayout BillPartiesLayout { get; }

		PanelLayout IPanelLayoutProvider.Layout => BillPartiesLayout;

		public UCC6TemporaryStorageBillPartiesLayout()
		{
			BillPartiesLayout = CreateUCC6TemporaryStorageLayout();
		}

		public Type GridUserControlType => typeof(UCC6TemporaryStorageBillGridControl);

		PanelLayout CreateUCC6TemporaryStorageLayout()
		{
			var builder = new UCC6TemporaryStorageBillPartiesBuilder();
			var commonBag = builder.CommonBag;

			builder.AddColumn();
			builder.Add(commonBag.ShipperSeparatorUserControl, ControlWidthClass.Long);
			builder.Add(commonBag.ShipperAddressControl, ControlWidthClass.Long);
			builder.Add(commonBag.ShipperNameTextBox, ControlWidthClass.Long);
			builder.Add(commonBag.ShipperStreet1TextBox, ControlWidthClass.Long);
			builder.Add(commonBag.ShipperStreet2TextBox, ControlWidthClass.Long);
			builder.Add(commonBag.ShipperCityTextBox, ControlWidthClass.Long);
			builder.Add(commonBag.ShipperCountryCodeFindBox, ControlWidthClass.Auto);
			builder.Add(commonBag.ShipperStateDropEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.ShipperPostCodeTextBox, ControlWidthClass.Auto);
			builder.Add(commonBag.ShipperPhoneTextBox, ControlWidthClass.Long);
			builder.Add(commonBag.ShipperRegNoTypeDropEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.ShipperRegNoTextBox, ControlWidthClass.Long);

			builder.AddColumn();
			builder.Add(commonBag.ConsigneeSeparatorUserControl, ControlWidthClass.Long);
			builder.Add(commonBag.ConsigneeAddressControl, ControlWidthClass.Long);
			builder.Add(commonBag.ConsigneeNameTextBox, ControlWidthClass.Long);
			builder.Add(commonBag.ConsigneeStreet1TextBox, ControlWidthClass.Long);
			builder.Add(commonBag.ConsigneeStreet2TextBox, ControlWidthClass.Long);
			builder.Add(commonBag.ConsigneeCityTextBox, ControlWidthClass.Long);
			builder.Add(commonBag.ConigneeCountryCodeFindBox, ControlWidthClass.Auto);
			builder.Add(commonBag.ConsigneeStateDropEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.ConsigneePostcodeTextBox, ControlWidthClass.Auto);
			builder.Add(commonBag.ConsigneePhoneTextBox, ControlWidthClass.Long);
			builder.Add(commonBag.ConsigneeRegNoTypeDropEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.ConsigneeRegoNoTextBox, ControlWidthClass.Long);

			builder.AddColumn();
			builder.Add(commonBag.NotifyPartySeparatorUserControl, ControlWidthClass.Long);
			builder.Add(commonBag.NotifyPartyAddressControl, ControlWidthClass.Long);
			builder.Add(commonBag.NotifyPartyNameTextBox, ControlWidthClass.Long);
			builder.Add(commonBag.NotifyPartyStreet1TextBox, ControlWidthClass.Long);
			builder.Add(commonBag.NotifyPartyStreet2TextBox, ControlWidthClass.Long);
			builder.Add(commonBag.NotifyPartyCityTextBox, ControlWidthClass.Long);
			builder.Add(commonBag.NotifyPartyCountryCodeFindBox, ControlWidthClass.Auto);
			builder.Add(commonBag.NotifyPartyStateDropEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.NotifyPartyPostcodeTextBox, ControlWidthClass.Auto);
			builder.Add(commonBag.NotifyPartyPhoneTextBox, ControlWidthClass.Long);
			builder.Add(commonBag.NotifyPartyRegNoTypeDropEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.NotifyPartyRegNoTextBox, ControlWidthClass.Long);
			return builder.Build();
		}
	}
}
