using System;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	sealed class DocumentProviderTest : DataProviderTestCase<DocumentProvider>
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown<ArgumentException>("Document missing", () => new DocumentProvider(null));
			});
		}

		public void TestType()
		{
			AssertEquals(string.Empty, Provider.Type);
			document.CSI_Code = "123";
			AssertEquals("123", Provider.Type);
		}

		public void TestReference()
		{
			AssertEquals(string.Empty, Provider.Reference);
			document.CSI_ReferenceNumber = "REFERENCE";
			AssertEquals("REFERENCE", Provider.Reference);
		}

		protected override DocumentProvider GetProvider() => new DocumentProvider(document);

		protected override void SetUp()
		{
			base.SetUp();
			document = Factory.New<NctsAdditionalInfo>();
		}
		NctsAdditionalInfo document;
	}
}
