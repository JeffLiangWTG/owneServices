using CargoWise.Types;
using Enterprise.Customs.IE.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IE.H7.Business.Test
{
	sealed class IM414HeaderProviderTest : IM413_414_415MessageProviderTest<IM414HeaderProvider>
	{
		public void TestImportOperation()
		{
			AssertEquals("LRN", AISOutboundEDIMessage.LRNPlaceHolder, Provider.ImportOperation.LRN);
		}

		public void TestLRN()
		{
			AssertEquals("LRN", "TestLRN", Provider.LRN);
		}

		public void TestMRN()
		{
			AssertEquals("MRN", "TestMRN", Provider.MRN);
		}

		public void TestCustomsRegistrationNumber()
		{
			AssertEquals("CustomsRegistrationNumber", string.Empty, Provider.CustomsRegistrationNumber);
		}

		[TestDate(2024, 2, 20, 15, 55, 01)]
		public void TestInvalidationRequestDateAndTime()
		{
			AssertEquals("TestInvalidationRequestDateAndTime", new ZDateTime(2024, 2, 20, 15, 55, 01), Provider.InvalidationRequestDateAndTime);
		}

		public void TestInvalidationReason()
		{
			AssertEquals("TestCustomsReferenceNumber", "TestInvalidReason", Provider.InvalidationReason);
		}

		protected override void SetUp()
		{
			base.SetUp();
			messageSendingObject.AmendmentInvalidationReason = "TestInvalidReason";
		}

		protected override IM414HeaderProvider GetProvider()
		{
			return new IM414HeaderProvider(messageSendingObject);
		}
	}
}
