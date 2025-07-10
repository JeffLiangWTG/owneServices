using System.Collections;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.DataMapping.Testing
{
	sealed class DummyBusinessObjectWithMultilingualStringPropertyImportWizardTestHelper : ImportWizardTestHelper
	{
		public DummyBusinessObjectWithMultilingualStringPropertyImportWizardTestHelper(BusinessObjectFactory factory) : base(factory)
		{
			collection = new DummyBusinessObjectWithMultilingualStringPropertyCollection(Factory);
		}

		IList GetBindToList(BusinessObject bizObj)
		{
			var list = new DummyBusinessObjectWithMultilingualStringPropertyCollection(Factory);

			var item1 = (DummyBusinessObjectWithMultilingualStringProperty)list.AddNew();
			item1.Z0_Guid = new ZGuid();
			item1.Z0_Description = "Knitting Cat";

			var item1Duplicate = (DummyBusinessObjectWithMultilingualStringProperty)list.AddNew();
			item1Duplicate.Z0_Guid = new ZGuid();
			item1Duplicate.Z0_Description = "Knitting Cat";

			var item2 = (DummyBusinessObjectWithMultilingualStringProperty)list.AddNew();
			item2.Z0_Guid = new ZGuid();
			item2.Z0_Description = "Fishing Cat";

			return list;
		}

		public override IImportCollectionInfo GetCollectionInfo()
		{
			var getListDelegate = new ImportPropertyInfoImpl.GetBindToListDelegate(GetBindToList);
			var collectionInfo = new ImportCollectionInfoImpl(collection)
			{
				new ImportPropertyInfoImpl<DummyBusinessObjectWithMultilingualStringProperty>(DummyBizoSchema.Constants.Z0_Description, getListDelegate) { HeaderText = "Z0_Desc" },
			};

			return collectionInfo;
		}

		readonly DummyBusinessObjectWithMultilingualStringPropertyCollection collection;
	}
}
