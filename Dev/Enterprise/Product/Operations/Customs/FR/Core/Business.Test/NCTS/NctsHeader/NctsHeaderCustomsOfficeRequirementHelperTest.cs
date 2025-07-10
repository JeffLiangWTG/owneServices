using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.NCTS.Testing
{
	[TestedType(typeof(NctsHeaderCustomsOfficeRequirementHelper))]
	sealed class NctsHeaderCustomsOfficeRequirementHelperTest : EU.NCTS.Business.Testing.NctsHeaderCustomsOfficeRequirementHelperAbstractTest<NctsHeaderCustomsOfficeRequirementHelper>
	{
		protected override string ExpectedCacheKey => "FR.NctsHeaderCustomsOfficeRequirementHelper.OtherRequirements.D.T1.NCT";

		protected override IEnumerable<CustomsOfficeRequirement> ExpectedOtherRequirements
		{
			get
			{
				var result = new List<CustomsOfficeRequirement>(base.ExpectedOtherRequirements);
				result.Add(new CustomsOfficeRequirement("CAU", false, false, "Competent Authority from the country of departure"));
				result.Add(new CustomsOfficeRequirement("REC", false, false, "Competent Authority of Recovery"));
				result.Add(new CustomsOfficeRequirement("PRE", false, false, "Office of Presentation"));
				return result;
			}
		}

		protected override IEnumerable<ZString> Phase4CustomsOfficeRequirementWithRoles() => base.Phase4CustomsOfficeRequirementWithRoles().Concat(new ZString[]
		{
			EuOfficeCodesTypes.Codes.CompetentAuthorityCountryOfDep,
			EuOfficeCodesTypes.Codes.CompetentAuthorityOfRecovery,
			EuOfficeCodesTypes.Codes.OfficeOfPresentation
		});

		protected override IEnumerable<ZString> Phase5CustomsOfficeRequirementWithRoles() => base.Phase5CustomsOfficeRequirementWithRoles().Concat(new ZString[]
		{
			EuOfficeCodesTypes.Codes.CompetentAuthorityCountryOfDep,
			EuOfficeCodesTypes.Codes.CompetentAuthorityOfRecovery,
			EuOfficeCodesTypes.Codes.OfficeOfPresentation
		});
	}
}
