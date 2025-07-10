namespace Enterprise.BarcodeParsing.GUI
{
	partial class BarcodeRuleComponentsUserControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZMultiControlColumnStyleInfo zMultiControlColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZMultiControlColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.RuleComponentsGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.RuleComponentsGrid)).BeginInit();
			this.RuleComponentsGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.BarcodeParsing.Business.BarcodeRule);
			// 
			// RuleComponentsGrid
			// 
			this.RuleComponentsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.RuleComponentsGrid, "Components");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.BarcodeParsing.Business.BarcodeRule)(null)).Components)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.BarcodeParsing.Business.BarcodeRuleComponent)(((System.Collections.IList)(((Enterprise.BarcodeParsing.Business.BarcodeRule)(null)).Components)).SyncRoot)).BRC_Sequence)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.BarcodeParsing.Business.BarcodeRuleComponent)(((System.Collections.IList)(((Enterprise.BarcodeParsing.Business.BarcodeRule)(null)).Components)).SyncRoot)).BRC_ApplicationID)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.BarcodeParsing.Business.BarcodeRuleComponent)(((System.Collections.IList)(((Enterprise.BarcodeParsing.Business.BarcodeRule)(null)).Components)).SyncRoot)).ApplicationIDDataFieldType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.BarcodeParsing.Business.BarcodeRuleComponent)(((System.Collections.IList)(((Enterprise.BarcodeParsing.Business.BarcodeRule)(null)).Components)).SyncRoot)).HasDelimiter)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.BarcodeParsing.Business.BarcodeRuleComponent)(((System.Collections.IList)(((Enterprise.BarcodeParsing.Business.BarcodeRule)(null)).Components)).SyncRoot)).IsDelimiterMultiComponent)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.BarcodeParsing.Business.BarcodeRuleComponent)(((System.Collections.IList)(((Enterprise.BarcodeParsing.Business.BarcodeRule)(null)).Components)).SyncRoot)).DelimiterForBinding)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.BarcodeParsing.Business.BarcodeRuleComponent)(((System.Collections.IList)(((Enterprise.BarcodeParsing.Business.BarcodeRule)(null)).Components)).SyncRoot)).LengthTypeForBinding)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.BarcodeParsing.Business.BarcodeRuleComponent)(((System.Collections.IList)(((Enterprise.BarcodeParsing.Business.BarcodeRule)(null)).Components)).SyncRoot)).BRC_MinLength)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.BarcodeParsing.Business.BarcodeRuleComponent)(((System.Collections.IList)(((Enterprise.BarcodeParsing.Business.BarcodeRule)(null)).Components)).SyncRoot)).BRC_MaxLength)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.BarcodeParsing.Business.BarcodeRuleComponent)(((System.Collections.IList)(((Enterprise.BarcodeParsing.Business.BarcodeRule)(null)).Components)).SyncRoot)).BRC_Format)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.BarcodeParsing.Business.BarcodeRuleComponent)(((System.Collections.IList)(((Enterprise.BarcodeParsing.Business.BarcodeRule)(null)).Components)).SyncRoot)).BRC_TargetField)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.BarcodeParsing.Business.BarcodeRuleComponent)(((System.Collections.IList)(((Enterprise.BarcodeParsing.Business.BarcodeRule)(null)).Components)).SyncRoot)).TargetFieldDescription)));
			this.RuleComponentsGrid.CaptionVisible = false;
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "BRC_Sequence";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zMultiControlColumnStyleInfo1.ColumnName = "BRC_ApplicationID";
			zMultiControlColumnStyleInfo1.FieldTypeColumnName = "ApplicationIDDataFieldType";
			zMultiControlColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(78);
			zCheckBoxColumnStyleInfo1.ColumnName = "HasDelimiter";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo2.ColumnName = "IsDelimiterMultiComponent";
			zCheckBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo1.ColumnName = "DelimiterForBinding";
			zTextBoxColumnStyleInfo1.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo1.ColumnName = "LengthTypeForBinding";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.ColumnName = "BRC_MinLength";
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(66);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.ColumnName = "BRC_MaxLength";
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(66);
			zDropEditColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo2.ColumnName = "BRC_Format";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(55);
			zDropEditColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo3.ColumnName = "BRC_TargetField";
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.ColumnName = "TargetFieldDescription";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.RuleComponentsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.RuleComponentsGrid.ColumnStyles.Add(zMultiControlColumnStyleInfo1);
			this.RuleComponentsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.RuleComponentsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.RuleComponentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.RuleComponentsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.RuleComponentsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.RuleComponentsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.RuleComponentsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.RuleComponentsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.RuleComponentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.RuleComponentsGrid.CopySelectedRowsAllowed = true;
			this.RuleComponentsGrid.DataSource = this.BindingSource;
			this.RuleComponentsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.RuleComponentsGrid.GridId = "6455771c-1cb3-4998-a75f-46550118a791";
			this.RuleComponentsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.RuleComponentsGrid.LayoutKey = "RuleComponentsGrid";
			this.RuleComponentsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.RuleComponentsGrid.Name = "RuleComponentsGrid";
			this.RuleComponentsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(782, 249, true);
			this.RuleComponentsGrid.TabIndex = 1;
			// 
			// BarcodeRuleComponentsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.RuleComponentsGrid);
			this.Name = "BarcodeRuleComponentsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(782, 249, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.RuleComponentsGrid)).EndInit();
			this.RuleComponentsGrid.ResumeLayout(false);
			this.RuleComponentsGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		protected ZArchitecture.ZGrid RuleComponentsGrid;

	}
}
