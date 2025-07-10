using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Business
{
	public class BMBoardSectionAdditionalComponentLookups : AutoBMBoardSectionAdditionalComponentLookups
	{
		public BMBoardSectionAdditionalComponentLookups(AutoBMBoardSectionAdditionalComponent parent)
			: base(parent)
		{
			this.parent = (BMBoardSectionAdditionalComponent)parent;
		}

		readonly BMBoardSectionAdditionalComponent parent;

		public BMComponentCollection Components
		{
			get
			{
				var collection = new BMComponentCollection(Factory, new ZQuery(new ZQuery(BMComponentSchema.FC_FC_ParentComponent, null)), false);
				var component = parent?.Component;
				var boardSystemPK = parent?.Section?.SectionConfiguration?.Board?.MB_FS_System;
				collection.AddComponentLookupFilterBusinessObjectDefaults(component, boardSystemPK);
				return collection;
			}
		}
	}
}
