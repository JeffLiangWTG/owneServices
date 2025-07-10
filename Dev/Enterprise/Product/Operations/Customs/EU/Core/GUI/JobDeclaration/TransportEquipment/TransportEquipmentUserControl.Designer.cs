namespace Enterprise.Customs.EU.GUI
{
	partial class TransportEquipmentUserControl
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
			this.EquipmentsGroupBoxPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.EquipmentsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.EquipmentsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.SealsGroupBoxPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.SealsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.SealsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.Splitter = new CargoWise.Windows.UI.KSplitter();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.EquipmentsGroupBoxPanel.SuspendLayout();
			this.EquipmentsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.EquipmentsGrid)).BeginInit();
			this.EquipmentsGrid.SuspendLayout();
			this.SealsGroupBoxPanel.SuspendLayout();
			this.SealsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.SealsGrid)).BeginInit();
			this.SealsGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.Business.Declaration.JobDeclaration);
			// 
			// EquipmentsGroupBoxPanel
			// 
			this.EquipmentsGroupBoxPanel.Controls.Add(this.EquipmentsGroupBox);
			this.EquipmentsGroupBoxPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.EquipmentsGroupBoxPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.EquipmentsGroupBoxPanel.Name = "EquipmentsGroupBoxPanel";
			this.EquipmentsGroupBoxPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(711, 217, true);
			this.EquipmentsGroupBoxPanel.TabIndex = 1;
			// 
			// EquipmentsGroupBox
			// 
			this.EquipmentsGroupBox.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("facde1ab-c245-4630-9ecc-d5354478633a", "Equipments");
			this.EquipmentsGroupBox.Controls.Add(this.EquipmentsGrid);
			this.EquipmentsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.EquipmentsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.EquipmentsGroupBox.Name = "EquipmentsGroupBox";
			this.EquipmentsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(711, 217, true);
			this.EquipmentsGroupBox.TabIndex = 0;
			this.EquipmentsGroupBox.TabStop = false;
			// 
			// EquipmentsGrid
			// 
			this.EquipmentsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.EquipmentsGrid, "Equipments");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).Equipments)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.CusEquipment)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).Equipments)).SyncRoot)).CEQ_IdentificationNumber)));
			this.EquipmentsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "CEQ_IdentificationNumber";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			this.EquipmentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.EquipmentsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.EquipmentsGrid.GridId = "9fdaa7fa-8551-49a5-8c57-8899985df5c3";
			this.EquipmentsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.EquipmentsGrid.LayoutKey = "EquipmentsGrid";
			this.EquipmentsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.EquipmentsGrid.Name = "EquipmentsGrid";
			this.EquipmentsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(705, 198, true);
			this.EquipmentsGrid.TabIndex = 0;
			// 
			// SealsGroupBoxPanel
			// 
			this.SealsGroupBoxPanel.Controls.Add(this.SealsGroupBox);
			this.SealsGroupBoxPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.SealsGroupBoxPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 220, true);
			this.SealsGroupBoxPanel.Name = "SealsGroupBoxPanel";
			this.SealsGroupBoxPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(711, 217, true);
			this.SealsGroupBoxPanel.TabIndex = 2;
			// 
			// SealsGroupBox
			// 
			this.SealsGroupBox.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("c0ac0029-ed41-42db-8a1c-82cdce4885a8", "Seals");
			this.SealsGroupBox.Controls.Add(this.SealsGrid);
			this.SealsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SealsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SealsGroupBox.Name = "SealsGroupBox";
			this.SealsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(711, 217, true);
			this.SealsGroupBox.TabIndex = 0;
			this.SealsGroupBox.TabStop = false;
			// 
			// SealsGrid
			// 
			this.SealsGrid.AllowNavigation = false;
			this.SealsGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.SealsGrid, "Equipments.Seals");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.CusEquipment)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).Equipments)).SyncRoot)).Seals)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.CusSeal)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.CusEquipment)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).Equipments)).SyncRoot)).Seals)).SyncRoot)).BK_SealNumber)));
			this.SealsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo2.ColumnName = "BK_SealNumber";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			this.SealsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.SealsGrid.GridId = "d87612d5-13b2-4f15-ab3c-83202d1e2bea";
			this.SealsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.SealsGrid.LayoutKey = "SealsGrid";
			this.SealsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.SealsGrid.Name = "SealsGrid";
			this.SealsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(705, 199, true);
			this.SealsGrid.TabIndex = 0;
			// 
			// Splitter
			// 
			this.Splitter.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.Splitter.DoNotSaveSplitterLayout = false;
			this.Splitter.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 217, true);
			this.Splitter.Name = "Splitter";
			this.Splitter.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(711, 3, true);
			this.Splitter.TabIndex = 1;
			this.Splitter.TabStop = false;
			// 
			// TransportEquipmentUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.EquipmentsGroupBoxPanel);
			this.Controls.Add(this.Splitter);
			this.Controls.Add(this.SealsGroupBoxPanel);
			this.Name = "TransportEquipmentUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(711, 437, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.EquipmentsGroupBoxPanel.ResumeLayout(false);
			this.EquipmentsGroupBoxPanel.PerformLayout();
			this.EquipmentsGroupBox.ResumeLayout(false);
			this.EquipmentsGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.EquipmentsGrid)).EndInit();
			this.EquipmentsGrid.ResumeLayout(false);
			this.EquipmentsGrid.PerformLayout();
			this.SealsGroupBoxPanel.ResumeLayout(false);
			this.SealsGroupBoxPanel.PerformLayout();
			this.SealsGroupBox.ResumeLayout(false);
			this.SealsGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.SealsGrid)).EndInit();
			this.SealsGrid.ResumeLayout(false);
			this.SealsGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		ZArchitecture.GUI.ZPanel EquipmentsGroupBoxPanel;
		internal ZArchitecture.GUI.ZGroupBox EquipmentsGroupBox;
		internal ZArchitecture.ZGrid EquipmentsGrid;
		ZArchitecture.GUI.ZPanel SealsGroupBoxPanel;
		internal ZArchitecture.GUI.ZGroupBox SealsGroupBox;
		internal ZArchitecture.ZGrid SealsGrid;
		CargoWise.Windows.UI.KSplitter Splitter;
	}
}
