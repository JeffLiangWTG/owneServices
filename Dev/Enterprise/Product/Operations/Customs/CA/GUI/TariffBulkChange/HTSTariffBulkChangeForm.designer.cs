using CargoWise.Types;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.GUI
{
	partial class HTSTariffBulkChangeForm
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

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		protected override void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo10 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo11 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo12 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo13 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo14 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo15 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo16 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo17 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo18 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo19 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo20 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo21 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo22 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo23 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo24 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo25 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo26 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.label1 = new Enterprise.ZArchitecture.ZLabel();
			this.label2 = new Enterprise.ZArchitecture.ZLabel();
			this.label3 = new Enterprise.ZArchitecture.ZLabel();
			this.label4 = new Enterprise.ZArchitecture.ZLabel();
			this.label5 = new Enterprise.ZArchitecture.ZLabel();
			this.MakeNewClassZButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.UpdateProductsZButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.NewTariffsZGrid = new Enterprise.ZArchitecture.ZGrid();
			this.PartsZGrid = new Enterprise.ZArchitecture.ZGrid();
			this.NewClassificationsZGrid = new Enterprise.ZArchitecture.ZGrid();
			this.OldClassificationsZGrid = new Enterprise.ZArchitecture.ZGrid();
			this.UpdateOriginalLookupsZButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.OldTariffsZGrid = new Enterprise.ZArchitecture.ZGrid();
			this.PostingButtonsUserControl = new Enterprise.Core.Forms.ZPostingButtonsUserControl();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.NewTariffsZGrid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.PartsZGrid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.NewClassificationsZGrid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.OldClassificationsZGrid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.OldTariffsZGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 622, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1016, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.Business.TariffBulkChange);
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 9, true);
			this.label1.Name = "label1";
			this.label1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(57, 13, true);
			this.label1.TabIndex = 30;
			this.label1.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("ddc1b9b9-f371-4891-b0e0-887688e5a60a", "Old Tariffs");
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 150, true);
			this.label2.Name = "label2";
			this.label2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(62, 13, true);
			this.label2.TabIndex = 31;
			this.label2.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("215f9111-bfc9-4ab2-b2eb-4d28777f400f", "New Tariffs");
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(189, 9, true);
			this.label3.Name = "label3";
			this.label3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(163, 13, true);
			this.label3.TabIndex = 32;
			this.label3.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("256f0b1b-b7ae-4b29-8465-69f9f8c81b18", "Original Lookups (Classifications)");
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 334, true);
			this.label4.Name = "label4";
			this.label4.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(157, 13, true);
			this.label4.TabIndex = 34;
			this.label4.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("db7cf242-24f9-45f0-a1e8-244ce2c3fe2f", "Products for selected Old Tariff");
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(189, 150, true);
			this.label5.Name = "label5";
			this.label5.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(148, 13, true);
			this.label5.TabIndex = 8;
			this.label5.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("17073ce0-b093-4636-adc3-2f32a1a19276", "New Lookups (Classifications)");
			// 
			// MakeNewClassZButton
			// 
			this.MakeNewClassZButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(192, 290, true);
			this.MakeNewClassZButton.Name = "MakeNewClassZButton";
			this.MakeNewClassZButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(154, 38, true);
			this.MakeNewClassZButton.TabIndex = 6;
			this.MakeNewClassZButton.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("350b0587-54c7-46f1-9569-a2629a754e56", "Make New Lookups From New Tariffs");
			this.MakeNewClassZButton.UseVisualStyleBackColor = true;
			this.MakeNewClassZButton.Click += new System.EventHandler(this.MakeNewClassZButton_Click);
			// 
			// UpdateProductsZButton
			// 
			this.UpdateProductsZButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(368, 290, true);
			this.UpdateProductsZButton.Name = "UpdateProductsZButton";
			this.UpdateProductsZButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(177, 38, true);
			this.UpdateProductsZButton.TabIndex = 7;
			this.UpdateProductsZButton.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("a39b8c0e-56e7-4fef-8378-26abdc1140f9", "Update Products with Selected New Lookup or New Tariff Number");
			this.UpdateProductsZButton.UseVisualStyleBackColor = true;
			this.UpdateProductsZButton.Click += new System.EventHandler(this.UpdateProductsZButton_Click);
			// 
			// NewTariffsZGrid
			// 
			this.NewTariffsZGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.NewTariffsZGrid, "TariffBulkChangeOldTariffs.TariffBulkChangeNewTariffs");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.Business.TariffBulkChange.TariffBulkChangeOldTariff)(((System.Collections.IList)(((Enterprise.Customs.Business.TariffBulkChange)(null)).TariffBulkChangeOldTariffs)).SyncRoot)).TariffBulkChangeNewTariffs)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.TariffBulkChange.TariffBulkChangeNewTariff)(((System.Collections.IList)(((Enterprise.Customs.Business.TariffBulkChange.TariffBulkChangeOldTariff)(((System.Collections.IList)(((Enterprise.Customs.Business.TariffBulkChange)(null)).TariffBulkChangeOldTariffs)).SyncRoot)).TariffBulkChangeNewTariffs)).SyncRoot)).NewTariffNum)));
			this.NewTariffsZGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("f4bdd5dc-f2d4-478a-aba1-5d4fdf5140d4", "New Tariff Num");
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "NewTariffNum";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			this.NewTariffsZGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.NewTariffsZGrid.CopySelectedRowsAllowed = true;
			this.NewTariffsZGrid.GridId = "0422d320-d806-44ff-8eae-b8ab950ca159";
			this.NewTariffsZGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.NewTariffsZGrid.LayoutKey = "NewTariffsZGrid";
			this.NewTariffsZGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(15, 169, true);
			this.NewTariffsZGrid.Name = "NewTariffsZGrid";
			this.NewTariffsZGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.NewTariffsZGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(154, 115, true);
			this.NewTariffsZGrid.TabIndex = 3;
			// 
			// PartsZGrid
			// 
			this.PartsZGrid.AllowNavigation = false;
			this.PartsZGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.PartsZGrid, "TariffBulkChangeOldTariffs.TariffItemPivots");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.Business.TariffBulkChange.TariffBulkChangeOldTariff)(((System.Collections.IList)(((Enterprise.Customs.Business.TariffBulkChange)(null)).TariffBulkChangeOldTariffs)).SyncRoot)).TariffItemPivots)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseCusClassPartPivot)(((System.Collections.IList)(((Enterprise.Customs.Business.TariffBulkChange.TariffBulkChangeOldTariff)(((System.Collections.IList)(((Enterprise.Customs.Business.TariffBulkChange)(null)).TariffBulkChangeOldTariffs)).SyncRoot)).TariffItemPivots)).SyncRoot)).Part.OP_PartNum)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseCusClassPartPivot)(((System.Collections.IList)(((Enterprise.Customs.Business.TariffBulkChange.TariffBulkChangeOldTariff)(((System.Collections.IList)(((Enterprise.Customs.Business.TariffBulkChange)(null)).TariffBulkChangeOldTariffs)).SyncRoot)).TariffItemPivots)).SyncRoot)).Part.OP_Desc)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseCusClassPartPivot)(((System.Collections.IList)(((Enterprise.Customs.Business.TariffBulkChange.TariffBulkChangeOldTariff)(((System.Collections.IList)(((Enterprise.Customs.Business.TariffBulkChange)(null)).TariffBulkChangeOldTariffs)).SyncRoot)).TariffItemPivots)).SyncRoot)).Part.AllOwners)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseCusClassPartPivot)(((System.Collections.IList)(((Enterprise.Customs.Business.TariffBulkChange.TariffBulkChangeOldTariff)(((System.Collections.IList)(((Enterprise.Customs.Business.TariffBulkChange)(null)).TariffBulkChangeOldTariffs)).SyncRoot)).TariffItemPivots)).SyncRoot)).Part.AllSuppliers)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseCusClassPartPivot)(((System.Collections.IList)(((Enterprise.Customs.Business.TariffBulkChange.TariffBulkChangeOldTariff)(((System.Collections.IList)(((Enterprise.Customs.Business.TariffBulkChange)(null)).TariffBulkChangeOldTariffs)).SyncRoot)).TariffItemPivots)).SyncRoot)).CI_ChildType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseCusClassPartPivot)(((System.Collections.IList)(((Enterprise.Customs.Business.TariffBulkChange.TariffBulkChangeOldTariff)(((System.Collections.IList)(((Enterprise.Customs.Business.TariffBulkChange)(null)).TariffBulkChangeOldTariffs)).SyncRoot)).TariffItemPivots)).SyncRoot)).OldClassificationCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseCusClassPartPivot)(((System.Collections.IList)(((Enterprise.Customs.Business.TariffBulkChange.TariffBulkChangeOldTariff)(((System.Collections.IList)(((Enterprise.Customs.Business.TariffBulkChange)(null)).TariffBulkChangeOldTariffs)).SyncRoot)).TariffItemPivots)).SyncRoot)).OldTariffCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseCusClassPartPivot)(((System.Collections.IList)(((Enterprise.Customs.Business.TariffBulkChange.TariffBulkChangeOldTariff)(((System.Collections.IList)(((Enterprise.Customs.Business.TariffBulkChange)(null)).TariffBulkChangeOldTariffs)).SyncRoot)).TariffItemPivots)).SyncRoot)).NewLookUpCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseCusClassPartPivot)(((System.Collections.IList)(((Enterprise.Customs.Business.TariffBulkChange.TariffBulkChangeOldTariff)(((System.Collections.IList)(((Enterprise.Customs.Business.TariffBulkChange)(null)).TariffBulkChangeOldTariffs)).SyncRoot)).TariffItemPivots)).SyncRoot)).NewTariffNum)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseCusClassPartPivot)(((System.Collections.IList)(((Enterprise.Customs.Business.TariffBulkChange.TariffBulkChangeOldTariff)(((System.Collections.IList)(((Enterprise.Customs.Business.TariffBulkChange)(null)).TariffBulkChangeOldTariffs)).SyncRoot)).TariffItemPivots)).SyncRoot)).Part.OP_StockKeepingUnit)));
			this.PartsZGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("9935612d-3b71-4a0b-a614-e71dd515790f", "Part Num");
			zTextBoxColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo2.ColumnName = "Part+OP_PartNum";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("2e32860a-1045-4337-8f48-b81e1bd0e099", "Product Description");
			zTextBoxColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo3.ColumnName = "Part+OP_Desc";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(300);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("1c3f2434-be12-4c5a-b090-8213319a4b9d", "Importers");
			zTextBoxColumnStyleInfo4.ColumnName = "Part+AllOwners";
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("baede318-8c15-4e13-b741-8f4dc2c1ab99", "Suppliers");
			zTextBoxColumnStyleInfo5.ColumnName = "Part+AllSuppliers";
			zTextBoxColumnStyleInfo6.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("698ac20e-9948-4e21-83fd-a935b59ad0ed", "Type");
			zTextBoxColumnStyleInfo6.ColumnName = "CI_ChildType";
			zTextBoxColumnStyleInfo6.IsReadOnly = true;
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo7.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("a628b529-d3f5-4212-8ac7-d7c242180c48", "Orig. Lookup");
			zTextBoxColumnStyleInfo7.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo7.ColumnName = "OldClassificationCode";
			zTextBoxColumnStyleInfo7.IsReadOnly = true;
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo8.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("9b36f710-3b40-4eef-b514-56e3d9b99c18", "Orig. Tariff");
			zTextBoxColumnStyleInfo8.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo8.ColumnName = "OldTariffCode";
			zTextBoxColumnStyleInfo8.IsReadOnly = true;
			zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zTextBoxColumnStyleInfo9.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("d5936e34-1588-4ffc-b07e-3c9da3dfd5ee", "New Lookup");
			zTextBoxColumnStyleInfo9.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo9.ColumnName = "NewLookUpCode";
			zTextBoxColumnStyleInfo9.IsReadOnly = true;
			zTextBoxColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo10.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("fa89dbac-1de6-4ccb-b17b-ab68bb91ec81", "New Tariff");
			zTextBoxColumnStyleInfo10.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo10.ColumnName = "NewTariffNum";
			zTextBoxColumnStyleInfo10.IsReadOnly = true;
			zTextBoxColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zTextBoxColumnStyleInfo11.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("3c789e5f-fbb7-4986-bdc8-fc039009746c", "Stock Keeping Unit");
			zTextBoxColumnStyleInfo11.ColumnName = "Part+OP_StockKeepingUnit";
			this.PartsZGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.PartsZGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.PartsZGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.PartsZGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.PartsZGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.PartsZGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.PartsZGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.PartsZGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
			this.PartsZGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo10);
			this.PartsZGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo11);
			this.PartsZGrid.CopySelectedRowsAllowed = true;
			this.PartsZGrid.GridId = "70275f40-832c-4ff2-a60e-38cab14ef06d";
			this.PartsZGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.PartsZGrid.LayoutKey = "PartsZGrid";
			this.PartsZGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 350, true);
			this.PartsZGrid.Name = "PartsZGrid";
			this.PartsZGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.PartsZGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(992, 234, true);
			this.PartsZGrid.TabIndex = 8;
			// 
			// NewClassificationsZGrid
			// 
			this.NewClassificationsZGrid.AllowNavigation = false;
			this.NewClassificationsZGrid.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.NewClassificationsZGrid, "TariffBulkChangeOldTariffs.NewClassifications");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.Business.TariffBulkChange.TariffBulkChangeOldTariff)(((System.Collections.IList)(((Enterprise.Customs.Business.TariffBulkChange)(null)).TariffBulkChangeOldTariffs)).SyncRoot)).NewClassifications)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.TariffBulkChange.TBCClassification)(((System.Collections.IList)(((Enterprise.Customs.Business.TariffBulkChange.TariffBulkChangeOldTariff)(((System.Collections.IList)(((Enterprise.Customs.Business.TariffBulkChange)(null)).TariffBulkChangeOldTariffs)).SyncRoot)).NewClassifications)).SyncRoot)).CC_LookupCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.TariffBulkChange.TBCClassification)(((System.Collections.IList)(((Enterprise.Customs.Business.TariffBulkChange.TariffBulkChangeOldTariff)(((System.Collections.IList)(((Enterprise.Customs.Business.TariffBulkChange)(null)).TariffBulkChangeOldTariffs)).SyncRoot)).NewClassifications)).SyncRoot)).NewLookupCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.TariffBulkChange.TBCClassification)(((System.Collections.IList)(((Enterprise.Customs.Business.TariffBulkChange.TariffBulkChangeOldTariff)(((System.Collections.IList)(((Enterprise.Customs.Business.TariffBulkChange)(null)).TariffBulkChangeOldTariffs)).SyncRoot)).NewClassifications)).SyncRoot)).CC_FormattedTariffNum)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.TariffBulkChange.TBCClassification)(((System.Collections.IList)(((Enterprise.Customs.Business.TariffBulkChange.TariffBulkChangeOldTariff)(((System.Collections.IList)(((Enterprise.Customs.Business.TariffBulkChange)(null)).TariffBulkChangeOldTariffs)).SyncRoot)).NewClassifications)).SyncRoot)).NewTariffNum)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.TariffBulkChange.TBCClassification)(((System.Collections.IList)(((Enterprise.Customs.Business.TariffBulkChange.TariffBulkChangeOldTariff)(((System.Collections.IList)(((Enterprise.Customs.Business.TariffBulkChange)(null)).TariffBulkChangeOldTariffs)).SyncRoot)).NewClassifications)).SyncRoot)).CC_Description)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.TariffBulkChange.TBCClassification)(((System.Collections.IList)(((Enterprise.Customs.Business.TariffBulkChange.TariffBulkChangeOldTariff)(((System.Collections.IList)(((Enterprise.Customs.Business.TariffBulkChange)(null)).TariffBulkChangeOldTariffs)).SyncRoot)).NewClassifications)).SyncRoot)).CC_ClassificationType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.TariffBulkChange.TBCClassification)(((System.Collections.IList)(((Enterprise.Customs.Business.TariffBulkChange.TariffBulkChangeOldTariff)(((System.Collections.IList)(((Enterprise.Customs.Business.TariffBulkChange)(null)).TariffBulkChangeOldTariffs)).SyncRoot)).NewClassifications)).SyncRoot)).CC_AddInfo)));
			this.NewClassificationsZGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo12.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("e7eff7bd-97f1-47b7-ba6d-08e7d6edb8fe", "Orig. Lookup");
			zTextBoxColumnStyleInfo12.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo12.ColumnName = "CC_LookupCode";
			zTextBoxColumnStyleInfo12.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo13.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("fa82e2e9-bd4c-4dcc-bfe9-bd7eca75bd8a", "New Lookup");
			zTextBoxColumnStyleInfo13.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo13.ColumnName = "NewLookupCode";
			zTextBoxColumnStyleInfo13.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo14.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("b131d1fa-bb0e-453c-8299-a6f975e42b2f", "Orig. Tariff");
			zTextBoxColumnStyleInfo14.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo14.ColumnName = "CC_FormattedTariffNum";
			zTextBoxColumnStyleInfo14.IsReadOnly = true;
			zTextBoxColumnStyleInfo14.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo15.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("8c3cc9e0-ab8e-4f87-b917-033df8dd2198", "New Tariff");
			zTextBoxColumnStyleInfo15.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo15.ColumnName = "NewTariffNum";
			zTextBoxColumnStyleInfo15.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zTextBoxColumnStyleInfo16.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("7db81412-eb64-49b1-967d-f8dbebece2e9", "Lookup Description");
			zTextBoxColumnStyleInfo16.ColumnName = "CC_Description";
			zTextBoxColumnStyleInfo16.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(300);
			zTextBoxColumnStyleInfo17.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("ceb54520-4af8-4eeb-b9b7-40870551f184", "Type");
			zTextBoxColumnStyleInfo17.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo17.ColumnName = "CC_ClassificationType";
			zTextBoxColumnStyleInfo17.IsReadOnly = true;
			zTextBoxColumnStyleInfo17.IsVisible = false;
			zTextBoxColumnStyleInfo17.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo18.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("405f1245-e5df-4f3d-87c4-f1b2650ae8ad", "Add Info");
			zTextBoxColumnStyleInfo18.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo18.ColumnName = "CC_AddInfo";
			zTextBoxColumnStyleInfo18.IsVisible = false;
			zTextBoxColumnStyleInfo18.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			this.NewClassificationsZGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo12);
			this.NewClassificationsZGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo13);
			this.NewClassificationsZGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo14);
			this.NewClassificationsZGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo15);
			this.NewClassificationsZGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo16);
			this.NewClassificationsZGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo17);
			this.NewClassificationsZGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo18);
			this.NewClassificationsZGrid.CopySelectedRowsAllowed = true;
			this.NewClassificationsZGrid.GridId = "a442b7f1-628c-4af4-857c-9fed4d0bc7f4";
			this.NewClassificationsZGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.NewClassificationsZGrid.LayoutKey = "zGrid1";
			this.NewClassificationsZGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(192, 169, true);
			this.NewClassificationsZGrid.Name = "NewClassificationsZGrid";
			this.NewClassificationsZGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.NewClassificationsZGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(812, 115, true);
			this.NewClassificationsZGrid.TabIndex = 4;
			// 
			// OldClassificationsZGrid
			// 
			this.OldClassificationsZGrid.AllowNavigation = false;
			this.OldClassificationsZGrid.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.OldClassificationsZGrid, "TariffBulkChangeOldTariffs.OriginalClassifications");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.Business.TariffBulkChange.TariffBulkChangeOldTariff)(((System.Collections.IList)(((Enterprise.Customs.Business.TariffBulkChange)(null)).TariffBulkChangeOldTariffs)).SyncRoot)).OriginalClassifications)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.TariffBulkChange.TBCClassification)(((System.Collections.IList)(((Enterprise.Customs.Business.TariffBulkChange.TariffBulkChangeOldTariff)(((System.Collections.IList)(((Enterprise.Customs.Business.TariffBulkChange)(null)).TariffBulkChangeOldTariffs)).SyncRoot)).OriginalClassifications)).SyncRoot)).CC_LookupCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.TariffBulkChange.TBCClassification)(((System.Collections.IList)(((Enterprise.Customs.Business.TariffBulkChange.TariffBulkChangeOldTariff)(((System.Collections.IList)(((Enterprise.Customs.Business.TariffBulkChange)(null)).TariffBulkChangeOldTariffs)).SyncRoot)).OriginalClassifications)).SyncRoot)).NewLookupCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.TariffBulkChange.TBCClassification)(((System.Collections.IList)(((Enterprise.Customs.Business.TariffBulkChange.TariffBulkChangeOldTariff)(((System.Collections.IList)(((Enterprise.Customs.Business.TariffBulkChange)(null)).TariffBulkChangeOldTariffs)).SyncRoot)).OriginalClassifications)).SyncRoot)).CC_FormattedTariffNum)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.TariffBulkChange.TBCClassification)(((System.Collections.IList)(((Enterprise.Customs.Business.TariffBulkChange.TariffBulkChangeOldTariff)(((System.Collections.IList)(((Enterprise.Customs.Business.TariffBulkChange)(null)).TariffBulkChangeOldTariffs)).SyncRoot)).OriginalClassifications)).SyncRoot)).NewTariffNum)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.TariffBulkChange.TBCClassification)(((System.Collections.IList)(((Enterprise.Customs.Business.TariffBulkChange.TariffBulkChangeOldTariff)(((System.Collections.IList)(((Enterprise.Customs.Business.TariffBulkChange)(null)).TariffBulkChangeOldTariffs)).SyncRoot)).OriginalClassifications)).SyncRoot)).CC_Description)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.TariffBulkChange.TBCClassification)(((System.Collections.IList)(((Enterprise.Customs.Business.TariffBulkChange.TariffBulkChangeOldTariff)(((System.Collections.IList)(((Enterprise.Customs.Business.TariffBulkChange)(null)).TariffBulkChangeOldTariffs)).SyncRoot)).OriginalClassifications)).SyncRoot)).CC_ClassificationType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.TariffBulkChange.TBCClassification)(((System.Collections.IList)(((Enterprise.Customs.Business.TariffBulkChange.TariffBulkChangeOldTariff)(((System.Collections.IList)(((Enterprise.Customs.Business.TariffBulkChange)(null)).TariffBulkChangeOldTariffs)).SyncRoot)).OriginalClassifications)).SyncRoot)).CC_AddInfo)));
			this.OldClassificationsZGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo19.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("e7eff7bd-97f1-47b7-ba6d-08e7d6edb8fe", "Orig. Lookup");
			zTextBoxColumnStyleInfo19.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo19.ColumnName = "CC_LookupCode";
			zTextBoxColumnStyleInfo19.IsReadOnly = true;
			zTextBoxColumnStyleInfo19.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo20.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("fa82e2e9-bd4c-4dcc-bfe9-bd7eca75bd8a", "New Lookup");
			zTextBoxColumnStyleInfo20.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo20.ColumnName = "NewLookupCode";
			zTextBoxColumnStyleInfo20.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo21.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("b131d1fa-bb0e-453c-8299-a6f975e42b2f", "Orig. Tariff");
			zTextBoxColumnStyleInfo21.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo21.ColumnName = "CC_FormattedTariffNum";
			zTextBoxColumnStyleInfo21.IsReadOnly = true;
			zTextBoxColumnStyleInfo21.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo22.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("8c3cc9e0-ab8e-4f87-b917-033df8dd2198", "New Tariff");
			zTextBoxColumnStyleInfo22.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo22.ColumnName = "NewTariffNum";
			zTextBoxColumnStyleInfo22.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zTextBoxColumnStyleInfo23.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("7db81412-eb64-49b1-967d-f8dbebece2e9", "Lookup Description");
			zTextBoxColumnStyleInfo23.ColumnName = "CC_Description";
			zTextBoxColumnStyleInfo23.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(300);
			zTextBoxColumnStyleInfo24.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("ceb54520-4af8-4eeb-b9b7-40870551f184", "Type");
			zTextBoxColumnStyleInfo24.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo24.ColumnName = "CC_ClassificationType";
			zTextBoxColumnStyleInfo24.IsReadOnly = true;
			zTextBoxColumnStyleInfo24.IsVisible = false;
			zTextBoxColumnStyleInfo24.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo25.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("405f1245-e5df-4f3d-87c4-f1b2650ae8ad", "Add Info");
			zTextBoxColumnStyleInfo25.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo25.ColumnName = "CC_AddInfo";
			zTextBoxColumnStyleInfo25.IsVisible = false;
			zTextBoxColumnStyleInfo25.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			this.OldClassificationsZGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo19);
			this.OldClassificationsZGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo20);
			this.OldClassificationsZGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo21);
			this.OldClassificationsZGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo22);
			this.OldClassificationsZGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo23);
			this.OldClassificationsZGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo24);
			this.OldClassificationsZGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo25);
			this.OldClassificationsZGrid.CopySelectedRowsAllowed = true;
			this.OldClassificationsZGrid.GridId = "8f7c5897-a808-4dcd-9fde-85e51282f00c";
			this.OldClassificationsZGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.OldClassificationsZGrid.LayoutKey = "OldClassificationsZGrid";
			this.OldClassificationsZGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(192, 28, true);
			this.OldClassificationsZGrid.Name = "OldClassificationsZGrid";
			this.OldClassificationsZGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.OldClassificationsZGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(812, 116, true);
			this.OldClassificationsZGrid.TabIndex = 2;
			// 
			// UpdateOriginalLookupsZButton
			// 
			this.UpdateOriginalLookupsZButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(15, 290, true);
			this.UpdateOriginalLookupsZButton.Name = "UpdateOriginalLookupsZButton";
			this.UpdateOriginalLookupsZButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(154, 38, true);
			this.UpdateOriginalLookupsZButton.TabIndex = 5;
			this.UpdateOriginalLookupsZButton.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("cf74bdb4-d0ba-406a-9d8c-073dba5ae7a9", "Update Original Lookups with Selected New Tariff");
			this.UpdateOriginalLookupsZButton.UseVisualStyleBackColor = true;
			this.UpdateOriginalLookupsZButton.Click += new System.EventHandler(this.UpdateOriginalLookups);
			// 
			// OldTariffsZGrid
			// 
			this.OldTariffsZGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.OldTariffsZGrid, "TariffBulkChangeOldTariffs");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.Business.TariffBulkChange)(null)).TariffBulkChangeOldTariffs)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.TariffBulkChange.TariffBulkChangeOldTariff)(((System.Collections.IList)(((Enterprise.Customs.Business.TariffBulkChange)(null)).TariffBulkChangeOldTariffs)).SyncRoot)).OldTariffNum)));
			this.OldTariffsZGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo26.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("b49d3a16-9b58-41e1-8c77-62ce0fd1a29e", "Old Tariff Num");
			zTextBoxColumnStyleInfo26.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo26.ColumnName = "OldTariffNum";
			zTextBoxColumnStyleInfo26.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			this.OldTariffsZGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo26);
			this.OldTariffsZGrid.CopySelectedRowsAllowed = true;
			this.OldTariffsZGrid.GridId = "f26d93a5-09d1-4037-b17e-9ec935f374bb";
			this.OldTariffsZGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.OldTariffsZGrid.LayoutKey = "OldTariffsZGrid";
			this.OldTariffsZGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(15, 28, true);
			this.OldTariffsZGrid.Name = "OldTariffsZGrid";
			this.OldTariffsZGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.OldTariffsZGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(154, 116, true);
			this.OldTariffsZGrid.TabIndex = 1;
			// 
			// PostingButtonsUserControl
			// 
			this.PostingButtonsUserControl.AllowDrop = true;
			this.PostingButtonsUserControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.PostingButtonsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(770, 590, true);
			this.PostingButtonsUserControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 25, true);
			this.PostingButtonsUserControl.Name = "PostingButtonsUserControl";
			this.PostingButtonsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 25, true);
			this.PostingButtonsUserControl.TabIndex = 9;
			// 
			// HTSTariffBulkChangeForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1016, 646, true);
			this.Controls.Add(this.PostingButtonsUserControl);
			this.Controls.Add(this.OldTariffsZGrid);
			this.Controls.Add(this.UpdateOriginalLookupsZButton);
			this.Controls.Add(this.NewClassificationsZGrid);
			this.Controls.Add(this.OldClassificationsZGrid);
			this.Controls.Add(this.PartsZGrid);
			this.Controls.Add(this.NewTariffsZGrid);
			this.Controls.Add(this.UpdateProductsZButton);
			this.Controls.Add(this.MakeNewClassZButton);
			this.Controls.Add(this.label5);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.DataSourceAssemblyName = "Enterprise.Customs.Business";
			this.DataSourceType = typeof(Enterprise.Customs.Business.TariffBulkChange);
			this.DataSourceTypeName = "Enterprise.Customs.Business.TariffBulkChange";
			this.Name = "HTSTariffBulkChangeForm";
			this.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("0431b926-d707-488d-af51-403aa9953165", "Tariff Bulk Change");
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.label1, 0);
			this.Controls.SetChildIndex(this.label2, 0);
			this.Controls.SetChildIndex(this.label3, 0);
			this.Controls.SetChildIndex(this.label4, 0);
			this.Controls.SetChildIndex(this.label5, 0);
			this.Controls.SetChildIndex(this.MakeNewClassZButton, 0);
			this.Controls.SetChildIndex(this.UpdateProductsZButton, 0);
			this.Controls.SetChildIndex(this.NewTariffsZGrid, 0);
			this.Controls.SetChildIndex(this.PartsZGrid, 0);
			this.Controls.SetChildIndex(this.OldClassificationsZGrid, 0);
			this.Controls.SetChildIndex(this.NewClassificationsZGrid, 0);
			this.Controls.SetChildIndex(this.UpdateOriginalLookupsZButton, 0);
			this.Controls.SetChildIndex(this.OldTariffsZGrid, 0);
			this.Controls.SetChildIndex(this.PostingButtonsUserControl, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.NewTariffsZGrid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.PartsZGrid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.NewClassificationsZGrid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.OldClassificationsZGrid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.OldTariffsZGrid)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Enterprise.ZArchitecture.ZLabel label1;
		private Enterprise.ZArchitecture.ZLabel label2;
		private Enterprise.ZArchitecture.ZLabel label3;
		private Enterprise.ZArchitecture.ZLabel label4;
		private Enterprise.ZArchitecture.ZLabel label5;
		private Enterprise.ZArchitecture.GUI.ZButton MakeNewClassZButton;
		private Enterprise.ZArchitecture.GUI.ZButton UpdateProductsZButton;
		private Enterprise.ZArchitecture.ZGrid NewTariffsZGrid;
		private Enterprise.ZArchitecture.ZGrid PartsZGrid;
		private Enterprise.ZArchitecture.ZGrid NewClassificationsZGrid;
		private Enterprise.ZArchitecture.ZGrid OldClassificationsZGrid;
		private Enterprise.ZArchitecture.GUI.ZButton UpdateOriginalLookupsZButton;
		private Enterprise.ZArchitecture.ZGrid OldTariffsZGrid;
		private Enterprise.Core.Forms.ZPostingButtonsUserControl PostingButtonsUserControl;
	}
}

