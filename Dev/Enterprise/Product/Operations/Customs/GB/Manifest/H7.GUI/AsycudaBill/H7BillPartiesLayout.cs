using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.EU.H7.Business;
using Enterprise.Customs.GB.H7.GUI.Bill;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GB.H7.GUI
{
	public class H7BillPartiesLayout : IPanelLayoutProvider
	{
		public H7BillPartiesLayout()
		{
			BillParties = CreateBillPartiesLayout();
		}

		PanelLayout BillParties { get; }

		PanelLayout IPanelLayoutProvider.Layout => BillParties;

		PanelLayout CreateBillPartiesLayout()
		{
			var builder = new BillPartiesLayoutBuilder<AsycudaBill>();
			var common = builder.CommonBag;
			var gbH7BillPartiesControlBag = H7BillPartiesControlBag.Instance;
			builder.AddControlBag(gbH7BillPartiesControlBag);

			builder.AddColumn();
			builder.Add(common.ConsigneeSeparatorUserControl, ControlWidthClass.Long);
			builder.Add(common.ConsigneeAddressControl, ControlWidthClass.Long);
			builder.Add(common.ConsigneeNameTextBox, ControlWidthClass.Long);
			builder.Add(common.ConsigneeRegoNoTextBox, ControlWidthClass.Long);
			builder.Add(gbH7BillPartiesControlBag.VatNumberTextBox, ControlWidthClass.Long);
			builder.Add(gbH7BillPartiesControlBag.PostponedVatAccountingCheckBox, ControlWidthClass.Long);
			builder.Add(common.ConsigneeStreet1TextBox, ControlWidthClass.Long);
			builder.Add(common.ConsigneeStreet2TextBox, ControlWidthClass.Long);
			builder.Add(common.ConsigneeCityTextBox, ControlWidthClass.Long);
			builder.Add(common.ConigneeCountryCodeFindBox, ControlWidthClass.Auto);
			builder.Add(common.ConsigneeStateDropEdit, ControlWidthClass.Auto);
			builder.Add(common.ConsigneePostcodeTextBox, ControlWidthClass.Auto);

			builder.AddColumn();
			builder.Add(common.ShipperSeparatorUserControl, ControlWidthClass.Long);
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
			builder.Add(common.SellerSeparatorUserControl, ControlWidthClass.Long);
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

			builder.SetCaption(common.ConsigneeSeparatorUserControl, _ => Res.GetData("82223b0d-fede-4265-87e8-b898c712a313", "Importer"));
			builder.SetCaption(common.ShipperSeparatorUserControl, _ => Res.GetData("b4a9ebca-ed82-435d-a72b-bff587dc9fe3", "Exporter"));
			builder.SetCaption(common.ConsigneeRegoNoTextBox, _ => Res.GetData("17e8c039-da3e-4363-b073-a9320f013a9c", "Identification No."));
			builder.SetCaption(common.ShipperRegNoTextBox, _ => Res.GetData("a4f1f441-8b60-498a-9793-f3afc170eeb3", "Identification No."));
			builder.SetCaption(common.SellerRegoNoTextBox, _ => Res.GetData("79bbff62-9904-4237-af96-c986221e25b2", "IOSS Number"));

			return builder.Build();
		}
	}
}
