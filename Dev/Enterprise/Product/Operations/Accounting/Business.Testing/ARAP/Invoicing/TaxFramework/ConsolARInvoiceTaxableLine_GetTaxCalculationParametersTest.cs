using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.Testing.ARAP.Invoicing.TaxFramework
{
	class ConsolARInvoiceTaxableLine_GetTaxCalculationParametersTest : ConsolRelatedTaxableLine_GetTaxCalculationParametersTest
	{
		protected override InvoicingLineBase CreateInvoiceLine(int attemptNumber, OrgHeader invoiceOrg)
		{
			var invoice = Creator.CreateInvoice(typeof(ARInvoice), organisation: invoiceOrg);
			var line = Creator.CreateInvoiceLine(invoice, 100M);
			var consol = Creator.CreateConsol(consolNum: "C00" + attemptNumber, transportMode: TransportModes.Air);
			var shipment = Creator.CreateShipment("S00" + attemptNumber, consol);
			var job = Creator.CreateJob(shipment, createWithMutex: false);
			line.AL_JH = job.PK;

			Creator.SetupConsolRelatedARInvoice(invoice, consol);

			return line;
		}

		protected override ZString GetExpectedJobType()
		{
			return JobInvoicingConsumerTypes.Shipment.Code;
		}

		protected override CostSell GetExpectedCostOrSell()
		{
			return CostSell.Revenue;
		}
	}
}
