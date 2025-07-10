using CargoWise.EntityFramework;
using Enterprise.BufferManagement.Integration;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.BufferManagement.Business
{
	[ModuleID(ModuleId.BMSystems)]
	public class BMSystemCollection : ActiveBusinessObjectCollection<BMSystem>, IBMSystemCollection
	{
		public BMSystemCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		#region IBMSystemCollection Members

		IBMSystem IBMSystemCollection.this[int index]
		{
			get { return base[index]; }
		}

		#endregion
	}
}
