using Enterprise.ZArchitecture.Schema;

namespace CargoWise.EntityFramework.Testing
{
	internal class DummyRelationshipBusinessObjectCollection : DummyBusinessObjectCollection
	{
		public DummyRelationshipBusinessObjectCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public DummyRelationshipBusinessObjectCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			return new ZQuery(DummyBizoSchema.Z0_Number, SQLComparisonOperator.Equal, 1);
		}
	}
}
