using CargoWise.Customs.CH.MessageContracts.Passar.Outgoing;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CH.NCTS.Business;

public class TraderDataProvider : ITrader
{
	public static TraderDataProvider New(NctsHeader nctsHeader) => nctsHeader == null ? null : new TraderDataProvider(nctsHeader);

	TraderDataProvider(NctsHeader nctsHeader)
	{
		this.nctsHeader = nctsHeader;
	}
	readonly NctsHeader nctsHeader;

	NctsCommonMovementHeader CommonMovementHeader => (NctsCommonMovementHeader)nctsHeader.MovementHeader ?? nctsHeader.ArrivalMovementHeader;

	public string CommunicationLanguage => (nctsHeader.BH_CommunicationLanguage.ReturnNullIfEmpty())?.ToLowerInvariant();

	public string IdentificationNumber => identificationNumber ?? (identificationNumber =  GetIdentificationNumber());
	string identificationNumber;

	public IContactPerson ContactPerson => contactPerson ?? (contactPerson = StaffContactPersonDataProvider.New(GlbStaff.CurrentUser));
	IContactPerson contactPerson;

	string GetIdentificationNumber()
	{
		var idNumber = string.Empty;

		if (CommonMovementHeader.IsArrivalMovementHeader)
		{
			idNumber = ((NctsArrivalMovementHeader)CommonMovementHeader)?.Header?.DestinationTrader?.Organisation.GetCHCustomsRegNo(OrgCusCode.SwissCodeTypes.BID);
		}
		else
		{
			idNumber = CommonMovementHeader?.Representative?.Organisation?.GetCHCustomsRegNo(OrgCusCode.SwissCodeTypes.BID);
		}

		return idNumber;
	}
}
