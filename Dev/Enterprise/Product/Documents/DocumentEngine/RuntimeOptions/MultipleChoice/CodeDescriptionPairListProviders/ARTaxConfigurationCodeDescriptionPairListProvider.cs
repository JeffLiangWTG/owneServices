using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesTaxFrameworkConstants;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	public class ARTaxConfigurationCodeDescriptionPairListProvider : TaxConfigurationCodeDescriptionPairListProvider
	{
		protected override void AddAdditionalFilter(ZQuery query)
		{
			base.AddAdditionalFilter(query);

			query.AddToFilter(AccTaxConfigurationSchema.ETC_Ledger, TaxConfigurationLedgers.AccountsReceivable.Code);
		}
	}
}
