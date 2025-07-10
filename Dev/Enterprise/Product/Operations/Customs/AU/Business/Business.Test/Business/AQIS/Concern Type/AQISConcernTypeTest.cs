using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(AQISConcernType))]
	sealed class AQISConcernTypeTest : AQISSingleValueBusinessObjectTest
	{
		public void TestCodeMaxLength()
		{
			AQISConcernType concernType = new AQISConcernType(Factory);
			AssertEquals("Concern Type", 4, concernType.CodeInfo.MaxLength);
		}

		public void TestLookups()
		{
			AQISConcernType concernType = new AQISConcernType(Factory);
			AssertNotNull("Lookups", concernType.Lookups);
		}

		protected override BusinessObject GetNewBusinessObject() => new AQISConcernType(Factory);

		AQISConcernType businessObjectToTest;
		public override AQISSingleValueBusinessObject BusinessObjectToTest => businessObjectToTest ?? (businessObjectToTest = new AQISConcernType(Factory));
	}
}
