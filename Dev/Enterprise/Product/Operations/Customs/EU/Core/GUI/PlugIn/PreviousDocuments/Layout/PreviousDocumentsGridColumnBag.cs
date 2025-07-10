using System;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI.PlugIn
{
	public sealed class PreviousDocumentsGridColumnBag
	{
		public static PreviousDocumentsGridColumnBag Instance => instance ??= new PreviousDocumentsGridColumnBag();

		[ThreadStatic]
		static PreviousDocumentsGridColumnBag instance;
		PreviousDocumentsGridColumnBag()
		{
			CodeDropEditColumn = new GridColumnReference<ZDropEditColumnStyleInfo>(PreviousDocument.Schema.CSI_Code, 80, c => c.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper);
			CodeDescriptionColumn = new GridColumnReference<ZTextBoxColumnStyleInfo>(PreviousDocument.Schema.CSI_CodeDescription, 200, c => c.IsReadOnly = true);
			SubTypeColumn = new GridColumnReference<ZDropEditColumnStyleInfo>(PreviousDocument.Schema.CSI_SubType, 80, c => c.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper);
			ReferenceNumberColumn = new GridColumnReference<ZTextBoxColumnStyleInfo>(PreviousDocument.Schema.CSI_ReferenceNumber, 120, c => c.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper);
			DateOfIssueColumn = new GridColumnReference<ZDateEditColumnStyleInfo>(PreviousDocument.Schema.CSI_DateOfIssue, 120);
			LineNoColumn = new GridColumnReference<ZCalcEditColumnStyleInfo>(PreviousDocument.Schema.CSI_LineNo, 80,
				c =>
				{
					c.BindToDecimalPlaces = null;
					c.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("b9d7098a-65cd-4c4a-bc50-0ec00e29ab2f", "Line No.");
					c.MaxValue = 99999;
				});
			ProcedureColumn = new GridColumnReference<ZDropEditColumnStyleInfo>(PreviousDocument.Schema.CSI_Procedure, 80);
			ReferenceNumber2Column = new GridColumnReference<ZTextBoxColumnStyleInfo>(PreviousDocument.Schema.CSI_ReferenceNumber2, 80);
			CustomsOfficeColumn = new GridColumnReference<ZCodeFindBoxColumnStyleInfo>(PreviousDocument.Schema.CSI_CustomsOffice, 100, c => c.ModuleID = ZArchitecture.Modules.ModuleIDs.Customs.Universal.ZZRefCusCodeList);
			QuantityColumn = new GridColumnReference<ZCalcEditColumnStyleInfo>(PreviousDocument.Schema.CSI_Quantity, 80, c => c.BindToDecimalPlaces = null);
			UnitOfQuantityColumn = new GridColumnReference<ZDropEditColumnStyleInfo>(PreviousDocument.Schema.CSI_UnitOfQuantity, 80);
			Quantity3Column = new GridColumnReference<ZCalcEditColumnStyleInfo>(PreviousDocument.Schema.CSI_Quantity3, 80, c => c.BindToDecimalPlaces = null);
			UnitOfQuantity3Column = new GridColumnReference<ZDropEditColumnStyleInfo>(PreviousDocument.Schema.CSI_UnitOfQuantity3, 80);
			PackQtyColumn = new GridColumnReference<ZCalcEditColumnStyleInfo>(PreviousDocument.Schema.CSI_PackQty, 80,
				c =>
				{
					c.BindToDecimalPlaces = null;
					c.MaxValue = 99999999;
				});
			PackTypeColumn = new GridColumnReference<ZDropEditColumnStyleInfo>(PreviousDocument.Schema.CSI_PackType, 80);
			ItemNumberColumn = new GridColumnReference<ZCalcEditColumnStyleInfo>(PreviousDocument.Schema.CSI_ItemNumber, 80,
				c =>
				{
					c.BindToDecimalPlaces = null;
					c.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("ee2d12c0-9d87-484c-a039-0f86f2bb987f", "Item No.");
				});
			Quantity2Column = new GridColumnReference<ZCalcEditColumnStyleInfo>(PreviousDocument.Schema.CSI_Quantity2, 80, c => c.BindToDecimalPlaces = null);
			UnitOfQuantity2Column = new GridColumnReference<ZDropEditColumnStyleInfo>(PreviousDocument.Schema.CSI_UnitOfQuantity2, 80);
			CountryCodeColumn = new GridColumnReference<ZCodeFindBoxColumnStyleInfo>(PreviousDocument.Schema.CSI_RN_NKCountryCode, 120, c => c.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper);
		}

		public IGridColumnReference CodeDropEditColumn { get; }

		public IGridColumnReference CodeDescriptionColumn { get; }

		public IGridColumnReference SubTypeColumn { get; }

		public IGridColumnReference ReferenceNumberColumn { get; }

		public IGridColumnReference DateOfIssueColumn { get; }

		public IGridColumnReference LineNoColumn { get; }

		public IGridColumnReference ProcedureColumn { get; }

		public IGridColumnReference ReferenceNumber2Column { get; }

		public IGridColumnReference CustomsOfficeColumn { get; }

		public IGridColumnReference QuantityColumn { get; }

		public IGridColumnReference UnitOfQuantityColumn { get; }

		public IGridColumnReference Quantity3Column { get; }

		public IGridColumnReference UnitOfQuantity3Column { get; }

		public IGridColumnReference PackQtyColumn { get; }

		public IGridColumnReference PackTypeColumn { get; }

		public IGridColumnReference ItemNumberColumn { get; }

		public IGridColumnReference Quantity2Column { get; }

		public IGridColumnReference UnitOfQuantity2Column { get; }

		public IGridColumnReference CountryCodeColumn { get; }
	}
}

