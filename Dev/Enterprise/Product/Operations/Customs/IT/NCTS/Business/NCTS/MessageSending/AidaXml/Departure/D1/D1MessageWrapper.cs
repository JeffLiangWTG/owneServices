using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.IT.MessageContracts.NCTS.Departure;

namespace Enterprise.Customs.IT.NCTS.Business.MessageSending.AidaXml;

sealed class D1MessageWrapper : ID1Message
{
	public D1MessageWrapper(NctsHeader header, IMessageSendingWrapperFactory messageSendingWrapperFactory, INctsAmendment amendment = null)
	{
		this.header = Argument.NotNull(header, nameof(header));
		this.messageSendingWrapperFactory = Argument.NotNull(messageSendingWrapperFactory, nameof(messageSendingWrapperFactory));

		lazyConsignment = new Lazy<ID1Consignment>(() => new D1ConsignmentWrapper(header, messageSendingWrapperFactory, amendment));
		lazyHouseConsignments = new Lazy<IReadOnlyCollection<ID1HouseConsignment>>(GetHouseConsignments);
	}

	#region ID1Message

	IReadOnlyCollection<ID1HouseConsignment> ID1Message.HouseConsignments => lazyHouseConsignments.Value;

	ID1Consignment ID1Message.Consignment => lazyConsignment.Value;

	#endregion

	IReadOnlyCollection<ID1HouseConsignment> GetHouseConsignments()
	{
		return header
			.Bills
			.Where(bill => !bill.IsCustomsStatusDeletionRequested && !bill.IsCustomsStatusDeleted)
			.Select(bill => new D1HouseConsignmentWrapper(bill, messageSendingWrapperFactory))
			.ToList()
			.AsReadOnly();
	}

	readonly NctsHeader header;
	readonly Lazy<ID1Consignment> lazyConsignment;
	readonly Lazy<IReadOnlyCollection<ID1HouseConsignment>> lazyHouseConsignments;
	readonly IMessageSendingWrapperFactory messageSendingWrapperFactory;
}
