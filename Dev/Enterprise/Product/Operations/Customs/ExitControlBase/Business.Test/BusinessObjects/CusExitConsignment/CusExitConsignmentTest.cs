using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ExitControlBase.Business.Testing;

[TestedType(typeof(CusExitConsignment))]
sealed class CusExitConsignmentTest : EnterpriseBusinessObjectTestCase
{
	public void TestSingleBusinessObjectAroundARow()
	{
		AssertEquals(1, typeof(CusExitConsignment).GetCustomAttributes(typeof(SingleObjectAroundARow), false).Length);
	}

	public void TestHeader()
	{
		(var consignment, var header) = GetNewBusinessObject(Factory);
		AssertEquals(header.PK, consignment.Header.PK);
	}

	public void TestCusExitConsignmentItems()
	{
		var consignment = Factory.New<CusExitConsignment>();
		var consignmentItem1 = Factory.New<CusExitConsignmentItem>();
		consignmentItem1.CCI_CXC_Consignment = consignment.PK;
		var consignmentItem2 = Factory.New<CusExitConsignmentItem>();
		consignmentItem2.CCI_CXC_Consignment = consignment.PK;
		var consignmentItem3 = Factory.New<CusExitConsignmentItem>();
		consignmentItem3.CCI_CXC_Consignment = Factory.New<CusExitConsignment>().PK;
		AssertContainsExactElementsInAnyOrder(new[] { consignmentItem1.PK, consignmentItem2.PK }, consignment.CusExitConsignmentItems.Select(x => x.PK));
	}

	public void TestValidation()
	{
		(var consignment, _) = GetNewBusinessObject(Factory);
		AssertType<CusExitConsignmentValidation>(consignment.Validation);
	}

	public void TestLookups()
	{
		(var consignment, _) = GetNewBusinessObject(Factory);
		AssertType<CusExitConsignmentLookups>(consignment.Lookups);
	}

	public void TestDelete()
	{
		(var consignment, _) = GetNewBusinessObject(Factory);
		var consignmentItem = consignment.CusExitConsignmentItems.AddNew();
		var report = consignment.Header.CusExitReports.AddNew();
		report.CER_CXC_Consignment = consignment.PK;

		consignment.Delete();
		AssertEquals("CusExitConsignmentItem should be deleted.", true, consignmentItem.IsDeleted);
		AssertEquals("CusExitReport should be deleted.", true, report.IsDeleted);
	}

	protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewBusinessObject(Factory).consignment;

	protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObject(Factory).consignment;

	protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObject(Factory).consignment;

	public static (CusExitConsignment consignment, CusExitHeader header) GetNewBusinessObject(BusinessObjectFactory factory)
	{
		var header = CusExitHeaderTest.GetNewBusinessObject(factory);
		header.CXH_JobReference = header.PK.ToString().Substring(0, 35);
		var consignment = header.CusExitConsignments.AddNew();
		consignment.CXC_Status = "REJ";
		return (consignment, header);
	}
}
