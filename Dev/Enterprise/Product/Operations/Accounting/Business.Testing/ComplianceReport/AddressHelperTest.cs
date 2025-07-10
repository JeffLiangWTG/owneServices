using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.ComplianceReport.ZMGermany;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ComplianceReport.Testing;

[TestedType(typeof(AddressHelper))]
public class AddressHelperTest : TestCaseWithFactory
{
	public void TestGetReportAddress_ReturnsAddress_WhenHasSingleMainOfficeAddress()
	{
		var orgProxy = ZmReport.ComplianceReport.Company.OrgProxy;
		var reportAddress = AddressHelper.GetReportAddress(orgProxy);

		AssertNotNull("Report address should be returned", reportAddress);
		Assert("Report address should be the main address", reportAddress.IsMainAddress);
		Assert("Report address type should be Office", reportAddress.AddressCapability.GetCapabilityEnabledMain(OrgConstants.AddressType.Office));
	}

	public void TestGetReportAddress_ReturnsNull_WhenNoMatchingAddress()
	{
		var complianceReport = Factory.NewWithValidTestData<AccComplianceReport>();
		var zmReport = new ZMGermanyReport(complianceReport);
		var orgProxy = zmReport.ComplianceReport.Company.OrgProxy;

		orgProxy.AddressesActive.Where(address =>
				address.AddressCapability.GetCapabilityEnabled(OrgConstants.AddressType.Office))
			.ForEach(officeAddress => officeAddress.AddressCapability
				.Where(capability => capability.AddressCapabilityType == OrgConstants.AddressType.Office).DeleteAll());

		var reportAddress = AddressHelper.GetReportAddress(orgProxy);

		AssertNull("When no address matched, returns null", reportAddress);
	}

	protected override void SetUp()
	{
		base.SetUp();
		ComplianceReport = Factory.NewWithValidTestData<AccComplianceReport>();
		ComplianceReport.ACR_ReportType = AccountingConstants.ComplianceReportTypes.ZusammenfassendeMeldungGermanyReportType;
		ZmReport = new (ComplianceReport);
		Factory.Save();
	}

	ZMGermanyReport ZmReport;
	AccComplianceReport ComplianceReport;
}
