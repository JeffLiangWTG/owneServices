using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.Manifest.ICS2.GUI
{
	public class EUICS2BillPartiesControlBag : ControlBag
	{
		public static EUICS2BillPartiesControlBag Instance => billPartiesControlBag.Value;

		public EUICS2BillPartiesControlBag()
		{
			ShipperPersonTypeDropEdit = RegisterControl(nameof(EUICS2BillPartiesFieldsUserControl.ShipperPersonTypeDropEdit));
			ConsigneePersonTypeDropEdit = RegisterControl(nameof(EUICS2BillPartiesFieldsUserControl.ConsigneePersonTypeDropEdit));
			NotifyPartyPersonTypeDropEdit = RegisterControl(nameof(EUICS2BillPartiesFieldsUserControl.NotifyPartyPersonTypeDropEdit));
			BuyerPersonTypeDropEdit = RegisterControl(nameof(EUICS2BillPartiesFieldsUserControl.BuyerPersonTypeDropEdit));

			SellerSeparatorUserControl = RegisterControl(nameof(EUICS2BillPartiesFieldsUserControl.SellerSeparatorUserControl));
			SellerAddressControl = RegisterControl(nameof(EUICS2BillPartiesFieldsUserControl.SellerAddressControl));
			ConvertSellerToOrganizationButton = RegisterControl(nameof(EUICS2BillPartiesFieldsUserControl.ConvertSellerToOrganizationButton));
			SellerNameTextBox = RegisterControl(nameof(EUICS2BillPartiesFieldsUserControl.SellerNameTextBox));
			SellerStreet1TextBox = RegisterControl(nameof(EUICS2BillPartiesFieldsUserControl.SellerStreet1TextBox));
			SellerStreet2TextBox = RegisterControl(nameof(EUICS2BillPartiesFieldsUserControl.SellerStreet2TextBox));
			SellerCityTextBox = RegisterControl(nameof(EUICS2BillPartiesFieldsUserControl.SellerCityTextBox));
			SellerCountryCodeFindBox = RegisterControl(nameof(EUICS2BillPartiesFieldsUserControl.SellerCountryCodeFindBox));
			SellerStateDropEdit = RegisterControl(nameof(EUICS2BillPartiesFieldsUserControl.SellerStateDropEdit));
			SellerPhoneTextBox = RegisterControl(nameof(EUICS2BillPartiesFieldsUserControl.SellerPhoneTextBox));
			SellerPostCodeTextBox = RegisterControl(nameof(EUICS2BillPartiesFieldsUserControl.SellerPostCodeTextBox));
			SellerRegNoTextBox = RegisterControl(nameof(EUICS2BillPartiesFieldsUserControl.SellerRegNoTextBox));
			SellerRegNoTypeDropEdit = RegisterControl(nameof(EUICS2BillPartiesFieldsUserControl.SellerRegNoTypeDropEdit));
			SellerPersonTypeDropEdit = RegisterControl(nameof(EUICS2BillPartiesFieldsUserControl.SellerPersonTypeDropEdit));
		}

		public ControlReference ShipperPersonTypeDropEdit { get; }
		public ControlReference ConsigneePersonTypeDropEdit { get; }
		public ControlReference NotifyPartyPersonTypeDropEdit { get; }
		public ControlReference BuyerPersonTypeDropEdit { get; }

		public ControlReference SellerSeparatorUserControl { get; }
		public ControlReference SellerAddressControl { get; }
		public ControlReference ConvertSellerToOrganizationButton { get; }
		public ControlReference SellerNameTextBox { get; }
		public ControlReference SellerStreet1TextBox { get; }
		public ControlReference SellerStreet2TextBox { get; }
		public ControlReference SellerCityTextBox { get; }
		public ControlReference SellerCountryCodeFindBox { get; }
		public ControlReference SellerStateDropEdit { get; }
		public ControlReference SellerPostCodeTextBox { get; }
		public ControlReference SellerPhoneTextBox { get; }
		public ControlReference SellerRegNoTextBox { get; }
		public ControlReference SellerRegNoTypeDropEdit { get; }
		public ControlReference SellerPersonTypeDropEdit { get; }

		protected override Control CreateTemplate() => new EUICS2BillPartiesFieldsUserControl();

		[WTG.StaticAnalysis.Annotation.ThreadSafe]
		readonly static Lazy<EUICS2BillPartiesControlBag> billPartiesControlBag = new Lazy<EUICS2BillPartiesControlBag>(() => new EUICS2BillPartiesControlBag());
	}
}
