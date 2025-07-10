using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.ES.MessageDefinitions;
using CargoWise.Customs.ES.MessageDefinitions.Version1;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business;
using Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.EDIMessages;
using Enterprise.Customs.ES.Business.MessageSending;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.Customs.ES.Messaging.MessageProcessors;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using static Enterprise.Customs.ES.Business.MessageProcessorConstants;
using static Enterprise.Customs.ES.Business.MessageSending.ESMessageSender;
using CusEntryHeader = Enterprise.Customs.ES.Business.Declaration.CusEntryHeader;

namespace Enterprise.Customs.ES.Business
{
	public abstract class XMLResponseMessageProcessor<TResponseProvider, TPrettyMessage> : ESResponseMessageProcessor<TResponseProvider>
		where TResponseProvider : class, ICommonServiceSegment, IResponseCode
		where TPrettyMessage : IMessagePrettyFormatter
	{
		protected XMLResponseMessageProcessor(LoggingInformation logger, IEDocsDelayedSaver eDocsSaver = null) : base(logger, eDocsSaver)
		{
		}

		protected abstract ZString XsdSchemaEmbeddedResourceName { get; }
		protected abstract ZString AcceptedResponseCode { get; }
		protected virtual ZBool CanTriggerAnnexSending => false;
		protected virtual ZBool IsOnlyAcceptedDeclaration => false;
		protected virtual ZBool IsAcceptedDeclaration(TResponseProvider response) => response.ResponseCode == AcceptedResponseCode;

		protected sealed override void ProcessMessageCore(EDIMessage message, CusEntryHeader linkedBusinessObject, TResponseProvider provider)
		{
			var serviceSegmentId = (ZString)provider.ServiceSegmentId;
			if (!serviceSegmentId.IsEmpty)
			{
				message.EM_MessageNum = serviceSegmentId.Left(AutoEDIMessage.Schema.EM_MessageNumMaxLength);
			}

			var messagePrettyFormatter = GetNewMessagePrettyFormatter(provider, message, linkedBusinessObject);

			SetMrnIfApplicable(linkedBusinessObject, provider, message);

			ProcessDeclarationCore(provider, message, linkedBusinessObject, messagePrettyFormatter);

			if (!IsAnnexMessage(message))
			{
				SetMessageStatusAsReceived(message);
				if (!CanTriggerAnnexSending)
				{
					SetCHStatusAsReceived(linkedBusinessObject);
				}
			}
		}

		void ProcessDeclarationCore(TResponseProvider response, EDIMessage message, CusEntryHeader entryHeader, TPrettyMessage messagePrettyFormatter)
		{
			RemoveCusPollingTransactionsIfNeeded(entryHeader.Factory, MessageTypesToInclude, entryHeader.MovementReferenceNumber);

			if (IsOnlyAcceptedDeclaration || IsAcceptedDeclaration(response))
			{
				var extraDataFromProcessing = ProcessAcceptedDeclaration(response, message, entryHeader);
				message.EM_MessageInterpretation = messagePrettyFormatter.CreateMessageDetailsAccepted(extraDataFromProcessing);
				SetMessageSubTypeAsAccepted(message);
			}
			else
			{
				ProcessRejectedDeclaration(response, message, entryHeader);
				message.EM_MessageInterpretation = messagePrettyFormatter.CreateMessageDetailsRejected();
				SetMessageSubTypeAsRejected(message);
				if (CanTriggerAnnexSending)
				{
					SetCHStatusAsReceived(entryHeader);
				}
			}
		}

		void SetMrnIfApplicable(CusEntryHeader entryHeader, TResponseProvider response, EDIMessage message)
		{
			if (ShouldSetMovementReferenceNumber(message) && response is IMRNField mrnResponse && !string.IsNullOrEmpty(mrnResponse.MRN) && entryHeader.MovementReferenceNumber.IsEmpty)
			{
				entryHeader.MovementReferenceNumberSetter(mrnResponse.MRN);
			}
		}

		protected abstract TPrettyMessage GetNewMessagePrettyFormatter(TResponseProvider response, EDIMessage message, CusEntryHeader entryHeader);
		protected abstract ZString ProcessAcceptedDeclaration(TResponseProvider response, EDIMessage message, CusEntryHeader entryHeader);
		protected virtual void ProcessRejectedDeclaration(TResponseProvider response, EDIMessage message, CusEntryHeader entryHeader) { }

		protected override TResponseProvider GetMessageProviderCore(EDIMessage message)
		{
			return DererializeMessage(message, XsdSchemaEmbeddedResourceName);
		}

		protected virtual TResponseProvider DererializeMessage(EDIMessage message, string xsdSchemaEmbeddedResourceName)
		{
			using (var textReader = message.GetEM_MessageTextReader())
			using (var bodyTextReader = XMLResponseMessageHelper.GetXmlBody(textReader))
			{
				return ESXmlObjectSerializer.DeserializeWithValidation<TResponseProvider>(xsdSchemaEmbeddedResourceName, bodyTextReader);
			}
		}

		protected void ResetGuaranteesAmountAndAddTransactionsForAcceptedDeclaration(TResponseProvider response, CusEntryHeader entryHeader, ZString transactionCommentPrefix, Func<TResponseProvider, ZString, ZDecimal[]> getDebtAmountArray)
		{
			var transactionCommentSuffix = Res.GetString("7a94f59d-06b1-462c-8367-84bf6ca16e3a", "(Customs Adj)");
			ResetGuaranteesAmountAndAddTransactionsCommon(entryHeader, transactionCommentPrefix, transactionCommentSuffix, false, response: response, getDebtAmountArray: getDebtAmountArray);
		}

		protected void ResetGuaranteesAmountAndAddTransactionsForCancellation(CusEntryHeader entryHeader, ZString transactionCommentPrefix)
		{
			var transactionCommentSuffix = Res.GetString("e7cb04cd-6b54-481a-a0ae-4e8a1f7967da", "(Canceled)");
			ResetGuaranteesAmountAndAddTransactionsCommon(entryHeader, transactionCommentPrefix, transactionCommentSuffix, true);
		}

		void ResetGuaranteesAmountAndAddTransactionsCommon(CusEntryHeader entryHeader, ZString transactionCommentPrefix, ZString transactionCommentSuffix, ZBool isCancellation, TResponseProvider response = null, Func<TResponseProvider, ZString, ZDecimal[]> getDebtAmountArray = null)
		{
			var guaranteesInDeclaration = entryHeader.Declaration.Guarantees.Where(x => ((ESGuarantee)x).EntryInstruction == entryHeader.EntryInstruction);

			foreach (ESGuarantee guarantee in guaranteesInDeclaration)
			{
				var reference = guarantee.PW_BondNumber;

				var debtAmountArray = getDebtAmountArray == null || response == null ? new ZDecimal[] { ZDecimal.Zero } : getDebtAmountArray(response, reference);

				var newAmount = debtAmountArray.Sum(x => x);
				guarantee.PW_BondAmount = newAmount;

				var guaranteeHeader = CusGuaranteeHeaderHelper.LoadCusGuaranteeHeaderFromReference(entryHeader.Factory, guarantee.PW_BondNumber, entryHeader.CountryCode, EUGuaranteeTypeList.Codes.IMP);
				if (guaranteeHeader != null)
				{
					AddTransactionsToGuarantee(entryHeader, guaranteeHeader, transactionCommentPrefix, transactionCommentSuffix, newAmount, debtAmountArray, isCancellation);
				}
			}
		}

		void AddTransactionsToGuarantee(CusEntryHeader entryHeader, CusGuaranteeHeader guaranteeHeader, ZString transactionCommentPrefix, ZString transactionCommentSuffix, ZDecimal newAmount, ZDecimal[] debtAmountArray, ZBool isCancellation)
		{
			var transactionsAmount = GetTransactionsAmount(guaranteeHeader, entryHeader);

			var entryReference = entryHeader.CH_BGMReference;

			if (transactionsAmount < 0)
			{
				var absTransactionsAmount = Math.Abs(transactionsAmount);
				var tranValue = newAmount != 0 ? -(newAmount - absTransactionsAmount) : absTransactionsAmount;
				AddGuaranteeTransaction(entryHeader, guaranteeHeader, transactionCommentPrefix + entryReference + transactionCommentSuffix, tranValue);
			}

			if (!isCancellation && transactionsAmount == 0 && !guaranteeHeader.GetTransactions().Any(x => x.CPL_TransactionStatus == PermitTransactionStatusList.Codes.Confirmed && (x.CPL_Reference == entryHeader.MovementReferenceNumber || x.CPL_Reference == entryHeader.CH_BGMReference)))
			{
				var debtReference = transactionCommentPrefix + entryReference;
				debtAmountArray.ForEach(x => AddDebtTransactionIfNotZero(entryHeader, guaranteeHeader, debtReference, x));
			}
		}

		decimal GetTransactionsAmount(CusGuaranteeHeader guaranteeHeader, CusEntryHeader entryHeader) => guaranteeHeader.GetTransactions()?.Cast<SharedCusPermitLineTransaction>().Where(x => x.CPL_TransactionStatus == PermitTransactionStatusList.Codes.Confirmed && (x.CPL_Reference == entryHeader.MovementReferenceNumber || x.CPL_Reference == entryHeader.CH_BGMReference)).Sum(x => x.CPL_TranValue) ?? ZDecimal.Zero;

		void AddDebtTransactionIfNotZero(CusEntryHeader entryHeader, CusGuaranteeHeader guaranteeHeader, string reference, decimal debt)
		{
			if (debt != 0)
			{
				AddGuaranteeTransaction(entryHeader, guaranteeHeader, reference, -debt);
			}
		}

		void AddGuaranteeTransaction(CusEntryHeader entryHeader, CusGuaranteeHeader guaranteeHeader, string reference, decimal tranValue)
		{
			if (guaranteeHeader.HasOpeningBalanceTransaction)
			{
				var mrnCode = entryHeader.MovementReferenceNumber;
				var acceptanceDate = entryHeader.MovementReferenceNumberIssueDate;
				var tranDate = acceptanceDate.IsEmpty ? ZDateTime.Today : acceptanceDate;

				guaranteeHeader.AddTransaction(mrnCode, reference, "", "", tranValue, ZDecimal.Zero, status: PermitTransactionStatusList.Codes.Confirmed, transactionDate: tranDate, checkBursting: true);
			}
		}

		protected ZBool ShouldChangeCHStatusForAnnexes(CusEntryHeader entryHeader)
		{
			var chStatus = entryHeader.CH_Status;
			return !entryHeader.HasAnnexesSentWithoutResponse() && chStatus != EDIMessageStatusList.Codes.Failed && chStatus != EDIMessageStatusList.Codes.Rejected;
		}

		protected void ProcessRejectedDeclarationForAnnexAESAndT2LPOUS(EDIMessage message, CusEntryHeader entryHeader)
		{
			ProcessRejectedDeclarationForAnnexes(message, entryHeader);
			entryHeader.ZG_RequestDispatch = ZString.Empty;
		}

		protected List<ESEDIMessage> SendAnnex(CusEntryHeader entryHeader, CertificateObject certificateObject)
		{
			var messages = new List<ESEDIMessage>();

			var messageBuilders = GetMessageBuildersData(entryHeader, certificateObject);

			if (!messageBuilders.IsNullOrEmpty())
			{
				ESMessageSender.Send(messageBuilders, messages);
			}

			return messages;
		}

		protected ZString ProcessAcceptedDeclarationForAnnexAESAndT2LPOUS(EDIMessage message, CusEntryHeader entryHeader, Func<CusEntryHeader, CertificateObject, List<MessageBuilderData>> messageBuildersData)
		{
			SetSentMessageStatusAsReceived(message, entryHeader.Factory);
			SetMessageStatusAsReceived(message);

			if (ShouldChangeCHStatusForAnnexes(entryHeader))
			{
				SetCHStatusAsReceived(entryHeader);

				var shouldTriggerAnnexes = entryHeader.ZG_RequestDispatch == Customs.Business.YesNoList.Codes.Yes;
				TriggerMessageSendingCommon(message, ((IESMessageInfoProvider)entryHeader).Broker.GS_Code, entryHeader.Declaration.CusAgent, shouldTriggerAnnexes, entryHeader, SendAnnex);
			}

			return ZString.Empty;
		}

		protected void CommonProcessCancelationResponse(CusEntryHeader entryHeader, ZDateTime date, ZString transactionsCommentPrefix, string premiseType = CusTempStorageRegPremisesTypeList.Codes.ExportStorageFacility)
		{
			var internalReferenceNumber = entryHeader.TemporaryStorageTransactionInternalReferenceNumber;
			var isNotCancelledResponse = entryHeader.CH_EntryStatus != EntryStatusCodes.Invalidated &&
										 entryHeader.CH_EntryStatus != EntryStatusCodes.Cancelled;

			EU.Business.TemporaryStorageHelper.ManageTemporaryStorageCancelationWhenProcessResponse(entryHeader.Factory, entryHeader.CountryCode, entryHeader.EntryInstruction?.GoodsLocation?.Address?.AuthorisationNumber ?? ZString.Empty,
			internalReferenceNumber, internalReferenceNumber, entryHeader.TemporaryStorageTransactionInternalReferenceType, entryHeader.MovementReferenceNumber, transactionsCommentPrefix, transactionsCommentPrefix,
			date, premiseType, isNotCancelledResponse, true, true);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Constant strings")]
		protected const string TransactionsAESCommentPrefix = "DUE:";

		protected virtual List<MessageBuilderData> GetMessageBuildersData(CusEntryHeader entryHeader, CertificateObject certificateObject) => null;
	}
}
