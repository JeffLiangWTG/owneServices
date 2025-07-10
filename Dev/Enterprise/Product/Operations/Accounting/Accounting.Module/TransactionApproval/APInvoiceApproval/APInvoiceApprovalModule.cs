#define CODE_ANALYSIS

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.GenericConsol;
using Enterprise.Accounting.Business.GenericJob;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Business.TransactionApproval;
using Enterprise.Accounting.GUI;
using Enterprise.Accounting.GUI.ARAP.Invoicing;
using Enterprise.Accounting.GUI.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Module.TransactionApproval
{
	public class APInvoiceApprovalModule : InvoicingBaseApprovalModule<APInvoiceChargesApprovalRequest, APInvoiceChargesApprovalRequestDetails>
	{
		public APInvoiceApprovalModule()
		{
		}

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.APInvoiceApproval; }
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			var invoice = selectedBusinessObject as InvoicingBase;
			if (invoice != null)
			{
				return AccountingControllerCreator.GetNewController(invoice, ID);
			}

			return ZControllerFactory.Create(ControllerIDs.APInvoiceApproval);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new APInvoicingBaseApprovalFilterBusinessObject();
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new APInvoiceApprovalFilterControl(GridCollection, FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new APInvoiceChargesApprovalRequestCollection(Factory);
		}

		protected override PerformSearchResult LoadCollection(BusinessObjectFactory factory, Type type, ZQuery query)
		{
			var bizos = new List<BusinessObject>();
			IsViewingRestrictionApplied = false;
			foreach (APInvoiceChargesApprovalRequest approvalRequest in factory.Load(type, query))
			{
				if (approvalRequest.IsAllowedtoViewTransactionOutsideLoginPermission())
				{
					bizos.Add(approvalRequest);
				}
				else
				{
					IsViewingRestrictionApplied = true;
				}
			}
			return PerformSearchResult.Success(factory, query, bizos.ToArray(), permitActiveCollectionUpdates: false);
		}

		protected override CancelStatus AutoFixInvalidCancelApprovals(APInvoiceChargesApprovalRequest approval)
		{
			var cancelStatus = CancelStatus.Valid;
			if (approval.XP_ParentTableCode != AccTransactionHeaderSchema.Constants.Prefix || approval.IsDeleted || approval.XP_ParentID.IsEmpty)
			{
				return cancelStatus;
			}

			var query = new ZDBOnlyQuery(typeof(GenApprovalRequest));
			query.AddToFilter(GenApprovalRequestSchema.XP_ParentID, approval.XP_ParentID);
			query.AddToFilter(GenApprovalRequestSchema.XP_ParentTableCode, AccTransactionHeaderSchema.Constants.Prefix);

			var factory = new BusinessObjectFactory();
			var requests = factory.Load<APInvoiceChargesApprovalRequest>(query);
			var requestsForCancel = requests.Where(x => !IsApprovalInvalidToCancel(x)).ToArray();
			var parentTransaction = factory.Load<InvoicingBase>(approval.XP_ParentID);

			var isPostedTransactionWithRequestsForCancelling = parentTransaction != null && (parentTransaction.IsCancelled || parentTransaction.IsPosted) && requestsForCancel.Any();
			var isUnpostedTransactionWithMultipleRequestsForCancelling = requestsForCancel.Length > 1;

			if (isPostedTransactionWithRequestsForCancelling || isUnpostedTransactionWithMultipleRequestsForCancelling)
			{
				var result = Globals.Message.Show(
					isPostedTransactionWithRequestsForCancelling
					? Res.GetString("CC360182-E432-4f87-A00E-BF5736902E0A", "Transaction '{0}' linked with the request '{1}' is already posted or canceled. Do you prefer system to cancel all relative invalid requests?", approval.ReferenceID, approval.XP_RequestID)
					: Res.GetString("e215d53c-b7be-4be0-9de6-68783f220cb9", "Transaction '{0}' linked with the request '{1}' has multiple non-canceled requests linked to it. Do you prefer system to cancel all relative invalid requests?", approval.ReferenceID, approval.XP_RequestID),
					Res.GetString("9604BE0B-BC96-4a4b-87F0-56A88FD96C22", "Invalid request detected"),
					MessageBoxButtons.YesNo, DialogResult.None);
				if (result == DialogResult.Yes)
				{
					var builder = new ZStringBuilder();
					foreach (var request in requestsForCancel)
					{
						builder.Append(request.XP_RequestID);
						request.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Cancelled;
						request.SetContext(BusinessContext.CancelApprovalRequestAsTransactionAlreadyCancelledOrPosted);
					}
					factory.Save();
					Globals.Message.ShowInformation(Res.GetString("74FE3556-2A14-4527-B624-5DCEE2F6B01F", "Relative invalid requests are canceled. Their request IDs are: {0}", builder.ToStringWithDelimiterBetweenAppends(",")));
					cancelStatus = CancelStatus.Fixed;
				}
				else
				{
					cancelStatus = CancelStatus.InvalidButSkipped;
				}
			}

			return cancelStatus;
		}

		bool IsViewingRestrictionApplied;

		protected override void OnAfterPerformSearchCore()
		{
			if (IsViewingRestrictionApplied)
			{
				Globals.Message.ShowWarning(ViewingRestrictionOutsideLoginWarningMessage, Caption);
			}
		}

		protected MultilingualString ViewingRestrictionOutsideLoginWarningMessage
		{
			get { return ResString.GetMultilingualString("457420a5-a3b3-4d28-8617-651ba29c2a99", "Transaction created in branch / dept outside your login permission are not listed."); }
		}

		protected MultilingualString Caption
		{
			get { return ResString.GetMultilingualString("458ae37d-ddef-48d8-abaa-5ad6c8d69a8c", "Search Results"); }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.APInvoiceApproval; }
		}

		protected override TransactionApprovalBulk<InvoicingBase, APInvoiceChargesApprovalRequest, APInvoiceChargesApprovalRequestDetails> GetNewApprovalBulk(BusinessObjectFactory factory, ISecurityOverrideProvider interactiveSecurityOverrideProvider, params APInvoiceChargesApprovalRequest[] approvalRequests)
		{
			return new APInvoiceChargesApprovalBulk(factory, interactiveSecurityOverrideProvider, approvalRequests);
		}

		protected override ZForm GetNewApprovalBulkForm(TransactionApprovalBulk<InvoicingBase, APInvoiceChargesApprovalRequest, APInvoiceChargesApprovalRequestDetails> bizo, TransactionApprovalFormModes actionMode)
		{
			return new APInvoiceChargesApprovalBulkForm((APInvoiceChargesApprovalBulk)bizo, actionMode);
		}

		public override bool AllowNew
		{
			get { return true; }
		}

		public override bool AllowEdit
		{
			get { return true; }
		}

		public override bool AllowDelete
		{
			get { return true; }
		}

		protected override MenuItem[] GetNewStandardMenuItems()
		{
			var menuItems = new List<MenuItem>(base.GetNewStandardMenuItems());

			NewMenuItem.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("49f79681-930d-42b1-b4a6-3e28093ec5dc", "New I&nvoice"), HandleNewInvoice) { DefaultItem = true });
			NewMenuItem.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("b532d146-a983-41fe-832a-51b10388b83f", "New Cre&dit Note"), HandleNewCreditNote));

			menuItems.Add(new ZMenuItem(ResString.GetMultilingualString("46F3806B-37C5-41A4-828A-5DCB9D4B358A", "P&ost"), HandlePost));
			menuItems.Remove(DeleteMenuItem);
			menuItems.Add(DeleteMenuItem);
			menuItems.Add(new ZMenuItem(ResString.GetMultilingualString("C658EA33-82DE-484E-9885-1D8757E44639", "&Preview"), HandlePreview));

			return menuItems.ToArray();
		}

		protected override ResourceStringData GetDeleteMenuItemText()
		{
			return Res.GetData("9e65fbd9-734d-402f-b68c-64f44b5ab4e4", "&Cancel", "Cancels the selected item after viewing its details read-only (shortcut Del)");
		}

		public override bool SupportsWorkflow => true;
		protected override IZForm ShowEditForm(BusinessObject selectedBusinessObject)
		{
			IZForm result = null;
			var newFactory = new BusinessObjectFactory();
			newFactory.SetContext(APInvoiceChargesApprovalRequest.Context.Editing);
			var selectedRequestInNewFactory = newFactory.Load<APInvoiceChargesApprovalRequest>(selectedBusinessObject.PK);
			if (selectedRequestInNewFactory != null)
			{
				if (selectedRequestInNewFactory.XP_ApprovalStatus != Constants.GenApprovalRequestApprovalStatus.Requested)
				{
					Globals.Message.ShowInformation(Res.GetString("9538547C-74A1-4C25-8CE4-2F4F68C82F11", "Can't Edit request ({0}) - Only requests with status 'Requested' can be edited.", selectedRequestInNewFactory.FormatedRequestId));
				}
				else
				{
					if (selectedRequestInNewFactory.IsTransactionRelated)
					{
						var (invoiceToEdit, restoreSavedDataResult) = selectedRequestInNewFactory.GetLinkedInvoice();
						if (restoreSavedDataResult != null && restoreSavedDataResult.Result != InvoicingBase.RestoreSavedDataResult.ResultType.Success)
						{
							Globals.Message.ShowError(Res.GetString("B015D280-3D6B-4267-B93A-676E94910390", "Request ({0}): {1}.", selectedRequestInNewFactory.FormatedRequestId, restoreSavedDataResult.Error));
						}
						if (invoiceToEdit == null)
						{
							Globals.Message.ShowError(Res.GetString("6BCDE0C8-1CC1-4C08-B7DC-DF42D1A47A50", "A transaction can't be found for request ({0}).", selectedRequestInNewFactory.FormatedRequestId));
						}
						else
						{
							var controller = GetNewController(invoiceToEdit);
#if DEBUG
							SetLastControllerForTest(controller);
#endif
							result = controller.ShowEditForm(invoiceToEdit);
						}
					}
					else if (selectedRequestInNewFactory.IsJobRelated)
					{
						var job = newFactory.Load<Job>(selectedRequestInNewFactory.XP_ParentID);
						var genericJob = (GenericJob)job.Factory.LoadGenericJob(job);

						var jobParent = genericJob.Consumer as BusinessObject;
						ControllerID consumerControllerID = genericJob.GetConsumerController();

						if (consumerControllerID != null)
						{
							var controller = ZControllerFactory.Create(consumerControllerID);
#if DEBUG
							SetLastControllerForTest(controller);
#endif
							result = controller.ShowEditForm(jobParent);
						}
						else
						{
							Globals.Message.ShowError(Res.GetString("51d219f0-7cb3-4994-95e5-e27165adaea9", "Cannot open corresponding operations job."));
						}
					}
					else
					{
						var genericConsol = newFactory.Load<GenericConsol>(selectedRequestInNewFactory.XP_ParentID);
						if (genericConsol != null)
						{
							var controller = ZControllerFactory.Create(genericConsol.GetParentConsolController());
#if DEBUG
							SetLastControllerForTest(controller);
#endif
							result = controller.ShowEditForm(genericConsol);
						}
						else
						{
							Globals.Message.ShowInformation(Res.GetString("478C2604-0F86-4FF4-A263-C3BF454A2C4C", "Can't Edit request ({0}) - editing is not supported.", selectedRequestInNewFactory.FormatedRequestId));
						}
					}
				}
			}

			return result;
		}

		protected override void HandleDeleteClickCore(object sender, EventArgs e)
		{
			HandleCancel(sender, e);
		}

		protected override IModuleDecisionProvider CreateDefaultModuleDecisionProvider()
		{
			return new DecisionProvider(this);
		}

		void HandleNewInvoice(object sender, EventArgs e)
		{
			var controller = AccountingControllerCreator.GetNewController(TransactionTypes.Invoice, LedgerTypes.AccountsPayable, ID);
#if DEBUG
			SetLastControllerForTest(controller);
#endif
			controller.Factory.SetContext(APInvoiceChargesApprovalRequest.Context.Editing);
			controller.ShowNewForm();
		}

		void HandleNewCreditNote(object sender, EventArgs e)
		{
			var controller = AccountingControllerCreator.GetNewController(TransactionTypes.CreditNote, LedgerTypes.AccountsPayable, ID);
#if DEBUG
			SetLastControllerForTest(controller);
#endif
			controller.Factory.SetContext(APInvoiceChargesApprovalRequest.Context.Editing);
			controller.ShowNewForm();
		}

		void HandlePost(object sender, EventArgs e)
		{
			var securityCheckPoint = Env.Security.APInvoiceApproval_Post;
			if (!securityCheckPoint.IsAllowed)
			{
				securityCheckPoint.ShowError();
			}
			else
			{
				var errorMessages = new ZStringBuilder();
				ZGuid[] selectedRequestPKs = Grid.SelectedElements.Select(x => x.PK).ToArray();
				if (selectedRequestPKs.Length == 0)
				{
					Globals.Message.ShowInformation(Res.GetString("7e2a8d3e-2ce5-4ea0-ba11-ef7b3337bad3", "Please select a record in the grid."));
				}
				else
				{
					var newFactory = new BusinessObjectFactory();
					var selectedRequestsInNewFactory = newFactory.Load<APInvoiceChargesApprovalRequest>(new ZQuery(GenApprovalRequestSchema.PK, selectedRequestPKs));
					if (selectedRequestsInNewFactory.Length > 1 || (selectedRequestsInNewFactory.Length == 1 && selectedRequestsInNewFactory[0].IsTransactionRelated))
					{
						if (selectedRequestsInNewFactory.Any(x => !x.IsTransactionRelated))
						{
							var messageToShow = new ZStringBuilder(Res.GetString("F73C3D04-507B-467B-B96D-0E7FBEE359A1", "To post multiple approval requests, they must all be 'Transaction' ref type."));
							Globals.Message.ShowInformation(messageToShow.ToString());
						}
						else
						{
							var invoicesToPost = new List<InvoicingBase>();
							foreach (var selectedRequestPK in selectedRequestPKs)
							{
								var newFactoryForPosting = new BusinessObjectFactory();
								newFactoryForPosting.SetContext(APInvoiceChargesApprovalRequest.Context.Posting);
								var request = newFactoryForPosting.Load<APInvoiceChargesApprovalRequest>(selectedRequestPK);
								if (request.XP_ApprovalStatus != Constants.GenApprovalRequestApprovalStatus.Approved)
								{
									errorMessages.AppendLine(Res.GetString("4DE6B5EC-8DC6-4E79-B2B2-497726A78E2E", "Request ({0}) - Only requests with status 'Approved' can be posted.", request.FormatedRequestId));
								}
								else
								{
									var (invoiceToPost, restoreSavedDataResult) = request.GetLinkedInvoice();
									if (restoreSavedDataResult != null && restoreSavedDataResult.Result != InvoicingBase.RestoreSavedDataResult.ResultType.Success)
									{
										errorMessages.AppendLine(Res.GetString("20994487-B773-4B24-B5CD-9E7242DB61A2", "Request ({0}): {1}.", request.FormatedRequestId, restoreSavedDataResult.Error));
									}
									if (invoiceToPost == null)
									{
										errorMessages.AppendLine(Res.GetString("CCD9C8AC-0EB6-469A-BBDF-EA097A4CFE1D", "A transaction can't be found for request ({0}).", request.FormatedRequestId));
									}
									else
									{
										invoicesToPost.Add(invoiceToPost);
									}
								}
							}

							if (invoicesToPost.Count > 0)
							{
								foreach (InvoicingBase invoice in invoicesToPost)
								{
									var controller = GetNewController(invoice);
#if DEBUG
									SetLastControllerForTest(controller);
#endif
									IZForm result = controller.ShowEditForm(invoice);
								}
							}
						}
					}
					else
					{
						var request = selectedRequestsInNewFactory[0];
						if (request.XP_ApprovalStatus != Constants.GenApprovalRequestApprovalStatus.Approved)
						{
							errorMessages.AppendLine(Res.GetString("B42CA97A-D1FB-41D3-AE1A-039CB158E06C", "Request ({0}) - Only requests with status 'Approved' can be posted.", request.FormatedRequestId));
						}
						else if (request.IsJobRelated || request.IsConsolRelated)
						{
							var postingErrorMessage = PostManagerGUIWrapper.PostRequest(request);
							if (!string.IsNullOrEmpty(postingErrorMessage))
							{
								errorMessages.AppendLine(Res.GetString("CFCC7938-A492-4BB4-BCD8-52F0E3F3F8CE", "Request ({0}) - {1}", request.FormatedRequestId, postingErrorMessage));
							}
						}
						else
						{
							errorMessages.AppendLine(Res.GetString("67977194-D93C-46B2-939D-7202F8537171", "Request ({0}) - reference type {1} is invalid.", request.FormatedRequestId, request.ReferenceType));
						}
					}

					if (errorMessages.Length > 0)
					{
						var messageToShow = new ZStringBuilder(Res.GetString("DAF1CECE-415D-453B-A8E2-DE8D9FD89950", "The following request(s) cannot be posted."));
						messageToShow.AppendLine();
						messageToShow.Append(errorMessages);
						Globals.Message.ShowInformation(messageToShow.ToString());
					}
				}
			}
		}

		void HandlePreview(object sender, EventArgs e)
		{
			var securityCheckPoint = Env.Security.APInvoiceApproval_Print;
			if (!securityCheckPoint.IsAllowed)
			{
				securityCheckPoint.ShowError();
			}
			else
			{
				var errorMessages = new ZStringBuilder();
				ZGuid[] selectedRequestPKs = Grid.SelectedElements.Select(x => x.PK).ToArray();
				if (selectedRequestPKs.Length == 0)
				{
					Globals.Message.ShowInformation(Res.GetString("7e2a8d3e-2ce5-4ea0-ba11-ef7b3337bad3", "Please select a record in the grid."));
				}
				else
				{
					var newFactory = new BusinessObjectFactory();
					var selectedRequestsInNewFactory = newFactory.Load<APInvoiceChargesApprovalRequest>(new ZQuery(GenApprovalRequestSchema.PK, selectedRequestPKs));
					var invoicesToPrint = new List<InvoicingBase>();
					InvoicingBase invoiceToPrint;
					if (selectedRequestsInNewFactory.Length > 1 || (selectedRequestsInNewFactory.Length == 1 && selectedRequestsInNewFactory[0].IsTransactionRelated))
					{
						if (selectedRequestsInNewFactory.Any(x => !x.IsTransactionRelated))
						{
							var messageToShow = new ZStringBuilder(Res.GetString("A07E614F-0014-474C-A95A-99ED2334E176", "To preview multiple approval requests, they must all be 'Transaction' ref type."));
							Globals.Message.ShowInformation(messageToShow.ToString());
						}
						else
						{
							foreach (var request in selectedRequestsInNewFactory)
							{
								var validStatuses = new[] { Constants.GenApprovalRequestApprovalStatus.Requested, Constants.GenApprovalRequestApprovalStatus.Approved, Constants.GenApprovalRequestApprovalStatus.Posted };
								if (!validStatuses.Contains((string)request.XP_ApprovalStatus))
								{
									var validStatusesAsString = new ZStringBuilder(validStatuses.Select(x => string.Format("'{0}'", x))).ToStringWithDelimiterBetweenAppends(", ");
									errorMessages.AppendLine(Res.GetString("6846EB02-705D-4EDA-828C-A3DB835F6F6C", "Request ({0}) - Only approvals with status {1} can be previewed.", request.FormatedRequestId, validStatusesAsString));
								}
								else
								{
									(invoiceToPrint, var restoreSavedDataResult) = request.GetLinkedInvoice();
									if (restoreSavedDataResult != null && restoreSavedDataResult.Result != InvoicingBase.RestoreSavedDataResult.ResultType.Success)
									{
										errorMessages.AppendLine(Res.GetString("47161044-9df1-4e01-88a2-b826ce43e2e2", "Request ({0}): {1}.", request.FormatedRequestId, restoreSavedDataResult.Error));
									}
									if (invoiceToPrint == null)
									{
										errorMessages.AppendLine(Res.GetString("B0E6DD64-C89C-450B-B04D-362EC631C647", "A transaction can't be found for request ({0}).", request.FormatedRequestId));
									}
									else if (!invoiceToPrint.Header.OH_IsActive)
									{
										errorMessages.AppendLine(Res.GetString("A85514B6-9822-430B-8F38-E8CEE9DECD2A", "Transaction {0} cannot be previewed because it is for an inactive organization", invoiceToPrint.AH_TransactionNum));
									}
									else
									{
										invoiceToPrint.MoveFromIncompleteToPayableLedger();
										invoicesToPrint.Add(invoiceToPrint);
									}
								}
							}
						}
					}
					else
					{
						var request = selectedRequestsInNewFactory[0];
						var validStatuses = new[] { Constants.GenApprovalRequestApprovalStatus.Requested, Constants.GenApprovalRequestApprovalStatus.Approved };
						if (!validStatuses.Contains((string)request.XP_ApprovalStatus))
						{
							var validStatusesAsString = new ZStringBuilder(validStatuses.Select(x => string.Format("'{0}'", x))).ToStringWithDelimiterBetweenAppends(", ");
							errorMessages.AppendLine(Res.GetString("8CB5A970-9CDC-4771-B9F5-70E16E4AC69E", "Request ({0}) - Only approvals with status {1} can be previewed.", request.FormatedRequestId, validStatusesAsString));
						}
						else
						{
#if DEBUG
							if (Globals.IsTest)
							{
								PostManagerGUIWrapper.ParentFormForInvoicePreview_ForTestOnly = ParentFormForInvoicePreview_ForTestOnly;
							}
#endif
							try
							{
								var getInvoiceToPreviewResult = PostManagerGUIWrapper.GetInvoiceToPreviewRequestCostConfirmationDocument(request);
								if (!string.IsNullOrEmpty(getInvoiceToPreviewResult.ErrorMessage))
								{
									errorMessages.AppendLine(Res.GetString("8f899c7c-9786-42f4-9ae0-e97c30c30b9b", "Request ({0}) - {1}", request.FormatedRequestId, getInvoiceToPreviewResult.ErrorMessage));
								}
								else
								{
									invoicesToPrint.Add(getInvoiceToPreviewResult.InvoiceToPreview);
								}
							}
							finally
							{
#if DEBUG
								if (Globals.IsTest)
								{
									PostManagerGUIWrapper.ParentFormForInvoicePreview_ForTestOnly = null;
								}
#endif
							}
						}
					}

					if (invoicesToPrint.Count > 0)
					{
						var invoiceFactory = invoicesToPrint[0].Factory;
						InvoicePrintHelper.PrintCostConfirmationDocument(invoiceFactory, false, invoicesToPrint.ToArray());

						var autofatturaInvoicesToPrint = new List<InvoicingBase>();
						foreach (InvoicingBase invoice in invoicesToPrint)
						{
							if (invoice.SupportPrintAutofatturaDocument() && AccountingConfigurationRegistry.Instance.PrintAutofatturaItalyDocument.Value)
							{
								autofatturaInvoicesToPrint.Add(invoice);
							}
						}

						if (autofatturaInvoicesToPrint.Count > 0)
						{
							InvoicePrintHelper.PrintITAutoFattura(autofatturaInvoicesToPrint.ToArray());
						}
					}

					if (errorMessages.Length > 0)
					{
						var messageToShow = new ZStringBuilder(Res.GetString("6AA6D1F2-7569-412F-9D96-939BD06F6604", "The following request(s) cannot be previewed."));
						messageToShow.AppendLine();
						messageToShow.Append(errorMessages);
						Globals.Message.ShowInformation(messageToShow.ToString());
					}
				}
			}
		}

#if DEBUG
		public Form ParentFormForInvoicePreview_ForTestOnly;
		public ZController LastController_ForTestOnly;

		void SetLastControllerForTest(ZController controller)
		{
			if (Globals.IsTest)
			{
				LastController_ForTestOnly = controller;
			}
		}
#endif
	}
}
