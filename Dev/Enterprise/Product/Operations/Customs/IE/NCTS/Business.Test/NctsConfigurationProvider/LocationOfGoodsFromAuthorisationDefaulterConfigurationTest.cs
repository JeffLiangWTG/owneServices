using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	[TestedType(typeof(LocationOfGoodsFromAuthorisationDefaulterConfiguration))]
	sealed class LocationOfGoodsFromAuthorisationDefaulterConfigurationTest : EU.NCTS.Business.Testing.LocationOfGoodsFromAuthorisationDefaulterConfigurationAbstractTest<LocationOfGoodsFromAuthorisationDefaulterConfiguration>
	{
		public override void TestQualifierCode()
		{
			AssertEquals("Qualifier Code", CusGoodsLocationQualifierList.Codes.UnLocode, configuration.QualifierCode);
		}

		public override void TestTypeCode()
		{
			AssertEquals("Type Code", CusGoodsLocationTypeList.Codes.AuthorizedPlace, configuration.TypeCode);
		}
	}
}
