using System;
using Enterprise.Customs.IN.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IN.GUI;

sealed class DfiaImportItemDetailsGridColumnsBag
{
	DfiaImportItemDetailsGridColumnsBag()
	{
		SerialNoTextBoxColumn = new GridColumnReference<ZCalcEditColumnStyleInfo>(DfiaImportItemDetail.Schema.CSI_LineNo, 50);
		LicenseImportItemSerialNoTextBoxColumn = new GridColumnReference<ZTextBoxColumnStyleInfo>(DfiaImportItemDetail.Schema.CSI_ReferenceNumber, 100);
		LicenseImportQuantityCalcEditColumn = new GridColumnReference<ZCalcEditColumnStyleInfo>(DfiaImportItemDetail.Schema.CSI_Quantity, 130, column => column.MaxValue = 9999999999.999m);
		LicenseImportQuantityUnitDropEditColumn = new GridColumnReference<ZDropEditColumnStyleInfo>(DfiaImportItemDetail.Schema.CSI_UnitOfQuantity, 50);
		ItemTypeDropEditColumn = new GridColumnReference<ZDropEditColumnStyleInfo>(DfiaImportItemDetail.Schema.CSI_IssuerType, 50);
	}

	public static DfiaImportItemDetailsGridColumnsBag Instance => instance ??= new DfiaImportItemDetailsGridColumnsBag();

	public IGridColumnReference SerialNoTextBoxColumn { get; }
	public IGridColumnReference LicenseImportItemSerialNoTextBoxColumn { get; }
	public IGridColumnReference LicenseImportQuantityCalcEditColumn { get; }
	public IGridColumnReference LicenseImportQuantityUnitDropEditColumn { get; }
	public IGridColumnReference ItemTypeDropEditColumn { get; }

	[ThreadStatic]
	static DfiaImportItemDetailsGridColumnsBag instance;
}
