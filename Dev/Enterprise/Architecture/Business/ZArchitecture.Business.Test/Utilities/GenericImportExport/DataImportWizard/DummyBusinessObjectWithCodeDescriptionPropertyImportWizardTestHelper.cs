using System.Collections;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.DataMapping.Testing
{
	sealed class DummyBusinessObjectWithCodeDescriptionPropertyImportWizardTestHelper : ImportWizardTestHelper
	{
		public DummyBusinessObjectWithCodeDescriptionPropertyImportWizardTestHelper(BusinessObjectFactory factory) : base(factory)
		{
			collection = new DummyBusinessObjectWithCodeDescriptionPropertyCollection(Factory);
		}

		IList GetBindToList(BusinessObject bizObj)
		{
			var list = new DummyBusinessObjectWithCodeDescriptionPropertyCollection(Factory);

			var item1 = (DummyBusinessObjectWithCodeDescriptionProperty)list.AddNew();
			item1.Z0_Guid = new ZGuid();
			item1.Z0_Description = "Z0_Description";

			return list;
		}

		public override IImportCollectionInfo GetCollectionInfo()
		{
			var getListDelegate = new ImportPropertyInfoImpl.GetBindToListDelegate(GetBindToList);
			var collectionInfo = new ImportCollectionInfoImpl(collection)
			{
				new ImportPropertyInfoImpl<DummyBaseBusinessObject>(DummyBizoSchema.Constants.Z0_Guid, getListDelegate) { HeaderText = "Guid" },
			};

			return collectionInfo;
		}

		readonly DummyBusinessObjectWithCodeDescriptionPropertyCollection collection;
	}
}
