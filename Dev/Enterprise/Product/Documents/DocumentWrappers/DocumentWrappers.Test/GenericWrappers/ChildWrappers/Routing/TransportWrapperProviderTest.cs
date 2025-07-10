using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	sealed class TransportWrapperProviderTest : TestCaseWithFactory
	{
		[TestDate(2021, 4, 29)]
		public void TestGetTransportReference()
		{
			var testObjectCreator = new TestObjectCreator(Factory);

			var declaration = (BaseJobDeclaration)testObjectCreator.CreateDeclaration("D0001");

			var transport = declaration.TransportsIncludingRelated.AddNew();
			transport.JW_VoyageFlight = "TestFlight";
			transport.JW_Vessel = "TestVessel";
			transport.JW_ETD = ZDateTime.Now.AddDays(-1);

			var transportWrapperProvider = new TransportWrapperProvider();
			var transportReference = transportWrapperProvider.GetTransportReference(declaration, Factory);

			AssertEquals("The type of created object should be DocDisbursementJobsCloseBatch", "TestVessel / TestFlight / 28-Apr", transportReference);
		}
	}
}
