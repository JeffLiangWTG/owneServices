using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.DataMapping;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.DataMapping.Testing
{
	sealed class ImportWizardFactoryTest : TestCase
	{
		public void TestIImportWizardProvider()
		{
			using (var grid = new ZGrid())
			{
				grid.Columns.AddBoolColumn("Z0_Bool", 123);
				grid.BindingContext = new System.Windows.Forms.BindingContext();
				grid.SetDataBinding(new IImportWizardProviderTestImpl(), "");
				var collectionInfo = new ZGridImportCollectionInfo(grid);
				var wizard = new ImportWizardFactory().New(collectionInfo, null, null);
				AssertType<ImportWizardForTest>(wizard);
			}
		}

		class IImportWizardProviderTestImpl : DummyChildEnterpriseBusinessObjectCollection, IImportWizardProvider
		{
			public IImportWizardProviderTestImpl() : base(new BusinessObjectFactory()) { }
			public ImportWizard GetImportWizard(IImportCollectionInfo collectionInfo, ISettingsStorage settingsStorage, IFileMapper fileMapper)
			{
				return new ImportWizardForTest(collectionInfo);
			}
		}

		class ImportWizardForTest : ImportWizard
		{
			public ImportWizardForTest(IImportCollectionInfo collectionInfo) : base(collectionInfo, null, null) { }
		}
	}
}
