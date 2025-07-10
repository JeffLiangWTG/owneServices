using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Business.Testing
{
	[TestedType(typeof(VanningAddressCollection))]
	sealed class VanningAddressCollectionTest : BusinessObjectCollectionTestCase
	{
		public override void TestAddNew()
		{
			base.TestAddNew();

			var entryInstruction = Factory.New<CusEntryInstruction>();
			var collection = new VanningAddressCollection(entryInstruction);

			var address = collection.AddNew();
			AssertEquals("E2_RN_NKCountryCode", Enterprise.Core.Constants.CountryCodes.Japan, address.E2_RN_NKCountryCode);
			AssertEquals("E2_GovRegNumType", string.Empty, address.E2_GovRegNumType);
			AssertEquals("E2_GovRegNum", string.Empty, address.E2_GovRegNum);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var instruction = Factory.New<CusEntryInstruction>();
			return instruction.VanningLocations;
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
	}
}
