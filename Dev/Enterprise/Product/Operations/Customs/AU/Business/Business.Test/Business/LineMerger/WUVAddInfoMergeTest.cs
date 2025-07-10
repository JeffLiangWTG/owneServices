namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	class WUVAddInfoMergeTest : MergeOnAddInfoValueTestCase
	{
		protected override void SetPropertyWithValue1(JobComInvoiceLine line)
		{
			line.AddInfo.ZA_WUV = 100m;
		}

		protected override void SetPropertyWithValue2(JobComInvoiceLine line)
		{
			line.AddInfo.ZA_WUV = 200m;
		}
	}
}
