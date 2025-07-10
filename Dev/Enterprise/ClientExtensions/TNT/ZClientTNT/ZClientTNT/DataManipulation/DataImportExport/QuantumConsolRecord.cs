using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.DataTransfer.Integration;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Client.TNT
{
	public class QuantumConsolRecord : FlightRecord
	{
		public QuantumConsolRecord(ZString line)
			: base(line)
		{
		}

		#region Field Property Override

		public override ZString MasterBill
		{
			get { return base.MasterBill.Trim(); }
		}

		public ZDateTime DepartureDate
		{
			get { return FlightDate; }
		}

		public override ZString PortOfLoading
		{
			get { return base.PortOfLoading.Trim(); }
		}

		public override ZString PortOfDischarge
		{
			get { return base.PortOfDischarge.Trim(); }
		}

		#endregion

		public ForwardingConsol CreateConsol(BusinessObjectFactory factory, INotifications notify)
		{
			TNTStringToBusinessObjectFieldConverter mapper = TNTStringToBusinessObjectFieldConverter.Instance;

			ForwardingConsol newConsol = (ForwardingConsol)factory.New(typeof(ForwardingConsol));

			((ISupportDataImporting)newConsol).IsImportingData = true;
			try
			{
				newConsol.JK_TransportMode = Constants.TransportModes.Air;

				Transport transport = newConsol.Transports[0];
				transport.JW_IsLinked = true;
				transport.JW_TransportMode = Core.Constants.TransportModes.Air;
				transport.JW_TransportType = Core.Constants.TransportPlanningType.Flight1;

				mapper.SetPropertyInfoValue(transport.JW_RL_NKLoadPortInfo, PortOfLoading, ForeignKeyType.PortNK, notify);
				mapper.SetPropertyInfoValue(transport.JW_RL_NKDiscPortInfo, PortOfDischarge, ForeignKeyType.PortNK, notify);
				transport.JW_ETD = DepartureDate;

				mapper.SetPropertyInfoValue(transport.JW_VoyageFlightInfo, FlightNumber, ForeignKeyType.None, notify);
				newConsol.JK_IsNeutralMaster = false;
				mapper.SetPropertyInfoValue(newConsol.JK_MasterBillNumInfo, MasterBill, ForeignKeyType.None, notify);
			}
			finally
			{
				((ISupportDataImporting)newConsol).IsImportingData = false;
			}

			return newConsol;
		}

		#region Implementation
		#endregion

	}
}
