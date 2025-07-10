using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesTaxFrameworkConstants;

namespace Enterprise.Accounting.TaxFramework.Business
{
	public interface ITaxRecordCreator
	{
		void CreateTaxRecords(ITaxRecordParent taxParent);
		void DeleteTaxRecordsNotInDB(ITaxRecordParent taxParent);
		void DeleteTaxRecordNotInDB(ITaxRecordParent taxParent, AccTaxTransaction taxRecordToDelete);
		IReadOnlyCollection<(AccTaxTransaction TaxRecord, List<AccTaxRecordTransactionLinePivot> Pivots)> CreateCopiesOfTaxRecords(IReadOnlyDictionary<ZGuid, ZGuid> reversalLinePKByOriginalLinePK, params AccTaxTransaction[] taxRecords);
		void UpdatePostDate(ITaxRecordParent taxParent);
		IReadOnlyCollection<(AccTaxTransaction taxRecord, AccTaxRecordTransactionLinePivot linePivot)> GetEstimatedTaxRecords(ITaxRecordParentBase taxParent, IEnumerable<ZString> taxSystemCodesForFilter);
	}

	class TaxRecordCreator : ITaxRecordCreator
	{
		public TaxRecordCreator()
		{
			ITaxFrameworkConfigurationHelper configurationHelper = new TaxFrameworkConfigurationHelper();

			taxRecoveryLineCreator_constructorInitializedOnly = new TaxRecoveryLineCreator();
			thresholdAmountProcessor_constructorInitializedOnly = new ThresholdAmountProcessor();
			taxRecordCalculator_constructorInitializedOnly = new TaxRecordCalculator(configurationHelper);
			taxRecordLoader_constructorInitializedOnly = new TaxRecordLoader();
			taxRecordPivotProcessor_constructorInitializedOnly = new TaxRecordPivotProcessor();
			taxRecordCollectionValidator_constructorInitializedOnly = new TaxRecordCollectionValidator(configurationHelper);
		}

		#region ITaxRecordCreator

		void ITaxRecordCreator.DeleteTaxRecordNotInDB(ITaxRecordParent taxParent, AccTaxTransaction taxRecordToDelete) => DeleteTaxRecordNotInDB(taxParent, taxRecordToDelete);

		void ITaxRecordCreator.UpdatePostDate(ITaxRecordParent taxParent)
		{
			var taxRecords = TaxRecordLoader.LoadAllTaxRecords(taxParent);
			taxRecords.ForEach(x => TaxRecordCalculator.SetDatesFromPostDateIfApplicable(x, taxParent));
		}

		IReadOnlyCollection<(AccTaxTransaction TaxRecord, List<AccTaxRecordTransactionLinePivot> Pivots)> ITaxRecordCreator.CreateCopiesOfTaxRecords(IReadOnlyDictionary<ZGuid, ZGuid> reversalLinePKByOriginalLinePK, params AccTaxTransaction[] taxRecords)
		{
			var columnNamesToExcludeWhileCopying = new[]
			{
				AccTaxTransactionSchema.ATT_AH.Name,
				AccTaxTransactionSchema.ATT_AH_MatchTransaction.Name,
				AccTaxTransactionSchema.ATT_RealisationDate.Name,
				AccTaxTransactionSchema.ATT_PostDate.Name,
				AccTaxTransactionSchema.ATT_SystemCreateTimeUtc.Name,
				AccTaxTransactionSchema.ATT_SystemCreateUser.Name,
				AccTaxTransactionSchema.ATT_SystemLastEditTimeUtc.Name,
				AccTaxTransactionSchema.ATT_SystemLastEditUser.Name,
			};
			BusinessObjectFactory factory = taxRecords.FirstOrDefault()?.Factory ;
			var copiesOfTaxRecords = new List<(AccTaxTransaction, List<AccTaxRecordTransactionLinePivot>)>(taxRecords.Length);
			foreach (var taxRecord in taxRecords)
			{
				var copyOfTaxRecord = factory.New<AccTaxTransaction>();

				try
				{
					factory.SetContext(BusinessContext.CopyingPersistentValues);
					copyOfTaxRecord.CopyPersistentValuesFrom(taxRecord, new BusinessObjectCloneArgs(columnNamesToExcludeWhileCopying));
				}
				finally
				{
					factory.RemoveContext(BusinessContext.CopyingPersistentValues);
				}
				var pivots = factory.Load<AccTaxRecordTransactionLinePivot>(new ZQuery(AccTaxRecordTransactionLinePivotSchema.ATP_ATT, taxRecord.PK));
				var copyOfPivots = new List<AccTaxRecordTransactionLinePivot>();
				foreach (var pivot in pivots)
				{
					var copyOfPivot = factory.New<AccTaxRecordTransactionLinePivot>();
					using (copyOfPivot.ReportLinkingLineSuspender.GetSuspender())
					{
						copyOfPivot.CopyPersistentValuesFrom(pivot);
						copyOfPivot.ATP_ATT = copyOfTaxRecord.PK;
						if (reversalLinePKByOriginalLinePK != null)
						{
							copyOfPivot.ATP_AL_TransactionLine = reversalLinePKByOriginalLinePK[copyOfPivot.ATP_AL_TransactionLine];
						}
					}
					copyOfPivots.Add(copyOfPivot);
				}

				copiesOfTaxRecords.Add((copyOfTaxRecord, copyOfPivots));
			}

			return copiesOfTaxRecords;
		}

		void ITaxRecordCreator.CreateTaxRecords(ITaxRecordParent taxParent)
		{
			taxParent.SetTransactionHeaderBranch();

			var allTaxRecords = new List<AccTaxTransaction>();
			try
			{
				taxParent.Factory.SetContext(BusinessContext.SystemGeneratedTaxRecordCreationInProgress);
				TaxRecordCalculator.CalculateTaxRecords(taxParent, allTaxRecords);
				if (allTaxRecords.Any())
				{
					TaxRecoveryLineCreator.CreateLines(TaxRecordPivotProcessor, taxParent, allTaxRecords.ToArray());

					ThresholdAmountProcessor.Process(TaxRecordPivotProcessor, taxParent, allTaxRecords, GetThresholdMethodCodes());

					UpdateParentTotals(taxParent, allTaxRecords);

					SetSystemCalculatedValuesOnTaxRecords(allTaxRecords);

					ValidateTaxRecords(allTaxRecords);
				}
			}
			finally
			{
				taxParent.Factory.RemoveContext(BusinessContext.SystemGeneratedTaxRecordCreationInProgress);
			}
		}

		IReadOnlyCollection<(AccTaxTransaction taxRecord, AccTaxRecordTransactionLinePivot linePivot)> ITaxRecordCreator.GetEstimatedTaxRecords(ITaxRecordParentBase taxParent, IEnumerable<ZString> taxSystemCodesForFilter)
		{
			var applicableTaxRecords = new List<(AccTaxTransaction, AccTaxRecordTransactionLinePivot)>();

			var taxRecordsWithLinePivots = TaxRecordCalculator.GetTaxRecordsWithLinePivots(taxParent, taxSystemCodesForFilter);
			if (taxRecordsWithLinePivots.Any())
			{
				var taxRecordsOutsideOfThreshold = ThresholdAmountProcessor.GetTaxRecordsOutsideOfThreshold(taxParent, taxRecordsWithLinePivots.Select(x => x.taxRecord).ToList(), GetThresholdMethodCodes().ToHashSet());

				foreach (var taxRecordWithLinePivot in taxRecordsWithLinePivots)
				{
					if (!taxRecordsOutsideOfThreshold.Contains(taxRecordWithLinePivot.taxRecord))
					{
						applicableTaxRecords.Add((taxRecordWithLinePivot.taxRecord, taxRecordWithLinePivot.linePivot));
					}
				}
			}

			return applicableTaxRecords.ToHashSet();
		}

		void ITaxRecordCreator.DeleteTaxRecordsNotInDB(ITaxRecordParent taxParent)
		{
			if (taxParent.IsPosted)
			{
				throw new InvalidOperationException("Tax records for posted transaction cannot be deleted.");
			}

			var taxRecords = TaxRecordLoader.LoadAllTaxRecords(taxParent);
			TaxRecordPivotProcessor.DeleteTaxRecordsWithPivots(taxRecords);

			TaxRecoveryLineCreator.DeleteLines(taxParent);

			UpdateParentTotals(taxParent, taxRecords.Where(x => !x.IsDeleted).ToHashSet());
		}

		#endregion

		#region Internals

		void SetSystemCalculatedValuesOnTaxRecords(IReadOnlyCollection<AccTaxTransaction> allTaxRecords)
		{
			allTaxRecords.ForEach(x => x.SetTaxTransactionsSystemCalculatedValues(x.ATT_OSTaxBaseAmount, x.ATT_OSTaxAmount, x.ATT_RateNumerator, x.ATT_RateDenominator, x.ATT_TaxDate, x.ATT_TaxAuthorityServiceCode, x.ATT_TaxAuthorityServiceCodeDescription));
		}

		#region Validation
		void ValidateTaxRecords(IReadOnlyCollection<AccTaxTransaction> allTaxRecords)
		{
			var hasErrors = ValidateDuplicatedTaxRecordsForSinglePostingTaxSystem(allTaxRecords);

			if (!hasErrors)
			{
				allTaxRecords.ForEach(x => x.RunPreSaveValidation());
			}
		}

		bool ValidateDuplicatedTaxRecordsForSinglePostingTaxSystem(IReadOnlyCollection<AccTaxTransaction> allTaxRecords)
		{
			var result = false;
			var duplicatedTaxRecords = TaxRecordCollectionValidator.GetDuplicatedTaxRecordsForSinglePostingTaxSystem(allTaxRecords);
			foreach (var taxRecord in duplicatedTaxRecords)
			{
				taxRecord.AddRowError(Res.GetString("0A3791EB-FB90-4900-A835-9952443224CD", "It is not allowed to have more than one Tax record for this Tax System per transaction."));
				result = true;
			}

			return result;
		}

		#endregion

		void DeleteTaxRecordNotInDB(ITaxRecordParent taxParent, AccTaxTransaction taxRecordToDelete)
		{
			if (taxRecordToDelete.IsInDatabase)
			{
				return;
			}

			var taxRecords = taxParent.Factory.Load<AccTaxTransaction>(new ZQuery(AccTaxTransactionSchema.ATT_AH, taxParent.PK));
			TaxRecordPivotProcessor.DeleteTaxRecordsWithPivots(taxRecordToDelete);

			UpdateParentTotals(taxParent, taxRecords.Where(x => !x.IsDeleted).ToHashSet());
		}

		static void UpdateParentTotals(ITaxRecordParent taxParent, IReadOnlyCollection<AccTaxTransaction> taxRecords)
		{
			ZDecimal osTaxAmount = 0m;
			ZDecimal localTaxAmount = 0m;
			foreach (var taxRecord in taxRecords)
			{
				if (taxRecord.ATT_AffectsSourceTransactionTotal)
				{
					osTaxAmount += taxRecord.ATT_OSTaxAmount;
					localTaxAmount += taxRecord.ATT_LocalTaxAmount;
				}
			}

			taxParent.OSTaxAmount = osTaxAmount;
			taxParent.LocalTaxAmount = localTaxAmount;
		}

		static HashSet<string> GetThresholdMethodCodes() => new ETC_ThresholdMethods().GetAllCodes().Where(x => x != ETC_ThresholdMethods.NoThreshold.Code).ToHashSet();

		#region Dependencies

		ITaxRecoveryLineCreator TaxRecoveryLineCreator => taxRecoveryLineCreator_constructorInitializedOnly;
		ITaxRecoveryLineCreator taxRecoveryLineCreator_constructorInitializedOnly;

		IThresholdProcessor ThresholdAmountProcessor => thresholdAmountProcessor_constructorInitializedOnly;
		IThresholdProcessor thresholdAmountProcessor_constructorInitializedOnly;

		ITaxRecordCalculator TaxRecordCalculator => taxRecordCalculator_constructorInitializedOnly;
		ITaxRecordCalculator taxRecordCalculator_constructorInitializedOnly;

		ITaxRecordLoader TaxRecordLoader => taxRecordLoader_constructorInitializedOnly;
		ITaxRecordLoader taxRecordLoader_constructorInitializedOnly;

		ITaxRecordPivotProcessor TaxRecordPivotProcessor => taxRecordPivotProcessor_constructorInitializedOnly;
		ITaxRecordPivotProcessor taxRecordPivotProcessor_constructorInitializedOnly;

		ITaxRecordCollectionValidator TaxRecordCollectionValidator => taxRecordCollectionValidator_constructorInitializedOnly;
		ITaxRecordCollectionValidator taxRecordCollectionValidator_constructorInitializedOnly;

#if DEBUG

		public void SubstituteTaxRecoveryLineCreator_ForTestOnly(ITaxRecoveryLineCreator replacement) => taxRecoveryLineCreator_constructorInitializedOnly = replacement;
		public ITaxRecoveryLineCreator TaxRecoveryLineCreator_ExposedForTestOnly => TaxRecoveryLineCreator;

		public void SubstituteThresholdAmountProcessor_ForTestOnly(IThresholdProcessor replacement) => thresholdAmountProcessor_constructorInitializedOnly = replacement;
		public IThresholdProcessor ThresholdAmountProcessor_ExposedForTestOnly => ThresholdAmountProcessor;

		public void SubstituteTaxRecordCalculator_ForTestOnly(ITaxRecordCalculator replacement) => taxRecordCalculator_constructorInitializedOnly = replacement;
		public ITaxRecordCalculator TaxRecordCalculator_ExposedForTestOnly => TaxRecordCalculator;

		public void SubstituteTaxRecordLoader_ForTestOnly(ITaxRecordLoader replacement) => taxRecordLoader_constructorInitializedOnly = replacement;
		public ITaxRecordLoader TaxRecordLoader_ExposedForTestOnly => TaxRecordLoader;

		public void SubstituteTaxRecordPivotProcessor_ForTestOnly(ITaxRecordPivotProcessor replacement) => taxRecordPivotProcessor_constructorInitializedOnly = replacement;
		public ITaxRecordPivotProcessor TaxRecordPivotProcessor_ExposedForTestOnly => TaxRecordPivotProcessor;

		public void SubstituteTaxRecordCollectionValidator_ForTestOnly(ITaxRecordCollectionValidator replacement) => taxRecordCollectionValidator_constructorInitializedOnly = replacement;
		public ITaxRecordCollectionValidator TaxRecordCollectionValidator_ExposedForTestOnly => TaxRecordCollectionValidator;

#endif

		#endregion

		#endregion
	}
}
