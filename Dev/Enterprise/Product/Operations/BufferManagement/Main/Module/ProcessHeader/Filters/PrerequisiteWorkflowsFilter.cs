using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Module
{
	public class PrerequisiteWorkflowsFilter : ModuleGuidPivotFilter
	{
		protected PrerequisiteWorkflowsFilter(FilterCategory category, ModuleFilterCollection parentCollection)
			: base(category, parentCollection)
		{
		}

		public PrerequisiteWorkflowsFilter(ZString description, IBusinessObjectCollection list)
			: base(description, ModuleIDs.ProcessHeader, ProcessHeaderLinkSchema.FP_FH_HeaderFrom, ProcessHeaderLinkSchema.FP_FH_HeaderTo, list, typeof(ProcessHeader), typeof(ProcessHeaderLink), new ZQuery(ProcessHeaderLinkSchema.FP_LinkType, ProcessHeaderLinkTypeList.Codes.Dependency))
		{
			SetDefaultProperties();
		}

		public PrerequisiteWorkflowsFilter(ZString description, GetList listDelegate)
			: base(description, ModuleIDs.ProcessHeader, ProcessHeaderLinkSchema.FP_FH_HeaderFrom, ProcessHeaderLinkSchema.FP_FH_HeaderTo, listDelegate, typeof(ProcessHeader), typeof(ProcessHeaderLink), new ZQuery(ProcessHeaderLinkSchema.FP_LinkType, ProcessHeaderLinkTypeList.Codes.Dependency))
		{
			SetDefaultProperties();
		}

		void SetDefaultProperties()
		{
			MultilingualDescription = DefaultDescription;
		}

		static MultilingualString DefaultDescription => ResString.GetMultilingualString("07AC13F1-6595-4BC0-9D5E-CDDE5CA33FB1", "Prerequisite Workflows");

		protected override FilterCategory DefaultCategory => FilterCategories.Other;
	}
}
