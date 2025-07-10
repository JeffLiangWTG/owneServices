using System;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.TemporaryStorage.GUI;

public sealed class UCC6TemporaryStoragePreviousDocumentsDetailsGridColumnsBag
{
	public static UCC6TemporaryStoragePreviousDocumentsDetailsGridColumnsBag Instance => instance ??= new UCC6TemporaryStoragePreviousDocumentsDetailsGridColumnsBag();

	[ThreadStatic]
	static UCC6TemporaryStoragePreviousDocumentsDetailsGridColumnsBag instance;

	public UCC6TemporaryStoragePreviousDocumentsDetailsGridColumnsBag()
	{
		GoodItemIdentifierCalcEditColumn = new GridColumnReference<ZCalcEditColumnStyleInfo>("CSI_LineNo", 110, config =>
		{
			config.CaptionResourceString = Res.GetData("89CB8833-7517-4DB7-B97D-CFED1741E92D", "Goods Item Identifier");
			config.DefaultCollectionIndex = 0;
			config.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			config.MaxValue = 99999;
		});
		PackQtyCalcEditColumn = new GridColumnReference<ZCalcEditColumnStyleInfo>("CSI_PackQty", 80, config =>
		{
			config.DefaultCollectionIndex = 0;
			config.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			config.GroupName = Res.GetData("8A3351C6-1BC3-4A7F-86A7-8E97825474DC", "Number of Packages");
		});
		PackTypeDropEditColumn = new GridColumnReference<ZDropEditColumnStyleInfo>("CSI_PackType", 80, config =>
		{
			config.DefaultCollectionIndex = 0;
			config.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			config.GroupName = Res.GetData("8A3351C6-1BC3-4A7F-86A7-8E97825474DC", "Number of Packages");
		});
		QuantityCalcEditColumn = new GridColumnReference<ZCalcEditColumnStyleInfo>("CSI_Quantity", 80, config =>
		{
			config.DefaultCollectionIndex = 0;
			config.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			config.GroupName = Res.GetData("854BC244-05E2-4F82-93F5-0E60A93B4438", "Quantity");
		});
		QuantityUnitDropEditColumn = new GridColumnReference<ZDropEditColumnStyleInfo>("CSI_UnitOfQuantity", 80, config =>
		{
			config.DefaultCollectionIndex = 0;
			config.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			config.GroupName = Res.GetData("854BC244-05E2-4F82-93F5-0E60A93B4438", "Quantity");
		});
		ReferenceNumberTextBoxColumn = new GridColumnReference<ZTextBoxColumnStyleInfo>("CSI_ReferenceNumber", 100, config =>
		{
			config.CaptionResourceString = Res.GetData("DEF8DF7A-F073-476D-BA98-B95339A263B9", "Reference Number");
			config.DefaultCollectionIndex = 0;
			config.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
		});
		TypeCodeFindBoxColumn = new GridColumnReference<ZCodeFindBoxColumnStyleInfo>("CSI_Code", 40, config =>
		{
			config.CaptionResourceString = Res.GetData("164EEE89-D475-4653-A20D-8578CEA4A94C", "Type");
			config.DefaultCollectionIndex = 0;
			config.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
		});
		ReferenceNumber2TextBoxColumn = new GridColumnReference<ZTextBoxColumnStyleInfo>("CSI_ReferenceNumber2", 80, config =>
		{
			config.DefaultCollectionIndex = 0;
			config.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
		});
	}

	public IGridColumnReference GoodItemIdentifierCalcEditColumn { get; }

	public IGridColumnReference PackQtyCalcEditColumn { get; }

	public IGridColumnReference PackTypeDropEditColumn { get; }

	public IGridColumnReference QuantityCalcEditColumn { get; }

	public IGridColumnReference QuantityUnitDropEditColumn { get; }

	public IGridColumnReference ReferenceNumberTextBoxColumn { get; }

	public IGridColumnReference ReferenceNumber2TextBoxColumn { get; }

	public IGridColumnReference TypeCodeFindBoxColumn { get; }
}
