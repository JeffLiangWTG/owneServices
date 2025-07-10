using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Module
{
	public class ComponentsFilter : ModuleGuidPivotFilter
	{
		public ComponentsFilter(ZString description, IBusinessObjectCollection list)
			: base(description, ModuleIDs.BMComponent, BMComponentLinkSchema.FL_FC_ComponentTo, BMComponentLinkSchema.FL_FC_ComponentFrom, list, typeof(BMComponent), typeof(BMComponentLink))
		{
			SetDefaultProperties();
		}

		void SetDefaultProperties()
		{
			MultilingualDescription = DefaultDescription;
		}

		static MultilingualString DefaultDescription => ResString.GetMultilingualString("BufferManagement|ComponentRelationshipFilter|Components", "Components");
	}
}
