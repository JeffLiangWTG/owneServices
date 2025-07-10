
namespace Enterprise.Client.EDI.IncidentManager.GUI
{
	partial class EDIOpportunityRelatedPSQsUserControl
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
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			this.DetatchButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.EditButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.NewButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.AttachButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.PSQsGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.PSQsGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Client.EDI.MasterFiles.Business.EDIOrgOpportunity);
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
			// PSQsGrid
			// 
			this.PSQsGrid.AllowNavigation = false;
			this.PSQsGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.PSQsGrid, "RelatedPSQs");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.EDI.MasterFiles.Business.EDIOrgOpportunity)(null)).RelatedPSQs)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IncidentManager.Business.ProfessionalServicesQuote)(((System.Collections.IList)(((Enterprise.Client.EDI.MasterFiles.Business.EDIOrgOpportunity)(null)).RelatedPSQs)).SyncRoot)).Number)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IncidentManager.Business.ProfessionalServicesQuote)(((System.Collections.IList)(((Enterprise.Client.EDI.MasterFiles.Business.EDIOrgOpportunity)(null)).RelatedPSQs)).SyncRoot)).Criticality)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IncidentManager.Business.ProfessionalServicesQuote)(((System.Collections.IList)(((Enterprise.Client.EDI.MasterFiles.Business.EDIOrgOpportunity)(null)).RelatedPSQs)).SyncRoot)).IM_Status)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IncidentManager.Business.ProfessionalServicesQuote)(((System.Collections.IList)(((Enterprise.Client.EDI.MasterFiles.Business.EDIOrgOpportunity)(null)).RelatedPSQs)).SyncRoot)).IM_Description)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IncidentManager.Business.ProfessionalServicesQuote)(((System.Collections.IList)(((Enterprise.Client.EDI.MasterFiles.Business.EDIOrgOpportunity)(null)).RelatedPSQs)).SyncRoot)).AssignedStaffCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Client.EDI.IncidentManager.Business.ProfessionalServicesQuote)(((System.Collections.IList)(((Enterprise.Client.EDI.MasterFiles.Business.EDIOrgOpportunity)(null)).RelatedPSQs)).SyncRoot)).IM_RequiredBy)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Client.EDI.IncidentManager.Business.ProfessionalServicesQuote)(((System.Collections.IList)(((Enterprise.Client.EDI.MasterFiles.Business.EDIOrgOpportunity)(null)).RelatedPSQs)).SyncRoot)).IM_CloseTimeUtc)));
			this.PSQsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = ZClientEDI.Res.GetData("EDIOpportunityRelatedPSQsUserControl|46fb36a4-f9fe-44b7-b41b-edf765528293", "No.");
			zTextBoxColumnStyleInfo1.ColumnName = "Number";
			zTextBoxColumnStyleInfo2.CaptionResourceString = ZClientEDI.Res.GetData("EDIOpportunityRelatedPSQsUserControl|ced20c86-c0e1-4e25-aa65-40886ec2f1bb", "Priority");
			zTextBoxColumnStyleInfo2.ColumnName = "Criticality";
			zTextBoxColumnStyleInfo3.ColumnName = "IM_Status";
			zTextBoxColumnStyleInfo4.ColumnName = "IM_Description";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zTextBoxColumnStyleInfo5.CaptionResourceString = ZClientEDI.Res.GetData("EDIOpportunityRelatedPSQsUserControl|20e5faf6-0065-4d9f-995b-86a947976df7", "Assigned To");
			zTextBoxColumnStyleInfo5.ColumnName = "AssignedStaffCode";
			zDateEditColumnStyleInfo1.ColumnName = "IM_RequiredBy";
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDateEditColumnStyleInfo2.ColumnName = "IM_CloseTime";
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDateEditColumnStyleInfo3.ColumnName = "IM_CloseTimeUtc";
			zDateEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			this.PSQsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.PSQsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.PSQsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.PSQsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.PSQsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.PSQsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.PSQsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.PSQsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo3);
			this.PSQsGrid.GridId = "973719cd-2a00-4306-9250-9d2e1b4f04d6";
			this.PSQsGrid.CopySelectedRowsAllowed = true;
			this.PSQsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.PSQsGrid.LayoutKey = "PSQsGrid";
			this.PSQsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.PSQsGrid.Name = "PSQsGrid";
			this.PSQsGrid.ReadOnly = true;
			this.PSQsGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.PSQsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(883, 490, true);
			this.PSQsGrid.TabIndex = 1;
			this.PSQsGrid.DoubleClick += new System.EventHandler(this.PSQsGrid_DoubleClick);
			// 
			// EDIOpportunityRelatedPSQsUserControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.PSQsGrid);
			this.Controls.Add(this.DetatchButton);
			this.Controls.Add(this.EditButton);
			this.Controls.Add(this.AttachButton);
			this.Controls.Add(this.NewButton);
			this.Name = "EDIOpportunityRelatedPSQsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(883, 518, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.PSQsGrid)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		protected Enterprise.ZArchitecture.GUI.ZButton DetatchButton;
		protected Enterprise.ZArchitecture.GUI.ZButton EditButton;
		protected Enterprise.ZArchitecture.GUI.ZButton NewButton;
		protected Enterprise.ZArchitecture.GUI.ZButton AttachButton;
		protected ZArchitecture.ZGrid PSQsGrid;
	}
}
