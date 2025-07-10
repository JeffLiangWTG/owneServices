using Enterprise.Customs.DE.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.DE.Module
{
	public class JobDeclarationFilterLookups : EU.Module.JobDeclarationFilterLookups
	{
		public JobDeclarationFilterLookups(EU.Module.JobDeclarationFilterBusinessObject filterBizObj) : base(filterBizObj)
		{
		}

		public override CodeDescriptionPairList DeclarationTypeList
		{
			get
			{
				return Factory.GetCachedValue("8C27171A-8F65-4E4B-96B9-FEF8AE6CB2E3", () =>
				{
					var result = new CodeDescriptionPairList(Factory.GetCachedValue<ExportDeclarationTypeProcedureList>());
					result.AddPairsIfNotExist(Factory.GetCachedValue<ImportDeclarationTypeList>().ToArray());
					result.Sort();
					return result;
				});
			}
		}
	}
}
