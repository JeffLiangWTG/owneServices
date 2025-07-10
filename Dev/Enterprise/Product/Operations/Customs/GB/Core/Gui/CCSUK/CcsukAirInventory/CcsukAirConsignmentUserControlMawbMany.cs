using System;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GB.GUI.Ccsuk
{
	public partial class CcsukAirConsignmentUserControlMawbMany : ZUserControl
	{
		ConsolToManyMawbsPluginHelper bindingSourceHelper;

		public CcsukAirConsignmentUserControlMawbMany()
		{
			InitializeComponent();
			mawbsGrid.AfterBind += MawbsGrid_AfterBind;
		}

		void UpdateLayout()
		{
			zPanel2.Visible = bindingSourceHelper.Mawbs.Count != 1;
		}

		void MawbsChangedHandler(object sender, EventArgs args) => UpdateLayout();

		void MawbsGrid_AfterBind(object sender, EventArgs e)
		{
			var listManager = mawbsGrid.ListManager;
			if (listManager != null)
			{
				listManager.CurrentChanged += ListManager_CurrentChanged;
				listManager.CurrentItemChanged += ListManager_CurrentChanged;
				ListManager_CurrentChanged(null, null);
			}

			if (bindingSourceHelper != null)
			{ bindingSourceHelper.MawbsChanged -= MawbsChangedHandler; }

			bindingSourceHelper = BindingSource?.DataSource as ConsolToManyMawbsPluginHelper;

			if (bindingSourceHelper != null)
			{
				bindingSourceHelper.MawbsChanged += MawbsChangedHandler;
				UpdateLayout();
			}
		}

		void ListManager_CurrentChanged(object sender, EventArgs e)
		{
			var listManager = mawbsGrid.ListManager;
			var cusMawb = (CusMAWB)listManager.GetCurrent();
			var userControlHelper = ccsukAirConsignmentUserControlMawb1.ccsukMainMasterUserControlHelpers1;
			if (cusMawb != null)
			{
				userControlHelper.SetCusMawb(cusMawb);
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (bindingSourceHelper != null)
				{
					bindingSourceHelper.MawbsChanged -= MawbsChangedHandler;
				}

				var listManager = mawbsGrid.ListManager;
				if (listManager != null)
				{
					listManager.CurrentChanged -= ListManager_CurrentChanged;
					listManager.CurrentItemChanged -= ListManager_CurrentChanged;
				}
			}
			base.Dispose(disposing);
		}
	}
}
