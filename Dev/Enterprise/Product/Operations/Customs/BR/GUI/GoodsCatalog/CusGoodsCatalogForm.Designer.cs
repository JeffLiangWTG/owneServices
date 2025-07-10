using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.BR.GUI
{
	partial class CusGoodsCatalogForm
    {
		new void InitializeComponent()
		{
			this.MessagesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.MessageTabUserControl = new Enterprise.Customs.BR.GUI.MessagesTabUserControl();
			this.MainTabControl.SuspendLayout();
			this.NotesTabPage.SuspendLayout();
			this.MainPanel.SuspendLayout();
			this.SaveButtonUserControl.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.MessagesTabPage.SuspendLayout();
			this.MessageTabUserControl.SuspendLayout();
			this.SuspendLayout();
			//
			// MainTabControl
			//
			this.MainTabControl.Controls.Add(this.MessagesTabPage);
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(848, 492, true);
			this.MainTabControl.Controls.SetChildIndex(this.LogsTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.NotesTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.MainTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.MessagesTabPage, 0);
			//
			// MainTabPage
			//
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(840, 548, true);
			//
			// NotesTabPage
			//
			this.NotesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(840, 548, true);
			//
			// LogsTabPage
			//
			this.LogsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(840, 548, true);
			//
			// MainPanel
			//
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(848, 492, true);
			//
			// SaveButtonUserControl
			//
			this.SaveButtonUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(546, 5, true);
			this.SaveButtonUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(302, 23, true);
			//
			// BindingSource
			//
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.BR.Business.CusGoodsCatalog);
			//
			// MessagesTabPage
			//
			this.MessagesTabPage.CaptionResourceString = Enterprise.Customs.BR.GUI.Res.GetData("09011e96-3123-4bca-bce2-9f48adf0a24b", "Messages");
			this.MessagesTabPage.Controls.Add(this.MessageTabUserControl);
			this.MessagesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.MessagesTabPage.Name = "MessagesTabPage";
			this.MessagesTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.MessagesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(840, 465, true);
			this.MessagesTabPage.TabIndex = 3;
			this.MessagesTabPage.UseVisualStyleBackColor = true;
			// 
			// MessageTabUserControl
			//
			this.MessageTabUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.MessageTabUserControl, "Messages");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Messaging.Business.EDIMessageCollection)(((Enterprise.Customs.BR.Business.CusGoodsCatalog)(null)).Messages)));
			this.MessageTabUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MessageTabUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 56, true);
			this.MessageTabUserControl.Name = "MessageTabUserControl";
			this.MessageTabUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(834, 459, true);
			this.MessageTabUserControl.TabIndex = 0;
			// 
			// CusGoodsCatalogForm
			//
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(848, 548, true);
			this.DataSourceType = typeof(Enterprise.Customs.BR.Business.CusGoodsCatalog);
			this.Name = "CusGoodsCatalogForm";
			this.MainTabControl.ResumeLayout(false);
			this.MainTabControl.PerformLayout();
			this.NotesTabPage.ResumeLayout(false);
			this.NotesTabPage.PerformLayout();
			this.MainPanel.ResumeLayout(false);
			this.MainPanel.PerformLayout();
			this.SaveButtonUserControl.ResumeLayout(true);
			this.SaveButtonUserControl.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.MessagesTabPage.ResumeLayout(false);
			this.MessagesTabPage.PerformLayout();
			this.MessageTabUserControl.ResumeLayout(true);
			this.MessageTabUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		internal ZTabPage MessagesTabPage;
		internal MessagesTabUserControl MessageTabUserControl;
	}
}
