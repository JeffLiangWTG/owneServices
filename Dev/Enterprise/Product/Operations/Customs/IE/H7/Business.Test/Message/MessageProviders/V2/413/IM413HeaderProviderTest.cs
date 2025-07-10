namespace Enterprise.Customs.IE.H7.Business.Test
{
	sealed class IM413HeaderProviderTest : IM413_414_415MessageProviderTest<IM413HeaderProvider>
	{
		public void TestImportOperation()
		{
			CombineAssertions("ImportOperation", () => {
				AssertEquals("LRN", "LRN001", Provider.ImportOperation.LRN);
				AssertEquals("MRN", "MRN001", Provider.ImportOperation.MRN);
				AssertEquals("AdditionalDeclarationType", "A", Provider.ImportOperation.AdditionalDeclarationType);
			});
		}

		protected override IM413HeaderProvider GetProvider()
		{
			return new IM413HeaderProvider(messageSendingObject);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var bill = messageSendingObject.Bill as AsycudaBill;
			bill.LocalReferenceNumber = "LRN001";
			bill.MovementReferenceNumberSetter("MRN001");
		}
	}
}
