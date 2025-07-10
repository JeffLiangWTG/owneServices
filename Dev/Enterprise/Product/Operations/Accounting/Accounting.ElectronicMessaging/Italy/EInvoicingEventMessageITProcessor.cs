using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Accounting.ElectronicMessaging.Italy
{
	public class EInvoicingEventMessageITProcessor : EInvoicingEventMessageProcessor
	{
		internal new static class ContextTypeCode
		{
			public const string EHubAllocatedNumber = "eHubAllocatedNumber";
			public const string GovernmentAllocatedNumber = "GovernmentAllocatedNumber";
			public const string ResponseMessage = "ResponseMessage";
		}

		internal static class MessageSubTypeCode
		{
			public const string RiceviFile = "rispostaSdIRiceviFile"; // message sub type does not need to be localised
			public const string RicevutaConsegna = "ricevutaConsegna"; // message sub type does not need to be localised
			public const string NotificaMancataConsegna = "notificaMancataConsegna"; // message sub type does not need to be localised
			public const string NotificaScarto = "notificaScarto"; // message sub type does not need to be localised
			public const string NotificaEsito = "notificaEsito"; // message sub type does not need to be localised
			public const string NotificaDecorrenzaTermini = "notificaDecorrenzaTermini"; // message sub type does not need to be localised
			public const string AttestazioneTranmissioneFattura = "attestazioneTranmissioneFattura"; // message sub type does not need to be localised
		}

		public EInvoicingEventMessageITProcessor(IXmlSessionTracker logger, IEDIMessage message, UniversalEvent universalEvent, AccEInvoicingBatch invoiceBatch)
			: base(logger, message, universalEvent, invoiceBatch)
		{
		}

		List<InvoicingBase> Transactions
		{
			get
			{
				if (transactions == null && transactionPivot != null)
				{
					transactions = new List<InvoicingBase>();
					var transaction = invoiceBatch.Factory.LoadTop1<InvoicingBase>(new ZQuery(AccTransactionHeaderSchema.PK, transactionPivot.AIP_ParentID));
					if (transaction != null)
					{
						transactions.Add(transaction);
					}
				}

				return transactions;
			}
		}
		List<InvoicingBase> transactions;
		AccEInvoicingTransactionPivot transactionPivot;

		public override void Process()
		{
			if (invoiceBatch.AIB_Status != Core.Constants.EInvoicingBatchState.Discarded)
			{
				transactionPivot = invoiceBatch.TransactionPivots.Cast<AccEInvoicingTransactionPivot>().FirstOrDefault();

				if (transactionPivot != null)
				{
					if (transactionPivot.AIP_Status != Core.Constants.EInvoicingPivotState.Succeed)
					{
						if (universalEvent.EventType.Value == AutoEvents.InterchangeAcknowledgedCode)
						{
							var messageSubType = universalEvent.EventParameters?.MessageSubType ?? ZString.Empty;
							if (messageSubType.IsEmpty)
							{
								ReportAndLogError("EmptyMessageSubType", Res.GetString("3029b75c-5a20-4ba2-ad93-186415549bbf", "No update performed due to universal event not containing message sub type for invoice batch {0} in {1}.", invoiceBatch.AIB_BatchNumber, companyName));
							}
							else
							{
								ProcessIAKEventMessage(messageSubType);
							}
						}
						else
						{
							ProcessIRJEventMessage();
						}
					}
					else
					{
						logger.LogBoth(LogType.Warning, Res.GetString("bdeff497-a255-4f0a-be38-78dc10f43743", "No update performed due to transaction pivot having 'SUC' status for invoice batch {0} in {1}.", invoiceBatch.AIB_BatchNumber, companyName));
					}
				}
				else
				{
					ReportAndLogError("PivotNotFound", Res.GetString("4a3bd231-08af-43e0-86d5-c2794324a5e5", "No update performed due to transaction pivot not found for invoice batch {0} in {1}.", invoiceBatch.AIB_BatchNumber, companyName));
				}
			}
		}

		#region IRJ Event Message

		void ProcessIRJEventMessage()
		{
			invoiceBatch.UpdateBatchAndPivotStatusAndErrorDescription(Transactions.Select(x => x.PK), universalEvent.EventParameters?.Reason ?? ZString.Empty, EInvoicingPivotState.Failed, lastResponseReceivedUtc: LastResponseReceivedTime);
		}

		#endregion

		#region IAK Event Message

		void ProcessIAKEventMessage(ZString messageType)
		{
			switch (messageType)
			{
				case MessageSubTypeCode.RiceviFile:
					ProcessRiceviFileMessage();
					break;
				case MessageSubTypeCode.RicevutaConsegna:
					ProcessRicevutaConsegnaMessage();
					break;
				case MessageSubTypeCode.NotificaMancataConsegna:
					ProcessNotificaMancataConsegnaMessage();
					break;
				case MessageSubTypeCode.NotificaScarto:
					ProcessNotificaScartoMessage();
					break;
				case MessageSubTypeCode.NotificaEsito:
					ProcessNotificaEsitoMessage();
					break;
				case MessageSubTypeCode.NotificaDecorrenzaTermini:
					ProcessNotificaDecorrenzaTerminiMessage();
					break;
				case MessageSubTypeCode.AttestazioneTranmissioneFattura:
					ProcessAttestazioneTranmissioneFatturaMessage();
					break;
				default:
					ReportAndLogError("UnknownMessageSubType", Res.GetString("d2aa393a-add5-4a59-9353-3518dd18aab4", "No update performed due to invalid message sub type [{0}] found for invoice batch {1} in {2}.", messageType, invoiceBatch.AIB_BatchNumber, companyName));
					break;
			}
		}

		void ProcessRiceviFileMessage()
		{
			if (universalEvent.ContextCollection != null)
			{
				var governmentAllocatedNumberContext = universalEvent.ContextCollection.FirstOrDefault(x => x.Type == ContextTypeCode.GovernmentAllocatedNumber);
				if (governmentAllocatedNumberContext != null)
				{
					if (governmentAllocatedNumberContext.Value.HasValue && !governmentAllocatedNumberContext.Value.Value.IsEmpty)
					{
						invoiceBatch.AIB_GovernmentAllocatedNumber = governmentAllocatedNumberContext.Value.Value;
					}
					else
					{
						ReportAndLogError("GovernmentAllocatedNumberContextHasNoValue", Res.GetString("8ac3f83f-24e0-4799-8f1a-309f70813dae", "Government Allocated Number was not updated due to Government Allocated Number Context value was empty for invoice batch {0} in {1}.", invoiceBatch.AIB_BatchNumber, companyName));
					}
				}
				else
				{
					ReportAndLogError("MissingGovernmentAllocatedNumberContext", Res.GetString("1ba2281f-0564-4037-9e42-41d021dc4caa", "Government Allocated Number was not updated due to context collection not having Government Allocated Number Context for invoice batch {0} in {1}.", invoiceBatch.AIB_BatchNumber, companyName));
				}

				var eHubAllocatedNumberContext = universalEvent.ContextCollection.FirstOrDefault(x => x.Type == ContextTypeCode.EHubAllocatedNumber);
				if (eHubAllocatedNumberContext != null)
				{
					if (eHubAllocatedNumberContext.Value.HasValue && !eHubAllocatedNumberContext.Value.Value.IsEmpty)
					{
						invoiceBatch.AIB_EHubAllocatedNumber = eHubAllocatedNumberContext.Value.Value;
					}
					else
					{
						ReportAndLogError("EHubAllocatedNumberContextHasNoValue", Res.GetString("b546c059-4cb1-4f86-a876-bad6777456fd", "eHub Allocated Number was not updated due to eHub Allocated Number Context value was empty for invoice batch {0} in {1}.", invoiceBatch.AIB_BatchNumber, companyName));
					}
				}
				else
				{
					ReportAndLogError("MissingEHubAllocatedNumberContext", Res.GetString("6e080a6a-a55f-467a-9739-49425355469c", "eHub Allocated Number was not updated due to context collection not having eHub Allocated Number Context for invoice batch {0} in {1}.", invoiceBatch.AIB_BatchNumber, companyName));
				}
			}
			else
			{
				ReportAndLogError("MissingContextCollection", Res.GetString("50e46b90-7393-4ac0-a529-27c8406ad568", "Government Allocated Number and eHub Allocated Number was not updated due to universal event message not having context collection for invoice batch {0} in {1}.", invoiceBatch.AIB_BatchNumber, companyName));
			}
			transactionPivot.AIP_LastResponseReceivedUtc = LastResponseReceivedTime;
		}

		void ProcessRicevutaConsegnaMessage()
		{
			invoiceBatch.UpdateBatchAndPivotStatusAndErrorDescription(Transactions.Select(x => x.PK), ZString.Empty, EInvoicingPivotState.Delivered, lastResponseReceivedUtc: LastResponseReceivedTime);
			AttachResponseMessageToeDocs();
		}

		void ProcessNotificaMancataConsegnaMessage()
		{
			transactionPivot.AIP_LastResponseReceivedUtc = LastResponseReceivedTime;
			AttachResponseMessageToeDocs();
		}

		void ProcessNotificaScartoMessage()
		{
			var errorMessage = (NoResString)"Scarto"; // error description does not need to be localised
			invoiceBatch.UpdateBatchAndPivotStatusAndErrorDescription(Transactions.Select(x => x.PK), errorMessage, EInvoicingPivotState.Failed, lastResponseReceivedUtc: LastResponseReceivedTime);
			AttachResponseMessageToeDocs();
			SendErrorNotificationEmail(errorMessage);
		}

		void ProcessNotificaEsitoMessage()
		{
			invoiceBatch.UpdateBatchAndPivotStatusAndErrorDescription(Transactions.Select(x => x.PK), ZString.Empty, EInvoicingPivotState.Succeed, lastResponseReceivedUtc: LastResponseReceivedTime);
			AttachResponseMessageToeDocs();
		}

		void ProcessNotificaDecorrenzaTerminiMessage()
		{
			var errorMessage = (NoResString)"Decorrenza Termini"; // error description does not need to be localised
			invoiceBatch.UpdateBatchAndPivotStatusAndErrorDescription(Transactions.Select(x => x.PK), errorMessage, EInvoicingPivotState.Failed, lastResponseReceivedUtc: LastResponseReceivedTime);
			AttachResponseMessageToeDocs();
			SendErrorNotificationEmail(errorMessage);
		}

		void ProcessAttestazioneTranmissioneFatturaMessage()
		{
			var errorMessage = (NoResString)"Attentazione Trasmissione Fattura"; // error description does not need to be localised
			invoiceBatch.UpdateBatchAndPivotStatusAndErrorDescription(Transactions.Select(x => x.PK), errorMessage, EInvoicingPivotState.Failed, lastResponseReceivedUtc: LastResponseReceivedTime);
			AttachResponseMessageToeDocs();
			SendErrorNotificationEmail(errorMessage);
		}

		#endregion

		#region Helpers

		protected override bool ShouldSendEmail => !Transactions.IsNullOrEmpty();

		protected override string LogErrorNotFoundKey => "TransactionNotFound"; // Error Message Key for Developers Only

		protected override GEIEmailNotificationCreator GetEmailNotificationCreator(ZString errorMessage, ILogger logCollector)
		{
			return new GEIEmailNotificationCreator(ediMessage, Transactions.First(), new List<ZString>() { errorMessage }, logCollector);
		}

		void AttachResponseMessageToeDocs()
		{
			var responseMessageContext = universalEvent.ContextCollection.FirstOrDefault(x => x.Type == ContextTypeCode.ResponseMessage);
			if (responseMessageContext != null)
			{
				if (responseMessageContext.Value.HasValue && !responseMessageContext.Value.Value.IsEmpty)
				{
					var transaction = Transactions.FirstOrDefault();
					if (transaction != null)
					{
						var fileName = universalEvent.EventParameters.MessageSubType.Value + "_" + ZDateTime.UtcNow.ToString("yyyyMMddhhmmss", CultureInfo.InvariantCulture) + ".xml";
						transaction.DocManagerInfo.AddFileOrDocument(Convert.FromBase64String(responseMessageContext.Value.Value), fileName, Core.Constants.ReferenceTypes.Accounting);
						transaction.DocManagerInfo.SetupEDocsFactoryToBeSavedWithMainFactory(false);
					}
					else
					{
						ReportAndLogError("TransactionNotFound", Res.GetString("a35e1175-98fd-4b04-a1d5-ecc17c921ece", "Response Message was not attached to eDocs due to related transaction was not found for pivot in invoice batch {0} in {1}.", invoiceBatch.AIB_BatchNumber, companyName));
					}
				}
				else
				{
					logger.LogBoth(LogType.Warning, Res.GetString("b1e1a21b-7a55-4ea8-86f5-155dbd12dc4c", "Response Message was not attached to eDocs due to Response Message Context value was empty for invoice batch {0} in {1}.", invoiceBatch.AIB_BatchNumber, companyName));
				}
			}
			else
			{
				logger.LogBoth(LogType.Warning, Res.GetString("15beb469-5637-4536-92b8-28878c001749", "Response Message was not attached to eDocs due to context collection not having Response Message Context for invoice batch {0} in {1}.", invoiceBatch.AIB_BatchNumber, companyName));
			}
		}

		#endregion
	}
}
