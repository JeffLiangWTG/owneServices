using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.FR.Business.NCTS
{
	public class NctsCusAuthorizationUsage : EU.NCTS.Business.CusAuthorizationUsage
	{
		public NctsCusAuthorizationUsage(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public override bool AGC_NumberReadOnly => false;
	}
}
