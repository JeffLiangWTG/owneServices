using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CMRCodeListsCollection))]
	sealed class CMRCodeListsCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestCodeTypeParameterContructor()
		{
			CMRCodeListsCollection collection = new CMRCodeListsCollection(Factory, "ZZZ");
			ZString query = ((ILegacyBusinessObjectCollectionInternals)collection).AdditionalFilter.LiteralTextADO;
			AssertEquals("Query includes the passed parameter", true, query.Contains("ZZZ"));
			AssertEquals("FilterBusinessObjectDefaults", true, collection.FilterBusinessObjectDefaults.ContainsDefaultFor("Code Type" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property"));
			AssertEquals("Additional Filter Not matched", "This record is not type of ZZZ", collection.GetAllNotificationsWhenAdditionalFilterNotMet(null));
		}

		protected override BusinessObjectCollection GetCollectionToTest() => new CMRCodeListsCollection(Factory);
	}
}
