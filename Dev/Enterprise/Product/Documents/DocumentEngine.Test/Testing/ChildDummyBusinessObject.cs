using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.CustomValues;

namespace Enterprise.DocumentEngine.Testing
{
	[UserDefinedValues]
	public class ChildDummyBusinessObject : DummyBaseBusinessObject
	{
		public ChildDummyBusinessObject(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public ZString[] ZStringArray { get; set; }

		public ZString DeliveryType => Res.GetString("2375508B-40B0-467F-9DE0-EF1EE149D562", "Delivery Type");

		public ZString ServiceType => Res.GetString("F057BB85-A8D4-4288-AAEF-232D6FF31D61", "Service Type");
	}
}
