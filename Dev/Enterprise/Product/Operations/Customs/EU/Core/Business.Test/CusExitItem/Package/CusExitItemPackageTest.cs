using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Testing
{
	[TestedType(typeof(CusExitItemPackage))]
	public class CusExitItemPackageTest : Customs.Business.Testing.CusInvPackTest<CusExitItem>
	{
		[ExpectNoExceptions]
		public void TestLookupsType()
		{
			var exitHeader = Factory.NewWithValidTestData<CusExitControlHeader>();
			var exitDetail = exitHeader.CusExitDetails.AddNew();
			var exitItem = exitDetail.CusExitItems.AddNew();
			var package = exitItem.Packages.AddNew();
			NUnit.Framework.Assert.That(package.Lookups, NUnit.Framework.Is.TypeOf<CusExitItemPackageLookups>());
		}

		protected override CusExitItem GetNewParent()
		{
			var exitHeader = Factory.NewWithValidTestData<CusExitControlHeader>();
			var exitDetail = exitHeader.CusExitDetails.AddNew();
			var exitItem = exitDetail.CusExitItems.AddNew();
			return exitItem;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewBusinessObject();

		protected override BusinessObject GetNewBusinessObject()
		{
			var exitHeader = Factory.NewWithValidTestData<CusExitControlHeader>();
			var exitDetail = exitHeader.CusExitDetails.AddNew();
			var exitItem = exitDetail.CusExitItems.AddNew();
			var package = exitItem.Packages.AddNew();
			return package;
		}
	}
}
