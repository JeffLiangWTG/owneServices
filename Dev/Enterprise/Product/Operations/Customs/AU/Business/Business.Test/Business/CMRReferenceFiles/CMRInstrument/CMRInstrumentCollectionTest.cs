using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CMRInstrumentCollection))]
	sealed class CMRInstrumentCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestCodeTypeParameterContructor()
		{
			CMRInstrumentCollection collection = new CMRInstrumentCollection(Factory, "ZZZ");
			ZString query = ((ILegacyBusinessObjectCollectionInternals)collection).AdditionalFilter.LiteralTextADO;
			AssertEquals("Query includes the passed parameter", true, query.Contains("ZZZ"));
			AssertEquals("FilterBusinessObjectDefaults", true, collection.FilterBusinessObjectDefaults.ContainsDefaultFor("Instrument Type" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property"));
			AssertEquals("Additional Filter Not matched", "This record is not type of ZZZ", collection.GetAllNotificationsWhenAdditionalFilterNotMet(null));
		}

		protected override BusinessObjectCollection GetCollectionToTest() => new CMRInstrumentCollection(Factory);
	}
}
