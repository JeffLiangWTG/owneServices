using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.DE.Module.Testing
{
	sealed class JobDeclarationFilterLookupsTest : TestCaseWithFactory
	{
		public void TestDeclarationLookup()
		{
			var lookups = new JobDeclarationFilterLookups(new JobDeclarationFilterBusinessObject());
			var declarationTypeList = lookups.DeclarationTypeList;
			CombineAssertions(() =>
			{
				AssertEquals("Codes", "000000, 000100, 000110, 000200, 000210, 000400, 000410, 000901, 000902, 001300, 001310, 001410, 110000, 110100, 110110, 110200, 110210, 110400, 110410, 111300, 111310, 111410, 120000, 120100, 120110, 120200, 120210, 200000, 200100, 200110, 200200, 200210, 200400, 200410, 201300, 201310, 201410, AAV, AVABR, AZ, AZL, BA, EAV, EGN, EGZ, EZA, EZL, LÜZ, VAV, VZA, VZL", declarationTypeList.CodesAsString);
				AssertSame("Cached", declarationTypeList, lookups.DeclarationTypeList);
			});
		}
	}
}
