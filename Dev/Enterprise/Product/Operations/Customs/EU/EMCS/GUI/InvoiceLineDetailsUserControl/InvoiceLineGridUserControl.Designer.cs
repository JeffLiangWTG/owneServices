namespace Enterprise.Customs.EU.EMCS.GUI
{
	partial class InvoiceLineGridUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.CustomsInvoiceLinesBoundGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.CustomsInvoiceLinesBoundGrid)).BeginInit();
			this.CustomsInvoiceLinesBoundGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.EMCS.Business.EMCSInvoiceLineViewCollection);
			// 
			// CustomsInvoiceLinesBoundGrid
			// 
			this.CustomsInvoiceLinesBoundGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.CustomsInvoiceLinesBoundGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.EU.EMCS.Business.EMCSJobComInvoiceLine)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.EMCS.Business.EMCSJobComInvoiceLine)(null)).JI_LineNo)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.EMCS.Business.EMCSJobComInvoiceLine)(null)).JI_PartNo)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.EU.EMCS.Business.EMCSJobComInvoiceLine)(null)).Lookups.PartsList)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.EMCS.Business.EMCSJobComInvoiceLine)(null)).JI_Tariff)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.EU.EMCS.Business.EMCSJobComInvoiceLine)(null)).Lookups.CNCodeList)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.EMCS.Business.EMCSJobComInvoiceLine)(null)).TariffDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.EMCS.Business.EMCSJobComInvoiceLine)(null)).ZG_ExciseProductCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.EMCS.Business.EMCSJobComInvoiceLine)(null)).JI_NDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.EMCS.Business.EMCSJobComInvoiceLine)(null)).JI_Weight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.EMCS.Business.EMCSJobComInvoiceLine)(null)).JI_WeightUQ)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.EU.EMCS.Business.EMCSJobComInvoiceLine)(null)).Lookups.WeightUQList)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.EMCS.Business.EMCSJobComInvoiceLine)(null)).JI_CustomsQuantity)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.EMCS.Business.EMCSJobComInvoiceLine)(null)).CustomsUnitQtyDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.EMCS.Business.EMCSJobComInvoiceLine)(null)).JI_NetWeight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.EMCS.Business.EMCSJobComInvoiceLine)(null)).JI_NetWeightUQ)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.EMCS.Business.EMCSJobComInvoiceLine)(null)).JI_BrandName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.EMCS.Business.EMCSJobComInvoiceLine)(null)).ZG_Origin)));
			this.CustomsInvoiceLinesBoundGrid.CaptionVisible = false;
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "JI_LineNo";
			zCalcEditColumnStyleInfo1.IsMandatory = true;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(30);
			zCodeFindBoxColumnStyleInfo1.BindToList = "Lookups.PartsList";
			zCodeFindBoxColumnStyleInfo1.ColumnName = "JI_PartNo";
			zCodeFindBoxColumnStyleInfo1.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.SupplierPart;
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCodeFindBoxColumnStyleInfo2.BindToList = "Lookups.CNCodeList";
			zCodeFindBoxColumnStyleInfo2.ColumnName = "JI_Tariff";
			zCodeFindBoxColumnStyleInfo2.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.Universal.ZZRefCusCodeList;
			zCodeFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.EU.EMCS.GUI.Res.GetData("EMCSInvoiceLineControl|FDB19792-9A22-494F-A4BA-5D7F4EDF5D02", "Description");
			zTextBoxColumnStyleInfo1.ColumnName = "TariffDescription";
			zTextBoxColumnStyleInfo1.GroupName = Enterprise.Customs.EU.EMCS.GUI.Res.GetData("EMCSInvoiceLineControl|006C512C-02D2-40B4-AA47-4B36B52DBF58", "Tariff");
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo1.ColumnName = "ZG_ExciseProductCode";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(88);
			zTextBoxColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			zTextBoxColumnStyleInfo2.ColumnName = "JI_NDescription";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.EU.EMCS.GUI.Res.GetData("EMCSInvoiceLineControl|02b3056e-85be-46d4-aff3-d5d1aa007ea0", "Gr. Weight");
			zCalcEditColumnStyleInfo2.ColumnName = "JI_Weight";
			zCalcEditColumnStyleInfo2.GroupName = Enterprise.Customs.EU.EMCS.GUI.Res.GetData("EMCSInvoiceLineControl|ef799b94-e141-40c8-aa9b-f2981531f701", "Gross Weight");
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zDropEditColumnStyleInfo2.BindToList = "Lookups.WeightUQList";
			zDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.EU.EMCS.GUI.Res.GetData("EMCSInvoiceLineControl|3b0ac72e-0082-4dc8-85c1-51f17fcf3d5f", "UQ");
			zDropEditColumnStyleInfo2.ColumnName = "JI_WeightUQ";
			zDropEditColumnStyleInfo2.GroupName = Enterprise.Customs.EU.EMCS.GUI.Res.GetData("EMCSInvoiceLineControl|ef799b94-e141-40c8-aa9b-f2981531f701", "Gross Weight");
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(30);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.EU.EMCS.GUI.Res.GetData("EMCSInvoiceLineControl|7c390c92-4d14-4488-b95e-f03ec7be87ce", "Customs Quantity");
			zCalcEditColumnStyleInfo3.ColumnName = "JI_CustomsQuantity";
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.EU.EMCS.GUI.Res.GetData("EMCSInvoiceLineControl|C4D9B023-6682-4E6D-AFAA-9919202657E9", "Customs Unit Qty");
			zTextBoxColumnStyleInfo3.ColumnName = "CustomsUnitQtyDescription";
			zTextBoxColumnStyleInfo3.IsReadOnly = true;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(137);
			zCalcEditColumnStyleInfo4.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo4.CaptionResourceString = Enterprise.Customs.EU.EMCS.GUI.Res.GetData("EMCSInvoiceLineControl|44d72a94-7b88-45d7-bdbc-efda1b953d06", "Net Wgt");
			zCalcEditColumnStyleInfo4.ColumnName = "JI_NetWeight";
			zCalcEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.EU.EMCS.GUI.Res.GetData("EMCSInvoiceLineControl|15adf56a-3f8b-4cd1-9e9f-76b7c8157c8f", "Net Weight Unit");
			zDropEditColumnStyleInfo3.ColumnName = "JI_NetWeightUQ";
			zDropEditColumnStyleInfo3.IsReadOnly = true;
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Customs.EU.EMCS.GUI.Res.GetData("EMCSInvoiceLineControl|64456bcf-45b0-4154-85dd-43fce6dada58", "Brand");
			zTextBoxColumnStyleInfo4.ColumnName = "JI_BrandName";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Customs.EU.EMCS.GUI.Res.GetData("EMCSInvoiceLineControl|0C4C8B1C-4CF7-4D7C-A0E9-0B94C813FFE1", "ORG", "Origin", "Goods Origin");
			zTextBoxColumnStyleInfo5.ColumnName = "ZG_Origin";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo2);
			this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
			this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.CustomsInvoiceLinesBoundGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CustomsInvoiceLinesBoundGrid.GridId = "43ea2b1e-2774-4c86-9336-9833c63e5a51";
			this.CustomsInvoiceLinesBoundGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.CustomsInvoiceLinesBoundGrid.LayoutKey = "DataEntryCustomsInvoiceLinesBoundGrid";
			this.CustomsInvoiceLinesBoundGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CustomsInvoiceLinesBoundGrid.Name = "CustomsInvoiceLinesBoundGrid";
			this.CustomsInvoiceLinesBoundGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1289, 183, true);
			this.CustomsInvoiceLinesBoundGrid.TabIndex = 0;
			// 
			// InvoiceLineGridUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.CustomsInvoiceLinesBoundGrid);
			this.Name = "InvoiceLineGridUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1289, 183, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.CustomsInvoiceLinesBoundGrid)).EndInit();
			this.CustomsInvoiceLinesBoundGrid.ResumeLayout(false);
			this.CustomsInvoiceLinesBoundGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.ZGrid CustomsInvoiceLinesBoundGrid;
	}
}
