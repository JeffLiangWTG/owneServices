using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.GUI.EInvoicing.PenaltyTaxMessage;
using Enterprise.Core;

namespace Enterprise.Accounting.GUI.Testing.eInvoicing.PenaltyTaxMessage
{
	public class EInvoicingPenaltyTaxInfoFormFactoryTest : TestCaseWithFactory
	{
		public void TestCreateEInvoicingPenaltyInfoFormProvider()
		{
			var provider = EInvoicingPenaltyTaxInfoFormFactory.CreateEInvoicingPenaltyInfoFormProvider(Constants.CountryCodes.KoreaSouth);

			AssertNotNull(provider);
			AssertType<KoreaSouthEInvoicingPenaltyTaxInfoFormProvider>(provider);

			provider = EInvoicingPenaltyTaxInfoFormFactory.CreateEInvoicingPenaltyInfoFormProvider(Constants.CountryCodes.Romania);
			AssertNull(provider);
		}
	}
}
