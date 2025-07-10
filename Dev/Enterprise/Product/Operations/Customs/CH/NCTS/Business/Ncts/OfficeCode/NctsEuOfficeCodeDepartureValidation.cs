using Enterprise.Core;
using Enterprise.Customs.Business.Extensions;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.CH.NCTS.Business;

public class NctsEuOfficeCodeDepartureValidation : NctsEuOfficeCodePhase5DepartureValidation
{
	public NctsEuOfficeCodeDepartureValidation(NctsEuOfficeCode parent) : base(parent)
	{
	}

	new NctsEuOfficeCode Parent => (NctsEuOfficeCode)base.Parent;

	public override void ValidateAll()
	{
		base.ValidateAll();
		ValidateEstimatedNumberOfDays();
	}

	protected override void CheckCY_Code()
	{
		base.CheckCY_Code();

		PassarValidation.CheckNP70041(Parent.CY_CodeInfo, Parent);
		PassarValidation.CheckNP70231(Parent.CY_CodeInfo, Parent);
	}

	protected override void CheckCY_Data()
	{
		base.CheckCY_Data();
		CheckCHOfficeForNationalTransit();
	}

	public void ValidateEstimatedNumberOfDays()
	{
		ValidateCalculatedProperty(Parent.EstimatedNumberOfDaysInfo);
	}

	protected void CheckEstimatedNumberOfDays()
	{
		NctsEuOfficeCodeDepartureValidationHelper.CheckEstimatedNumberOfDays(Parent);

		var movementHeader = Parent.MovementHeader;
		if (movementHeader != null)
		{
			if (movementHeader.Header?.IsInPhase5TransitionPeriod ?? false)
			{
				CheckEstimatedNumberOfDays_RuleCodeNS30033();
			}
			else
			{
				CheckEstimatedNumberOfDays_RuleCodeNS30113();
			}
		}
	}

	void CheckEstimatedNumberOfDays_RuleCodeNS30033()
	{
		if ((Parent.MovementHeader.BM_TypeOfSecurity == NctsTypeOfSecurityList.Codes.ENT
			|| Parent.MovementHeader.BM_TypeOfSecurity == NctsTypeOfSecurityList.Codes.EXI
			|| Parent.MovementHeader.BM_TypeOfSecurity == NctsTypeOfSecurityList.Codes.BTH)
			&& Parent.CY_Code.Equals(OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit))
		{
			if (Parent.EstimatedNumberOfDays.IsEmpty)
			{
				Parent.EstimatedNumberOfDaysInfo.AddMessageError(PassarValidationMessages.MessageNS30033);
			}
		}
	}

	void CheckEstimatedNumberOfDays_RuleCodeNS30113()
	{
		if ((Parent.MovementHeader.BM_TypeOfSecurity == NctsTypeOfSecurityList.Codes.ENT
			|| Parent.MovementHeader.BM_TypeOfSecurity == NctsTypeOfSecurityList.Codes.BTH)
			&& Parent.CY_Code.Equals(OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit)
			&& Parent.Factory.IsCountryConsideredInEuForSafetyAndSecurity(Parent.CY_Data.Left(2)))
		{
			if (Parent.EstimatedNumberOfDays.IsEmpty)
			{
				Parent.EstimatedNumberOfDaysInfo.AddMessageError(PassarValidationMessages.MessageNS30113);
			}
		}
	}

	void CheckCHOfficeForNationalTransit()
	{
		if (Parent.MovementHeader.BM_InBondEntryType == NctsConstants.NctsTypeOfDeclaration.Codes.NationalTransitSwitzerland
			&& Parent.CY_Code == OfficeCodes_NCTS.Codes.NCTSOfficeOfDestination
			&& !Parent.CY_Data.IsEmpty
			&& Parent.CY_Data.SubstringSafe(0, 2) != Constants.CountryCodes.Switzerland)
		{
			Parent.CY_DataInfo.AddMessageError(Res.GetString("EE355284-04C0-467B-A24A-15AF6DFD02AD", "You must enter a Customs Office starting with 'CH' (National Customs Office)."));
		}
	}
}
