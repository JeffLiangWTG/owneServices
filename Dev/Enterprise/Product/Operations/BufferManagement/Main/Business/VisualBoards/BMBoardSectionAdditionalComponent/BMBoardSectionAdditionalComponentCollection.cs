using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Business
{
	public class BMBoardSectionAdditionalComponentCollection : ActiveBusinessObjectCollection<BMBoardSectionAdditionalComponent>
	{
		public BMBoardSectionAdditionalComponentCollection(BMBoardSection section)
			: base(section.Factory, section, new ZQuery(), BMBoardSectionAdditionalComponentSchema.BSA_MS_Section)
		{
		}
	}
}
