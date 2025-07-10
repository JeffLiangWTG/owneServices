using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.TaxFramework.Business
{
	public interface ITaxRecordCollectionValidator
	{
		IReadOnlyCollection<AccTaxTransaction> GetDuplicatedTaxRecordsForSinglePostingTaxSystem(IReadOnlyCollection<AccTaxTransaction> taxRecordsForTheSameParent);
	}

	public class TaxRecordCollectionValidator : ITaxRecordCollectionValidator
	{
		public TaxRecordCollectionValidator(ITaxFrameworkConfigurationHelper taxFrameworkConfigurationHelper)
		{
			TaxFrameworkConfigurationHelper = Argument.NotNull(taxFrameworkConfigurationHelper, nameof(taxFrameworkConfigurationHelper));
		}
		readonly ITaxFrameworkConfigurationHelper TaxFrameworkConfigurationHelper;

		IReadOnlyCollection<AccTaxTransaction> ITaxRecordCollectionValidator.GetDuplicatedTaxRecordsForSinglePostingTaxSystem(IReadOnlyCollection<AccTaxTransaction> taxRecordsForTheSameParent)
		{
			var duplicatedTransactions = new List<AccTaxTransaction>();
			if (taxRecordsForTheSameParent.Any())
			{
				var countryCode = taxRecordsForTheSameParent.First().Company.GC_RN_NKCountryCode;

				foreach (var group in taxRecordsForTheSameParent.GroupBy(x => x.ATT_TaxSystemCode))
				{
					if (group.Count() > 1 && (TaxFrameworkConfigurationHelper.GetTaxSystem(group.Key, taxRecordsForTheSameParent.First().Factory)?.IsSingleRecordPerARTransactionRequired(countryCode) ?? false))
					{
						duplicatedTransactions.AddRange(group);
					}
				}
			}

			return duplicatedTransactions;
		}
	}
}
