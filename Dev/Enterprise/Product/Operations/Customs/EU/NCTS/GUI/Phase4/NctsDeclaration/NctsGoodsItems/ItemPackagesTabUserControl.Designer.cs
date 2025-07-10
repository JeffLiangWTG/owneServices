namespace Enterprise.Customs.EU.NCTS.GUI
{
	partial class ItemPackagesTabUserControl
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
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.PackagesSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.PackagesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.PackagesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.MarksAndNumbersTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.NumberOfPackagesCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.PackageTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.PackagesSplitContainer)).BeginInit();
			this.PackagesSplitContainer.Panel1.SuspendLayout();
			this.PackagesSplitContainer.Panel2.SuspendLayout();
			this.PackagesSplitContainer.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.PackagesGrid)).BeginInit();
			this.PackagesGrid.SuspendLayout();
			this.PackagesGroupBox.SuspendLayout();
			this.PackageTypeDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.NCTS.Business.NctsCommonCargoDesc);
			// 
			// PackagesSplitContainer
			// 
			this.PackagesSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PackagesSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.PackagesSplitContainer.Name = "PackagesSplitContainer";
			this.PackagesSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// PackagesSplitContainer.Panel1
			// 
			this.PackagesSplitContainer.Panel1.Controls.Add(this.PackagesGrid);
			// 
			// PackagesSplitContainer.Panel2
			// 
			this.PackagesSplitContainer.Panel2.Controls.Add(this.PackagesGroupBox);
			this.PackagesSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(687, 389, true);
			this.PackagesSplitContainer.Panel2MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(105);
			this.PackagesSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(280);
			this.PackagesSplitContainer.TabIndex = 0;
			// 
			// PackagesGrid
			// 
			this.PackagesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.PackagesGrid, "Packages");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.NctsCommonCargoDesc)(null)).Packages)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.NctsPackage)(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.NctsCommonCargoDesc)(null)).Packages)).SyncRoot)).B5_UnitType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.NCTS.Business.NctsPackage)(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.NctsCommonCargoDesc)(null)).Packages)).SyncRoot)).B5_UnitCount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.NctsPackage)(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.NctsCommonCargoDesc)(null)).Packages)).SyncRoot)).B5_MarksAndNumbers)));
			this.PackagesGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.Caption = "";
			zDropEditColumnStyleInfo1.ColumnName = "B5_UnitType";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "B5_UnitCount";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			zTextBoxColumnStyleInfo1.ColumnName = "B5_MarksAndNumbers";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(340);
			this.PackagesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.PackagesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.PackagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.PackagesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PackagesGrid.GridId = "1afd7f99-62b9-4a2b-a6fe-c9c9250710fc";
			this.PackagesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.PackagesGrid.LayoutKey = "zGrid1";
			this.PackagesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.PackagesGrid.Name = "PackagesGrid";
			this.PackagesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(687, 280, true);
			this.PackagesGrid.TabIndex = 0;
			// 
			// PackagesGroupBox
			// 
			this.PackagesGroupBox.Controls.Add(this.MarksAndNumbersTextBox);
			this.PackagesGroupBox.Controls.Add(this.NumberOfPackagesCalcEdit);
			this.PackagesGroupBox.Controls.Add(this.PackageTypeDropEdit);
			this.PackagesGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.PackagesGroupBox, false);
			this.PackagesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.PackagesGroupBox.Name = "PackagesGroupBox";
			this.PackagesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(687, 110, true);
			this.PackagesGroupBox.TabIndex = 0;
			this.PackagesGroupBox.TabStop = false;
			// 
			// MarksAndNumbersTextBox
			// 
			this.BindingSource.SetBindingMember(this.MarksAndNumbersTextBox, "Packages.B5_MarksAndNumbers");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.NctsPackage)(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.NctsCommonCargoDesc)(null)).Packages)).SyncRoot)).B5_MarksAndNumbers)));
			this.MarksAndNumbersTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.MarksAndNumbersTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(119, 71, true);
			this.MarksAndNumbersTextBox.Name = "MarksAndNumbersTextBox";
			this.MarksAndNumbersTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(460, 20, true);
			this.MarksAndNumbersTextBox.TabIndex = 3;
			// 
			// NumberOfPackagesCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.NumberOfPackagesCalcEdit, "Packages.B5_UnitCount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.NCTS.Business.NctsPackage)(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.NctsCommonCargoDesc)(null)).Packages)).SyncRoot)).B5_UnitCount)));
			this.NumberOfPackagesCalcEdit.DecimalPlaces = 0;
			this.NumberOfPackagesCalcEdit.Decimals = 0;
			this.NumberOfPackagesCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(119, 45, true);
			this.NumberOfPackagesCalcEdit.Name = "NumberOfPackagesCalcEdit";
			this.NumberOfPackagesCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(460, 20, true);
			this.NumberOfPackagesCalcEdit.TabIndex = 1;
			this.NumberOfPackagesCalcEdit.Text = "0";
			this.NumberOfPackagesCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// PackageTypeDropEdit
			// 
			this.PackageTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PackageTypeDropEdit, "Packages.B5_UnitType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.NCTS.Business.NctsPackage)(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.NctsCommonCargoDesc)(null)).Packages)).SyncRoot)).B5_UnitType)));
			this.PackageTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(119, 19, true);
			this.PackageTypeDropEdit.Name = "PackageTypeDropEdit";
			this.PackageTypeDropEdit.PreBoundMaxLength = 2;
			this.PackageTypeDropEdit.ShouldResizeByMaxLength = true;
			this.PackageTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(460, 20, true);
			this.PackageTypeDropEdit.TabIndex = 0;
			// 
			// ItemPackagesTabUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.PackagesSplitContainer);
			this.Name = "ItemPackagesTabUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(687, 389, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.PackagesSplitContainer.Panel1.ResumeLayout(false);
			this.PackagesSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.PackagesSplitContainer)).EndInit();
			this.PackagesSplitContainer.ResumeLayout(false);
			this.PackagesSplitContainer.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.PackagesGrid)).EndInit();
			this.PackagesGrid.ResumeLayout(false);
			this.PackagesGrid.PerformLayout();
			this.PackagesGroupBox.ResumeLayout(false);
			this.PackagesGroupBox.PerformLayout();
			this.PackageTypeDropEdit.ResumeLayout(true);
			this.PackageTypeDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private CargoWise.Windows.UI.KSplitContainer PackagesSplitContainer;
		protected ZArchitecture.ZGrid PackagesGrid;
		protected ZArchitecture.GUI.ZGroupBox PackagesGroupBox;
		protected ZArchitecture.ZTextBox MarksAndNumbersTextBox;
		protected ZArchitecture.ZCalcEdit NumberOfPackagesCalcEdit;
		protected ZArchitecture.GUI.ZDropEdit PackageTypeDropEdit;
	}
}
