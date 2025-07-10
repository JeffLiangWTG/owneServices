using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.AsycudaCustoms.Business
{
	public class AsycudaDeclarationTypesProvider : DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProvider, Integration.Customs.AsycudaCustoms.IAsycudaDeclarationTypesProvider
	{
		public ReadOnlyCodeDescriptionPairList GetCodeDescriptionPairList()
		{
			var result = new CodeDescriptionPairList();
			var provider = new AsycudaCustomsCountryProvider();
			var countries = provider.GetAsycudaCustomsCountryCodes();
			var factory = new BusinessObjectFactory();

			foreach (var dataGroupingCode in countries)
			{
				if (ZZCustomsFunctionalityEffectiveDate.IsFunctionalityValid(Universal.Constants.FunctionalityTypes.Risk, dataGroupingCode, ZDateTime.Today))
				{
					var styles = EntryInstructionStyleListHelper.EntryInstructionStyleListForDataGrouping(factory, dataGroupingCode);
					foreach (CodeDescriptionPair style in styles)
					{
						result.AddPairIfNotExist(style.Code, style.Code);
					}
				}
			}
			result.Sort();
			return result;
		}
	}
}
