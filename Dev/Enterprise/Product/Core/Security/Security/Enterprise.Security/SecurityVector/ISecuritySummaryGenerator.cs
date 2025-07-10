using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Security
{
	public interface ISecuritySummaryGenerator<T>
	{
		T GenerateLocalAdministratorSummary(GlbStaff staff, IEnumerable<IGlbSecurity> securities);
		T GenerateSummary(GlbStaff staff, ZGuid[] departmentsGuids, ZGuid[] branchesGuids,
			string[] departments, string[] branches,
			IEnumerable<IGlbSecurity> allSecurities, SecurityCalculatorForStaff securityCalculator, SecurityCheckpoint checkpoint, bool startFrom, bool useBranchesAndDepartments);

		T CalculateInitialValue(GlbStaff staff);

		T DeniedValue { get; }

		T GrantedValue { get; }
	}
}