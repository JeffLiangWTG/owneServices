using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.CH.NCTS.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.CH.NCTS.Module;

public partial class NctsMovementFilterControl : EU.NCTS.Module.NctsMovementFilterControl
{
	public NctsMovementFilterControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterStripBusinessObject)
		: base(gridCollection, filterStripBusinessObject)
	{
		InitializeComponent();
		AddColumns();
	}

	void AddColumns()
	{
		var columnStyles = grid.ColumnStyles;

		columnStyles.Add(CreateZDateEditColumn(nameof(NctsHeader.MovementReferenceExpiryDate), Res.GetData("5D4B15E2-9857-4A88-A976-823488D1F7CA", "Activation Deadline")));
		columnStyles.Add(CreateZTextBoxColumn($"{nameof(NctsHeader.ArrivalMovementHeader)}+{nameof(NctsArrivalMovementHeader.BM_TransportAtArrivalID)}", Res.GetData("DCF1F196-C1A5-43BC-A608-6A7D605229FB", "Arrival Transport ID")));
		columnStyles.Add(CreateZTextBoxColumn($"{nameof(NctsHeader.ArrivalMovementHeader)}+{nameof(NctsArrivalMovementHeader.BM_RN_NKTransportAtArrivalIDNationality)}", Res.GetData("4741D32B-9A14-415B-827F-02C5B707C863", "Arrival Transport Nationality")));

		ZArchitecture.ZDateEditColumnStyleInfo CreateZDateEditColumn(string columnName, ResourceStringData caption)
		{
			return new ZArchitecture.ZDateEditColumnStyleInfo
			{
				ColumnName = columnName,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120),
				DateTimeFormat = ZArchitecture.Core.ZDateTimePickerFormat.Short,
				CaptionResourceString = caption
			};
		}

		ZArchitecture.ZTextBoxColumnStyleInfo CreateZTextBoxColumn(string columnName, ResourceStringData caption)
		{
			return new ZArchitecture.ZTextBoxColumnStyleInfo
			{
				ColumnName = columnName,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120),
				CaptionResourceString = caption
			};
		}
	}
}
