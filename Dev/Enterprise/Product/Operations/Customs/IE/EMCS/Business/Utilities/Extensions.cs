using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IE.EMCS.Business
{
	public static class Extensions
	{
		public static OrgHeader GetOrgHeaderByCustomsRegNo(this BusinessObjectFactory factory, ZString countryCode, ZString codeType, ZString regNo)
		{
			OrgHeader result = null;
			if (factory != null && !countryCode.IsEmpty && !codeType.IsEmpty && !regNo.IsEmpty)
			{
				result = new OrgHeader.Loader(factory).LoadDBOrganisations(countryCode, codeType, regNo).OrderBy(x => x.OH_SystemCreateTimeUtc).FirstOrDefault();
			}
			return result;
		}

		public static OrgAddress GetOrgAddressByCustomsRegNo(this BusinessObjectFactory factory, ZString countryCode, ZString codeType, ZString regNo)
		{
			OrgAddress result = null;
			if (factory != null && !countryCode.IsEmpty && !codeType.IsEmpty && !regNo.IsEmpty)
			{
				result = new OrgAddress.Loader(factory).LoadDBAddresses(countryCode, codeType, regNo).OrderBy(x => x.OA_SystemCreateTimeUtc).FirstOrDefault();
			}
			return result;
		}
	}
}
