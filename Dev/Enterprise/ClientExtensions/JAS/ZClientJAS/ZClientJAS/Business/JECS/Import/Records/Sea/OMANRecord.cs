
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.JAS.Business.JXC.Import
{
	public class OMANRecord : JXCRecord
	{
		public OMANRecord(ZString lineType, ZString lineContent)
			: base(lineType, lineContent)
		{
		}

		public JASForwardingConsol LoadConsol(BusinessObjectFactory factory)
		{
			JASForwardingConsol result = null;

			if (factory != null)
			{
				result = new ConsolLocator<JASForwardingConsol>().Find(factory, "", ManifestNo, Core.Constants.TransportModes.Sea, VesselName, VoyageNumber, ETD);
			}

			return result;
		}

		Transport GetTransportToUpdate(CommonConsol consol)
		{
			Transport result = consol.Transports[0];
			foreach (Transport transport in consol.Transports)
			{
				if (transport.JW_TransportMode == consol.JK_TransportMode &&
					(transport.JW_TransportType == Core.Constants.TransportPlanningType.Flight1 ||
					transport.JW_TransportType == Core.Constants.TransportPlanningType.MainVessel))
				{
					result = transport;
					break;
				}
			}
			return result;
		}

		public void UpdateConsol(JASForwardingConsol consol)
		{
			if (consol != null)
			{
				using (new DataImportFlagChanger(consol))
				{
					consol.JK_AgentsReference = ManifestNo.Left(consol.JK_AgentsReferenceInfo.MaxLength);
					RefVessel vessel = RefVessel.LookupVesselByCode(VesselName, consol.Factory);

					Transport transport = GetTransportToUpdate(consol);
					if (vessel != null)
					{
						transport.JW_Vessel = vessel.RV_Code.Left(transport.JW_VesselInfo.MaxLength);
					}

					transport.JW_VoyageFlight = VoyageNumber.Left(transport.JW_VoyageFlightInfo.MaxLength);
					transport.JW_RL_NKLoadPort = PortOfLoading.Left(transport.JW_RL_NKLoadPortInfo.MaxLength).ToUpper();
					transport.JW_RL_NKDiscPort = PortOfDischarge.Left(transport.JW_RL_NKDiscPortInfo.MaxLength).ToUpper();
					transport.JW_ETD = ETD;
					transport.JW_ETA = ETA;
				}
			}
		}

		#region Implementation

		ZString ManifestNo
		{
			get { return Fields.GetFieldValue(JXCConstants.OMANFieldPositions.ManifestNo); }
		}

		ZString VesselName
		{
			get { return Fields.GetFieldValue(JXCConstants.OMANFieldPositions.VesselName); }
		}

		ZString VoyageNumber
		{
			get { return Fields.GetFieldValue(JXCConstants.OMANFieldPositions.VoyageNo); }
		}

		ZString PortOfLoading
		{
			get
			{
				ZString result = Fields.GetFieldValue(JXCConstants.OMANFieldPositions.PortOfLoadingCode);
				if (result.IsEmpty)
				{
					result = Fields.GetFieldValue(JXCConstants.OMANFieldPositions.PortOfLoadingCodeForCustoms);
				}
				return result;
			}
		}

		ZString PortOfDischarge
		{
			get
			{
				ZString result = Fields.GetFieldValue(JXCConstants.OMANFieldPositions.PortOfDischargeCode);
				if (result.IsEmpty)
				{
					result = Fields.GetFieldValue(JXCConstants.OMANFieldPositions.PortOfDichargeCodeForCustoms);
				}
				return result;
			}
		}

		ZDateTime ETD
		{
			get { return Fields.GetDateTimeFieldValue(JXCConstants.OMANFieldPositions.EstimatedShippingDate); }
		}

		ZDateTime ETA
		{
			get { return Fields.GetDateTimeFieldValue(JXCConstants.OMANFieldPositions.EstimatedArrivalDate); }
		}

		#endregion
	}
}
