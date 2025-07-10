using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.GB.MessageContracts.EMCS;
using CargoWise.Customs.GB.MessageDefinitions.EMCS.Version4_1.ie801;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.EMCS.Messaging;

namespace Enterprise.Customs.GB.EMCS.Messaging.Version4_1
{
	public sealed class IE801Provider : IIE801
	{
		public IE801Provider(Ie801Type message)
		{
			this.message = Argument.NotNull(message, nameof(message));
		}
		readonly Ie801Type message;

		public ZString MessageSender => message.Header.MessageSender;

		public ZString MrnNumber => message.Body.EadesadContainer.ExciseMovement.AdministrativeReferenceCode;

		public ZString MrnNumberSequenceNumber => message.Body.EadesadContainer.HeaderEadEsad.SequenceNumber;

		public ZString LocalReferenceNumber => message.Body.EadesadContainer.EadEsad.LocalReferenceNumber;

		public ZDateTime DateAndTimeOfValidationOfEad => message.Body.EadesadContainer.ExciseMovement.DateAndTimeOfValidationOfEadEsad;

		public ZString DispatchImportOffice => message.Body.EadesadContainer.DispatchImportOffice?.ReferenceNumber ?? ZString.Empty;

		public ZString DeliveryPlaceCustomsOffice => message.Body.EadesadContainer.DeliveryPlaceCustomsOffice?.ReferenceNumber ?? ZString.Empty;

		public ZString CompetentAuthorityDispatchOffice => message.Body.EadesadContainer.CompetentAuthorityDispatchOffice.ReferenceNumber;

		public ZString JourneyTime => (journeyTime ?? (journeyTime = new CachedValue<ZString>(() => message.Body.EadesadContainer.HeaderEadEsad.JourneyTime.Substring(1, 2) + message.Body.EadesadContainer.HeaderEadEsad.JourneyTime.Substring(0, 1)))).Value;
		CachedValue<ZString> journeyTime;

		public ZString DestinationTypeCode => message.Body.EadesadContainer.HeaderEadEsad.DestinationTypeCode.XmlEnumToString();

		public ZString TransportArrangement => message.Body.EadesadContainer.HeaderEadEsad.TransportArrangement.XmlEnumToString();

		public ZDateTime DispatchTime
		{
			get
			{
				if (dispatchTime == null)
				{
					var ead = message.Body.EadesadContainer.EadEsad;
					var dateOfDispatch = ead.DateOfDispatch;
					var timeOfDispatch = ead.TimeOfDispatchValueSpecified ? ead.TimeOfDispatch.Value : ZDateTime.Empty;
					dispatchTime = new CachedValue<ZDateTime>(() => new ZDateTime(dateOfDispatch.Year, dateOfDispatch.Month, dateOfDispatch.Day, timeOfDispatch.Hour, timeOfDispatch.Minute, 0));
				}
				return dispatchTime.Value;
			}
		}
		CachedValue<ZDateTime> dispatchTime;

		public ZString OriginTypeCode => message.Body.EadesadContainer.EadEsad.OriginTypeCode.XmlEnumToString();

		public ZString InvoiceNumber => message.Body.EadesadContainer.EadEsad.InvoiceNumber;

		public ZDate InvoiceDate => message.Body.EadesadContainer.EadEsad.InvoiceDateValueSpecified ? new ZDate(message.Body.EadesadContainer.EadEsad.InvoiceDate) : ZDate.Empty;

		public IReadOnlyCollection<ZString> ImportSadNumbers => importSadNumbers ?? (importSadNumbers = message.Body.EadesadContainer.EadEsad.ImportSad?.Select(x => new ZString(x.ImportSadNumber)).ToArray() ?? Array.Empty<ZString>());
		IReadOnlyCollection<ZString> importSadNumbers;

		public ZString GuarantorTypeCode => message.Body.EadesadContainer.MovementGuarantee.GuarantorTypeCode.XmlEnumToString();

		public IEMCSPartyGuarantor Guarantor => guarantor ?? (GuarantorTypeCode == CargoWise.Customs.GB.MessageDefinitions.EMCS.Version4_1.tcl.GuarantorTypeCode.Item3.XmlEnumToString() ? guarantor = IE801PartyGuarantorProvider.NewOrNull(message.Body.EadesadContainer.MovementGuarantee.GuarantorTrader?.FirstOrDefault()) : null);
		IEMCSPartyGuarantor guarantor;

		public IEMCSPartyConsignee Consignee => consignee ?? (consignee = IE801PartyConsigneeProvider.NewOrNull(message.Body.EadesadContainer.ConsigneeTrader));
		IEMCSPartyConsignee consignee;

		public IEMCSPartyConsignor Consignor => consignor ?? (consignor = IE801PartyConsignorProvider.NeworNull(message.Body.EadesadContainer.ConsignorTrader));
		IEMCSPartyConsignor consignor;

		public IEMCSPartyPlaceOfDispatch PlaceOfDispatch => placeOfDispatch ?? (placeOfDispatch = IE801PartyPlaceOfDispatchProvider.NewOrNull(message.Body.EadesadContainer.PlaceOfDispatchTrader));
		IEMCSPartyPlaceOfDispatch placeOfDispatch;

		public IEMCSPartyDeliveryPlace DeliveryPlace => deliveryPlace ?? (deliveryPlace = IE801PartyDeliveryPlaceProvider.NewOrNull(message.Body.EadesadContainer.DeliveryPlaceTrader));
		IEMCSPartyDeliveryPlace deliveryPlace;

		public IEMCSPartyTransporter TransportArranger => transportArranger ?? (transportArranger = IE801PartyTransportArrangerProvider.NewOrNull(message.Body.EadesadContainer.TransportArrangerTrader));
		IEMCSPartyTransporter transportArranger;

		public IEMCSPartyTransporter FirstTransporter => firstTransporter ?? (firstTransporter = IE801PartyFirstTransporterProvider.NewOrNull(message.Body.EadesadContainer.FirstTransporterTrader));
		IEMCSPartyTransporter firstTransporter;

		public ZString TransportModeCode => message.Body.EadesadContainer.TransportMode?.TransportModeCode;

		public ZString ComplementaryInfo => message.Body.EadesadContainer.TransportMode?.ComplementaryInformation?.Value;

		public IReadOnlyCollection<IEMCSDocumentCert> DocumentCertificates => documentCertificates ?? (documentCertificates = message.Body.EadesadContainer.DocumentCertificate?.Select(x => new IE801DocumentCertProvider(x)).ToArray() ?? Array.Empty<IE801DocumentCertProvider>());
		IReadOnlyCollection<IEMCSDocumentCert> documentCertificates;

		public ZString MemberStateCode => message.Body.EadesadContainer.ComplementConsigneeTrader?.MemberStateCode ?? ZString.Empty;

		public ZString CertificateOfExemption => message.Body.EadesadContainer.ComplementConsigneeTrader?.SerialNumberOfCertificateOfExemption ?? ZString.Empty;

		public IReadOnlyCollection<IEMCSTransportDetails> TransportDetails => transportDetails ?? (transportDetails = message.Body.EadesadContainer.TransportDetails.Select(x => new IE801TransportDetailsProvider(x)).ToArray());
		IReadOnlyCollection<IEMCSTransportDetails> transportDetails;

		public IEMCSEvent ExciseMovementEad => exciseMovementEad ?? (exciseMovementEad = new IE801EventProvider(message));
		IEMCSEvent exciseMovementEad;

		public IReadOnlyCollection<IIE801Line> Lines => lines ?? (lines = message.Body.EadesadContainer.BodyEadEsad.Select(x => new IE801LineProvider(x)).ToArray());
		IReadOnlyCollection<IIE801Line> lines;

		sealed class IE801EventProvider : IEMCSEvent
		{
			public IE801EventProvider(Ie801Type message)
			{
				this.message = message;
			}
			readonly Ie801Type message;

			public ZString AdministrativeReferenceCode => message.Body.EadesadContainer.ExciseMovement.AdministrativeReferenceCode;

			public ZString SequenceNumber => message.Body.EadesadContainer.HeaderEadEsad.SequenceNumber;
		}
	}
}
