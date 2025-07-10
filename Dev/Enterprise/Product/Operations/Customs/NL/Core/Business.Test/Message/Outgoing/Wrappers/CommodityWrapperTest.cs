using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.NL.Business.Common;
using Enterprise.Customs.NL.Business.Declaration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.NL.Business.Testing;

sealed class CommodityWrapperTest : DataProviderTestCase<CommodityWrapper>
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>(() => new CommodityWrapper(null));
	}

	public void TestDescription()
	{
		AssertEquals("Test Description", wrapper.Description);
	}

	public void TestClassifications()
	{
		CombineAssertions(() =>
		{
			AssertNotNull(wrapper.Classifications.FirstOrDefault());
			AssertType<ClassificationWrapper>(wrapper.Classifications.FirstOrDefault());
			AssertEquals("No Classifications", 8, wrapper.Classifications.Count);
			AssertEquals("No different TypeCodes", 6, wrapper.Classifications.Select(x => x.IdentificationTypeCode).Distinct().Count());
			AssertEquals("No Classifications for " + NLConstants.Classification.IdentificationTypeCodes.TSP, 1, wrapper.Classifications.Count(x => x.IdentificationTypeCode.Equals(NLConstants.Classification.IdentificationTypeCodes.TSP)));
			AssertEquals("No Classifications for " + NLConstants.Classification.IdentificationTypeCodes.TRC, 1, wrapper.Classifications.Count(x => x.IdentificationTypeCode.Equals(NLConstants.Classification.IdentificationTypeCodes.TRC)));
			AssertEquals("No Classifications for " + NLConstants.Classification.IdentificationTypeCodes.TRA, 3, wrapper.Classifications.Count(x => x.IdentificationTypeCode.Equals(NLConstants.Classification.IdentificationTypeCodes.TRA)));
			AssertEquals("No Classifications for " + NLConstants.Classification.IdentificationTypeCodes.GN, 1, wrapper.Classifications.Count(x => x.IdentificationTypeCode.Equals(NLConstants.Classification.IdentificationTypeCodes.GN)));
			AssertEquals("SequenceNo of 1st Classifications", 1, wrapper.Classifications.ElementAt(0).SequenceNo);
			AssertEquals("ID of 1st Classifications", "39269097", wrapper.Classifications.ElementAt(0).Id);
			AssertEquals("TypeCode of 1st Classification", NLConstants.Classification.IdentificationTypeCodes.TSP, wrapper.Classifications.ElementAt(0).IdentificationTypeCode);
			AssertEquals("SequenceNo of 2nd Classifications", 1, wrapper.Classifications.ElementAt(1).SequenceNo);
			AssertEquals("ID of 2nd Classifications", "90", wrapper.Classifications.ElementAt(1).Id);
			AssertEquals("TypeCode of 2nd Classification", NLConstants.Classification.IdentificationTypeCodes.TRC, wrapper.Classifications.ElementAt(1).IdentificationTypeCode);
			AssertEquals("SequenceNo of 3rd Classifications", 1, wrapper.Classifications.ElementAt(2).SequenceNo);
			AssertEquals("ID of 3rd Classifications", "2500", wrapper.Classifications.ElementAt(2).Id);
			AssertEquals("TypeCode of 3rd Classification", NLConstants.Classification.IdentificationTypeCodes.GN, wrapper.Classifications.ElementAt(2).IdentificationTypeCode);
			AssertEquals("SequenceNo of 4th Classifications", 1, wrapper.Classifications.ElementAt(3).SequenceNo);
			AssertEquals("ID of 4th Classifications", "2501", wrapper.Classifications.ElementAt(3).Id);
			AssertEquals("TypeCode of 4th Classification", NLConstants.Classification.IdentificationTypeCodes.TRA, wrapper.Classifications.ElementAt(3).IdentificationTypeCode);
			AssertEquals("SequenceNo of 5th Classifications", 2, wrapper.Classifications.ElementAt(4).SequenceNo);
			AssertEquals("ID of 5th Classifications", "1500", wrapper.Classifications.ElementAt(4).Id);
			AssertEquals("TypeCode of 5th Classification", NLConstants.Classification.IdentificationTypeCodes.TRA, wrapper.Classifications.ElementAt(4).IdentificationTypeCode);
			AssertEquals("SequenceNo of 6th Classifications", 3, wrapper.Classifications.ElementAt(5).SequenceNo);
			AssertEquals("ID of 6th Classifications", "1501", wrapper.Classifications.ElementAt(5).Id);
			AssertEquals("TypeCode of 6th Classification", NLConstants.Classification.IdentificationTypeCodes.TRA, wrapper.Classifications.ElementAt(5).IdentificationTypeCode);
			AssertEquals("ID of 7th Classifications", "123456789", wrapper.Classifications.ElementAt(6).Id);
			AssertEquals("TypeCode of 7th Classification", NLConstants.Classification.IdentificationTypeCodes.CV, wrapper.Classifications.ElementAt(6).IdentificationTypeCode);
			AssertEquals("ID of 8th Classifications", "123", wrapper.Classifications.ElementAt(7).Id);
			AssertEquals("TypeCode of 8th Classification", NLConstants.Classification.IdentificationTypeCodes.UIN, wrapper.Classifications.ElementAt(7).IdentificationTypeCode);

			var invoiceLine = (JobComInvoiceLine)cusEntryLine.InvoiceLines.FirstOrDefault();
			invoiceLine.JI_Tariff = "1234567899";
			wrapper = new CommodityWrapper(cusEntryLine);
			AssertEquals("ID of 1st Classification", "12345678", wrapper.Classifications.ElementAt(0).Id);
			AssertEquals("TypeCode of 1st Classification", NLConstants.Classification.IdentificationTypeCodes.TSP, wrapper.Classifications.ElementAt(0).IdentificationTypeCode);
			AssertEquals("ID of 2nd Classification", "99", wrapper.Classifications.ElementAt(1).Id);
			AssertEquals("TypeCode of 2nd Classification", NLConstants.Classification.IdentificationTypeCodes.TRC, wrapper.Classifications.ElementAt(1).IdentificationTypeCode);

			invoiceLine.JI_Tariff = "";
			wrapper = new CommodityWrapper(cusEntryLine);
			AssertEquals("ID of 1st Classification", "2500", wrapper.Classifications.ElementAt(0).Id);
			AssertEquals("TypeCode of 1st Classification", NLConstants.Classification.IdentificationTypeCodes.GN, wrapper.Classifications.ElementAt(0).IdentificationTypeCode);
			AssertEquals("ID of 2nd Classification", "2501", wrapper.Classifications.ElementAt(1).Id);
			AssertEquals("TypeCode of 2nd Classification", NLConstants.Classification.IdentificationTypeCodes.TRA, wrapper.Classifications.ElementAt(1).IdentificationTypeCode);
			AssertEquals("ID of 3th Classifications", "1500", wrapper.Classifications.ElementAt(2).Id);
			AssertEquals("TypeCode of 3th Classification", NLConstants.Classification.IdentificationTypeCodes.TRA, wrapper.Classifications.ElementAt(2).IdentificationTypeCode);
			AssertEquals("ID of 4th Classifications", "1501", wrapper.Classifications.ElementAt(3).Id);
			AssertEquals("TypeCode of 4th Classification", NLConstants.Classification.IdentificationTypeCodes.TRA, wrapper.Classifications.ElementAt(3).IdentificationTypeCode);
		});
	}

	public void TestDangerousGoods()
	{
		AssertEquals("Length of DangerousGoods", 2, wrapper.DangerousGoods.Count);
	}

	public void TestGoodsMeasure()
	{
		AssertNotNull(wrapper.GoodsMeasure);
		AssertType<GoodsMeasureWrapper>(wrapper.GoodsMeasure);
	}

	public void TestItemChargeAmount()
	{
		AssertEquals(31m, wrapper.ItemChargeAmount);
	}

	public void TestTaxCalculation()
	{
		AssertNotNull(wrapper.TaxCalculation);
		AssertType<TaxCalculationWrapper>(wrapper.TaxCalculation);
	}

	protected override CommodityWrapper GetProvider() => wrapper;

	protected override void SetUp()
	{
		base.SetUp();

		declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = Customs.Common.EU.EUJobMessageTypeList.Codes.Import;
		var invoiceHeader = declaration.Invoices.AddNew();
		invoiceHeader.JZ_RX_NKInvoice_Currency = "EUR";

		CreateInvoiceLine(invoiceHeader.InvoiceLines, 1, 7, 6, 2, 15, "3926909790");
		CreateInvoiceLine(invoiceHeader.InvoiceLines, 2, 5, 4, 1, 16, "3926909790");

		var merge = new EU.Business.Declaration.LineMerger(declaration);
		merge.DoMerge();

		cusEntryLine = declaration.CustomsEntryHeaders.Cast<CusEntryHeader>().FirstOrDefault().MergedLines.First();
		cusEntryLine.CL_Description = "Test Description";
		cusEntryLine.CL_StatisticalValue = 10.2;
		cusEntryLine.CL_CustomsValue = 20.3;
		wrapper = new CommodityWrapper(cusEntryLine);
	}

	JobDeclaration declaration;
	CommodityWrapper wrapper;
	CusEntryLine cusEntryLine;

	void CreateInvoiceLine(JobComInvoiceLineViewCollection invoiceLines,
		ZShort lineNo, ZDecimal weight, ZDecimal netWeight, ZDecimal customsSecondQuantity, ZDecimal linePrice, ZString tarrif)
	{
		var invoiceLine = invoiceLines.AddNew();
		invoiceLine.JI_LineNo = lineNo;
		invoiceLine.JI_Weight = weight;
		invoiceLine.JI_NetWeight = netWeight;
		invoiceLine.JI_CustomsSecondQuantity = customsSecondQuantity;
		invoiceLine.JI_LinePrice = linePrice;
		invoiceLine.JI_Tariff = tarrif;
		invoiceLine.JI_SupplementaryCode1 = "1500";
		invoiceLine.JI_SupplementaryCode2 = "1501";
		invoiceLine.ZG_CusNumber = "123456789";
		invoiceLine.JI_PartNo = "123";

		WrapperTestHelper.CreateSupplementaryCode(invoiceLine.AdditionalSupplementaryCodes, "2500", Core.Constants.CountryCodes.Netherlands);
		WrapperTestHelper.CreateSupplementaryCode(invoiceLine.AdditionalSupplementaryCodes, "2501", Core.Constants.CountryCodes.EuropeanUnion);

		WrapperTestHelper.CreateUndg(Factory, invoiceLine.UNDGs, "8232");
		WrapperTestHelper.CreateUndg(Factory, invoiceLine.UNDGs, "2328");
	}
}
