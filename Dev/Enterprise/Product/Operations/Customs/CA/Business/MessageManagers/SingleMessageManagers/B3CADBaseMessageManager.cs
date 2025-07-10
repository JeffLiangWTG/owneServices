using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.MessageManagers;
using Enterprise.Customs.CA.Business.MessageProcessors;
using Enterprise.Customs.CA.Messaging;
using Enterprise.Customs.CA.Registry;
using Enterprise.Customs.Common.CA;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Environment;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.CA.Business.MessageManagers
{
	public abstract class B3CADBaseMessageManager : CAMessageManager
	{
		public B3CADBaseMessageManager(IB3Header b3Header, IUserNotification notification, B3DeferInstruction deferInstruction, EDIFACTMessageStatusCalculator calculator, bool fromServiceTask = false)
			: base(b3Header, calculator, notification)
		{
			this.deferInstruction = deferInstruction;
			this.fromServiceTask = fromServiceTask;
		}

		readonly bool fromServiceTask;

		protected new IB3Header DataWrapper
		{
			get { return (IB3Header)base.DataWrapper; }
		}

		protected abstract ZString MessageTypeForDisplay { get; }

		protected override ValidateForMessageType GetValidationType()
		{
			return ValidateForMessageType.B3CUSDEC;
		}

#if DEBUG
		protected
#endif
		B3DeferInstruction deferInstruction;

		#region Overrides of SingleMessageManager

		protected override void ShowQueuedForSending(MessageSubTypes actionCodeToSend)
		{
			var message = Res.GetString("2E02DA01-479E-48A2-8792-17E92BA54E3C", "{0} {1} has been generated.", GetActionCodeDescription(actionCodeToSend), MessageFriendlyName);
			var caption = Res.GetString("9840130A-CF42-44DF-B7D5-3C6507BEB82E", "Message generated");
			notification.ShowInformation(message, caption);
		}

		protected override string GetActionCodeDescription(MessageSubTypes actionCodeToSend)
		{
			var result = base.GetActionCodeDescription(actionCodeToSend);
			switch (actionCodeToSend)
			{
				case MessageSubTypes.Create:
					result = ZString.Format("Original");
					break;
				default:
					break;
			}
			return result;
		}

		public override string MessageFriendlyName
		{
			get { return ResString.GetMultilingualString("0596bd04-ee2b-477d-a0e5-750d2eb4e17f", "{0} Message for {1}", MessageTypeForDisplay, DataWrapper.TopLevelBusinessObject.HumanReadableName); }
		}

		protected bool CreditCheckForCLVS
		{
			get
			{
				var result = true;
				var jobLVS = BusinessObject as JobDeclaration;
				if (jobLVS != null)
				{
					result = !(CACustomsDataRegistry.Instance.EnableCreditCheckForCLVS.Value && jobLVS.IsConsolidatedLVS);
				}

				return result;
			}
		}

		#endregion

		#region Overrides of CAMessageManager

		#region CanSendThisMessage
		protected virtual ZBool PreventSendingMessageWhenEntryStatusIsCleared => false;

		protected override bool CanSendThisMessage(MessageSubTypes actionCode, out ZString messageText)
		{
			var result = base.CanSendThisMessage(actionCode, out messageText);
			var declaration = BusinessObject as JobDeclaration;

			if (result && CreditCheckForCLVS)
			{
				result = IsCreditCheckOKToSend(declaration, out messageText);
			}
			if (result)
			{
				result = !declaration.IsB3CADNotMessageAllowedToSend;
				if (!result)
				{
					CodeDescriptionPairList messageSubTypeList = declaration.IsCADEnabled ? new CADEntryTypeList() : new B3EntryTypeList();
					var messageSubTypeDesc = messageSubTypeList.GetDescriptionFromCode(declaration.JE_MessageSubType);
					notification.ShowError(Res.GetString("5919B4BB-1740-40CE-9940-8DCF596AB7C8", "{0} messages may not be sent when {0} Entry Type is '{1}'", MessageTypeForDisplay, messageSubTypeDesc), string.Empty);
				}
			}

			if (result)
			{
				if (PreventSendingMessageWhenEntryStatusIsCleared)
				{
					result = false;
					notification.ShowError(Res.GetString("8574CD55-57FC-4BB5-A1BD-CC4C981E3ECD", "{0} messages may not be sent when {0} Entry Status is Clear", MessageTypeForDisplay), string.Empty);
				}
			}

			result &= DoesNotHaveOtherJobsWaitingForResponse();

			if (result)
			{
				CheckExchangeRate(declaration);

				ZDate k84CutOffDate;

				if (declaration.IsPaymentDueDateAfterCutOffDate(out k84CutOffDate))
				{
					if (deferInstruction == null)
					{
						deferInstruction = new B3DeferInstruction(declaration, k84CutOffDate);
					}

					var entryHeader = declaration.B3EntryHeader;

					if (entryHeader.NeedResendDeferredB3CADMessage && entryHeader.NeedResendB3CADMessageSilent && !entryHeader.OriginalDeferredB3CADMessageTime.IsEmpty)
					{
						deferInstruction.heldUntilDateTimeUTC = entryHeader.OriginalDeferredB3CADMessageTime;
					}
					else
					{
						result = deferInstruction.DeferActionCode != DeferredB3SendActionListWithCancel.Codes.Cancel;
					}
				}

				if (UniversalReferenceConstants.IsCBSABOValid(deferInstruction?.heldUntilDateTimeUTC ?? ZDateTime.Today))
				{
					notification.ShowError(Res.GetString("6C3FC09F-7AEE-41BB-AD64-EDC4255A822B", "CBSA is currently in the CARM Blackout period where no CAD entries may be sent. Entry cannot be sent at this time."), string.Empty);
					result = false;
				}
			}
			return result;
		}

		protected override bool PreCheck4CreditOKToSendChecking(JobDeclaration declaration) => declaration?.B3EntryHeader?.HasDiscrepancyInTotalDutyAndTaxes ?? true;

		bool DoesNotHaveOtherJobsWaitingForResponse()
		{
			var result = true;
			var declaration = BusinessObject as JobDeclaration;
			if (declaration != null)
			{
				var matchingDeclarations = ImportLinkedObjectManager.LoadDeclarationsWithCusEntryHeaderReference(declaration.Factory, declaration.TransactionNumber.UniqueIdentifier, new[] { MessageTypeList.Codes.B3CUSDEC, MessageTypeList.Codes.CommercialAccountingDeclaration });
				var otherJobsWaitingForResponse = matchingDeclarations.Where(dec => dec.PK != declaration.PK && dec.B3EntryHeader != null && StatusCalculator.IsAwaitingReply(dec.B3EntryHeader.CH_Status));

				if (otherJobsWaitingForResponse.Any())
				{
					var otherJobs = new ZStringBuilder(otherJobsWaitingForResponse.OrderBy(x => x.JE_DeclarationReference).Select(x => x.JE_DeclarationReference)).ToStringWithDelimiterBetweenAppends(",");

					ResourceString otherJobsMessage = null;
					if (otherJobsWaitingForResponse.Count() == 1)
					{
						otherJobsMessage = ResString.GetMultilingualString("A38C026E-EE3C-4EBF-B00A-772551BC6E2D", "There is another job {0} having the same transaction sequence number waiting for response. A {1} response from Customs does not contain an ASEC number and system might not be able to identify a correct originating job. It is recommended that you wait until other jobs are responded. Are you sure you wish to continue?", otherJobs, MessageTypeForDisplay);
					}
					else
					{
						otherJobsMessage = ResString.GetMultilingualString("8E6389EF-0FE9-4F2F-BB1D-C91EEFD23B8C", "There are other jobs {0} having the same transaction sequence number waiting for response. A {1} response from Customs does not contain an ASEC number and system might not be able to identify a correct originating job. It is recommended that you wait until other jobs are responded. Are you sure you wish to continue?", otherJobs, MessageTypeForDisplay);
					}

					result = notification.ShowConfirmation(otherJobsMessage, ResString.GetMultilingualString("D64388E0-39B7-4603-B08D-50A77227B25A", "Are you sure you wish to continue?"));
				}
			}
			return result;
		}

		protected override void ActionWhenMessageCanNotSend()
		{
			if (deferInstruction != null && deferInstruction.DeferActionCode == DeferredB3SendActionListWithCancel.Codes.Cancel)
			{
				var wrapper = DataWrapper as IB3HeaderWithScheduledMessageSupport;
				if (wrapper != null)
				{
					try
					{
						wrapper.CancelAndDeactivateScheduledB3Message();
						if (StatusCalculator.IsAwaitingReply(wrapper.MessageStatus))
						{
							wrapper.MessageStatus = ZString.Empty;
						}
						wrapper.Factory.Save();
						notification.ShowInformation(ResString.GetMultilingualString("CDE05D90-BC25-4615-B884-9B377BFB9CF4", "Existing deferred {0} Message has been canceled.", MessageTypeForDisplay), ResString.GetMultilingualString("029C7AF5-AD60-45DB-8506-538D50C983E4", "Message Canceled"));
					}
					catch (ZSaveException e)
					{
						ZExceptionReporting.HandleSaveException(e);
					}
				}
			}
		}

		protected override ZString GetAdditionalWarningsMessage(MessageSubTypes actionCode)
		{
			var result = string.Empty;
			var declaration = BusinessObject as JobDeclaration;
			if (declaration != null)
			{
				var warningMessages = new ZStringBuilder();
				if (!DataWrapper.IsCalculationsDone)
				{
					warningMessages.Append(Res.GetString("C9CE537F-B396-471B-AA3F-1964FDD48F51",
						"One, or more, invoice lines do not have any GST details. It is recommended that you do NOT continue. You should run 'Generate Entries (Merge)' from the Brokerage menu and then check the GST details on lines."));
				}
				if (!EDIReleaseImportStatusCalculator.IsShipmentCleared(DataWrapper.TopLevelBusinessObject))
				{
					warningMessages.Append(Res.GetString("6e291002-85f1-4521-9639-3664d948f622",
						"The Release Status indicates that this shipment has not yet been cleared."));
				}

				if (!declaration.IsCADEnabled)
				{
					var importerAddInfo = declaration.ImporterOfRecordAddInfo ?? declaration.ImporterAddInfo;
					if (importerAddInfo != null && !importerAddInfo.ZO_PreventWarningOnSendingB3)
					{
						if (declaration.IsImporterPaysFlagged)
						{
							warningMessages.Append(Res.GetString("85086E41-AF8E-44BA-9D8A-8237792A7871",
								"This job is flagged as 'Importer Pays' and so the Importer-Lodged-Security flag will be set in the message sent to CBSA."));
						}
						if (declaration.IsGSTDirectPayment && !declaration.IsGSTDirectAutoRated)
						{
							warningMessages.Append(Res.GetString("B131F5C2-3C4B-4DFF-B3D5-6732F0BA2257",
								"This client is flagged as 'GST Direct' and GST will not be auto-rated for this job."));
						}
					}
				}

				var entryHasBeenLodgedMessage = EntryHasBeenLodged(declaration);
				if (!string.IsNullOrEmpty(entryHasBeenLodgedMessage))
				{
					warningMessages.Append(entryHasBeenLodgedMessage);
				}

				if (declaration.IsAnyInvoiceHasFutureDirectShipmentDate)
				{
					warningMessages.Append(Res.GetString("97a4679a-6b01-4eaa-98c6-5c334f456d01", "The direct shipment date of some invoice(s) is in the future."));
				}

				ZDate k84CutOffDate;
				var shouldShowAfterCutOffDateMessage = declaration.IsPaymentDueDateAfterCutOffDate(out k84CutOffDate) && !declaration.IsCSAApprovedImporter;
				if (shouldShowAfterCutOffDateMessage)
				{
					if (declaration.IsLowValueNormalReleaseJob)
					{
						if (deferInstruction.DeferActionCode == DeferredB3SendActionList.Codes.Defer)
						{
							warningMessages.Append(Res.GetString("37E049E9-E7F3-49E4-9288-3557F2840D81",
							"The due date for accounting of this Low Value Shipment (released on {0}) appears to fall after the Monthly Statement cut-off date of {1}, based on the system settings the Entry message will be deferred until the next pay period.",
							declaration.JE_EntryAuthorisationDate.ToShortDateString(), k84CutOffDate.ToShortDateString()));
						}
						else
						{
							warningMessages.Append(Res.GetString("37E049E9-E7F3-49E4-9288-3557F2840D82",
							"The due date for accounting of this Low Value Shipment (released on {0}) appears to fall after the Monthly Statement cut-off date of {1}.",
							declaration.JE_EntryAuthorisationDate.ToShortDateString(), k84CutOffDate.ToShortDateString()));
						}
					}
					else
					{
						if (deferInstruction.DeferActionCode == DeferredB3SendActionList.Codes.Defer)
						{
							warningMessages.Append(Res.GetString("A769FEBC-6734-4E42-AD04-E6548D2ABD17",
							"The due date for accounting of this job (released on {0}) appears to fall after the Monthly Statement cut-off date of {1}, based on the system settings the Entry message will be deferred until the next pay period.",
							declaration.JE_EntryAuthorisationDate.ToShortDateString(), k84CutOffDate.ToShortDateString()));
						}
						else
						{
							warningMessages.Append(Res.GetString("A769FEBC-6734-4E42-AD04-E6548D2ABD18",
							"The due date for accounting of this job (released on {0}) appears to fall after the Monthly Statement cut-off date of {1}.",
							declaration.JE_EntryAuthorisationDate.ToShortDateString(), k84CutOffDate.ToShortDateString()));
						}
					}
				}

				if (declaration.CA_AssesmentOption == AssessmentOptions.Codes.AQtoFollow && (declaration.CA_ServiceOption == ServiceOptions.Codes.PARS))
				{
					warningMessages.Append(Res.GetString("c4fb73ad-0630-4f8c-8b19-09aee5488326", "AQ to follow was specified but AQ data does not appear to have been transmitted."));
				}

				if (warningMessages.Length != 0)
				{
					warningMessages.Prepend(Res.GetString("0E1423BB-AAA5-4D48-9A74-6064402C273A", "The following unusual situations have been detected. Please read the following carefully."));
					if (shouldShowAfterCutOffDateMessage && deferInstruction.DeferActionCode == DeferredB3SendActionList.Codes.Defer)
					{
						warningMessages.Append(Res.GetString("AFF6E7C3-3802-4591-B3A1-165AE92FE36D", "Are you sure you wish to continue and schedule the {0} message at this time?", MessageTypeForDisplay));
					}
					else
					{
						warningMessages.Append(Res.GetString("AFF6E7C3-3802-4591-B3A1-165AE92FE18C", "Are you sure you wish to continue and send the {0} message at this time?", MessageTypeForDisplay));
					}
					result = warningMessages.ToStringWithNewLineBetweenAppends();
				}
			}

			return result;
		}

		protected virtual string EntryHasBeenLodged(JobDeclaration declaration)
		{
			return ZString.Empty;
		}

		void CheckExchangeRate(JobDeclaration declaration)
		{
			bool newRatesExist = false;
			foreach (JobComInvoiceHeader invoiceHeader in declaration.Invoices)
			{
				if (invoiceHeader.EffectiveExchangeRateForInvoiceCurr != invoiceHeader.JZ_InvoiceCurrExRate)
				{
					newRatesExist = true;
					break;
				}
			}
			if (newRatesExist || !DataWrapper.IsCalculationsDone)
			{
				var message = newRatesExist ? RecalculateDutyAndTax_NewRate : RecalculateDutyAndTax;

				if (notification.ShowConfirmation(message, WarningCaption, true))
				{
					declaration.MarkApportionmentDirty();

					var sendsToCustoms = new Customs.Business.SendsMessagesToCustomsShutterUpperer(false);
					declaration.DoMerge(sendsToCustoms);

					var invalidText = sendsToCustoms.InvalidOperationText;
					if (!string.IsNullOrEmpty(invalidText))
					{
						notification.ShowWarning(invalidText, ZString.Empty);
					}

					DataWrapper.RefreshCachedValues();
					notification.ShowInformation(RecalculateDutyAndTax_Confirmation, ZString.Empty);
				}
			}
		}

		#endregion

		protected override Enterprise.Messaging.Business.EDIMessage[] PopulateMessage(MessageSubTypes actionCode)
		{
			Enterprise.Messaging.Business.EDIMessage[] result = null;
			Action restoreToPreMessagingState = null;
			var needsToRestoreToPreMessagingState = false;
			var declaration = BusinessObject as JobDeclaration;
			var preMessagingActionSucceeded = true;
			try
			{
				var preMessagingActionResult = declaration.PreMessagingAction(actionCode);
				var preMessagingAction = preMessagingActionResult.PreMessagingAction;
				restoreToPreMessagingState = preMessagingActionResult.RestoreToPreMessagingState;

				var errorMessages = preMessagingActionResult.ErrorMessageForCheckFieldsForBondedWarehous;
				if (!errorMessages.IsEmpty)
				{
					notification.ShowError(errorMessages, ZString.Empty);
					preMessagingActionSucceeded = false;
				}
				else
				{
					var isBondedWarehouse = preMessagingActionResult.IsBondedWarehouse;
					if (isBondedWarehouse && preMessagingAction != null)
					{
						var universalResult = preMessagingAction();

						if (universalResult.ResultType == UniversalResult.HadErrors)
						{
							notification.ShowError(universalResult.ErrorMessage, ZString.Empty);
							preMessagingActionSucceeded = false;
						}
						else
						{
							needsToRestoreToPreMessagingState = true;
						}
					}
				}

				if (preMessagingActionSucceeded)
				{
					var wrapper = DataWrapper as IB3HeaderWithScheduledMessageSupport;
					if (wrapper != null)
					{
						wrapper.CancelAndDeactivateScheduledB3Message();
					}
					result = base.PopulateMessage(actionCode);
					needsToRestoreToPreMessagingState = false;
				}
			}
			finally
			{
				if (needsToRestoreToPreMessagingState && restoreToPreMessagingState != null)
				{
					restoreToPreMessagingState();
				}
			}

			return result ?? Array.Empty<Enterprise.Messaging.Business.EDIMessage>();
		}

		protected override void RunAdditionalEDIMessageModification(System.Collections.Generic.IEnumerable<Enterprise.Messaging.Business.EDIMessage> messages)
		{
			base.RunAdditionalEDIMessageModification(messages);

			var messageDelayInstruction = this.deferInstruction;
			if (messageDelayInstruction != null && messageDelayInstruction.DeferActionCode == DeferredB3SendActionList.Codes.Defer)
			{
				scheduledTime = messageDelayInstruction.HeldUntilDateTime;
				var scheduledTimeUtc = messageDelayInstruction.heldUntilDateTimeUTC;
				foreach (var message in messages)
				{
					message.EM_HeldUntilDate = scheduledTimeUtc;
				}
			}
		}
		protected ZDateTime scheduledTime = ZDateTime.Empty;

		protected override void OnMessageQueuedForSending(MessageSubTypes actionCode)
		{
			base.OnMessageQueuedForSending(actionCode);
			if (actionCode == MessageSubTypes.Create)
			{
				var wrapper = DataWrapper as IB3HeaderWithScheduledMessageSupport;
				if (wrapper != null)
				{
					wrapper.PopulateEntrySubmittedDateIfRequired(scheduledTime.IsValid ? scheduledTime : null);
				}
			}
			scheduledTime = ZDateTime.Empty;
		}

		protected override Security.SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.CAB3MsgSend; }
		}

		#endregion

		#region Static String

		public static string RecalculateDutyAndTax
		{
			get { return Res.GetString("9C85FD2B-D6B0-4037-846F-993B76DF79F6", "This job needs Duty and Tax recalculated. Would you like the system to recalculate now using the most recent data?"); }
		}

		public static string RecalculateDutyAndTax_NewRate
		{
			get { return Res.GetString("DE8A2C35-8B97-4541-874B-BC8EEA97C9B4", "The exchange rate last used with this job is not current and a new rate now exists. Would you like the system to recalculate now using the most recent data?"); }
		}

		public string RecalculateDutyAndTax_Confirmation
		{
			get { return Res.GetString("689B9712-1E33-44DB-B538-1F83C2E93553", "Duty and Tax has been recalculated. These new values will be sent in the {0} message.", MessageTypeForDisplay); }
		}

		#endregion

		protected override bool ShowJobReadyForPostingCore()
		{
			var options = CustomsDataRegistry.Instance.EnableAccountingIntegration.GetFallBackValueAtAllLevels(MasterFiles.Business.GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);
			return options.PreApprovalBillingJob;
		}

		protected override void HandleSaveException(ZSaveException e)
		{
			if (fromServiceTask)
			{
				throw e;
			}
			else
			{
				base.HandleSaveException(e);
			}
		}
	}
}
