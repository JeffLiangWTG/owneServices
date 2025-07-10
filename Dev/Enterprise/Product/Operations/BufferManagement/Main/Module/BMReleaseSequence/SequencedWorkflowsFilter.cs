using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.BufferManagement.Module
{
	public class SequencedWorkflowsFilter : ModuleTextFilter
	{
		public SequencedWorkflowsFilter()
			: base(ProcessHeader.ModuleFilterConstants.SequencedWorkflows, SequencedWorkflowsQuery, new SequencedWorkflowsList())
		{
			MultilingualDescription = ResString.GetMultilingualString("BufferManagement|ProcessHeaderFilterBusinessObject|SequencedWorkflows", "Sequenced Workflows");
			Category = FilterCategories.Other;
		}

		static ZQuery SequencedWorkflowsQuery(ZString value)
		{
			if (value == SequencedWorkflowsList.Codes.DirectlySequenced)
			{
				return new ZDBOnlyQuery(typeof(ProcessHeader)).AddFilterAndZSQLParameterCollection(
				$@"FH_PK in
				(
					{GetDirectSequenceSQL()}
				)", null);
			}

			if (value == SequencedWorkflowsList.Codes.IndirectlySequenced)
			{
				return new ZDBOnlyQuery(typeof(ProcessHeader)).AddFilterAndZSQLParameterCollection(
				$@"FH_PK in
				(
					{GetIndirectSequenceSQL()}
				)", null);
			}

			if (value == SequencedWorkflowsList.Codes.BothDirectlyAndIndirectlySequenced)
			{
				return new ZDBOnlyQuery(typeof(ProcessHeader)).AddFilterAndZSQLParameterCollection(
				$@"FH_PK in
				(
					{GetDirectSequenceSQL()}

					union all

					{GetIndirectSequenceSQL()}
				)", null);
			}

			if (value == SequencedWorkflowsList.Codes.NotSequenced)
			{
				return new ZDBOnlyQuery(typeof(ProcessHeader)).AddFilterAndZSQLParameterCollection(
				$@"FH_PK not in
				(
					{GetDirectSequenceSQL()}

					union all

					{GetIndirectSequenceSQL()}
				)", null);
			}

			return new ZQuery();
		}

		static string GetDirectSequenceSQL()
		{
			return $@"select FH_PK from dbo.ProcessHeader where FH_PK in ({ActiveSequenceSQL})";
		}

		static string GetIndirectSequenceSQL()
		{
			return $@"
			{GetDescendentsSQL()}

			union all

			{GetParentHeaderActiveSequenceSQL()}

			union all

			{GetParentHeaderDescendentsSQL()}

			union all

			select FH_PK from dbo.ProcessHeader
			where FH_FH_ParentHeader in ({GetParentHeaderDescendentsSQL()})

			union all

			select FH_PK from dbo.ProcessHeader
			where FH_FH_ParentHeader in ({GetDescendentsSQL()})";
		}

		static string GetDescendentsSQL()
		{
			return $@"
select descendents.FH_PK from dbo.ProcessHeader
cross apply dbo.GetWorkflowAndJobLevelWorkflowDescendentHierarchy
(
	FH_PK, {BMConstants.MaximumReleaseSequenceParentWorkflowsDepth}
) as descendents
where descendents.FH_PK not in ({ActiveSequenceSQL})
and descendents.RootPK in ({ActiveSequenceSQL})";
		}

		static string GetParentHeaderActiveSequenceSQL()
		{
			return $@"
select FH_PK from dbo.ProcessHeader
where FH_FH_ParentHeader in ({ActiveSequenceSQL})";
		}

		static string GetParentHeaderDescendentsSQL()
		{
			return $@"
select descendents.FH_PK from dbo.ProcessHeader
cross apply dbo.GetWorkflowAndJobLevelWorkflowDescendentHierarchy
(
	FH_PK, {BMConstants.MaximumReleaseSequenceParentWorkflowsDepth}
) as descendents
where descendents.Parent is not null
and descendents.RootPK in
(
	{GetParentHeaderActiveSequenceSQL()}
)";
		}

		const string ActiveSequenceSQL = @"
select BMI_FH_ProcessHeader from dbo.BMReleaseSequenceItem where BMI_BMR_Sequence in
(
	select BMR_PK from dbo.BMReleaseSequence where BMR_IsActive = 1
)";
	}
}
