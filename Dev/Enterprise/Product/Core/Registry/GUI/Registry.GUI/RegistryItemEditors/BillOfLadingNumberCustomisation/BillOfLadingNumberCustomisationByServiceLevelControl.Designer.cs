using CargoWise.Types;
namespace Enterprise.Registry.GUI
{
	internal partial class BillOfLadingNumberCustomisationByServiceLevelControl
	{
		void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZGroupBox codeElementsPositionGroupBox;
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.elementsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.HouseBillNumberByServiceLevelGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.CustomisationControl = new Enterprise.Registry.GUI.BillOfLadingNumberCustomisationControl();
			this.splitContainer1 = new CargoWise.Windows.UI.KSplitContainer();
			this.CustomiseByServiceLevelCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.zPanel1 = new Enterprise.ZArchitecture.GUI.ZPanel();
			codeElementsPositionGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			codeElementsPositionGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.elementsGrid)).BeginInit();
			this.HouseBillNumberByServiceLevelGroupBox.SuspendLayout();
			this.splitContainer1.Panel1.SuspendLayout();
			this.splitContainer1.Panel2.SuspendLayout();
			this.splitContainer1.SuspendLayout();
			this.zPanel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Registry.Business.BillOfLadingNumberCustomisationsByServiceLevel);
			// 
			// codeElementsPositionGroupBox
			// 
			codeElementsPositionGroupBox.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("BillOfLadingNumberCustomisationByServiceLevelControl|929ca15a-6656-4c6d-bb2e-4b76d39ba06e", "Service Levels");
			codeElementsPositionGroupBox.Controls.Add(this.elementsGrid);
			codeElementsPositionGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(codeElementsPositionGroupBox, false);
			codeElementsPositionGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			codeElementsPositionGroupBox.Name = "codeElementsPositionGroupBox";
			codeElementsPositionGroupBox.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(5, 3, 5, 5, true);
			codeElementsPositionGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(667, 78, true);
			codeElementsPositionGroupBox.TabIndex = 2;
			codeElementsPositionGroupBox.TabStop = false;
			// 
			// elementsGrid
			// 
			this.elementsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.elementsGrid, "BillOfLadingNumberCustomisations");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.BillOfLadingNumberCustomisationsByServiceLevel)(null)).BillOfLadingNumberCustomisations)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.BillOfLadingNumberCustomisation)(((System.Collections.IList)(((Enterprise.Registry.Business.BillOfLadingNumberCustomisationsByServiceLevel)(null)).BillOfLadingNumberCustomisations)).SyncRoot)).ServiceLevel)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.BillOfLadingNumberCustomisation)(((System.Collections.IList)(((Enterprise.Registry.Business.BillOfLadingNumberCustomisationsByServiceLevel)(null)).BillOfLadingNumberCustomisations)).SyncRoot)).RefServiceLevel.RS_Description)));
			this.elementsGrid.CaptionVisible = false;
			zCodeFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("BillOfLadingNumberCustomisationByServiceLevelControl|054923c0-0f34-4f7a-971e-95278c57421d", "Service Level");
			zCodeFindBoxColumnStyleInfo1.ColumnName = "ServiceLevel";
			zCodeFindBoxColumnStyleInfo1.IsMandatory = true;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("BillOfLadingNumberCustomisationByServiceLevelControl|c842a820-6a8c-4ab1-a789-09a68b7ae70f", "Description", "Service Level Description.");
			zTextBoxColumnStyleInfo1.ColumnName = "RefServiceLevel+RS_Description";
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140);
			this.elementsGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.elementsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.elementsGrid.GridId = "988fbfa5-8271-45f2-8467-a1c5886f6add";
			this.elementsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.elementsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.elementsGrid.LayoutKey = "panel1";
			this.elementsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 16, true);
			this.elementsGrid.Name = "elementsGrid";
			this.elementsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(657, 57, true);
			this.elementsGrid.TabIndex = 0;
			// 
			// HouseBillNumberByServiceLevelGroupBox
			// 
			this.HouseBillNumberByServiceLevelGroupBox.Controls.Add(this.CustomisationControl);
			this.HouseBillNumberByServiceLevelGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.HouseBillNumberByServiceLevelGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.HouseBillNumberByServiceLevelGroupBox.Name = "HouseBillNumberByServiceLevelGroupBox";
			this.HouseBillNumberByServiceLevelGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(667, 231, true);
			this.HouseBillNumberByServiceLevelGroupBox.TabIndex = 0;
			this.HouseBillNumberByServiceLevelGroupBox.TabStop = false;
			this.HouseBillNumberByServiceLevelGroupBox.Text = "Bill Of Lading {3} Bill Number Customization {0}";
			// 
			// CustomisationControl
			// 
			this.BindingSource.SetBindingMember(this.CustomisationControl, "BillOfLadingNumberCustomisations");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Registry.Business.BillOfLadingNumberCustomisation)(((Enterprise.Registry.Business.BillOfLadingNumberCustomisation)(((System.Collections.IList)(((Enterprise.Registry.Business.BillOfLadingNumberCustomisationsByServiceLevel)(null)).BillOfLadingNumberCustomisations)).SyncRoot)))));
			this.CustomisationControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CustomisationControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.CustomisationControl.Name = "CustomisationControl";
			this.CustomisationControl.ReadOnly = false;
			this.CustomisationControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(661, 212, true);
			this.CustomisationControl.TabIndex = 0;
			// 
			// splitContainer1
			// 
			this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 23, true);
			this.splitContainer1.Name = "splitContainer1";
			this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// splitContainer1.Panel1
			// 
			this.splitContainer1.Panel1.Controls.Add(codeElementsPositionGroupBox);
			// 
			// splitContainer1.Panel2
			// 
			this.splitContainer1.Panel2.Controls.Add(this.HouseBillNumberByServiceLevelGroupBox);
			this.splitContainer1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(667, 313, true);
			this.splitContainer1.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(78);
			this.splitContainer1.TabIndex = 1;
			// 
			// CustomiseByServiceLevelCheckBox
			// 
			this.CustomiseByServiceLevelCheckBox.AutoSize = true;
			this.CustomiseByServiceLevelCheckBox.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("BillOfLadingNumberCustomisationByServiceLevelControl|363e8f01-4543-4bc7-9e82-e0880d68e9c0", "Customize By Service Level");
			this.CustomiseByServiceLevelCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.CustomiseByServiceLevelCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.CustomiseByServiceLevelCheckBox.Name = "CustomiseByServiceLevelCheckBox";
			this.CustomiseByServiceLevelCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(157, 17, true);
			this.CustomiseByServiceLevelCheckBox.TabIndex = 1;
			this.CustomiseByServiceLevelCheckBox.UseVisualStyleBackColor = true;
			this.CustomiseByServiceLevelCheckBox.CheckedChanged += new System.EventHandler(this.CustomiseByServiceLevelCheckBox_CheckedChanged);
			// 
			// zPanel1
			// 
			this.zPanel1.Controls.Add(this.CustomiseByServiceLevelCheckBox);
			this.zPanel1.Dock = System.Windows.Forms.DockStyle.Top;
			this.zPanel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.zPanel1.Name = "zPanel1";
			this.zPanel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(667, 23, true);
			this.zPanel1.TabIndex = 2;
			// 
			// BillOfLadingNumberCustomisationByServiceLevelControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.splitContainer1);
			this.Controls.Add(this.zPanel1);
			this.Name = "BillOfLadingNumberCustomisationByServiceLevelControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(667, 336, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			codeElementsPositionGroupBox.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.elementsGrid)).EndInit();
			this.HouseBillNumberByServiceLevelGroupBox.ResumeLayout(false);
			this.splitContainer1.Panel1.ResumeLayout(false);
			this.splitContainer1.Panel2.ResumeLayout(false);
			this.splitContainer1.ResumeLayout(false);
			this.zPanel1.ResumeLayout(false);
			this.zPanel1.PerformLayout();
			this.ResumeLayout(false);

		}
		private Enterprise.ZArchitecture.GUI.ZGroupBox HouseBillNumberByServiceLevelGroupBox;
		private Enterprise.ZArchitecture.ZGrid elementsGrid;
		private CargoWise.Windows.UI.KSplitContainer splitContainer1;
		protected BillOfLadingNumberCustomisationControl CustomisationControl;
		private Enterprise.ZArchitecture.GUI.ZCheckBox CustomiseByServiceLevelCheckBox;
		private Enterprise.ZArchitecture.GUI.ZPanel zPanel1;
	}
}
