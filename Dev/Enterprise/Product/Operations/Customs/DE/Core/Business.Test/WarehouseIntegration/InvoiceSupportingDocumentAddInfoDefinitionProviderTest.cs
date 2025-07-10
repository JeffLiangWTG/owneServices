using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.DE.Business.Testing
{
	sealed class InvoiceSupportingDocumentAddInfoDefinitionProviderTest : TestCaseWithFactory
	{
		public void TestConstructor_NullParameter()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new InvoiceSupportingDocumentAddInfoDefinitionProvider(null));
		}

		public void TestType()
		{
			AssertEquals("N380", provider.Type);
		}

		public void TestReference()
		{
			AssertEquals("ref", provider.Reference);
		}

		public void TestDateOfIssue()
		{
			AssertEquals(new ZDateTime(2023, 8, 31), provider.DateOfIssue);
		}

		public void TestAvailable()
		{
			AssertEquals("N", provider.Available);
		}

		public void TestQuantity()
		{
			AssertEquals(12m, provider.Quantity);
		}

		public void TestUnitOfMeasure()
		{
			AssertEquals("019", provider.UnitOfMeasure);
		}

		protected override void SetUp()
		{
			base.SetUp();

			var cusAddInfo = Factory.New<WarehouseCustomsAddInfo>();
			cusAddInfo.B7_AddInfoData =
				"*Type=N380*Reference=ref*DateOfIssue=2023-08-31 00:00:00.000*Available=N*Quantity=12*UnitofMeasure=019";
			provider = new InvoiceSupportingDocumentAddInfoDefinitionProvider(cusAddInfo);
		}
		InvoiceSupportingDocumentAddInfoDefinitionProvider provider;
	}
}
