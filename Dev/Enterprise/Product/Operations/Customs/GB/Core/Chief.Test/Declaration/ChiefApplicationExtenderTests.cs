using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Registry.Business;

namespace Enterprise.Customs.GB.Chief.Declaration.Testing
{
	sealed class ChiefApplicationExtenderTests : TestCaseWithFactory
	{
		public void TestGetFeeCodeFromRateCodeCore()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();

			var entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
			var entryLine = entryHeader.AllEntryLines.AddNew();
			entryLine.InvoiceLines.Add(invoiceLine);

			var addInfo = invoiceLine.AdditionalInfos.AddNew();
			addInfo.CSI_Code = "NIIMP";

			var rateCode = appExtender.GetFeeCodeFromRateCode(entryLine, "A00");
			AssertEquals("RateCode should remain unchanged", "A00", rateCode);
		}

		public void TestGetDefaultMethodOfPaymentValueCore()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();

			var entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
			var entryLine = entryHeader.AllEntryLines.AddNew();
			entryLine.InvoiceLines.Add(invoiceLine);
			var fee = entryLine.Fees.AddNew();
			fee.CF_ChargeType = UniversalReferenceConstants.RefCusRateCodes.Vat;
			AssertEquals("CHIEF - Should be Empty", ZString.Empty, appExtender.GetDefaultMethodOfPaymentValue(fee));
		}

		protected override void SetUp()
		{
			appExtender = ApplicationExtender.New(DeclarationApplicationCodeList.Codes.CHIEF);
			AssertType<ChiefApplicationExtender>("Pre-Condition Expecting Chief Extender", appExtender);
		}

		ApplicationExtender appExtender;
	}
}
