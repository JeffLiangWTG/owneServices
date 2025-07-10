using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.EDI.Registry.Business
{
	public class TestRigRegistryItem : StronglyTypedRegistryItem<TestRigRegistryHeader>
	{
		public TestRigRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage)
			: base(new RegistryItemImpl(name, category, caption, hint, new TestRigRegistryDataType(), storage, new TestRigRegistryHeader()))
		{
		}
	}
}

