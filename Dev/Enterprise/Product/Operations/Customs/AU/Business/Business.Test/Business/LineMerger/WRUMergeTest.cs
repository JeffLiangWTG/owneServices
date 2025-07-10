namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	class WRUMergeTest : MergeOnAddInfoValueTestCase
	{
		protected override void SetPropertyWithValue1(JobComInvoiceLine line)
		{
			line.AddInfo.ZA_WRU = "KG";
		}

		protected override void SetPropertyWithValue2(JobComInvoiceLine line)
		{
			line.AddInfo.ZA_WRU = "ML";
		}
	}
}
