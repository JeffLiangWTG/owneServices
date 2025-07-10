using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI
{
	public sealed class ShipmentDetailsControlBag : ControlBag
	{
		protected override Control CreateTemplate() => new ShipmentDetailsUserControl();

		public static ShipmentDetailsControlBag Instance => instance ?? (instance = new ShipmentDetailsControlBag());

		[ThreadStatic]
		static ShipmentDetailsControlBag instance;

		ShipmentDetailsControlBag()
		{
			ShipmentDetailsQuantitiesUserControl = RegisterControl(nameof(ShipmentDetailsUserControl.ShipmentDetailsQuantitiesUserControl));
			ShipmentDetailsCountUserControl = RegisterControl(nameof(ShipmentDetailsUserControl.ShipmentDetailsCountUserControl));
			ShipmentDetailsIncoTermsPlaceUserControl = RegisterControl(nameof(ShipmentDetailsUserControl.ShipmentDetailsIncoTermsPlaceUserControl));
			ShipmentDetailsUnlocoIncoTermsPlaceUserControl = RegisterControl(nameof(ShipmentDetailsUserControl.ShipmentDetailsUnlocoIncoTermsPlaceUserControl));
			GoodsLocationDropEdit = RegisterControl(nameof(ShipmentDetailsUserControl.GoodsLocationDropEdit));
			AgentsReferenceTextBox = RegisterControl(nameof(ShipmentDetailsUserControl.AgentsReferenceTextBox));
			UCRTextBox = RegisterControl(nameof(ShipmentDetailsUserControl.UCRTextBox));
			ShipmentIncoTermPlaceTextBox = RegisterControl(nameof(ShipmentDetailsUserControl.ShipmentIncoTermPlaceTextBox));
			AgreedPlaceCodeFindBox = RegisterControl(nameof(ShipmentDetailsUserControl.AgreedPlaceCodeFindBox));
			RegionOfDestinationDropEdit = RegisterControl(nameof(ShipmentDetailsUserControl.RegionOfDestinationDropEdit));
		}

		public ControlReference ShipmentDetailsCountUserControl;
		public ControlReference ShipmentDetailsQuantitiesUserControl;
		public ControlReference ShipmentDetailsIncoTermsPlaceUserControl;
		public ControlReference ShipmentDetailsUnlocoIncoTermsPlaceUserControl;
		public ControlReference GoodsLocationDropEdit;
		public ControlReference AgentsReferenceTextBox;
		public ControlReference UCRTextBox;
		public ControlReference ShipmentIncoTermPlaceTextBox;
		public ControlReference AgreedPlaceCodeFindBox;
		public ControlReference RegionOfDestinationDropEdit;
	}
}
