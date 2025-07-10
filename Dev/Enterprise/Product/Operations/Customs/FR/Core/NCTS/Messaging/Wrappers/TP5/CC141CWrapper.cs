using System;
using CargoWise.Common;
using CargoWise.Customs.FR.MessageDefinitions.TP5.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.FR.Business;
using Enterprise.Customs.FR.Business.NCTS;

namespace Enterprise.Customs.FR.NCTS.Messaging.TP5
{
	public class CC141CWrapper : ICC141C
	{
		public CC141CWrapper(TP5MessageSendingObject sendingObject)
		{
			this.sendingObject = sendingObject;
			this.nctsHeader = Argument.NotNull((NctsHeader)sendingObject.NctsHeader, nameof(sendingObject));
		}

		readonly NctsHeader nctsHeader;
		readonly TP5MessageSendingObject sendingObject;

		public static CC141CWrapper New(TP5MessageSendingObject sendingObject) => sendingObject == null ? null : new CC141CWrapper(sendingObject);

		public string MessageSender => FRConstants.NCTSMessage.Operator;

		public string MessageRecipient => FRConstants.NCTSMessage.NationalAdministration;

		public DateTime PreparationDateAndTime => ZDateTime.Now.ToDateTime();

		public string CorrelationIdentifier => ZString.Empty;

		public ITransitOperation TransitOperation => transitOperation ?? (transitOperation = TransitOperationWrapper.New(sendingObject, true));
		ITransitOperation transitOperation;

		public ICustomsOffice CustomsOfficeOfDestinationActual => customsOfficeOfDestinationActual ?? (customsOfficeOfDestinationActual = CustomsOfficeWrapper.New(sendingObject.ActualOfficeOfDestination));
		ICustomsOffice customsOfficeOfDestinationActual;

		public ICustomsOffice CustomsOfficeOfEnquiryAtDeparture => customsOfficeOfEnquiryAtDeparture ?? (customsOfficeOfEnquiryAtDeparture = CustomsOfficeWrapper.New(nctsHeader.MovementHeader.DepartureCustomsOfficeCode));
		ICustomsOffice customsOfficeOfEnquiryAtDeparture;

		public IConsignment Consignment => consignment ?? (consignment = ConsignmentWrapper.New(nctsHeader.MovementHeader, sendingObject.ActualConsignee.Organisation));
		IConsignment consignment;

		public IEnquiry Enquiry => enquiry ?? (enquiry = EnquiryWrapper.New(sendingObject));
		IEnquiry enquiry;

		public IHolderOfTheTransitProcedure HolderOfTheTransitProcedure => holderOfTheTransitProcedure ?? (holderOfTheTransitProcedure = HolderOfTheTransitProcedureWrapper.New(nctsHeader.MovementHeader.ProcedureHolder));
		IHolderOfTheTransitProcedure holderOfTheTransitProcedure;

		public IMessageEnveloppe MessageEnveloppe => messageEnveloppe ?? (messageEnveloppe = MessageEnveloppeWrapper.New(nctsHeader, "IE141"));
		IMessageEnveloppe messageEnveloppe;
	}
}
