using System;
using CargoWise.Common;
using CargoWise.Customs.FR.MessageDefinitions.TP5.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.FR.Business;
using Enterprise.Customs.FR.Business.NCTS;

namespace Enterprise.Customs.FR.NCTS.Messaging.TP5
{
	public class CC014CWrapper : ICC014C
	{
		public CC014CWrapper(TP5MessageSendingObject sendingObject)
		{
			this.sendingObject = sendingObject;
			this.nctsHeader = Argument.NotNull((NctsHeader)sendingObject.NctsHeader, nameof(sendingObject));
		}

		readonly NctsHeader nctsHeader;
		readonly TP5MessageSendingObject sendingObject;

		public static CC014CWrapper New(TP5MessageSendingObject sendingObject) => sendingObject == null ? null : new CC014CWrapper(sendingObject);

		public string MessageSender => FRConstants.NCTSMessage.Operator;

		public string MessageRecipient => FRConstants.NCTSMessage.NationalAdministration;

		public DateTime PreparationDateAndTime => ZDateTime.Now.ToDateTime();

		public string CorrelationIdentifier => ZString.Empty;

		public ITransitOperation TransitOperation => transitOperation ?? (transitOperation = TransitOperationWrapper.New(sendingObject));
		ITransitOperation transitOperation;

		public ICustomsOffice CustomsOfficeOfDeparture => customsOfficeOfDeparture ?? (customsOfficeOfDeparture = CustomsOfficeWrapper.New(nctsHeader.MovementHeader.DepartureCustomsOfficeCode));
		ICustomsOffice customsOfficeOfDeparture;

		public IHolderOfTheTransitProcedure HolderOfTheTransitProcedure => holderOfTheTransitProcedure ?? (holderOfTheTransitProcedure = HolderOfTheTransitProcedureWrapper.New(nctsHeader.MovementHeader.ProcedureHolder));
		IHolderOfTheTransitProcedure holderOfTheTransitProcedure;

		public IInvalidation Invalidation => invalidation ?? (invalidation = InvalidationWrapper.New(sendingObject));
		IInvalidation invalidation;

		public IMessageEnveloppe MessageEnveloppe => messageEnveloppe ?? (messageEnveloppe = MessageEnveloppeWrapper.New(nctsHeader, "IE014"));
		IMessageEnveloppe messageEnveloppe;
	}
}
