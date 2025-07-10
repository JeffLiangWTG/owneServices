using System;
using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.AIS.Testing
{
	sealed class IM414HeaderProviderTest : DataProviderTestCase<IM414HeaderProvider>
	{
		public void TestIIM414Header()
		{
			Assert("Should implement IIM414Header", Provider is IIM414Header);
		}

		public void TestCustomsRegistrationNumber()
		{
			entryHeader.CRN = "Test CRN";
			AssertEquals("CustomsRegistrationNumber", "Test CRN", Provider.CustomsRegistrationNumber);
		}

		[TestDate(2022, 5, 15, 16, 45, 35)]
		public void TestInvalidationRequestDateAndTime()
		{
			AssertEquals("InvalidationRequestDateAndTime", new DateTime(2022, 5, 15, 16, 45, 35), Provider.InvalidationRequestDateAndTime);
		}

		public void TestInvalidationReason()
		{
			sendingAction.Annotation = "Reason";
			AssertEquals("InvalidationReason", "Reason", Provider.InvalidationReason);
		}

		public void TestLRN()
		{
			entryHeader.CH_BGMReference = "LRN001";
			AssertEquals("LRN", "LRN001", Provider.LRN);
		}

		public void TestMRN()
		{
			entryHeader.MovementReferenceNumberSetter("MRN001");
			AssertEquals("MRN", "MRN001", Provider.MRN);
		}

		protected override IM414HeaderProvider GetProvider() => new IM414HeaderProvider(sendingAction);

		protected override void SetUp()
		{
			base.SetUp();
			var testBizObjs = MessageProviderTestHelper.SetupBasicTestBizObjs(Factory);
			entryHeaderWrapper = testBizObjs.entryHeaderWrapper;
			entryHeader = entryHeaderWrapper.EntryHeader;
			sendingAction = new AISMessageSendingAction(entryHeaderWrapper.EntryHeader);
		}
		AISMessageSendingAction sendingAction;
		EntryHeaderWrapper entryHeaderWrapper;
		CusEntryHeader entryHeader;
	}
}
