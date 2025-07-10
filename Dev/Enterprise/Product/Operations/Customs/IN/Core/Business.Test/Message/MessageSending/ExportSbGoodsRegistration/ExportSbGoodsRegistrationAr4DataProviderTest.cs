using System.Linq;
using CargoWise.Customs.IN.MessageContracts.ExportSbGoodsRegistration;
using Enterprise.Customs.IN.Business.MessageSending.ExportSb;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Business.MessageSending.ExportSbGoodsRegistration.Testing;

[TestedType(typeof(ExportSbGoodsRegistrationCACHE05DataProvider))]
sealed class ExportSbGoodsRegistrationAr4DataProviderTest : ExportSbGoodsRegistrationAr4DataProviderAbstractClassBase
{
	public override void TestMessageType()
	{
		AssertEquals("TBA", CreateDataProvider().MessageType);
	}

	public override void TestCustomHouseCode()
	{
		AssertEquals("TBA", CreateDataProvider().CustomHouseCode);
	}

	public override void TestSbNo()
	{
		AssertEquals("TBA", CreateDataProvider().SbNo);
	}

	public override void TestSbDate()
	{
		AssertEquals("TBA", CreateDataProvider().SbDate);
	}

	public override void TestInvoiceSrNumber()
	{
		AssertEquals("TBA", CreateDataProvider().InvoiceSrNumber);
	}

	public override void TestItemSrNumberInInvoice()
	{
		AssertEquals("TBA", CreateDataProvider().ItemSrNumberInInvoice);
	}

	public override void TestAr4Number()
	{
		AssertEquals("TBA", CreateDataProvider().Ar4Number);
	}

	public override void TestAr4Date()
	{
		AssertEquals("TBA", CreateDataProvider().Ar4Date);
	}

	public override void TestCommissionerate()
	{
		AssertEquals("TBA", CreateDataProvider().Commissionerate);
	}

	public override void TestDivision()
	{
		AssertEquals("TBA", CreateDataProvider().Division);
	}

	public override void TestRange()
	{
		AssertEquals("TBA", CreateDataProvider().Range);
	}

	public override void TestRemarks()
	{
		AssertEquals("TBA", CreateDataProvider().Remarks);
	}

	protected override Ar4DataProviderAbstractClass CreateDataProvider()
	{
		return ExportSbGoodsRegistrationCACHE05DataProvider.CreateProvider(messageSendingObject).Goodsregistration.Ar4.First();
	}
}
