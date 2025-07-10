using System.Windows.Forms;
using Enterprise.Core.Forms;
using Enterprise.Customs.Business;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.GUI
{
	public partial class PreviousDocumentsUserControl : EU.GUI.PlugIn.PreviousDocumentsUserControl
	{
		public PreviousDocumentsUserControl()
		{
			InitializeComponent();
			InitializeGridLayout();
		}

		protected override void InitializeGridLayoutCore()
		{
			base.InitializeGridLayoutCore();
			PreviousDocumentsGrid.ColumnStyles.Remove(PreviousDocumentsGrid.GetColumnStyle(SupportingDocument.Schema.CSI_Code));
			PreviousDocumentsGrid.ColumnStyles.Insert(0, new ZCodeFindBoxColumnStyleInfo
			{
				ColumnName = SupportingDocument.Schema.CSI_Code,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80),
				CharacterCasing = CharacterCasing.Upper
			});
			PreviousDocumentsGrid.ColumnStyles.AddRange(columnsToAdd);
		}

		readonly ZGridColumnInfo[] columnsToAdd =
		{
			new ZCalcEditColumnStyleInfo
			{
				Decimals = 5,
				ColumnName = SupportingDocument.Schema.CSI_Quantity,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80)
			},
			new ZDropEditColumnStyleInfo
			{
				CharacterCasing = CharacterCasing.Upper,
				ColumnName = SupportingDocument.Schema.CSI_UnitOfQuantity,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80)
			},
			new ZCalcEditColumnStyleInfo
			{
				ColumnName = AutoCusSupportingInfo.Schema.CSI_PackQty,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80),
			},
			new ZDropEditColumnStyleInfo
			{
				ColumnName = AutoCusSupportingInfo.Schema.CSI_PackType,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80),
			},
			new ZCodeFindBoxColumnStyleInfo
			{
				ColumnName = AutoCusSupportingInfo.Schema.CSI_RN_NKCountryCode,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(142)
			},
		};

		JobDeclaration ESDeclaration => (JobDeclaration)base.JobDeclaration;

		protected override void ChangeGridColumnsVisibility()
		{
			base.ChangeGridColumnsVisibility();
			bool showImportColumn = JobDeclaration.IsImport;
			var isImportVersionH1 = ESDeclaration.IsUCC6AndIsImport;
			PreviousDocumentsGrid.SetAvailability(showImportColumn, AutoCusSupportingInfo.Schema.CSI_PackQty);
			PreviousDocumentsGrid.SetAvailability(showImportColumn, AutoCusSupportingInfo.Schema.CSI_PackType);
			PreviousDocumentsGrid.SetAvailability(isImportVersionH1, AutoCusSupportingInfo.Schema.CSI_RN_NKCountryCode);
		}
	}
}
