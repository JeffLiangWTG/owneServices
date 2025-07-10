using CargoWise.EntityFramework;
using static Enterprise.Customs.CH.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.CH.Business;

public class AdditionalInformationValidation : Customs.Business.CusSupportingInfoValidation
{
	public AdditionalInformationValidation(AdditionalInformation parent) : base(parent)
	{
	}

	protected new AdditionalInformation Parent => (AdditionalInformation)base.Parent;

	protected PlausiValidation PlausiValidation => plausiValidation ?? (plausiValidation = PlausiValidation.New(Parent.Parent));
	PlausiValidation plausiValidation;

	protected override void CheckCSI_Code()
	{
		base.CheckCSI_Code();
		ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CSI_CodeInfo);

		PlausiValidation.CheckNP70213_V1201(Parent.CSI_CodeInfo, Parent);
		PlausiValidation.CheckNS30003_CusSupportingInfo(Parent, PassarValidationMessages.MessageNS30003_NotAllowed(Parent.HumanReadableName));
		PlausiValidation.CheckNS30104(Parent.CSI_CodeInfo, Parent);
	}

	protected override void CheckCSI_Description()
	{
		base.CheckCSI_Description();
		PlausiValidation.CheckNP70168(Parent.CSI_DescriptionInfo, Parent);
		PlausiValidation.CheckNP70212(Parent.CSI_DescriptionInfo, Parent);
		PlausiValidation.CheckNP70213_V1202(Parent.CSI_DescriptionInfo, Parent);
		PlausiValidation.CheckNP70197(Parent.CSI_DescriptionInfo, Parent);

		if (Parent.Parent is JobComInvoiceLine)
		{
			PlausiValidation.CheckNP70195(Parent.CSI_DescriptionInfo, Parent);
			PlausiValidation.CheckNP70155(Parent.CSI_DescriptionInfo, Parent);
		}
	}

	protected override void CheckCSI_ReferenceNumber()
	{
		base.CheckCSI_ReferenceNumber();

		if (!(Parent.Parent is CusEntryInstruction))
		{
			switch (Parent.CSI_Code)
			{
				case AdditionalInformationTypeCodes.FreeZoneTraffic:
				case AdditionalInformationTypeCodes.BorderZoneTraffic:
				case AdditionalInformationTypeCodes.ExportCodeMineralOil:
					ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CSI_ReferenceNumberInfo);
					break;
				default:
					if (!Parent.Parent.JobDeclaration.IsExportOrExportDeclarationActivation)
					{
						MandatoryValidation.MessageErrorIfNotEntered(Parent.CSI_ReferenceNumberInfo);
					}
					break;
			}
		}
	}
}
