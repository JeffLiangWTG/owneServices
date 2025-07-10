using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.EMCS.Business
{
	public class EMCSCustomsOfficeRequirementHelper : CustomsOfficeRequirementHelper
	{
		public EMCSCustomsOfficeRequirementHelper(EMCSJobDeclaration declaration) : base(declaration)
		{
			mainOfficeRequirement = new CustomsOfficeRequirement(MainOfficeTypeCode, true, false) { OfficeRolesForLookup = new ZString[] { UniversalReferenceConstants.CustomsOfficeAttributes.Excise } };
		}

		public const string MainOfficeTypeCode = OfficeCodes_EMCS.Codes.CompetentAuthorityOfDispatch;

		readonly CustomsOfficeRequirement mainOfficeRequirement;

		protected EMCSJobDeclaration Declaration => (EMCSJobDeclaration)OfficeCodeProvider;

		protected override CustomsOfficeRequirement GetMainOffice() => mainOfficeRequirement;

		protected CodeDescriptionPairList OtherOfficeCodes => Factory.GetCachedValue("EMCSCustomsOfficeRequirementHelper.OtherOfficesCodes", () =>
		{
			var result = new OfficeCodes_EMCS();
			result.RemoveCode(MainOfficeTypeCode);
			return result;
		});

		protected override IEnumerable<CustomsOfficeRequirement> GetOtherRequirements() => Factory.GetCachedValue("EMCSCustomsOfficeRequirementHelper.OtherRequirements", () =>
		{
			var otherRequirementsList = new List<CustomsOfficeRequirement>();
			foreach (CodeDescriptionPair pair in OtherOfficeCodes)
			{
				otherRequirementsList.Add(new CustomsOfficeRequirement(pair.Code, true, false) { OfficeRolesForLookup = new ZString[] { UniversalReferenceConstants.CustomsOfficeAttributes.Excise } });
			}
			return otherRequirementsList;
		});
	}
}
