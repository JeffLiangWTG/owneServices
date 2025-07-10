using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.Declaration.GUI
{
	public partial class DrawbackRefEntryLinesForm
	{
		protected override void InitializeComponent()
		{
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo5 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo6 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			this.drawbackRefEntryLinesModuleGrid = new DrawbackEntryLineModuleButtonGrid();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.drawbackRefEntryLinesModuleGrid.InnerGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 574, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(923, 24, true);
			// 
			// DrawbackRefEntryLinesModuleGrid
			// 
			this.drawbackRefEntryLinesModuleGrid.AttachButtonText = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("F457F354-A3E6-40FD-925C-814B05B78AA0", "Select Lines");
			this.drawbackRefEntryLinesModuleGrid.BindToFindBoxList = "AddInfo+Lookups+GlobalDrawbackEntryLineKeys";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((System.Collections.IList)(((JobComInvoiceLine)(null)).AddInfo.Lookups.GlobalDrawbackEntryLineKeys)));
			this.drawbackRefEntryLinesModuleGrid.BindToGridList = "DrawbackCusEntryLineCollection";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((System.Collections.IList)(((JobComInvoiceLine)(null)).DrawbackCusEntryLineCollection)));
			zTextBoxColumnStyleInfo1.Caption = "EntryNumber";
			zTextBoxColumnStyleInfo1.ColumnName = "Header+EntryNumber";
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.Caption = "Line Num";
			zCalcEditColumnStyleInfo1.ColumnName = "EffectiveLineNumber";
			zCalcEditColumnStyleInfo1.IsReadOnly = true;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.Caption = "Qty for Average Calc";
			zCalcEditColumnStyleInfo2.ColumnName = "DrawbackClaimQuantity";
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.Caption = "Customs Qty";
			zCalcEditColumnStyleInfo3.ColumnName = "CustomsQuantity";
			zCalcEditColumnStyleInfo3.IsReadOnly = true;
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo2.Caption = "Units";
			zTextBoxColumnStyleInfo2.ColumnName = "CustomsUnitQty";
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(30);
			zCalcEditColumnStyleInfo4.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo4.Caption = "Invoice Qty";
			zCalcEditColumnStyleInfo4.ColumnName = "InvoiceQuantity";
			zCalcEditColumnStyleInfo4.IsReadOnly = true;
			zCalcEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo3.Caption = "Units";
			zTextBoxColumnStyleInfo3.ColumnName = "InvoiceUQ";
			zTextBoxColumnStyleInfo3.IsReadOnly = true;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(30);
			zTextBoxColumnStyleInfo4.Caption = "Tariff";
			zTextBoxColumnStyleInfo4.ColumnName = "CL_AdValoremTariff";
			zTextBoxColumnStyleInfo4.IsReadOnly = true;
			zCalcEditColumnStyleInfo5.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo5.Caption = "Customs Value";
			zCalcEditColumnStyleInfo5.ColumnName = "CL_CustomsValue";
			zCalcEditColumnStyleInfo5.IsReadOnly = true;
			zCalcEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCalcEditColumnStyleInfo6.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo6.Caption = "Duty Percent";
			zCalcEditColumnStyleInfo6.ColumnName = "CL_DutyPercent";
			zCalcEditColumnStyleInfo6.IsReadOnly = true;
			zCalcEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zTextBoxColumnStyleInfo5.Caption = "Description";
			zTextBoxColumnStyleInfo5.ColumnName = "CL_Description";
			zTextBoxColumnStyleInfo5.IsReadOnly = true;
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			this.drawbackRefEntryLinesModuleGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.drawbackRefEntryLinesModuleGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.drawbackRefEntryLinesModuleGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.drawbackRefEntryLinesModuleGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.drawbackRefEntryLinesModuleGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.drawbackRefEntryLinesModuleGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
			this.drawbackRefEntryLinesModuleGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.drawbackRefEntryLinesModuleGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.drawbackRefEntryLinesModuleGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo5);
			this.drawbackRefEntryLinesModuleGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo6);
			this.drawbackRefEntryLinesModuleGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.drawbackRefEntryLinesModuleGrid.DetachButtonText = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("A01D4659-5819-4AA7-B6D2-ABD7F21BB2A2", "Remove");
			this.drawbackRefEntryLinesModuleGrid.DetachMessage = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("1CE8F482-0FC1-4BDF-9484-571E1BBFAA6C", "Are you sure you want to remove this line from the list?");
			// 
			// 
			// 
			this.drawbackRefEntryLinesModuleGrid.InnerGrid.AllowNavigation = false;
			this.drawbackRefEntryLinesModuleGrid.InnerGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
									| System.Windows.Forms.AnchorStyles.Left)
									| System.Windows.Forms.AnchorStyles.Right)));
			this.drawbackRefEntryLinesModuleGrid.InnerGrid.BindTo = "DrawbackCusEntryLineCollection";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((System.Collections.IList)(((JobComInvoiceLine)(null)).DrawbackCusEntryLineCollection)));
			this.drawbackRefEntryLinesModuleGrid.InnerGrid.CaptionVisible = false;
			this.drawbackRefEntryLinesModuleGrid.InnerGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.drawbackRefEntryLinesModuleGrid.InnerGrid.LayoutKey = "Grid";
			this.drawbackRefEntryLinesModuleGrid.InnerGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 2, true);
			this.drawbackRefEntryLinesModuleGrid.InnerGrid.Name = "Grid";
			this.drawbackRefEntryLinesModuleGrid.InnerGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.Remove;
			this.drawbackRefEntryLinesModuleGrid.InnerGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(901, 531, true);
			this.drawbackRefEntryLinesModuleGrid.InnerGrid.TabIndex = 0;
			// Compile time check for the above grid columns. If any of these lines fail, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((CusEntryLine)(((object)(((JobComInvoiceLine)(null)).DrawbackCusEntryLineCollection)))).Header.EntryNumberInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((CusEntryLine)(((object)(((JobComInvoiceLine)(null)).DrawbackCusEntryLineCollection)))).Header.EntryNumber)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.INumericZType)(((CusEntryLine)(((object)(((JobComInvoiceLine)(null)).DrawbackCusEntryLineCollection)))).EffectiveLineNumber)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((CusEntryLine)(((object)(((JobComInvoiceLine)(null)).DrawbackCusEntryLineCollection)))).EffectiveLineNumberInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.INumericZType)(((CusEntryLine)(((object)(((JobComInvoiceLine)(null)).DrawbackCusEntryLineCollection)))).DrawbackClaimQuantity)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((CusEntryLine)(((object)(((JobComInvoiceLine)(null)).DrawbackCusEntryLineCollection)))).DrawbackClaimQuantityInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.INumericZType)(((CusEntryLine)(((object)(((JobComInvoiceLine)(null)).DrawbackCusEntryLineCollection)))).CustomsQuantity)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((CusEntryLine)(((object)(((JobComInvoiceLine)(null)).DrawbackCusEntryLineCollection)))).CustomsQuantityInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((CusEntryLine)(((object)(((JobComInvoiceLine)(null)).DrawbackCusEntryLineCollection)))).CustomsUnitQtyInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((CusEntryLine)(((object)(((JobComInvoiceLine)(null)).DrawbackCusEntryLineCollection)))).CustomsUnitQty)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.INumericZType)(((CusEntryLine)(((object)(((JobComInvoiceLine)(null)).DrawbackCusEntryLineCollection)))).InvoiceQuantity)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((CusEntryLine)(((object)(((JobComInvoiceLine)(null)).DrawbackCusEntryLineCollection)))).InvoiceQuantityInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((CusEntryLine)(((object)(((JobComInvoiceLine)(null)).DrawbackCusEntryLineCollection)))).InvoiceUQInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((CusEntryLine)(((object)(((JobComInvoiceLine)(null)).DrawbackCusEntryLineCollection)))).InvoiceUQ)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((CusEntryLine)(((object)(((JobComInvoiceLine)(null)).DrawbackCusEntryLineCollection)))).CL_AdValoremTariffInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((CusEntryLine)(((object)(((JobComInvoiceLine)(null)).DrawbackCusEntryLineCollection)))).CL_AdValoremTariff)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.INumericZType)(((CusEntryLine)(((object)(((JobComInvoiceLine)(null)).DrawbackCusEntryLineCollection)))).CL_CustomsValue)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((CusEntryLine)(((object)(((JobComInvoiceLine)(null)).DrawbackCusEntryLineCollection)))).CL_CustomsValueInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.INumericZType)(((CusEntryLine)(((object)(((JobComInvoiceLine)(null)).DrawbackCusEntryLineCollection)))).CL_DutyPercent)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((CusEntryLine)(((object)(((JobComInvoiceLine)(null)).DrawbackCusEntryLineCollection)))).CL_DutyPercentInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((CusEntryLine)(((object)(((JobComInvoiceLine)(null)).DrawbackCusEntryLineCollection)))).CL_DescriptionInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((CusEntryLine)(((object)(((JobComInvoiceLine)(null)).DrawbackCusEntryLineCollection)))).CL_Description)));
			this.drawbackRefEntryLinesModuleGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 1, true);
			this.drawbackRefEntryLinesModuleGrid.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.AU.DrawbackEntryLine;
			this.drawbackRefEntryLinesModuleGrid.Name = "DrawbackRefEntryLinesModuleGrid";
			this.drawbackRefEntryLinesModuleGrid.NameOfAGridElement = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("9D92CB52-D969-4FAF-A8B1-0ABF94B2A13E", "Entry Line");
			this.drawbackRefEntryLinesModuleGrid.ShowEditButton = false;
			this.drawbackRefEntryLinesModuleGrid.ShowNewButton = false;
			this.drawbackRefEntryLinesModuleGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(911, 569, true);
			this.drawbackRefEntryLinesModuleGrid.TabIndex = 2;
			// 
			// DrawbackRefEntryLinesForm
			// 
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(923, 598, true);
			this.Controls.Add(this.drawbackRefEntryLinesModuleGrid);
			this.DataSourceAssemblyName = "Enterprise.Customs.AU.Declaration.Business";
			this.DataSourceType = typeof(JobComInvoiceLine);
			this.DataSourceTypeName = "Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine";
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(924, 618, true);
			this.Name = "DrawbackRefEntryLinesForm";
			this.Controls.SetChildIndex(this.drawbackRefEntryLinesModuleGrid, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.drawbackRefEntryLinesModuleGrid.InnerGrid)).EndInit();
			this.ResumeLayout(false);
		}

		DrawbackEntryLineModuleButtonGrid drawbackRefEntryLinesModuleGrid;
	}
}
