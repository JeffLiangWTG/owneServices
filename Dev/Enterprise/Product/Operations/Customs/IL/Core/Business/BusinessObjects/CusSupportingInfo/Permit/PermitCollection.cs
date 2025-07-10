using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.IL.Business
{
	public class PermitCollection : CusSupportingInfoCollection<Permit>
	{
		public PermitCollection(BusinessObject parent) : base(parent, Common.IL.CusSupportingInfoTypeList.Codes.Permit)
		{
		}
	}
}
