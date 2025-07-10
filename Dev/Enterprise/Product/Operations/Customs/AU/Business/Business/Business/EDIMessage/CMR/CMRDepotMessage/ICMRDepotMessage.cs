using System.Collections.Generic;

using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public enum CMRDepotMessageType
	{
		ExpectedArrival,
		Approval,
		Status
	}

	public interface ICMRDepotMessage
	{
		ZString LloydsNumber { get; }
		ZString VoyageNumber { get; }
		ZString OriginPremiseID { get; }
		ZString DestinationPremiseID { get; }
		ZString OurPremiseID { get; }
		CMRDepotMessageType MessageType { get; }
		ICMRDepotMessageLine[] Lines { get; }
		BusinessObjectFactory Factory { get; }
		bool IsSea { get; }
		Dictionary<string, List<CARSTRecord>> UnmatchedContainers { get; }

		void AddUnmatchedContainer(CARSTRecord carstRecord);
		CMRCUSRESMessage LinkOrCloneMessage(BusinessObject businessObjectToLink);
	}

	public interface ICMRDepotMessageLine
	{
		ZInt NumberOfPackages { get; }
		ZString PackageType { get; }
		ZString ContainerNumber { get; }
		ZString HouseBillNumber { get; }
		ZString OceanBillNumber { get; }
		ZString ContainerMode { get; }
		ZString MarksAndNumbers { get; }
		ZString GoodsDescription { get; }
		ICMRDepotMessage Parent { get; }
		ZDecimal ActualWeight { get; }
		ZString WeightUnits { get; }
		ZDecimal ActualVolume { get; }
		ZString VolumeUnits { get; }
	}
}
