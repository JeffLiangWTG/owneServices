using System;
using System.Linq;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.NL.Business.Declaration;

namespace Enterprise.Customs.NL.Business.Testing;

class GoodsMeasureWrapperTest : DataProviderTestCase<GoodsMeasureWrapper>
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>(() => new GoodsMeasureWrapper(null));
	}

	public void TestGrossMassMeasure()
	{
		invLine1.JI_Weight = 7;
		invLine2.JI_Weight = 5;
		AssertEquals(12m, wrapper.GrossMassMeasure);
	}

	public void TestNetNetWeightMeasure()
	{
		invLine1.JI_NetWeight = 6;
		invLine2.JI_NetWeight = 4;
		AssertEquals(10m, wrapper.NetNetWeightMeasure);
	}

	public void TestSupplementaryUnitsQty()
	{
		AssertEquals(0m, wrapper.SupplementaryUnitsQty);
	}

	public void TestTariffQuantity()
	{
		AssertNull(wrapper.TariffQuantity);

		invLine1.JI_CustomsSecondQuantity = 0;
		invLine2.JI_CustomsSecondQuantity = 0;
		AssertNull(wrapper.TariffQuantity);

		invLine1.JI_CustomsSecondQuantity = 2;
		invLine2.JI_CustomsSecondQuantity = 1;
		AssertEquals(3m, wrapper.TariffQuantity);
	}

	protected override GoodsMeasureWrapper GetProvider() => wrapper;

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		var invoice = declaration.Invoices.AddNew();
		invLine1 = invoice.InvoiceLines.AddNew();
		invLine2 = invoice.InvoiceLines.AddNew();

		var shutterUpperer = new SendsMessagesToCustomsShutterUpperer(false);
		declaration.DoMerge(shutterUpperer);

		var entryHeader = declaration.CustomsEntryHeaders.Cast<Declaration.CusEntryHeader>().FirstOrDefault();
		cusEntryLine = entryHeader.MergedLines.First();
		wrapper = new GoodsMeasureWrapper(cusEntryLine);
	}
	Declaration.CusEntryLine cusEntryLine;
	JobDeclaration declaration;
	GoodsMeasureWrapper wrapper;
	JobComInvoiceLine invLine1;
	JobComInvoiceLine invLine2;
}
