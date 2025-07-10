namespace Enterprise.BufferManagement.GUI
{
	partial class WorkflowProvidersLinkageUserControl
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
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo zGuidDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo zGuidDropEditColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			this.LinksPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.LinksGrid = new Enterprise.ZArchitecture.ZGrid();
			this.HintPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.HintLabel = new Enterprise.ZArchitecture.ZLabel();
			this.CreateLinkButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CancelLinkButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.LinksPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.LinksGrid)).BeginInit();
			this.LinksGrid.SuspendLayout();
			this.HintPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.BufferManagement.Business.WorkflowProvidersLinkViewModel);
			// 
			// LinksPanel
			// 
			this.LinksPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.LinksPanel.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.LinksPanel.Controls.Add(this.LinksGrid);
			this.LinksPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 32, true);
			this.LinksPanel.Name = "LinksPanel";
			this.LinksPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(928, 242, true);
			this.LinksPanel.TabIndex = 20;
			// 
			// LinksGrid
			// 
			this.LinksGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.LinksGrid, "ProposedProcessHeaderLinks");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.BufferManagement.Business.WorkflowProvidersLinkViewModel)(null)).ProposedProcessHeaderLinks)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.BufferManagement.Business.ProposedProcessHeaderLink)(((System.Collections.IList)(((Enterprise.BufferManagement.Business.WorkflowProvidersLinkViewModel)(null)).ProposedProcessHeaderLinks)).SyncRoot)).FP_LinkType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.BufferManagement.Business.ProposedProcessHeaderLink)(((System.Collections.IList)(((Enterprise.BufferManagement.Business.WorkflowProvidersLinkViewModel)(null)).ProposedProcessHeaderLinks)).SyncRoot)).FP_FH_HeaderFrom)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.BufferManagement.Business.ProposedProcessHeaderLink)(((System.Collections.IList)(((Enterprise.BufferManagement.Business.WorkflowProvidersLinkViewModel)(null)).ProposedProcessHeaderLinks)).SyncRoot)).FP_FH_HeaderTo)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.BufferManagement.Business.ProposedProcessHeaderLink)(((System.Collections.IList)(((Enterprise.BufferManagement.Business.WorkflowProvidersLinkViewModel)(null)).ProposedProcessHeaderLinks)).SyncRoot)).ActionDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.BufferManagement.Business.ProposedProcessHeaderLink)(((System.Collections.IList)(((Enterprise.BufferManagement.Business.WorkflowProvidersLinkViewModel)(null)).ProposedProcessHeaderLinks)).SyncRoot)).FP_SynchroniseBufferPenetration)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.BufferManagement.Business.ProposedProcessHeaderLink)(((System.Collections.IList)(((Enterprise.BufferManagement.Business.WorkflowProvidersLinkViewModel)(null)).ProposedProcessHeaderLinks)).SyncRoot)).FP_TimeDelayFactor)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.BufferManagement.Business.ProposedProcessHeaderLink)(((System.Collections.IList)(((Enterprise.BufferManagement.Business.WorkflowProvidersLinkViewModel)(null)).ProposedProcessHeaderLinks)).SyncRoot)).FP_TimeDelayMinutes)));
			this.LinksGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo2.ColumnName = "FP_LinkType";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zGuidDropEditColumnStyleInfo3.ColumnName = "FP_FH_HeaderFrom";
			zGuidDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180);
			zGuidDropEditColumnStyleInfo4.ColumnName = "FP_FH_HeaderTo";
			zGuidDropEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180);
			zTextBoxColumnStyleInfo2.ColumnName = "ActionDescription";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(450);
			zCheckBoxColumnStyleInfo2.ColumnName = "FP_SynchroniseBufferPenetration";
			zCheckBoxColumnStyleInfo2.IsVisible = false;
			zCheckBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.ColumnName = "FP_TimeDelayFactor";
			zCalcEditColumnStyleInfo3.IsVisible = false;
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo4.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo4.ColumnName = "FP_TimeDelayMinutes";
			zCalcEditColumnStyleInfo4.IsVisible = false;
			zCalcEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.LinksGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.LinksGrid.ColumnStyles.Add(zGuidDropEditColumnStyleInfo3);
			this.LinksGrid.ColumnStyles.Add(zGuidDropEditColumnStyleInfo4);
			this.LinksGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.LinksGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.LinksGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.LinksGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
			this.LinksGrid.CopySelectedRowsAllowed = true;
			this.LinksGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LinksGrid.GridId = "4f3aaac8-c2cf-4159-b3c2-e03b43d272b6";
			this.LinksGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.LinksGrid.LayoutKey = "PrerequisitesGrid";
			this.LinksGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.LinksGrid.Name = "LinksGrid";
			this.LinksGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(924, 238, true);
			this.LinksGrid.TabIndex = 2;
			// 
			// HintPanel
			// 
			this.HintPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.HintPanel.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
			this.HintPanel.Controls.Add(this.HintLabel);
			this.HintPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.HintPanel.Name = "HintPanel";
			this.HintPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(928, 31, true);
			this.HintPanel.TabIndex = 19;
			// 
			// HintLabel
			// 
			this.HintLabel.AutoSize = true;
			this.HintLabel.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("8c7f21f7-642b-4878-80c3-5dd55ea55868", "You have linked jobs which have relationships configured in a Workflow Template. Please review those relationships and press Create Links to proceed.");
			this.HintLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 9, true);
			this.HintLabel.Name = "HintLabel";
			this.HintLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(723, 13, true);
			this.HintLabel.TabIndex = 0;
			// 
			// CreateLinkButton
			// 
			this.CreateLinkButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CreateLinkButton.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("946fca13-8370-46ad-898b-99a50d1cae29", "Create Links");
			this.CreateLinkButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.CreateLinkButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(743, 280, true);
			this.CreateLinkButton.Name = "CreateLinkButton";
			this.CreateLinkButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(88, 25, true);
			this.CreateLinkButton.TabIndex = 17;
			this.CreateLinkButton.UseVisualStyleBackColor = true;
			this.CreateLinkButton.Click += new System.EventHandler(this.CreateLinkButton_Click);
			// 
			// CancelLinkButton
			// 
			this.CancelLinkButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CancelLinkButton.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("3b2dc9ff-6511-4806-8544-8613d2a228f4", "Cancel");
			this.CancelLinkButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CancelLinkButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(837, 280, true);
			this.CancelLinkButton.Name = "CancelLinkButton";
			this.CancelLinkButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(88, 25, true);
			this.CancelLinkButton.TabIndex = 18;
			this.CancelLinkButton.UseVisualStyleBackColor = true;
			this.CancelLinkButton.Click += new System.EventHandler(this.CancelLinkButton_Click);
			// 
			// WorkflowProvidersLinkageUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.LinksPanel);
			this.Controls.Add(this.HintPanel);
			this.Controls.Add(this.CreateLinkButton);
			this.Controls.Add(this.CancelLinkButton);
			this.Name = "WorkflowProvidersLinkageUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(928, 308, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.LinksPanel.ResumeLayout(false);
			this.LinksPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.LinksGrid)).EndInit();
			this.LinksGrid.ResumeLayout(false);
			this.LinksGrid.PerformLayout();
			this.HintPanel.ResumeLayout(false);
			this.HintPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZPanel LinksPanel;
		public ZArchitecture.ZGrid LinksGrid;
		private ZArchitecture.GUI.ZPanel HintPanel;
		private ZArchitecture.ZLabel HintLabel;
		public ZArchitecture.GUI.ZButton CreateLinkButton;
		public ZArchitecture.GUI.ZButton CancelLinkButton;
	}
}
