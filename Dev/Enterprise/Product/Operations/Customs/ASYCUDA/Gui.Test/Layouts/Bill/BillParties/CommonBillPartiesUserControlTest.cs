using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.ASYCUDA.GUI.Testing
{
	[TestedType(typeof(CommonBillPartiesUserControl))]
	sealed class CommonBillPartiesUserControlTest : TestCaseWithFactory
	{
		public void TestBuyerSeparatorUserControl()
		{
			var buyerSeparator = form.BuyerSeparatorUserControl;
			CombineAssertions(() =>
			{
				AssertType<SeparatorUserControl>(buyerSeparator);
				AssertEquals("Caption", "Buyer Details", buyerSeparator.CaptionResourceString.Caption);
			});
		}

		public void TestBuyerAddressControl()
		{
			var buyerAddressControl = form.BuyerAddressControl;
			CombineAssertions(() =>
			{
				AssertType<ZAddressControl>(buyerAddressControl);
				AssertEquals("BindingMember", nameof(AsycudaBill.ABL_OA_Buyer), buyerAddressControl.GetBindingMember());
				AssertEquals("BindToOrgList", "Lookups.Buyers", buyerAddressControl.BindToOrgList);
				AssertEquals("Caption", "Party", buyerAddressControl.CaptionResourceString.Caption);
			});
		}

		public void TestConvertBuyerToOrganizationButton()
		{
			var convertBuyerToOrganizationButton = form.ConvertBuyerToOrganizationButton;
			AssertEquals("Caption", "Convert Buyer To Org.", convertBuyerToOrganizationButton.CaptionResourceString.Caption);
		}

		public void TestBuyerNameTextBox()
		{
			var buyerNameTextBox = form.BuyerNameTextBox;
			CombineAssertions(() =>
			{
				AssertType<ZTextBox>(buyerNameTextBox);
				AssertEquals("BindingMember", nameof(AsycudaBill.ABL_BuyerName), buyerNameTextBox.GetBindingMember());
				AssertEquals("Caption", "Name", buyerNameTextBox.CaptionResourceString.Caption);
			});
		}

		public void TestBuyerStreet1TextBox()
		{
			var buyerStreet1TextBox = form.BuyerStreet1TextBox;
			CombineAssertions(() =>
			{
				AssertType<ZTextBox>(buyerStreet1TextBox);
				AssertEquals("BindingMember", nameof(AsycudaBill.ABL_BuyerStreet1), buyerStreet1TextBox.GetBindingMember());
				AssertEquals("Caption", "Street 1", buyerStreet1TextBox.CaptionResourceString.Caption);
			});
		}

		public void TestBuyerStreet2TextBox()
		{
			var buyerStreet2TextBox = form.BuyerStreet2TextBox;
			CombineAssertions(() =>
			{
				AssertType<ZTextBox>(buyerStreet2TextBox);
				AssertEquals("BindingMember", nameof(AsycudaBill.ABL_BuyerStreet2), buyerStreet2TextBox.GetBindingMember());
				AssertEquals("Caption", "Street 2", buyerStreet2TextBox.CaptionResourceString.Caption);
			});
		}

		public void TestBuyerCityTextBox()
		{
			var buyerCityTextBox = form.BuyerCityTextBox;
			CombineAssertions(() =>
			{
				AssertType<ZTextBox>(buyerCityTextBox);
				AssertEquals("BindingMember", nameof(AsycudaBill.ABL_BuyerCity), buyerCityTextBox.GetBindingMember());
				AssertEquals("Caption", "City", buyerCityTextBox.CaptionResourceString.Caption);
			});
		}

		public void TestBuyerCountryCodeFindBox()
		{
			var buyerCountryCodeFindBox = form.BuyerCountryCodeFindBox;
			CombineAssertions(() =>
			{
				AssertType<ZCodeFindBox>(buyerCountryCodeFindBox);
				AssertEquals("BindingMember", nameof(AsycudaBill.ABL_RN_NKBuyerCountry), buyerCountryCodeFindBox.GetBindingMember());

				var resourceStringData = buyerCountryCodeFindBox.CaptionResourceString;
				AssertEquals("Short Caption", "Ctry/Rgn.", resourceStringData.ShortCaption);
				AssertEquals("Caption", "Country/Region", resourceStringData.Caption);
				AssertEquals("Full Caption", "The Country/Region code for the Buyer address", resourceStringData.FullDescription);
			});
		}

		public void TestBuyerStateDropEdit()
		{
			var buyerStateDropEdit = form.BuyerStateDropEdit;
			CombineAssertions(() =>
			{
				AssertType<ZDropEdit>(buyerStateDropEdit);
				AssertEquals("BindingMember", nameof(AsycudaBill.ABL_BuyerState), buyerStateDropEdit.GetBindingMember());
				AssertEquals("Caption", "State", buyerStateDropEdit.CaptionResourceString.Caption);
			});
		}

		public void TestBuyerPostCodeTextBox()
		{
			var buyerPostcodeTextBox = form.BuyerPostcodeTextBox;
			CombineAssertions(() =>
			{
				AssertType<ZTextBox>(buyerPostcodeTextBox);
				AssertEquals("BindingMember", nameof(AsycudaBill.ABL_BuyerPostcode), buyerPostcodeTextBox.GetBindingMember());
				AssertEquals("Caption", "Postcode", buyerPostcodeTextBox.CaptionResourceString.Caption);
			});
		}

		public void TestBuyerPhoneTextBox()
		{
			var buyerPhoneTextBox = form.BuyerPhoneTextBox;
			CombineAssertions(() =>
			{
				AssertType<ZTextBox>(buyerPhoneTextBox);
				AssertEquals("BindingMember", nameof(AsycudaBill.ABL_BuyerPhone), buyerPhoneTextBox.GetBindingMember());
				AssertEquals("Caption", "Phone", buyerPhoneTextBox.CaptionResourceString.Caption);
			});
		}

		public void TestBuyerRegNoTypeDropEdit()
		{
			var buyerRegNoTypeDropEdit = form.BuyerRegNoTypeDropEdit;
			CombineAssertions(() =>
			{
				AssertType<ZDropEdit>(buyerRegNoTypeDropEdit);
				AssertEquals("BindingMember", nameof(AsycudaBill.ABL_BuyerRegNoType), buyerRegNoTypeDropEdit.GetBindingMember());
				AssertEquals("Caption", "Reg. No. Type", buyerRegNoTypeDropEdit.CaptionResourceString.Caption);
			});
		}

		public void TestBuyerRegNoTextBox()
		{
			var buyerRegNoTextBox = form.BuyerRegNoTextBox;
			CombineAssertions(() =>
			{
				AssertType<ZTextBox>(buyerRegNoTextBox);
				AssertEquals("BindingMember", nameof(AsycudaBill.ABL_BuyerRegNo), buyerRegNoTextBox.GetBindingMember());
				AssertEquals("Caption", "Reg.No", buyerRegNoTextBox.CaptionResourceString.Caption);
			});
		}

		public void TestSellerSeparatorUserControl()
		{
			var sellerSeparator = form.SellerSeparatorUserControl;
			CombineAssertions(() =>
			{
				AssertType<SeparatorUserControl>(sellerSeparator);
				AssertEquals("Caption", "Seller Details", sellerSeparator.CaptionResourceString.Caption);
			});
		}

		public void TestSellerAddressControl()
		{
			var sellerAddressControl = form.SellerAddressControl;
			CombineAssertions(() =>
			{
				AssertType<ZAddressControl>(sellerAddressControl);
				AssertEquals("BindingMember", nameof(AsycudaBill.ABL_OA_Seller), sellerAddressControl.GetBindingMember());
				AssertEquals("BindToOrgList", "Lookups.SellersOrgList", sellerAddressControl.BindToOrgList);
				AssertEquals("Caption", "Party", sellerAddressControl.CaptionResourceString.Caption);
			});
		}

		public void TestConvertSellerToOrganizationButton()
		{
			var convertSellerToOrganizationButton = form.ConvertSellerToOrganizationButton;
			AssertEquals("Caption", "Convert Seller To Org.", convertSellerToOrganizationButton.CaptionResourceString.Caption);
		}

		public void TestSellerNameTextBox()
		{
			var sellerNameTextBox = form.SellerNameTextBox;
			CombineAssertions(() =>
			{
				AssertType<ZTextBox>(sellerNameTextBox);
				AssertEquals("BindingMember", nameof(AsycudaBill.ABL_SellerName), sellerNameTextBox.GetBindingMember());
				AssertEquals("Caption", "Name", sellerNameTextBox.CaptionResourceString.Caption);
			});
		}

		public void TestSellerStreet1TextBox()
		{
			var sellerStreet1TextBox = form.SellerStreet1TextBox;
			CombineAssertions(() =>
			{
				AssertType<ZTextBox>(sellerStreet1TextBox);
				AssertEquals("BindingMember", nameof(AsycudaBill.ABL_SellerStreet1), sellerStreet1TextBox.GetBindingMember());
				AssertEquals("Caption", "Street 1", sellerStreet1TextBox.CaptionResourceString.Caption);
			});
		}

		public void TestSellerStreet2TextBox()
		{
			var sellerStreet2TextBox = form.SellerStreet2TextBox;
			CombineAssertions(() =>
			{
				AssertType<ZTextBox>(sellerStreet2TextBox);
				AssertEquals("BindingMember", nameof(AsycudaBill.ABL_SellerStreet2), sellerStreet2TextBox.GetBindingMember());
				AssertEquals("Caption", "Street 2", sellerStreet2TextBox.CaptionResourceString.Caption);
			});
		}

		public void TestSellerCityTextBox()
		{
			var sellerCityTextBox = form.SellerCityTextBox;
			CombineAssertions(() =>
			{
				AssertType<ZTextBox>(sellerCityTextBox);
				AssertEquals("BindingMember", nameof(AsycudaBill.ABL_SellerCity), sellerCityTextBox.GetBindingMember());
				AssertEquals("Caption", "City", sellerCityTextBox.CaptionResourceString.Caption);
			});
		}

		public void TestSellerCountryCodeFindBox()
		{
			var sellerCountryCodeFindBox = form.SellerCountryCodeFindBox;
			CombineAssertions(() =>
			{
				AssertType<ZCodeFindBox>(sellerCountryCodeFindBox);
				AssertEquals("BindingMember", nameof(AsycudaBill.ABL_RN_NKSellerCountry), sellerCountryCodeFindBox.GetBindingMember());

				var resourceStringData = sellerCountryCodeFindBox.CaptionResourceString;
				AssertEquals("Short Caption", "Ctry/Rgn.", resourceStringData.ShortCaption);
				AssertEquals("Caption", "Country/Region", resourceStringData.Caption);
				AssertEquals("Full Caption", "The Country/Region code for the Seller address", resourceStringData.FullDescription);
			});
		}

		public void TestSellerStateDropEdit()
		{
			var sellerStateDropEdit = form.SellerStateDropEdit;
			CombineAssertions(() =>
			{
				AssertType<ZDropEdit>(sellerStateDropEdit);
				AssertEquals("BindingMember", nameof(AsycudaBill.ABL_SellerState), sellerStateDropEdit.GetBindingMember());
				AssertEquals("Caption", "State", sellerStateDropEdit.CaptionResourceString.Caption);
			});
		}

		public void TestSellerPostCodeTextBox()
		{
			var sellerPostcodeTextBox = form.SellerPostcodeTextBox;
			CombineAssertions(() =>
			{
				AssertType<ZTextBox>(sellerPostcodeTextBox);
				AssertEquals("BindingMember", nameof(AsycudaBill.ABL_SellerPostcode), sellerPostcodeTextBox.GetBindingMember());
				AssertEquals("Caption", "Postcode", sellerPostcodeTextBox.CaptionResourceString.Caption);
			});
		}

		public void TestSellerPhoneTextBox()
		{
			var sellerPhoneTextBox = form.SellerPhoneTextBox;
			CombineAssertions(() =>
			{
				AssertType<ZTextBox>(sellerPhoneTextBox);
				AssertEquals("BindingMember", nameof(AsycudaBill.ABL_SellerPhone), sellerPhoneTextBox.GetBindingMember());
				AssertEquals("Caption", "Phone", sellerPhoneTextBox.CaptionResourceString.Caption);
			});
		}

		public void TestSellerRegNoTypeDropEdit()
		{
			var sellerRegNoTypeDropEdit = form.SellerRegNoTypeDropEdit;
			CombineAssertions(() =>
			{
				AssertType<ZDropEdit>(sellerRegNoTypeDropEdit);
				AssertEquals("BindingMember", nameof(AsycudaBill.ABL_SellerRegNoType), sellerRegNoTypeDropEdit.GetBindingMember());
				AssertEquals("Caption", "Reg. No. Type", sellerRegNoTypeDropEdit.CaptionResourceString.Caption);
			});
		}

		public void TestSellerRegNoTextBox()
		{
			var sellerRegoNoTextBox = form.SellerRegoNoTextBox;
			CombineAssertions(() =>
			{
				AssertType<ZTextBox>(sellerRegoNoTextBox);
				AssertEquals("BindingMember", nameof(AsycudaBill.ABL_SellerRegNo), sellerRegoNoTextBox.GetBindingMember());
				AssertEquals("Caption", "Reg.No", sellerRegoNoTextBox.CaptionResourceString.Caption);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			form = new CommonBillPartiesUserControl();
		}
		CommonBillPartiesUserControl form;

		protected override void TearDown()
		{
			base.TearDown();

			form.Dispose();
		}
	}
}
