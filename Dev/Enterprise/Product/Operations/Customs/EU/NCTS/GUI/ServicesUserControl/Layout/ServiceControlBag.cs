using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public sealed class ServiceControlBag : ControlBag
	{
		public ServiceControlBag()
		{
			MeasurementBasisDropEdit = RegisterControl(nameof(ServiceUserControl.MeasurementBasisDropEdit));
			ContractorFindBox = RegisterControl(nameof(ServiceUserControl.ContractorCodeFindBox));
			NotesTextBox = RegisterControl(nameof(ServiceUserControl.NotesTextBox));
			ReferenceTextBox = RegisterControl(nameof(ServiceUserControl.ReferenceTextBox));
			DurationTimeEdit = RegisterControl(nameof(ServiceUserControl.DurationTimeEdit));
			ServiceCountCalcEdit = RegisterControl(nameof(ServiceUserControl.ServiceCountCalcEdit));
			SubLocationTextBox = RegisterControl(nameof(ServiceUserControl.SubLocationTextBox));
			ServiceLocationAddressControl = RegisterControl(nameof(ServiceUserControl.ServiceLocationAddressControl));
			CompletedDateEdit = RegisterControl(nameof(ServiceUserControl.CompletedDateEdit));
			BookedDateEdit = RegisterControl(nameof(ServiceUserControl.BookedDateEdit));
			ServiceTypeDropEdit = RegisterControl(nameof(ServiceUserControl.ServiceTypeDropEdit));
			RateAndCurrencyCalcFindBox = RegisterControl(nameof(ServiceUserControl.RateAndCurrencyCalcFindBox));
		}

		public ControlReference MeasurementBasisDropEdit { get; }
		public ControlReference ContractorFindBox { get; }
		public ControlReference NotesTextBox { get; }
		public ControlReference ReferenceTextBox { get; }
		public ControlReference DurationTimeEdit { get; }
		public ControlReference ServiceCountCalcEdit { get; }
		public ControlReference SubLocationTextBox { get; }
		public ControlReference ServiceLocationAddressControl { get; }
		public ControlReference CompletedDateEdit { get; }
		public ControlReference BookedDateEdit { get; }
		public ControlReference ServiceTypeDropEdit { get; }
		public ControlReference RateAndCurrencyCalcFindBox { get; }

		public static ServiceControlBag Instance => instance ?? (instance = new ServiceControlBag());

		protected override Control CreateTemplate() => new ServiceUserControl();

		[ThreadStatic]
		static ServiceControlBag instance;
	}
}
