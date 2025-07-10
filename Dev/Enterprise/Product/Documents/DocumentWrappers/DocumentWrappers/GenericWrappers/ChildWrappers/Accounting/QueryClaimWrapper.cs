using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	[DefaultField("JobNumber"), WrapperTypeName("Query Claim")]
	public class QueryClaimWrapper : GenericWrapper
	{
		public QueryClaimWrapper(AccQueryClaim queryClaim, BusinessObjectFactory factory)
			: base(queryClaim, factory)
		{
			this.queryClaim = queryClaim;
		}

		protected readonly AccQueryClaim queryClaim;

		#region Wrapper Properties

		public OrgHeader Debtor
		{
			get { return queryClaim.Debtor; }
		}

		public ContactWrapper Contact
		{
			get { return new ContactWrapper(queryClaim.Contact, Factory); }
		}

		public RefCurrency TransactionCurrency
		{
			get { return Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, queryClaim.AY_RX_TransactionCurrencyCode); }
		}

		public MoneyWrapper TransactionAmount
		{
			get { return new MoneyWrapper(new Money(queryClaim.AY_QueryClaimAmount, TransactionCurrency), Factory); }
		}

		#endregion

		#region Z Types

		public ZString JobNumber
		{
			get { return queryClaim.AY_QueryClaimReference; }
		}

		public ZString InvoiceNo
		{
			get { return queryClaim.TransactionHeader == null ? ZString.Empty : queryClaim.TransactionHeader.AH_TransactionNum; }
		}

		public ZString Amount
		{
			get { return TransactionCurrency == null ? queryClaim.AY_QueryClaimAmount.ToString(2) : TransactionCurrency.RX_Symbol + TransactionAmount.AmountAndCurrencyCode; }
		}

		public ZString ShortDescription
		{
			get { return queryClaim.AY_ShortDescriptionOfClaim; }
		}

		public ZString TypeCode
		{
			get { return queryClaim.AY_QueryClaimType; }
		}

		public ZString TypeDescription
		{
			get { return queryClaim.Lookups.ClaimType.GetDescriptionFromCode(queryClaim.AY_QueryClaimType); }
		}

		public ZString ReasonCode
		{
			get { return queryClaim.AY_QueryClaimReasonCode; }
		}

		public ZString ReasonDescription
		{
			get { return queryClaim.Lookups.ClaimReason.GetDescriptionFromCode(queryClaim.AY_QueryClaimReasonCode); }
		}

		public ZString StatusCode
		{
			get { return queryClaim.AY_QueryClaimStatus; }
		}

		public ZString StatusDescription
		{
			get { return queryClaim.Lookups.ClaimStatus.GetDescriptionFromCode(queryClaim.AY_QueryClaimStatus); }
		}

		public ZString Details
		{
			get { return queryClaim.Details; }
		}

		public ZString AccountType
		{
			get { return queryClaim.Ledger == LedgerTypes.AccountsReceivable ? "DEBTOR" : "CREDITOR"; }
		}

		public ZDateTime NextFollowUp
		{
			get { return queryClaim.AY_QueryClaimNextFollowUp; }
		}

		public ZString Creator
		{
			get
			{
				GlbStaff creator = Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, queryClaim.AY_GS_NKCreator);
				return creator != null ? creator.GS_FullName : ZString.Empty;
			}
		}

		public ZString AssignedTo
		{
			get
			{
				GlbStaff assignedTo = Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, queryClaim.AY_GS_NKStaffAssignedTo);
				return assignedTo != null ? assignedTo.GS_FullName : ZString.Empty;
			}
		}

		public ZString BranchCode
		{
			get { return Factory.Load<GlbBranch>(queryClaim.AY_GB).GB_Code; }
		}

		#endregion
	}
}
