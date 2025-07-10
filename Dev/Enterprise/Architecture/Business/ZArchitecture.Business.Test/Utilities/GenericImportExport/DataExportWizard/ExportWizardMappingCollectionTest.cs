using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.DataMapping.Testing
{
	[TestedType(typeof(ExportWizardMappingCollection))]
	sealed class ExportWizardMappingCollectionTest : NonPersistentBusinessObjectCollectionTestCase<ExportWizardMappingCollection>
	{
		#region Implementation

		protected override ExportWizardMappingCollection GetCollectionToTest()
		{
			collection = new ExportWizardMappingCollection(Helper.Wizard);
			collection.Load();
			return collection;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new ExportWizardMapping(Helper.CollectionInfo.RowTypes.First().Properties.First(), collection);
		}

		ExportWizardTest.ExportWizardTestHelper Helper
		{
			get
			{
				if (helper == null)
				{
					helper = new ExportWizardTest.ExportWizardTestHelper(Factory);
				}

				return helper;
			}
		}
		ExportWizardTest.ExportWizardTestHelper helper;

		ExportWizardMappingCollection collection;

		#endregion
	}
}
