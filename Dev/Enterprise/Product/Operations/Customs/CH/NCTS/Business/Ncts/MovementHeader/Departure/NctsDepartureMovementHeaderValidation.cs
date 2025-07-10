using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.CH.NCTS.Business;

public class NctsDepartureMovementHeaderValidation : NctsDepartureMovementHeaderPhase5Validation
{
	public NctsDepartureMovementHeaderValidation(NctsDepartureMovementHeader parent) : base(parent)
	{
	}

	protected new NctsDepartureMovementHeader Parent => (NctsDepartureMovementHeader)base.Parent;

	protected override void CheckBM_PaperlessInbondNum()
	{
		base.CheckBM_PaperlessInbondNum();
		if (Parent.ShouldGenerateLocalReferenceNumberOnSaving && Parent.BM_PaperlessInbondNum.IsEmpty && EnvironmentHelper.GetBusinessPartnerId().IsEmpty)
		{
			Parent.BM_PaperlessInbondNumInfo.AddMessageError(Res.GetString("27F07297-34B9-4622-930F-497386F33135", "Company or Branch must have a Business Partner ID (BID) in order to generate the LRN number."));
		}
	}

	protected override void CheckBM_InBondEntryType()
	{
		base.CheckBM_InBondEntryType();
		ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.BM_InBondEntryTypeInfo);
		PassarValidation.CheckNS30068(Parent.BM_InBondEntryTypeInfo, Parent);
		PassarValidation.CheckNS30163(Parent.BM_InBondEntryTypeInfo, Parent);
		PassarValidation.CheckNP70176(Parent.BM_InBondEntryTypeInfo, Parent);
		PassarValidation.CheckNP70278(Parent.BM_InBondEntryTypeInfo, Parent);
	}

	protected override void CheckBM_SpecificCircumstance()
	{
		base.CheckBM_SpecificCircumstance();
		ListValidation.MessageErrorIfInvalidCode(Parent.BM_SpecificCircumstanceInfo);
	}

	protected override void CheckBM_TypeOfSecurity()
	{
		base.CheckBM_TypeOfSecurity();

		var mrn = Parent.Header.MovementReferenceNumber;

		if (!Parent.IsNationalTransitSwitzerland && !mrn.IsEmpty)
		{
			string GetDecodedMRNSecurityCode(string decodeSecurity)
			{
				switch (decodeSecurity)
				{
					case "J":
						return NctsTypeOfSecurityList.Codes.NON;
					case "K":
						return NctsTypeOfSecurityList.Codes.EXI;
					case "L":
						return NctsTypeOfSecurityList.Codes.ENT;
					case "M":
						return NctsTypeOfSecurityList.Codes.BTH;
					default:
						return null;
				}
			}

			ZString decodedMRNSecurityCode = GetDecodedMRNSecurityCode(mrn.SubstringSafe(16, 1));

			if (!decodedMRNSecurityCode.IsEmpty && Parent.BM_TypeOfSecurity != decodedMRNSecurityCode)
			{
				Parent.BM_TypeOfSecurityInfo.AddMessageError(PassarValidationMessages.MessageNP70061(decodedMRNSecurityCode));
			}
		}
	}

	protected override void CheckMandatoryGuaranteeIfNeeded(EU.NCTS.Business.NctsHeader nctsHeader, ZPropertyInfo info)
	{
		if (!Parent.IsNationalTransitSwitzerland)
		{
			base.CheckMandatoryGuaranteeIfNeeded(nctsHeader, info);
		}
	}

	protected override void CheckBM_ActiveBorderIdentificationType()
	{
		base.CheckBM_ActiveBorderIdentificationType();

		MandatoryValidation.AddMessageErrorIfNotEnteredAndOtherPropertyIsEntered(Parent.BM_ActiveBorderIdentificationTypeInfo, Parent.BM_ExportTransportModeInfo);
	}

	protected override void CheckBM_ExportTimeLimit()
	{
		base.CheckBM_ExportTimeLimit();

		PassarValidation.CheckNP70123(Parent.BM_ExportTimeLimitInfo, Parent);
	}
}
