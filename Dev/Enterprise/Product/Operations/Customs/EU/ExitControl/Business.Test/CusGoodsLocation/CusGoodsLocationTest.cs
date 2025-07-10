using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.ExitControl.Business.Testing;

[TestedType(typeof(CusGoodsLocation))]
sealed class CusGoodsLocationTest : EnterpriseBusinessObjectTestCase
{
	public void TestTypeDecider() => AssertType<CusGoodsLocationTypeDecider>(CusGoodsLocation.TypeDecider);

	public void TestAddress() => AssertType<CusGoodsLocationAddress>(((CusGoodsLocation)GetNewBusinessObject()).Address);

	protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewBusinessObject();

	protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObject();

	protected override BusinessObject GetNewBusinessObject()
	{
		var header = Factory.GetUcc6ExitHeader();
		var consignment = header.CusExitConsignments.AddNew();
		var report = header.CusExitReports.AddNew();
		report.CER_CXC_Consignment = consignment.PK;

		return report.GoodsLocation;
	}
}
