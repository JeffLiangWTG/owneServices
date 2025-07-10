using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing.TaxFramework.Testing;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.Testing.ARAP.Invoicing.TaxFramework
{
	abstract class ConsolRelatedTaxableLine_GetTaxCalculationParametersTest : InvoicingLineBaseTaxable_GetTaxCalculationParametersTest
	{
		protected override ZString GetExpectedTransportMode()
		{
			return TransportModes.Air;
		}
	}
}
