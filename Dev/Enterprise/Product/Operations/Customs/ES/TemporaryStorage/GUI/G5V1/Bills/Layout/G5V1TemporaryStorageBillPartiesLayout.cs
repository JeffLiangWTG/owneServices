using System;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.TemporaryStorage.GUI
{
	public class G5V1TemporaryStorageBillPartiesLayout : IPanelLayoutWithGridProvider
	{
		PanelLayout BillPartiesLayout { get; }

		PanelLayout IPanelLayoutProvider.Layout => BillPartiesLayout;

		public G5V1TemporaryStorageBillPartiesLayout()
		{
			BillPartiesLayout = CreateG5V1TemporaryStorageLayout();
		}

		public Type GridUserControlType => typeof(EU.TemporaryStorage.GUI.UCC6TemporaryStorageBillGridControl);

		PanelLayout CreateG5V1TemporaryStorageLayout()
		{
			var builder = new EU.TemporaryStorage.GUI.UCC6TemporaryStorageBillPartiesBuilder();
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

			return builder.Build();
		}
	}
}
