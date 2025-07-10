using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using Enterprise.Customs.IT.Business;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.IT.Business.MessageSending.AidaXml.UniqueTransactionIdentifier;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.IT.TemporaryStorage.Business;

class TemporaryStorageCustomsLinkedObjectAdapter : IXmlCustomsLinkedObjectAdapter, ITemporaryStorageRegisterSupporter
{
	public TemporaryStorageCustomsLinkedObjectAdapter(TemporaryStorageHeader header)
	{
		this.header = CargoWise.Common.Argument.NotNull(header, nameof(header));
		this.registerSupporter = new TemporaryStorageRegisterSupporter(header);
	}

	#region ICustomsLinkedObjectAdapter

	ZGuid ICustomsLinkedObjectAdapter.PK => header.PK;

	ZString ICustomsLinkedObjectAdapter.EntryReferenceNumber => header.AMA_JobReference;

	ZString ICustomsLinkedObjectAdapter.JobReferenceNumber => header.AMA_JobReference;

	BusinessObjectFactory ICustomsLinkedObjectAdapter.Factory => header.Factory;

	public ZString CustomsOfficeOfPresentation => ZString.Empty;

	public bool IsAwaitingMessage => header.AMA_MessageStatus == PNTSMessageStatusList.Codes.Sent;

	public bool IsDeposited => false;

	public IReadOnlyCollection<EDIMessage> Messages => header.Messages.Cast<EDIMessage>().ToCollection();

	void ICustomsLinkedObjectAdapter.GenerateDocuments()
	{
	}

	ZString ICustomsLinkedObjectAdapter.CustomsProfile => header.AMA_CustomsProfile;

	#endregion ICustomsLinkedObjectAdapter

	#region IXmlCustomsLinkedObjectAdapter

	public IEnumerable<CusEntryNumber> GetAllRelatedEntryNumbers()
	{
		throw new NotImplementedException();
	}

	public IEnumerable<BusinessObject> GetAllEntryLines() => header.Bills;

	public void SetStatusAsError() => header.AMA_MessageStatus = PNTSMessageStatusList.Codes.FunctionalRejection;

	public void SetStatusAsAcknowledged() => header.AMA_MessageStatus = PNTSMessageStatusList.Codes.Acknowledged;

	public void SetStatusAsFailedForTransmission() => header.AMA_MessageStatus = PNTSMessageStatusList.Codes.TechnicalFailure;

	public void SetStatusAsAcceptedBySystem() => header.CustomsStatus = PNTSCustomsStatusList.Codes.FullyActivated;

	public void SetStatusAsRegistered(ZDateTime acceptanceDate)
	{
		header.SetCustomsStatusAsRegistered(acceptanceDate);
	}

	public void SetStatusAsCleared(ZDateTime releaseDateTime)
	{
		throw new NotImplementedException();
	}

	public void SetStatusAsCancelled()
	{
		throw new NotImplementedException();
	}

	public void SetStatusAsDeposited()
	{
		throw new NotImplementedException();
	}

	public void SetStatusAsUnderControl()
	{
		throw new NotImplementedException();
	}

	public void SetStatusAsExitCompleted()
	{
		throw new NotImplementedException();
	}

	public void SetStatusAsGoodsWrittenOffClosed()
	{
		throw new NotImplementedException();
	}

	public void SetSentEntryLinesCount()
	{
		throw new NotImplementedException();
	}

	public IReadOnlyCollection<CusEntryPayInfo> GetAllPaymentInfo()
	{
		throw new NotImplementedException();
	}

	public void SetStatusAsAmended()
	{
		throw new NotImplementedException();
	}

	public CusEntryNumber UpdateOrInsertEntryNumber(ZString entryType, ZString entryNum, ZString entryLineReference, ZDateTime issueDate, int? lineNumber)
	{
		var parentBill = header.Bills.FirstOrDefault(b => b.Lrn == entryLineReference);

		if (parentBill is null)
		{
			var exceptionMessage = Res.GetString("2BBD82BC-9CAD-4E2F-A224-244B406BDD53", "Unable to identify a parent bill");
			throw new CustomsMessageProcessorException(exceptionMessage);
		}

		if (lineNumber is null)
		{
			return CreateCusEntryNumber(parentBill, entryType, entryNum, issueDate);
		}

		var parentItem = parentBill.PackedItems.FirstOrDefault(i => i.API_LineNo == lineNumber);

		if (parentItem is null)
		{
			var exceptionMessage = Res.GetString("612761A5-13DD-4505-8C8C-D28AAC739F2C", "Unable to identify a parent packed item");
			throw new CustomsMessageProcessorException(exceptionMessage);
		}

		return CreateCusEntryNumber(parentItem, entryType, entryNum, issueDate);
	}

	public IReadOnlyCollection<IFee> GetFeesForLine(int lineNumber, Func<IFee, bool> feeFilter)
	{
		throw new NotImplementedException();
	}

	public IReadOnlyCollection<IFee> GetAllFees(Func<IFee, bool> feeFilter)
	{
		throw new NotImplementedException();
	}

	public void AddA93Number(IUcc6A93NumberPayment paymentInfo)
	{
		throw new NotImplementedException();
	}

	public void SetCustomsChannel(string customsChannel)
	{
		throw new NotImplementedException();
	}

	public IUniqueTransactionIdentifierRequestContext GetUniqueTransactionIdentifierRequestContext(ZString uniqueTransactionID, ZString messageNumber)
		=> new TemporaryStorageHeaderUniqueTransactionIdentifierRequestContext(header, new GlbCertificateProvider(), uniqueTransactionID, messageNumber);

	public EDIMessage GetLastSuccessfullySentMessageForDepositedStatus()
	{
		throw new NotImplementedException();
	}

	public void UpdateOrInsertIvistoEntryNumber(ZDateTime exitDate, ZString exitOffice, ZString exitResult)
	{
		throw new NotImplementedException();
	}

	public void UpdateOrInsertIrildesEntryNumber(ZString departureOffice, ZDateTime writtenOffDate)
	{
		throw new NotImplementedException();
	}

	public EDIMessage GetOriginalSentMessageByUniqueTransactionIdentifier(ZString uniqueTransactionID)
	{
		throw new NotImplementedException();
	}

	public void ProcessBondedWarehouseIfRequired(Messaging.Integration.ILoggingInformation logger, EDIMessage incomingMessage)
	{
	}

	public void SetLocalReferenceNumber(string localReferenceNumber)
	{
		throw new NotImplementedException();
	}

	public void AddEDoc(byte[] content, string filename, string documentType)
	{
		throw new NotImplementedException();
	}

	public void AddMessage(EDIMessage message)
	{
		header.Messages.Add(message);
	}

	public EDIMessage GetLastSuccessfullySentMessage()
	{
		throw new NotImplementedException();
	}

	void IXmlCustomsLinkedObjectAdapter.CreateIvistoRequestMessage()
	{
		throw new CustomsMessageProcessorException("Ivisto is not supported by Temporary Storage");
	}

	void IXmlCustomsLinkedObjectAdapter.CreateIrildesRequestMessage()
	{
		throw new CustomsMessageProcessorException("Irildes is not supported by Temporary Storage");
	}

	#endregion IXmlCustomsLinkedObjectAdapter

	CusEntryNumber CreateCusEntryNumber(BusinessObject parent, ZString entryType, ZString entryNum, ZDateTime issueDate)
	{
		var entryNumber = header.Factory.New<CusEntryNumber>();
		entryNumber.CE_ParentID = parent.PK;
		entryNumber.CE_ParentTable = parent.TableName;
		entryNumber.CE_EntryType = entryType;
		entryNumber.CE_EntryNum = entryNum;
		entryNumber.CE_Category = CusEntryNumber.Categories.CustomsPermitClearanceNumber;
		entryNumber.CE_IssueDate = issueDate;
		entryNumber.CE_RN_NKCountryCode = header.AMA_RN_NKCountry;
		return entryNumber;
	}

	void ITemporaryStorageRegisterSupporter.CreateRegisterTransactionsForBill(ZString movementReferenceNumber)
	{
		registerSupporter.CreateRegisterTransactionsForBill(movementReferenceNumber);
	}

	readonly TemporaryStorageHeader header;
	readonly TemporaryStorageRegisterSupporter registerSupporter;
}
