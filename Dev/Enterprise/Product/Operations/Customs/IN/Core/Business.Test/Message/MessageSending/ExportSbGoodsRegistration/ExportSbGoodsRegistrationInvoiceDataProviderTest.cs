using System.Linq;
using CargoWise.Customs.IN.MessageContracts.ExportSbGoodsRegistration;
using Enterprise.Customs.IN.Business.MessageSending.ExportSb;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Business.MessageSending.ExportSbGoodsRegistration.Testing;

[TestedType(typeof(ExportSbGoodsRegistrationCACHE05DataProvider))]
sealed class ExportSbGoodsRegistrationInvoiceDataProviderTest : ExportSbGoodsRegistrationInvoiceDataProviderAbstractClassBase
{
	public override void TestCustomHouseCode()
	{
		AssertEquals("TBA", CreateDataProvider().CustomHouseCode);
	}

	public override void TestFreightAmount()
	{
		AssertEquals("TBA", CreateDataProvider().FreightAmount);
	}

	public override void TestFreightCurrency()
	{
		AssertEquals("TBA", CreateDataProvider().FreightCurrency);
	}

	public override void TestInsuranceAmount()
	{
		AssertEquals("TBA", CreateDataProvider().InsuranceAmount);
	}

	public override void TestInsuranceCurrency()
	{
		AssertEquals("TBA", CreateDataProvider().InsuranceCurrency);
	}

	public override void TestInsuranceRate()
	{
		AssertEquals("TBA", CreateDataProvider().InsuranceRate);
	}

	public override void TestInvoiceSrNo()
	{
		AssertEquals("TBA", CreateDataProvider().InvoiceSrNo);
	}

	public override void TestMessageType()
	{
		AssertEquals("TBA", CreateDataProvider().MessageType);
	}

	public override void TestNatureOfContract()
	{
		AssertEquals("TBA", CreateDataProvider().NatureOfContract);
	}

	public override void TestSbDate()
	{
		AssertEquals("TBA", CreateDataProvider().SbDate);
	}

	public override void TestSbNo()
	{
		AssertEquals("TBA", CreateDataProvider().SbNo);
	}

	public override void TestUnitPriceIncludesFreightInsuranceBothNone()
	{
		AssertEquals("TBA", CreateDataProvider().UnitPriceIncludesFreightInsuranceBothNone);
	}

	protected override InvoiceDataProviderAbstractClass CreateDataProvider()
	{
		return ExportSbGoodsRegistrationCACHE05DataProvider.CreateProvider(messageSendingObject).Goodsregistration.Invoice.First();
	}
}
