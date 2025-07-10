using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Dash.Business
{
	[CodeAlive("In development, so this enum may still be required")]
	public enum DocumentType
	{
		Unknown,
		BillOfLading,
		CommercialInvoice,
		HouseBillOfLading,
		PackingList,
		SupplierInvoice
	}
}
