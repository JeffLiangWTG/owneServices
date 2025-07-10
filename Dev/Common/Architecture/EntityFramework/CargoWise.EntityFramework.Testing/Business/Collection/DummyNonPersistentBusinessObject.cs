using CargoWise.ComponentModel;
using CargoWise.Types;

namespace CargoWise.EntityFramework.Testing
{
	[CodeProperty("Code"), DescriptionProperty("Description")]
	public class DummyNonPersistentBusinessObject : DummyNonPersistentBusinessObjectWithoutAttribute
	{
		public DummyNonPersistentBusinessObject() : base()
		{ }

		public DummyNonPersistentBusinessObject(DummyNonPersistentBusinessObjectCollection passedCollection)
			: base()
		{
			collection = passedCollection;
		}

		public ZPropertyInfo ChildDummyCodeInfo => GetZPropertyInfo(nameof(ChildDummyCode));

		[List(nameof(Collection))]
		public ZString ChildDummyCode
		{
			get => childDummyCode;
			set => SetNonPersistentPropertyValue(ChildDummyCodeInfo, ref childDummyCode, value);
		}
		ZString childDummyCode;

		public ZPropertyInfo ChildDummyPKInfo => GetZPropertyInfo(nameof(ChildDummyPK));

		[List(nameof(Collection))]
		public ZGuid ChildDummyPK
		{
			get => childDummyPK;
			set => SetNonPersistentPropertyValue(ChildDummyPKInfo, ref childDummyPK, value);
		}
		ZGuid childDummyPK;

		DummyNonPersistentBusinessObjectCollection collection;
		public DummyNonPersistentBusinessObjectCollection Collection => collection ?? (collection = new DummyNonPersistentBusinessObjectCollection(Factory));
	}
}
