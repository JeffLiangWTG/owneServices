using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Business
{
	public static class WorkingResourcesCalculator
	{
		public static IEnumerable<GlbStaff> GetWorkingResourcesWithCapability(ZGuid capabilityPK, BusinessObjectFactory factory, WorkingTimeContext workingTimeContext, params ZGuid[] releaseGroupPKs)
		{
			var result = new List<GlbStaff>();
			var resources = GetResourcesWithCapability(capabilityPK, factory, releaseGroupPKs);

			foreach (var resource in resources)
			{
				resource.AddBMSHolidayFetchHint();
			}

			foreach (var resource in resources)
			{
				var branch = resource.HomeBranch ?? workingTimeContext.Branch;
				var localTime = ZDateTime.UtcNow.ToLocalBranchTime(branch);

				if (resource.IsWorking(localTime.Date))
				{
					result.Add(resource);
				}
			}

			return result;
		}

		internal static ICollection<GlbStaff> GetResourcesWithCapability(ZGuid capabilityPK, BusinessObjectFactory factory, params ZGuid[] releaseGroupPKs)
		{
			var query = new ZDBOnlyQuery(typeof(GlbStaff));
			query.AddToFilter(GlbStaffSchema.GS_IsActive, true);

			var capabilitySubQuery = new ZDBOnlySubQuery(typeof(GlbResourceCapabilityPivot), GlbResourceCapabilityPivotSchema.G5_GS_Resource);
			capabilitySubQuery.AddToFilter(GlbResourceCapabilityPivotSchema.G5_G4_Capability, capabilityPK);
			query.AddSubQuery(capabilitySubQuery, JoinCondition.And);

			var capability = factory.LoadTop1<GlbCapability>(new ZQuery(GlbCapabilitySchema.PK, capabilityPK));

			if (capability != null && capability.G4_CapacityScope == GlbCapabilityScopeList.Codes.GroupScope)
			{
				var validReleaseGroupPKs = releaseGroupPKs.Where(pk => pk.IsValid).Distinct().ToArray();
				if (validReleaseGroupPKs.Length > 0)
				{
					var releaseGroupSubQuery = new ZDBOnlySubQuery(typeof(GlbGroupLink), GlbGroupLinkSchema.GK_GS);
					releaseGroupSubQuery.AddToFilter(GlbGroupLinkSchema.GK_GG, validReleaseGroupPKs);
					query.AddSubQuery(releaseGroupSubQuery, JoinCondition.And);
				}
			}

			var resourcesWithCapabilities = factory.Load<GlbStaff>(query);

			foreach (var staff in resourcesWithCapabilities)
			{
				var fetchHintQuery = new ZQuery(BMComponentResourceLinkSchema.FD_GS_NKResource, staff.GS_Code);
				factory.AddFetchHint(BMComponentResourceLinkSchema.Instance, fetchHintQuery);
			}

			return resourcesWithCapabilities;
		}

		internal static GlbStaff[] GetResourcesWithTasksInReleaseGate(BMComponent buffer)
		{
			var codes = GetResourceCodesWithTasksInReleaseGate(buffer);
			var query = new ZQuery(GlbStaffSchema.GS_Code, codes) { OrderBy = GlbStaffSchema.Constants.GS_Code };
			query.AddToFilter(GlbStaffSchema.GS_IsActive, true);

			return buffer.Factory.Load<GlbStaff>(query);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		static IEnumerable<string> GetResourceCodesWithTasksInReleaseGate(BMComponent buffer)
		{
			var componentPKs = buffer.GetFeedingComponents().Select(c => c.PK).Append(buffer.PK).ToArray();

			var results = new HashSet<string>();

			using (var command = Db.Connection.Command(ResourcesWithTasksInReleaseGateSql))// Using Business Objects in this case is overkill.
			{
				command.AddTableValuedParameter("@ComponentPKs", ProcessHeaderSchema.FH_FC_CurrentComponent, componentPKs);
				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						results.Add(reader.GetString(0));
					}
				}
			}

			return results.Where(s => !string.IsNullOrEmpty(s));
		}

		const string ResourcesWithTasksInReleaseGateSql = @"
DECLARE @Result TABLE (GS_Code varchar(3) not null)

INSERT @Result 
SELECT distinct P9_GS_NKAssignedStaffMember
FROM dbo.ProcessTasks
WHERE P9_FC_CurrentComponent IN
(
	SELECT Value FROM @ComponentPKs
)

INSERT @Result
SELECT GS_Code
FROM dbo.GlbStaff
WHERE GS_PK IN
(
	SELECT G5_GS_Resource
	FROM dbo.ProcessTasks
	JOIN dbo.GlbResourceCapabilityPivot ON P9_G4_RequiredCapability = G5_G4_Capability
	WHERE 1=1
		AND P9_Status in ('ASN', 'OPN', 'WRK', 'SUS')
		AND P9_G4_RequiredCapability IS NOT NULL
		AND P9_FC_CurrentComponent IN (SELECT Value FROM @ComponentPKs)
		AND P9_GS_NKAssignedStaffMember = ''
		AND P9_Type <> 'MIL'
		AND P9_Type <> 'EXC'
		AND P9_Type <> 'TRG'
)
AND GS_IsActive = 1


INSERT @Result
SELECT GS_Code FROM dbo.GlbStaff
WHERE GS_PK IN
(
	SELECT GK_GS FROM dbo.GlbGroupLink
	WHERE GK_GG IN
	(
		SELECT FH_GG_ReleaseGroup 
		FROM dbo.ProcessHeader 
		WHERE FH_FC_CurrentComponent in (SELECT Value FROM @ComponentPKs)
	)
)

SELECT * FROM @Result
";
	}
}
