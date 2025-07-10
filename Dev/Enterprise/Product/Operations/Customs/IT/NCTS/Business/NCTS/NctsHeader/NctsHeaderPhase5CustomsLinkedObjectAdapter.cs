using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Extensions;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Customs.IT.Business;
using Enterprise.Customs.IT.Business.MessageSending.AidaXml.UniqueTransactionIdentifier;
using Enterprise.Customs.IT.NCTS.Business.MessageSending.AidaXml;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;
using Argument = CargoWise.Common.Argument;

namespace Enterprise.Customs.IT.NCTS.Business;

class NctsHeaderPhase5CustomsLinkedObjectAdapter : IXmlCustomsLinkedObjectAdapter, IGuaranteeTransactionSupporter
{
	public NctsHeaderPhase5CustomsLinkedObjectAdapter(NctsHeader header)
	{
		this.header = Argument.NotNull(header, nameof(header));
		movementHeader = Argument.NotNull(header.MovementHeader, nameof(header.MovementHeader));
	}

	#region ICustomsLinkedObjectAdapter

	ZGuid ICustomsLinkedObjectAdapter.PK => header.PK;

	ZString ICustomsLinkedObjectAdapter.EntryReferenceNumber => header.BH_JobReference;

	ZString ICustomsLinkedObjectAdapter.JobReferenceNumber => header.BH_JobReference;

	BusinessObjectFactory ICustomsLinkedObjectAdapter.Factory => header.Factory;

	void ICustomsLinkedObjectAdapter.AddMessage(EDIMessage message)
	{
		movementHeader.Messages.Add(message);
	}

	void ICustomsLinkedObjectAdapter.GenerateDocuments()
	{
	}

	EDIMessage ICustomsLinkedObjectAdapter.GetLastSuccessfullySentMessage() => null;

	ZString ICustomsLinkedObjectAdapter.CustomsProfile => header.BH_CustomsProfile;

	#endregion

	#region IXmlCustomsLinkedObjectAdapter

	bool IXmlCustomsLinkedObjectAdapter.IsAwaitingMessage => movementHeader.BM_MessageStatus == NctsMessageStatusList.Codes.SentToCustoms;

	ZString IXmlCustomsLinkedObjectAdapter.CustomsOfficeOfPresentation => (header as IAutHeaderWithCusOfficeProvider)?.CustomsOffice ?? ZString.Empty;

	bool IXmlCustomsLinkedObjectAdapter.IsDeposited => false;

	void IXmlCustomsLinkedObjectAdapter.AddA93Number(IUcc6A93NumberPayment paymentInfo)
	{
	}

	IEnumerable<BusinessObject> IXmlCustomsLinkedObjectAdapter.GetAllEntryLines() => header.GetGoodsItems();

	IReadOnlyCollection<IFee> IXmlCustomsLinkedObjectAdapter.GetAllFees(Func<IFee, bool> feeFilter) => Array.Empty<IFee>();

	IEnumerable<CusEntryNumber> IXmlCustomsLinkedObjectAdapter.GetAllRelatedEntryNumbers() => GetEntryNumbers();

	IReadOnlyCollection<IFee> IXmlCustomsLinkedObjectAdapter.GetFeesForLine(int lineNumber, Func<IFee, bool> feeFilter) => Array.Empty<IFee>();

	EDIMessage IXmlCustomsLinkedObjectAdapter.GetLastSuccessfullySentMessageForDepositedStatus() => null;

	EDIMessage IXmlCustomsLinkedObjectAdapter.GetOriginalSentMessageByUniqueTransactionIdentifier(ZString uniqueTransactionID)
	{
		var messageCollection = header.MovementHeader.Messages;
		var acknowledgmentMessage = messageCollection.GetFirstAcknowledgmentByUniqueTransactionID(uniqueTransactionID);
		var acknowledgmentInterchange = acknowledgmentMessage?.Interchange;

		return acknowledgmentInterchange switch
		{
			null => null,
			_ => messageCollection.GetFirstSentMessageBySessionGuid(acknowledgmentInterchange.EI_SessionGUID)
		};
	}

	IUniqueTransactionIdentifierRequestContext IXmlCustomsLinkedObjectAdapter.GetUniqueTransactionIdentifierRequestContext(ZString uniqueTransactionID, ZString messageNumber)
		=> new NctsUniqueTransactionIdentifierRequestContext(header, new GlbCertificateProvider(), uniqueTransactionID, messageNumber);

	void IXmlCustomsLinkedObjectAdapter.SetCustomsChannel(string customsChannel)
	{
		movementHeader.BM_ControlChannel = customsChannel;
	}

	void IXmlCustomsLinkedObjectAdapter.SetStatusAsCleared(ZDateTime releaseDateTime)
	{
		if (SafeCustomsStatusSetter.TrySetCleared(releaseDateTime))
		{
			GetNewIrildesRequestMessageFactory().CreateMessage(header);
		}
	}

	void IXmlCustomsLinkedObjectAdapter.SetStatusAsAmended()
	{
		_ = SafeCustomsStatusSetter.TrySetAmended();
		movementHeader.BM_Phase = NctsMovementHeaderTransactionStatusList.Codes.Declaration;

		header.Bills
			.Where(x => x.IsCustomsStatusDeletionRequested)
			.ForEach(x => x.B0_BillStatus = NctsDeletionStatusList.Codes.Deleted);

		header.GetGoodsItems()
			.Cast<NctsDepartureCargoDesc>()
			.Where(x => x.IsCustomsStatusDeletionRequested)
			.ForEach(x => x.BY_Status = NctsDeletionStatusList.Codes.Deleted);
	}

	void IXmlCustomsLinkedObjectAdapter.SetStatusAsCancelled()
	{
		_ = SafeCustomsStatusSetter.TrySetCancelled();
	}

	void IXmlCustomsLinkedObjectAdapter.SetStatusAsDeposited()
	{
	}

	void IXmlCustomsLinkedObjectAdapter.SetStatusAsExitCompleted()
	{
	}

	void IXmlCustomsLinkedObjectAdapter.SetStatusAsGoodsWrittenOffClosed()
	{
		_ = SafeCustomsStatusSetter.TrySetGoodsWrittenOffClosed();
	}

	void IXmlCustomsLinkedObjectAdapter.SetStatusAsRegistered(ZDateTime acceptanceDate)
	{
		_ = SafeCustomsStatusSetter.TrySetRegistered();

		if (movementHeader.BM_EntryDate.IsEmpty)
		{
			movementHeader.BM_EntryDate = acceptanceDate;
		}
	}

	void IXmlCustomsLinkedObjectAdapter.SetStatusAsUnderControl()
	{
		_ = SafeCustomsStatusSetter.TrySetUnderControl();
	}

	void IXmlCustomsLinkedObjectAdapter.SetStatusAsAcceptedBySystem()
	{
		_ = SafeCustomsStatusSetter.TrySetAcceptedBySystem();
	}

	void IXmlCustomsLinkedObjectAdapter.SetStatusAsAcknowledged()
	{
		_ = SafeCustomsStatusSetter.TrySetAcknowledged();
	}

	void IXmlCustomsLinkedObjectAdapter.SetStatusAsError()
	{
		_ = SafeCustomsStatusSetter.TrySetErrorOriginal();

		if (header.IsLocked)
		{
			header.UnlockFile(Res.GetString("8E11C54B-36B7-41B4-B39D-3797BCFA363D", "Response message error"));
		}
	}

	void IXmlCustomsLinkedObjectAdapter.SetStatusAsFailedForTransmission()
	{
		header.MovementHeader.BM_MessageStatus = LogicalStatusList.Codes.Failed;

		if (header.IsLocked)
		{
			header.UnlockFile(Res.GetString("9591E016-C3E4-4D75-B81E-58D81E7423B9", "Response message error failed for transmission"));
		}
	}

	void IXmlCustomsLinkedObjectAdapter.SetSentEntryLinesCount()
	{
		// No action needed for NCTS; kept blank.
	}

	void IXmlCustomsLinkedObjectAdapter.CreateIvistoRequestMessage()
	{
		throw new CustomsMessageProcessorException("Ivisto is not supported by NCTS");
	}

	void IXmlCustomsLinkedObjectAdapter.CreateIrildesRequestMessage()
	{
		GetNewIrildesRequestMessageFactory().CreateMessage(header);
	}

	CusEntryNumber IXmlCustomsLinkedObjectAdapter.UpdateOrInsertEntryNumber(ZString entryType, ZString entryNum, ZString entryLineReference, ZDateTime issueDate, int? lineNumber)
	{
		return UpdateOrInsertEntryNumber(
			entryNumber => entryNumber.CE_EntryType == entryType && entryNumber.CE_EntryLineReference == entryLineReference,
			entryType,
			entryNum,
			entryLineReference,
			issueDate,
			ZString.Empty);
	}

	void IXmlCustomsLinkedObjectAdapter.UpdateOrInsertIvistoEntryNumber(ZDateTime exitDate, ZString exitOffice, ZString exitResult)
	{
	}

	void IXmlCustomsLinkedObjectAdapter.UpdateOrInsertIrildesEntryNumber(ZString departureOffice, ZDateTime writtenOffCloseDate)
	{
		UpdateOrInsertEntryNumber(x => x.CE_EntryType == EDIMessageTypeList.Codes.IrildesRequest,
			EDIMessageTypeList.Codes.IrildesRequest,
			entryNum: ZString.Empty,
			departureOffice,
			writtenOffCloseDate,
			NCTS5DepartureCustomsStatusList.Codes.GoodsWrittenOffClosed);
	}

	IReadOnlyCollection<IT.Business.Declaration.CusEntryPayInfo> IXmlCustomsLinkedObjectAdapter.GetAllPaymentInfo() => [];

	void IXmlCustomsLinkedObjectAdapter.ProcessBondedWarehouseIfRequired(Enterprise.Messaging.Integration.ILoggingInformation logger, EDIMessage incomingMessage)
	{
		// No action needed for NCTS; kept blank.
	}

	void IXmlCustomsLinkedObjectAdapter.SetLocalReferenceNumber(string localReferenceNumber)
	{
	}

	IReadOnlyCollection<EDIMessage> IXmlCustomsLinkedObjectAdapter.Messages => movementHeader.Messages.Cast<EDIMessage>().ToCollection();

	void IXmlCustomsLinkedObjectAdapter.AddEDoc(byte[] content, string filename, string documentType)
	{
		var docManagerInfo = header.DocManagerInfo;
		docManagerInfo.ForceToUseAnotherFactory(header.Factory);
		docManagerInfo.AddFileOrDocument(content, filename, documentType);
	}

	#endregion

	#region IGuaranteeTransactionSupporter

	void IGuaranteeTransactionSupporter.DeletePendingTransactions(ZString applicationId)
	{
		var transactionProcessor = new NctsHeaderGuaranteeTransactionProcessor(header);
		transactionProcessor.DeletePendingTransactions(applicationId);
	}

	void IGuaranteeTransactionSupporter.ConfirmPendingTransactions(ZString applicationId)
	{
		var transactionProcessor = new NctsHeaderGuaranteeTransactionProcessor(header);
		transactionProcessor.ConfirmPendingTransactions(applicationId);
	}

	#endregion

	#region Implementation

	CusEntryNumber UpdateOrInsertEntryNumber(Func<CusEntryNumber, bool> searchCriteria, ZString entryType, ZString entryNum, ZString entryLineReference, ZDateTime issueDate, ZString entryStatus)
	{
		var entryNumbers = GetEntryNumbers();
		var entryNumber = entryNumbers.SingleOrDefault(searchCriteria);
		if (entryNumber is null)
		{
			entryNumber = CreateNewCusEntryNumber();
			entryNumber.CE_EntryType = entryType;
		}

		entryNumber.CE_EntryNum = entryNum;
		entryNumber.CE_EntryLineReference = entryLineReference;
		entryNumber.CE_IssueDate = issueDate;
		entryNumber.CE_EntryStatus = entryStatus;
		return entryNumber;
	}

	CusEntryNumber CreateNewCusEntryNumber()
	{
		var entryNumber = header.Factory.New<CusEntryNumber>();
		var parent = (BusinessObject)header;
		entryNumber.CE_ParentID = parent.PK;
		entryNumber.CE_ParentTable = parent.TableName;
		entryNumber.CE_Category = CusEntryNumber.Categories.CustomsPermitClearanceNumber;
		entryNumber.CE_RN_NKCountryCode = header.CountryCode;
		return entryNumber;
	}

	IEnumerable<CusEntryNumber> GetEntryNumbers()
	{
		var entryNumberQuery = new ZQuery(CusEntryNumSchema.CE_ParentID, header.PK);
		entryNumberQuery.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, header.CountryCode);
		entryNumberQuery.OrderBy = CusEntryNumSchema.CE_SystemCreateTimeUtc.Name;
		return header.Factory.Load<CusEntryNumber>(entryNumberQuery);
	}

	ICustomsStatusSetter SafeCustomsStatusSetter => safeCustomsStatusSetter ??= new NctsDepartureSafeCustomsStatusSetter(movementHeader);

	ICustomsStatusSetter safeCustomsStatusSetter;

	#endregion

	protected virtual IrildesRequestMessageFactory GetNewIrildesRequestMessageFactory()
	{
		return new IrildesRequestMessageFactory();
	}

	readonly NctsHeader header;
	readonly NctsDepartureMovementHeader movementHeader;
}
