using System;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.SeaCargo.GUI
{
	public partial class SeaCargoHouseBillCustomFieldsUserControl : ZUserControl
	{
		public SeaCargoHouseBillCustomFieldsUserControl()
		{
			InitializeComponent();
			CustomFieldsControl.NothingSetupMessageLabelText = Declaration.GUI.Res.GetString("E1B2340D-8BF4-4C19-A0E6-B1FB3FCFCD28", "To make use of this tab, please setup Sea Cargo House Bill custom fields in Workflow Manager");
		}

		public new CusSCAHouse CurrentDataItem => (CusSCAHouse)base.CurrentDataItem;

		protected override void OnCurrentDataItemChanging(EventArgs e)
		{
			var houseBill = CurrentDataItem;
			if (houseBill != null)
			{
				houseBill.OnResetCustomBusinessObject = null;
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
			var houseBill = CurrentDataItem;
			if (houseBill != null)
			{
				houseBill.OnResetCustomBusinessObject = UpdateCustomFieldsControlBinding;
			}
			UpdateCustomFieldsControlBinding();
		}
	}
}
