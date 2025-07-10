using Enterprise.Registry.GUI;

namespace Enterprise.Accounting.Registry.GUI
{
	partial class CreditCardFeeControl : RegistryZUserControl
	{
		public CreditCardFeeControl()
		{
			InitializeComponent();
		}

		#region Dispose

		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#endregion

		#region Overriden

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			CreditCardFeeGrid.ReadOnly = readOnly;
		}

		#endregion
	}
}

