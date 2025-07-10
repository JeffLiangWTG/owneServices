using System;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Registry.Business.Accounting;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.AccQueryClaims
{
	public class APAccQueryClaim : AccQueryClaimBase, Enterprise.Integration.Accounting.IAPAccQueryClaim, IDocManagerSupport, IEDocsParsingSupport
	{
		public APAccQueryClaim(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public bool IsAllowMatch => AY_HoldOption == HoldOptionType.Codes.ALM;

		#region Overrides

		public override void OnSaving()
		{
			base.OnSaving();

			if (!IsInDatabase)
			{
				if (AY_HoldOption == DefaultHoldOptionSafe)
				{
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
					Logs.AddNew(Events.EditedARecord, string.Format(CultureInfo.InvariantCulture, "Invoice Hold Option set to {0} based on creditor group.", AY_HoldOption));
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				}
				else
				{
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
					Logs.AddNew(Events.EditedARecord, string.Format(CultureInfo.InvariantCulture, "Invoice Hold Option set to {0}.", AY_HoldOption));
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				}
			}
			else if (AY_HoldOptionInfo.HasChanges)
			{
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				Logs.AddNew(Events.EditedARecord, string.Format(CultureInfo.InvariantCulture, "Invoice Hold Option changed from {0} to {1}.", AY_HoldOptionInfo.OriginalValue, AY_HoldOption));
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			}
		}

		public override bool IsHoldOptionVisible
		{
			get { return true; }
		}

		protected override AccQueryClaimValidation GetNewValidation()
		{
			return new APAccQueryClaimValidation(this);
		}

		public override ZString Ledger
		{
			get { return LedgerTypes.AccountsPayable; }
		}

		protected override ContactType DefaultContactType
		{
			get { return ContactType.Payables; }
		}

		public override bool IsCreatedFromCASS => !string.IsNullOrEmpty(AY_MasterBillNumber) && CASSClaimTypes.Any(x => x == AY_QueryClaimType);

		string[] CASSClaimTypes => new[] {
						QueryClaimTypeCodeList.Codes.QCType5,
						QueryClaimTypeCodeList.Codes.QCType6,
						QueryClaimTypeCodeList.Codes.QCType7
					};
		public override bool CanAddClaimDetails
		{
			get
			{
				return TransactionHeader != null &&
					TransactionHeader.AH_Ledger == LedgerTypes.AccountsPayable &&
					TransactionHeader.AH_TransactionType == ZArchitecture.Core.TransactionTypes.Invoice &&
					!TransactionHeader.AH_TransactionBelongsToGroup.IsValid &&
					!TransactionHeader.AH_IsCancelled;
			}
		}

		public (bool IsAllowed, ZString ReasonWhyNoAllowed) CheckIsRelatedCreditNoteAllowed(bool returnReason = false)
		{
			if (AccountingMasterFilesUtils.ShouldPreventCreateCreditNote(LedgerTypes.AccountsPayable, GlbCompany.CurrentCompany.PK))
			{
				return (false, GetReason(AccountingMasterFilesUtils.APCreditNoteDisallowedMessage));
			}

			return (true, ZString.Empty);

			ZString GetReason(ZString reason) => returnReason ? reason : ZString.Empty;
		}

		public UACreditNote CreateAndAttachRelatedCreditNote()
		{
			UACreditNote result = Factory.New<UACreditNote>();
			result.RelatedClaim = this;
			result.AH_OH = AY_OH_Debtor;
			result.AH_TransactionNum = AY_QueryClaimReference;
			result.AH_TransactionCategory = Constants.TransactionCategory.Codes.ClaimRelated;
			result.AH_Desc = Res.GetString("7d7f2a54-9719-48a1-b9a2-b006546e0e2c", "Claim For Credit");
			result.SubmittedFromInvoicingForm = true;
			if (TransactionHeader != null)
			{
				result.ExchangeRate.Currency = TransactionHeader.AH_RX_NKTransactionCurrency;
			}
			RelatedUnapprovedCreditNote = result;

			return result;
		}

		[List("Lookups.Creditors")]
		public override ZGuid AY_OH_Debtor
		{
			get
			{
				return base.AY_OH_Debtor;
			}
			set
			{
				base.AY_OH_Debtor = value;
				AY_HoldOption = DefaultHoldOptionSafe;
			}
		}

		public override ZString AY_QueryClaimReference
		{
			get
			{
				return base.AY_QueryClaimReference;
			}
			set
			{
				base.AY_QueryClaimReference = value;
				if (RelatedUnapprovedCreditNote != null)
				{
					RelatedUnapprovedCreditNote.AH_TransactionNum = AY_QueryClaimReference;
				}
			}
		}

		public override RefCurrency Currency => AY_MasterBillNumber.IsEmpty ? base.Currency : GlbCompany.CurrentCompany.LocalCurrency;

		protected override AccQueryClaimLookups GetNewLookups()
		{
			return new APAccQueryClaimLookups(this);
		}

		protected override INumberFountainProxy NumberFountainNo
		{
			get { return Env.NumberFountains.APQueryClaimNo; }
		}

		string DefaultHoldOptionSafe => Debtor?.CompanyData?.APCreditorGroup?.OG_DefaultHoldOption ?? AccountingMasterFilesConstants.CreditorGroupConstants.DefaultHoldOption;

		#endregion

		#region IDocManagerSupport Members

		DocManagerInfo IDocManagerSupport.DocManagerInfo
		{
			get
			{
				if (docManagerInfo == null)
				{
					docManagerInfo = new DocManagerInfo(this, Core.Constants.DocManagerCodes.APClaimsAndQueries);
				}
				return docManagerInfo;
			}
		}

		DocManagerInfo docManagerInfo;

		#endregion

		public static APAccQueryClaim[] LoadOpenClaimsWithMAWB(BusinessObjectFactory factory, string mawbNumber) => factory.Load<APAccQueryClaim>(GetOpenClaimQueryForMAWBNumber(mawbNumber));

		public static bool IsThereAnyOpenClaimWithMAWB(BusinessObjectFactory factory, string mawbNumber) => factory.Exists(typeof(APAccQueryClaim), GetOpenClaimQueryForMAWBNumber(mawbNumber));

		static ZQuery GetOpenClaimQueryForMAWBNumber(string mawbNumber)
		{
			var query = new ZQuery(AccQueryClaimSchema.AY_MasterBillNumber, mawbNumber);
			foreach (var status in GetClosedStatuses())
			{
				query.AddToFilter(AccQueryClaimSchema.AY_QueryClaimStatus, SQLComparisonOperator.NotEqual, status);
			}
			return query;
		}

		#region IEDocsParsingSupport Members

		string IEDocsParsingSupport.UtilityData => throw new NotImplementedException();

		bool IEDocsParsingSupport.DenySendForParsing(Guid docPK, string docType, string fileName)
		{
			return true;
		}

		#endregion
	}
}
