using Enterprise.Customs.BR.Business;
using Enterprise.Customs.Common.BR;
using Enterprise.MasterFiles.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.BR.Module
{
	public class LicenseFilterLookups : Customs.Module.JobDeclarationFilterLookups
	{
		public LicenseFilterLookups(Customs.Module.JobDeclarationFilterBusinessObject filterBizObj)
			: base(filterBizObj)
		{
		}

		public override CodeDescriptionPairList MessageTypeList
		{
			get
			{
				var declaration = Factory.GetNull<JobDeclaration>();
				declaration.FixedJobMessageType = BRJobMessageTypeList.Codes.ImportLicense;
				return declaration.Lookups.MessageTypeList;
			}
		}

		protected override CodeDescriptionPairList GetMainMessageStatusList() => Factory.GetCachedValue<BRMessageStatusList>();

		public override CodeDescriptionPairList EntryStatusList()
		{
			return Factory.GetCachedValue("BRLicenseFilterLookups_EntryStatusList",
				() => base.EntryStatusList().GetEntryStatusList(true));
		}

		public CodeDescriptionPairList ExchangeHedgeTypeList => Factory.GetCachedValue<ExchangeHedgeList>();

		public CodeDescriptionPairList ManufacturerIndicatorList => Factory.GetCachedValue<ManufacturerIndicatorList>();

		public CodeDescriptionPairList DrawbackModalityList => Factory.GetCachedValue<DrawbackModalityList>();

		public OrgHeaderCollection ManufacturerList => new ConsignorCollection(Factory);
	}
}
