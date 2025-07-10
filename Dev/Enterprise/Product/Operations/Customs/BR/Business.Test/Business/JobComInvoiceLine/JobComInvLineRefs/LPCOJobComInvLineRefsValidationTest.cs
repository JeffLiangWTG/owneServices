using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.BR.Business.Testing
{
	public class LPCOJobComInvLineRefsValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckJG_ReferenceNumber()
		{
			var oLineRefs = oInvoiceLine.LPCOJobComInvLineRefsCollection.AddNew();
			oLineRefs.JG_ReferenceNumber = ZString.Empty;
			AssertHasMessageErrorContaining(oLineRefs.JG_ReferenceNumberInfo, MandatoryValidation.YouHaveNotEntered);
			oLineRefs.JG_ReferenceNumber = "E1800000001";
			AssertNoMessageErrorContaining(oLineRefs.JG_ReferenceNumberInfo, MandatoryValidation.YouHaveNotEntered);
		}

		JobComInvoiceLine oInvoiceLine;
		protected override void SetUp()
		{
			base.SetUp();
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			oInvoiceLine = invoice.JobComInvoiceLines.AddNew();
		}
	}
}
