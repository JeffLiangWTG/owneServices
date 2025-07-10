using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesTaxFrameworkConstants;

namespace Enterprise.Accounting.TaxFramework.Business
{
	public interface ITaxRecordRealiser
	{
		void RealiseTaxRecord(ITaxRecordParent taxParent, ZDate realisationDate);
		string RealisePaymentRetentionTaxRecords(ITaxRecordParent taxParent, IEnumerable<IMatchTransactionDetails> matchTransactionDetails);
	}

	public class TaxRecordRealiser : ITaxRecordRealiser
	{
		public TaxRecordRealiser()
		{
			taxRecordLoader_constructorInitializedOnly = new TaxRecordLoader();
		}

		void ITaxRecordRealiser.RealiseTaxRecord(ITaxRecordParent taxParent, ZDate realisationDate)
		{
			var taxRecords = TaxRecordLoader.LoadMatchingBasisTaxRecords(taxParent);
			foreach (var taxRecord in taxRecords)
			{
				taxRecord.ATT_RealisationDate = realisationDate;
			}
		}

		string ITaxRecordRealiser.RealisePaymentRetentionTaxRecords(ITaxRecordParent taxParent, IEnumerable<IMatchTransactionDetails> matchTransactionDetails)
		{
			var notionalSPRTaxRecordsToProcess = TaxRecordLoader.LoadSPRAPTaxRecords(taxParent, getNotionalRecordsOnly: true, useLocalCacheOnly: true);
			var notionalSPRTaxRecordPKsFromCache = notionalSPRTaxRecordsToProcess.Select(x => x.PK).ToHashSet();

			var matchTransactionDetailsList = matchTransactionDetails.ToList();
			var taxRecordPKsFromMatchDetails = new HashSet<ZGuid>();
			matchTransactionDetailsList.ForEach(x => taxRecordPKsFromMatchDetails.UnionWith(x.GetTaxRecordPKs()));

			if (!taxRecordPKsFromMatchDetails.IsSubsetOf(notionalSPRTaxRecordPKsFromCache))
			{
				return Res.GetString("F63394DE-5E62-496B-BC3B-87B808C6C77C", "Some of the Tax Records have been already realized by another user. Please cancel this operation and try again.");
			}

			var taxRecordsDictionary = notionalSPRTaxRecordsToProcess.ToDictionary(x => x.PK);
			foreach (var matchTransaction in matchTransactionDetailsList)
			{
				foreach (var taxRecordPK in matchTransaction.GetTaxRecordPKs())
				{
					var taxRecord = taxRecordsDictionary[taxRecordPK];
					if (taxRecord.ATT_Basis == TaxBasisList.PostingOnMatching.Code)
					{
						taxRecord.ATT_AH_MatchTransaction = matchTransaction.PK;
						taxRecord.ATT_RealisationDate = matchTransaction.RealisationDate;
					}
					else
					{
						var errorMessage = ResString.GetMultilingualString("2E07C9EF-164B-4F1D-BE6B-330A20990776", "SPR tax record has invalid tax basis '{0}'. Only '{1}' tax basis is allowed for SPR tax", taxRecord.ATT_Basis, TaxBasisList.PostingOnMatching.Code);
						throw new TaxFrameworkUnknownConfigurationValueException(errorMessage);
					}
				}
			}

			return string.Empty;
		}

		ITaxRecordLoader TaxRecordLoader => taxRecordLoader_constructorInitializedOnly;
		ITaxRecordLoader taxRecordLoader_constructorInitializedOnly;

#if DEBUG
		public void SubstituteTaxRecordLoader_ForTestOnly(ITaxRecordLoader replacement) => taxRecordLoader_constructorInitializedOnly = replacement;
		public ITaxRecordLoader TaxRecordLoader_ExposedForTestOnly => TaxRecordLoader;
#endif
	}
}
