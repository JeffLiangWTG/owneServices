using CargoWise.EntityFramework;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.BR.Business
{
	public class UpdateImportEntryStatusObjectLookups : ZLookups
	{
		public UpdateImportEntryStatusObjectLookups(UpdateImportEntryStatusObject parent)
			: base(parent)
		{
		}

		new UpdateImportEntryStatusObject Parent => base.Parent as UpdateImportEntryStatusObject;

		public CodeDescriptionPairList RiskChannelList => Factory.GetCachedValue<RiskChannelList>();

		public CodeDescriptionPairList EntryStatusList => Parent.Declaration.Lookups.EntryStatusList;
	}
}
