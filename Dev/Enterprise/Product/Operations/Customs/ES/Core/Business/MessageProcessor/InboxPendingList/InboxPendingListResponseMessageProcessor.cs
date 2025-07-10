using System;
using System.Collections.Generic;
using System.Xml.Schema;
using CargoWise.Common;
using CargoWise.Customs.ES.MessageDefinitions;
using CargoWise.Customs.ES.MessageDefinitions.Version1.InboxNotification.LISTADECV4ENT;
using CargoWise.Customs.ES.MessageDefinitions.Version1.InboxNotification.LISTADECV4SAL;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business;
using Enterprise.Customs.ES.Business.EDIMessages;
using Enterprise.Customs.ES.Business.MessageSending;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.Customs.ES.Messaging.MessageProcessors;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Customs.ES.Business.ESConstants;

namespace Enterprise.Customs.ES.Business
{
	public class InboxPendingListResponseMessageProcessor : ESBranchCustomsApplicationTypeMessageProcessor<ListaDecV4Sal>
	{
		public InboxPendingListResponseMessageProcessor(LoggingInformation logger) : base(logger)
		{
		}

		protected override string MessageFriendlyNameCore => (NoResString)"Inbox Pending List Declaration Message Processor";

		protected override IReadOnlyList<ZString> MessageTypesToIncludeCoreES => new ZString[] { Messaging.DeclarationMessageTypeList.Codes.InboxPendingList };

		protected override void ProcessMessageCore(EDIMessage message)
		{
			try
			{
				CheckMessageTextNotEmpty(message);
				var provider = GetMessageProvider(message);
				ProcessDeclaration(provider, message);
			}
			catch (Exception ex) when (ex is XmlSchemaValidationException || ex is InvalidOperationException)
			{
				SetMessageStatusAsFailed(message);
				Logger.LogWarning(GetEDIMessageFailedLogDescription(message));
				Logger.LogWarning(GetExceptionLogDescription(ex));

				message.EM_MessageInterpretation = GetFailureMessageDetails(ex.Message, message);
			}
		}

		void ProcessDeclaration(ListaDecV4Sal response, EDIMessage message)
		{
			var factory = message.Factory;
			var typesList = new List<ZString>();
			foreach (var declarationData in response.Declaracion)
			{
				var type = GetTypeFromResponse(declarationData.TipoRespuesta);
				var mrn = GetMRNFromResponse(declarationData.Referencia, type);
				var transaction = GetTransaction(factory, type, mrn);
				if (transaction != null && transaction.CPT_Status != Core.Constants.Customs.CusPollingTransactionStatus.Codes.CLS)
				{
					CreateDetailsMessage(factory, transaction, type, declarationData.Clave);

					transaction.CPT_Status = Core.Constants.Customs.CusPollingTransactionStatus.Codes.CLS;
				}

				if (!typesList.Contains(type))
				{
					typesList.Add(type);
				}
			}

			if (typesList.IsNullOrEmpty())
			{
				typesList.Add(GetTypeFromOutgoingMessage(message));
			}

			ResetTransactionsWithTypeAndNotInResponse(factory, typesList);
			SetMessageStatusAsReceived(message);
			SetMessageSubTypeAsAccepted(message);
		}

		ZString GetTypeFromResponse(ZString responseType)
		{
			return responseType.ToString() switch
			{
				InboxNotificationResponseTypes.Export => DeclarationMessageTypeList.Codes.InBoxNotificationForExport,
				InboxNotificationResponseTypes.Import => DeclarationMessageTypeList.Codes.InBoxNotificationForImport,
				InboxNotificationResponseTypes.AESInvalidation => DeclarationMessageTypeList.Codes.ExportInvalidationCommunication,
				InboxNotificationResponseTypes.AESClearance => DeclarationMessageTypeList.Codes.ExportClearanceCommunication,
				InboxNotificationResponseTypes.AESNonConformity => DeclarationMessageTypeList.Codes.ExportNonConformityCommunication,
				InboxNotificationResponseTypes.AESCceControl => DeclarationMessageTypeList.Codes.ExportCceControlCommunication,
				InboxNotificationResponseTypes.AESExitResult => DeclarationMessageTypeList.Codes.ExportExitResultCommunication,
				InboxNotificationResponseTypes.AESExitClearance => DeclarationMessageTypeList.Codes.ExportExitClearanceNotification,
				InboxNotificationResponseTypes.AESExitNonConformity => DeclarationMessageTypeList.Codes.ExportExitNonConformityNotification,
				InboxNotificationResponseTypes.DVD => DeclarationMessageTypeList.Codes.InboxNotificationForDvdH2,
				InboxNotificationResponseTypes.NCTSClearance => DeclarationMessageTypeList.Codes.InboxNotificationNctsDepartureClearance,
				InboxNotificationResponseTypes.NCTSNonConformity => DeclarationMessageTypeList.Codes.InboxNotificationForNonConformityNctsDeparture,
				InboxNotificationResponseTypes.NCTSCceControl => DeclarationMessageTypeList.Codes.InboxNotificationNctsControls,
				InboxNotificationResponseTypes.NCTSInvalidation => DeclarationMessageTypeList.Codes.InboxNotificationNctsDepartureInvalidation,
				_ => ZString.Empty
			};
		}

		ZString GetMRNFromResponse(ZString responseMRN, ZString type)
		{
			return type.ToString() switch
			{
				DeclarationMessageTypeList.Codes.InBoxNotificationForExport => GetMrnCode(PrefixMRNCommonExport, SuffixMRNExport),
				DeclarationMessageTypeList.Codes.InBoxNotificationForImport => GetMrnCode(ZString.Empty, SuffixMRNImport),
				DeclarationMessageTypeList.Codes.ExportInvalidationCommunication => GetMrnCode(PrefixMRNCommonExport, SuffixMRNINE),
				DeclarationMessageTypeList.Codes.ExportClearanceCommunication => GetMrnCode(PrefixMRNCommonExport, SuffixMRNLVE),
				DeclarationMessageTypeList.Codes.ExportNonConformityCommunication => GetMrnCode(PrefixMRNCommonExport, SuffixMRNDIE),
				DeclarationMessageTypeList.Codes.ExportCceControlCommunication => GetMrnCode(PrefixMRNCommonExport, SuffixMRNCCE),
				DeclarationMessageTypeList.Codes.ExportExitResultCommunication => GetMrnCode(PrefixMRNCommonExport, SuffixMRNRES),
				DeclarationMessageTypeList.Codes.ExportExitClearanceNotification => GetMrnCode(PrefixMRNCommonExport, SuffixMRNLVS),
				DeclarationMessageTypeList.Codes.ExportExitNonConformityNotification => GetMrnCode(PrefixMRNCommonExport, SuffixMRNDIS),
				DeclarationMessageTypeList.Codes.InboxNotificationForDvdH2 => GetMrnCode(ZString.Empty, SuffixMRNImport),
				DeclarationMessageTypeList.Codes.InboxNotificationNctsDepartureClearance => GetMrnCode(PrefixMRNCommonNCTS, SuffixMRNLVT),
				DeclarationMessageTypeList.Codes.InboxNotificationForNonConformityNctsDeparture => GetMrnCode(PrefixMRNCommonNCTS, SuffixMRNDIT),
				DeclarationMessageTypeList.Codes.InboxNotificationNctsControls => GetMrnCode(PrefixMRNCommonNCTS, SuffixMRNCCT),
				DeclarationMessageTypeList.Codes.InboxNotificationNctsDepartureInvalidation => GetMrnCode(PrefixMRNCommonNCTS, SuffixMRNINT),
				_ => ZString.Empty
			};

			ZString GetMrnCode(string prefixText, string suffixText)
			{
				var mrnStartIndex = prefixText.Length;
				var mrnEndIndex = responseMRN.IndexOf(suffixText, StringComparison.OrdinalIgnoreCase);
				var mrnCode = responseMRN.SubstringSafe(mrnStartIndex, mrnEndIndex - mrnStartIndex);

				return mrnCode;
			}
		}

		CusPollingTransaction GetTransaction(BusinessObjectFactory factory, ZString type, ZString mrn)
		{
			var query = new ZQuery(CusPollingTransactionSchema.CPT_ApplicationCode, Enterprise.Messaging.Integration.ApplicationCodeList.Codes.ESCustomsMessage);
			query.AddToFilter(CusPollingTransactionSchema.CPT_Type, SQLComparisonOperator.Equal, type);
			query.AddToFilter(CusPollingTransactionSchema.CPT_TransactionID, SQLComparisonOperator.Equal, mrn);
			return factory.LoadTop1<CusPollingTransaction>(query);
		}

		void CreateDetailsMessage(BusinessObjectFactory factory, CusPollingTransaction transaction, ZString type, ZString key)
		{
			var parent = transaction.ParentObject as IPollingTransactionParent;

			if (parent != null && parent.Broker != null)
			{
				var certificateObject = new CertificateObject(parent.Broker, parent.CertificateName, ZString.Empty);

				var wrapper = new InboxNotificationDetailCommonSendMessageWrapper(factory, parent, parent.IsTest, key, certificateObject);
				IMessageBuilderBase messageBuilder;
				if (IsDetailV4Type(type))
				{
					messageBuilder = new InboxNotificationDetailV4MessageBuilder(wrapper, type, DeclarationMessageSubTypeList.Codes.OriginalDeclaration);
				}
				else
				{
					messageBuilder = new InboxNotificationDetailV5MessageBuilder(wrapper, type, DeclarationMessageSubTypeList.Codes.OriginalDeclaration);
				}

				var messageCreator = new EDIMessageCreator(messageBuilder, factory);
				var messageToSend = messageCreator.CreateMessage();
				parent.MessageCollection.Add(messageToSend);
			}
		}

		ZBool IsDetailV4Type(ZString type) => detailV4Types.Contains(type);

		ZString GetTypeFromOutgoingMessage(EDIMessage message)
		{
			var outgoingMessage = MessageProcessorHelper.GetOutgoingMessage(message);
			using (var textReader = outgoingMessage.GetEM_MessageTextReader())
			using (var bodyTextReader = XMLResponseMessageHelper.GetXmlBody(textReader))
			{
				var outgoingMessageTextObject = ESXmlObjectSerializer.DeserializeWithValidation<ListaDecV4Ent>("CargoWise.Customs.ES.MessageDefinitions.Version1.InboxNotification.Outgoing.ListaDecV4Ent.xsd", bodyTextReader);
				return GetTypeFromResponse(outgoingMessageTextObject.TipoRespuesta);
			}
		}

		void ResetTransactionsWithTypeAndNotInResponse(BusinessObjectFactory factory, List<ZString> typesList)
		{
			foreach (var type in typesList)
			{
				var pendingTransactions = GetPendingTransactions(factory, type);
				pendingTransactions.ForEach(t => t.CPT_Status = Core.Constants.Customs.CusPollingTransactionStatus.Codes.OPN);
			}
		}

		CusPollingTransaction[] GetPendingTransactions(BusinessObjectFactory factory, ZString type)
		{
			var query = new ZQuery(CusPollingTransactionSchema.CPT_ApplicationCode, Enterprise.Messaging.Integration.ApplicationCodeList.Codes.ESCustomsMessage);
			query.AddToFilter(CusPollingTransactionSchema.CPT_Type, SQLComparisonOperator.Equal, type);
			query.AddToFilter(CusPollingTransactionSchema.CPT_Status, SQLComparisonOperator.Equal, Core.Constants.Customs.CusPollingTransactionStatus.Codes.PND);
			return factory.Load<CusPollingTransaction>(query);
		}

		protected override ListaDecV4Sal GetMessageProviderCore(EDIMessage message)
		{
			using (var textReader = message.GetEM_MessageTextReader())
			using (var bodyTextReader = XMLResponseMessageHelper.GetXmlBody(textReader))
			{
				return ESXmlObjectSerializer.DeserializeWithValidation<ListaDecV4Sal>(XsdSchemaEmbeddedResourceName, bodyTextReader);
			}
		}

		ZString XsdSchemaEmbeddedResourceName => "CargoWise.Customs.ES.MessageDefinitions.Version1.InboxNotification.Incoming.ListaDecV4Sal.xsd";

		const string PrefixMRNCommonExport = "ADEX";
		const string PrefixMRNCommonNCTS = "ADTR";
		const string SuffixMRNExport = "NPD";
		const string SuffixMRNImport = "NPM";
		const string SuffixMRNINE = "CINVAL";
		const string SuffixMRNLVE = "CLEVEX";
		const string SuffixMRNDIE = "CDISEX";
		const string SuffixMRNCCE = "CONCCE";
		const string SuffixMRNRES = "CSALID";
		const string SuffixMRNLVS = "CLEVSA";
		const string SuffixMRNDIS = "CDISSA";
		const string SuffixMRNLVT = "CLETPA";
		const string SuffixMRNDIT = "CDITPA";
		const string SuffixMRNCCT = "CCOTPA";
		const string SuffixMRNINT = "CINVAT";

		readonly List<string> detailV4Types = new List<string>()
		{
			DeclarationMessageTypeList.Codes.InBoxNotificationForExport,
			DeclarationMessageTypeList.Codes.InBoxNotificationForImport,
			DeclarationMessageTypeList.Codes.InboxNotificationForDvdH2,
		};
	}
}
