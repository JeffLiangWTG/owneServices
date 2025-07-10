using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.AE.Business;

public class CusEngineValidation : Customs.Business.CusEngineValidation
{
	public CusEngineValidation(AutoCusEngine parent) : base(parent)
	{
	}

	protected override void CheckCEG_EngineNumber()
	{
		base.CheckCEG_EngineNumber();
		var targetInfo = Parent.CEG_EngineNumberInfo;
		MandatoryValidation.MessageErrorIfNotEntered(targetInfo);
	}

	protected override void CheckCEG_CapacityCC()
	{
		base.CheckCEG_CapacityCC();
		var targetInfo = Parent.CEG_CapacityCCInfo;
		MandatoryValidation.MessageErrorIfIsNegative(targetInfo);
	}

	protected override void CheckCEG_CapacityCCIsValidZDecimal()
	{
		TypeValidation.CheckValidDecimal(Parent.CEG_CapacityCCInfo, CusEngine.Schema.CEG_CapacityCCPrecision, CusEngine.Schema.CEG_CapacityCCScale);
	}
}
