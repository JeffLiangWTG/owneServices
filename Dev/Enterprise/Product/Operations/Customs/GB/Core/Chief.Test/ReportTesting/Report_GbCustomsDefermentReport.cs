using CargoWise.Types;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Registry.Business;

namespace Enterprise.Customs.GB.Chief.ReportTesting
{
	class Report_GbCustomsDefermentReport : Business.ReportTesting.Report_GbCustomsDefermentReport
	{
		protected override string ApplicationCode => DeclarationApplicationCodeList.Codes.CHIEF;

		protected override void SetupTax(JobComInvoiceLine invoiceLine, string type, string mop, ZDecimal amount)
		{
			var tax = invoiceLine.Taxes.AddNew();

			tax.Data.G4_Type = type;
			tax.Data.G4_MethodOfPayment = mop;
			tax.Data.G4_Amount = $"{amount:F2}";
		}

		protected override string DeferredMOP => MethodOfPaymentCodes.F;
		protected override string ImmediateMOP => MethodOfPaymentCodes.D;
	}
}
