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
	public class DependentWorkflowsFilter : ModuleGuidPivotFilter
	{
		protected DependentWorkflowsFilter(FilterCategory category, ModuleFilterCollection parentCollection)
			: base(category, parentCollection)
		{
		}

		public DependentWorkflowsFilter(ZString description, IBusinessObjectCollection list)
			: base(description, ModuleIDs.ProcessHeader, ProcessHeaderLinkSchema.FP_FH_HeaderTo, ProcessHeaderLinkSchema.FP_FH_HeaderFrom, list, typeof(ProcessHeader), typeof(ProcessHeaderLink), new ZQuery(ProcessHeaderLinkSchema.FP_LinkType, ProcessHeaderLinkTypeList.Codes.Dependency))
		{
			SetDefaultProperties();
		}

		public DependentWorkflowsFilter(ZString description, GetList listDelegate)
			: base(description, ModuleIDs.ProcessHeader, ProcessHeaderLinkSchema.FP_FH_HeaderTo, ProcessHeaderLinkSchema.FP_FH_HeaderFrom, listDelegate, typeof(ProcessHeader), typeof(ProcessHeaderLink), new ZQuery(ProcessHeaderLinkSchema.FP_LinkType, ProcessHeaderLinkTypeList.Codes.Dependency))
		{
			SetDefaultProperties();
		}

		void SetDefaultProperties()
		{
			MultilingualDescription = DefaultDescription;
		}

		static MultilingualString DefaultDescription => ResString.GetMultilingualString("1CE40097-A48A-46FC-9BF2-5EFF992050A5", "Dependent Workflows");

		protected override FilterCategory DefaultCategory => FilterCategories.Other;
	}
}
