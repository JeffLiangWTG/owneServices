using System;
using CargoWise.Windows.UI;
using Enterprise.DocumentEngine.RuntimeOptions;

namespace Enterprise.DocumentEngine.GUI.RuntimeOptions
{
	partial class PermitTypeChecklistUserControl : RuntimeOptionUserControl
	{
		public PermitTypeChecklistUserControl()
		{
			InitializeComponent();
		}

		protected override int DesiredCaptionWidthCore => GetCaptionWidth(fieldLabel);

		protected override void ChangeLabelSizeForAlignmentCore(int descriptionSize)
		{
			ControlDpiScalingHelper.SetWidth(fieldLabelPanel, descriptionSize, false);
			ControlDpiScalingHelper.SetWidth(fieldLabel, descriptionSize, false);
			ControlDpiScalingHelper.SetLeft(splitContainer1, descriptionSize, false);
			ControlDpiScalingHelper.SetWidth(splitContainer1, Width - descriptionSize, false);
		}

		public override Type ExpectedFilterType()
		{
			return typeof(PermitTypeChecklistField);
		}

		public override void SetFilter(FilterField filter)
		{
			base.SetFilter(filter);
			SetDataBinding(filter, "");
		}
	}
}
