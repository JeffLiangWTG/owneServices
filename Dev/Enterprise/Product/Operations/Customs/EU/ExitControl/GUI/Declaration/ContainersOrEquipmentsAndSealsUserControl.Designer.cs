using CargoWise.Windows.UI;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.ExitControl.GUI
{
	partial class ContainersOrEquipmentsAndSealsUserControl
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
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.ContainersOrEquipmentsAndSealsSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.ContainersOrEquipmentsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ContainersOrEquipmentsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.SealsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.SealsGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ContainersOrEquipmentsAndSealsSplitContainer)).BeginInit();
			this.ContainersOrEquipmentsAndSealsSplitContainer.Panel1.SuspendLayout();
			this.ContainersOrEquipmentsAndSealsSplitContainer.Panel2.SuspendLayout();
			this.ContainersOrEquipmentsAndSealsSplitContainer.SuspendLayout();
			this.ContainersOrEquipmentsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ContainersOrEquipmentsGrid)).BeginInit();
			this.ContainersOrEquipmentsGrid.SuspendLayout();
			this.SealsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.SealsGrid)).BeginInit();
			this.SealsGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.ExitControl.Business.CusExitHeader);
			// 
			// ContainersOrEquipmentsAndSealsSplitContainer
			// 
			this.ContainersOrEquipmentsAndSealsSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ContainersOrEquipmentsAndSealsSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ContainersOrEquipmentsAndSealsSplitContainer.Name = "ContainersOrEquipmentsAndSealsSplitContainer";
			// 
			// ContainersOrEquipmentsAndSealsSplitContainer.Panel1
			// 
			this.ContainersOrEquipmentsAndSealsSplitContainer.Panel1.Controls.Add(this.ContainersOrEquipmentsGroupBox);
			// 
			// ContainersOrEquipmentsAndSealsSplitContainer.Panel2
			// 
			this.ContainersOrEquipmentsAndSealsSplitContainer.Panel2.Controls.Add(this.SealsGroupBox);
			this.ContainersOrEquipmentsAndSealsSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(706, 390, true);
			this.ContainersOrEquipmentsAndSealsSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(406);
			this.ContainersOrEquipmentsAndSealsSplitContainer.SplitterWidth = 11;
			this.ContainersOrEquipmentsAndSealsSplitContainer.TabIndex = 0;
			// 
			// ContainersOrEquipmentsGroupBox
			// 
			this.ContainersOrEquipmentsGroupBox.CaptionResourceString = Enterprise.Customs.EU.ExitControl.GUI.Res.GetData("ea2b8917-13c5-433a-960a-0abc6783fc4d", "Containers/Equipments");
			this.ContainersOrEquipmentsGroupBox.Controls.Add(this.ContainersOrEquipmentsGrid);
			this.ContainersOrEquipmentsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ContainersOrEquipmentsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ContainersOrEquipmentsGroupBox.Name = "ContainersOrEquipmentsGroupBox";
			this.ContainersOrEquipmentsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(406, 390, true);
			this.ContainersOrEquipmentsGroupBox.TabIndex = 0;
			this.ContainersOrEquipmentsGroupBox.TabStop = false;
			// 
			// ContainersOrEquipmentsGrid
			// 
			this.ContainersOrEquipmentsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ContainersOrEquipmentsGrid, "CusExitContainers");
			this.ContainersOrEquipmentsGrid.CaptionVisible = false;
			this.ContainersOrEquipmentsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ContainersOrEquipmentsGrid.GridId = "1d4424ae-eade-417a-bf29-41d8ee6d444f";
			this.ContainersOrEquipmentsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ContainersOrEquipmentsGrid.LayoutKey = "ContainersOrEquipmentsGrid";
			this.ContainersOrEquipmentsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 22, true);
			this.ContainersOrEquipmentsGrid.Name = "ContainersOrEquipmentsGrid";
			this.ContainersOrEquipmentsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(402, 366, true);
			this.ContainersOrEquipmentsGrid.TabIndex = 0;
			// 
			// SealsGroupBox
			// 
			this.SealsGroupBox.CaptionResourceString = Enterprise.Customs.EU.ExitControl.GUI.Res.GetData("D38447B2-D15D-4969-8C45-11A15ACC49CB", "Seals");
			this.SealsGroupBox.Controls.Add(this.SealsGrid);
			this.SealsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SealsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SealsGroupBox.Name = "SealsGroupBox";
			this.SealsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(290, 390, true);
			this.SealsGroupBox.TabIndex = 1;
			this.SealsGroupBox.TabStop = false;
			// 
			// SealsGrid
			// 
			this.SealsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.SealsGrid, "CusExitContainers.AllSealNumbers");
			this.SealsGrid.CaptionVisible = false;
			this.SealsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SealsGrid.GridId = "39fe993d-6e4e-4632-9a43-3381926a2a9a";
			this.SealsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.SealsGrid.LayoutKey = "SealsGrid";
			this.SealsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 22, true);
			this.SealsGrid.Name = "SealsGrid";
			this.SealsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(286, 366, true);
			this.SealsGrid.TabIndex = 1;
			// 
			// ContainersOrEquipmentsAndSealsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.AutoSize = true;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ContainersOrEquipmentsAndSealsSplitContainer);
			this.Name = "ContainersOrEquipmentsAndSealsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(706, 390, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ContainersOrEquipmentsAndSealsSplitContainer.Panel1.ResumeLayout(false);
			this.ContainersOrEquipmentsAndSealsSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.ContainersOrEquipmentsAndSealsSplitContainer)).EndInit();
			this.ContainersOrEquipmentsAndSealsSplitContainer.ResumeLayout(false);
			this.ContainersOrEquipmentsAndSealsSplitContainer.PerformLayout();
			this.ContainersOrEquipmentsGroupBox.ResumeLayout(false);
			this.ContainersOrEquipmentsGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ContainersOrEquipmentsGrid)).EndInit();
			this.ContainersOrEquipmentsGrid.ResumeLayout(false);
			this.ContainersOrEquipmentsGrid.PerformLayout();
			this.SealsGroupBox.ResumeLayout(false);
			this.SealsGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.SealsGrid)).EndInit();
			this.SealsGrid.ResumeLayout(false);
			this.SealsGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		KSplitContainer ContainersOrEquipmentsAndSealsSplitContainer;
		ZGroupBox ContainersOrEquipmentsGroupBox;
		ZGroupBox SealsGroupBox;
		ZGrid ContainersOrEquipmentsGrid;
		ZGrid SealsGrid;
	}
}

