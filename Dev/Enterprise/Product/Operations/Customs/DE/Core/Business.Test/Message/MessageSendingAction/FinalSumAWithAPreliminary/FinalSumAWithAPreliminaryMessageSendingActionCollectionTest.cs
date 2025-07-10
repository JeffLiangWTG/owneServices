using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.DE.Business.CusTempStorage;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Testing
{
	[TestedType(typeof(FinalSumAWithAPreliminaryMessageSendingActionCollection))]
	class FinalSumAWithAPreliminaryMessageSendingActionCollectionTest : NonPersistentBusinessObjectCollectionTestCase<FinalSumAWithAPreliminaryMessageSendingActionCollection>
	{
		public void TestGetSendingAction()
		{
			var collection = GetCollectionToTest();
			CombineAssertions(() =>
			{
				AssertEquals("Empty Collection", 0, collection.Count);
				collection.PopulateElements();
				AssertEquals("Loaded with actions now", 1, collection.Count);
			});
		}

		protected override FinalSumAWithAPreliminaryMessageSendingActionCollection GetCollectionToTest() => new FinalSumAWithAPreliminaryMessageSendingActionCollection(declaration.CusTempStorageLines.Cast<CusTempStorageLine>(), Factory);

		protected override BusinessObject GetNewElementToAddToTheCollection() => new FinalSumAWithAPreliminaryMessageSendingAction(line);

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<CUSPRLCusTempStorageDec>();
			line = declaration.CusTempStorageLines.AddNew();
		}
		CUSPRLCusTempStorageDec declaration;
		CusTempStorageLine line;
	}
}
