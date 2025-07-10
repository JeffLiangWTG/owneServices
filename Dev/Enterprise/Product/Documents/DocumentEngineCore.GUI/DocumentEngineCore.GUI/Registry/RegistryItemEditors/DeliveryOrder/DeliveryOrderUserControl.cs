using CargoWise.EntityFramework;
using Enterprise.Registry.GUI;

namespace Enterprise.DocumentEngineCore.GUI.Registry
{
	public partial class DeliveryOrderUserControl : RegistryZUserControl
	{
		public DeliveryOrderUserControl()
		{
			InitializeComponent();
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);
			if (BindingSource.Current != null)
			{
				((IBusiness)BindingSource.Current).SetCountedReadOnlyIncludingChildren(ReadOnlySet);
			}
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			ReadOnlySet = readOnly;
			base.SetControlOrBusinessEntityReadOnly(readOnly);

			this.PrincipalGrid.ReadOnly = readOnly;
			this.ImageBoundImageSelectionControl.ReadOnly = readOnly;

			if (BindingSource.Current != null)
			{
				((IBusiness)BindingSource.Current).SetCountedReadOnlyIncludingChildren(readOnly);
			}
		}

		bool ReadOnlySet;
	}
}
