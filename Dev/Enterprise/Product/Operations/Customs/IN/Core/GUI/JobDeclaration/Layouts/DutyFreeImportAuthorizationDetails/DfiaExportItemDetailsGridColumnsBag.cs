using System;
using Enterprise.Customs.IN.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IN.GUI;

sealed class DfiaExportItemDetailsGridColumnsBag
{
	DfiaExportItemDetailsGridColumnsBag()
	{
		SerialNoTextBoxColumn = new GridColumnReference<ZCalcEditColumnStyleInfo>(DfiaExportItemDetail.Schema.CSI_LineNo, 50);
		LicenseNoTextBoxColumn = new GridColumnReference<ZTextBoxColumnStyleInfo>(DfiaExportItemDetail.Schema.CSI_ReferenceNumber, 175);
		LicenseDateEditColumn = new GridColumnReference<ZDateEditColumnStyleInfo>(DfiaExportItemDetail.Schema.CSI_DateOfIssue, 80, info => info.DateTimeFormat = ZDateTimePickerFormat.Short);
		LicenseExportItemSerialNoTextBoxColumn = new GridColumnReference<ZTextBoxColumnStyleInfo>(DfiaExportItemDetail.Schema.CSI_ReferenceNumber2, 80);
		LicenseExportQuantityCalcEditColumn = new GridColumnReference<ZCalcEditColumnStyleInfo>(DfiaExportItemDetail.Schema.CSI_Quantity, 100, column => column.MaxValue = 9999999999.999m);
		LicenseExportQuantityUnitDropEditColumn = new GridColumnReference<ZDropEditColumnStyleInfo>(DfiaExportItemDetail.Schema.CSI_UnitOfQuantity, 40);
	}

	public static DfiaExportItemDetailsGridColumnsBag Instance => instance ??= new DfiaExportItemDetailsGridColumnsBag();

	public IGridColumnReference SerialNoTextBoxColumn { get; }
	public IGridColumnReference LicenseNoTextBoxColumn { get; }
	public IGridColumnReference LicenseDateEditColumn { get; }
	public IGridColumnReference LicenseExportItemSerialNoTextBoxColumn { get; }
	public IGridColumnReference LicenseExportQuantityCalcEditColumn { get; }
	public IGridColumnReference LicenseExportQuantityUnitDropEditColumn { get; }

	[ThreadStatic]
	static DfiaExportItemDetailsGridColumnsBag instance;
}
