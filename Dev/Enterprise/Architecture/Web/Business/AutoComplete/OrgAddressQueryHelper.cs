using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.Web.Business
{
	public static class OrgAddressQueryHelper
	{
		public static ZDBOnlySubQuery GetAddressTypeSubQuery(OrgAddressType addressType, bool includeMainAddress = true)
		{
			var addressTypeSubQuery = GetAddressTypeSubQueryCore(addressType);
			if (includeMainAddress)
			{
				var mainOfficeSubQuery = GetAddressTypeSubQueryCore(OrgAddressType.Office, true);
				addressTypeSubQuery.AddAsUnionQuery(mainOfficeSubQuery);
			}

			return addressTypeSubQuery;
		}

		static ZDBOnlySubQuery GetAddressTypeSubQueryCore(OrgAddressType addressType, bool mainAddressOnly = false)
		{
			var subQuery = new ZDBOnlySubQuery(typeof(OrgAddressCapability), OrgAddressCapabilitySchema.PZ_OA);
			subQuery.AddToFilter(OrgAddressCapabilitySchema.PZ_AddressType, addressType.Code);
			if (mainAddressOnly)
			{
				subQuery.AddToFilter(OrgAddressCapabilitySchema.PZ_IsMainAddress, ZBool.True);
			}

			return subQuery;
		}
	}
}
