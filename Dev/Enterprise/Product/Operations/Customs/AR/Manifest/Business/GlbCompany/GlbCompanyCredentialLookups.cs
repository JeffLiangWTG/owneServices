using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.AR.Manifest.Business
{
	public class GlbCompanyCredentialLookups : GlbExternalPasswordLookups
	{
		public GlbCompanyCredentialLookups(AutoGlbExternalPassword parent) : base(parent)
		{
		}
		public override CodeDescriptionPairList PasswordStatusList
			=> Factory.GetCachedValue("Enterprise.Customs.AR.Manifest.Business.GlbCompanyCredentialLookups.PasswordStatusList", delegate
			{
				var codeDescriptionPairList = new CodeDescriptionPairList();
				codeDescriptionPairList.AddRange(base.PasswordStatusList);
				codeDescriptionPairList.AddPair(GlbARExternalPassword.PasswordAwaitingCode, GlbARExternalPassword.PasswordAwaiting);
				codeDescriptionPairList.AddPair(GlbARExternalPassword.PasswordRegisteredCode, GlbARExternalPassword.PasswordRegistered);
				return codeDescriptionPairList;
			});
	}
}
