using Enterprise.Customs.GUI;

namespace Enterprise.Customs.EU.EMCS.GUI
{
	partial class EMCSInvoiceLinePackagesUserControl
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
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.PackagesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.PackagesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.UnitTypeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.UnitCountTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.MarksAndNumbersTextBox = new Enterprise.Customs.GUI.LongTextControl();
			this.DescriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ReferenceNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.PackagesGrid)).BeginInit();
			this.PackagesGrid.SuspendLayout();
			this.PackagesGroupBox.SuspendLayout();
			this.MarksAndNumbersTextBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.EMCS.Business.NonPersistentPackagePivot);
			// 
			// PackagesGrid
			// 
			this.PackagesGrid.AllowNavigation = false;
			this.PackagesGrid.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.PackagesGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.EU.EMCS.Business.NonPersistentPackagePivot)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.EU.EMCS.Business.NonPersistentPackagePivot)(null)).IsForInvoiceLine)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZLong)(((Enterprise.Customs.EU.EMCS.Business.NonPersistentPackagePivot)(null)).UnitCount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.EMCS.Business.NonPersistentPackagePivot)(null)).UnitType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.EMCS.Business.NonPersistentPackagePivot)(null)).MarksAndNumbers)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.EMCS.Business.NonPersistentPackagePivot)(null)).SealComment)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.EMCS.Business.NonPersistentPackagePivot)(null)).SealNumber)));
			this.PackagesGrid.CaptionVisible = false;
			zCheckBoxColumnStyleInfo1.ColumnName = "IsForInvoiceLine";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130);
			zTextBoxColumnStyleInfo1.ColumnName = "UnitCount";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo2.ColumnName = "UnitType";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo3.ColumnName = "MarksAndNumbers";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(325);
			zTextBoxColumnStyleInfo4.ColumnName = "SealComment";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zTextBoxColumnStyleInfo5.ColumnName = "SealNumber";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			this.PackagesGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.PackagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.PackagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.PackagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.PackagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.PackagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.PackagesGrid.GridId = "e0afb17c-6d43-49c8-a6d5-ae03948aa164";
			this.PackagesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.PackagesGrid.LayoutKey = "PackagesGrid";
			this.PackagesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 20, true);
			this.PackagesGrid.Name = "PackagesGrid";
			this.PackagesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(871, 153, true);
			this.PackagesGrid.TabIndex = 1;
			// 
			// PackagesGroupBox
			// 
			this.PackagesGroupBox.CaptionResourceString = Enterprise.Customs.EU.EMCS.GUI.Res.GetData("08a66f5b-b41a-4ef5-bddb-7149efbac937", "Packages");
			this.PackagesGroupBox.Controls.Add(this.UnitTypeTextBox);
			this.PackagesGroupBox.Controls.Add(this.UnitCountTextBox);
			this.PackagesGroupBox.Controls.Add(this.MarksAndNumbersTextBox);
			this.PackagesGroupBox.Controls.Add(this.DescriptionTextBox);
			this.PackagesGroupBox.Controls.Add(this.ReferenceNumberTextBox);
			this.PackagesGroupBox.Controls.Add(this.PackagesGrid);
			this.PackagesGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PackagesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.PackagesGroupBox.Name = "PackagesGroupBox";
			this.PackagesGroupBox.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(4, true);
			this.PackagesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(885, 309, true);
			this.PackagesGroupBox.TabIndex = 0;
			this.PackagesGroupBox.TabStop = false;
			// 
			// UnitTypeTextBox
			// 
			this.BindingSource.SetBindingMember(this.UnitTypeTextBox, "UnitType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.EMCS.Business.NonPersistentPackagePivot)(null)).UnitType)));
			this.UnitTypeTextBox.CaptionResourceString = null;
			this.UnitTypeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(404, 183, true);
			this.UnitTypeTextBox.Name = "UnitTypeTextBox";
			this.UnitTypeTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.UnitTypeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(43, 20, true);
			this.UnitTypeTextBox.TabIndex = 3;
			// 
			// UnitCountTextBox
			// 
			this.BindingSource.SetBindingMember(this.UnitCountTextBox, "UnitCount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZLong)(((Enterprise.Customs.EU.EMCS.Business.NonPersistentPackagePivot)(null)).UnitCount)));
			this.UnitCountTextBox.CaptionResourceString = Enterprise.Customs.EU.EMCS.GUI.Res.GetData("8ee69a5f-a492-4851-b5f4-d07f846862cb", "No. of Packages");
			this.UnitCountTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(113, 183, true);
			this.UnitCountTextBox.Name = "UnitCountTextBox";
			this.UnitCountTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.UnitCountTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(287, 20, true);
			this.UnitCountTextBox.TabIndex = 2;
			// 
			// MarksAndNumbersTextBox
			// 
			this.MarksAndNumbersTextBox.AllowDrop = true;
			this.MarksAndNumbersTextBox.CaptionResourceString = Enterprise.Customs.EU.EMCS.GUI.Res.GetData("157eb413-9ab1-434b-9f20-30fb71acb679", "Shipping Marks");
			this.MarksAndNumbersTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.MarksAndNumbersTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(113, 205, true);
			this.MarksAndNumbersTextBox.Name = "MarksAndNumbersTextBox";
			this.MarksAndNumbersTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(334, 20, true);
			this.MarksAndNumbersTextBox.TabIndex = 4;
			// 
			// DescriptionTextBox
			// 
			this.BindingSource.SetBindingMember(this.DescriptionTextBox, "SealComment");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.EMCS.Business.NonPersistentPackagePivot)(null)).SealComment)));
			this.DescriptionTextBox.CaptionResourceString = Enterprise.Customs.EU.EMCS.GUI.Res.GetData("330e1685-78b2-433b-a18e-7362be4b936a", "Seal Comment");
			this.DescriptionTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.DescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(113, 250, true);
			this.DescriptionTextBox.Multiline = true;
			this.DescriptionTextBox.Name = "DescriptionTextBox";
			this.DescriptionTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.DescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(334, 17, true);
			this.DescriptionTextBox.TabIndex = 6;
			// 
			// ReferenceNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.ReferenceNumberTextBox, "SealNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.EMCS.Business.NonPersistentPackagePivot)(null)).SealNumber)));
			this.ReferenceNumberTextBox.CaptionResourceString = Enterprise.Customs.EU.EMCS.GUI.Res.GetData("ac2ce3c9-e376-4070-ac87-44f8aa1a081e", "Seal Number");
			this.ReferenceNumberTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ReferenceNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(113, 229, true);
			this.ReferenceNumberTextBox.Name = "ReferenceNumberTextBox";
			this.ReferenceNumberTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.ReferenceNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(334, 20, true);
			this.ReferenceNumberTextBox.TabIndex = 5;
			// 
			// EMCSInvoiceLinePackagesUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.PackagesGroupBox);
			this.Name = "EMCSInvoiceLinePackagesUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(885, 309, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.PackagesGrid)).EndInit();
			this.PackagesGrid.ResumeLayout(false);
			this.PackagesGrid.PerformLayout();
			this.PackagesGroupBox.ResumeLayout(false);
			this.PackagesGroupBox.PerformLayout();
			this.MarksAndNumbersTextBox.ResumeLayout(true);
			this.MarksAndNumbersTextBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZGrid PackagesGrid;
		private ZArchitecture.GUI.ZGroupBox PackagesGroupBox;
		private ZArchitecture.ZTextBox DescriptionTextBox;
		private ZArchitecture.ZTextBox ReferenceNumberTextBox;
		private LongTextControl MarksAndNumbersTextBox;
		private ZArchitecture.ZTextBox UnitCountTextBox;
		private ZArchitecture.ZTextBox UnitTypeTextBox;
	}
}
