using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public partial class JobProfitLossCalculation
	{
		public (string baseSqlQuery, ZSqlParameter[] parameters) GetSqlQueryParameters_ForTestOnly(JobHeader job)
		{
			return GetSqlQueryParameters(job);
		}
	}
}
