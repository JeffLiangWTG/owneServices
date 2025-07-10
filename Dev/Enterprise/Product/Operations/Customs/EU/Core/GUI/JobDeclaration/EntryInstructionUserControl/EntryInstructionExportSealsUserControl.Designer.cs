namespace Enterprise.Customs.EU.GUI
{
	public partial class EntryInstructionExportSealsUserControl
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

		void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.SealsPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.SealsTotalCountCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.SealsGridGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.SealsGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SealsPanel.SuspendLayout();
			this.SealsGridGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.SealsGrid)).BeginInit();
			this.SealsGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.Business.Declaration.JobDeclaration);
			// 
			// SealsPanel
			// 
			this.SealsPanel.AutoScroll = true;
			this.SealsPanel.Controls.Add(this.SealsTotalCountCalcEdit);
			this.SealsPanel.Controls.Add(this.SealsGridGroupBox);
			this.SealsPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.SealsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SealsPanel.Name = "SealsPanel";
			this.SealsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1097, 235, true);
			this.SealsPanel.TabIndex = 6;
			// 
			// SealsTotalCountCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.SealsTotalCountCalcEdit, "CustomsEntryInstructions.ZG_SealsCount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.Business.Declaration.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).ZG_SealsCount)));
			this.SealsTotalCountCalcEdit.CaptionResourceString = null;
			this.SealsTotalCountCalcEdit.DecimalPlaces = 2;
			this.SealsTotalCountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(150, 7, true);
			this.SealsTotalCountCalcEdit.Name = "SealsTotalCountCalcEdit";
			this.SealsTotalCountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 18, true);
			this.SealsTotalCountCalcEdit.TabIndex = 1;
			this.SealsTotalCountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// SealsGridGroupBox
			// 
			this.SealsGridGroupBox.Controls.Add(this.SealsGrid);
			this.SealsGridGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 30, true);
			this.SealsGridGroupBox.Name = "SealsGridGroupBox";
			this.SealsGridGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 180, true);
			this.SealsGridGroupBox.TabIndex = 2;
			this.SealsGridGroupBox.TabStop = false;
			// 
			// SealsGrid
			// 
			this.SealsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.SealsGrid, "CustomsEntryInstructions.Seals");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).Seals)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.SealNumber)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).Seals)).SyncRoot)).CY_Data)));
			this.SealsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("941EDBC3-0B28-4708-8B0D-AC3B861F4104", "Seal Number");
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "CY_Data";
			zTextBoxColumnStyleInfo1.IsMandatory = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(210);
			this.SealsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.SealsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SealsGrid.GridId = "a258c3d7-3a24-4f20-803d-68b053562ef8";
			this.SealsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.SealsGrid.LayoutKey = "zGrid1";
			this.SealsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 14, true);
			this.SealsGrid.Name = "SealsGrid";
			this.SealsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(245, 163, true);
			this.SealsGrid.TabIndex = 2;
			// 
			// EntryInstructionExportSealsUserControl
			// 
			this.Controls.Add(this.SealsPanel);
			this.Name = "EntryInstructionExportSealsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1097, 237, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.SealsPanel.ResumeLayout(false);
			this.SealsPanel.PerformLayout();
			this.SealsGridGroupBox.ResumeLayout(false);
			this.SealsGridGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.SealsGrid)).EndInit();
			this.SealsGrid.ResumeLayout(false);
			this.SealsGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		protected ZArchitecture.GUI.ZPanel SealsPanel;
		protected ZArchitecture.ZCalcEdit SealsTotalCountCalcEdit;
		protected Enterprise.ZArchitecture.ZGrid SealsGrid;
		protected Enterprise.ZArchitecture.GUI.ZGroupBox SealsGridGroupBox;
	}
}
