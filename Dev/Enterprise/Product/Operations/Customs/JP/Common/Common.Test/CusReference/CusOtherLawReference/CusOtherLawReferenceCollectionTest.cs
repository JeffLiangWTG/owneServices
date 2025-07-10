using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Common.Testing
{
	[TestedType(typeof(CusOtherLawReferenceCollection<CusOtherLawReference>))]
	sealed class CusOtherLawReferenceCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var instruction = Factory.New<CusEntryInstruction>();
			return new CusOtherLawReferenceCollection<CusOtherLawReference>(instruction);
		}

		public void TestMaxCount()
		{
			const int maxRowCount = 5;
			var testCollection = GetCollectionToTest();
			AssertEquals(maxRowCount, testCollection.MaxCount);

			for (var i = 0; i < maxRowCount - 1; i++)
			{
				testCollection.AddNew();
			}
			Assert($"Currently, collection has {testCollection.Count} elements", testCollection.AllowNew);
			testCollection.AddNew();
			Assert($"Currently, collection has {testCollection.Count} elements", !testCollection.AllowNew);
		}

		public void TestSetDefaultsForNewChild()
		{
			var collection = GetCollectionToTest();
			var item = collection.AddNew() as CusOtherLawReference;
			AssertEquals(CusEntryInstructionSchema.Constants.Prefix, item.CFR_ParentTableCode);
		}
	}
}
