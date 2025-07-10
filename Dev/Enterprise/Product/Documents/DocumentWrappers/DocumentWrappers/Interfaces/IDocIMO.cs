using CargoWise.Types;

namespace Enterprise.DocumentWrappers
{
	public interface IDocIMO
	{
		DocOrganisation IMOSender { get; }
		DocOrganisation IMOConsignee { get; }
		DocOrganisation IMOCarrier { get; }
		ZString IMOConsolNumber { get; }
		ZString IMOShippersRef { get; }
		ZString IMOForwardersRef { get; }
		ZString IMOVesselVoyage { get; }
		ZString IMOETD { get; }
		ZString IMOPortOfLoading { get; }
		ZString IMOPortOfDischarge { get; }
		ZString IMODestination { get; }
		ZString IMOHandlingInstructions { get; }
		ZString IMOContainerNum { get; }
		ZString IMOSealNum { get; }
		ZString IMOContainerType { get; }
		ZString IMOContainerTare { get; }
		ZString IMOTotalGrossMassAndTare { get; }
		DocIMOBodyCollection IMOShipments { get; }
	}
}
