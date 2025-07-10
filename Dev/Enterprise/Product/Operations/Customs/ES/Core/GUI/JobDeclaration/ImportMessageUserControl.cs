using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.ES.GUI;
public partial class ImportMessageUserControl : MessageUserControl
{
	public ImportMessageUserControl(JobDeclaration jobDeclaration) : base(jobDeclaration)
	{
		InitializeComponent();
		SetupEntryHeaderColumns();
	}

	protected override EU.GUI.EntryLineAdditionalDataUserControl GetEntryLineAdditionalData() => IsUCC6 ? new UCC6EntryLineAdditionalDataUserControl() : new ImportEntryLineAdditionalDataUserControl();

	void SetupEntryHeaderColumns()
	{
		EntriesBoundGrid.ReadOnly = false;
		EntriesBoundGrid.ColumnStyles.AddRange(new Core.Forms.ZGridColumnInfo[]
		{
			new ZTextBoxColumnStyleInfo
			{
				CaptionResourceString = Res.GetData("1B4AAA43-9E52-408C-A795-E359A10545B8", "CSV Import Certificate"),
				ColumnName = CusEntryHeader.Schema.ZG_CSVImportCertificate,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130),
			},
			new ZTextBoxColumnStyleInfo
			{
				CaptionResourceString = Res.GetData("F3F0BBEA-83A5-4B38-ADE8-226CA869F681", "Export MRN"),
				ColumnName = CusEntryHeader.Schema.ZG_ExportMRN,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130),
			},
			new ZDateEditColumnStyleInfo
			{
				CaptionResourceString = Res.GetData("0C337950-479A-4FF6-94F8-BEFC507D4DF1", "Limit payment date"),
				ColumnName = CusEntryHeader.Schema.ZG_LimitPaymentDate,
				IsMandatory = false,
				DateTimeFormat = ZArchitecture.Core.ZDateTimePickerFormat.Short,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130)
			},
			new ZDateEditColumnStyleInfo
			{
				CaptionResourceString = Res.GetData("56E10CEE-C7E2-4FB5-97D7-9E9D2268CE2C", "ATC Limit payment date"),
				ColumnName = CusEntryHeader.Schema.ZG_ATCLimitPaymentDate,
				IsMandatory = false,
				DateTimeFormat = ZArchitecture.Core.ZDateTimePickerFormat.Short,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140)
			},
			new ZTextBoxColumnStyleInfo
			{
				CaptionResourceString = Res.GetData("5B1264F7-F0AB-404E-B17B-ED58F574F38D", "Payment Proof Number"),
				ColumnName = CusEntryHeader.Schema.ZG_PaymentProofNumber,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140),
			},
			new ZTextBoxColumnStyleInfo
			{
				CaptionResourceString = Res.GetData("D5507096-199A-4ADB-B5EC-4E599490AFBC", "ATC Payment Proof Number"),
				ColumnName = CusEntryHeader.Schema.ZG_ATCPaymentProofNumber,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160),
			},
			new ZCheckBoxColumnStyleInfo
			{
				CaptionResourceString = Res.GetData("1909503A-62F8-48A3-ACC0-07B74ADE90DA", "Parallel"),
				ColumnName = CusEntryHeader.Schema.ZG_Parallel,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80),
			},
			new ZTextBoxColumnStyleInfo
			{
				CaptionResourceString = Res.GetData("DBBB2EE2-4768-47C8-8237-2DCB3A7E5134", "T2L Clearance MRN"),
				ColumnName = CusEntryHeader.Schema.T2CMovementReferenceNumber,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80)
			},
		});
	}
}
