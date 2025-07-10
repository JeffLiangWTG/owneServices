using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Integration.Customs.CN;
using Enterprise.ZArchitecture.Core;
using RefCusCodeListTypesCodes = Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes;

namespace Enterprise.Customs.CN.Business
{
	public sealed class RefCusPackListProvider : MasterFiles.Business.RefCusPackListProvider, IRefCusPackListProvider
	{
		public override CodeDescriptionPairList GetCIPCustomsPackList(BusinessObjectFactory factory, ZString country)
		{
			return RefCusCodeListTypes.GetCachedList(factory, country, RefCusCodeListTypesCodes.CustomsUQ, ZDateTime.Today);
		}

		public override CodeDescriptionPairList GetDeclarationPackTypeList(BusinessObjectFactory factory)
		{
			return factory.GetNull<JobDeclaration>().Lookups.PackingUnitTypesList;
		}
	}
}
