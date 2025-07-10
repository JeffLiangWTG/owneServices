using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.NCTS.Business
{
	sealed class NctsRuleC0542_1Validation
	{
		internal NctsRuleC0542_1Validation(NctsHeader nctsHeader)
		{
			this.nctsHeader = Argument.NotNull(nctsHeader, nameof(nctsHeader));
			movementHeader = nctsHeader.MovementHeader;
		}

		internal void ValidateConsignor(ZPropertyInfo targetInfo, JobDocAddress consignorAddress, string parentHumanReadableName)
		{
			if (movementHeader is null
				|| !nctsHeader.Configuration.ValidationRuleConfiguration.IsRuleC0542_1Active
				|| !nctsHeader.IsPhase5Departure
				|| consignorAddress.OrganisationPK.IsEmpty)
			{
				return;
			}

			if (SecurityNonAndReducedDataset)
			{
				targetInfo.AddMessageError(Res.GetString("8A239FF4-0FF2-4DAA-B4A7-D19101C7EA6D", "{0} The Consignor in the {1} must not be entered if Security is NON and Reduced Dataset Ind. is flagged.", RuleCodePrefix, parentHumanReadableName));
			}
		}

		internal void ValidateHouseConsignor(ZPropertyInfo targetInfo, JobDocAddress houseConsignorAddress)
		{
			if (movementHeader is null
				|| !nctsHeader.Configuration.ValidationRuleConfiguration.IsRuleC0542_1Active
				|| !nctsHeader.IsPhase5Departure
				|| houseConsignorAddress.OrganisationPK.IsEmpty
				|| nctsHeader.Consignor.OrganisationPK.IsEmpty)
			{
				return;
			}

			if (!SecurityNonAndReducedDataset)
			{
				targetInfo.AddMessageError(Res.GetString("10DEEFC9-D6EE-4410-92F1-87997FD8928E", "{0} When Security is different from 'NON' or Reduced Dataset Ind. is not flagged, the Consignor in the House must not be entered if the Consignor in the Consignment Header is entered.", RuleCodePrefix));
			}
		}

		string RuleCodePrefix => ValidationRuleCodeConstants.C0542_1.GetRuleCodeMessagePrefix();

		bool SecurityNonAndReducedDataset => movementHeader.BM_TypeOfSecurity == NctsTypeOfSecurityList.Codes.NON
			&& movementHeader.BM_ReducedDatasetIndicator;

		readonly NctsHeader nctsHeader;
		readonly NctsDepartureMovementHeader movementHeader;
	}
}
