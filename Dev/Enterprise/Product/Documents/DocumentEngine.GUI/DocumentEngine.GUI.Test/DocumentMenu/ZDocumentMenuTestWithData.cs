using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentEngine.GUI.DocumentMenu.Testing
{
	sealed class ZDocumentMenuTestWithData : TestCaseWithFactory
	{
		[StressTest]
		public void TestAllSystemDefinedDocumentCategoriesHaveResourceStrings()
		{
			CombineAssertions(delegate
			{
				var helper = new ZDocumentsMenuItemMenuHelper(new ZMenuItem(ResString.GetMultilingualString("x", "x")));
				var query = new ZQuery(StmMenuItemSchema.SU_IsSystemDefined, true);
				query.AddToFilter(StmMenuItemSchema.SU_MenuPath, SQLComparisonOperator.NotEqual, string.Empty);
				foreach (DocumentCommand command in Factory.Load<DocumentCommand>(query))
				{
					var menu = (ZMenuItem)helper.GetParentMenu(command);
					while (menu != null)
					{
						AssertType("Parent menu " + menu.Text + " should have a valid resource string defined", typeof(ResourceString), menu.Caption);
						menu = (ZMenuItem)menu.Parent;
					}
				}
			});
		}
	}
}
