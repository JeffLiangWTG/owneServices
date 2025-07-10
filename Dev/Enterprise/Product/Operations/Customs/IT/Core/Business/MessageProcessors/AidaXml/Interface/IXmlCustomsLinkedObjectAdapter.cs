using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.IT.Business.MessageSending.AidaXml.UniqueTransactionIdentifier;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.IT.Business;

public interface IXmlCustomsLinkedObjectAdapter : ICustomsLinkedObjectAdapter
{
	IEnumerable<CusEntryNumber> GetAllRelatedEntryNumbers();

	ZString CustomsOfficeOfPresentation { get; }

	IEnumerable<BusinessObject> GetAllEntryLines();

	bool IsAwaitingMessage { get; }

	bool IsDeposited { get; }

	void SetStatusAsError();

	void SetStatusAsAcknowledged();

	void SetStatusAsFailedForTransmission();

	void SetStatusAsAcceptedBySystem();

	void SetStatusAsRegistered(ZDateTime acceptanceDate);

	void SetStatusAsCleared(ZDateTime releaseDateTime);

	void SetStatusAsCancelled();

	void SetStatusAsDeposited();

	void SetStatusAsUnderControl();

	void SetStatusAsExitCompleted();

	void SetStatusAsGoodsWrittenOffClosed();

	void SetSentEntryLinesCount();

	IReadOnlyCollection<CusEntryPayInfo> GetAllPaymentInfo();

	void SetStatusAsAmended();

	CusEntryNumber UpdateOrInsertEntryNumber(ZString entryType,
		ZString entryNum,
		ZString entryLineReference,
		ZDateTime issueDate,
		int? lineNumber);

	IReadOnlyCollection<IFee> GetFeesForLine(int lineNumber, Func<IFee, bool> feeFilter);

	IReadOnlyCollection<IFee> GetAllFees(Func<IFee, bool> feeFilter);

	void AddA93Number(IUcc6A93NumberPayment paymentInfo);

	void SetCustomsChannel(string customsChannel);

	IUniqueTransactionIdentifierRequestContext GetUniqueTransactionIdentifierRequestContext(ZString uniqueTransactionID, ZString messageNumber);

	EDIMessage GetLastSuccessfullySentMessageForDepositedStatus();

	void UpdateOrInsertIvistoEntryNumber(ZDateTime exitDate, ZString exitOffice, ZString exitResult);

	void UpdateOrInsertIrildesEntryNumber(ZString departureOffice, ZDateTime writtenOffDate);

	EDIMessage GetOriginalSentMessageByUniqueTransactionIdentifier(ZString uniqueTransactionID);

	void ProcessBondedWarehouseIfRequired(ILoggingInformation logger, EDIMessage incomingMessage);

	void SetLocalReferenceNumber(string localReferenceNumber);

	IReadOnlyCollection<EDIMessage> Messages { get; }

	void AddEDoc(byte[] content, string filename, string documentType);

	void CreateIvistoRequestMessage();

	void CreateIrildesRequestMessage();
}
