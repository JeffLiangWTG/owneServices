using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Testing
{
	[TestedType(typeof(CusExitItem))]
	class CusExitItemTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var exitHeader = Factory.NewWithValidTestData<CusExitControlHeader>();
			var exitDetail = exitHeader.CusExitDetails.AddNew();
			var exitItem = exitDetail.CusExitItems.AddNew();
			return exitItem;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewBusinessObject();
	}
}
