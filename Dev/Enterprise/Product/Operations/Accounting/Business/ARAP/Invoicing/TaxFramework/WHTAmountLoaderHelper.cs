using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.TaxFramework
{
	public static class WHTAmountLoaderHelper
	{
		public static void RegisterTransactionsForLoadingWHTAmounts(this BusinessObjectFactory factory, bool isWHTRealizationInProgress, params ZGuid[] transactionPKs)
		{
			var loader = TaxFrameworkObjectFactory.GetWHTAmountLoader(factory);
			loader.IsWHTRealizationInProgress = isWHTRealizationInProgress;
			loader.RegisterForLoadingWHTAmounts(transactionPKs);
		}

		public static ZDecimal LoadRealizedWHT(this BusinessObjectFactory factory, ZGuid transactionPK) =>
			TaxFrameworkObjectFactory.GetWHTAmountLoader(factory).GetRealizedWHT(transactionPK);

		public static ZDecimal LoadNotionalWHT(this BusinessObjectFactory factory, ZGuid transactionPK) =>
			TaxFrameworkObjectFactory.GetWHTAmountLoader(factory).GetNotionalWHT(transactionPK);
	}
}