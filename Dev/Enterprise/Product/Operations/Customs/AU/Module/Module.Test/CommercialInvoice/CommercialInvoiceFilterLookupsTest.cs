using Enterprise.Customs.Common.AU;

namespace Enterprise.Customs.AU.Module.Testing
{
	sealed class CommercialInvoiceFilterLookupsTest : Customs.Module.Testing.CommercialInvoiceFilterLookupsTest
	{
		public override void TestMessageTypesList()
		{
			var filterBizObj = new CommercialInvoiceFilterBusinessObject();
			var lookups = new CommercialInvoiceFilterLookups(filterBizObj);
			var messageTypes = lookups.MessageTypes;
			AssertEquals("Should contains the AQS on a AU commercial invoice.", true, messageTypes.ContainsCode(AUJobMessageTypeList.Codes.Quarantine));
		}
	}
}
