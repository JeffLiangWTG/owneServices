using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.Business.Testing;

[TestedType(typeof(SealCollection<Seal>))]
public class SealCollectionNctsArrivalMovementHeaderTest : BusinessObjectCollectionTestCase
{
	protected override BusinessObjectCollection GetCollectionToTest()
	{
		var header = Factory.New<NctsHeader>();
		header.SetMovementType(NctsMovementType.Codes.DepartureAndArrival);
		var arrivalMovementHeader = header.ArrivalMovementHeader;
		return new SealCollection<Seal>(arrivalMovementHeader);
	}
}
