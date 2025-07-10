using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.AU.Declaration.GUI
{
	partial class ImportManifestImportForm
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
		protected override void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			this.CancellButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.SaveButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ManifestsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.SelectButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.DeselectButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ArrivalPortsDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.DischargePort = new Enterprise.ZArchitecture.ZLabel();
			this.FindButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ManifestsGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 336, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(725, 24, true);
			this.MainStatusBar.TabIndex = 8;
			// 
			// CancellButton
			// 
			this.CancellButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CancellButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(571, 307, true);
			this.CancellButton.Name = "CancellButton";
			this.CancellButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(142, 23, true);
			this.CancellButton.TabIndex = 7;
			this.CancellButton.Text = "Cancel";
			this.CancellButton.UseVisualStyleBackColor = true;
			this.CancellButton.Click += new System.EventHandler(this.CancellButton_Click);
			// 
			// SaveButton
			// 
			this.SaveButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.SaveButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(420, 307, true);
			this.SaveButton.Name = "SaveButton";
			this.SaveButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(145, 23, true);
			this.SaveButton.TabIndex = 6;
			this.SaveButton.Text = "Create/Update Manifest";
			this.SaveButton.UseVisualStyleBackColor = true;
			this.SaveButton.Click += new System.EventHandler(this.SaveButton_Click);
			// 
			// ManifestsGrid
			// 
			this.ManifestsGrid.AllowNavigation = false;
			this.ManifestsGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.ManifestsGrid.BindTo = "ItemsView";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.ImportManifestItem)(((object)(((Enterprise.Customs.AU.Declaration.Business.ImportManifestItemCollection)(null)))))).ItemsView)));
			this.ManifestsGrid.CaptionVisible = false;
			zCheckBoxColumnStyleInfo1.Caption = "Select";
			zCheckBoxColumnStyleInfo1.ColumnName = "ShouldSave";
			zTextBoxColumnStyleInfo1.Caption = "Discharge Port";
			zTextBoxColumnStyleInfo1.ColumnName = "DischargePort";
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.Caption = "Vessel Name";
			zTextBoxColumnStyleInfo2.ColumnName = "VesselName";
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo3.Caption = "Voyage Number";
			zTextBoxColumnStyleInfo3.ColumnName = "VoyageNumber";
			zTextBoxColumnStyleInfo3.IsReadOnly = true;
			zTextBoxColumnStyleInfo4.Caption = "Bill Number";
			zTextBoxColumnStyleInfo4.ColumnName = "BillNumber";
			zTextBoxColumnStyleInfo4.IsReadOnly = true;
			zCheckBoxColumnStyleInfo2.Caption = "Is CargoList?";
			zCheckBoxColumnStyleInfo2.ColumnName = "IsCargoList";
			zCheckBoxColumnStyleInfo2.IsReadOnly = true;
			this.ManifestsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.ManifestsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ManifestsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.ManifestsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.ManifestsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.ManifestsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.ManifestsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ManifestsGrid.LayoutKey = "ManifestsGrid";
			this.ManifestsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 36, true);
			this.ManifestsGrid.Name = "ManifestsGrid";
			this.ManifestsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(701, 265, true);
			this.ManifestsGrid.TabIndex = 3;
			// Compile time check for the above grid columns. If any of these lines fail, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZBool)(((Enterprise.Customs.AU.Declaration.Business.ImportManifestItem)(((object)(((Enterprise.Customs.AU.Declaration.Business.ImportManifestItem)(((object)(((Enterprise.Customs.AU.Declaration.Business.ImportManifestItemCollection)(null)))))).ItemsView)))).ShouldSave)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.ImportManifestItem)(((object)(((Enterprise.Customs.AU.Declaration.Business.ImportManifestItem)(((object)(((Enterprise.Customs.AU.Declaration.Business.ImportManifestItemCollection)(null)))))).ItemsView)))).ShouldSaveInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.ImportManifestItem)(((object)(((Enterprise.Customs.AU.Declaration.Business.ImportManifestItem)(((object)(((Enterprise.Customs.AU.Declaration.Business.ImportManifestItemCollection)(null)))))).ItemsView)))).DischargePortInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.ImportManifestItem)(((object)(((Enterprise.Customs.AU.Declaration.Business.ImportManifestItem)(((object)(((Enterprise.Customs.AU.Declaration.Business.ImportManifestItemCollection)(null)))))).ItemsView)))).DischargePort)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.ImportManifestItem)(((object)(((Enterprise.Customs.AU.Declaration.Business.ImportManifestItem)(((object)(((Enterprise.Customs.AU.Declaration.Business.ImportManifestItemCollection)(null)))))).ItemsView)))).VesselNameInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.ImportManifestItem)(((object)(((Enterprise.Customs.AU.Declaration.Business.ImportManifestItem)(((object)(((Enterprise.Customs.AU.Declaration.Business.ImportManifestItemCollection)(null)))))).ItemsView)))).VesselName)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.ImportManifestItem)(((object)(((Enterprise.Customs.AU.Declaration.Business.ImportManifestItem)(((object)(((Enterprise.Customs.AU.Declaration.Business.ImportManifestItemCollection)(null)))))).ItemsView)))).VoyageNumberInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.ImportManifestItem)(((object)(((Enterprise.Customs.AU.Declaration.Business.ImportManifestItem)(((object)(((Enterprise.Customs.AU.Declaration.Business.ImportManifestItemCollection)(null)))))).ItemsView)))).VoyageNumber)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.ImportManifestItem)(((object)(((Enterprise.Customs.AU.Declaration.Business.ImportManifestItem)(((object)(((Enterprise.Customs.AU.Declaration.Business.ImportManifestItemCollection)(null)))))).ItemsView)))).BillNumberInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.ImportManifestItem)(((object)(((Enterprise.Customs.AU.Declaration.Business.ImportManifestItem)(((object)(((Enterprise.Customs.AU.Declaration.Business.ImportManifestItemCollection)(null)))))).ItemsView)))).BillNumber)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZBool)(((Enterprise.Customs.AU.Declaration.Business.ImportManifestItem)(((object)(((Enterprise.Customs.AU.Declaration.Business.ImportManifestItem)(((object)(((Enterprise.Customs.AU.Declaration.Business.ImportManifestItemCollection)(null)))))).ItemsView)))).IsCargoList)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.ImportManifestItem)(((object)(((Enterprise.Customs.AU.Declaration.Business.ImportManifestItem)(((object)(((Enterprise.Customs.AU.Declaration.Business.ImportManifestItemCollection)(null)))))).ItemsView)))).IsCargoListInfo)));
			// 
			// SelectButton
			// 
			this.SelectButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.SelectButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 307, true);
			this.SelectButton.Name = "SelectButton";
			this.SelectButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.SelectButton.TabIndex = 4;
			this.SelectButton.Text = "Select";
			this.SelectButton.UseVisualStyleBackColor = true;
			this.SelectButton.Click += new System.EventHandler(this.SelectButton_Click);
			// 
			// DeselectButton
			// 
			this.DeselectButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.DeselectButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(93, 307, true);
			this.DeselectButton.Name = "DeselectButton";
			this.DeselectButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.DeselectButton.TabIndex = 5;
			this.DeselectButton.Text = "Deselect";
			this.DeselectButton.UseVisualStyleBackColor = true;
			this.DeselectButton.Click += new System.EventHandler(this.DeselectButton_Click);
			// 
			// ArrivalPortsDropEdit
			// 
			this.ArrivalPortsDropEdit.BindTo = "SelectedPort";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.ImportManifestItem)(((object)(((Enterprise.Customs.AU.Declaration.Business.ImportManifestItemCollection)(null)))))).SelectedPortInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.ImportManifestItem)(((object)(((Enterprise.Customs.AU.Declaration.Business.ImportManifestItemCollection)(null)))))).SelectedPort)));
			this.ArrivalPortsDropEdit.BindToList = "ArrivalPorts";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.ImportManifestItem)(((object)(((Enterprise.Customs.AU.Declaration.Business.ImportManifestItemCollection)(null)))))).ArrivalPorts)));
			this.ArrivalPortsDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(113, 10, true);
			this.ArrivalPortsDropEdit.MaxItemsToShowInDropDown = 30;
			this.ArrivalPortsDropEdit.Name = "ArrivalPortsDropEdit";
			this.ArrivalPortsDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(312, 20, true);
			this.ArrivalPortsDropEdit.TabIndex = 1;
			// 
			// DischargePort
			// 
			this.DischargePort.AutoSize = true;
			this.DischargePort.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(13, 13, true);
			this.DischargePort.Name = "DischargePort";
			this.DischargePort.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(92, 13, true);
			this.DischargePort.TabIndex = 0;
			this.DischargePort.Text = "Port of Discharge:";
			// 
			// FindButton
			// 
			this.FindButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(467, 8, true);
			this.FindButton.Name = "FindButton";
			this.FindButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.FindButton.TabIndex = 2;
			this.FindButton.Text = "Find";
			this.FindButton.UseVisualStyleBackColor = true;
			this.FindButton.Click += new System.EventHandler(this.FindButton_Click);
			// 
			// ImportManifestImportForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(725, 360, true);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(725, 360, true);
			this.Controls.Add(this.DischargePort);
			this.Controls.Add(this.DeselectButton);
			this.Controls.Add(this.ArrivalPortsDropEdit);
			this.Controls.Add(this.SelectButton);
			this.Controls.Add(this.ManifestsGrid);
			this.Controls.Add(this.FindButton);
			this.Controls.Add(this.SaveButton);
			this.Controls.Add(this.CancellButton);
			this.DataSourceAssemblyName = "Enterprise.Customs.AU.Declaration.Business";
			this.DataSourceType = typeof(Enterprise.Customs.AU.Declaration.Business.ImportManifestItemCollection);
			this.DataSourceTypeName = "Enterprise.Customs.AU.Declaration.Business.ImportManifestItemCollection";
			this.Name = "ImportManifestImportForm";
			this.Text = "Create Import Manifests";
			this.Controls.SetChildIndex(this.CancellButton, 0);
			this.Controls.SetChildIndex(this.SaveButton, 0);
			this.Controls.SetChildIndex(this.FindButton, 0);
			this.Controls.SetChildIndex(this.ManifestsGrid, 0);
			this.Controls.SetChildIndex(this.SelectButton, 0);
			this.Controls.SetChildIndex(this.ArrivalPortsDropEdit, 0);
			this.Controls.SetChildIndex(this.DeselectButton, 0);
			this.Controls.SetChildIndex(this.DischargePort, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ManifestsGrid)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal Enterprise.ZArchitecture.GUI.ZButton CancellButton;
		internal Enterprise.ZArchitecture.GUI.ZButton SaveButton;
		private Enterprise.ZArchitecture.ZGrid ManifestsGrid;
		private Enterprise.ZArchitecture.GUI.ZButton SelectButton;
		private Enterprise.ZArchitecture.GUI.ZButton DeselectButton;
		private Enterprise.ZArchitecture.GUI.ZDropEdit ArrivalPortsDropEdit;
		private Enterprise.ZArchitecture.ZLabel DischargePort;
		private Enterprise.ZArchitecture.GUI.ZButton FindButton;
	}
}
