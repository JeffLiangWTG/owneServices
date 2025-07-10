using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Base.AccStatement.Testing
{
	[TestedType(typeof(UnreconciledStatementCollectionView))]
	public class UnreconciledStatementCollectionViewTest : BusinessObjectCollectionViewTestCase<UnreconciledStatementCollectionView>
	{
		public void TestIsThisPartOfTheCollection()
		{
			var bankStatement = Factory.New<BankStatement>();
			var collection = new StatementCollection(bankStatement, Factory);
			var view = new UnreconciledStatementCollectionView(collection);

			AssertEquals(0, collection.Count);
			AssertEquals(0, view.Count);

			collection.AddNew().IsCleared = true;
			collection.AddNew().IsCleared = true;
			collection.AddNew().IsCleared = true;

			AssertEquals(3, collection.Count);
			AssertEquals(0, view.Count);

			collection.AddNew().IsCleared = false;

			AssertEquals(4, collection.Count);
			AssertEquals(1, view.Count);

			collection.RemoveAll();

			AssertEquals(0, collection.Count);
			AssertEquals(0, view.Count);

			collection.AddNew().IsCleared = false;
			collection.AddNew().IsCleared = false;

			AssertEquals(2, collection.Count);
			AssertEquals(2, view.Count);
		}

		#region Implementation

		protected override UnreconciledStatementCollectionView GetCollectionToTest()
		{
			StatementCollection = new StatementCollection(Factory.New<BankStatement>(), Factory);
			return new UnreconciledStatementCollectionView(StatementCollection);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var result = StatementCollection.AddNew();
			result.IsCleared = false;
			return result;
		}

		protected TestObjectCreator TestObjectCreator
		{
			get { return testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory)); }
		}
		TestObjectCreator testObjectCreator;

		StatementCollection StatementCollection;

		#endregion
	}
}
