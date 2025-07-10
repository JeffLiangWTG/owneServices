using System.Windows.Forms;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.FR.GUI.PlugIn
{
	public partial class InvoiceLinePreviousDocumentsUserControl : PreviousDocumentsUserControl
	{
		public InvoiceLinePreviousDocumentsUserControl() : base()
		{
			InitializeGridLayoutCore();
			SetColumnVisible();
		}

		protected override void InitializeGridLayoutCore()
		{
			PreviousDocumentsGrid.SetColumnCaption(PreviousDocument.Schema.CSI_LineNo, Res.GetString("4B0129F6-6B64-4638-AFC5-26462FDDFCD3", "Sequence No"));
			PreviousDocumentsGrid.ColumnStyles.AddRange(new IZColumnStyleInfo[]
			{
				new ZDropEditColumnStyleInfo
				{
					ColumnName = PreviousDocument.Schema.CSI_PackType,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120),
					CharacterCasing = CharacterCasing.Upper,
					CaptionResourceString = Res.GetData("284ACF53-609A-4DD1-AEB0-680F489E5441", "Type Of Package"),
				},
				new ZCalcEditColumnStyleInfo
				{
					ColumnName = PreviousDocument.Schema.CSI_PackQty,
					CaptionResourceString = Res.GetData("68B32548-5186-40E8-99D7-A16E87CE6A05", "Number of Packages"),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120),
				},
				new ZCalcEditColumnStyleInfo
				{
					ColumnName = PreviousDocument.Schema.CSI_Quantity,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80),
					CaptionResourceString = Res.GetData("AE13E242-CA2C-45C8-AFD4-15FE66BD8757", "Quantity"),
				},
				new ZDropEditColumnStyleInfo
				{
					ColumnName = PreviousDocument.Schema.CSI_UnitOfQuantity,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200),
					CharacterCasing = CharacterCasing.Upper,
					CaptionResourceString = Res.GetData("909982A1-5CB8-4CE3-B9AD-12FC807D96F0", "Measurement unit and qualifier"),
				},
				new ZTextBoxColumnStyleInfo
				{
					ColumnName = PreviousDocument.Schema.CSI_ItemNumber,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150),
					CharacterCasing = CharacterCasing.Upper,
					CaptionResourceString = Res.GetData("FBC331CB-3EBE-4418-B7DB-075A6253BAFF", "Goods Item Identifier"),
				}
			});
		}

		protected override void ChangeControlsVisibility()
		{
			base.ChangeControlsVisibility();
			SetColumnVisible();
		}

		void SetColumnVisible()
		{
			if (IsUCC6)
			{
				PreviousDocumentsGrid.SetAvailability(true, SpecificColumnsForUCC);
				PreviousDocumentsGrid.SetColumnVisible(true, SpecificColumnsForUCC);
				PreviousDocumentsGrid.SetColumnVisible(false, SpecificColumnsForNonUCC);
				PreviousDocumentsGrid.SetAvailability(false, SpecificColumnsForNonUCC);
				PreviousDocumentsGrid.ReOrderColumns(OrderedColumnsForUCC);
			}
			else
			{
				PreviousDocumentsGrid.SetAvailability(false, SpecificColumnsForUCC);
				PreviousDocumentsGrid.SetColumnVisible(false, SpecificColumnsForUCC);
				PreviousDocumentsGrid.SetAvailability(true, SpecificColumnsForNonUCC);
				PreviousDocumentsGrid.SetColumnVisible(true, SpecificColumnsForNonUCC);
				PreviousDocumentsGrid.ReOrderColumns(OrderedColumnsForNonUCC);
			}
		}

		bool IsUCC6 => CurrentDataItem is JobDeclaration declaration && declaration.IsUCC6;

		string[] OrderedColumnsForUCC => new[]
					{
						PreviousDocument.Schema.CSI_LineNo,
						PreviousDocument.Schema.CSI_Code,
						PreviousDocument.Schema.CSI_ReferenceNumber,
						PreviousDocument.Schema.CSI_PackType,
						PreviousDocument.Schema.CSI_PackQty,
						PreviousDocument.Schema.CSI_Quantity,
						PreviousDocument.Schema.CSI_UnitOfQuantity,
						PreviousDocument.Schema.CSI_ItemNumber
					};

		string[] OrderedColumnsForNonUCC => new[]
					{
						PreviousDocument.Schema.CSI_LineNo,
						PreviousDocument.Schema.CSI_Code,
						PreviousDocument.Schema.CSI_SubType,
						PreviousDocument.Schema.CSI_ReferenceNumber,
						PreviousDocument.Schema.CSI_DateOfIssue
					};

		string[] SpecificColumnsForUCC => new[]
					{
						PreviousDocument.Schema.CSI_PackType,
						PreviousDocument.Schema.CSI_PackQty,
						PreviousDocument.Schema.CSI_Quantity,
						PreviousDocument.Schema.CSI_UnitOfQuantity,
						PreviousDocument.Schema.CSI_ItemNumber
					};

		string[] SpecificColumnsForNonUCC => new[]
					{
						PreviousDocument.Schema.CSI_SubType,
						PreviousDocument.Schema.CSI_DateOfIssue
					};
	}
}
