
using CargoWise.EntityFramework;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRAqisDocumentTypeCollection : BusinessObjectCollection<CMRAqisDocumentType>
	{
		public CMRAqisDocumentTypeCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}
	}
}
