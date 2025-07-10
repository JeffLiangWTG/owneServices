using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(LocalExportStevedoreWrapper))]
	sealed class LocalExportStevedoreWrapperTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var stevedore = new LocalExportStevedore();
			return new LocalExportStevedoreWrapper(stevedore, Factory);
		}

		public void TestStevedoreFull()
		{
			var entry = new TestDataSetupHelper(Factory).GetLocalExportEntry5DQWithFullData();
			var wrapper = new EntryDocumentWrapper(entry, Factory).LocalExportEntryWrapper;

			var stevedoreItems1 = wrapper.StevedoreItems[0];
			var stevedoreItems1entryLine = stevedoreItems1.Stevedore;

			AssertEquals(1, stevedoreItems1entryLine.SequenceNo);
			AssertEquals("홍길동", stevedoreItems1entryLine.FullName);
			AssertEquals("19910506", stevedoreItems1entryLine.Birthday.ToString(DateFormatType.Date));
			AssertEquals("110001", stevedoreItems1entryLine.RoadNameCode);
			AssertEquals("121200", stevedoreItems1entryLine.BuildingNumber);
			AssertEquals("43012", stevedoreItems1entryLine.Postcode);
			AssertEquals("기본주소", stevedoreItems1entryLine.AddressLine1);
			AssertEquals("상세주소", stevedoreItems1entryLine.AddressLine2);
			AssertEquals("기본주소 상세주소", stevedoreItems1.AddressDetails);

			var stevedoreItems2 = wrapper.StevedoreItems[1];
			var stevedoreItems2entryLine = stevedoreItems2.Stevedore;

			AssertEquals(2, stevedoreItems2entryLine.SequenceNo);
			AssertEquals("Hong-Gil-Dong", stevedoreItems2entryLine.FullName);
			AssertEquals("19910606", stevedoreItems2entryLine.Birthday.ToString(DateFormatType.Date));
		}
	}
}
