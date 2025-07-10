using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public interface ICargoListReportLine
	{
		ZString CargoCode { get; }
		ZString CargoIdentifier { get; }
		ZString PortOfDestination { get; }
		ZString PortOfLoading { get; }
		ZString ImportCargoType { get; }
		ZInt NumberOfPackages { get; }
		ZString PackageType { get; }
	}
}
