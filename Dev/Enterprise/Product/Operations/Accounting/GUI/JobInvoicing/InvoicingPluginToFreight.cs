using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.AccountingPresentationProviders;
using Enterprise.Accounting.Business.ARAP.CashAdvance;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Invoicing.Printing;
using Enterprise.Accounting.Business.Base.Reversing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Business.JobInvoicing.Posting;
using Enterprise.Accounting.Business.JobInvoicing.ProfitShare;
using Enterprise.Accounting.Business.Presentation;
using Enterprise.Accounting.GUI.ARAP.Invoicing;
using Enterprise.Accounting.GUI.ARAP.UnapprovedAPTransaction;
using Enterprise.Accounting.GUI.Base;
using Enterprise.Accounting.Integration;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Integration.QuotedBooking;
using Enterprise.Integration.Accounting;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Accounting.CriticalValidation;
using Enterprise.MasterFiles.CreditControl.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Rating.Business;
using Enterprise.Rating.GUI.RateSelector.Services;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.PlugIn;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Accounting.GUI.JobInvoicing.JobChargeUserControl;

namespace Enterprise.Accounting.GUI.JobInvoicing
{
	public partial class InvoicingPluginToFreight : ZAlwaysLoadPlugIn, IPluginShouldRefreshMenuForGateway
	{
		public InvoicingPluginToFreight(IBusiness hostEntity)
			: this(hostEntity, true)
		{
			InitializePlugIn(hostEntity);
		}

		protected InvoicingPluginToFreight(IBusiness hostEntity, bool initializeTabPage) : base(hostEntity)
		{
			if (Factory != null)
			{
				jobLoader = new Job.Loader(Factory, PlugInParent);
			}

			if (initializeTabPage)
			{
				TabPage.Binding += delegate
				{ BindUserControlsIfRequired(); };
			}
		}

		protected void InitializePlugIn(IBusiness hostEntity)
		{
			if (Factory != null)
			{
				SetBusinessContexts();
				Factory.Saved += Factory_Saved;
				Factory.Saving += Factory_Saving;
			}

			if (hostEntity is CommonShipment shipment)
			{
				shipment.JS_INCOInfo.ValueChanged += Shipment_JS_INCOInfo_ValueChanged;
				shipment.Consols.CountChanged += Shipment_Consols_CountChanged;
			}

			chargesWithSuspendedValidation = new HashSet<Charge>();
		}

		void SetBusinessContexts()
		{
			Factory.SetContext(BusinessContext.InvoicingPlugInGUI);
			if (!(HostBusinessEntity is ForwardingConsol))
			{
				Factory.SetContext(BusinessContext.InvoicingPluginGUIExcludingConsol);
			}
		}

		readonly Job.Loader jobLoader;

		HashSet<Charge> chargesWithSuspendedValidation;

		protected override void UnHookFormEventsCore()
		{
			if (HostBusinessEntity != null)
			{
				HostBusinessEntity.HasChangesChanged -= BusinessEntity_HasChangesChanged;
			}
			if (queuedJobLoadTask != null)
			{
				queuedJobLoadTask.Dispose();
				queuedJobLoadTask = null;
			}

			if (HostBusinessEntity is CommonShipment shipment)
			{
				shipment.JS_INCOInfo.ValueChanged -= Shipment_JS_INCOInfo_ValueChanged;
				shipment.Consols.CountChanged -= Shipment_Consols_CountChanged;
			}

			if (HostBusinessEntity is ForwardingConsol && JobChargeUserControl != null)
			{
				JobChargeUserControl.RelatedJobFilterUpdated -= JobChargeUserControl_RelatedJobFilterUpdated;
			}

			base.UnHookFormEventsCore();
		}

		#region Actions

		#region Printing Filter

		protected JobARInvoicePrintingFilter fARPrintingFilter;

		protected JobAPInvoicePrintingFilter fAPPrintingFilter;

		protected JobDraftInvoicePrintingFilter draftInvoiceFilter;

#if DEBUG
		protected JobAPInvoicePrintingFilter APPrintingFilter_ForTestOnly => new JobAPInvoicePrintingFilter(HostBusinessEntity, Job.PK);

		protected JobARInvoicePrintingFilter ARPrintingFilter_ForTestOnly => new JobARInvoicePrintingFilter(HostBusinessEntity, Job.PK);
#endif

		void SafeRefreshInvoiceList()
		{
			SafeRefreshARInvoiceList();
			SafeRefreshAPInvoiceList();
		}

		void SafeRefreshARInvoiceList()
		{
			if (fARPrintingFilter != null)
			{
				fARPrintingFilter.RefreshInvoiceList();
			}
		}

		void SafeRefreshAPInvoiceList()
		{
			if (fAPPrintingFilter != null)
			{
				fAPPrintingFilter.RefreshInvoiceList();
			}
		}

		void ResetPrintingFilter()
		{
			fARPrintingFilter = null;
		}

		void ResetAPPrintingFilter()
		{
			fAPPrintingFilter = null;
		}

		#endregion

		#region Inactive Invoicing Job
		void MenuItemMarkJobHeaderAsInactive_Click(object sender, EventArgs e)
		{
			if (!SecurityHelper.CheckIsAllowedForSecurityCheckPoint(SecurityCore.DeleteJob))
			{
				SecurityHelper.ShowError(SecurityCore.DeleteJob);
			}
			else if (Job.IsInDatabase && HostBusinessEntity.HasChanges)
			{
				Globals.Message.ShowError(Res.GetString("CB66D409-1872-4160-8E15-FAF10E47FEE8", "Please save this {0} before marking the job inactive", PlugInParent.InvoicingSupporter.ConsumerType.Description));
			}
			else if (!IsInvoiceDeletionAllowedByParent(PlugInParent))
			{
				Globals.Message.ShowError(Res.GetString("3D86EFA8-93E7-485D-B40F-1F6F1380ED50", "This invoice cannot be marked as inactive."));
			}
			else
			{
				var reasonNotAbleToDeactive = Job.CheckIfCanDeactiveJobInCurrentCompany();
				if (!reasonNotAbleToDeactive.IsEmpty)
				{
					Globals.Message.ShowInformation(reasonNotAbleToDeactive);
				}
				else
				{
					var gatewayLockedChildShipmentsJobsErrorsMessage = GatewaySellToCostSynchroniser.GetGatewayApportionmentListingLockedChildShipmentsJobsErrorMessage(Job);
					if (!gatewayLockedChildShipmentsJobsErrorsMessage.IsEmpty)
					{
						Globals.Message.ShowError(gatewayLockedChildShipmentsJobsErrorsMessage);
					}
					else
					{
						PromptUserAndDeactiveInvoice();
					}
				}
			}
		}

		#region PromptUserAndDeactiveInvoice

		void PromptUserAndDeactiveInvoice()
		{
			if (Job.IsInDatabase)
			{
				var result = Globals.Message.ShowConfirmation(Res.GetString("F0DDBDAC-0E47-49A1-B8CD-8BED61424D0F", "Please type 'YES' to confirm deactivation of job.\r\nOn saving, the job will be deactivated and the form will be closed."), Res.GetString("AD60C02B-B593-46C1-B2D4-3E231E3E0EF1", "Mark Invoicing Job as Inactive"), Res.GetString("69EEFEC0-02F7-426D-BB20-FC27DBDA5559", "Yes"), MessageBoxIcon.Information);

				if (result == DialogResult.OK)
				{
					try
					{
						Factory.SetContext(BusinessContext.JobIsManuallyDeactivating);
						DeactivateJobSaveAndClosedForm();
					}
					finally
					{
						Factory.RemoveContext(BusinessContext.JobIsManuallyDeactivating);
					}
				}
			}
			else
			{
				var result = Globals.Message.Show(Res.GetString("74699315-41FF-48C6-AC3B-ECD7667975FF", "Invoicing job has not been created"), Res.GetString("AD60C02B-B593-46C1-B2D4-3E231E3E0EF1", "Mark Invoicing Job as Inactive"), MessageBoxButtons.OK, MessageBoxIcon.Question);

				if (result == DialogResult.OK)
				{
					DeactivateJobAndShowCoveringLabel();
				}
			}
		}

		void DeactivateJobSaveAndClosedForm()
		{
			if (Job != null && Job.JH_IsActive)
			{
				Job.MarkAsInactive();
				Job = null;
			}

			if (Form != null)
			{
				Form.FireSaveButton();
				Form.Close();
			}
		}

		void DeactivateJobAndShowCoveringLabel()
		{
			if (Job != null && Job.JH_IsActive)
			{
				try
				{
					ShowCoveringLabelForDeactive();
					Job.MarkAsInactive();
				}
				finally
				{
					Job = null;
					isDeactivatingJob = false;
					jobHasBeenDeactivated = true;
					HostBusinessEntity.HasChanges = true;
				}
			}
		}

		void ShowCoveringLabelForDeactive()
		{
			SelectTabPage();
			TabPage.ClearNotificationImage();
			IsJobHookedUp = false;
			ResetBusinessEntityToNull();
			isDeactivatingJob = true;
			ShowCoveringLabel(JobWillBeDeactivatedOnSaveMessage);
			SynchroniseIfTabPageVisible();
			DiscardCurrentUserControl();
			ResetPrintingFilter();
			ResetAPPrintingFilter();
			userControlsBound = false;
		}

		bool isDeactivatingJob;
		bool jobWasDeactivatedBySomeoneElse;
		bool jobWasDeactivatedBySomeOtherForm;
		bool jobHasBeenDeactivated;

		#endregion

		#endregion

		#region Delete Invoicing Job

		#region IsForbiddenByParentToDelete

		public static bool IsInvoiceDeletionAllowedByParent(IJobInvoicingPlugIn parent)
		{
			return parent == null || parent.AllowInvoiceDeletion;
		}

		#endregion

		#endregion

		#region Delete Unposted Charges

		void MenuItemDeleteUnPosted_Click(object sender, EventArgs e)
		{
			if (!SecurityHelper.CheckIsAllowedForSecurityCheckPoint(SecurityCore.DeleteUnpostedLine))
			{
				SecurityHelper.ShowError(SecurityCore.DeleteUnpostedLine);
			}
			else if (CheckIsReadyForFinancialClosureWithoutModifySecurity)
			{
				Globals.Message.ShowError(Res.GetString("D71EE418-FADF-43AA-A58B-3B1AEB6EC96A", @"Cannot delete unposted lines from this job, because it has Ready For Financial Closure status."));
			}
			else
			{
				if (Job != null)
				{
					Job.DeleteUnpostedLines();
				}
			}
		}

		#endregion

		#region Reset Unposted Charge Invoice Type

		void MenuItemRecalculateInvoiceType_Click(object sender, EventArgs e)
		{
			if (!SecurityHelper.CheckIsAllowedForSecurityCheckPoint(SecurityCore.ResetUnpostedLine))
			{
				SecurityHelper.ShowError(SecurityCore.ResetUnpostedLine);
			}
			else if (CheckIsReadyForFinancialClosureWithoutModifySecurity)
			{
				Globals.Message.ShowError(Res.GetString("760CE178-9F5F-4FBC-9533-D643234B7979", @"Cannot reset invoice types of unposted lines from this job, because it has Ready For Financial Closure status."));
			}
			else
			{
				if (Job != null)
				{
					Job.ResetUnpostedLinesInvoiceType();
				}
			}
		}

		#endregion

		#region Reset Default Debtor On Unposted Lines

		void ResetDefaultDebtorOnUnpostedLines()
		{
			if (Job != null)
			{
				Job.ResetDefaultDebtorOnUnpostedLines();
			}
		}

		void MenuItemResetDefaultDebtorOnUnpostedLines_Click(object sender, EventArgs e)
		{
			if (CheckIsReadyForFinancialClosureWithoutModifySecurity)
			{
				Globals.Message.ShowError(Res.GetString("65FE5197-073B-43F7-9E59-52A990A97CD8", @"Cannot reset default debtor on unposted lines from this job, because it has Ready For Financial Closure status."));
				return;
			}

			ResetDefaultDebtorOnUnpostedLines();
		}

		void Shipment_JS_INCOInfo_ValueChanged(object sender, EventArgs e)
		{
			if (Job != null && !Job.IsInDatabase)
			{
				ResetDefaultDebtorOnUnpostedLines();
			}
		}

		void Shipment_Consols_CountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			ResetStandaloneShipmentMenuItemsAvailability();
		}

		void ResetStandaloneShipmentMenuItemsAvailability()
		{
			if (MenuItemAutorateCostsSTS != null)
			{
				MenuItemAutorateCostsSTS.Enabled = PlugInParent.InvoicingSupporter.IsStandaloneShipment;
				MenuItemAutorateCostsSTSRevenue.Enabled = PlugInParent.InvoicingSupporter.IsStandaloneShipment;
			}
		}

		#endregion

		#region Reverse Invoices

		SecurityCheckpoint ReverseStandardInvoiceSecurity
		{
			get { return SecurityHelper.GetInvSecurity(SecurityCore.ReverseBilling); }
		}

		SecurityCheckpoint ReverseSelfBilledInvoiceSecurity
		{
			get { return SecurityHelper.GetInvSecurity(SecurityCore.ReverseSelfBilled); }
		}

		SecurityCheckpoint ReverseStandardInvoiceWithPaidAPInvoiceSecurity
		{
			get { return SecurityHelper.GetInvSecurity(SecurityCore.AllowReversalWhenRelatedAPTrArePaid); }
		}

		SecurityCheckpoint ReverseSelfBilledInvoiceWithPaidAPInvoiceSecurity
		{
			get { return SecurityHelper.GetInvSecurity(SecurityCore.AllowSelfBilledReversalWhenRelatedAPTrArePaid); }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		void MenuItemReverseInvoices_Click(object sender, EventArgs e)
		{
			if (CheckCanNotPostOrCreateTransaction)
			{
				Globals.Message.ShowError(Res.GetString("977B0EDA-4CD1-41E3-8464-3CB0629B0D4C", @"Cannot reset reverse invoices and redo billing from this job, because it has Ready For Financial Closure status."));
				return;
			}

			bool result = true;
			ReverseReceivablesInvoiceWhenAPTransactionsArePaidSecurityOverrideProvider paidRelatedInvoicesSecurityProvider = null;

			if (Job != null)
			{
				var approvalGUIProvider = new InvoicingPostManagerGUIWrapper(JobInvoicingPostingOption.Revenue, Factory, Job, JobChargeUserControl.FindForm()).ARCreditNoteApprovalGUIProvider;
				var reverser = new JobInvoicingReverser(Job, delegate(InvoicingBase[] transactions)
				{
					var reversingProvider = new MultipleReversingProviderForHeader();
					reversingProvider.BizObjectsForReversing.AddRange(transactions);

					var arInvoices = transactions.Where(x => x is ARInvoice);
					if (arInvoices.Any())
					{
						result = CheckSecurityRightsForReversingInvoice(arInvoices);
						if (result)
						{
							paidRelatedInvoicesSecurityProvider = new ReverseReceivablesInvoiceWhenAPTransactionsArePaidSecurityOverrideProvider(reversingProvider);

							foreach (var invoice in arInvoices)
							{
								SecurityOverrideProviderSource.Get(invoice).Provider = paidRelatedInvoicesSecurityProvider;
								ReversingFactory reversingFactory = new ReversingFactory();
								ReversingBase arInvoiceReverser = reversingFactory.NewReversing(invoice, SecurityHelper);
								bool canReverse = arInvoiceReverser.CanReverseTransaction;
								result &= canReverse;

								if (!canReverse)
								{
									string message = arInvoiceReverser.CantReverseErrorMessage;
									string arInvoiceMessage = invoice.IsSelfBillingInvoice ? Env.Security.ReverseARSelfBilledInvoiceWhenAPArePaid.ErrorMessageForNotAllowed : Env.Security.ReverseReceivablesInvoiceWhenAPTransactionsArePaid.ErrorMessageForNotAllowed;
									if (message.Contains(arInvoiceMessage))
									{
										message = message.Replace(arInvoiceMessage, invoice.IsSelfBillingInvoice ? ReverseSelfBilledInvoiceWithPaidAPInvoiceSecurity.ErrorMessageForNotAllowed : ReverseStandardInvoiceWithPaidAPInvoiceSecurity.ErrorMessageForNotAllowed);
									}
									Globals.Message.ShowError(message);

									break; // Stop Reversing
								}
							}
						}
					}
					else
					{
						result = false;
						Globals.Message.ShowError(Res.GetString("393a58c8-aa44-4e1a-a368-c0016931aeaf", "No appropriate invoices were found for reversing."), Res.GetString("ab3b74d4-e2ef-4b26-9238-e6454064296c", "Nothing was reversed."));
					}
				}, approvalGUIProvider);

				string validationErrors = reverser.IsValidToReverseAllInvoices();
				if (!string.IsNullOrEmpty(validationErrors))
				{
					Globals.Message.ShowError(validationErrors);
				}
				else
				{
					if (Job.IsClosed && !JobReopenSecurityCheckHelper.CanReopenJob_InteractiveSecurityCheck(Job, Job))
					{
						result = false;
					}

					if (result)
					{
						string reversingReason = "";
						string reversingCode = "";
						GetReversingReason(ref reversingReason, ref reversingCode);

						if (!string.IsNullOrEmpty(reversingReason))
						{
							ReverseAllInvoicesCore(reverser, reversingReason, reversingCode);
							if (reverser.ContinueWithSave)
							{
								try
								{
									if (reverser.ContinueWithApprovalFactorySave && approvalGUIProvider.FactoryForApprovalRequests != null)
									{
										reverser.ReversingFactory.ChildFactories.Add(approvalGUIProvider.FactoryForApprovalRequests);
									}
									reverser.ReversingFactory.Save();
									SecurityOverrideProviderSource.Get(Job).Provider = new JobInvoicingSecurityOverrideProvider();
									if (PlugInParent != null)
									{
										PlugInParent.InvoicingSupporter.PostedStateChanged();
									}
								}
								catch (ZSaveConcurrencyException)
								{
									Globals.Message.ShowError(Res.GetString("a6f66784-ad2d-4b81-b1dd-5de54db324e2", "While you were working, another user has modified this job. Please try again."));
								}
								catch (Exception ex) when (!ex.IsCriticalException())
								{
									ZExceptionReporting.HandleSaveException(ex);
								}
							}
							else if (reverser.ContinueWithApprovalFactorySave && approvalGUIProvider.FactoryForApprovalRequests != null)
							{
								try
								{
									approvalGUIProvider.FactoryForApprovalRequests.Save();
								}
								catch (ZSaveConcurrencyException)
								{
									Globals.Message.ShowError(Res.GetString("7e8c78ff-0c31-4a49-a673-0515d6c6f8e6", "While you were working, another user has modified this job. Please try again."));
								}
								catch (Exception ex) when (!ex.IsCriticalException())
								{
									ZExceptionReporting.HandleSaveException(ex);
								}
							}
							else if (reverser.Errors.Count > 0)
							{
								Globals.Message.ShowError(reverser.Errors.ToString());
							}
							else
							{
								Globals.Message.ShowWarning(Res.GetString("ea9538ab-5504-4712-97ed-3b1b673db947", "Since you canceled, nothing was reversed."));
							}
						}
					}
				}
			}
		}

		bool CheckSecurityRightsForReversingInvoice(IEnumerable<InvoicingBase> arInvoices)
		{
			var isAllowedToReverse = true;
			var hasStandardInvoice = arInvoices.Any(x => !((ARInvoice)x).IsSelfBillingInvoice);
			var hasSelfBilledInvoice = arInvoices.Any(x => ((ARInvoice)x).IsSelfBillingInvoice);
			if (hasStandardInvoice && hasSelfBilledInvoice)
			{
				var errorMessage = ZString.Empty;
				if (!(ReverseStandardInvoiceSecurity.IsAllowed && ReverseSelfBilledInvoiceSecurity.IsAllowed))
				{
					errorMessage = GetSecurityErrorMessageForReversing(ReverseStandardInvoiceSecurity, ReverseSelfBilledInvoiceSecurity);
				}
				else
				{
					errorMessage = CheckSecurityRightsForReversingInvoiceWithRelatedPaidAPInvoice(arInvoices);
				}
				if (!errorMessage.IsEmpty)
				{
					isAllowedToReverse = false;
					Globals.Message.ShowError(errorMessage.ToString(), Res.GetString("2dfd2e26-4eb1-4e9f-b405-f5a1c4576382", "Access Denied"));
				}
			}
			else if (hasStandardInvoice && !ReverseStandardInvoiceSecurity.IsAllowed)
			{
				isAllowedToReverse = false;
				ReverseStandardInvoiceSecurity.ShowError();
			}
			else if (hasSelfBilledInvoice && !ReverseSelfBilledInvoiceSecurity.IsAllowed)
			{
				isAllowedToReverse = false;
				ReverseSelfBilledInvoiceSecurity.ShowError();
			}
			return isAllowedToReverse;
		}

		ZString CheckSecurityRightsForReversingInvoiceWithRelatedPaidAPInvoice(IEnumerable<InvoicingBase> arInvoices)
		{
			var errorMessage = ZString.Empty;
			var invoicesWithRelatedPaidAPInvoice = new List<InvoicingBase>();
			foreach (var inv in arInvoices)
			{
				if (inv.RelatedInvoices.ToArray<InvoicingBase>().Any(x =>
						x.AH_Ledger == LedgerTypes.AccountsPayable &&
						x.AH_TransactionType == TransactionTypes.Invoice &&
						!x.AH_IsCancelled && ((IMatching)x).IsMatched))
				{
					invoicesWithRelatedPaidAPInvoice.Add(inv);
				}
			}

			if (invoicesWithRelatedPaidAPInvoice.Any(x => !((ARInvoice)x).IsSelfBillingInvoice) &&
				invoicesWithRelatedPaidAPInvoice.Any(x => ((ARInvoice)x).IsSelfBillingInvoice) &&
				(!ReverseStandardInvoiceWithPaidAPInvoiceSecurity.IsAllowed || !ReverseSelfBilledInvoiceWithPaidAPInvoiceSecurity.IsAllowed))
			{
				errorMessage = GetSecurityErrorMessageForReversing(ReverseStandardInvoiceWithPaidAPInvoiceSecurity, ReverseSelfBilledInvoiceWithPaidAPInvoiceSecurity);
			}
			return errorMessage;
		}

		ZString GetSecurityErrorMessageForReversing(SecurityCheckpoint reverseStandardInvoice, SecurityCheckpoint reverseSelfBilledInvoice)
		{
			var errorMessage = new ZStringBuilder();
			errorMessage.AppendLine(Res.GetString("611376a9-40ad-410f-85ce-62ec275fbdfc", "There are mix of standard and self billed invoices on this job."));

			var invoiecType = ZString.Empty;
			var standardInvoiceType = Res.GetString("cf226b48-7270-4d14-a977-c10aa7995616", "standard");
			var selfBilledInvoiceType = Res.GetString("72d275a2-8955-4055-afce-49aeee42be30", "self-billed");
			var manualReverseMessage = ZString.Empty;
			if (!reverseStandardInvoice.IsAllowed && !reverseSelfBilledInvoice.IsAllowed)
			{
				invoiecType = Res.GetString("2463f799-20ae-44ee-bcf7-bbcc1438c2a8", "standard and self-billed");
			}
			else
			{
				invoiecType = !reverseStandardInvoice.IsAllowed ? standardInvoiceType : selfBilledInvoiceType;
				manualReverseMessage = Res.GetString("b3c3dc7e-512c-4fa8-a2ae-cb2ee380c4ec", "Alternatively, you may manually reverse the {0} invoices.", !reverseStandardInvoice.IsAllowed ? selfBilledInvoiceType : standardInvoiceType);
			}
			errorMessage.AppendLine(Res.GetString("7f8157d3-167b-45a2-b63e-616aef0444ca", "You do not have the appropriate security rights to reverse {0} invoices.", invoiecType));
			errorMessage.AppendLine();
			errorMessage.AppendLine(Res.GetString("84bf148c-06a7-43fe-b860-3c7818841da4", "If you require access to this function, ask your system administrator to change either your staff or Group Security Rights to allow access to:"));
			if (!reverseStandardInvoice.IsAllowed)
			{
				errorMessage.AppendLine(reverseStandardInvoice.DisplayTextPathToSecurityRight);
			}
			if (!reverseSelfBilledInvoice.IsAllowed)
			{
				errorMessage.AppendLine(reverseSelfBilledInvoice.DisplayTextPathToSecurityRight);
			}
			if (!manualReverseMessage.IsEmpty)
			{
				errorMessage.AppendLine();
				errorMessage.AppendLine(manualReverseMessage);
			}

			return errorMessage.ToString();
		}

		protected virtual void ReverseAllInvoicesCore(JobInvoicingReverser reverser, string reversingReason, string reversingCode)
		{
			reverser.ReverseAllInvoices(reversingReason, reversingCode, ShowReversalDatesForm);
		}

		bool? ShowReversalDatesForm(TransactionHeaderCollection reversedInvoices)
		{
			return ARReversalDateHelper.DefaultReversalDatesJobRelated(reversedInvoices, PlugInParent, ShowReversalDatesForm, false);
		}

		bool? ShowReversalDatesForm(TransactionHeaderCollection reversedInvoices, ChangeTransactionDatesBusinessObject changeTransactionDatesBusinessObject)
		{
			var invoiceDateConfigurationHelper = new InvoiceDateConfigurationHelper(new OperationsJobConfigurationCodes(Job.PlugInData), Job.PlugInData, true);
			var invoiceDateConfiguration = invoiceDateConfigurationHelper.FindInvoiceDateConfiguration();

			var canModifyTransactionDate = (invoiceDateConfiguration != null && (invoiceDateConfiguration.Today || invoiceDateConfiguration.Override));
			SecurityHelper.CheckIsAllowedForSecurityCheckPoint(SecurityCore.ModifyTransactionDate);
			var canModifyPostDate = AccountingConfigurationRegistry.Instance.BackDateInvoicesConfiguration.Value.OverridePostDate &&
				SecurityHelper.CheckIsAllowedForSecurityCheckPoint(SecurityCore.ModifyPostDate);

			if (canModifyTransactionDate || canModifyPostDate)
			{
				var dialog = new ReversalDatesForm(reversedInvoices, changeTransactionDatesBusinessObject);
				var dialogResult = ZFormModaliser.ShowDialogAndDispose(dialog);
				switch (dialogResult)
				{
					case DialogResult.Yes:
						return true;
					case DialogResult.No:
						return false;
					case DialogResult.Cancel:
					default:
						return null;
				}
			}
			else
			{
				return true; // I.e. continue with the default dates
			}
		}

#if DEBUG
		protected virtual
#endif
 void GetReversingReason(ref string reversingReason, ref string reversingCode)
		{
			TransactionReasonHolder reversingHolder = new TransactionReasonHolder(null);
			ZFormModaliser.ShowDialogAndDispose(new TransactionReasonForm(reversingHolder, Res.GetString("9cf6330a-3c7e-4649-ba86-90d6d711b59c", "Please enter the reason for Reversing this transaction"), Res.GetString("683ba051-670a-4abd-a0bb-db212bc58c7d", "Reversing Reason")));
			if (!string.IsNullOrEmpty(reversingHolder.Reason))
			{
				reversingReason = Res.GetString("f24f49e2-31b8-4aa7-91d5-80375f51efb1", "- {0} - {1} Entered By {2}", reversingHolder.Code, reversingHolder.Reason, GlbStaff.CurrentUser.GS_LoginName);
			}

			if (!string.IsNullOrEmpty(reversingHolder.Code))
			{
				reversingCode = reversingHolder.Code;
			}
		}

		#endregion

		#region Unapproved AP invoices

		void MenuItemImportAPInvoices_Click(object sender, EventArgs e)
		{
			if (CheckCanNotPostOrCreateTransaction)
			{
				Globals.Message.ShowError(Res.GetString("262DD5F1-DE79-4A92-9B1F-196C6A300EE1", @"Cannot import AP invoices issued by other group companies from this job, because it has Ready For Financial Closure status."));
				return;
			}

			string jobAndParentSaved = new InvoicingPostManagerGUIWrapper(JobInvoicingPostingOption.All, Factory, Job, JobChargeUserControl.FindForm()).ValidateJobAndParentSaved();
			if (string.IsNullOrEmpty(jobAndParentSaved))
			{
				if (ValidateAndSaveNewlyCreatedAndNotSavedJobToLoadFromNewFactory(Res.GetString("6BD52A0F-8985-40CE-9876-045D19CA50B7", "Import AP invoices")))
				{
					var jobUniqueRefQuery = new ZQuery(AccTransactionHeaderSchema.AH_JobNumber, Job.JH_JobNum);

					if (PlugInParent.InvoicingSupporter.ConsumerType != JobInvoicingConsumerTypes.GatewayConsol)
					{
						jobUniqueRefQuery.AddToFilter(JoinCondition.And, AccTransactionHeaderSchema.AH_JH, SQLComparisonOperator.NotEqual, null);
					}

					var factory = new BusinessObjectFactory();
					var additionalFiltersForConverter = new ZQuery(jobUniqueRefQuery);
					ZDBOnlyQuery gatewayTargetJobQuery;
					if (PlugInParent is ForwardingConsol forwardingConsol
						&& (gatewayTargetJobQuery = GatewayTargetJobQueryCreator.CreateQueryForInvoiceTargetJob(factory, forwardingConsol)) != null)
					{
						additionalFiltersForConverter.AddToFilter(gatewayTargetJobQuery, JoinCondition.Or);
					}
					else if (PlugInParent is ForwardingShipment forwardingShipment
						&& (gatewayTargetJobQuery = GatewayTargetJobQueryCreator.CreateQueryForInvoiceTargetJob(factory, forwardingShipment)) != null)
					{
						additionalFiltersForConverter.AddToFilter(gatewayTargetJobQuery, JoinCondition.Or);
					}

					var converter = new UnapprovedTransactionConverter(factory, additionalFiltersForConverter);
					ZFormModaliser.ShowDialogAndDispose(new UnapprovedTransactionAuthorisationForm(converter) { Owner = JobChargeUserControl.FindForm() });
				}
			}
			else
			{
				Globals.Message.ShowError(jobAndParentSaved);
			}
		}

		#endregion

		#region Recognize Revenue using relevant date

		void MenuItemRecognizeRevenue_Click(object sender, EventArgs e)
		{
			if (!SecurityHelper.CheckIsAllowedForSecurityCheckPoint(SecurityCore.RecognizeRevenue))
			{
				SecurityHelper.ShowError(SecurityCore.RecognizeRevenue);
			}
			else if (!Job.IsInDatabase || HostBusinessEntity.HasChanges)
			{
				Globals.Message.ShowError(Res.GetString("14CDC156-AC3E-42D2-9E35-8BDD93EDDE2A", "Please save this {0} before revenue recognition.", PlugInParent.InvoicingSupporter.ConsumerType.Description));
			}
			else
			{
				if (Job != null)
				{
					bool areREVandCSTLinesUpdated = false;

					try
					{
						var factoryForFixedLines = new BusinessObjectFactory() { NameForDebugging = "InvoicingPluginToFreight_MenuItemRecognizeRevenueClick" };
						FixRelatedLinesCore(Job, factoryForFixedLines);
						try
						{
							factoryForFixedLines.Save();
						}
						catch (ZSaveException ex)
						{
							ZExceptionReporting.HandleSaveException(ex);
						}
						Job.Charges.RefreshBinding();

						Job.ShouldUseImmediateRevenueRecognisedDate += new EventHandler<UserQueryEventArgs>(Job_ShouldUseImmediateRevenueRecognisedDate);
						Job.ResetPreviouseRespose_ShouldUseImmediateRevenueRecognisedDate();

						JobValidation jobValidation = Job.Validation as JobValidation;
						if (jobValidation != null)
						{
							string warnings = jobValidation.GetRevenueRecognitionDateValidationErrors();
							if (!string.IsNullOrEmpty(warnings))
							{
								Globals.Message.ShowWarning(warnings);
							}
						}

						Job.ApplyRevenueRecognitionDateForWholeJob(out areREVandCSTLinesUpdated);
					}
					finally
					{
						Job.ShouldUseImmediateRevenueRecognisedDate -= new EventHandler<UserQueryEventArgs>(Job_ShouldUseImmediateRevenueRecognisedDate);
					}

					if (PlugInParent != null)
					{
						PlugInParent.InvoicingSupporter.PostedStateChanged();
					}

					if (Job.HasChanges || areREVandCSTLinesUpdated)
					{
						Job.RunPreSaveValidation();
						if (!Job.HasErrors)
						{
							try
							{
								Factory.Save();
							}
							catch (ZSaveException ex)
							{
								ZExceptionReporting.HandleSaveException(ex);
							}
						}
						else
						{
							Globals.Message.ShowError(Res.GetString("7DC6F188-6257-439C-A5AB-100190C6FCB4", "After revenue recognition the job contains errors and can't be saved."));
						}
					}
				}
			}
		}

		void MenuItemFixRevenueRecognitionData_Click(object sender, EventArgs e)
		{
			if (Job != null)
			{
				var provider = GetInvoicingPluginToFreightPresentationProvider();
				var result = provider.FixRevenueRecognitionData(
					message => Globals.Message.Show(message, Res.GetString("D8700470-BE73-45F7-B572-92DDB6FFA904", "Confirmation"), MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes);

				if (!string.IsNullOrEmpty(result))
				{
					Globals.Message.ShowInformation(result);
				}
			}
		}

		protected virtual void FixRelatedLinesCore(Job job, BusinessObjectFactory factoryForFixedLines)
		{
			Job.FixRelatedLinesWithEmptyRecognitionType(factoryForFixedLines);
		}

		#endregion

		#region Posting / Preview

		void PreviewTransactions(JobInvoicingPostingOption previewOption)
		{
			new InvoicingPostManagerGUIWrapper(previewOption, Factory, Job, JobChargeUserControl.FindForm()).Preview();
		}

		void PostTransactions(JobInvoicingPostingOption postingOption)
		{
			if (Job != null && !Job.HasErrors)
			{
				foreach (Charge charge in Job.Charges)
				{
					charge.Debtors.Load(new ZQuery(OrgHeaderSchema.PK, charge.JR_OH_SellAccount));
					charge.Validation.ValidateJR_OH_SellAccount();
					charge.Validation.ValidateJR_InvoiceType();
				}
				if (Job.HasErrors)
				{
					Globals.Message.ShowError(Res.GetString("2c7d67ad-0daf-46b2-ad2a-0b4810e4b0ad", @"This job cannot be posted because it has errors. 
Please fix the errors and retry posting."));
					return;
				}

				string reasonNotToAllowPosting = Job.ReasonNotToAllowPosting;
				if (!string.IsNullOrEmpty(reasonNotToAllowPosting))
				{
					Globals.Message.ShowError(reasonNotToAllowPosting);
					return;
				}

				if (postingOption == JobInvoicingPostingOption.All || postingOption == JobInvoicingPostingOption.Costs || postingOption == JobInvoicingPostingOption.CustomsDSBChargeAPOnly)
				{
					string reasonNotToAllowPostCost = Job.ReasonNotToAllowPostCost;

					if (!string.IsNullOrEmpty(reasonNotToAllowPostCost))
					{
						Globals.Message.ShowError(Res.GetString("05f53916-ee69-4175-9c7d-506254856b94", @"This job cannot be posted for costs because {0}. 

Please retry posting for other types invoices.", reasonNotToAllowPostCost));
						return;
					}
				}

				if (postingOption == JobInvoicingPostingOption.All || postingOption == JobInvoicingPostingOption.Revenue ||
					postingOption == JobInvoicingPostingOption.Agent || postingOption == JobInvoicingPostingOption.LocalClient ||
					postingOption == JobInvoicingPostingOption.CustomsDSBChargeAROnly || postingOption == JobInvoicingPostingOption.Disbursement)
				{
					if (Job.JH_Status != JobHeaderStatus.JobInvoiced.Code
						&& AccountingConfigurationRegistry.Instance.DoesJobStatusGetChangedToInvoicedWhenARInvoicePosted(Job.JH_Status)
						&& ((JobValidation)Job.Validation).IsProfitLossReasonCodeInvalidForThisJobStatus(JobHeaderStatus.JobInvoiced.Code))
					{
						Globals.Message.ShowError(Res.GetString("732fb348-ba81-451e-8aec-6657027770c6", @"This job cannot be posted for revenue because it requires a valid Profit/Loss Reason when Job Status will be automatically set to Invoiced after posting the first AR Invoice. 

Please enter a Profit/Loss Reason, save and retry posting."));
						return;
					}
				}
			}

			Action refreshChargesAction = null;
			if (Job.JobType == JobInvoicingConsumerTypes.WarehouseStorage)
			{
				refreshChargesAction = () => { JobChargeUserControl.JobChargeBoundGrid.Refresh(); };
			}

			var wrapper = new InvoicingPostManagerGUIWrapper(postingOption, Factory, Job, JobChargeUserControl.FindForm(), refreshChargesAction);

#if DEBUG

			wrapper = PreparePostingWrapperForTest(wrapper);

#endif

			wrapper.Post();

			if (PlugInParent != null)
			{
				PlugInParent.InvoicingSupporter.PostedStateChanged();
			}

			ValidateCreditLimits(CreditLimitCheckMode.ClearCache);
		}

#if DEBUG
		protected virtual InvoicingPostManagerGUIWrapper PreparePostingWrapperForTest(InvoicingPostManagerGUIWrapper wrapperAsPerProductionCode)
		{
			InvoicingPostWrapper_ForTestOnly = wrapperAsPerProductionCode;
			return wrapperAsPerProductionCode;
		}

		public InvoicingPostManagerGUIWrapper InvoicingPostWrapper_ForTestOnly;

#endif

		#region Post All Charges

		void MenuItemPostAll_Click(object sender, EventArgs e)
		{
			if (!SecurityHelper.CheckIsAllowedForSecurityCheckPoint(SecurityCore.PostAll))
			{
				SecurityHelper.ShowError(SecurityCore.PostAll);
			}
			else if (!SecurityHelper.CheckIsAllowedForSecurityCheckPoint(SecurityCore.PostCost))
			{
				SecurityHelper.ShowError(SecurityCore.PostCost);
			}
			else
			{
				PostTransactions(JobInvoicingPostingOption.All);
				SafeRefreshInvoiceList();
			}
		}

		#endregion

		#region Post Local Client Charges

		void MenuItemPostBillTo_Click(object sender, EventArgs e)
		{
			if (!SecurityHelper.CheckIsAllowedForSecurityCheckPoint(SecurityCore.PostLocalClient))
			{
				SecurityHelper.ShowError(SecurityCore.PostLocalClient);
			}
			else
			{
				PostTransactions(JobInvoicingPostingOption.LocalClient);
				SafeRefreshARInvoiceList();
			}
		}

		#endregion

		#region Post Agent Charges

		void MenuItemPostAgent_Click(object sender, EventArgs e)
		{
			if (!SecurityHelper.CheckIsAllowedForSecurityCheckPoint(SecurityCore.PostOverseas))
			{
				SecurityHelper.ShowError(SecurityCore.PostOverseas);
			}
			else
			{
				PostTransactions(JobInvoicingPostingOption.Agent);
				SafeRefreshARInvoiceList();
			}
		}

		#endregion

		#region Post Both Charges

		void MenuItemPostBoth_Click(object sender, EventArgs e)
		{
			if (!SecurityHelper.CheckIsAllowedForSecurityCheckPoint(SecurityCore.PostRevenue))
			{
				SecurityHelper.ShowError(SecurityCore.PostRevenue);
			}
			else
			{
				PostTransactions(JobInvoicingPostingOption.Revenue);
				SafeRefreshARInvoiceList();
			}
		}

		#endregion

		void MenuItemRequestCashAdvance_Click(object sender, EventArgs e)
		{
			if (!SecurityHelper.CheckIsAllowedForSecurityCheckPoint(SecurityCore.RequestCashAdvance))
			{
				SecurityHelper.ShowError(SecurityCore.RequestCashAdvance);
			}
			else
			{
				var requestor = new ARCashAdvanceRequestor(Job);
				var validationErrors = requestor.GetValidErrorsBeforeGenerateRequest();
				if (!string.IsNullOrEmpty(validationErrors))
				{
					Globals.Message.ShowError(validationErrors);
				}
				else
				{
					var validationWarnings = requestor.GetValidWarningsBeforeGenerateRequest();
					if (!string.IsNullOrEmpty(validationWarnings))
					{
						Globals.Message.ShowWarning(validationWarnings);
					}
					var request = requestor.GenerateRequests();
					if (!request.Item1.IsEmpty)
					{
						Globals.Message.ShowInformation(request.Item1);
						PrintCashAdvanceRequests(request.Item2);
						RefreshCashAdvances();
					}
				}
			}
		}

		void PrintCashAdvanceRequests(IEnumerable<CashAdvanceRequestHeader> requestsToPrint)
		{
			if (requestsToPrint != null)
			{
				try
				{
					foreach (var request in requestsToPrint)
					{
						var message = Res.GetString("1b3c6749-14f2-4747-8787-7b845ecf19b7", @"Do you want to print Advance Payment Request
{0}, {1}, {2}, {3}", request.Organization.OH_Code, request.CAH_RequestReferenceNumber, request.TransactionCurrency.RX_Code, request.CAH_OSAmount.ToString(request.TransactionCurrency.Decimals));
						var title = Res.GetString("3dabf89e-f81b-4ef6-9383-444947994d78", "Print Advance Payment Request BRANCH: {0} - Company: {1}", request.JobBranch, request.Company?.CompanyName);

						if (Globals.Message.Show(message, title, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
						{
							new CashAdvancePrintTask(new CashAdvanceRequestHeader[] { request }).Run();
						}
					}
				}
				catch (UnableToFindInvoiceDocumentCommandException ex)
				{
					Globals.Message.ShowError(ex.Message);
				}
			}
		}

		internal void RefreshCashAdvances()
		{
			Job.ResetCashAdvanceRequests();
			CashAdvanceRequestUserControl.RefreshCashAdvances(Job.CashAdvanceRequests);
		}

		#region Preview Invoices

		void PreviewInvoices_Click(object sender, EventArgs e)
		{
			PreviewInvoices(JobInvoicingPostingOption.Revenue);
		}

		void PreviewCosts_Click(object sender, EventArgs e)
		{
			PreviewInvoices(JobInvoicingPostingOption.Costs);
		}

		void PreviewInvoices(JobInvoicingPostingOption option)
		{
			if (!SecurityHelper.CheckIsAllowedForSecurityCheckPoint(SecurityCore.PreviewInvoices))
			{
				SecurityHelper.ShowError(SecurityCore.PreviewInvoices);
			}
			else if (!SecurityHelper.CheckIsAllowedForSecurityCheckPoint(SecurityCore.PreviewOnly))
			{
				SecurityHelper.ShowError(SecurityCore.PreviewOnly);
			}
			else
			{
				PreviewTransactions(option);
			}
		}

		#endregion

		#region Post Disbursement Charges

		void MenuItemPostDisbursement_Click(object sender, EventArgs e)
		{
			if (!SecurityHelper.CheckIsAllowedForSecurityCheckPoint(SecurityCore.PostDSB))
			{
				SecurityHelper.ShowError(SecurityCore.PostDSB);
			}
			else
			{
				PostTransactions(JobInvoicingPostingOption.Disbursement);
				SafeRefreshARInvoiceList();
			}
		}

		#endregion

		#region Post Sister company charges

		void MenuItemPostAllSisterCompanyCharges_Click(object sender, EventArgs e)
		{
			if (!SecurityHelper.CheckIsAllowedForSecurityCheckPoint(SecurityCore.PostAllSisterCompanyCharges))
			{
				SecurityHelper.ShowError(SecurityCore.PostAllSisterCompanyCharges);
			}
			else
			{
				PostTransactions(JobInvoicingPostingOption.AllSisterCompanyCharges);
				SafeRefreshARInvoiceList();
			}
		}

		#endregion

		#region Post Sister company in current login company charges

		void MenuItemPostLocalSisterCompanyChargesOnly_Click(object sender, EventArgs e)
		{
			if (!SecurityHelper.CheckIsAllowedForSecurityCheckPoint(SecurityCore.PostLocalSisterCompanyChargesOnly))
			{
				SecurityHelper.ShowError(SecurityCore.PostLocalSisterCompanyChargesOnly);
			}
			else
			{
				PostTransactions(JobInvoicingPostingOption.LocalSisterCompanyChargesOnly);
				SafeRefreshARInvoiceList();
			}
		}

		#endregion

		#region Post Cost Charges

		void MenuItemPostCost_Click(object sender, EventArgs e)
		{
			if (!SecurityHelper.CheckIsAllowedForSecurityCheckPoint(SecurityCore.PostCost))
			{
				SecurityHelper.ShowError(SecurityCore.PostCost);
			}
			else
			{
				PostTransactions(JobInvoicingPostingOption.Costs);
			}
		}

		#endregion

		#region Reset Tax Defaults

		void MenuItemResetTaxDefaults_Click(object sender, EventArgs e)
		{
			if (CheckIsReadyForFinancialClosureWithoutModifySecurity)
			{
				Globals.Message.ShowError(Res.GetString("DEBD4378-B9CC-49FE-98B6-FB31582C974B", @"Cannot reset tax defaults of unposted lines from this job, because it has Ready For Financial Closure status."));
				return;
			}

			Job.SetDefaultValueForTaxBranch();

			foreach (Charge charge in Job.Charges)
			{
				charge.ResetUnpostedSellTaxDefault();
				charge.ResetUnpostedCostTaxDefault();
			}
		}

		#endregion

		void MenuItemCreateJobRevenueJournal_Click(object sender, EventArgs e)
		{
			string securityCode = SecurityCore.CreateJRJournal;
			if (!SecurityHelper.CheckIsAllowedForSecurityCheckPoint(securityCode))
			{
				SecurityHelper.ShowError(securityCode);
			}
			else
			{
				string jobAndParentSaved = new InvoicingPostManagerGUIWrapper(JobInvoicingPostingOption.All, Factory, Job, JobChargeUserControl.FindForm()).ValidateJobAndParentSaved();
				if (string.IsNullOrEmpty(jobAndParentSaved))
				{
					if (ValidateAndSaveNewlyCreatedAndNotSavedJobToLoadFromNewFactory(Res.GetString("23291456-C048-4F54-AB92-B8C8B6E0AF0B", "Create Job Revenue Journal")))
					{
						JobRevenueJournalForm journalForm = JobRevenueJournalFormFactory.GetJobRevenueJournalForm(new BusinessObjectFactory().New<JobRevenueJournal>());
						journalForm.DisplayMode = ODisplayMode.New;
						journalForm.DisableNewAction();
						journalForm.ActivateSimpleEntry(Job);
						bool isTest = false;
#if DEBUG
						isTest = Globals.IsTest;
#endif
						if (isTest)
						{
							ZFormModaliser.ShowDialogWithoutDispose(journalForm);
						}
						else
						{
							ZFormModaliser.ShowDialogAndDispose(journalForm);
						}
					}
				}
				else
				{
					Globals.Message.ShowError(jobAndParentSaved);
				}
			}
		}

		#endregion

		#region Profit Share

		void MenuItemProfitShare_Click(object sender, EventArgs e)
		{
			if (Job.HasChanges)
			{
				Globals.Message.ShowError(AccountingConstants.ProfitShareErrorMessages.UnsavedChanges);
			}
			else if (!AccountingUtils.ValidateProfitShareRegistry(Factory, Job.JH_GC, out string registryErrorMessage))
			{
				Globals.Message.ShowError(registryErrorMessage);
			}
			else if (CheckIsReadyForFinancialClosureWithoutModifySecurity)
			{
				Globals.Message.ShowError(Res.GetString("746F9786-75EB-4CCF-8089-F1987C92EF2E", @"Cannot create profit share charges from this job, because it has Ready For Financial Closure status."));
			}
			else
			{
				var processor = new ProfitShareShipmentChargeProcessor(Factory, PlugInParent, Job);
				processor.OnErrorOccurred += ProfitShareShipmentChargeProcessor_OnErrorOccurred;
				processor.Process();
				processor.OnErrorOccurred -= ProfitShareShipmentChargeProcessor_OnErrorOccurred;
			}
		}

		void ProfitShareShipmentChargeProcessor_OnErrorOccurred(object sender, ProfitShareChargeCreationEventArgs e)
		{
			Globals.Message.ShowError(e.Message);
		}

		#endregion

		void MenuItemRedefaultExRate_Click(object sender, EventArgs e)
		{
			if (CheckIsReadyForFinancialClosureWithoutModifySecurity)
			{
				Globals.Message.ShowError(Res.GetString("83432CB5-ED62-4007-9ED7-18975C91B601", @"Cannot re-default Job Billing Exchange Rate, because the invoicing Job has Ready For Financial Closure status."));
				return;
			}

			if (Job != null)
			{
				Job.UpdateExchangeRates(false);
			}
		}

		#region Autorating

		#region Execution

		void HandleAutoRateMenu(AutoRateMenuAction action)
		{
			if (!HasAutorateClicked)
			{
				(Job.Parent as IJobCostingPlugIn)?.GetApportionments()?.LoadChildShipmentJobsFromDBOnly();
			}

			HasAutorateClicked = true;

			ExecuteAutorating(new AutoRateOptions
			(
				autoRateRevenue: (action & AutoRateMenuAction.AutorateRevenue) != 0,
				autoRateCost: (action & AutoRateMenuAction.AutorateCosts) != 0,
				excludeConsolLevelChargesOnCosting: (action & AutoRateMenuAction.NonConsolLevelChargeOnly) != 0,
				standaloneShipmentOnly: (action & AutoRateMenuAction.StandaloneShipmentOnly) != 0,
				triggerSource: IsGatewayBilling ? AutoRateTriggerSource.GatewayBilling : AutoRateTriggerSource.Menu
			));
		}

		bool HasAutorateClicked { get; set; }

		[Flags]
#if DEBUG
		internal
#endif
		enum AutoRateMenuAction
		{
			AutorateRevenue = 0b_0001,
			AutorateCosts = 0b_0010,
			NonConsolLevelChargeOnly = 0b_0100, // Not a separate menu item. Should be combined with AutorateCosts
			StandaloneShipmentOnly = 0b_1000, // Not a separate menu item. Should be combined with AutorateCosts
		}

		public RatingResults ExecuteAutorating(AutoRateOptions options) =>
			ExecuteAutorating(new AutoRatingGUIInteractor(UserControl), options);

		public RatingResults ExecuteAutorating(IAutoRatingGUIInteractor interactor, AutoRateOptions options) =>
			ExecuteAutorating(CreateRatingContext(interactor), options);

		public RatingResults ExecuteAutorating(IRatingContext ratingContext, AutoRateOptions options)
		{
			var autoRatingStarter = new AutoRatingStarter(new[] { (IBusiness)PlugInParent }, ratingContext);
			return ExecuteAutoratingWithStarter(autoRatingStarter, options.With(billingType: BillingType.Invoicing));
		}

		IRatingContext CreateRatingContext(IAutoRatingGUIInteractor interactor)
		{
			DialogService dialogService = null;

			if (Form != null)
			{
				dialogService = new DialogService(Form);
			}

			if (DataRegistryRating.Instance.RatesServiceRateSelector.IsAllowed())
			{
				return RatingContext.CreateForManualSelect(interactor, dialogService);
			}

			return new RatingContext(interactor, dialogService);
		}

		RatingResults ExecuteAutoratingWithStarter(AutoRatingStarter autoRatingStarter, AutoRateOptions options)
		{
			try
			{
				var result = autoRatingStarter.ExecuteAutorating(options, isSaveCalledManuallyAfterSession: true, AdditionalJobsForAutoRating);
				// reload charges and cleanup
				if (Job != null)
				{
					((IAutoRatingAccountingUtils)Job).ReloadChargesFromAdditionalJobs();
					Job.UpdateTotals();
					if (Job.Parent != HostBusinessEntity)
					{
						Job.Parent = (IJobHeaderParent)HostBusinessEntity;
					}
				}
				return result;
			}
			catch (AutoRater.RatingCancelledException)
			{
				return null;
			}
		}

		protected override void HookFormEventsCore()
		{
			base.HookFormEventsCore();

			if (HostBusinessEntity != null)
			{
				HostBusinessEntity.HasChangesChanged += BusinessEntity_HasChangesChanged;
			}
		}

		void BusinessEntity_HasChangesChanged(object sender, HasChangesChangedEventArgs e)
		{
			if (e.ObjectJustWasChanged && queuedJobLoadTask == null)
			{
				// the job will be loaded on save, queue it to load on idle to improve save performance
				queuedJobLoadTask = UserIdleWorker.QueueWorkItem(TabPage, 3000, new MethodInvoker(delegate
				{
					object loadedJob = Job;
				}));
			}
		}
		IUserIdleWorkItem queuedJobLoadTask;

		public override void OnSaving()
		{
			if (!jobHasBeenDeactivated)
			{
				if (PlugInParent.InvoicingSupporter.JobInvoicingSecurity.IsAllowed && ObjectFactory.Get<IAccounting>().ShouldAddJobInvoicingRecordAtSavingOrEditingOfOperationsJob((BusinessObject)PlugInParent))
				{
					CreateInvoicingJobIfRequired();

					if (Job != null)
					{
						SetJobBranchAndDepartmentIfEmpty();
					}
				}

				HandleIncoTermChanged();
				HandleExRateUpdate();
			}
			else
			{
				var label = ZString.Empty;

				if (jobWasDeactivatedBySomeoneElse)
				{
					label = GetJobWasDeactivatedByAnotherUserMessage(Job.PK);
				}
				else if (jobLoader.HasInactiveJobHeader(GlbCompany.CurrentCompany))
				{
					label = JobHasBeenDeactivatedMessage;
				}
				else
				{
					label = JobNotBeenCreatedMessage;
				}

				ShowCoveringLabel(label);
			}

			base.OnSaving();
		}

		void HandleIncoTermChanged()
		{
			CommonShipment shipment = PlugInParent as CommonShipment;

			if (shipment != null && Job != null)
			{
				if (shipment.JS_INCOInfo.OriginalValue.ToString() != shipment.JS_INCOInfo.Value.ToString())
				{
					if (Job.Charges.ContainsUnPostedAR_AnyAmount)
					{
						bool proceed = true;

						if (!AccountingConfigurationRegistry.Instance.AutomaticallyReDefaultDebtorsWhenIncoTermChanges.Value)
						{
							ZString messageText = Res.GetString("ffa28432-fee4-4717-a6db-745b41c3e27b", @"The {0} has been changed.
Do you want the debtors to be re-defaulted on the billing tab?", shipment.JS_INCOInfo.HumanReadableName);

							if (Job.Charges.ContainsPostedRevenue())
							{
								messageText += "\r\n\r\n" + Res.GetString("bd6d338f-5150-49b1-ad01-eed0a6440b74", "This will not affect already posted charges.");
							}

							proceed = Globals.Message.Show(messageText, Res.GetString("4a1b24dc-43c4-401e-8135-4bca95168e10", "Confirm debtors re-defaulting"), MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
						}

						if (proceed)
						{
							ResetDefaultDebtorOnUnpostedLines();

							ZString messageText = Res.GetString("360ee5cd-68b8-479b-b609-8ba346ee62d2", "The debtors on the billing tab have been re-defaulted. The data needs to be inspected to confirm that the operation has performed correctly.");

							if (Job.Charges.ContainsPostedRevenue())
							{
								messageText += "\r\n\r\n" + Res.GetString("53e1c066-cfa6-4c14-83fc-d35d6a1391c9", "Some charges could not been re-defaulted because they are already posted.");
							}

							Globals.Message.Show(messageText, Res.GetString("2a768ed3-2640-42cc-88c6-5a06cd706b9c", "Debtors have been re-defaulted"), MessageBoxButtons.OK, MessageBoxIcon.Information);
						}
					}
				}
			}
		}

		void HandleExRateUpdate()
		{
			CommonShipment shipment = PlugInParent as CommonShipment;
			if (shipment != null && Job != null && !Job.IsDeleted)
			{
				if (AccExchangeRateConfigurationRateFinder.GetExchangeRateConfigurationPromptAtCompanyLevel(Job.ExchangeRateConfigurationRateConsumer))
				{
					bool arrivalDateHasChanged = AccExchangeRateConfigurationRateFinder.IsPreferenceArrivalDateAtCompanyLevel(Job.ExchangeRateConfigurationRateConsumer) &&
						  !shipment.JS_E_ARV.IsEmpty && (ZDateTime)shipment.JS_E_ARVInfo.OriginalValue != shipment.JS_E_ARV;
					bool departureDateHasChanged = AccExchangeRateConfigurationRateFinder.IsPreferenceDepartureDateAtCompanyLevel(Job.ExchangeRateConfigurationRateConsumer) &&
						  !shipment.JS_E_DEP.IsEmpty && (ZDateTime)shipment.JS_E_DEPInfo.OriginalValue != shipment.JS_E_DEP;

					if ((arrivalDateHasChanged || departureDateHasChanged) && Job.ExchangeRates.Count > 0)
					{
						string caption = Res.GetString("33F04CDE-6FC1-4f9b-A1C6-DF3D5B9F4097", "Confirm Exchange Rates re-defaulting");
						string message = Res.GetString("03f4620f-206d-470d-a78e-58aeb4c6b1a3", @"Actual/Estimated Departure or Arrival dates have been changed.
Do you want to re-default the exchange rates on the shipment billing tabs?");

						if (Globals.Message.Show(message, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
						{
							MenuItemRedefaultExRate_Click(null, EventArgs.Empty);
						}
					}
				}
			}
		}

		void Factory_Saving(BusinessObjectFactory factory)
		{
			if (IsNeedToDeactivateRelatedJobsOnThisSave)
			{
				JobHeaderHelper.DeactivateAllRelatedEmptyJobHeaders(((BusinessObject)HostBusinessEntity).PK, factory);
				IsNeedToDeactivateRelatedJobsOnThisSave = ZBool.False;
			}
		}

		void Factory_Saved(BusinessObjectFactory factory, bool savedSuccessfully)
		{
			ValidateCreditLimits(CreditLimitCheckMode.ClearCache, new CreditCheckTracer());
		}

		void CreateInvoicingJobIfRequired()
		{
			if (ShouldCreateJob)
			{
				OnGUIShown();

				if (JobIsActive)
				{
					Job.PlugInData = PlugInParent;
					Job.Validation.ValidateAll();
				}
			}
		}

		ZBool JobIsNullOrInactive => Job == null || (!Job.IsDeleted && !Job.JH_IsActive);

		ZBool JobIsActive => Job != null && !Job.IsDeleted && Job.JH_IsActive;

		bool ShouldCreateJob
		{
			get
			{
				return Job == null && PlugInParent != null
					&& (PlugInParent.InvoicingSupporter.CreateAccountingJobOnSavingOfOperationsJob || AccountingUtils.EnableElectronicProcessingCharge(PlugInParent));
			}
		}

		void SetJobBranchAndDepartmentIfEmpty()
		{
			if (Job != null && !Job.IsDeleted && !Job.IsInDatabase)
			{
				Job.SetDefaultBranch(PlugInParent);
				Job.SetDefaultDepartment(PlugInParent);
			}
		}

		enum CreditLimitCheckMode
		{
			Normal,
			AsyncPreFetch,
			ClearCache
		}

		void ValidateCreditLimits(CreditLimitCheckMode mode = CreditLimitCheckMode.Normal, CreditCheckTracer traceCollector = null)
		{
			if (Job == null || Job.IsDeleted)
			{
				return;
			}

			var uniqueOrgs = Job.Charges.ToArray<Charge>()
				.Select(charge => charge.SellAccount)
				.Append(Job.LocalCharges)
				.Append(Job.AgentCollect)
				.WhereNotNull()
				.ToHashSet();

			if (uniqueOrgs.Count < 1)
			{
				return;
			}

			var orgsWhoseCreditLimitCacheCleared = new HashSet<OrgHeader>();
			if ((mode & CreditLimitCheckMode.ClearCache) == CreditLimitCheckMode.ClearCache)
			{
				foreach (var org in uniqueOrgs)
				{
					org.CreditChecker.ClearCreditLimitAndOutstandingBalanceCache();
					orgsWhoseCreditLimitCacheCleared.Add(org);
				}
			}

			var isAsyncPrefetchEnabled = (mode & CreditLimitCheckMode.AsyncPreFetch) == CreditLimitCheckMode.AsyncPreFetch
				&& AccountingConfigurationRegistry.Instance.EnableBillingTabBackgroundValidationWhenCreditCheckUsesWebService.Value
				&& (AccountingConfigurationRegistry.Instance.UseWebServiceForCreditLimit.Value
					|| AccountingConfigurationRegistry.Instance.UseWebServiceForOutstandingBalance.Value
					|| AccountingConfigurationRegistry.Instance.UseWebServiceForTransactionPaymentStatus.Value
					|| AccountingConfigurationRegistry.Instance.UseWebServiceForUnpostedRevenue.Value);

			if (isAsyncPrefetchEnabled)
			{
				foreach (var org in uniqueOrgs)
				{
					// Create CreditChecker to get IOrgCreditLimitAndBalanceDetailsProvider by ObjectFactory as ObjectFactory is not threadsafe.
					_ = org.CreditChecker;

					Task.Factory
						.StartNew(AsyncFetchCreditLimitDetails(org), TaskCreationOptions.LongRunning)
						.ContinueWith(SyncQueueValidation(org.PK), TaskScheduler.FromCurrentSynchronizationContext());
				}
			}
			else
			{
				ValidateCreditLimitsCore(isIdleWorker: false, null, uniqueOrgs.Select(x => x.PK).ToArray());
			}

			PrintTraceInfo(traceCollector, uniqueOrgs, orgsWhoseCreditLimitCacheCleared);
		}

		Action AsyncFetchCreditLimitDetails(OrgHeader org)
		{
			return () =>
			{
				try
				{
#if DEBUG
					if (Globals.IsTest && AsyncFetchCreditLimitDetailsException_ForTest != null)
					{
						throw AsyncFetchCreditLimitDetailsException_ForTest;
					}
#endif
					using (Db.DisposableActionForDbConnection())
					{
						org.CreditChecker.AsyncFetchCreditLimitAndOutstandingBalance();
					}
				}
				catch (Exception ex)
				{
					exceptionsFromAsyncCreditLimitCheck.Add(ex);
				}
			};
		}
		readonly ConcurrentBag<Exception> exceptionsFromAsyncCreditLimitCheck = new ConcurrentBag<Exception>();

		Action<Task> SyncQueueValidation(ZGuid orgPK)
		{
			void SyncAction(Task t)
			{
				if (!t.IsFaulted && !t.IsCanceled && Form != null && !Form.IsDisposed)
				{
					using (Db.DisposableActionForDbConnection())
					{
						UserIdleWorker.QueueWorkItem(TabPage, UserIdleWorkItemOptions.DisableSlowRunningWarning, new MethodInvoker(() => ValidateCreditLimitsCore(true, null, orgPK)));
					}
				}
			}
			return SyncAction;
		}

		void PrintTraceInfo(CreditCheckTracer traceCollector, IEnumerable<OrgHeader> debtors, IEnumerable<OrgHeader> debtorsWhoseCreditLimitCacheCleared)
		{
			#region SuppressResourceStringsCheckRegion for logging

			if (traceCollector != null && traceCollector.IsEnabled())
			{
				var messageBuilder = new ZStringBuilder();

				messageBuilder.Append(FormattableString.Invariant($"""
											CreditCheckDiagnosticInfo_Billing_2
											Collected against BusinessObject: {Job.GetType()} PK: {Job.PK} ->

											{CriticalValidationInfoCollectorServiceKeyType.CreditLimitCheckFromBilling_IfCollectionActivated}:
											{AccountingUtils.CollectRegistryInfoForOrganizationCreditLimitCheck()}
											.Debtors: {string.Join(", ", debtors.Select(o => o.OH_Code))}
											.Debtors Whose Credit Limit Cache have been cleared: {string.Join(", ", debtorsWhoseCreditLimitCacheCleared.Select(o => o.OH_Code))}
											"""));

				var exceptionsSnapshot = exceptionsFromAsyncCreditLimitCheck.ToArray();
				if (exceptionsSnapshot.Length > 1)
				{
					var exceptionMessages = string.Join(System.Environment.NewLine, exceptionsSnapshot.Select(ex => ex.ToString()));
					messageBuilder.Append($".Exceptions Thrown from AsyncFetchCreditLimitDetails: {exceptionMessages}");
				}

				traceCollector.TraceInformation(messageBuilder.ToStringWithNewLineBetweenAppends);
			}

			#endregion
		}

		void ValidateCreditLimitsCore(bool isIdleWorker, List<ZGuid> alreadyValidatedCharges, params ZGuid[] orgPKs)
		{
			if (Job != null && !Job.IsDeleted)
			{
				Stopwatch stopwatch = new Stopwatch();
				if (isIdleWorker)
				{
					stopwatch.Start();
				}
				if (alreadyValidatedCharges == null)
				{
					if (Job.LocalCharges != null &&
						orgPKs.Contains(Job.LocalCharges.PK))
					{
						Job.Validation.ValidateJH_OA_LocalChargesAddr();
						Job.JH_OA_LocalChargesAddrInfo.RefreshBinding();
					}
					if (Job.AgentCollect != null &&
						orgPKs.Contains(Job.AgentCollect.PK))
					{
						Job.Validation.ValidateJH_OA_AgentCollectAddr();
						Job.JH_OA_AgentCollectAddrInfo.RefreshBinding();
					}
				}

				SleepAfterOrgValidationForTestOnly();

				if (isIdleWorker && stopwatch.Elapsed.Seconds > 2)
				{
					UserIdleWorker.QueueWorkItem(TabPage, UserIdleWorkItemOptions.DisableSlowRunningWarning, new MethodInvoker(() => ValidateCreditLimitsCore(true, alreadyValidatedCharges ?? new List<ZGuid>(), orgPKs)));
				}
				else
				{
					var charges = Job.Charges.Cast<Charge>().Where(x => x.SellAccount != null && orgPKs.Contains(x.SellAccount.PK));
					var chargesToValidate = alreadyValidatedCharges == null ? charges : charges.Where(x => !alreadyValidatedCharges.Contains(x.PK)).ToArray();
					foreach (Charge charge in chargesToValidate)
					{
						charge.Validation.ValidateJR_OH_SellAccount();
						charge.JR_OH_SellAccountInfo.RefreshBinding();
						if (alreadyValidatedCharges == null)
						{
							alreadyValidatedCharges = new List<ZGuid>() { charge.PK };
						}
						else
						{
							alreadyValidatedCharges.Add(charge.PK);
						}

						SleepAfterChargeValidationForTestOnly();

						if (isIdleWorker && stopwatch.Elapsed.Seconds > 2)
						{
							UserIdleWorker.QueueWorkItem(TabPage, UserIdleWorkItemOptions.DisableSlowRunningWarning, new MethodInvoker(() => ValidateCreditLimitsCore(true, alreadyValidatedCharges, orgPKs)));
							break;
						}
					}
				}
				if (isIdleWorker)
				{
					stopwatch.Stop();
				}
			}
		}

		protected virtual void SleepAfterOrgValidationForTestOnly() { }
		protected virtual void SleepAfterChargeValidationForTestOnly() { }

		#endregion

		#region Group Companies Charges

		void MenuItemGroupCompanyCharges_Click(object sender, EventArgs e)
		{
			if (CheckIsReadyForFinancialClosureWithoutModifySecurity)
			{
				Globals.Message.ShowError(Res.GetString("FFF7B656-0ADA-455D-B4C7-4FA73A744BB0", @"Cannot do group companies charges, because it has Ready For Financial Closure status."));
				return;
			}

			var groupCompanyChargesForJob = new GroupCompanyChargesForJob(Job);
			ZFormModaliser.ShowDialogAndDispose(new DebtorsAcceptGroupChargesForm(groupCompanyChargesForJob), Form);
		}

		#endregion

		#region Storage Not Rated Dialog

		ContinueWithSave AutorateIfNecessary(ContinueWithSave continueWithSave)
		{
			var isQuotedBookingForwardRegistered = ZBool.False;
			if (PlugInParent is IQuotedBooking quotedBooking)
			{
				if (PlugInParent.InvoicingSupporter?.IsQuote == ZBool.True)
				{
					isQuotedBookingForwardRegistered = ((ForwardingShipment)quotedBooking.ForwardingShipment)?.JS_IsForwardRegistered ?? ZBool.False;
				}
			}

			ZBool shouldCheckForAutoRatingRun = Job != null
												&& !Job.IsDeleted
												&& !Job.JH_RatingHasBeenRun
												&& Job.Charges.Count == 0
												&& !isQuotedBookingForwardRegistered
												&& RatingDataRegistry.Instance.ShouldShowAutoRatingNotRunWarning.Value
												&& PlugInParent is IRatingSupporter;

			bool storageNotRated;
			if (!autoratingNotRunWarningHasBeenShownInThisSession && ((storageNotRated = StorageNotRated) || shouldCheckForAutoRatingRun))
			{
				string message = storageNotRated
									? Res.GetString("29ca844f-8ce5-4b04-8e28-22c6945007dc", "Storage is applicable on this job, but there are no Storage charge codes present on the Invoice. Do you want to autorate now?")
									: Res.GetString("29826a3d-4498-4575-8e5b-c9d517c7331e", "AutoRating has not yet been run on this job. Do you want to autorate now?");

				DialogResult userResponse = Globals.Message.Show(message, Res.GetString("780672b1-a6e3-4faa-8542-9162bffef01d", "Autorating"), MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);

				switch (userResponse)
				{
					case DialogResult.Yes:
						autoratingNotRunWarningHasBeenShownInThisSession = true;
						SelectTabPage();
						ExecuteAutorating(AutoRateOptions.AutorateCostsRevenue);
						continueWithSave = ContinueWithSave.No;
						break;

					case DialogResult.No:
						autoratingNotRunWarningHasBeenShownInThisSession = true;
						continueWithSave = ContinueWithSave.Yes;
						break;

					case DialogResult.Cancel:
						continueWithSave = ContinueWithSave.No;
						break;
				}
			}

			return continueWithSave;
		}

		bool autoratingNotRunWarningHasBeenShownInThisSession;

		public override ContinueWithSave ShowPreSaveDialogsCore()
		{
			ContinueWithSave continueWithSave = AutorateIfNecessary(base.ShowPreSaveDialogsCore());

			if (Job != null)
			{
				if (continueWithSave == ContinueWithSave.Yes)
				{
					SecurityOverrideProviderSource.Get(Job).Provider = new JobInvoicingSecurityOverrideProvider();

					if (!Job.IsDeleted && Job.JH_IsActive)
					{
						Job.Validation.ValidateAll();
					}
					if (Job.HasErrors)
					{
						continueWithSave = ContinueWithSave.No;
					}
				}

				if (!Job.IsJobActivating && Job.Factory.ExistsInDatabase(JobHeaderSchema.Constants.TableName, new ZQuery(JobHeaderSchema.PK, Job.PK).AddToFilter(JobHeaderSchema.JH_IsActive, false)))
				{
					Globals.Message.ShowError(GetJobWasDeactivatedByAnotherUserMessage(Job.PK), Res.GetString("946FB1E6-E74B-4539-8006-8F2A10B2A048", "Invoicing Job has been marked as inactive"));
					jobWasDeactivatedBySomeoneElse = true;
					jobHasBeenDeactivated = true;
					continueWithSave = ContinueWithSave.No;
				}

				if (continueWithSave == ContinueWithSave.Yes)
				{
					CheckGatewayAbleToApportionChargeAsJRJ(Job);
				}
			}

			if (continueWithSave == ContinueWithSave.Yes &&
				Job != null &&
				Job.PlugInData != null &&
				Job.HasChanges &&
				Job.PlugInData.InvoicingSupporter.EditSecurityLock)
			{
				continueWithSave = SecurityHelperClass.RequestEditAuthorisation(Job.PlugInData) ? ContinueWithSave.Yes : ContinueWithSave.No;
			}

			if (continueWithSave == ContinueWithSave.Yes && Job != null)
			{
				var invoicingPluginToFreightPresentationProvider = GetInvoicingPluginToFreightPresentationProvider();
				var preSaveActionResult = invoicingPluginToFreightPresentationProvider.PreSaveActions();
				if (!preSaveActionResult.CanProceed)
				{
					Globals.Message.ShowError(preSaveActionResult.ErrorMessage);
					continueWithSave = ContinueWithSave.No;
				}
			}

			return continueWithSave;
		}

		IInvoicingPluginToFreightPresentationProvider GetInvoicingPluginToFreightPresentationProvider()
		{
			return ObjectFactory.Get<IAccountingPresentationProviderFactory>().GetInvoicingPluginToFreightPresentationProvider(Job.ClosedJobReopener, new JobRevRecognitionDataRetriever(Job.PK));
		}

		void CheckGatewayAbleToApportionChargeAsJRJ(Job job)
		{
			if (GatewaySellToCostSynchroniser.IsRecommendToEnableAutoJRJForGatewaySellApportionment(job))
			{
				Globals.Message.ShowOrDefault(RecommendEnableAutoJRJInfoControl.DialogDefaultContext, () => new RecommendEnableAutoJRJInfoControl());
			}
		}

		bool StorageNotRated
		{
			get
			{
				return PlugInParent is IAutoRating autoRating
						&& autoRating.JobServices.IsEnabled(ChargeCodeGroupList.Codes.CFSShipment, ChargeCodeSubGroupList.Storage)
						&& autoRating.StatusInformation.CanExecute
					|| PlugInParent is IRatingSupporter supporter
						&& supporter.AdaptersProvider is RatingAdaptersProvider provider
						&& provider.IsAdapterAvailable(AutoRateOptions.AutorateRevenue, ChargeCodeGroupList.Codes.CFSShipment, ChargeCodeSubGroupList.Storage)
					&& Job is Job job
					&& !job.Charges.OfType<Charge>().Any(charge => charge.ChargeCode != null
																&& charge.ChargeCode.AC_ChargeGroup == ChargeCodeGroupList.Codes.CFSShipment
																&& charge.ChargeCode.AC_ChargeSubGroup == ChargeCodeSubGroupList.Storage);
			}
		}

		#endregion

		#endregion

		#region Update With Gateway

		void IPluginShouldRefreshMenuForGateway.RefreshGatewayElements(bool isGatewayEnabled)
		{
			Enabled = isGatewayEnabled;
			JobChargeUserControl.ToggleShipmentAndBillingDetailsIsActive(isGatewayEnabled);

			foreach (MenuItem item in TopLevelMenu.MenuItems)
			{
				if (item.Text == Constants.MenuNameConstants.PostLocalClientCharges)
				{
					item.Visible = !isGatewayEnabled;
				}
			}

			if (isGatewayEnabled)
			{
				TabPage.Text = Res.GetString("a6660e23-090a-403b-a581-b42d2e90460f", "Gateway Billing");
				TopLevelMenu.Text = Res.GetString("18797043-fbec-43c4-a06b-8579f2509e8f", "Gateway Invoicing");
			}
			else
			{
				TabPage.Text = Name;
			}

			IsGatewayBilling = isGatewayEnabled;
		}

		bool IsGatewayBilling { get; set; }

		#endregion

		#region JobCharge Related Job Filter

		void JobChargeUserControl_RelatedJobFilterUpdated(object sender, RelatedJobFilterUpdatedEventArgs args)
		{
			TopLevelMenu.MenuItems.Cast<MenuItem>().ForEach(x => x.Enabled = !args.AnyFiltersApplied);
		}

		#endregion

		#endregion

		#region Business Objects

		public override BusinessObjectFactory Factory
		{
			get { return HostBusinessEntity != null ? HostBusinessEntity.Factory : null; }
		}

		protected override IBusiness GetBusinessEntityForPlugIn()
		{
			return Job;
		}

		protected IJobInvoicingPlugIn PlugInParent
		{
			get { return (IJobInvoicingPlugIn)HostBusinessEntity; }
		}

		Job Job
		{
			get
			{
				if (fJob == null)   // Load the job so we can do this validation, if there is a job (DON'T create a new one)
				{
					fJob = jobLoader.Load(true);
					HookJobEventForDataRefresh();
					if (fJob != null)
					{
						SetupJobDependencies(fJob);
					}
				}
				return fJob;
			}
			set
			{
				if (fJob != value)
				{
					DisposeJobIfNotNull();
					fJob = value;
					HookJobEventForDataRefresh();
					if (fJob != null)
					{
						SetupJobDependencies(fJob);
					}

					if (auditMenuItem != null)
					{
						auditMenuItem.SetLoggedBusinessObject(value);
					}
				}
			}
		}

		Job fJob;

		void SetupJobDependencies(Job job)
		{
			job.SetupDependency(new ClosedJobReopener(new InvoicingPluginToFreightReopenClosedInternalJobDataProvider(job), new ReopenClosedJobSecurityOverrideProvider()));
		}

		bool CheckCanNotPostOrCreateTransaction => Job?.IsReadyForFinancialClosureWithoutPostSecurity ?? false;

		bool CheckIsReadyForFinancialClosureWithoutModifySecurity => Job?.IsReadyForFinancialClosureWithoutModifySecurity ?? false;

		void Job_OnDeactivatedByDataRefresh(object sender, EventArgs e)
		{
			ShowCoveringLabel(GetJobWasDeactivatedByAnotherUserMessage(Job.PK));
			jobWasDeactivatedBySomeoneElse = true;
			jobHasBeenDeactivated = true;
		}

		void Job_OnDeactivated(object sender, EventArgs e)
		{
			if (!isDeactivatingJob)
			{
				ShowCoveringLabel(JobWasDeactivatedByAnotherFormMessage);
				jobWasDeactivatedBySomeOtherForm = true;
				jobHasBeenDeactivated = true;
			}
		}

		void Charges_OnCannotDelete(object sender, OnCannotDeleteEventArgs e)
		{
			if (IsHostBusinessEntityCancelled) //Deactivating host business entity
			{
				var chargeCode = e.Charge.ChargeCode == null ? "" : e.Charge.ChargeCode.AC_Code.ToString();
				if (CanDelete || (Job?.CanDeactivate ?? true)) //Job header can be deleted, so the charge will also be deleted
				{
					var message = Res.GetString("25f20345-cd7f-45bf-a4bf-22eae50e3352", "No accounting transaction has been recorded against this job. Job charge {0} will be deleted when you deactivate this {1}.", chargeCode, PlugInParent.InvoicingSupporter.ConsumerType.Description);
					Globals.Message.ShowWarning(message);
				}
				else //Job header can not be deleted, so the charge will not be deleted
				{
					var error = Res.GetString("e5121f83-d103-420b-b09f-61414bcb2d30", "You cannot delete job charge {0} because there is at least one accounting transaction that has been recorded against this job and prevents deleting this job.", chargeCode);
					Globals.Message.ShowError(error);
				}
			}
			else //Delete unposted lines called from JobInvoicing Menu. Not deactivating host business entity.
			{
				Globals.Message.ShowError(e.Error);
			}
		}

		void Job_OnCloseJobError(Job job, string errorMessage)
		{
			Globals.Message.ShowError(errorMessage);
		}

		void Job_OnCloseJobYesNoQuestion(object sender, UserQueryEventArgs e)
		{
			DialogResult result = Globals.Message.Show(e.QueryMessage, GetJobNumberString((Job)sender), MessageBoxButtons.YesNo, e.Response ? DialogResult.Yes : DialogResult.No);
			e.Response = result == DialogResult.Yes;
		}

		void Job_OnCannotChangeStatusUserMessage(object sender, UserMessageEventArgs e)
		{
			Globals.Message.Show(e.Message, GetJobNumberString((Job)sender), MessageBoxButtons.OK, MessageBoxIcon.Information);
		}

		void Job_ShouldUseImmediateRevenueRecognisedDate(object sender, UserQueryEventArgs e)
		{
			e.Response = Globals.Message.Show(e.QueryMessage, GetJobNumberString((Job)sender), MessageBoxButtons.YesNo,
				MessageBoxIcon.Question, DialogResult.No) == DialogResult.Yes;
		}

		string GetJobNumberString(Job job)
		{
			return Res.GetString("eb8f38e4-bcd7-4fd5-b94b-95b8b7c88bf9", "Job") + " " + job.JH_JobNum;
		}

		public override void OnBusinessObjectIsCancelledChanged(ZBool isCancelled)
		{
			if (IsActive && Job != null)
			{
				IsNeedToDeactivateRelatedJobsOnThisSave = isCancelled;

				if (isCancelled && Job.Parent != null)
				{
					// Deactivate job and show covering label only if the job is related to currently opened Host Business Object
					DeactivateJobAndShowCoveringLabel();
				}
				else
				{
					Job.SetReadOnlyIncludingChildren(isCancelled);
				}
			}
		}
		ZBool IsNeedToDeactivateRelatedJobsOnThisSave;

		bool ValidateAndSaveNewlyCreatedAndNotSavedJobToLoadFromNewFactory(string operationName)
		{
			bool isJobSavedSuccessfully = true;
			bool newlyCreatedAndNotSavedJob = !Job.IsInDatabase && !Job.HasChanges;
			if (newlyCreatedAndNotSavedJob)
			{
				DialogResult userAnswer = Globals.Message.Show(
					Res.GetString("4FE34EAB-8F5C-4876-9849-DF8D8A8A1851", "Invoicing job must be created for this operation. Do you want to create invoicing job?"),
					operationName,
					MessageBoxButtons.YesNo, MessageBoxIcon.Question, DialogResult.Yes);
				if (userAnswer == DialogResult.Yes)
				{
					Job.RunPreSaveValidation();
					if (Job.HasErrors)
					{
						Globals.Message.ShowError(Res.GetString("C7EA2321-EAE4-4763-BA00-CBDE756ABE51", "An error occurred while trying to create the invoicing job header. Please correct these errors on the 'Billing' tab and try again."));
						this.SelectTabPage();
						isJobSavedSuccessfully = false;
					}
					else
					{
						BusinessObjectFactory newFactoryToSaveJob = new BusinessObjectFactory() { NameForDebugging = "InvoicingPluginToFreight_ValidateAndSaveNewlyCreatedAndNotSavedJobToLoadFromNewFactory" };
						newFactoryToSaveJob.ImportFromAnotherFactory(Job);
						newFactoryToSaveJob.Save();

						((IBusinessObjectInternals)Job).Row.AcceptChanges();
						Job.Reload();
					}
				}
				else
				{
					isJobSavedSuccessfully = false;
				}
			}

			return isJobSavedSuccessfully;
		}

		#endregion

		#region Deleting

		public override string CannotDeleteMessage
		{
			get
			{
				var result = "";

				if (PlugInParent != null && jobLoader != null)
				{
					result = jobLoader.GetJobDeleteErrorMessage();
				}
				return result;
			}
		}

		public override bool CanDelete
		{
			get { return string.IsNullOrEmpty(CannotDeleteMessage); }
		}

		public override void Delete()
		{
			ZQuery query = new ZQuery(JobHeaderSchema.JH_ParentID, PlugInParent.PK);
			query.AddToFilter(JobHeaderSchema.JH_GC, GlbCompany.CurrentCompany.PK);
			JobCollection allJobs = new JobCollection(Factory, query);
			allJobs.Load();
			allJobs.RemoveAndDeleteAll();
			base.Delete();
		}

		#endregion

		#region GUI

		#region Menu

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000: Dispose objects before losing scope")]
		protected override MenuItem GetNewTopLevelMenu()
		{
			if (MainMenuItem == null)
			{
				var consumerType = PlugInParent.InvoicingSupporter.ConsumerType;

				var menuName = consumerType.MenuName(PlugInParent) ?? ResString.GetMultilingualString("Accounting.JobInvoicing", "&Job Invoicing");
				MainMenuItem = new ZMenuItem(menuName);
				var revenueChargeDescription = consumerType.RevenueChargeDescription(PlugInParent);
				var allowRevenuePosting = consumerType.AllowRevenuePosting(PlugInParent);
				var allowCostInvoicing = consumerType.AllowCostPosting(PlugInParent);

				var defaultARInvoiceDateMenuCaption = ARDefaultInvoiceAndPostDateCalculator.GetDefaultDateMenuCaption();
				if (defaultARInvoiceDateMenuCaption != null)
				{
					MainMenuItem.MenuItems.Add(new ZMenuItem(defaultARInvoiceDateMenuCaption));
				}

				var isQuotedBookingForwardRegistered = ZBool.False;
				if (PlugInParent is IQuotedBooking quotedBooking)
				{
					if (PlugInParent.InvoicingSupporter?.IsQuote == ZBool.True)
					{
						isQuotedBookingForwardRegistered = ((ForwardingShipment)quotedBooking.ForwardingShipment)?.JS_IsForwardRegistered ?? ZBool.False;
					}
				}

				if (IsFormEditable && !isQuotedBookingForwardRegistered)
				{
					var revenueDescription = consumerType.RevenueChargeDescription(PlugInParent);
					var costsDescription = ResString.GetMultilingualString("21F7CFE9-57C4-4638-9387-3AEE50704D5F", "Costs");
					var nonConsolLevelOnlyDescription = ResString.GetMultilingualString("B5EA21B9-D553-46D3-A162-7FE89435E682", "(Non-Consol Level Charge Only)");
					var standaloneShipmentOnlyDescription = ResString.GetMultilingualString("9DCCCA96-8727-48F9-98ED-1C4661F0FE32", "(Standalone with Consol Level Charge)");

					var menuItemAutorateRevenueCosts = new ZMenuItem(ResString.GetMultilingualString("9DD12C91-A1CA-47F0-BDD1-1D3F6C10A8D8", "Autorate {0} and {1}", costsDescription, revenueDescription));
					menuItemAutorateRevenueCosts.Click += delegate
					{ HandleAutoRateMenu(AutoRateMenuAction.AutorateCosts | AutoRateMenuAction.AutorateRevenue); };
					MainMenuItem.MenuItems.Add(menuItemAutorateRevenueCosts);

					var menuItemAutorateRevenue = new ZMenuItem(ResString.GetMultilingualString("53869035-5B82-4E79-9344-C84D3F3FF846", "Autorate {0}", revenueDescription));
					menuItemAutorateRevenue.Click += delegate
					{ HandleAutoRateMenu(AutoRateMenuAction.AutorateRevenue); };
					MainMenuItem.MenuItems.Add(menuItemAutorateRevenue);

					var menuItemAutorateCosts = new ZMenuItem(ResString.GetMultilingualString("5D3FA8FC-1669-4E84-AD5F-2D04A781D432", "Autorate {0}", costsDescription));
					menuItemAutorateCosts.Click += delegate
					{ HandleAutoRateMenu(AutoRateMenuAction.AutorateCosts); };
					MainMenuItem.MenuItems.Add(menuItemAutorateCosts);

					var menuItemAutorateCostsNCL = new ZMenuItem(ResString.GetMultilingualString("1A1020A2-F758-4FC7-9B89-23767E9F2166", "Autorate {0} {1}", costsDescription, nonConsolLevelOnlyDescription));
					menuItemAutorateCostsNCL.Click += delegate
					{ HandleAutoRateMenu(AutoRateMenuAction.AutorateCosts | AutoRateMenuAction.NonConsolLevelChargeOnly); };
					MainMenuItem.MenuItems.Add(menuItemAutorateCostsNCL);

					var menuItemAutorateRevenueCostsNCL = new ZMenuItem(ResString.GetMultilingualString("BDBD05CB-913B-4BFB-A8A7-4A284D0B2207", "Autorate {0} {1} and {2}", costsDescription, nonConsolLevelOnlyDescription, revenueDescription));
					menuItemAutorateRevenueCostsNCL.Click += delegate
					{ HandleAutoRateMenu(AutoRateMenuAction.AutorateCosts | AutoRateMenuAction.NonConsolLevelChargeOnly | AutoRateMenuAction.AutorateRevenue); };
					MainMenuItem.MenuItems.Add(menuItemAutorateRevenueCostsNCL);

					if (HostBusinessEntity is CommonShipment)
					{
						var caption = ResString.GetMultilingualString("1BB9109F-9B98-49D7-82E0-64FD9307A07D", "Autorate {0} {1}", costsDescription, standaloneShipmentOnlyDescription);
						MenuItemAutorateCostsSTS = new ZMenuItem(caption);
						MenuItemAutorateCostsSTS.Click += delegate
						{ HandleAutoRateMenu(AutoRateMenuAction.AutorateCosts | AutoRateMenuAction.StandaloneShipmentOnly); };
						MainMenuItem.MenuItems.Add(MenuItemAutorateCostsSTS);

						caption = ResString.GetMultilingualString("78CD4FA3-63D9-4072-A1AF-FCD6BE57181F", "Autorate {0} {1} and {2}", costsDescription, standaloneShipmentOnlyDescription, revenueDescription);
						MenuItemAutorateCostsSTSRevenue = new ZMenuItem(caption);
						MenuItemAutorateCostsSTSRevenue.Click += delegate
						{ HandleAutoRateMenu(AutoRateMenuAction.AutorateCosts | AutoRateMenuAction.StandaloneShipmentOnly | AutoRateMenuAction.AutorateRevenue); };
						MainMenuItem.MenuItems.Add(MenuItemAutorateCostsSTSRevenue);

						ResetStandaloneShipmentMenuItemsAvailability();
					}

					if (!AutoJRJRegistryStatusHelper.IsAutoJRJEnabled())
					{
						var menuItemFindGroupCompanyCharges = new ZMenuItem(ResString.GetMultilingualString("5b704cf3-b49f-4bd9-a385-7feae13e463f", "Group Companies Charges"));
						menuItemFindGroupCompanyCharges.Click += new EventHandler(MenuItemGroupCompanyCharges_Click);
						MainMenuItem.MenuItems.Add(menuItemFindGroupCompanyCharges);
					}

					MainMenuItem.MenuItems.Add(new ZMenuItem("-"));
				}

				if (allowRevenuePosting)
				{
					var menuItemPostAll = new ZMenuItem(Constants.MenuNameConstants.PostAllChargesAndCosts);
					menuItemPostAll.Click += new EventHandler(MenuItemPostAll_Click);
					MainMenuItem.MenuItems.Add(menuItemPostAll);

					MainMenuItem.MenuItems.Add(new ZMenuItem("-"));

					var menuItemPostBillTo = new ZMenuItem(Constants.MenuNameConstants.PostLocalClientCharges);
					menuItemPostBillTo.Click += new EventHandler(MenuItemPostBillTo_Click);
					MainMenuItem.MenuItems.Add(menuItemPostBillTo);

					var menuItemPostAgent = new ZMenuItem(Constants.MenuNameConstants.PostOverseasAgentCharges);
					menuItemPostAgent.Click += new EventHandler(MenuItemPostAgent_Click);
					MainMenuItem.MenuItems.Add(menuItemPostAgent);

					var menuItemPostBoth = new ZMenuItem(Constants.MenuNameConstants.PostAllRevenueCharges);
					menuItemPostBoth.Click += new EventHandler(MenuItemPostBoth_Click);
					MainMenuItem.MenuItems.Add(menuItemPostBoth);

					var menuItemPostDisbursement = new ZMenuItem(Constants.MenuNameConstants.PostDisbursementChargesonly);
					menuItemPostDisbursement.Click += new EventHandler(MenuItemPostDisbursement_Click);
					MainMenuItem.MenuItems.Add(menuItemPostDisbursement);

					var menuItemPostAllSisterCompanyCharges = new ZMenuItem(Constants.MenuNameConstants.PostAllSisterCompanyCharges);
					menuItemPostAllSisterCompanyCharges.Click += new EventHandler(MenuItemPostAllSisterCompanyCharges_Click);
					MainMenuItem.MenuItems.Add(menuItemPostAllSisterCompanyCharges);

					var menuItemPostLocalSisterCompanyChargesOnly = new ZMenuItem(Constants.MenuNameConstants.PostLocalSisterCompanyChargesOnly);
					menuItemPostLocalSisterCompanyChargesOnly.Click += new EventHandler(MenuItemPostLocalSisterCompanyChargesOnly_Click);
					MainMenuItem.MenuItems.Add(menuItemPostLocalSisterCompanyChargesOnly);

					var menuItemCreateJobRevenueJournal = new ZMenuItem(ResString.GetMultilingualString("MenuItem.Constants.CreateJobRevenueJournal", "Create Job Revenue Journal"));
					menuItemCreateJobRevenueJournal.Click += new EventHandler(MenuItemCreateJobRevenueJournal_Click);
					MainMenuItem.MenuItems.Add(menuItemCreateJobRevenueJournal);

					MainMenuItem.MenuItems.Add(new ZMenuItem("-"));

					var previewInvoices = new ZMenuItem(Constants.MenuNameConstants.PreviewInvoices);
					previewInvoices.Click += new EventHandler(PreviewInvoices_Click);
					MainMenuItem.MenuItems.Add(previewInvoices);

					if (ObjectFactory.Get<IAccCashAdvanceFunctionalityChecker>().IsReceivablesCashAdvanceFunctionalityEnabled)
					{
						var menuItemRequestCashAdvance = new ZMenuItem(ResString.GetMultilingualString("Accounting.JobInvoicing.RequestCashAdvanceRenamed", "Request Advance Payment"));
						menuItemRequestCashAdvance.Click += new EventHandler(MenuItemRequestCashAdvance_Click);
						MainMenuItem.MenuItems.Add(menuItemRequestCashAdvance);
					}

					MainMenuItem.MenuItems.Add(new ZMenuItem("-"));
				}

				if (allowCostInvoicing)
				{
					var menuItemPostCost = new ZMenuItem(Constants.MenuNameConstants.PostCosts);
					menuItemPostCost.Click += new EventHandler(MenuItemPostCost_Click);
					MainMenuItem.MenuItems.Add(menuItemPostCost);

					var previewCosts = new ZMenuItem(Constants.MenuNameConstants.PreviewCosts);
					previewCosts.Click += new EventHandler(PreviewCosts_Click);
					MainMenuItem.MenuItems.Add(previewCosts);

					MainMenuItem.MenuItems.Add(new ZMenuItem("-"));
				}

				var menuItemMarkJobHeaderAsInactive = new ZMenuItem(ResString.GetMultilingualString("Accounting.JobInvoicing.MarkJobHeaderAsInactive", "Mark Job Header as Inactive"));
				menuItemMarkJobHeaderAsInactive.Click += new EventHandler(MenuItemMarkJobHeaderAsInactive_Click);
				MainMenuItem.MenuItems.Add(menuItemMarkJobHeaderAsInactive);

				MainMenuItem.MenuItems.Add(new ZMenuItem("-"));

				var menuItemDeleteUnPosted = new ZMenuItem(ResString.GetMultilingualString("Accounting.JobInvoicing.DeleteUnpostedLines", "Delete Unposted lines"));
				menuItemDeleteUnPosted.Click += new EventHandler(MenuItemDeleteUnPosted_Click);
				MainMenuItem.MenuItems.Add(menuItemDeleteUnPosted);

				var menuItemRecalculateInvoiceType = new ZMenuItem(ResString.GetMultilingualString("Accounting.JobInvoicing.ResetUnpostedLinesInvoiceType", "Reset Unposted lines Invoice Type"));
				menuItemRecalculateInvoiceType.Click += new EventHandler(MenuItemRecalculateInvoiceType_Click);
				MainMenuItem.MenuItems.Add(menuItemRecalculateInvoiceType);

				var menuItemResetTaxDefaults = new ZMenuItem(Constants.MenuNameConstants.ResetUnpostedLinesTaxDefault);
				menuItemResetTaxDefaults.Click += new EventHandler(MenuItemResetTaxDefaults_Click);
				MainMenuItem.MenuItems.Add(menuItemResetTaxDefaults);

				var menuItemResetDefaultDebtorOnUnpostedLines = new ZMenuItem(ResString.GetMultilingualString("MenuItem.Constants.ResetDefaultDebtorOnUnpostedLines", "Reset Default debtor on unposted lines"));
				menuItemResetDefaultDebtorOnUnpostedLines.Click += MenuItemResetDefaultDebtorOnUnpostedLines_Click;
				MainMenuItem.MenuItems.Add(menuItemResetDefaultDebtorOnUnpostedLines);

				MainMenuItem.MenuItems.Add(new ZMenuItem("-"));

				if (allowRevenuePosting || allowCostInvoicing)
				{
					var menuItemReverseInvoices = new ZMenuItem(ResString.GetMultilingualString("Accounting.JobInvoicing.ReverseInvoicesRedoBilling", "Reverse Invoices and redo billing"));
					menuItemReverseInvoices.Click += new EventHandler(MenuItemReverseInvoices_Click);
					MainMenuItem.MenuItems.Add(menuItemReverseInvoices);

					MainMenuItem.MenuItems.Add(new ZMenuItem("-"));
				}

				if (allowCostInvoicing)
				{
					var menuItemImportAPInvoices = new ZMenuItem(ResString.GetMultilingualString("Accounting.JobInvoicing.ImportAPInvoices", "Import AP invoices issued by other group companies"));
					menuItemImportAPInvoices.Click += new EventHandler(MenuItemImportAPInvoices_Click);
					MainMenuItem.MenuItems.Add(menuItemImportAPInvoices);

					MainMenuItem.MenuItems.Add(new ZMenuItem("-"));
				}

				if (allowRevenuePosting)
				{
					var menuItemRecognizeRevenueText = ResString.GetMultilingualString("Accounting.JobInvoicing.RecognizeRevenue", "Recognize Revenue");
					var menuItemRecognizeRevenue = new ZMenuItem(menuItemRecognizeRevenueText);
					menuItemRecognizeRevenue.Click += new EventHandler(MenuItemRecognizeRevenue_Click);
					MainMenuItem.MenuItems.Add(menuItemRecognizeRevenue);
				}

				if (GlbStaff.CurrentUser.IsSupportUser)
				{
					var menuItemFixRevenueRecognitionDataText = ResString.GetMultilingualString("Accounting.JobInvoicing.FixRecognizeRevenue", "Fix Revenue Recognition Data(Support Only)");
					var menuItemFixRevenueRecognition = new ZMenuItem(menuItemFixRevenueRecognitionDataText);
					menuItemFixRevenueRecognition.Click += new EventHandler(MenuItemFixRevenueRecognitionData_Click);
					MainMenuItem.MenuItems.Add(menuItemFixRevenueRecognition);
				}

				if (allowRevenuePosting || allowCostInvoicing)
				{
					var hostBusinessEntityAsStmALogParent = HostBusinessEntity as IStmALogParent;
					if (hostBusinessEntityAsStmALogParent != null)
					{
						auditMenuItem = new WriteToLogMenuItem(hostBusinessEntityAsStmALogParent, delegate
						{ return Job; }, PlugInParent.InvoicingSupporter.AuditSecurity, ResString.GetMultilingualString("Accounting|InvoicingPluginToFreight", "Audit Billing"));
						auditMenuItem.AddTo(MainMenuItem);
					}

					MainMenuItem.MenuItems.Add(new ZMenuItem("-"));
				}

				if (allowRevenuePosting)
				{
					var menuItemProfitShare = new ZMenuItem(ResString.GetMultilingualString("Accounting.JobInvoicing.CreateProfitShareCharges", "Create Profit Share Charges"));
					menuItemProfitShare.Click += new EventHandler(MenuItemProfitShare_Click);
					MainMenuItem.MenuItems.Add(menuItemProfitShare);

					MainMenuItem.MenuItems.Add(new ZMenuItem("-"));
				}

				var menuItemRedefaultExRate = new ZMenuItem(ResString.GetMultilingualString("Accounting.JobInvoicing.RedefaultJobBillingExchangeRate", "Re-default Job Billing Exchange Rate"));
				menuItemRedefaultExRate.Click += new EventHandler(MenuItemRedefaultExRate_Click);
				MainMenuItem.MenuItems.Add(menuItemRedefaultExRate);

				var eInvoicingGUIActionHelper = new EInvoicingGUIActionHelper(() => null);
				MainMenuItem.MenuItems.AddIfNotNull(eInvoicingGUIActionHelper.GetPenaltyTaxInfoActionMenuItem());
			}

			return MainMenuItem;
		}

		MenuItem MainMenuItem;
		WriteToLogMenuItem auditMenuItem;

		ZMenuItem MenuItemAutorateCostsSTS;
		ZMenuItem MenuItemAutorateCostsSTSRevenue;

		#endregion

		public override void UpdateTabPageMinimumAutoSized()
		{
			const int ExtraWidthRequiredToSizeTabPageProperly = 6;
			TabPage.MinimumAutoSizedWidth = UserControl.MinimumSize.Width + ControlDpiScalingHelper.ScaleToCurrentDpiX(ExtraWidthRequiredToSizeTabPageProperly);
			TabPage.MinimumAutoSizedHeight = UserControl.MinimumSize.Height;
		}

		protected override ZTabPagePlugIn GetTabPage()
		{
			return new JobInvoicingTabPagePlugIn(this);
		}

		public override string Name
		{
			get { return Res.GetString("f0203a14-87bd-416d-8be8-b16ba980d965", "Billing"); }
		}

		protected override string TextOverride
		{
			get { return PlugInParent.InvoicingSupporter.ConsumerType.DisplayName(PlugInParent); }
		}

		protected JobChargeUserControl JobChargeUserControl
		{
			get { return ((JobInvoicingUserControl)UserControl).JobChargeUserControl; }
		}

		protected JobInvoicePrintingControl JobInvoicePrintingControl
		{
			get { return ((JobInvoicingUserControl)UserControl).JobInvoicePrintingControl; }
		}

		protected APInvoicePrintingUserControl APInvoicePrintingUserControl
		{
			get { return ((JobInvoicingUserControl)UserControl).APInvoicePrintingUserControl; }
		}

		protected APDraftInvoicePrintingUserControl APDraftInvoiceListUserControl
		{
			get { return ((JobInvoicingUserControl)UserControl).APDraftInvoicePrintingUserControl; }
		}

		protected CashAdvanceRequestUserControl CashAdvanceRequestUserControl
		{
			get { return ((JobInvoicingUserControl)UserControl).CashAdvanceRequestUserControl; }
		}

		protected ZGuid JobHeaderPK;

		public override ZString PlugInNotDisplayedMessage
		{
			get
			{
				ZString result = null;

				if (isDeactivatingJob || IsHostBusinessEntityCancelled)
				{
					result = JobWillBeDeactivatedOnSaveMessage;
				}
				else if (jobWasDeactivatedBySomeoneElse)
				{
					result = GetJobWasDeactivatedByAnotherUserMessage(Job.PK);
				}
				else if (jobWasDeactivatedBySomeOtherForm)
				{
					result = JobWasDeactivatedByAnotherFormMessage;
				}
				else if (!IsAllowedToViewBillingTab)
				{
					result = JobCannotBeAccessedMessage;
				}
				else
				{
					result = jobLoader.GetJobCreationError();
				}

				return result;
			}
		}

		ZString JobWillBeDeactivatedOnSaveMessage
		{
			get
			{
				var result = new StringBuilder();
				if (IsNeedToDeactivateRelatedJobsOnThisSave)
				{
					result.AppendLine(Res.GetString("966DC5E0-A86F-4704-A565-6E80D4836AC8", "You have set this {0} {1} to inactive.", PlugInParent.InvoicingSupporter.ConsumerType.Description, PlugInParent.JobNumber));
					result.AppendLine(Res.GetString("16E17565-E3FA-46D7-81BD-63B9EA3B89BE", "This will mark the invoicing Job {0} as inactive when you save this {1}.", PlugInParent.JobNumber, PlugInParent.InvoicingSupporter.ConsumerType.Description));
				}
				else if (Job != null && Job.IsInDatabase)
				{
					result.AppendLine(Res.GetString("93A20120-2B45-4CCF-A27A-0C9B71C876CF", "{0} will be marked as inactive when you save this {1}.", JobDescription, PlugInParent.InvoicingSupporter.ConsumerType.Description));
				}
				else if (PlugInParent is ICancellable cancellable && cancellable.IsCancelled && !cancellable.IsCancelledHasChanged)
				{
					result.AppendLine(Res.GetString("0C4ED750-AA44-497C-AAA4-C813FD83FAD2", "This {0} {1} has been set to inactive.", PlugInParent.InvoicingSupporter.ConsumerType.Description, PlugInParent.JobNumber));
				}
				else
				{
					result.AppendLine(Res.GetString("47C6ED5C-703C-4ACC-ACE1-BA934F5DA298", "{0} has not been created.", JobDescription));
				}

				result.AppendLine(string.Empty);

				if (!IsHostBusinessEntityCancelled)
				{
					if (Job.IsInDatabase)
					{
						result.AppendLine(Res.GetString("83FC5C99-8B1C-4F9E-9A59-C380874DAD1D", "Please change to another tab, then click back to this tab to activate invoicing job for this {0}", PlugInParent.InvoicingSupporter.ConsumerType.Description));
					}
					else
					{
						result.AppendLine(Res.GetString("66409D91-BAC5-4B20-8E35-FEC55077284E", "Please change to another tab, then click back to this tab to create an invoicing job for this {0}", PlugInParent.InvoicingSupporter.ConsumerType.Description));
					}
				}
				return result.ToString();
			}
		}

		ZString GetJobWasDeactivatedByAnotherUserMessage(ZGuid deactivatedJobPK)
		{
			ZString userWhoDeactivatedTheJob = Res.GetString("0CD45BBC-9509-4C21-87EB-2B6A6CEFA5B9", "another user");

			ZQuery findDeactivateStmALogRecordFilter = new ZQuery(StmALogSchema.SL_Parent, deactivatedJobPK);
			findDeactivateStmALogRecordFilter.AddToFilter(StmALogSchema.SL_SE_NKEvent, AutoEvents.SetToInactive.Code);
			StmALog deactivateLogForDeletedJob = Factory.LoadTop1<StmALog>(findDeactivateStmALogRecordFilter);

			if (deactivateLogForDeletedJob != null && deactivateLogForDeletedJob.User != null)
			{
				userWhoDeactivatedTheJob = deactivateLogForDeletedJob.User.GS_FullName;
			}

			return Res.GetString("CE0C261A-1AFC-489C-985F-F57C1656DAC3",
				"{0} was marked as inactive by {1}. You will not be able to save the changes you made to this {2}.",
				JobDescription,
				userWhoDeactivatedTheJob,
				PlugInParent.InvoicingSupporter.ConsumerType.Description)
				+ "\r\n"
				+ Res.GetString("E5DD8F1E-814B-4E60-A03E-1CD85B65684A",
					"Please close this {0} before re-entering your changes.",
					PlugInParent.InvoicingSupporter.ConsumerType.Description);
		}

		ZString JobWasDeactivatedByAnotherFormMessage =>
			Res.GetString("4C925DD5-AFC1-4E16-B8BB-72555D1FC33F", "{0} was marked as inactive by another form. You will not be able to save the changes you made to this {1}.",
				JobDescription,
				PlugInParent.InvoicingSupporter.ConsumerType.Description)
			+ "\r\n"
			+ Res.GetString("7D212185-BE92-4010-985A-19628AD04F4B", "Please close this {0} before re-entering your changes.", PlugInParent.InvoicingSupporter.ConsumerType.Description);

		ZString JobHasBeenDeactivatedMessage =>
			Res.GetString("CFC94E97-140A-48B7-8BC8-3EA758A3CF2B"
				, "{0} has been marked as inactive."
				, JobDescription)
			+ "\r\n\r\n"
			+ Res.GetString("93E24A40-3D80-42BF-AB8C-913155FE18FD",
				"Please change to another tab, then click back to this tab to activate invoicing job for this {0}",
				PlugInParent.InvoicingSupporter.ConsumerType.Description);

		ZString JobNotBeenCreatedMessage =>
			Res.GetString("A20745D9-CBBA-4C21-ADDE-64FBF8D13AFA"
				, "{0} has not been created."
				, JobDescription)
			+ "\r\n\r\n"
			+ Res.GetString("0B4B91FE-B888-49B9-9CBD-78AB732518F2",
				"Please change to another tab, then click back to this tab to create job for this {0}",
				PlugInParent.InvoicingSupporter.ConsumerType.Description);

		ZString JobDescription
		{
			get
			{
				return !PlugInParent.IsDeleted && PlugInParent.JobNumber.Length > 0 ?
					Res.GetString("Accounting|InvoicingPluginToFreight|JobTitle", "Invoicing job") + " " + PlugInParent.JobNumber :
					Res.GetString("Accounting|InvoicingPluginToFreight|ThisJob", "This invoicing job");
			}
		}

		ZString JobCannotBeAccessedMessage
		{
			get
			{
				ZString result = Res.GetString("b5e0eae2-2237-404a-a339-c41b6be81187", "This billing job has a branch {0} and department {1}. Viewing of the billing tab for this job is disallowed because you do not have security rights to login to this branch and department.", Job.Branch.GB_Code, Job.Department.GE_Code);
				result += SecurityHelper.GetErrorTextForSecurityCheckPoint(SecurityCore.AllowViewEditBilling);
				return result;
			}
		}

		protected override bool ShouldPlugInGUIAndBusinessEntityBeCreatedCore()
		{
			return !isDeactivatingJob && !jobWasDeactivatedBySomeoneElse && !jobWasDeactivatedBySomeOtherForm && Job != null;
		}

		protected override bool QueryUserShouldPlugInGUIAndBusinessEntityBeCreated()
		{
			bool result = !isDeactivatingJob && !jobWasDeactivatedBySomeoneElse && !jobWasDeactivatedBySomeOtherForm && MakeOrActivateJob() && IsAllowedToViewBillingTab;

			if (result)
			{
				if (!AccountingConfigurationRegistry.Instance.EnableLightValidationForChargeAndConsolCost.Value)
				{
					foreach (var charge in chargesWithSuspendedValidation)
					{
						charge.ResumeValidation();
					}
					chargesWithSuspendedValidation.Clear();
				}
				Job.MarkAsNeedingValidationIncludingChildren();
			}
			else if (Job != null && !AccountingConfigurationRegistry.Instance.EnableLightValidationForChargeAndConsolCost.Value)
			{
				foreach (Charge charge in Job.Charges)
				{
					charge.SuspendValidation();
					chargesWithSuspendedValidation.Add(charge);
				}
			}
			return result;
		}

		protected override bool ShouldPluginDropdownMenuBeCreated()
		{
			return QueryUserShouldPlugInGUIAndBusinessEntityBeCreated();
		}

		bool MakeOrActivateJob()
		{
			if (JobIsNullOrInactive && !IsHostBusinessEntityCancelled)
			{
#if DEBUG
				if (Globals.IsTest && Form != null && Form.IsFormToBash_ForTestOnly)
				{
					Factory.SetContext(BusinessContext.DisableSetHasChangesIfHasErrors_ForTestOnly);
				}
#endif
				Job = jobLoader.TryLoadOrCreateWithMutex();
			}

			jobHasBeenDeactivated = false;

			return Job != null;
		}

#if DEBUG
		public ZString AccessJH_GBWhileJobIsDetached_ErrorReportKey;
#endif

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Combining Cache key, not related to GUI")]
		bool IsAllowedToViewBillingTab
		{
			get
			{
				bool result = true;

				if (Job != null && ((IBusinessObjectInternals)Job).Row.RowState == DataRowState.Detached)
				{
					var service = CriticalValidationInfoCollectorService.GetService(Job.Factory);
					var message = string.Empty;
					if (service != null)
					{
						message = service.GetInfo(Job.PK, CriticalValidationInfoCollectorServiceKeyType.DeleteJobHeader);
					}

					ZStringBuilder messageInfo = new ZStringBuilder((NoResString)"Access branch property from a detached Job."); // Developer Notification Key not shown to client
					messageInfo.AppendLine(message);

					ExceptionReporter.Instance.ReportDeveloperException("AccessJH_GBWhileJobIsDetached", messageInfo.ToString(), new RowNotInTableException(messageInfo.ToString()));// Developer Notification Key not shown to client

#if DEBUG
					AccessJH_GBWhileJobIsDetached_ErrorReportKey = "AccessJH_GBWhileJobIsDetached";
#endif
				}

				if (Job != null && Job.Branch != null && Job.Department != null
					&& (Job.JH_GB != GlbBranch.CurrentBranch.PK || Job.JH_GE != GlbDepartment.CurrentDepartment.PK)
					&& !SecurityHelper.CheckIsAllowedForSecurityCheckPoint(SecurityCore.AllowViewEditBilling))
				{
					result = Factory.GetCachedValue("Login BRN:" + Job.Branch.GB_Code + " DEP:" + Job.Department.GE_Code, delegate
					{
						SecurityCore security = new UserLoginController().GetSecurityForUser(GlbStaff.CurrentUser.GS_LoginName, Job.JH_GB.ToGuid(), Job.JH_GE.ToGuid());
						return security.Login.IsAllowed;
					});
				}

				return result;
			}
		}

		bool IsHostBusinessEntityCancelled
		{
			get
			{
				return HostBusinessEntity is ICancellable && ((ICancellable)HostBusinessEntity).IsCancelled;
			}
		}

		bool HasClickedMenu;

		public override void OnMenuShown()
		{
			HasClickedMenu = true;
			try
			{
				base.OnMenuShown();
			}
			finally
			{
				HasClickedMenu = false;
			}
		}

		int CaptionWidthInPixels(Control control)
		{
			using (var graphics = control.CreateGraphics())
			{
				System.Drawing.Font font = control.Font;
				string caption = control.GetExtension<ILabelCaptionRenderer>().Caption;
				return (int)graphics.MeasureString(caption, font).Width;
			}
		}

		[return:DpiState(DpiState.ScaledVariant)]
		System.Drawing.Point CalculatedPosition(Control left, int spacing, Control right)
		{
			int x = ControlDpiScalingHelper.UnscaleFromCurrentDpiX(right.Location.X - CaptionWidthInPixels(right) - left.Size.Width) - spacing;
			return ControlDpiScalingHelper.NewScaledPoint(x, ControlDpiScalingHelper.UnscaleFromCurrentDpiY(left.Location.Y));
		}

		protected virtual bool AddUserControlIfRequired()
		{
			if (!TabPage.Controls.Contains(UserControl))
			{
				TabPage.AddPlugInUserControl();
				return true;
			}

			return false;
		}

		void BindUserControlsIfRequired()
		{
			if (Job != null && !userControlsBound)
			{
				if (AddUserControlIfRequired())
				{
					((ZUserControl)UserControl).SetDataBinding(Job, "");
				}

				JobChargeUserControl.Bind(Job);
				JobChargeUserControl.ToggleOverseasAgentControlsVisibility(Job.OverseasAgentIsApplicable);
				if (Job != null && Job.JobType != null)
				{
					JobChargeUserControl.JH_OH_LocalChargesBoundOrgCard.Text = Job.JobType.LocalClientText;
					JobChargeUserControl.JH_OH_AgentCollectBoundOrgCard.Text = Job.JobType.OverseasAgentText;

					bool isCFXAmountZero = true;

					foreach (Charge charge in Job.Charges)
					{
						if (charge.JR_CFXAmt != 0)
						{
							isCFXAmountZero = false;
							break;
						}
					}

					int spacing = 20;
					int profitCaptionPixels = CaptionWidthInPixels(JobChargeUserControl.ProfitLossAmountCalcEdit);
					int cfxCaptionPixels = CaptionWidthInPixels(JobChargeUserControl.TotalCFXAmountCalcEdit);
					int cfxTotalPixels = ControlDpiScalingHelper.UnscaleFromCurrentDpiX(cfxCaptionPixels + JobChargeUserControl.TotalCFXAmountCalcEdit.Size.Width) + spacing;

					if (isCFXAmountZero)
					{
						JobChargeUserControl.TotalCFXAmountCalcEdit.Visible = false;
						cfxTotalPixels = 0;
					}

					JobChargeUserControl.TotalCFXAmountCalcEdit.Location = CalculatedPosition(JobChargeUserControl.TotalCFXAmountCalcEdit, spacing, JobChargeUserControl.ProfitLossAmountCalcEdit);
					JobChargeUserControl.TotalAgentAmountCalcEdit.Location = CalculatedPosition(JobChargeUserControl.TotalAgentAmountCalcEdit, spacing + cfxTotalPixels, JobChargeUserControl.ProfitLossAmountCalcEdit);
					JobChargeUserControl.TotalCostAmountCalcEdit.Location = CalculatedPosition(JobChargeUserControl.TotalCostAmountCalcEdit, spacing, JobChargeUserControl.TotalAgentAmountCalcEdit);
					JobChargeUserControl.TotalsLabel.Location = CalculatedPosition(JobChargeUserControl.TotalsLabel, spacing, JobChargeUserControl.TotalCostAmountCalcEdit);
				}
				if (IsCashAdvanceFunctionalityEnabledForAROrAP())
				{
					CashAdvanceRequestUserControl.Bind(Job.CashAdvanceRequests);
				}
				HideCoveringLabel();
				userControlsBound = true;
			}
		}
		bool userControlsBound;

		bool HasGuiBeenShown { get; set; }
		public override void OnGUIShown()
		{
			base.OnGUIShown();

			MakeOrActivateJob();

			if (JobIsActive)
			{
				if (!Job.HasChanges && !(HasClickedMenu && IsJobHookedUp))
				{
					Job.PlugInData = PlugInParent;
				}
				using (Job.SuspendSettingHasChanges())
				{
					Job.SetDefaultBranch(PlugInParent);
					Job.SetDefaultDepartment(PlugInParent);
				}

				if (PlugInParent.InvoicingSupporter.IsPlugInReadOnly || IsHostBusinessEntityCancelled)
				{
					Job.SetReadOnlyIncludingChildren(true);
				}

				if (!IsJobHookedUp)
				{
					SecurityOverrideProviderSource.Get(Job).Provider = new JobInvoicingSecurityOverrideProvider();
					HookJobEvents();

					BindUserControlsIfRequired();
					IsJobHookedUp = true;
				}

				if (!HasGuiBeenShown)
				{
					ValidateCreditLimits(CreditLimitCheckMode.AsyncPreFetch);
					DoWarningOnlyValidationOnCharges();
				}

				UpdateCaptionIfCrossTrade();
			}

			HasGuiBeenShown = true;
		}

		void DoWarningOnlyValidationOnCharges()
		{
			Job.Factory.SetContext(BusinessContext.WarningOnlyValidation);
			try
			{
				foreach (Charge charge in Job.Charges.ToArray())
				{
					charge.Validation.ValidateJR_OSCostGSTAmt_Calc();
					charge.Validation.ValidateJR_OSSellGSTAmt_Calc();
				}
			}
			finally
			{
				Job.Factory.RemoveContext(BusinessContext.WarningOnlyValidation);
			}
		}

		bool HasCaptionBeenChanged;

		void UpdateCaptionIfCrossTrade()
		{
			if (Job != null && Job.JobType != null)
			{
				if (Job.CanCrossTradeDebtorDefaultingBeApplied && !HasCaptionBeenChanged)
				{
					JobChargeUserControl.JH_OH_LocalChargesBoundOrgCard.Text = Job.JobType.PrepaidBillToPartyText;
					JobChargeUserControl.JH_OH_AgentCollectBoundOrgCard.Text = Job.JobType.CollectBillToPartyText;
					JobChargeUserControl.JH_OH_LocalChargesBoundOrgCard.CaptionResourceString = Job.PrepaidBillToPartyCaption;
					JobChargeUserControl.JH_OH_AgentCollectBoundOrgCard.CaptionResourceString = Job.CollectBillToPartyCaption;

					foreach (MenuItem item in MainMenuItem.MenuItems)
					{
						if (item.Text == Constants.MenuNameConstants.PostLocalClientCharges)
						{
							item.Text = Constants.MenuNameConstants.PostPrepaidBillToParty;
						}
						else if (item.Text == Constants.MenuNameConstants.PostOverseasAgentCharges)
						{
							item.Text = Constants.MenuNameConstants.PostCollectBillToParty;
							break;
						}
					}

					HasCaptionBeenChanged = true;
				}
				else if (!Job.CanCrossTradeDebtorDefaultingBeApplied && HasCaptionBeenChanged)
				{
					JobChargeUserControl.JH_OH_LocalChargesBoundOrgCard.Text = Job.JobType.LocalClientText;
					JobChargeUserControl.JH_OH_AgentCollectBoundOrgCard.Text = Job.JobType.OverseasAgentText;
					JobChargeUserControl.JH_OH_LocalChargesBoundOrgCard.CaptionResourceString = Job.JH_OA_LocalChargesAddrCaption;
					JobChargeUserControl.JH_OH_AgentCollectBoundOrgCard.CaptionResourceString = Job.JH_OA_AgentCollectAddrCaption;

					foreach (MenuItem item in MainMenuItem.MenuItems)
					{
						if (item.Text == Constants.MenuNameConstants.PostPrepaidBillToParty)
						{
							item.Text = Constants.MenuNameConstants.PostLocalClientCharges;
						}
						else if (item.Text == Constants.MenuNameConstants.PostCollectBillToParty)
						{
							item.Text = Constants.MenuNameConstants.PostOverseasAgentCharges;
							break;
						}
					}

					HasCaptionBeenChanged = false;
				}
			}
		}

		bool AllowOverrideFormToPopUp(Job senderJob, ValueChangedEventArgs valArg, OrgHeader newOrg)
		{
			bool allowInvoiceAddressOverrideFormPopUp = true;

			if (valArg != null)
			{
				var oldOrgCode = ZString.Empty;
				if (valArg.Info.Name.Equals(senderJob.JH_OC_LocalBillingContactInfo.Name))
				{
					var oldContact = Factory.Load<OrgContact>((ZGuid)valArg.OldValue);
					oldOrgCode = oldContact != null ? oldContact.Header.OH_Code : ZString.Empty;
				}
				else if (valArg.Info.Name.Equals(senderJob.JH_OA_LocalChargesAddrInfo.Name) || valArg.Info.Name.Equals(senderJob.JH_OA_AgentCollectAddrInfo.Name))
				{
					var oldAddr = Factory.Load<OrgAddress>((ZGuid)valArg.OldValue);
					oldOrgCode = oldAddr != null ? oldAddr.Header.OH_Code : ZString.Empty;
				}
				var newOrgCode = newOrg != null ? newOrg.OH_Code : ZString.Empty;
				allowInvoiceAddressOverrideFormPopUp = oldOrgCode.Equals(newOrgCode);
			}

			return allowInvoiceAddressOverrideFormPopUp;
		}

		void JH_OA_LocalChargesAddrInfo_ValueChanged(object sender, EventArgs e)
		{
			var valArg = e as ValueChangedEventArgs;
			var senderAsJob = sender as Job;

			if (Job.LocalCharges != null
					&& Job.LocalCharges.OH_IsDebtor
						&& Job.JH_OA_LocalChargesAddrInfo.HasChanges
							&& !Job.Factory.IsInTransaction
								&& Env.Security.ModifyAddressContactAfterPostedCharge.IsAllowed
									&& Job.IsAnyCostOrRevenuePosted(Job.LocalCharges.PK)
										&& AllowOverrideFormToPopUp(senderAsJob, valArg, Job.LocalCharges))
			{
				var originalAddress = Factory.Load<OrgAddress>((ZGuid)Job.JH_OA_LocalChargesAddrInfo.OriginalValue);
				var weHaveChangedFromOneAddressToAnotherAddressForTheSameOrg = originalAddress != null && Job.LocalChargesAddr != null &&
					originalAddress.OA_OH == Job.LocalChargesAddr.OA_OH;

				List<ZGuid> invoicesToUpdate;
				ZString invoicesLinkedToOtherJobs = string.Empty;
				CheckIfInvoiceLinkedToOtherJobs(Job.LocalCharges.PK, out invoicesToUpdate, out invoicesLinkedToOtherJobs);

				if (!invoicesLinkedToOtherJobs.IsEmpty && invoicesToUpdate.Count == 0)
				{
					Globals.Message.ShowError(GetInvoiceAddressCouldNotBeUpdatedMessage(ref invoicesLinkedToOtherJobs));
				}
				else if (!string.IsNullOrEmpty(invoicesLinkedToOtherJobs) && invoicesToUpdate.Count > 0)
				{
					DialogResult result = Globals.Message.Show(GetInvoiceAddressCouldNotBeUpdatedMessageForYesNoWindow(ref invoicesLinkedToOtherJobs), Res.GetString("faedad08-8646-4f17-b62a-8c0a089ab3ef", "Information"), MessageBoxButtons.YesNo, MessageBoxIcon.Information);
					if (result == DialogResult.Yes)
					{
						ShowInvoiceAddressOverrideForm(Job.JH_OA_LocalChargesAddr, invoicesToUpdate);
					}
				}
				else
				{
					ShowInvoiceAddressOverrideForm(Job.JH_OA_LocalChargesAddr, invoicesToUpdate);
				}
			}
		}

		void JH_OC_LocalBillingContactInfo_ValueChanged(object sender, EventArgs e)
		{
			var valArg = e as ValueChangedEventArgs;
			var senderAsJob = sender as Job;

			if (Job.LocalCharges != null
					&& Job.LocalCharges.OH_IsDebtor
						&& Job.JH_OC_LocalBillingContactInfo.HasChanges
							&& !Job.Factory.IsInTransaction
								&& Env.Security.ModifyAddressContactAfterPostedCharge.IsAllowed
									&& Job.IsAnyCostOrRevenuePosted(Job.LocalCharges.PK)
										&& AllowOverrideFormToPopUp(senderAsJob, valArg, Job.LocalCharges))
			{
				List<ZGuid> invoicesToUpdate;
				ZString invoicesLinkedToOtherJobs = string.Empty;
				CheckIfInvoiceLinkedToOtherJobs(Job.LocalCharges.PK, out invoicesToUpdate, out invoicesLinkedToOtherJobs);

				if (!invoicesLinkedToOtherJobs.IsEmpty && invoicesToUpdate.Count == 0)
				{
					Globals.Message.ShowError(Res.GetString("ef0bde75-e540-4b55-8e18-e423073a4abe", "The job has following posted invoice(s) with lines that are related to other job(s):\r\n{0}\r\nPlease update invoice contact for these invoices from Transactions module.", invoicesLinkedToOtherJobs));
				}
				else if (!string.IsNullOrEmpty(invoicesLinkedToOtherJobs) && invoicesToUpdate.Count > 0)
				{
					DialogResult result = Globals.Message.Show(Res.GetString("90899cc6-80eb-4af6-9dc8-6c294c5b8b59", "The job has following posted invoice(s) with lines that are related to other job(s):\r\n{0}\r\nPlease update invoice contact for these invoices from Transactions module.\r\n\r\nThere are however other invoices only related to this job. Do you want to continue to update contact for those?", invoicesLinkedToOtherJobs), Res.GetString("faedad08-8646-4f17-b62a-8c0a089ab3ef", "Information"), MessageBoxButtons.YesNo, MessageBoxIcon.Information);
					if (result == DialogResult.Yes)
					{
						ShoInvoiceContactOverrideForm(Job.JH_OC_LocalBillingContact, invoicesToUpdate);
					}
				}
				else
				{
					ShoInvoiceContactOverrideForm(Job.JH_OC_LocalBillingContact, invoicesToUpdate);
				}
			}
		}

		void JH_OA_AgentCollectAddrInfo_ValueChanged(object sender, EventArgs e)
		{
			var valArg = e as ValueChangedEventArgs;
			var senderAsJob = sender as Job;

			if (Job.AgentCollect != null
					&& Job.AgentCollect.OH_IsDebtor
						&& Job.JH_OA_AgentCollectAddrInfo.HasChanges
							&& !Job.Factory.IsInTransaction
								&& Env.Security.ModifyAddressContactAfterPostedCharge.IsAllowed
									&& Job.IsAnyCostOrRevenuePosted(Job.AgentCollect.PK)
										&& AllowOverrideFormToPopUp(senderAsJob, valArg, Job.AgentCollect))
			{
				List<ZGuid> invoicesToUpdate;
				ZString invoicesLinkedToOtherJobs = string.Empty;
				CheckIfInvoiceLinkedToOtherJobs(Job.AgentCollect.PK, out invoicesToUpdate, out invoicesLinkedToOtherJobs);

				if (!invoicesLinkedToOtherJobs.IsEmpty && invoicesToUpdate.Count == 0)
				{
					Globals.Message.ShowError(GetInvoiceAddressCouldNotBeUpdatedMessage(ref invoicesLinkedToOtherJobs));
				}
				else if (!string.IsNullOrEmpty(invoicesLinkedToOtherJobs) && invoicesToUpdate.Count > 0)
				{
					DialogResult result = Globals.Message.Show(GetInvoiceAddressCouldNotBeUpdatedMessageForYesNoWindow(ref invoicesLinkedToOtherJobs), Res.GetString("faedad08-8646-4f17-b62a-8c0a089ab3ef", "Information"), MessageBoxButtons.YesNo, MessageBoxIcon.Information);
					if (result == DialogResult.Yes)
					{
						ShowInvoiceAddressOverrideForm(Job.JH_OA_AgentCollectAddr, invoicesToUpdate);
					}
				}
				else
				{
					ShowInvoiceAddressOverrideForm(Job.JH_OA_AgentCollectAddr, invoicesToUpdate);
				}
			}
		}

		void CheckIfInvoiceLinkedToOtherJobs(ZGuid orgPk, out List<ZGuid> invoicesToUpdate, out ZString invoicesLinkedToOtherJobs)
		{
			var lines = Job.GetPostedARAPLines(orgPk);

			var invoices = Factory.Load<InvoicingBase>(new ZQuery(AccTransactionHeaderSchema.PK, lines.Select(bizo => bizo.AL_AH).Distinct().ToArray()));

			invoicesToUpdate = new List<ZGuid>();
			var stringBuilder = new ZStringBuilder();
			foreach (var invoice in invoices)
			{
				var jobPks = invoice.Lines.Select(x => ((InvoicingLineBase)x).AL_JH).Distinct().ToArray();
				if (jobPks.Length > 1)
				{
					stringBuilder.Append(invoice.AH_TransactionNum);
				}
				else
				{
					invoicesToUpdate.Add(invoice.PK);
				}
			}

			if (!stringBuilder.IsEmpty)
			{
				invoicesLinkedToOtherJobs = stringBuilder.ToStringWithNewLineBetweenAppends();
			}
			else
			{
				invoicesLinkedToOtherJobs = ZString.Empty;
			}
		}

		void ShowInvoiceAddressOverrideForm(ZGuid addressPk, List<ZGuid> invoicesToUpdate)
		{
			OverrideInvoiceAddressHelper helper = new OverrideInvoiceAddressHelper(new BusinessObjectFactory() { NameForDebugging = "InvoicingPluginToFreight_ShowInvoiceAddressOverrideForm" }, Factory, addressPk, false, invoicesToUpdate.ToArray());
			helper.SetDefaultValues();

			ZFormModaliser.Show(new OverrideInvoiceAddressContactForm(helper), this.Form);
		}

		void ShoInvoiceContactOverrideForm(ZGuid contactPk, List<ZGuid> invoicesToUpdate)
		{
			OverrideInvoiceContactHelper helper = new OverrideInvoiceContactHelper(new BusinessObjectFactory() { NameForDebugging = "InvoicingPluginToFreight_ShoInvoiceContactOverrideForm" }, Factory, Job.JH_OC_LocalBillingContact, false, invoicesToUpdate.ToArray());
			helper.SetDefaultValues();

			ZFormModaliser.Show(new OverrideInvoiceAddressContactForm(helper), this.Form);
		}

		static string GetInvoiceAddressCouldNotBeUpdatedMessageForYesNoWindow(ref ZString invoicesLinkedToOtherJobs)
		{
			return Res.GetString("371633bc-fcea-4d15-9239-6fc6d7ecb74f", "The job has following posted invoice(s) with lines that are related to other job(s):\r\n{0}\r\nPlease update invoice address for these invoices from Transactions module.\r\n\r\nThere are however other invoices only related to this job. Do you want to continue to update address for those?", invoicesLinkedToOtherJobs);
		}

		static string GetInvoiceAddressCouldNotBeUpdatedMessage(ref ZString invoicesLinkedToOtherJobs)
		{
			return Res.GetString("e990b5c2-6298-4ab8-b7a7-3a441986aa4c", "The job has following posted invoice(s) with lines that are related to other job(s):\r\n{0}\r\nPlease update invoice address for these invoices from Transactions module.", invoicesLinkedToOtherJobs);
		}

		public void UpdateJobChargeGrid()
		{
			this.JobChargeUserControl.JobChargeBoundGrid.Refresh();
		}

		bool IsJobHookedUp;

		public override void OnUserControlShown()
		{
			base.OnUserControlShown();
			InitializeTabPage();
		}

		void InitializeTabPage()
		{
			(UserControl as JobInvoicingUserControl)?.ARInvoicesTabPage.RunWhenTabInitialized((sender, args) =>
			{
				if (fARPrintingFilter == null)
				{
					fARPrintingFilter = new JobARInvoicePrintingFilter(HostBusinessEntity, Job.PK);
					JobInvoicePrintingControl.Bind(fARPrintingFilter);
				}
				else
				{
					fARPrintingFilter.RefreshInvoiceList();
				}
			});

			(UserControl as JobInvoicingUserControl)?.APInvoicesTabPage.RunWhenTabInitialized((sender, args) =>
			{
				if (fAPPrintingFilter == null)
				{
					fAPPrintingFilter = new JobAPInvoicePrintingFilter(HostBusinessEntity, Job.PK);
					APInvoicePrintingUserControl.Bind(fAPPrintingFilter);
				}
				else
				{
					fAPPrintingFilter.RefreshInvoiceList();
				}

				if (draftInvoiceFilter == null)
				{
					draftInvoiceFilter = new JobDraftInvoicePrintingFilter(HostBusinessEntity as BusinessObject);
					APDraftInvoiceListUserControl.Bind(draftInvoiceFilter);
				}
				else
				{
					draftInvoiceFilter.RefreshInvoiceList();
				}
			});
		}

		protected override Control GetNewUserControl()
		{
			JobInvoicingUserControl result = new JobInvoicingUserControl(PluginSecurity);

			if (PlugInParent != null && PlugInParent.InvoicingSupporter.ConsumerType != null && !PlugInParent.InvoicingSupporter.ConsumerType.InvoicingPrintingApplicable(PlugInParent))
			{
				result.ARInvoicesTabPage.Dispose();
				result.APInvoicesTabPage.Dispose();
			}
			else
			{
				if (!SecurityHelper.CheckIsAllowedForSecurityCheckPoint(SecurityCore.ARInvoices))
				{
					result.JobInvoicePrintingControl.Visible = false;
					result.ARInvoicingPrintingSecurityPanel.Visible = true;
					result.ARInvoicingPrintingSecurityLabel.Text = SecurityHelper.GetErrorTextForSecurityCheckPoint(SecurityCore.ARInvoices);
				}
				else
				{
					result.JobInvoicePrintingControl.Visible = true;
					result.ARInvoicingPrintingSecurityPanel.Visible = false;
					result.ARInvoicingPrintingSecurityLabel.Text = "";
				}

				if (!SecurityHelper.CheckIsAllowedForSecurityCheckPoint(SecurityCore.APInvoices))
				{
					result.APInvoicePrintingSplitContainer.Visible = false;
					result.APInvoicingPrintingSecurityPanel.Visible = true;
					result.APInvoicingPrintingSecurityLabel.Text = SecurityHelper.GetErrorTextForSecurityCheckPoint(SecurityCore.APInvoices);
				}
				else
				{
					result.APInvoicePrintingSplitContainer.Visible = true;
					result.APInvoicingPrintingSecurityLabel.Visible = false;
					result.APInvoicingPrintingSecurityLabel.Text = "";
				}
			}

			if (PlugInParent != null && PlugInParent.InvoicingSupporter.ConsumerType != null && !PlugInParent.InvoicingSupporter.ConsumerType.CreditStatusApplicable(PlugInParent))
			{
				result.CreditStatusTabPage.Dispose();
			}
			else if (!SecurityHelper.CheckIsAllowedForSecurityCheckPoint(SecurityCore.ViewCreditStatus))
			{
				result.JobCreditStatusControl.Visible = false;
				result.CreditStatusSecurityPanel.Visible = true;
				result.CreditStatusSecurityLabel.Text = SecurityHelper.GetErrorTextForSecurityCheckPoint(SecurityCore.ViewCreditStatus);
			}
			else
			{
				result.JobCreditStatusControl.Visible = true;
				result.CreditStatusSecurityPanel.Visible = false;
				result.CreditStatusSecurityLabel.Text = "";
			}

			if (PlugInParent != null && PlugInParent.InvoicingSupporter.ConsumerType != null && !PlugInParent.InvoicingSupporter.ConsumerType.ProfitLossApplicable(PlugInParent))
			{
				result.ProfitLossTabPage.Dispose();
			}
			else if (!SecurityHelper.CheckIsAllowedForSecurityCheckPoint(SecurityCore.ProfitLoss))
			{
				result.JobProfitLossControl.Visible = false;
				result.ProfitLossSecurityPanel.Visible = true;
				result.ProfitLossSecurityLabel.Text = SecurityHelper.GetErrorTextForSecurityCheckPoint(SecurityCore.ProfitLoss);
			}
			else
			{
				result.JobProfitLossControl.Visible = true;
				result.ProfitLossSecurityPanel.Visible = false;
				result.ProfitLossSecurityLabel.Text = "";
			}

			if (!SecurityHelper.CheckIsAllowedForSecurityCheckPoint(SecurityCore.Invoicing))
			{
				result.JobChargeUserControl.Visible = false;
				result.InvoicingSecurityPanel.Visible = true;
				result.InvoicingSecurityLabel.Text = SecurityHelper.GetErrorTextForSecurityCheckPoint(SecurityCore.Invoicing);
			}
			else
			{
				result.JobChargeUserControl.Visible = true;
				result.InvoicingSecurityPanel.Visible = false;
				result.InvoicingSecurityLabel.Text = "";
			}

			if (!SecurityHelper.CheckIsAllowedForSecurityCheckPoint(SecurityCore.InvoicingEntry))
			{
				result.JobChargeUserControl.SetReadOnlyIncludingChildren(true);
			}

			if (!IsCashAdvanceFunctionalityEnabledForAROrAP())
			{
				result.CashAdvanceRequestsTabPage.Dispose();
			}

			// this event should only be used for Gateway related job
			if (HostBusinessEntity is ForwardingConsol)
			{
				result.JobChargeUserControl.RelatedJobFilterUpdated += JobChargeUserControl_RelatedJobFilterUpdated;
			}

			return result;
		}

		bool IsCashAdvanceFunctionalityEnabledForAROrAP()
		{
			var checker = ObjectFactory.Get<IAccCashAdvanceFunctionalityChecker>();
			return checker.IsReceivablesCashAdvanceFunctionalityEnabled || checker.IsPayablesCashAdvanceFunctionalityEnabled;
		}

		protected override ZBool HasUserControl
		{
			get { return true; }
		}

		#endregion

		#region Licence/Security

		protected override LicenceCheckpoint LicenceCheckPoint
		{
			get { return Env.Licence.Core; }
		}

		SecurityCheckpoint PluginSecurity
		{
			get
			{
				return PlugInParent != null
					? PlugInParent.InvoicingSupporter.JobInvoicingSecurity
					: Env.Security.None;
			}
		}

		JobInvoicingSecurityHelper SecurityHelper
		{
			get { return fSecurityHelper ?? (fSecurityHelper = new JobInvoicingSecurityHelper(() => PluginSecurity)); }
		}

		JobInvoicingSecurityHelper fSecurityHelper;

		#endregion

		#region Hook/Unhook Job Events

		void HookJobEvents()
		{
			Job.Charges.OnCannotDelete += new OnCannotDeleteHandler(Charges_OnCannotDelete);
			Job.OnCloseJobError += new Job.ErrorMessageHandler(Job_OnCloseJobError);
			Job.OnCloseJobYesNoQuestion += new EventHandler<UserQueryEventArgs>(Job_OnCloseJobYesNoQuestion);
			Job.OnCannotChangeStatusUserMessage += new EventHandler<UserMessageEventArgs>(Job_OnCannotChangeStatusUserMessage);

			Job.JH_OA_LocalChargesAddrInfo.ValueChanged += JH_OA_LocalChargesAddrInfo_ValueChanged;
			Job.JH_OC_LocalBillingContactInfo.ValueChanged += JH_OC_LocalBillingContactInfo_ValueChanged;
			Job.JH_OA_AgentCollectAddrInfo.ValueChanged += JH_OA_AgentCollectAddrInfo_ValueChanged;
		}

		void HookJobEventForDataRefresh()
		{
			if (fJob != null)
			{
				fJob.OnJobDeactivatedByDataRefresh += new EventHandler(Job_OnDeactivatedByDataRefresh);
				fJob.OnJobDeactivated += new EventHandler(Job_OnDeactivated);
			}
		}

		void UnHookJobEvents()
		{
			fJob.OnJobDeactivatedByDataRefresh -= new EventHandler(Job_OnDeactivatedByDataRefresh);
			fJob.OnJobDeactivated -= new EventHandler(Job_OnDeactivated);
			fJob.Charges.OnCannotDelete -= new OnCannotDeleteHandler(Charges_OnCannotDelete);
			fJob.OnCloseJobError -= new Job.ErrorMessageHandler(Job_OnCloseJobError);
			fJob.OnCloseJobYesNoQuestion -= new EventHandler<UserQueryEventArgs>(Job_OnCloseJobYesNoQuestion);
			fJob.OnCannotChangeStatusUserMessage -= new EventHandler<UserMessageEventArgs>(Job_OnCannotChangeStatusUserMessage);
			fJob.JH_OA_LocalChargesAddrInfo.ValueChanged -= JH_OA_LocalChargesAddrInfo_ValueChanged;
			fJob.JH_OC_LocalBillingContactInfo.ValueChanged -= JH_OC_LocalBillingContactInfo_ValueChanged;
			fJob.JH_OA_AgentCollectAddrInfo.ValueChanged -= JH_OA_AgentCollectAddrInfo_ValueChanged;
		}

		#endregion

		#region Dispose

		void DisposeJobIfNotNull()
		{
			if (fJob != null)
			{
				UnHookJobEvents();
				fJob.Dispose();

				DisposeAdditionalJobsForAutoRating();
			}
		}

		void DisposeAdditionalJobsForAutoRating()
		{
			foreach (var additionalJobForAutoRating in AdditionalJobsForAutoRating)
			{
				additionalJobForAutoRating.Dispose();
			}
		}

		HashSet<Job> AdditionalJobsForAutoRating { get; } = new HashSet<Job>();

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				DisposeJobIfNotNull();
				if (Factory != null)
				{
					Factory.Saved -= Factory_Saved;
					Factory.Saving -= Factory_Saving;
					Factory.RemoveContext(BusinessContext.InvoicingPluginGUIExcludingConsol);
				}
			}

			base.Dispose(disposing);
		}

		#endregion
	}
}
