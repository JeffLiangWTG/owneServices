using CargoWise.EntityFramework;

namespace Enterprise.UniversalDataBuss.Integration
{
	public interface IUniversalValidationRulesEvaluator
	{
		void EvaluateRules(IDataObject dataObject, BusinessObject businessObject, IXmlImportLogger logger);
	}
}
