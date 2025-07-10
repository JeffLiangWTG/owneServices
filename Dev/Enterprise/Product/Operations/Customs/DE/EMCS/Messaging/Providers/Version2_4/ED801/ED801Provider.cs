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
	public class ED801Provider : IED801
	{
		public ED801Provider(ED801D message)
		{
			this.message = Argument.NotNull(message, nameof(message));
		}
		readonly ED801D message;

		public ZString MessageSender => message.Header.MessageSender;

		public ZString MessageGroup => message.Header.MessageGroup.XmlEnumToString();

		public string MessageIdentifier => message.Header.MessageIdentifier;

		public ZString LocalReferenceNumber => message.Body.EadContainer.Ead.LocalReferenceNumber;

		public ZString MessageRecipient => message.Header.MessageRecipient;

		public ZDateTime DateAndTimeOfValidationOfEadEsad => message.Body.EadContainer.ExciseMovementEad.DateAndTimeOfValidationOfEad;

		public ZString DispatchImportOffice => message.Body.EadContainer.DispatchImportOffice?.ReferenceNumber ?? ZString.Empty;

		public ZString DeliveryPlaceCustomsOffice => message.Body.EadContainer.DeliveryPlaceCustomsOffice?.ReferenceNumber ?? ZString.Empty;

		public ZString CompetentAuthorityDispatchOffice => message.Body.EadContainer.CompetentAuthorityDispatchOffice.ReferenceNumber;

		public ZString JourneyTime
		{
			get
			{
				var time = message.Body.EadContainer.HeaderEad.JourneyTime.Substring(1, 2);
				var timeFormat = message.Body.EadContainer.HeaderEad.JourneyTime.Substring(0, 1);
				return time + timeFormat;
			}
		}

		public ZString DestinationTypeCode => message.Body.EadContainer.HeaderEad.DestinationTypeCode.XmlEnumToString();

		public ZString TransportArrangement => message.Body.EadContainer.HeaderEad.TransportArrangement.XmlEnumToString();

		public ZDateTime DispatchTime
		{
			get
			{
				var dateOfDispatch = message.Body.EadContainer.Ead.DateOfDispatch;
				var timeOfDispatch = new ZString(message.Body.EadContainer.Ead.TimeOfDispatch);
				return new ZDateTime(dateOfDispatch.Year, dateOfDispatch.Month, dateOfDispatch.Day, ZInt.ParseEmptyAsZero(timeOfDispatch.SubstringSafe(0, 2)), ZInt.ParseEmptyAsZero(timeOfDispatch.SubstringSafe(3, 2)), 0);
			}
		}

		public ZString OriginTypeCode => message.Body.EadContainer.Ead.OriginTypeCode.XmlEnumToString();

		public ZString InvoiceNumber => message.Body.EadContainer.Ead.InvoiceNumber;

		public ZDate InvoiceDate => message.Body.EadContainer.Ead.InvoiceDateSpecified ? new ZDate(message.Body.EadContainer.Ead.InvoiceDate) : ZDate.Empty;

		public IReadOnlyCollection<ZString> ImportSadNumbers => importSadNumbers ?? (importSadNumbers = message.Body.EadContainer.Ead.ImportSad?.Select(x => new ZString(x.ImportSadNumber)).ToArray() ?? Array.Empty<ZString>());
		IReadOnlyCollection<ZString> importSadNumbers;

		public ZString GuarantorTypeCode => message.Body.EadContainer.MovementGuarantee.GuarantorTypeCode.XmlEnumToString();

		public IEMCSPartyGuarantor Guarantor => guarantor ?? (GuarantorTypeCode == ED801DBodyEadContainerMovementGuaranteeGuarantorTypeCode.Item3.XmlEnumToString() ? guarantor = ED801PartyGuarantorProvider.NewOrNull(message.Body.EadContainer.MovementGuarantee.GuarantorTrader?.FirstOrDefault()) : null);
		IEMCSPartyGuarantor guarantor;

		public IEMCSPartyConsignee Consignee => consignee ?? (consignee = ED801PartyConsigneeProvider.NewOrNull(message.Body.EadContainer.ConsigneeTrader));
		IEMCSPartyConsignee consignee;

		public IEMCSPartyConsignor Consignor => consignor ?? (consignor = ED801PartyConsignorProvider.NeworNull(message.Body.EadContainer.ConsignorTrader));
		IEMCSPartyConsignor consignor;

		public IEMCSPartyPlaceOfDispatch PlaceOfDispatch => placeOfDispatch ?? (placeOfDispatch = ED801PartyPlaceOfDispatchProvider.NewOrNull(message.Body.EadContainer.PlaceOfDispatchTrader));
		IEMCSPartyPlaceOfDispatch placeOfDispatch;

		public IEMCSPartyDeliveryPlace DeliveryPlace => deliveryPlace ?? (deliveryPlace = ED801PartyDeliveryPlaceProvider.NewOrNull(message.Body.EadContainer.DeliveryPlaceTrader));
		IEMCSPartyDeliveryPlace deliveryPlace;

		public IEMCSPartyTransporter TransportArranger => transportArranger ?? (transportArranger = ED801PartyTransportArrangerProvider.NewOrNull(message.Body.EadContainer.TransportArrangerTrader));
		IEMCSPartyTransporter transportArranger;

		public IEMCSPartyTransporter FirstTransporter => firstTransporter ?? (firstTransporter = ED801PartyFirstTransporterProvider.NewOrNull(message.Body.EadContainer.FirstTransporterTrader));
		IEMCSPartyTransporter firstTransporter;

		public ZString TransportModeCode => message.Body.EadContainer.TransportMode.TransportModeCode;

		public ZString ComplementaryInfo => message.Body.EadContainer.TransportMode.ComplementaryInformation;

		public IReadOnlyCollection<IEMCSDocumentCert> DocumentCertificates => documentCertificates ?? (documentCertificates = message.Body.EadContainer.DocumentCertificate?.Select(x => new ED801DocumentCertProvider(x)).ToArray() ?? Array.Empty<ED801DocumentCertProvider>());
		IReadOnlyCollection<IEMCSDocumentCert> documentCertificates;

		public ZString MemberStateCode => message.Body.EadContainer.ComplementConsigneeTrader?.MemberStateCode ?? ZString.Empty;

		public ZString CertificateOfExemption => message.Body.EadContainer.ComplementConsigneeTrader?.SerialNumberOfCertificateOfExemption ?? ZString.Empty;

		public IReadOnlyCollection<IEMCSTransportDetails> TransportDetails => transportDetails ?? (transportDetails = message.Body.EadContainer.TransportDetails.Select(x => new ED801TransportDetailsProvider(x)).ToArray());
		IReadOnlyCollection<IEMCSTransportDetails> transportDetails;

		public IEMCSEvent ExciseMovement => exciseMovementEad ?? (exciseMovementEad = new ED801EventProvider(message));
		IEMCSEvent exciseMovementEad;

		public IReadOnlyCollection<IED801Line> Lines => lines ?? (lines = message.Body.EadContainer.BodyEad.Select(x => new ED801LineProvider(x)).ToArray());
		IReadOnlyCollection<IED801Line> lines;
	}

	class ED801EventProvider : IEMCSEvent
	{
		public ED801EventProvider(ED801D message)
		{
			this.message = message;
		}
		readonly ED801D message;

		public ZString AdministrativeReferenceCode => message.Body.EadContainer.ExciseMovementEad.AdministrativeReferenceCode;

		public ZString SequenceNumber => message.Body.EadContainer.HeaderEad.SequenceNumber;
	}
}
