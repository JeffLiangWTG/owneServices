using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ExitControlBase.Business.Testing;

[TestedType(typeof(CusExitReportItem))]
sealed class CusExitReportItemTest : EnterpriseBusinessObjectTestCase
{
	public void TestSingleBusinessObjectAroundARow()
	{
		AssertEquals(1, typeof(CusExitReportItem).GetCustomAttributes(typeof(SingleObjectAroundARow), false).Length);
	}

	public void TestValidation()
	{
		(var reportItem, _, _) = GetNewBusinessObject(Factory);
		AssertType<CusExitReportItemValidation>(reportItem.Validation);
	}

	public void TestLookups()
	{
		(var reportItem, _, _) = GetNewBusinessObject(Factory);
		AssertType<CusExitReportItemLookups>(reportItem.Lookups);
	}

	public void TestReport()
	{
		(var reportItem, var report, _) = GetNewBusinessObject(Factory);
		AssertEquals(report.PK, reportItem.Report.PK);
	}

	public void TestPackage()
	{
		(var reportItem, _, _) = GetNewBusinessObject(Factory);
		var package = Factory.New<CusExitConsignmentPackage>();
		reportItem.ERI_CXP_Package = package.PK;
		AssertEquals(package.PK, reportItem.Package.PK);
	}

	public void TestConsignmentItem()
	{
		(var reportItem, var report, var consignmentItem) = GetNewBusinessObject(Factory);
		AssertEquals(consignmentItem.PK, reportItem.ConsignmentItem.PK);
	}

	protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObject(Factory).Item;

	protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewBusinessObject(Factory).Item;

	protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObject(Factory).Item;

	public static (CusExitReportItem Item, CusExitReport Report, CusExitConsignmentItem ExitConsignmentItem) GetNewBusinessObject(BusinessObjectFactory factory)
	{
		var header = factory.New<CusExitHeader>();
		header.CXH_JobReference = header.PK.ToString().Substring(0, 35);
		header.CXH_ApplicationCode = Common.CusExitHeaderApplicationCodeList.Codes.ExitControl;
		var consignment = header.CusExitConsignments.AddNew();
		var consignmentItem = consignment.CusExitConsignmentItems.AddNew();
		consignmentItem.CCI_LineNumber = 1;
		var report = header.CusExitReports.AddNew();
		report.CER_CXC_Consignment = consignment.PK;
		report.CER_DateTime = ZDateTimeOffset.Now;
		report.CER_OfficeOfExit = "DE001";
		var reportItem = report.CusExitReportItems.AddNew();
		reportItem.ERI_CCI_ConsignmentItem = consignmentItem.PK;
		return (reportItem, report, consignmentItem);
	}
}
