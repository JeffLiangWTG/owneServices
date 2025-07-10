using System;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.SeaCargo.GUI
{
	public partial class SeaCargoCustomFieldsUserControl : ZUserControl
	{
		public SeaCargoCustomFieldsUserControl()
		{
			InitializeComponent();
			CustomFieldsControl.NothingSetupMessageLabelText = Declaration.GUI.Res.GetString("AABFEE0C-D853-4DE3-8CC9-2AF41FF4F502", "To make use of this tab, please setup Sea Cargo custom fields in Workflow Manager");
		}

		public new CusSCAOceanBill CurrentDataItem => (CusSCAOceanBill)base.CurrentDataItem;

		protected override void OnCurrentDataItemChanging(EventArgs e)
		{
			var oceanBill = CurrentDataItem;
			if (oceanBill != null)
			{
				oceanBill.OnResetCustomBusinessObject = null;
			}
			base.OnCurrentDataItemChanging(e);
		}

		void UpdateCustomFieldsControlBinding()
		{
			CustomFieldsControl.SetDataBinding(CurrentDataItem, string.Empty);
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);
			var oceanBill = CurrentDataItem;
			if (oceanBill != null)
			{
				oceanBill.OnResetCustomBusinessObject = UpdateCustomFieldsControlBinding;
			}
			UpdateCustomFieldsControlBinding();
		}
	}
}
