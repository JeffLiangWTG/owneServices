using System;
using Enterprise.Customs.IN.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IN.GUI;

sealed class SWProductionGridColumnsBag
{
	SWProductionGridColumnsBag()
	{
		LineNoCalcEditColumn = new GridColumnReference<ZCalcEditColumnStyleInfo>(SWProduction.Schema.CSI_LineNo, 50);
		ReferenceNumberTextBoxColumn = new GridColumnReference<ZTextBoxColumnStyleInfo>(SWProduction.Schema.CSI_ReferenceNumber, 100);
		BatchQuantityColumn = new GridColumnReference<ZCalcEditColumnStyleInfo>(SWProduction.Schema.CSI_Quantity, 100, column => column.MaxValue = 9999999999.999999m);
		UnitOfQuantityColumn = new GridColumnReference<ZDropEditColumnStyleInfo>(SWProduction.Schema.CSI_UnitOfQuantity, 100);
		ManufacturingDateColumn = new GridColumnReference<ZDateEditColumnStyleInfo>(SWProduction.Schema.CSI_DateOfIssue, 100, column => column.DateTimeFormat = ZDateTimePickerFormat.Short);
		ExpiryDateColumn = new GridColumnReference<ZDateEditColumnStyleInfo>(SWProduction.Schema.CSI_DateOfExpiry, 100, column => column.DateTimeFormat = ZDateTimePickerFormat.Short);
		BestBeforeDateColumn = new GridColumnReference<ZDateTimeOffsetEditColumnStyleInfo>(SWProduction.Schema.CSI_EffectiveDate, 100, info =>
		{
			info.DateTimeFormat = ZDateTimePickerFormat.Short;
		});
	}

	public static SWProductionGridColumnsBag Instance => instance ??= new SWProductionGridColumnsBag();

	public IGridColumnReference LineNoCalcEditColumn { get; }
	public IGridColumnReference ReferenceNumberTextBoxColumn { get; }
	public IGridColumnReference BatchQuantityColumn { get; }
	public IGridColumnReference UnitOfQuantityColumn { get; }
	public IGridColumnReference ManufacturingDateColumn { get; }
	public IGridColumnReference ExpiryDateColumn { get; }
	public IGridColumnReference BestBeforeDateColumn { get; }

	[ThreadStatic]
	static SWProductionGridColumnsBag instance;
}
