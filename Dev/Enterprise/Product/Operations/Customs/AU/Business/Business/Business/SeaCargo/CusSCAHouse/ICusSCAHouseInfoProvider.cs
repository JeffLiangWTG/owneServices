using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public interface ICusSCAHouseInfoProvider
	{
		ZString LloydsNumber { get; }
		ZString VoyageNumber { get; }
		ZString OceanBillNumber { get; }
		ZString HouseBillNumber { get; }
	}
}
