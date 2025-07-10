using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.ZArchitecture.DataMapping.Testing
{
	public class DummyBusinessObjectWithForeignKey : DummyBusinessObject
	{
		public DummyBusinessObjectWithForeignKey(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		[List("AllCollection")]
		public override ZGuid Z0_Guid
		{
			get
			{
				return base.Z0_Guid;
			}
			set
			{
				base.Z0_Guid = value;
			}
		}

		public DummyBusinessObjectCollection AllCollection
		{
			get
			{
				return allCollection ?? (allCollection = new DummyBusinessObjectCollection(this.Factory));
			}
		}
		DummyBusinessObjectCollection allCollection;
	}
}
