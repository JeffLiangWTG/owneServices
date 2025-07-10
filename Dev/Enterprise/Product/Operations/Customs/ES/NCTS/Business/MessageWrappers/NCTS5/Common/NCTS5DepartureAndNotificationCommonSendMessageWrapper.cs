using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.Customs.ES.NCTS.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.NCTS.Business.MessageWrappers;

public class NCTS5DepartureAndNotificationCommonSendMessageWrapper : NCTS5CommonSendMessageWrapper, IDepartureAndNotificationNCTSCommonMessageDataProvider
{
	public NCTS5DepartureAndNotificationCommonSendMessageWrapper(NctsHeader header, ICertificateProvider certificateData) : base(header, certificateData)
	{
		departureMovement = Argument.NotNull(nctsHeader.MovementHeader, nameof(nctsHeader.MovementHeader));
	}
	protected readonly NctsDepartureMovementHeader departureMovement;

	public ZString CustomsOfficeOfDeparture => departureMovement.CustomsOffices.Where(x => x.CY_Code == EU.Business.OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture).FirstOrDefault()?.CY_Data ?? ZString.Empty;

	public ICommonRepresentativeWithContactPerson Representative => representative ?? (representative = nctsHeader?.Principal?.Address?.Header != departureMovement.Representative?.Address?.Header ? NCTS5CommonRepresentativeWrapper.New(departureMovement.Representative) : null);
	NCTS5CommonRepresentativeWrapper representative;
}
