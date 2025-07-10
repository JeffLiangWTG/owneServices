namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	partial class CMRCusSCAOceanBillDataObjectReaderTest
	{
		protected override void AssertPivot1AllPropertiesSet(CusSCAPivot pivot)
		{
			base.AssertPivot1AllPropertiesSet(pivot);
			AssertEquals("CT", pivot.CV_PackageType);
			AssertEquals(4m, pivot.CV_Volume);
			Assert(pivot.CV_HazardousGoods);
			Assert(pivot.CV_FumigationCert);
			Assert(pivot.CV_PersonalEffects);
			Assert(pivot.CV_Timber);
			Assert(pivot.CV_PerishableGoods);
			Assert(pivot.CV_IsSAC);
			Assert(pivot.CV_Flammable);
			AssertEquals(pivot.HouseBill.CA_CB, pivot.CV_CB);
		}

		protected override void AssertPivot2AllPropertiesSet(CusSCAPivot pivot)
		{
			base.AssertPivot2AllPropertiesSet(pivot);
			AssertEquals("DR", pivot.CV_PackageType);
			AssertEquals(40m, pivot.CV_Volume);
			Assert(!pivot.CV_HazardousGoods);
			Assert(!pivot.CV_FumigationCert);
			Assert(!pivot.CV_PersonalEffects);
			Assert(!pivot.CV_Timber);
			Assert(!pivot.CV_PerishableGoods);
			Assert(!pivot.CV_IsSAC);
			Assert(!pivot.CV_Flammable);
			AssertEquals(pivot.HouseBill.CA_CB, pivot.CV_CB);
		}
	}
}
