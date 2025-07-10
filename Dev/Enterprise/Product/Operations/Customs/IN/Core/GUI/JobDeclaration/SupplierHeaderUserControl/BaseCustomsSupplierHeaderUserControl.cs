using Enterprise.Customs.GUI;
using Enterprise.Customs.IN.Business;

namespace Enterprise.Customs.IN.GUI;

public partial class BaseCustomsSupplierHeaderUserControl : LayoutDeclarationInvoiceHeaderUserControl
{
	public BaseCustomsSupplierHeaderUserControl()
	{
		InitializeComponent();
		UpdateGridColumns();

		SupportingDocumentTabPage.RunWhenBindingOrFirstShown((_, _) => SupportingDocumentUserControl.UserControlType = typeof(LayoutSupportingDocumentsUserControl));
	}

	void UpdateGridColumns()
	{
		var incoTermPlaceColumn = JobComInvoiceHeadersBoundGrid.InnerGrid.GetColumnStyle(JobComInvoiceHeader.Schema.JZ_IncoTermPlace);
		incoTermPlaceColumn.CaptionResourceString = Res.GetData("B15044B7-1FC9-46E8-801E-A5F309A5127D", "Place", "Inco Place", "Incoterm Place", "");
	}
}
