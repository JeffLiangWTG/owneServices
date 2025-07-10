using Enterprise.Customs.DE.Business.Declaration;

namespace Enterprise.Customs.DE.GUI
{
	public partial class ImportMessageUserControl : EU.GUI.MessageUserControl
	{
		public ImportMessageUserControl()
		{
			InitializeComponent();
			SetupEntryHeaderColumns();
			SetupEntryLineColumns();
		}

		protected override EU.GUI.EntryLineAdditionalDataUserControl GetEntryLineAdditionalData() => new EU.GUI.UCC6EntryLineAdditionalDataUserControl();

		protected override bool SupportWarehouseTransactionStatusColumns => true;

		void SetupEntryHeaderColumns()
		{
			EntriesBoundGrid.ColumnStyles.AddRange(new Core.Forms.ZGridColumnInfo[]
			{
				new ZArchitecture.ZTextBoxColumnStyleInfo
				{
					CaptionResourceString = Res.GetData("05C65E3A-9032-4DDD-BB99-C49FF79BE05A", "Declaration Type"),
					ColumnName = CusEntryHeader.Schema.Style,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100),
				},
				new ZArchitecture.ZTextBoxColumnStyleInfo
				{
					CaptionResourceString = Res.GetData("A4E2F164-DCAC-40A9-8026-A7C3CD95B7CB", "Sub style"),
					ColumnName = CusEntryHeader.Schema.SubStyle,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80),
				},
				new ZArchitecture.ZTextBoxColumnStyleInfo
				{
					CaptionResourceString = Res.GetData("38DCB780-BF3B-46D2-9FDE-7CDDE56EB88F", "Description"),
					ColumnName = CusEntryHeader.Schema.Description,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80),
				},
				new ZArchitecture.ZTextBoxColumnStyleInfo
				{
					CaptionResourceString = Res.GetData("B9D5B961-9F5E-4908-8CF3-B0235AE067D1", "LRN"),
					ColumnName = CusEntryHeader.Schema.LocalReferenceNumber,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100),
					IsReadOnly = true,
				}
			});
			EntriesBoundGrid.GetColumnStyle(CusEntryHeader.Schema.CH_WarehouseTransactionStatus).IsVisible = true;
			EntriesBoundGrid.ColumnStyles.Remove(EntriesBoundGrid.GetColumnStyle(CusEntryHeader.Schema.CH_HasManualWhsUpdate));
		}

		void SetupEntryLineColumns()
		{
			EntryLineGrid.ColumnStyles.AddRange(new Core.Forms.ZGridColumnInfo[]
			{
				new ZArchitecture.ZTextBoxColumnStyleInfo
				{
					ColumnName = nameof(CusEntryLine.ZG_CustomsStatus),
					CaptionResourceString = Res.GetData("ADFBB591-9FD1-4956-9FF4-975327E1E3F4", "Customs Status"),
					IsReadOnly = true,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(85)
				},
				new ZArchitecture.ZTextBoxColumnStyleInfo
				{
					ColumnName = nameof(CusEntryLine.CustomsStatusDescription),
					CaptionResourceString = Res.GetData("6C5A0155-3703-43F5-9836-AF4AFAC6EBB3", "Customs Status Description"),
					IsReadOnly = true,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(145)
				}
			});

			EntryLineGrid.ReOrderColumns(ColumnsInOrder);
		}

		string[] ColumnsInOrder
		{
			get
			{
				if (columnNamesInSortOrder == null)
				{
					columnNamesInSortOrder = new[]
					{
						Customs.Business.AutoCusEntryLine.Schema.CL_LineNumber,
						Customs.Business.CusEntryLine.Schema.LineSubmissionStatusDescription,
						nameof(CusEntryLine.ZG_CustomsStatus),
						nameof(CusEntryLine.CustomsStatusDescription),
						Customs.Business.CusEntryLine.Schema.FormattedTariff,
						Customs.Business.CusEntryLine.Schema.EffectiveDescription,
						Customs.Business.CusEntryLine.Schema.DutyAmount,
						Customs.Business.AutoCusEntryLine.Schema.CL_DutyPercent,
						Customs.Business.CusEntryLine.Schema.GSTVATAmount,
						Customs.Business.CusEntryLine.Schema.GSTVATDeferred,
						Customs.Business.AutoCusEntryLine.Schema.CL_CustomsValue,
						Customs.Business.AutoCusEntryLine.Schema.CL_StatisticalValue,
					};
				}
				return columnNamesInSortOrder;
			}
		}
		string[] columnNamesInSortOrder;
	}
}
