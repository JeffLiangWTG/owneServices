using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.ES.Manifest.H7.Business;
using Enterprise.Customs.EU.H7.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.Manifest.H7.GUI
{
	public class ESH7BillPartiesLayout : IPanelLayoutProvider
	{
		public ESH7BillPartiesLayout()
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

			builder.Add(common.ConsigneeSeparatorUserControl, ControlWidthClass.Long);
			builder.Add(common.ConsigneeAddressControl, ControlWidthClass.Long);
			builder.Add(common.ConsigneeNameTextBox, ControlWidthClass.Long);
			builder.Add(common.ConsigneeRegoNoTextBox, ControlWidthClass.Long);
			builder.Add(common.ConsigneeRegNoTypeDropEdit, ControlWidthClass.Long);
			builder.Add(common.ConsigneeStreet1TextBox, ControlWidthClass.Long);
			builder.Add(common.ConsigneeStreet2TextBox, ControlWidthClass.Long);
			builder.Add(common.ConsigneeCityTextBox, ControlWidthClass.Long);
			builder.Add(controlBag.CountryOfImportDropEdit, ControlWidthClass.Auto);
			builder.Add(common.ConsigneeStateDropEdit, ControlWidthClass.Auto);
			builder.Add(common.ConsigneePostcodeTextBox, ControlWidthClass.Auto);
			builder.Add(common.ConsigneePhoneTextBox, ControlWidthClass.Long);
			builder.Add(common.ConsigneeEmailTextBox, ControlWidthClass.Long);

			builder.AddColumn();
			builder.Add(common.ShipperSeparatorUserControl, ControlWidthClass.Long);
			builder.Add(common.ShipperAddressControl, ControlWidthClass.Long);
			builder.Add(common.ShipperNameTextBox, ControlWidthClass.Long);
			builder.Add(common.ShipperRegNoTextBox, ControlWidthClass.Long);
			builder.Add(common.ShipperStreet1TextBox, ControlWidthClass.Long);
			builder.Add(common.ShipperStreet2TextBox, ControlWidthClass.Long);
			builder.Add(common.ShipperCityTextBox, ControlWidthClass.Long);
			builder.Add(controlBag.CountryOfExportDropEdit, ControlWidthClass.Auto);
			builder.Add(common.ShipperStateDropEdit, ControlWidthClass.Auto);
			builder.Add(common.ShipperPostCodeTextBox, ControlWidthClass.Auto);

			builder.AddColumn();
			builder.Add(common.SellerSeparatorUserControl, ControlWidthClass.Long);
			builder.Add(common.SellerAddressControl, ControlWidthClass.Long);
			builder.Add(common.SellerNameTextBox, ControlWidthClass.Long);
			builder.Add(common.SellerRegNoTypeDropEdit, ControlWidthClass.Long);
			builder.Add(common.SellerRegoNoTextBox, ControlWidthClass.Long);
			builder.Add(common.SellerStreet1TextBox, ControlWidthClass.Long);
			builder.Add(common.SellerStreet2TextBox, ControlWidthClass.Long);
			builder.Add(common.SellerCityTextBox, ControlWidthClass.Long);
			builder.Add(controlBag.CountryOfSellerDropEdit, ControlWidthClass.Auto);
			builder.Add(common.SellerStateDropEdit, ControlWidthClass.Auto);
			builder.Add(common.SellerPostcodeTextBox, ControlWidthClass.Auto);

			builder.SetCaption(common.ConsigneeSeparatorUserControl, _ => Res.GetData("1936b2df-314d-4ada-97ba-6b83f47c9049", "Importer"));
			builder.SetCaption(common.ShipperSeparatorUserControl, _ => Res.GetData("7dc6cd2b-f24f-49f0-bb33-39fc7d9a9dea", "Exporter"));
			builder.SetCaption(common.SellerSeparatorUserControl, _ => Res.GetData("c04ef6f1-426a-4ed2-8cce-15066281e90d", "Seller"));
			builder.SetCaption(common.ConsigneeRegoNoTextBox, _ => Res.GetData("e9dd9282-17e2-443d-ad8b-f36ea6647030", "Identification No."));
			builder.SetCaption(common.ShipperRegNoTextBox, _ => Res.GetData("30f0241c-a6ca-46f4-8a2f-c50654663452", "Identification No."));
			builder.SetCaption(common.ConsigneeRegNoTypeDropEdit, _ => Res.GetData("5eb17221-a869-47d6-a3f7-96cab4145fde", "ID No. Type"));
			builder.SetCaption(common.SellerRegoNoTextBox, _ => Res.GetData("cf5621c1-2f9f-4d68-82e4-26841f452758", "IOSS Number"));
			builder.SetCaption(controlBag.CountryOfImportDropEdit, x => Res.GetData("90e7a9c6-923f-4683-929c-d0d7ab0ba0c2", "Ctry/Rgn.", "Ctry/Rgn.", "Country/Region", "The Country/Region code for the Consignee address"));
			builder.SetCaption(controlBag.CountryOfExportDropEdit, x => Res.GetData("d2c27ebc-c759-4712-b5a3-c2310cb762ad", "Ctry/Rgn.", "Ctry/Rgn.", "Country/Region", "The Country/Region code for the Shipper address"));
			builder.SetCaption(controlBag.CountryOfSellerDropEdit, x => Res.GetData("36250b6d-9220-4615-bfb1-95b923cdbf36", "Ctry/Rgn.", "Ctry/Rgn.", "Country/Region", "The Country/Region code for the Seller address"));

			return builder.Build();
		}
	}
}
