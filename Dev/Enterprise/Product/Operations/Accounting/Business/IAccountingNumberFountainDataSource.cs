using CargoWise.Data;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business
{
	public interface IAccountingNumberFountainDataSource
	{
		IDbConnected Factory { get; }
		ZDateTime PostDate { get; }
		GlbBranch Branch { get; }
		GlbDepartment Department { get; }
	}
}