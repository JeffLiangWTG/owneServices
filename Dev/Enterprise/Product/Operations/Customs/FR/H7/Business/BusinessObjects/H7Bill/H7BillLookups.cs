using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.FR.H7.Business
{
	public class H7BillLookups : EU.H7.Business.AsycudaBillLookups
	{
		public H7BillLookups(ASYCUDA.Business.AsycudaBill parent) : base(parent)
		{
		}

		public new CodeDescriptionPairList ShipmentTypes => Factory.GetCachedValue<FRH7ShipmentTypeList>();
	}
}
