using CargoWise.Common;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.EntityFramework;
using Enterprise.Customs.BE.Business.CusTempStorage;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;
using static Enterprise.MasterFiles.Business.OrgCusCode;

namespace Enterprise.Customs.BE.Business;

public class PNTSRepresentativeProvider : IPNTSRepresentative
{
	public PNTSRepresentativeProvider(TemporaryStorageHeader temporaryStorageHeader)
	{
		this.temporaryStorageHeader = Argument.NotNull(temporaryStorageHeader, nameof(temporaryStorageHeader));
		this.representative = Argument.NotNull(temporaryStorageHeader.Representative, nameof(temporaryStorageHeader.Representative));
	}
	readonly OrgAddress representative;
	readonly TemporaryStorageHeader temporaryStorageHeader;
	OrgAddress declarant => temporaryStorageHeader.Declarant;
	OrgHeader orgHeader => representative.Header;

	public int? Status => CachedValueHelper.GetValue(ref status, () => representative == declarant ? 3 : declarant != null ? 2 : null);
	CachedValue<int?> status;

	public ICommunication Communication => CachedValueHelper.GetValue(ref communication, () => !allocatedCUSContact?.OC_Email.IsEmpty ?? false ? new PNTSCommunicationProvider(allocatedCUSContact.OC_Email, CommunicationType.Codes.EM) : !allocatedCUSContact?.OC_Phone.IsEmpty ?? false ? new PNTSCommunicationProvider(allocatedCUSContact.OC_Phone, CommunicationType.Codes.TE) : null);
	CachedValue<ICommunication> communication;

	public string Name => orgHeader.OH_FullName;

	public string IdentificationNumber => CachedValueHelper.GetValue(ref identificationNumber, () => orgHeader.GetConcatenatedSingleOrgCusCode(EuropeanUnionSharedCodeTypes.Eori));
	CachedValue<string> identificationNumber;

	OrgContact allocatedCUSContact => orgHeader.AllocatedContacts.GetAllocatedContact(OrgConstants.ContactAllocationType.CUS);
}
