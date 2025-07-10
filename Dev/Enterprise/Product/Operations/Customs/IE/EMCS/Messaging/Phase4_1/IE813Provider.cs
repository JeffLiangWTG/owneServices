using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.IE.MessageContracts.EMCS.Interfaces;
using CargoWise.Customs.IE.MessageDefinitions.EMCSPhase4_1.IE813;
using CargoWise.Types;
using Enterprise.Customs.EU.EMCS.Messaging;

namespace Enterprise.Customs.IE.EMCS.Messaging.Phase4_1
{
	public class IE813Provider : IIE813
	{
		public IE813Provider(Ie813Type message)
		{
			this.message = Argument.NotNull(message, nameof(message));
		}
		readonly Ie813Type message;

		public ZString MrnNumber => AdministrativeReferenceCode;

		public ZString MrnNumberSequenceNumber => (ZShort.ParseSafe(UpdateEad.SequenceNumber, 1) - 1).ToString();

		public ZString AdministrativeReferenceCode => UpdateEad.AdministrativeReferenceCode;

		public IEMCSEvent UpdateEad => updateEad ?? (updateEad = new IE813EventProvider(message.Body.ChangeOfDestination.UpdateEadEsad));
		IEMCSEvent updateEad;

		public ZString NewDestinationCode => message.Body.ChangeOfDestination.DestinationChanged.DestinationTypeCode.XmlEnumToString();

		public ZString JourneyTime
		{
			get
			{
				var journeyTimeString = new ZString(message.Body.ChangeOfDestination.UpdateEadEsad.JourneyTime);
				return journeyTimeString.SubstringSafe(1, 2) + journeyTimeString.SubstringSafe(0, 1);
			}
		}

		public ZString TransportModeCode => message.Body.ChangeOfDestination.UpdateEadEsad.TransportModeCode;

		public ZString TransportArrangement => message.Body.ChangeOfDestination.UpdateEadEsad.ChangedTransportArrangementValueSpecified ? message.Body.ChangeOfDestination.UpdateEadEsad.ChangedTransportArrangement.Value.XmlEnumToString() : ZString.Empty;

		public ZString ComplementaryInfo => message.Body.ChangeOfDestination.UpdateEadEsad.ComplementaryInformation?.Value ?? ZString.Empty;

		public ZString InvoiceNumber => message.Body.ChangeOfDestination.UpdateEadEsad.InvoiceNumber;

		public ZDate InvoiceDate => message.Body.ChangeOfDestination.UpdateEadEsad.InvoiceDateValueSpecified ? new ZDate(message.Body.ChangeOfDestination.UpdateEadEsad.InvoiceDate) : ZDate.Empty;

		public ZString DestinationTypeCode => message.Body.ChangeOfDestination.DestinationChanged.DestinationTypeCode.XmlEnumToString();

		public ZString GuarantorTypeCode => message.Body.ChangeOfDestination.DestinationChanged.MovementGuarantee?.GuarantorTypeCode.XmlEnumToString() ?? ZString.Empty;

		public IEMCSPartyGuarantor Guarantor => guarantor ?? ((message.Body.ChangeOfDestination.DestinationChanged.MovementGuarantee?.GuarantorTraderSpecified ?? false) && GuarantorTypeCode == CargoWise.Customs.IE.MessageDefinitions.EMCSPhase4_1.tcl.GuarantorTypeCode.Item3.XmlEnumToString() ? guarantor = IE813PartyGuarantorProvider.NewOrNull(message.Body.ChangeOfDestination.DestinationChanged.MovementGuarantee.GuarantorTrader?.FirstOrDefault()) : null);
		IEMCSPartyGuarantor guarantor;

		public IEMCSPartyDeliveryPlace DeliveryPlace => deliveryPlace ?? (deliveryPlace = IE813PartyDeliveryPlaceProvider.NewOrNull(message.Body.ChangeOfDestination.DestinationChanged.DeliveryPlaceTrader));
		IEMCSPartyDeliveryPlace deliveryPlace;

		public IEMCSPartyTransporter NewTransportArranger => newTransportArranger ?? (CanHaveTransportArranger ? newTransportArranger = IE813PartyNewTransportArrangerProvider.NewOrNull(message.Body.ChangeOfDestination.NewTransportArrangerTrader) : null);
		IEMCSPartyTransporter newTransportArranger;

		public IEMCSPartyTransporter NewTransporter => newTransporter ?? (newTransporter = IE813PartyNewTransporterProvider.NewOrNull(message.Body.ChangeOfDestination.NewTransporterTrader));
		IEMCSPartyTransporter newTransporter;

		public IReadOnlyCollection<IEMCSTransportDetails> TransportDetails => transportDetails ?? (transportDetails = message.Body.ChangeOfDestination.TransportDetails?.Select(x => new IE813TransportDetailsProvider(x)).ToArray<IEMCSTransportDetails>() ?? Array.Empty<IEMCSTransportDetails>());
		IReadOnlyCollection<IEMCSTransportDetails> transportDetails;

		public IEMCSPartyConsignee Consignee => consignee ?? (consignee = IE813PartyNewConsigneeProvider.NewOrNull(message.Body.ChangeOfDestination.DestinationChanged.NewConsigneeTrader));
		IEMCSPartyConsignee consignee;

		bool CanHaveTransportArranger => TransportArrangement == CargoWise.Customs.IE.MessageDefinitions.EMCSPhase4_1.tcl.TransportArrangement.Item3.XmlEnumToString() ||
										TransportArrangement == CargoWise.Customs.IE.MessageDefinitions.EMCSPhase4_1.tcl.TransportArrangement.Item4.XmlEnumToString();
	}

	class IE813EventProvider : IEMCSEvent
	{
		public IE813EventProvider(UpdateEadEsadType updateEad)
		{
			this.updateEad = Argument.NotNull(updateEad, nameof(updateEad));
		}
		readonly UpdateEadEsadType updateEad;

		public ZString AdministrativeReferenceCode => updateEad.AdministrativeReferenceCode;

		public ZString SequenceNumber => updateEad.SequenceNumber;
	}
}
