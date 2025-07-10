using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ASYCUDA.GUI
{
	public class BillPartiesLayoutBuilder<T> : ColumnLayoutBuilder<T, CommonBillPartiesControlBag> where T : Business.AsycudaBill
	{
		public override CommonBillPartiesControlBag CommonBag { get; } = CommonBillPartiesControlBag.Instance;

		protected override int MaxColumns => 3;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		protected override void SetDefaultVisibilities()
		{
			base.SetDefaultVisibilities();

			SetVisibility(CommonBag.ConvertShipperToOrganizationButton, b => b.CanConvertShipperToOrganization, b => b.ABL_OA_ShipperInfo, b => b.ABL_ShipperNameInfo, b => b.ABL_ShipperStreet1Info,
				b => b.ABL_ShipperStreet2Info, b => b.ABL_ShipperCityInfo, b => b.ABL_ShipperStateInfo, b => b.ABL_ShipperPostcodeInfo, b => b.ABL_RN_NKShipperCountryInfo);

			SetVisibility(CommonBag.ConvertConsigneeToOrganizationButton, b => b.CanConvertConsigneeToOrganization, b => b.ABL_OA_ConsigneeInfo, b => b.ABL_ConsigneeNameInfo, b => b.ABL_ConsigneeStreet1Info,
				b => b.ABL_ConsigneeStreet2Info, b => b.ABL_ConsigneeCityInfo, b => b.ABL_ConsigneeStateInfo, b => b.ABL_ConsigneePostcodeInfo, b => b.ABL_RN_NKConsigneeCountryInfo, b => b.ABL_ConsigneePhoneInfo);

			SetVisibility(CommonBag.ConvertNotifyPartyToOrganizationButton, b => b.CanConvertNotifyPartyToOrganization, b => b.ABL_OA_NotifyPartyInfo, b => b.ABL_NotifyPartyNameInfo, b => b.ABL_NotifyPartyStreet1Info,
				 b => b.ABL_NotifyPartyStreet2Info, b => b.ABL_NotifyPartyCityInfo, b => b.ABL_NotifyPartyStateInfo, b => b.ABL_NotifyPartyPostcodeInfo, b => b.ABL_RN_NKNotifyPartyCountryInfo, b => b.ABL_NotifyPartyPhoneInfo);

			SetVisibility(CommonBag.ConvertBuyerToOrganizationButton, b => b.CanConvertBuyerToOrganization, b => b.ABL_OA_BuyerInfo, b => b.ABL_BuyerNameInfo, b => b.ABL_BuyerStreet1Info,
				 b => b.ABL_BuyerStreet2Info, b => b.ABL_BuyerCityInfo, b => b.ABL_BuyerStateInfo, b => b.ABL_BuyerPostcodeInfo, b => b.ABL_RN_NKBuyerCountryInfo, b => b.ABL_BuyerPhoneInfo);

			SetVisibility(CommonBag.ConvertSellerToOrganizationButton, b => b.CanConvertSellerToOrganization, b => b.ABL_OA_SellerInfo, b => b.ABL_SellerNameInfo, b => b.ABL_SellerStreet1Info,
				b => b.ABL_SellerStreet2Info, b => b.ABL_SellerCityInfo, b => b.ABL_SellerStateInfo, b => b.ABL_SellerPostcodeInfo, b => b.ABL_RN_NKSellerCountryInfo, b => b.ABL_SellerPhoneInfo);
		}
	}
}
