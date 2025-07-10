using System;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.GUI;
using Enterprise.ZArchitecture.GUI;
using NctsHeader = Enterprise.Customs.CH.NCTS.Business.NctsHeader;

namespace Enterprise.Customs.CH.NCTS.GUI.Testing;

sealed class ArrivalMessagingMenuProviderTest : MessagingMenuProviderTest
{
	protected override ZString NctsHeaderMovementType => NctsMovementType.Codes.Arrival;
	protected override ZString MessageTypeForSending => ZString.Empty;
	protected override Type ExpectedMessageSendingFormType => typeof(ArrivalMessageSendingForm);
	protected override ZTemplateForm GetNctsMovementForm(NctsHeader nctsHeader) => new Phase5ArrivalMovementForm(nctsHeader);
	protected override bool ProvidesSendActivationMenuEntry => false;
}
