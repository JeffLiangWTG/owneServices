using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentWrappers
{
	public partial class DocARInvoice
	{
		DocumentWrapper ElectronicInvoice
		{
			get { return electronicInvoice ?? (electronicInvoice = DocumentWrapperFactory.CreateAccountingWrapper(Core.Constants.DataContext.ElectronicInvoice, Invoice, GlbCompany.CurrentCompany.GC_RN_NKCountryCode)); }
		}
		DocumentWrapper electronicInvoice;

		public KR.DocElectronicInvoice KoreaElectronicInvoice => ElectronicInvoice as KR.DocElectronicInvoice;
	}
}
