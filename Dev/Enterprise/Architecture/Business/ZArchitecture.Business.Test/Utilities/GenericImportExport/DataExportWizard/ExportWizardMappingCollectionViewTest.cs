using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.DataMapping.Testing
{
	[TestedType(typeof(ExportWizardMappingCollectionView))]
	sealed class ExportWizardMappingCollectionViewTest : BusinessObjectCollectionViewTestCase<ExportWizardMappingCollectionView>
	{
		public void TestMoveSelectedElementsUp()
		{
			var view = GetCollectionToTest();
			view.RemoveAndDelete(view[0]);
			view.RemoveAndDelete(view[0]);
			var mapping = new ExportWizardMapping(Helper.CollectionInfo.RowTypes.First(), collection);
			view.Add(mapping);

			for (int i = 3; i >= 0; i--)
			{
				view.MoveSelectedElementsUpDown(new ExportWizardMapping[] { mapping }, ExportWizardMappingCollectionView.Direction.Up);
				AssertEquals(mapping, view[i]);
			}

			view.MoveSelectedElementsUpDown(new ExportWizardMapping[] { mapping }, ExportWizardMappingCollectionView.Direction.Up);
			AssertEquals(mapping, view[0]);
		}

		#region Implementation

		protected override ExportWizardMappingCollectionView GetCollectionToTest()
		{
			collection = new ExportWizardMappingCollection(Helper.Wizard);
			collection.Load();
			return new ExportWizardMappingCollectionView(collection);
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
