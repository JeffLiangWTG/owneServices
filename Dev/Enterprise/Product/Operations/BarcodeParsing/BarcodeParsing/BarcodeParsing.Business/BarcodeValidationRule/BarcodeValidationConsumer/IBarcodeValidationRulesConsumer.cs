using CargoWise.Types;

namespace Enterprise.BarcodeParsing.Business
{
	public interface IBarcodeValidationRulesConsumer
	{
		string ValidateTargetFieldForValidationRules(ZString targetField);
	}
}
