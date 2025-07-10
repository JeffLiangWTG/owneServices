using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public sealed class ExtendedHoursRequestNewControlBag : ControlBag
	{
		ExtendedHoursRequestNewControlBag()
		{
			MessageTypeDropEdit = RegisterControl(nameof(ExtendedHoursRequestNewControl.MessageTypeDropEdit));
			CustomsOfficeCodeFindBox = RegisterControl(nameof(ExtendedHoursRequestNewControl.CustomsOfficeCodeFindBox));
			CustomsDivisionCodeFindBox = RegisterControl(nameof(ExtendedHoursRequestNewControl.CustomsDivisionCodeFindBox));
			RequestPeriodStartDateEdit = RegisterControl(nameof(ExtendedHoursRequestNewControl.RequestPeriodStartDateEdit));
			RequestPeriodEndDateEdit = RegisterControl(nameof(ExtendedHoursRequestNewControl.RequestPeriodEndDateEdit));
			BranchGuidFindBox = RegisterControl(nameof(ExtendedHoursRequestNewControl.BranchGuidFindBox));

			RequestReasonTextBox = RegisterControl(nameof(ExtendedHoursRequestNewControl.RequestReasonTextBox));
		}

		public static ExtendedHoursRequestNewControlBag Instance => instance ?? (instance = new ExtendedHoursRequestNewControlBag());

		[ThreadStatic]
		static ExtendedHoursRequestNewControlBag instance;

		protected override Control CreateTemplate() => new ExtendedHoursRequestNewControl();

		public ControlReference MessageTypeDropEdit { get; }
		public ControlReference CustomsOfficeCodeFindBox { get; }
		public ControlReference CustomsDivisionCodeFindBox { get; }
		public ControlReference RequestPeriodStartDateEdit { get; }
		public ControlReference RequestPeriodEndDateEdit { get; }
		public ControlReference BranchGuidFindBox { get; }
		public ControlReference RequestReasonTextBox { get; }
	}
}
