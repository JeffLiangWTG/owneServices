using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public sealed class UnloadingDetailsControlBag : ControlBag
	{
		public UnloadingDetailsControlBag()
		{
			UnloadingDateDateEdit = RegisterControl(nameof(UnloadingDetailsUserControl.UnloadingDateDateEdit));
			UnloadingConformCheckBox = RegisterControl(nameof(UnloadingDetailsUserControl.UnloadingConformCheckBox));
			StateOfSealsCheckBox = RegisterControl(nameof(UnloadingDetailsUserControl.StateOfSealsCheckBox));
			UnloadingCompletedCheckBox = RegisterControl(nameof(UnloadingDetailsUserControl.UnloadingCompletedCheckBox));
			UnloadingRemarksTextBox = RegisterControl(nameof(UnloadingDetailsUserControl.UnloadingRemarksTextBox));
			OtherThingsToReportTextBox = RegisterControl(nameof(UnloadingDetailsUserControl.OtherThingsToReportTextBox));
		}

		public static UnloadingDetailsControlBag Instance => instance ?? (instance = new UnloadingDetailsControlBag());

		[ThreadStatic]
		static UnloadingDetailsControlBag instance;

		public ControlReference UnloadingDateDateEdit { get; }

		public ControlReference UnloadingConformCheckBox { get; }

		public ControlReference StateOfSealsCheckBox { get; }

		public ControlReference UnloadingCompletedCheckBox { get; }

		public ControlReference UnloadingRemarksTextBox { get; }

		public ControlReference OtherThingsToReportTextBox { get; }

		protected override Control CreateTemplate() => new UnloadingDetailsUserControl();
	}
}
