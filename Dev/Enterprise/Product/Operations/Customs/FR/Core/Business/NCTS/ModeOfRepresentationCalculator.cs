using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.Business.NCTS
{
	public class ModeOfRepresentationCalculator : EU.NCTS.Business.ModeOfRepresentationCalculator
	{
		public ModeOfRepresentationCalculator(NctsHeader nctsHeader) : base(nctsHeader)
		{
		}

		protected override ZString GetModeOfRepresentationCore()
		{
			ZString result = EU.NCTS.Business.NctsConstants.ModeOfRepresentation.Codes.Two;

			if (nctsHeader.Declarant?.Organisation is OrgHeader declarantOrganisation && nctsHeader.Principal?.Organisation is OrgHeader principalOrganisation)
			{
				if (OrgCusAccount.Loader.LoadByCodeAndCountry(nctsHeader.Factory, declarantOrganisation.PK, Business.OrgCusAccountCodeList.Codes.DTA, nctsHeader.CountryCode).Select(x => x.CZ_Account).ToArray() is ZString[] declarantDTAs && declarantDTAs.Length > 0 &&
					OrgCusAccount.Loader.LoadByCodeAndCountry(nctsHeader.Factory, principalOrganisation.PK, Business.OrgCusAccountCodeList.Codes.DTA, nctsHeader.CountryCode).Select(x => x.CZ_Account).Distinct().ToHashSet() is HashSet<ZString> principalDTAs && principalDTAs.Count > 0 && declarantDTAs.Any(x => principalDTAs.Contains(x)))
				{
					result = EU.NCTS.Business.NctsConstants.ModeOfRepresentation.Codes.One;
				}
			}

			return result;
		}
	}
}
