using System.Linq;
using CargoWise.Customs.AE.MessageContracts.Mirsal2;
using Enterprise.Customs.AE.Business.MessageSending.Mirsal2.Testing;

namespace Enterprise.Customs.AE.Business.Testing;

sealed class InvoiceItemsDetailsTypeDataProviderTest : Mirsal2InvoiceItemsDetailsTypeDataProviderAbstractClassBase
{
	public override void TestInvoiceItemLineNumber()
	{
		invoiceLine.JI_LineNo = 123;
		AssertEquals(invoiceLine.JI_LineNo, CreateDataProvider().InvoiceItemLineNumber);
	}

	public override void TestCommodityCode()
	{
		invoiceLine.JI_Tariff = "TEST1";
		AssertEquals(invoiceLine.JI_Tariff, CreateDataProvider().CommodityCode);
	}

	public override void TestGoodsDescription()
	{
		invoiceLine.JI_Description = "TEST2";
		AssertEquals(invoiceLine.JI_Description, CreateDataProvider().GoodsDescription);
	}

	public override void TestGoodsCondition()
	{
		invoiceLine.JI_NewUsed = "N";
		AssertEquals(invoiceLine.JI_NewUsed, CreateDataProvider().GoodsCondition);
	}

	public override void TestStatisticalQuantityMeasurementUnit()
	{
		invoiceLine.JI_CustomsUnitQty = "U1";
		AssertEquals(invoiceLine.JI_CustomsUnitQty, CreateDataProvider().StatisticalQuantityMeasurementUnit);
	}

	public override void TestStatisticalQuantity()
	{
		invoiceLine.JI_CustomsQuantity = 99.9999m;
		AssertEquals(invoiceLine.JI_CustomsQuantity, CreateDataProvider().StatisticalQuantity);
	}

	public override void TestNetWeightUnit()
	{
		invoiceLine.JI_WeightUQ = "U2";
		AssertEquals(invoiceLine.JI_WeightUQ, CreateDataProvider().NetWeightUnit);
	}

	public override void TestNetWeight()
	{
		invoiceLine.JI_Weight = 999.9999m;
		AssertEquals(invoiceLine.JI_Weight, CreateDataProvider().NetWeight);
	}

	public override void TestVehicleIndicator()
	{
		Assert("to do in future WI", true);
	}

	public override void TestSupplementaryQuantityMeasurementUnit()
	{
		invoiceLine.JI_CustomsSecondUnitQty = "U3";
		AssertEquals(invoiceLine.JI_CustomsSecondUnitQty, CreateDataProvider().SupplementaryQuantityMeasurementUnit);
	}

	public override void TestSupplementaryQuantity()
	{
		invoiceLine.JI_CustomsSecondQuantity = 9999.9999m;
		AssertEquals(invoiceLine.JI_CustomsSecondQuantity, CreateDataProvider().SupplementaryQuantity);
	}

	public override void TestValueOfGoods()
	{
		invoiceLine.JI_LinePrice = 11.111m;
		AssertEquals(invoiceLine.JI_LinePrice, CreateDataProvider().ValueOfGoods);
	}

	public override void TestCountryOfOrigin()
	{
		invoiceLine.JI_CountryOfOrigin = "AE";
		AssertEquals(invoiceLine.JI_CountryOfOrigin, CreateDataProvider().CountryOfOrigin);
	}

	public override void TestPreviousCustomsDeclarationReferenceNumber()
	{
		Assert("to do in future WI", true);
	}

	public override void TestPreviousCustomsDeclarationInvoiceNumber()
	{
		Assert("to do in future WI", true);
	}

	public override void TestPreviousCustomsDeclarationInvoiceLineNumber()
	{
		Assert("to do in future WI", true);
	}

	public override void TestExemptionType()
	{
		Assert("to do in future WI", true);
	}

	public override void TestExemptionReferenceNumber()
	{
		Assert("to do in future WI", true);
	}

	public override void TestIsRestricted()
	{
		Assert("to do in future WI", true);
	}

	public override void TestPermitRefenceDetails()
	{
		Assert("to do in future WI", true);
	}

	public override void TestVehicleDetail()
	{
		invoiceLine.Vehicles.AddNew();
		AssertEquals($"{nameof(VehicleDetailsTypeDataProviderAbstractClass)} Type", "VehicleDetailsTypeDataProvider", CreateDataProvider().VehicleDetail.Single().GetType().Name);
	}

	protected override InvoiceItemsDetailsTypeDataProviderAbstractClass CreateDataProvider()
	{
		return DeclarationRequestDataProvider.CreateProvider(header, additionalDataProvider).Declaration.ShippingDetails.Invoices.First().InvoiceItemsDetail.First();
	}

	protected override void SetUp()
	{
		base.SetUp();
		var declaration = Factory.New<JobDeclaration>();
		var invoiceHeader = declaration.Invoices.AddNew();
		invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();

		var cusEntryLine = header.MergedLines.AddNew();
		cusEntryLine.InvoiceLines.Add(invoiceLine);
	}

	JobComInvoiceLine invoiceLine;
}
