namespace Enterprise.Customs.AU.PRA.GUI.Testing
{
	partial class AUContainerMessagingFormForTest
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}

			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		protected override void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.anchorForTabControl = new CargoWise.Windows.UI.KPanel();
			this.zTabControl1 = new Enterprise.ZArchitecture.GUI.ZTemplateTabControl();
			this.zTabPage1 = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.button1 = new CargoWise.Windows.UI.KButton();
			this.zGrid1 = new Enterprise.ZArchitecture.ZGrid();
			this.bottomButtonPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.oPostingButtonsUserControl = new Enterprise.Core.Forms.ZPostingButtonsUserControl();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.anchorForTabControl.SuspendLayout();
			this.zTabControl1.SuspendLayout();
			this.zTabPage1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.zGrid1)).BeginInit();
			this.zGrid1.SuspendLayout();
			this.bottomButtonPanel.SuspendLayout();
			this.oPostingButtonsUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 575, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1016, 24, true);
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(500);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(501);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.Business.ContainerNonDependentCollection);
			// 
			// anchorForTabControl
			// 
			this.anchorForTabControl.Controls.Add(this.zTabControl1);
			this.anchorForTabControl.Controls.Add(this.zGrid1);
			this.anchorForTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.anchorForTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.anchorForTabControl.Name = "anchorForTabControl";
			this.anchorForTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1016, 599, true);
			this.anchorForTabControl.TabIndex = 1;
			// 
			// zTabControl1
			// 
			this.zTabControl1.Controls.Add(this.zTabPage1);
			this.zTabControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(24, 240, true);
			this.zTabControl1.Name = "zTabControl1";
			this.zTabControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(696, 277, true);
			this.zTabControl1.TabIndex = 1;
			// 
			// zTabPage1
			// 
			this.zTabPage1.Controls.Add(this.button1);
			this.zTabPage1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.zTabPage1.Name = "zTabPage1";
			this.zTabPage1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(688, 250, true);
			this.zTabPage1.TabIndex = 0;
			this.zTabPage1.Text = "zTabPage1";
			// 
			// button1
			// 
			this.button1.IsCaptionOverridden = true;
			this.button1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(224, 80, true);
			this.button1.Name = "button1";
			this.button1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.button1.TabIndex = 0;
			this.button1.Text = "button1";
			this.button1.ToolTipCaption = null;
			// 
			// zGrid1
			// 
			this.zGrid1.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.zGrid1, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Freight.Business.CommonContainer)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.CommonContainer)(null)).JC_ContainerNum)));
			this.zGrid1.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.Caption = "Container Num";
			zTextBoxColumnStyleInfo1.ColumnName = "JC_ContainerNum";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.zGrid1.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.zGrid1.GridId = "ac148771-8f40-48ed-9ef9-0461fba21880";
			this.zGrid1.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.zGrid1.LayoutKey = "zGrid1";
			this.zGrid1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(24, 16, true);
			this.zGrid1.Name = "zGrid1";
			this.zGrid1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(680, 184, true);
			this.zGrid1.TabIndex = 0;
			// 
			// bottomButtonPanel
			// 
			this.bottomButtonPanel.Controls.Add(this.oPostingButtonsUserControl);
			this.bottomButtonPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.bottomButtonPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 535, true);
			this.bottomButtonPanel.Name = "bottomButtonPanel";
			this.bottomButtonPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1016, 40, true);
			this.bottomButtonPanel.TabIndex = 8;
			// 
			// oPostingButtonsUserControl
			// 
			this.oPostingButtonsUserControl.AllowDrop = true;
			this.oPostingButtonsUserControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.oPostingButtonsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(699, 8, true);
			this.oPostingButtonsUserControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 25, true);
			this.oPostingButtonsUserControl.Name = "oPostingButtonsUserControl";
			this.oPostingButtonsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(302, 25, true);
			this.oPostingButtonsUserControl.TabIndex = 0;
			// 
			// AUContainerMessagingForm
			// 
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1016, 599, true);
			this.Controls.Add(this.bottomButtonPanel);
			this.Controls.Add(this.anchorForTabControl);
			this.DataSourceAssemblyName = "Enterprise.Freight";
			this.DataSourceType = typeof(Enterprise.Freight.Business.ContainerNonDependentCollection);
			this.DataSourceTypeName = "Enterprise.Freight.Business.ContainerNonDependentCollection";
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1024, 633, true);
			this.Name = "AUContainerMessagingForm";
			this.Text = "AU Container Messaging";
			this.Controls.SetChildIndex(this.anchorForTabControl, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.bottomButtonPanel, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.anchorForTabControl.ResumeLayout(false);
			this.anchorForTabControl.PerformLayout();
			this.zTabControl1.ResumeLayout(false);
			this.zTabControl1.PerformLayout();
			this.zTabPage1.ResumeLayout(false);
			this.zTabPage1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.zGrid1)).EndInit();
			this.zGrid1.ResumeLayout(false);
			this.zGrid1.PerformLayout();
			this.bottomButtonPanel.ResumeLayout(false);
			this.bottomButtonPanel.PerformLayout();
			this.oPostingButtonsUserControl.ResumeLayout(true);
			this.oPostingButtonsUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		#endregion

		private Enterprise.Freight.Business.ContainerNonDependentCollection fContainerCollection;
		private CargoWise.Windows.UI.KPanel anchorForTabControl;
		private Enterprise.ZArchitecture.GUI.ZPanel bottomButtonPanel;
		private Core.Forms.ZPostingButtonsUserControl oPostingButtonsUserControl;
		internal ZArchitecture.ZGrid zGrid1;
		private Enterprise.ZArchitecture.GUI.ZTemplateTabControl zTabControl1;
		private Enterprise.ZArchitecture.GUI.ZTabPage zTabPage1;
		private CargoWise.Windows.UI.KButton button1;
	}
}
