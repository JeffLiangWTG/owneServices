using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.Business.Testing
{
	[TestedType(typeof(StmTemplateBaseCollection))]
	sealed class StmTemplateBaseCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new StmTemplateBaseCollection(Factory);
		}

		public void TestEditingMode()
		{
			StmTemplateBaseCollection collection = new StmTemplateBaseCollection(Factory);

			AssertEquals("Default EditingMode", MenuEditingMode.NotAllowEditingOfSystemOrClientMenus, collection.EditingMode);
			var template1 = collection.AddNew();
			AssertEquals("Template1 EditingMode", collection.EditingMode, template1.EditingMode);

			collection.EditingMode = MenuEditingMode.AllowEditingOfClientSpecificOnly;
			var template2 = collection.AddNew();
			AssertEquals("Template1 EditingMode", MenuEditingMode.AllowEditingOfClientSpecificOnly, template1.EditingMode);
			AssertEquals("Template2 EditingMode", MenuEditingMode.AllowEditingOfClientSpecificOnly, template2.EditingMode);

			var template3 = collection.AddNew();
			AssertEquals("Template1 EditingMode", MenuEditingMode.AllowEditingOfClientSpecificOnly, template1.EditingMode);
			AssertEquals("Template2 EditingMode", MenuEditingMode.AllowEditingOfClientSpecificOnly, template2.EditingMode);
			AssertEquals("Template3 EditingMode", MenuEditingMode.AllowEditingOfClientSpecificOnly, template3.EditingMode);
		}

		public void TestIsManagedByDataRefresh()
		{
			StmTemplateBaseCollection collection = new StmTemplateBaseCollection(Factory);
			AssertEquals("IsManagedForDataRefresh", true, collection.IsManagedForDataRefresh);
		}
	}
}
