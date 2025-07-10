using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.MX.Business;
using Enterprise.MasterFiles.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.MX.Manifest.Business
{
	public class RefContainerMapProvider : IRefContainerMapProvider
	{
		public UsageRequirement IsUsageNeeded => UsageRequirement.MayRequire;

		public ICodeDescriptionPairList GetUsageList(BusinessObjectFactory factory) => factory.GetCachedValue<MXContainerUsageList>();

		public ICodeDescriptionPairList GetCodeList(BusinessObjectFactory factory, ZString usage) => usage == MXContainerUsageList.Codes.CUS ? factory.GetCachedValue<MXCustomsContainerCodeList>() : factory.GetCachedValue<MXContainerCodeList>();

		public ZString GetDefaultCustomsCode(BusinessObjectFactory factory, ZString containerType) => ZString.Empty;
	}
}
