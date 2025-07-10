using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	public class CreateDeclarationHelperTest : TestCaseWithFactory
	{
		public void TestGetNewImportJobDeclaration()
		{
			var helper = new CreateDeclarationHelper();
			var shipment = Factory.New<ForwardingShipment>();
			AssertEquals(typeof(ImportDeclaration), helper.GetNewImportJobDeclaration(shipment).GetType());
		}
	}
}
