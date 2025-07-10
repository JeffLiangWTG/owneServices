namespace Enterprise.Customs.CA.GUI
{
	partial class RemissionsUserControl
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
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZMultiControlColumnStyleInfo zMultiControlColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZMultiControlColumnStyleInfo();
			this.Tariff99CodeFindBox = new Enterprise.Customs.GUI.TariffFindBox();
			this.CalculationMethodDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.SplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.AuthorityNumberCodeFindBox = new ZRulingFindBox();
			this.RemissionDescriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.TRSNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.RemissionTypeDropEdit = new ZArchitecture.GUI.ZDropEdit();
			this.ConfigurationsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ConfigurationsGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.Tariff99CodeFindBox.SuspendLayout();
			this.CalculationMethodDropEdit.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.SplitContainer)).BeginInit();
			this.SplitContainer.Panel1.SuspendLayout();
			this.SplitContainer.Panel2.SuspendLayout();
			this.SplitContainer.SuspendLayout();
			this.AuthorityNumberCodeFindBox.SuspendLayout();
			this.RemissionTypeDropEdit.SuspendLayout();
			this.ConfigurationsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ConfigurationsGrid)).BeginInit();
			this.ConfigurationsGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.CA.Business.JobComInvoiceLine);
			// 
			// Tariff99CodeFindBox
			// 
			this.Tariff99CodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.Tariff99CodeFindBox, "CA_99TariffCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.JobComInvoiceLine)(null)).CA_99TariffCode)));
			this.Tariff99CodeFindBox.BindToTariffPropertyInfo = null;
			this.Tariff99CodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 10, true);
			this.Tariff99CodeFindBox.Name = "Tariff99CodeFindBox";
			this.Tariff99CodeFindBox.ShouldResize = true;
			this.Tariff99CodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(411, 20, true);
			this.Tariff99CodeFindBox.TabIndex = 1;
			// 
			// CalculationMethodDropEdit
			// 
			this.CalculationMethodDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CalculationMethodDropEdit, "CA_CalculationMethod");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CA.Business.JobComInvoiceLine)(null)).CA_CalculationMethod)));
			this.CalculationMethodDropEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("da5b8c33-e374-4101-84a5-ddf81b744d67", "Type");
			this.CalculationMethodDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(278, 32, true);
			this.CalculationMethodDropEdit.Name = "CalculationMethodDropEdit";
			this.CalculationMethodDropEdit.PreBoundMaxLength = 1;
			this.CalculationMethodDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(221, 20, true);
			this.CalculationMethodDropEdit.TabIndex = 3;
			// 
			// SplitContainer
			// 
			this.SplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SplitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
			this.SplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SplitContainer.Name = "SplitContainer";
			this.SplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// SplitContainer.Panel1
			// 
			this.SplitContainer.Panel1.Controls.Add(this.Tariff99CodeFindBox);
			this.SplitContainer.Panel1.Controls.Add(this.CalculationMethodDropEdit);
			this.SplitContainer.Panel1.Controls.Add(this.AuthorityNumberCodeFindBox);
			this.SplitContainer.Panel1.Controls.Add(this.RemissionDescriptionTextBox);
			this.SplitContainer.Panel1.Controls.Add(this.TRSNumberTextBox);
			this.SplitContainer.Panel1.Controls.Add(this.RemissionTypeDropEdit);
			this.SplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(980, 254, true);
			this.SplitContainer.Panel1MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(80);
			// 
			// SplitContainer.Panel2
			// 
			this.SplitContainer.Panel2.Controls.Add(this.ConfigurationsGroupBox);
			this.SplitContainer.Panel2MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(100);
			this.SplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(89);
			this.SplitContainer.TabIndex = 2;
			this.SplitContainer.TabStop = false;
			// 
			// AuthorityNumberCodeFindBox
			// 
			this.AuthorityNumberCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AuthorityNumberCodeFindBox, "CA_AuthorityNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.JobComInvoiceLine)(null)).CA_AuthorityNumber)));
			this.AuthorityNumberCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 32, true);
			this.AuthorityNumberCodeFindBox.Name = "AuthorityNumberCodeFindBox";
			this.AuthorityNumberCodeFindBox.PreBoundMaxLength = 16;
			this.AuthorityNumberCodeFindBox.ShouldResize = true;
			this.AuthorityNumberCodeFindBox.ShowDescriptionBox = false;
			this.AuthorityNumberCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(137, 20, true);
			this.AuthorityNumberCodeFindBox.TabIndex = 2;
			// 
			// RemissionDescriptionTextBox
			//
			this.RemissionDescriptionTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.RemissionDescriptionTextBox, "RulingDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.JobComInvoiceLine)(null)).RulingDescription)));
			this.RemissionDescriptionTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("0afe1966-c451-4800-8a07-bebbe0605dbe", "Remission Description");
			this.RemissionDescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(584, 10, true);
			this.RemissionDescriptionTextBox.Multiline = true;
			this.RemissionDescriptionTextBox.Name = "RemissionDescriptionTextBox";
			this.RemissionDescriptionTextBox.ReadOnly = true;
			this.RemissionDescriptionTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.RemissionDescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(385, 65, true);
			this.RemissionDescriptionTextBox.TabIndex = 5;
			// 
			// TRSNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.TRSNumberTextBox, "CA_TRSNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.JobComInvoiceLine)(null)).CA_TRSNumber)));
			this.TRSNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 55, true);
			this.TRSNumberTextBox.Name = "TRSNumberTextBox";
			this.TRSNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(411, 20, true);
			this.TRSNumberTextBox.TabIndex = 4;
			// 
			// RemissionTypeDropEdit
			// 
			this.BindingSource.SetBindingMember(this.RemissionTypeDropEdit, "CA_RemissionType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.JobComInvoiceLine)(null)).CA_RemissionType)));
			this.RemissionTypeDropEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("8DB1AFC8-B588-4AB4-B3BE-10D9BD1AE5CC", "Special Authority Type");
			this.RemissionTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(130, 55, true);
			this.RemissionTypeDropEdit.Name = "RemissionTypeDropEdit";
			this.RemissionTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(350, 20, true);
			this.RemissionTypeDropEdit.TabIndex = 4;
			// 
			// ConfigurationsGroupBox
			// 
			this.ConfigurationsGroupBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("562e7d23-92a1-43fd-b342-1727b21b4361", "Configurations");
			this.ConfigurationsGroupBox.Controls.Add(this.ConfigurationsGrid);
			this.ConfigurationsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ConfigurationsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ConfigurationsGroupBox.Name = "ConfigurationsGroupBox";
			this.ConfigurationsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(980, 161, true);
			this.ConfigurationsGroupBox.TabIndex = 3;
			this.ConfigurationsGroupBox.TabStop = false;
			// 
			// ConfigurationsGrid
			// 
			this.ConfigurationsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ConfigurationsGrid, "RulingConfigurations");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobComInvoiceLine)(null)).RulingConfigurations)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CACusRulingConfig)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobComInvoiceLine)(null)).RulingConfigurations)).SyncRoot)).ZZY_Category)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CACusRulingConfig)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobComInvoiceLine)(null)).RulingConfigurations)).SyncRoot)).ZZY_Type)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CA.Business.CACusRulingConfig)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobComInvoiceLine)(null)).RulingConfigurations)).SyncRoot)).ZZY_Rate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CA.Business.CACusRulingConfig)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobComInvoiceLine)(null)).RulingConfigurations)).SyncRoot)).ZZY_ValueDecimalPlaces)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CACusRulingConfig)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobComInvoiceLine)(null)).RulingConfigurations)).SyncRoot)).ZZY_Value)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CACusRulingConfig)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobComInvoiceLine)(null)).RulingConfigurations)).SyncRoot)).ValueFieldType)));
			this.ConfigurationsGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("99b1a426-f963-4533-9ef3-6dca3281c8ed", "Category");
			zDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo1.ColumnName = "ZZY_Category";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(83);
			zDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("6e5dffd4-6c31-4453-976c-ad3a0ffe99ba", "Type");
			zDropEditColumnStyleInfo2.ColumnName = "ZZY_Type";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(83);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("b03aaf50-cf58-4832-8ced-c225bc1fb179", "Rate(%)");
			zCalcEditColumnStyleInfo1.ColumnName = "ZZY_Rate";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zMultiControlColumnStyleInfo1.BindToDecimalPlaces = "ZZY_ValueDecimalPlaces";
			zMultiControlColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("6c3f0591-26f0-408b-a79a-e04006137b63", "Value");
			zMultiControlColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zMultiControlColumnStyleInfo1.ColumnName = "ZZY_Value";
			zMultiControlColumnStyleInfo1.FieldTypeColumnName = "ValueFieldType";
			zMultiControlColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			this.ConfigurationsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.ConfigurationsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.ConfigurationsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.ConfigurationsGrid.ColumnStyles.Add(zMultiControlColumnStyleInfo1);
			this.ConfigurationsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ConfigurationsGrid.GridId = "9A4DBB59-ED04-4905-A737-B3DB8D34939B";
			this.ConfigurationsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ConfigurationsGrid.LayoutKey = "ConfigurationsGrid";
			this.ConfigurationsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.ConfigurationsGrid.Name = "ConfigurationsGrid";
			this.ConfigurationsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(974, 142, true);
			this.ConfigurationsGrid.TabIndex = 1;
			// 
			// RemissionsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.SplitContainer);
			this.Name = "RemissionsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(980, 254, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.Tariff99CodeFindBox.ResumeLayout(true);
			this.Tariff99CodeFindBox.PerformLayout();
			this.CalculationMethodDropEdit.ResumeLayout(true);
			this.CalculationMethodDropEdit.PerformLayout();
			this.SplitContainer.Panel1.ResumeLayout(false);
			this.SplitContainer.Panel1.PerformLayout();
			this.SplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.SplitContainer)).EndInit();
			this.SplitContainer.ResumeLayout(false);
			this.SplitContainer.PerformLayout();
			this.AuthorityNumberCodeFindBox.ResumeLayout(true);
			this.AuthorityNumberCodeFindBox.PerformLayout();
			this.RemissionTypeDropEdit.ResumeLayout(true);
			this.RemissionTypeDropEdit.PerformLayout();
			this.ConfigurationsGroupBox.ResumeLayout(false);
			this.ConfigurationsGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ConfigurationsGrid)).EndInit();
			this.ConfigurationsGrid.ResumeLayout(false);
			this.ConfigurationsGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		Enterprise.Customs.GUI.TariffFindBox Tariff99CodeFindBox;
		CargoWise.Windows.UI.KSplitContainer SplitContainer;
		ZArchitecture.GUI.ZDropEdit CalculationMethodDropEdit;
		ZRulingFindBox AuthorityNumberCodeFindBox;
		Enterprise.ZArchitecture.ZTextBox TRSNumberTextBox;
		ZArchitecture.ZTextBox RemissionDescriptionTextBox;
		ZArchitecture.GUI.ZDropEdit RemissionTypeDropEdit;
		Enterprise.ZArchitecture.GUI.ZGroupBox ConfigurationsGroupBox;
		Enterprise.ZArchitecture.ZGrid ConfigurationsGrid;

		#endregion
	}
}
