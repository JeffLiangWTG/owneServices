using CargoWise.Types;

namespace Enterprise.Customs.ES.NCTS.Business;

public class ArrivalCusTransportMeansLookups(ArrivalCusTransportMeans parent) : EU.NCTS.Business.ArrivalCusTransportMeansLookups(parent)
{
	protected override ZBool ShouldIncludeDIFInUnloadedStatesList => true;
}
