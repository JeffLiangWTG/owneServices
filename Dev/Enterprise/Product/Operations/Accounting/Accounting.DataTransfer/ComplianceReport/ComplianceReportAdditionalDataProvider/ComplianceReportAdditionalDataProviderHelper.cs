namespace Enterprise.Accounting.DataTransfer.ComplianceReport
{
	internal class ComplianceReportAdditionalDataProviderHelper
	{
		public static IComplianceReportAdditionalDataProvider GetComplianceReportAdditionalDataProvider(ComplianceReportDataCollectionMode mode)
		{
			switch (mode)
			{
				case ComplianceReportDataCollectionMode.SAFT:
				case ComplianceReportDataCollectionMode.SAFTSelfBilling:
					return new ComplianceReportAdditionalDataProviderSAFT();
				case ComplianceReportDataCollectionMode.SAFT1_10:
					return new ComplianceReportAdditionalDataProviderSAFT1_10();
				case ComplianceReportDataCollectionMode.SAFT1_30:
					return new ComplianceReportAdditionalDataProviderSAFT1_30();
				default:
					return new ComplianceReportAdditionalDataProviderNotSAFT();
			}
		}

		public static string NoneTaxRegistrationNumber => "NA";
	}
}
