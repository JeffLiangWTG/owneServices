using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	[TestedType(typeof(NctsDeparturePayInfoCollection))]
	class NctsDeparturePayInfoCollectionTest : ActiveBusinessObjectCollectionTestCase<NctsDeparturePayInfoCollection>
	{
		protected override NctsDeparturePayInfoCollection GetCollectionToTest()
		{
			var moveHeader = Factory.New<NctsDepartureMovementHeader>();
			return new NctsDeparturePayInfoCollection(moveHeader);
		}
	}
}
