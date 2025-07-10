using CargoWise.EntityFramework;
using Enterprise.Customs.Common.BR;

namespace Enterprise.Customs.BR.Business
{
	public class PermitCollection : Customs.Business.CusSupportingInfoCollection<Permit>
	{
		public PermitCollection(BusinessObject parent)
			: base(parent, CusSupportingInfoTypeList.Codes.Permit)
		{
		}
	}
}
