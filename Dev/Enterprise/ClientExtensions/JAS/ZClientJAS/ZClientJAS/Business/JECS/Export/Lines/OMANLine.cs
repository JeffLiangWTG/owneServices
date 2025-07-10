
using CargoWise.Types;
using Enterprise.Freight.Business;

namespace Enterprise.Client.JAS.Business.JXC.Export
{
	public class OMANLine : MessageLine
	{
		public OMANLine(JASForwardingConsol consol)
		{
			this.Consol = consol;
		}

		protected override int FieldCount
		{
			get { return JXCConstants.OMANFieldCount; }
		}

		protected override ZString LineType
		{
			get { return JXCConstants.LineTypes.OMAN; }
		}

		protected override void SetFieldValues(JXCFlatFileDataRow dataRow)
		{
			dataRow.SetField(JXCConstants.OMANFieldPositions.TypeOfRecord, "N");
			dataRow.SetField(JXCConstants.OMANFieldPositions.ManifestNo, Consol.JK_UniqueConsignRef, JXCConstants.OMANFieldBoundaries.ManifestNoMaxLength);
			dataRow.SetField(JXCConstants.OMANFieldPositions.VesselName, VesselName);
			dataRow.SetField(JXCConstants.OMANFieldPositions.VoyageNo, VoyageNo);
			dataRow.SetField(JXCConstants.OMANFieldPositions.PortOfLoadingName, PortOfLoadingName);
			dataRow.SetField(JXCConstants.OMANFieldPositions.PortOfDischargeName, PortOfDischargeName);
			dataRow.SetField(JXCConstants.OMANFieldPositions.PortOfLoadingCodeForCustoms, Consol.JK_RL_NKLoadPort);
			dataRow.SetField(JXCConstants.OMANFieldPositions.PortOfLoadingCode, Consol.JK_RL_NKLoadPort);
			dataRow.SetField(JXCConstants.OMANFieldPositions.PortOfDichargeCodeForCustoms, Consol.JK_RL_NKDischargePort);
			dataRow.SetField(JXCConstants.OMANFieldPositions.PortOfDischargeCode, Consol.JK_RL_NKDischargePort);
			dataRow.SetField(JXCConstants.OMANFieldPositions.ManifestPrintDate, ZDateTime.Now);
			dataRow.SetField(JXCConstants.OMANFieldPositions.EstimatedShippingDate, Consol.Transports.DepartureTransport.JW_ETD);
			dataRow.SetField(JXCConstants.OMANFieldPositions.EstimatedArrivalDate, Consol.Transports.ArrivalTransport.JW_ETA);
		}

		#region Implementation

		ZString PortOfLoadingName
		{
			get { return (Consol.LoadPort != null) ? Consol.LoadPort.RL_PortName : ZString.Empty; }
		}

		ZString PortOfDischargeName
		{
			get { return (Consol.DischargePort != null) ? Consol.DischargePort.RL_PortName : ZString.Empty; }
		}

		ZString VesselName
		{
			get { return (TransportToUse != null) ? TransportToUse.JW_Vessel : ZString.Empty; }
		}

		ZString VoyageNo
		{
			get { return (TransportToUse != null) ? TransportToUse.JW_VoyageFlight : ZString.Empty; }
		}

		Transport TransportToUse
		{
			get
			{
				return Consol.Transports.FirstTransportWithTransportMode(Core.Constants.TransportModes.Sea) ?? Consol.Transports.MostInterestingTransport;
			}
		}

		readonly JASForwardingConsol Consol;

		#endregion
	}
}
