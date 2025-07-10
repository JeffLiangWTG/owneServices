using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.ContainerYard.Business;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1501: Avoid excessive inheritance")]
	public class FreightWrapperFromGateTransport : FreightWrapper
	{
		FreightWrapperFromGateTransport(GateTransport gateTransportBO, GateTransportCYDetail gateTransportCYDetailBO, GateTransportCFSDetail gateTransportCFSDetailBO, BusinessObjectFactory factory)
			: base(gateTransportBO, factory)
		{
			this.gateTransportBO = gateTransportBO;
			this.gateTransportCYDetailBO = gateTransportCYDetailBO;
			this.gateTransportCFSDetailBO = gateTransportCFSDetailBO;
		}

		public static FreightWrapperFromGateTransport New(GateTransport gateTransportBO, GateTransportCYDetail gateTransportCYDetailBO, BusinessObjectFactory factory)
		{
			return new FreightWrapperFromGateTransport(
				gateTransportBO: gateTransportBO ?? factory.GetNull<GateTransport>(),
				gateTransportCYDetailBO: gateTransportCYDetailBO ?? factory.GetNull<GateTransportCYDetail>(),
				gateTransportCFSDetailBO: factory.GetNull<GateTransportCFSDetail>(),
				factory: factory);
		}

		public static FreightWrapperFromGateTransport New(GateTransport gateTransportBO, GateTransportCFSDetail gateTransportCFSDetailBO, BusinessObjectFactory factory)
		{
			return new FreightWrapperFromGateTransport(
				gateTransportBO: gateTransportBO ?? factory.GetNull<GateTransport>(),
				gateTransportCYDetailBO: factory.GetNull<GateTransportCYDetail>(),
				gateTransportCFSDetailBO: gateTransportCFSDetailBO ?? factory.GetNull<GateTransportCFSDetail>(),
				factory: factory);
		}

		readonly GateTransport gateTransportBO;
		readonly GateTransportCYDetail gateTransportCYDetailBO;
		readonly GateTransportCFSDetail gateTransportCFSDetailBO;

		protected override ZString GetJobNumber() => gateTransportBO.GTT_JobNumber;

		protected override ZString GetJobNumberHeading() => gateTransportBO.GTT_JobNumber;

		protected override GateTransportWrapper GetGateTransport()
		{
			if (gateTransportCYDetailBO != null && !gateTransportCYDetailBO.IsNull)
			{
				return GateTransportWrapper.NewWithCYDetail(gateTransportBO, Factory);
			}
			else
			{
				return GateTransportWrapper.NewWithCFSDetail(gateTransportBO, gateTransportCFSDetailBO, Factory);
			}
		}
	}
}
