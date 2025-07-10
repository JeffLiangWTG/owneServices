
using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRAqisConcern : AutoCMRAqisConcern
	{
		public CMRAqisConcern(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public static CMRAqisConcern New(BusinessObjectFactory factory)
		{
			return factory.New<CMRAqisConcern>();
		}
	}
}
