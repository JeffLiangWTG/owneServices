using System.Windows.Forms;
using Enterprise.Customs.BE.Business.Declaration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.BE.GUI;

public partial class SupplierHeaderPreviousDocumentsUserControl : EU.GUI.PlugIn.PreviousDocumentsUserControl
{
	public SupplierHeaderPreviousDocumentsUserControl()
	{
		InitializeComponent();
		InitializeGridLayout();
	}

	protected override void InitializeGridLayoutCore()
	{
		PreviousDocumentsGrid.ColumnStyles.Clear();
		PreviousDocumentsGrid.ColumnStyles.AddRange(new IZColumnStyleInfo[]
		{
			new ZCodeFindBoxColumnStyleInfo
			{
				ColumnName = PreviousDocument.Schema.CSI_Code,
				ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.Universal.ZZRefCusCodeList,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(72),
				CharacterCasing = CharacterCasing.Normal
			},
			new ZTextBoxColumnStyleInfo
			{
				ColumnName = PreviousDocument.Schema.CSI_ReferenceNumber,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(250),
				CharacterCasing = CharacterCasing.Normal
			}
		});
	}
}
