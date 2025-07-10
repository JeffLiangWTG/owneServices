using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.KR.Business
{
	public partial class KRInvoiceLineDetailsView : AutoKRInvoiceLineDetailsView
	{
		public KRInvoiceLineDetailsView(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
	}
}
