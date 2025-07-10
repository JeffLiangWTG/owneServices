using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using ECC = Enterprise.Core.Constants;

namespace Enterprise.Customs.BR.Business.Testing
{
	class AdditionalIdentificationLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestIssuingAgencyList()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType(ECC.Customs.Universal.RefCusCodeListTypes.Codes.BRIssuingAgency, "Issuing Agency");
			helper.CreateCusCodeList(ECC.CountryCodes.Brazil, ECC.Customs.Universal.RefCusCodeListTypes.Codes.BRIssuingAgency, "0000001", "Test1", ZDateTime.Now.AddDays(-5), ZDateTime.Now.AddDays(5));
			Factory.Save();

			var additionalIdentification = Factory.New<AdditionalIdentification>();
			var lookup = new AdditionalIdentificationLookups(additionalIdentification);
			var collection = lookup.IssuingAgencyList;
			collection.Load();
			AssertEquals("Issuing Agency List should have 1 item !", 1, collection.Count);
		}
	}
}
