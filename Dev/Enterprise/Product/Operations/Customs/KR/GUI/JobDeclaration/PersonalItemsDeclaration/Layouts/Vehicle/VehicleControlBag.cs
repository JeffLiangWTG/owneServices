using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public class VehicleControlBag : ControlBag
	{
		public VehicleControlBag()
		{
			NameTextBox = RegisterControl(nameof(VehicleUserControl.NameTextBox));
			VehicleIDNumberTextBox = RegisterControl(nameof(VehicleUserControl.VehicleIDNumberTextBox));
			ExhaustVolumeCalcEdit = RegisterControl(nameof(VehicleUserControl.ExhaustVolumeCalcEdit));
			ModelYearTextBox = RegisterControl(nameof(VehicleUserControl.ModelYearTextBox));
			ManufacturingCountryCodeFindBox = RegisterControl(nameof(VehicleUserControl.ManufacturingCountryCodeFindBox));
			SeatCapacityCalcEdit = RegisterControl(nameof(VehicleUserControl.SeatCapacityCalcEdit));
			FirstRegistrationDateEdit = RegisterControl(nameof(VehicleUserControl.FirstRegistrationDateEdit));
			CurrentRegistrationDateEdit = RegisterControl(nameof(VehicleUserControl.CurrentRegistrationDateEdit));
		}

		public ControlReference NameTextBox { get; }
		public ControlReference VehicleIDNumberTextBox { get; }
		public ControlReference ExhaustVolumeCalcEdit { get; }
		public ControlReference ModelYearTextBox { get; }
		public ControlReference ManufacturingCountryCodeFindBox { get; }
		public ControlReference SeatCapacityCalcEdit { get; }
		public ControlReference FirstRegistrationDateEdit { get; }
		public ControlReference CurrentRegistrationDateEdit { get; }
		public static VehicleControlBag Instance => instance ??= new ();
		[ThreadStatic]
		static VehicleControlBag instance;

		protected override Control CreateTemplate() => new VehicleUserControl();
	}
}
