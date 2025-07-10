using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.DocumentWrappers.Freight;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.Registry.Business;
using Res = DocumentWrappers.Res;

namespace Enterprise.DocumentWrappers
{
	public class DocTransport : DocBaseWrapper
	{
		public static DocTransport New(CommonConsol consol, Transport transport, BusinessObjectFactory factory)
		{
			return (transport == null) ? null : new DocTransport(consol, new TransportSource(transport), factory);
		}

		public static DocTransport New(CommonShipment shipment, Transport transport, BusinessObjectFactory factory)
		{
			return (transport == null) ? null : new DocTransport(shipment, new TransportSource(transport), factory);
		}

		public static DocTransport New(JobSailing sailing, BusinessObjectFactory factory)
		{
			return (sailing == null) ? null : new DocTransport(new SailingSource(sailing), factory);
		}

		public static DocTransport New(BaseJobDeclaration declaration, BusinessObjectFactory factory)
		{
			return (declaration == null) ? null : new DocTransport(new DeclarationSource(declaration), factory);
		}

		public static DocTransport New(Order order, OrderSource.Leg leg, BusinessObjectFactory factory)
		{
			return (order == null) ? null : new DocTransport(new OrderSource(order, leg), factory);
		}

		internal DocTransport(ITransportDetails transport, BusinessObjectFactory factoryToWrap)
			: base(transport, factoryToWrap)
		{
		}

		protected DocTransport(CommonShipment shipment, ITransportDetails transport, BusinessObjectFactory factoryToWrap)
			: this(transport, factoryToWrap)
		{
			docShipment = DocShipment.New(shipment, factoryToWrap);
		}

		protected DocTransport(CommonConsol consol, ITransportDetails transport, BusinessObjectFactory factoryToWrap)
			: this(transport, factoryToWrap)
		{
			docConsol = DocShipmentConsol.New(consol, factoryToWrap);
		}

		public DocShipmentConsol Consol
		{
			get { return Shipment == null ? docConsol : Shipment.Consol; }
		}
		readonly DocShipmentConsol docConsol;

		public DocShipment Shipment
		{
			get { return docShipment; }
		}
		readonly DocShipment docShipment;

		#region ZByte

		public ZByte LegOrder
		{
			get { return Details.LegOrder; }
		}

		#endregion

		#region ZString Fields

		public ZString ATDString
		{
			get { return FreightHelperClass.FormatETAETDDate(TransportModeIsSea, ATD); }
		}

		public ZString ATAString
		{
			get { return FreightHelperClass.FormatETAETDDate(TransportModeIsSea, ATA); }
		}

		public ZString ETDString
		{
			get { return Suppression.GetValue(FreightHelperClass.FormatETAETDDate(TransportModeIsSea, ETD), Details.SuppressingBizO, SuppressFields.ETD, ZString.Empty, DocumentContactType); }
		}

		public ZString ETAString
		{
			get { return Suppression.GetValue(FreightHelperClass.FormatETAETDDate(TransportModeIsSea, ETA), Details.SuppressingBizO, SuppressFields.ETA, ZString.Empty, DocumentContactType); }
		}

		public ZString PortOfDischargeCode
		{
			get { return Details.Discharge; }
		}

		public ZString TransportHeading
		{
			get
			{
				string transportHeading;

				switch (this.TransportMode)
				{
					case "":
						transportHeading = "";
						break;

					case Core.Constants.TransportModes.Air:
					case Core.Constants.TransportModes.AirSea:
						transportHeading = Res.GetString("a0136d7e-8ce1-4224-b87d-f7197cc2a1e4", "FLIGHT & DATE");
						break;

					case Core.Constants.TransportModes.Rail:
					case Core.Constants.TransportModes.Road:
						transportHeading = Res.GetString("9e109503-e100-47b6-8f3a-1e38ccb5255d", "JOURNEY NAME / JOURNEY NUMBER");
						break;

					default:
					case Core.Constants.TransportModes.Sea:
					case Core.Constants.TransportModes.SeaAir:
						transportHeading = Res.GetString("6e56bf61-1059-49a7-a046-502e31196411", "VESSEL / VOYAGE / IMO(Lloyds)");
						break;
				}

				return transportHeading;
			}
		}

		public ZString ConsolTransportInfo
		{
			get
			{
				ZString result = ZString.Empty;
				switch (TransportMode)
				{
					case Core.Constants.TransportModes.Air:
						result = FormatFlightDetails(VoyageFlight, PortOfDischargeCode, ETD);
						break;

					case Core.Constants.TransportModes.Rail:
					case Core.Constants.TransportModes.Road:
						result = FormatTransportDetails(VesselName, VoyageFlight);
						break;

					case Core.Constants.TransportModes.Sea:
						ZString lloydsNumber = Vessel == null ? ZString.Empty : Vessel.LloydsNumber;
						result = FormatTransportDetails(VesselName, VoyageFlight, lloydsNumber);
						break;
				}
				return result;
			}
		}

		public ZString PortOfLoadingCode
		{
			get { return Details.Load; }
		}

		public ZString TransportMode
		{
			get { return Details.TransportMode; }
		}

		public ZString TransportModeDescription
		{
			get
			{
				ZString mode = TransportMode;
				switch (mode)
				{
					case Core.Constants.TransportModes.Air:
						return Res.GetString("f6ab79db-3d42-484a-9dc7-2ba5eee48ebf", "Air");
					case Core.Constants.TransportModes.Rail:
						return Res.GetString("59358166-426e-471c-940a-b124235cb343", "Rail");
					case Core.Constants.TransportModes.Road:
						return Res.GetString("64de5b40-c0c1-45d8-b1bc-c1d567d47bf7", "Road");
					case Core.Constants.TransportModes.Sea:
						return Res.GetString("90021705-36ae-452f-b201-f1f50023bc13", "Sea");
					case Core.Constants.TransportModes.Storage:
						return Res.GetString("1ca0b6fc-5dfe-4fd1-b36c-6727bc1bb88f", "Storage");
					default:
						return mode;
				}
			}
		}

		public ZString TransportType
		{
			get { return Details.TransportType; }
		}

		public ZString TransportTypeDescription
		{
			get { return Details.TransportTypeDescription; }
		}

		public ZString VesselName
		{
			get { return Details.Vessel; }
		}

		public ZString VoyageFlight
		{
			get { return Suppression.GetValue(Details.VoyageFlight, Details.SuppressingBizO, SuppressFields.FlightNumber, ZString.Empty, DocumentContactType); }
		}

		public ZString VesselVoyageFlight
		{
			get { return VesselName + VesselVoyageDelimiter + VoyageFlight; }
		}

		#endregion

		#region ZBool Fields

		public ZBool TransportModeIsSea
		{
			get { return Details.TransportMode == Core.Constants.TransportModes.Sea; }
		}

		#endregion

		#region ZDateTime Fields

		public ZDateTime ATD
		{
			get { return Details.ATD; }
		}

		public ZDateTime ATA
		{
			get { return Details.ATA; }
		}

		public ZDateTime ETD
		{
			get { return Details.ETD; }
		}

		public ZDateTime ETA
		{
			get { return Details.ETA; }
		}

		public ZDateTime LCLReceivalCommences
		{
			get { return Details.LCLReceivalCommences; }
		}

		public ZDateTime LCLCutOff
		{
			get { return Details.LCLCutOff; }
		}

		public ZDateTime LCLAvailabilityDate
		{
			get { return Details.LCLAvailabilityDate; }
		}

		public ZDateTime LCLStorageDate
		{
			get { return Details.LCLStorageDate; }
		}

		public ZDateTime FCLReceivalCommences
		{
			get { return Details.FCLReceivalCommences; }
		}

		public ZDateTime FCLCutOff
		{
			get { return Details.FCLCutOff; }
		}

		public ZDateTime AvailabilityDate
		{
			get { return Details.FCLAvailabilityDate; }
		}

		public ZDateTime StorageDate
		{
			get { return Details.FCLStorageDate; }
		}

		#endregion

		#region Document Wrapper Fields

		public DocUNLOCO PortOfDischarge
		{
			get
			{
				if (fPortOfDischarge == null || fPortOfDischarge.Code != PortOfDischargeCode)
				{
					fPortOfDischarge = DocUNLOCO.New(Factory, PortOfDischargeCode);
				}

				return fPortOfDischarge;
			}
		}
		protected DocUNLOCO fPortOfDischarge;

		public DocUNLOCO PortOfLoading
		{
			get
			{
				if (fPortOfLoading == null || fPortOfLoading.Code != PortOfLoadingCode)
				{
					fPortOfLoading = DocUNLOCO.New(Factory, PortOfLoadingCode);
				}

				return fPortOfLoading;
			}
		}
		protected DocUNLOCO fPortOfLoading;

		public DocVessel Vessel
		{
			get { return DocVessel.New(Factory, VesselName); }
		}

		public DocOrganisation Carrier
		{
			get { return DocOrganisation.New(Factory, Details.Carrier); }
		}

		#endregion

		#region Implementation

		#region FormatFlightDetails

		ZString FormatFlightDetails(ZString voyageNo, ZString portOfDischarge, ZDateTime eTD)
		{
			ZString departureDate = eTD.IsValid ? Suppression.GetValue(eTD.ToShortDateString(), Details.SuppressingBizO, SuppressFields.ETD, string.Empty, DocumentContactType) : ZString.Empty;
			return FormatTransportDetails(voyageNo, portOfDischarge, departureDate);
		}

		#endregion

		#region FormatTransportDetails

		ZString FormatTransportDetails(params ZString[] args)
		{
			ZStringBuilder builder = new ZStringBuilder();
			foreach (ZString arg in args)
			{
				builder.Append(arg.IsEmpty ? "   " : (string)arg);
			}
			return builder.ToStringWithDelimiterBetweenAppends(" / ");
		}

		#endregion

		public override string ToString()
		{
			return Details.ParentDescription;
		}

		protected ITransportDetails Details
		{
			get { return (ITransportDetails)WrappedObject; }
		}

		protected ZString VesselVoyageDelimiter
		{
			get { return (!VesselName.IsEmpty && !VoyageFlight.IsEmpty) ? " / " : ""; }
		}

		#endregion

	}
}
