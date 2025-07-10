using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(EFTPaymentInformationCollection))]
	sealed class EFTPaymentInformationCollectionTest : NonPersistentBusinessObjectCollectionTestCase<EFTPaymentInformationCollection>
	{
		public void TestAllowNew()
		{
			EFTPaymentInformationCollection collection = new EFTPaymentInformationCollection(TestDec);
			AssertEquals("Allow new is false as users should not be able to add an entry header and corresponding PayInfo", false, collection.AllowNew);
		}

		public void TestLoadCollection()
		{
			CusEntryHeader entryHeaderWithNumber = TestDec.CustomsEntryHeaders.AddNew();
			entryHeaderWithNumber.EntryNumber = "AAA111BBB";

			CusEntryHeader entryHeaderWithoutNumber = TestDec.CustomsEntryHeaders.AddNew();

			EFTPaymentInformationCollection collection = new EFTPaymentInformationCollection(TestDec);
			AssertEquals("There should be only one item in the collection", 1, collection.Count);
			AssertEquals("There should be only one item in the collection", entryHeaderWithNumber, collection[0].EntryHeader);
		}

		public void TestHasAmountsToPay()
		{
			CusEntryHeader entryHeader = TestDec.CustomsEntryHeaders.AddNew();
			entryHeader.EntryNumber = "AAA111BBB";

			EFTPaymentInformationCollection collection = new EFTPaymentInformationCollection(TestDec);
			EFTPaymentInformation info = collection[0];
			AssertEquals("Has nothing to pay", false, info.HasAmountsToPay);
			AssertEquals("Has nothing to pay for a collection", false, collection.HasAmountsToPay);

			info.CustomsChargeAmountPayableNow = 100m;
			AssertEquals("Has nothing to pay", true, info.HasAmountsToPay);
			AssertEquals("Has nothing to pay for a collection", true, collection.HasAmountsToPay);
		}

		public void TestPaymentInformationInitialiseFromLastClearance()
		{
			var entryHeaderMoq = Factory.NewMoq<CusEntryHeader>();
			entryHeaderMoq.Setup(m => m.TotalPayableDueAdvisedInLastClearanceMessage).Returns(new ZDecimal(123.50m));
			var entryHeader = entryHeaderMoq.Object;
			TestDec.CustomsEntryHeaders.Add(entryHeader);
			entryHeader.EntryNumber = "AAA111BBB";
			entryHeader.CustomsChargeAmountPayableNow = 100m;
			entryHeader.AQISServicePaymentAmountPayableNow = 50m;

			var collection = new EFTPaymentInformationCollection(TestDec, initialiseFromLastClearance: false);
			AssertEquals("Customs Charge Amount", 100m, collection.CustomsChargeAmount);
			AssertEquals("AQIS Amount", 50m, collection.AQISAmount);

			collection = new EFTPaymentInformationCollection(TestDec, initialiseFromLastClearance: true);
			AssertEquals("Customs Charge Amount", 123.50m, collection.CustomsChargeAmount);
			AssertEquals("AQIS Amount", 0m, collection.AQISAmount);
		}

		public void TestTotalAmounts()
		{
			CusEntryHeader entryHeader = TestDec.CustomsEntryHeaders.AddNew();
			entryHeader.EntryNumber = "AAA111BBB";
			CusEntryHeader entryHeader2 = TestDec.CustomsEntryHeaders.AddNew();
			entryHeader2.EntryNumber = "AAA222BBB";

			EFTPaymentInformationCollection collection = new EFTPaymentInformationCollection(TestDec);
			AssertEquals("PreCondition:There should be two items in the collection", 2, collection.Count);

			collection[0].AQISServicePaymentAmountPayableNow = 0m;
			collection[0].CustomsChargeAmountPayableNow = 100m;

			collection[1].AQISServicePaymentAmountPayableNow = 150m;
			collection[1].CustomsChargeAmountPayableNow = 200m;

			AssertEquals("Customs Charge Amount", 300m, collection.CustomsChargeAmount);
			AssertEquals("AQIS Amount", 150m, collection.AQISAmount);
		}

		protected override EFTPaymentInformationCollection GetCollectionToTest()
		{
			return new EFTPaymentInformationCollection(TestDec);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new EFTPaymentInformation(TestDec.CustomsEntryHeaders.AddNew());
		}

		JobDeclaration TestDec
		{
			get
			{
				if (fTestDec == null)
				{
					fTestDec = JobDeclaration.New(Factory);
				}
				return fTestDec;
			}
		}
		JobDeclaration fTestDec;
	}
}
