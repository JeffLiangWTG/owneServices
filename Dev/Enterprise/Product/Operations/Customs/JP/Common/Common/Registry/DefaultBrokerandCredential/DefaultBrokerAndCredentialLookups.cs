using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.JP.Common
{
	public class DefaultBrokerAndCredentialLookups : ZLookups
	{
		public DefaultBrokerAndCredentialLookups(DefaultBrokerAndCredential parent)
			 : base(parent)
		{
		}

		protected new DefaultBrokerAndCredential Parent => (DefaultBrokerAndCredential)base.Parent;

		protected override BusinessObjectFactory Factory => Parent.Factory;

		public GlbStaffCollection DefaultBrokerCodeList => new GlbStaffCollection(Factory);

		public CodeDescriptionPairList CredentialSEAList => GetCredentialCodeList(UserCodeSpecificTransportModeList.Codes.SEA);

		public CodeDescriptionPairList CredentialAIRList => GetCredentialCodeList(UserCodeSpecificTransportModeList.Codes.AIR);

		CodeDescriptionPairList GetCredentialCodeList(string transportMode)
		{
			var pairList = new CodeDescriptionPairList();
			var passwords = PasswordCollection?.Where(x => x.GP_PasswordType == JPPasswordType.Codes.CUS && (x.GP_Transport == UserCodeSpecificTransportModeList.Codes.BTH || x.GP_Transport == transportMode));
			var userCodeSpecificTransportModeList = new UserCodeSpecificTransportModeList();
			passwords?.ForEach(x => pairList.AddPair(x.GP_MailBoxID + x.GP_UserID, userCodeSpecificTransportModeList[x.GP_Transport].Description));

			return pairList;
		}

		GlbExternalPasswordCUSCollection PasswordCollection => GlbStaffWrapper.Get(Parent.DefaultBroker)?.PasswordCollection;
	}
}
