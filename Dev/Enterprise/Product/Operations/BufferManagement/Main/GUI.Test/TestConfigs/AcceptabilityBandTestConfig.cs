using CargoWise.EntityFramework;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.BufferManagement.GUI.Test
{
	public class AcceptabilityBandTestConfig : TagsTestConfig
	{
		protected AcceptabilityBandTestConfig(BusinessObjectFactory factory, string workflowType, bool shouldUseExistingSystem)
			: base(factory, workflowType, shouldUseExistingSystem)
		{
			PlatinumRule = BMSTestHelper.CreateAcceptabilityBand(Buffer, 0, 1, 2, 3, 4, 5, "Platinum Golden Rule", type: AcceptabilityBandTypes.Codes.Count);
			RedRule = BMSTestHelper.CreateAcceptabilityBand(Buffer, 0, 10, 20, 30, 40, 50, "Red Golden Rule", type: AcceptabilityBandTypes.Codes.Aggregate, sql: @"
				select distinct convert(decimal(10,2), coalesce(Value, 0)) Value, FC_PK Component, GG_PK ReleaseGroup 
				from dbo.BMComponent
				full outer join dbo.GlbGroup on 1 = 1
				left join(
					select 100 * cast(headersByTag.TotalHeaders as decimal) / cast(headersInComponent.TotalHeaders as decimal) as Value, headersByTag.Component, headersByTag.ReleaseGroup 
					from (
						select count(*) TotalHeaders, FH_FC_CurrentComponent as Component, FH_GG_ReleaseGroup ReleaseGroup from Workflows
						group by FH_FC_CurrentComponent, FH_GG_ReleaseGroup
					) headersByTag
					left join (
						select count(*) TotalHeaders, FH_FC_CurrentComponent as Component, FH_GG_ReleaseGroup ReleaseGroup from dbo.ProcessHeader 
						group by FH_FC_CurrentComponent, FH_GG_ReleaseGroup
					) headersInComponent on headersByTag.Component = headersInComponent.Component and headersByTag.ReleaseGroup = headersInComponent.ReleaseGroup
				) headersCount on Component = FC_PK and ReleaseGroup = GG_PK
				");

			FilterStripsTestHelper.AddFilterStrips(PlatinumRule.FilterRule,
			new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.TagMagnitude,
				FilterStripValueSetter = f => ((ModuleGuidFilter)f).Property = PlatinumTag.PK,
			});

			FilterStripsTestHelper.AddFilterStrips(RedRule.FilterRule,
			new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.TagMagnitude,
				FilterStripValueSetter = f => ((ModuleGuidFilter)f).Property = RedTag.PK,
			});
		}

		internal static AcceptabilityBandTestConfig Create(BusinessObjectFactory factory, string workflowType, bool shouldUseExistingSystem)
		{
			return new AcceptabilityBandTestConfig(factory, workflowType, shouldUseExistingSystem);
		}

		public BMComponentAcceptabilityBand PlatinumRule { get; private set; }
		public BMComponentAcceptabilityBand RedRule { get; private set; }
	}
}
