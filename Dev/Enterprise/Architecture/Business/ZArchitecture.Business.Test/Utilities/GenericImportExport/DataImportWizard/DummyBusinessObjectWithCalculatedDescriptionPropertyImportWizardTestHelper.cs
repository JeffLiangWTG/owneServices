using System.Collections;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.DataMapping.Testing
{
	sealed class DummyBusinessObjectWithCalculatedDescriptionPropertyImportWizardTestHelper : ImportWizardTestHelper
	{
		public DummyBusinessObjectWithCalculatedDescriptionPropertyImportWizardTestHelper(BusinessObjectFactory factory) : base(factory)
		{
			collection = new DummyBusinessObjectWithCalculatedDescriptionPropertyCollection(Factory);
		}

		IList GetBindToList(BusinessObject bizObj)
		{
			var list = new DummyBusinessObjectWithCalculatedDescriptionPropertyCollection(Factory);

			var item1 = (DummyBusinessObjectWithCalculatedDescriptionProperty)list.AddNew();
			item1.Z0_Guid = new ZGuid();
			item1.Z0_Description = "Calico Cat";
			item1.Z0_Code = "Meow1";
			item1.TestDescriptionProperty = "Z0_Code";

			var item2 = (DummyBusinessObjectWithCalculatedDescriptionProperty)list.AddNew();
			item2.Z0_Guid = new ZGuid();
			item2.Z0_Description = "Black Cat";
			item2.Z0_Code = "Meow2";
			item1.TestDescriptionProperty = "Z0_Code";

			return list;
		}

		IList GetBindToListEmpty(BusinessObject bizObj)
		{
			var list = new DummyBusinessObjectWithCalculatedDescriptionPropertyCollection(Factory);

			return list;
		}

		public override IImportCollectionInfo GetCollectionInfo()
		{
			var getListDelegate = new ImportPropertyInfoImpl.GetBindToListDelegate(GetBindToList);
			var getEmptyListDelegate = new ImportPropertyInfoImpl.GetBindToListDelegate(GetBindToListEmpty);
			var collectionInfo = new ImportCollectionInfoImpl(collection)
			{
				new ImportPropertyInfoImpl<DummyBaseBusinessObject>(DummyBizoSchema.Constants.Z0_Guid, getListDelegate) { HeaderText = "Guid" },
				new ImportPropertyInfoImpl<DummyBaseBusinessObject>(DummyBizoSchema.Constants.Z0_Code, getEmptyListDelegate) { HeaderText = "Code" },
			};

			return collectionInfo;
		}

		readonly DummyBusinessObjectWithCalculatedDescriptionPropertyCollection collection;
	}
}
