using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Business
{
	public class AcceptabilityBandSqlBuilderParameters
	{
		AcceptabilityBandSqlBuilderParameters(ZGuid acceptabilityBandPk, params string[] unionClauses)
		{
			additionalUnionClauses = unionClauses;
			ComponentOverridePKs = System.Array.Empty<ZGuid>();
			ReleaseGateFeedingComponentPKs = System.Array.Empty<ZGuid>();
			AcceptabilityBandPk = acceptabilityBandPk;
		}

		public AcceptabilityBandSqlBuilderParameters(BMComponentAcceptabilityBand band, AcceptabilityBandVisualizationOption shouldFilterByReleaseGroup = AcceptabilityBandVisualizationOption.Default,
			AcceptabilityBandVisualizationOption shouldFilterBySection = AcceptabilityBandVisualizationOption.Default, AcceptabilityBandBoundaryValues overriddenBoundaryValues = null, params string[] unionClauses)
			: this(band.PK, unionClauses)
		{
			ShouldFilterByReleaseGroup = band.GetOverriddenShouldFilterByReleaseGroup(shouldFilterByReleaseGroup);
			ShouldFilterBySection = band.GetOverriddenShouldFilterBySection(shouldFilterBySection);
			BoundaryValues = overriddenBoundaryValues ?? band.BoundaryValues;
		}

		public AcceptabilityBandSqlBuilderParameters(
			ZGuid acceptabilityBandPk,
			bool shouldFilterByReleaseGroup,
			bool shouldFilterBySection,
			AcceptabilityBandBoundaryValues boundaryValues,
			ZGuid releaseGroupPK,
			int maximumItems
			)
			: this(acceptabilityBandPk)
		{
			ShouldFilterByReleaseGroup = shouldFilterByReleaseGroup;
			ShouldFilterBySection = shouldFilterBySection;
			BoundaryValues = boundaryValues;
			ReleaseGroupPK = releaseGroupPK;
			MaximumItems = maximumItems;
		}

		readonly string[] additionalUnionClauses;

		public bool CacheQuery { get; set; }
		public ZGuid ReleaseGroupPK { get; set; }
		public ZGuid AdditionalWorkflowPK { get; set; }

		[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
		public ZGuid[] ComponentOverridePKs { get; set; }

		[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
		public ZGuid[] ReleaseGateFeedingComponentPKs { get; set; }
		public TagMagnitude Tag { get; set; }
		public bool RunAsGoldenRule { get; set; }
		public HashSet<ZGuid> WorkflowPKs { get; set; }
		public ZDateTime WorkflowPksActualTime { get; set; }
		public bool ShouldFilterBySection { get; set; }
		public bool ShouldFilterByReleaseGroup { get; set; }
		public AcceptabilityBandBoundaryValues BoundaryValues { get; set; }
		public ZGuid AcceptabilityBandPk { get; set; }
		public ZSqlParameterCollection SqlParameters { get; set; }
		public bool IsQueryForValidation { get; set; }
		public int MaximumItems { get; set; }
		public bool AreValid { get; set; } = true;

		#region Additional Filter

		internal ZQuery GetAdditionalProcessHeaderFilter()
		{
			if (Tag != null)
			{
				var query = new ZDBOnlyQuery(typeof(ProcessHeader));

				var workflowQuery = new ZDBOnlyQuery(typeof(ProcessHeader));
				var workflowLinkSubQuery = new ZDBOnlySubQuery(typeof(TagLink), TagLinkSchema.TGL_ParentId);
				workflowLinkSubQuery.AddToFilter(TagLinkSchema.TGL_TGM_Magnitude, Tag.PK);
				workflowLinkSubQuery.AddToFilter(TagLinkSchema.TGL_ParentId, SQLComparisonOperator.NotEqual, ZGuid.Empty);

				var jobHeaderSubQuery = new ZDBOnlySubQuery(typeof(ProcessHeader), ProcessHeaderSchema.PK);
				var jobLinkSubQuery = new ZDBOnlySubQuery(typeof(TagLink), TagLinkSchema.TGL_ParentId);
				jobLinkSubQuery.AddToFilter(TagLinkSchema.TGL_TGM_Magnitude, Tag.PK);
				jobLinkSubQuery.AddToFilter(TagLinkSchema.TGL_ParentId, SQLComparisonOperator.NotEqual, ZGuid.Empty);

				var workflowNotInLinkSubQuery = new ZDBOnlySubQuery(typeof(TagLink), TagLinkSchema.TGL_ParentId, notIn: true);
				var workflowNotInMagnitudeSubQuery = new ZDBOnlySubQuery(typeof(TagMagnitude), TagMagnitudeSchema.PK);
				workflowNotInMagnitudeSubQuery.AddToFilter(TagMagnitudeSchema.TGM_TGD_Tag, Tag.TGM_TGD_Tag);
				workflowNotInLinkSubQuery.AddSubQuery(TagLinkSchema.TGL_TGM_Magnitude, workflowNotInMagnitudeSubQuery, JoinCondition.And);

				jobHeaderSubQuery.AddSubQuery(ProcessHeaderSchema.FH_FH_ParentHeader, jobLinkSubQuery, JoinCondition.And);
				jobHeaderSubQuery.AddSubQuery(ProcessHeaderSchema.PK, workflowNotInLinkSubQuery, JoinCondition.And);

				workflowLinkSubQuery.AddAsUnionQuery(jobHeaderSubQuery);
				workflowQuery.AddSubQuery(ProcessHeaderSchema.PK, workflowLinkSubQuery, JoinCondition.And);

				query.AddToFilter(workflowQuery);

				if (ComponentOverridePKs.Length > 0)
				{
					query.AddToFilter(ProcessHeaderSchema.FH_FC_CurrentComponent, ComponentOverridePKs);
				}

				return query;
			}
			else
			{
				return new ZQuery();
			}
		}

		#endregion

		#region Union Clauses

		internal string GetAdditionalUnionClauses()
		{
			var result = new StringBuilder();

			foreach (var clause in additionalUnionClauses)
			{
				result.AppendFormat(CultureInfo.InvariantCulture, (NoResString)"{0} UNION {0}{1}", System.Environment.NewLine, clause); // SQL constant
			}

			return result.ToString();
		}

		#endregion

		#region Object Overrides

		[SuppressMessage("Microsoft.Maintainability", "CA1502", Justification = "The complexity here comes from the use of && operators which effectively doubles our complexity, however the code itself reads extremely easily.")]
		public override bool Equals(object obj)
		{
			var other = obj as AcceptabilityBandSqlBuilderParameters;

			return other != null
				&& AcceptabilityBandPk == other.AcceptabilityBandPk
				&& CacheQuery == other.CacheQuery
				&& ReleaseGroupPK == other.ReleaseGroupPK
				&& AdditionalWorkflowPK == other.AdditionalWorkflowPK
				&& DoTagsMatch(other)
				&& additionalUnionClauses.OrderBy(s => s).SequenceEqual(other.additionalUnionClauses.OrderBy(s => s))
				&& ComponentOverridePKs.OrderBy(x => x).SequenceEqual(other.ComponentOverridePKs.OrderBy(x => x))
				&& RunAsGoldenRule == other.RunAsGoldenRule
				&& DoWorkflowPKsMatch(other)
				&& DoVisualizationSettingsMatch(other)
				&& BoundaryValues.Equals(other.BoundaryValues)
				&& MaximumItems == other.MaximumItems;
		}

		bool DoWorkflowPKsMatch(AcceptabilityBandSqlBuilderParameters other)
		{
			return (WorkflowPKs == null && other.WorkflowPKs == null) || (WorkflowPKs != null && other.WorkflowPKs != null && WorkflowPKs.SetEquals(other.WorkflowPKs));
		}

		bool DoTagsMatch(AcceptabilityBandSqlBuilderParameters other)
		{
			return ((Tag == null && other.Tag == null) || (Tag != null && other.Tag != null && Tag.PK == other.Tag.PK));
		}

		bool DoVisualizationSettingsMatch(AcceptabilityBandSqlBuilderParameters other)
		{
			return ShouldFilterBySection == other.ShouldFilterBySection && ShouldFilterByReleaseGroup == other.ShouldFilterByReleaseGroup;
		}

		public override int GetHashCode()
		{
			return Hash(new object[] { CacheQuery, ReleaseGroupPK, AdditionalWorkflowPK, Tag != null ? Tag.PK : ZGuid.Empty }.Concat(additionalUnionClauses.OrderBy(s => s)).Concat(ComponentOverridePKs.OrderBy(x => x).Cast<object>()));
		}

		static int Hash(IEnumerable<object> things)
		{
			return (int)things.Aggregate((o1, o2) => o1.GetHashCode() ^ o2.GetHashCode());
		}

		#endregion

		public AcceptabilityBandSqlBuilderParameters Clone()
		{
			return new AcceptabilityBandSqlBuilderParameters(AcceptabilityBandPk)
			{
				CacheQuery = CacheQuery,
				ReleaseGroupPK = ReleaseGroupPK,
				AdditionalWorkflowPK = AdditionalWorkflowPK,
				ComponentOverridePKs = ComponentOverridePKs.ToArray(),
				Tag = Tag,
				RunAsGoldenRule = RunAsGoldenRule,
				WorkflowPKs = WorkflowPKs,
				WorkflowPksActualTime = WorkflowPksActualTime,
				ShouldFilterBySection = ShouldFilterBySection,
				ShouldFilterByReleaseGroup = ShouldFilterByReleaseGroup,
				BoundaryValues = BoundaryValues,
				SqlParameters = SqlParameters,
				IsQueryForValidation = IsQueryForValidation,
				MaximumItems = MaximumItems,
			};
		}
	}
}
