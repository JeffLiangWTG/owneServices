using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.DE.MessageContracts.EMCS;
using CargoWise.Customs.DE.MessageDefinitions.EMCSVersion2_4;
using CargoWise.Types;
using Enterprise.Customs.DE.Messaging;

namespace Enterprise.Customs.DE.EMCS.Messaging.Version2_4
{
	public class ED813Provider : IED813
	{
		public ED813Provider(ED813E message)
		{
			this.message = Argument.NotNull(message, nameof(message));
		}
		readonly ED813E message;

		public ZString MessageGroup => message.Header.MessageGroup.XmlEnumToString();

		public string MessageIdentifier => message.Header.MessageIdentifier;

		public IEMCSEvent UpdateEadEsad => updateEad ?? (updateEad = new ED813EventProvider(message.Body.ChangeOfDestination.UpdateEad));
		IEMCSEvent updateEad;

		public ZString NewDestinationCode => message.Body.ChangeOfDestination.DestinationChanged.DestinationTypeCode.XmlEnumToString();

		public ZString JourneyTime
		{
			get
			{
				var journeyTimeString = new ZString(message.Body.ChangeOfDestination.UpdateEad.JourneyTime);
				return journeyTimeString.SubstringSafe(1, 2) + journeyTimeString.SubstringSafe(0, 1);
			}
		}

		public ZString TransportModeCode => message.Body.ChangeOfDestination.UpdateEad.TransportModeCode;

		public ZString TransportArrangement => message.Body.ChangeOfDestination.UpdateEad.ChangedTransportArrangementSpecified ? message.Body.ChangeOfDestination.UpdateEad.ChangedTransportArrangement.XmlEnumToString() : ZString.Empty;

		public ZString ComplementaryInfo => message.Body.ChangeOfDestination.UpdateEad.ComplementaryInformation;

		public ZString InvoiceNumber => message.Body.ChangeOfDestination.UpdateEad.InvoiceNumber;

		public ZDate InvoiceDate => message.Body.ChangeOfDestination.UpdateEad.InvoiceDateSpecified ? new ZDate(message.Body.ChangeOfDestination.UpdateEad.InvoiceDate) : ZDate.Empty;

		public ZString DestinationTypeCode => message.Body.ChangeOfDestination.DestinationChanged.DestinationTypeCode.XmlEnumToString();

		public ZString GuarantorTypeCode => message.Body.ChangeOfDestination.DestinationChanged.MovementGuarantee?.GuarantorTypeCode.XmlEnumToString() ?? ZString.Empty;

		public IEMCSPartyGuarantor Guarantor => guarantor ?? (GuarantorTypeCode == ED813EBodyChangeOfDestinationDestinationChangedMovementGuaranteeGuarantorTypeCode.Item3.XmlEnumToString() ? guarantor = ED813PartyGuarantorProvider.NewOrNull(message.Body.ChangeOfDestination.DestinationChanged.MovementGuarantee.GuarantorTrader?.FirstOrDefault()) : null);
		IEMCSPartyGuarantor guarantor;

		public IEMCSPartyDeliveryPlace DeliveryPlace => deliveryPlace ?? (deliveryPlace = ED813PartyDeliveryPlaceProvider.NewOrNull(message.Body.ChangeOfDestination.DestinationChanged.DeliveryPlaceTrader));
		IEMCSPartyDeliveryPlace deliveryPlace;

		public IEMCSPartyTransporter NewTransportArranger => newTransportArranger ?? (CanHaveTransportArranger ? newTransportArranger = ED813PartyNewTransportArrangerProvider.NewOrNull(message.Body.ChangeOfDestination.NewTransportArrangerTrader) : null);
		IEMCSPartyTransporter newTransportArranger;

		public IEMCSPartyTransporter NewTransporter => newTransporter ?? (newTransporter = ED813PartyNewTransporterProvider.NewOrNull(message.Body.ChangeOfDestination.NewTransporterTrader));
		IEMCSPartyTransporter newTransporter;

		public IReadOnlyCollection<IEMCSTransportDetails> TransportDetails => transportDetails ?? (transportDetails = message.Body.ChangeOfDestination.TransportDetails?.Select(x => new ED813TransportDetailsProvider(x)).ToArray<IEMCSTransportDetails>() ?? Array.Empty<IEMCSTransportDetails>());
		IReadOnlyCollection<IEMCSTransportDetails> transportDetails;

		public IEMCSPartyConsignee Consignee => consignee ?? (consignee = ED813PartyNewConsigneeProvider.NewOrNull(message.Body.ChangeOfDestination.DestinationChanged.NewConsigneeTrader));
		IEMCSPartyConsignee consignee;

		bool CanHaveTransportArranger => TransportArrangement == ED813EBodyChangeOfDestinationUpdateEadChangedTransportArrangement.Item3.XmlEnumToString() ||
										TransportArrangement == ED813EBodyChangeOfDestinationUpdateEadChangedTransportArrangement.Item4.XmlEnumToString();
	}

	class ED813EventProvider : IEMCSEvent
	{
		public ED813EventProvider(ED813EBodyChangeOfDestinationUpdateEad updateEad)
		{
			this.updateEad = Argument.NotNull(updateEad, nameof(updateEad));
		}
		readonly ED813EBodyChangeOfDestinationUpdateEad updateEad;

		public ZString AdministrativeReferenceCode => updateEad.AdministrativeReferenceCode;

		public ZString SequenceNumber => updateEad.SequenceNumber;
	}
}
