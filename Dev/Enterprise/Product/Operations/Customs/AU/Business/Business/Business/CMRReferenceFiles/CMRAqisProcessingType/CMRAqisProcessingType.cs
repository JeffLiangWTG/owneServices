
using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRAqisProcessingType : AutoCMRAqisProcessingType
	{
		public CMRAqisProcessingType(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public static CMRAqisProcessingType New(BusinessObjectFactory factory)
		{
			return factory.New<CMRAqisProcessingType>();
		}
	}
}
