using System;
using CargoWise.Types;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class ExportCustomsManifestLinesWrapper : ICTOMessageLine
	{
		public ExportCustomsManifestLinesWrapper(ExportCustomsManifestLines line)
		{
			if (line == null)
			{
				throw new ArgumentNullException(nameof(line));
			}

			this.line = line;
		}
		readonly ExportCustomsManifestLines line;

		#region ICTOItemWrapper Members

		public ZString ExportDeclarationExemptionCode
		{
			get { return line.IsExemptLine ? line.EL_TypeOfCAN : ZString.Empty; }
		}

		public ZString CarrierPartyID
		{
			get { return ZString.Empty; }
		}

		public ZString CustomsAuthorityNumber
		{
			get { return line.IsCANLine ? line.EL_CAN : ZString.Empty; }
		}

		public ZString CustomsContingencyAuthorityNumber
		{
			get { return line.IsCCANLine ? line.EL_CAN : ZString.Empty; }
		}

		public ZString CountryOfDestination
		{
			get { return line.EL_RN_NKCountryOfDestination; }
		}

		public ZString GoodsOwnerPartyID
		{
			get { return line.EL_GoodsOwnerPartyID; }
		}

		public ZBool OffloadIndicator
		{
			get { return false; }
		}

		public ZString OwnerName
		{
			get { return line.EL_GoodsOwner; }
		}

		public ZDate ProposedDateOfDeparture
		{
			get { return line.Header.ED_DepartureDate.Date; }
		}

		public ZString GoodsDescription
		{
			get { return line.EL_GoodsDescription; }
		}

		public ZString AirWaybill
		{
			get { return line.EL_AirWayBill; }
		}

		public ZString VesselID
		{
			get { return ZString.Empty; }
		}

		public ZString VoyageNumber
		{
			get { return ZString.Empty; }
		}

		public ZString ContainerNumber
		{
			get { return ZString.Empty; }
		}

		public ZString NonContainerisedIdentifier
		{
			get { return ZString.Empty; }
		}

		public ZBool IsAir
		{
			get { return true; }
		}

		public ZBool IsSea
		{
			get { return false; }
		}

		#endregion

		public ZString CTOEstablishmentID
		{
			get { return line.Header.CTOAddress == null ? ZString.Empty : line.Header.CTOAddress.LocalControlledPremisesID; }
		}

		public EDIMessageCollection Messages
		{
			get { return line.Header.Messages; }
		}
	}
}
