using Enterprise.ZArchitecture.GUI;
using CargoWise.Types;

using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.GUI.DataMapping
{
	partial class CustomMapListsForm
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

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.ListsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.ListDefinitionGrid = new Enterprise.ZArchitecture.ZGrid();
			this.CloseButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ListsLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ListDefinitionLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ListsGrid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ListDefinitionGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 182, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(344, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.ZArchitecture.DataMapping.ImportWizard);
			// 
			// ListsGrid
			// 
			this.ListsGrid.AllowNavigation = false;
			this.ListsGrid.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)));
			this.BindingSource.SetBindingMember(this.ListsGrid, "CustomMapLists");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.ZArchitecture.DataMapping.ImportWizard)(null)).CustomMapLists)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ZArchitecture.DataMapping.CustomMapPairListWrapper)(((System.Collections.IList)(((Enterprise.ZArchitecture.DataMapping.ImportWizard)(null)).CustomMapLists)).SyncRoot)).Name)));
			this.ListsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("CustomMapListsForm|696b4776-5dc7-4950-9df7-a193a3978b11", "Name");
			zTextBoxColumnStyleInfo1.ColumnName = "Name";
			this.ListsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ListsGrid.GridId = "02defc49-75d7-4a82-adb2-6508bd4fa5bc";
			this.ListsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ListsGrid.LayoutKey = "ListsGrid";
			this.ListsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 28, true);
			this.ListsGrid.Name = "ListsGrid";
			this.ListsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(123, 118, true);
			this.ListsGrid.TabIndex = 1;
			// 
			// ListDefinitionGrid
			// 
			this.ListDefinitionGrid.AllowNavigation = false;
			this.ListDefinitionGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ListDefinitionGrid, "CustomMapLists.List");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.ZArchitecture.DataMapping.CustomMapPairListWrapper)(((System.Collections.IList)(((Enterprise.ZArchitecture.DataMapping.ImportWizard)(null)).CustomMapLists)).SyncRoot)).List)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ZArchitecture.DataMapping.CustomMapPair)(((System.Collections.IList)(((Enterprise.ZArchitecture.DataMapping.CustomMapPairListWrapper)(((System.Collections.IList)(((Enterprise.ZArchitecture.DataMapping.ImportWizard)(null)).CustomMapLists)).SyncRoot)).List)).SyncRoot)).Input)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ZArchitecture.DataMapping.CustomMapPair)(((System.Collections.IList)(((Enterprise.ZArchitecture.DataMapping.CustomMapPairListWrapper)(((System.Collections.IList)(((Enterprise.ZArchitecture.DataMapping.ImportWizard)(null)).CustomMapLists)).SyncRoot)).List)).SyncRoot)).Output)));
			this.ListDefinitionGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("CustomMapListsForm|b8a22d85-179c-4b13-9e16-e6a13e72238e", "Input");
			zTextBoxColumnStyleInfo2.ColumnName = "Input";
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("CustomMapListsForm|a7c45407-9c2b-4c1f-b8f6-a897c03721eb", "Output");
			zTextBoxColumnStyleInfo3.ColumnName = "Output";
			this.ListDefinitionGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.ListDefinitionGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.ListDefinitionGrid.GridId = "939a449d-b3d3-4fd6-b62c-7617fbe6c781";
			this.ListDefinitionGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ListDefinitionGrid.LayoutKey = "ListDefinitionGrid";
			this.ListDefinitionGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(152, 28, true);
			this.ListDefinitionGrid.Name = "ListDefinitionGrid";
			this.ListDefinitionGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(184, 118, true);
			this.ListDefinitionGrid.TabIndex = 2;
			// 
			// CloseButton
			// 
			this.CloseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CloseButton.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("CustomMapListsForm|9d3f768e-94d1-4aeb-ba27-3fa64204eadd", "Close");
			this.CloseButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(244, 154, true);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(92, 22, true);
			this.CloseButton.TabIndex = 3;
			this.CloseButton.UseVisualStyleBackColor = true;
			this.CloseButton.Click += new System.EventHandler(this.CloseButton_Click);
			// 
			// ListsLabel
			// 
			this.ListsLabel.AutoSize = true;
			this.ListsLabel.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("CustomMapListsForm|4cfa9c80-8b9a-4f4c-aa0a-7ca155219827", "Lists");
			this.ListsLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 9, true);
			this.ListsLabel.Name = "ListsLabel";
			this.ListsLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 13, true);
			this.ListsLabel.TabIndex = 4;
			// 
			// ListDefinitionLabel
			// 
			this.ListDefinitionLabel.AutoSize = true;
			this.ListDefinitionLabel.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("CustomMapListsForm|8df3e8c6-1eb2-4db4-a670-628d1aa26ad5", "Define Input/Output Pairs");
			this.ListDefinitionLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(149, 9, true);
			this.ListDefinitionLabel.Name = "ListDefinitionLabel";
			this.ListDefinitionLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 13, true);
			this.ListDefinitionLabel.TabIndex = 4;
			// 
			// CustomMapListsForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CancelButton = this.CloseButton;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(344, 206, true);
			this.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("CustomMapListsForm|635f23e5-c0e9-4afd-aec1-85599486c633", "Custom Map Lists");
			this.Controls.Add(this.ListDefinitionLabel);
			this.Controls.Add(this.ListsLabel);
			this.Controls.Add(this.CloseButton);
			this.Controls.Add(this.ListDefinitionGrid);
			this.Controls.Add(this.ListsGrid);
			this.DataSourceType = typeof(Enterprise.ZArchitecture.DataMapping.ImportWizard);
			this.DataSourceTypeName = "Enterprise.ZArchitecture.DataMapping.ImportWizard";
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(360, 240, true);
			this.Name = "CustomMapListsForm";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Controls.SetChildIndex(this.ListsGrid, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.ListDefinitionGrid, 0);
			this.Controls.SetChildIndex(this.CloseButton, 0);
			this.Controls.SetChildIndex(this.ListsLabel, 0);
			this.Controls.SetChildIndex(this.ListDefinitionLabel, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ListsGrid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ListDefinitionGrid)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZGrid ListsGrid;
		private ZGrid ListDefinitionGrid;
		private ZButton CloseButton;
		private ZLabel ListsLabel;
		private ZLabel ListDefinitionLabel;
	}
}
