using Enterprise.MasterFiles.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.BR.Business
{
	public class GlbExternalPasswordLookups_BRS : GlbExternalPasswordLookups
	{
		public GlbExternalPasswordLookups_BRS(GlbExternalPassword_BRS parent)
			: base(parent)
		{
		}

		protected new GlbExternalPassword_BRS Parent => (GlbExternalPassword_BRS)base.Parent;

		public CodeDescriptionPairList EventIdCodeDescriptionPairList => Factory.GetCachedValue<EventIdList>();
	}
}
