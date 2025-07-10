using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Integration.Compliance;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public class TransactionCreationRestrictionHelper : ITransactionCreationRestrictionHelper
	{
		public bool AllowToCreatePaymentApproval(AccPaymentApproval paymentApproval, out ResourceString errorMessage)
		{
			errorMessage = null;

			var result = true;
			var ledger = paymentApproval.AV_Ledger;
			var org = paymentApproval.Header;
			var policy = GetRestrictionPolicy(ledger, org, paymentApproval.AV_GC);

			switch (policy)
			{
				case Core.Constants.TransactionCreationRestriction.All:
					result = false;
					break;
				case Core.Constants.TransactionCreationRestriction.OutstandingBalance:
					result = ledger == LedgerTypes.AccountsPayable;
					break;
				case Core.Constants.TransactionCreationRestriction.Invoice:
				case Core.Constants.TransactionCreationRestriction.None:
				default:
					break;
			}

			if (!result)
			{
				errorMessage = GetErrorMessage(ZString.Empty, org?.OH_Code, ledger, policy);
			}

			return result;
		}

		public bool AllowToCreateTransaction(AccTransactionHeader transaction, out ResourceString errorMessage)
		{
			var result = true;
			errorMessage = null;

			var ledger = transaction.AH_Ledger;
			var transactionType = transaction.AH_TransactionType;
			var org = transaction.Header;
			bool isReverse = transaction.AH_IsCancelled;
			var policy = GetRestrictionPolicy(ledger, org, transaction.AH_GC);

			if (transaction.IsInDatabase && !IsNotCompleteTransaction(transaction))
			{
				if (policy == Core.Constants.TransactionCreationRestriction.OutstandingBalance && transaction.AH_OutstandingAmountInfo.HasChanges &&
						(
							(ledger == LedgerTypes.AccountsReceivable &&
								((ZDecimal)transaction.AH_OutstandingAmountInfo.Value - (ZDecimal)transaction.AH_OutstandingAmountInfo.OriginalValue > 0)) ||
							(ledger == LedgerTypes.AccountsPayable &&
								((ZDecimal)transaction.AH_OutstandingAmountInfo.Value - (ZDecimal)transaction.AH_OutstandingAmountInfo.OriginalValue < 0))
						))
				{
					result = false;
				}
			}
			else
			{
				switch (policy)
				{
					case Enterprise.Core.Constants.TransactionCreationRestriction.Invoice:
						result = isReverse || !InvoiceTypes.Contains<string>(transactionType);
						break;
					case Enterprise.Core.Constants.TransactionCreationRestriction.All:
						result = isReverse;
						break;
					case Enterprise.Core.Constants.TransactionCreationRestriction.OutstandingBalance:
						result = !(
									(ledger == LedgerTypes.AccountsPayable && transaction.AH_OutstandingAmount < 0) ||
									(ledger == LedgerTypes.AccountsReceivable && transaction.AH_OutstandingAmount > 0)
								 );
						break;
					case Enterprise.Core.Constants.TransactionCreationRestriction.None:
					default:
						break;
				}
			}

			if (!result)
			{
				errorMessage = GetErrorMessage(transaction.AH_TransactionNum, org?.OH_Code, transaction.AH_Ledger, policy);
			}

			return result;
		}

		public bool CheckOrgHeaderAllowsPosting(AccTransactionHeader transaction, out ResourceString errorMessage, out bool isErrorMessage)
		{
			errorMessage = null;
			isErrorMessage = false;
			var complianceInfo = ObjectFactory.Get<ICountryComplianceFactoryIntegration>()?.GetICountryComplianceInfo(transaction.Company.Country.Code);
			if (complianceInfo is IOrgHeaderPostingValidation)
			{
				(errorMessage, isErrorMessage) = ((IOrgHeaderPostingValidation)complianceInfo).CheckOrgHeaderForPosting(transaction);
				if (!string.IsNullOrEmpty(errorMessage))
				{
					return false;
				}
			}
			return true;
		}

		ResourceString GetErrorMessage(ZString transactionNum, ZString? orgHeaderCode, ZString ledger, ZString policy)
		{
			return ResString.GetMultilingualString("D126441D-1F93-4dd2-AF93-68A4376A7D92", @"You cannot create transaction '{0}' because organization '{1}' has an {2} transaction creation restriction policy set to {3}.
Transaction creation restriction policy is configured under Organization > A/R or A/P > Configuration > Company Data.", transactionNum, orgHeaderCode, ledger, policy);
		}

		bool IsNotCompleteTransaction(AccTransactionHeader transaction) =>
			transaction.AH_LedgerInfo.HasChanges && (transaction.AH_Ledger == LedgerTypes.AccountsPayable || transaction.AH_Ledger == LedgerTypes.AccountsReceivable);

		string GetRestrictionPolicy(string ledger, OrgHeader org, ZGuid companyPK)
		{
			var result = Enterprise.Core.Constants.TransactionCreationRestriction.None;

			var companyData = org == null ? null : OrgCompanyData.Load(org.Factory, org.PK, companyPK);
			if (companyData != null)
			{
				switch (ledger)
				{
					case LedgerTypes.AccountsPayable:
						result = companyData.OB_APTransactionCreationRestriction;
						break;
					case LedgerTypes.AccountsReceivable:
						result = companyData.OB_ARTransactionCreationRestriction;
						break;
					default:
						break;
				}
			}

			return result;
		}

		IEnumerable<string> InvoiceTypes => invoiceTypes ?? (invoiceTypes = new List<string> { TransactionTypes.Invoice, TransactionTypes.CreditNote, TransactionTypes.AdjustmentNote });
		IEnumerable<string> invoiceTypes;

		public static TransactionCreationRestrictionHelper Instance
		{
			get
			{
				if (instance == null)
				{
					instance = new TransactionCreationRestrictionHelper();
				}
				return instance;
			}
		}
		[ThreadStatic]
		static TransactionCreationRestrictionHelper instance;
	}
}
