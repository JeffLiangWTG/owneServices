using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;
using static Enterprise.MasterFiles.Business.OrgCusCode;

namespace Enterprise.Customs.BE.Business;

public class PNTSDeclarantProvider : IPNTSPartyWithNameAndCommunications
{
	public PNTSDeclarantProvider(OrgAddress address)
	{
		this.declarant = Argument.NotNull(address, nameof(address));
	}

	readonly OrgAddress declarant;
	OrgHeader orgHeader => declarant.Header;

	public string IdentificationNumber => CachedValueHelper.GetValue(ref identificationNumber, () => orgHeader.GetConcatenatedSingleOrgCusCode(EuropeanUnionSharedCodeTypes.Eori));
	CachedValue<string> identificationNumber;

	public string Name => orgHeader.OH_FullName;

	public IReadOnlyCollection<ICommunication> Communications => communications ??= MessageProviderHelper.GetCommunicationsFromOrgHeader(orgHeader); 
	IReadOnlyCollection<ICommunication> communications;
}
