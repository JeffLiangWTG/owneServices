using System;
using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Windows.UI;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.AccountingCountryFactory;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.ARAP
{
	public partial class ComplianceDocumentUserControl : ZUserControl, IDataGridLayoutIdentifierRoot
	{
		public ComplianceDocumentUserControl()
		{
			fReadOnly = false;

			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();
			this.AddressWithContactControl.ContactInfoTabVisible = false;
		}

		public void SetOrgInformationGroupBoxCaption(string accountType)
		{
			SetCaption(this.AddressWithContactControl, accountType, false);
		}

		void SetCaption(Control control, string value, bool format)
		{
			if (format)
			{
				IResCaptionedControl resControl = control as IResCaptionedControl;
				resControl.CaptionResourceString = resControl.CaptionResourceString.Format(value);
			}
			else
			{
				control.GetExtension<LabelCaptionRenderer>().Caption = value;
			}
		}

		void zForm_Saved(object sender, EventArgs e)
		{
			var invoicingBase = DataSource as InvoicingBase;

			if (invoicingBase != null && invoicingBase.AH_OHInfo.ReadOnly)
			{
				AddressWithContactControl.SetReadOnlyIncludingChildren();
			}
		}

		bool fReadOnly;

		[Browsable(true), Category(ZGUIConstants.DesignerCategory)]
		public bool ReadOnly
		{
			get { return fReadOnly; }
			set
			{
				if (fReadOnly != value)
				{
					fReadOnly = value;

					this.DescTextbox.ReadOnly = fReadOnly;
				}
			}
		}

		void InvoiceCollapsibleTableLayoutPanel_VisibleChanged(object sender, EventArgs e)
		{
			if (InvoiceCollapsibleTableLayoutPanel.Visible && CallCollapseTableWhenBecomeVisible)
			{
				CollapseTable();
			}
		}

		#region IDataGridLayoutIdentifierRoot Members

		string IDataGridLayoutIdentifierRoot.ID
		{
			get
			{
				ZForm form = FindForm() as ZForm;

				string result = string.Empty;

				if (form != null)
				{
					result = form.Name;

					if (form.BusinessEntity != null)
					{
						result += form.BusinessEntity.GetType().Name;
					}
				}

				return result;
			}
		}

		#endregion

		public void CollapseTableWhenBecomeVisible()
		{
			if (InvoiceCollapsibleTableLayoutPanel.Visible)
			{
				CollapseTable();
			}
			else
			{
				CallCollapseTableWhenBecomeVisible = true;
			}
		}

		void CollapseTable()
		{
			InvoiceCollapsibleTableLayoutPanel.Collapse();
			InvoiceCollapsibleTableLayoutPanel.StopFlicker();
			CallCollapseTableWhenBecomeVisible = false;
		}

		bool CallCollapseTableWhenBecomeVisible;

		void LineSummaryTabPage_InitializeTab(object sender, EventArgs e)
		{
			//
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			//
			ZMultiLineTextBoxColumnInfo zMultiLineTextBoxColumnInfo1 = new ZMultiLineTextBoxColumnInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new ZTextBoxColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo5 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo6 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo7 = new ZCalcEditColumnStyleInfo();
			this.ComplianceDocumentLineGrid = new ZGrid();
			this.ComplianceDocumentLineSummaryTabPage.SuspendLayout();
			((ISupportInitialize)(this.ComplianceDocumentLineGrid)).BeginInit();
			this.ComplianceDocumentLineGrid.SuspendLayout();
			this.ComplianceDocumentLineSummaryTabPage.Controls.Add(this.ComplianceDocumentLineGrid);
			//
			// ComplianceDocumentLineGrid
			//
			this.ComplianceDocumentLineGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ComplianceDocumentLineGrid, "ComplianceDocumentLines");
			this.ComplianceDocumentLineGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("6E78F66C-7F73-4C1E-8B09-7126FE617AB9", "Charge");
			zTextBoxColumnStyleInfo4.ColumnName = "Charge";
			zTextBoxColumnStyleInfo4.IsReadOnly = true;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zMultiLineTextBoxColumnInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("A447E564-333B-494F-A13E-78D731B6FF52", "Line Description");
			zMultiLineTextBoxColumnInfo1.ColumnName = "ADL_Description";
			zMultiLineTextBoxColumnInfo1.MinimumEditControlWidth = 300;
			zMultiLineTextBoxColumnInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140);
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("24CB3955-E40E-45E6-9DBC-F1EED1FEC279", "Currency");
			zTextBoxColumnStyleInfo5.ColumnName = "Currency";
			zTextBoxColumnStyleInfo5.IsReadOnly = true;
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("D920CC83-4FA3-44E8-8401-E19B519A73E6", "Amount");
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "Amount";
			zCalcEditColumnStyleInfo1.IsReadOnly = true;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("9B5BC565-4A84-482E-A45B-6DDDEA4AC9AB", "Tax Amount");
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.ColumnName = "TaxAmount";
			zCalcEditColumnStyleInfo2.IsReadOnly = true;
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo3.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("FCFD851D-FDEF-4161-9E55-909019A70164", "Total Amount");
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.ColumnName = "TotalAmount";
			zCalcEditColumnStyleInfo3.IsReadOnly = true;
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo5.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("3F1E8983-4908-4351-882F-6236033BE3DC", "Local Amount");
			zCalcEditColumnStyleInfo5.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo5.ColumnName = "LocalAmount";
			zCalcEditColumnStyleInfo5.IsReadOnly = true;
			zCalcEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo6.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("AE7A2600-7E89-47FA-B1CD-849D8B48EF05", "Local Tax Amount");
			zCalcEditColumnStyleInfo6.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo6.ColumnName = "LocalTaxAmount";
			zCalcEditColumnStyleInfo6.IsReadOnly = true;
			zCalcEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo7.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("A4E0A4AF-75F9-48BC-A092-5EB6A08DB254", "Local Total");
			zCalcEditColumnStyleInfo7.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo7.ColumnName = "LocalTotalAmount";
			zCalcEditColumnStyleInfo7.IsReadOnly = true;
			zCalcEditColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.ComplianceDocumentLineGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.ComplianceDocumentLineGrid.ColumnStyles.Add(zMultiLineTextBoxColumnInfo1);
			this.ComplianceDocumentLineGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.ComplianceDocumentLineGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.ComplianceDocumentLineGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.ComplianceDocumentLineGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.ComplianceDocumentLineGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo5);
			this.ComplianceDocumentLineGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo6);
			this.ComplianceDocumentLineGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo7);
			this.ComplianceDocumentLineGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ComplianceDocumentLineGrid.GridId = "FDE6618A-F47F-425A-B583-021D2536EF24";
			this.ComplianceDocumentLineGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ComplianceDocumentLineGrid.LayoutKey = "ComplianceDocumentLineGrid";
			this.ComplianceDocumentLineGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.ComplianceDocumentLineGrid.Name = "ComplianceDocumentLineGrid";
			this.ComplianceDocumentLineGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1002, 271, true);
			this.ComplianceDocumentLineGrid.TabIndex = 3;
			this.ComplianceDocumentLineSummaryTabPage.PerformLayout();
			((ISupportInitialize)(this.ComplianceDocumentLineGrid)).EndInit();
			this.ComplianceDocumentLineGrid.ResumeLayout(false);
			this.ComplianceDocumentLineGrid.PerformLayout();
			this.ComplianceDocumentLineSummaryTabPage.ResumeLayout(true);
		}

		void TransactionLineSummaryTabPage_InitializeTab(object sender, EventArgs e)
		{
			//
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			//
			ZMultiLineTextBoxColumnInfo zMultiLineTextBoxColumnInfo1 = new ZMultiLineTextBoxColumnInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo5 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo6 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo8 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo9 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo10 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo11 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo12 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo13 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo14 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo15 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo16 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo17 = new ZCalcEditColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo10 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo11 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo12 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo13 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo14 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo15 = new ZTextBoxColumnStyleInfo();
			ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new ZDateEditColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo16 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo17 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo18 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo19 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo20 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo21 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo22 = new ZTextBoxColumnStyleInfo();

			this.TransactionLineGrid = new ZGrid();
			this.TransactionLineSummaryTabPage.SuspendLayout();
			((ISupportInitialize)(this.TransactionLineGrid)).BeginInit();
			this.TransactionLineGrid.SuspendLayout();
			this.TransactionLineSummaryTabPage.Controls.Add(this.TransactionLineGrid);
			//
			// TransactionLineGrid
			//
			this.TransactionLineGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.TransactionLineGrid, "LineSummaries");
			this.TransactionLineGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo16.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("29DB9AF2-E1DD-4B8B-8447-705EDCD3DC3B", "Charge");
			zTextBoxColumnStyleInfo16.ColumnName = "AL_AC";
			zTextBoxColumnStyleInfo16.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zMultiLineTextBoxColumnInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("35050CF8-164C-48BC-A9F5-B41AAEC41CA2", "Description");
			zMultiLineTextBoxColumnInfo1.ColumnName = "AL_Desc";
			zMultiLineTextBoxColumnInfo1.MinimumEditControlWidth = 300;
			zMultiLineTextBoxColumnInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140);
			zTextBoxColumnStyleInfo17.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("0C72D2F2-87B0-4925-B2AC-062E3D360BFA", "Branch");
			zTextBoxColumnStyleInfo17.ColumnName = "AL_GB";
			zTextBoxColumnStyleInfo17.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo18.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("5C921204-FBF7-4CBE-8BD4-318627CED67F", "Dept");
			zTextBoxColumnStyleInfo18.ColumnName = "AL_GE";
			zTextBoxColumnStyleInfo18.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo20.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("A32EE099-30AE-40DC-BADA-1404891A4AAA", "Tax ID");
			zTextBoxColumnStyleInfo20.ColumnName = "AL_AT";
			zTextBoxColumnStyleInfo20.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zDateEditColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("63C24D08-7F59-4F94-8BA5-05FACC5BC03F", "Tax Date");
			zDateEditColumnStyleInfo1.ColumnName = "AL_TaxDate";
			zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo15.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("1B94E0F9-7A7E-45C6-A147-2D2B68FFF9C2", "Posting Group");
			zTextBoxColumnStyleInfo15.ColumnName = "PostingGroup";
			zTextBoxColumnStyleInfo15.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo22.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("15DG9GT2-E1EE-1A0B-1236-412EDCD3DC4c", "GL Account");
			zTextBoxColumnStyleInfo22.ColumnName = "AL_AG";
			zTextBoxColumnStyleInfo22.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo19.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("FAA6AD26-40B7-41F7-B6BD-BA96715484B6", "Cur.", "Currency", "Line Currency.");
			zTextBoxColumnStyleInfo19.ColumnName = "AL_RX_NKTransactionCurrency";
			zTextBoxColumnStyleInfo19.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo5.BindToDecimalPlaces = "AL_ExchangeRate_Decimals";
			zCalcEditColumnStyleInfo5.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("89FE6A9F-360E-4608-86AF-99B8C595BC1B", "Exch.", "Ex. Rate", "Exchange Rate", "Line Exchange Rate.");
			zCalcEditColumnStyleInfo5.ColumnName = "AL_ExchangeRate";
			zCalcEditColumnStyleInfo5.Decimals = 4;
			zCalcEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo6.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo6.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("1C693E9F-AAA1-4D8D-B292-F19BEC67F603", "Amount");
			zCalcEditColumnStyleInfo6.ColumnName = "AL_OSExTaxAmount";
			zCalcEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo8.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo8.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("849F571D-B68F-4EFF-BCBB-2071330F80E1", "Tax Amount");
			zCalcEditColumnStyleInfo8.ColumnName = "AL_OSTaxAmount";
			zCalcEditColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo10.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo10.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("435AE16C-E051-4944-AAF5-FB4EEF4FB706", "Local Amount");
			zCalcEditColumnStyleInfo10.ColumnName = "AL_LocalExTaxAmount";
			zCalcEditColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo11.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo11.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("65E19807-7864-4765-AD21-DF1F1C7F3EC7", "Local Tax Amount");
			zCalcEditColumnStyleInfo11.ColumnName = "AL_LocalTaxAmount";
			zCalcEditColumnStyleInfo11.IsReadOnly = true;
			zCalcEditColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo9.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo9.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("04D98012-F674-4399-81EB-113B759FC5D3", "Local Total");
			zCalcEditColumnStyleInfo9.ColumnName = "AL_LocalTotalAmount";
			zCalcEditColumnStyleInfo9.IsReadOnly = true;
			zCalcEditColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo10.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("0DB9056A-B831-42E8-89BA-A3F07AC983CC", "Ledger");
			zTextBoxColumnStyleInfo10.ColumnName = "Ledger";
			zTextBoxColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo11.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("1B345582-538F-4400-B5AB-02834C9B8BE4", "Transaction Type");
			zTextBoxColumnStyleInfo11.ColumnName = "TransactionType";
			zTextBoxColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo12.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("4B5D762A-3444-4B04-95DE-AFF5D8C2E358", "Transaction Number");
			zTextBoxColumnStyleInfo12.ColumnName = "TransactionNum";
			zTextBoxColumnStyleInfo12.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo21.ColumnName = "AL_A9_VATClass";
			zTextBoxColumnStyleInfo21.IsVisible = false;
			zTextBoxColumnStyleInfo21.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo14.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("A95FCDCE-0EC1-4A2F-98BA-982DDE583067", "Tax Basis", "Tax Reporting Basis", "");
			zTextBoxColumnStyleInfo14.ColumnName = "TaxReportingBasisHumanReadableName";
			zTextBoxColumnStyleInfo14.IsVisible = false;
			zTextBoxColumnStyleInfo14.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo17.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo17.ColumnName = "AL_Calc_InputGSTVATRecoverablePercentage";
			zCalcEditColumnStyleInfo17.IsVisible = false;
			zCalcEditColumnStyleInfo12.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo12.ColumnName = "AL_OSTaxAmount_Recoverable";
			zCalcEditColumnStyleInfo12.IsVisible = false;
			zCalcEditColumnStyleInfo12.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo13.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo13.ColumnName = "AL_OSTaxAmount_NotRecoverable";
			zCalcEditColumnStyleInfo13.IsVisible = false;
			zCalcEditColumnStyleInfo13.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo14.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo14.ColumnName = "AL_LocalTaxAmount_NotRecoverable";
			zCalcEditColumnStyleInfo14.IsVisible = false;
			zCalcEditColumnStyleInfo14.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo15.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo15.ColumnName = "AL_LocalTaxAmount_Recoverable";
			zCalcEditColumnStyleInfo15.IsVisible = false;
			zCalcEditColumnStyleInfo15.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo16.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("79A3E24E-B67A-402C-B1AC-E1C91D363EE0", "Extra Tax");
			zCalcEditColumnStyleInfo16.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo16.ColumnName = "AL_OSExtraTaxAmount";
			zCalcEditColumnStyleInfo16.IsReadOnly = true;
			zCalcEditColumnStyleInfo16.IsVisible = false;
			zCalcEditColumnStyleInfo16.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo13.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("32D8777D-34FB-424A-ACF7-FEC6317C1A51", "Govt Charge Code", "Government Charge Code", "Line Government Charge Code");
			zTextBoxColumnStyleInfo13.ColumnName = "AL_GovtChargeCode";
			zTextBoxColumnStyleInfo13.IsVisible = false;
			zTextBoxColumnStyleInfo13.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			this.TransactionLineGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo16);
			this.TransactionLineGrid.ColumnStyles.Add(zMultiLineTextBoxColumnInfo1);
			this.TransactionLineGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo17);
			this.TransactionLineGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo18);
			this.TransactionLineGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo20);
			this.TransactionLineGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.TransactionLineGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo15);
			this.TransactionLineGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo22);
			this.TransactionLineGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo19);
			this.TransactionLineGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo5);
			this.TransactionLineGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo6);
			this.TransactionLineGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo8);
			this.TransactionLineGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo10);
			this.TransactionLineGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo11);
			this.TransactionLineGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo9);
			this.TransactionLineGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo10);
			this.TransactionLineGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo11);
			this.TransactionLineGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo12);
			this.TransactionLineGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo21);
			this.TransactionLineGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo14);
			this.TransactionLineGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo17);
			this.TransactionLineGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo12);
			this.TransactionLineGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo13);
			this.TransactionLineGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo14);
			this.TransactionLineGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo15);
			this.TransactionLineGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo16);
			this.TransactionLineGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo13);
			this.TransactionLineGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TransactionLineGrid.GridId = "A462340B-BA51-47A3-8759-498F548CECEE";
			this.TransactionLineGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.TransactionLineGrid.LayoutKey = "TransactionLineGrid";
			this.TransactionLineGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.TransactionLineGrid.Name = "TransactionLineGrid";
			this.TransactionLineGrid.ReadOnly = true;
			this.TransactionLineGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1002, 271, true);
			this.TransactionLineGrid.TabIndex = 4;
			this.TransactionLineSummaryTabPage.PerformLayout();
			((ISupportInitialize)(this.TransactionLineGrid)).EndInit();
			this.TransactionLineGrid.ResumeLayout(false);
			this.TransactionLineGrid.PerformLayout();
			this.TransactionLineSummaryTabPage.ResumeLayout(true);
		}

		public void SetSpecialVoidingPanel()
		{
			var isAllowedSpecialVoid = ((ObjectFactory.Get<IGlobalAccountingCountryFactory>().GetCountryFactory(GlbCompany.CurrentCompany.GC_RN_NKCountryCode) as IInstanceProvider<IComplianceDocumentVoidingProvider>)?.Get())?.IsAllowedSpecialVoid(DataSource as AccComplianceDocumentHeader) ?? false;
			if (isAllowedSpecialVoid)
			{
				specialVoidingPanel.Visible = true;
				specialVoidingPanel.SetReadOnlyIncludingChildren(false);
			}
		}
	}
}

