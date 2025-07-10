using System.Collections;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class DummyBusinessObjectWithNumberList : DummyBusinessObject
	{
		public DummyBusinessObjectWithNumberList(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		[List("NumberList")]
		public override ZInt Z0_Number { get; set; }
		public IList NumberList { get; set; }
	}
}
