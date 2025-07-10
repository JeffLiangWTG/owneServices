using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.TaxFramework.Business
{
	public interface ITaxProcessor : ITaxProcessorCrossAssembly
	{
		string ProcessTaxesOnPosting(ITaxRecordParent taxParent);
		void ProcessTaxesOnMatching(ITaxRecordParent taxParent, ZDate matchDate);
		void ProcessOnParentReversing(ITaxRecordParent originalTaxParent, ITaxRecordParent reversedTaxParent, ZDate reversedTaxPostAndRealisationDate);
		string ProcessPaymentRetentionTaxes(ITaxRecordParent taxParent, IEnumerable<IMatchTransactionDetails> matchTransactionDetails);
		void UpdatePostDateOnParentReversing(ITaxRecordParent originalTaxParent, ITaxRecordParent reversedTaxParent, ZDate reversedTaxPostAndRealisationDate);
		(ZDecimal NotionalWHT, ZDecimal RealizedWHT, IMatchTransactionDetails[] MatchDetails) GetPaymentRetentionMatchTransactionDetails(ITaxRecordParent taxParent);
		void DeleteTaxesNotInDB(ITaxRecordParent taxParent);
		void DeleteTaxRecordNotInDB(ITaxRecordParent taxParent, AccTaxTransaction taxRecordToDelete);
		void ProcessRealisedSPRAPTaxRecordsOnReversing(BusinessObjectFactory factory, ZGuid matchTransactionPK, ZGuid reversedTaxMatchTransactionPK, ZDate reversedTaxPostAndRealisationDate);
		bool HasRealisedAPPaymentRetentionRecords(ITaxRecordParent taxParent);
		AccTaxTransaction[] GetTaxTransactions(ITaxRecordParent taxParent);
		void UpdatePostDate(ITaxRecordParent taxParent);
		(IReadOnlyCollection<(AccTaxTransaction, AccTaxRecordTransactionLinePivot)> result, string errorMessage) GetEstimatedTaxRecordsByTaxSystemCodes(ITaxRecordParentBase taxParent, IEnumerable<ZString> taxSystemCodesForFilter);
		IReadOnlyCollection<IGLMovementDetails> GetTaxDetailsForAccountingJournal(BusinessObjectFactory factory, ZGuid taxParentPK, bool useGeneralLedgerData = false);
		void RecalculatePeriodForAllGLMovementRecords(ZGuid companyPK);
		AccTaxRecordTransactionLinePivot[] LoadTaxRecordPivots(params AccTaxTransaction[] taxRecords);
		IReadOnlyCollection<IReadOnlyTaxRecordData> GetTaxRecordDataForDataTransfer(ITaxRecordParent taxParent);
		void RestoreFromTaxRecordData(ITaxRecordParent taxParent, IReadOnlyCollection<IReadOnlyTaxRecordData> taxRecordsData);
	}

	class TaxProcessor : ITaxProcessor
	{
		public TaxProcessor()
			: this(new TaxRecordCreator(), new TaxRecordRealiser(), new TaxRecordLoader(), new TaxRecordReverser(), new TaxFrameworkDataTransfer(new TaxRecordPivotProcessor()))
		{
		}

		internal TaxProcessor(ITaxRecordCreator taxRecordCreator, ITaxRecordRealiser taxRecordRealiser, ITaxRecordLoader taxRecordLoader, ITaxRecordReverser taxRecordReverser, ITaxFrameworkDataTransfer taxFrameworkDataTransfer)
		{
			taxRecordCreator_constructorInitializedOnly = taxRecordCreator;
			taxRecordRealiser_constructorInitializedOnly = taxRecordRealiser;
			taxRecordLoader_constructorInitializedOnly = taxRecordLoader;
			taxRecordReverser_constructorInitializedOnly = taxRecordReverser;
			TaxFrameworkDataTransfer = taxFrameworkDataTransfer;
		}

		string ITaxProcessor.ProcessTaxesOnPosting(ITaxRecordParent taxParent)
		{
			var errorMessage = string.Empty;
			try
			{
				TaxRecordCreator.CreateTaxRecords(taxParent);

				taxParent.IsTaxTransactionsCalculatedBeforePosting = true;
			}
			catch (TaxFrameworkUserDataException ex)
			{
				((ITaxProcessor)this).DeleteTaxesNotInDB(taxParent);
				errorMessage = ex.LocalLanguageMessage;
				if (ex is TaxFrameworkUnknownConfigurationValueException)
				{
					ErrorReporter.ReportOnce(nameof(TaxFrameworkUnknownConfigurationValueException), ex);
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				((ITaxProcessor)this).DeleteTaxesNotInDB(taxParent);
				throw;
			}

			return errorMessage;
		}

		void ITaxProcessor.ProcessTaxesOnMatching(ITaxRecordParent taxParent, ZDate matchDate)
		{
			TaxRecordRealiser.RealiseTaxRecord(taxParent, matchDate);
		}

		string ITaxProcessor.ProcessPaymentRetentionTaxes(ITaxRecordParent taxParent, IEnumerable<IMatchTransactionDetails> matchTransactionDetails)
		{
			string errorMessage;
			try
			{
				errorMessage = TaxRecordRealiser.RealisePaymentRetentionTaxRecords(taxParent, matchTransactionDetails);
			}
			catch (TaxFrameworkUnknownConfigurationValueException ex)
			{
				errorMessage = ex.LocalLanguageMessage;
				ErrorReporter.ReportOnce(nameof(TaxFrameworkUnknownConfigurationValueException), ex);
			}

			return errorMessage;
		}

		void ITaxProcessor.ProcessOnParentReversing(ITaxRecordParent originalTaxParent, ITaxRecordParent reversedTaxParent, ZDate reversedTaxPostAndRealisationDate)
		{
			TaxRecordReverser.ReverseNotRealisedSPRAPRecords(originalTaxParent);
			TaxRecordReverser.ReverseNonSPRAPTaxRecords(originalTaxParent, reversedTaxParent, reversedTaxPostAndRealisationDate);
		}

		void ITaxProcessor.UpdatePostDateOnParentReversing(ITaxRecordParent originalTaxParent, ITaxRecordParent reversedTaxParent, ZDate reversedTaxPostAndRealisationDate)
		{
			if (originalTaxParent == null)
			{
				throw new ArgumentNullException(nameof(originalTaxParent));
			}
			if (reversedTaxParent == null)
			{
				throw new ArgumentNullException(nameof(reversedTaxParent));
			}

			TaxRecordReverser.UpdatePostDateOnNonSPRAPTaxRecords(originalTaxParent, reversedTaxParent, reversedTaxPostAndRealisationDate);
		}

		void ITaxProcessor.DeleteTaxesNotInDB(ITaxRecordParent taxParent)
		{
			try
			{
				TaxRecordCreator.DeleteTaxRecordsNotInDB(taxParent);
				taxParent.IsTaxTransactionsCalculatedBeforePosting = false;
			}
			catch (InvalidOperationException ex)
			{
				ErrorReporter.ReportOnce(nameof(InvalidOperationException), ex);
			}
		}

		(ZDecimal NotionalWHT, ZDecimal RealizedWHT, IMatchTransactionDetails[] MatchDetails) ITaxProcessor.GetPaymentRetentionMatchTransactionDetails(ITaxRecordParent taxParent)
		{
			var validSPRTaxRecords = TaxRecordLoader.LoadSPRAPTaxRecords(taxParent, getNotionalRecordsOnly: false);
			if (validSPRTaxRecords.Any())
			{
				var notionalWHT = validSPRTaxRecords.Where(t => !t.ATT_RealisationDate.IsValid).Sum(t => t.ATT_LocalTaxAmount);
				var realizedWHT = validSPRTaxRecords.Where(t => t.ATT_RealisationDate.IsValid).Sum(t => t.ATT_LocalTaxAmount);
				var matchDetails = GetMatchDetails(validSPRTaxRecords);

				return (notionalWHT, realizedWHT, matchDetails.ToArray());
			}
			return default;
		}

		static IEnumerable<IMatchTransactionDetails> GetMatchDetails(AccTaxTransaction[] validSPRTaxRecords)
		{
			if (validSPRTaxRecords != null)
			{
				return from AccTaxTransaction t in validSPRTaxRecords
					   where !t.ATT_RealisationDate.IsValid
					   group t by new { t.ATT_ETC, t.ATT_TaxAuthorityServiceCode, t.ATT_GB, t.ATT_GE_Department, t.ATT_RateNumerator, t.ATT_RateDenominator } into g
					   let transactionCurrency = g.First().ATT_RX_NKOSTaxCurrency
					   let localTotalAmount = g.Sum(x => x.ATT_LocalTaxAmount)
					   let osTotalAmount = g.Sum(x => x.ATT_OSTaxAmount)
					   select new MatchTransactionDetails(g.Key.ATT_GB, g.Key.ATT_GE_Department, transactionCurrency, localTotalAmount, osTotalAmount, g.First().ATT_AG_TaxControlAccount, g.Select(x => x.PK).ToArray());
			}
			return null;
		}

		void ITaxProcessor.DeleteTaxRecordNotInDB(ITaxRecordParent taxParent, AccTaxTransaction taxRecordToDelete)
		{
			if (!taxRecordToDelete.IsInDatabase)
			{
				TaxRecordCreator.DeleteTaxRecordNotInDB(taxParent, taxRecordToDelete);
			}
		}

		bool ITaxProcessor.HasRealisedAPPaymentRetentionRecords(ITaxRecordParent taxParent) => TaxRecordLoader.HasRealisedSPRAPTaxRecordsInDB(taxParent);

		void ITaxProcessor.ProcessRealisedSPRAPTaxRecordsOnReversing(BusinessObjectFactory factory, ZGuid matchTransactionPK, ZGuid reversedTaxMatchTransactionPK, ZDate reversedTaxPostAndRealisationDate)
		{
			TaxRecordReverser.ReverseRealisedSPRAPTaxRecords(factory, matchTransactionPK, reversedTaxMatchTransactionPK, reversedTaxPostAndRealisationDate);
		}

		AccTaxTransaction[] ITaxProcessor.GetTaxTransactions(ITaxRecordParent taxParent) => TaxRecordLoader.LoadAllReportableTaxRecords(taxParent);

		void ITaxProcessor.UpdatePostDate(ITaxRecordParent taxParent)
		{
			TaxRecordCreator.UpdatePostDate(taxParent);
		}

		(IReadOnlyCollection<(AccTaxTransaction, AccTaxRecordTransactionLinePivot)> result, string errorMessage) ITaxProcessor.GetEstimatedTaxRecordsByTaxSystemCodes(ITaxRecordParentBase taxParent, IEnumerable<ZString> taxSystemCodesForFilter)
		{
			var errorMessage = string.Empty;
			IReadOnlyCollection<(AccTaxTransaction, AccTaxRecordTransactionLinePivot)> result = new List<(AccTaxTransaction, AccTaxRecordTransactionLinePivot)>();
			try
			{
				result = TaxRecordCreator.GetEstimatedTaxRecords(taxParent, taxSystemCodesForFilter);
			}
			catch (TaxFrameworkUserDataException ex)
			{
				if (ex is TaxFrameworkUnknownConfigurationValueException)
				{
					ErrorReporter.ReportOnce(nameof(TaxFrameworkUnknownConfigurationValueException), ex);
				}

				errorMessage = ex.LocalLanguageMessage;
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				throw;
			}

			return (result, errorMessage);
		}

		(ZDate TaxExpenseDate, ZDecimal TaxExpenseAmount)[] ITaxProcessorCrossAssembly.GetTaxExpenses(BusinessObjectFactory factory, ZGuid linePK)
		{
			return TaxRecordLoader.GetTaxExpenses(factory, linePK);
		}

		IReadOnlyCollection<IGLMovementDetails> ITaxProcessor.GetTaxDetailsForAccountingJournal(BusinessObjectFactory factory, ZGuid taxParentPK, bool useGeneralLedgerData)
		{
			return useGeneralLedgerData ? TaxRecordLoader.LoadGLMovementDetailsWithGeneralLedgerData(factory, taxParentPK) : TaxRecordLoader.LoadGLMovementDetails(factory, taxParentPK);
		}

		void ITaxProcessor.RecalculatePeriodForAllGLMovementRecords(ZGuid companyPK)
		{
			var sql = @"
EXEC dbo.SuspendTrigger 'TG_AccTaxGLMovement_Update'

UPDATE dbo.AccTaxGLMovement
	SET ATM_Period = AM_Period,
	ATM_SystemLastEditTimeUtc = GETUTCDATE(),
	ATM_SystemLastEditUser = @SystemLastEditUser
FROM dbo.AccTaxGLMovement 
	JOIN dbo.AccTaxTransaction ON ATM_ATT_TaxTransaction = ATT_PK
	CROSS APPLY dbo.GetPeriodFromDateInline (ATM_Date, ATT_GC) as GetPeriod
WHERE
	ATT_GC = @CompanyPK
	AND ATM_Period <> AM_Period

EXEC dbo.ResumeTrigger 'TG_AccTaxGLMovement_Update'
";

			using (var manager = Db.Connection.BeginTransactionWithManager())
			{
				using (var sqlCommand = Db.Connection.Command(sql))
				{
					sqlCommand.CommandTimeout = int.MaxValue;
					sqlCommand.AddParameterBasedOnDbColumn("@CompanyPK", companyPK.ToGuid(), AccTaxTransactionSchema.ATT_GC);
					sqlCommand.AddParameterBasedOnDbColumn("@SystemLastEditUser", GlbStaff.CurrentUser.GS_Code.ToString(), GlbStaffSchema.GS_Code);

					sqlCommand.ExecuteNonQuery();
				}
				manager.CommitTransaction();
			}
		}

		AccTaxRecordTransactionLinePivot[] ITaxProcessor.LoadTaxRecordPivots(params AccTaxTransaction[] taxRecords)
		{
			return TaxRecordLoader.LoadTaxRecordPivots(false, taxRecords);
		}

		IReadOnlyCollection<IReadOnlyTaxRecordData> ITaxProcessor.GetTaxRecordDataForDataTransfer(ITaxRecordParent taxParent)
		{
			Argument.NotNull(taxParent, nameof(taxParent));

			var taxTransactions = TaxRecordLoader.LoadAllReportableTaxRecords(taxParent);
			var taxRecordTransactionLinePivots = TaxRecordLoader.LoadTaxRecordPivots(false, taxTransactions);

			return TaxFrameworkDataTransfer.CreateTaxRecordDataFromTaxTransactions(taxParent, taxTransactions, taxRecordTransactionLinePivots);
		}

		void ITaxProcessor.RestoreFromTaxRecordData(ITaxRecordParent taxParent, IReadOnlyCollection<IReadOnlyTaxRecordData> taxRecordsData)
		{
			Argument.NotNull(taxParent, nameof(taxParent));

			TaxFrameworkDataTransfer.CreateTaxTransactionFromTaxRecordData(taxParent, taxRecordsData);
		}

		ITaxRecordCreator TaxRecordCreator => taxRecordCreator_constructorInitializedOnly;
		ITaxRecordCreator taxRecordCreator_constructorInitializedOnly;

		ITaxRecordRealiser TaxRecordRealiser => taxRecordRealiser_constructorInitializedOnly;
		ITaxRecordRealiser taxRecordRealiser_constructorInitializedOnly;

		ITaxRecordReverser TaxRecordReverser => taxRecordReverser_constructorInitializedOnly;
		ITaxRecordReverser taxRecordReverser_constructorInitializedOnly;

		ITaxRecordLoader TaxRecordLoader => taxRecordLoader_constructorInitializedOnly;
		ITaxRecordLoader taxRecordLoader_constructorInitializedOnly;

		ITaxFrameworkDataTransfer TaxFrameworkDataTransfer { get; }

#if DEBUG
		public void SubstituteTaxRecordCreator_ForTestOnly(ITaxRecordCreator replacement) => taxRecordCreator_constructorInitializedOnly = replacement;
		public ITaxRecordCreator TaxRecordCreator_ExposedForTestOnly => TaxRecordCreator;

		public void SubstituteTaxRecordRealiser_ForTestOnly(ITaxRecordRealiser replacement) => taxRecordRealiser_constructorInitializedOnly = replacement;
		public ITaxRecordRealiser TaxRecordRealiser_ExposedForTestOnly => TaxRecordRealiser;

		public void SubstituteTaxRecordReverser_ForTestOnly(ITaxRecordReverser replacement) => taxRecordReverser_constructorInitializedOnly = replacement;
		public ITaxRecordReverser TaxRecordReverser_ExposedForTestOnly => TaxRecordReverser;

		public void SubstituteTaxRecordLoader_ForTestOnly(ITaxRecordLoader replacement) => taxRecordLoader_constructorInitializedOnly = replacement;

		public ITaxRecordLoader TaxRecordLoader_ExposedForTestOnly => TaxRecordLoader;
#endif
	}
}
