using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.JP.Manifest.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.JP.Manifest.GUI
{
	public sealed class JPBillPartiesLayout : IPanelLayoutProvider
	{
		PanelLayout BillPartiesLayout { get; }

		public PanelLayout Layout => BillPartiesLayout;

		public JPBillPartiesLayout()
		{
			BillPartiesLayout = CreateBillPartiesLayout();
		}

		PanelLayout CreateBillPartiesLayout()
		{
			AddControls();
			SetVisibilities();
			return Builder.Build();
		}

		void AddControls()
		{
			var common = Builder.CommonBag;
			var jpBillPartiesBag = JPBillPartiesControlBag.Instance;
			Builder.AddControlBag(jpBillPartiesBag);

			Builder.AddColumn();
			Builder.Add(common.ShipperSeparatorUserControl, ControlWidthClass.Long);
			Builder.Add(common.ShipperAddressControl, ControlWidthClass.Long);
			Builder.Add(common.ConvertShipperToOrganizationButton, ControlWidthClass.Long);
			Builder.Add(common.ShipperNameTextBox, ControlWidthClass.Long);
			Builder.Add(common.ShipperStreet1TextBox, ControlWidthClass.Long);
			Builder.Add(common.ShipperStreet2TextBox, ControlWidthClass.Long);
			Builder.Add(common.ShipperCityTextBox, ControlWidthClass.Long);
			Builder.Add(common.ShipperCountryCodeFindBox, ControlWidthClass.Auto);
			Builder.Add(common.ShipperStateDropEdit, ControlWidthClass.Auto);
			Builder.Add(common.ShipperPostCodeTextBox, ControlWidthClass.Auto);
			Builder.Add(common.ShipperPhoneTextBox, ControlWidthClass.Long);
			Builder.Add(common.ShipperRegNoTextBox, ControlWidthClass.Long);
			Builder.Add(jpBillPartiesBag.ShipperRegNoPanel, ControlWidthClass.Long);

			Builder.AddColumn();
			Builder.Add(common.ConsigneeSeparatorUserControl, ControlWidthClass.Long);
			Builder.Add(common.ConsigneeAddressControl, ControlWidthClass.Long);
			Builder.Add(common.ConvertConsigneeToOrganizationButton, ControlWidthClass.Long);
			Builder.Add(common.ConsigneeNameTextBox, ControlWidthClass.Long);
			Builder.Add(common.ConsigneeStreet1TextBox, ControlWidthClass.Long);
			Builder.Add(common.ConsigneeStreet2TextBox, ControlWidthClass.Long);
			Builder.Add(common.ConsigneeCityTextBox, ControlWidthClass.Long);
			Builder.Add(common.ConigneeCountryCodeFindBox, ControlWidthClass.Auto);
			Builder.Add(common.ConsigneeStateDropEdit, ControlWidthClass.Auto);
			Builder.Add(common.ConsigneePostcodeTextBox, ControlWidthClass.Auto);
			Builder.Add(common.ConsigneePhoneTextBox, ControlWidthClass.Long);
			Builder.Add(common.ConsigneeRegoNoTextBox, ControlWidthClass.Long);
			Builder.Add(jpBillPartiesBag.ConsigneeRegNoPanel, ControlWidthClass.Long);

			Builder.AddColumn();
			Builder.Add(common.NotifyPartySeparatorUserControl, ControlWidthClass.Long);
			Builder.Add(common.NotifyPartyAddressControl, ControlWidthClass.Long);
			Builder.Add(common.ConvertNotifyPartyToOrganizationButton, ControlWidthClass.Long);
			Builder.Add(common.NotifyPartyNameTextBox, ControlWidthClass.Long);
			Builder.Add(common.NotifyPartyStreet1TextBox, ControlWidthClass.Long);
			Builder.Add(common.NotifyPartyStreet2TextBox, ControlWidthClass.Long);
			Builder.Add(common.NotifyPartyCityTextBox, ControlWidthClass.Long);
			Builder.Add(common.NotifyPartyCountryCodeFindBox, ControlWidthClass.Auto);
			Builder.Add(common.NotifyPartyStateDropEdit, ControlWidthClass.Auto);
			Builder.Add(common.NotifyPartyPostcodeTextBox, ControlWidthClass.Auto);
			Builder.Add(common.NotifyPartyPhoneTextBox, ControlWidthClass.Long);
			Builder.Add(common.NotifyPartyRegNoTextBox, ControlWidthClass.Long);
			Builder.Add(jpBillPartiesBag.NotifyPartyRegNoPanel, ControlWidthClass.Long);
		}

		void SetVisibilities()
		{
			var common = Builder.CommonBag;
			var jpBillPartiesBag = JPBillPartiesControlBag.Instance;

			Builder.SetVisibility(common.ShipperRegNoTextBox, b => !b.ShouldShowShipperRegNoType);
			Builder.SetVisibility(jpBillPartiesBag.ShipperRegNoPanel, b => b.ShouldShowShipperRegNoType);
			Builder.SetVisibility(common.ConvertShipperToOrganizationButton, b => b.CanConvertShipperToOrganization, b => b.ABL_OA_ShipperInfo, b => b.ABL_ShipperNameInfo, b => b.ABL_ShipperStreet1Info,
				b => b.ABL_ShipperStreet2Info, b => b.ABL_ShipperCityInfo, b => b.ABL_ShipperStateInfo, b => b.ABL_ShipperPostcodeInfo, b => b.ABL_RN_NKShipperCountryInfo, b => b.ABL_ShipperPhoneInfo,
				b => b.ABL_ShipperRegNoTypeInfo, b => b.ABL_ShipperRegNoInfo);
			Builder.SetVisibility(common.ConsigneeRegoNoTextBox, b => !b.ShouldShowConsigneeRegNoType);
			Builder.SetVisibility(jpBillPartiesBag.ConsigneeRegNoPanel, b => b.ShouldShowConsigneeRegNoType);
			Builder.SetVisibility(common.ConvertConsigneeToOrganizationButton, b => b.CanConvertConsigneeToOrganization, b => b.ABL_OA_ConsigneeInfo, b => b.ABL_ConsigneeNameInfo, b => b.ABL_ConsigneeStreet1Info,
				b => b.ABL_ConsigneeStreet2Info, b => b.ABL_ConsigneeCityInfo, b => b.ABL_ConsigneeStateInfo, b => b.ABL_ConsigneePostcodeInfo, b => b.ABL_RN_NKConsigneeCountryInfo, b => b.ABL_ConsigneePhoneInfo,
				b => b.ABL_ConsigneeRegNoTypeInfo, b => b.ABL_ConsigneeRegNoInfo);
			Builder.SetVisibility(jpBillPartiesBag.NotifyPartyRegNoPanel, b => b.ShouldShowNotifyPartyRegNoType);
			SetThirdColumnControlsVisibilities(common);
		}

		void SetThirdColumnControlsVisibilities(CommonBillPartiesControlBag common)
		{
			Builder.SetVisibility(common.NotifyPartySeparatorUserControl, b => !b.IsHCH);
			Builder.SetVisibility(common.NotifyPartyAddressControl, b => !b.IsHCH);
			Builder.SetVisibility(common.ConvertNotifyPartyToOrganizationButton, b => b.CanConvertNotifyPartyToOrganization, b => b.ABL_OA_NotifyPartyInfo, b => b.ABL_NotifyPartyNameInfo, b => b.ABL_NotifyPartyStreet1Info,
				b => b.ABL_NotifyPartyStreet2Info, b => b.ABL_NotifyPartyCityInfo, b => b.ABL_NotifyPartyStateInfo, b => b.ABL_NotifyPartyPostcodeInfo, b => b.ABL_RN_NKNotifyPartyCountryInfo, b => b.ABL_NotifyPartyPhoneInfo,
				b => b.ABL_NotifyPartyRegNoTypeInfo, b => b.ABL_NotifyPartyRegNoInfo);
			Builder.SetVisibility(common.NotifyPartyNameTextBox, b => !b.IsHCH);
			Builder.SetVisibility(common.NotifyPartyStreet1TextBox, b => !b.IsHCH);
			Builder.SetVisibility(common.NotifyPartyStreet2TextBox, b => !b.IsHCH);
			Builder.SetVisibility(common.NotifyPartyCityTextBox, b => !b.IsHCH);
			Builder.SetVisibility(common.NotifyPartyCountryCodeFindBox, b => !b.IsHCH);
			Builder.SetVisibility(common.NotifyPartyStateDropEdit, b => !b.IsHCH);
			Builder.SetVisibility(common.NotifyPartyPostcodeTextBox, b => !b.IsHCH);
			Builder.SetVisibility(common.NotifyPartyPhoneTextBox, b => !b.IsHCH);
			Builder.SetVisibility(common.NotifyPartyRegNoTextBox, b => !b.IsHCH && !b.ShouldShowNotifyPartyRegNoType);
		}

		BillPartiesLayoutBuilder<AsycudaBill> Builder => builder ??= new BillPartiesLayoutBuilder<AsycudaBill>();
		BillPartiesLayoutBuilder<AsycudaBill> builder;
	}
}
