using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.MessageDelivery;
using Enterprise.MasterFiles.Business.UniversalData;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.XmlIO.XmlWriting;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using WTG.StaticAnalysis.Annotation;
using static Enterprise.Accounting.Business.AccountingConstants;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Accounting.DataTransfer.Universal.Netting
{
	public static class UniversalTransactionTransmitter
	{
		public static bool UniversalTransmitForNettingSystem(INotifications notifications, BusinessObjectFactory factory, InvoicingBase transaction, string status)
		{
			var result = false;

			if (!ShouldTransmit(transaction))
			{
				return result;
			}

			var debtorOrCreditor = factory.Load<OrgHeader>(transaction.AH_OH);
			var workflowProvider = (IWorkflowProvider)transaction;

			var workflowDescriptor = workflowProvider.WorkflowType != string.Empty ? WorkflowDescriptors.Instance.TryGetValueSafe(workflowProvider.WorkflowType) : null;
			if (workflowDescriptor != null && debtorOrCreditor != null && debtorOrCreditor.CustomsCodes.GetOrgCusCodesForCodeIgnoringCountry(OrgCusCode.CodeTypes.EHubOrganisationID).Any())
			{
				var nettingSystemOrgHeader = factory.Load<OrgHeader>(AccountingConfigurationRegistry.Instance.NettingSystemOrg.Value);

				var eHubCustomCode = nettingSystemOrgHeader?.CustomsCodes.GetOrgCusCodesForCodeIgnoringCountry(OrgCusCode.CodeTypes.EHubOrganisationID).FirstOrDefault();
				if (eHubCustomCode != null)
				{
					var destinationeHubId = eHubCustomCode.OK_CustomsRegNo;
					var communicationMode = new OrgProxyCommunicationModeProvider(EDICommunicationsMode.Modules.Netting, EDICommunicationsModeFileFormatList.Codes.XmlUniversalTransaction, () => debtorOrCreditor, x => x.EK_Destination == destinationeHubId);
					IDeliveryResult deliveryResult;
					if (communicationMode != null)
					{
						if (status == InvoiceAdditionalReference.Posted || status == InvoiceAdditionalReference.Reversed)
						{
							ProcessUniversalTransaction(transaction, communicationMode, notifications, workflowDescriptor);
							deliveryResult = DeliveryResult.Success;
						}
						else if (status == InvoiceAdditionalReference.FullyMatched || status == InvoiceAdditionalReference.UndoFullyMatched)
						{
							deliveryResult = DeliverUniversalEvent(factory, transaction, communicationMode, notifications, status);
						}
						else if (status == InvoiceAdditionalReference.PostedAndFullyMatched)
						{
							ProcessUniversalTransaction(transaction, communicationMode, notifications, workflowDescriptor);

#if DEBUG
							if (Globals.IsTest && isThreadDelayForTestOnly)
							{
								Thread.Sleep(100);
							}
#endif

							deliveryResult = DeliverUniversalEvent(factory, transaction, communicationMode, notifications, status);
						}
						else
						{
							throw new NotImplementedException();
						}

						result = deliveryResult.Succeeded;
					}
				}
			}

			return result;
		}
#if DEBUG
		[ThreadSafe]
		static bool isThreadDelayForTestOnly = false;
		public static bool IsThreadDelayForTestOnly { get => isThreadDelayForTestOnly; set => isThreadDelayForTestOnly = value; }
#endif

		static void ProcessUniversalTransaction(InvoicingBase transaction, IMessageProcessorCommunicationModesResult communicationMode, INotifications notificationBuffer, WorkflowDescriptor workflowDescriptor)
		{
			var action = new ActionWrapper(transaction, MessageRecipientPartyTypeList.Codes.WiseNettingSystem);
			Func<IDataWritingManager, ITopLevelDataObjectWriter> dataWriterGetter = (outboundSessionTracker) => workflowDescriptor.GetUniversalTransactionDataObjectWriter(outboundSessionTracker);
			var processor = UniversalXmlWorkflowProcessorBuilder.New(
							action,
							communicationMode,
							dataWriterGetter,
							transaction,
							null,
							null,
							null);

			var replaceThisTokenEventuallyQuestionMarkExclamationMark = CancellationToken.None;
			processor.Process(notificationBuffer, replaceThisTokenEventuallyQuestionMarkExclamationMark);
		}

		static IDeliveryResult DeliverUniversalEvent(BusinessObjectFactory factory, InvoicingBase transaction, IMessageProcessorCommunicationModesResult communicationMode, INotifications notificationBuffer, ZString status)
		{
			var mode = communicationMode.Destinations.FirstOrDefault();
			if (mode != null)
			{
				var deliveryContext = new DeliveryContext(factory)
				{
					ApplicationCode = ApplicationCodeList.Codes.UniversalDataMessaging,
					MessageSubTypeCode = EDIMessageSubTypeList.Codes.XmlUniversalEvent,
					MessageTypeCode = EDIMessageTypeList.Codes.XDC,
					Notifications = notificationBuffer,
					ParentInfo = EntityInfo.New(transaction)
				};

				UniversalEvent universalEvent = GetUniversalEvent(factory, transaction, status);

				return new EHubDelivery().Deliver(deliveryContext, (IEDICommunicationsMode)mode, new DeliveryStreamWrapperUXML(deliveryContext.ParentInfo, universalEvent, new XmlWriter(), null));
			}
			else
			{
				return DeliveryResult.ReportInvalidConfiguration(communicationMode.ConfigurationLogging);
			}
		}

		static UniversalEvent GetUniversalEvent(BusinessObjectFactory factory, InvoicingBase transaction, ZString status)
		{
			var dataContext = DataContextFactory.New();
			dataContext.SetCompanyAndDataProviderDetails(transaction.Company);
			var contextCollection = new List<Context>()
			{
				new Context() { Type = InvoiceForNettingInfoList.Ledger, Value = transaction.AH_Ledger },
				new Context() { Type = InvoiceForNettingInfoList.TransactionType, Value = transaction.AH_TransactionType },
				new Context() { Type = InvoiceForNettingInfoList.Number, Value = transaction.AH_TransactionNum },
				new Context() { Type = InvoiceForNettingInfoList.InternalReferenceNumber, Value = transaction.AH_ConsolidatedInvoiceRef },
				new Context() { Type = InvoiceForNettingInfoList.Status, Value = status }
			};

			var initiatingEHubId = GetEHubIDFromOrganization(transaction.Company.OrgProxy);
			contextCollection.Add(new Context() { Type = new ContextType() { Type = InvoiceForNettingInfoList.InitiatingEHubId }, Value = initiatingEHubId });

			var receivingEHubId = GetEHubIDFromOrganization(factory.Load<OrgHeader>(transaction.AH_OH));
			contextCollection.Add(new Context() { Type = new ContextType() { Type = InvoiceForNettingInfoList.ReceivingEHubId }, Value = receivingEHubId });

			var universalEvent = new UniversalEvent()
			{
				EventTime = ZDateTimeOffset.Now,
				EventType = AutoEvents.DataImport.Code,
				DataContext = dataContext,
				ContextCollection = contextCollection
			};
			return universalEvent;
		}

		static bool ShouldTransmit(InvoicingBase transaction)
		{
			var companyOrgProxyPK = transaction.Company.GC_OH_OrgProxy;
			var branchOrgProxyPKs = transaction.Company.Branches.Select(x => x.GB_OH_OrgProxy);
			var isCommonConditionOk = AccountingMasterFilesRegistry.Instance.EnableNetting.Value
					&& AccountingConfigurationRegistry.Instance.NettingStartDate.Value > DateTime.MinValue
					&& AccountingConfigurationRegistry.Instance.NettingStartDate.Value.ToUniversalTime().Date <= transaction.AH_DueDate.ToDateTime().ToUniversalTime().Date
					&& transaction.AH_OH != companyOrgProxyPK
					&& !branchOrgProxyPKs.Contains(transaction.AH_OH);

			var isSpecificInvoice = transaction != null
				&& (transaction.AH_Ledger == LedgerTypes.AccountsPayable || transaction.AH_Ledger == LedgerTypes.AccountsReceivable)
				&& (transaction.AH_TransactionType == TransactionTypes.Invoice || transaction.AH_TransactionType == TransactionTypes.CreditNote || transaction.AH_TransactionType == TransactionTypes.AdjustmentNote);

			return isCommonConditionOk && isSpecificInvoice;
		}

		static ZString GetEHubIDFromOrganization(OrgHeader org)
		{
			var eHubCusCode = org?.CustomsCodes.GetOrgCusCodesForCodeIgnoringCountry(OrgCusCode.CodeTypes.EHubOrganisationID).FirstOrDefault();
			return eHubCusCode?.OK_CustomsRegNo ?? ZString.Empty;
		}

#if DEBUG
		public static UniversalEvent GetUniversalEvent_ForTestOnly(BusinessObjectFactory factory, InvoicingBase transaction, ZString status)
		{
			return GetUniversalEvent(factory, transaction, status);
		}
#endif
	}
}
