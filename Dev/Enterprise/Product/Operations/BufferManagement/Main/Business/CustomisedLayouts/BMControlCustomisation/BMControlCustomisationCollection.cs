using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Business
{
	[ModuleID(ModuleId.BMControlCustomisation)]
	public class BMControlCustomisationCollection : ActiveBusinessObjectCollection<BMControlCustomisation>
	{
		public BMControlCustomisationCollection(BusinessObjectFactory factory)
			: this(factory, new ZQuery())
		{
		}

		public BMControlCustomisationCollection(BusinessObjectFactory factory, string controlType)
			: this(factory, new ZQuery(BMControlCustomisationSchema.FM_ControlType, controlType))
		{
		}

		public BMControlCustomisationCollection(BusinessObjectFactory factory, ZQuery query)
			: base(factory, query)
		{
		}
	}
}
