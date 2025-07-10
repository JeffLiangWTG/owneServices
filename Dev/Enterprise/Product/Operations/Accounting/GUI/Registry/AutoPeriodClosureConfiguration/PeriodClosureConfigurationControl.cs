using Enterprise.Registry.GUI;

namespace Enterprise.Accounting.Registry.GUI
{
	public partial class PeriodClosureConfigurationControl : RegistryZUserControl
	{
		public PeriodClosureConfigurationControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);

			Enabled = !readOnly;
		}

		readonly System.ComponentModel.IContainer components;

		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}
	}
}

