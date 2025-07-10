using CargoWise.EntityFramework;
using Enterprise.Customs.CH.NCTS.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.CH.NCTS.GUI.Testing;

[TestedType(typeof(TransportDepartureLayout))]
sealed class TransportDepartureLayoutTest : BaseTransportDepartureLayoutTest
{
	protected override ICommonLayoutBuilder CommonLayoutBuilder => new EU.NCTS.GUI.TransportDepartureLayoutBuilder<NctsDepartureMovementHeader>();

	protected override IPanelLayoutProvider CreateLayout() => new TransportDepartureLayout();

	protected override BusinessObject GetBusinessObject() => NctsHeader.MovementHeader;

	protected override void SetTransportMode(string transportMode) => NctsHeader.MovementHeader.BM_InlandTransportMode = transportMode;
}
