using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Accounting.TaxFramework.Business
{
	public class TaxTransactionsLinkedToJobChargeForDisplay : NonPersistentBusinessObject
	{
		public TaxTransactionsLinkedToJobChargeForDisplay(BusinessObjectFactory factory, ZGuid apLine, ZGuid arLine) : base(factory)
		{
			this.JR_AL_APLine = apLine;
			this.JR_AL_ARLine = arLine;
			taxRecordLoader_constructorInitializedOnly = new TaxRecordLoader();
		}
		ZGuid JR_AL_APLine { get; }
		ZGuid JR_AL_ARLine { get; }

		public TaxTransactionsLinkedToJobChargeCollection TaxTransactionsLinkedToJobCharge
		{
			get
			{
				if (taxTransactionsLinkedToJobCharge == null)
				{
					taxTransactionsLinkedToJobCharge = LoadTaxTransactionsLinkedToJobChargeCollection();
				}
				return taxTransactionsLinkedToJobCharge;
			}
		}

		TaxTransactionsLinkedToJobChargeCollection taxTransactionsLinkedToJobCharge;

		TaxTransactionsLinkedToJobChargeCollection LoadTaxTransactionsLinkedToJobChargeCollection()
		{
			var accTaxTransactionAndPivotList = TaxRecordLoader.LoadTaxRecordsAndPivotsLinkedToTaxableLines(Factory, JR_AL_APLine, JR_AL_ARLine);

			var taxTransactionsLinkedToJobChargeCollection = new TaxTransactionsLinkedToJobChargeCollection(Factory);

			foreach (var accTaxTransactionAndPivot in accTaxTransactionAndPivotList)
			{
				taxTransactionsLinkedToJobChargeCollection.Add(new TaxTransactionsLinkedToJobCharge(accTaxTransactionAndPivot.accTaxTransaction, accTaxTransactionAndPivot.accTaxRecordTransactionLinePivot));
			}

			return taxTransactionsLinkedToJobChargeCollection;
		}

		ITaxRecordLoader TaxRecordLoader => taxRecordLoader_constructorInitializedOnly;
		ITaxRecordLoader taxRecordLoader_constructorInitializedOnly;

#if DEBUG
		public void SubstituteTaxRecordLoader_ForTestOnly(ITaxRecordLoader replacement) => taxRecordLoader_constructorInitializedOnly = replacement;
		public ITaxRecordLoader TaxRecordLoader_ExposedForTestOnly => TaxRecordLoader;
#endif
	}
}
