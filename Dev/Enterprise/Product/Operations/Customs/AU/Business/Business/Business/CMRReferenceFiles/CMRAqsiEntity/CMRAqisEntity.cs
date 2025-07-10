
using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRAqisEntity : AutoCMRAqisEntity
	{
		public CMRAqisEntity(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public static CMRAqisEntity New(BusinessObjectFactory factory)
		{
			return factory.New<CMRAqisEntity>();
		}
	}
}
