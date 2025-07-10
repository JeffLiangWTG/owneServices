namespace Enterprise.Customs.BR.Business.Testing
{
	abstract class IncoTermAndCustomsChargeFactoryTest : Common.Testing.IncoTermAndCustomsChargeFactoryTest
	{
		public override void TestGetAllIncoTerms()
		{
			var allIncoTerms = Factory.GetCachedValue<BRIncoTermList>().GetAllCodes();
			AssertEquals(15, allIncoTerms.Length);
			AssertCollectionContains("C+I", allIncoTerms);
			AssertCollectionContains("C+F", allIncoTerms);
			AssertCollectionContains("CFR", allIncoTerms);
			AssertCollectionContains("CIF", allIncoTerms);
			AssertCollectionContains("CIP", allIncoTerms);
			AssertCollectionContains("CPT", allIncoTerms);
			AssertCollectionContains("DAP", allIncoTerms);
			AssertCollectionContains("DAT", allIncoTerms);
			AssertCollectionContains("DDP", allIncoTerms);
			AssertCollectionContains("DPU", allIncoTerms);
			AssertCollectionContains("EXW", allIncoTerms);
			AssertCollectionContains("FAS", allIncoTerms);
			AssertCollectionContains("FCA", allIncoTerms);
			AssertCollectionContains("FOB", allIncoTerms);
			AssertCollectionContains("OCV", allIncoTerms);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1051:DoNotUseBaseSourcePath", Justification = "Baseline")]
		protected override string IncoTermAndCustomsChargeConfigurationFilename => BaseSourcePath + @"Enterprise\Product\Operations\Customs\BR\Business.Test\Business\JobComInvHeaderCharge\InvoiceCharge\TestFile\IncoTermAndCustomsChargeConfiguration.csv";
	}
}
