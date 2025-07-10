using CargoWise.Types;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business.MessageWrappers
{
	public class EXSPartyProviderWrapper : PartyWrapper, IEXSPartyProvider
	{
		public static EXSPartyProviderWrapper New(OrgAddress address, ZString email) => address?.Header == null ? null : new EXSPartyProviderWrapper(address, email);

		public static EXSPartyProviderWrapper New(OrgHeader orgHeader, OrgAddress orgAddress) => orgAddress == null
																											? orgHeader?.MainAddress == null ? null : new EXSPartyProviderWrapper(orgHeader?.MainAddress)
																											: new EXSPartyProviderWrapper(orgAddress);

		EXSPartyProviderWrapper(OrgAddress address)
			: base(address)
		{
			EmailAddress = address.OA_Email;
		}

		EXSPartyProviderWrapper(OrgAddress address, ZString email)
			: base(address)
		{
			EmailAddress = email;
		}

		public ZString EmailAddress { get; }
	}
}
