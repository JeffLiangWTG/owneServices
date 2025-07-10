using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.AE.Business;

public class JobDeclarationValidation : AutoAEJobDeclarationValidation
{
	public JobDeclarationValidation(JobDeclaration parent)
		: base(parent)
	{
	}

	public new JobDeclaration Parent
	{
		get { return (JobDeclaration)base.Parent; }
	}

	public override bool IsMasterBillMandatory
	{
		get { return true; }
	}

	protected override void CheckJE_DateOfArrival()
	{
		base.CheckJE_DateOfArrival();
		MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_DateOfArrivalInfo);
	}

	protected override void CheckJE_MarksAndNumbersShort()
	{
		base.CheckJE_MarksAndNumbersShort();
		if (Parent.JE_MarksAndNumbers.Length > 350)
		{
			Parent.JE_MarksAndNumbersShortInfo.AddMessageError("Numbers of characters should not be more than 350");
		}
	}

	protected override void CheckJE_RL_NKPortOfArrival()
	{
		base.CheckJE_RL_NKPortOfArrival();
		MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_RL_NKPortOfArrivalInfo);
	}

	protected override void CheckJE_VoyageFlightNo()
	{
		base.CheckJE_VoyageFlightNo();
		MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_VoyageFlightNoInfo);
	}

	protected override void CheckJE_RL_NKPortOfLoading()
	{
		base.CheckJE_RL_NKPortOfLoading();
		MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_RL_NKPortOfLoadingInfo);
		if (Parent.JE_TransportMode == TransportTypeList.Codes.Air &&
			Parent.PortOfLoading != null &&
			Parent.PortOfLoading.RL_IATA.IsEmpty)
		{
			Parent.JE_RL_NKPortOfLoadingInfo.AddMessageError("Port of Loading has no IATA Code");
		}
	}

	protected override void CheckJE_RL_NKOrigin()
	{
		base.CheckJE_RL_NKOrigin();
		MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_RL_NKOriginInfo);
	}

	protected override void CheckJE_TotalNoOfPacks()
	{
		base.CheckJE_TotalNoOfPacks();
		MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_TotalNoOfPacksInfo);
	}

	protected override void CheckJE_TotalWeight()
	{
		base.CheckJE_TotalWeight();
		MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_TotalWeightInfo);
	}

	protected override void CheckJE_TotalWeightUnit()
	{
		base.CheckJE_TotalWeightUnit();
		ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.JE_TotalWeightUnitInfo, Parent.Lookups.WeightUnitList);
	}

	protected override void CheckJE_TransportMode()
	{
		base.CheckJE_TransportMode();
		ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.JE_TransportModeInfo, Parent.Lookups.TransportTypeList);
	}

	protected override void CheckJE_RL_NKFinalDestination()
	{
		base.CheckJE_RL_NKFinalDestination();
		MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_RL_NKFinalDestinationInfo);
	}

	protected override void CheckJE_OH_Importer()
	{
		base.CheckJE_OH_Importer();
		if (!Parent.IsTranshipment && Parent.IsHighValue)
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_OH_ImporterInfo);
		}
	}

	protected override void CheckJE_OH_Supplier()
	{
		base.CheckJE_OH_Supplier();
		if (!Parent.IsTranshipment && Parent.IsHighValue)
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_OH_SupplierInfo);
		}
	}
}
