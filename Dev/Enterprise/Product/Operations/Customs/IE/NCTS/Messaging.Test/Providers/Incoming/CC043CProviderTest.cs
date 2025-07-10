using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.CC043C;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.IE.NCTS.Messaging.Testing
{
	class CC043CProviderTest : TestCaseWithFactory
	{
		public void TestNullValue()
		{
			CombineAssertions(() =>
			{
				var emptyProvider = new CC043CProvider(new Cc043CType());
				AssertEquals("MRN", ZString.Empty, emptyProvider.MRN);
				AssertEquals("CustomsOfficeOfDestinationActual", ZString.Empty, emptyProvider.CustomsOfficeOfDestinationActual);
				AssertEquals("TraderAtDestination", ZString.Empty, emptyProvider.TraderAtDestination);
				AssertEquals("Address", ZString.Empty, emptyProvider.Address);
			});
		}

		public void TestMRN()
		{
			AssertEquals("MRN", "19AA12345678901230", provider.MRN);
		}

		public void TestSecurity()
		{
			AssertEquals("Security", "1", provider.Security);
		}

		public void TestCustomsOfficeOfDestinationActual()
		{
			AssertEquals("CustomsOfficeOfDestinationActual", "RNALPHN9", provider.CustomsOfficeOfDestinationActual);
		}

		public void TestTraderAtDestination()
		{
			AssertEquals("TraderAtDestination", "IN043", provider.TraderAtDestination);
		}

		public void TestAddress()
		{
			AssertEquals("Address", "123 WHERE ST, CITY, IE", provider.Address);
		}

		public void TestConsignment()
		{
			AssertType<CC043CConsignmentProvider>(provider.Consignment);
		}

		protected override void SetUp()
		{
			base.SetUp();
			InterchangeProcessorTestHelper.Security = "1";
			provider = new CC043CProvider(InterchangeProcessorTestHelper.GetStandardCC043C("19AA12345678901230"));
		}
		CC043CProvider provider;
	}
}
