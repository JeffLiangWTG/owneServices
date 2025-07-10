using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.DE.Business.CodeDescriptionPairLists;

namespace Enterprise.Customs.DE.Business.Declaration.Testing
{
	sealed class AddInfoJobComInvoiceHeaderLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestTransportChargesModeOfPayment()
		{
			CombineAssertions(() =>
			{
				var transportChargesMethodOfPaymentList = lookups.TransportChargesMethodOfPaymentList;
				AssertEquals("Codes", "A, B, C, D, H, Y, Z", transportChargesMethodOfPaymentList.CodesAsString);
				AssertSame("Cached", Factory.GetCachedValue<DEExportMethodOfPaymentList>(), transportChargesMethodOfPaymentList);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			var invoiceHeader = Factory.New<JobComInvoiceHeader>();
			var addInfo = new AddInfoJobComInvoiceHeader(invoiceHeader.JZ_AddInfoInfo);
			lookups = new AddInfoJobComInvoiceHeaderLookups(addInfo);
		}
		AddInfoJobComInvoiceHeaderLookups lookups;
	}
}
