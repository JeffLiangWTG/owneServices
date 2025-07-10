using CargoWise.Windows.UI;
using Enterprise.Customs.GUI;
using Enterprise.Customs.IN.Business;
using Enterprise.Freight.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IN.GUI;

public class ContainerUserControl : BaseCustomsCusContainersWithTrackingUserControl
{
	public ContainerUserControl()
	{
		AddColumnsToGrid();
	}

	protected override ContainersUserControl GetContainerTrackingUserControl() => DetailUserControl;

	protected override void ChangeGridColumnsVisibility()
	{
		var isExport = JobDeclaration?.IsExport ?? false;
		CusContainersBoundGrid.InnerGrid.SetAvailability(isExport,
			new[] { CusContainer.Schema.CO_SealType, CusContainer.Schema.CO_SealDeviceID, CusContainer.Schema.CO_MovementDocumentType, CusContainer.Schema.CO_MovementDocumentNum });
		CusContainersBoundGrid.InnerGrid.ReOrderColumns(columnOrder);
		DetailUserControl.SetSealTypeVisibility(isExport);
	}

	void AddColumnsToGrid()
	{
		var sealTypeDropEditColumnStyleInfo = new ZDropEditColumnStyleInfo();
		sealTypeDropEditColumnStyleInfo.ColumnName = CusContainer.Schema.CO_SealType;
		ControlDpiScalingHelper.SetWidth(ref sealTypeDropEditColumnStyleInfo, 60, isOnStandardDpi: true);
		CusContainersBoundGrid.ColumnStyles.Add(sealTypeDropEditColumnStyleInfo);

		var sealDeviceIDTextBoxColumnStyleInfo = new ZTextBoxColumnStyleInfo();
		sealDeviceIDTextBoxColumnStyleInfo.ColumnName = CusContainer.Schema.CO_SealDeviceID;
		ControlDpiScalingHelper.SetWidth(ref sealDeviceIDTextBoxColumnStyleInfo, 80, isOnStandardDpi: true);
		CusContainersBoundGrid.ColumnStyles.Add(sealDeviceIDTextBoxColumnStyleInfo);

		var movementDocumentTypeTextBoxColumnStyleInfo = new ZTextBoxColumnStyleInfo();
		movementDocumentTypeTextBoxColumnStyleInfo.ColumnName = CusContainer.Schema.CO_MovementDocumentType;
		ControlDpiScalingHelper.SetWidth(ref movementDocumentTypeTextBoxColumnStyleInfo, 60, isOnStandardDpi: true);
		CusContainersBoundGrid.ColumnStyles.Add(movementDocumentTypeTextBoxColumnStyleInfo);

		var movementDocumentNumTextBoxColumnStyleInfo = new ZTextBoxColumnStyleInfo();
		movementDocumentNumTextBoxColumnStyleInfo.ColumnName = CusContainer.Schema.CO_MovementDocumentNum;
		ControlDpiScalingHelper.SetWidth(ref movementDocumentNumTextBoxColumnStyleInfo, 80, isOnStandardDpi: true);
		CusContainersBoundGrid.ColumnStyles.Add(movementDocumentNumTextBoxColumnStyleInfo);
	}

	readonly string[] columnOrder =
	{
		CusContainer.Schema.CO_ContainerNumber,
		CusContainer.Schema.CO_Seal,
		CusContainer.Schema.CO_SealType,
		CusContainer.Schema.CO_SealDeviceID,
		CusContainer.Schema.CO_MovementDocumentType,
		CusContainer.Schema.CO_MovementDocumentNum
	};

	ContainerDetailUserControl DetailUserControl => detailUserControl ??= new ContainerDetailUserControl();
	ContainerDetailUserControl detailUserControl;
}
