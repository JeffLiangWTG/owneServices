using Enterprise.Registry.GUI;

namespace Enterprise.Accounting.Registry.GUI
{
	partial class ENettRegisteredBankAccountControl : RegistryZUserControl
	{
		public ENettRegisteredBankAccountControl()
		{
			InitializeComponent();
		}

		#region Overriden

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			ENettRegisteredBankAccountGrid.ReadOnly = readOnly;
		}

		#endregion
	}
}

