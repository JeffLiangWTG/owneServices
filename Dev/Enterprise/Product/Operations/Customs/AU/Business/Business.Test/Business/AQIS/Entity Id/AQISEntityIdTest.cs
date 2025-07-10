using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(AQISEntityId))]
	sealed class AQISEntityIdTest : AQISSingleValueBusinessObjectTest
	{
		public void TestLookups()
		{
			AQISEntityId entityId = new AQISEntityId(Factory);
			AssertNotNull("Lookups", entityId.Lookups);
		}

		AQISEntityId aqisEntityId;
		public override AQISSingleValueBusinessObject BusinessObjectToTest => aqisEntityId ?? (aqisEntityId = new AQISEntityId(Factory));

		protected override BusinessObject GetNewBusinessObject() => new AQISEntityId(Factory);
	}
}
