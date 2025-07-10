using Enterprise.Customs.Business;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.BE.NCTS.Business.Testing;

[TestedType(typeof(LocationOfGoodsFromAuthorisationDefaulterConfiguration))]
sealed class LocationOfGoodsFromAuthorisationDefaulterConfigurationTest : LocationOfGoodsFromAuthorisationDefaulterConfigurationAbstractTest<LocationOfGoodsFromAuthorisationDefaulterConfiguration>
{
	public override void TestIsDefaultingEnabled() => AssertEquals(nameof(configuration.IsDefaultingEnabled), expected: true, configuration.IsDefaultingEnabled);
	public override void TestQualifierCode() => AssertEquals(nameof(configuration.QualifierCode), expected: CusGoodsLocationQualifierList.Codes.UnLocode, configuration.QualifierCode);
	public override void TestTypeCode() => AssertEquals(nameof(configuration.TypeCode), expected: CusGoodsLocationTypeList.Codes.ApprovedPlace, configuration.TypeCode);
}
