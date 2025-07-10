using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CO.Manifest.Business
{
	public class RefContainerMapProvider : IRefContainerMapProvider
	{
		public UsageRequirement IsUsageNeeded => UsageRequirement.NotRequire;

		public ICodeDescriptionPairList GetUsageList(BusinessObjectFactory factory) => new CodeDescriptionPairList();

		public ICodeDescriptionPairList GetCodeList(BusinessObjectFactory factory, ZString usage)
		{
			return factory.GetCachedValue("Enterprise.Customs.CO.Manifest.Business.RefContainerMapProvider.CodeList", () =>
			{
				return new COContainerCodeList();
			});
		}

		public ZString GetDefaultCustomsCode(BusinessObjectFactory factory, ZString containerType)
		{
			return containerType.IsEmpty ? ZString.Empty : ZZRefCusMapCombined.MapCW1CodeToCustomsCode(factory, Core.Constants.CountryCodes.Colombia, RefCusMapTypeList.Codes.CTYPE, containerType, ZDateTime.Today);
		}
	}
}
