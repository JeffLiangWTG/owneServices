using System;
using System.Windows.Forms;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public sealed class ShipmentDetailsControlBag : ControlBag
	{
		ShipmentDetailsControlBag()
		{
			WeightCalcDropEdit = RegisterControl(nameof(ShipmentDetailsUserControl.WeightCalcDropEdit));
			VolumeCalcDropEdit = RegisterControl(nameof(ShipmentDetailsUserControl.VolumeCalcDropEdit));
			OriginCodeFindBox = RegisterControl(nameof(ShipmentDetailsUserControl.OriginCodeFindBox));
			EstimatedDepartureDateEdit = RegisterControl(nameof(ShipmentDetailsUserControl.EstimatedDepartureDateEdit));
			FinalDestinationCodeFindBox = RegisterControl(nameof(ShipmentDetailsUserControl.FinalDestinationCodeFindBox));
			EstimatedArrivalDateEdit = RegisterControl(nameof(ShipmentDetailsUserControl.EstimatedArrivalDateEdit));
			ShipmentDetailsScreeningUserControl = RegisterControl(nameof(ShipmentDetailsUserControl.ShipmentDetailsScreeningUserControl));
		}
		public static ShipmentDetailsControlBag Instance => instance ?? (instance = new ShipmentDetailsControlBag());

		[ThreadStatic]
		static ShipmentDetailsControlBag instance;

		public ControlReference WeightCalcDropEdit;
		public ControlReference VolumeCalcDropEdit;
		public ControlReference OriginCodeFindBox;
		public ControlReference EstimatedDepartureDateEdit;
		public ControlReference FinalDestinationCodeFindBox;
		public ControlReference EstimatedArrivalDateEdit;
		public ControlReference ShipmentDetailsScreeningUserControl;
		protected override Control CreateTemplate() => new ShipmentDetailsUserControl();

		public static ResourceStringData OwnersReferenceCaption => Res.GetData("E0A6DF4A-46A0-4B0C-A6C2-E4A226348406", "Owners Reference");
	}
}
