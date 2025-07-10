using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.NCTS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	[TestedType(typeof(NctsESOfficeCodeCollectionForDepartureGrid))]
	public class NctsESOfficeCodeCollectionForDepartureGridTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var parent = Factory.New<NctsHeader>();
			parent.SetMovementType(NctsMovementType.Codes.Departure);
			return new NctsESOfficeCodeCollectionForDepartureGrid(parent.MovementHeader);
		}
	}
}
