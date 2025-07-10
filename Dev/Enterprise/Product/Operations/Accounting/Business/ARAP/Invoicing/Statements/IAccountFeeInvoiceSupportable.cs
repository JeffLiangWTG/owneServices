using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public interface IAccountFeeInvoiceSupportable
	{
		string GetOrgPKQueryForAccountFee(ZSqlParameterCollection sqlParams);
		GlbCompany Company { get; }
		GlbBranch Branch { get; }
		bool CreateAccountFee { get; }
	}
}
