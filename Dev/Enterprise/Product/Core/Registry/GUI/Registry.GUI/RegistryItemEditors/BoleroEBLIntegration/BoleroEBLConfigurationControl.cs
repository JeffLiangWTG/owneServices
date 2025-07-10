using System;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Registry.GUI
{
	[SuppressBindingMemberBashingTestAttribute]
	public sealed partial class BoleroEBLConfigurationControl
	{
		public BoleroEBLConfigurationControl()
		{
			InitializeComponent();
		}

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);

			if (DataSource is BoleroEBLConfiguration configuration)
			{
				YesRadioButton.Checked = configuration.EnableEBLIntegration;
				NoRadioButton.Checked = !configuration.EnableEBLIntegration;
			}
		}
	}
}
