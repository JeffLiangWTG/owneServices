using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.ZArchitecture.Business.Testing
{
	class DummyBusinessObjectWithManyBoolProperties : DummyBusinessObject
	{
		public DummyBusinessObjectWithManyBoolProperties(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public ZBool Bool01 { get; set; }
		public ZBool Bool02 { get; set; }
		public ZBool Bool03 { get; set; }
		public ZBool Bool04 { get; set; }
		public ZBool Bool05 { get; set; }
		public ZBool Bool06 { get; set; }
		public ZBool Bool07 { get; set; }
		public ZBool Bool08 { get; set; }
		public ZBool Bool09 { get; set; }
		public ZBool Bool10 { get; set; }
		public ZBool Bool11 { get; set; }
		public ZBool Bool12 { get; set; }
		public ZBool Bool13 { get; set; }
		public ZBool Bool14 { get; set; }
		public ZBool Bool15 { get; set; }
		public ZBool Bool16 { get; set; }
		public ZBool Bool17 { get; set; }
		public ZBool Bool18 { get; set; }
		public ZBool Bool19 { get; set; }
		public ZBool Bool20 { get; set; }
		public ZBool Bool21 { get; set; }
		public ZBool Bool22 { get; set; }
	}
}
