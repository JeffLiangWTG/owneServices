using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.NCTS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

[TestedType(typeof(CusSealCollection))]
class CusSealCollectionTest : BusinessObjectCollectionTestCase
{
	protected override BusinessObjectCollection GetCollectionToTest()
	{
		var header = Factory.New<NctsHeader>();
		header.SetMovementType(NctsMovementType.Codes.Arrival);
		return new CusSealCollection(header);
	}
}
