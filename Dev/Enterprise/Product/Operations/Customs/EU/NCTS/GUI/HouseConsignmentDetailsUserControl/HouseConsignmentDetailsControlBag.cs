using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public sealed class HouseConsignmentDetailsControlBag : ControlBag
	{
		public HouseConsignmentDetailsControlBag()
		{
			CountryOfDispatchDropEdit = RegisterControl(nameof(HouseConsignmentDetailsUserControl.CountryOfDispatchDropEdit));
			CountryOfDestinationDropEdit = RegisterControl(nameof(HouseConsignmentDetailsUserControl.CountryOfDestinationDropEdit));
			GrossWeightCalcDropEdit = RegisterControl(nameof(HouseConsignmentDetailsUserControl.GrossWeightCalcDropEdit));
			ReferenceNumberUCRTextBox = RegisterControl(nameof(HouseConsignmentDetailsUserControl.ReferenceNumberUCRTextBox));
			TransportMoPDropEdit = RegisterControl(nameof(HouseConsignmentDetailsUserControl.TransportMoPDropEdit));
			ConsignorDocAddressControl = RegisterControl(nameof(HouseConsignmentDetailsUserControl.ConsignorDocAddressControl));
			ConsigneeDocAddressControl = RegisterControl(nameof(HouseConsignmentDetailsUserControl.ConsigneeDocAddressControl));
			LinePriceCurrencyDropEdit = RegisterControl(nameof(HouseConsignmentDetailsUserControl.LinePriceCurrencyDropEdit));
		}

		public static HouseConsignmentDetailsControlBag Instance => instance ?? (instance = new HouseConsignmentDetailsControlBag());

		[ThreadStatic]
		static HouseConsignmentDetailsControlBag instance;

		public ControlReference CountryOfDispatchDropEdit { get; }

		public ControlReference CountryOfDestinationDropEdit { get; }

		public ControlReference GrossWeightCalcDropEdit { get; }

		public ControlReference ReferenceNumberUCRTextBox { get; }

		public ControlReference TransportMoPDropEdit { get; }

		public ControlReference ConsignorDocAddressControl { get; }

		public ControlReference ConsigneeDocAddressControl { get; }

		public ControlReference LinePriceCurrencyDropEdit { get; }

		protected override Control CreateTemplate() => new HouseConsignmentDetailsUserControl();
	}
}
