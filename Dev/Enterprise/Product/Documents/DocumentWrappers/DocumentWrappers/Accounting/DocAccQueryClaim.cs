using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.AccQueryClaims;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Res = DocumentWrappers.Res;

namespace Enterprise.DocumentWrappers
{
	public class DocAccQueryClaim : DocBaseWrapper
	{
		protected DocAccQueryClaim(AccQueryClaimBase claim, BusinessObjectFactory factory)
			: base(claim, factory)
		{
		}

		public static DocAccQueryClaim New(AccQueryClaimBase claim, BusinessObjectFactory factory)
		{
			if (claim == null)
			{
				return null;
			}
			return new DocAccQueryClaim(claim, factory);
		}

		public ZString DocumentTitle
		{
			get { return Res.GetString("5d8eaefc-6456-4f58-8aa3-1bd0e7f9fa7c", "{0} CLAIM LOG", Claim.Ledger); }
		}

		public ZString AccountType
		{
			get { return Claim.Ledger == LedgerTypes.AccountsReceivable ? Res.GetString("f461d6e8-0852-4b4f-8781-a25db3575edd", "DEBTOR") : Res.GetString("eb104cab-aae8-488e-a4a0-eaeeb071ba71", "CREDITOR"); }
		}

		public ZString Reference
		{
			get { return Claim.AY_QueryClaimReference; }
		}

		public ZString InvoiceNo
		{
			get { return Claim.TransactionHeader == null ? ZString.Empty : Claim.TransactionHeader.AH_TransactionNum; }
		}

		public ZString Amount
		{
			get { return TransactionCurrency == null ? (ZString)Claim.AY_QueryClaimAmount.ToString(2) : TransactionCurrency.FormatMoney(Claim.AY_QueryClaimAmount); }
		}

		public ZString TypeCode
		{
			get { return Claim.AY_QueryClaimType; }
		}

		public ZString TypeDescription
		{
			get { return Claim.Lookups.ClaimType.GetDescriptionFromCode(Claim.AY_QueryClaimType); }
		}

		public ZString ReasonCode
		{
			get { return Claim.AY_QueryClaimReasonCode; }
		}

		public ZString ReasonDescription
		{
			get { return Claim.Lookups.ClaimReason.GetDescriptionFromCode(Claim.AY_QueryClaimReasonCode); }
		}

		public ZString StatusCode
		{
			get { return Claim.AY_QueryClaimStatus; }
		}

		public ZString StatusDescription
		{
			get { return Claim.Lookups.ClaimStatus.GetDescriptionFromCode(Claim.AY_QueryClaimStatus); }
		}

		public ZString ShortDescription
		{
			get { return Claim.AY_ShortDescriptionOfClaim; }
		}

		public ZString Details
		{
			get { return Claim.Details; }
		}

		public ZDateTime NextFollowUp
		{
			get { return Claim.AY_QueryClaimNextFollowUp; }
		}

		public DocOrganisation AccountOrg
		{
			get { return DocOrganisation.New(Claim.Debtor, Factory); }
		}

		public DocContacts Contact
		{
			get { return DocContacts.New(Claim.Contact, Factory); }
		}

		public DocStaff Creator
		{
			get { return DocStaff.New(Claim.AY_GS_NKCreator, Factory); }
		}

		public DocStaff AssignedTo
		{
			get { return DocStaff.New(Claim.AY_GS_NKStaffAssignedTo, Factory); }
		}

		public DocBranch Branch
		{
			get { return DocBranch.New(Claim.AY_GB, Factory); }
		}

		#region Implementation

		AccQueryClaim Claim
		{
			get { return (AccQueryClaim)WrappedObject; }
		}

		DocCurrency TransactionCurrency
		{
			get { return DocCurrency.New(Claim.AY_RX_TransactionCurrencyCode, Factory); }
		}

		#endregion
	}
}
