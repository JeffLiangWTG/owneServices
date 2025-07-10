using CargoWise.Types;

namespace Enterprise.Customs.IE.Business
{
	public interface IExportCusAuthorizationUsageValidationStrategy
	{
		string[] GetRulesApplicableToCode(ZString agc_Code);
	}
}
