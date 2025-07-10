namespace Enterprise.Customs.BR.GUI
{
	partial class AttributesUserControl
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
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.Customs.BR.GUI.AttributeMultiControlColumnStyleInfo attributeMultiControlColumnStyleInfo1 = new Enterprise.Customs.BR.GUI.AttributeMultiControlColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			this.AttributesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.AttributesGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.AttributesGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.AttributesGrid)).BeginInit();
			this.AttributesGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.BR.Business.AttributeCusCodeDataCollection);
			// 
			// AttributesGroupBox
			// 
			this.AttributesGroupBox.CaptionResourceString = Enterprise.Customs.BR.GUI.Res.GetData("BCAB60A0-C63D-4917-B1F8-C74DEB0F1769", "Attributes");
			this.AttributesGroupBox.Controls.Add(this.AttributesGrid);
			this.AttributesGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AttributesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.AttributesGroupBox.Name = "AttributesGroupBox";
			this.AttributesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(846, 228, true);
			this.AttributesGroupBox.TabIndex = 5;
			this.AttributesGroupBox.TabStop = false;
			// 
			// AttributesGrid
			// 
			this.AttributesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.AttributesGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.BR.Business.AttributeCusCodeData)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.BR.Business.AttributeCusCodeData)(null)).TaxType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.BR.Business.AttributeCusCodeData)(null)).LegalBase)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.BR.Business.AttributeCusCodeData)(null)).CY_Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.BR.Business.AttributeCusCodeData)(null)).Label)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.BR.Business.AttributeCusCodeData)(null)).CY_DataDecimalPlaces)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.BR.Business.AttributeCusCodeData)(null)).Content)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.BR.Business.AttributeCusCodeData)(null)).CY_DataFieldType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.BR.Business.AttributeCusCodeData)(null)).IsMandatory)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.BR.Business.AttributeCusCodeData)(null)).FillOrientation)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.BR.Business.AttributeCusCodeData)(null)).Example)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.BR.Business.AttributeCusCodeData)(null)).ParentAttributeCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.BR.Business.AttributeCusCodeData)(null)).ConditionDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.BR.Business.AttributeCusCodeData)(null)).StartDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.BR.Business.AttributeCusCodeData)(null)).EndDate)));
			this.AttributesGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.ColumnName = "TaxType";
			zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo2.ColumnName = "LegalBase";
			zTextBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo3.ColumnName = "CY_Code";
			zTextBoxColumnStyleInfo3.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(85);
			zTextBoxColumnStyleInfo4.ColumnName = "Label";
			zTextBoxColumnStyleInfo4.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			attributeMultiControlColumnStyleInfo1.BindToDecimalPlaces = "CY_DataDecimalPlaces";
			attributeMultiControlColumnStyleInfo1.ColumnName = "Content";
			attributeMultiControlColumnStyleInfo1.DefaultCollectionIndex = 0;
			attributeMultiControlColumnStyleInfo1.FieldTypeColumnName = "CY_DataFieldType";
			attributeMultiControlColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(250);
			zCheckBoxColumnStyleInfo1.ColumnName = "IsMandatory";
			zCheckBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			zTextBoxColumnStyleInfo5.ColumnName = "FillOrientation";
			zTextBoxColumnStyleInfo5.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zTextBoxColumnStyleInfo6.ColumnName = "Example";
			zTextBoxColumnStyleInfo6.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo7.ColumnName = "ParentAttributeCode";
			zTextBoxColumnStyleInfo7.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo8.ColumnName = "ConditionDescription";
			zTextBoxColumnStyleInfo8.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zDateEditColumnStyleInfo1.ColumnName = "StartDate";
			zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo2.ColumnName = "EndDate";
			zDateEditColumnStyleInfo2.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo2.DefaultCollectionIndex = 0;
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.AttributesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.AttributesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.AttributesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.AttributesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.AttributesGrid.ColumnStyles.Add(attributeMultiControlColumnStyleInfo1);
			this.AttributesGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.AttributesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.AttributesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.AttributesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.AttributesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.AttributesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.AttributesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.AttributesGrid.DisableImportDataMenuItem = true;
			this.AttributesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AttributesGrid.GridId = "eeedf854-f51f-48f7-ab14-125c113d6a00";
			this.AttributesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.AttributesGrid.LayoutKey = "AttributesGrid";
			this.AttributesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 14, true);
			this.AttributesGrid.Name = "AttributesGrid";
			this.AttributesGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.AttributesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(841, 211, true);
			this.AttributesGrid.TabIndex = 2;
			// 
			// AttributesUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.AttributesGroupBox);
			this.Name = "AttributesUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(846, 228, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.AttributesGroupBox.ResumeLayout(false);
			this.AttributesGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.AttributesGrid)).EndInit();
			this.AttributesGrid.ResumeLayout(false);
			this.AttributesGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.GUI.ZGroupBox AttributesGroupBox;
		internal ZArchitecture.ZGrid AttributesGrid;
	}
}
