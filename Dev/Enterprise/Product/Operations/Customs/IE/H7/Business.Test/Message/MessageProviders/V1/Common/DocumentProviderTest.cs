using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.IE.H7.Business.Messaging.V1.Test
{
	sealed class DocumentProviderTest : DataProviderTestCase<DocumentProvider>
	{
		public void TestType()
		{
			AssertEquals("Type", "Code", Provider.Type);
		}

		public void TestReference()
		{
			AssertEquals("ReferenceNumber", "Description", Provider.Reference);
		}

		protected override DocumentProvider GetProvider()
		{
			var supportingInfo = Factory.New<CusSupportingInfo>();
			supportingInfo.CSI_Code = "Code";
			supportingInfo.CSI_ReferenceNumber = "Description";

			return new DocumentProvider(supportingInfo);
		}
	}
}
