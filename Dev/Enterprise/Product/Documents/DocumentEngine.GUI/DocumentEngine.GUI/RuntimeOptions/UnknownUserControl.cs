using System;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.GUI.RuntimeOptions
{
	internal partial class UnknownUserControl : RuntimeOptionUserControl
	{
		public UnknownUserControl()
		{
			// This call is required by the Windows Form Designer.
			InitializeComponent();
		}
		protected override int DesiredCaptionWidthCore => 0; //Unnecessary method for such Control

		protected override void ChangeLabelSizeForAlignmentCore(int descriptionSize)
		{
			//Not applicable to this control
		}

		public override void SetFilter(FilterField filter)
		{
			base.SetFilter(filter);
			ErrorLabel.Text = EscapeMnemonics(filter.DisplayName) + (NoResString)" - Error: filters of type " + filter.GetType().Name + (NoResString)" cannot be displayed";
		}

		public override Type ExpectedFilterType()
		{
			return typeof(FilterField);
		}
	}
}
