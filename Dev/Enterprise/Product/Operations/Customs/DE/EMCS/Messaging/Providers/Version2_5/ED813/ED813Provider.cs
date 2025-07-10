using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.DE.MessageContracts.EMCS;
using CargoWise.Customs.DE.MessageDefinitions.EMCSVersion2_5;
using CargoWise.Types;
using Enterprise.Customs.DE.Messaging;

namespace Enterprise.Customs.DE.EMCS.Messaging.Version2_5
{
	public class ED813Provider : IED813
	{
		public ED813Provider(ED813F message)
		{
			this.message = Argument.NotNull(message, nameof(message));
		}
		readonly ED813F message;

		public ZString MessageGroup => message.Header.MessageGroup.XmlEnumToString();

		public string MessageIdentifier => message.Header.MessageIdentifier;

		public IEMCSEvent UpdateEadEsad => updateEadEsad ?? (updateEadEsad = new ED813EventProvider(message.Body.ChangeOfDestination.UpdateEadEsad));
		IEMCSEvent updateEadEsad;

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

		public ZString TransportArrangement => message.Body.ChangeOfDestination.UpdateEadEsad.ChangedTransportArrangementSpecified ? message.Body.ChangeOfDestination.UpdateEadEsad.ChangedTransportArrangement.XmlEnumToString() : ZString.Empty;

		public ZString ComplementaryInfo => message.Body.ChangeOfDestination.UpdateEadEsad.ComplementaryInformation;

		public ZString InvoiceNumber => message.Body.ChangeOfDestination.UpdateEadEsad.InvoiceNumber;

		public ZDate InvoiceDate => message.Body.ChangeOfDestination.UpdateEadEsad.InvoiceDateSpecified ? new ZDate(message.Body.ChangeOfDestination.UpdateEadEsad.InvoiceDate) : ZDate.Empty;

		public ZString DestinationTypeCode => message.Body.ChangeOfDestination.DestinationChanged.DestinationTypeCode.XmlEnumToString();

		public ZString GuarantorTypeCode => message.Body.ChangeOfDestination.DestinationChanged.MovementGuarantee?.GuarantorTypeCode.XmlEnumToString() ?? ZString.Empty;

		public IEMCSPartyGuarantor Guarantor => guarantor ?? (GuarantorTypeCode == ED813FBodyChangeOfDestinationDestinationChangedMovementGuaranteeGuarantorTypeCode.Item3.XmlEnumToString() ? guarantor = ED813PartyGuarantorProvider.NewOrNull(message.Body.ChangeOfDestination.DestinationChanged.MovementGuarantee.GuarantorTrader?.FirstOrDefault()) : null);
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

		bool CanHaveTransportArranger => TransportArrangement == ED813FBodyChangeOfDestinationUpdateEadEsadChangedTransportArrangement.Item3.XmlEnumToString() ||
										TransportArrangement == ED813FBodyChangeOfDestinationUpdateEadEsadChangedTransportArrangement.Item4.XmlEnumToString();
	}

	class ED813EventProvider : IEMCSEvent
	{
		public ED813EventProvider(ED813FBodyChangeOfDestinationUpdateEadEsad updateEadEsad)
		{
			this.updateEadEsad = Argument.NotNull(updateEadEsad, nameof(updateEadEsad));
		}
		readonly ED813FBodyChangeOfDestinationUpdateEadEsad updateEadEsad;

		public ZString AdministrativeReferenceCode => updateEadEsad.AdministrativeReferenceCode;

		public ZString SequenceNumber => updateEadEsad.SequenceNumber;
	}
}
