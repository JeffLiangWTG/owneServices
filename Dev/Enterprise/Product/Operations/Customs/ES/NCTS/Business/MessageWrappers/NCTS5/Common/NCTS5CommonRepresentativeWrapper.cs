using CargoWise.Types;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.NCTS.Business.MessageWrappers;

public class NCTS5CommonRepresentativeWrapper : PartyIdWrapper, ICommonRepresentativeWithContactPerson
{
	public new static NCTS5CommonRepresentativeWrapper New(JobDocAddress jobDocAddress) => jobDocAddress?.Address?.Header == null ? null : new NCTS5CommonRepresentativeWrapper(jobDocAddress);

	NCTS5CommonRepresentativeWrapper(JobDocAddress docAd) : base(docAd?.Address?.Header)
	{
		docAddress = docAd;
	}
	readonly JobDocAddress docAddress;

	const string StatusCode = "2";

	public ZString Status => StatusCode;

	public IPartyContactProvider ContactPerson => contactPerson ?? (contactPerson = PartyContactWrapper.New(docAddress));
	PartyContactWrapper contactPerson;
}
