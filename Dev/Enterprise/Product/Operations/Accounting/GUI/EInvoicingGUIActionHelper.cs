using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.AccountingCountryFactory;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.GUI.EInvoicing.PenaltyTaxMessage;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.GUI
{
	public class EInvoicingGUIActionHelper
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public EInvoicingGUIActionHelper(Func<IEnumerable<TransactionHeader>> getSelectedTransactions)
		{
			Argument.NotNull(getSelectedTransactions, nameof(getSelectedTransactions));

			GetSelectedTransactions = getSelectedTransactions;

			ComplianceInfoEInvoicingGUIActionProvider = CountryComplianceFactory.GetIComplianceInfoEInvoicingGUIActionProvider(GlbCompany.CurrentCompany.Country.Code) ?? CountryComplianceEInvoicingExtensionFactory.GetCountryComplianceInfoExtension(GlbCompany.CurrentCompany.Country.Code) as IComplianceInfoEInvoicingGUIActionProvider;

			SupportsRequeueForAmendPivotActionType = ObjectFactory.Get<IGlobalEInvoicingObjectFactory>().GetCountryEInvoicingBatchCreatorStrategy(GlbCompany.CurrentCompany.Country.Code)?.SupportsWaitForOriginalTransactionForAmending ?? false;
		}

		readonly IComplianceInfoEInvoicingGUIActionProvider ComplianceInfoEInvoicingGUIActionProvider;

		readonly bool SupportsRequeueForAmendPivotActionType;
		IElectronicInvoicingRequeueStrategy RequeueStrategy => ObjectFactory.Get<IElectronicInvoicingAccountingObjectFactory>().GetElectronicInvoicingRequeueStrategy();

		public MenuItem[] GetActionMenuItems(bool isAPTransaction = false)
		{
			var result = new List<MenuItem>();

			if (ComplianceInfoEInvoicingGUIActionProvider?.IsCountryEnableComplianceEInvoicing(isAPTransaction) ?? false)
			{
				if (!DocumentRequestMenuName.IsEmpty)
				{
					result.Add(new ZMenuItem(DocumentRequestMenuName, isAPTransaction ? new EventHandler(RequestPdfCopyHandlerAP) : new EventHandler(RequestPdfCopyHandlerAR)));
				}

				if (!StatusRequestMenuName.IsEmpty)
				{
					result.Add(new ZMenuItem(StatusRequestMenuName, isAPTransaction ? new EventHandler(RequestInvoiceStatusUpdateHandlerAP) : new EventHandler(RequestInvoiceStatusUpdateHandlerAR)));
				}

				var queuePendingInvoiceMenuItem = GetQueuePendingInvoiceActionMenuItem();
				if (queuePendingInvoiceMenuItem != null)
				{
					result.Add(queuePendingInvoiceMenuItem);
				}
			}

			return result.ToArray();
		}

		public ZMenuItem GetQueuePendingInvoiceActionMenuItem() =>
			(QueuePendingInvoiceMenuName.IsEmpty || AccountingMasterFilesRegistry.Instance.EReportingSubmitPivotDefaultStatus.Value != EInvoicingPivotState.Pending)
			? null
			: new ZMenuItem(QueuePendingInvoiceMenuName, new EventHandler(QueuePendingInvoiceHandler));

		public MenuItem GetPenaltyTaxInfoActionMenuItem()
		{
			if (!(ComplianceInfoEInvoicingGUIActionProvider?.IsCountryEnableComplianceEInvoicing() ?? false))
			{
				return null;
			}

			var eInvoicingPenaltyTaxInfoProvider = EInvoicingPenaltyTaxInfoFormFactory.CreateEInvoicingPenaltyInfoFormProvider(GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			if (eInvoicingPenaltyTaxInfoProvider == null)
			{
				return null;
			}

			return new ZMenuItem(eInvoicingPenaltyTaxInfoProvider.PenaltyTaxInfoMenuName,
				(_, _) =>
				{
					eInvoicingPenaltyTaxInfoProvider.ShowPenaltyTaxInfoForm();
				});
		}

		IComplianceInfoEInvoicingGUIActionDocumentRequest ComplianceInfoEInvoicingGUIActionDocumentRequest =>
			fComplianceInfoEInvoicingGUIActionDocumentRequest ??= ComplianceInfoEInvoicingGUIActionProvider as IComplianceInfoEInvoicingGUIActionDocumentRequest;
		IComplianceInfoEInvoicingGUIActionDocumentRequest fComplianceInfoEInvoicingGUIActionDocumentRequest;

		IComplianceInfoEInvoicingGUIActionStatusRequest ComplianceInfoEInvoicingGUIActionStatusRequest =>
			fComplianceInfoEInvoicingGUIActionStatusRequest ??= ComplianceInfoEInvoicingGUIActionProvider as IComplianceInfoEInvoicingGUIActionStatusRequest;
		IComplianceInfoEInvoicingGUIActionStatusRequest fComplianceInfoEInvoicingGUIActionStatusRequest;

		IComplianceInfoEInvoicingGUIActionDocumentRequestAP ComplianceInfoEInvoicingGUIActionDocumentRequestAP
			=> fComplianceInfoEInvoicingGUIActionDocumentRequestAP ??= ComplianceInfoEInvoicingGUIActionProvider as IComplianceInfoEInvoicingGUIActionDocumentRequestAP;
		IComplianceInfoEInvoicingGUIActionDocumentRequestAP fComplianceInfoEInvoicingGUIActionDocumentRequestAP;

		IComplianceInfoEInvoicingGUIActionStatusRequestAP ComplianceInfoEInvoicingGUIActionStatusRequestAP
			=> fComplianceInfoEInvoicingGUIActionStatusRequestAP ??= ComplianceInfoEInvoicingGUIActionProvider as IComplianceInfoEInvoicingGUIActionStatusRequestAP;
		IComplianceInfoEInvoicingGUIActionStatusRequestAP fComplianceInfoEInvoicingGUIActionStatusRequestAP;

		IComplianceInfoEInvoicingGUIActionQueuePendingInvoice ComplianceInfoEInvoicingGUIActionQueuePendingInvoice =>
			fComplianceInfoEInvoicingGUIActionQueuePendingInvoice ??= CountryComplianceFactory.GetCountryComplianceInfo(GlbCompany.CurrentCompany.Country.Code) as IComplianceInfoEInvoicingGUIActionQueuePendingInvoice;
		IComplianceInfoEInvoicingGUIActionQueuePendingInvoice fComplianceInfoEInvoicingGUIActionQueuePendingInvoice;

		IComplianceInfoEInvoicingRequeueHandler ComplianceInfoEInvoicingRequeueHandler =>
			cachedComplianceInfoEInvoicingRequeueHandler ??= CountryComplianceFactory.GetCountryComplianceInfo(GlbCompany.CurrentCompany.Country.Code) as IComplianceInfoEInvoicingRequeueHandler;
		IComplianceInfoEInvoicingRequeueHandler cachedComplianceInfoEInvoicingRequeueHandler;

		ZString DocumentRequestMenuName => ComplianceInfoEInvoicingGUIActionDocumentRequest?.DocumentRequestMenuName ?? ZString.Empty;

		ZString StatusRequestMenuName => ComplianceInfoEInvoicingGUIActionStatusRequest?.StatusRequestMenuName ?? ZString.Empty;

		ZString QueuePendingInvoiceMenuName => ComplianceInfoEInvoicingGUIActionQueuePendingInvoice?.QueuePendingInvoiceMenuName ?? ZString.Empty;

		Func<IEnumerable<TransactionHeader>> GetSelectedTransactions { get; }

		void RequestPdfCopyHandlerAR(object sender, EventArgs e)
		{
			ApplyAction(EInvoicingPivotActionType.DocumentAction, LedgerTypes.AccountsReceivable);
		}

		public void RequestInvoiceStatusUpdateHandlerAR(object sender, EventArgs e)
		{
			ApplyAction(EInvoicingPivotActionType.StatusCheck, LedgerTypes.AccountsReceivable);
		}

		void RequestPdfCopyHandlerAP(object sender, EventArgs e)
		{
			ApplyAction(EInvoicingPivotActionType.DocumentAction, LedgerTypes.AccountsPayable);
		}

		public void RequestInvoiceStatusUpdateHandlerAP(object sender, EventArgs e)
		{
			ApplyAction(EInvoicingPivotActionType.StatusCheck, LedgerTypes.AccountsPayable);
		}

		void QueuePendingInvoiceHandler(object sender, EventArgs e)
		{
			SetStatusToQueued(Env.Security.ReceivablesAuthorizeAndSend);
		}

		string GetMessageCaption(string actionType)
		{
			switch (actionType)
			{
				case EInvoicingPivotActionType.DocumentAction:
					return DocumentRequestMenuName;
				case EInvoicingPivotActionType.StatusCheck:
					return StatusRequestMenuName;
				default:
					return Res.GetString("D2902922-E4C3-4025-A0D9-D80D1EABB08F", "Request for e-Invoice");
			}
		}

		bool CheckExistActivePivot(IEnumerable<AccEInvoicingTransactionPivot> transactionPivots, string actionType)
		{
			var activeStatuses = new ZString[] { EInvoicingPivotState.Batched, EInvoicingPivotState.Queued, EInvoicingPivotState.Sent };

			var result = false;
			var currentPivot = transactionPivots.FirstOrDefault(x => x.AIP_ActionType == actionType && activeStatuses.Contains(x.AIP_Status));
			if (currentPivot != null)
			{
				var existPivotCheckProvider = CountryComplianceEInvoicingHelper.GetExistPivotCheckProvider(GlbCompany.CurrentCompany);
				if (existPivotCheckProvider != null)
				{
					result = existPivotCheckProvider.CheckExistActivePivot(currentPivot);
				}
				else
				{
					result = currentPivot.AIP_LastResponseReceivedUtc.IsEmpty || currentPivot.AIP_LastResponseReceivedUtc < currentPivot.AIP_LastSentTimeUtc;
				}

				if (result && currentPivot.AIP_ActionType == EInvoicingPivotActionType.DocumentAction && currentPivot.AIP_Status == EInvoicingPivotState.Sent)
				{
					result = ComplianceInfoEInvoicingGUIActionProvider.ExistActiveDocumentRequestPivot(currentPivot.AIP_LastSentTimeUtc);
				}
			}

			return result;
		}

		void DiscardRelatedlPivotAndBatchForAction(List<AccEInvoicingTransactionPivot> transactionPivots, string actionType)
		{
			var discardPiviotActionTypes = new List<string>() { actionType };

			var eInvoicingPivotPreHandleProvider = (ObjectFactory.Get<IGlobalAccountingCountryFactory>().GetCountryFactory(GlbCompany.CurrentCompany.GC_RN_NKCountryCode) as IInstanceProvider<IEInvoicingDiscardAdditionalPiviotActionTypesProvider>)?.Get();
			if (eInvoicingPivotPreHandleProvider != null)
			{
				discardPiviotActionTypes.AddRange(eInvoicingPivotPreHandleProvider.GetAdditionalPivotActionTypes(actionType));
			}

			DiscardPivotAndBatchForActionCore(transactionPivots, discardPiviotActionTypes);
		}

		void DiscardPivotAndBatchForActionCore(IEnumerable<AccEInvoicingTransactionPivot> transactionPivots, IEnumerable<string> actionTypes)
		{
			foreach (var actionType in actionTypes)
			{
				var pivotShouldBeDiscarded = transactionPivots.FirstOrDefault(x => x.AIP_ActionType == actionType);
				if (pivotShouldBeDiscarded != null)
				{
					pivotShouldBeDiscarded.AIP_Status = EInvoicingPivotState.Discarded;
					var batchOfPivot = pivotShouldBeDiscarded.Batch;
					if (batchOfPivot != null)
					{
						batchOfPivot.AIB_Status = EInvoicingBatchState.Discarded;
					}
				}
			}
		}

		(bool result, ZString errorMessage) IsEligibleToBatch(IEnumerable<AccEInvoicingTransactionPivot> pivots, string actionType)
		{
			if (!pivots.Any())
			{
				return (false, Res.GetString("3CE86AB0-533F-42F9-935D-F9E92CAC153D", "Transaction is not eligible for Electronic Invoicing."));
			}

			if (CheckExistActivePivot(pivots, actionType))
			{
				return (false, Res.GetString("A5B11814-4FD8-46DD-A363-EE502D8B33E4", "The request has already been sent."));
			}

			var additionalValidation = CountryComplianceEInvoicingHelper.GetAdditionalValidation(GlbCompany.CurrentCompany);
			if (additionalValidation != null)
			{
				var message = additionalValidation.GetValidationMessageForAfterPostAction(pivots, actionType);
				if (!message.IsEmpty)
				{
					return (false, message);
				}
			}

			if (actionType == EInvoicingPivotActionType.StatusCheck)
			{
				var existPivotCheckProvider = CountryComplianceEInvoicingHelper.GetExistPivotCheckProvider(GlbCompany.CurrentCompany);
				if (existPivotCheckProvider != null)
				{
					return existPivotCheckProvider.CanExistSucceedPivot();
				}

				var inactiveStatuses = new ZString[] { EInvoicingPivotState.BatchedWithError, EInvoicingPivotState.Failed, EInvoicingPivotState.Discarded };
				var statusPivot = pivots.FirstOrDefault(x => x.AIP_ActionType == EInvoicingPivotActionType.StatusCheck && !inactiveStatuses.Contains(x.AIP_Status));
				if (statusPivot != null)
				{
					if (statusPivot.AIP_Status == EInvoicingPivotState.Batched && statusPivot.Batch.AIB_Status == EInvoicingBatchState.Sent)
					{
						return (false, Res.GetString("0ABC9FA6-457A-492E-9DF1-98BF57673C1C", "System has already requested status for this transaction."));
					}
					else if (statusPivot.AIP_Status == EInvoicingPivotState.Succeed)
					{
						return (false, Res.GetString("71CC8DC6-092F-4C1F-BA7A-A73AD2A577B1", "Transaction already succeeded, no further request will be made."));
					}
				}
			}
			return (true, ZString.Empty);
		}

		void ShowActionInformation(string actionType, string ledger)
		{
			switch (actionType)
			{
				case EInvoicingPivotActionType.DocumentAction:
					Globals.Message.ShowInformation(
						ledger == LedgerTypes.AccountsReceivable || ComplianceInfoEInvoicingGUIActionDocumentRequestAP == null
						? ComplianceInfoEInvoicingGUIActionDocumentRequest.DocumentRequestActionInformation
						: ComplianceInfoEInvoicingGUIActionDocumentRequestAP.DocumentRequestActionInformationAP,
						DocumentRequestMenuName);
					break;
				case EInvoicingPivotActionType.StatusCheck:
					Globals.Message.ShowInformation(
						ledger == LedgerTypes.AccountsReceivable || ComplianceInfoEInvoicingGUIActionStatusRequestAP == null
						? ComplianceInfoEInvoicingGUIActionStatusRequest.StatusRequestActionInformation
						: ComplianceInfoEInvoicingGUIActionStatusRequestAP.StatusRequestActionInformationAP,
						StatusRequestMenuName);
					break;
			}
		}

		void ApplyAction(string actionType, string ledger = LedgerTypes.AccountsReceivable)
		{
			ShowActionInformation(actionType, ledger);

			var selectedTransactions = ComplianceInfoEInvoicingGUIActionProvider.GetEligibleInvoices(GetSelectedTransactions.Invoke()).Cast<TransactionHeader>();
			if (selectedTransactions.Any())
			{
				var factory = new BusinessObjectFactory();

				var actionMessage = new ZStringBuilder();
				foreach (var transaction in selectedTransactions.OrderBy(x => x.AH_TransactionNum))
				{
					var transactionMessage = ZString.Empty;
					var messageSuffix = ZString.Empty;
					var transactionPivots = GetAllPivots(factory, transaction.PK).ToList();
					(var isEligibleToBatch, var whyIsNotEligibleToBatch) = IsEligibleToBatch(transactionPivots, actionType);
					if (!isEligibleToBatch)
					{
						transactionMessage = Res.GetString("04084597-2FB5-413B-9635-9C84C9AC89B4", "Transaction Number {0}: {1}", transaction.AH_TransactionNum, whyIsNotEligibleToBatch);
					}
					else
					{
						DiscardRelatedlPivotAndBatchForAction(transactionPivots, actionType);

						(var result, var faultReason) = TransactionHeader.CreateEInvoicingPivotForRequest(factory, transaction, actionType);
						if (!result)
						{
							messageSuffix = Res.GetString("5764A6C4-F6D3-47C3-85C8-0630416FB686", "The transaction is not eligible for requests. {0}", faultReason);
						}
						else
						{
							messageSuffix = Res.GetString("811403FF-0864-49FA-812B-583397380031", "The request is sent.");
							(ComplianceInfoEInvoicingGUIActionProvider as IEInvoicingTransactionUpdater)?.UpdateTransaction(factory, transaction, actionType);
						}
						transactionMessage = Res.GetString("04084597-2FB5-413B-9635-9C84C9AC89B4", "Transaction Number {0}: {1}", transaction.AH_TransactionNum, messageSuffix);
					}
					actionMessage.Append(transactionMessage);
				}
				factory.Save();

				if (!actionMessage.IsEmpty)
				{
					Globals.Message.ShowInformation(actionMessage.ToStringWithNewLineBetweenAppends(), GetMessageCaption(actionType));
				}
			}
		}

		IEnumerable<AccEInvoicingTransactionPivot> GetAllPivots(BusinessObjectFactory factory, ZGuid transactionPk) =>
			factory.Load<AccEInvoicingTransactionPivot>(new ZQuery(AccEInvoicingTransactionPivotSchema.AIP_ParentID, transactionPk)
				.AddToFilter(AccEInvoicingTransactionPivotSchema.AIP_ParentTableCode, AccTransactionHeaderSchema.Constants.Prefix)
				.AddToFilter(AccEInvoicingTransactionPivotSchema.AIP_Status, SQLComparisonOperator.NotEqual, EInvoicingPivotState.Discarded)).ToList();

		#region E-Invoicing Transaction Status Change

		public void ResetStatusToQueued(SecurityCheckpoint securityCheckpointForRequeueTransactionWithSentStatus)
		{
			var selectedTransactions = GetSelectedTransactions();
			if (selectedTransactions.Any())
			{
				var statusProvider = (ObjectFactory.Get<IGlobalAccountingCountryFactory>().GetCountryFactory(GlbCompany.CurrentCompany.GC_RN_NKCountryCode) as IInstanceProvider<IEInvoicingRequeuePivotsStatusProvider>)?.Get();
				var filterProvider = (ObjectFactory.Get<IGlobalAccountingCountryFactory>().GetCountryFactory(GlbCompany.CurrentCompany.GC_RN_NKCountryCode) as IInstanceProvider<IEInvoicingPivotsToRequeueFilterProvider>)?.Get();
				var pivotStatusesEligibleForRequeuing = statusProvider?.GetEligibleForRequeuingStatus() ?? new List<ZString> { EInvoicingPivotState.Failed, EInvoicingPivotState.BatchedWithError, EInvoicingPivotState.Sent };
				var baseActionTypesEligibleForRequeuing = new List<ZString> { EInvoicingPivotActionType.Submit };
				var additionalActionTypesEligibleForRequeuing = new List<ZString> { EInvoicingPivotActionType.Cancel, EInvoicingPivotActionType.Adjustment };

				if (SupportsRequeueForAmendPivotActionType)
				{
					baseActionTypesEligibleForRequeuing.Add(EInvoicingPivotActionType.Amend);
				}

				var transactionPivots = GetTransactionsByPivotsDictionary(selectedTransactions);
				var pivotsToRequeueFilterCondition = filterProvider?.GetPivotsToRequeueFilter(transactionPivots.Keys.FirstOrDefault()?.Factory ?? new BusinessObjectFactory())
					?? delegate (AccEInvoicingTransactionPivot pivot)
					{
						return pivotStatusesEligibleForRequeuing.Contains(pivot.AIP_Status) && (baseActionTypesEligibleForRequeuing.Contains(pivot.AIP_ActionType) || additionalActionTypesEligibleForRequeuing.Contains(pivot.AIP_ActionType));
					};

				var pivotsToRequeue = transactionPivots.Keys.Where(pivotsToRequeueFilterCondition);

				var provider = (ObjectFactory.Get<IGlobalAccountingCountryFactory>().GetCountryFactory(GlbCompany.CurrentCompany.GC_RN_NKCountryCode) as IInstanceProvider<IEInvoicingRequeueProvider>)?.Get();
				if (provider != null)
				{
					var addtionalTransactionPivots = provider.GetAdditionalTransactionPivotsToRequeue(transactionPivots.Keys.FirstOrDefault()?.Factory ?? new BusinessObjectFactory(), selectedTransactions);
					pivotsToRequeue = pivotsToRequeue.Union(addtionalTransactionPivots);
				}

				var validStatusList = statusProvider?.GetSecurityConstraintStatus() ?? new List<ZString> { EInvoicingPivotState.Sent };
				var securityConstraintPivotsToRequeue = pivotsToRequeue.Where(x => validStatusList.Contains(x.AIP_Status));

				var otherSentPivots = transactionPivots.Keys.Where(x => x.AIP_Status == EInvoicingPivotState.Sent && x.AIP_ActionType != EInvoicingPivotActionType.Submit && x.AIP_ActionType != EInvoicingPivotActionType.Cancel);
				var otherNotSentPivots = transactionPivots.Keys.Where(x => x.AIP_Status != EInvoicingPivotState.Sent && x.AIP_ActionType != EInvoicingPivotActionType.Submit && x.AIP_ActionType != EInvoicingPivotActionType.Cancel);

				var rejectedPivots = new List<AccEInvoicingTransactionPivot>();
				var rejectionReasons = new HashSet<RequeueDecision>();
				foreach (var pivot in pivotsToRequeue)
				{
					if ((transactionPivots.Keys.Contains(pivot)) && baseActionTypesEligibleForRequeuing.Contains(pivot.AIP_ActionType))
					{
						var requeueDecision = RequeueStrategy.IsTransactionEnableToRequeue(transactionPivots[pivot]);
						if (requeueDecision != RequeueDecision.Requeue)
						{
							rejectedPivots.Add(pivot);
							rejectionReasons.Add(requeueDecision);
						}
					}
				}

				var userMessageBuilder = new ZStringBuilder();
				var showErrorMessage = false;

				if (SupportsRequeueForAmendPivotActionType)
				{
					otherSentPivots = otherSentPivots.Where(x => x.AIP_ActionType != EInvoicingPivotActionType.Amend);
					otherNotSentPivots = otherNotSentPivots.Where(x => x.AIP_ActionType != EInvoicingPivotActionType.Amend);
				}

				removeRejectedPivots(otherSentPivots);
				removeRejectedPivots(rejectedPivots);

				var allDelayed = false;
				if (!securityCheckpointForRequeueTransactionWithSentStatus.IsAllowed)
				{
					removeRejectedPivots(securityConstraintPivotsToRequeue);
				}
				else if (ComplianceInfoEInvoicingRequeueHandler?.IsPivotRequeueRestricted(EInvoicingPivotState.Sent, out var warningMessage) ?? false)
				{
					if (securityConstraintPivotsToRequeue.Any())
					{
						if (DialogResult.OK != Globals.Message.ShowConfirmation(
						warningMessage ?? ZString.Empty,
						Res.GetString("0B1CF6F5-D4BE-4FA7-A447-70985D624020", "Confirm re-queue transactions"),
						Res.GetString("c87336eb-e937-4911-944b-46f660de3965", "Yes"),
						MessageBoxIcon.Warning))
						{
							removeRejectedPivots(securityConstraintPivotsToRequeue);
						}
					}
				}
				else if (securityConstraintPivotsToRequeue.Any() && AccountingMasterFilesRegistry.Instance.DelayTimeForRequeueInvoices.Value > 0)
				{
					var delayTime = AccountingMasterFilesRegistry.Instance.DelayTimeForRequeueInvoices.Value;
					var pivotsCantSent = new List<AccEInvoicingTransactionPivot>();
					foreach (var pivot in securityConstraintPivotsToRequeue)
					{
						if ((ZDateTime.UtcNow - pivot.AIP_LastSentTimeUtc).TotalMinutes <= delayTime)
						{
							pivotsCantSent.Add(pivot);
						}
					}

					if (pivotsCantSent.Count > 0)
					{
						if (pivotsCantSent.Count == pivotsToRequeue.Count())
						{
							allDelayed = true;
							userMessageBuilder.Append(Res.GetString("cb19bf03-bc14-49ad-a632-d7f96bad63c6", "The selected transactions cannot be re-queued. Re-queuing is allowed {0} minutes after the last transmission.", delayTime));
						}
						else
						{
							var transactionNums = pivotsCantSent.SelectMany(x => selectedTransactions.Where(t => t.PK == x.AIP_ParentID).Select(t => t.AH_TransactionNum)).OrderBy(x => x);
							userMessageBuilder.Append(Res.GetString("f5a9d6eb-585a-4575-9cf3-df7725ee1a22", "Some of the selected transactions cannot be re-queued. Re-queuing is allowed {0} minutes after the last transmission.\r\nThe transactions are: {1}",
								delayTime, string.Join(", ", transactionNums)));
						}

						userMessageBuilder.AppendLine();
					}

					removeRejectedPivots(pivotsCantSent);
				}

				if (showErrorMessage || pivotsToRequeue.Count() < selectedTransactions.Count())
				{
					if (!allDelayed)
					{
						var message = Res.GetString("d7428d74-9dc1-4ebe-9040-aafbd6bfbef4", @"Previously queued for e-Reporting transactions will be re-queued if they have the following statuses:

- 'FAL' - Fail, or
- 'BER' - Batched with errors, or
- 'SNT' - Sent (when you have appropriate security rights).");
						message = filterProvider?.GetPivotsToRequeueMessage() ?? message;
						userMessageBuilder.Append(message);
						userMessageBuilder.AppendLine();
					}

					if (rejectedPivots.Any())
					{
						RequeueStrategy.GetReasonsToExcludeTransaction(rejectionReasons).ForEach(x => userMessageBuilder.AppendLine(x));
					}
				}

				if (pivotsToRequeue.Any())
				{
					if (!userMessageBuilder.IsEmpty)
					{
						Globals.Message.ShowWarning(userMessageBuilder.ToStringWithNewLineBetweenAppends());
					}

					ResetStatusToQueuedCore(pivotsToRequeue.ToArray(), otherNotSentPivots.ToArray());
				}
				else
				{
					userMessageBuilder.Append(Res.GetString("f74f9597-d335-4765-a7d6-1d364afcf543", "No eligible transactions found to be re-queued."));
					Globals.Message.ShowError(userMessageBuilder.ToStringWithNewLineBetweenAppends());
				}

				void removeRejectedPivots(IEnumerable<AccEInvoicingTransactionPivot> pivots)
				{
					if (pivots.Any())
					{
						showErrorMessage = true;
						foreach (var pivot in pivots.ToArray())
						{
							transactionPivots.Where(t => t.Key.AIP_ParentID == pivot.AIP_ParentID).ToList().ForEach(t => transactionPivots.Remove(t.Key));
						}
					}
				}
			}
		}

		public void ResetStatusToDelivered()
		{
			var messageForEligibleType = ResString.GetMultilingualString("69f40c43-b79a-4788-979d-8ec2f78e7423", @"Previously delivered (DLV) e-Reporting transactions will be re-queued to delivered if they have the following statuses:

- 'FAL' - Fail, or
- 'BER' - Batched With Error");
			var messageForNoEligibleTypeFound = ResString.GetMultilingualString("4a2ea81f-b8cb-4a31-bac9-b673e7d9b42d", "No eligible transactions found to be re-queued");

			var transactions = GetSelectedTransactions();

			var transactionAndPivots = GetTransactionsByPivotsDictionary(transactions);
			var supportResetToDeliveredInstance = (ObjectFactory.Get<IGlobalAccountingCountryFactory>().GetCountryFactory(GlbCompany.CurrentCompany.GC_RN_NKCountryCode) as IInstanceProvider<ISupportResetStatusToDelivered>)?.Get();
			var pivotEligibleForRepolling = supportResetToDeliveredInstance?.GetEligiblePivotToRequeue(transactionAndPivots.Keys);

			if (pivotEligibleForRepolling != null && !pivotEligibleForRepolling.Any())
			{
				Globals.Message.ShowError($"{messageForEligibleType}\r\n\r\n{messageForNoEligibleTypeFound}");
				return;
			}

			if (pivotEligibleForRepolling.Count() < transactionAndPivots.Count)
			{
				Globals.Message.ShowWarning(messageForEligibleType);
			}

			var factory = pivotEligibleForRepolling.First().Factory;
			foreach (var eligiblePivot in pivotEligibleForRepolling)
			{
				eligiblePivot.Batch.AIB_Status = EInvoicingBatchState.Discarded;
				eligiblePivot.AIP_Status = EInvoicingPivotState.Delivered;
				eligiblePivot.AIP_AIB = ZGuid.Empty;
				eligiblePivot.AIP_ErrorDescription = ZString.Empty;
			}

			factory.Save();
		}

		static Dictionary<AccEInvoicingTransactionPivot, TransactionHeader> GetTransactionsByPivotsDictionary(IEnumerable<TransactionHeader> selectedTransactions)
		{
			var requeueFactory = new BusinessObjectFactory();
			var pivotsQuery = new ZQuery(AccEInvoicingTransactionPivotSchema.AIP_ParentID, selectedTransactions.Select(x => x.PK))
					.AddToFilter(AccEInvoicingTransactionPivotSchema.AIP_Status, SQLComparisonOperator.NotEqual, EInvoicingPivotState.Discarded);
			var transactionPivots = new List<AccEInvoicingTransactionPivot>(requeueFactory.Load<AccEInvoicingTransactionPivot>(pivotsQuery));
			return transactionPivots.ToDictionary(x => x, x => selectedTransactions.FirstOrDefault(t => t.PK == x.AIP_ParentID));
		}

		protected virtual void ResetStatusToQueuedCore(IReadOnlyCollection<AccEInvoicingTransactionPivot> submitPivotsToRequeue, IReadOnlyCollection<AccEInvoicingTransactionPivot> otherPivotsToDiscard)
		{
			try
			{
				var factory = submitPivotsToRequeue.First().Factory;
				var batchPks = otherPivotsToDiscard.Select(x => x.AIP_AIB).Union(submitPivotsToRequeue.Select(x => x.AIP_AIB).Distinct());
				var batches = factory.Load<AccEInvoicingBatch>(new ZQuery(AccEInvoicingBatchSchema.PK, batchPks));
				var authorizationBehaviourProvider = (ObjectFactory.Get<IGlobalAccountingCountryFactory>().GetCountryFactory(GlbCompany.CurrentCompany.GC_RN_NKCountryCode) as IInstanceProvider<IEInvoicingAuthorizationBehaviourProvider>)?.Get();

				foreach (var pivot in otherPivotsToDiscard)
				{
					pivot.AIP_Status = EInvoicingPivotState.Discarded;
				}

				foreach (var pivot in submitPivotsToRequeue)
				{
					authorizationBehaviourProvider?.ResetAuthorisationRecordWhenResetPivotStatusToQueued(pivot);
					pivot.Requeue();
				}

				var successfulRequeueOrDiscardedStates = new List<ZString> { EInvoicingPivotState.Queued, EInvoicingPivotState.Pending, EInvoicingPivotState.Discarded };
				DiscardBatchesBasedOnPivotsStatus(batches, successfulRequeueOrDiscardedStates);

				factory.Save();
				var parentTransactionPks = otherPivotsToDiscard.Select(x => x.AIP_ParentID).ToHashSet();
				var parentTransactions = GetSelectedTransactions().Where(x => parentTransactionPks.Contains(x.PK)).ToList();

				foreach (var transaction in parentTransactions)
				{
					transaction.Refresh();
				}

				if (submitPivotsToRequeue.Count == 1)
				{
					Globals.Message.ShowInformation(Res.GetString("cf873766-e2f8-4c22-b5e5-94a4b9f3c06d", "Transaction was successfully re-queued."));
				}
				else
				{
					Globals.Message.ShowInformation(Res.GetString("2d7980c1-e0c5-4d53-9343-ec12f75c3547", "Eligible transactions were successfully re-queued."));
				}
			}
			catch (ZSaveConcurrencyException)
			{
				ShowConcurrencyError(submitPivotsToRequeue.Count == 1);
			}
		}

		void DiscardBatchesBasedOnPivotsStatus(AccEInvoicingBatch[] batches, List<ZString> applicablePivotStatus)
		{
			foreach (var batch in batches)
			{
				var pivots = batch.TransactionPivots.Cast<AccEInvoicingTransactionPivot>().ToArray();
				if (pivots.All(x => applicablePivotStatus.Contains(x.AIP_Status)))
				{
					batch.AIB_Status = EInvoicingBatchState.Discarded;
				}
			}
		}

		public void SetStatusToAwait(SecurityCheckpoint securityCheckpointForTransactionStatusChange)
		{
			if (!securityCheckpointForTransactionStatusChange.IsAllowed)
			{
				Globals.Message.ShowError(Res.GetString("78135F95-A703-4280-8797-66C084F289D4", "You do not have appropriate security rights to Review transactions."));
			}
			else
			{
				var selectedTransactions = GetSelectedTransactions();
				if (selectedTransactions.Any())
				{
					var selectedValidTransactions = selectedTransactions.Where(x => x.IsEligibleToCreateEInvoicingTransactionPivot);
					var pivotsToAwait = new List<AccEInvoicingTransactionPivot>();

					if (selectedValidTransactions.Any())
					{
						var awaitFactory = new BusinessObjectFactory();
						var pivotsQuery = new ZQuery(AccEInvoicingTransactionPivotSchema.AIP_ParentID, selectedValidTransactions.Select(x => x.PK))
								.AddToFilter(AccEInvoicingTransactionPivotSchema.AIP_Status, SQLComparisonOperator.Equal, EInvoicingPivotState.Pending);
						pivotsToAwait.AddRange(awaitFactory.Load<AccEInvoicingTransactionPivot>(pivotsQuery));
					}
					var userMessageBuilder = new ZStringBuilder();

					if (pivotsToAwait.Count < selectedTransactions.Count())
					{
						userMessageBuilder.Append(Res.GetString("B7E63FF9-CB79-42A7-A488-303DD67E8B5B", "You can only Review transactions where the E-Reporting status is 'PEN' - Pending."));
					}

					if (pivotsToAwait.Any())
					{
						var isInfo = userMessageBuilder.IsEmpty;
						var resultMsg = SetStatusToAwaitCore(pivotsToAwait);
						resultMsg = userMessageBuilder.Append(resultMsg).ToStringWithNewLineBetweenAppends();
						if (isInfo)
						{
							Globals.Message.ShowInformation(resultMsg);
						}
						else
						{
							Globals.Message.ShowWarning(resultMsg);
						}
					}
					else
					{
						userMessageBuilder.Append(Res.GetString("CB7C9FBF-5F3B-4F2F-B709-F9C66183E504", "No transactions will be reviewed."));
						Globals.Message.ShowError(userMessageBuilder.ToStringWithNewLineBetweenAppends());
					}
				}
			}
		}

		string SetStatusToAwaitCore(IReadOnlyCollection<AccEInvoicingTransactionPivot> submitPivotsToAwait)
		{
			try
			{
				var factory = submitPivotsToAwait.First().Factory;
				submitPivotsToAwait.ForEach(pivot => pivot.AwaitReview());
				factory.Save();

				GetSelectedTransactions().ForEach(transaction => transaction.Refresh());

				if (submitPivotsToAwait.Count == 1)
				{
					return Res.GetString("45FEDE3A-8BD6-4404-A60E-650F981E215D", "Transaction status was successfully set to Awaiting Review.");
				}
				else
				{
					return Res.GetString("86475875-E521-4552-ACBE-AA576A367335", "Transactions status were successfully set to Awaiting Review.");
				}
			}
			catch (ZSaveConcurrencyException)
			{
				ShowConcurrencyError(submitPivotsToAwait.Count == 1);
				return null;
			}
		}

		void ShowConcurrencyError(bool isOnlyOneObject)
		{
			if (isOnlyOneObject)
			{
				Globals.Message.ShowError(Res.GetString("21c0e82b-25f5-45cb-b656-2fa84f8b1795", "While you were working, another user has modified this transaction. Please try again."));
			}
			else
			{
				Globals.Message.ShowError(Res.GetString("ef0017fc-390e-47e5-93b8-3ab4763a1f6e", "While you were working, another user has modified these transactions. Please refresh the grid and try again."));
			}
		}

		#endregion

		#region E-Invoicing Transaction Status Setting to QUE

		public void SetStatusToQueued(SecurityCheckpoint securityCheckpointForQueueTransactions)
		{
			var userMessageBuilder = new ZStringBuilder();

			if (!securityCheckpointForQueueTransactions.IsAllowed)
			{
				userMessageBuilder.Append(Res.GetString("8C54A45D-E81C-4F0D-B6BB-DCF753741C21",
					"You do not have appropriate security rights to Authorize and Send transactions."));
				Globals.Message.ShowError(userMessageBuilder.ToStringWithNewLineBetweenAppends());
			}
			else
			{
				var selectedTransactions = GetSelectedTransactions();
				if (selectedTransactions.Any())
				{
					var transactionTypesEligibleForQueuing = new List<ZString> { TransactionTypes.CreditNote, TransactionTypes.AdjustmentNote, TransactionTypes.Invoice };

					var transactionEligibleForQueuing = selectedTransactions.Where(x => transactionTypesEligibleForQueuing.Contains(x.AH_TransactionType));

					if (transactionEligibleForQueuing.Count() < selectedTransactions.Count())
					{
						userMessageBuilder.Append(Res.GetString("BE5D9119-7419-4D4C-86A2-B4CBDCCE6B1C",
							"You can only Authorize and Send transactions where the Transaction Type is 'CRD - Credit Note', 'ADJ - Adjustment Note'  or 'INV - Invoice'."));
					}

					var transactionPivots = GetTransactionsByPivotsDictionary(transactionEligibleForQueuing);

					var submitPendingAndAwatingReviewPivotsToQueue = transactionPivots.Keys
						.Where(x => ComplianceInfoEInvoicingGUIActionQueuePendingInvoice?.PivotStatusesEligibleForQueuing?.Contains(x.AIP_Status) ?? false);

					if (submitPendingAndAwatingReviewPivotsToQueue.Count() < transactionEligibleForQueuing.Count())
					{
						userMessageBuilder.Append(ComplianceInfoEInvoicingGUIActionQueuePendingInvoice?.PivotStatusesEligibleForQueuingErrorMessage ?? ZString.Empty);
					}

					if (submitPendingAndAwatingReviewPivotsToQueue.Any())
					{
						if (!userMessageBuilder.IsEmpty)
						{
							Globals.Message.ShowWarning(userMessageBuilder.ToStringWithNewLineBetweenAppends());
						}

						SetStatusToQueuedCore(submitPendingAndAwatingReviewPivotsToQueue.ToArray());
					}
					else
					{
						userMessageBuilder.Append(Res.GetString("1A3C93A4-843C-409D-817E-0A6E00A80F1C", "No transactions will be set."));
						Globals.Message.ShowError(userMessageBuilder.ToStringWithNewLineBetweenAppends());
					}
				}
			}
		}

		void SetStatusToQueuedCore(IReadOnlyCollection<AccEInvoicingTransactionPivot> submitPivotsToQueue)
		{
			try
			{
				var factory = submitPivotsToQueue.First().Factory;
				var batchPks = submitPivotsToQueue.Select(x => x.AIP_AIB).Distinct();
				var batches = factory.Load<AccEInvoicingBatch>(new ZQuery(AccEInvoicingBatchSchema.PK, batchPks));

				foreach (var pivot in submitPivotsToQueue)
				{
					pivot.Requeue(true);
				}

				var successfulRequeueStates = new List<ZString> { EInvoicingPivotState.Queued };
				DiscardBatchesBasedOnPivotsStatus(batches, successfulRequeueStates);

				factory.Save();

				if (submitPivotsToQueue.Count == 1)
				{
					Globals.Message.ShowInformation(Res.GetString("5DB9A151-957B-4ACC-8137-74CCA6CE4EAD", "Transaction is now queued for sending."));
				}
				else
				{
					Globals.Message.ShowInformation(Res.GetString("728B39F3-4B7B-4F70-B870-65530281666A", "Transactions are now queued for sending."));
				}
			}
			catch (ZSaveConcurrencyException)
			{
				ShowConcurrencyError(submitPivotsToQueue.Count == 1);
			}
		}

		#endregion

		#region E-Invoicing Queue Invoice For Transmission

		public void QueueInvoiceForTransmission(SecurityCheckpoint securityCheckpointForQueueTransactions)
		{
			if (!securityCheckpointForQueueTransactions.IsAllowed)
			{
				Globals.Message.ShowError(securityCheckpointForQueueTransactions.ErrorMessageForNotAllowed);
			}
			else
			{
				var selectedTransactions = GetSelectedTransactions();
				if (selectedTransactions.Any())
				{
					var hasEligibleInvoices = false;
					var factory = selectedTransactions.First().Factory;
					var ineligibleTransactions = new List<TransactionHeader>();
					var queueInvoiceForTransmissionProvider = (ObjectFactory.Get<IGlobalAccountingCountryFactory>().GetCountryFactory(GlbCompany.CurrentCompany.GC_RN_NKCountryCode) as IInstanceProvider<IQueueInvoiceForTransmissionProvider>)?.Get();

					foreach (var transaction in selectedTransactions)
					{
						var isPivotExist = factory.Exists(BusinessObjectFactory.GetBusinessObjectBaseTypeFromTablePrefix(AccEInvoicingTransactionPivotSchema.Constants.Prefix), new ZQuery(AccEInvoicingTransactionPivotSchema.AIP_ParentID, transaction.PK));

						if (queueInvoiceForTransmissionProvider.ShouldQueueTransactionForTransmission(transaction) && !isPivotExist)
						{
							hasEligibleInvoices = true;
							transaction.EInvoicingProxy.CreateNewPivot();
						}
						else
						{
							ineligibleTransactions.Add(transaction);
						}
					}

					factory.Save();

					if (ineligibleTransactions.Any())
					{
						var errorMessage = GetInvoiceQueueForTransmissionErrorMessage(ineligibleTransactions, hasEligibleInvoices, queueInvoiceForTransmissionProvider.GetAdditionalErrorMessage());
						Globals.Message.ShowError(errorMessage.ToStringWithNewLineBetweenAppends());
					}
					else
					{
						Globals.Message.ShowInformation(Res.GetString("D64FB09C-10E4-4303-95EC-8D21CDBB9067", "Transaction(s) is queued for transmission."));
					}
				}
			}
		}

		ZStringBuilder GetInvoiceQueueForTransmissionErrorMessage(IEnumerable<TransactionHeader> ineligibleTransactions, bool hasEligibleInvoices, string criteriaErrorMessage)
		{
			var stringBuilder = new ZStringBuilder();
			var headerMessage = ineligibleTransactions.Count() == 1 && !hasEligibleInvoices
				? Res.GetString("DF98FCC9-2442-4A8C-BA14-A3FDFC650916", @"The transaction was not queued, due to one of the following reasons:")
				: Res.GetString("3E5DC16D-4FA7-487D-85E5-28CF782159F9", @"The following transactions were not queued:
{0}

due to one of the following reasons:", string.Join(", ", ineligibleTransactions.Select(x => x.AH_TransactionNum).OrderBy(x => x)));
			stringBuilder.Append(headerMessage);

			stringBuilder.Append(criteriaErrorMessage);

			if (AccountingConfigurationRegistry.Instance.DoNotQueueInvoicesContainingSpecificChargesForTransmission.Value.Count > 0)
			{
				var extraMessage = Res.GetString("2A349AA0-65BA-4F33-96DE-EE287055FBCF", "* Contains charges listed in ‘Do Not Queue Invoices Containing Specific Charges For Transmission’ registry.");
				stringBuilder.Append(extraMessage);
			}

			return stringBuilder;
		}

		#endregion
	}
}
