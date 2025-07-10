using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(AirOrStandAloneCusUnderbondCollection))]
	public class AirOrStandAloneCusUnderbondCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestRelationshipFilter()
		{
			var underbond1 = Factory.New<CusUnderbond>();
			var underbond2 = Factory.New<CusUnderbond>();
			underbond2.C4_ParentTableCode = CusMAWBSchema.Constants.Prefix;
			var underbond3 = Factory.New<CusUnderbond>();
			underbond3.C4_ParentTableCode = CusSCAContainerSchema.Constants.Prefix;
			var underbond4 = Factory.New<CusUnderbond>();
			underbond4.C4_ApplicationCode = "T#@";
			Factory.Save();
			var testCollection = new AirOrStandAloneCusUnderbondCollection(Factory);
			testCollection.Load();
			AssertEquals(true, testCollection.Contains(underbond1.PK));
			AssertEquals(false, testCollection.Contains(underbond3.PK));
			AssertEquals(true, testCollection.Contains(underbond2.PK));
			AssertEquals(false, testCollection.Contains(underbond4.PK));
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new AirOrStandAloneCusUnderbondCollection(Factory);
		}
	}
}
