using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(GLPresentationJournalCategoryCollection))]
	class GLPresentationJournalCategoryCollectionTest : CodeDescriptionBoolWithExtraBoolCollectionAbstractTest<GLPresentationJournalCategoryCollection>
	{
		public override void TestDefaultBoolForNewChild()
		{
			var element = Collection.AddNew();
			AssertEquals("AddNew().Bool", true, element.Bool);
			AssertEquals("AddNew().Bool2", false, element.Bool2);
			AssertEquals("AddNew().Bool3", false, element.Bool3);
			AssertEquals("AddNew().Bool4", false, element.Bool4);
		}

		public void TestEliminationCategory()
		{
			AssertNull(Collection.EliminationCategory);

			var element = Collection.AddNew();
			AssertEquals("AddNew().Bool", true, element.Bool);
			AssertEquals("AddNew().Bool2", false, element.Bool2);
			element.Code = "abc";
			AssertNull(Collection.EliminationCategory);

			element.Bool2 = true;
			AssertEquals(element.Code, Collection.EliminationCategory.Code);
		}

		protected override GLPresentationJournalCategoryCollection GetCollectionToTest()
		{
			return new GLPresentationJournalCategoryCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new GLPresentationJournalCategory();
		}
	}
}
