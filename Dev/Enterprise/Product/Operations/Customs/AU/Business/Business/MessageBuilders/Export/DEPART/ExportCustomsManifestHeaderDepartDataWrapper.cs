using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class ExportCustomsManifestHeaderDepartDataWrapper : IDepartDataWrapper
	{
		public ExportCustomsManifestHeaderDepartDataWrapper(ExportCustomsManifestHeader header)
		{
			this.header = header;
		}

		public bool IsAir
		{
			get { return header.IsAir; }
		}

		public bool IsSea
		{
			get { return header.IsSea; }
		}

		public ZString FlightNumber
		{
			get { return header.ED_FlightNumber.KeepChars("0123456789"); }
		}

		public ZString AirlineCode
		{
			get { return header.ED_FlightNumber.ToUpper().KeepChars("ABCDEFGHIJKLMNOPQRSTUVWXYZ"); }
		}

		public ZString VoyageNumber
		{
			get { return header.ED_VoyageNumber; }
		}

		public ZString VesselID
		{
			get { return header.ED_LloydsIMO; }
		}

		public ZString CTOEstablishmentID
		{
			get { return header.CTOAddress != null ? header.CTOAddress.LocalControlledPremisesID : ZString.Empty; }
		}

		public ZDateTime DepartureDateTime
		{
			get { return header.ED_DepartureDate; }
		}

		public ZString PortOfDestination
		{
			get { return header.ED_RL_NKPortOfDestination; }
		}

		public ZString CarrierPartyID
		{
			get { return header.CarrierPartyID; }
		}

		#region Implementation

		protected ExportCustomsManifestHeader header;

		#endregion
	}
}
