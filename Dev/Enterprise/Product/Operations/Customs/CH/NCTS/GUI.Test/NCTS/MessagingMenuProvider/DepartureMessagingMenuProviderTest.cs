using System;
using CargoWise.Types;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.GUI;
using Enterprise.ZArchitecture.GUI;
using NctsHeader = Enterprise.Customs.CH.NCTS.Business.NctsHeader;

namespace Enterprise.Customs.CH.NCTS.GUI.Testing;

sealed class DepartureMessagingMenuProviderTest : MessagingMenuProviderTest
{
	protected override ZString NctsHeaderMovementType => NctsMovementType.Codes.Departure;
	protected override ZString MessageTypeForSending => PassarMessageTypeList.Codes.NT015;
	protected override Type ExpectedMessageSendingFormType => typeof(DepartureMessageSendingForm);
	protected override ZTemplateForm GetNctsMovementForm(NctsHeader nctsHeader) => new Phase5DepartureMovementForm(nctsHeader);
}
