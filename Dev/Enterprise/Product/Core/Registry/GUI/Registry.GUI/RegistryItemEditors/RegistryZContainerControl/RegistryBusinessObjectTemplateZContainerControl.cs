using System;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Registry.GUI
{
	[SuppressFormsLocalizedTest]
	public partial class RegistryBusinessObjectTemplateZUserControl : RegistryZUserControl
	{
		public new RegistryBusinessObjectTemplate CurrentDataItem
		{
			get { return (RegistryBusinessObjectTemplate)base.CurrentDataItem; }
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);
			SetControlOrBusinessEntityReadOnly(ReadOnly);
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			var businessObject = CurrentDataItem as BusinessObject;
			if (businessObject != null)
			{
				businessObject.SetReadOnlyIncludingChildren(readOnly);
			}
		}
	}
}
