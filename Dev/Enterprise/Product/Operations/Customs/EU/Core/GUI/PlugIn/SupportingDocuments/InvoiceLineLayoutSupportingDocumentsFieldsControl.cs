using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI.PlugIn
{
	public partial class InvoiceLineLayoutSupportingDocumentsFieldsControl : LayoutSupportingDocumentsFieldsControl
	{
		public InvoiceLineLayoutSupportingDocumentsFieldsControl(JobDeclaration declaration) : base(declaration)
		{
			InitializeComponent();
		}

		protected override IPanelLayoutProvider GetLayout()
		{
			var result = base.GetLayout();

			if (JobDeclaration is JobDeclaration declaration && declaration.IsUCC6 && (declaration.IsExport || declaration.IsImport) && declaration.Configuration.UseEucdmSupportingDocumentGoodsShipmentAndItem)
			{
				var layout = result.Layout;
				var euBag = SupportingDocumentFieldsControlBag.Instance;
				layout.SetVisibility<Business.Declaration.MultiLineAddInfos.SupportingDocument>(euBag.StatusDropEdit, (doc) => false);
				layout.SetVisibility<Business.Declaration.MultiLineAddInfos.SupportingDocument>(euBag.DateOfIssueDateEdit, (doc) => false);
			}

			return result;
		}
	}
}
