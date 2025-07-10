using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.EU.EMCS.Business.Testing
{
	public class EMCSJobComInvoiceHeaderValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckJZ_InvoiceNumber()
		{
			CombineAssertions(() =>
			{
				ValidationTestHelper.AssertYouHaveNotEnteredMessageError(dec.InvoiceNumberInfo);

				const string message = "Invoice Lines should have at least one row.";
				AssertHasMessageError("No invoice lines", dec.InvoiceNumberInfo, message);

				dec.FilteredInvoiceLines.AddNew();
				inv.Validation.ValidateJZ_InvoiceNumber();
				AssertNoMessageError("Has invoice line", dec.InvoiceNumberInfo, message);
			});
		}

		public void TestJZ_IncoTerm_NotMandatory()
		{
			ValidationTestHelper.AssertFieldIsNotMandatory(inv.JZ_IncoTermInfo, MandatoryValidation.MustBeEntered);
		}

		protected override void SetUp()
		{
			base.SetUp();
			dec = Factory.New<EMCSJobDeclaration>();
			inv = dec.Invoices.AddNew();
		}
		EMCSJobDeclaration dec;
		EMCSJobComInvoiceHeader inv;
	}
}
