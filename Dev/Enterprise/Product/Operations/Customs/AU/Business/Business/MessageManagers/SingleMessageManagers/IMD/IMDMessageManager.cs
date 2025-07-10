using System;
using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.AU;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class IMDMessageManager : CMRMessageManager
	{
		public IMDMessageManager(CusEntryHeader entryHeader, IMDMultiMessageManager multiManager)
		{
			this.entryHeader = entryHeader;
			this.multiManager = multiManager;
		}

		public override BusinessObject BusinessObject => entryHeader;

		#region Notifications

		const string NoSecurityRightForCustomsPayment = "You do not have the security right to send a Customs Payment Message. Please contact your system administrator.";

		public override MessageSendingNotificationCollection GetNotificationsForSendingAnOriginal()
		{
			MessageSendingNotificationCollection result = base.GetNotificationsForSendingAnOriginal();
			if (CanSendOriginal)
			{
				var declaration = entryHeader.Declaration;
				var messageType = multiManager.MessageType;

				if (messageType == CMRMessageTypes.PreLodge && declaration.IsSAC)
				{
					result.AddError("You cannot send a Pre-Lodgement for SAC entries");
				}

				if (messageType == CMRMessageTypes.LodgeWithPay || messageType == CMRMessageTypes.LodgeWithoutPay)
				{
					if (declaration.IsQueuedEntryLodgementsEnabled && declaration.JE_EDITransmitDate.Date < ZDateTime.Today)
					{
						result.AddError("You cannot Submit to Customs if your EDI Transmit Date is in the past.");
					}
				}

				var paymentDetailRetriever = new PaymentDetailRetrieverIncludingAQIS(declaration, messageType, multiManager.EFTPaymentInformations);

				if (messageType == CMRMessageTypes.Payment && paymentDetailRetriever.TotalAmountPayable == 0m)
				{
					result.AddError("Payment Message has no Amounts to Pay.");
				}

				if (messageType == CMRMessageTypes.LodgeWithPay || messageType == CMRMessageTypes.Payment)
				{
					if (paymentDetailRetriever.IsPayingCustomsCharge)
					{
						if (declaration.Importer != null &&
							declaration.Importer.MiscServ.OM_IMEftHoldUntilPayAuthorised &&
							declaration.Importer.MiscServ.OM_IMEftCustomsFromImport &&
							declaration.LiveAuthorityToPayLog == null)
						{
							result.AddError("The client requires an EFT Authority to be sent, please do so.\r\nOnce the client has authorised this payment, please click a menu 'Add a log 'EFT Payment Authority' Given by Importer' before you pay or Lodge & Pay the entry.");
						}
					}
				}

				if (declaration.JE_PaymentMethod == JobDeclaration.PaymentMethods.Default
					&& declaration.Importer != null
					&& declaration.Importer.MiscServ.OM_IMEftCustomsFromImport
					&& !declaration.IsSACWithoutLines
					&& !paymentDetailRetriever.IsPayingAQISChargeOnly)
				{
					var paymentParty = paymentDetailRetriever.PartyToPayEntry;
					if (paymentParty == PaymentParty.Broker)
					{
						result.AddWarning("Importer is on direct debit according to its EFT configurations, but for this entry, the system recognises that BROKER should be the payee as either the total payable amount exceeds their maximum amount, or the importer's bank details are not complete. If you still want the importer to pay for this entry, please set the payment party on Misc. Options to importer.");
					}
				}

				if (paymentDetailRetriever.IsPayingAQISChargeOnly
					&& declaration.Importer != null
					&& !declaration.Importer.MiscServ.OM_IMEftQuarantineFromImport
					&& declaration.JE_PaymentMethod != JobDeclaration.PaymentMethods.Broker)
				{
					result.AddWarning("This importer is not configured as Direct EFT payment for Quarantine amount as 'EFT Quarantine From Importer A/C' is not ticked on the Consignee tab (Electronic Fund Transfer) of Organisation form. This amount will be debited from Broker's account.");
				}
			}

			return result;
		}

		public override MessageSendingNotificationCollection GetNotificationsForSendingAReplacement()
		{
			var declaration = entryHeader.Declaration;
			MessageSendingNotificationCollection result = base.GetNotificationsForSendingAReplacement();
			if (declaration.IsSAC)
			{
				result.AddError("You cannot send an Amendment for SAC entries");
			}

			if (entryHeader.MergedLines.IsRefundLikely && !entryHeader.MergedLines.HasRefundReason)
			{
				result.AddWarning("There are entry lines that would require a Refund Reason as the current Duty and Tax is less than the Duty and Tax advised in Last Lodgement Response Message.");
			}

			if (!entryHeader.MergedLines.IsRefundLikely && entryHeader.MergedLines.HasRefundReason)
			{
				result.AddWarning("There are no entry lines that would require a Refund Reason, but you have a Refund Reason entered on the header or one of the entry lines.");
			}

			if (declaration.IsImportCMR && (declaration.SingleWarehouseEntry?.HasAnInvoiceLineMarkedForBondedWarehousingWithoutAProduct ?? false))
			{
				result.AddError("Cannot send an Amendment message for an Inventory Managed job unless all Invoice Lines marked for Bonded Warehousing have a product specified.");
			}

			foreach (ICusEntryLine line in entryHeader.PendingDeletedLinesForAmendment)
			{
				if (line.RefundReasonCode == DeletedLineAmendment.RefundReasonForDeletedLines)
				{
					result.AddWarning(@"The system has set a refund reason code, '126A' (REMISSION OF DUTY IF AN IMPORT ENTRY IS TAKEN TO BE WITHDRAWN) 
for entry lines to be deleted.");
					break;
				}
			}

			return result;
		}

		protected override MessageSendingNotificationCollection GetCommonNotificationsForSending()
		{
			var result = base.GetCommonNotificationsForSending();
			var declaration = entryHeader.Declaration;

			if (declaration.HasStatusBeenChangedSinceLoading())
			{
				result.AddError("Someone else has changed the status of this declaration.  You may not send.");
			}

			if (!Env.Security.CustomsDeclarationPreLodge.IsAllowed && multiManager.MessageType == CMRMessageTypes.PreLodge)
			{
				result.AddError("You do not have the security right to send a Pre-Lodgement Message. Please contact your system administrator.");
			}

			if (!Env.Security.CustomsDeclarationLodgement.IsAllowed
				&& (multiManager.MessageType == CMRMessageTypes.LodgeWithPay
					|| multiManager.MessageType == CMRMessageTypes.LodgeWithoutPay
					|| multiManager.MessageType == CMRMessageTypes.Amendment
					|| multiManager.MessageType == CMRMessageTypes.Withdrawal)
				)
			{
				result.AddError("You do not have the security right to send a Lodgement Message. Please contact your system administrator.");
			}

			if (!Env.Security.CustomsDeclarationPaymentCustoms.IsAllowed
				&& multiManager.MessageType == CMRMessageTypes.LodgeWithPay)
			{
				result.AddError(NoSecurityRightForCustomsPayment);
			}

			if (multiManager.MessageType == CMRMessageTypes.Payment)
			{
				if (multiManager.EFTPaymentInformations == null)
				{
					result.AddError("You cannot send this payment message because no EFT Payment Information has been entered.");
				}
				else
				{
					if (!Env.Security.CustomsDeclarationPaymentCustoms.IsAllowed
						&& multiManager.EFTPaymentInformations.CustomsChargeAmount != 0m)
					{
						result.AddError(NoSecurityRightForCustomsPayment);
					}
					if (!Env.Security.CustomsDeclarationPaymentAQIS.IsAllowed
						&& multiManager.EFTPaymentInformations.AQISAmount != 0m)
					{
						result.AddError("You do not have the security right to send a Quarantine Payment Message. Please contact your system administrator.");
					}
				}
			}

			ZString brokerLicenceError = GetBrokerLicenceError();
			if (!brokerLicenceError.IsEmpty)
			{
				result.AddError(brokerLicenceError);
			}

			if (string.IsNullOrEmpty(Env.Registry.AUCustoms.LocalCustomsBranchIdentifier))
			{
				if (declaration.IsSAC)
				{
					if (string.IsNullOrEmpty(AUCustomsDataRegistry.Instance.LocalContactPhoneNumber.Value))
					{
						result.AddError("You cannot send a message because either the Local Customs Branch Id or the Local Contact Phone Number is needed. Please enter this through the registry.");
					}
				}
				else
				{
					result.AddError("You cannot send a message because the Local Customs Branch Id has not been entered. Please enter this through the registry.");
				}
			}

			if (!declaration.SubmitWeeklyNilReturnN30 && !declaration.IsThereAtLeastOneInvoiceHeaderAndOneInvoiceLinePerInvoiceHeader && Declaration.Business.ConsolidatedDeclaration.GetConsolidatedDeclaration(declaration) == null)
			{
				result.AddError("Please ensure there is at least one invoice header and each invoice header has at least one invoice line.");
			}

			if (declaration.GetExchangeRateRelatedWarnings().Count > 0)
			{
				result.AddWarning("One or more of the currencies you have used do not have a valid exchange rate for the date entered.  Sending the entry will result in an old exchange rate being used for all conversions");
			}

			if (declaration.JE_PaymentMethod == JobDeclaration.PaymentMethods.Cash && (multiManager.MessageType == CMRMessageTypes.LodgeWithPay || multiManager.MessageType == CMRMessageTypes.Payment))
			{
				result.AddError("You have indicated that this entry will be paid via Cash at a Customs Counter and therefore you cannot approve Payment or send a Payment Message.");
			}

			if (!declaration.Invoices.AreChargesBalancedForInvoices(out var message))
			{
				result.AddError(string.Format("Current apportionment is not balanced, {0}. Please click Brokerage > Perform Apportionment to correct calculation or remove the specified cause of imbalance.", message));
			}

			if (entryHeader.MergedLines.Count > AUCustomsDataRegistry.Instance.MaxNumberOfEntryLinesAcceptedAtCustoms.Value &&
				multiManager.MessageType != CMRMessageTypes.Payment)
			{
				var errorMessage = new ZStringBuilder();
				errorMessage.Append("The total number of entry lines has exceeded the maximum number Customs accepts and this entry will fail. Currently the maximum number Customs accepts is ");
				errorMessage.Append(AUCustomsDataRegistry.Instance.MaxNumberOfEntryLinesAcceptedAtCustoms.Value.ToString());
				errorMessage.Append(". Please try and merge more invoice lines. Or you can create separate entries.");
				result.AddError(errorMessage.ToString());
			}

			return result;
		}

		ZString GetBrokerLicenceError()
		{
			ZString result = ZString.Empty;

			var brokerLicence = GlbStaff.CurrentUser.Certificates.GetFirstCertificate(CertificateTypePairList.Codes.BR1);

			if (brokerLicence != null && !brokerLicence.XZ_ExpiryOrDueDate.IsEmpty && brokerLicence.XZ_ExpiryOrDueDate < ZDateTime.Today)
			{
				result = "A message cannot be sent.  Your Broker Licence has expired.  Please renew your Broker Licence and update the expiry date in your Staff record.";
			}
			else if (!entryHeader.Declaration.IsEntryForAnImporter && !AUCustomsDataRegistry.Instance.SoleTrader.Value)
			{
				if (multiManager.MessageType == CMRMessageTypes.PreLodge)
				{
					if (brokerLicence == null && string.IsNullOrEmpty(Env.Registry.AUCustoms.PreLodgementLicenceCode))
					{
						result = @"Customs Agent Nominee Licence number  missing on Pre-lodge: -
In order to pre-lodge entries you must supply a valid Nominee Licence Number. We have provided two work arounds to this restriction.
a) If you are a licenced Customs Agent you may place your Nominee Licence Number on your staff file and this will be placed in the message (This is the same place that we use for formal declarations).
b) You may put a valid Customs Agent Nominee Licence Number under Customs -> Australia -> Pre-Lodgement Licence Code in the registry. This will be used for all pre-lodgement entries done by non Customs Agents.
Please note that pre-lodge is not a formal entry, is not retained by the ACS and is not a formal declaration.";
					}
				}
				else if (multiManager.MessageType != CMRMessageTypes.Payment
					&& !entryHeader.Declaration.IsSAC
					&& brokerLicence == null)
				{
					result = "A message cannot be sent.  No Broker Licence Number has been entered in your Staff record.  Please update your Staff record with your Licence Number.";
				}
			}

			return result;
		}

		#endregion

		#region Implementation

		readonly CusEntryHeader entryHeader;
		readonly IMDMultiMessageManager multiManager;

		protected override EDIMessage[] GenerateOriginalMessagesCore(BusinessObject bizo)
		{
			var messageType = multiManager.MessageType;
			var declaration = entryHeader.Declaration;

			if (messageType == CMRMessageTypes.Payment)
			{
				if (declaration.IsQueuedEntryPaymentsEnabled && !declaration.IsQueuedEntryPayment)
				{
					var scheduledPaymentDate = entryHeader.ScheduledPaymentDate;
					if (scheduledPaymentDate > ZDateTime.Now)
					{
						var transmitTime = scheduledPaymentDate.ToDateTime();
						CreateScheduledLodgementMessageLog(declaration, messageType, transmitTime);
						entryHeader.CH_Status = CustomsEntryStatus.ScheduledPayment.Code;
						declaration.JE_MessageStatus = entryHeader.CH_Status;

						return Array.Empty<EDIMessage>();
					}
				}
				else if (!multiManager.ShouldGeneratePaymentMessageForThisEntryHeader(entryHeader))
				{
					entryHeader.CH_Status = CustomsEntryStatus.FailPayment.Code;
					declaration.JE_MessageStatus = entryHeader.CH_Status;
					return Array.Empty<EDIMessage>();
				}
			}
			else
			{
				var isLodgeWithPay = messageType == CMRMessageTypes.LodgeWithPay;
				if ((isLodgeWithPay || messageType == CMRMessageTypes.LodgeWithoutPay) && declaration.IsQueuedEntryLodgementsEnabled && !declaration.IsQueuedEntryLodgement)
				{
					var transmitDate = declaration.JE_EDITransmitDate.Date;
					if (transmitDate > ZDateTime.Today)
					{
						CreateScheduledLodgementMessageLog(declaration, messageType, transmitDate);
						entryHeader.CH_Status = isLodgeWithPay ? CustomsEntryStatus.ScheduledLodgeWithPayment.Code : CustomsEntryStatus.ScheduledLodgeWithoutPayment.Code;
						declaration.JE_MessageStatus = entryHeader.CH_Status;

						return Array.Empty<EDIMessage>();
					}
				}
			}

			return base.GenerateOriginalMessagesCore(bizo);
		}

		public static void CreateScheduledLodgementMessageLog(EnterpriseBusinessObject logParent, CMRMessageTypes messageType, ZDate transmitDate)
		{
			var transmitTime = transmitDate.AddHours(8).ToDateTime(); // give the exchange rates time to run and update
			CreateScheduledLodgementMessageLog(logParent, messageType, transmitTime);
		}

		public static void CreateScheduledLodgementMessageLog(EnterpriseBusinessObject logParent, CMRMessageTypes messageType, DateTime transmitTime)
		{
			logParent.Logs.CreateOrRecreateEventLog(new EventValue(AutoEvents.DeferredScheduledMessage, reference: messageType.ToString(), eventTime: transmitTime, isEstimate: false));
		}

		protected override EDIMessage[] GenerateMessagesForAmendmentDetection(BusinessObject bizo)
		{
			EDIMessage[] result = Array.Empty<EDIMessage>();
			if (!bizo.IsDeleted)
			{
				CusEntryHeader entryHeader = bizo as CusEntryHeader;

				BaseImportMessageBuilder builder = null;
				if (entryHeader.Declaration.IsSAC)
				{
					builder = new SACMessageBuilder(entryHeader, CMRMessageTypes.OriginalForAmendmentDetection);
				}
				else
				{
					builder = new IMDMessageBuilder(entryHeader, CMRMessageTypes.OriginalForAmendmentDetection);
				}

				builder.MessageSubType = Customs.Common.MessageBuilders.MessageSubTypes.Create;
				builder.Messages = GetMessages(bizo);
				result = new EDIMessage[] { builder.PopulateMessagesReturningResult() };
#if DEBUG
				ImportMessageBuilderForTesting = builder;
#endif
			}

			return result;
		}

#if DEBUG
		public BaseImportMessageBuilder ImportMessageBuilderForTesting;
#endif

		protected override bool RequiresAmendmentCore()
		{
			bool result = base.RequiresAmendmentCore();
			if (!result)
			{
				result = entryHeader.Questions.HaveDeclarationQuestionsBeenModified;
			}

			return result;
		}

		public override string MessageFriendlyName => "IMD Message for " + entryHeader.CH_BGMReference;

		internal override EDIMessageCollection GetMessages(BusinessObject bizo) => bizo.IsDeleted ? null : ((CusEntryHeader)bizo).Messages;

		internal override CMRMessageBuilder[] GetBuilder(BusinessObject bizo)
		{
			CMRMessageBuilder[] result = null;
			if (!bizo.IsDeleted)
			{
				CusEntryHeader entryHeader = bizo as CusEntryHeader;
				if (multiManager.MessageType == CMRMessageTypes.Payment)
				{
					if (multiManager.ShouldGeneratePaymentMessageForThisEntryHeader(entryHeader))
					{
						result = new CMRMessageBuilder[] { new PAYSTDMessageBuilder(entryHeader, multiManager.EFTPaymentInformations) };
					}
				}
				else if (entryHeader.Declaration.IsSAC)
				{
					CMRMessageBuilder builder = new SACMessageBuilder(entryHeader, multiManager.MessageType);
					((BaseImportMessageBuilder)builder).AmendmentWithdrawalReason = multiManager.AmendmentWithdrawalReason;
					result = new CMRMessageBuilder[] { builder };
				}
				else
				{
					CMRMessageBuilder builder = new IMDMessageBuilder(entryHeader, multiManager.MessageType);
					((BaseImportMessageBuilder)builder).AmendmentWithdrawalReason = multiManager.AmendmentWithdrawalReason;
					result = new CMRMessageBuilder[] { builder };
				}
			}

			return result;
		}

		internal override CMRAmendmentGenerator GetAmendmentManager(BusinessObject bizo) => new IMDAmendmentGenerator(bizo as CusEntryHeader, multiManager.AmendmentWithdrawalReason);

		internal override string GetStatus()
		{
			ZString result = ZString.Empty;

			if (!entryHeader.IsDeleted)
			{
				result = entryHeader.CH_Status;
			}

			return result;
		}

		internal override ICalculatedCusStatusCalculator[] StatusCalculators => new ICalculatedCusStatusCalculator[] { entryHeader.Calculator };

		public override bool CanSendOriginal
		{
			get { return base.CanSendOriginal || multiManager.MessageType == CMRMessageTypes.Payment || entryHeader.IsNextMessageOriginalForCMR; }
		}

		public override bool CanSendWithdrawal
		{
			get
			{
				return base.CanSendWithdrawal
					|| entryHeader.CH_Status == CustomsEntryStatus.ClearFormalLodge.Code
					|| entryHeader.CH_Status == CustomsEntryStatus.ClearPreLodge.Code
					|| entryHeader.CH_Status == CustomsEntryStatus.ClearSAC.Code
					|| entryHeader.CH_Status == CustomsEntryStatus.ClearAmendment.Code
					|| entryHeader.CH_Status == CustomsEntryStatus.FailAmendment.Code
					|| entryHeader.CH_Status == CustomsEntryStatus.FailPayment.Code
					|| entryHeader.CH_Status == CustomsEntryStatus.FailWithdrawal.Code
					|| entryHeader.CH_Status == CustomsEntryStatus.ClearPayment.Code;
			}
		}

		public override bool IsWaitingForResponse
		{
			get { return base.IsWaitingForResponse || entryHeader.IsWaitingForResponse; }
		}

		#region Can Send Amendment

		public bool CanSendAmendment
		{
			get
			{
				return entryHeader.CH_Status == CustomsEntryStatus.FailAmendment.Code ||
					((entryHeader.CH_Status == CustomsEntryStatus.ClearFormalLodge.Code
					|| entryHeader.CH_Status == CustomsEntryStatus.ClearAmendment.Code
					|| entryHeader.CH_Status == CustomsEntryStatus.FailWithdrawal.Code
					|| entryHeader.CH_Status == CustomsEntryStatus.ClearPayment.Code
					|| entryHeader.CH_Status == CustomsEntryStatus.FailPayment.Code)
					&&
					(entryHeader.CH_EntryStatus == CMRImportEntryAdvice.Held.Code
					|| entryHeader.CH_EntryStatus == CMRImportEntryAdvice.Clear.Code
					|| entryHeader.CH_EntryStatus == CMRImportEntryAdvice.Finalised.Code
					|| entryHeader.CH_EntryStatus == CMRImportEntryAdvice.Rejected.Code
					|| entryHeader.CH_EntryStatus == CMRImportEntryAdvice.ATDReceived.Code));
			}
		}

		#endregion

		protected override IEnumerable<INotification> GetMessageErrors()
		{
			return new IMDMultiMessageManager.ZNotificationCollectorMinusCPDecQuestionsAndDuplicates(BusinessObjectForNotification).GetMessageErrors();
		}

		protected override bool ShouldSendDeveloperExceptionForFalsePositiveCore(ZString factoryMessages, ZString databaseMessages)
		{
			bool result = base.ShouldSendDeveloperExceptionForFalsePositiveCore(factoryMessages, databaseMessages);
			if (result)
			{
				result = GetMessageTextExceptForSegments(factoryMessages) != GetMessageTextExceptForSegments(databaseMessages);
			}

			return result;
		}

		readonly string[] segmentsToExclude = new string[] { "NAD+DP+", "UNT+" };

		internal ZString GetMessageTextExceptForSegments(ZString messageText)
		{
			ZString message = messageText;
			foreach (string segment in segmentsToExclude)
			{
				message = GetMessageTextExceptForThisSegment(message, segment);
			}

			return message;
		}

		internal ZString GetMessageTextExceptForThisSegment(ZString messageText, ZString segmentToExclude)
		{
			ZString result = messageText;
			int indexOfStartOfSegment = messageText.IndexOf(segmentToExclude);
			if (indexOfStartOfSegment > 0)
			{
				ZString partialMessageText = messageText.SubstringSafe(indexOfStartOfSegment);
				int lengthOfSegmentIncludingSeparator = partialMessageText.IndexOf('\'', 1) + 1;

				ZString messageTextForTheSegment = messageText.SubstringSafe(indexOfStartOfSegment, lengthOfSegmentIncludingSeparator);
				result = messageText.Replace(messageTextForTheSegment, "");
			}

			return result;
		}

		#endregion
	}
}
