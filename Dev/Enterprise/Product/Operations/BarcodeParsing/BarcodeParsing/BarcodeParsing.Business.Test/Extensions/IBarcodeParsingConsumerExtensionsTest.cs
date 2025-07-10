
namespace Enterprise.BarcodeParsing.Business.Testing
{
	class IBarcodeParsingConsumerExtensionsTest : BarcodeParsingTestCase
	{
		#region TestIsBuyerRequiredForRelatedEntity

		public void TestIsBuyerRequiredForRelatedEntity()
		{
			IBarcodeParsingConsumer consumer = null;
			AssertEquals(false, consumer.IsBuyerRequiredForRelatedEntity());

			var dummy = DummyBarcodeParsingConsumer.GetDummy(Factory);
			AssertEquals(false, dummy.IsBuyerRequiredForRelatedEntity());

			dummy.RelatedEntityRequirements = RelatedEntityRequirements.MustHaveBuyer;
			AssertEquals(true, dummy.IsBuyerRequiredForRelatedEntity());
		}

		#endregion
	}
}
