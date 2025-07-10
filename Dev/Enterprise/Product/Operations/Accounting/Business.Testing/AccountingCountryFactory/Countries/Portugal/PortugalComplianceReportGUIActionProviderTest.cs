using Enterprise.Accounting.Business.ComplianceReport;

namespace Enterprise.Accounting.Business.AccountingCountryFactory.Testing
{
	public class PortugalComplianceReportGUIActionProviderTest : BaseComplianceReportGUIActionProviderTest
	{
		public override void TestValidateBeforeGenerateSAFT()
		{
			var provider = GetProvider();

			var report = Factory.NewWithValidTestData<AccComplianceReport>();

			AssertNotNull(provider);
			AssertEquals(string.Empty, provider.ValidateBeforeGenerateSAFT(new[] { report }));
		}

		public void TestGetSAFTFileCompressionInfo()
		{
			var provider = GetProvider();

			AssertNotNull(provider);
			var sAFTFileCompressionInfo = provider.GetSAFTFileCompressionInfo();
			Assert(!sAFTFileCompressionInfo.IsNeedCompression);
			AssertEquals(0, sAFTFileCompressionInfo.AllowedMaxSize);
		}

		protected override IComplianceReportGUIActionProvider GetProvider()
		{
			return new PortugalComplianceReportGUIActionProvider();
		}

		protected override bool ExpectedIsCountrySupportGenerateSAFT => true;
	}
}
