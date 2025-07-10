using System;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.Bi.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.FeatureControl.Abstractions;
using Enterprise.Accounting.Business.ComplianceReport;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Moq;

namespace Enterprise.Accounting.Business.Testing.ComplianceReport
{
	public class AccComplianceReportTypeDeciderTest : TestCaseWithFactory
	{
		public void TestGetTypeForLoad()
		{
			var factory = new BusinessObjectFactory();
			var report1 = factory.New<AccComplianceReport>();
			var decider = new AccComplianceReportTypeDecider();

			var type1 = decider.GetTypeForLoad(((INeedRow)report1).Row, factory);

			AssertEquals(typeof(AccComplianceReport), type1);

			var complianceConfig = AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.Value;
			var reportConfig = complianceConfig.AddNew();
			reportConfig.ReportCode = "TST";
			reportConfig.ReportTitle = "Test GLD Report";
			reportConfig.ReportPeriodicity = ComplianceReportConfigurationLookups.ReportPeriodicityCodes.AccountingPeriod;
			reportConfig.Country = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			reportConfig.TaxRegistrationType = "ABN";
			reportConfig.ReportLineGrouping = ComplianceReportConfigurationLookups.ReportLineGroupingListCodes.DayBookWithoutGrouping;
			reportConfig.ReportBaseTablePrefix = ComplianceReportConfigurationLookups.ReportBaseTablePrefixListCodes.GeneralLedgerData;
			AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, complianceConfig);
			report1.ACR_ReportType = "TST";

			var type2 = decider.GetTypeForLoad(((INeedRow)report1).Row, factory);
			AssertEquals(typeof(AccGLDComplianceReport), type2);

			using (var connection = Db.NewExtraConnectionWithMainDbCredentials(BiServers.LoadDataWarehouseServerUsingCacheIfPossible(Db.Connection), Db.SqlMasterDb))
			using (connection.BeginTransactionWithManager())
			using (ObjectFactory.Substitute(SetGldComplianceReportUsingEDWFeatureControl(true)))
			{
				var type3 = decider.GetTypeForLoad(((INeedRow)report1).Row, factory);
				AssertEquals(typeof(AccGLDComplianceReportUsingEDW), type3);
			}
		}

		IFeatureControlManager SetGldComplianceReportUsingEDWFeatureControl(bool isEnabled)
		{
			var mockIFeatureData = new Mock<IFeatureData>();
			var gLDFeatureControlData = new GeneralLedgerDataFeatureControlModel() { EnableGLDComplianceReportUsingEDW = isEnabled };
			var mockIFeatureControlManager = new Mock<IFeatureControlManager>();
			mockIFeatureData.Setup(x => x.TryDeserializeParameterAsJson(out gLDFeatureControlData)).Returns(true);
			mockIFeatureControlManager.Setup(x => x.GetFeatureDataAsync(CargoWise.Definitions.LicenceFeatureCodeList.Codes.AccountingGeneralLedgerDataFeature, CancellationToken.None)).Returns(Task.FromResult(mockIFeatureData.Object));

			return mockIFeatureControlManager.Object;
		}

		public void TestGetTypeForNew()
		{
			var decider = new AccComplianceReportTypeDecider();
			var type = decider.GetTypeForNew();
			AssertEquals(typeof(AccComplianceReport), type);
		}

		public void TestGetTypeForBinding()
		{
			var decider = new AccComplianceReportTypeDecider();
			var type = decider.GetTypeForBinding();
			AssertEquals(typeof(AccComplianceReport), type);
		}
	}
}
