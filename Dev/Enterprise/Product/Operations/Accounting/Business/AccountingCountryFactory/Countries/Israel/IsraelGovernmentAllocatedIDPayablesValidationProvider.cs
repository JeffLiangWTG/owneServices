using System.Text.RegularExpressions;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.AccountingCountryFactory.Israel
{
	class IsraelGovernmentAllocatedIDPayablesValidationProvider : IGovernmentAllocatedIDValidationProvider
	{
		public string ValidateGovernmentAllocatedID(IGovernmentAllocatedIDValidationData validationData)
		{
			if (!IsLedgerTypeEligible(validationData.Ledger))
			{
				return null;
			}

			if (validationData.OrgCountryCode != CountryCodes.Israel)
			{
				if (!string.IsNullOrEmpty(validationData.GovernmentAllocatedID))
				{
					return Res.GetString("2aadd47a-f306-4146-b7cf-3a2cd5e650f3",
						"The transaction is not suitable for entering Govt. ID. The country of the creditor's address must be IL.");
				}
			}
			else
			{
				if (!validationData.EReportingStatus.IsNullOrEmpty() && validationData.EReportingStatus != "FAL")
				{
					return Res.GetString("7fada63c-c7a0-4a88-828e-d1fa0ff0cf97",
							"The Government Allocated Number can only be overridden for transactions with e-Reporting Status of FAL.");
				}

				if (!MatchValidationPattern(validationData.GovernmentAllocatedID))
				{
					return Res.GetString("57f59cc1-f536-46c8-ad4e-7d7da2218178",
							"Govt. ID must be between 9 and 30 characters long and only letters and number. The last 9 right digits must be numeric.");
				}
			}

			return null;
		}

		bool MatchValidationPattern(ZString governmentAllocatedID)
		{
			var validationPattern = (NoResString)@"^$|^[a-zA-Z0-9]{0,21}\d{9}$"; // Empty or Between 9 and 30 characters long and only letters and numbers. The last 9 digits must be numeric.
			var match = Regex.Match(governmentAllocatedID, validationPattern);

			return match.Success;
		}

		bool IsLedgerTypeEligible(string ledger)
			=> ledger == LedgerTypes.AccountsPayable
			|| ledger == LedgerTypes.IncompleteTransactions
			|| ledger == LedgerTypes.TransactionsPendingAllocation;
	}
}
