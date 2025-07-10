namespace Enterprise.BufferManagement.GUI
{
	partial class ComponentRelationshipControl
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
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			this.ComponentRelationshipSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.DetailsBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.IsActiveCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.NameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.zGroupBox3 = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.zLabel1 = new Enterprise.ZArchitecture.ZLabel();
			this.ComponentLinkGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ComponentRelationshipSplitContainer)).BeginInit();
			this.ComponentRelationshipSplitContainer.Panel1.SuspendLayout();
			this.ComponentRelationshipSplitContainer.Panel2.SuspendLayout();
			this.ComponentRelationshipSplitContainer.SuspendLayout();
			this.DetailsBox.SuspendLayout();
			this.zGroupBox3.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ComponentLinkGrid)).BeginInit();
			this.ComponentLinkGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.BufferManagement.Business.ComponentRelationship);
			// 
			// ComponentRelationshipSplitContainer
			// 
			this.ComponentRelationshipSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ComponentRelationshipSplitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
			this.ComponentRelationshipSplitContainer.IsSplitterFixed = true;
			this.ComponentRelationshipSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ComponentRelationshipSplitContainer.Name = "ComponentRelationshipSplitContainer";
			this.ComponentRelationshipSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// ComponentRelationshipSplitContainer.Panel1
			// 
			this.ComponentRelationshipSplitContainer.Panel1.Controls.Add(this.DetailsBox);
			// 
			// ComponentRelationshipSplitContainer.Panel2
			// 
			this.ComponentRelationshipSplitContainer.Panel2.Controls.Add(this.zGroupBox3);
			this.ComponentRelationshipSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(692, 301, true);
			this.ComponentRelationshipSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(40);
			this.ComponentRelationshipSplitContainer.TabIndex = 3;
			// 
			// DetailsBox
			// 
			this.DetailsBox.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("bb328897-3728-4a88-9d6a-2b323ee53238", "Component Relationship");
			this.DetailsBox.Controls.Add(this.IsActiveCheckBox);
			this.DetailsBox.Controls.Add(this.NameTextBox);
			this.DetailsBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DetailsBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DetailsBox.Name = "DetailsBox";
			this.DetailsBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(692, 40, true);
			this.DetailsBox.TabIndex = 4;
			this.DetailsBox.TabStop = false;
			// 
			// IsActiveCheckBox
			// 
			this.IsActiveCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.IsActiveCheckBox, "FC_IsActive");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.BufferManagement.Business.ComponentRelationship)(null)).FC_IsActive)));
			this.IsActiveCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IsActiveCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(254, 17, true);
			this.IsActiveCheckBox.Name = "IsActiveCheckBox";
			this.IsActiveCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.IsActiveCheckBox.TabIndex = 1;
			this.IsActiveCheckBox.UseVisualStyleBackColor = true;
			// 
			// NameTextBox
			// 
			this.BindingSource.SetBindingMember(this.NameTextBox, "FC_Name");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.BufferManagement.Business.ComponentRelationship)(null)).FC_Name)));
			this.NameTextBox.CaptionResourceString = null;
			this.NameTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.NameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 17, true);
			this.NameTextBox.Name = "NameTextBox";
			this.NameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(133, 20, true);
			this.NameTextBox.TabIndex = 0;
			// 
			// zGroupBox3
			// 
			this.zGroupBox3.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.zGroupBox3.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("43faacb3-7a20-491a-9fb4-fbf1b9b55170", "Components");
			this.zGroupBox3.Controls.Add(this.zLabel1);
			this.zGroupBox3.Controls.Add(this.ComponentLinkGrid);
			this.zGroupBox3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.zGroupBox3.Name = "zGroupBox3";
			this.zGroupBox3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(692, 255, true);
			this.zGroupBox3.TabIndex = 5;
			this.zGroupBox3.TabStop = false;
			// 
			// zLabel1
			// 
			this.zLabel1.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("9d711408-f4eb-439d-8c75-ed1c771e7ff6", "Lists components included in the relationship.");
			this.zLabel1.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 15, true);
			this.zLabel1.Name = "zLabel1";
			this.zLabel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(767, 18, true);
			this.zLabel1.TabIndex = 6;
			// 
			// ComponentLinkGrid
			// 
			this.ComponentLinkGrid.AllowNavigation = false;
			this.ComponentLinkGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ComponentLinkGrid, "RelatedComponentLinks");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.BufferManagement.Business.ComponentRelationship)(null)).RelatedComponentLinks)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.BufferManagement.Business.ComponentRelationshipLink)(((System.Collections.IList)(((Enterprise.BufferManagement.Business.ComponentRelationship)(null)).RelatedComponentLinks)).SyncRoot)).FL_FC_ComponentTo)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.BufferManagement.Business.ComponentRelationshipLink)(((System.Collections.IList)(((Enterprise.BufferManagement.Business.ComponentRelationship)(null)).RelatedComponentLinks)).SyncRoot)).ComponentToSystemPK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.BufferManagement.Business.ComponentRelationshipLink)(((System.Collections.IList)(((Enterprise.BufferManagement.Business.ComponentRelationship)(null)).RelatedComponentLinks)).SyncRoot)).ComponentTo.FC_Type)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.BufferManagement.Business.ComponentRelationshipLink)(((System.Collections.IList)(((Enterprise.BufferManagement.Business.ComponentRelationship)(null)).RelatedComponentLinks)).SyncRoot)).ComponentTo.FC_IsActive)));
			this.ComponentLinkGrid.CaptionVisible = false;
			zGuidFindBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			zGuidFindBoxColumnStyleInfo1.ColumnName = "FL_FC_ComponentTo";
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			zGuidFindBoxColumnStyleInfo2.ColumnName = "ComponentToSystemPK";
			zGuidFindBoxColumnStyleInfo2.IsReadOnly = true;
			zGuidFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo1.ColumnName = "ComponentTo+FC_Type";
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo1.ColumnName = "ComponentTo+FC_IsActive";
			zCheckBoxColumnStyleInfo1.IsReadOnly = true;
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.ComponentLinkGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.ComponentLinkGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo2);
			this.ComponentLinkGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ComponentLinkGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.ComponentLinkGrid.GridId = "5b4e7ed3-10f7-47e5-845c-30402c313eef";
			this.ComponentLinkGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ComponentLinkGrid.LayoutKey = "BMComponentGrid";
			this.ComponentLinkGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 36, true);
			this.ComponentLinkGrid.Name = "ComponentLinkGrid";
			this.ComponentLinkGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(692, 219, true);
			this.ComponentLinkGrid.TabIndex = 2;
			// 
			// ComponentRelationshipControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ComponentRelationshipSplitContainer);
			this.Name = "ComponentRelationshipControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(692, 301, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ComponentRelationshipSplitContainer.Panel1.ResumeLayout(false);
			this.ComponentRelationshipSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.ComponentRelationshipSplitContainer)).EndInit();
			this.ComponentRelationshipSplitContainer.ResumeLayout(false);
			this.ComponentRelationshipSplitContainer.PerformLayout();
			this.DetailsBox.ResumeLayout(false);
			this.DetailsBox.PerformLayout();
			this.zGroupBox3.ResumeLayout(false);
			this.zGroupBox3.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ComponentLinkGrid)).EndInit();
			this.ComponentLinkGrid.ResumeLayout(false);
			this.ComponentLinkGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private CargoWise.Windows.UI.KSplitContainer ComponentRelationshipSplitContainer;
		private ZArchitecture.GUI.ZGroupBox DetailsBox;
		private ZArchitecture.GUI.ZCheckBox IsActiveCheckBox;
		private ZArchitecture.ZTextBox NameTextBox;
		private ZArchitecture.GUI.ZGroupBox zGroupBox3;
		private ZArchitecture.ZLabel zLabel1;
		public ZArchitecture.ZGrid ComponentLinkGrid;
	}
}
