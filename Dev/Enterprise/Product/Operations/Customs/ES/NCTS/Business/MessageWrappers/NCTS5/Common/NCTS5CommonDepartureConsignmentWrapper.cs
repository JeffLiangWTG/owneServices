using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.ES.NCTS.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.NCTS.Business.MessageWrappers
{
	public class NCTS5CommonDepartureConsignmentWrapper : NCTS5CommonConsignmentWrapper, INCTSCommonDepartureConsignment
	{
		public NCTS5CommonDepartureConsignmentWrapper(NctsHeader header) : base(header)
		{
		}

		public ZBool ContainerIndicator => nctsHeader.Bills.Any(b => b.GoodsItems.Any(i => i.Packages.Cast<NctsPackage>().Any(x => x.ContainersPivotsForBindingOnly.Cast<NonPersistentContainerPivotPhase5>().Any(y => y.ContainerSelected && y.ContainerMode == Core.Constants.ContainerModes.Containerised))));

		public ZString InlandModeOfTransport => departureMovement.InlandTransportModeAtDeparture;

		public ZString ModeOfTransportAtTheBorder => departureMovement.BM_ExportTransportMode;
	}
}
