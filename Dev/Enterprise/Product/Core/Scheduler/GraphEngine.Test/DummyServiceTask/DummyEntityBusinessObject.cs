using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.GraphEngine.Test;

namespace Enterprise.Scheduler.GraphEngine.Test
{
	public class DummyEntityBusinessObject : DummyBusinessObject, IDummyEntity
	{
		public DummyEntityBusinessObject(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public int ID => Z0_Number;
	}
}
