using Enterprise.Registry.Business;

namespace Enterprise.Registry.GUI
{
	public abstract partial class ModuleButtonGridControl<T> : RegistryZUserControl
		where T : RegistryProxyBusinessObject, new()
	{
		public ModuleButtonGridControl()
		{
			InitializeComponent();
		}

		#region ReadOnly

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			ProxyCollectionGrid.SetButtonsReadOnly(readOnly);
		}

		#endregion

	}
}
