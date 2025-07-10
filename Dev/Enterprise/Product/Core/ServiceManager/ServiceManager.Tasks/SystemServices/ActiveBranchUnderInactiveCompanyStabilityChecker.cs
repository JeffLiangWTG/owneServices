using System.Collections.Generic;
using CargoWise.Data;
using Enterprise.ServiceManager.Tasks.SystemServices;
using Enterprise.StabilityChecker;

[assembly: StabilityChecker("Active Branch Under Inactive Company Stability Checker", "BCC", typeof(ActiveBranchUnderInactiveCompanyStabilityChecker))]

namespace Enterprise.ServiceManager.Tasks.SystemServices
{
	class ActiveBranchUnderInactiveCompanyStabilityChecker : IStabilityChecker
	{
		public StabilityResult[] Check()
		{
			var results = new List<StabilityResult>();

			var sqlText = @"
SELECT GB_Code, GB_BranchName, GC_Code, GC_Name
FROM dbo.GlbBranch
LEFT JOIN dbo.GlbCompany ON [GB_GC] = GC_PK
WHERE GB_IsActive = 1 AND GC_IsActive = 0
			";

			//Have to extract all warnings before using Res.GetString as it hits the DB if this is the first time Res.GetString has been called this CW instance.
			var warnings = new List<(string, string)>();
			using (var reader = Db.Connection.Command(sqlText).ExecuteReader())
			{
				while (reader.Read())
				{
					warnings.Add(((string)reader["GB_Code"], (string)reader["GC_Code"]));
				}
			}
			foreach (var warning in warnings)
			{
				results.Add(new StabilityResult(StabilityResultLevel.Warning,
						Res.GetString("ee985bb1-457e-481b-960e-ed56141f5d48", "Branch ({0}) is attached to an inactive Company ({1}) record. Please activate Company ({1}) record or change to a valid branch code that is attached to an active company record.",
							warning.Item1, warning.Item2)));
			}
			return results.ToArray();
		}
	}
}
