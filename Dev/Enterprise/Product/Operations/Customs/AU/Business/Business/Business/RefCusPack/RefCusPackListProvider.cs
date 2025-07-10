using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business.Customs;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class RefCusPackListProvider : MasterFiles.Business.RefCusPackListProvider, IRefCusPackListProvider
	{
		public override CodeDescriptionPairList GetPackConversionTypeList(BusinessObjectFactory factory)
		{
			{
				return factory.GetCachedValue("AU.RefCusPackListProvider.GetPackConversionTypeList", () =>
				{
					return AUCustomsDataRegistry.Instance.EnableNewPackTypeConversions.Value
						? new EnableNewPackTypeConversionRPTypeList()
						: new RPTypeList();
				});
			}
		}
	}
}
