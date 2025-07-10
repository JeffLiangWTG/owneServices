using CargoWise.Types;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Registry.Business;

namespace Enterprise.Customs.GB.CDS.Report.Testing
{
	class Report_GbCustomsDefermentReport : Business.ReportTesting.Report_GbCustomsDefermentReport
	{
		protected override string ApplicationCode => DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;

		protected override void SetupTax(JobComInvoiceLine invoiceLine, string type, string mop, ZDecimal amount)
		{
			var tax = invoiceLine.CusEntryLine.Fees.AddNew();
			tax.CF_ChargeType = type;
			tax.CF_MethodOfPayment = mop;
			tax.CF_ChargeAmount = amount;
		}

		protected override string DeferredMOP => MethodOfPaymentCodes.E;
		protected override string ImmediateMOP => MethodOfPaymentCodes.A;
	}
}
