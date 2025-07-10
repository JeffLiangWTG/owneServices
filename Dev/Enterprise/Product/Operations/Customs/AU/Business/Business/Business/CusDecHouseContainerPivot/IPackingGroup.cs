using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public interface IPackingGroup
	{
		JobDeclaration Declaration { get; }

		ZShort HouseContainerNumber { get; }
		ZInt NumberOfPackages { get; }
		ZInt WarehouseNumberOfPackages { get; }
		ZInt PackingUnitCount { get; }
		ZString ContainerMode { get; }
		ZString ContainerNumber { get; }
		ZString MasterBillNumber { get; }
		ZString HouseBillNumber { get; }

		ZString MarksAndNumbers { get; }
		ZString ConsignRefNumber { get; }
		ZString ActionCodeForMessage(CusEntryHeader entryHeader);
	}
}
