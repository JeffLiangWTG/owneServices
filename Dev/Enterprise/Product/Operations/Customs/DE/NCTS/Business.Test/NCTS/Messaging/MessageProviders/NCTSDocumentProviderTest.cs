using CargoWise.Customs.DE.MessageContracts.NCTS;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.DE.NCTS.Business.Testing
{
	sealed class NCTSDocumentProviderTest : Customs.Business.Testing.DataProviderTestCase<INCTSDocument>
	{
		public void TestType() => CombineAssertions(() =>
		{
			AssertEquals("A999", Provider.Type);
			doc.CSI_Code = ZString.Empty;
			AssertNull(Provider.Type);
		});

		public void TestReferenceNumber() => CombineAssertions(() =>
		{
			AssertEquals("ReferenceNumber", Provider.ReferenceNumber);
			doc.CSI_ReferenceNumber = ZString.Empty;
			AssertNull(Provider.ReferenceNumber);
		});

		public void TestDocumentLineItemNumber()
		{
			AssertEquals(17, Provider.DocumentLineItemNumber);
		}

		public void TestComplementOfInformation() => CombineAssertions(() =>
		{
			AssertEquals("ComplementInformation", Provider.ComplementOfInformation);
			doc.CSI_ReferenceNumber2 = ZString.Empty;
			AssertNull(Provider.ComplementOfInformation);
		});

		protected override void SetUp()
		{
			base.SetUp();

			doc = Factory.New<CusSupportingInfo>();
			doc.CSI_Code = "A999";
			doc.CSI_ReferenceNumber = "ReferenceNumber";
			doc.CSI_ReferenceNumber2 = "ComplementInformation";
			doc.CSI_ItemNumber = 17;
		}

		protected override INCTSDocument GetProvider() => NCTSDocumentProvider.NewOrNull(doc);

		CusSupportingInfo doc;
	}
}
