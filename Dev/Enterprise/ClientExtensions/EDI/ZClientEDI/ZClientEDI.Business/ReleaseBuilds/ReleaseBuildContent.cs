using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.ReleaseBuilds.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using Microsoft.SqlServer.Types;

namespace Enterprise.Client.EDI.ReleaseBuilds
{
	public class ReleaseBuildContent : IReleaseBuildContent
	{
		[SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public static readonly Overridable<Func<BusinessObjectFactory, IReleaseBuildContent>> Factory = new Overridable<Func<BusinessObjectFactory, IReleaseBuildContent>>(factory => new ReleaseBuildContent(factory));

		public static IReleaseBuildContent New(BusinessObjectFactory factory) => Factory.Value(factory);

		public ReleaseBuildContent(BusinessObjectFactory factory)
		{
			this.factory = factory;
		}
		readonly BusinessObjectFactory factory;

		public ZQuery GetPatchedWorkItemsQuery(ZGuid from, ZGuid to)
		{
			return GetPatchedWorkItemsQuery(factory.Load<ReleaseBuild>(from), factory.Load<ReleaseBuild>(to));
		}

		public ZQuery GetPatchedWorkItemsQuery(ReleaseBuild from, ReleaseBuild to)
		{
			return GetPatchedWorkItemsSubQuery(from, to);
		}

		public ZQuery GetPatchedIncidentsQuery(ZGuid from, ZGuid to)
		{
			return GetPatchedIncidentsQuery(factory.Load<ReleaseBuild>(from), factory.Load<ReleaseBuild>(to));
		}

		public ZQuery GetPatchedIncidentsQuery(ReleaseBuild from, ReleaseBuild to)
		{
			var patchedWorkItemQuery = GetPatchedWorkItemsSubQuery(from, to);
			ZDBOnlyQuery supportIncidentQuery = new ZDBOnlyQuery(typeof(SupportIncident));
			supportIncidentQuery.AddSubQuery(GetPatchedIncidentsSubQuery(patchedWorkItemQuery, GenPivotSchema.XX_Relation2ID, GenPivotSchema.XX_Relation1ID), JoinCondition.And);
			supportIncidentQuery.AddSubQuery(GetPatchedIncidentsSubQuery(patchedWorkItemQuery, GenPivotSchema.XX_Relation1ID, GenPivotSchema.XX_Relation2ID), JoinCondition.Or);
			return supportIncidentQuery;
		}

		ZDBOnlySubQuery GetPatchedIncidentsSubQuery(ZDBOnlyQuery patchedWorkItemQuery, SchemaColumn linkFk, SchemaColumn workItemFk)
		{
			var linkSubQuery = new ZDBOnlySubQuery(typeof(GenPivot), linkFk);
			linkSubQuery.AddToFilter(GenPivotSchema.XX_RelationType, Core.Constants.GenPivotTypes.ProcessManagement);
			var workItemSubQuery = new ZDBOnlySubQuery(typeof(NewWorkItem), workItemFk);
			workItemSubQuery.AddToFilter(patchedWorkItemQuery, JoinCondition.And);
			linkSubQuery.AddSubQuery(workItemSubQuery, JoinCondition.And);
			return linkSubQuery;
		}

		public SupportIncidentCollection GetPatchedIncidents(ReleaseBuild from, ReleaseBuild to)
		{
			SupportIncidentCollection patchedIncidents = new SupportIncidentCollection(factory);
			patchedIncidents.Load(GetPatchedIncidentsQuery(from, to));
			return patchedIncidents;
		}

		public NewWorkItemCollection GetPatchedWorkItems(ReleaseBuild from, ReleaseBuild to)
		{
			NewWorkItemCollection patchedWorkItems = new NewWorkItemCollection(factory);
			patchedWorkItems.Load(GetPatchedWorkItemsQuery(from, to));
			return patchedWorkItems;
		}

		ZDBOnlyQuery GetPatchedWorkItemsSubQuery(ReleaseBuild from, ReleaseBuild to)
		{
			if (from.VersionNumber > to.VersionNumber)
			{
				throw new ReleaseBuildContentException(FormattableString.Invariant($"From version {from.VersionNumber} is greater than to version {to.VersionNumber}"));
			}

			var releaseBuildInfo = GetReleaseBuildInfo(from.VersionNumber.ToVersion(), to.VersionNumber.ToVersion());

			if (releaseBuildInfo.FromBranch is null || releaseBuildInfo.ToBranch is null)
			{
				throw new ReleaseBuildContentException(FormattableString.Invariant($"No valid deployment found for fromVersionNumber: {from.VersionNumber}, and toVersionNumber is {to.VersionNumber}"));
			}

			string sql;
			var parameters = new ZSqlParameterCollection();
			if (IsReleaseBranch(releaseBuildInfo.ToBranch))
			{
				if (IsReleaseBranch(releaseBuildInfo.FromBranch))
				{
					if (releaseBuildInfo.FromBranch.Equals(releaseBuildInfo.ToBranch, StringComparison.OrdinalIgnoreCase))
					{
						// same release branch
						sql = FormattableString.Invariant(
							$@"select UH_WorkItem from UserTestHeader
							join UserTest on UT_UH = UH_PK
							where UT_Branch = @releaseBranch
							and {FirstBuildStartedSubquery} > @fromDate
							and UH_CommitTime <= @toDate
							and UH_Status in ('{ShelfStatuses.CheckedIn}', '{ShelfStatuses.CheckedInAndNotified}')
							and UH_WorkItem is not null");
						parameters.Add("@releaseBranch", releaseBuildInfo.FromBranch, Schema.GenericStringSchemaColumn);
						parameters.Add("@fromDate", releaseBuildInfo.FromDate, Schema.GenericDateTimeColumn);
						parameters.Add("@toDate", releaseBuildInfo.ToDate, Schema.GenericDateTimeColumn);
					}
					else
					{
						// across release branches
						sql = FormattableString.Invariant(
							$@"	select UH_WorkItem from UserTestHeader
								join UserTest on UT_UH = UH_PK
								where UT_Branch = @toReleaseBranch
								and UH_CommitTime <= @toDate
								and UH_Status in ('{ShelfStatuses.CheckedIn}', '{ShelfStatuses.CheckedInAndNotified}')
								and UH_WorkItem is not null
							union
								select UH_WorkItem from UserTestHeader
								join UserTest on UT_UH = UH_PK
								join BranchRoot on (UT_TargetRepository + '/' like BO_Branch + '/%' and UT_Branch = 'master')
								where {FirstBuildStartedSubquery} > (select min(UH_CommitTime) from UserTestHeader
														join UserTest on UT_UH = UH_PK
														where UT_Branch = @fromReleaseBranch)
								and UH_CommitTime <= (select min(UH_CommitTime) from UserTestHeader
														join UserTest on UT_UH = UH_PK
														where UT_Branch = @toReleaseBranch)
								and UH_Status in ('{ShelfStatuses.CheckedIn}', '{ShelfStatuses.CheckedInAndNotified}')
								and UH_WorkItem is not null
							except
								select UH_WorkItem from UserTestHeader
								join UserTest on UT_UH = UH_PK
								where UT_Branch = @fromReleaseBranch
								and UH_CommitTime <= @fromDate
								and UH_Status in ('{ShelfStatuses.CheckedIn}', '{ShelfStatuses.CheckedInAndNotified}')
								and UH_WorkItem is not null");
						parameters.Add("@fromReleaseBranch", releaseBuildInfo.FromBranch, Schema.GenericStringSchemaColumn);
						parameters.Add("@toReleaseBranch", releaseBuildInfo.ToBranch, Schema.GenericStringSchemaColumn);
						parameters.Add("@fromDate", releaseBuildInfo.FromDate, Schema.GenericDateTimeColumn);
						parameters.Add("@toDate", releaseBuildInfo.ToDate, Schema.GenericDateTimeColumn);
					}
				}
				else
				{
					// from trunk to release branch
					sql = FormattableString.Invariant(
							$@"	select UH_WorkItem from UserTestHeader
								join UserTest on UT_UH = UH_PK
								join BranchRoot on (UT_TargetRepository + '/' like BO_Branch + '/%' and UT_Branch = 'master')
								where {FirstBuildStartedSubquery} > @fromDate
								and UH_CommitTime <= (select min(UH_CommitTime) from UserTestHeader
														join UserTest on UT_UH = UH_PK
														where UT_Branch = @toReleaseBranch)
								and UH_Status in ('{ShelfStatuses.CheckedIn}', '{ShelfStatuses.CheckedInAndNotified}')
								and UH_WorkItem is not null
							union
								select UH_WorkItem from UserTestHeader
								join UserTest on UT_UH = UH_PK
								where UT_Branch = @toReleaseBranch
								and UH_CommitTime <= @toDate
								and UH_Status in ('{ShelfStatuses.CheckedIn}', '{ShelfStatuses.CheckedInAndNotified}')
								and UH_WorkItem is not null");
					parameters.Add("@toReleaseBranch", releaseBuildInfo.ToBranch, Schema.GenericStringSchemaColumn);
					parameters.Add("@fromDate", releaseBuildInfo.FromDate, Schema.GenericDateTimeColumn);
					parameters.Add("@toDate", releaseBuildInfo.ToDate, Schema.GenericDateTimeColumn);
				}
			}
			else
			{
				if (IsReleaseBranch(releaseBuildInfo.FromBranch))
				{
					// from release branch to trunk
					sql = FormattableString.Invariant(
						$@"	select UH_WorkItem from UserTestHeader
							join UserTest on UT_UH = UH_PK
							join BranchRoot on (UT_TargetRepository + '/' like BO_Branch + '/%' and UT_Branch = 'master')
							where {FirstBuildStartedSubquery} > (select min(UH_CommitTime) from UserTestHeader
													join UserTest on UT_UH = UH_PK
													where UT_Branch = @fromReleaseBranch)
							and UH_CommitTime <= @toDate
							and UH_Status in ('{ShelfStatuses.CheckedIn}', '{ShelfStatuses.CheckedInAndNotified}')
							and UH_WorkItem is not null
						except
							select UH_WorkItem from UserTestHeader
							join UserTest on UT_UH = UH_PK
							where UT_Branch = @fromReleaseBranch
							and UH_CommitTime <= @fromDate
							and UH_Status in ('{ShelfStatuses.CheckedIn}', '{ShelfStatuses.CheckedInAndNotified}')
							and UH_WorkItem is not null");
					parameters.Add("@fromReleaseBranch", releaseBuildInfo.FromBranch, Schema.GenericStringSchemaColumn);
					parameters.Add("@fromDate", releaseBuildInfo.FromDate, Schema.GenericDateTimeColumn);
					parameters.Add("@toDate", releaseBuildInfo.ToDate, Schema.GenericDateTimeColumn);
				}
				else
				{
					// along trunk
					sql = FormattableString.Invariant(
						$@"select UH_WorkItem from UserTestHeader
						join UserTest on UT_UH = UH_PK
						join BranchRoot on (UT_TargetRepository + '/' like BO_Branch + '/%' and UT_Branch = 'master')
						where {FirstBuildStartedSubquery} > @fromdate
						and UH_CommitTime <= @toDate
						and UH_Status in ('{ShelfStatuses.CheckedIn}', '{ShelfStatuses.CheckedInAndNotified}')
						and UH_WorkItem is not null");
					parameters.Add("@fromDate", releaseBuildInfo.FromDate, Schema.GenericDateTimeColumn);
					parameters.Add("@toDate", releaseBuildInfo.ToDate, Schema.GenericDateTimeColumn);
				}
			}

			var workItemPKs = new List<Guid>();
			using (var crikeyConnection = DbConnectionCrikey.GetAutoTesterUserTestsConnection())
			using (var command = crikeyConnection.Command(sql))
			{
				command.AddParameters(parameters);
				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						workItemPKs.Add((Guid)reader["UH_WorkItem"]);
					}
				}
			}

			var patchedWorkItemsSubQuery = new ZDBOnlyQuery(typeof(NewWorkItem)) { AllowTableValuedParameters = true };
			patchedWorkItemsSubQuery.AddToFilter(WorkItemSchema.PK, workItemPKs.ToArray());
			return patchedWorkItemsSubQuery;
		}

		public class ReleaseBuildInfo
		{
			public DateTime? FromDate { get; set; }
			public string FromBranch { get; set; }
			public DateTime? ToDate { get; set; }
			public string ToBranch { get; set; }
		}

		[SuppressMessage("Usage", "CW1107", Justification = "Querying non-CW1 database, no entities available")]
		public ReleaseBuildInfo GetReleaseBuildInfo(Version fromVersion, Version toVersion = null)
		{
			var fromVersionHierarchyId = fromVersion.ToSqlHierarchyId();
			var toVersionHierarchyId = toVersion != null ? toVersion.ToSqlHierarchyId() : SqlHierarchyId.Null;

			DateTime? fromDate = null;
			string fromBranch = null;
			DateTime? toDate = null;
			string toBranch = null;

			var key = $"{fromVersion}:{toVersion}";

			return releaseBuildInfoLookup.GetOrAdd(key, () =>
			{
				using (var crikeyConnection = DbConnectionCrikey.GetAutoTesterUserTestsConnection())
				using (var command = crikeyConnection.Command("XT_GetReleaseBuildInfo"))
				{
					command.CommandType = CommandType.StoredProcedure;
					command.AddUdtParameter("@fromVersionNumber", "HierarchyId", fromVersionHierarchyId);
					if (toVersion != null)
					{
						command.AddUdtParameter("@toVersionNumber", "HierarchyId", toVersionHierarchyId);
					}

					command.AddParameter("@deploymentConfiguration1", SqlDbType.NVarChar, IBPArchiveDeploymentConfiguration);
					command.AddParameter("@deploymentConfiguration2", SqlDbType.NVarChar, IBPMasterPackageDeploymentConfiguration);

					using (var reader = command.ExecuteReader())
					{
						if (reader.Read() && SqlHierarchyId.Parse(reader[BR_VersionNumberColumn].ToString()) == fromVersionHierarchyId)
						{
							fromDate = reader["BuildStarted"] as DateTime?;
							fromBranch = reader["UT_Branch"] as string;

							if (reader.Read() && SqlHierarchyId.Parse(reader[BR_VersionNumberColumn].ToString()) == toVersionHierarchyId)
							{
								toDate = reader["BuildStarted"] as DateTime?;
								toBranch = reader["UT_Branch"] as string;
							}
						}
					}
				}
				return new ReleaseBuildInfo { FromDate = fromDate, FromBranch = fromBranch, ToDate = toDate, ToBranch = toBranch };
			});
		}

		bool IsReleaseBranch(string branch)
		{
			return branch.IndexOf("releases/", StringComparison.OrdinalIgnoreCase) > -1;
		}

		public bool IsPatchedTo(NewWorkItem workItem, ReleaseBuild releaseBuild)
		{
			return IsPatchedTo(workItem, releaseBuild.VersionNumber.ToVersion());
		}

		public bool IsPatchedTo(NewWorkItem workItem, Version releaseBuildVersion)
		{
			var releaseBuildInfo = GetReleaseBuildInfo(releaseBuildVersion);

			if (releaseBuildInfo.FromBranch is null)
			{
				return false;
			}

			var rbDate = releaseBuildInfo.FromDate ?? SqlDateTime.MinValue.Value;
			var rbBranch = releaseBuildInfo.FromBranch;

			using var crikeyConnection = DbConnectionCrikey.GetAutoTesterUserTestsConnection();

			if (!IsReleaseBranch(rbBranch))
			{
				using (var command = crikeyConnection.Command(FormattableString.Invariant(
					$@"select UH_WorkItem from UserTestHeader
							join UserTest on UT_UH = UH_PK
							join BranchRoot on (UT_TargetRepository + '/' like BO_Branch + '/%' and UT_Branch = 'master')
							where UH_WorkItem = @workItem
							group by UH_WorkItem
							having max(UH_CommitTime) <= @rbDate")))
				{
					command.AddParameter("@rbDate", SqlDbType.DateTime, rbDate);
					command.AddParameter("@workItem", SqlDbType.UniqueIdentifier, workItem.PK.ToGuid());
					return command.ExecuteScalar() != null;
				}
			}
			else
			{
				using (var command = crikeyConnection.Command(FormattableString.Invariant(
					$@" select UH_WorkItem from UserTestHeader
							join UserTest on UT_UH = UH_PK
							where UT_Branch = @releaseBranch
							and UH_WorkItem = @workItem
							group by UH_WorkItem
							having max(UH_CommitTime) <= @rbDate
						  union
							select UH_WorkItem from UserTestHeader
							join UserTest on UT_UH = UH_PK
							join BranchRoot on (UT_TargetRepository + '/' like BO_Branch + '/%' and UT_Branch = 'master')
							where UH_WorkItem = @workItem
							group by UH_WorkItem
							having max(UH_CommitTime) <= (select min(UH_CommitTime) from UserTestHeader
															join UserTest on UT_UH = UH_PK
															where UT_Branch = @releaseBranch)")))
				{
					command.AddParameter(ZSqlParameter.New("@releaseBranch", rbBranch, Schema.GenericStringSchemaColumn));
					command.AddParameter(ZSqlParameter.New("@rbDate", rbDate, Schema.GenericDateTimeColumn));
					command.AddParameter("@workItem", SqlDbType.UniqueIdentifier, workItem.PK.ToGuid());
					return command.ExecuteScalar() != null;
				}
			}
		}

		public bool IsCargoWiseOneChange(NewWorkItem workItem)
		{
			using (var crikeyConnection = DbConnectionCrikey.GetAutoTesterUserTestsConnection())
			{
				if (crikeyConnection == null) { return false; }
				using (var command = crikeyConnection.Command(FormattableString.Invariant(
			$@"select top 1 1 from UserTestHeader
			join UserTest on UT_UH = UH_PK
			where
				UH_WorkItem = @workItem
				and UH_Type = 'SCH'
				and UH_Status in ('{ShelfStatuses.CheckedIn}', '{ShelfStatuses.CheckedInAndNotified}')
				and (
					exists (select * from BranchRoot where UT_TargetRepository + '/' like BO_Branch + '/%' or UT_Branch + '/' like BO_Branch + '/%')
					or exists (select * from ReleaseBranch where UT_Branch + '/' like RH_Branch + '/CW%'))")))
				{
					command.AddParameter("@workItem", SqlDbType.UniqueIdentifier, workItem.PK.ToGuid());
					return command.ExecuteScalar() != null;
				}
			}
		}

		public ReleaseBuild GetReleaseBuild(ZGuid taskPK)
		{
			Version versionNumber = null;
			using (var crikeyConnection = DbConnectionCrikey.GetAutoTesterUserTestsConnection())
			{
				using (var command = crikeyConnection.Command(FormattableString.Invariant(
				$@"select BR_VersionNumber
				from UserTestHeader
				join UserTest on UT_UH = UH_PK
				join BuildJob on BJ_UT = UT_PK and BJ_Configuration = 'RELEASE'
				join BuildResult on BR_BJ = BJ_PK
				join DeploymentJob on DJ_UT = BJ_UT 
				join DeploymentTarget on DT_PK = DJ_DT and DT_DeploymentConfiguration in ('{IBPArchiveDeploymentConfiguration}', '{IBPMasterPackageDeploymentConfiguration}')
				where UH_P9 = @taskPK")))
				{
					command.AddParameter("@taskPK", SqlDbType.UniqueIdentifier, taskPK.ToGuid());
					using (var reader = command.ExecuteReader())
					{
						if (reader.Read())
						{
							versionNumber = SqlHierarchyId.Parse(reader[BR_VersionNumberColumn].ToString()).ToVersion();
						}
					}
				}

				if (versionNumber == null)
				{
					DateTime commitTime;
					string branch;
					using (var command = crikeyConnection.Command("SELECT top 1 UH_CommitTime, UT_Branch FROM UserTestHeader JOIN UserTest ON UT_UH = UH_PK WHERE UH_P9 = @taskPK ORDER BY UH_ProcessingFinished DESC"))
					{
						command.AddParameter("@taskPK", SqlDbType.UniqueIdentifier, taskPK.ToGuid());
						using (var reader = command.ExecuteReader())
						{
							if (!reader.Read())
							{
								return null;
							}

							commitTime = (reader[UH_CommitTimeColumn] as DateTime?) ?? SqlDateTime.MinValue.Value;
							branch = (string)reader[UT_BranchColumn];
						}
					}

					if (commitTime != SqlDateTime.MinValue.Value)
					{
						var branchClause = IsReleaseBranch(branch) ? "= @releaseBranch" : "= 'master'";

						using (var command = crikeyConnection.Command(FormattableString.Invariant(
							$@"select top 1 BR_VersionNumber
				from UserTestHeader
				join UserTest on UT_UH = UH_PK
				join BuildJob on BJ_UT = UT_PK and BJ_Configuration = 'RELEASE'
				join BuildResult on BR_BJ = BJ_PK
				join DeploymentJob on DJ_UT = BJ_UT 
				join DeploymentTarget on DT_PK = DJ_DT and DT_DeploymentConfiguration in ('{IBPArchiveDeploymentConfiguration}', '{IBPMasterPackageDeploymentConfiguration}')
				where {FirstBuildStartedSubquery} > @commitTime
				and UT_Branch {branchClause}
				order by {FirstBuildStartedSubquery}")))
						{
							command.AddParameter("@commitTime", SqlDbType.DateTime, commitTime);
							if (IsReleaseBranch(branch))
							{
								command.AddParameter("@releaseBranch", SqlDbType.VarChar, 128, branch);
							}
							using (var reader = command.ExecuteReader())
							{
								if (!reader.Read())
								{
									return null;
								}

								versionNumber = SqlHierarchyId.Parse(reader[BR_VersionNumberColumn].ToString()).ToVersion();
							}
						}
					}
				}
			}

			if (versionNumber != null)
			{
				var query = new ZQuery(ReleaseBuildSchema.HL_MajorVersion, versionNumber.Major);
				query.AddToFilter(ReleaseBuildSchema.HL_MinorVersion, versionNumber.Minor);
				query.AddToFilter(ReleaseBuildSchema.HL_Release, versionNumber.Build);
				query.AddToFilter(ReleaseBuildSchema.HL_Patch, SQLComparisonOperator.GreaterThanOrEqualTo, versionNumber.Revision);
				query.OrderBy = ReleaseBuildSchema.HL_Patch.Name + OrderByClause.Ascending;

				var releaseBuild = factory.LoadTop1<ReleaseBuild>(query);
				return releaseBuild;
			}

			return null;
		}

		string FirstBuildStartedSubquery => $"(select min(BJ_Started) from BuildJob join UserTest on UT_PK = BJ_UT where UT_UH = UH_PK)";

		const string UH_CommitTimeColumn = "UH_CommitTime";
		const string UT_BranchColumn = "UT_Branch";
		const string BR_VersionNumberColumn = "BR_VersionNumber";

		readonly Dictionary<string, ReleaseBuildInfo> releaseBuildInfoLookup = new();

		public const string IBPArchiveDeploymentConfiguration = @"archive:\\cw1datfiles.wtg.zone\IBPLatestBuildArchive";
		public const string IBPMasterPackageDeploymentConfiguration = "IBPMasterPackage";
	}

	[Serializable]
	[ExceptionVisibility(ExceptionVisibility.User)]
	public class ReleaseBuildContentException : Exception
	{
		public ReleaseBuildContentException(string message)
			: base(message)
		{
		}

#if NETFRAMEWORK
		protected ReleaseBuildContentException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif
	}
}
