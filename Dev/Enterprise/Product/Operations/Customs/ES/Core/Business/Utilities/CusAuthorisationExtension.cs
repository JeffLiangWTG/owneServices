using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business
{
	public static class CusAuthorisationExtension
	{
		public static ZBool IsAuthorizedHolder(this JobDocAddress docAddress, BusinessObjectFactory factory, ZString countryCode)
		{
			var result = false;
			if (docAddress != null && docAddress.OrganisationPK.IsValid)
			{
				var authorisationHeaders = CusAuthorisationHeader.Loader.GetAuthorisations(factory, countryCode, new ZString[] { CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTransit }, ZDateTime.Today, docAddress.OrganisationPK);
				if (authorisationHeaders != null && authorisationHeaders.Any())
				{
					result = true;
				}
			}
			return result;
		}
	}
}
