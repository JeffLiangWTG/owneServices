using CargoWise.EntityFramework;

namespace Enterprise.ZArchitecture.Business
{
	public interface IRefCountryRulesValidator
	{
		void ValidateRule(BusinessObject bizO, ZPropertyInfo property, string noteText);
	}
}
