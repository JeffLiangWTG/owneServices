using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Business
{
	class SimpleQueryBoardLoader
	{
		internal static ProcessTask[] LoadTasks(BMBoardSection boardSection, BMBoardSectionChannel[] channels, ZQuery workflowSectionFilter, ZQuery taskSectionFilter)
		{
			bool IsSupportedChannelType(BMBoardSectionChannel channel)
			{
				switch (channel.MSC_ChannelType)
				{
					case ChannelTypeList.Codes.Capability:
					case ChannelTypeList.Codes.Resource:
					case ChannelTypeList.Codes.ReleaseSchedulerChannels:
					case ChannelTypeList.Codes.Tag:
					case ChannelTypeList.Codes.Group:
					case ChannelTypeList.Codes.CurrentUser:
						return true;
					default:
						return false;
				}
			}

			bool IsSupportedSecondaryChannelType(BMBoardSectionChannel channel)
			{
				switch (channel.MSC_ChannelType)
				{
					case ChannelTypeList.Codes.NotChanneled:
						return true;
					default:
						return false;
				}
			}

			var factory = boardSection.Factory;
			var allComponentPKs = boardSection.AllShownComponentPKs.ToArray();
			var splitChannels = channels.Split(c => c.IsPrimaryAxis);
			var primaryChannels = splitChannels.MatchingSet;
			var secondaryChannels = splitChannels.NonMatchingSet;
			var boardQueryInfo = $@"
-- Simple Board Query
-- Database: {Db.DatabaseName}
-- Board:    {boardSection.Board.MB_Name}
-- Section:  {boardSection.SectionName}";

			if (!ExperimentalSettingsProvider.SimpleBoardQueryEnabled(boardSection.Board) ||
				allComponentPKs.Length == 0 ||
				channels.Length == 0 ||
				!primaryChannels.All(IsSupportedChannelType) ||
				secondaryChannels.Any(n => !IsSupportedSecondaryChannelType(n)))
			{
				return WorkflowLoader.LoadTasks(boardSection, channels, workflowSectionFilter, taskSectionFilter);
			}

			var staffPKs = primaryChannels.Where(c => c.MSC_ChannelType == ChannelTypeList.Codes.Resource || c.MSC_ChannelType == ChannelTypeList.Codes.ReleaseSchedulerChannels).Select(c => c.MSC_ParentID).ToList();
			if (primaryChannels.Any(c => c.MSC_ChannelType == ChannelTypeList.Codes.CurrentUser))
			{
				staffPKs.Add(GlbStaff.CurrentUser.PK);
			}

			var haveStaff = staffPKs.Count > 0;
			var capabilityPKs = primaryChannels.Where(c => c.MSC_ChannelType == ChannelTypeList.Codes.Capability).Select(c => c.MSC_ParentID).ToList();
			var groupPKs = primaryChannels.Where(c => c.MSC_ChannelType == ChannelTypeList.Codes.Group).Select(c => c.MSC_ParentID).Distinct().ToList();
			var haveGroups = groupPKs.Count > 0;

			if (haveStaff)
			{
				var capabilityPivots = factory.Load<GlbResourceCapabilityPivot>(new ZQuery(GlbResourceCapabilityPivotSchema.G5_GS_Resource, staffPKs));
				capabilityPKs.AddRange(capabilityPivots.Select(c => c.G5_G4_Capability));
			}

			if (haveGroups)
			{
				var groupPivots = factory.Load<GlbGroupLink>(new ZQuery(GlbGroupLinkSchema.GK_GG, groupPKs));
				staffPKs.AddRange(groupPivots.Select(s => s.GK_GG));
			}

			const string componentsParameterName = "@ComponentPKs";
			const string currentComponentColumnName = ProcessTasksSchema.Constants.P9_FC_CurrentComponent;
			var componentsAndTaskFilter = $"{currentComponentColumnName} IN (SELECT value FROM {componentsParameterName}) AND {taskSectionFilter.FilterString}";
			var componentsParameter = new ZSqlParameterCollection();
			componentsParameter.Add(ZSqlParameter.New(componentsParameterName, allComponentPKs, ProcessTasksSchema.P9_FC_CurrentComponent, isTableValued: true));
			componentsParameter.Add(ZSqlParameter.New("@BoardInfo", boardQueryInfo, BMBoardSchema.MB_Description));
			componentsParameter.AddRange(taskSectionFilter.Params);

			var unionAllQuery = new UnionZDBOnlyQuery(typeof(ProcessTask));
			unionAllQuery.IsNoLock = true;
			unionAllQuery.AddFilter(componentsAndTaskFilter, componentsParameter);

			if (boardSection.AreWorkflowFiltersSpecified)
			{
				unionAllQuery.AddFilter($"{ProcessTasksSchema.Constants.P9_FH_ProcessHeader} IN (SELECT {ProcessHeaderSchema.Constants.PK} FROM {ProcessHeaderSchema.Constants.SqlSchemaName}.{ProcessHeaderSchema.Constants.TableName} WHERE {workflowSectionFilter.FilterString})", new ZSqlParameterCollection(workflowSectionFilter.Params));
			}

			if (haveGroups)
			{
				// Group channels load any task with P9_GG_AssignedGroup
				var groupSubQuery = new ZDBOnlyQuery(typeof(ProcessTask));
				AddParameterList(groupSubQuery, groupPKs, "@Group", ProcessTasksSchema.P9_GG_AssignedGroup);
				unionAllQuery.AddSubQuery(groupSubQuery);

				// Group channels also load ProcessHeader's with FH_GG_ReleaseGroup
				const string groupTVPParameterName = "@Groups";
				var groupHeaderSubQuery = new ZDBOnlyQuery(typeof(ProcessTask));
				var groupHeaderParameters = new ZSqlParameterCollection();
				groupHeaderParameters.Add(ZSqlParameter.New(groupTVPParameterName, groupPKs, ProcessHeaderSchema.FH_GG_ReleaseGroup, isTableValued: true));
				var groupHeaderFilter = $"P9_FH_ProcessHeader in (select FH_PK from dbo.ProcessHeader where FH_GG_ReleaseGroup IN (SELECT value FROM {groupTVPParameterName}))";
				groupHeaderSubQuery.AddFilterAndZSQLParameterCollection(groupHeaderFilter, groupHeaderParameters);
				unionAllQuery.AddSubQuery(groupHeaderSubQuery);
			}

			if (haveStaff)
			{
				var staff = factory.Load<GlbStaff>(new ZQuery(GlbStaffSchema.PK, staffPKs));
				var staffSubQuery = new ZDBOnlyQuery(typeof(ProcessTask));
				staffSubQuery.TableIndexHints.Add(new TableIndexHint("NR_RX__P9_FC_CurrentComponent_P9_GS_NKAssignedStaffMember"));
				AddParameterList(staffSubQuery, staff.Select(s => (string)s.GS_Code), "@StaffCode", ProcessTasksSchema.P9_GS_NKAssignedStaffMember);
				unionAllQuery.AddSubQuery(staffSubQuery);
			}

			var haveCapabilities = capabilityPKs.Count > 0;
			if (haveCapabilities)
			{
				var capabilitySubQuery = new ZDBOnlyQuery(typeof(ProcessTask));
				capabilitySubQuery.TableIndexHints.Add(new TableIndexHint("NR_RX__P9_FC_CurrentComponent_P9_G4_RequiredCapability"));
				AddParameterList(capabilitySubQuery, capabilityPKs, "@Capability", ProcessTasksSchema.P9_G4_RequiredCapability);
				unionAllQuery.AddSubQuery(capabilitySubQuery);
			}

			var tagPks = primaryChannels.Where(c => c.MSC_ChannelType == ChannelTypeList.Codes.Tag).Select(c => c.MSC_ParentID).Distinct().ToList();
			var haveTags = tagPks.Count > 0;

			if (haveTags)
			{
				const string tagMagnitudeParameterName = "@TagMagnitudes";
				var parameters = new ZSqlParameterCollection();
				parameters.Add(ZSqlParameter.New(tagMagnitudeParameterName, tagPks, TagLinkSchema.TGL_TGM_Magnitude, isTableValued: true));
				var workflowTagSubQuery = new ZDBOnlyQuery(typeof(ProcessTask));
				var workflowFIlter = $"P9_FH_ProcessHeader in (select TGL_ParentId from dbo.TagLink where TGL_TGM_Magnitude in (select value from {tagMagnitudeParameterName}))";
				workflowTagSubQuery.AddFilterAndZSQLParameterCollection(workflowFIlter, parameters);
				unionAllQuery.AddSubQuery(workflowTagSubQuery);

				var jobHeaderTagSubQuery = new ZDBOnlyQuery(typeof(ProcessTask));
				var jobHeaderFilter = $"P9_FH_ProcessHeader in (select FH_PK from dbo.ProcessHeader where FH_FH_ParentHeader in (select TGL_ParentId from dbo.TagLink where TGL_TGM_Magnitude in (select value from {tagMagnitudeParameterName})))";
				jobHeaderTagSubQuery.AddFilterAndZSQLParameterCollection(jobHeaderFilter, parameters);
				unionAllQuery.AddSubQuery(jobHeaderTagSubQuery);
			}

#if NETFRAMEWORK
			return factory.Load<ProcessTask>(unionAllQuery).DistinctBy(t => t.PK).ToArray();
#else
			return IEnumerableExtensions.DistinctBy(factory.Load<ProcessTask>(unionAllQuery), t => t.PK).ToArray();
#endif
		}

		static void AddParameterList<T>(ZQuery query, IEnumerable<T> parameters, string parameterPrefix, SchemaColumn column)
		{
			var parameterCollection = new ZSqlParameterCollection();
			var builder = new StringBuilder();
			builder.Append(column.Name).Append((NoResString)" IN (");
			var index = 0;
			parameters.OrderBy(p => p)
				.ForEachWithBetween(value =>
			{
				var paramName = parameterPrefix + ++index;
				builder.Append(paramName);
				parameterCollection.Add(paramName, value, column);
			},
			() => builder.Append(" ,"));
			builder.Append(")");

			query.AddFilterAndZSQLParameterCollection(builder.ToString(), parameterCollection);
		}
	}
}
