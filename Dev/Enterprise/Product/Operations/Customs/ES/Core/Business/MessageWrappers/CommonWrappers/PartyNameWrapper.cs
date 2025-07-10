using CargoWise.Types;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business.MessageWrappers
{
	public class PartyNameWrapper : PartyIdWrapper, IPartyNameProvider
	{
		public new static PartyNameWrapper New(OrgHeader orgHeader) => orgHeader == null ? null : new PartyNameWrapper(orgHeader);

		public new static PartyNameWrapper New(OrgAddress address) => PartyNameWrapper.New(address?.Header);

		public new static PartyNameWrapper New(JobDocAddress jobDocAddress) => PartyNameWrapper.New(jobDocAddress?.Address?.Header);

		protected PartyNameWrapper(OrgHeader orgH) : base(orgH)
		{
		}

		public ZString Name => NameCore;
		protected virtual ZString NameCore => orgHeader.OH_FullName;
	}
}
