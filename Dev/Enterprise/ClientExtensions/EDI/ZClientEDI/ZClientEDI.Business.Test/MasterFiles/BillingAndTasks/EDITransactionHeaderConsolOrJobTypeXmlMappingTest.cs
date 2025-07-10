using Enterprise.DataTransfer.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	[TestedType(typeof(EDITransactionHeaderConsolOrJobTypeXmlMapping))]
	public class EDITransactionHeaderConsolOrJobTypeXmlMappingTest : EnterpriseCodeExternalCodeMappingsTest
	{
		public override void TestNoNewCodesAdded()
		{
			EDITransactionHeaderConsolOrJobTypeXmlMapping mappings = (EDITransactionHeaderConsolOrJobTypeXmlMapping)GetNewMappings();
			AssertNotNull(mappings);
			Assert(mappings.ContainsEnterpriseCode(EDIJobInvoicingConsumerTypes.Incident.Code));
			Assert(mappings.ContainsEnterpriseCode(EDIJobInvoicingConsumerTypes.PSQuote.Code));
		}

		protected override DataTransfer.Business.EnterpriseCodeExternalCodeMappings GetNewMappings()
		{
			return EDITransactionHeaderConsolOrJobTypeXmlMapping.New();
		}
	}
}
