using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.Messaging;
using MessageSender = Enterprise.Customs.IE.Business.MessageSender;

namespace Enterprise.Customs.IE.NCTS.Business
{
	public class QueryOnGuaranteeSendingAction : Customs.Business.BaseMessageSendingObject, IMessageSendingAction
	{
		public QueryOnGuaranteeSendingAction(NctsHeader header) : base(header.Factory)
		{
			Header = header;
		}
		public NctsHeader Header;

		public MessageSender CreateSender() => (MessageSender)Activator.CreateInstance(typeof(NctsMessageSender), this);

		[ResourceStringData("16561B04-9670-4E2F-9516-282A76D0FCB0", Caption = "Query Identifier")]
		[List(nameof(Lookups) + "." + nameof(QueryOnGuaranteeSendingActionLookups.QueryIdentifier))]
		public ZString QueryIdentifier
		{
			get { return queryIdentifier; }
			set
			{
				if (SetNonPersistentPropertyValue(QueryIdentifierInfo, ref queryIdentifier, value))
				{
					Validation.ValidateQueryIdentifier();
				}
			}
		}
		ZString queryIdentifier;

		public ZPropertyInfo QueryIdentifierInfo => GetZPropertyInfo(nameof(QueryIdentifier));

		[ResourceStringData("120280DC-9539-4EF2-A715-3C99E366AF11", Caption = "Period From")]
		public ZDate PeriodFrom
		{
			get { return periodFrom; }
			set
			{
				if (SetNonPersistentPropertyValue(PeriodFromInfo, ref periodFrom, value))
				{
					Validation.ValidatePeriodTo();
				}
			}
		}
		ZDate periodFrom;

		public ZPropertyInfo PeriodFromInfo => GetZPropertyInfo(nameof(PeriodFrom));

		[ResourceStringData("3895AB98-A15F-40F5-972B-8FC3F6E409AD", Caption = "Period To")]
		public ZDate PeriodTo
		{
			get { return periodTo; }
			set
			{
				SetNonPersistentPropertyValue(PeriodToInfo, ref periodTo, value);
			}
		}
		ZDate periodTo;

		public ZPropertyInfo PeriodToInfo => GetZPropertyInfo(nameof(PeriodTo));

		[ChildEditable(false)]

		public QueryOnGuaranteeSendingObjectCollection AllGuarantees
		{
			get
			{
				if (allGuaranteesCollection == null)
				{
					allGuaranteesCollection = new QueryOnGuaranteeSendingObjectCollection(Header);
					allGuaranteesCollection.PopulateElements();
				}
				return allGuaranteesCollection;
			}
		}
		QueryOnGuaranteeSendingObjectCollection allGuaranteesCollection;

		public IReadOnlyCollection<QueryOnGuaranteeSendingObject> GetSelectedGuarantees => AllGuarantees.Cast<QueryOnGuaranteeSendingObject>().Where(p => p.ShouldSend).ToArray();

		public QueryOnGuaranteeSendingActionLookups Lookups => GetNewLookups();

		public QueryOnGuaranteeSendingActionValidation Validation => GetNewValidation();

		public ZString MessageType { get; set; }

		public IMessageAttachee MessageAttachee => Header.IsDepartureMovement ? Header.MovementHeader : Header;

		public void AddMessage(OutboundEDIMessage message)
		{
			if (Header.IsDepartureMovement)
			{
				Header.MovementHeader.Messages.Add(message);
			}
			else
			{
				Header.Messages.Add(message);
			}
		}

		protected QueryOnGuaranteeSendingActionValidation GetNewValidation() => new QueryOnGuaranteeSendingActionValidation(this);

		protected QueryOnGuaranteeSendingActionLookups GetNewLookups() => new QueryOnGuaranteeSendingActionLookups(this);
	}
}
