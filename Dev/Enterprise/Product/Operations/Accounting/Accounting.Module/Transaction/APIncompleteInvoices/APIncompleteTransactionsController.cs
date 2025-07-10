using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.GUI;
using Enterprise.Accounting.Module.Transaction.Base;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using static Enterprise.Accounting.Business.ARAP.Invoicing.InvoicingBase;

namespace Enterprise.Accounting.Module.Transaction
{
	public abstract class APIncompleteTransactionsController : TransactionControllerWithReadOnlyBehaviourControlledBySource, INavigationControllerIDProvider
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.APIncompleteInvoicesDelete; }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.APIncompleteInvoices; }
		}

		protected virtual SecurityCheckpoint CheckPointForCancel
		{
			get { return Env.Security.APIncompleteInvoicesCancel; }
		}

		protected override string AlreadyDeletedOrIrreversiblyChangedMessageCore
		{
			get
			{
				return Res.GetString("1b866cf4-6a54-4c9a-a1c8-30cde14dfd93", "The AP Incomplete invoice cannot be displayed because another user has changed, deleted or posted the record. Closing and re-opening this window will refresh your data.");
			}
		}

		protected override IBusiness GetLoadedBusinessEntityInLocalFactoryCore(IBusiness sourceEntity)
		{
			var invoice = (InvoicingBase)sourceEntity;
			if (invoice.HasApprovalRequest)
			{
				return sourceEntity;
			}
			else
			{
				return base.GetLoadedBusinessEntityInLocalFactoryCore(sourceEntity);
			}
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			ReportInvalidDataSource(businessEntity);

			IZForm result = null;
			var invoice = (InvoicingBase)businessEntity;

			if (invoice.AH_Ledger == LedgerTypes.IncompleteTransactions)
			{
				invoice.SubmittedFromInvoicingForm = true;
				if (!invoice.HasApprovalRequest)
				{
					if (!(AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.Value && invoice.UserAllowedToBackPost))
					{
						invoice.AH_PostDate = ZDateTime.Today;
					}

					var restoreResult = invoice.RestoreSavedData();

					if (restoreResult.Result != RestoreSavedDataResult.ResultType.Success)
					{
						var caption = Res.GetString("C2519C27-667C-422B-B77C-269E36D59984", "Unable to View / Edit");
						if (restoreResult.Result == RestoreSavedDataResult.ResultType.SuccessWithErrors)
						{
							ShowError(restoreResult.Error, caption);
						}
						else
						{
							Globals.Message.ShowError(restoreResult.Error, caption);
						}

						return null; // Ideally, 'null' should be returned only in the case of 'Failed' result from InvoicingBase.RestoreSavedData' and incomplete invoice form should be shown in cases of 'Success' and 'SuccessWithErrors.
									 // But 'null' is returned in the case of 'SuccessWithErrors' as well to keep the funtionality existing at the time of this change.
					}
				}
			}

			if (!invoice.HasErrors && invoice.IsIncompleteInvoice && invoice.Lines.Count == 0)
			{
				ErrorReporter.ReportOnce("Incomplete transaction must have transaction lines");
			}

			IZForm alreadyOpenedForm = GetFormIfTransactionIsAlreadyOpen(invoice);
			if (alreadyOpenedForm != null)
			{
				var formCache = OpenedFormCache.GetInstance();
				formCache.SwitchToCachedForm(invoice.PK.ToGuid(), alreadyOpenedForm.ControllerID.Name);
				result = alreadyOpenedForm;
			}
			else
			{
				switch (invoice.AH_TransactionType)
				{
					case TransactionTypes.Invoice:
					case TransactionTypes.IncompleteInvoice:
						result = new InvoiceForm(invoice);
						break;
					case TransactionTypes.CreditNote:
					case TransactionTypes.IncompleteCreditNote:
						result = new CreditNoteForm(invoice);
						break;
					case TransactionTypes.AdjustmentNote:
					case TransactionTypes.IncompleteAdjustmentNote:
						result = new AdjustmentNoteForm(invoice);
						break;
				}
			}
			return result;
		}

		void ShowError(string message, string caption)
		{
			if (LoadedFormAction == FormAction.View || LoadedFormAction == FormAction.Edit)
			{
				var formActionString = LoadedFormAction == FormAction.View
					? Res.GetString("e538f207-ab1f-4121-8c01-1ac142b4fce5", "viewed")
					: Res.GetString("3ed13459-1bfe-4609-b1d2-879afc74fcd3", "edited");
				Globals.Message.ShowError(
					Res.GetString("f9fbeff9-d33c-4edb-a573-af096f9e194e",
						"This transaction cannot be {0}. Please Delete or Cancel this record.\r\nThis transaction contains obsolete data that cannot be successfully restored.\r\n{1}",
						formActionString, message), caption);
			}
			else if (CancelInsteadOfDelete && SourceBusinessEntity is ICancellable && ShowPreDeleteOrCancelDialogs(Res.GetString("9a12ea5b-5486-43bb-ba67-3e8a6e12eab0", "cancellation"), Res.GetString("388da539-905c-4ccc-abe7-1cd5d3daf1e1", "Cancel Confirmation"), message))
			{
				((ICancellable)SourceBusinessEntity).IsCancelled = true;
				SourceBusinessEntity.Factory.Save();
			}
			else if (LoadedFormAction == FormAction.Delete && ShowPreDeleteOrCancelDialogs(Res.GetString("99e5c302-2e4a-4b42-9695-0320199c4612", "deletion"), Res.GetString("9a459652-8ec6-4f78-afbd-73027fcc7ba9", "Delete Confirmation"), message))
			{
				SourceBusinessEntity.Delete();
				SourceBusinessEntity.Factory.Save();
			}
			else
			{
				Globals.Message.ShowError(message, caption);
			}
		}

		bool ShowPreDeleteOrCancelDialogs(string formActionString, string caption, string errorMessage)
		{
			var message = Res.GetString("5d468786-7287-4d47-b743-f427dc470e94", "This transaction cannot be viewed before {0}.\r\nThis transaction contains obsolete data that cannot be successfully restored.\r\nDo you want to proceed with the {0} of this transaction?\r\n{1}", formActionString, errorMessage);
			return Globals.Message.Show(message, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Warning, DialogResult.No) == DialogResult.Yes;
		}

		FormAction LoadedFormAction;
		IBusiness SourceBusinessEntity;
		bool CancelInsteadOfDelete;

		protected override IZForm ShowLoadedForm(IBusiness sourceEntity, FormAction action)
		{
			LoadedFormAction = action;
			SourceBusinessEntity = sourceEntity;
			return base.ShowLoadedForm(sourceEntity, action);
		}

		protected override bool CheckControllerIDMismatch(ControllerID controllerID) => !GetValidControllerIDCollection().Contains(controllerID);

		protected virtual IEnumerable<ControllerID> GetValidControllerIDCollection()
		{
			return new List<ControllerID> { ID };
		}

		public override IZForm ShowEditForm(BusinessObject sourceEntity)
		{
			IZForm result = null;

			var cancellableEntity = sourceEntity as ICancellable;
			var invoice = sourceEntity as InvoicingBase;
			if (cancellableEntity != null && cancellableEntity.IsCancelled)
			{
				Globals.Message.ShowInformation(Res.GetString("d1ed9f02-56e3-4416-96e7-9ef566d703de", "This transaction is canceled and cannot be modified."),
					EditIncompleteTransactionMessage);
			}
			else if (invoice.AH_Ledger == LedgerTypes.IncompleteTransactions && invoice.AH_TransactionType == TransactionTypes.IncompleteCreditNote && AccountingMasterFilesUtils.ShouldPreventCreateCreditNote(invoice.AH_Ledger, invoice.AH_GC))
			{
				Globals.Message.ShowInformation(Res.GetString("D22B5784-680B-4655-8612-EEA7462BF739", "This transaction cannot be modified as {0}", AccountingMasterFilesUtils.APCreditNoteDisallowedMessage),
					EditIncompleteTransactionMessage);
			}
			else
			{
				result = TrySwitchToExistingInvoiceForm(invoice) ?? base.ShowEditForm(sourceEntity);
			}

			return result;
		}

		public IZForm ShowCancelForm(BusinessObject sourceEntity)
		{
			CancelInsteadOfDelete = true;
			return ShowCancelFormCore(sourceEntity);
		}

		protected virtual IZForm ShowCancelFormCore(BusinessObject sourceEntity)
		{
			AccountingZForm result = null;
			if (sourceEntity is ICancellable)
			{
				if (!((ICancellable)sourceEntity).IsCancelled)
				{
					if (CheckPointForCancel.IsAllowed)
					{
						result = (AccountingZForm)SwitchToOpenFormForCancelOrDeleteAction(sourceEntity);

						if (result == null)
						{
							result = (AccountingZForm)ShowLoadedForm(sourceEntity, FormAction.Delete);
							if (result != null)
							{
								PrepareFormForCanceling(result);
							}
						}
					}
					else
					{
						CheckPointForCancel.ShowError();
					}
				}
				else
				{
					Globals.Message.ShowInformation(Res.GetString("d3560d85-b7d2-48a4-8cb5-bbfb6c385b44", "This transaction is already canceled."),
						Res.GetString("de33b0bb-76ae-4079-aacc-a649009c0c40", "Cancel Incomplete Transaction"));
				}
			}
			return result;
		}

		protected virtual IEnumerable<ControllerID> GetRelatedControllerIDs() => Enumerable.Empty<ControllerID>();

		IZForm TrySwitchToExistingInvoiceForm(InvoicingBase dataSource)
		{
			if (dataSource != null)
			{
				foreach (var controllerID in GetRelatedControllerIDs())
				{
					var controller = ZControllerFactory.Create(controllerID);
					if (controller != null && controller.IsFormShownFor(dataSource))
					{
						controller.SwitchToFormFor(dataSource);
						var form = controller.LastShownForm;
						if (form != null)
						{
							return form;
						}
					}
				}
			}

			return null;
		}

		void PrepareFormForCanceling(AccountingZForm form1)
		{
			string originalVerb = form1.FormVerb;
			form1.CancelInsteadOfDelete = true;
			form1.Text = form1.Text.Replace(originalVerb, form1.FormVerb);

			IPostingButtonsProvider form = form1;
			form.CommandButtonPost.Text = Res.GetString("be4a89d7-e3a6-4980-b415-07d46f629d1a", "&Cancel");
			form.CommandButtonCancel.Text = Res.GetString("c0df5b67-2445-4ff4-8d1b-1989d6594d41", "C&lose");
		}

		public override IZForm ShowDeleteForm(BusinessObject sourceEntity)
		{
			IZForm result = null;
			if (sourceEntity is ICancellable)
			{
				if (((ICancellable)sourceEntity).IsCancelled)
				{
					Globals.Message.ShowError(Res.GetString("2ff08a99-d5b2-4236-b200-9e441a4d6be2", "Canceled incomplete invoices cannot be deleted."));
				}
				else
				{
					result = SwitchToOpenFormForCancelOrDeleteAction(sourceEntity) ?? base.ShowDeleteForm(sourceEntity);
				}
			}
			return result;
		}

		IZForm GetFormIfTransactionIsAlreadyOpen(InvoicingBase invoice)
		{
			IZForm alreadyOpenedForm = null;
			if (invoice.AH_Ledger == LedgerTypes.IncompleteTransactions)
			{
				var formCache = OpenedFormCache.GetInstance();
				var bizObjPK = invoice.PK.ToGuid();

				if (formCache.Contains(bizObjPK, ControllerIDs.APInvoice.Name))
				{
					alreadyOpenedForm = formCache.GetForm(bizObjPK, ControllerIDs.APInvoice.Name) as IZForm;
				}
				else if (formCache.Contains(bizObjPK, ControllerIDs.APCreditNote.Name))
				{
					alreadyOpenedForm = formCache.GetForm(bizObjPK, ControllerIDs.APCreditNote.Name) as IZForm;
				}
				else if (formCache.Contains(bizObjPK, ControllerIDs.APAdjustmentNote.Name))
				{
					alreadyOpenedForm = formCache.GetForm(bizObjPK, ControllerIDs.APAdjustmentNote.Name) as IZForm;
				}
			}
			return alreadyOpenedForm;
		}

		IZForm SwitchToOpenFormForCancelOrDeleteAction(BusinessObject sourceEntity)
		{
			if (sourceEntity != null)
			{
				var invoice = sourceEntity as InvoicingBase;
				IZForm existingForm = null;
				var controller = ZControllerFactory.Create(this.ID);

				if (controller != null && controller.IsFormShownFor(invoice))
				{
					controller.SwitchToFormFor(invoice);
					existingForm = controller.LastShownForm;
					if (existingForm != null)
					{
						return existingForm;
					}
				}

				existingForm = GetFormIfTransactionIsAlreadyOpen(invoice);

				if (existingForm != null)
				{
					OpenedFormCache.GetInstance().SwitchToCachedForm(invoice.PK.ToGuid(), existingForm.ControllerID.Name);
					return existingForm;
				}
			}
			return null;
		}

		#region INavigationControllerIDProvider

		ControllerID INavigationControllerIDProvider.GetValidControllerID(object dataSource)
		{
			var result = ID;
			var invoice = dataSource as AccTransactionHeader;

			if (invoice == null)
			{
				return result;
			}

			if (invoice.AH_Ledger == LedgerTypes.AccountsPayable || invoice.AH_Ledger == LedgerTypes.IncompleteTransactions)
			{
				result = new AccountingControllerIdDecider().GetControllerID(invoice.AH_TransactionType, invoice.AH_Ledger, GetValidControllerIdHelper.IsToSkipModuleId(ID) ? null : ModuleID);
			}

			return result;
		}

		public bool ShouldLoadBusinessObject { get => true; }

		string EditIncompleteTransactionMessage => Res.GetString("ad1e662e-5909-4668-a744-0c92f14a0f49", "Edit Incomplete Transaction");
		#endregion
	}
}
