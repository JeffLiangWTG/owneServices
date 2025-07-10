using System;
using System.Collections.Generic;
using System.Linq;
#if NETFRAMEWORK
using System.Runtime.Serialization;
#endif
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Interfaces;
using Enterprise.Accounting.Business.Base.Reversing;
using Enterprise.Accounting.Business.Base.Reversing.BadDebtWritingOff;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.TransactionApproval;
using Enterprise.Accounting.GUI.Base;
using Enterprise.Accounting.GUI.JobInvoicing;
using Enterprise.Accounting.Module.Transaction.Base;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module
{
	public abstract partial class AccountingTransactionController : TransactionControllerWithLoginCompanyCheck
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;
		protected List<ZGuid> ApprovalUsersForReverseTransaction;
		protected ZDateTime ApprovalDateForReverseTransaction;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502: Avoid excessive complexity")]
		public sealed override IZForm ShowDeleteForm(BusinessObject sourceEntity)
		{
			IZForm result = null;

			if (!IsMultipleReversing && sourceEntity is MultipleReversingProviderForHeader)
			{
				throw new ArgumentException(@"When reversing multiple transactions, the MultipleReversingProvider shouldn't be null.
This method should be called using controller's DeleteMultiple() method.");
			}

			BusinessObject sourceEntityForReversing = (BusinessObject)GetCurrentBusinessEntity(sourceEntity);

			bool isOperationDone;
			try
			{
				isOperationDone = DoWritingOff(sourceEntityForReversing);
				if (!isOperationDone)
				{
					var shouldCheckLevelAuthorizationForSingleTransaction = !IsMultipleReversing && ShouldCheckLevelAuthorizationForSingleTransaction(sourceEntity);
					var continueReversing = true;
					var shouldSaveApprovalRequestFactory = false;
					var approvalGUIProvider = new InvoicingPostManagerGUIWrapper(JobInvoicingPostingOption.Revenue, Factory, null, null).ARCreditNoteApprovalGUIProvider;
					if (shouldCheckLevelAuthorizationForSingleTransaction)
					{
						var approvalResult = new ARCreditNoteForReversalLevelAuthorizationWithApprovalRequest(approvalGUIProvider).PerformLevelAuthorizationForReversing(new[] { sourceEntity as InvoicingBase });
						continueReversing = approvalResult.Item1;
						shouldSaveApprovalRequestFactory = approvalResult.Item2;
					}

					if (continueReversing)
					{
						isOperationDone = DoReversing(sourceEntityForReversing);
						if (isOperationDone && shouldSaveApprovalRequestFactory && approvalGUIProvider.FactoryForApprovalRequests != null)
						{
							Factory.ChildFactories.Add(approvalGUIProvider.FactoryForApprovalRequests);
						}
					}
					else if (shouldSaveApprovalRequestFactory && approvalGUIProvider.FactoryForApprovalRequests != null)
					{
						try
						{
							approvalGUIProvider.FactoryForApprovalRequests.Save();
						}
						catch (ZSaveConcurrencyException)
						{
							Globals.Message.ShowError(Res.GetString("5e9d0c23-1b9f-45b1-9737-327591ae56e2", "While you were working, another user has modified this transaction. Please try again."));
						}
						catch (Exception ex) when (!ex.IsCriticalException())
						{
							ZExceptionReporting.HandleSaveException(ex);
						}
					}
				}
			}
			catch (InterruptOperationException)
			{
				isOperationDone = false;
			}

			if (isOperationDone || IsMultipleReversing)
			{
				result = base.ShowDeleteForm(sourceEntity);
			}

			if (IsMultipleReversing)
			{
				BusinessObject reversingResultBizo = isOperationDone ? (BusinessObject)GetBusinessEntityForForm(sourceEntityForReversing) : sourceEntityForReversing;
				if (reversingResultBizo != null)
				{
					((IReversing)reversingResultBizo).MultipleReversingErrors = ReversingErrors.ToArray();
					foreach (string error in ReversingErrors)
					{
						reversingResultBizo.RemoveRowError(error);
						reversingResultBizo.AddRowError(error);
					}
					MultipleReversingProvider.TransactionsAlreadyReversed.Add(new IReversingImplicitlyImplementedWrapperForBinding((IReversing)reversingResultBizo));
				}
				else
				{
					var sourceEntityDesc = sourceEntity != null ? sourceEntity.ToString() : "null";
					var sourceEntityForReversingDesc = sourceEntityForReversing != null ? sourceEntityForReversing.ToString() : "null";
					var reverseTransactionDesc = Reversing != null && Reversing.ReverseTransaction != null ? sourceEntityForReversing.ToString() : "null";
					var originalTransationDesc = Reversing != null && Reversing.OriginalTransaction != null ? sourceEntityForReversing.ToString() : "null";
					ErrorReporter.ReportOnce(string.Format("Reversing Result was null. SourceEntity: {0}, SourceEntityForReversing: {1}, ReverseTransaction: {2}, OriginalTransaction: {3}",
						sourceEntityDesc, sourceEntityForReversingDesc, reverseTransactionDesc, originalTransationDesc));
				}
			}

			return result;
		}

		bool ShouldCheckLevelAuthorizationForSingleTransaction(BusinessObject sourceEntity)
		{
			var isARInvoice = sourceEntity is ARInvoice;
			var arAdjNote = sourceEntity as ARAdjustmentNote;
			var ispositiveARAdjustmentNote = arAdjNote != null && arAdjNote.AH_LocalTotalAmount > 0;
			return !(sourceEntity as TransactionHeader).AH_IsCancelled && (isARInvoice || ispositiveARAdjustmentNote);
		}

		protected IZForm ShowEditFormBase(BusinessObject sourceEntity)
		{
			return base.ShowEditForm(sourceEntity);
		}

		protected override void DeleteMultipleCore(BusinessObject[] selectedBusinessObjects)
		{
			MultipleReversingProvider = selectedBusinessObjects.Length == 1 ?
				selectedBusinessObjects[0] as MultipleReversingProviderForHeader : null;
			if (IsMultipleReversing)
			{
				ShowDeleteForm(MultipleReversingProvider);
			}
			else
			{
				Globals.Message.ShowInformation(Res.GetString("628CAE3B-17B9-4cb7-8371-D714E8C2EFAF", "The action for multiple objects is not implemented in this version."));
			}
		}

		protected override bool DisallowMultiDeleteBecauseDeleteIsNotWhatIsReallyHappeningInAccounting
		{
			get { return false; }
		}

		public override SecurityCheckpoint GetCheckPointForDelete(BusinessObject bizObject)
		{
			return bizObject is MultipleReversingProviderForHeader ? Env.Security.None : CheckPointForDelete;
		}

		protected sealed override IZForm GetForm(IBusiness businessEntity)
		{
			return IsMultipleReversing ? new MultipleReversingForHeaderForm(MultipleReversingProvider) : GetFormCore(GetBusinessEntityForForm(businessEntity));
		}

		protected abstract IZForm GetFormCore(IBusiness businessEntity);

		protected override string GetIDForFormCache(IBusiness businessEntity)
		{
			var result = base.GetIDForFormCache(businessEntity);
			if (Reversing != null)
			{
				result += (NoResString)"Reversing";
			}
			return result;
		}

		public sealed override ControllerID ID
		{
			get
			{
				return IsMultipleReversing ? ControllerIDs.APInvoice : IDCore;
			}
		}

		protected abstract ControllerID IDCore { get; }

		protected override BusinessObjectFactory GetNewFactory()
		{
			return IsMultipleReversing ? MultipleReversingProvider.Factory : base.GetNewFactory();
		}

		#region Implementation

		protected sealed override IBusiness GetLoadedBusinessEntityInLocalFactoryCore(IBusiness sourceEntity)
		{
			return sourceEntity is MultipleReversingProviderForHeader ? sourceEntity : GetLoadedBusinessEntityInLocalFactoryCore2(sourceEntity);
		}

		protected virtual IBusiness GetLoadedBusinessEntityInLocalFactoryCore2(IBusiness sourceEntity)
		{
			return GetTopLevelBusinessObjectCached(sourceEntity);
		}

		//Disable the copy function by default. Child classes that need to enable copying should override this function and call 'ShowTemplateCopyFormFromBase' to create a copy
		public override IZForm ShowTemplateCopyForm(BusinessObject inMemorySourceEntity)
		{
			Globals.Message.ShowInformation(Res.GetString("ccf26749-aba6-496d-918b-78d286b6fa9a", "The copy function is only used for AR Invoice, AP Invoice, Bank Transfer, Direct Receipt and Direct Payment at this point"), Res.GetString("259c084b-9eb0-45e9-97f5-fb1da2aa8e9b", "Copy Transaction"));
			return null;
		}

		public IZForm ShowTemplateCopyFormFromBase(BusinessObject inMemorySourceEntity)
		{
			return base.ShowTemplateCopyForm(inMemorySourceEntity);
		}

		protected virtual IBusiness GetTopLevelBusinessObject(IBusiness sourceEntity)
		{
			return sourceEntity;
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.None; }
		}

		protected ReversingBase Reversing
		{
			get { return fReversing; }
#if DEBUG
			set { fReversing = value; }
#endif
		}

		protected bool DoWritingOff(IBusiness sourceEntity)
		{
			bool isOperationDone = false;
			IBusiness transaction = GetTopLevelBusinessObjectCached(sourceEntity);
			IBadDebtWritingOff badDebtSourceEntity = sourceEntity as IBadDebtWritingOff;
			IBadDebtWritingOff badDebtTransaction = transaction as IBadDebtWritingOff;
			if (badDebtTransaction != null && badDebtSourceEntity != null)
			{
				badDebtTransaction.IsWritingOff = badDebtSourceEntity.IsWritingOff;
				if (badDebtTransaction.IsWritingOff)
				{
					BadDebtWritingOffFactory badDebtWritingOffFactory = new BadDebtWritingOffFactory();
					fReversing = badDebtWritingOffFactory.NewWritingOff(badDebtTransaction);
					CurrentMessageBoxCaption = CantWriteOffMessageBoxCaption;
					if (!(isOperationDone = DoBaseReversing(transaction)))
					{
						throw new InterruptOperationException();
					}
				}
			}
			return isOperationDone;
		}

		protected bool DoReversing(IBusiness sourceEntity)
		{
			bool isOperationDone = false;
			IBusiness transaction = GetTopLevelBusinessObjectCached(sourceEntity);
			if (transaction is IReversing)
			{
				ReversingFactory reversingFactory = new ReversingFactory();
				fReversing = reversingFactory.NewReversing((IReversing)transaction);
				fReversing.IsPartOfMultipleReversing = IsMultipleReversing;
				CurrentMessageBoxCaption = CantReverseMessageBoxCaption;
				if (sourceEntity is InvoicingBase invoiceSourceEntity)
				{
					ApprovalUsersForReverseTransaction = invoiceSourceEntity.ApprovingUserPKList;
					ApprovalDateForReverseTransaction = invoiceSourceEntity.ApprovalDate;
				}
				if (!(isOperationDone = DoBaseReversing(transaction)))
				{
					throw new InterruptOperationException();
				}
			}
			return isOperationDone;
		}

		protected bool DoBaseReversing(IBusiness transaction)
		{
			bool isOperationDone = false;
			var result = true;
			if (fReversing != null)
			{
				BeforeBaseReversing(transaction);
				var transactionAsBizObj = (BusinessObject)transaction;
				SecurityCheckpoint controllerDeleteCheckPoint, firstDisallowedReversingCheckpoint;
				if (IsMultipleReversing && MultipleReversingProvider.TransactionsWithLevelAuthorizationProblems.Contains(transactionAsBizObj.PK))
				{
					ShowErrorMessage(Res.GetString("82ad780b-6d34-468e-a00e-45bb6d8ab2f2", "You do not have sufficient security rights to post a credit note for this amount and this reversal does not have an approved request at this time."));
					result = false;
				}
				else if (!fReversing.CanReverseTransaction)
				{
					ShowErrorMessage(fReversing.CantReverseErrorMessage);
					result = false;
				}
				else if (IsMultipleReversing && !(controllerDeleteCheckPoint = GetCheckPointForDelete(transactionAsBizObj)).IsAllowed)
				{
					ShowErrorMessage(controllerDeleteCheckPoint.ErrorMessageForNotAllowed);
					result = false;
				}
				else if ((firstDisallowedReversingCheckpoint = fReversing.CheckpointsToReverse.FirstOrDefault(cp => !cp.IsAllowed)) != null)
				{
					ShowErrorMessage(firstDisallowedReversingCheckpoint.ErrorMessageForNotAllowed);
					result = false;
				}
				else if (fReversing.ShouldShowReverseConfirmationMessage())
				{
					var message = fReversing.GetReverseConfirmationMessage();
					result = Globals.Message.Show(message, AccountingConstants.ReverseConfirmationCaptionText, MessageBoxButtons.YesNo, MessageBoxIcon.Warning, DialogResult.No) == DialogResult.Yes;
					if (!result && IsMultipleReversing)
					{
						ReversingErrors.Add(Res.GetString("587F3DA2-DEE2-40F4-BE6F-90B9F718C7BA",
@"User has answered 'No' to the next question:
{0}",
							message));
					}
				}

				if (result)
				{
					fReversing.Reverse();
					isOperationDone = true;
				}
				AfterBaseReversing(transaction);
			}
			else
			{
				ShowErrorMessage(CantWriteOffMessage);
			}
			return isOperationDone;
		}

		protected virtual void BeforeBaseReversing(IBusiness transaction)
		{
		}

		protected virtual void AfterBaseReversing(IBusiness transaction)
		{
		}

		[Serializable]
		protected class InterruptOperationException : Exception
		{
			public InterruptOperationException()
			{
			}

			public InterruptOperationException(string message)
				: base(message)
			{
			}

			public InterruptOperationException(string message, Exception inner)
				: base(message, inner)
			{
			}

#if NETFRAMEWORK
			protected InterruptOperationException(SerializationInfo info, StreamingContext context)
				: base(info, context)
			{
			}
#endif
		}

		ReversingBase fReversing;
		ZGuid latestSourceEntityIdentifier;

		protected virtual ZString CantReverseMessageBoxCaption
		{
			get { return Res.GetString("4cfd3313-b09d-4f91-b19a-d3f18dedf2d0", "Reversing Transaction"); }
		}

		protected ZString CantReverseMessage
		{
			get { return Res.GetString("e6057cbe-3b7b-434a-aa8b-3f684ddc4397", "Reversing can not be done"); }
		}

		protected ZString CantWriteOffMessageBoxCaption
		{
			get { return Res.GetString("84031f9f-afee-4798-a6b0-7c2e78c94e68", "Writing Off Transaction"); }
		}

		protected ZString CantWriteOffMessage
		{
			get { return Res.GetString("bb9e06e2-8ca3-4d4f-9df0-590373803399", "Writing Off can not be done"); }
		}

		protected ZString CurrentMessageBoxCaption;

		IBusiness latestTopLevelBusinessObject;

		IBusiness GetTopLevelBusinessObjectCached(IBusiness sourceEntity)
		{
			var settings = this as IBusinessEntityFactorySettings;
			if (settings != null && settings.ShouldUseSourceEntityFactory)
			{
				latestSourceEntityIdentifier = sourceEntity.Identifier;
#if DEBUG
				GetTopLevelBusinessObjectCounter++;
#endif
				latestTopLevelBusinessObject = GetTopLevelBusinessObject(sourceEntity);
			}
			else if (latestSourceEntityIdentifier != sourceEntity.Identifier)
			{
				var bizo = sourceEntity as BusinessObject;
				if (bizo != null && bizo.IsInDatabase)
				{
					sourceEntity = Factory.Load(sourceEntity.GetType(), sourceEntity.Identifier);
				}
				latestSourceEntityIdentifier = sourceEntity == null ? ZGuid.Empty : sourceEntity.Identifier;
#if DEBUG
				GetTopLevelBusinessObjectCounter++;
#endif
				latestTopLevelBusinessObject = GetTopLevelBusinessObject(sourceEntity);
			}
			return latestTopLevelBusinessObject;
		}

#if DEBUG
		public int GetTopLevelBusinessObjectCounter;
#endif

		protected virtual bool ShouldHaveReversedBizo
		{
			get { return true; }
		}

		protected MultipleReversingProviderForHeader MultipleReversingProvider;

		List<string> ReversingErrors = new List<string>();

		protected void ShowErrorMessage(string errorMessage)
		{
			if (!string.IsNullOrEmpty(errorMessage))
			{
				if (IsMultipleReversing)
				{
					ReversingErrors.Add(errorMessage);
				}
				else
				{
					Globals.Message.ShowError(errorMessage, CurrentMessageBoxCaption);
				}
			}
		}

		protected bool IsMultipleReversing
		{
			get { return MultipleReversingProvider != null; }
		}

		IBusiness GetCurrentBusinessEntity(IBusiness businessEntity)
		{
			return IsMultipleReversing ? MultipleReversingProvider.Current : businessEntity;
		}

		IBusiness GetBusinessEntityForForm(IBusiness businessEntity)
		{
			IBusiness businessEntityForForm = GetCurrentBusinessEntity(businessEntity);
			if (Reversing != null)
			{
				businessEntityForForm = ShouldHaveReversedBizo ? Reversing.ReverseTransaction : Reversing.OriginalTransaction;
			}
			return businessEntityForForm;
		}

#endregion
	}
}
