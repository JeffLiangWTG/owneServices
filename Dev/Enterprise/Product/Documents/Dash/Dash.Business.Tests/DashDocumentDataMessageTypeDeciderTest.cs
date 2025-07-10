using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentScanning.Business;

namespace Enterprise.Dash.Business.Tests
{
	public sealed class DashDocumentDataMessageTypeDeciderTest : TestCaseWithFactory
	{
		public void TestGetTypeForLoad_When_ApplicationCode_Is_DashDocumentDataProcessing()
		{
			var typeDecider = new DashDocumentDataMessageTypeDecider();
			var masterFactory = new DocumentFactoryProvider().GetFactory(Factory);
			var message = masterFactory.New<DashDocumentDataMessage>();
			var row = ((INeedRow)message).Row;

			AssertEquals(typeof(DashDocumentDataMessage), typeDecider.GetTypeForLoad(row, Factory));
		}

		public void TestGetTypeForNew()
		{
			var typeDecider = new DashDocumentDataMessageTypeDecider();
			AssertNull("GetTypeForNew should return null", typeDecider.GetTypeForNew());
		}

		public void TestGetTypeForBinding()
		{
			var typeDecider = new DashDocumentDataMessageTypeDecider();
			AssertNull("GetTypeForBinding return null", typeDecider.GetTypeForBinding());
		}
	}
}
