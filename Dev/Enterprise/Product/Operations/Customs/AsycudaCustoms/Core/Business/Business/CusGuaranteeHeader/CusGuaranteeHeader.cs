using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.AsycudaCustoms.Business
{
	public class CusGuaranteeHeader : BaseCusGuaranteeHeader, Integration.Customs.AsycudaCustoms.ICusGuaranteeHeader
	{
		public CusGuaranteeHeader(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}
	}
}
