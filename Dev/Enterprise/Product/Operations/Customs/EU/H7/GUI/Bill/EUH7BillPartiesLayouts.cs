using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.EU.H7.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.H7.GUI
{
	public class EUH7BillPartiesLayouts : IPanelLayoutProvider
	{
		public EUH7BillPartiesLayouts()
		{
			BillParties = CreateBillPartiesLayout();
		}

		PanelLayout BillParties { get; }

		PanelLayout IPanelLayoutProvider.Layout => BillParties;

		PanelLayout CreateBillPartiesLayout()
		{
			var builder = new BillPartiesLayoutBuilder<AsycudaBill>();
			var common = builder.CommonBag;

			var controlBag = EUH7BillPartiesControlBag.Instance;
			builder.AddControlBag(controlBag);

			builder.AddColumn();
			builder.Add(controlBag.ImporterSeparatorUserControl, ControlWidthClass.Long);
			builder.Add(common.ConsigneeAddressControl, ControlWidthClass.Long);
			builder.Add(common.ConsigneeNameTextBox, ControlWidthClass.Long);
			builder.Add(controlBag.IdentificationNoTextBox, ControlWidthClass.Long);
			builder.Add(controlBag.ImporterIdentificationTypeDropEdit, ControlWidthClass.Long);
			builder.Add(common.ConsigneeStreet1TextBox, ControlWidthClass.Long);
			builder.Add(common.ConsigneeStreet2TextBox, ControlWidthClass.Long);
			builder.Add(common.ConsigneeCityTextBox, ControlWidthClass.Long);
			builder.Add(common.ConigneeCountryCodeFindBox, ControlWidthClass.Auto);
			builder.Add(common.ConsigneeStateDropEdit, ControlWidthClass.Auto);
			builder.Add(common.ConsigneePostcodeTextBox, ControlWidthClass.Auto);

			builder.AddColumn();
			builder.Add(controlBag.ExporterSeparatorUserControl, ControlWidthClass.Long);
			builder.Add(common.ShipperAddressControl, ControlWidthClass.Long);
			builder.Add(common.ShipperNameTextBox, ControlWidthClass.Long);
			builder.Add(common.ShipperRegNoTextBox, ControlWidthClass.Long);
			builder.Add(common.ShipperStreet1TextBox, ControlWidthClass.Long);
			builder.Add(common.ShipperStreet2TextBox, ControlWidthClass.Long);
			builder.Add(common.ShipperCityTextBox, ControlWidthClass.Long);
			builder.Add(common.ShipperCountryCodeFindBox, ControlWidthClass.Auto);
			builder.Add(common.ShipperStateDropEdit, ControlWidthClass.Auto);
			builder.Add(common.ShipperPostCodeTextBox, ControlWidthClass.Auto);

			builder.AddColumn();
			builder.Add(controlBag.SellerSeparatorUserControl, ControlWidthClass.Long);
			builder.Add(common.SellerAddressControl, ControlWidthClass.Long);
			builder.Add(common.SellerNameTextBox, ControlWidthClass.Long);
			builder.Add(common.SellerRegNoTypeDropEdit, ControlWidthClass.Long);
			builder.Add(common.SellerRegoNoTextBox, ControlWidthClass.Long);
			builder.Add(common.SellerStreet1TextBox, ControlWidthClass.Long);
			builder.Add(common.SellerStreet2TextBox, ControlWidthClass.Long);
			builder.Add(common.SellerCityTextBox, ControlWidthClass.Long);
			builder.Add(common.SellerCountryCodeFindBox, ControlWidthClass.Auto);
			builder.Add(common.SellerStateDropEdit, ControlWidthClass.Auto);
			builder.Add(common.SellerPostcodeTextBox, ControlWidthClass.Auto);

			builder.SetCaption(common.ConsigneeAddressControl, x => Res.GetData("7e6a52c5-97e0-499a-967a-6fc9e1ae5a1a", "Party", "Party", "Party", "The party who makes, or on whose behalf an import declaration is made."));
			builder.SetCaption(common.ConsigneeNameTextBox, x => Res.GetData("92b2afbe-b500-45af-acee-ecb12e4b40c6", "Name", "Name", "Party Name", "Importer full name and where applicable the legal form of the party."));
			builder.SetCaption(controlBag.IdentificationNoTextBox, x => Res.GetData("f1eaa61c-33ae-424c-9562-4f1c0faa4c0d", "Id. No.", "Identification No.", "Identification Number", "Identification Number associated with the Identification Number Type used to identify the importer party."));
			builder.SetCaption(controlBag.ImporterIdentificationTypeDropEdit, x => Res.GetData("a8da837a-0d39-47f9-b649-bfdbe504eb2c", "ID No. Type", "ID No. Type", "Identification Number Type", "Identification Number Type used to identify the importer party."));
			builder.SetCaption(common.ConsigneeStreet1TextBox, x => Res.GetData("14505291-06ab-4f68-9f6d-22dc283b1f70", "St. 1", "Street 1", "Street Address 1", "Name of the street of the importer party’s address and the number of the building or facility."));
			builder.SetCaption(common.ConsigneeStreet2TextBox, x => Res.GetData("ed1c50df-ad94-4ce8-9ef1-8fc1e9687f61", "St. 2", "Street 2", "Street Address 2", "Name of the street of the importer party’s address and the number of the building or facility."));
			builder.SetCaption(common.ConsigneeCityTextBox, x => Res.GetData("83a73816-adcc-4d57-ad40-b3cd6b52889a", "Cty.", "City", "City Name", "City name of the importer party's address."));
			builder.SetCaption(common.ConigneeCountryCodeFindBox, x => Res.GetData("f3da5a8e-94b6-49ce-9d77-ab92d31b2ac3", "Ctry/Rgn.", "Ctry/Rgn.", "Country/Region", "ISO 3166-1 alpha-2 country code of the importer party's address."));
			builder.SetCaption(common.ConsigneeStateDropEdit, x => Res.GetData("10a4e22c-0ee8-411e-924e-a95f57e23b48", "St.", "State", "State", "State code of the importer party’s address."));
			builder.SetCaption(common.ConsigneePostcodeTextBox, x => Res.GetData("06b7bb64-fb53-4e83-aa45-91868c0b28ff", "PC", "PC", "Postcode", "Postcode of the importer party’s address."));

			builder.SetCaption(common.ShipperAddressControl, x => Res.GetData("59d747cc-21b3-4e58-8eb7-2a87296715e4", "Party", "Party", "Party", "The party consigning the goods as stipulated in the transport contract by the party ordering the transport."));
			builder.SetCaption(common.ShipperNameTextBox, x => Res.GetData("bcc431e1-e87c-4bbe-8ccb-cb70e704cd53", "Name", "Name", "Party Name", "Exporter full name and where applicable the legal form of the party."));
			builder.SetCaption(common.ShipperRegNoTextBox, x => Res.GetData("A530FAF4-80D8-4FD1-93E1-8A4838DEDE83", "Id. No.", "Identification No.", "Identification Number", "The EORI number of the last seller of the goods prior to their importation into the European Union (EU)."));
			builder.SetCaption(common.ShipperStreet1TextBox, x => Res.GetData("6464f72d-7768-40cf-98b6-f6c153661ea4", "St. 1", "Street 1", "Street Address 1", "Name of the street of the exporter party’s address and the number of the building or facility."));
			builder.SetCaption(common.ShipperStreet2TextBox, x => Res.GetData("242541ef-5e0f-46ba-be78-7e1c0cc36573", "St. 2", "Street 2", "Street Address 2", "Name of the street of the exporter party’s address and the number of the building or facility."));
			builder.SetCaption(common.ShipperCityTextBox, x => Res.GetData("fd07f4f8-44a3-409d-bc40-14a6a906b092", "Cty.", "City", "City Name", "City name of the exporter party's address."));
			builder.SetCaption(common.ShipperCountryCodeFindBox, x => Res.GetData("ac58fb1d-00ad-4bf2-9c8e-6fee4a19430f", "Ctry/Rgn.", "Ctry/Rgn.", "Country/Region", "ISO 3166-1 alpha-2 country code of the exporter party's address."));
			builder.SetCaption(common.ShipperStateDropEdit, x => Res.GetData("7e490647-e99e-4fb5-bd9d-0418016cba91", "St.", "State", "State", "State code of the exporter party’s address."));
			builder.SetCaption(common.ShipperPostCodeTextBox, x => Res.GetData("ab352e53-bd30-4b8e-b449-b7827c2db63a", "PC", "PC", "Postcode", "Postcode of the exporter party’s address."));

			builder.SetCaption(common.SellerAddressControl, x => Res.GetData("c6cef67e-6ec9-42e7-97dd-1ed3272d7dcf", "Party", "Party", "Party", "The party selling the goods."));
			builder.SetCaption(common.SellerNameTextBox, x => Res.GetData("3bc3656a-d670-47ba-8244-b1bf60348090", "Name", "Name", "Party Name", "Seller full name and where applicable the legal form of the party."));
			builder.SetCaption(common.SellerRegNoTypeDropEdit, x => Res.GetData("e322f0b1-32bd-4ea5-aba4-1464b40429ca", "Reg. No. Type", "Reg. No. Type", "Registration Number Type", "Identification Number Type used to identify the seller party."));
			builder.SetCaption(common.SellerRegoNoTextBox, x => Res.GetData("d2d47bba-80ff-4921-b3c1-a04ed571e71b", "IOSS No.", "IOSS No.", "IOSS Number", "Import One-Stop-Shop VAT Registration Number of the seller party."));
			builder.SetCaption(common.SellerStreet1TextBox, x => Res.GetData("adf4f094-197b-4db1-aac3-efbab9c1295d", "St. 1", "Street 1", "Street Address 1", "Name of the street of the seller party’s address and the number of the building or facility."));
			builder.SetCaption(common.SellerStreet2TextBox, x => Res.GetData("74f87c4a-80a5-4ae2-8091-5ff61afc100d", "St. 2", "Street 2", "Street Address 2", "Name of the street of the seller party’s address and the number of the building or facility."));
			builder.SetCaption(common.SellerCityTextBox, x => Res.GetData("af42c028-7a53-4842-b6e5-b293d385d25a", "Cty.", "City", "City Name", "City name of the seller party's address."));
			builder.SetCaption(common.SellerCountryCodeFindBox, x => Res.GetData("581c5241-cf96-41b6-9f03-d439f6e0c56b", "Ctry/Rgn.", "Ctry/Rgn.", "Country/Region", "ISO 3166-1 alpha-2 country code of the seller party's address."));
			builder.SetCaption(common.SellerStateDropEdit, x => Res.GetData("175b0dfd-d5b4-4f36-ad2f-c435db89c1ba", "St.", "State", "State", "State code of the seller party’s address."));
			builder.SetCaption(common.SellerPostcodeTextBox, x => Res.GetData("2894c030-5d4c-41b5-ab7e-fdfb240859e9", "PC", "PC", "Postcode", "Postcode of the seller party’s address."));

			return builder.Build();
		}
	}
}
