using System;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Registry.GUI;

namespace Enterprise.Accounting.Registry.GUI
{
	public partial class EnforceZeroBalanceDisbursementsConfigurationControl : RegistryZUserControl
	{
		public EnforceZeroBalanceDisbursementsConfigurationControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			validationType.ReadOnly = readOnly;
			maximumVariance.ReadOnly = readOnly;
		}

		EnforceZeroBalanceDisbursementsConfiguration BizObj => BoundBusinessObject as EnforceZeroBalanceDisbursementsConfiguration;

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);
			EnableDisableMaxVariance();
		}

		void SelectionChanged(object sender, EventArgs e)
		{
			EnableDisableMaxVariance();
		}

		void EnableDisableMaxVariance()
		{
			if (BizObj.IsMaximumVarianceApplicable)
			{
				maximumVarianceHelpText.Show();
				maximumVariance.Show();
			}
			else
			{
				maximumVarianceHelpText.Hide();
				maximumVariance.Hide();
			}
		}
	}
}
