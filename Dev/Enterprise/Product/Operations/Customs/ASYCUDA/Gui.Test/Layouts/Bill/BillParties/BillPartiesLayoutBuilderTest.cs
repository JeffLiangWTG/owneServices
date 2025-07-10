using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ASYCUDA.GUI.Testing
{
	[TestedType(typeof(BillPartiesLayoutBuilder<AsycudaBill>))]
	sealed class BillPartiesLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<BillPartiesLayoutBuilder<AsycudaBill>, AsycudaBill, CommonBillPartiesControlBag>
	{
		public void TestConvertShipperToOrganizationButtonVisibility()
		{
			var bill = GetAsycudaBill();
			var expectedPropertyInfoDependencies = new ZPropertyInfo[] { bill.ABL_OA_ShipperInfo, bill.ABL_ShipperNameInfo, bill.ABL_ShipperStreet1Info, bill.ABL_ShipperStreet2Info, bill.ABL_ShipperCityInfo, bill.ABL_ShipperStateInfo, bill.ABL_ShipperPostcodeInfo, bill.ABL_RN_NKShipperCountryInfo };
			AssertConvertButtonVisible(bill, CommonBillPartiesControlBag.Instance.ConvertShipperToOrganizationButton, bill.ABL_ShipperStreet1Info, expectedPropertyInfoDependencies);
		}

		public void TestConvertConsigneeToOrganizationButton()
		{
			var bill = GetAsycudaBill();
			var expectedPropertyInfoDependencies = new ZPropertyInfo[] { bill.ABL_OA_ConsigneeInfo, bill.ABL_ConsigneeNameInfo, bill.ABL_ConsigneeStreet1Info, bill.ABL_ConsigneeStreet2Info, bill.ABL_ConsigneeCityInfo, bill.ABL_ConsigneeStateInfo, bill.ABL_ConsigneePostcodeInfo, bill.ABL_RN_NKConsigneeCountryInfo, bill.ABL_ConsigneePhoneInfo };
			AssertConvertButtonVisible(bill, CommonBillPartiesControlBag.Instance.ConvertConsigneeToOrganizationButton, bill.ABL_ConsigneeNameInfo, expectedPropertyInfoDependencies);
		}

		public void TestConvertNotifyPartyToOrganizationButton()
		{
			var bill = GetAsycudaBill();
			var expectedPropertyInfoDependencies = new ZPropertyInfo[] { bill.ABL_OA_NotifyPartyInfo, bill.ABL_NotifyPartyNameInfo, bill.ABL_NotifyPartyStreet1Info, bill.ABL_NotifyPartyStreet2Info, bill.ABL_NotifyPartyCityInfo, bill.ABL_NotifyPartyStateInfo, bill.ABL_NotifyPartyPostcodeInfo, bill.ABL_RN_NKNotifyPartyCountryInfo, bill.ABL_NotifyPartyPhoneInfo };
			AssertConvertButtonVisible(bill, CommonBillPartiesControlBag.Instance.ConvertNotifyPartyToOrganizationButton, bill.ABL_NotifyPartyCityInfo, expectedPropertyInfoDependencies);
		}

		public void TestConvertBuyerToOrganizationButton()
		{
			var bill = GetAsycudaBill();
			var expectedPropertyInfoDependencies = new ZPropertyInfo[] { bill.ABL_OA_BuyerInfo, bill.ABL_BuyerNameInfo, bill.ABL_BuyerStreet1Info, bill.ABL_BuyerStreet2Info, bill.ABL_BuyerCityInfo, bill.ABL_BuyerStateInfo, bill.ABL_BuyerPostcodeInfo, bill.ABL_RN_NKBuyerCountryInfo, bill.ABL_BuyerPhoneInfo };
			AssertConvertButtonVisible(bill, CommonBillPartiesControlBag.Instance.ConvertBuyerToOrganizationButton, bill.ABL_BuyerCityInfo, expectedPropertyInfoDependencies);
		}

		public void TestConvertSellerToOrganizationButton()
		{
			var bill = GetAsycudaBill();
			var expectedPropertyInfoDependencies = new ZPropertyInfo[] { bill.ABL_OA_SellerInfo, bill.ABL_SellerNameInfo, bill.ABL_SellerStreet1Info, bill.ABL_SellerStreet2Info, bill.ABL_SellerCityInfo, bill.ABL_SellerStateInfo, bill.ABL_SellerPostcodeInfo, bill.ABL_RN_NKSellerCountryInfo, bill.ABL_SellerPhoneInfo };
			AssertConvertButtonVisible(bill, CommonBillPartiesControlBag.Instance.ConvertSellerToOrganizationButton, bill.ABL_SellerCityInfo, expectedPropertyInfoDependencies);
		}

		protected override int ExpectedMaxColumns => 3;

		protected override BillPartiesLayoutBuilder<AsycudaBill> GetColumnLayoutBuilderForTesting() => new BillPartiesLayoutBuilder<AsycudaBill>();

		AsycudaBill GetAsycudaBill()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.SuspendCheckBusinessObjectType();
			return header.Bills.AddNew();
		}

		void AssertConvertButtonVisible(AsycudaBill bill, ControlReference controlToTest, ZPropertyInfo propertyInfoToMakeVisible, ZPropertyInfo[] expectedPropertyInfoDependencies)
		{
			var layout = ((IPanelLayoutProvider)new DefaultBillPartiesLayouts()).Layout;
			var controlVisibilityDependencies = layout.GetVisibilityDependencies(controlToTest, bill);

			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder("PropertyInfo's that change visibility", expectedPropertyInfoDependencies, controlVisibilityDependencies);

				AssertEquals("Default no visible", false, layout.IsVisible(controlToTest, bill));
				propertyInfoToMakeVisible.Value = new ZString("SOMETHING");
				AssertEquals("any property entered now visible", true, layout.IsVisible(controlToTest, bill));
			});
		}
	}
}
