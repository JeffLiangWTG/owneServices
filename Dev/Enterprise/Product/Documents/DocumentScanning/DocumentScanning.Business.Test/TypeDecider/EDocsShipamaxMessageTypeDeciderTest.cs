using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.DocumentScanning.Business.Test
{
	public sealed class EDocsShipamaxMessageTypeDeciderTest : TestCaseWithFactory
	{
		public void TestGetTypeForLoad()
		{
			var typeDecider = new EDocsShipamaxMessageTypeDecider();
			var masterFactory = new DocumentFactoryProvider().GetFactory(Factory);
			var message = masterFactory.New<EDocsShipamaxMessage>();
			var row = ((INeedRow)message).Row;

			AssertEquals(typeof(EDocsShipamaxMessage), typeDecider.GetTypeForLoad(row, Factory));
		}

		public void TestGetTypeForNew()
		{
			var typeDecider = new EDocsShipamaxMessageTypeDecider();
			AssertNull("GetTypeForNew should return null", typeDecider.GetTypeForNew());
		}

		public void TestGetTypeForBinding()
		{
			var typeDecider = new EDocsShipamaxMessageTypeDecider();
			AssertNull("GetTypeForBinding return null", typeDecider.GetTypeForBinding());
		}
	}
}
