using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using CargoWise.Common;
using CargoWise.Customs.FR.MessageDefinitions.TP5.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.FR.Business;
using Enterprise.Customs.FR.Business.NCTS;

namespace Enterprise.Customs.FR.NCTS.Messaging.TP5
{
	public class CC007CWrapper : ICC007C
	{
		CC007CWrapper(NctsHeader nctsHeader)
		{
			this.nctsHeader = Argument.NotNull(nctsHeader, nameof(nctsHeader));
		}
		readonly NctsHeader nctsHeader;

		public static CC007CWrapper New(NctsHeader nctsHeader) => nctsHeader == null ? null : new CC007CWrapper(nctsHeader);

		public string MessageSender => FRConstants.NCTSMessage.Operator;

		public string MessageRecipient => FRConstants.NCTSMessage.NationalAdministration;

		public DateTime PreparationDateAndTime => ZDateTime.Now.ToDateTime();

		public string CorrelationIdentifier => ZString.Empty;

		public IArrivalTransitOperation TransitOperation => transitOperation ?? (transitOperation = ArrivalTransitOperationWrapper.New(nctsHeader));
		IArrivalTransitOperation transitOperation;

		public ICollection<IAuthorisation> Authorisation => authorisation ?? (authorisation = GetAuthorisations());
		ICollection<IAuthorisation> authorisation;

		ICollection<IAuthorisation> GetAuthorisations()
		{
			var result = new Collection<IAuthorisation>();
			result.Add(MovementAuthorisationWrapper.New(nctsHeader.ArrivalMovementHeader));
			return result;
		}

		public ICustomsOffice CustomsOfficeOfDestinationActual => customsOfficeOfDestinationActual ?? (customsOfficeOfDestinationActual = CustomsOfficeWrapper.New(nctsHeader.ArrivalMovementHeader?.DestinationCustomsOfficeCodeForArrival));
		ICustomsOffice customsOfficeOfDestinationActual;

		public ITrader TraderAtDestination => traderAtDestination ?? (traderAtDestination = TraderWrapper.New(nctsHeader.DestinationTrader));
		ITrader traderAtDestination;

		public IConsignment Consignment => consignment ?? (consignment = ConsignmentWrapper.New(nctsHeader.ArrivalMovementHeader));
		IConsignment consignment;

		public IMessageEnveloppe MessageEnveloppe => messageEnveloppe ?? (messageEnveloppe = MessageEnveloppeWrapper.New(nctsHeader, "IE007"));
		IMessageEnveloppe messageEnveloppe;
	}
}
