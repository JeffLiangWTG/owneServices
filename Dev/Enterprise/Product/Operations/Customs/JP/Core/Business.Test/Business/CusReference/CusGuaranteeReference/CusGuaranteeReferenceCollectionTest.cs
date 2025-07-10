using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Business.Testing
{
	[TestedType(typeof(CusGuaranteeReferenceCollection))]
	sealed public class CusGuaranteeReferenceCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestMaxCount()
		{
			const int maxRowCount = 2;
			AssertEquals(maxRowCount, Collection.MaxCount);

			for (var i = 0; i < maxRowCount - 1; i++)
			{
				Collection.AddNew();
			}
			Assert($"Currently, collection has {Collection.Count} elements", Collection.AllowNew);
			Collection.AddNew();
			Assert($"Currently, collection has {Collection.Count} elements", !Collection.AllowNew);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var instruction = Factory.New<CusEntryInstruction>();
			return instruction.Guarantees;
		}
	}
}
