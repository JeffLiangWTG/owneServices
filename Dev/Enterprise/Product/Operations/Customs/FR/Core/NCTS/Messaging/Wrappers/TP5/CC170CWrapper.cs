using System;
using CargoWise.Common;
using CargoWise.Customs.FR.MessageDefinitions.TP5.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.FR.Business;
using Enterprise.Customs.FR.Business.NCTS;

namespace Enterprise.Customs.FR.NCTS.Messaging.TP5
{
	public class CC170CWrapper : ICC170C
	{
		public CC170CWrapper(TP5MessageSendingObject sendingObject)
		{
			this.sendingObject = sendingObject;
			this.nctsHeader = Argument.NotNull((NctsHeader)sendingObject.NctsHeader, nameof(sendingObject));
		}

		readonly NctsHeader nctsHeader;
		readonly TP5MessageSendingObject sendingObject;

		public static CC170CWrapper New(TP5MessageSendingObject sendingObject) => sendingObject == null ? null : new CC170CWrapper(sendingObject);

		public string MessageSender => FRConstants.NCTSMessage.Operator;

		public string MessageRecipient => FRConstants.NCTSMessage.NationalAdministration;

		public DateTime PreparationDateAndTime => ZDateTime.Now.ToDateTime();

		public string CorrelationIdentifier => ZString.Empty;

		public ITransitOperation TransitOperation => transitOperation ?? (transitOperation = TransitOperationWrapper.New(sendingObject, true));
		ITransitOperation transitOperation;

		public ICustomsOffice CustomsOfficeOfDeparture => customsOfficeOfDeparture ?? (customsOfficeOfDeparture = CustomsOfficeWrapper.New(nctsHeader.MovementHeader.DepartureCustomsOfficeCode));
		ICustomsOffice customsOfficeOfDeparture;

		public IHolderOfTheTransitProcedure HolderOfTheTransitProcedure => holderOfTheTransitProcedure ?? (holderOfTheTransitProcedure = HolderOfTheTransitProcedureWrapper.New(nctsHeader.MovementHeader.ProcedureHolder));
		IHolderOfTheTransitProcedure holderOfTheTransitProcedure;

		public IRepresentative Representative => representative ?? (representative = RepresentativeWrapper.New(nctsHeader.MovementHeader.Representative.Organisation));
		IRepresentative representative;

		public IConsignment Consignment => consignment ?? (consignment = ConsignmentWrapper.New(nctsHeader.MovementHeader));
		IConsignment consignment;

		public IMessageEnveloppe MessageEnveloppe => messageEnveloppe ?? (messageEnveloppe = MessageEnveloppeWrapper.New(nctsHeader, "IE170"));
		IMessageEnveloppe messageEnveloppe;
	}
}
