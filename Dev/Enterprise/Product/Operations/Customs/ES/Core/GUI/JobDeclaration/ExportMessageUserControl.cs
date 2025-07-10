using Enterprise.Customs.ES.Business.Declaration;

namespace Enterprise.Customs.ES.GUI
{
	public partial class ExportMessageUserControl : MessageUserControl
	{
		public ExportMessageUserControl()
		{
			InitializeComponent();
			SetupEntryHeaderColumns();
		}

		protected override EU.GUI.EntryLineAdditionalDataUserControl GetEntryLineAdditionalData() => new ExportEntryLineAdditionalDataUserControl();

		void SetupEntryHeaderColumns()
		{
			EntriesBoundGrid.ColumnStyles.AddRange(new Core.Forms.ZGridColumnInfo[]
			{
				new ZArchitecture.ZDateEditColumnStyleInfo
				{
					CaptionResourceString = Res.GetData("C822CD2F-C9E6-4336-9903-98D288BB2314", "Limit Date of Arrival"),
					ColumnName = CusEntryHeader.Schema.ZG_LimitDateOfArrival,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120),
					DateTimeFormat = ZArchitecture.Core.ZDateTimePickerFormat.Short,
				},
				new ZArchitecture.ZTextBoxColumnStyleInfo
				{
					CaptionResourceString = Res.GetData("75CA6478-6994-4FBD-90B7-12157B18A875", "Clearance Result"),
					ColumnName = CusEntryHeader.Schema.FormattedClearanceResult,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120),
				},
				new ZArchitecture.ZTextBoxColumnStyleInfo
				{
					CaptionResourceString = Res.GetData("31A20FA1-E6C1-4FBA-AEA0-9CCEFFAE29AC", "EAD Print Procedure"),
					ColumnName = CusEntryHeader.Schema.FormattedEADPrint,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120),
				},
				new ZArchitecture.ZTextBoxColumnStyleInfo
				{
					CaptionResourceString = Res.GetData("9067E31F-DBC3-46CC-92D4-9F302A818810", "CSV T2L"),
					ColumnName = CusEntryHeader.Schema.ZG_CSVT2L,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80),
				},
				new ZArchitecture.ZCheckBoxColumnStyleInfo
				{
					CaptionResourceString = Res.GetData("44C4AF1A-3485-49F1-8633-8D869928667C", "Indirect Export"),
					ColumnName = CusEntryHeader.Schema.IndirectExport,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80),
				},
				new ZArchitecture.ZTextBoxColumnStyleInfo
				{
					CaptionResourceString = Res.GetData("04390DFE-7BDC-4511-B9B3-12498ABCE4BA", "CSV Exit Certificate"),
					ColumnName = CusEntryHeader.Schema.ZG_CSVExitCertificate,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120),
				},
			});
		}
	}
}
