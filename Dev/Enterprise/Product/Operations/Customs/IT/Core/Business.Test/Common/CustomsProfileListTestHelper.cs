using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IT.Business.Testing;

public static class CustomsProfileListTestHelper
{
	public static void ClearCustomsProfilesLookupsCache(BusinessObjectFactory factory, ZGuid effectiveCompanyPK)
	{
		var key = $"IT.NodeListLookups|NodesForCompany_{effectiveCompanyPK}";
		factory.ClearCachedValue<CodeDescriptionPairList>(key);
	}
}
