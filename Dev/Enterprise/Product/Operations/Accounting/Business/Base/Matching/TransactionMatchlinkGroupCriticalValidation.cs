using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.CriticalValidation;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Business.Base.Matching
{
	public class TransactionMatchLinkGroupCriticalValidation : CriticalValidation<TransactionMatchLinkGroup>
	{
		public TransactionMatchLinkGroupCriticalValidation(TransactionMatchLinkGroup parent) : base(parent)
		{
		}

		protected override IEnumerable<CriticalValidationResult> OnSavingOnlyCriticalChecks()
		{
			foreach (var result in base.OnSavingOnlyCriticalChecks())
			{
				yield return result;
			}
			yield return CrticalValidateMatchLinkAmountsBalanceNotZero(Parent);
			yield return CrticalValidateTransactionHeaderSameCompany(Parent);
		}

		CriticalValidationResult CrticalValidateMatchLinkAmountsBalanceNotZero(TransactionMatchLinkGroup parent)
		{
			var isNotBanlanced = parent.GetBalance() != 0M;
			if (isNotBanlanced)
			{
				return new CriticalValidationResult(
						CriticalValidationErrorType.TransactionMatchGroupOutOfBalance_2,
						CriticalValidationMessageTemplate.GetTransactionMatchGroupOutOfBalanceErrorMessage(parent[0].AP_MatchGroupNum),
						parent.GetTransactionMatchLinkGroupInfoAboutBalanceIssue()
					);
			}

			return null;
		}

		CriticalValidationResult CrticalValidateTransactionHeaderSameCompany(TransactionMatchLinkGroup parent)
		{
#if DEBUG
			if (Globals.IsTest && Enterprise.Accounting.Integration.Testing.SuspendCriticalValidateTransactionHeaderSameCompanyAttribute.IsActive)
			{
				return null;
			}
#endif

			var matchLinksGroupedByCompanyPk = parent
				.Select(matchLink => matchLink)
				.Where(matchLink => matchLink?.TransactionHeader != null)
				.Where(matchLink => !matchLink.TransactionHeader.AH_GC.IsEmpty)
				.GroupBy(matchLink => matchLink.TransactionHeader.AH_GC)
				.Select(group => (AH_GC: group.Key, matchLinks: group))
				.ToArray();
			var hasDifferenceCompany = matchLinksGroupedByCompanyPk.Length > 1;
			if (hasDifferenceCompany)
			{
				var mainCompanyGroup = matchLinksGroupedByCompanyPk
					.OrderByDescending(matchLinks => matchLinks.matchLinks.Count())
					.FirstOrDefault();
				var companiesFocusing = (
					mainCompany: mainCompanyGroup.AH_GC
					, companiesFocusing: matchLinksGroupedByCompanyPk.Where(group => group != mainCompanyGroup).Select(group => group.AH_GC).ToArray()
					);
				var propInfo = parent.GetTransactionMatchLinkGroupInfoAboutCompanyIssue(
					companiesFocusing: companiesFocusing
				);
				return new CriticalValidationResult(
					CriticalValidationErrorType.TransactionMatchGroupNotAllHeaderInSameCompanies_1,
					CriticalValidationMessageTemplate.TransactionMatchGroupNotAllHeaderInSameCompaniesErrorMessage,
					$"only return different company matchLinks, {propInfo}");
			}
			else
			{
				return null;
			}
		}
	}
}
