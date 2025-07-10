using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class DummyWithReferenceableCollection : DummyBusinessObject
	{
		public DummyWithReferenceableCollection(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }
		[List("Codes")]
		public ZString Code { get; set; }
		public ZPropertyInfo CodeInfo => GetZPropertyInfo(nameof(Code));
		ReferenceableCollection codes;
		public ReferenceableCollection Codes => codes ?? (codes = new ReferenceableCollection(Factory));
	}
}
