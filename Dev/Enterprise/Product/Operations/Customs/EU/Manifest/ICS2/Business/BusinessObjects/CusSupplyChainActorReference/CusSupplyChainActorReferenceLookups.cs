using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public class CusSupplyChainActorReferenceLookups : EU.Business.Declaration.CusSupplyChainActorReferenceLookups
	{
		public CusSupplyChainActorReferenceLookups(CusSupplyChainActorReference parent)
			: base(parent)
		{
		}

		public override CodeDescriptionPairList CodeList
		{
			get
			{
				var today = ZDateTime.Today;
				return Factory.GetCachedValue("EU.ICS2.CusSupplyChainActorReferenceLookUps.CodeList" + today, () =>
				{
					var result = new CodeDescriptionPairList();
					var codeList = ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, UniversalReferenceConstants.RefCusCodeListType.Code.Code_IC2SA, today);

					if (!codeList.IsLoaded)
					{
						codeList.Load();
						codeList.Sort(RefCusCodeListSchema.Constants.ZZD_Code);
					}

					foreach (ZZRefCusCodeListCombined item in codeList)
					{
						result.AddPairIfNotExist(item.ZZD_Code, item.ZZD_Description);
					}

					return result;
				});
			}
		}
	}
}
