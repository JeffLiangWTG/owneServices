using CargoWise.Types;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business.MessageWrappers;

public class ImportH1CommonImporterWrapper : PartyNameWrapper, IH1PartyProviderWithAddress
{
	public static new ImportH1CommonImporterWrapper New(JobDocAddress jobDocAddress) => jobDocAddress?.Address?.Header == null ? null : new ImportH1CommonImporterWrapper(jobDocAddress);

	ImportH1CommonImporterWrapper(JobDocAddress jobDocAddress) : base(jobDocAddress.Address.Header)
	{
		orgAddress = jobDocAddress.Address;
	}

	readonly OrgAddress orgAddress;

	protected override ZString IdCore
	{
		get
		{
			var (id, isNaturalPersonIndividual) = GetIdForNaturalPerson();
			return isNaturalPersonIndividual ? id : base.IdCore;
		}
	}

	protected override ZString NameCore => ZString.Empty;

	public IPartyAddressProvider Address => address ??= GetAddressForNaturalPerson(orgAddress, PartyAddressWrapper.New(orgAddress));
	PartyAddressWrapper address;
}
