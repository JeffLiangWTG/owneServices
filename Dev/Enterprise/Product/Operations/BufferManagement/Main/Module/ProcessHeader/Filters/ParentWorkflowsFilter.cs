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
	public class ParentWorkflowsFilter : ModuleGuidPivotFilter
	{
		protected ParentWorkflowsFilter(FilterCategory category, ModuleFilterCollection parentCollection)
			: base(category, parentCollection)
		{
		}

		public ParentWorkflowsFilter(ZString description, IBusinessObjectCollection list)
			: base(description, ModuleIDs.ProcessHeader, ProcessHeaderLinkSchema.FP_FH_HeaderTo, ProcessHeaderLinkSchema.FP_FH_HeaderFrom, list, typeof(ProcessHeader), typeof(ProcessHeaderLink), new ZQuery(ProcessHeaderLinkSchema.FP_LinkType, ProcessHeaderLinkTypeList.Codes.ParentChild))
		{
			SetDefaultProperties();
		}

		public ParentWorkflowsFilter(ZString description, GetList listDelegate)
			: base(description, ModuleIDs.ProcessHeader, ProcessHeaderLinkSchema.FP_FH_HeaderTo, ProcessHeaderLinkSchema.FP_FH_HeaderFrom, listDelegate, typeof(ProcessHeader), typeof(ProcessHeaderLink), new ZQuery(ProcessHeaderLinkSchema.FP_LinkType, ProcessHeaderLinkTypeList.Codes.ParentChild))
		{
			SetDefaultProperties();
		}

		void SetDefaultProperties()
		{
			MultilingualDescription = DefaultDescription;
		}

		static MultilingualString DefaultDescription => ResString.GetMultilingualString("d9c85ef7-4ebd-4da1-adda-dccc3022bfb7", "Parent Workflows");

		protected override FilterCategory DefaultCategory => FilterCategories.Other;
	}
}
