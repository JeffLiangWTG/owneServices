using CargoWise.EntityFramework;
using Enterprise.Customs.IT.GUI;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.IT.NCTS.Module;

public partial class NctsMovementFilterControl : EU.NCTS.Module.NctsMovementFilterControl
{
	public NctsMovementFilterControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterStripBusinessObject)
		: base(gridCollection, filterStripBusinessObject)
	{
		InitializeComponent();
		AddColumns();

		grid.GetColumnStyle(Business.NctsHeader.Schema.MovementReferenceIssueDate).CaptionResourceString = Enterprise.Customs.IT.NCTS.Module.Res.GetData("49AA617F-27D4-4431-9240-D3DA01033BE2", "MRN Release Date");
	}

	void AddColumns()
	{
		AddIrildesColumns();

		grid.AddAdjoiningColumn(Business.NctsHeader.Schema.RepresentativeForBinding, new ZArchitecture.ZTextBoxColumnStyleInfo
		{
			ColumnName = Business.NctsHeader.Schema.DeclarantCodeForBinding,
			Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150),
			CaptionResourceString = Enterprise.Customs.IT.NCTS.Module.Res.GetData("45A0D092-0729-42BE-948F-ED6963ECC7B8", "Declarant")
		});

		grid.AddAdjoiningColumn(Business.NctsHeader.Schema.DeclarantCodeForBinding, new ZArchitecture.ZTextBoxColumnStyleInfo
		{
			ColumnName = Business.NctsHeader.Schema.RepresentationType,
			Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150),
			CaptionResourceString = Enterprise.Customs.IT.NCTS.Module.Res.GetData("B1A55A19-B07B-4D56-852F-D3AF16958CA8", "Representation Type")
		});

		grid.AddAdjoiningColumn(Business.NctsHeader.Schema.RepresentationType, new ZArchitecture.ZTextBoxColumnStyleInfo
		{
			ColumnName = Business.NctsHeader.Schema.ApprovalDeferNoForBinding,
			Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120),
			CaptionResourceString = Enterprise.Customs.IT.NCTS.Module.Res.GetData("DBD39465-0A1D-4066-8E52-6221843D6E0A", "Approval Defer No")
		});

		grid.AddAdjoiningColumn(Business.NctsHeader.Schema.IrildesArrivalDateForBinding, new ZArchitecture.ZDateEditColumnStyleInfo
		{
			ColumnName = Business.NctsHeader.Schema.RegistrationDateForBinding,
			Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110),
			DateTimeFormat = ZArchitecture.Core.ZDateTimePickerFormat.Short,
			CaptionResourceString = Enterprise.Customs.IT.NCTS.Module.Res.GetData("8008B5CA-F478-42FD-8BC0-4211610DF124", "Registration Date")
		});

		grid.AddAdjoiningColumn(Business.NctsHeader.Schema.RegistrationDateForBinding, new ZArchitecture.ZDateEditColumnStyleInfo
		{
			ColumnName = Business.NctsHeader.Schema.ReleaseDateForBinding,
			Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100),
			DateTimeFormat = ZArchitecture.Core.ZDateTimePickerFormat.Short,
			CaptionResourceString = Enterprise.Customs.IT.NCTS.Module.Res.GetData("4715B0E2-820E-43A8-A756-0A3660E14961", "Release Date")
		});
	}

	void AddIrildesColumns()
	{
		var columnStyles = grid.ColumnStyles;

		columnStyles.Add(new ZArchitecture.ZDateEditColumnStyleInfo
		{
			ColumnName = Business.NctsHeader.Schema.IrildesArrivalDateForBinding,
			Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120),
			DateTimeFormat = ZArchitecture.Core.ZDateTimePickerFormat.Short,
			CaptionResourceString = Enterprise.Customs.IT.NCTS.Module.Res.GetData("34F5D3C4-9866-493A-9337-0EC2B361C4BD", "IRILDES Arrival Date")
		});

		columnStyles.Add(new ZArchitecture.ZTextBoxColumnStyleInfo
		{
			ColumnName = Business.NctsHeader.Schema.IrildesArrivalOfficeCodeForBinding,
			Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150),
			CaptionResourceString = Enterprise.Customs.IT.NCTS.Module.Res.GetData("CB951847-2A1E-473F-A065-FD2D3F04D0A2", "IRILDES Arrival Office Code")
		});

		columnStyles.Add(new ZArchitecture.ZTextBoxColumnStyleInfo
		{
			ColumnName = Business.NctsHeader.Schema.IrildesArrivalOfficeDescriptionForBinding,
			Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(190),
			CaptionResourceString = Enterprise.Customs.IT.NCTS.Module.Res.GetData("3437351E-F289-4D8D-989A-443555A74C92", "IRILDES Arrival Office Description")
		});

		columnStyles.Add(new ZArchitecture.ZTextBoxColumnStyleInfo
		{
			ColumnName = Business.NctsHeader.Schema.IrildesArrivalStatusForBinding,
			Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130),
			CaptionResourceString = Enterprise.Customs.IT.NCTS.Module.Res.GetData("6F1D5ABA-37E3-461F-9988-07B19782B01C", "IRILDES Arrival Status")
		});
	}
}
