using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing.TaxFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.Testing.ARAP.Invoicing.TaxFramework
{
	abstract class NonJobRelatedInvoicingLineBaseTaxableGetTest_TaxCalculationParametersTest : InvoicingLineBaseTaxable_GetTaxCalculationParametersTest
	{
		protected override ZString GetExpectedJobType()
		{
			return AccountingMasterFilesConstants.JobTypes.NonJobRelated;
		}

		protected override ZString GetExpectedTransportMode()
		{
			return ZString.Empty;
		}
	}
}
