using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.BR;

namespace Enterprise.Customs.BR.Business.Testing
{
	public class TariffDetachValidationTest : CusCodeDataValidationTest
	{
		public new void TestCheckCY_Code()
		{
			var tariffDetach = invoiceLine.TariffDetachs.AddNew();
			tariffDetach.CY_Code = ZString.Empty;
			AssertNoMessageErrors(tariffDetach.CY_CodeInfo);
			tariffDetach.CY_Code = "999";
			AssertNoMessageErrors(tariffDetach.CY_CodeInfo);
		}

		public void TestDuplicateCY_Code()
		{
			var tariffDetach1 = invoiceLine.TariffDetachs.AddNew();
			tariffDetach1.CY_Code = "999";
			var tariffDetach2 = invoiceLine.TariffDetachs.AddNew();
			tariffDetach2.CY_Code = "999";
			AssertHasMessageError(tariffDetach2.CY_CodeInfo, "This Tariff Detach code already exists in this invoice line");
		}

		JobComInvoiceLine invoiceLine;

		protected override void SetUp()
		{
			base.SetUp();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			invoiceLine = invoice.JobComInvoiceLines.AddNew();
		}
	}
}

