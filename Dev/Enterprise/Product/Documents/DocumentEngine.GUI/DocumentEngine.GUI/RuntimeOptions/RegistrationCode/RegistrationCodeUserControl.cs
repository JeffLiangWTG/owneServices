using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.DocumentEngine.GUI.RuntimeOptions
{
	partial class RegistrationCodeUserControl : RuntimeOptionUserControl
	{
		public RegistrationCodeUserControl()
		{
			InitializeComponent();
		}

		protected override int DesiredCaptionWidthCore => Controls.Cast<Control>().Max(GetCaptionWidth);

		protected override void ChangeLabelSizeForAlignmentCore(int descriptionSize)
		{
			foreach (var control in Controls.Cast<Control>())
			{
				ControlDpiScalingHelper.SetLeft(control, descriptionSize, false);
				ControlDpiScalingHelper.SetWidth(control, Width - descriptionSize, false);
			}
		}

		public override Type ExpectedFilterType()
		{
			return typeof(RegistrationCodeField);
		}

		public override void SetFilter(FilterField filter)
		{
			base.SetFilter(filter);
			var field = (RegistrationCodeField)filter;
			CountryFindBox.ModuleID = ModuleIDs.RefCountry;
			CountryFindBox.SetDataBinding(filter, "CodeCountry");
			CustomTypeDropEdit.SetDataBinding(filter, "CustomType");
			SetDataBinding(filter, "");
		}
	}
}
