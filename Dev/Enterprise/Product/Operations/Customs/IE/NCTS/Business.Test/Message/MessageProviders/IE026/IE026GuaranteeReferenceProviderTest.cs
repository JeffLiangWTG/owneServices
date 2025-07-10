using System;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	sealed class IE026GuaranteeReferenceProviderTest : Customs.Business.Testing.DataProviderTestCase<IE026GuaranteeReferenceProvider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>("IE026GuaranteeReferenceProvider Constructor should throw ArgumentNullException with null parameter.", () => { _ = new IE026GuaranteeReferenceProvider(null); });
		}

		public void TestGrn()
		{
			AssertEquals("ABC123", Provider.Grn);
		}

		public void TestMasterAccessCode()
		{
			AssertEquals("MC99", Provider.MasterAccessCode);
		}

		public void TestAccessCode()
		{
			AssertType<IE026AccessCodeProvider>(Provider.AccessCode);
		}

		protected override IE026GuaranteeReferenceProvider GetProvider() => new IE026GuaranteeReferenceProvider(sendingObject);

		protected override void SetUp()
		{
			base.SetUp();
			sendingObject = new GuaranteeAccessCodesSendingAction(NctsTestDataProvider.CreateNctsGuaranteeWithCusGuaranteeHeader(Factory).cusGuaranteeHeader);
			sendingObject.MasterCode = "MC99";
		}
		GuaranteeAccessCodesSendingAction sendingObject;
	}
}
