using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

[TestedType(typeof(NctsEuOfficeCodeCollectionForDepartureGrid))]
internal class NctsEuOfficeCodeCollectionForDepartureGridTest : BusinessObjectCollectionTestCase
{
	protected override BusinessObjectCollection GetCollectionToTest()
	{
		var parent = Factory.New<NctsHeader>();
		parent.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
		return new NctsEuOfficeCodeCollectionForDepartureGrid(parent.MovementHeader);
	}
}
