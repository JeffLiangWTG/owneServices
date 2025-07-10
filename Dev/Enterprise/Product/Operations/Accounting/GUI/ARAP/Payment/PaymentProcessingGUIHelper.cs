using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.CashBook.Transfer;
using Enterprise.Accounting.GUI.ARAP.AutoAllocationAndPrinting;
using Enterprise.Accounting.GUI.ARAP.PaymentApproval;
using Enterprise.Accounting.GUI.ARAP.ReceiptPayment;
using Enterprise.Accounting.Integration;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Registry.Business.AmountBasedMultiLevelAuthorisationRequirement;
using ActionType = Enterprise.Accounting.Business.ARAP.PaymentApproval.PaymentApprovalWithAuthorisation.ActionType;
using PaymentType = Enterprise.Accounting.Business.ARAP.ReceiptPayment.Payment;

namespace Enterprise.Accounting.GUI.ARAP
{
	//This class will be Refactor in WI00421194 - Refactor for PaymentProcessingGUIHelper
	public class PaymentProcessingGUIHelper
	{
		public PaymentProcessingGUIHelper()
		{
		}

		static string ChequeBookReprintErrorMessage => Res.GetString("3e57c06d-f966-4f81-a392-e4f8859d88fc", @"The registry 'Allow users to modify cheque number before posting a payment' is set to 'Yes'.

However, this function can only be used when all selected payments use the same cheque book and the cheque book is valid.

Please select payments using the same cheque book.");

		public static MultilingualString AuthorisationMenuText => ResString.GetMultilingualString("7579e0c3-c85e-408d-a3b9-fc9d268f00f3", "Authorization");
		public static MultilingualString AuthoriseMenuText => ResString.GetMultilingualString("3c57da13-327b-4e0e-bc0f-3e36cc479ac8", "Authorize");
		public static MultilingualString UnAuthoriseMenuText => ResString.GetMultilingualString("80c61e20-0267-44cf-abe9-81e5800aade2", "Unauthorize");
		public static MultilingualString PostApprovalsMenuText => ResString.GetMultilingualString("3db7818b-c550-4ea8-8776-a9c4462a6e34", "Post Approvals");
		public static MultilingualString PostItemMenuText => ResString.GetMultilingualString("1e199714-cbc6-4b6f-967a-6fe2ae14db42", "Post");
		public static MultilingualString AllocateChequeNoItemMenuText => ResString.GetMultilingualString("d00da606-fc30-4463-97cd-c3fe8f36f812", "Allocate Check No.");
		public static MultilingualString AllocateChequeNoAndPostItemMenuText => ResString.GetMultilingualString("de0badd1-e1ce-471c-b7bd-c334b9860113", "Allocate Check No. and Post");
		public static MultilingualString RejectItemMenuText => ResString.GetMultilingualString("188a07ff-894e-4020-b063-f52c546a6bc7", "Reject");
		public static MultilingualString CancelMenuText => ResString.GetMultilingualString("59da520c-cce5-411c-b18d-465571ff9d19", "Cancel");
		public static MultilingualString SubmitForApprovalMenuText => ResString.GetMultilingualString("308CEF72-717B-4D17-B126-C66783F21BCC", "Submit for Approval");
		public static MultilingualString ApproveForPostingMenuText => ResString.GetMultilingualString("CDE31E27-0B89-4F1E-9FB5-7CB6733CEEED", "Approve for Posting");
		public static MultilingualString PrintMenuItemText => ResString.GetMultilingualString("9971774e-2db2-433a-84e7-8e9978259ce9", "Print");

		void HandlePaymentApprovals(BusinessObject[] paymentApprovals, ActionType actionType, string message, bool isEPayment = false)
		{
			if (paymentApprovals == null || paymentApprovals.Length == 0)
			{
				Globals.Message.Show(message);
			}
			else
			{
				PerformActionForPaymentSelectedApprovals(actionType, paymentApprovals, isEPayment);
			}
		}

		#region Authorise Payments

		public void AuthorisePaymentApprovals(BusinessObject[] paymentApprovals)
		{
			if (paymentApprovals == null || paymentApprovals.Length == 0)
			{
				Globals.Message.Show(Res.GetString("b1b95193-55f7-4ffd-8b4f-b585bb2e3e4d", "Please select one or more Payments to authorize."));
			}
			else
			{
				AuthorisePaymentSelectedApprovals(paymentApprovals);
			}
		}

		void AuthorisePaymentSelectedApprovals(BusinessObject[] paymentApprovals)
		{
			var approvals = PerformActionForPaymentSelectedApprovals(ActionType.Authorize, paymentApprovals);
			PrintPaymentsForApprovals(approvals);
		}

		void PrintPaymentsForApprovals(List<PaymentApprovalWithAuthorisation> approvalList)
		{
			var collection = new List<TransactionHeader>();
			foreach (var approval in approvalList)
			{
				if (approval.NewPayment != null)
				{
					collection.Add(approval.NewPayment);
				}
			}
			if (collection.Count > 0)
			{
				PrintPayments(collection, true);
			}
		}

		#endregion

		#region Print

		public void PrintPaymentApproval(BusinessObject[] paymentApprovals)
		{
			if (paymentApprovals == null || paymentApprovals.Length != 1)
			{
				Globals.Message.Show(Res.GetString("cabd1abb-3758-438b-b89a-250511c0adcd", "Please select a transaction to print"));
			}
			else
			{
				PaymentApprovalBase selectedApproval = paymentApprovals[0] as PaymentApprovalBase;

#if DEBUG
				if (!Globals.IsTest || EnablePrint_ForTestOnly)
				{
#endif
					new PaymentPrintManager(selectedApproval, TransactionTypes.Payment, new BusinessObjectFactory()).Print();
#if DEBUG
				}
#endif
			}
		}

		#endregion

		#region UnAuthorise Payments

		public void UnAuthorisePaymentApprovals(BusinessObject[] paymentApprovals)
		{
			HandlePaymentApprovals(paymentApprovals, ActionType.Unauthorize, Res.GetString("48012f31-48c5-4bb9-a602-5124fa958c9b", "Please select one or more Payments to unauthorize."));
		}

		List<PaymentApprovalWithAuthorisation> PerformActionForPaymentSelectedApprovals(ActionType actionType, BusinessObject[] paymentApprovals, bool isEPayment = false)
		{
			var newFactory = new BusinessObjectFactory();
			var approvalList = GetSelectedBusinessObjectsLoadedInNewFactory(newFactory, paymentApprovals);

			var actionSuccessful = new List<PaymentApprovalWithAuthorisation>();
			var actionUnsuccessful = new List<(PaymentApprovalWithAuthorisation approval, ZString failReason)>();
			var buffer = new NotificationBuffer();
			foreach (var approval in approvalList)
			{
				buffer.Clear();
				switch (actionType)
				{
					case ActionType.Authorize:
						approval.TryAuthorisePayment(buffer);
						break;
					case ActionType.Unauthorize:
						approval.TryUnauthorisePayment(buffer);
						break;
					case ActionType.Reject:
						approval.TryRejectPayment(buffer);
						break;
					case ActionType.Cancel:
						approval.TryCancelPayment(buffer);
						break;
					case ActionType.SubmitForApproval:
					case ActionType.ApproveForPosting:
						approval.TrySubmitForApproval(buffer);
						break;
					default:
						break;
				}
				if (buffer.HasErrors)
				{
					actionUnsuccessful.Add((approval, buffer.AsString.Trim()));
				}
				else
				{
					actionSuccessful.Add(approval);
				}
			}

			var userMessageBuilder = new ZStringBuilder();
			if (actionUnsuccessful.Any())
			{
				userMessageBuilder.AppendLine(GetActionTypeMessage(actionType, false));
				foreach (var item in actionUnsuccessful)
				{
					userMessageBuilder.AppendLine(item.approval.GetDescription());
					userMessageBuilder.AppendLine(item.failReason);
					userMessageBuilder.AppendLine();
				}
			}

			if (actionSuccessful.Any())
			{
				if (actionType == ActionType.Reject)
				{
					if (actionUnsuccessful.Any())
					{
						Globals.Message.ShowError(userMessageBuilder.ToString().Trim());
					}

					RejectWithReasonPrompt(newFactory, actionSuccessful);
				}
				else
				{
					userMessageBuilder.AppendLine(GetActionTypeMessage(actionType, true));
					actionSuccessful.ForEach(x => userMessageBuilder.AppendLine(x.GetDescription()));
					userMessageBuilder.AppendLine();
					userMessageBuilder.Append(GetActionTypeMessage(actionType, false, true));

					var result = Globals.Message.Show(userMessageBuilder.ToString(), Res.GetString("8d481f25-14fe-40c9-aca8-7f55a04f1c16", "Payment Processing"), MessageBoxButtons.YesNo, DialogResult.No);
					if (result == DialogResult.Yes)
					{
						try
						{
							newFactory.Save();
							if (actionType == ActionType.SubmitForApproval && isEPayment)
							{
								Globals.Message.ShowInformation(
									Res.GetString(
										"ebeb2d76-dc8d-499f-a449-14f3a1ad1221",
										"Payment Approval saved. Authorized users can approve the payment in the Payment Processing module."
									)
								);
							}
						}
						catch (ZSaveConcurrencyException)
						{
							Globals.Message.ShowWarning(Res.GetString("ebeb2d76-dc8d-499f-a449-14f3a1ad3067", "Another user has just updated the payment approval. Please re-open the current form and try again."));
						}
					}
				}
			}
			else
			{
				Globals.Message.ShowError(userMessageBuilder.ToString().Trim());
			}

			return approvalList;
		}

		static ZString GetActionTypeMessage(ActionType actionType, bool willBe, bool isQuestion = false)
		{
			//this code is intentionally repetitive to make it translatable
			switch (actionType)
			{
				case ActionType.Authorize:
					return isQuestion
						? Res.GetString("0767F291-6863-45DE-B8D0-D0630D60E49B", "Do you want to authorize these Payments?")
						: willBe
							? Res.GetString("1A037D55-9479-46A5-95D0-032B9007B36C", "The following Payments will be authorized:")
							: Res.GetString("6D99C274-EA45-4616-9D31-709DA9A953C1", "The following Payments cannot be authorized:");

				case ActionType.Unauthorize:
					return isQuestion
						? Res.GetString("2B8E2903-D819-403C-BDD4-61C9D37D9307", "Do you want to unauthorize these Payments?")
						: willBe
							? Res.GetString("50349F82-8CEA-41B3-A901-DC6EC4159629", "The following Payments will be unauthorized:")
							: Res.GetString("F74D0E07-33BB-4E1F-BCCA-686434B07B60", "The following Payments cannot be unauthorized:");

				case ActionType.Reject:
					return isQuestion
						? Res.GetString("5BE326E7-1DF6-4C38-A27C-B9B6DE1B7545", "Do you want to reject these Payments?")
						: willBe
							? Res.GetString("5D893CDA-44EE-4FA7-B2B4-56C783D92E94", "The following Payments will be rejected:")
							: Res.GetString("CF67B099-58C0-4D6A-B26D-C0629BE68207", "The following Payments cannot be rejected:");

				case ActionType.Cancel:
					return isQuestion
						? Res.GetString("B4757FEB-6A9B-4982-94FB-10B4FD649C0B", "Do you want to cancel these Payments?")
						: willBe
							? Res.GetString("9C49A4A4-E9A6-4AF0-B011-D162C71EC6F6", "The following Payments will be canceled:")
							: Res.GetString("9E79CC85-F3E0-49FB-B7BA-8656AEBF70EF", "The following Payments cannot be canceled:");

				case ActionType.SubmitForApproval:
					return isQuestion
						? Res.GetString("586FD9C6-5054-4CAB-89E6-7CC28818CD37", "Do you want to submit these Payments for approval?")
						: willBe
							? Res.GetString("766CE2F1-BDF2-427E-92D1-1372E3F6E907", "The following Payments will be submitted for approval:")
							: Res.GetString("D54A1229-E640-4AEA-A9E0-504A10DEC457", "The following Payments cannot be submitted for approval:");

				case ActionType.ApproveForPosting:
					return isQuestion
						? Res.GetString("1B2FD37-61B5-4854-9DC6-F7A3774D9426", "Do you want to approve these Payments for posting?")
						: willBe
							? Res.GetString("0835E4AA-9E26-4C6A-8A27-A86297F1E157", "The following Payments will be approved for posting:")
							: Res.GetString("4F0330A6-E728-48A7-9B17-70A90089FD77", "The following Payments cannot be approved for posting:");

				default:
					return string.Empty;
			}
		}

		static void RejectWithReasonPrompt(BusinessObjectFactory newFactory, List<PaymentApprovalWithAuthorisation> actionSuccessful)
		{
			var reasonHolder = new PaymentRejectionReasonHolder();
			var reasonForm = new PaymentRejectionReasonForm(reasonHolder, actionSuccessful.Select(x => x.GetDescription()).ToArray());
			var rejectionReasonDialog = ZFormModaliser.ShowDialogAndDispose(reasonForm);
			if (rejectionReasonDialog == DialogResult.OK)
			{
				var eventReason = $"{reasonHolder.ReasonCodesList.GetDescriptionFromCode(reasonHolder.Code)}. {reasonHolder.Reason}"; // Information log
				var parameters = new KeyValuePair<string, string>("RES", eventReason);

				actionSuccessful.ForEach(x =>
				{
					x.AV_RejectionReasonCode = reasonHolder.Code;
					x.AV_RejectionReasonDetails = reasonHolder.Reason;

					x.Logs.AddNew(AutoEvents.AuthorisationRejected, "Payment Request Rejected", parameters);
				});

				newFactory.Save();
			}
		}

		#endregion

		#region Post Payments

		public IDisposable RegisterAfterSavedAction(Action action)
		{
			AfterSavedAction = action;
			return new DisposableAction(() => AfterSavedAction = null);
		}
		Action AfterSavedAction { get; set; }

		protected bool DoesUserWantToContinue(bool posting, bool populatingChequeNumber, List<PaymentApprovalBase> approvalsToProcess)
		{
			if (approvalsToProcess == null || approvalsToProcess.Count == 0)
			{
				Globals.Message.Show(Res.GetString("20c82e3e-46d1-4b71-b001-338d21f7b585", "Please select one or more Transactions to process."));
				return false;
			}
			else
			{
				var approvalsNotToBePosted = new List<PaymentApprovalBase>();
				var transactionsToBePosted = new ZStringBuilder();
				var transactionsNotToBePosted = new ZStringBuilder();
				bool showPostDateWarning = false;

				foreach (PaymentApprovalBase approval in approvalsToProcess)
				{
					var approvalErrorMessages = GetErrorMessages(approval, posting, populatingChequeNumber);
					ZString approvalDescription = approval.GetDescription();

					if (populatingChequeNumber && !posting && ((IChequeNumberAutoAllocation)approval).IsAutoAllocationEnabled)
					{
						approvalErrorMessages.AppendLine(Res.GetString("27692539-421b-4278-b23a-a75f3f21e3f3", "The check book of this approval is set to 'Auto Print'. Check number will be allocated upon posting."));
					}
					if (approval.IsBackDatePostingNotAllowedAndPostDateNotToday)
					{
						showPostDateWarning = true;
					}
					if (approvalErrorMessages.IsEmpty)
					{
						transactionsToBePosted.AppendLine(approvalDescription);
					}
					else
					{
						approvalsNotToBePosted.Add(approval);
						transactionsNotToBePosted.AppendLine(approvalDescription);
						transactionsNotToBePosted.AppendLine(approvalErrorMessages.ToString().Trim());
					}
				}

				if (approvalsNotToBePosted.Any())
				{
					approvalsToProcess.RemoveAll(x => approvalsNotToBePosted.Contains(x));
				}
#if DEBUG
				Test_ApprovalsAfterRemovingApprovalsThatCanNotBeProcessed = new List<PaymentApprovalBase>(approvalsToProcess);
#endif

				var messageToDisplay = new ZStringBuilder();

				if (!transactionsNotToBePosted.IsEmpty)
				{
					messageToDisplay.AppendLine(Res.GetString("054534d7-fdeb-4b48-a6df-968843e331f0", "The following Payments cannot be processed:"));
					messageToDisplay.AppendLine(transactionsNotToBePosted.ToString());
				}

				if (!transactionsToBePosted.IsEmpty)
				{
					messageToDisplay.AppendLine(Res.GetString("5db8e83f-8178-4e69-b984-f0994f8e8d48", "The following Payments will be processed:"));
					messageToDisplay.AppendLine(transactionsToBePosted.ToString());
					if (showPostDateWarning)
					{
						messageToDisplay.AppendLine(PaymentApprovalBase.UpdatePostDateWarning + System.Environment.NewLine);
					}

					if (posting && !populatingChequeNumber)
					{
						messageToDisplay.Append(Res.GetString("03382f7d-705c-41fb-bbd5-d9225980d937", "Do you want to post these transactions?"));
					}
					else if (posting && populatingChequeNumber)
					{
						messageToDisplay.Append(Res.GetString("e3e9ac05-96b1-4d93-8b4c-9a61cc755145", "Do you want to populate the cheque number and post these transactions?"));
					}
					else
					{
						messageToDisplay.Append(Res.GetString("53ac265d-e4d6-4dbc-a465-592c6e2f4778", "Do you want to populate the cheque number for these transactions?"));
					}

					DialogResult result = Globals.Message.Show(messageToDisplay.ToString(), Res.GetString("72556a47-074e-49c9-9ced-e24a6f9c58ac", "Payment Processing"), MessageBoxButtons.YesNo, DialogResult.No);
					if (result == DialogResult.Yes && posting)
					{
						result = ShowWarningIfPrinterUsedByAnotherChequeBook(approvalsToProcess);
					}

					return
#if DEBUG
						Globals.IsTest ||
#endif
						result == DialogResult.Yes;
				}
				else
				{
					messageToDisplay.Append(Res.GetString("01486581-56d2-41ab-b64a-ca5c83bcc5d5", "These transactions cannot be posted"));
					Globals.Message.Show(messageToDisplay.ToString());
					return false;
				}
			}
		}

		protected DialogResult ShowWarningIfPrinterUsedByAnotherChequeBook(List<PaymentApprovalBase> selectedApprovals)
		{
			DialogResult result = DialogResult.Yes;
			ZString messageIfChequeBookUsesSamePrinter = ZString.Empty;
			foreach (PaymentApprovalBase approval in selectedApprovals)
			{
				if (approval.ChequeBook != null)
				{
					messageIfChequeBookUsesSamePrinter = approval.ChequeBook.MessageIfChequeBookUsesSamePrinter();
					if (!messageIfChequeBookUsesSamePrinter.IsEmpty)
					{
						if (Globals.Message.Show(messageIfChequeBookUsesSamePrinter, AccChequeBook.WarningSamePrinterMessageCaption, MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) == DialogResult.Cancel)
						{
							result = DialogResult.No;
						}
					}
				}
			}
			return result;
		}

		public ContinueWithSave DisplayDealErrorMessagesBeforeBatchIsSaved(IEnumerable<PaymentApprovalBase> approvalsToProcess, string batchPaymentType)
		{
			var result = ContinueWithSave.Yes;

			if (batchPaymentType == ReceiptTypes.EPayment && approvalsToProcess.Any())
			{
				var errorMessages = new ZStringBuilder();

				foreach (var approval in approvalsToProcess)
				{
					if (!approval.CheckIfDealIsConfirmedByProvider(out var message))
					{
						errorMessages.AppendLine(approval.GetDescription());
						errorMessages.AppendLine(message.ToString().Trim());
					}
				}

				if (errorMessages.Length > 0)
				{
					result = ContinueWithSave.No;
					errorMessages.Prepend(Res.GetString("054534d7-fdeb-4b48-a6df-968843e331f0", "The following Payments cannot be processed:") + System.Environment.NewLine);
					errorMessages.AppendLine();
					errorMessages.AppendLine(Res.GetString("01486581-56d2-41ab-b64a-ca5c83bcc5d5", "These transactions cannot be posted"));
					errorMessages.AppendLine();
					errorMessages.Append(Res.GetString("70a8387b-8d19-4de2-b6f1-fb63158dd1c4", "Alternatively, change the payment type from E-Payment to another payment type"));
					Globals.Message.Show(errorMessages.ToString());
				}
			}

			return result;
		}

		public void PopulateChequeNoForPaymentApprovals(BusinessObject[] paymentApprovals)
		{
			var newFactory = new BusinessObjectFactory();
			var approvalsToProcess = ReloadApprovalsInTheNewFactory(paymentApprovals, newFactory);
			if (DoesUserWantToContinue(false, true, approvalsToProcess))
			{
				if (DoApprovalsToProcessContainsCheques(approvalsToProcess, false))
				{
					bool proceed = true;
					PaymentApprovalBulkProcessor processor = new PaymentApprovalBulkProcessor(newFactory);
					if (AccountingConfigurationRegistry.Instance.AllowEditCheckNumberBeforePosting.Value)
					{
						if (CheckValidForReprint(approvalsToProcess))
						{
							string chequeNumber = QueryChequeNumber(approvalsToProcess[0].ChequeBook.AK_Calc_CurrentNoString);
							processor.PopulateChequeNumbersOnPaymentApprovals(approvalsToProcess, chequeNumber);
						}
						else
						{
							Globals.Message.ShowError(ChequeBookReprintErrorMessage);
							proceed = false;
						}
					}
					else
					{
						processor.PopulateChequeNumbersOnPaymentApprovals(approvalsToProcess);
					}

					if (proceed)
					{
						processor.SaveChanges();
					}
				}
			}
		}

		public void PopulateChequeNoAndPostPaymentApprovals(BusinessObject[] paymentApproval)
		{
			var newFactory = new BusinessObjectFactory();
			var approvalsToProcess = ReloadApprovalsInTheNewFactory(paymentApproval, newFactory);

			if (DoesUserWantToContinue(true, true, approvalsToProcess))
			{
				if (DoApprovalsToProcessContainsCheques(approvalsToProcess, true))
				{
					PaymentApprovalBulkProcessor processor = new PaymentApprovalBulkProcessor(newFactory);
					bool proceed = true;
					if (AccountingConfigurationRegistry.Instance.AllowEditCheckNumberBeforePosting.Value)
					{
						if (CheckValidForReprint(approvalsToProcess))
						{
							string chequeNumber = QueryChequeNumber(approvalsToProcess[0].ChequeBook.AK_Calc_CurrentNoString);
							processor.PopulateChequeNumberAndPostPaymentApprovals(approvalsToProcess, chequeNumber);
						}
						else
						{
							Globals.Message.ShowError(ChequeBookReprintErrorMessage);
							proceed = false;
						}
					}
					else
					{
						processor.PopulateChequeNumberAndPostPaymentApprovals(approvalsToProcess);
						ShowPaymentErrorMessages(approvalsToProcess);
					}

					if (proceed)
					{
						PostAndAutoPrintPayments(processor, approvalsToProcess);
					}
				}
			}
		}

		bool DoApprovalsToProcessContainsCheques(List<PaymentApprovalBase> approvalsToProcess, ZBool isPostingApproval)
		{
			var result = false;
			if (approvalsToProcess != null)
			{
				var approvalsWithCheques = approvalsToProcess.Where(x => x.AV_PaymentType == ReceiptTypes.Cheque).ToList();

				if (approvalsWithCheques.Any())
				{
					if (approvalsWithCheques.Count < approvalsToProcess.Count)
					{
						string errorMessage;
						if (isPostingApproval)
						{
							errorMessage = Res.GetString("58a13096-2a73-40d4-8aa6-6e349dec7bcf", "Only Payments of Cheque Type will have a Cheque Number Allocated and Posted.");
						}
						else
						{
							errorMessage = Res.GetString("437af70d-9814-41fa-92b3-ec3bc6f1d14c", "Only Payments of Cheque Type will have a Cheque Number Allocated.");
						}
						Globals.Message.ShowWarning(errorMessage);
						approvalsToProcess.RemoveAll(x => !approvalsWithCheques.Contains(x));
					}
					result = true;
				}
				else
				{
					Globals.Message.ShowWarning(Res.GetString("56e1e71f-25bf-4875-a511-ad1cdbb692c9", "No Cheques Selected"));
					result = false;
				}
			}
			return result;
		}

		protected string QueryChequeNumber(string defaultChequeNumber)
		{
			return Globals.InteractiveNotification.QueryDefaultValue(defaultChequeNumber, Res.GetString("dc82aa5e-ab4a-43ca-8d55-62768b5f514b", "Please enter the number of the first check in the printer"), Res.GetString("79339ae9-0b73-4a73-b01a-e940c62c6997", "Check number"), 1);
		}

		protected bool CheckValidForReprint(List<PaymentApprovalBase> approvalsToProcess)
		{
			bool result = true;
			ZGuid chequeBook = ZGuid.Empty;

			foreach (PaymentApprovalBase payment in approvalsToProcess)
			{
				if (!payment.AV_AK.IsValid)
				{
					result = false;
					break;
				}
				else if (!chequeBook.IsValid)
				{
					chequeBook = payment.AV_AK;
				}
				else
				{
					if (chequeBook != payment.AV_AK)
					{
						result = false;
						break;
					}
				}
			}

			return result;
		}

		GetFxQuoteResult ENettGetExchangeRate(PaymentApprovalBase payment)
		{
			using (new CursorSwitcher(Cursors.WaitCursor))
			{
				return payment.GetEnettExchangeRate(x => Globals.Message.ShowError(x));
			}
		}

		bool CheckAreMultipleENettDirectDebitFXPaymentsSelected(BusinessObject[] paymentApprovals)
		{
			int count = 0;

			if (paymentApprovals != null && paymentApprovals.Length != 0)
			{
				PaymentApprovalCollection collection = new PaymentApprovalCollection(new BusinessObjectFactory());
				collection.AddRange(paymentApprovals);
				PaymentApprovalBase[] arrayToProcess = collection.ToArray<PaymentApprovalBase>();

				foreach (PaymentApprovalBase payment in arrayToProcess)
				{
					if (payment.AV_PaymentType == ReceiptTypes.eNettDirectDebit
						&& payment.AV_RX_NKPaymentCurrency != GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency
						&& payment.UseExchangeRateFromENettWebService)
					{
						count++;
					}
				}
			}

			return count > 1;
		}

		public void PostPaymentApprovals(BusinessObject[] paymentApprovals, ZString? paymentBatchReference = null)
		{
			if (CheckAreMultipleENettDirectDebitFXPaymentsSelected(paymentApprovals))
			{
				Globals.Message.ShowError(Res.GetString("9d664f42-d4ec-47aa-85ca-c84cfbb3982e", "You may only post one ComPay Direct Debit FX payment at a time."));
				return;
			}

			var newFactory = new BusinessObjectFactory();
			var approvalsToProcess = ReloadApprovalsInTheNewFactory(paymentApprovals, newFactory);
			PaymentApprovalBulkProcessor processor = new PaymentApprovalBulkProcessor(newFactory);

			if (DoesUserWantToContinue(true, false, approvalsToProcess))
			{
				Dictionary<ZGuid, ZString> cardSecurityCodes = new Dictionary<ZGuid, ZString>();
				foreach (PaymentApprovalBase payment in approvalsToProcess)
				{
					if (payment.AV_PaymentType == ReceiptTypes.eNettCreditCard
						&& payment.BankAccount != null
						&& payment.BankAccount.IsCreditCardOrLinkedAccount)
					{
						ZString cardSecurityCode = ZString.Empty;
						if (!cardSecurityCodes.ContainsKey(payment.BankAccount.PK))
						{
							PaymentCreditCardSecurityCode codeBO = new PaymentCreditCardSecurityCode();
							codeBO.Message = Res.GetString("2d7b3a19-2d3c-495f-9bb5-b3ec39510bcc", "Enter the Credit Card Security Code for the {0} credit card with number {1}.", payment.BankAccount.AB_Code, payment.BankAccount.AB_AccountNum);
							CreditCardSecurityCodeForm form = new CreditCardSecurityCodeForm(codeBO);
							ZFormModaliser.ShowDialogAndDispose(form);
							if (codeBO.Continue)
							{
								cardSecurityCode = codeBO.CardSecurityCode;
							}
							cardSecurityCodes.Add(payment.BankAccount.PK, cardSecurityCode);
						}
						if (cardSecurityCodes.TryGetValue(payment.BankAccount.PK, out cardSecurityCode))
						{
							payment.CreditCardSecurityCode = cardSecurityCode;
						}
					}
				}

				GetFxQuoteResult quoteResult = new GetFxQuoteResult();
				foreach (PaymentApprovalBase payment in approvalsToProcess)
				{
					if (payment.AV_PaymentType == ReceiptTypes.eNettDirectDebit
						&& payment.AV_RX_NKPaymentCurrency != GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency
						&& payment.UseExchangeRateFromENettWebService)
					{
						//update exchange rate
						//Custom House does not permit using the same quote for multiple FX bookings
						quoteResult = ENettGetExchangeRate(payment);

						if (quoteResult.Rate != payment.ExchangeRate.Rate)
						{
							DialogResult result = DialogResult.Cancel;

							while (result == DialogResult.Cancel)
							{
								result = new CountdownMessageBox(Res.GetString("23286391-ef2b-4db0-84a2-b49cfba214c3", @"ComPay exchange rate for {0} has changed since it was last retrieved.
Current exchange rate is {1}.
Overseas payment amount is {2} {3}.
Local payment amount is {4} {5}.

Do you wish to use the updated exchange rate for this payment?

If you click 'Yes', the exchange rate will be updated and saving will continue.
If you click 'No', the exchange rate will not be updated and saving will be canceled.
If you click 'Refresh', a new exchange rate will be retrieved.", payment.CurrencyCode, quoteResult.Rate, ZArchitecture.Core.Utilities.Round(payment.AV_Amount, payment.RXDecimals), payment.AV_RX_NKPaymentCurrency, quoteResult.LocalAmount, GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency), Res.GetString("9a3f9d50-8a05-4632-b69f-fdd2291eaa0c", "Confirm Exchange Rate Update"), MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question, 20).ShowDialog();

								if (result == DialogResult.Yes)
								{
									break;
								}
								else if (result == DialogResult.No)
								{
									return;
								}
								else
								{
									quoteResult = ENettGetExchangeRate(payment);
								}
							}
						}

						if (quoteResult.Rate != payment.ExchangeRate.Rate)
						{
							ZDecimal newLocalAmount = Env.CurrentCompany.ExchangeRate.ForeignToLocal(payment.AV_Amount, quoteResult.Rate);
							payment.AV_ExchangeDifference -= newLocalAmount - payment.AV_Calc_LocalAmount;
							payment.AV_Calc_LocalAmount = newLocalAmount;
						}
					}
				}

				processor.PostPaymentApprovals(approvalsToProcess);

				PostAndAutoPrintPayments(processor, approvalsToProcess, paymentBatchReference);

				ShowPaymentErrorMessages(approvalsToProcess);
			}
		}

		void ShowPaymentErrorMessages(List<PaymentApprovalBase> approvalsToProcess)
		{
			ZStringBuilder message = GetPaymentErrorMessages(approvalsToProcess);
			if (message != null && !message.IsEmpty)
			{
				Globals.Message.ShowError(message.ToString());
			}
		}

		ZStringBuilder GetPaymentErrorMessages(List<PaymentApprovalBase> approvalsToProcess)
		{
			ZStringBuilder message = new ZStringBuilder();

			foreach (PaymentApprovalBase payment in approvalsToProcess)
			{
				if (payment.HasErrors || payment.PaymentCreationErrorMessages.HasErrors())
				{
					message.AppendLine(Res.GetString("2c432b26-23ad-4003-b472-c6c72de5283b", "You cannot post {0}. Please fix the following errors before posting.", payment.GetDescription()));

					ZNotificationCollector notificationsCollector = new ZNotificationCollector(payment, true, false, ZNotificationCollector.PropertyDescriptionType.HumanReadableName);
					var combinedErrors = new NotificationCollection();
					combinedErrors.AddRange(notificationsCollector.GetErrors());
					combinedErrors.AddRange(payment.PaymentCreationErrorMessages.GetErrors());
					foreach (INotification error in combinedErrors.GetUniqueNotifications().GetErrors())
					{
						message.AppendLine("- " + error.Message);
					}
					message.AppendLine();
				}
			}

			return message;
		}

		public List<PaymentApprovalBase> ReloadApprovalsInTheNewFactory(BusinessObject[] selectedApprovals, BusinessObjectFactory factory)
		{
			var result = new List<PaymentApprovalBase>();
			if (selectedApprovals != null && selectedApprovals.Length != 0)
			{
				var keys = selectedApprovals.Select((v, i) => new { v.PK, i }).ToDictionary(p => p.PK, p => p.i);
				result = factory.Load<PaymentApprovalBase>(new ZQuery(AccPaymentApprovalSchema.PK, selectedApprovals.Select(bizO => bizO.PK).ToArray())).OrderBy(bizO => keys[bizO.PK]).ToList();
			}

			return result;
		}

		[SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider")]
		[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		void PostAndAutoPrintPayments(PaymentApprovalBulkProcessor processor, List<PaymentApprovalBase> approvals, ZString? paymentBatchReference = null)
		{
			var isSavedSuccessfully = false;
			bool canPrint = true;
			try
			{
				if (processor.PaymentsForAutoAllocation.Count > 0)
				{
#if DEBUG
					if (Globals.IsTest && !Globals.GetIsUnitTestingProductionFunctionality())
					{
						Test_Allocator = new PaymentBatchChequeNumberAllocator.DummyPaymentBatchChequeNumberAllocator(processor.PaymentsForAutoAllocation, processor.PaymentsForAutoAllocation.Factory);
						Test_Allocator.SetChequeBookToInactiveOnSaving = Test_DeactivateChequeBookOnAllocation;
						BusinessObjectFactory.SaveTogether(Test_Allocator.GetFactoriesForTest());
						isSavedSuccessfully = true;
					}
					else
					{
#endif
						var allocator = new PaymentBatchChequeNumberAllocator(processor.PaymentsForAutoAllocation, processor.PaymentsForAutoAllocation.Factory);
						BusinessObjectFactory.SaveTogether(allocator.GetFactoriesWithAllocationCodeToBeCalledOnSaving());
						isSavedSuccessfully = true;
#if DEBUG
					}
#endif
				}
				else
				{
					List<ITransactionParticipant> participants = new List<ITransactionParticipant>();
					participants.Add(processor.Factory);
					participants.AddRange(processor.ExtraTransactionParticipants);
					processor.ExtraTransactionParticipants.Clear();
					BusinessObjectFactory.SaveTogether(participants.ToArray());
					isSavedSuccessfully = true;
				}
			}
			catch (AllocationSaveException e)
			{
				canPrint = false;
				Globals.Message.ShowError(e.UserFriendlyMessage, Res.GetString("e0efe27c-dd1d-449c-b8a4-9c0affce7d65", "Check Book Busy"));
			}
			catch (AllocationChequeBookException e)
			{
				canPrint = false;
				Globals.Message.ShowError(e.UserFriendlyMessage, Res.GetString("b8cf2fc7-9c39-4ac8-a781-d73c16501df9", "Check Book Full"));
			}
			catch (ENettProcessCreditCardException e)
			{
				canPrint = false;
				ENettProcessCreditCardException ex = e;
				var message = new ZStringBuilder();
				message.AppendLine(Res.GetString("3c310a92-f086-4bc8-ac88-4b317d6e212e", "Failed to process credit card payments via ComPay.\r\nComPay Error: ({0}) {1}\r\n\r\nThe following payments failed:", ex.eNettErrorCode, ex.eNettErrorMessage));
				foreach (PaymentApprovalBase payment in approvals)
				{
					message.AppendLine(Res.GetString("1e764018-fd38-415e-ade1-8784f6614700", "Payment {0} {1} {2} ({3} {4})", payment.Header.OH_Code, payment.AV_PaymentType, payment.AV_ChequeOrReference, payment.AV_RX_NKPaymentCurrency, payment.AV_Amount.ToString(string.Format("N{0}", payment.PaymentCurrency.Decimals))));
					payment.UndoCreateNewPayment();
				}
				Globals.Message.ShowError(message.ToString());
			}
			catch (ENettProcessDirectDebitFxException e)
			{
				canPrint = false;
				ENettProcessDirectDebitFxException ex = e;
				var message = new ZStringBuilder();
				message.AppendLine(Res.GetString("9b6d38a3-7293-4df0-aeaf-2495b278ef96", "Failed to process direct debit FX payment via ComPay.\r\nComPay Error: ({0}) {1}\r\n\r\nThe following payments failed:", ex.eNettErrorCode, ex.eNettErrorMessage));
				foreach (PaymentApprovalBase payment in approvals)
				{
					message.AppendLine(Res.GetString("1e764018-fd38-415e-ade1-8784f6614700", "Payment {0} {1} {2} ({3} {4})", payment.Header.OH_Code, payment.AV_PaymentType, payment.AV_ChequeOrReference, payment.AV_RX_NKPaymentCurrency, payment.AV_Amount.ToString(string.Format("N{0}", payment.PaymentCurrency.Decimals))));
					payment.UndoCreateNewPayment();
				}
				Globals.Message.ShowError(message.ToString());
			}
			catch (Exception e)
			{
				canPrint = false;
				ZExceptionReporting.HandleSaveException(e);
			}

			if (canPrint)
			{
				var allPaymentsForPrinting = new List<TransactionHeader>();
				allPaymentsForPrinting.AddRange(processor.PaymentsForAutoAllocation.ToArray<TransactionHeader>());
				ZBool shouldActivateChequePrinting = ZBool.False;
				foreach (TransactionHeader payment in processor.OtherPayments)
				{
					allPaymentsForPrinting.Add(payment);
					shouldActivateChequePrinting = shouldActivateChequePrinting || !((IChequeNumberAutoAllocation)payment).IsAutoAllocationEnabled;
				}
				if (allPaymentsForPrinting.Count > 0)
				{
					PrintPayments(allPaymentsForPrinting, shouldActivateChequePrinting);
				}
				PromptBankTransferForEPayment(processor, paymentBatchReference);
			}

			if (isSavedSuccessfully)
			{
				AfterSavedAction?.Invoke();
			}
		}

		void PrintPayments(IEnumerable<TransactionHeader> payments, ZBool activateChequePrinting)
		{
#if DEBUG
			if (!Globals.IsTest)
			{
#endif
				PaymentBatchPrintManager printManager = new PaymentBatchPrintManager(payments, null, TransactionTypes.Payment, new BusinessObjectFactory());
				printManager.ChequeIsAutoPrinted = !activateChequePrinting;
				printManager.Print();
#if DEBUG
			}

			Test_PaymentsPassedForPrinting = payments;
			Test_ActivateChequePrinting = activateChequePrinting;
#endif
		}

		#region EPA Bank Transfer Prompt

#if DEBUG
		public List<IZForm> bankTransferFormsPrompted = new List<IZForm>();
#endif

		void PromptBankTransferForEPayment(PaymentApprovalBulkProcessor processor, ZString? paymentBatchReference = null)
		{
			if (AccountingMasterFilesRegistry.Instance.PromptBankTransferOnPostingEPayment.GetFallBackValueAtAllLevels(GlbCompany.CurrentCompany.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty))
			{
				var allPayments = new List<PaymentType>();
				allPayments.AddRange(processor.PaymentsForAutoAllocation.ToArray<PaymentType>());
				allPayments.AddRange(processor.OtherPayments.ToArray<PaymentType>());

				var ePayments = allPayments.Where(x => x.HasAcceptedEPaymentDeal);
				var ePaymentsByBankAccount = ePayments.GroupBy(x => new { x.AH_AB, x.FundingBankAccountPK });
				foreach (var paymentsWithTheSameBankAccount in ePaymentsByBankAccount)
				{
					var form = PrepareAndPromptBankTransfer(paymentsWithTheSameBankAccount, paymentBatchReference);
#if DEBUG
					if (Globals.IsTest)
					{
						bankTransferFormsPrompted.Add(form);
					}
#endif
				}
			}
		}

		internal static void PromptBankTransferForEPayment(PaymentType payment)
		{
			if (AccountingMasterFilesRegistry.Instance.PromptBankTransferOnPostingEPayment.GetFallBackValueAtAllLevels(GlbCompany.CurrentCompany.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty))
			{
				if (payment.HasAcceptedEPaymentDeal)
				{
					PrepareAndPromptBankTransfer(new List<PaymentType>() { payment });
				}
			}
		}

		static IZForm PrepareAndPromptBankTransfer(IEnumerable<PaymentType> paymentsWithTheSameBankAccount, ZString? paymentBatchReference = null)
		{
			var bankTransfer = BankTransfer.PrepareBankTransferFromPayments(paymentsWithTheSameBankAccount, paymentBatchReference);
			var controller = AccountingControllerCreator.GetNewController(TransactionTypes.Transfer, LedgerTypes.CashBook);
			return controller.ShowFormForNewEntity(bankTransfer);
		}

		#endregion

		ZStringBuilder GetErrorMessages(PaymentApprovalBase approval, bool posting, bool populatingChequeNumber)
		{
			var errorMessages = new ZStringBuilder();
			if (posting && !approval.IsFullyApproved)
			{
				if (approval.IsAwaitingApproval)
				{
					errorMessages.AppendLine(Res.GetString("b3a406f5-f1cc-4dba-b7c9-c1bd102949b1", "This Payment is currently awaiting authorization"));
				}
				else if (approval.IsPosted)
				{
					errorMessages.AppendLine(Res.GetString("8a2758bd-4268-473c-ad61-32b9cbabed49", "This Payment is already posted"));
				}
			}

			if (posting && !approval.CheckIfDealIsConfirmedByProvider(out var message))
			{
				errorMessages.AppendLine(message);
			}

			if (approval.AV_Ledger == LedgerTypes.AccountsPayable && approval.PaymentMatchingBaseObject.MatchedTransactions.Any(x =>
																				x is TransactionHeader &&
																				((TransactionHeader)x).OpenQueryClaim != null &&
																				!((TransactionHeader)x).OpenQueryClaim.IsAllowMatch))
			{
				errorMessages.AppendLine(Res.GetString("4ed579d8-482d-4b8c-9e1c-1075247608c6", "One or more invoices attached to this approval is linked to an open claim"));
			}

			if (posting && !populatingChequeNumber && approval.AV_ChequeOrReference.IsEmpty && !((IChequeNumberAutoAllocation)approval).IsAutoAllocationEnabled)
			{
				errorMessages.AppendLine(Res.GetString("43ffdc16-a939-4f35-bb35-5f0b95851ccd", "The Check Number for this Payment is empty"));
			}

			if (approval.HasReversedTransaction)
			{
				errorMessages.AppendLine(Res.GetString("f89ffd51-b3ba-44b1-81ea-dfade87670ee", "This Payment has a reversed transaction"));
			}

			if (approval.IsRejected)
			{
				errorMessages.AppendLine(Res.GetString("D5332DB8-EC48-45BB-BED1-2F3830C907DD", "This payment has been rejected."));
			}

			if (approval.IsCancelled)
			{
				errorMessages.AppendLine(Res.GetString("70D6FAD6-F23D-4BA0-9B5B-0F6558F27942", "This payment has been canceled."));
			}

			if (approval.IsDraft)
			{
				errorMessages.AppendLine(PaymentApprovalWithAuthorisation.DealWithDraftPaymentErrorMessage);
			}

			if (approval.HasErrors)
			{
				errorMessages.AppendLine(approval.Notifications.GetErrors().ToUniqueMessageListString());
			}

			return errorMessages;
		}

		#endregion

		#region Reject Payments

		public void RejectPaymentApprovals(BusinessObject[] paymentApprovals)
		{
			HandlePaymentApprovals(paymentApprovals, ActionType.Reject, Res.GetString("db80b386-a26f-4caf-bd09-72a1ab5e83d3", "Please select one or more Payments to Reject."));
		}

		#endregion

		#region Cancel Payments

		public void CancelPaymentApprovals(BusinessObject[] paymentApprovals)
		{
			HandlePaymentApprovals(paymentApprovals, ActionType.Cancel, Res.GetString("213138be-08ae-41fc-8cf1-112b80cd2fce", "Please select one or more Payments to Cancel."));
		}

		#endregion

		#region Submit For Approval

		public void SubmitForApproval(BusinessObject[] paymentApprovals, bool isEPayment = false)
		{
			HandlePaymentApprovals(
				paymentApprovals,
				ActionType.SubmitForApproval,
				Res.GetString("CDDCDCA9-69AE-4BC1-9E19-A7D3281B5FE0", "Please select one or more Payments to Submit for Approval."),
				isEPayment
			);
		}

		#endregion

		#region

		public void ApproveForPosting(BusinessObject[] paymentApprovals)
		{
			HandlePaymentApprovals(paymentApprovals, ActionType.ApproveForPosting, Res.GetString("EE004C2A-9FD6-4A5B-975E-7378E0826D23", "Please select one or more Payments to Approve for Posting."));
		}

		#endregion

		#region
		public bool IsPaymentAuthorisationRequired()
		{
			return AccountingConfigurationRegistry.Instance.PayableAuthorizationSettings.Value.OfType<PaymentAuthorisationSettings>().Any(setting => setting.AuthorisationRequirement != AuthorisationRequirementCodes.NoApprovalRequired);
		}

		#endregion

		List<PaymentApprovalWithAuthorisation> GetSelectedBusinessObjectsLoadedInNewFactory(BusinessObjectFactory factory, BusinessObject[] paymentApprovals)
		{
			var approvalList = new List<PaymentApprovalWithAuthorisation>();
			var selectedApprovals = factory.Load<PaymentApprovalWithAuthorisation>(new ZQuery(AccPaymentApprovalSchema.PK, paymentApprovals.Select(bizO => bizO.PK).ToArray()));
			if (selectedApprovals.Length > 0)
			{
				foreach (var bizObj in paymentApprovals)
				{
					var approval = selectedApprovals.FirstOrDefault(bizO => bizO.PK == bizObj.PK);
					if (approval != null)
					{
						approvalList.Add(approval);
					}
				}
			}
			return approvalList;
		}

		#region For Test Only
#if DEBUG
		public PaymentBatchChequeNumberAllocator.DummyPaymentBatchChequeNumberAllocator Test_Allocator;
		public List<PaymentApprovalBase> Test_ApprovalsAfterRemovingApprovalsThatCanNotBeProcessed;
		public IEnumerable<TransactionHeader> Test_PaymentsPassedForPrinting;
		public ZBool Test_ActivateChequePrinting;
		public ZBool EnablePrint_ForTestOnly;
		public ZBool Test_DeactivateChequeBookOnAllocation;
#endif
		#endregion
	}
}
