using System;
using System.Linq;
using System.Windows.Forms;
using Enterprise.Core.Forms;
using Enterprise.Customs.Business;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.IL.GUI
{
	public partial class EntriesTabUserControl : ImportMessageUserControl
	{
		public EntriesTabUserControl()
		{
			InitializeComponent();

			SetupEntryFeesTab();
			LoadEntryLineAdditionalDataUserControl();
			SetupEntryLineGrid();
		}

		protected override Type GetBaseMessagesTabUserControlType() => typeof(CustomsMessagingControl);

		protected override string MessagesUserControlBindingPath => "CustomsEntryHeaders";

		protected EntryLineAdditionalDataUserControl EntryLineAdditionalDataUserControl => entryLineAdditionalDataUserControl ?? (entryLineAdditionalDataUserControl = new EntryLineAdditionalDataUserControl());
		EntryLineAdditionalDataUserControl entryLineAdditionalDataUserControl;

		void LoadEntryLineAdditionalDataUserControl()
		{
			EntryLineAdditionalDataUserControl.AllowDrop = true;
			EntryLineAdditionalDataUserControl.Dock = System.Windows.Forms.DockStyle.Bottom;
			EntryLineAdditionalDataUserControl.Name = nameof(Enterprise.Customs.IL.GUI.EntryLineAdditionalDataUserControl);
			EntryLineAdditionalDataUserControl.TabIndex = 2;
			EntryLineAdditionalDataUserControl.Visible = true;
			EntryLinesTabPage.Controls.Add(EntryLineAdditionalDataUserControl);
		}

		void SetupEntryFeesTab()
		{
			var entryFeePanel = new ZPanel();
			entryFeePanel.Dock = DockStyle.Fill;
			EntryFeesTabPage.Controls.Add(entryFeePanel);
			entryFeePanel.Controls.Add(SetupEntryFeesGrid());
			entryFeePanel.Controls.Add(SetupEntryConfirmedChargesGrid());
		}

		ZGrid SetupEntryFeesGrid()
		{
			var entryFeeGrid = new ZGrid();
			entryFeeGrid.GridId = "86101D1A-FB67-4228-AA23-10E40104FC76";
			entryFeeGrid.Name = "EntryFeesGrid";
			entryFeeGrid.BindTo = "CustomsEntryHeaders.Charges";
			entryFeeGrid.CaptionText = Res.GetString("2FC7B923-46AC-4566-B23A-5925E629E36A", "           Calculated");
			entryFeeGrid.CaptionVisible = true;
			entryFeeGrid.CaptionBackColor = System.Drawing.Color.WhiteSmoke;

			entryFeeGrid.ColumnStyles.AddRange(GetEntryChargesColumnInfos());

			entryFeeGrid.ColumnStyles.Add(new ZDropEditColumnStyleInfo
			{
				ColumnName = CusEntryHeaderChargesSchema.Constants.C1_RateOverrideReasonCode,
				CaptionResourceString = Res.GetData("IL.CusEntryHeaderCharges.C1_RateOverrideReasonCode", "Action"),
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80)
			});

			entryFeeGrid.Anchor = (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right);
			entryFeeGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			entryFeeGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(840, 220, true);

			return entryFeeGrid;
		}

		ZGrid SetupEntryConfirmedChargesGrid()
		{
			var entryConfirmedChargesGrid = new ZGrid();
			entryConfirmedChargesGrid.GridId = "4E4CFF57-D0F3-4304-AB2A-13D778A855B8";
			entryConfirmedChargesGrid.Name = "ConfirmedFeesGrid";
			entryConfirmedChargesGrid.BindTo = "CustomsEntryHeaders.ConfirmedCharges";
			entryConfirmedChargesGrid.CaptionText = Res.GetString("5E1B0AE6-5803-42A0-B3D8-D8728B46AC8D", "           Confirmed");
			entryConfirmedChargesGrid.CaptionVisible = true;
			entryConfirmedChargesGrid.CaptionBackColor = System.Drawing.Color.WhiteSmoke;
			entryConfirmedChargesGrid.ReadOnly = true;

			entryConfirmedChargesGrid.ColumnStyles.AddRange(GetEntryChargesColumnInfos());

			entryConfirmedChargesGrid.Anchor = ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right);
			entryConfirmedChargesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 235, true);
			entryConfirmedChargesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(840, 220, true);

			return entryConfirmedChargesGrid;
		}

		ZGridColumnInfo[] GetEntryChargesColumnInfos()
		{
			return new ZGridColumnInfo[]
			{
				new ZTextBoxColumnStyleInfo
				{
					ColumnName = "C1_ChargeType",
					CaptionResourceString = Res.GetData("IL.CusEntryHeaderCharges.C1_ChargeType", "Fee Code"),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90),
				},
				new ZTextBoxColumnStyleInfo
				{
					ColumnName = "C1_ChargeAmount",
					CaptionResourceString = Res.GetData("IL.CusEntryHeaderCharges.C1_ChargeAmount", "Amount"),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90),
				},
				new ZTextBoxColumnStyleInfo
				{
					ColumnName = "C1_MethodOfPayment",
					CaptionResourceString = Res.GetData("IL.CusEntryHeaderCharges.C1_MethodOfPayment", "Method Of Payment"),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150),
				}
			};
		}

		void SetupEntryLineGrid()
		{
			var columnStyles = EntryLineGrid.ColumnStyles;

			var gridColumnLineNumber = columnStyles.Cast<ZGridColumnInfo>().Single(x => x.ColumnName == CusEntryLine.Schema.CL_LineNumber);
			gridColumnLineNumber.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(75);
			gridColumnLineNumber.CaptionResourceString = Res.GetData("6FFB618E-8E8E-40E6-9424-2C9A6C34DF47", "Line Number");

			var groupInvoiceAmount = Res.GetData("IL.GUI.EntriesTabUserControl|F8345FD7-0E62-4FD8-B074-8161159ACA99", "Invoice Amount");
			columnStyles.AddRange(new ZGridColumnInfo[]
			{
				new ZTextBoxColumnStyleInfo
				{
					ColumnName = Business.CusEntryLine.Schema.LineSubmissionStatusDescription,
					CaptionResourceString = Res.GetData("3C2C3FE1-EC9D-49C4-8A43-8B50230C93F4", "Status"),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80),
					IsVisible = true,
					IsReadOnly = true
				},
				new ZTextBoxColumnStyleInfo
				{
					ColumnName = CusEntryLine.Schema.CL_AdValoremTariff,
					CaptionResourceString = Res.GetData("10950912-9756-43D7-B34D-B968D252FF79", "Tariff"),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80),
					IsVisible = true,
					IsReadOnly = true
				},
				new ZTextBoxColumnStyleInfo
				{
					ColumnName = CusEntryLine.Schema.CL_Description,
					CaptionResourceString = Res.GetData("AC103B9B-5767-4D4B-B61F-DB0C1BD1977F", "Description"),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120),
					IsVisible = true,
					IsReadOnly = true
				},
				new ZCalcEditColumnStyleInfo
				{
					ColumnName = CusEntryLine.Schema.CL_ConfirmedCustomsValue,
					CaptionResourceString = Res.GetData("318A32D7-ED58-49A3-BEDD-CA905BCF9356", "Customs Value"),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100),
					IsVisible = true,
					IsReadOnly = true
				},
				new ZCalcEditColumnStyleInfo
				{
					ColumnName = CusEntryLine.Schema.CL_ConfirmedStatisticalValue,
					CaptionResourceString = Res.GetData("Statistical Value", "Statistical Value"),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100),
					IsVisible = true,
					IsReadOnly = true
				},
				new ZCalcEditColumnStyleInfo
				{
					GroupName = groupInvoiceAmount,
					ColumnName = CusEntryLine.Schema.CL_InvoiceAmount,
					CaptionResourceString = Res.GetData("DD18A4FD-FB6B-4058-B214-13D0E6326EA7", "Invoice Amount"),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100),
					IsVisible = false,
					IsReadOnly = true
				},
				new ZCodeFindBoxColumnStyleInfo
				{
					GroupName = groupInvoiceAmount,
					ColumnName = CusEntryLine.Schema.CL_RX_NKInvoiceAmountCurrency,
					CaptionResourceString = Res.GetData("929C2A1D-3A2F-45C0-840C-55F23614CB11", "Currency"),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60),
					IsVisible = false,
					IsReadOnly = true
				},
				new ZDateEditColumnStyleInfo
				{
					ColumnName = CusEntryLine.Schema.CL_SystemLastEditTimeUtc,
					CaptionResourceString = Res.GetData("08FB9891-ACC6-4C4A-97ED-2A363F8117DC", "Last Edited Time (UTC)"),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(125),
					IsVisible = false,
					IsReadOnly = true
				}
			});
		}
	}
}
