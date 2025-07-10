using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.GUI.EInvoicing.PenaltyTaxMessage;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.Testing.EInvoicing.PenaltyTaxMessage
{
	public class KoreaSouthEInvoicingPenaltyTaxInfoFormProviderTest : TestCaseWithFactory
	{
		public void TestShowPenaltyTaxInfoForm()
		{
			var provider = new KoreaSouthEInvoicingPenaltyTaxInfoFormProvider();

			provider.ShowPenaltyTaxInfoForm();

			using (var form = ZFormModaliser.LastFormShownDialogForTest)
			{
				AssertNotNull(form);
				AssertType<KoreaSouthEInvoicingPenaltyTaxInfoForm>(form);
			}
		}

		public void TestPenaltyTaxInfoMenuName()
		{
			var provider = new KoreaSouthEInvoicingPenaltyTaxInfoFormProvider();

			AssertEquals("Additional Tax Information", provider.PenaltyTaxInfoMenuName);
		}
	}
}
