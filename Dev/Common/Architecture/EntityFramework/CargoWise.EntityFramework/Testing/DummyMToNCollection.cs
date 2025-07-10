#if DEBUG
using System;

namespace CargoWise.EntityFramework.Testing
{
	public class DummyMToNCollection : ManyToManyBusinessObjectCollection<DummyDependantBusinessObject, DummyBaseBusinessObject>
	{
		public DummyMToNCollection(DummyBaseBusinessObject assocObj)
			: base(assocObj)
		{
		}

		public DummyMToNCollection(DummyBaseBusinessObject assocObj, ZQuery additionalFilter)
			: base(assocObj, additionalFilter)
		{
		}

		protected override Type TypeOfRelationshipBusinessObject
		{
			get { return typeof(DummyPivot); }
		}

		protected override string PivotTablePrefix
		{
			get { return "ZDP"; } // overriden for testing only!
		}
	}
}
#endif
