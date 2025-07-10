namespace Enterprise.Customs.CN.GUI
{
	partial class EnterpriseQualificationsUserControl
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
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			this.EnterpriseQualificationsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.XC_EnterprisePromisedCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.EnterpriseQualificationsGrid)).BeginInit();
			this.EnterpriseQualificationsGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.CN.Business.CusEntryInstruction);
			// 
			// EnterpriseQualificationsGrid
			// 
			this.EnterpriseQualificationsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.EnterpriseQualificationsGrid, "EnterpriseQualifications");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.CN.Business.CusEntryInstruction)(null)).EnterpriseQualifications)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CN.Business.EnterpriseQualification)(((System.Collections.IList)(((Enterprise.Customs.CN.Business.CusEntryInstruction)(null)).EnterpriseQualifications)).SyncRoot)).CY_Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CN.Business.EnterpriseQualification)(((System.Collections.IList)(((Enterprise.Customs.CN.Business.CusEntryInstruction)(null)).EnterpriseQualifications)).SyncRoot)).Description)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CN.Business.EnterpriseQualification)(((System.Collections.IList)(((Enterprise.Customs.CN.Business.CusEntryInstruction)(null)).EnterpriseQualifications)).SyncRoot)).CY_Data)));
			this.EnterpriseQualificationsGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.ColumnName = "CY_Code";
			zDropEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo1.ColumnName = "Description";
			zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180);
			zDropEditColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo2.ColumnName = "CY_Data";
			zDropEditColumnStyleInfo2.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			this.EnterpriseQualificationsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.EnterpriseQualificationsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.EnterpriseQualificationsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.EnterpriseQualificationsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.EnterpriseQualificationsGrid.GridId = "21712238-b61d-4502-85f7-1529a648097f";
			this.EnterpriseQualificationsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.EnterpriseQualificationsGrid.LayoutKey = "UpperGrid";
			this.EnterpriseQualificationsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.EnterpriseQualificationsGrid.Name = "EnterpriseQualificationsGrid";
			this.EnterpriseQualificationsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(387, 192, true);
			this.EnterpriseQualificationsGrid.TabIndex = 1;
			// 
			// XC_EnterprisePromisedCheckBox
			// 
			this.BindingSource.SetBindingMember(this.XC_EnterprisePromisedCheckBox, "CEI_EnterprisePromised");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.CN.Business.CusEntryInstruction)(null)).CEI_EnterprisePromised)));
			this.XC_EnterprisePromisedCheckBox.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.XC_EnterprisePromisedCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 192, true);
			this.XC_EnterprisePromisedCheckBox.Name = "XC_EnterprisePromisedCheckBox";
			this.XC_EnterprisePromisedCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(387, 58, true);
			this.XC_EnterprisePromisedCheckBox.TabIndex = 6;
			// 
			// EnterpriseQualificationsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.EnterpriseQualificationsGrid);
			this.Controls.Add(this.XC_EnterprisePromisedCheckBox);
			this.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, true);
			this.Name = "EnterpriseQualificationsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(387, 250, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.EnterpriseQualificationsGrid)).EndInit();
			this.EnterpriseQualificationsGrid.ResumeLayout(false);
			this.EnterpriseQualificationsGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Enterprise.ZArchitecture.ZGrid EnterpriseQualificationsGrid;
		private ZArchitecture.GUI.ZCheckBox XC_EnterprisePromisedCheckBox;
	}
}
