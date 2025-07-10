using CargoWise.EntityFramework;
using Enterprise.Customs.CH.NCTS.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.CH.NCTS.GUI.Testing;

[TestedType(typeof(ActivationTransportDepartureLayout))]
sealed class ActivationTransportDepartureLayoutTest : BaseTransportDepartureLayoutTest
{
	protected override ICommonLayoutBuilder CommonLayoutBuilder => new EU.NCTS.GUI.TransportDepartureLayoutBuilder<NctsHeaderDepartureMessageSendingObject>();

	protected override IPanelLayoutProvider CreateLayout() => new ActivationTransportDepartureLayout();

	NctsHeaderDepartureMessageSendingObject SendingObject => sendingObject ??= new NctsHeaderDepartureMessageSendingObject(NctsHeader);
	NctsHeaderDepartureMessageSendingObject sendingObject;

	protected override BusinessObject GetBusinessObject() => SendingObject;

	protected override void SetTransportMode(string transportMode) => SendingObject.InlandTransportModeAtDeparture = transportMode;
}
