using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentWrappers
{
	public class DocAppointedAgentPorts : DocumentWrapper
	{
		DocAppointedAgentPorts(OrgAppointedAgentPorts orgAppointedAgentPorts, BusinessObjectFactory factory)
			: base(orgAppointedAgentPorts, factory)
		{
		}

		public static DocAppointedAgentPorts New(OrgAppointedAgentPorts orgAppoitedAgentPorts, BusinessObjectFactory factory)
		{
			return (orgAppoitedAgentPorts != null) ? new DocAppointedAgentPorts(orgAppoitedAgentPorts, factory) : null;
		}

		public ZString AgentPortOrCountry
		{
			get { return OrgAppointedAgentPorts.O5_PortOrCountry; }
		}

		public ZString AgentDirection
		{
			get { return OrgAppointedAgentPorts.O5_AgentDirection; }
		}

		public ZString AirAgentStatus
		{
			get { return OrgAppointedAgentPorts.O5_AirAgentStatus; }
		}

		public ZString SeaAgentStatus
		{
			get { return OrgAppointedAgentPorts.O5_SeaAgentStatus; }
		}

		public ZString RoadAgentStatus
		{
			get { return OrgAppointedAgentPorts.O5_RoadAgentStatus; }
		}

		public ZString RailAgentStatus
		{
			get { return OrgAppointedAgentPorts.O5_RailAgentStatus; }
		}

		public ZString CarrierOrForwarderType
		{
			get { return OrgAppointedAgentPorts.O5_SeaAirCarrierOrForwarderType; }
		}

		public DocAddress CarrierOrForwarderAddress
		{
			get { return docAddress ?? (docAddress = DocAddress.New(Factory.Load<OrgAddress>(OrgAppointedAgentPorts.O5_OA_AgentOfficeAddress), Factory)); }
		}
		DocAddress docAddress;

		#region Implementation

		public override string ToString()
		{
			return OrgAppointedAgentPorts.O5_SeaAirCarrierOrForwarderType;
		}

		OrgAppointedAgentPorts OrgAppointedAgentPorts
		{
			get { return (OrgAppointedAgentPorts)WrappedObject; }
		}

		#endregion
	}
}
