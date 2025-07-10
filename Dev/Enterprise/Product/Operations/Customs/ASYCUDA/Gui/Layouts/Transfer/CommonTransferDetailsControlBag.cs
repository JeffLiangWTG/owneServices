using System.Diagnostics.CodeAnalysis;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ASYCUDA.GUI
{
	public class CommonTransferDetailsControlBag : ControlBag
	{
		[SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule", Justification = "There is no way to change static backing field of this property.")]
		public static CommonTransferDetailsControlBag Instance { get; } = new CommonTransferDetailsControlBag();

		CommonTransferDetailsControlBag()
		{
			DestinationPortCodeFindBox = RegisterControl(nameof(CommonTransferDetailsUserControl.DestinationPortCodeFindBox));
			TransferTypeDropEdit = RegisterControl(nameof(CommonTransferDetailsUserControl.TransferTypeDropEdit));
			CarrierAddressControl = RegisterControl(nameof(CommonTransferDetailsUserControl.CarrierAddressControl));
			CarrierIDTextBox = RegisterControl(nameof(CommonTransferDetailsUserControl.CarrierIDTextBox));
			OnwardCarrierCodeFindBox = RegisterControl(nameof(CommonTransferDetailsUserControl.OnwardCarrierCodeFindBox));
			DestinationWarehouseAddressControl = RegisterControl(nameof(CommonTransferDetailsUserControl.DestinationWarehouseAddressControl));
			DestinationWarehouseIDTextBox = RegisterControl(nameof(CommonTransferDetailsUserControl.DestinationWarehouseIDTextBox));
		}

		protected override Control CreateTemplate() => new CommonTransferDetailsUserControl();

		public ControlReference DestinationPortCodeFindBox { get; }
		public ControlReference TransferTypeDropEdit { get; }
		public ControlReference CarrierAddressControl { get; }
		public ControlReference CarrierIDTextBox { get; }
		public ControlReference OnwardCarrierCodeFindBox { get; }
		public ControlReference DestinationWarehouseAddressControl { get; }
		public ControlReference DestinationWarehouseIDTextBox { get; }
	}
}
