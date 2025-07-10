
using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRAqisCommodityStatisticalClassification : AutoCMRAqisCommodityStatisticalClassification
	{
		public CMRAqisCommodityStatisticalClassification(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public static CMRAqisCommodityStatisticalClassification New(BusinessObjectFactory factory)
		{
			return factory.New<CMRAqisCommodityStatisticalClassification>();
		}
	}
}
