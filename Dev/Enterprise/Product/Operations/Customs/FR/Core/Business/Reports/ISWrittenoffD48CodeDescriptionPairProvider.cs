using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.FR.Business.Reports
{
	public class ISWrittenoffD48CodeDescriptionPairProvider : CodeDescriptionPairList,
			Integration.Customs.FR.IISWrittenoffD48CodeDescriptionPairProvider,
			DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProvider
	{
		BusinessObjectFactory Factory => factory ?? (factory = new BusinessObjectFactory());
		BusinessObjectFactory factory;

		public ReadOnlyCodeDescriptionPairList GetCodeDescriptionPairList()
		{
			return Factory.GetCachedValue("FR|" + nameof(ISWrittenoffD48CodeDescriptionPairProvider), () =>
			{
				var result = new CodeDescriptionPairList();
				result.AddPair("YES", Res.GetString("EAE7760F-7ADC-4A58-85B8-03EF401A779A", "Show only declarations with written off guarantee D48"));
				result.AddPair("NO", Res.GetString("3C06B4F3-A6D2-4A62-BD62-4C6237DA16D3", "Show only declarations with consuming D48 guarantee"));
				result.AddPair("BOTH", Res.GetString("D4B54E85-9A61-411D-8EA6-4B82D1A21BD9", "Show declarations with consuming and written off guarantee"));

				result.Sort();
				return result;
			});
		}
	}
}
