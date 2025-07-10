using System;
using CargoWise.EntityFramework;
using Enterprise.Registry.GUI;

namespace Enterprise.DocumentEngineCore.GUI.Registry
{
	public partial class LocalTransportCompanyBrandingControl : RegistryZUserControl
	{
		public LocalTransportCompanyBrandingControl()
		{
			InitializeComponent();
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);
			if (CurrentDataItem != null)
			{
				((IBusiness)CurrentDataItem).SetCountedReadOnlyIncludingChildren(readOnlySet);
			}
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			readOnlySet = readOnly;
			base.SetControlOrBusinessEntityReadOnly(readOnly);

			BrandingGrid.ReadOnly = readOnly;
			ImageSelectionControl.ReadOnly = readOnly;

			var current = BindingSource.Current as IBusiness;
			if (current != null)
			{
				current.SetCountedReadOnlyIncludingChildren(readOnly);
			}
		}

		bool readOnlySet;
	}
}
