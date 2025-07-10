namespace Enterprise.Customs.DE.GUI
{
	partial class OutwardProcessingUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.ReimportCountryGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ReimportCountryGrid = new Enterprise.ZArchitecture.ZGrid();
			this.IdentificationMeansGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.IdentificationMeansGrid = new Enterprise.ZArchitecture.ZGrid();
			this.ProductGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ProductGrid = new Enterprise.ZArchitecture.ZGrid();
			this.BottomGridPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ReimportCountryGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ReimportCountryGrid)).BeginInit();
			this.ReimportCountryGrid.SuspendLayout();
			this.IdentificationMeansGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.IdentificationMeansGrid)).BeginInit();
			this.IdentificationMeansGrid.SuspendLayout();
			this.ProductGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ProductGrid)).BeginInit();
			this.ProductGrid.SuspendLayout();
			this.BottomGridPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.DE.Business.Declaration.JobDeclaration);
			// 
			// ReimportCountryGroupBox
			// 
			this.ReimportCountryGroupBox.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("f51f02fe-891f-410f-98d6-e6220d410b5d", "Reimport Country/Region");
			this.ReimportCountryGroupBox.Controls.Add(this.ReimportCountryGrid);
			this.ReimportCountryGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.ReimportCountryGroupBox.Name = "ReimportCountryGroupBox";
			this.ReimportCountryGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(268, 159, true);
			this.ReimportCountryGroupBox.TabIndex = 0;
			this.ReimportCountryGroupBox.TabStop = false;
			// 
			// ReimportCountryGrid
			// 
			this.ReimportCountryGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ReimportCountryGrid, "CustomsEntryInstructions.ReimportCountryCodes");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).ReimportCountryCodes)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.ReimportCountryCode)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).ReimportCountryCodes)).SyncRoot)).CY_Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.ReimportCountryCode)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).ReimportCountryCodes)).SyncRoot)).CountryDescription)));
			this.ReimportCountryGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo1.ColumnName = "CY_Code";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("bdccf8fe-5973-4988-88bf-8a5e869913af", "Description");
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "CountryDescription";
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140);
			this.ReimportCountryGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.ReimportCountryGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ReimportCountryGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ReimportCountryGrid.GridId = "65f1cbfd-aaf0-4c42-9799-3a85507997fd";
			this.ReimportCountryGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ReimportCountryGrid.LayoutKey = "ReimportCountryGrid";
			this.ReimportCountryGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.ReimportCountryGrid.Name = "ReimportCountryGrid";
			this.ReimportCountryGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(262, 140, true);
			this.ReimportCountryGrid.TabIndex = 0;
			// 
			// IdentificationMeansGroupBox
			// 
			this.IdentificationMeansGroupBox.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("febc3cec-e740-4d74-9700-d2c25dfff7f4", "Identification Means");
			this.IdentificationMeansGroupBox.Controls.Add(this.IdentificationMeansGrid);
			this.IdentificationMeansGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(624, 3, true);
			this.IdentificationMeansGroupBox.Name = "IdentificationMeansGroupBox";
			this.IdentificationMeansGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(355, 159, true);
			this.IdentificationMeansGroupBox.TabIndex = 2;
			this.IdentificationMeansGroupBox.TabStop = false;
			// 
			// IdentificationMeansGrid
			// 
			this.IdentificationMeansGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.IdentificationMeansGrid, "CustomsEntryInstructions.IdentificationMeanCodes");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).IdentificationMeanCodes)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.IdentificationMeansCode)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).IdentificationMeanCodes)).SyncRoot)).CY_Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.IdentificationMeansCode)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).IdentificationMeanCodes)).SyncRoot)).CY_Data)));
			this.IdentificationMeansGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("987388f9-8f52-4ace-bf12-c64c54d297fa", "Type");
			zDropEditColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo2.ColumnName = "CY_Code";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("421f37c3-7e9d-4060-a169-5494eabbb34e", "Description");
			zTextBoxColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			zTextBoxColumnStyleInfo2.ColumnName = "CY_Data";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(220);
			this.IdentificationMeansGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.IdentificationMeansGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.IdentificationMeansGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.IdentificationMeansGrid.GridId = "e4a37359-b2d8-4c61-9879-eede0b5f7add";
			this.IdentificationMeansGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.IdentificationMeansGrid.LayoutKey = "IdentificationMeansGrid";
			this.IdentificationMeansGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.IdentificationMeansGrid.Name = "IdentificationMeansGrid";
			this.IdentificationMeansGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(349, 140, true);
			this.IdentificationMeansGrid.TabIndex = 0;
			// 
			// ProductGroupBox
			// 
			this.ProductGroupBox.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("989b2289-60d4-491f-9283-4440295f808a", "Product");
			this.ProductGroupBox.Controls.Add(this.ProductGrid);
			this.ProductGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(277, 3, true);
			this.ProductGroupBox.Name = "ProductGroupBox";
			this.ProductGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(344, 159, true);
			this.ProductGroupBox.TabIndex = 1;
			this.ProductGroupBox.TabStop = false;
			// 
			// ProductGrid
			// 
			this.ProductGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ProductGrid, "CustomsEntryInstructions.Products");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).Products)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.ProductSupportingInfo)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).Products)).SyncRoot)).CSI_Description)));
			this.ProductGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("73d9cdac-d7f5-442d-a77e-12f5e1031e8f", "Description");
			zTextBoxColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			zTextBoxColumnStyleInfo3.ColumnName = "CSI_Description";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(220);
			this.ProductGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.ProductGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ProductGrid.GridId = "f43589cd-77e3-465f-a886-ac4fc6d23cdf";
			this.ProductGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ProductGrid.LayoutKey = "ProductGrid";
			this.ProductGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.ProductGrid.Name = "ProductGrid";
			this.ProductGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(338, 140, true);
			this.ProductGrid.TabIndex = 0;
			// 
			// BottomGridPanel
			// 
			this.BottomGridPanel.Controls.Add(this.IdentificationMeansGroupBox);
			this.BottomGridPanel.Controls.Add(this.ProductGroupBox);
			this.BottomGridPanel.Controls.Add(this.ReimportCountryGroupBox);
			this.BottomGridPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.BottomGridPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.BottomGridPanel.Name = "BottomGridPanel";
			this.BottomGridPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(982, 209, true);
			this.BottomGridPanel.TabIndex = 2;
			// 
			// OutwardProcessingUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.BottomGridPanel);
			this.Name = "OutwardProcessingUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(982, 209, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ReimportCountryGroupBox.ResumeLayout(false);
			this.ReimportCountryGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ReimportCountryGrid)).EndInit();
			this.ReimportCountryGrid.ResumeLayout(false);
			this.ReimportCountryGrid.PerformLayout();
			this.IdentificationMeansGroupBox.ResumeLayout(false);
			this.IdentificationMeansGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.IdentificationMeansGrid)).EndInit();
			this.IdentificationMeansGrid.ResumeLayout(false);
			this.IdentificationMeansGrid.PerformLayout();
			this.ProductGroupBox.ResumeLayout(false);
			this.ProductGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ProductGrid)).EndInit();
			this.ProductGrid.ResumeLayout(false);
			this.ProductGrid.PerformLayout();
			this.BottomGridPanel.ResumeLayout(false);
			this.BottomGridPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.GUI.ZGroupBox ReimportCountryGroupBox;
		private ZArchitecture.ZGrid ReimportCountryGrid;
		private ZArchitecture.GUI.ZGroupBox IdentificationMeansGroupBox;
		private ZArchitecture.ZGrid IdentificationMeansGrid;
		private ZArchitecture.GUI.ZGroupBox ProductGroupBox;
		private ZArchitecture.ZGrid ProductGrid;
		private ZArchitecture.GUI.ZPanel BottomGridPanel;
	}
}
