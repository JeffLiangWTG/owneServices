using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.Business
{
	public class RefCusPackListProvider : Enterprise.MasterFiles.Business.RefCusPackListProvider, Enterprise.MasterFiles.Integration.Customs.EU.IRefCusPackListProvider
	{
		public override CodeDescriptionPairList GetDeclarationPackTypeList(BusinessObjectFactory factory)
		{
			return factory.GetNull<JobDeclaration>().Lookups.PackingUnitTypesList;
		}

		public override CodeDescriptionPairList GetCustomsPackList(BusinessObjectFactory factory, ZString type, ZString country)
		{
			return Universal.AsycudaUniversalReference.RefCusCodeListTypes.GetCachedList(factory, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PackageTypes);
		}
	}
}
