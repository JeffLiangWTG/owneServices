using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CMRInstrumentTypeListTest : TestCaseWithFactory
	{
		public void TestGetInstrumentTypeMappedFromLegacy()
		{
			AssertEquals("TariffConcessionOrder", CMRInstrumentTypeList.Codes.TariffConcessionOrder, CMRInstrumentTypeList.GetInstrumentTypeMappedFromLegacy(CustomsInstrumentTypeList.Codes.TariffConcession));
			AssertEquals("Determination", CMRInstrumentTypeList.Codes.Determination, CMRInstrumentTypeList.GetInstrumentTypeMappedFromLegacy(CustomsInstrumentTypeList.Codes.MinisterialDetermination));
			AssertEquals("ByLaw", CMRInstrumentTypeList.Codes.ByLaw, CMRInstrumentTypeList.GetInstrumentTypeMappedFromLegacy(CustomsInstrumentTypeList.Codes.ByLaw));
			AssertEquals("TariffQuota", CMRInstrumentTypeList.Codes.TariffQuota, CMRInstrumentTypeList.GetInstrumentTypeMappedFromLegacy(CustomsInstrumentTypeList.Codes.TariffQuota));
		}

		public void TestGetInstrumentTypeMappedFromCMR()
		{
			AssertEquals("TariffConcessionOrder", CustomsInstrumentTypeList.Codes.TariffConcession, CMRInstrumentTypeList.GetInstrumentTypeMappedFromCMR(CMRInstrumentTypeList.Codes.TariffConcessionOrder));
			AssertEquals("Determination", CustomsInstrumentTypeList.Codes.MinisterialDetermination, CMRInstrumentTypeList.GetInstrumentTypeMappedFromCMR(CMRInstrumentTypeList.Codes.Determination));
			AssertEquals("ByLaw", CustomsInstrumentTypeList.Codes.ByLaw, CMRInstrumentTypeList.GetInstrumentTypeMappedFromCMR(CMRInstrumentTypeList.Codes.ByLaw));
			AssertEquals("TariffQuota", CustomsInstrumentTypeList.Codes.TariffQuota, CMRInstrumentTypeList.GetInstrumentTypeMappedFromCMR(CMRInstrumentTypeList.Codes.TariffQuota));
		}
	}
}
