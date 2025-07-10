using System;
using Enterprise.DocumentEngine.RuntimeOptions;

namespace Enterprise.DocumentEngine.GUI.RuntimeOptions
{
	partial class SecurityFilterControl : RuntimeOptionUserControl
	{
		public SecurityFilterControl()
		{
			InitializeComponent();
		}

		public override Type ExpectedFilterType()
		{
			return typeof(SecurityFilterField);
		}

		public override void SetFilter(FilterField filter)
		{
			base.SetFilter(filter);
			SetDataBinding(filter, "");
		}
	}
}
