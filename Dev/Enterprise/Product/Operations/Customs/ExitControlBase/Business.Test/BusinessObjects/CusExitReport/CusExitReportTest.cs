using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ExitControlBase.Business.Testing;

[TestedType(typeof(CusExitReport))]
sealed class CusExitReportTest : EnterpriseBusinessObjectTestCase
{
	public void TestSingleBusinessObjectAroundARow()
	{
		AssertEquals(1, typeof(CusExitReport).GetCustomAttributes(typeof(SingleObjectAroundARow), false).Length);
	}

	public void TestValidation()
	{
		(var report, _) = GetNewBusinessObject(Factory);
		AssertType<CusExitReportValidation>(report.Validation);
	}

	public void TestLookups()
	{
		(var report, _) = GetNewBusinessObject(Factory);
		AssertType<CusExitReportLookups>(report.Lookups);
	}

	public void TestCusExitReportItems()
	{
		(var report, var header) = GetNewBusinessObject(Factory);
		var consignment = header.CusExitConsignments.AddNew();
		var consignmentItem = consignment.CusExitConsignmentItems.AddNew();
		consignmentItem.CCI_LineNumber = 1;
		var consignmentItem2 = consignment.CusExitConsignmentItems.AddNew();
		consignmentItem2.CCI_LineNumber = 2;
		var reportItem1 = report.CusExitReportItems.AddNew();
		reportItem1.ERI_CCI_ConsignmentItem = consignmentItem.PK;
		var reportItem2 = report.CusExitReportItems.AddNew();
		reportItem2.ERI_CCI_ConsignmentItem = consignmentItem.PK;
		var reportItem3 = report.CusExitReportItems.AddNew();
		reportItem3.ERI_CCI_ConsignmentItem = consignmentItem2.PK;
		var reportItem4 = report.CusExitReportItems.AddNew();

		AssertContainsExactElementsInAnyOrder(new[] { reportItem1.PK, reportItem2.PK, reportItem3.PK, reportItem4.PK }, report.CusExitReportItems.Select(x => x.PK));
	}

	protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObject(Factory).report;

	protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewBusinessObject(factory).report;

	protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObject(Factory).report;

	public static (CusExitReport report, CusExitHeader header) GetNewBusinessObject(BusinessObjectFactory factory)
	{
		var header = factory.New<CusExitHeader>();
		header.CXH_JobReference = header.PK.ToString().Substring(0, 35);
		header.CXH_ApplicationCode = Common.CusExitHeaderApplicationCodeList.Codes.ExitControl;
		var consignment = header.CusExitConsignments.AddNew();
		var report = header.CusExitReports.AddNew();
		report.CER_CXC_Consignment = consignment.PK;
		report.CER_DateTime = ZDateTimeOffset.Now;
		report.CER_OfficeOfExit = "DE001";
		return (report, header);
	}
}
