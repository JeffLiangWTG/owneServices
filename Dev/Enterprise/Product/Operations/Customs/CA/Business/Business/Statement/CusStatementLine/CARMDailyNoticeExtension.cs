using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.CA.Business
{
	public class CARMDailyNoticeExtension : CusSupportingInfo
	{
		public CARMDailyNoticeExtension(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new CusStatementLine Parent => base.Parent as CusStatementLine;
	}
}
