using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.IT.GUI;

namespace Enterprise.Customs.IT.NCTS.GUI;

public partial class NctsSupportingDocumentsUserControl : EU.NCTS.GUI.SupportingDocumentsUserControl
{
	public NctsSupportingDocumentsUserControl()
	{
		InitializeComponent();
	}

	protected override void InitializeGridLayout()
	{
		base.InitializeGridLayout();
		new SupportingDocumentsGridInitializer(SupportingDocumentsGrid).Initialize();
		SupportingDocumentsGrid.SetAllAvailability(false);
		SupportingDocumentsGrid.SetAvailability(true, columnNamesInOrder);
		SupportingDocumentsGrid.ReOrderColumns(columnNamesInOrder);
	}

	readonly string[] columnNamesInOrder = new string[]
	{
		SupportingDocument.Schema.CSI_Code,
		SupportingDocument.Schema.CSI_ReferenceNumber,
		SupportingDocument.Schema.CSI_Status,
		SupportingDocument.Schema.CSI_Quantity,
		SupportingDocument.Schema.CSI_UnitOfQuantity,
		SupportingDocument.Schema.CSI_YearOfIssue,
		SupportingDocument.Schema.CSI_RN_NKCountryCode,
	};
}
