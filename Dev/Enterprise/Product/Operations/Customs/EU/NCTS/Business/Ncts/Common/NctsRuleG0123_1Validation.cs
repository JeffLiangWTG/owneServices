using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.NCTS.Business
{
	sealed class NctsRuleG0123_1Validation
	{
		internal NctsRuleG0123_1Validation(NctsHeader nctsHeader)
		{
			this.nctsHeader = Argument.NotNull(nctsHeader, nameof(nctsHeader));
		}

		internal void ValidateConsignor(ZPropertyInfo targetInfo, JobDocAddress consignorAddress, string parentHumanReadableName)
		{
			var consignorOrganisationPK = consignorAddress.OrganisationPK;
			if (consignorOrganisationPK.IsEmpty
				|| !nctsHeader.Configuration.ValidationRuleConfiguration.IsRuleG0123_1Active
				|| !nctsHeader.IsPhase5Departure)
			{
				return;
			}

			var ruleCodePrefix = ValidationRuleCodeConstants.G0123_1.GetRuleCodeMessagePrefix();
			var principal = nctsHeader.Principal;
			if (consignorOrganisationPK == principal.OrganisationPK)
			{
				targetInfo.AddMessageError(Res.GetString("B8BED153-8F3E-4778-9231-49F4D4881152", "{0} The Consignor in the {1} must be entered only if different from the Principal.", ruleCodePrefix, parentHumanReadableName));
				return;
			}

			if (consignorAddress.Organisation.HasSameEoriOrTcu(principal.Organisation))
			{
				targetInfo.AddMessageError(Res.GetString("E92DB752-0441-4D5E-ADAA-F35E1DDA9A40", "{0} Consignor EOR/TCU code is the same as Principal EOR/TCU code. The Consignor in the {1} must be entered only if different from the Principal.", ruleCodePrefix, parentHumanReadableName));
			}
		}

		readonly NctsHeader nctsHeader;
	}
}
