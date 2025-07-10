using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public interface ICusSeaManOBLHeaderInfoProvider
	{
		ZString LloydsNumber { get; }
		ZString VoyageNumber { get; }
		ZString OceanBillNumber { get; }
	}
}
