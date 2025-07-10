using System.Collections;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.ZArchitecture.DataMapping.Testing
{
	sealed class NonPersistentBusinessObjectImportWizardTestHelper : ImportWizardTestHelper
	{
		public NonPersistentBusinessObjectImportWizardTestHelper(BusinessObjectFactory factory)
			: base(factory)
		{
			collection = new DummyNonPersistentBusinessObjectCollection(Factory);
		}

		public override IImportCollectionInfo GetCollectionInfo()
		{
			ImportPropertyInfoImpl.GetBindToListDelegate getListDelegate = new ImportPropertyInfoImpl.GetBindToListDelegate(GetBindToList);
			return new ImportCollectionInfoImpl(collection)
					{
						new ImportPropertyInfoImpl<DummyNonPersistentBusinessObject>("Description",getListDelegate)
					};
		}

		IList GetBindToList(BusinessObject bizObj)
		{
			DummyNonPersistentBusinessObjectCollection list = new DummyNonPersistentBusinessObjectCollection(Factory);
			for (int i = 0; i < 3; i++)
			{
				DummyNonPersistentBusinessObject item = new DummyNonPersistentBusinessObject();
				item.Code = "Code" + i.ToString();
				item.Description = "Description" + i.ToString();
				list.Add(item);
			}
			return list;
		}

		readonly DummyNonPersistentBusinessObjectCollection collection;
	}
}
