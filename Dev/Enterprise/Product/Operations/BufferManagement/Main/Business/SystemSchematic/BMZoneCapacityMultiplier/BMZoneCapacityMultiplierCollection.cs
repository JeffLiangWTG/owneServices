using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Business
{
	public class BMZoneCapacityMultiplierCollection : ActiveBusinessObjectCollection<BMZoneCapacityMultiplier>
	{
		public BMZoneCapacityMultiplierCollection(BMComponent component)
			: base(component.Factory, component, new ZQuery(), BMZoneCapacityMultiplierSchema.BZC_FC_Component)
		{
		}
	}
}
