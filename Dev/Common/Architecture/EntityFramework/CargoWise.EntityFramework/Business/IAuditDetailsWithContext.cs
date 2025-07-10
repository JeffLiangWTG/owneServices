using CargoWise.Types;

namespace CargoWise.EntityFramework
{
	public interface IAuditDetailsWithContext : IAuditDetails
	{
		ZString SystemCreateBranch { get; }
		ZString SystemCreateDepartment { get; }
	}
}
