using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.IE.H7.Business.Test
{
	sealed class DocumentProviderTest : DataProviderTestCase<DocumentProvider>
	{
		public void TestType()
		{
			documentProvider = new DocumentProvider(supportingInfo);
			AssertEquals("Type", "Code", documentProvider.Type);

			documentProvider = new DocumentProvider(bill);
			AssertEquals("Type", "N703", documentProvider.Type);
		}

		public void TestReference()
		{
			documentProvider = new DocumentProvider(supportingInfo);
			AssertEquals("Reference Number", "Description", documentProvider.Reference);

			documentProvider = new DocumentProvider(bill);
			AssertEquals("Reference Number", "BillNumber", documentProvider.Reference);
		}

		protected override void SetUp()
		{
			base.SetUp();

			supportingInfo = Factory.New<CusSupportingInfo>();
			supportingInfo.CSI_Code = "Code";
			supportingInfo.CSI_ReferenceNumber = "Description";
			bill = Factory.New<AsycudaBill>();
			bill.ABL_BillNumber = "BillNumber";
		}
		CusSupportingInfo supportingInfo;
		DocumentProvider documentProvider;
		AsycudaBill bill;

		protected sealed override DocumentProvider GetProvider()
		{
			return new DocumentProvider(supportingInfo);
		}
	}
}
