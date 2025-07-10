
using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRAqisDocumentType : AutoCMRAqisDocumentType
	{
		public CMRAqisDocumentType(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public static CMRAqisDocumentType New(BusinessObjectFactory factory)
		{
			return factory.New<CMRAqisDocumentType>();
		}
	}
}
