
namespace Enterprise.Client.EDI.IncidentManager.GUI
{
	partial class PSQRelatedOpportunitiesUserControl
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
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.DetatchButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.EditButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.NewButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.AttachButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.OpportunitiesGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.OpportunitiesGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Client.EDI.IncidentManager.Business.ProfessionalServicesQuote);
			// 
			// DetatchButton
			// 
			this.DetatchButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.DetatchButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(805, 492, true);
			this.DetatchButton.Name = "DetatchButton";
			this.DetatchButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.DetatchButton.TabIndex = 5;
			this.DetatchButton.Text = "Detach";
			this.DetatchButton.UseVisualStyleBackColor = true;
			this.DetatchButton.Click += new System.EventHandler(this.DettachButton_Click);
			// 
			// EditButton
			// 
			this.EditButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.EditButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(724, 492, true);
			this.EditButton.Name = "EditButton";
			this.EditButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.EditButton.TabIndex = 4;
			this.EditButton.Text = "Edit";
			this.EditButton.UseVisualStyleBackColor = true;
			this.EditButton.Click += new System.EventHandler(this.EditButton_Click);
			// 
			// NewButton
			// 
			this.NewButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.NewButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(562, 492, true);
			this.NewButton.Name = "NewButton";
			this.NewButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.NewButton.TabIndex = 2;
			this.NewButton.Text = "New";
			this.NewButton.UseVisualStyleBackColor = true;
			this.NewButton.Click += new System.EventHandler(this.NewButton_Click);
			// 
			// AttachButton
			// 
			this.AttachButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.AttachButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(643, 492, true);
			this.AttachButton.Name = "AttachButton";
			this.AttachButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.AttachButton.TabIndex = 3;
			this.AttachButton.Text = "Attach";
			this.AttachButton.UseVisualStyleBackColor = true;
			this.AttachButton.Click += new System.EventHandler(this.AttachButton_Click);
			// 
			// OpportunitiesGrid
			// 
			this.OpportunitiesGrid.AllowNavigation = false;
			this.OpportunitiesGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.OpportunitiesGrid, "RelatedOpportunities");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.EDI.IncidentManager.Business.ProfessionalServicesQuote)(null)).RelatedOpportunities)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.MasterFiles.Business.EDIOrgOpportunity)(((System.Collections.IList)(((Enterprise.Client.EDI.IncidentManager.Business.ProfessionalServicesQuote)(null)).RelatedOpportunities)).SyncRoot)).P8_OpportunityID)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.MasterFiles.Business.EDIOrgOpportunity)(((System.Collections.IList)(((Enterprise.Client.EDI.IncidentManager.Business.ProfessionalServicesQuote)(null)).RelatedOpportunities)).SyncRoot)).P8_OpportunityDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.MasterFiles.Business.EDIOrgOpportunity)(((System.Collections.IList)(((Enterprise.Client.EDI.IncidentManager.Business.ProfessionalServicesQuote)(null)).RelatedOpportunities)).SyncRoot)).P8_OpportunityType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Client.EDI.MasterFiles.Business.EDIOrgOpportunity)(((System.Collections.IList)(((Enterprise.Client.EDI.IncidentManager.Business.ProfessionalServicesQuote)(null)).RelatedOpportunities)).SyncRoot)).SalesRelationModel.RecentActivityDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.MasterFiles.Business.EDIOrgOpportunity)(((System.Collections.IList)(((Enterprise.Client.EDI.IncidentManager.Business.ProfessionalServicesQuote)(null)).RelatedOpportunities)).SyncRoot)).StatusDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.MasterFiles.Business.EDIOrgOpportunity)(((System.Collections.IList)(((Enterprise.Client.EDI.IncidentManager.Business.ProfessionalServicesQuote)(null)).RelatedOpportunities)).SyncRoot)).SalesPersonName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.MasterFiles.Business.EDIOrgOpportunity)(((System.Collections.IList)(((Enterprise.Client.EDI.IncidentManager.Business.ProfessionalServicesQuote)(null)).RelatedOpportunities)).SyncRoot)).OutcomeDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.MasterFiles.Business.EDIOrgOpportunity)(((System.Collections.IList)(((Enterprise.Client.EDI.IncidentManager.Business.ProfessionalServicesQuote)(null)).RelatedOpportunities)).SyncRoot)).StageDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Client.EDI.MasterFiles.Business.EDIOrgOpportunity)(((System.Collections.IList)(((Enterprise.Client.EDI.IncidentManager.Business.ProfessionalServicesQuote)(null)).RelatedOpportunities)).SyncRoot)).P8_EstimatedValue)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Client.EDI.MasterFiles.Business.EDIOrgOpportunity)(((System.Collections.IList)(((Enterprise.Client.EDI.IncidentManager.Business.ProfessionalServicesQuote)(null)).RelatedOpportunities)).SyncRoot)).P8_ClosedDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.MasterFiles.Business.EDIOrgOpportunity)(((System.Collections.IList)(((Enterprise.Client.EDI.IncidentManager.Business.ProfessionalServicesQuote)(null)).RelatedOpportunities)).SyncRoot)).CloseReasonDescription)));

			this.OpportunitiesGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.ColumnName = "P8_OpportunityID";
			zTextBoxColumnStyleInfo2.ColumnName = "P8_OpportunityDescription";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zTextBoxColumnStyleInfo3.ColumnName = "P8_OpportunityType";
			zTextBoxColumnStyleInfo4.CaptionResourceString = ZClientEDI.Res.GetData("PSQRelatedOpportunitiesUserControl|54e7ccd3-766d-47d1-9795-68a78a87d5fe", "Status");
			zTextBoxColumnStyleInfo4.ColumnName = "StatusDescription";
			zTextBoxColumnStyleInfo5.CaptionResourceString = ZClientEDI.Res.GetData("PSQRelatedOpportunitiesUserControl|880df9e7-5929-418b-81e8-fd28f58a0460", "Sales Person");
			zTextBoxColumnStyleInfo5.ColumnName = "SalesPersonName";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo6.CaptionResourceString = ZClientEDI.Res.GetData("PSQRelatedOpportunitiesUserControl|1dfb5c9f-ddaf-42b1-8fd3-a1bd040607a2", "Outcome");
			zTextBoxColumnStyleInfo6.ColumnName = "OutcomeDescription";
			zTextBoxColumnStyleInfo7.CaptionResourceString = ZClientEDI.Res.GetData("PSQRelatedOpportunitiesUserControl|2fcf0c3f-52ea-46d8-bffd-4ef2d6821970", "Stage");
			zTextBoxColumnStyleInfo7.ColumnName = "StageDescription";
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "P8_EstimatedValue";
			zDateEditColumnStyleInfo2.CaptionResourceString = ZClientEDI.Res.GetData("PSQRelatedOpportunitiesUserControl|7fd9bc45-5ebc-44c3-a826-6d32cb815b6a", "Close Date");
			zDateEditColumnStyleInfo2.ColumnName = "P8_ClosedDate";
			zTextBoxColumnStyleInfo8.CaptionResourceString = ZClientEDI.Res.GetData("PSQRelatedOpportunitiesUserControl|45d27e8a-b49e-47c9-a106-0a2b9eb614ac", "Close Reason");
			zTextBoxColumnStyleInfo8.ColumnName = "CloseReasonDescription";
			this.OpportunitiesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.OpportunitiesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.OpportunitiesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.OpportunitiesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.OpportunitiesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.OpportunitiesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.OpportunitiesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.OpportunitiesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.OpportunitiesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.OpportunitiesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.OpportunitiesGrid.GridId = "af93909e-9dcf-48b7-8e45-704ed11e58ed";
			this.OpportunitiesGrid.CopySelectedRowsAllowed = true;
			this.OpportunitiesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.OpportunitiesGrid.LayoutKey = "OpportunitiesGrid";
			this.OpportunitiesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.OpportunitiesGrid.Name = "OpportunitiesGrid";
			this.OpportunitiesGrid.ReadOnly = true;
			this.OpportunitiesGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.OpportunitiesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(883, 490, true);
			this.OpportunitiesGrid.TabIndex = 1;
			this.OpportunitiesGrid.DoubleClick += new System.EventHandler(this.OpportunitiesGrid_DoubleClick);
			// 
			// PSQRelatedOpportunitiesUserControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.OpportunitiesGrid);
			this.Controls.Add(this.DetatchButton);
			this.Controls.Add(this.EditButton);
			this.Controls.Add(this.AttachButton);
			this.Controls.Add(this.NewButton);
			this.Name = "PSQRelatedOpportunitiesUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(883, 518, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.OpportunitiesGrid)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		protected Enterprise.ZArchitecture.GUI.ZButton DetatchButton;
		protected Enterprise.ZArchitecture.GUI.ZButton EditButton;
		protected Enterprise.ZArchitecture.GUI.ZButton NewButton;
		protected Enterprise.ZArchitecture.GUI.ZButton AttachButton;
		protected ZArchitecture.ZGrid OpportunitiesGrid;
	}
}
