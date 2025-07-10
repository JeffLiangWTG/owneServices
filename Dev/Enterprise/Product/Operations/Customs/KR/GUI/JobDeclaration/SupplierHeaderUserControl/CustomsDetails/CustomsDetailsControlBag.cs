using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public sealed class CustomsDetailsControlBag : ControlBag
	{
		protected override Control CreateTemplate() => new CustomsDetailsUserControl();

		[ThreadStatic]
		static CustomsDetailsControlBag instance;

		public static CustomsDetailsControlBag Instance => instance ?? (instance = new CustomsDetailsControlBag());

		CustomsDetailsControlBag()
		{
			BillZGuidDropEdit = RegisterControl(nameof(CustomsDetailsUserControl.BillZGuidDropEdit));
			CargoManagementNoTextBox = RegisterControl(nameof(CustomsDetailsUserControl.CargoManagementNoTextBox));
			COStatusDropEdit = RegisterControl(nameof(CustomsDetailsUserControl.COStatusDropEdit));
			ValuationDeclarationStatusDropEdit = RegisterControl(nameof(CustomsDetailsUserControl.ValuationDeclarationStatusDropEdit));
			BlanketValuationDeclarationNoTextBox = RegisterControl(nameof(CustomsDetailsUserControl.BlanketValuationDeclarationNoTextBox));
			CustomsBrokerCommentMultiTextBox = RegisterControl(nameof(CustomsDetailsUserControl.CustomsBrokerCommentMultiTextBox));
			SupplierZOrganisationFindBox = RegisterControl(nameof(CustomsDetailsUserControl.SupplierZOrganisationFindBox));
			ShipperZAddressControl = RegisterControl(nameof(CustomsDetailsUserControl.ShipperZAddressControl));
			EmptyLabel = RegisterControl(nameof(CustomsDetailsUserControl.EmptyLabel));
			OnlineTradeTypeDropEdit = RegisterControl(nameof(CustomsDetailsUserControl.OnlineTradeTypeDropEdit));
			OnlineTradeDistributorZAddressControl = RegisterControl(nameof(CustomsDetailsUserControl.OnlineTradeDistributorZAddressControl));
			OnlineTradeSellerZAddressControl = RegisterControl(nameof(CustomsDetailsUserControl.OnlineTradeSellerZAddressControl));
			OnlineTradeSellingAgentZOrganisationFindBox = RegisterControl(nameof(CustomsDetailsUserControl.OnlineTradeSellingAgentZOrganisationFindBox));
		}

		public ControlReference BillZGuidDropEdit { get; }
		public ControlReference CargoManagementNoTextBox { get; }
		public ControlReference COStatusDropEdit { get; }
		public ControlReference ValuationDeclarationStatusDropEdit { get; }
		public ControlReference BlanketValuationDeclarationNoTextBox { get; }
		public ControlReference CustomsBrokerCommentMultiTextBox { get; }
		public ControlReference SupplierZOrganisationFindBox { get; }
		public ControlReference ShipperZAddressControl { get; }
		public ControlReference EmptyLabel { get; }
		public ControlReference OnlineTradeTypeDropEdit { get; }
		public ControlReference OnlineTradeDistributorZAddressControl { get; }
		public ControlReference OnlineTradeSellerZAddressControl { get; }
		public ControlReference OnlineTradeSellingAgentZOrganisationFindBox { get; }
	}
}
