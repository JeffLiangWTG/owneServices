using System;
using Enterprise.Customs.EU.NCTS.Business.Testing;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	sealed class IE026MessageProviderTest : Customs.Business.Testing.DataProviderTestCase<IE026MessageProvider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new IE026MessageProvider(null));
		}

		public void TestHolderOfTheTransitProcedure()
		{
			SetPrincipal("IE12345678");
			AssertType<HolderOfTransitProcedureProvider>(Provider.HolderOfTheTransitProcedure);
		}

		public void TestCustomsOfficeOfGuaranteeReferenceNumber()
		{
			AssertEquals("IE123", Provider.CustomsOfficeOfGuaranteeReferenceNumber);
		}

		public void TestGuaranteeReference()
		{
			AssertType<IE026GuaranteeReferenceProvider>(Provider.GuaranteeReference);
		}

		protected override IE026MessageProvider GetProvider() => new IE026MessageProvider(sendingObject);

		protected override void SetUp()
		{
			base.SetUp();
			var testData = NctsTestDataProvider.CreateNctsGuaranteeWithCusGuaranteeHeader(Factory);
			nctsHeader = testData.nctsGuarantee.NctsHeader;
			sendingObject = new GuaranteeAccessCodesSendingAction(testData.cusGuaranteeHeader);
			sendingObject.OfficeOfGuarantee = "IE123";
		}

		void SetPrincipal(string id)
		{
			NCTSTestHelper.CreateJobDocAddressForTest(Factory, "PC1", nctsHeader.Principal, string.Empty, "Test Company Limited", "123 Test Street", "A12B3C4", "City", "IEXX", "IE", id, "TIR123");
		}

		NctsHeader nctsHeader;
		GuaranteeAccessCodesSendingAction sendingObject;
	}
}
