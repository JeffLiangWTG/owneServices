using System.Data;

namespace CargoWise.EntityFramework.Testing
{
	sealed class DummyBusinessObjectForTest : DummyBusinessObject
	{
		public DummyBusinessObjectForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		CachedRelatedBusinessObject<DummyBusinessObject> relatedZ0_Guid;

		public DummyBusinessObject RelatedZ0_Guid => (relatedZ0_Guid ?? (relatedZ0_Guid =
			new CachedRelatedBusinessObject<DummyBusinessObject>((ZPropertyInfoGuid)Z0_GuidInfo,
				() =>
				{
					return Factory.Load<DummyBusinessObject>(Z0_Guid);
				}))).Value;
	}
}
