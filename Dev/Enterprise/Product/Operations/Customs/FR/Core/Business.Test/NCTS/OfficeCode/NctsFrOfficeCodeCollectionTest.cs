using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.NCTS.Testing
{
	[TestedType(typeof(NctsFrOfficeCodeCollection))]
	public class NctsFrOfficeCodeCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var parent = Factory.New<NctsHeader>();
			parent.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			return new NctsFrOfficeCodeCollection(parent.MovementHeader);
		}
	}
}
