using System.Collections.Generic;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.H7.GUI.Testing
{
	[TestedType(typeof(EUH7BillPartiesLayouts))]
	sealed class EUH7BillPartiesLayoutsTest : LayoutsAbstractTest
	{
		protected override int ControlBagCount => 2;

		protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
		{
			get
			{
				yield return FirstColumnControls;
				yield return SecondColumnControls;
				yield return ThridColumnControls;
			}
		}

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new BillPartiesLayoutBuilder<Business.AsycudaBill>();

		IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
		{
			get
			{
				yield return (EUH7BillPartiesControlBag.Instance.ImporterSeparatorUserControl, ControlWidthClass.Long);
				yield return (CommonBillPartiesControlBag.Instance.ConsigneeAddressControl, ControlWidthClass.Long);
				yield return (CommonBillPartiesControlBag.Instance.ConsigneeNameTextBox, ControlWidthClass.Long);
				yield return (EUH7BillPartiesControlBag.Instance.IdentificationNoTextBox, ControlWidthClass.Long);
				yield return (EUH7BillPartiesControlBag.Instance.ImporterIdentificationTypeDropEdit, ControlWidthClass.Long);
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
				yield return (EUH7BillPartiesControlBag.Instance.ExporterSeparatorUserControl, ControlWidthClass.Long);
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

		IEnumerable<(ControlReference, ControlWidthClass)> ThridColumnControls
		{
			get
			{
				yield return (EUH7BillPartiesControlBag.Instance.SellerSeparatorUserControl, ControlWidthClass.Long);
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
			AssertCaption(CommonBillPartiesControlBag.Instance.ConsigneeAddressControl, "Party", "Party", "Party", "The party who makes, or on whose behalf an import declaration is made.");
			AssertCaption(CommonBillPartiesControlBag.Instance.ConsigneeNameTextBox, "Party Name", "Name", "Name", "Importer full name and where applicable the legal form of the party.");
			AssertCaption(EUH7BillPartiesControlBag.Instance.IdentificationNoTextBox, "Identification Number", "Id. No.", "Identification No.", "Identification Number associated with the Identification Number Type used to identify the importer party.");
			AssertCaption(EUH7BillPartiesControlBag.Instance.ImporterIdentificationTypeDropEdit, "Identification Number Type", "ID No. Type", "ID No. Type", "Identification Number Type used to identify the importer party.");
			AssertCaption(CommonBillPartiesControlBag.Instance.ConsigneeStreet1TextBox, "Street Address 1", "St. 1", "Street 1", "Name of the street of the importer party’s address and the number of the building or facility.");
			AssertCaption(CommonBillPartiesControlBag.Instance.ConsigneeStreet2TextBox, "Street Address 2", "St. 2", "Street 2", "Name of the street of the importer party’s address and the number of the building or facility.");
			AssertCaption(CommonBillPartiesControlBag.Instance.ConsigneeCityTextBox, "City Name", "Cty.", "City", "City name of the importer party's address.");
			AssertCaption(CommonBillPartiesControlBag.Instance.ConigneeCountryCodeFindBox, "Country/Region", "Ctry/Rgn.", "Ctry/Rgn.", "ISO 3166-1 alpha-2 country code of the importer party's address.");
			AssertCaption(CommonBillPartiesControlBag.Instance.ConsigneeStateDropEdit, "State", "St.", "State", "State code of the importer party’s address.");
			AssertCaption(CommonBillPartiesControlBag.Instance.ConsigneePostcodeTextBox, "Postcode", "PC", "PC", "Postcode of the importer party’s address.");

			AssertCaption(CommonBillPartiesControlBag.Instance.ShipperAddressControl, "Party", "Party", "Party", "The party consigning the goods as stipulated in the transport contract by the party ordering the transport.");
			AssertCaption(CommonBillPartiesControlBag.Instance.ShipperNameTextBox, "Party Name", "Name", "Name", "Exporter full name and where applicable the legal form of the party.");
			AssertCaption(CommonBillPartiesControlBag.Instance.ShipperRegNoTextBox, "Identification Number", "Id. No.", "Identification No.", "The EORI number of the last seller of the goods prior to their importation into the European Union (EU).");
			AssertCaption(CommonBillPartiesControlBag.Instance.ShipperStreet1TextBox, "Street Address 1", "St. 1", "Street 1", "Name of the street of the exporter party’s address and the number of the building or facility.");
			AssertCaption(CommonBillPartiesControlBag.Instance.ShipperStreet2TextBox, "Street Address 2", "St. 2", "Street 2", "Name of the street of the exporter party’s address and the number of the building or facility.");
			AssertCaption(CommonBillPartiesControlBag.Instance.ShipperCityTextBox, "City Name", "Cty.", "City", "City name of the exporter party's address.");
			AssertCaption(CommonBillPartiesControlBag.Instance.ShipperCountryCodeFindBox, "Country/Region", "Ctry/Rgn.", "Ctry/Rgn.", "ISO 3166-1 alpha-2 country code of the exporter party's address.");
			AssertCaption(CommonBillPartiesControlBag.Instance.ShipperStateDropEdit, "State", "St.", "State", "State code of the exporter party’s address.");
			AssertCaption(CommonBillPartiesControlBag.Instance.ShipperPostCodeTextBox, "Postcode", "PC", "PC", "Postcode of the exporter party’s address.");

			AssertCaption(CommonBillPartiesControlBag.Instance.SellerAddressControl, "Party", "Party", "Party", "The party selling the goods.");
			AssertCaption(CommonBillPartiesControlBag.Instance.SellerNameTextBox, "Party Name", "Name", "Name", "Seller full name and where applicable the legal form of the party.");
			AssertCaption(CommonBillPartiesControlBag.Instance.SellerRegNoTypeDropEdit, "Registration Number Type", "Reg. No. Type", "Reg. No. Type", "Identification Number Type used to identify the seller party.");
			AssertCaption(CommonBillPartiesControlBag.Instance.SellerRegoNoTextBox, "IOSS Number", "IOSS No.", "IOSS No.", "Import One-Stop-Shop VAT Registration Number of the seller party.");
			AssertCaption(CommonBillPartiesControlBag.Instance.SellerStreet1TextBox, "Street Address 1", "St. 1", "Street 1", "Name of the street of the seller party’s address and the number of the building or facility.");
			AssertCaption(CommonBillPartiesControlBag.Instance.SellerStreet2TextBox, "Street Address 2", "St. 2", "Street 2", "Name of the street of the seller party’s address and the number of the building or facility.");
			AssertCaption(CommonBillPartiesControlBag.Instance.SellerCityTextBox, "City Name", "Cty.", "City", "City name of the seller party's address.");
			AssertCaption(CommonBillPartiesControlBag.Instance.SellerCountryCodeFindBox, "Country/Region", "Ctry/Rgn.", "Ctry/Rgn.", "ISO 3166-1 alpha-2 country code of the seller party's address.");
			AssertCaption(CommonBillPartiesControlBag.Instance.SellerStateDropEdit, "State", "St.", "State", "State code of the seller party’s address.");
			AssertCaption(CommonBillPartiesControlBag.Instance.SellerPostcodeTextBox, "Postcode", "PC", "PC", "Postcode of the seller party’s address.");

			void AssertCaption(ControlReference controlReference, string expectedCaption, string expectedShortCaption, string expectedMediumCaption, string expectedFullDescription)
			{
				LayoutForTesting.TryGetCaption(controlReference, null, out var captionData);
				AssertEquals($"Caption for {controlReference.ControlName}", expectedCaption, captionData?.Caption);
				AssertEquals($"ShortCaption for {controlReference.ControlName}", expectedShortCaption, captionData?.ShortCaption);
				AssertEquals($"MediumCaption for {controlReference.ControlName}", expectedMediumCaption, captionData?.MediumCaption);
				AssertEquals($"FullDescription for {controlReference.ControlName}", expectedFullDescription, captionData?.FullDescription);
			}
		}
	}
}
