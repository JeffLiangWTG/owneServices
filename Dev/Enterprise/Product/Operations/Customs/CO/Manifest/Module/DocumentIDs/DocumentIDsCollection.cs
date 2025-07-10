using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.CO.Manifest.Module
{
	public class DocumentIDsCollection : BusinessObjectCollection<CusTransactionNumber>
	{
		public DocumentIDsCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{ }
	}
}
