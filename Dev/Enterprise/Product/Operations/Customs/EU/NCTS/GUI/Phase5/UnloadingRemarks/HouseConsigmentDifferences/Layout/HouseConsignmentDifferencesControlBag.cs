using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public sealed class HouseConsignmentDifferencesControlBag : ControlBag
	{
		public HouseConsignmentDifferencesControlBag()
		{
			SequenceNumberTextBox = RegisterControl(nameof(HouseConsignmentDifferencesUserControl.SequenceNumberTextBox));
			SecurityCheckBox = RegisterControl(nameof(HouseConsignmentDifferencesUserControl.SecurityCheckBox));
			HouseConsignmentTextBox = RegisterControl(nameof(HouseConsignmentDifferencesUserControl.HouseConsignmentTextBox));
			UnloadedStateDropEdit = RegisterControl(nameof(HouseConsignmentDifferencesUserControl.UnloadedStateDropEdit));
			GrossWeightCalcDropEdit = RegisterControl(nameof(HouseConsignmentDifferencesUserControl.GrossWeightCalcDropEdit));
			GrossWeightUnloadedCalcDropEdit = RegisterControl(nameof(HouseConsignmentDifferencesUserControl.GrossWeightUnloadedCalcDropEdit));

			DeclaredValueLabel = RegisterControl(nameof(HouseConsignmentDifferencesUserControl.DeclaredValueLabel));
			UnloadedValueLabel = RegisterControl(nameof(HouseConsignmentDifferencesUserControl.UnloadedValueLabel));

			ConsignorDocAddressControl = RegisterControl(nameof(HouseConsignmentDifferencesUserControl.ConsignorDocAddressControl));
			ConsigneeDocAddressControl = RegisterControl(nameof(HouseConsignmentDifferencesUserControl.ConsigneeDocAddressControl));

			ArrivalTransportInfosUserControl = RegisterControl(nameof(HouseConsignmentDifferencesUserControl.ArrivalTransportInfosUserControl));
		}

		public static HouseConsignmentDifferencesControlBag Instance => instance ?? (instance = new HouseConsignmentDifferencesControlBag());

		[ThreadStatic]
		static HouseConsignmentDifferencesControlBag instance;

		protected override Control CreateTemplate() => new HouseConsignmentDifferencesUserControl();

		public ControlReference SequenceNumberTextBox { get; }

		public ControlReference SecurityCheckBox { get; }

		public ControlReference HouseConsignmentTextBox { get; }

		public ControlReference UnloadedStateDropEdit { get; }

		public ControlReference GrossWeightCalcDropEdit { get; }
		public ControlReference GrossWeightUnloadedCalcDropEdit;

		public ControlReference DeclaredValueLabel;
		public ControlReference UnloadedValueLabel;

		public ControlReference ConsignorDocAddressControl;
		public ControlReference ConsigneeDocAddressControl;

		public ControlReference ArrivalTransportInfosUserControl;
	}
}
