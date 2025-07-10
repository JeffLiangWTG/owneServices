using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.DataTransfer.Universal;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.Universal.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.FR.DataTransfer.Universal.Testing
{
	class CommercialInvoiceHeaderDataObjectWriterTest : TestCaseWithFactory
	{
		public void TestPopulateInvoiceLineCalculatedTaxesAmount()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var euGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.France, "France", euGrouping);
			CusRefRateCodeViewTestHelper.CreateAndSaveCusRateCodeForCountryIfNotExists(Factory, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Customs.Business.UniversalReferenceConstants.RefCusRateTypes.Dty, EU.Business.UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Import;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.EuropeanUnion;

			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 1000m;

			var entry = Factory.New<CusEntryHeader>();
			entry.CH_JE = declaration.PK;
			var entryLine = entry.AllEntryLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;

			entryLine.Fees.SetAmount(EU.Business.UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts, 200);
			entryLine.Fees.SetAmount(EU.Business.UniversalReferenceConstants.RefCusRateCodes.Vat, 300);
			entryLine.Fees.SetAmount("Other", 700);

			AssertEquals("Pre-condition", 200m, invoiceLine.JI_Calc_DutyAmount);
			AssertEquals("Pre-condition", 300m, invoiceLine.JI_Calc_GSTVATAmount);
			AssertEquals("Pre-condition", 700m, invoiceLine.JI_Calc_OtherTaxesAmount);

			var writer = new CommercialInvoiceHeaderDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, invoice)), new UniversalDataObjectWriterHelper(declaration.Factory, declaration.CountryCode));
			var invoiceData = writer.GetDataObject(invoice);

			AssertEquals("invoiceData.CommercialInvoiceLineCollection.Count", 1, invoiceData.CommercialInvoiceLineCollection.Count);

			var invoiceLineData = invoiceData.CommercialInvoiceLineCollection.FirstOrDefault();
			AssertEquals("invoiceLineData.AddInfoCollection.EstimatedDutyBreakdown should match invoice line JI_Calc_DutyAmount", "200", invoiceLineData.AddInfoCollection.FirstOrDefault(x => x.Key.GetValueOrDefault() == Constants.AddInfoKeys.InvoiceLine.EstimatedDutyBreakdown).Value);
			AssertEquals("invoiceLineData.AddInfoCollection.EstimatedVATBreakdown should match invoice line JI_Calc_GSTVATAmount", "300", invoiceLineData.AddInfoCollection.FirstOrDefault(x => x.Key.GetValueOrDefault() == Constants.AddInfoKeys.InvoiceLine.EstimatedVATBreakdown).Value);
			AssertEquals("invoiceLineData.AddInfoCollection.EstimatedOtherTaxesBreakdown should match invoice line JI_Calc_OtherTaxesAmount", "700", invoiceLineData.AddInfoCollection.FirstOrDefault(x => x.Key.GetValueOrDefault() == Constants.AddInfoKeys.InvoiceLine.EstimatedOtherTaxesBreakdown).Value);
		}
	}
}
