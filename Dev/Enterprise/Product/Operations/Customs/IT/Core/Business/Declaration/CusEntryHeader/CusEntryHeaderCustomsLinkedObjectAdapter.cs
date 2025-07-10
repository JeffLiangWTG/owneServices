using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Extensions;
using Enterprise.Customs.Business.MessageProcessors;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.IT.Business.MessageSending.AidaXml.Export;
using Enterprise.Customs.IT.Business.MessageSending.AidaXml.UniqueTransactionIdentifier;
using Enterprise.Customs.IT.Messaging.MessageStructure.IRISP;
using Enterprise.Customs.IT.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Argument = CargoWise.Common.Argument;

namespace Enterprise.Customs.IT.Business.Declaration;

public class CusEntryHeaderCustomsLinkedObjectAdapter : ISadCustomsLinkedObjectAdapter
	, ISingleWindowCustomsLinkedObjectAdapter
	, IXmlCustomsLinkedObjectAdapter
{
	public CusEntryHeaderCustomsLinkedObjectAdapter(CusEntryHeader entryHeader)
	{
		this.entryHeader = Argument.NotNull(entryHeader, nameof(entryHeader));
		declaration = Argument.NotNull(entryHeader.Declaration, nameof(entryHeader.Declaration));
	}

	readonly CusEntryHeader entryHeader;
	readonly JobDeclaration declaration;

	#region ISadCustomsLinkedObjectAdapter Members

	ZString ISadCustomsLinkedObjectAdapter.EntryCustomsStatus => entryHeader.CH_EntryStatus;

	ISadCustomsStatusProvider ISadCustomsLinkedObjectAdapter.StatusProvider => StatusProvider;

	ZBool ISadCustomsLinkedObjectAdapter.IsImport => declaration.IsImport;

	ZBool ISadCustomsLinkedObjectAdapter.IsExport => declaration.IsExport;

	ZBool ISadCustomsLinkedObjectAdapter.IsEntryRegisteredOrNbRejected => entryHeader.IsEntryStatusRegisteredOrNbRejected;

	IEnumerable<ISadCustomsLineLinkedObjectAdapter> ISadCustomsLinkedObjectAdapter.CustomsLines => customsLines ?? (customsLines = entryHeader.MergedLines.Cast<CusEntryLine>().Select(x => new CusEntryLineCustomsLineLinkedObjectAdapter(x)));
	IEnumerable<ISadCustomsLineLinkedObjectAdapter> customsLines;

	ISingleWindowRequestDataProvider ISadCustomsLinkedObjectAdapter.SingleWindowRequestDataProvider => entryHeader;

	ZString ISadCustomsLinkedObjectAdapter.Mrn => entryHeader.MovementReferenceNumber;

	CusEntryNumber ISadCustomsLinkedObjectAdapter.IrildesCusEntryNum => entryHeader.EntryNumbersProvider.Irildes;

	CusEntryNumber ISadCustomsLinkedObjectAdapter.IvistoCusEntryNum => entryHeader.EntryNumbersProvider.Ivisto;

	IEnumerable<CusEntryNumber> ISadCustomsLinkedObjectAdapter.GetEntryNumbers() => GetEntryNumbersCore();

	CusEntryNumber ISadCustomsLinkedObjectAdapter.GetNewCusEntryNumber() => GetNewCusEntryNumberCore();

	void ISadCustomsLinkedObjectAdapter.InsertOrUpdateA93Numbers(ISadPositiveResponseMessageA93EntryPayments entryPayments)
	{
		var entryPayInfoCollection = entryHeader.EntryPayInfos;
		var entryLineCollection = entryHeader.MergedLines;
		var registerCode = entryHeader.EntryNumbersProvider.RegistrationInfoWrapper.RegisterIncludingSeries;
		var a93Number = entryPayments.A93Number;

		InsertOrUpdateEntryPayInfo(a93Number, entryPayments.HasA93FirstPayment, entryPayments.FirstPaymentMethod, entryPayments.FirstPaymentDueDate, registerCode);
		InsertOrUpdateEntryPayInfo(a93Number, entryPayments.HasA93SecondPayment, entryPayments.SecondPaymentMethod, entryPayments.SecondPaymentDueDate, registerCode);
		InsertOrUpdateEntryPayInfo(a93Number, entryPayments.HasA93ThirdPayment, entryPayments.ThirdPaymentMethod, entryPayments.ThirdPaymentDueDate, registerCode);

		void InsertOrUpdateEntryPayInfo(ZString number, ZBool hasValidData, ZString methodOfPayment, ZDate paymentDueDate, ZString registry)
		{
			if (hasValidData)
			{
				entryPayInfoCollection.InsertOrUpdateEntryPayInfo(x => x.MethodOfPayment == methodOfPayment, GetFeesTotalAmount(entryLineCollection, methodOfPayment), registry, paymentDueDate, number, methodOfPayment);
			}
		}
	}

	ZDecimal GetFeesTotalAmount(ICusEntryLineCollection<CusEntryLine> entryLineCollection, ZString methodOfPayment)
		=> entryLineCollection.Cast<CusEntryLine>().Sum(x => x.Fees.GetTotalAmount(methodOfPayment));

	void ISadCustomsLinkedObjectAdapter.SetEntryReleaseDate(ZDateTime releaseDateTime)
	{
		entryHeader.CH_EntryReleaseDate = releaseDateTime;
	}

	ZBool ISadCustomsLinkedObjectAdapter.IsIncomingMessageAlreadyLinked(ZString incomingMessageType) => entryHeader.Messages.GetLastMessageByType(incomingMessageType) != null;

	void ISadCustomsLinkedObjectAdapter.SetEntryCustomsStatus(ZString entryCustomsStatus) => entryHeader.CH_EntryStatus = entryCustomsStatus;

	void ISadCustomsLinkedObjectAdapter.UpdatePendingGuaranteeTransactions(ZString transactionsNewStatus)
	{
	}

	void ISadCustomsLinkedObjectAdapter.WriteOffGuarantee(ZString applicationId, ZDate transactionDate)
	{
	}

	#endregion

	#region ISingleWindowCustomsLinkedObjectAdapter Members

	DocManagerInfo ISingleWindowCustomsLinkedObjectAdapter.DocManagerInfo => declaration.DocManagerInfo;

	void ISingleWindowCustomsLinkedObjectAdapter.AddLog(Event eventType, ZDateTime eventDate, KeyValuePair<string, string>[] eventAttributes) => entryHeader.Logs.AddNew(eventType, eventDate.ToOffset(), eventAttributes);

	void ISingleWindowCustomsLinkedObjectAdapter.SetEntryCustomsChannel(ZString entryCustomsChannel) => entryHeader.CustomsChannel = entryCustomsChannel;

	void ISingleWindowCustomsLinkedObjectAdapter.SetEntryAsCleared(ZDateTime releaseDateTime)
	{
		SetEntryAsCleared(releaseDateTime);
	}

	void ISingleWindowCustomsLinkedObjectAdapter.InsertOrUpdateReleaseCode(ZString releaseCode, ZDateTime releaseDate) => entryHeader.EntryNumbersProvider.InsertOrUpdateReleaseCode(releaseCode, releaseDate);

	ZBool ISingleWindowCustomsLinkedObjectAdapter.IsEntryCleared => entryHeader.CH_Status == ITMessageStatusList.Codes.ClearOriginal;

	#endregion

	#region ICustomsLinkedObjectAdapter Members

	ZGuid ICustomsLinkedObjectAdapter.PK => entryHeader.PK;

	void ICustomsLinkedObjectAdapter.AddMessage(EDIMessage message)
	{
		Argument.NotNull(message, nameof(message));
		entryHeader.Messages.Add(message);
	}

	EDIMessage ICustomsLinkedObjectAdapter.GetLastSuccessfullySentMessage()
	{
		var messages = entryHeader.Messages;

		if (declaration.IsUCC6)
		{
			return messages.GetLastSuccessfullySentMessageBySubType(entryHeader.EntryInstruction?.CEI_Style ?? ZString.Empty);
		}
		return messages.GetLastSuccessfullySentIdoc();
	}

	void ICustomsLinkedObjectAdapter.GenerateDocuments()
	{
	}

	ZString ICustomsLinkedObjectAdapter.EntryReferenceNumber => entryHeader.CH_BGMReference;

	ZString ICustomsLinkedObjectAdapter.JobReferenceNumber => declaration.JE_DeclarationReference;

	ZString ICustomsLinkedObjectAdapter.CustomsProfile => declaration.JE_CustomsProfile;

	#endregion

	#region ICustomsStatusLinkedObjectAdapter

	ZString ICustomsStatusLinkedObjectAdapter.MessageStatus => entryHeader.CH_Status;

	ZString ICustomsStatusLinkedObjectAdapter.AwaitingMessageStatus => StatusProvider.AwaitingMessageStatus;

	void ICustomsStatusLinkedObjectAdapter.SetMessageStatus(ZString status) => entryHeader.CH_Status = status;

	BusinessObjectFactory ICustomsLinkedObjectAdapter.Factory => entryHeader.Factory;

	#endregion

	#region IXmlCustomsLinkedObjectAdapter

	IEnumerable<CusEntryNumber> IXmlCustomsLinkedObjectAdapter.GetAllRelatedEntryNumbers() => GetEntryNumbersCore();

	IEnumerable<BusinessObject> IXmlCustomsLinkedObjectAdapter.GetAllEntryLines() => entryHeader.MergedLines;

	bool IXmlCustomsLinkedObjectAdapter.IsAwaitingMessage => entryHeader.CH_Status == ITMessageStatusList.Codes.AwaitingOriginal;

	bool IXmlCustomsLinkedObjectAdapter.IsDeposited => entryHeader.IsInDepositStatus;

	void IXmlCustomsLinkedObjectAdapter.SetStatusAsError()
	{
		SafeCustomsStatusSetter.TrySetErrorOriginal();
	}

	void IXmlCustomsLinkedObjectAdapter.SetStatusAsAcknowledged()
	{
		SafeCustomsStatusSetter.TrySetAcknowledged();
	}

	void IXmlCustomsLinkedObjectAdapter.SetStatusAsFailedForTransmission()
	{
		entryHeader.SetAsFailedFromTransmission();
	}

	void IXmlCustomsLinkedObjectAdapter.SetStatusAsRegistered(ZDateTime acceptanceDate)
	{
		SafeCustomsStatusSetter.TrySetRegistered();
	}

	void IXmlCustomsLinkedObjectAdapter.SetStatusAsCleared(ZDateTime releaseDateTime)
	{
		var isStatusChanged = SafeCustomsStatusSetter.TrySetCleared(releaseDateTime);

		if (isStatusChanged)
		{
			CreateIvistoRequestMessage();
			return;
		}

		var clearedStatus = declaration.IsImport ? ITEntryStatusList.Codes.ImportCleared : ITEntryStatusList.Codes.ExportCleared;
		entryHeader.Logs.AddNew(Events.CustomsEntryStatus, clearedStatus, ZDateTimeOffset.Now);
	}

	void IXmlCustomsLinkedObjectAdapter.SetStatusAsCancelled()
	{
		SafeCustomsStatusSetter.TrySetCancelled();
	}

	void IXmlCustomsLinkedObjectAdapter.SetStatusAsDeposited()
	{
		SafeCustomsStatusSetter.TrySetDeposited();
	}

	void IXmlCustomsLinkedObjectAdapter.SetStatusAsUnderControl()
	{
		SafeCustomsStatusSetter.TrySetUnderControl();
	}

	void IXmlCustomsLinkedObjectAdapter.SetStatusAsExitCompleted()
	{
		SafeCustomsStatusSetter.TrySetExitCompleted();
	}

	void IXmlCustomsLinkedObjectAdapter.SetStatusAsGoodsWrittenOffClosed()
	{
		throw new CustomsMessageProcessorException(
			FormattableString.Invariant($"Irildes is not supported by Entry Header [BGMReference: {entryHeader.CH_BGMReference}]"));
	}

	void IXmlCustomsLinkedObjectAdapter.SetSentEntryLinesCount()
	{
		entryHeader.ZG_SentEntryLinesCount = entryHeader.MergedLinesCount;
	}

	IReadOnlyCollection<CusEntryPayInfo> IXmlCustomsLinkedObjectAdapter.GetAllPaymentInfo()
		=> entryHeader.EntryPayInfos.ToCollection<CusEntryPayInfo>();

	void IXmlCustomsLinkedObjectAdapter.SetStatusAsAmended()
	{
		SafeCustomsStatusSetter.TrySetAmended();
	}

	ICustomsStatusSetter SafeCustomsStatusSetter => safeCustomsStatusSetter ?? (safeCustomsStatusSetter = new SafeCustomsStatusSetter(entryHeader));
	ICustomsStatusSetter safeCustomsStatusSetter;

	CusEntryNumber IXmlCustomsLinkedObjectAdapter.UpdateOrInsertEntryNumber(ZString entryType,
		ZString entryNum,
		ZString entryLineReference,
		ZDateTime issueDate,
		int? lineNumber)
	{
		return UpdateOrInsertEntryNumber(
			entryNumber => entryNumber.CE_EntryType == entryType && entryNumber.CE_EntryLineReference == entryLineReference,
			entryType,
			entryNum,
			entryLineReference,
			issueDate,
			lineNumber,
			ZString.Empty);
	}

	IReadOnlyCollection<IFee> IXmlCustomsLinkedObjectAdapter.GetFeesForLine(int lineNumber, Func<IFee, bool> feeFilter)
	{
		var line = GetCusEntryNumberParent(lineNumber) as CusEntryLine;
		if (line == null)
		{
			var exceptionMessage = Res.GetString("9A12C44C-F1FB-43D9-BCA6-1103F684EC8E", "Not able to find an Entry Line with a Line Number: {0}", lineNumber);
			throw new CustomsMessageProcessorException(exceptionMessage);
		}

		var lineFees = line.Fees.Cast<IFee>();
		if (feeFilter != null)
		{
			lineFees = lineFees.Where(feeFilter);
		}

		return lineFees.ToCollection();
	}

	IReadOnlyCollection<IFee> IXmlCustomsLinkedObjectAdapter.GetAllFees(Func<IFee, bool> feeFilter)
	{
		var fees = entryHeader.MergedLines
			.Select(line => line.Fees)
			.SelectMany(fee => fee)
			.Cast<IFee>();

		if (feeFilter != null)
		{
			fees = fees.Where(feeFilter);
		}

		return fees.ToCollection();
	}

	void IXmlCustomsLinkedObjectAdapter.AddA93Number(IUcc6A93NumberPayment paymentInfo)
	{
		Argument.NotNull(paymentInfo, nameof(paymentInfo));

		var registerIncludingSeries = entryHeader.EntryNumbersProvider.RegistrationInfoWrapper.RegisterIncludingSeries;
		var paymentAmount = paymentInfo.AmountCalculator?.CalculateAmount() ?? 0m;

		AddCusEntryPayInfo(
			responseNo: paymentInfo.PaymentResponseNo,
			paymentAmount: paymentAmount,
			transactionType: registerIncludingSeries,
			paymentStatus: Customs.Business.CusEntryPayInfoStatusList.Codes.Pending,
			paymentParty: paymentInfo.PaymentType,
			paymentDate: paymentInfo.PaymentDate);
	}

	void IXmlCustomsLinkedObjectAdapter.SetCustomsChannel(string customsChannel)
	{
		Argument.NotNullOrEmpty(customsChannel, nameof(customsChannel));

		entryHeader.CustomsChannel = customsChannel;
	}

	ZString IXmlCustomsLinkedObjectAdapter.CustomsOfficeOfPresentation => declaration.JE_CustomsOffice;

	IUniqueTransactionIdentifierRequestContext IXmlCustomsLinkedObjectAdapter.GetUniqueTransactionIdentifierRequestContext(ZString uniqueTransactionID, ZString messageNumber)
		=> new EntryHeaderUniqueTransactionIdentifierRequestContext(entryHeader, new GlbCertificateProvider(), uniqueTransactionID, messageNumber);

	EDIMessage IXmlCustomsLinkedObjectAdapter.GetLastSuccessfullySentMessageForDepositedStatus() => entryHeader.Messages.GetLastSuccessfullySentMessageForDepositedStatus();

	void IXmlCustomsLinkedObjectAdapter.SetStatusAsAcceptedBySystem()
	{
		SafeCustomsStatusSetter.TrySetAcceptedBySystem();
	}

	void IXmlCustomsLinkedObjectAdapter.UpdateOrInsertIvistoEntryNumber(ZDateTime exitDate, ZString exitOffice, ZString exitResult)
	{
		var ivistoEntryNumber = entryHeader.EntryNumbersProvider.Ivisto;
		if (ivistoEntryNumber is null || IsExitDateNewer(exitDate, ivistoEntryNumber.CE_IssueDate))
		{
			UpdateOrInsertEntryNumber(
				entryNumber => entryNumber.CE_EntryType == CusEntryNumberConstants.EntryTypes.Ivisto,
				CusEntryNumberConstants.EntryTypes.Ivisto,
				ZString.Empty,
				exitOffice,
				exitDate,
				null,
				exitResult);
		}

		bool IsExitDateNewer(ZDateTime newExitDate, ZDateTime currentExitDate)
			=> newExitDate.CompareTo(currentExitDate) > 0;
	}

	void IXmlCustomsLinkedObjectAdapter.UpdateOrInsertIrildesEntryNumber(ZString departureOffice, ZDateTime writtenOffDate)
	{
		throw new CustomsMessageProcessorException(
			FormattableString.Invariant($"Irildes is not supported by Entry Header [BGMReference: {entryHeader.CH_BGMReference}]"));
	}

	EDIMessage IXmlCustomsLinkedObjectAdapter.GetOriginalSentMessageByUniqueTransactionIdentifier(ZString uniqueTransactionID)
	{
		var messageCollection = entryHeader.Messages;
		var acknowledgmentMessage = messageCollection.GetFirstAcknowledgmentByUniqueTransactionID(uniqueTransactionID);
		var acknowledgmentInterchange = acknowledgmentMessage?.Interchange;

		if (acknowledgmentInterchange is null)
		{
			return null;
		}

		return messageCollection.GetFirstSentMessageBySessionGuid(acknowledgmentInterchange.EI_SessionGUID);
	}

	void IXmlCustomsLinkedObjectAdapter.ProcessBondedWarehouseIfRequired(ILoggingInformation logger, EDIMessage incomingMessage)
	{
		if (!entryHeader.BondedWarehouseProcessingRequired)
		{
			return;
		}

		var bondedWhsMsgProcessorCreator = new BondedWarehouseMessageProcessorCreator(logger, GetNewBondedWarehouseMessageProcessor, GetBondedWarehouseNotificationGroupPK);

		entryHeader.Factory.Saved -= bondedWhsMsgProcessorCreator.ProcessBondedWarehouseOnFactorySaved;
		entryHeader.Factory.Saved += bondedWhsMsgProcessorCreator.ProcessBondedWarehouseOnFactorySaved;

		BondedWarehouseEntryMessageProcessor GetNewBondedWarehouseMessageProcessor(Action<EmailDef, EDIMessage> sendEmail)
			=> new BondedWarehouseEntryMessageProcessor(incomingMessage.PK, null, sendEmail);

		Guid GetBondedWarehouseNotificationGroupPK(Guid companyPK, Guid branchPK, Guid departmentPK)
			=> ITCustomsDataRegistry.Instance.BondedWarehouseNotificationGroup.GetFallBackValueAtAllLevels(companyPK, branchPK, departmentPK);
	}

	void IXmlCustomsLinkedObjectAdapter.SetLocalReferenceNumber(string localReferenceNumber)
	{
		if (entryHeader.EntryInstruction == null || !declaration.IsImport)
		{
			return;
		}

		var previousDocument = entryHeader.EntryInstruction.PreviousDocuments.AddNew();
		previousDocument.CSI_Procedure = ImportPreviousDocumentProcedureList.Codes.NumeroLrn;
		previousDocument.CSI_Code = PreviousDocumentCodeList.Codes.AltriDocumenti;
		previousDocument.CSI_ReferenceNumber = localReferenceNumber;
	}

	IReadOnlyCollection<EDIMessage> IXmlCustomsLinkedObjectAdapter.Messages => entryHeader.Messages.Cast<EDIMessage>().ToCollection();

	public void AddEDoc(byte[] content, string filename, string documentType)
	{
		var docManagerInfo = entryHeader.DocManagerInfo();
		docManagerInfo.ForceToUseAnotherFactory(entryHeader.Factory);
		docManagerInfo.AddFileOrDocument(content, filename, documentType);
	}

	void IXmlCustomsLinkedObjectAdapter.CreateIvistoRequestMessage() => CreateIvistoRequestMessage();

	void IXmlCustomsLinkedObjectAdapter.CreateIrildesRequestMessage()
	{
		throw new CustomsMessageProcessorException("Irildes is not supported by Entry Header");
	}

	#endregion

	#region Implementation

	ISadCustomsStatusProvider StatusProvider => statusProvider ?? (statusProvider = new CusEntryHeaderCustomsStatusProvider(entryHeader));
	ISadCustomsStatusProvider statusProvider;

	IEnumerable<CusEntryNumber> GetEntryNumbersCore()
	{
		var entryNumberQuery = new ZQuery(CusEntryNumSchema.CE_ParentID, entryHeader.PK);
		entryNumberQuery.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, entryHeader.CountryCode);
		entryNumberQuery.OrderBy = CusEntryNumSchema.CE_SystemCreateTimeUtc.Name;
		return entryHeader.Factory.Load<CusEntryNumber>(entryNumberQuery);
	}

	CusEntryNumber GetNewCusEntryNumberCore(int? lineNumber = null, bool throwErrorOnMissingParent = false)
	{
		var entryNumber = entryHeader.Factory.New<CusEntryNumber>();

		var parent = GetCusEntryNumberParent(lineNumber);

		if (throwErrorOnMissingParent && parent is null)
		{
			var exceptionMessage = Res.GetString("15D5688D-7458-4F53-B3AD-6813162808E1", "Unable to create a new Entry Number");
			throw new CustomsMessageProcessorException(exceptionMessage);
		}

		entryNumber.CE_ParentID = parent.PK;
		entryNumber.CE_ParentTable = parent.TableName;
		entryNumber.CE_Category = CusEntryNumber.Categories.CustomsPermitClearanceNumber;
		entryNumber.CE_RN_NKCountryCode = entryHeader.CountryCode;
		return entryNumber;
	}

	BusinessObject GetCusEntryNumberParent(int? lineNumber)
	{
		return lineNumber is null
			? entryHeader
			: entryHeader.MergedLines.FindByLineNumber(lineNumber.Value);
	}

	void SetEntryAsCleared(ZDateTime releaseDateTime)
	{
		SafeCustomsStatusSetter.TrySetCleared(releaseDateTime);
	}

	CusEntryPayInfo AddCusEntryPayInfo(ZString responseNo, ZDecimal paymentAmount, ZString transactionType, ZString paymentStatus, ZString paymentParty, ZDateTime paymentDate)
	{
		var cusEntryPayInfo = entryHeader.EntryPayInfos.AddNew();
		cusEntryPayInfo.C9_IncomingPayResponseNo = responseNo;
		cusEntryPayInfo.C9_PaymentAmount = paymentAmount;
		cusEntryPayInfo.C9_TransactionType = transactionType;
		cusEntryPayInfo.C9_PaymentDate = paymentDate;
		cusEntryPayInfo.C9_PaymentReference = ZString.Empty;
		cusEntryPayInfo.C9_PaymentStatus = paymentStatus;
		cusEntryPayInfo.C9_PaymentParty = paymentParty;
		return cusEntryPayInfo;
	}

	CusEntryNumber UpdateOrInsertEntryNumber(Func<CusEntryNumber, bool> searchCriteria, ZString entryType, ZString entryNum, ZString entryLineReference, ZDateTime issueDate, int? lineNumber, ZString entryStatus)
	{
		Argument.NotNullOrEmpty(entryType, nameof(entryType));

		var entryNumbers = GetEntryNumbersCore();
		var entryNumber = entryNumbers.SingleOrDefault(searchCriteria);
		if (entryNumber is null)
		{
			entryNumber = GetNewCusEntryNumberCore(lineNumber, true);
			entryNumber.CE_EntryType = entryType;
		}

		entryNumber.CE_EntryNum = entryNum;
		entryNumber.CE_EntryLineReference = entryLineReference;
		entryNumber.CE_IssueDate = issueDate;
		entryNumber.CE_EntryStatus = entryStatus;
		return entryNumber;
	}

	void CreateIvistoRequestMessage()
	{
		if (!entryHeader.IsUCC6AndIsExport())
		{
			return;
		}

		var ivistoRequestMessageFactory = GetNewIvistoRequestMessageFactory();
		ivistoRequestMessageFactory.CreateIvistoRequestMessage(entryHeader, entryHeader.Factory);
	}

	protected virtual IvistoRequestMessageFactory GetNewIvistoRequestMessageFactory()
	{
		return new IvistoRequestMessageFactory();
	}

	#endregion
}
