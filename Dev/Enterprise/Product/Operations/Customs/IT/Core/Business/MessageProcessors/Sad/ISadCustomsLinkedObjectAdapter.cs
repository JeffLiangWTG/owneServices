using System.Collections.Generic;
using System.Collections.Immutable;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.IT.Messaging.MessageStructure.IRISP;

namespace Enterprise.Customs.IT.Business;

public interface ISadCustomsLinkedObjectAdapter : ICustomsLinkedObjectAdapter, ICustomsStatusLinkedObjectAdapter
{
	ZString EntryCustomsStatus { get; }
	void SetEntryCustomsStatus(ZString entryCustomsStatus);

	ISadCustomsStatusProvider StatusProvider { get; }

	ZBool IsImport { get; }
	ZBool IsExport { get; }

	ZBool IsEntryRegisteredOrNbRejected { get; }
	IEnumerable<ISadCustomsLineLinkedObjectAdapter> CustomsLines { get; }

	ISingleWindowRequestDataProvider SingleWindowRequestDataProvider { get; }

	void InsertOrUpdateA93Numbers(ISadPositiveResponseMessageA93EntryPayments entryPayments);
	IEnumerable<CusEntryNumber> GetEntryNumbers();
	CusEntryNumber GetNewCusEntryNumber();

	ZString Mrn { get; }
	ZBool IsIncomingMessageAlreadyLinked(ZString incomingMessageType);
	CusEntryNumber IrildesCusEntryNum { get; }
	CusEntryNumber IvistoCusEntryNum { get; }

	void SetEntryReleaseDate(ZDateTime releaseDateTime);

	void UpdatePendingGuaranteeTransactions(ZString transactionsNewStatus);
	void WriteOffGuarantee(ZString applicationId, ZDate transactionDate);
}

public interface ISadCustomsStatusProvider
{
	ZString AwaitingMessageStatus { get; }
	ZString AcknowledgedMessageStatus { get; }
	ZString ClearedMessageStatus { get; }
	ZString ErrorMessageStatus { get; }

	ZString RegisteredCustomsStatus { get; }
	ZString UnderControlCustomsStatus { get; }
	ZString ClearedCustomsStatus { get; }
	ZString NbRejectedCustomsStatus { get; }
	ZString ArrivalCustomsStatus { get; }

	ImmutableArray<CustomsStatusOrder> StatusWithInformationOrderCollection { get; }
}
