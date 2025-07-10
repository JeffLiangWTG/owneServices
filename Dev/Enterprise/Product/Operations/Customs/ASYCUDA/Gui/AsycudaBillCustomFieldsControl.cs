using System;
using Enterprise.ZArchitecture.GUI;
using Res = Enterprise.Customs.ASYCUDA.Gui.Res;

namespace Enterprise.Customs.ASYCUDA.GUI
{
	public partial class AsycudaBillCustomFieldsControl : ZUserControl
	{
		public AsycudaBillCustomFieldsControl()
		{
			InitializeComponent();
			CustomFieldsControl.NothingSetupMessageLabelText = Res.GetString("f6d6cf4a-1aea-47f4-bcd3-be641c54de82", "To make use of this tab, please setup Global Manifest Bill custom fields in Workflow Manager");
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);
			CustomFieldsControl.SetDataBinding(CurrentDataItem, string.Empty);
		}
	}
}
