using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public abstract class OutturnLineCollectionTest<T> : NonPersistentBusinessObjectCollectionTestCase<T> where T : OutturnLineCollection
	{
		public void TestDeepCopyOfOutturnLineCollection()
		{
			var collection = SetupTestCollection();
			var copyColleciton = collection.DeepCopy();
			AssertEquals(copyColleciton.Count, 3);
			AssertNotEquals(collection[0], copyColleciton[0]);
			AssertNotEquals(collection[1], copyColleciton[1]);
			AssertNotEquals(collection[2], copyColleciton[2]);

			AssertEquals("Rederence1", copyColleciton[0].ConsignmentRef);
			AssertEquals("Rederence2", copyColleciton[1].ConsignmentRef);
			AssertEquals("Rederence3", copyColleciton[2].ConsignmentRef);
		}

		public void TestResetOutturnLineCountField()
		{
			var collection = SetupTestCollection();
			collection.ResetOutturnLineCountField();
			AssertEquals(0, collection[0].Count);
			AssertEquals(0, collection[1].Count);
			AssertEquals(0, collection[2].Count);
		}

		public void TestAddRemoveContainsGet()
		{
			var collection = SetupTestCollection();
			Assert(collection.ContainsConsignmentRef("Rederence2"));
			Assert(!collection.ContainsConsignmentRef("Rederence4"));
			var line = collection.FindByConsignmentRef("Rederence2");
			AssertEquals(13, line.Count);
			collection.Remove(line);
			Assert(!collection.ContainsConsignmentRef("Rederence2"));
			line = collection.FindByConsignmentRef("Rederence2");
			AssertNull(line);
		}

		OutturnLineCollection SetupTestCollection()
		{
			var testTime = ZDateTime.Now;
			var cusHAWB = Factory.NewWithValidTestData<CusHAWB>();
			var cusUnderbond = Factory.NewWithValidTestData<CusUnderbond>();

			var line1 = (OutturnLine)GetNewElementToAddToTheCollection();
			line1.ManifestInfo = new ManifestInformationForTest() { Quantity = 10 };
			line1.ConsignmentRef = "Rederence1";
			line1.Status = "Held";
			line1.ScannedDateTime = testTime;
			line1.Count = 12;
			line1.HouseBill = cusHAWB;
			line1.Underbond = cusUnderbond;

			var line2 = (OutturnLine)GetNewElementToAddToTheCollection();
			line2.ManifestInfo = new ManifestInformationForTest() { Quantity = 11 };
			line2.ConsignmentRef = "Rederence2";
			line2.Status = "Held";
			line2.ScannedDateTime = testTime;
			line2.Count = 13;
			line2.HouseBill = cusHAWB;
			line2.Underbond = cusUnderbond;

			var line3 = (OutturnLine)GetNewElementToAddToTheCollection();
			line3.ManifestInfo = new ManifestInformationForTest() { Quantity = 12 };
			line3.ConsignmentRef = "Rederence3";
			line3.Status = "Held";
			line3.ScannedDateTime = testTime;
			line3.Count = 14;
			line3.HouseBill = cusHAWB;
			line3.Underbond = cusUnderbond;

			var collection = (OutturnLineCollection)GetCollectionToTest();
			collection.Add(line1);
			collection.Add(line2);
			collection.Add(line3);

			return collection;
		}
	}
}
