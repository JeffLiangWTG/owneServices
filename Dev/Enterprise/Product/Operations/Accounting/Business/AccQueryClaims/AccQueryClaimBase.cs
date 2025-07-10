using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Reversing;
using Enterprise.Accounting.Business.EmailNotification;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.AccQueryClaims
{
	public abstract class AccQueryClaimBase : AccQueryClaim, IDocumentSupportable
	{
		public AccQueryClaimBase(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Properties

		#region Overriden Properties

		protected override bool AY_OH_Debtor_ReadOnly
		{
			get { return base.AY_OH_Debtor_ReadOnly || RelatedUnapprovedCreditNote != null; }
		}

		protected override bool AY_AH_ReadOnly
		{
			get { return base.AY_AH_ReadOnly || RelatedUnapprovedCreditNote != null; }
		}

		public override ZString AY_QueryClaimStatus
		{
			get
			{
				return base.AY_QueryClaimStatus;
			}
			set
			{
				if (GetClosedStatuses().Any(x => x == value) && !IsIntercompanyClaim)
				{
					if (!RaiseCancelling())
					{
						CancelRelatedUnapprovedCreditNote();
					}
				}
				base.AY_QueryClaimStatus = value;
			}
		}

		protected override bool AY_QueryClaimStatus_ReadOnly
		{
			get { return IsIntercompanyClaim; }
		}

		protected override bool IsPropertiesReadOnly
		{
			get { return base.IsPropertiesReadOnly || IsClosed || IsClosedByOriginalValue; }
		}

		#endregion

		#region IsClosed

		public bool IsClosed
		{
			get { return GetClosedStatuses().Any(x => x == AY_QueryClaimStatus); }
		}

		public bool IsClosedByOriginalValue
		{
			get { return GetClosedStatuses().Any(x => x == ((ZString)AY_QueryClaimStatusInfo.OriginalValue)); }
		}

		public static string[] GetClosedStatuses()
		{
			return new[]
			{
					QueryClaimStatusCodeList.Codes.QCStatus3AcceptedAndCreditNoteIssuedAndClosed,
					QueryClaimStatusCodeList.Codes.QCStatus5RejectedClosed,
					QueryClaimStatusCodeList.Codes.QCStatus6CancelledAndClosed,
					QueryClaimStatusCodeList.Codes.QCStatus9AcceptedClosed
			};
		}

		#endregion

#if DEBUG
		public INumberFountainProxy NumberFountainNo_ForTestOnly
		{
			get { return NumberFountainNo; }
		}

#endif
		protected abstract INumberFountainProxy NumberFountainNo { get; }

		public bool IsReassigned { get; set; }

		#endregion

		#region Methods

		#region Overriden Methods

		protected override void OnFactorySaving()
		{
			base.OnFactorySaving();
			if (IsReassigned && StaffAssignedTo != null && !StaffAssignedTo.GS_EmailAddress.IsEmpty)
			{
				QueryClaimReassignedEmail email = new QueryClaimReassignedEmail(this);
				email.Send();
			}
		}

		public override void OnSaving()
		{
			if (!IsInDatabase)
			{
				AY_QueryClaimReference = NumberFountainNo.GetNextFormatted(Factory);
			}
			base.OnSaving();
		}

		protected override AccQueryClaimValidation GetNewValidation()
		{
			return new AccQueryClaimBaseValidation(this);
		}

		#endregion

		public virtual void Approve()
		{
		}

		public virtual void Reject()
		{
		}

		public virtual bool CanAddClaimDetails
		{
			get { return false; }
		}

		public void CancelRelatedUnapprovedCreditNote()
		{
			if (RelatedUnapprovedCreditNote != null && !IsIntercompanyClaim)
			{
				if (RelatedUnapprovedCreditNote.IsInDatabase)
				{
					ReversingFactory reversingFactory = new ReversingFactory();
					ReversingBase reversing = reversingFactory.NewReversing(RelatedUnapprovedCreditNote);
					reversing.Reverse();
				}
				RelatedUnapprovedCreditNote.Delete();
				fRelatedUnapprovedCreditNote = null;
			}
		}

		public event CancelEventHandler Canceling;

		bool RaiseCancelling()
		{
			bool isCancel = false;
			if (Canceling != null)
			{
				CancelEventArgs eventArgs = new CancelEventArgs(isCancel);
				Canceling(this, eventArgs);
				return eventArgs.Cancel;
			}
			return isCancel;
		}

		#endregion

		#region RelatedUnapprovedCreditNote

		public UACreditNote RelatedUnapprovedCreditNote
		{
			get
			{
				return fRelatedUnapprovedCreditNote ?? (fRelatedUnapprovedCreditNote = GetRelatedUnapprovedCreditNote());
			}
			set
			{
				if (fRelatedUnapprovedCreditNote != value)
				{
					UACreditNote oldRelatedUnapprovedCreditNote = fRelatedUnapprovedCreditNote;
					fRelatedUnapprovedCreditNote = value;
					if (oldRelatedUnapprovedCreditNote != null)
					{
						TransactionHeader.AH_TransactionBelongsToGroup = ZGuid.Empty;
						oldRelatedUnapprovedCreditNote.AH_TransactionBelongsToGroup = ZGuid.Empty;
						oldRelatedUnapprovedCreditNote.RelatedClaim = null;
						UnRegisterEditableChildObject(oldRelatedUnapprovedCreditNote);
					}
					if (value != null)
					{
						if (!TransactionHeader.AH_TransactionBelongsToGroup.IsValid)
						{
							TransactionHeader.AH_TransactionBelongsToGroup = ZGuid.NewZGuid();
						}
						fRelatedUnapprovedCreditNote.AH_TransactionBelongsToGroup = TransactionHeader.AH_TransactionBelongsToGroup;
						RegisterEditableChildObject(fRelatedUnapprovedCreditNote);
					}
					AY_OH_DebtorInfo.RefreshBinding();
					AY_AHInfo.RefreshBinding();
				}
			}
		}
		UACreditNote fRelatedUnapprovedCreditNote;

		public UACreditNote GetRelatedUnapprovedCreditNote()
		{
			if (TransactionHeader == null)
			{
				return null;
			}

			ZQuery uaCreditNoteQuery = new ZQuery(AccTransactionHeaderSchema.AH_TransactionBelongsToGroup, TransactionHeader.AH_TransactionBelongsToGroup);
			uaCreditNoteQuery.AddToFilter(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.UnapprovedPayableTransactions);
			uaCreditNoteQuery.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.UACreditNote);
			uaCreditNoteQuery.AddToFilter(AccTransactionHeaderSchema.AH_IsCancelled, false);
			uaCreditNoteQuery.AddToFilter(AccTransactionHeaderSchema.AH_GC, TransactionHeader.AH_GC);
			return Factory.LoadTop1<UACreditNote>(uaCreditNoteQuery);
		}

		#endregion

		#region MementoForAssignClaimAction

		public object SaveToMementoForAssignClaimAction()
		{
			return new MementoForAssignClaimAction(this);
		}

		public void RestoreMementoForAssignClaimAction(object memento)
		{
			((MementoForAssignClaimAction)memento).RestoreMemento(this);
		}

		class MementoForAssignClaimAction
		{
			public MementoForAssignClaimAction(AccQueryClaimBase claim)
			{
				Details = claim.Details;
				AY_GS_NKStaffAssignedTo = claim.AY_GS_NKStaffAssignedTo;
				AY_GB = claim.AY_GB;
				HasChanges = claim.HasChanges;
			}

			public void RestoreMemento(AccQueryClaimBase claim)
			{
				claim.Details = Details;
				claim.AY_GS_NKStaffAssignedTo = AY_GS_NKStaffAssignedTo;
				claim.AY_GB = AY_GB;
				claim.HasChanges = HasChanges;
			}

			readonly ZString Details;
			readonly ZString AY_GS_NKStaffAssignedTo;
			readonly ZGuid AY_GB;
			readonly bool HasChanges;
		}

		#endregion

		#region IDocumentSupportable Members

		DocumentSupporter IDocumentSupportable.DocumentSupporter
		{
			get { return new AccQueryClaimBaseDocumentSupporter(this); }
		}

		public class AccQueryClaimBaseDocumentSupporter : DocumentSupporter
		{
			public AccQueryClaimBaseDocumentSupporter(AccQueryClaimBase queryClaim)
				: base(queryClaim)
			{
			}

			AccQueryClaimBase QueryClaim
			{
				get { return (AccQueryClaimBase)BusinessObject; }
			}

			public override BusinessContext BusinessContext
			{
				get { return QueryClaim.Ledger == LedgerTypes.AccountsReceivable ? BusinessContext.ARAccQueryClaim : BusinessContext.APAccQueryClaim; }
			}

			protected override DocumentWrapper[] GetDocumentWrappersInternal(Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
			{
				return DocumentWrapperFactory.GenerateGenericWrappers(dataContext, QueryClaim);
			}

			protected override Constants.DataContext[] GetSupportedDataContexts()
			{
				return new[] { Constants.DataContext.GenericFreightJob };
			}

			public override ISecurityCheckpoint CustomisationSecurityCheckpoint
			{
				get { return QueryClaim.Ledger == LedgerTypes.AccountsReceivable ? Env.Security.ReceivablesCustomiseDocuments : Env.Security.PayablesCustomiseDocuments; }
			}
		}

		#endregion

		#region ICanDelete

		public override bool CanDelete
		{
			get
			{
				return !IsClosed && !IsIntercompanyClaim;
			}
		}

		public override MultilingualString ReasonForNotAbleToDelete
		{
			get
			{
				if (IsClosed)
				{
					return ResString.GetMultilingualString("134fd8d8-2df7-41e1-9fd7-2ddf46026314", @"This claim is already closed and can not be canceled.");
				}
				else if (IsIntercompanyClaim)
				{
					return ResString.GetMultilingualString("1b1fbc5a-aec9-48f5-92bf-3f0620286cc9", @"Intercompany claim can not be canceled.");
				}
				else
				{
					return base.ReasonForNotAbleToDelete;
				}
			}
		}

		#endregion

#if DEBUG

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			if ((kind & TestBusinessObjectKind.MinimumRequiredToSave) != 0)
			{
				AY_OC = Factory.NewWithValidTestData<OrgContact>().PK;
			}
		}

#endif
	}
}
