using CargoWise.Definitions;
using CargoWise.EntityFramework;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.Testing;

namespace Enterprise.DocumentWrappers.Testing.DocumentTests.RunDocuments.RunDocsSectionGroupingTest
{
	internal class RunDocGroupByClientRateTest : RunDocGroupByBaseTest
	{
		protected override string TestDocBuilder_SimilarDestinationFreightRates_SimilarDestination_EmptyCarrierServiceLevel_ExpectedOutput => @"{C}-[Test Client #3 Client Rate]


{C}-[AIR - Air Freight Rates]

{C}-[ From Sydney To Auckland]   {AZ}-[Validity: 01-Jan-20  -  01-Jul-20]

{C}-[Charge Description]   {AA}-[Cur.]   {AE}-[Flat]
{C}-[International Freight]   {AA}-[AUD]   {AE}-[10.00]





{S}-[Continued Over… ]   {BE}-[Page 1 of 2]
{C}-[AIR - Air Destination Charges]

{C}-[Auckland (from Sydney)]
{C}-[Destination Documentation Fee]
{AI}-[NZD]   {AM}-[20.00]
{E}-[-  Carrier Service Level: AM]


{C}-[New Zealand (from Sydney)]
{C}-[Destination Documentation Fee]
{AI}-[NZD]   {AM}-[21.00]





{S}-[END OF DOCUMENT ]   {BE}-[Page 2 of 2]
{S}-[END OF DOCUMENT ]   {BE}-[Page 1 of 1]


";

		protected override string TestDocBuilder_SimilarDestinationFreightRates_SimilarDestination_EmptyServiceLevel_ExpectedOutput => @"{C}-[Test Client #3 Client Rate]


{C}-[AIR - Air Freight Rates]

{C}-[ From Sydney To Auckland]   {AZ}-[Validity: 01-Jan-20  -  01-Jul-20]

{C}-[Charge Description]   {AA}-[Cur.]   {AE}-[Flat]
{C}-[International Freight]   {AA}-[AUD]   {AE}-[10.00]





{S}-[Continued Over… ]   {BE}-[Page 1 of 2]
{C}-[AIR - Air Destination Charges]

{C}-[Auckland (from Sydney)]   {AS}-[Service Level: AM]
{C}-[Destination Documentation Fee]
{E}-[ AM]   {AI}-[NZD]   {AM}-[20.00]


{C}-[New Zealand (from Sydney)]
{C}-[Destination Documentation Fee]
{AI}-[NZD]   {AM}-[21.00]





{S}-[END OF DOCUMENT ]   {BE}-[Page 2 of 2]
{S}-[END OF DOCUMENT ]   {BE}-[Page 1 of 1]


";

		protected override string TestDocBuilder_SimilarDestinationFreightRates_SimilarOrigin_EmptyCarrierServiceLevel_ExpectedOutput => @"{C}-[Test Client #3 Client Rate]


{C}-[AIR - Air Freight Rates]

{C}-[ From Sydney To Auckland]   {AZ}-[Validity: 01-Jan-20  -  01-Jul-20]

{C}-[Charge Description]   {AA}-[Cur.]   {AE}-[Flat]
{C}-[International Freight]   {AA}-[AUD]   {AE}-[10.00]





{S}-[Continued Over… ]   {BE}-[Page 1 of 2]
{C}-[AIR - Air Destination Charges]

{C}-[Auckland (from Australia)]
{C}-[Destination Documentation Fee]
{AI}-[NZD]   {AM}-[21.00]


{C}-[Auckland (from Sydney)]
{C}-[Destination Documentation Fee]
{AI}-[NZD]   {AM}-[20.00]
{E}-[-  Carrier Service Level: AM]





{S}-[END OF DOCUMENT ]   {BE}-[Page 2 of 2]
{S}-[END OF DOCUMENT ]   {BE}-[Page 1 of 1]


";

		protected override string TestDocBuilder_SimilarDestinationFreightRates_SimilarOrigin_EmptyServiceLevel_ExpectedOutput => @"{C}-[Test Client #3 Client Rate]


{C}-[AIR - Air Freight Rates]

{C}-[ From Sydney To Auckland]   {AZ}-[Validity: 01-Jan-20  -  01-Jul-20]

{C}-[Charge Description]   {AA}-[Cur.]   {AE}-[Flat]
{C}-[International Freight]   {AA}-[AUD]   {AE}-[10.00]





{S}-[Continued Over… ]   {BE}-[Page 1 of 2]
{C}-[AIR - Air Destination Charges]

{C}-[Auckland (from Australia)]
{C}-[Destination Documentation Fee]
{AI}-[NZD]   {AM}-[21.00]


{C}-[Auckland (from Sydney)]   {AS}-[Service Level: AM]
{C}-[Destination Documentation Fee]
{E}-[ AM]   {AI}-[NZD]   {AM}-[20.00]





{S}-[END OF DOCUMENT ]   {BE}-[Page 2 of 2]
{S}-[END OF DOCUMENT ]   {BE}-[Page 1 of 1]


";

		protected override string TestDocBuilder_SimilarDestinationFreightRates_SimilarOrigin_SameCommodity_ExpectedOutput => @"{C}-[Test Client #3 Client Rate]


{C}-[AIR - Air Freight Rates]

{C}-[ From Sydney To Auckland]   {AZ}-[Validity: 01-Jan-20  -  01-Jul-20]

{C}-[Charge Description]   {AA}-[Cur.]   {AE}-[Flat]
{C}-[International Freight]   {AA}-[AUD]   {AE}-[10.00]





{S}-[Continued Over… ]   {BE}-[Page 1 of 2]
{C}-[AIR - Air Destination Charges]

{C}-[Auckland (from Australia)]   {BG}-[Commodity Code: HAZ]
{C}-[Destination Documentation Fee]   {AI}-[NZD]   {AM}-[21.00]


{C}-[Auckland (from Sydney)]   {BG}-[Commodity Code: HAZ]
{C}-[Destination Documentation Fee  HAZ]   {AI}-[NZD]   {AM}-[20.00]





{S}-[END OF DOCUMENT ]   {BE}-[Page 2 of 2]
{S}-[END OF DOCUMENT ]   {BE}-[Page 1 of 1]


";

		protected override string TestDocBuilder_SimilarDestinationFreightRates_SimilarOrigin_DifferentCommodity_ExpectedOutput => @"{C}-[Test Client #3 Client Rate]


{C}-[AIR - Air Freight Rates]

{C}-[ From Sydney To Auckland]   {AZ}-[Validity: 01-Jan-20  -  01-Jul-20]

{C}-[Charge Description]   {AA}-[Cur.]   {AE}-[Flat]
{C}-[International Freight]   {AA}-[AUD]   {AE}-[10.00]





{S}-[Continued Over… ]   {BE}-[Page 1 of 2]
{C}-[AIR - Air Destination Charges]

{C}-[Auckland (from Australia)]   {BG}-[Commodity Code: ALUM]
{C}-[Destination Documentation Fee]
{E}-[ ALUM]   {AI}-[NZD]   {AM}-[21.00]


{C}-[Auckland (from Sydney)]   {BG}-[Commodity Code: HAZ]
{C}-[Destination Documentation Fee]
{E}-[ HAZ]   {AI}-[NZD]   {AM}-[20.00]





{S}-[END OF DOCUMENT ]   {BE}-[Page 2 of 2]
{S}-[END OF DOCUMENT ]   {BE}-[Page 1 of 1]


";

		protected override string TestDocBuilder_SimilarDestinationFreightRates_DifferentOrigin_ExpectedOutput => @"{C}-[Test Client #3 Client Rate]


{C}-[AIR - Air Freight Rates]

{C}-[ From Sydney To Auckland]   {AZ}-[Validity: 01-Jan-20  -  01-Jul-20]

{C}-[Charge Description]   {AA}-[Cur.]   {AE}-[Flat]
{C}-[International Freight]   {AA}-[AUD]   {AE}-[10.00]





{S}-[Continued Over… ]   {BE}-[Page 1 of 2]
{C}-[AIR - Air Destination Charges]

{C}-[Auckland (from Australia)]
{C}-[Destination Documentation Fee]   {AI}-[NZD]   {AM}-[21.00]


{C}-[Auckland (from Australia East Coast)]
{C}-[Destination Documentation Fee]   {AI}-[NZD]   {AM}-[20.00]





{S}-[END OF DOCUMENT ]   {BE}-[Page 2 of 2]
{S}-[END OF DOCUMENT ]   {BE}-[Page 1 of 1]


";

		protected override string TestDocBuilder_SimilarDestinationFreightRates_SameTransportProvider_ExpectedOutput => @"{C}-[Test Client #3 Client Rate]


{C}-[AIR - Air Freight Rates]

{C}-[ From Sydney To Auckland]   {AZ}-[Validity: 01-Jan-20  -  01-Jul-20]

{C}-[Charge Description]   {AA}-[Cur.]   {AE}-[Flat]
{C}-[International Freight]   {AA}-[AUD]   {AE}-[10.00]





{S}-[Continued Over… ]   {BE}-[Page 1 of 2]
{C}-[AIR - Air Destination Charges]

{C}-[Auckland (from Australia) - TEST CLIENT #1]
{C}-[Destination Documentation Fee]
{AI}-[NZD]   {AM}-[20.00]
{E}-[-  Transport Provider: Test Client #1]
{AI}-[NZD]   {AM}-[21.00]
{E}-[-  Transport Provider: Test Client #1]





{S}-[END OF DOCUMENT ]   {BE}-[Page 2 of 2]
{S}-[END OF DOCUMENT ]   {BE}-[Page 1 of 1]


";

		protected override string TestDocBuilder_SimilarDestinationFreightRates_DifferentTransportProvider_ExpectedOutput => @"{C}-[Test Client #3 Client Rate]


{C}-[AIR - Air Freight Rates]

{C}-[ From Sydney To Auckland]   {AZ}-[Validity: 01-Jan-20  -  01-Jul-20]

{C}-[Charge Description]   {AA}-[Cur.]   {AE}-[Flat]
{C}-[International Freight]   {AA}-[AUD]   {AE}-[10.00]





{S}-[Continued Over… ]   {BE}-[Page 1 of 2]
{C}-[AIR - Air Destination Charges]

{C}-[Auckland (from Australia) - TEST CLIENT #1]
{C}-[Destination Documentation Fee]
{AI}-[NZD]   {AM}-[20.00]
{E}-[-  Transport Provider: Test Client #1]


{C}-[Auckland (from Australia) - TEST CLIENT #2]
{C}-[Destination Documentation Fee]
{AI}-[NZD]   {AM}-[21.00]
{E}-[-  Transport Provider: Test Client #2]





{S}-[END OF DOCUMENT ]   {BE}-[Page 2 of 2]
{S}-[END OF DOCUMENT ]   {BE}-[Page 1 of 1]


";

		protected override string TestDocBuilder_SimilarDestinationFreightRates_SameCarrierServiceLevel_ExpectedOutput => @"{C}-[Test Client #3 Client Rate]


{C}-[AIR - Air Freight Rates]

{C}-[ From Sydney To Auckland]   {AZ}-[Validity: 01-Jan-20  -  01-Jul-20]

{C}-[Charge Description]   {AA}-[Cur.]   {AE}-[Flat]
{C}-[International Freight]   {AA}-[AUD]   {AE}-[10.00]





{S}-[Continued Over… ]   {BE}-[Page 1 of 2]
{C}-[AIR - Air Destination Charges]

{C}-[Auckland (from Australia)]
{C}-[Destination Documentation Fee]
{AI}-[NZD]   {AM}-[20.00]
{E}-[-  Carrier Service Level: AM]
{AI}-[NZD]   {AM}-[21.00]
{E}-[-  Carrier Service Level: AM]





{S}-[END OF DOCUMENT ]   {BE}-[Page 2 of 2]
{S}-[END OF DOCUMENT ]   {BE}-[Page 1 of 1]


";

		// carrierServiceLevel is only for costing hence not considered to be grouped into its own section in ClientRate
		protected override string TestDocBuilder_SimilarDestinationFreightRates_DifferentCarrierServiceLevel_ExpectedOutput => @"{C}-[Test Client #3 Client Rate]


{C}-[AIR - Air Freight Rates]

{C}-[ From Sydney To Auckland]   {AZ}-[Validity: 01-Jan-20  -  01-Jul-20]

{C}-[Charge Description]   {AA}-[Cur.]   {AE}-[Flat]
{C}-[International Freight]   {AA}-[AUD]   {AE}-[10.00]





{S}-[Continued Over… ]   {BE}-[Page 1 of 2]
{C}-[AIR - Air Destination Charges]

{C}-[Auckland (from Australia)]
{C}-[Destination Documentation Fee]
{AI}-[NZD]   {AM}-[20.00]
{E}-[-  Carrier Service Level: AM]
{AI}-[NZD]   {AM}-[21.00]
{E}-[-  Carrier Service Level: XX]





{S}-[END OF DOCUMENT ]   {BE}-[Page 2 of 2]
{S}-[END OF DOCUMENT ]   {BE}-[Page 1 of 1]


";

		protected override string TestDocBuilder_SimilarDestinationFreightRates_SameServiceLevel_ExpectedOutput => @"{C}-[Test Client #3 Client Rate]


{C}-[AIR - Air Freight Rates]

{C}-[ From Sydney To Auckland]   {AZ}-[Validity: 01-Jan-20  -  01-Jul-20]

{C}-[Charge Description]   {AA}-[Cur.]   {AE}-[Flat]
{C}-[International Freight]   {AA}-[AUD]   {AE}-[10.00]





{S}-[Continued Over… ]   {BE}-[Page 1 of 2]
{C}-[AIR - Air Destination Charges]

{C}-[Auckland (from Australia)]   {AS}-[Service Level: AM]
{C}-[Destination Documentation Fee]
{E}-[ AM]   {AI}-[NZD]   {AM}-[20.00]
{E}-[ AM]   {AI}-[NZD]   {AM}-[21.00]





{S}-[END OF DOCUMENT ]   {BE}-[Page 2 of 2]
{S}-[END OF DOCUMENT ]   {BE}-[Page 1 of 1]


";

		protected override string TestDocBuilder_SimilarDestinationFreightRates_DifferentServiceLevel_ExpectedOutput => @"{C}-[Test Client #3 Client Rate]


{C}-[AIR - Air Freight Rates]

{C}-[ From Sydney To Auckland]   {AZ}-[Validity: 01-Jan-20  -  01-Jul-20]

{C}-[Charge Description]   {AA}-[Cur.]   {AE}-[Flat]
{C}-[International Freight]   {AA}-[AUD]   {AE}-[10.00]





{S}-[Continued Over… ]   {BE}-[Page 1 of 2]
{C}-[AIR - Air Destination Charges]

{C}-[Auckland (from Australia)]   {AS}-[Service Level: AM]
{C}-[Destination Documentation Fee]
{E}-[ AM]   {AI}-[NZD]   {AM}-[20.00]


{C}-[Auckland (from Australia)]   {AS}-[Service Level: XX]
{C}-[Destination Documentation Fee]
{E}-[ XX]   {AI}-[NZD]   {AM}-[21.00]





{S}-[END OF DOCUMENT ]   {BE}-[Page 2 of 2]
{S}-[END OF DOCUMENT ]   {BE}-[Page 1 of 1]


";

		protected override string TestDocBuilder_SimilarDestinationFreightRates_SameCommodity_ExpectedOutput => @"{C}-[Test Client #3 Client Rate]


{C}-[AIR - Air Freight Rates]

{C}-[ From Sydney To Auckland]   {AZ}-[Validity: 01-Jan-20  -  01-Jul-20]

{C}-[Charge Description]   {AA}-[Cur.]   {AE}-[Flat]
{C}-[International Freight]   {AA}-[AUD]   {AE}-[10.00]





{S}-[Continued Over… ]   {BE}-[Page 1 of 2]
{C}-[AIR - Air Destination Charges]

{C}-[Auckland (from Australia)]   {BG}-[Commodity Code: HAZ]
{C}-[Destination Documentation Fee]
{E}-[ HAZ]   {AI}-[NZD]   {AM}-[20.00]
{E}-[ HAZ]   {AI}-[NZD]   {AM}-[21.00]





{S}-[END OF DOCUMENT ]   {BE}-[Page 2 of 2]
{S}-[END OF DOCUMENT ]   {BE}-[Page 1 of 1]


";

		protected override string TestDocBuilder_SimilarDestinationFreightRates_DifferentCommodity_ExpectedOutput => @"{C}-[Test Client #3 Client Rate]


{C}-[AIR - Air Freight Rates]

{C}-[ From Sydney To Auckland]   {AZ}-[Validity: 01-Jan-20  -  01-Jul-20]

{C}-[Charge Description]   {AA}-[Cur.]   {AE}-[Flat]
{C}-[International Freight]   {AA}-[AUD]   {AE}-[10.00]





{S}-[Continued Over… ]   {BE}-[Page 1 of 2]
{C}-[AIR - Air Destination Charges]

{C}-[Auckland (from Australia)]   {BG}-[Commodity Code: ALUM]
{C}-[Destination Documentation Fee]
{E}-[ ALUM]   {AI}-[NZD]   {AM}-[21.00]


{C}-[Auckland (from Australia)]   {BG}-[Commodity Code: GEN]
{C}-[Destination Documentation Fee]
{E}-[ GEN]   {AI}-[NZD]   {AM}-[20.00]





{S}-[END OF DOCUMENT ]   {BE}-[Page 2 of 2]
{S}-[END OF DOCUMENT ]   {BE}-[Page 1 of 1]


";

		public override BusinessObject GetBusinessObject => clientRate ?? (clientRate = TestHelper.NewClientRate(TestHelper.NewOrgHeader(1)));
		ClientRate clientRate;

		public override BusinessContext BusinessContext => BusinessContext.Rating;
	}
}
