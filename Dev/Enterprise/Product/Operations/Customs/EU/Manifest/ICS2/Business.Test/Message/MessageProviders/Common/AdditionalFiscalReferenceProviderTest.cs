using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	public class AdditionalFiscalReferenceProviderTest : DataProviderTestCase<AdditionalFiscalReferenceProvider>
	{
		public void TestNewOrNull()
		{
			AssertNull(AdditionalFiscalReferenceProvider.NewOrNull(null));
			AssertNotNull(AdditionalFiscalReferenceProvider.NewOrNull(fiscalReference));
		}

		public void TestIdentifier()
		{
			AssertEquals("AdditionalFiscalReference CFR_Reference", "4321", Provider.Identifier);
		}

		public void TestType()
		{
			AssertEquals("AdditionalFiscalReference CFR_Code", "FR5", Provider.Type);
		}

		protected override void SetUp()
		{
			base.SetUp();

			header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			fiscalReference = bill.AdditionalFiscalReferences.AddNew();
			fiscalReference.CFR_Reference = "4321";
			fiscalReference.CFR_Code = "FR5";
		}
		AsycudaManifestHeader header;
		AdditionalFiscalReference fiscalReference;

		protected sealed override AdditionalFiscalReferenceProvider GetProvider()
		{
			return AdditionalFiscalReferenceProvider.NewOrNull(fiscalReference);
		}
	}
}
