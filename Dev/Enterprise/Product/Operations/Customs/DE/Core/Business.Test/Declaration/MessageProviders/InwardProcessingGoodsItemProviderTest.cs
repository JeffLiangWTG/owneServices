using Enterprise.Customs.DE.Business.Declaration;

namespace Enterprise.Customs.DE.Business.Testing
{
	sealed class InwardProcessingGoodsItemProviderTest : Customs.Business.Testing.DataProviderTestCase<InwardProcessingGoodsItemProvider>
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertNull("NULL", InwardProcessingGoodsItemProvider.NewOrNull(null));
				AssertNotNull("Not NULL", InwardProcessingGoodsItemProvider.NewOrNull(previousDocument));
			});
		}

		public void TestReferencedRegistrationNumber()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Empty", string.Empty, Provider.ReferencedRegistrationNumber);

				previousDocument.CSI_ReferenceNumber = "REF12345";
				AssertEquals("Not empty", "REF12345", Provider.ReferencedRegistrationNumber);
			});
		}

		public void TestReferencedSequenceNumber()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Empty", 0, Provider.ReferencedSequenceNumber);

				previousDocument.CSI_LineNo = 2;
				AssertEquals("Not empty", 2, Provider.ReferencedSequenceNumber);
			});
		}

		public void TestAccessViaAtlasFlag()
		{
			CombineAssertions(() =>
			{
				AssertEquals("false", false, Provider.AccessViaAtlasFlag);

				previousDocument.Status = true;
				AssertEquals("true", true, Provider.AccessViaAtlasFlag);
			});
		}

		public void TestGoodsRelatedInformation()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Empty", string.Empty, Provider.GoodsRelatedInformation);

				previousDocument.CSI_Description = "Description";
				AssertEquals("Not empty", "Description", Provider.GoodsRelatedInformation);
			});
		}

		protected override InwardProcessingGoodsItemProvider GetProvider() => InwardProcessingGoodsItemProvider.NewOrNull(previousDocument);

		protected override void SetUp()
		{
			base.SetUp();
			previousDocument = Factory.New<PreviousDocument>();
		}
		PreviousDocument previousDocument;
	}
}
