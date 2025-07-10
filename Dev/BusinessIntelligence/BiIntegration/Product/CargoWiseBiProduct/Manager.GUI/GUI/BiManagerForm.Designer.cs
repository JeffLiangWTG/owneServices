using CargoWise.Common;
using System;

namespace CargoWise.Bi.Product.Manager.GUI
{
	partial class BiManagerForm
	{
		protected override void Dispose(bool disposing)
		{
			try
			{
				infoControl?.Dispose();
				cdcControl?.Dispose();
				auditControl?.Dispose();
				edwControl?.Dispose();
				ssasControl?.Dispose();

				if (disposing && (components != null))
				{
					components.Dispose();
				}

				base.Dispose(disposing);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{ }
		}

		#region Windows Form Designer generated code

		private new void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.retrievingTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.zTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.infoTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.cdcTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.auditTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.edwTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.ssasTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.retrievingTextBox.SuspendLayout();
			this.zTabControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(CargoWise.Bi.Product.Manager.Business.BiMonitorBusinessObject);
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 714, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(729, 24, true);
			// 
			// zTabControl
			// 
			this.zTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.zTabControl.Controls.Add(this.infoTabPage);
			this.zTabControl.Controls.Add(this.cdcTabPage);
			this.zTabControl.Controls.Add(this.auditTabPage);
			this.zTabControl.Controls.Add(this.edwTabPage);
			this.zTabControl.Controls.Add(this.ssasTabPage);
			this.zTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.zTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.zTabControl.Name = "zTabControl";
			this.zTabControl.SelectedIndex = 0;
			this.zTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(729, 765, true);
			this.zTabControl.TabIndex = 1;
			// 
			// infoTabPage
			// 
			this.infoTabPage.CaptionResourceString = CargoWise.Bi.Product.Manager.GUI.Res.GetData("79f59fff-bea3-46e2-a784-e678d2ccf5a0", "Information");
			this.infoTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.infoTabPage.Name = "infoTabPage";
			this.infoTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.infoTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(721, 765, true);
			this.infoTabPage.TabIndex = 0;
			this.infoTabPage.UseVisualStyleBackColor = true;
			this.infoTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.infoTabPage_InitializeTab));
			// 
			// cdcTabPage
			// 
			this.cdcTabPage.CaptionResourceString = CargoWise.Bi.Product.Manager.GUI.Res.GetData("80fb73ad-3c55-4bd9-8115-66cb887530e1", "CDC");
			this.cdcTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.cdcTabPage.Name = "cdcTabPage";
			this.cdcTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.cdcTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(721, 765, true);
			this.cdcTabPage.TabIndex = 1;
			this.cdcTabPage.TabVisible = false;
			this.cdcTabPage.UseVisualStyleBackColor = true;
			this.cdcTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.cdcTabPage_InitializeTab));
			// 
			// auditTabPage
			// 
			this.auditTabPage.CaptionResourceString = CargoWise.Bi.Product.Manager.GUI.Res.GetData("80794490-64f6-4eee-8c30-5a386c449e0e", "Audit");
			this.auditTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.auditTabPage.Name = "auditTabPage";
			this.auditTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.auditTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(721, 765, true);
			this.auditTabPage.TabIndex = 2;
			this.auditTabPage.TabVisible = false;
			this.auditTabPage.UseVisualStyleBackColor = true;
			this.auditTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.auditTabPage_InitializeTab));
			// 
			// edwTabPage
			// 
			this.edwTabPage.CaptionResourceString = CargoWise.Bi.Product.Manager.GUI.Res.GetData("ce321c2d-7eea-4ea8-b50f-9efd3d703757", "EDW");
			this.edwTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.edwTabPage.Name = "edwTabPage";
			this.edwTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.edwTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(721, 765, true);
			this.edwTabPage.TabIndex = 3;
			this.edwTabPage.TabVisible = false;
			this.edwTabPage.UseVisualStyleBackColor = true;
			this.edwTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.edwTabPage_InitializeTab));
			// 
			// ssasTabPage
			// 
			this.ssasTabPage.CaptionResourceString = CargoWise.Bi.Product.Manager.GUI.Res.GetData("60770d1f-d1e8-40b7-94e5-1fa0564b6003", "Analysis Server");
			this.ssasTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.ssasTabPage.Name = "ssasTabPage";
			this.ssasTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.ssasTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(721, 765, true);
			this.ssasTabPage.TabIndex = 4;
			this.ssasTabPage.TabVisible = false;
			this.ssasTabPage.UseVisualStyleBackColor = true;
			this.ssasTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.ssasTabPage_InitializeTab));
			// 
			// retrievingTextBox
			// 
			this.retrievingTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.retrievingTextBox.CaptionResourceString = null;
			this.retrievingTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.retrievingTextBox.Enabled = false;
			this.retrievingTextBox.VisibleChanged += new System.EventHandler(this.CenterTextBox);
			this.retrievingTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(90, 91, true);
			this.retrievingTextBox.Name = "retrievingTextBox";
			this.retrievingTextBox.ReadOnly = true;
			this.retrievingTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(115, 18, true);
			// 
			// BiManagerForm
			// 
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = CargoWise.Bi.Product.Manager.GUI.Res.GetData("e9249e5f-d578-457e-93a7-3a9a164a3aee", "Business Intelligence Manager");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(729, 870, true);
			this.Controls.Add(this.retrievingTextBox);
			this.Controls.Add(this.zTabControl);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(720, 725, true);
			this.Name = "BiManagerForm";
			this.ShouldSerializeTabPageMethods = true;
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.zTabControl, 0);
			this.Controls.SetChildIndex(this.retrievingTextBox, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.retrievingTextBox.ResumeLayout(false);
			this.retrievingTextBox.PerformLayout();
			this.zTabControl.ResumeLayout(false);
			this.zTabControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Enterprise.ZArchitecture.ZTextBox retrievingTextBox;
		private Enterprise.ZArchitecture.GUI.ZTabControl zTabControl;
		private Enterprise.ZArchitecture.GUI.ZTabPage infoTabPage;
		private Enterprise.ZArchitecture.GUI.ZTabPage cdcTabPage;
		private Enterprise.ZArchitecture.GUI.ZTabPage auditTabPage;
		private Enterprise.ZArchitecture.GUI.ZTabPage edwTabPage;
		private Enterprise.ZArchitecture.GUI.ZTabPage ssasTabPage;
		private CargoWise.Bi.Product.Manager.GUI.InformationControl infoControl;
		private CargoWise.Bi.Product.Manager.GUI.CdcControl cdcControl;
		private CargoWise.Bi.Product.Manager.GUI.AuditControl auditControl;
		private CargoWise.Bi.Product.Manager.GUI.EdwControl edwControl;
		private CargoWise.Bi.Product.Manager.GUI.AnalysisServerControl ssasControl;
		private System.ComponentModel.IContainer components;
	}
}
