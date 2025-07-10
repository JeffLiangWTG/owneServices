using System;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProviderTesting
{
	sealed class ReportTypeOfGLAccountPairProviderTest : CodeDescriptionPairListProviderTest
	{
		protected override ICodeDescriptionPairListProvider CreateCodeDescriptionPairListProvider()
		{
			return new ReportTypeOfGLAccountPairProvider();
		}

		public override void TestIsReturningCorrectCollection()
		{
			ZString currentCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			try
			{
				AssertEquals("Should be 1", 1, CreateCodeDescriptionPairListProvider().GetCodeDescriptionPairList().Count);

				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.China);
				var list = new ComplianceReportTypeCollection(Core.Constants.CountryCodes.China);
				AccountingMasterFilesRegistry.Instance.ComplianceReportsSetupsCN.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, list);
				ZInt reportTypesCount = AccountingMasterFilesRegistry.Instance.ComplianceReportsSetupsCN.Value.Count;
				AssertEquals(CreateCodeDescriptionPairListProvider().GetCodeDescriptionPairList().Count, reportTypesCount + 1);

				list = new ComplianceReportTypeCollection();
				ComplianceReportType complianceReportType = list.AddNew();
				complianceReportType.ReportType = "ABC";
				ComplianceReportType complianceReportType1 = list.AddNew();
				complianceReportType1.ReportType = "EFG";

				AccountingMasterFilesRegistry.Instance.ComplianceReportsSetupsUserDefined.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, list);

				AssertEquals(reportTypesCount + 3, CreateCodeDescriptionPairListProvider().GetCodeDescriptionPairList().Count);

				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);
				AssertEquals("Should be only user define types", reportTypesCount + 3, CreateCodeDescriptionPairListProvider().GetCodeDescriptionPairList().Count);
			}
			finally
			{
				GlbCompany.CurrentCompany.SetCountry(currentCountryCode);
			}
		}
	}
}
