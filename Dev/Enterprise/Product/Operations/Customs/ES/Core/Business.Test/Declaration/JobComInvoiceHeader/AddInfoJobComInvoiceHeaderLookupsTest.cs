using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.ES.Business.Declaration.Testing
{
	sealed class AddInfoJobComInvoiceHeaderLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestTransportChargesModeOfPayment()
		{
			CombineAssertions(() =>
			{
				var transportChargesMethodOfPaymentList = lookups.TransportChargesMethodOfPaymentList;
				AssertEquals("Codes", "A, B, C, D, H, Y, Z", transportChargesMethodOfPaymentList.CodesAsString);
				AssertSame("Cached", Factory.GetCachedValue<TransportChargesMethodOfPaymentList>(), transportChargesMethodOfPaymentList);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			var addInfoJobComInvoiceHeader = new AddInfoJobComInvoiceHeader(Factory.New<JobComInvoiceHeader>().JZ_AddInfoInfo);
			lookups = new AddInfoJobComInvoiceHeaderLookups(addInfoJobComInvoiceHeader);
		}
		AddInfoJobComInvoiceHeaderLookups lookups;
	}
}
