using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.Customs;
using Enterprise.MasterFiles.Integration.Customs.CA;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public sealed class RefCusPackListProvider : MasterFiles.Business.RefCusPackListProvider, IRefCusPackListProvider
	{
		public override CodeDescriptionPairList GetCIPCustomsPackList(BusinessObjectFactory factory, ZString country)
		{
			return factory.GetCachedValue<CustomsUnitOfMeasureList>();
		}

		public override CodeDescriptionPairList GetCommercialPackList(BusinessObjectFactory factory, ZString type)
		{
			return factory.GetCachedValue("CACommercialPackList" + type, () =>
			{
				var baselist = base.GetCommercialPackList(factory, type);
				if (RPTypeList.Codes.CommercialInvoice == type)
				{
					var list = factory.GetCachedValue<IIDUnitOfCountCodeList>();
					baselist.AddRangeOverwriteIfExists(list);
				}
				baselist.Sort();
				return baselist;
			});
		}

		public override CodeDescriptionPairList GetDeclarationPackTypeList(BusinessObjectFactory factory)
		{
			var result = new CodeDescriptionPairList();

			result.AddRange(factory.GetNull<Package>().UNPackTypeList);

			result.AddRange(factory.GetCachedValue<ACROSSPackageTypes>());

			return result;
		}
	}
}
