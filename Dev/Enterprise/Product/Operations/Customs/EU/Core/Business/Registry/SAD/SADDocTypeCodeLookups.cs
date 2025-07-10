using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.Business.Registry
{
	public static class SADDocTypeCodeLookups
	{
		public static CodeDescriptionPairList GetCachedAllOrSupplyChainLogisticsDocTypeList(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("GetCachedAllOrSupplyChainLogisticsDocTypeList" + Res.CurrentLanguage, delegate
			{
				var docTypeQuery = new ZQuery(RefDocTypeSchema.RT_ReferenceType, Core.Constants.ReferenceTypes.All);
				docTypeQuery.AddToFilter(JoinCondition.Or, RefDocTypeSchema.RT_ReferenceType, SQLComparisonOperator.Equal, Core.Constants.ReferenceTypes.SupplyChainLogistics);
				var result = new CodeDescriptionPairList();
				foreach (var docType in factory.Load<RefDocType>(docTypeQuery).OrderBy(p => p.RT_DocType).ThenByDescending(p => p.RT_SystemCreateTimeUtc))
				{
					result.AddPair(docType.RT_DocType, docType.RT_DescMultilingual);
				}

				return result;
			});
		}
	}
}
