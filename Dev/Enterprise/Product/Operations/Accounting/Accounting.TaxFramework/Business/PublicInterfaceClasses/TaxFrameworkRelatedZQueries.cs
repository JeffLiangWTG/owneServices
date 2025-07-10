using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.TaxFramework.Business.PublicInterfaceClasses
{
	public static class TaxFrameworkRelatedZQueries
	{
		public static ZQuery FilterForInvoicesWithoutTaxTranscation()
		{
			ZDBOnlyQuery transactionHeaderQuery = new ZDBOnlyQuery(typeof(AccTransactionHeader));
			ZDBOnlySubQuery taxTransactionQuery = new ZDBOnlySubQuery(typeof(AccTaxTransaction), AccTaxTransactionSchema.ATT_AH, true);
			transactionHeaderQuery.AddSubQuery(taxTransactionQuery, JoinCondition.And);

			return transactionHeaderQuery;
		}
	}
}
