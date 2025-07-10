using CargoWise.Types;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business.MessageWrappers
{
	public class EXSContactPersonWithIdWrapper : PartyNameWrapper, IEXSContactPersonWithId
	{
		public new static EXSContactPersonWithIdWrapper New(OrgHeader orgHeader)
		{
			return orgHeader != null ? new EXSContactPersonWithIdWrapper(orgHeader.MainAddress) : null;
		}

		protected EXSContactPersonWithIdWrapper(OrgAddress orgA) : base(orgA.Header)
		{
			orgAddress = orgA;
		}
		readonly OrgAddress orgAddress;

		public ZString Phone => PhoneCore;

		protected virtual ZString PhoneCore => orgAddress.OA_Phone;

		public ZString EmailAddress => EmailAddressCore;

		protected virtual ZString EmailAddressCore => orgAddress.OA_Email;
	}
}
