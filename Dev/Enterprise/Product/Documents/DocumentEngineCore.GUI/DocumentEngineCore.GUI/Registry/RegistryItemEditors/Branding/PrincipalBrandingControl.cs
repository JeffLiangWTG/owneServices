using System;
using CargoWise.EntityFramework;
using Enterprise.Registry.GUI;

namespace Enterprise.DocumentEngineCore.GUI.Registry
{
	public partial class PrincipalBrandingControl : RegistryZUserControl
	{
		public PrincipalBrandingControl()
		{
			InitializeComponent();
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);
			if (CurrentDataItem != null)
			{
				((IBusiness)CurrentDataItem).SetCountedReadOnlyIncludingChildren(ReadOnlySet);
			}
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			ReadOnlySet = readOnly;
			base.SetControlOrBusinessEntityReadOnly(readOnly);

			this.BrandingGrid.ReadOnly = readOnly;
			this.ImageBoundImageSelectionControl.ReadOnly = readOnly;

			IBusiness current = BindingSource.Current as IBusiness;
			if (current != null)
			{
				current.SetCountedReadOnlyIncludingChildren(readOnly);
			}
		}

		bool ReadOnlySet;
	}
}
