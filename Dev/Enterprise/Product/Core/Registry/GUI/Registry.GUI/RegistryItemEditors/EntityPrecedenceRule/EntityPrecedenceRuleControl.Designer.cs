namespace Enterprise.Registry.GUI
{
	partial class EntityPrecedenceRuleControl
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
			this.ItemsSelectionPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.AvailableItemsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.SelectedItemsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.SelectedItemsLabel = new Enterprise.ZArchitecture.ZLabel();
			this.AvailableItemsLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ResetButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.MoveDownButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.MoveUpButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.RemoveItemButon = new Enterprise.ZArchitecture.GUI.ZButton();
			this.AddItemButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ItemsSelectionPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.AvailableItemsGrid)).BeginInit();
			this.AvailableItemsGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.SelectedItemsGrid)).BeginInit();
			this.SelectedItemsGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Registry.Business.EntityPrecedenceRule);
			// 
			// ItemsSelectionPanel
			// 
			this.ItemsSelectionPanel.Controls.Add(this.AvailableItemsGrid);
			this.ItemsSelectionPanel.Controls.Add(this.SelectedItemsGrid);
			this.ItemsSelectionPanel.Controls.Add(this.SelectedItemsLabel);
			this.ItemsSelectionPanel.Controls.Add(this.AvailableItemsLabel);
			this.ItemsSelectionPanel.Controls.Add(this.ResetButton);
			this.ItemsSelectionPanel.Controls.Add(this.MoveDownButton);
			this.ItemsSelectionPanel.Controls.Add(this.MoveUpButton);
			this.ItemsSelectionPanel.Controls.Add(this.RemoveItemButon);
			this.ItemsSelectionPanel.Controls.Add(this.AddItemButton);
			this.ItemsSelectionPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ItemsSelectionPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ItemsSelectionPanel.Name = "ItemsSelectionPanel";
			this.ItemsSelectionPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(440, 267, true);
			this.ItemsSelectionPanel.TabIndex = 3;
			// 
			// AvailableItemsGrid
			// 
			this.AvailableItemsGrid.AllowNavigation = false;
			this.AvailableItemsGrid.AllowSorting = false;
			this.AvailableItemsGrid.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
			this.BindingSource.SetBindingMember(this.AvailableItemsGrid, "AvailableItems");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.EntityPrecedenceRule)(null)).AvailableItems)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.EntityPrecedenceRuleItem)(((System.Collections.IList)(((Enterprise.Registry.Business.EntityPrecedenceRule)(null)).AvailableItems)).SyncRoot)).Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.EntityPrecedenceRuleItem)(((System.Collections.IList)(((Enterprise.Registry.Business.EntityPrecedenceRule)(null)).AvailableItems)).SyncRoot)).Description)));
			this.AvailableItemsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("d1937fe2-9d69-4705-baab-f028094ab7dd", "Code");
			zTextBoxColumnStyleInfo1.ColumnName = "Code";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(27);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("6d521649-5a25-45c8-9daf-cc66cb5650cc", "Description");
			zTextBoxColumnStyleInfo2.ColumnName = "Description";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(135);
			this.AvailableItemsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.AvailableItemsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.AvailableItemsGrid.CopySelectedRowsAllowed = true;
			this.AvailableItemsGrid.GridId = "190ae894-4f41-43a4-8e97-5b38ef9fa384";
			this.AvailableItemsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.AvailableItemsGrid.IsWholeRowSelectedOnClick = true;
			this.AvailableItemsGrid.LayoutKey = "AvailableItemsGrid";
			this.AvailableItemsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 29, true);
			this.AvailableItemsGrid.Name = "AvailableItemsGrid";
			this.AvailableItemsGrid.RowHeadersVisible = false;
			this.AvailableItemsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(177, 234, true);
			this.AvailableItemsGrid.TabIndex = 5;
			// 
			// SelectedItemsGrid
			// 
			this.SelectedItemsGrid.AllowNavigation = false;
			this.SelectedItemsGrid.AllowSorting = false;
			this.BindingSource.SetBindingMember(this.SelectedItemsGrid, "SelectedItems");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.EntityPrecedenceRule)(null)).SelectedItems)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.EntityPrecedenceRuleItem)(((System.Collections.IList)(((Enterprise.Registry.Business.EntityPrecedenceRule)(null)).SelectedItems)).SyncRoot)).Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.EntityPrecedenceRuleItem)(((System.Collections.IList)(((Enterprise.Registry.Business.EntityPrecedenceRule)(null)).SelectedItems)).SyncRoot)).Description)));
			this.SelectedItemsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("ef2c5b83-f8b2-4d08-88bb-3fa70b41eb2e", "Code");
			zTextBoxColumnStyleInfo3.ColumnName = "Code";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(27);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("6e16d8d6-a123-4163-94e3-4cb983742287", "Description");
			zTextBoxColumnStyleInfo4.ColumnName = "Description";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(135);
			this.SelectedItemsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.SelectedItemsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.SelectedItemsGrid.CopySelectedRowsAllowed = true;
			this.SelectedItemsGrid.GridId = "ad7e1612-b730-4a23-bed1-84099bdf3ae7";
			this.SelectedItemsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.SelectedItemsGrid.IsWholeRowSelectedOnClick = true;
			this.SelectedItemsGrid.LayoutKey = "SelectedItemsGrid";
			this.SelectedItemsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(260, 29, true);
			this.SelectedItemsGrid.Name = "SelectedItemsGrid";
			this.SelectedItemsGrid.RowHeadersVisible = false;
			this.SelectedItemsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(178, 234, true);
			this.SelectedItemsGrid.TabIndex = 12;
			// 
			// SelectedItemsLabel
			// 
			this.SelectedItemsLabel.AutoSize = true;
			this.SelectedItemsLabel.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("8d5d8166-a7aa-4ab2-b89c-adade98a6d05", "Selected Items:");
			this.SelectedItemsLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(264, 13, true);
			this.SelectedItemsLabel.Name = "SelectedItemsLabel";
			this.SelectedItemsLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 13, true);
			this.SelectedItemsLabel.TabIndex = 11;
			// 
			// AvailableItemsLabel
			// 
			this.AvailableItemsLabel.AutoSize = true;
			this.AvailableItemsLabel.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("4a8a1949-472c-47e5-a461-7b7c964dbaf0", "Items available for selection:");
			this.AvailableItemsLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 13, true);
			this.AvailableItemsLabel.Name = "AvailableItemsLabel";
			this.AvailableItemsLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(139, 13, true);
			this.AvailableItemsLabel.TabIndex = 4;
			// 
			// ResetButton
			// 
			this.ResetButton.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("96d86a4c-e93b-4227-8ade-1ecc0b5380b1", "Reset");
			this.ResetButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(181, 148, true);
			this.ResetButton.Name = "ResetButton";
			this.ResetButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.ResetButton.TabIndex = 10;
			this.ResetButton.UseVisualStyleBackColor = true;
			this.ResetButton.Click += new System.EventHandler(this.ResetButton_Click);
			// 
			// MoveDownButton
			// 
			this.MoveDownButton.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("6c1df757-ff0d-4b2f-9883-731606892f42", "Move Down");
			this.MoveDownButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(181, 113, true);
			this.MoveDownButton.Name = "MoveDownButton";
			this.MoveDownButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.MoveDownButton.TabIndex = 9;
			this.MoveDownButton.UseVisualStyleBackColor = true;
			this.MoveDownButton.Click += new System.EventHandler(this.MoveDownButton_Click);
			// 
			// MoveUpButton
			// 
			this.MoveUpButton.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("ddd259fb-ef62-4f11-8878-e98db94153f9", "Move Up");
			this.MoveUpButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(181, 89, true);
			this.MoveUpButton.Name = "MoveUpButton";
			this.MoveUpButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.MoveUpButton.TabIndex = 8;
			this.MoveUpButton.UseVisualStyleBackColor = true;
			this.MoveUpButton.Click += new System.EventHandler(this.MoveUpButton_Click);
			// 
			// RemoveItemButon
			// 
			this.RemoveItemButon.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("05564f32-ffc4-4378-b5aa-de4af0dd8897", "Remove");
			this.RemoveItemButon.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(181, 53, true);
			this.RemoveItemButon.Name = "RemoveItemButon";
			this.RemoveItemButon.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.RemoveItemButon.TabIndex = 7;
			this.RemoveItemButon.UseVisualStyleBackColor = true;
			this.RemoveItemButon.Click += new System.EventHandler(this.RemoveItemButon_Click);
			// 
			// AddItemButton
			// 
			this.AddItemButton.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("f5a1e272-0466-4648-a1ab-48bf151f11df", "Add");
			this.AddItemButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(181, 29, true);
			this.AddItemButton.Name = "AddItemButton";
			this.AddItemButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.AddItemButton.TabIndex = 6;
			this.AddItemButton.UseVisualStyleBackColor = true;
			this.AddItemButton.Click += new System.EventHandler(this.AddItemButton_Click);
			// 
			// EntityPrecedenceRuleControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ItemsSelectionPanel);
			this.Name = "EntityPrecedenceRuleControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(440, 267, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ItemsSelectionPanel.ResumeLayout(false);
			this.ItemsSelectionPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.AvailableItemsGrid)).EndInit();
			this.AvailableItemsGrid.ResumeLayout(false);
			this.AvailableItemsGrid.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.SelectedItemsGrid)).EndInit();
			this.SelectedItemsGrid.ResumeLayout(false);
			this.SelectedItemsGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZPanel ItemsSelectionPanel;
		private ZArchitecture.ZLabel SelectedItemsLabel;
		private ZArchitecture.ZLabel AvailableItemsLabel;
		private ZArchitecture.GUI.ZButton ResetButton;
		private ZArchitecture.GUI.ZButton MoveDownButton;
		private ZArchitecture.GUI.ZButton MoveUpButton;
		private ZArchitecture.GUI.ZButton RemoveItemButon;
		private ZArchitecture.GUI.ZButton AddItemButton;
		internal ZArchitecture.ZGrid SelectedItemsGrid;
		internal ZArchitecture.ZGrid AvailableItemsGrid;
	}
}
