using System.Collections.Generic;
using System.Linq;
using Enterprise.Customs.EU.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.FR.Business.NCTS
{
	public class NctsHeaderCustomsOfficeRequirementHelper : EU.NCTS.Business.NctsHeaderCustomsOfficeRequirementHelper
	{
		public NctsHeaderCustomsOfficeRequirementHelper(EU.NCTS.Business.NctsHeader header) : base(header)
		{
		}

		protected override string GetCacheKeyCombination() => string.Join(".", CountryCodes.France, base.GetCacheKeyCombination());

		protected override IEnumerable<CustomsOfficeRequirement> GetOtherRequirementsCore()
		{
			return base.GetOtherRequirementsCore().Concat(new CustomsOfficeRequirement[]
			{
				new CustomsOfficeRequirement(EuOfficeCodesTypes.Codes.CompetentAuthorityCountryOfDep, false, false, Res.GetString("da1bb1d7-1593-4ab6-ae73-04cbd912ea06", "Competent Authority from the country of departure")),
				new CustomsOfficeRequirement(EuOfficeCodesTypes.Codes.CompetentAuthorityOfRecovery, false, false),
				new CustomsOfficeRequirement(EuOfficeCodesTypes.Codes.OfficeOfPresentation, false, false)
			});
		}
	}
}
