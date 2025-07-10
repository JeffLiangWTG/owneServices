using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.Customs.ES.NCTS.Messaging.MessageBuilders;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.NCTS.Business.MessageWrappers
{
	public class NCTS5CommonCarrierWrapper : PartyIdWrapper, INCTSCommonCarrier
	{
		public new static NCTS5CommonCarrierWrapper New(JobDocAddress jobDocAddress) => jobDocAddress?.Address?.Header == null ? null : new NCTS5CommonCarrierWrapper(jobDocAddress?.Address);

		NCTS5CommonCarrierWrapper(OrgAddress orgA) : base(orgA?.Header)
		{
			orgAddress = orgA;
		}
		readonly OrgAddress orgAddress;

		public IPartyContactProvider ContactPerson => contactPerson ?? (contactPerson = PartyContactWrapper.New(orgAddress));
		PartyContactWrapper contactPerson;
	}
}
