namespace Enterprise.Customs.IE.GUI
{
	partial class CusLineTariffDetailUserControl
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
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.CusLineTariffDetailGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.CusLineTariffDetailGrid)).BeginInit();
			this.CusLineTariffDetailGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.IE.Business.Declaration.JobComInvoiceLine);
			// 
			// CusLineTariffDetailGrid
			// 
			this.CusLineTariffDetailGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.CusLineTariffDetailGrid, "CusLineTariffDetails");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.IE.Business.Declaration.JobComInvoiceLine)(null)).CusLineTariffDetails)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IE.Business.CusLineTariffDetail)(((System.Collections.IList)(((Enterprise.Customs.IE.Business.Declaration.JobComInvoiceLine)(null)).CusLineTariffDetails)).SyncRoot)).BZ_Type)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IE.Business.CusLineTariffDetail)(((System.Collections.IList)(((Enterprise.Customs.IE.Business.Declaration.JobComInvoiceLine)(null)).CusLineTariffDetails)).SyncRoot)).TypeDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IE.Business.CusLineTariffDetail)(((System.Collections.IList)(((Enterprise.Customs.IE.Business.Declaration.JobComInvoiceLine)(null)).CusLineTariffDetails)).SyncRoot)).BZ_Tariff)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.IE.Business.CusLineTariffDetail)(((System.Collections.IList)(((Enterprise.Customs.IE.Business.Declaration.JobComInvoiceLine)(null)).CusLineTariffDetails)).SyncRoot)).BZ_Qty1)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IE.Business.CusLineTariffDetail)(((System.Collections.IList)(((Enterprise.Customs.IE.Business.Declaration.JobComInvoiceLine)(null)).CusLineTariffDetails)).SyncRoot)).BZ_UQ1)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.IE.Business.CusLineTariffDetail)(((System.Collections.IList)(((Enterprise.Customs.IE.Business.Declaration.JobComInvoiceLine)(null)).CusLineTariffDetails)).SyncRoot)).BZ_Qty2)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IE.Business.CusLineTariffDetail)(((System.Collections.IList)(((Enterprise.Customs.IE.Business.Declaration.JobComInvoiceLine)(null)).CusLineTariffDetails)).SyncRoot)).BZ_UQ2)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IE.Business.CusLineTariffDetail)(((System.Collections.IList)(((Enterprise.Customs.IE.Business.Declaration.JobComInvoiceLine)(null)).CusLineTariffDetails)).SyncRoot)).ZG_MethodOfPayment)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IE.Business.CusLineTariffDetail)(((System.Collections.IList)(((Enterprise.Customs.IE.Business.Declaration.JobComInvoiceLine)(null)).CusLineTariffDetails)).SyncRoot)).RateFormula)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IE.Business.CusLineTariffDetail)(((System.Collections.IList)(((Enterprise.Customs.IE.Business.Declaration.JobComInvoiceLine)(null)).CusLineTariffDetails)).SyncRoot)).ExciseReferenceNumberDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IE.Business.CusLineTariffDetail)(((System.Collections.IList)(((Enterprise.Customs.IE.Business.Declaration.JobComInvoiceLine)(null)).CusLineTariffDetails)).SyncRoot)).PaymentMethodDescription)));
			this.CusLineTariffDetailGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.ColumnName = "BZ_Type";
			zDropEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo1.ColumnName = "TypeDescription";
			zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zCodeFindBoxColumnStyleInfo1.ColumnName = "BZ_Tariff";
			zCodeFindBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zCodeFindBoxColumnStyleInfo1.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.Universal.RefCusTariff;
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "BZ_Qty1";
			zCalcEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo2.ColumnName = "BZ_UQ1";
			zTextBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.ColumnName = "BZ_Qty2";
			zCalcEditColumnStyleInfo2.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo3.ColumnName = "BZ_UQ2";
			zTextBoxColumnStyleInfo3.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo2.ColumnName = "ZG_MethodOfPayment";
			zDropEditColumnStyleInfo2.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo4.ColumnName = "RateFormula";
			zTextBoxColumnStyleInfo4.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zTextBoxColumnStyleInfo5.ColumnName = "ExciseReferenceNumberDescription";
			zTextBoxColumnStyleInfo5.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zTextBoxColumnStyleInfo6.ColumnName = "PaymentMethodDescription";
			zTextBoxColumnStyleInfo6.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			this.CusLineTariffDetailGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.CusLineTariffDetailGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.CusLineTariffDetailGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.CusLineTariffDetailGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.CusLineTariffDetailGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.CusLineTariffDetailGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.CusLineTariffDetailGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.CusLineTariffDetailGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.CusLineTariffDetailGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.CusLineTariffDetailGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.CusLineTariffDetailGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.CusLineTariffDetailGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CusLineTariffDetailGrid.GridId = "efd83c01-ce2d-4f49-8a1a-13f412d42208";
			this.CusLineTariffDetailGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.CusLineTariffDetailGrid.LayoutKey = "CusLineTariffDetailGrid";
			this.CusLineTariffDetailGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CusLineTariffDetailGrid.Name = "CusLineTariffDetailGrid";
			this.CusLineTariffDetailGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(744, 308, true);
			this.CusLineTariffDetailGrid.TabIndex = 0;
			// 
			// CusLineTariffDetailUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.CusLineTariffDetailGrid);
			this.Name = "CusLineTariffDetailUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(744, 308, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.CusLineTariffDetailGrid)).EndInit();
			this.CusLineTariffDetailGrid.ResumeLayout(false);
			this.CusLineTariffDetailGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZGrid CusLineTariffDetailGrid;
	}
}
