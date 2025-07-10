using System;

namespace Enterprise.ComplianceRisk.GUI
{
	partial class ComplianceLogUserControl
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
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.complianceLogControl = new Enterprise.ZArchitecture.GUI.ZTemplateTabControl();
			this.complianceRiskStatusChangesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.removedCommoditiesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.complianceLogControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.ComplianceRisk.Integration.IComplianceItemRiskStatusProvider);
			// 
			// complianceLogControl
			// 
			this.complianceLogControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.complianceLogControl.Controls.Add(this.complianceRiskStatusChangesTabPage);
			this.complianceLogControl.Controls.Add(this.removedCommoditiesTabPage);
			this.complianceLogControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.complianceLogControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.complianceLogControl.Name = "complianceLogControl";
			this.complianceLogControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(896, 544, true);
			this.complianceLogControl.TabIndex = 2;
			// 
			// complianceRiskStatusChangesTabPage
			// 
			this.complianceRiskStatusChangesTabPage.CaptionResourceString = Enterprise.ComplianceRisk.GUI.Res.GetData("637c1f7a-8bc5-4ad1-8dfb-31cfb0456e7e", "Compliance Risk Status Changes");
			this.complianceRiskStatusChangesTabPage.ExcludeFromBindingOnSave = true;
			this.complianceRiskStatusChangesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 23, true);
			this.complianceRiskStatusChangesTabPage.Name = "complianceRiskStatusChangesTabPage";
			this.complianceRiskStatusChangesTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(5, true);
			this.complianceRiskStatusChangesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(891, 519, true);
			this.complianceRiskStatusChangesTabPage.TabIndex = 0;
			this.complianceRiskStatusChangesTabPage.Text = "Compliance Risk Status Changes";
			this.complianceRiskStatusChangesTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.ComplianceRiskStatusChangesTabPage_InitializeTab));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.ComplianceRisk.Business.ComplianceRiskStatusChangeLogCollection)(((Enterprise.ComplianceRisk.Integration.IComplianceItemRiskStatusProvider)(null)))));
			// 
			// removedCommoditiesTabPage
			// 
			this.removedCommoditiesTabPage.CaptionResourceString = Enterprise.ComplianceRisk.GUI.Res.GetData("46a2734f-3c60-4705-9c7f-0209eda92e5f", "Removed Commodities");
			this.removedCommoditiesTabPage.ExcludeFromBindingOnSave = true;
			this.removedCommoditiesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 23, true);
			this.removedCommoditiesTabPage.Name = "removedCommoditiesTabPage";
			this.removedCommoditiesTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(5, true);
			this.removedCommoditiesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(891, 519, true);
			this.removedCommoditiesTabPage.TabIndex = 1;
			this.removedCommoditiesTabPage.Text = "Removed Commodities";
			this.removedCommoditiesTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.RemovedCommoditiesTabPage_InitializeTab));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.ComplianceRisk.Business.RemovedCommoditiesLogCollection)(((Enterprise.ComplianceRisk.Integration.IComplianceItemRiskStatusProvider)(null)))));
			// 
			// ComplianceLogUserControl
			// 
			this.Controls.Add(this.complianceLogControl);
			this.Name = "ComplianceLogUserControl";
			this.ShouldSerializeTabPageMethods = true;
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(896, 544, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.complianceLogControl.ResumeLayout(false);
			this.complianceLogControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		private void ComplianceRiskStatusChangesTabPage_InitializeTab(object sender, EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.cpwLogViewer = new Enterprise.ComplianceRisk.GUI.ComplianceRiskLogViewerUserControl();
			this.complianceRiskStatusChangesTabPage.SuspendLayout();
			this.cpwLogViewer.SuspendLayout();
			this.complianceRiskStatusChangesTabPage.Controls.Add(this.cpwLogViewer);
			// 
			// cpwLogViewer
			// 
			this.cpwLogViewer.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.cpwLogViewer, ".");
			this.cpwLogViewer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.cpwLogViewer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 5, true);
			this.cpwLogViewer.Name = "cpwLogViewer";
			this.cpwLogViewer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(881, 509, true);
			this.cpwLogViewer.TabIndex = 0;
			this.complianceRiskStatusChangesTabPage.PerformLayout();
			this.cpwLogViewer.ResumeLayout(true);
			this.cpwLogViewer.PerformLayout();
			this.complianceRiskStatusChangesTabPage.ResumeLayout(true);

		}

		private void RemovedCommoditiesTabPage_InitializeTab(object sender, EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.removedCommoditiesLogViewer = new Enterprise.ComplianceRisk.GUI.RemovedCommoditiesLogViewerUserControl();
			this.removedCommoditiesTabPage.SuspendLayout();
			this.removedCommoditiesLogViewer.SuspendLayout();
			this.removedCommoditiesTabPage.Controls.Add(this.removedCommoditiesLogViewer);
			// 
			// removedCommoditiesLogViewer
			// 
			this.removedCommoditiesLogViewer.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.removedCommoditiesLogViewer, ".");
			this.removedCommoditiesLogViewer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.removedCommoditiesLogViewer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 5, true);
			this.removedCommoditiesLogViewer.Name = "removedCommoditiesLogViewer";
			this.removedCommoditiesLogViewer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(881, 509, true);
			this.removedCommoditiesLogViewer.TabIndex = 0;
			this.removedCommoditiesTabPage.PerformLayout();
			this.removedCommoditiesLogViewer.ResumeLayout(true);
			this.removedCommoditiesLogViewer.PerformLayout();
			this.removedCommoditiesTabPage.ResumeLayout(true);

		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZTemplateTabControl complianceLogControl;
		private Enterprise.ZArchitecture.GUI.ZTabPage complianceRiskStatusChangesTabPage;
		private Enterprise.ZArchitecture.GUI.ZTabPage removedCommoditiesTabPage;
		private ComplianceRiskLogViewerUserControl cpwLogViewer;
		private RemovedCommoditiesLogViewerUserControl removedCommoditiesLogViewer;
	}
}
