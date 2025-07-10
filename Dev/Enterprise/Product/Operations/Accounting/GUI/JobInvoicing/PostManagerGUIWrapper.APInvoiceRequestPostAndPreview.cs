using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.GenericConsol;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Business.TransactionApproval;
using Enterprise.Accounting.GUI.JobInvoicing.BatchPosting;
using Enterprise.Accounting.GUI.JobInvoicing.ConsolCosting;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.GUI.JobInvoicing
{
	public abstract partial class PostManagerGUIWrapper
	{
		#region Request Post And Preview

		[SuppressMessage("Microsoft.Performance", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes", Justification = "Used for parameter passing")]
		public struct PostRequestResult : IApprovalRequestPostingResult
		{
			public string ErrorMessage { get; set; }
			public IBulkPostingCache BulkPostingCache { get; set; }
		}

		class BulkPostingCache : IBulkPostingCache
		{
			public BulkPostingCache()
			{
				bulkPostingDataCollector = new BulkPostingDataCollector();
			}

			public BulkPostingDataCollector bulkPostingDataCollector { get; }

			public void FinalizePosting(IEnumerable<ZGuid> postedTransactions)
			{
				JobBatchInvoicingPostManagerGUIWrapper.CheckCreditLimitExceeded(bulkPostingDataCollector);

				var combinedBulkPostingDataCollectorForPrinting = new BulkPostingDataCollector(bulkPostingDataCollector);
				combinedBulkPostingDataCollectorForPrinting.AllAPInvoicesAndCreditNotesExcludingUAInvoicesAndUACreditNotePKs.AddRange(postedTransactions);
				JobBatchInvoicingPostManagerGUIWrapper.BatchPrintJobInvoicing(combinedBulkPostingDataCollectorForPrinting);
			}
		}

		public static string PostRequest(APInvoiceChargesApprovalRequest request)
		{
			return PostRequest(request, null, false).ErrorMessage;
		}

		public static IApprovalRequestPostingResult PostRequest(APInvoiceChargesApprovalRequest request, IBulkPostingCache bulkPostingCache)
		{
			return PostRequest(request, bulkPostingCache, true);
		}

		public static void BulkPrintAPInvoicesAndCreditNotes(IEnumerable<ZGuid> postedTransactionPKs)
		{
			var bulkPostingDataCollector = new BulkPostingDataCollector();
			bulkPostingDataCollector.AllAPInvoicesAndCreditNotesExcludingUAInvoicesAndUACreditNotePKs.AddRange(postedTransactionPKs);
			JobBatchInvoicingPostManagerGUIWrapper.BatchPrintJobInvoicing(bulkPostingDataCollector);
		}

		static PostRequestResult PostRequest(APInvoiceChargesApprovalRequest request, IBulkPostingCache bulkPostingCache, bool isBulkPosting)
		{
			Argument.NotNull(request, nameof(request));

			var postingCache = isBulkPosting ? bulkPostingCache as BulkPostingCache : null;
			if (isBulkPosting && postingCache == null)
			{
				postingCache = new BulkPostingCache();
			}

			string errorMessage = null;
			if (request.IsJobRelated)
			{
				errorMessage = PostJobRelatedRequest(request, postingCache);
			}
			else if (request.IsConsolRelated)
			{
				errorMessage = PostConsolRelatedRequest(request, postingCache);
			}
			else
			{
				errorMessage = Res.GetString("8c94d5be-7071-4a22-99bf-2a411f44c861", "Request reference type '{0}' is not supported.", request.ReferenceType);
			}

			return new PostRequestResult
			{
				ErrorMessage = errorMessage,
				BulkPostingCache = postingCache
			};
		}

		public static InvoiceToPreviewOrErrorMessage GetInvoiceToPreviewRequestCostConfirmationDocument(APInvoiceChargesApprovalRequest request)
		{
			Argument.NotNull(request, nameof(request));

			if (request.IsJobRelated)
			{
				return PreviewJobRelatedRequestCostConfirmationDocument(request);
			}
			else if (request.IsConsolRelated)
			{
				return GetInvoiceToPreviewConsolRelatedRequestCostConfirmationDocument(request);
			}
			else
			{
				return new InvoiceToPreviewOrErrorMessage(Res.GetString("8c94d5be-7071-4a22-99bf-2a411f44c861", "Request reference type '{0}' is not supported.", request.ReferenceType));
			}
		}

		#region Job related request

		static string PostJobRelatedRequest(APInvoiceChargesApprovalRequest request, BulkPostingCache postingCache)
		{
			string errorMessage;
			var wrapper = GetJobRelatedPostManagerGUIWrapper(request, out errorMessage);
			if (string.IsNullOrEmpty(errorMessage))
			{
				wrapper.OverriddenNothingPostedMessage = GetNothingToPostMessage();
				if (postingCache != null)
				{
					wrapper.bulkPostingDataCollector = postingCache.bulkPostingDataCollector;
					wrapper.IsPostedForBatchInvoicing = true;
				}
				wrapper.PostForExistingRequest(request);
				errorMessage = wrapper.RequestPreviewOrBulkPostErrorMessages;
			}

			return errorMessage;
		}

		static InvoiceToPreviewOrErrorMessage PreviewJobRelatedRequestCostConfirmationDocument(APInvoiceChargesApprovalRequest request)
		{
			string errorMessage;
			var wrapper = GetJobRelatedPostManagerGUIWrapper(request, out errorMessage);
			if (string.IsNullOrEmpty(errorMessage))
			{
#if DEBUG
				if (Globals.IsTest)
				{
					wrapper.ParentForm = ParentFormForInvoicePreview_ForTestOnly; //when tests are run in DAT then we do not have main form open that's why we have to set a parent form
				}
#endif
				wrapper.OverriddenNothingPostedMessage = GetNothingToPreviewMessage();

				return wrapper.GetInvoiceToPreviewForExistingRequest(request);
			}
			else
			{
				return new InvoiceToPreviewOrErrorMessage(errorMessage);
			}
		}

		static InvoicingPostManagerGUIWrapper GetJobRelatedPostManagerGUIWrapper(APInvoiceChargesApprovalRequest request, out string errorMessage)
		{
			errorMessage = "";

			var newFactory = request.Factory;
			Job jobReloaded = newFactory.Load<Job>(request.XP_ParentID);
			if (jobReloaded != null)
			{
				jobReloaded.InitializeParentFromGenericJobWithoutSettingDefaults();
				var wrapper = new InvoicingPostManagerGUIWrapper(JobInvoicingPostingOption.Costs, newFactory, jobReloaded, null);

				return wrapper;
			}
			else
			{
				errorMessage = Res.GetString("5D6A8A91-8255-4CF5-BEBF-760017C56991", "Job was not found.");

				return null;
			}
		}

		#endregion

		#region Consol related request

		#region Post

		static string PostConsolRelatedRequest(APInvoiceChargesApprovalRequest request, BulkPostingCache postingCache)
		{
			var result = DoActionWithConsolRelatedPostManagerGUIWrapper(request, PostWithConsolRelatedPostManagerGUIWrapper, postingCache);

			return result.ErrorMessage;
		}

		static InvoiceToPreviewOrErrorMessage PostWithConsolRelatedPostManagerGUIWrapper(APInvoiceChargesApprovalRequest request, ConsolInvoicingPostManagerGUIWrapper wrapper)
		{
			wrapper.OverriddenNothingPostedMessage = GetNothingToPostMessage();
			wrapper.SupressProfitSharePrintTask = true;
			wrapper.PostForExistingRequest(request);

			return new InvoiceToPreviewOrErrorMessage(wrapper.RequestPreviewOrBulkPostErrorMessages);
		}

		#endregion

		#region Preview

		static InvoiceToPreviewOrErrorMessage GetInvoiceToPreviewConsolRelatedRequestCostConfirmationDocument(APInvoiceChargesApprovalRequest request)
		{
			return DoActionWithConsolRelatedPostManagerGUIWrapper(request, GetInvoiceToPreviewFromConsolRelatedPostManagerGUIWrapper, null);
		}

		static InvoiceToPreviewOrErrorMessage GetInvoiceToPreviewFromConsolRelatedPostManagerGUIWrapper(APInvoiceChargesApprovalRequest request, ConsolInvoicingPostManagerGUIWrapper wrapper)
		{
#if DEBUG
			if (Globals.IsTest)
			{
				wrapper.ParentForm = ParentFormForInvoicePreview_ForTestOnly; //when tests are run in DAT then we do not have main form open that's why we have to set a parent form
			}
#endif
			wrapper.OverriddenNothingPostedMessage = GetNothingToPreviewMessage();
			wrapper.SupressProfitSharePrintTask = true;
			wrapper.AllowedReportDeliveryOption = Enterprise.DocumentEngine.AllowedDeliveryOptions.PreviewOnly;

			return wrapper.GetInvoiceToPreviewForExistingRequest(request);
		}

		#endregion

		delegate InvoiceToPreviewOrErrorMessage actionWithConsolRelatedPostManagerGUIWrapper(APInvoiceChargesApprovalRequest request, ConsolInvoicingPostManagerGUIWrapper wrapper);

		static InvoiceToPreviewOrErrorMessage DoActionWithConsolRelatedPostManagerGUIWrapper(APInvoiceChargesApprovalRequest request, actionWithConsolRelatedPostManagerGUIWrapper action, BulkPostingCache postingCache)
		{
			var newFactory = request.Factory;
			var consol = GenericConsol.GetIJobCostingPlugInByPK(newFactory, request.XP_ParentID, request.XP_ParentTableCode);
			if (consol != null)
			{
				using (ApportionmentPlugin plugIn = new ApportionmentPlugin((IBusiness)consol))
				{
					var wrapper = new ConsolInvoicingPostManagerGUIWrapper(JobInvoicingPostingOption.ConsolCosts, newFactory, plugIn.Jobs, consol, null, plugIn.Apportionments);
					if (postingCache != null)
					{
						wrapper.bulkPostingDataCollector = postingCache.bulkPostingDataCollector;
						wrapper.IsPostedForBatchInvoicing = true;
					}

					return action(request, wrapper);
				}
			}
			else
			{
				return new InvoiceToPreviewOrErrorMessage(Res.GetString("971A1AB9-182F-464B-B855-EB361082B61D", "Consol was not found."));
			}
		}

		#endregion

		static MultilingualString GetNothingToPreviewMessage()
		{
			return ResString.GetMultilingualString("F2F9934A-8434-47BA-BCF0-2CDAD0474DD9", @"No preview available as there are no charges valid for posting with the request Creditor and Invoice Number.");
		}

		static MultilingualString GetNothingToPostMessage()
		{
			return ResString.GetMultilingualString("19116A4B-3B54-47E4-848A-AB1741096C91", @"Nothing to post as there are no charges valid for posting with the request Creditor and Invoice Number.");
		}

		#region Preview

		InvoiceToPreviewOrErrorMessage GetInvoiceToPreviewForExistingRequest(APInvoiceChargesApprovalRequest request)
		{
			this.requestToCompare = request;
			action = PostManagerAction.PrepareTransactionsForPreview;
			var transactions = Perform();

			var errorMessage = RequestPreviewOrBulkPostErrorMessages;
			APInvoice invoiceToPreview = null;
			if (string.IsNullOrEmpty(errorMessage) && transactions != null)
			{
				var invoices = transactions.GetAllAPInvoicesAndCreditNotes().OfType<APInvoice>().ToArray();
				if (invoices.Length > 1)
				{
					ErrorReporter.ReportOnce("Only one Invoice expected to preview request.");
				}
				else if (invoices.Length == 1)
				{
					invoiceToPreview = invoices[0];
				}
			}
			if (invoiceToPreview == null && string.IsNullOrEmpty(errorMessage))
			{
				errorMessage = Res.GetString("96BD11F8-0882-4999-BAEA-D147EF197F97", "Related invoice was not created.");
			}

			return string.IsNullOrEmpty(errorMessage) ? new InvoiceToPreviewOrErrorMessage(invoiceToPreview) : new InvoiceToPreviewOrErrorMessage(errorMessage);
		}

		ZString RequestPreviewOrBulkPostErrorMessages
		{
			get { return previewOrBulkPostErrorMessages != null ? previewOrBulkPostErrorMessages.ToStringWithNewLineBetweenAppends() : ""; }
			set
			{
				if (previewOrBulkPostErrorMessages == null)
				{
					previewOrBulkPostErrorMessages = new ZStringBuilder();
				}
				if (!value.IsEmpty)
				{
					if (!previewOrBulkPostErrorMessages.IsEmpty)
					{
						previewOrBulkPostErrorMessages.AppendLine();
					}
					previewOrBulkPostErrorMessages.Append(value);
				}
			}
		}
		ZStringBuilder previewOrBulkPostErrorMessages;

		#endregion

		void PostForExistingRequest(APInvoiceChargesApprovalRequest request)
		{
			requestToCompare = request;
			action = IsPostedForBatchInvoicing ? PostManagerAction.BulkPost : PostManagerAction.Post;
			Perform();
		}

		APInvoiceChargesApprovalRequest requestToCompare;

		public class InvoiceToPreviewOrErrorMessage
		{
			public InvoiceToPreviewOrErrorMessage()
			{
			}

			public InvoiceToPreviewOrErrorMessage(APInvoice invoice)
			{
				invoiceToPreview = invoice;
				invoiceToPreview.SetContext(BusinessContext.UnapprovedAPInvoiceCreatedForRequestPreview);
			}

			public InvoiceToPreviewOrErrorMessage(string error)
			{
				Argument.NotNull(error, nameof(error));

				errorMessaage = error;
			}

			public APInvoice InvoiceToPreview { get { return invoiceToPreview; } }
			public string ErrorMessage { get { return errorMessaage; } }

			readonly APInvoice invoiceToPreview;
			readonly string errorMessaage = "";
		}

		#endregion
	}
}
