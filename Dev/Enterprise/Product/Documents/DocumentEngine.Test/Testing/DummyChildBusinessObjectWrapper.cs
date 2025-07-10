using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.DocumentEngine.Testing
{
	sealed class DummyChildBusinessObjectWrapper : DocumentWrapper
	{
		public DummyChildBusinessObjectWrapper(ChildDummyBusinessObject parent, BusinessObjectFactory factory)
			: base(parent, factory)
		{
			this.parent = parent;
		}

		readonly ChildDummyBusinessObject parent;

		public ChildDummyBusinessObject Internal { get { return parent; } }

		public ZString Code { get { return parent.Z0_Code; } }
		public ZString Text { get { return parent.Z0_VarCharMax; } }
		public ZString Description { get { return parent.Z0_Description; } }
		public ZInt Number { get { return parent.Z0_Number; } }
		public ZDecimal Decimal { get { return parent.Z0_Decimal; } }
		public ZDecimal AnotherDecimal { get { return parent.Z0_AnotherDecimal; } }
		public ZDecimal Money { get { return parent.Z0_Money; } }
		public ZString DeliveryType { get { return parent.DeliveryType; } }
		public ZString ServiceType { get { return parent.ServiceType; } }
	}
}
