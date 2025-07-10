using System.Data;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace CargoWise.EntityFramework.Testing
{
	sealed class DummyBusinessObjectSupportMultipleResourceStringData2 : DummyBusinessObjectSupportMultipleResourceStringData1
	{
		public DummyBusinessObjectSupportMultipleResourceStringData2(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		[ResourceStringData("Override|DummyBusinessObject|Z0_Description", Caption = "[36] Test Description")]
		public override ZString Z0_Description { get; set; }
	}
}
