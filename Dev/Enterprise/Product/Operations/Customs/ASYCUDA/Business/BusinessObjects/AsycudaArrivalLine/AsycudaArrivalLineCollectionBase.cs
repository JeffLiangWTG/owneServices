using CargoWise.EntityFramework;

namespace Enterprise.Customs.ASYCUDA.Business;

public class AsycudaArrivalLineCollection : ActiveBusinessObjectCollection<AsycudaArrivalLine>
{
	public AsycudaArrivalLineCollection(AsycudaArrivalHeader master)
		: base(master)
	{ }
}
