using System.Collections.Generic;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.GB.H7.GUI.Bill;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GB.H7.GUI.Testing
{
	[TestedType(typeof(H7BillPartiesLayout))]
	sealed class H7BillPartiesLayoutTestTest : LayoutsAbstractTest
	{
		protected override int ControlBagCount => 2;

		protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
		{
			get
			{
				yield return FirstColumnControls;
				yield return SecondColumnControls;
				yield return ThirdColumnControls;
			}
		}

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new BillPartiesLayoutBuilder<Business.AsycudaBill>();

		IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
		{
			get
			{
				yield return (CommonBillPartiesControlBag.Instance.ConsigneeSeparatorUserControl, ControlWidthClass.Long);
				yield return (CommonBillPartiesControlBag.Instance.ConsigneeAddressControl, ControlWidthClass.Long);
				yield return (CommonBillPartiesControlBag.Instance.ConsigneeNameTextBox, ControlWidthClass.Long);
				yield return (CommonBillPartiesControlBag.Instance.ConsigneeRegoNoTextBox, ControlWidthClass.Long);
				yield return (H7BillPartiesControlBag.Instance.VatNumberTextBox, ControlWidthClass.Long);
				yield return (H7BillPartiesControlBag.Instance.PostponedVatAccountingCheckBox, ControlWidthClass.Long);
				yield return (CommonBillPartiesControlBag.Instance.ConsigneeStreet1TextBox, ControlWidthClass.Long);
				yield return (CommonBillPartiesControlBag.Instance.ConsigneeStreet2TextBox, ControlWidthClass.Long);
				yield return (CommonBillPartiesControlBag.Instance.ConsigneeCityTextBox, ControlWidthClass.Long);
				yield return (CommonBillPartiesControlBag.Instance.ConigneeCountryCodeFindBox, ControlWidthClass.Auto);
				yield return (CommonBillPartiesControlBag.Instance.ConsigneeStateDropEdit, ControlWidthClass.Auto);
				yield return (CommonBillPartiesControlBag.Instance.ConsigneePostcodeTextBox, ControlWidthClass.Auto);
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> SecondColumnControls
		{
			get
			{
				yield return (CommonBillPartiesControlBag.Instance.ShipperSeparatorUserControl, ControlWidthClass.Long);
				yield return (CommonBillPartiesControlBag.Instance.ShipperAddressControl, ControlWidthClass.Long);
				yield return (CommonBillPartiesControlBag.Instance.ShipperNameTextBox, ControlWidthClass.Long);
				yield return (CommonBillPartiesControlBag.Instance.ShipperRegNoTextBox, ControlWidthClass.Long);
				yield return (CommonBillPartiesControlBag.Instance.ShipperStreet1TextBox, ControlWidthClass.Long);
				yield return (CommonBillPartiesControlBag.Instance.ShipperStreet2TextBox, ControlWidthClass.Long);
				yield return (CommonBillPartiesControlBag.Instance.ShipperCityTextBox, ControlWidthClass.Long);
				yield return (CommonBillPartiesControlBag.Instance.ShipperCountryCodeFindBox, ControlWidthClass.Auto);
				yield return (CommonBillPartiesControlBag.Instance.ShipperStateDropEdit, ControlWidthClass.Auto);
				yield return (CommonBillPartiesControlBag.Instance.ShipperPostCodeTextBox, ControlWidthClass.Auto);
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> ThirdColumnControls
		{
			get
			{
				yield return (CommonBillPartiesControlBag.Instance.SellerSeparatorUserControl, ControlWidthClass.Long);
				yield return (CommonBillPartiesControlBag.Instance.SellerAddressControl, ControlWidthClass.Long);
				yield return (CommonBillPartiesControlBag.Instance.SellerNameTextBox, ControlWidthClass.Long);
				yield return (CommonBillPartiesControlBag.Instance.SellerRegNoTypeDropEdit, ControlWidthClass.Long);
				yield return (CommonBillPartiesControlBag.Instance.SellerRegoNoTextBox, ControlWidthClass.Long);
				yield return (CommonBillPartiesControlBag.Instance.SellerStreet1TextBox, ControlWidthClass.Long);
				yield return (CommonBillPartiesControlBag.Instance.SellerStreet2TextBox, ControlWidthClass.Long);
				yield return (CommonBillPartiesControlBag.Instance.SellerCityTextBox, ControlWidthClass.Long);
				yield return (CommonBillPartiesControlBag.Instance.SellerCountryCodeFindBox, ControlWidthClass.Auto);
				yield return (CommonBillPartiesControlBag.Instance.SellerStateDropEdit, ControlWidthClass.Auto);
				yield return (CommonBillPartiesControlBag.Instance.SellerPostcodeTextBox, ControlWidthClass.Auto);
			}
		}

		public void TestCaptions()
		{
			AssertCaption(CommonBillPartiesControlBag.Instance.ConsigneeRegoNoTextBox, expectedCaption: "Identification No.");
			AssertCaption(CommonBillPartiesControlBag.Instance.ShipperRegNoTextBox, expectedCaption: "Identification No.");
			AssertCaption(CommonBillPartiesControlBag.Instance.ConsigneeSeparatorUserControl, expectedCaption: "Importer");
			AssertCaption(CommonBillPartiesControlBag.Instance.ShipperSeparatorUserControl, expectedCaption: "Exporter");
			AssertCaption(CommonBillPartiesControlBag.Instance.SellerRegoNoTextBox, expectedCaption: "IOSS Number");

			void AssertCaption(ControlReference controlReference, string expectedCaption)
			{
				LayoutForTesting.TryGetCaption(controlReference, null, out var captionData);
				AssertEquals($"Caption for {controlReference.ControlName}", expectedCaption, captionData?.Caption);
			}
		}
	}
}
