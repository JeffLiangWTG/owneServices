using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.Testing
{
	public class AccountingNumberFountainDataSourceForTest : IAccountingNumberFountainDataSource
	{
		public AccountingNumberFountainDataSourceForTest(BusinessObjectFactory factory, ZDateTime postDate, GlbBranch branch, GlbDepartment department)
		{
			this.factory = factory;
			this.postDate = postDate;
			this.branch = branch;
			this.department = department;
		}

		readonly BusinessObjectFactory factory;
		readonly ZDateTime postDate;
		readonly GlbBranch branch;
		readonly GlbDepartment department;

		IDbConnected IAccountingNumberFountainDataSource.Factory => factory;

		ZDateTime IAccountingNumberFountainDataSource.PostDate => postDate;

		GlbBranch IAccountingNumberFountainDataSource.Branch => branch;

		GlbDepartment IAccountingNumberFountainDataSource.Department => department;
	}
}
