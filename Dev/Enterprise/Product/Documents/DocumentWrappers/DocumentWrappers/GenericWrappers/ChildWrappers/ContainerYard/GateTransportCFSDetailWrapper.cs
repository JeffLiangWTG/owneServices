using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.Freight.ContainerYard.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	[DefaultField("JobNumber")]
	public class GateTransportCFSDetailWrapper : GenericWrapper
	{
		public GateTransportCFSDetailWrapper(GateTransportCFSDetail gateTransportCFSDetailBO, BusinessObjectFactory factory)
			: base(gateTransportCFSDetailBO, factory)
		{
			this.gateTransportCFSDetailBO = gateTransportCFSDetailBO ?? factory.GetNull<GateTransportCFSDetail>();
		}
		readonly GateTransportCFSDetail gateTransportCFSDetailBO;

		public FreightWrapperFromGateTransport GateTransport
		{
			get { return gateTransport ?? (gateTransport = FreightWrapperFromGateTransport.New(gateTransportCFSDetailBO.GateTransport, gateTransportCFSDetailBO, Factory)); }
		}
		FreightWrapperFromGateTransport gateTransport;

		public GateBookingDetailWrapper GateBookingDetail
		{
			get { return gateBookingDetail ?? (gateBookingDetail = new GateBookingDetailWrapper(gateTransportCFSDetailBO.GateBookingDetail, Factory)); }
		}
		GateBookingDetailWrapper gateBookingDetail;

		public OrganisationWrapper Owner
		{
			get { return owner ?? (owner = new OrganisationWrapper(OrganisationUsageType.Carrier, gateTransportCFSDetailBO.GateBookingDetail?.YardUnit?.UnitOwnerAddress, ContactType.All, Factory)); }
		}
		OrganisationWrapper owner;

		public ZString JobNumber => gateTransportCFSDetailBO.GTF_JobNumber;

		public ZString TrailerRegistration => gateTransportCFSDetailBO.GTF_TrailerRegistration;

		public ZBool IsPickup => gateTransportCFSDetailBO.GTF_IsPickup;

		public ZString Purpose => gateTransportCFSDetailBO.GTF_Purpose;
	}
}
