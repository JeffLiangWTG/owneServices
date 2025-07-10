using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Universal;

namespace Enterprise.UniversalDataBuss.DataObjects.Core
{
	public interface IPackageParentDataObject
	{
		DataObjectList<Container> ContainerCollection { get; }
		DataObjectList<PackingLine> PackingLineCollection { get; }

		ZInt? OuterPacks { get; set; }
		PackageType OuterPacksPackageType { get; set; }

		ZInt? TotalNoOfPacks { get; set; }
		PackageType TotalNoOfPacksPackageType { get; set; }

		ZInt? TotalNoOfPieces { get; set; }

		ZDecimal? TotalVolume { get; set; }
		UnitOfVolume TotalVolumeUnit { get; set; }

		ZDecimal? TotalWeight { get; set; }
		UnitOfWeight TotalWeightUnit { get; set; }

		ZString? GoodsDescription { get; set; }
	}
}
