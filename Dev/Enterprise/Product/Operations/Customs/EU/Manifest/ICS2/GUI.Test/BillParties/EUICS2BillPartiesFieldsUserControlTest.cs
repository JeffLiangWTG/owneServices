using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Customs.EU.Manifest.ICS2.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Manifest.ICS2.GUI.Test
{
	sealed class EUICS2BillPartiesFieldsUserControlTest : TestCaseWithFactory
	{
		[RequiresSTA]
		public void TestBuyerPersonTypeDropEdit()
		{
			var buyerPersonTypeDropEdit = form.BuyerPersonTypeDropEdit;
			CombineAssertions(() =>
			{
				AssertType<ZDropEdit>(buyerPersonTypeDropEdit);
				AssertEquals("BindingMember", nameof(AsycudaBill.BuyerPersonType), buyerPersonTypeDropEdit.GetBindingMember());
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
				AssertEquals("BindToOrgList", "Lookups.Sellers", sellerAddressControl.BindToOrgList);
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

		[RequiresSTA]
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
			var sellerPostcodeTextBox = form.SellerPostCodeTextBox;
			CombineAssertions(() =>
			{
				AssertType<ZTextBox>(sellerPostcodeTextBox);
				AssertEquals("BindingMember", nameof(AsycudaBill.ABL_SellerPostcode), sellerPostcodeTextBox.GetBindingMember());
				AssertEquals("Caption", "Postcode", sellerPostcodeTextBox.CaptionResourceString.Caption);
			});
		}

		public void TestSellerPhoneTextBox()
		{
			var sellerPostcodeTextBox = form.SellerPostCodeTextBox;
			CombineAssertions(() =>
			{
				AssertType<ZTextBox>(sellerPostcodeTextBox);
				AssertEquals("BindingMember", nameof(AsycudaBill.ABL_SellerPostcode), sellerPostcodeTextBox.GetBindingMember());
				AssertEquals("Caption", "Postcode", sellerPostcodeTextBox.CaptionResourceString.Caption);
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
			var sellerRegNoTextBox = form.SellerRegNoTextBox;
			CombineAssertions(() =>
			{
				AssertType<ZTextBox>(sellerRegNoTextBox);
				AssertEquals("BindingMember", nameof(AsycudaBill.ABL_SellerRegNo), sellerRegNoTextBox.GetBindingMember());
				AssertEquals("Caption", "Reg.No", sellerRegNoTextBox.CaptionResourceString.Caption);
			});
		}

		public void TestSellerPersonTypeDropEdit()
		{
			var sellerPersonTypeDropEdit = form.SellerPersonTypeDropEdit;
			CombineAssertions(() =>
			{
				AssertType<ZDropEdit>(sellerPersonTypeDropEdit);
				AssertEquals("BindingMember", nameof(AsycudaBill.SellerPersonType), sellerPersonTypeDropEdit.GetBindingMember());
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			form = new EUICS2BillPartiesFieldsUserControl();
		}
		EUICS2BillPartiesFieldsUserControl form;

		protected override void TearDown()
		{
			base.TearDown();

			form.Dispose();
		}
	}
}
