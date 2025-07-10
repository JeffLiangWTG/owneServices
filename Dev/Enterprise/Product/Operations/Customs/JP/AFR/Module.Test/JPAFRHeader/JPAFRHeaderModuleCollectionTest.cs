using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.JP.AFR.Business;
using NUnit.Framework;

namespace Enterprise.Customs.JP.AFR.Module.Testing
{
	[TestedType(typeof(JPAFRHeaderModuleCollection))]
	class JPAFRHeaderModuleCollectionTest : ActiveBusinessObjectCollectionTestCase<JPAFRHeaderModuleCollection>
	{
		public void TestGetJPAFRHeaders()
		{
			var testHeader1 = Factory.NewWithValidTestData<JPAFRHeader>();
			var testHeader2 = Factory.NewWithValidTestData<JPAFRHeader>();
			testHeader1.JPH_IsShippingLineEntry = false;
			testHeader2.JPH_IsShippingLineEntry = true;
			Factory.Save();

			CombineAssertions(() =>
			{
				var testCollection = GetCollectionToTest();
				AssertEquals(2, testCollection.Count);
				Assert(testCollection.Any(header => header.PK == testHeader1.PK));
				Assert(testCollection.Any(header => header.PK == testHeader2.PK));
			});
		}

		protected override JPAFRHeaderModuleCollection GetCollectionToTest() => new JPAFRHeaderModuleCollection(Factory);
	}
}
