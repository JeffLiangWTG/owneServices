using Enterprise.Customs.IT.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.GUI;

public partial class GroupedPreviousDocumentsUserControl : ZUserControl
{
	public GroupedPreviousDocumentsUserControl()
	{
		InitializeComponent();
		SetupM2LinesColumns();
	}

	void SetupM2LinesColumns()
	{
		M2LinesGrid.ColumnStyles.Add(new ZArchitecture.ZTextBoxColumnStyleInfo
		{
			ColumnName = GroupedPreviousDocument.Schema.EntryLineNumber,
			Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60),
			CaptionResourceString = Res.GetData("E4623322-33B0-453A-90CF-A5BA4669572A", "Line No."),
		});
		M2LinesGrid.ColumnStyles.Add(new ZArchitecture.ZTextBoxColumnStyleInfo
		{
			ColumnName = GroupedPreviousDocument.Schema.EntryLineCustomsStatusDescription,
			Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70),
			CaptionResourceString = Res.GetData("52DB80B0-B741-4468-917F-F98773C005D2", "Status"),
		});
		M2LinesGrid.ColumnStyles.Add(new ZArchitecture.ZTextBoxColumnStyleInfo
		{
			ColumnName = GroupedPreviousDocument.Schema.SummaryDeclarationDocumentRegister,
			Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(35),
			CaptionResourceString = Res.GetData("AE1D3039-A1B6-46C9-B16A-3ABBE4B84E96", "PA"),
		});
		M2LinesGrid.ColumnStyles.Add(new ZArchitecture.ZTextBoxColumnStyleInfo
		{
			ColumnName = GroupedPreviousDocument.Schema.SummaryDeclarationDocumentReferenceNumber,
			Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(78),
			CaptionResourceString = Res.GetData("A3F3A29D-10C4-4C40-9F0F-99AD6E48F442", "PA Number"),
		});
		M2LinesGrid.ColumnStyles.Add(new ZArchitecture.ZTextBoxColumnStyleInfo
		{
			ColumnName = GroupedPreviousDocument.Schema.SummaryDeclarationDocumentReferenceCIN,
			Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(45),
			CaptionResourceString = Res.GetData("4B1F38C9-0B22-4A69-840E-AA81C4DB398D", "PA CIN"),
		});
		M2LinesGrid.ColumnStyles.Add(new ZArchitecture.ZDateEditColumnStyleInfo
		{
			ColumnName = GroupedPreviousDocument.Schema.SummaryDeclarationDocumentDate,
			Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(78),
			CaptionResourceString = Res.GetData("7F956191-0AFA-4017-8989-B80A6056CAF7", "PA Date"),
			DateTimeFormat = ZArchitecture.Core.ZDateTimePickerFormat.Short
		});
		M2LinesGrid.ColumnStyles.Add(new ZArchitecture.ZTextBoxColumnStyleInfo
		{
			ColumnName = GroupedPreviousDocument.Schema.SummaryDeclarationDocumentSeries,
			Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60),
			CaptionResourceString = Res.GetData("EC11A0CB-4FD0-46CC-8BE2-B60BBA649B81", "PA Series"),
		});
		M2LinesGrid.ColumnStyles.Add(new ZArchitecture.ZTextBoxColumnStyleInfo
		{
			ColumnName = GroupedPreviousDocument.Schema.SummaryDeclarationDocumentCustomsOffice,
			Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(65),
			CaptionResourceString = Res.GetData("B04A9CFD-7EAE-4EF6-839C-FC5F911BE106", "PA Office"),
		});
		M2LinesGrid.ColumnStyles.Add(new ZArchitecture.ZTextBoxColumnStyleInfo
		{
			ColumnName = GroupedPreviousDocument.Schema.SummaryDeclarationDocumentMRN,
			Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(78),
			CaptionResourceString = Res.GetData("55AF4181-159A-4F1F-8B1B-6817B1ACB734", "PA MRN"),
		});
		M2LinesGrid.ColumnStyles.Add(new ZArchitecture.ZTextBoxColumnStyleInfo
		{
			ColumnName = GroupedPreviousDocument.Schema.SummaryDeclarationDocumentItemNumber,
			Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(95),
			CaptionResourceString = Res.GetData("5D2607F4-DE76-4922-A0AF-532A6E594869", "PA Item Number"),
		});

		M2LinesGrid.ColumnStyles.Add(new ZArchitecture.ZTextBoxColumnStyleInfo
		{
			ColumnName = GroupedPreviousDocument.Schema.PreviousProcedureDocumentRegister,
			Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(35),
			CaptionResourceString = Res.GetData("66CA8943-4F7A-4AC3-96C3-E159486EC4EF", "RP"),
		});
		M2LinesGrid.ColumnStyles.Add(new ZArchitecture.ZTextBoxColumnStyleInfo
		{
			ColumnName = GroupedPreviousDocument.Schema.PreviousProcedureDocumentReferenceNumber,
			Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(78),
			CaptionResourceString = Res.GetData("52B631FF-08C2-4C99-9366-A58CDF8A54A8", "RP Number"),
		});
		M2LinesGrid.ColumnStyles.Add(new ZArchitecture.ZTextBoxColumnStyleInfo
		{
			ColumnName = GroupedPreviousDocument.Schema.PreviousProcedureDocumentReferenceCIN,
			Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(45),
			CaptionResourceString = Res.GetData("21FC4024-D84B-4080-B8E6-A20BE3D83C7C", "RP CIN"),
		});
		M2LinesGrid.ColumnStyles.Add(new ZArchitecture.ZDateEditColumnStyleInfo
		{
			ColumnName = GroupedPreviousDocument.Schema.PreviousProcedureDocumentDate,
			Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(78),
			CaptionResourceString = Res.GetData("14E71B30-40DE-4777-8280-FB2ED4651A61", "RP Date"),
			DateTimeFormat = ZArchitecture.Core.ZDateTimePickerFormat.Short,
		});
		M2LinesGrid.ColumnStyles.Add(new ZArchitecture.ZTextBoxColumnStyleInfo
		{
			ColumnName = GroupedPreviousDocument.Schema.PreviousProcedureDocumentSeries,
			Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60),
			CaptionResourceString = Res.GetData("FD06AC3A-5B06-42A8-A172-452CF19D3A27", "RP Series"),
		});
		M2LinesGrid.ColumnStyles.Add(new ZArchitecture.ZTextBoxColumnStyleInfo
		{
			ColumnName = GroupedPreviousDocument.Schema.PreviousProcedureDocumentCustomsOffice,
			Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(65),
			CaptionResourceString = Res.GetData("E520C34F-1D48-401E-BE8B-4F8BDE758DEC", "RP Office"),
		});
		M2LinesGrid.ColumnStyles.Add(new ZArchitecture.ZTextBoxColumnStyleInfo
		{
			ColumnName = GroupedPreviousDocument.Schema.PreviousProcedureDocumentItemNumber,
			Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(95),
			CaptionResourceString = Res.GetData("DC252B69-B03E-4AA3-AE3F-6E05D3D16F10", "RP Item Number"),
		});

		M2LinesGrid.ColumnStyles.Add(new ZArchitecture.ZTextBoxColumnStyleInfo
		{
			ColumnName = GroupedPreviousDocument.Schema.PackageQuantity,
			Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(78),
			CaptionResourceString = Res.GetData("25D62E3F-00D2-4668-B5AC-F1B948BB8967", "Package Qty."),
		});
		M2LinesGrid.ColumnStyles.Add(new ZArchitecture.ZTextBoxColumnStyleInfo
		{
			ColumnName = GroupedPreviousDocument.Schema.GrossMass,
			Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(78),
			CaptionResourceString = Res.GetData("4DCF818D-937B-4036-B23D-7F93EA2E5019", "Gross Mass"),
		});
		M2LinesGrid.ColumnStyles.Add(new ZArchitecture.ZTextBoxColumnStyleInfo
		{
			ColumnName = GroupedPreviousDocument.Schema.Tariff,
			Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(78),
			CaptionResourceString = Res.GetData("23AA65AD-6F3B-4231-B929-C05FC7BFC104", "Tariff"),
		});
		M2LinesGrid.ColumnStyles.Add(new ZArchitecture.ZTextBoxColumnStyleInfo
		{
			ColumnName = GroupedPreviousDocument.Schema.NetMass,
			Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(78),
			CaptionResourceString = Res.GetData("F1F328F6-9FDD-4FB6-8182-18C886F562F2", "Net Mass"),
		});
		M2LinesGrid.ColumnStyles.Add(new ZArchitecture.ZTextBoxColumnStyleInfo
		{
			ColumnName = GroupedPreviousDocument.Schema.SupplementaryQuantity,
			Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(78),
			CaptionResourceString = Res.GetData("1F709443-FD8F-4679-AE80-213ADC7683A1", "Supp. Qty."),
		});
	}
}
