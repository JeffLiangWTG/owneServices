using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AR.Manifest.GUI
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
	public class ARBillPartiesLayouts : IPanelLayoutProvider
	{
		PanelLayout BillParties { get; }

		PanelLayout IPanelLayoutProvider.Layout => BillParties;

		public ARBillPartiesLayouts()
		{
			BillParties = CreateBillPartiesLayout();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		PanelLayout CreateBillPartiesLayout()
		{
			var builder = new BillPartiesLayoutBuilder<AsycudaBill>();
			var common = builder.CommonBag;

			builder.AddColumn();
			builder.Add(common.ShipperSeparatorUserControl, ControlWidthClass.Long);
			builder.Add(common.ShipperAddressControl, ControlWidthClass.Long);
			builder.Add(common.ConvertShipperToOrganizationButton, ControlWidthClass.Long);
			builder.Add(common.ShipperNameTextBox, ControlWidthClass.Long);
			builder.Add(common.ShipperStreet1TextBox, ControlWidthClass.Long);
			builder.Add(common.ShipperStreet2TextBox, ControlWidthClass.Long);
			builder.Add(common.ShipperCityTextBox, ControlWidthClass.Long);
			builder.Add(common.ShipperCountryCodeFindBox, ControlWidthClass.Auto);
			builder.Add(common.ShipperStateDropEdit, ControlWidthClass.Auto);
			builder.Add(common.ShipperPostCodeTextBox, ControlWidthClass.Auto);
			builder.Add(common.ShipperPhoneTextBox, ControlWidthClass.Long);
			builder.Add(common.ShipperRegNoTypeDropEdit, ControlWidthClass.Long);
			builder.Add(common.ShipperRegNoTextBox, ControlWidthClass.Long);

			builder.AddColumn();
			builder.Add(common.ConsigneeSeparatorUserControl, ControlWidthClass.Long);
			builder.Add(common.ConsigneeAddressControl, ControlWidthClass.Long);
			builder.Add(common.ConvertConsigneeToOrganizationButton, ControlWidthClass.Long);
			builder.Add(common.ConsigneeNameTextBox, ControlWidthClass.Long);
			builder.Add(common.ConsigneeStreet1TextBox, ControlWidthClass.Long);
			builder.Add(common.ConsigneeStreet2TextBox, ControlWidthClass.Long);
			builder.Add(common.ConsigneeCityTextBox, ControlWidthClass.Long);
			builder.Add(common.ConigneeCountryCodeFindBox, ControlWidthClass.Auto);
			builder.Add(common.ConsigneeStateDropEdit, ControlWidthClass.Auto);
			builder.Add(common.ConsigneePostcodeTextBox, ControlWidthClass.Auto);
			builder.Add(common.ConsigneePhoneTextBox, ControlWidthClass.Long);
			builder.Add(common.ConsigneeRegNoTypeDropEdit, ControlWidthClass.Long);
			builder.Add(common.ConsigneeRegoNoTextBox, ControlWidthClass.Long);

			builder.AddColumn();
			builder.Add(common.NotifyPartySeparatorUserControl, ControlWidthClass.Long);
			builder.Add(common.NotifyPartyAddressControl, ControlWidthClass.Long);
			builder.Add(common.ConvertNotifyPartyToOrganizationButton, ControlWidthClass.Long);
			builder.Add(common.NotifyPartyNameTextBox, ControlWidthClass.Long);
			builder.Add(common.NotifyPartyStreet1TextBox, ControlWidthClass.Long);
			builder.Add(common.NotifyPartyStreet2TextBox, ControlWidthClass.Long);
			builder.Add(common.NotifyPartyCityTextBox, ControlWidthClass.Long);
			builder.Add(common.NotifyPartyCountryCodeFindBox, ControlWidthClass.Auto);
			builder.Add(common.NotifyPartyStateDropEdit, ControlWidthClass.Auto);
			builder.Add(common.NotifyPartyPostcodeTextBox, ControlWidthClass.Auto);
			builder.Add(common.NotifyPartyPhoneTextBox, ControlWidthClass.Long);
			builder.Add(common.NotifyPartyRegNoTextBox, ControlWidthClass.Long);

			return builder.Build();
		}
	}
}
