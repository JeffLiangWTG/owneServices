using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.AE.Business
{
	public class RefContainerMapProvider : IRefContainerMapProvider
	{
		public UsageRequirement IsUsageNeeded => UsageRequirement.NotRequire;

		public ICodeDescriptionPairList GetUsageList(BusinessObjectFactory factory) => new CodeDescriptionPairList();

		public ICodeDescriptionPairList GetCodeList(BusinessObjectFactory factory, ZString usage) => factory.GetCachedValue<AEContainerTypeCodeList>();

		public ZString GetDefaultCustomsCode(BusinessObjectFactory factory, ZString containerType) => ZString.Empty;
	}
}
