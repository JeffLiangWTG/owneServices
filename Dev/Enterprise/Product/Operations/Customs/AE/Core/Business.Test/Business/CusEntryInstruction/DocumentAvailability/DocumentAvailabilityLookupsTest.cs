using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.AE.Business.Testing;

sealed class DocumentAvailabilityLookupsTest : BusinessObjectLookupsTestCase
{
	public void TestDocumentTypeList()
		=> AssertSame(Factory.GetCachedValue<DocumentTypeList>(), Lookups.DocumentTypeList);

	public void TestAvailabilityStatusList()
		=> AssertSame(Factory.GetCachedValue<AvailabilityStatusList>(), Lookups.AvailabilityStatusList);

	public void TestReasonCodeList()
		=> AssertSame(Factory.GetCachedValue<ReasonCodeList>(), Lookups.ReasonCodeList);

	DocumentAvailabilityLookups Lookups => lookups ??= Factory.New<DocumentAvailability>().Lookups;
	DocumentAvailabilityLookups lookups;
}
