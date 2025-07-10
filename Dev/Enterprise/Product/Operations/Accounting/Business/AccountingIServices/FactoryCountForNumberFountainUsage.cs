using CargoWise.EntityFramework;

namespace Enterprise.Accounting.Business.AccountingIServices
{
	internal class FactoryCountForNumberFountainUsage : BizoDataRowRelatedValues<int>
	{
		public static void SetDataRowRelatedValue(BusinessObject bizoForDataRow, string flagName, int flagValue) => BizoDataRowRelatedValuesAccessor<FactoryCountForNumberFountainUsage>.SetDataRowRelatedValue(bizoForDataRow, flagName, flagValue);

		public static int GetDataRowRelatedValue(BusinessObject bizoForDataRow, string flagName) => BizoDataRowRelatedValuesAccessor<FactoryCountForNumberFountainUsage>.GetDataRowRelatedValue(bizoForDataRow, flagName);
	}
}