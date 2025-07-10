using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Module
{
	public class CapabilityFilter : ModuleGuidFilter
	{
		public CapabilityFilter(ZString description, ModuleIdentifier id, IBusinessObjectCollection list, FilterCategory category)
			: base(description, id, EmptyQuery, list)
		{
			Category = category;
			MultilingualDescription = ResString.GetMultilingualString("BufferManagement|ProcessHeaderFilterBusinessObject|Capability", "Capability Required on Any Task");
		}

		static ZQuery EmptyQuery(ZGuid pk)
		{
			throw new InvalidOperationException();
		}

		protected override ZQuery GetQuery()
		{
			if (Property.IsEmpty)
			{
				return new ZQuery();
			}

			var processHeaderQuery = new ZDBOnlyQuery(typeof(ProcessHeader));

			var processTaskSubQuery = new ZDBOnlySubQuery(typeof(ProcessTask), ProcessTasksSchema.P9_FH_ProcessHeader);
			processTaskSubQuery.AddToFilter(ProcessTasksSchema.P9_G4_RequiredCapability, Property);

			if (BMFilterStripsHelper.ShouldOptimiseQueryForWorkflowOnly(this))
			{
				processHeaderQuery.AddSubQuery(processTaskSubQuery, JoinCondition.And);
			}
			else
			{
				var processHeaderSubQuery = new ZDBOnlySubQuery(typeof(ProcessHeader), ProcessHeaderSchema.FH_FH_ParentHeader);
				processHeaderSubQuery.AddSubQuery(processTaskSubQuery, JoinCondition.And);
				processHeaderSubQuery.AddAsUnionQuery(processTaskSubQuery, addAsUnionAll: true);
				processHeaderQuery.AddSubQuery(processHeaderSubQuery, JoinCondition.And);
			}

			return processHeaderQuery;
		}
	}
}
