using System;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public partial class UnloadingRemarksUserControl : ZUserControl
	{
		public UnloadingRemarksUserControl()
		{
			InitializeComponent();
			InitializeTabsLazyCreate();
		}

		void InitializeTabsLazyCreate()
		{
			HeaderDifferencesTabPage.RunWhenBindingOrFirstShown((s, args) => UnloadingHeaderDifferencesDynamicUserControl.UserControlType = UnloadingHeaderDifferencesUserControlType());
			GoodsItemDifferencesTabPage.RunWhenBindingOrFirstShown((s, args) => UnloadingItemDifferencesDynamicUserControl.UserControlType = UnloadingItemDifferencesDynamicUserControlType());
		}

		protected virtual Type UnloadingItemDifferencesDynamicUserControlType()
		{
			return typeof(UnloadingItemDifferencesTabUserControl);
		}

		protected virtual Type UnloadingHeaderDifferencesUserControlType()
		{
			return typeof(UnloadingHeaderDifferencesTabUserControl);
		}

		void HeaderDifferencesTabPage_InitializeTab(object sender, EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.UnloadingHeaderDifferencesDynamicUserControl = new ZDynamicControlCreationUserControl();
			this.HeaderDifferencesTabPage.SuspendLayout();
			this.UnloadingHeaderDifferencesDynamicUserControl.SuspendLayout();
			this.HeaderDifferencesTabPage.Controls.Add(this.UnloadingHeaderDifferencesDynamicUserControl);
			// 
			// UnloadingRemarksDynamicUserControl
			// 
			this.UnloadingHeaderDifferencesDynamicUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.UnloadingHeaderDifferencesDynamicUserControl, ".");
			this.UnloadingHeaderDifferencesDynamicUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.UnloadingHeaderDifferencesDynamicUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.UnloadingHeaderDifferencesDynamicUserControl.Name = "UnloadingHeaderDifferencesDynamicUserControl";
			this.UnloadingHeaderDifferencesDynamicUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1117, 723, true);
			this.UnloadingHeaderDifferencesDynamicUserControl.TabIndex = 0;
			this.UnloadingHeaderDifferencesDynamicUserControl.UserControlType = UnloadingHeaderDifferencesUserControlType();
			this.HeaderDifferencesTabPage.PerformLayout();
			this.UnloadingHeaderDifferencesDynamicUserControl.ResumeLayout(true);
			this.UnloadingHeaderDifferencesDynamicUserControl.PerformLayout();
			this.HeaderDifferencesTabPage.ResumeLayout(true);
		}

		void ItemDifferencesTabPage_InitializeTab(object sender, EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.UnloadingItemDifferencesDynamicUserControl = new ZDynamicControlCreationUserControl();
			this.GoodsItemDifferencesTabPage.SuspendLayout();
			this.UnloadingItemDifferencesDynamicUserControl.SuspendLayout();
			this.GoodsItemDifferencesTabPage.Controls.Add(this.UnloadingItemDifferencesDynamicUserControl);
			// 
			// UnloadingRemarksDynamicUserControl
			// 
			this.UnloadingItemDifferencesDynamicUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.UnloadingItemDifferencesDynamicUserControl, ".");
			this.UnloadingItemDifferencesDynamicUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.UnloadingItemDifferencesDynamicUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.UnloadingItemDifferencesDynamicUserControl.Name = "UnloadingItemDifferencesDynamicUserControl";
			this.UnloadingItemDifferencesDynamicUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1123, 729, true);
			this.UnloadingItemDifferencesDynamicUserControl.TabIndex = 0;
			this.UnloadingItemDifferencesDynamicUserControl.UserControlType = UnloadingHeaderDifferencesUserControlType();
			this.GoodsItemDifferencesTabPage.PerformLayout();
			this.UnloadingItemDifferencesDynamicUserControl.ResumeLayout(true);
			this.UnloadingItemDifferencesDynamicUserControl.PerformLayout();
			this.GoodsItemDifferencesTabPage.ResumeLayout(true);
		}
	}
}
