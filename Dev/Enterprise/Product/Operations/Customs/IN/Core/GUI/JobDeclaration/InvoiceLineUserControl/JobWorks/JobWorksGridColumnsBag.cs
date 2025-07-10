using System;
using Enterprise.Customs.IN.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IN.GUI;

sealed class JobWorksGridColumnsBag
{
	JobWorksGridColumnsBag()
	{
		LineNoCalcEditColumn = new GridColumnReference<ZCalcEditColumnStyleInfo>(JobWork.Schema.CSI_LineNo, 50, info => info.IsMandatory = true);
		ReferenceNumberTextBoxColumn = new GridColumnReference<ZTextBoxColumnStyleInfo>(JobWork.Schema.CSI_ReferenceNumber, 100, info => info.IsMandatory = true);
		DateOfIssueDateEditColumn = new GridColumnReference<ZDateEditColumnStyleInfo>(JobWork.Schema.CSI_DateOfIssue, 100, info =>
		{
			info.IsMandatory = true;
			info.DateTimeFormat = ZArchitecture.Core.ZDateTimePickerFormat.Short;
		});
		CustomsOfficeTextBoxColumn = new GridColumnReference<ZTextBoxColumnStyleInfo>(JobWork.Schema.CSI_CustomsOffice, 100, info => info.IsMandatory = true);
		ReferenceNumber2TextBoxColumn = new GridColumnReference<ZTextBoxColumnStyleInfo>(JobWork.Schema.CSI_ReferenceNumber2, 100, info => info.IsMandatory = true);
		ItemNumberCalcEditColumn = new GridColumnReference<ZCalcEditColumnStyleInfo>(JobWork.Schema.CSI_ItemNumber, 100, info =>
		{
			info.IsMandatory = true;
			info.MaxValue = 9999;
		});
		QuantityCalcEditColumn = new GridColumnReference<ZCalcEditColumnStyleInfo>(JobWork.Schema.CSI_Quantity, 100, info =>
		{
			info.IsMandatory = true;
			info.MaxValue = 99999999;
		});
		UnitOfQuantityTextBoxColumn = new GridColumnReference<ZTextBoxColumnStyleInfo>(JobWork.Schema.CSI_UnitOfQuantity, 100, info => info.IsMandatory = true);
	}

	public static JobWorksGridColumnsBag Instance => instance ??= new JobWorksGridColumnsBag();

	public IGridColumnReference LineNoCalcEditColumn { get; }
	public IGridColumnReference ReferenceNumberTextBoxColumn { get; }
	public IGridColumnReference DateOfIssueDateEditColumn { get; }
	public IGridColumnReference CustomsOfficeTextBoxColumn { get; }
	public IGridColumnReference ReferenceNumber2TextBoxColumn { get; }
	public IGridColumnReference ItemNumberCalcEditColumn { get; }
	public IGridColumnReference QuantityCalcEditColumn { get; }
	public IGridColumnReference UnitOfQuantityTextBoxColumn { get; }

	[ThreadStatic]
	static JobWorksGridColumnsBag instance;
}

