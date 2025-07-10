using CargoWise.Common;

namespace Enterprise.Accounting.TaxFramework.Business
{
	public interface ITaxRecordPivotProcessor
	{
		AccTaxRecordTransactionLinePivot Create(AccTaxTransaction taxRecord, ITaxableTransactionLineBase line);
		void DeleteTaxRecordsWithPivots(params AccTaxTransaction[] taxRecords);
	}

	class TaxRecordPivotProcessor : ITaxRecordPivotProcessor
	{
		public TaxRecordPivotProcessor()
		{
			taxRecordLoader_constructorInitializedOnly = new TaxRecordLoader();
		}

		ITaxRecordLoader TaxRecordLoader => taxRecordLoader_constructorInitializedOnly;
		ITaxRecordLoader taxRecordLoader_constructorInitializedOnly;

#if DEBUG

		public void SubstituteTaxRecordLoader_ForTestOnly(ITaxRecordLoader replacement) => taxRecordLoader_constructorInitializedOnly = replacement;
		public ITaxRecordLoader TaxRecordLoader_ExposedForTestOnly => TaxRecordLoader;

#endif

		AccTaxRecordTransactionLinePivot ITaxRecordPivotProcessor.Create(AccTaxTransaction taxRecord, ITaxableTransactionLineBase line)
		{
			var pivot = taxRecord.Factory.New<AccTaxRecordTransactionLinePivot>();
			pivot.ATP_ATT = taxRecord.PK;
			pivot.LinkLine(line);
			pivot.ATP_IsTaxExpense = !taxRecord.ATT_AG_TaxExpenseAccount.IsEmpty;

			return pivot;
		}

		void ITaxRecordPivotProcessor.DeleteTaxRecordsWithPivots(params AccTaxTransaction[] taxRecords)
		{
			if (taxRecords.Length == 0)
			{
				return;
			}

			var linePivots = TaxRecordLoader.LoadTaxRecordPivots(useLocalCacheOnly: true, taxRecords);
			linePivots.ForEach(x => x.Delete());
			taxRecords.ForEach(x => x.Delete());
		}
	}
}
