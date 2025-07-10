using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.CA.Business
{
	public interface ICasualImportTaxData : IFactoryProvider
	{
		ZBool IsEffectiveCasualImport { get; }
		ZString EffectiveCasualImportCommodity { get; }
		ZString EffectiveCasualImportDestinationProvince { get; }
		ZString EffectiveCasualImportClearanceProvince { get; }
		ZDate EffectiveDate { get; }
		ZBool IsExempt { get; }
	}
}
