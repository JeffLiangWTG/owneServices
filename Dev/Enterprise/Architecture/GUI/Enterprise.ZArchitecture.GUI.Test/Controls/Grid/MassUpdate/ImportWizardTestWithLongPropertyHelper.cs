using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.DataMapping;
using Enterprise.ZArchitecture.DataMapping.Testing;

namespace Enterprise.ZArchitecture.GUI.DataMapping.Testing
{
	sealed class ImportWizardTestWithLongPropertyHelper : ImportWizardTestHelper
	{
		public ImportWizardTestWithLongPropertyHelper(BusinessObjectFactory factory) : base(factory)
		{
		}

		public override IImportCollectionInfo GetCollectionInfo()
		{
			var collection = base.GetCollectionInfo();
			((ImportCollectionInfoImpl)collection).Add(new ImportPropertyInfoImpl<DummyBusinessObjectWithLongProperty>("__WWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWW__prop__ZString") { HeaderText = new string('W', 32) });

			var dummy = Factory.New<DummyBusinessObjectWithLongProperty>();
			collection.Collection.Add(dummy);
			return collection;
		}
	}
}
