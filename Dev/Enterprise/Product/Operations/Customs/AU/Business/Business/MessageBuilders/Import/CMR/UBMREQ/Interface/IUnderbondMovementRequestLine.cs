using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public interface IUnderbondMovementRequestLine
	{
		ZString ContainerNumber { get; }
		ZString HouseBillOfLading { get; }
		ZString HouseAirWaybillNumber { get; }
		ZString OceanBillOfLading { get; }
		ZString MasterAirWaybillNumber { get; }
		ZString UniqueConsignmentReferenceNumber { get; }
		ZInt NumberOfPackages { get; }
		ZString PackageType { get; }
		ZString ImportCargoType { get; }
	}
}

