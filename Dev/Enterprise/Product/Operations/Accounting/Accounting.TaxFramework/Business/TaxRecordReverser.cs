using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.TaxFramework.Business
{
	public interface ITaxRecordReverser
	{
		void ReverseNotRealisedSPRAPRecords(ITaxRecordParent taxParent);
		void ReverseRealisedSPRAPTaxRecords(BusinessObjectFactory factory, ZGuid matchTransactionPK, ZGuid reversedTaxMatchTransactionPK, ZDate reversedTaxPostAndRealisationDate);
		void ReverseNonSPRAPTaxRecords(ITaxRecordParent originalTaxParent, ITaxRecordParent reversedTaxParent, ZDate reversedTaxPostAndRealisationDate);
		void UpdatePostDateOnNonSPRAPTaxRecords(ITaxRecordParent originalTaxParent, ITaxRecordParent reversedTaxParent, ZDate reversedTaxPostAndRealisationDate);
	}

	class TaxRecordReverser : ITaxRecordReverser
	{
		public TaxRecordReverser()
		{
			taxRecordLoader_constructorInitializedOnly = new TaxRecordLoader();
			taxRecordCreator_constructorInitializedOnly = new TaxRecordCreator();
		}

		void ITaxRecordReverser.ReverseNotRealisedSPRAPRecords(ITaxRecordParent taxParent)
		{
			var taxRecordsToCancel = TaxRecordLoader.LoadSPRAPTaxRecords(taxParent);
			taxRecordsToCancel.ForEach(x => x.ATT_IsCancelled = true);
		}

		void ITaxRecordReverser.ReverseRealisedSPRAPTaxRecords(BusinessObjectFactory factory, ZGuid matchTransactionPK, ZGuid reversedTaxMatchTransactionPK, ZDate reversedTaxPostAndRealisationDate)
		{
			var taxRecords = TaxRecordLoader.LoadSPRAPTaxRecordsLinkedToMatchTransaction(factory, matchTransactionPK);
			foreach (var taxRecord in taxRecords)
			{
				var taxRecordCopyWithPivot = TaxRecordCreator.CreateCopiesOfTaxRecords(null, taxRecord).ElementAt(0);
				var taxRecordCopy = taxRecordCopyWithPivot.TaxRecord;
				taxRecordCopy.ATT_AH = taxRecord.ATT_AH;
				using (factory.SetTempContext(BusinessContext.ReversingTaxAmounts))
				{
					taxRecordCopy.ATT_OSTaxBaseAmount *= -1;
					taxRecordCopy.ATT_OSTaxAmount *= -1;
					taxRecordCopy.ATT_LocalTaxBaseAmount *= -1;
					taxRecordCopy.ATT_LocalTaxAmount *= -1;
				}
				var pivots = taxRecordCopyWithPivot.Pivots;
				foreach (var p in pivots)
				{
					p.ATP_LocalTaxAmount *= -1;
				}
				taxRecordCopy.ATT_AH_MatchTransaction = reversedTaxMatchTransactionPK;
				taxRecordCopy.ATT_RealisationDate = reversedTaxPostAndRealisationDate;
				taxRecordCopy.ATT_PostDate = reversedTaxPostAndRealisationDate;
				taxRecordCopy.ATT_IsCancelled = true;
				taxRecord.ATT_IsCancelled = true;
			}
		}

		void ITaxRecordReverser.ReverseNonSPRAPTaxRecords(ITaxRecordParent originalTaxParent, ITaxRecordParent reversedTaxParent, ZDate reversedTaxPostAndRealisationDate)
		{
			using (new DisposableAction(() => reversedTaxParent.Factory.SuspendValidation(), () => reversedTaxParent.Factory.ResumeValidation()))
			{
				var taxRecords = TaxRecordLoader.LoadNonSPRAPTaxRecords(originalTaxParent);

				if (taxRecords.Length != 0)
				{
					var reversalLinePKByOriginalLinePK = reversedTaxParent.GetLines().ToDictionary(x => x.CopiedFromPK, x => x.PK);
					var taxRecordsCopiesWithPivots = TaxRecordCreator.CreateCopiesOfTaxRecords(reversalLinePKByOriginalLinePK, taxRecords);
					foreach (var taxRecordCopyWithPivot in taxRecordsCopiesWithPivots)
					{
						var taxRecordCopy = taxRecordCopyWithPivot.TaxRecord;
						taxRecordCopy.ATT_AH = reversedTaxParent.PK;
						using (reversedTaxParent.Factory.SetTempContext(BusinessContext.ReversingTaxAmounts))
						{
							taxRecordCopy.ATT_OSTaxBaseAmount *= -1;
							taxRecordCopy.ATT_OSTaxAmount *= -1;
							taxRecordCopy.ATT_LocalTaxBaseAmount *= -1;
							taxRecordCopy.ATT_LocalTaxAmount *= -1;
						}
						var pivots = taxRecordCopyWithPivot.Pivots;
						foreach (var p in pivots)
						{
							p.ATP_LocalTaxAmount *= -1;
						}
						UpdatePostAndRealisationDateForReversalTaxRecord(taxRecordCopy, reversedTaxPostAndRealisationDate);
						taxRecordCopy.ATT_IsCancelled = true;
					}

					foreach (var taxRecord in taxRecords)
					{
						taxRecord.ATT_IsCancelled = true;
						UpdateRealisationDateForOriginalTaxRecord(taxRecord, reversedTaxPostAndRealisationDate);
					}
				}
			}
		}

		void ITaxRecordReverser.UpdatePostDateOnNonSPRAPTaxRecords(ITaxRecordParent originalTaxParent, ITaxRecordParent reversedTaxParent, ZDate reversedTaxPostAndRealisationDate)
		{
			var reversedTaxRecords = TaxRecordLoader.LoadNonSPRAPTaxRecords(reversedTaxParent);

			if (reversedTaxRecords.Length != 0)
			{
				foreach (var reversedTaxRecord in reversedTaxRecords)
				{
					UpdatePostAndRealisationDateForReversalTaxRecord(reversedTaxRecord, reversedTaxPostAndRealisationDate);
				}

				var taxRecords = TaxRecordLoader.LoadNonSPRAPTaxRecords(originalTaxParent);
				foreach (var taxRecord in taxRecords)
				{
					UpdateRealisationDateForOriginalTaxRecord(taxRecord, reversedTaxPostAndRealisationDate);
				}
			}
		}

		void UpdatePostAndRealisationDateForReversalTaxRecord(AccTaxTransaction taxRecord, ZDate postAndRealisationDate)
		{
			taxRecord.ATT_PostDate = postAndRealisationDate;
			taxRecord.ATT_RealisationDate = postAndRealisationDate;
		}

		void UpdateRealisationDateForOriginalTaxRecord(AccTaxTransaction taxRecord, ZDate realisationDate)
		{
			if (taxRecord.ATT_RealisationDateInfo.OriginalValue.IsEmpty && (taxRecord.ATT_RealisationDate != realisationDate))
			{
				taxRecord.ATT_RealisationDate = realisationDate;
			}
		}

		ITaxRecordLoader TaxRecordLoader => taxRecordLoader_constructorInitializedOnly;
		ITaxRecordLoader taxRecordLoader_constructorInitializedOnly;
		ITaxRecordCreator TaxRecordCreator => taxRecordCreator_constructorInitializedOnly;
		ITaxRecordCreator taxRecordCreator_constructorInitializedOnly;

#if DEBUG
		public void SubstituteTaxRecordLoader_ForTestOnly(ITaxRecordLoader replacement) => taxRecordLoader_constructorInitializedOnly = replacement;
		public ITaxRecordLoader TaxRecordLoader_ExposedForTestOnly => TaxRecordLoader;
		public void SubstituteTaxRecordCreator_ForTestOnly(ITaxRecordCreator replacement) => taxRecordCreator_constructorInitializedOnly = replacement;
		public ITaxRecordCreator TaxRecordCreator_ExposedForTestOnly => TaxRecordCreator;
#endif
	}
}
