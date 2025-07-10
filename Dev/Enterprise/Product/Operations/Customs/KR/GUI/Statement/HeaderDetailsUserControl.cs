using System;
using Enterprise.Customs.KR.Business;
using Enterprise.Customs.KR.Messaging;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public partial class HeaderDetailsUserControl : ZUserControl
	{
		public HeaderDetailsUserControl()
		{
			InitializeComponent();
		}

		public new CusStatementHeader CurrentDataItem => base.CurrentDataItem as CusStatementHeader;

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			SetDynamicHeaderDetailsPanelLayout();
			UpdateGroupBoxCaptionsAndVisiblities();
		}

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);
			UpdateEntryGridColumns();
		}

		void SetDynamicHeaderDetailsPanelLayout()
		{
			if (CurrentDataItem != null)
			{
				switch (CurrentDataItem.B2_StatementType)
				{
					case StatementHeaderTypeList.Codes.Normal:
						DynamicHeaderDetailsPanel.UpdateLayout(new ChargeBillLayouts());
						break;

					case StatementHeaderTypeList.Codes.NormalReport:
						DynamicHeaderDetailsPanel.UpdateLayout(new ChargeInvoiceLayouts());
						break;

					case StatementHeaderTypeList.Codes.MonthlyReceipt:
						DynamicHeaderDetailsPanel.UpdateLayout(new MonthlyInvoiceVATLayout());
						break;

					case StatementHeaderTypeList.Codes.IndividualCollectionReceipt:
						DynamicHeaderDetailsPanel.UpdateLayout(new MonthlyInvoiceVATLayout());
						break;

					case StatementHeaderTypeList.Codes.Invoice:
						DynamicHeaderDetailsPanel.UpdateLayout(new ChargeMonthlyBillLayout());
						break;

					case StatementHeaderTypeList.Codes.CustomsDisbursementBill:
						DynamicHeaderDetailsPanel.UpdateLayout(new DisbursementBillsLayout());
						break;

					default:
						break;
				}
			}
		}

		void UpdateGroupBoxCaptionsAndVisiblities()
		{
			EntryGroupBox.Text = Res.GetString("4D64B6EF-1318-41B8-A6AA-660EEFAAF7CC", "Related Entries");
			if (CurrentDataItem != null)
			{
				switch (CurrentDataItem.B2_StatementType)
				{
					case StatementHeaderTypeList.Codes.Normal:
						HeaderDetailsGroupBox.Text = StatementHeaderTypeList.GetFormName(StatementHeaderTypeList.Codes.Normal);
						HeaderDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(890, 230, true);
						EntryGroupBox.Visible = false;
						splitter.Visible = false;
						FeesGroupBox.Text = Res.GetString("F27E9230-3DD2-4874-ACE4-8DE069244D0B", "Fees");
						FeesGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
						AmountDetailsGroupBox.Visible = false;
						break;

					case StatementHeaderTypeList.Codes.NormalReport:
						HeaderDetailsGroupBox.Text = StatementHeaderTypeList.GetFormName(StatementHeaderTypeList.Codes.NormalReport);
						HeaderDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(890, 120, true);
						EntryGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(400, 170, true);
						FeesGroupBox.Text = Res.GetString("F27E9230-3DD2-4874-ACE4-8DE069244D0B", "Fees");
						AmountDetailsGroupBox.Visible = false;
						break;

					case StatementHeaderTypeList.Codes.MonthlyReceipt:
						HeaderDetailsGroupBox.Text = StatementHeaderTypeList.GetFormName(StatementHeaderTypeList.Codes.MonthlyReceipt);
						HeaderDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(890, 190, true);
						EntryGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
						FeesGroupBox.Visible = false;
						AmountDetailsGroupBox.Visible = false;
						break;

					case StatementHeaderTypeList.Codes.IndividualCollectionReceipt:
						HeaderDetailsGroupBox.Text = StatementHeaderTypeList.GetFormName(StatementHeaderTypeList.Codes.IndividualCollectionReceipt);
						HeaderDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(890, 190, true);
						EntryGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
						FeesGroupBox.Visible = false;
						AmountDetailsGroupBox.Visible = false;
						break;

					case StatementHeaderTypeList.Codes.Invoice:
						HeaderDetailsGroupBox.Text = StatementHeaderTypeList.GetFormName(StatementHeaderTypeList.Codes.Invoice);
						HeaderDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(890, 170, true);
						EntryGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(520, 170, true);
						FeesGroupBox.Text = Res.GetString("BF9A8E05-2B77-4E88-9DF8-11E526131FE4", "Entry Charges");
						AmountDetailsGroupBox.Visible = false;
						break;

					case StatementHeaderTypeList.Codes.CustomsDisbursementBill:
						HeaderDetailsGroupBox.Text = Res.GetString("B7CB4B14-CE22-43BF-AB58-7DD220195BCB", "Customs Individual Disbursement Bills");
						HeaderDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(890, 200, true);
						EntryGroupBox.Visible = false;
						splitter.Visible = false;
						FeesGroupBox.Visible = false;
						AmountDetailsGroupBox.Text = Res.GetString("2E1456B1-4E1C-4393-8028-43C54317F6D8", "Amount Details");
						AmountDetailsPanel.UpdateLayout(new DisbursementBillsLinesLayout());
						break;

					default:
						break;
				}
			}
		}

		void UpdateEntryGridColumns()
		{
			if (CurrentDataItem != null)
			{
				switch (CurrentDataItem.B2_StatementType)
				{
					case StatementHeaderTypeList.Codes.NormalReport:
						EntriesGrid.RemoveFromAvailableColumns(
							CusStatementLine.Schema.B3_SequenceNumber,
							nameof(CusStatementLine.FormattedLinePaymentNumber),
							nameof(CusStatementLine.BaseAmount),
							CusStatementLine.Schema.B3_EntryDate);

						EntriesGrid.SetColumnVisible(true, CusStatementLine.Schema.B3_EntryType);
						break;

					case StatementHeaderTypeList.Codes.Invoice:
						EntriesGrid.RemoveFromAvailableColumns(
							nameof(CusStatementLine.BaseAmount),
							CusStatementLine.Schema.B3_EntryDate);

						EntriesGrid.SetColumnVisible(false, CusStatementLine.Schema.B3_EntryType);
						break;

					case StatementHeaderTypeList.Codes.MonthlyReceipt:
					case StatementHeaderTypeList.Codes.IndividualCollectionReceipt:
						EntriesGrid.SetColumnVisible(false, [CusStatementLine.Schema.B3_EntryType, CusStatementLine.Schema.B3_EntryDate]);
						break;
				}
			}
		}
	}
}
