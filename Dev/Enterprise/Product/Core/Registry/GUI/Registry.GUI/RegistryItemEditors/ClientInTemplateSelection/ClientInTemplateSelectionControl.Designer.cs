namespace Enterprise.Registry.GUI
{
	partial class ClientInTemplateSelectionControl
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
		void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.WorkflowTypesLabel = new Enterprise.ZArchitecture.ZLabel();
			this.OrgTypeSelectionPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.ClientInTemplateSelectionCriteriaAvailableOrgTypesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.ClientInTemplateSelectionCriteriaSelectedOrgTypesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.SelectedOrgTypesLabel = new Enterprise.ZArchitecture.ZLabel();
			this.AvailableOrgTypesLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ResetButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.MoveDownButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.MoveUpButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.RemoveOrgTypeButon = new Enterprise.ZArchitecture.GUI.ZButton();
			this.AddOrgTypeButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.OrgTypeMatchingLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ClientInTemplateSelectionCriteriaGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.OrgTypeSelectionPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ClientInTemplateSelectionCriteriaAvailableOrgTypesGrid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ClientInTemplateSelectionCriteriaSelectedOrgTypesGrid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ClientInTemplateSelectionCriteriaGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Registry.Business.ClientInTemplateSelectionCriteria);
			// 
			// WorkflowTypesLabel
			// 
			this.WorkflowTypesLabel.AutoSize = true;
			this.WorkflowTypesLabel.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("ClientInTemplateSelectionControl|d1f22b03-5a3a-4933-942c-5a7222839794", "Workflow Types");
			this.WorkflowTypesLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 0, true);
			this.WorkflowTypesLabel.Name = "WorkflowTypesLabel";
			this.WorkflowTypesLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(84, 13, true);
			this.WorkflowTypesLabel.TabIndex = 1;
			// 
			// OrgTypeSelectionPanel
			// 
			this.OrgTypeSelectionPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.OrgTypeSelectionPanel.Controls.Add(this.ClientInTemplateSelectionCriteriaAvailableOrgTypesGrid);
			this.OrgTypeSelectionPanel.Controls.Add(this.ClientInTemplateSelectionCriteriaSelectedOrgTypesGrid);
			this.OrgTypeSelectionPanel.Controls.Add(this.SelectedOrgTypesLabel);
			this.OrgTypeSelectionPanel.Controls.Add(this.AvailableOrgTypesLabel);
			this.OrgTypeSelectionPanel.Controls.Add(this.ResetButton);
			this.OrgTypeSelectionPanel.Controls.Add(this.MoveDownButton);
			this.OrgTypeSelectionPanel.Controls.Add(this.MoveUpButton);
			this.OrgTypeSelectionPanel.Controls.Add(this.RemoveOrgTypeButon);
			this.OrgTypeSelectionPanel.Controls.Add(this.AddOrgTypeButton);
			this.OrgTypeSelectionPanel.Controls.Add(this.OrgTypeMatchingLabel);
			this.OrgTypeSelectionPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 88, true);
			this.OrgTypeSelectionPanel.Name = "OrgTypeSelectionPanel";
			this.OrgTypeSelectionPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(436, 219, true);
			this.OrgTypeSelectionPanel.TabIndex = 3;
			// 
			// ClientInTemplateSelectionCriteriaAvailableOrgTypesGrid
			// 
			this.ClientInTemplateSelectionCriteriaAvailableOrgTypesGrid.AllowNavigation = false;
			this.ClientInTemplateSelectionCriteriaAvailableOrgTypesGrid.AllowSorting = false;
			this.ClientInTemplateSelectionCriteriaAvailableOrgTypesGrid.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
			this.BindingSource.SetBindingMember(this.ClientInTemplateSelectionCriteriaAvailableOrgTypesGrid, "AvailableItems");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.ClientInTemplateSelectionCriteria)(null)).AvailableItems)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.ClientInTemplateSelectionCriteriaOrgType)(((System.Collections.IList)(((Enterprise.Registry.Business.ClientInTemplateSelectionCriteria)(null)).AvailableItems)).SyncRoot)).OrgTypeCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.ZArchitecture.Core.MultilingualString)(((Enterprise.Registry.Business.ClientInTemplateSelectionCriteriaOrgType)(((System.Collections.IList)(((Enterprise.Registry.Business.ClientInTemplateSelectionCriteria)(null)).AvailableItems)).SyncRoot)).OrgTypeDescription)));
			this.ClientInTemplateSelectionCriteriaAvailableOrgTypesGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("ClientInTemplateSelectionControl|e4980e41-0eaa-442a-a32d-2ae8064839c1", "Code");
			zTextBoxColumnStyleInfo1.ColumnName = "OrgTypeCode";
			zTextBoxColumnStyleInfo1.IsVisible = false;
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("ClientInTemplateSelectionControl|9a4d6917-3b0b-4b45-94cd-386ce1c3121b", "Description");
			zTextBoxColumnStyleInfo2.ColumnName = "OrgTypeDescription";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140);
			this.ClientInTemplateSelectionCriteriaAvailableOrgTypesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ClientInTemplateSelectionCriteriaAvailableOrgTypesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.ClientInTemplateSelectionCriteriaAvailableOrgTypesGrid.CopySelectedRowsAllowed = true;
			this.ClientInTemplateSelectionCriteriaAvailableOrgTypesGrid.GridId = "190ae894-4f41-43a4-8e97-5b38ef9fa384";
			this.ClientInTemplateSelectionCriteriaAvailableOrgTypesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ClientInTemplateSelectionCriteriaAvailableOrgTypesGrid.IsWholeRowSelectedOnClick = true;
			this.ClientInTemplateSelectionCriteriaAvailableOrgTypesGrid.LayoutKey = "ClientInTemplateSelectionCriteriaAvailableOrgTypesGrid";
			this.ClientInTemplateSelectionCriteriaAvailableOrgTypesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 29, true);
			this.ClientInTemplateSelectionCriteriaAvailableOrgTypesGrid.Name = "ClientInTemplateSelectionCriteriaAvailableOrgTypesGrid";
			this.ClientInTemplateSelectionCriteriaAvailableOrgTypesGrid.RowHeadersVisible = false;
			this.ClientInTemplateSelectionCriteriaAvailableOrgTypesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(183, 186, true);
			this.ClientInTemplateSelectionCriteriaAvailableOrgTypesGrid.TabIndex = 5;
			// 
			// ClientInTemplateSelectionCriteriaSelectedOrgTypesGrid
			// 
			this.ClientInTemplateSelectionCriteriaSelectedOrgTypesGrid.AllowNavigation = false;
			this.ClientInTemplateSelectionCriteriaSelectedOrgTypesGrid.AllowSorting = false;
			this.ClientInTemplateSelectionCriteriaSelectedOrgTypesGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ClientInTemplateSelectionCriteriaSelectedOrgTypesGrid, "SelectedItems");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.ClientInTemplateSelectionCriteria)(null)).SelectedItems)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.ClientInTemplateSelectionCriteriaOrgType)(((System.Collections.IList)(((Enterprise.Registry.Business.ClientInTemplateSelectionCriteria)(null)).SelectedItems)).SyncRoot)).OrgTypeCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.ZArchitecture.Core.MultilingualString)(((Enterprise.Registry.Business.ClientInTemplateSelectionCriteriaOrgType)(((System.Collections.IList)(((Enterprise.Registry.Business.ClientInTemplateSelectionCriteria)(null)).SelectedItems)).SyncRoot)).OrgTypeDescription)));
			this.ClientInTemplateSelectionCriteriaSelectedOrgTypesGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("ClientInTemplateSelectionControl|85ac21e4-7e3e-4cd0-be87-5bc74de4b273", "Code");
			zTextBoxColumnStyleInfo3.ColumnName = "OrgTypeCode";
			zTextBoxColumnStyleInfo3.IsVisible = false;
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("ClientInTemplateSelectionControl|6dde46ff-6aaf-4896-bcf3-11285d41358a", "Description");
			zTextBoxColumnStyleInfo4.ColumnName = "OrgTypeDescription";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140);
			this.ClientInTemplateSelectionCriteriaSelectedOrgTypesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.ClientInTemplateSelectionCriteriaSelectedOrgTypesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.ClientInTemplateSelectionCriteriaSelectedOrgTypesGrid.CopySelectedRowsAllowed = true;
			this.ClientInTemplateSelectionCriteriaSelectedOrgTypesGrid.GridId = "ad7e1612-b730-4a23-bed1-84099bdf3ae7";
			this.ClientInTemplateSelectionCriteriaSelectedOrgTypesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ClientInTemplateSelectionCriteriaSelectedOrgTypesGrid.IsWholeRowSelectedOnClick = true;
			this.ClientInTemplateSelectionCriteriaSelectedOrgTypesGrid.LayoutKey = "ClientInTemplateSelectionCriteriaSelectedOrgTypesGrid";
			this.ClientInTemplateSelectionCriteriaSelectedOrgTypesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(270, 29, true);
			this.ClientInTemplateSelectionCriteriaSelectedOrgTypesGrid.Name = "ClientInTemplateSelectionCriteriaSelectedOrgTypesGrid";
			this.ClientInTemplateSelectionCriteriaSelectedOrgTypesGrid.RowHeadersVisible = false;
			this.ClientInTemplateSelectionCriteriaSelectedOrgTypesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(163, 186, true);
			this.ClientInTemplateSelectionCriteriaSelectedOrgTypesGrid.TabIndex = 12;
			// 
			// SelectedOrgTypesLabel
			// 
			this.SelectedOrgTypesLabel.AutoSize = true;
			this.SelectedOrgTypesLabel.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("ClientInTemplateSelectionControl|d151c093-996a-4808-9597-82aa63ea7098", "Match Organizations in this order");
			this.SelectedOrgTypesLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(269, 13, true);
			this.SelectedOrgTypesLabel.Name = "SelectedOrgTypesLabel";
			this.SelectedOrgTypesLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(161, 13, true);
			this.SelectedOrgTypesLabel.TabIndex = 11;
			// 
			// AvailableOrgTypesLabel
			// 
			this.AvailableOrgTypesLabel.AutoSize = true;
			this.AvailableOrgTypesLabel.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("ClientInTemplateSelectionControl|397abb42-07da-4e7e-99e8-d56a3d1bf5e5", "Organizations available for matching");
			this.AvailableOrgTypesLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 13, true);
			this.AvailableOrgTypesLabel.Name = "AvailableOrgTypesLabel";
			this.AvailableOrgTypesLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(177, 13, true);
			this.AvailableOrgTypesLabel.TabIndex = 4;
			// 
			// ResetButton
			// 
			this.ResetButton.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("ClientInTemplateSelectionControl|ff090435-c4cb-4bdd-8d2c-f8d85fc5ac4b", "Reset");
			this.ResetButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(189, 148, true);
			this.ResetButton.Name = "ResetButton";
			this.ResetButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.ResetButton.TabIndex = 10;
			this.ResetButton.UseVisualStyleBackColor = true;
			this.ResetButton.Click += new System.EventHandler(this.ResetButton_Click);
			// 
			// MoveDownButton
			// 
			this.MoveDownButton.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("ClientInTemplateSelectionControl|386d471c-7aaa-4a71-a441-03daec4060f9", "Move Down");
			this.MoveDownButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(189, 113, true);
			this.MoveDownButton.Name = "MoveDownButton";
			this.MoveDownButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.MoveDownButton.TabIndex = 9;
			this.MoveDownButton.UseVisualStyleBackColor = true;
			this.MoveDownButton.Click += new System.EventHandler(this.MoveDownButton_Click);
			// 
			// MoveUpButton
			// 
			this.MoveUpButton.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("ClientInTemplateSelectionControl|db6b37ad-43f1-49de-a527-2fff902d0cb1", "Move Up");
			this.MoveUpButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(189, 89, true);
			this.MoveUpButton.Name = "MoveUpButton";
			this.MoveUpButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.MoveUpButton.TabIndex = 8;
			this.MoveUpButton.UseVisualStyleBackColor = true;
			this.MoveUpButton.Click += new System.EventHandler(this.MoveUpButton_Click);
			// 
			// RemoveOrgTypeButon
			// 
			this.RemoveOrgTypeButon.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("ClientInTemplateSelectionControl|392956ce-bdfb-4d85-ab40-774975f64cfa", "<- Remove");
			this.RemoveOrgTypeButon.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(189, 53, true);
			this.RemoveOrgTypeButon.Name = "RemoveOrgTypeButon";
			this.RemoveOrgTypeButon.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.RemoveOrgTypeButon.TabIndex = 7;
			this.RemoveOrgTypeButon.UseVisualStyleBackColor = true;
			this.RemoveOrgTypeButon.Click += new System.EventHandler(this.RemoveOrgTypeButon_Click);
			// 
			// AddOrgTypeButton
			// 
			this.AddOrgTypeButton.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("ClientInTemplateSelectionControl|8fef6a5d-629f-4a81-ba50-ede65fc7c823", "Add ->");
			this.AddOrgTypeButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(189, 29, true);
			this.AddOrgTypeButton.Name = "AddOrgTypeButton";
			this.AddOrgTypeButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.AddOrgTypeButton.TabIndex = 6;
			this.AddOrgTypeButton.UseVisualStyleBackColor = true;
			this.AddOrgTypeButton.Click += new System.EventHandler(this.AddOrgTypeButton_Click);
			// 
			// OrgTypeMatchingLabel
			// 
			this.OrgTypeMatchingLabel.AutoSize = true;
			this.OrgTypeMatchingLabel.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("ClientInTemplateSelectionControl|5f68020e-1ba1-4643-816a-493f2721b5b0", "Match \'Client\' on selected template to the Organizations on the right in specified order");
			this.OrgTypeMatchingLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 0, true);
			this.OrgTypeMatchingLabel.Name = "OrgTypeMatchingLabel";
			this.OrgTypeMatchingLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 13, true);
			this.OrgTypeMatchingLabel.TabIndex = 0;
			// 
			// ClientInTemplateSelectionCriteriaGrid
			// 
			this.ClientInTemplateSelectionCriteriaGrid.AllowNavigation = false;
			this.ClientInTemplateSelectionCriteriaGrid.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ClientInTemplateSelectionCriteriaGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.ClientInTemplateSelectionCriteria)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.ClientInTemplateSelectionCriteria)(null)).ProcessTaskCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.ZArchitecture.Core.MultilingualString)(((Enterprise.Registry.Business.ClientInTemplateSelectionCriteria)(null)).ProcessTaskDescription)));
			this.ClientInTemplateSelectionCriteriaGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("ClientInTemplateSelectionControl|c70a26d2-41a2-46e5-b399-3f931b911a4e", "Code");
			zTextBoxColumnStyleInfo5.ColumnName = "ProcessTaskCode";
			zTextBoxColumnStyleInfo5.IsVisible = false;
			zTextBoxColumnStyleInfo6.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("ClientInTemplateSelectionControl|7d508de6-3142-4442-8f39-e4c42c2c0e10", "Description");
			zTextBoxColumnStyleInfo6.ColumnName = "ProcessTaskDescription";
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(300);
			this.ClientInTemplateSelectionCriteriaGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.ClientInTemplateSelectionCriteriaGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.ClientInTemplateSelectionCriteriaGrid.CopySelectedRowsAllowed = true;
			this.ClientInTemplateSelectionCriteriaGrid.GridId = "79785b53-9eef-4bd5-afaa-1dc82fc7fb75";
			this.ClientInTemplateSelectionCriteriaGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ClientInTemplateSelectionCriteriaGrid.IsWholeRowSelectedOnClick = true;
			this.ClientInTemplateSelectionCriteriaGrid.LayoutKey = "ClientInTemplateSelectionCriteriaGrid";
			this.ClientInTemplateSelectionCriteriaGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.ClientInTemplateSelectionCriteriaGrid.Name = "ClientInTemplateSelectionCriteriaGrid";
			this.ClientInTemplateSelectionCriteriaGrid.RowHeadersVisible = false;
			this.ClientInTemplateSelectionCriteriaGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(436, 66, true);
			this.ClientInTemplateSelectionCriteriaGrid.TabIndex = 2;
			// 
			// ClientInTemplateSelectionControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.OrgTypeSelectionPanel);
			this.Controls.Add(this.ClientInTemplateSelectionCriteriaGrid);
			this.Controls.Add(this.WorkflowTypesLabel);
			this.Name = "ClientInTemplateSelectionControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(442, 310, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.OrgTypeSelectionPanel.ResumeLayout(false);
			this.OrgTypeSelectionPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ClientInTemplateSelectionCriteriaAvailableOrgTypesGrid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ClientInTemplateSelectionCriteriaSelectedOrgTypesGrid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ClientInTemplateSelectionCriteriaGrid)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZLabel WorkflowTypesLabel;
		private ZArchitecture.GUI.ZPanel OrgTypeSelectionPanel;
		private ZArchitecture.ZLabel SelectedOrgTypesLabel;
		private ZArchitecture.ZLabel AvailableOrgTypesLabel;
		private ZArchitecture.GUI.ZButton ResetButton;
		private ZArchitecture.GUI.ZButton MoveDownButton;
		private ZArchitecture.GUI.ZButton MoveUpButton;
		private ZArchitecture.GUI.ZButton RemoveOrgTypeButon;
		private ZArchitecture.GUI.ZButton AddOrgTypeButton;
		private ZArchitecture.ZLabel OrgTypeMatchingLabel;
		internal ZArchitecture.ZGrid ClientInTemplateSelectionCriteriaSelectedOrgTypesGrid;
		internal ZArchitecture.ZGrid ClientInTemplateSelectionCriteriaGrid;
		internal ZArchitecture.ZGrid ClientInTemplateSelectionCriteriaAvailableOrgTypesGrid;
	}
}
