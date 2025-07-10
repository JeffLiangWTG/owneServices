using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.KR.Business
{
	public class CusReconCustomsCharge : Customs.Business.CusReconCustomsCharge, Integration.Customs.KR.ICusReconCustomsCharge
	{
		public CusReconCustomsCharge(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}
	}
}
