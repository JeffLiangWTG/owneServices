using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(CACSubLocationCollection))]
	sealed class CACSubLocationCollectionTest : NonPersistentBusinessObjectCollectionTestCase<CACSubLocationCollection>
	{
		public void TestLoad()
		{
			var otherFactory = new BusinessObjectFactory();
			var helper = new UniversalReferenceTestDataHelper(otherFactory);
			helper.CreateCusCodeType("SUBLC", "Sub Location Codes");
			helper.CreateCusCodeType("CUSOF", "Customs Office");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Canada, "SUBLC", "1111", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Canada, "SUBLC", "2222", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Canada, "CUSOF", "3333", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			helper.CreateCusCodeList(Core.Constants.CountryCodes.UnitedStates, "SUBLC", "4444", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			otherFactory.Save();

			var locationCollection = new CACSubLocationCollection(Factory);
			locationCollection.Load();
			AssertEquals("CACSubLocationCollection.Count", 2, locationCollection.Count);
			AssertNotNull(locationCollection.Cast<CACSubLocation>().FirstOrDefault(x => x.Code == "1111"));
			AssertNotNull(locationCollection.Cast<CACSubLocation>().FirstOrDefault(x => x.Code == "2222"));
		}

		protected override CACSubLocationCollection GetCollectionToTest()
		{
			return new CACSubLocationCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return CACSubLocationTest.CreateSubLocation(Factory, "XXX");
		}
	}
}
