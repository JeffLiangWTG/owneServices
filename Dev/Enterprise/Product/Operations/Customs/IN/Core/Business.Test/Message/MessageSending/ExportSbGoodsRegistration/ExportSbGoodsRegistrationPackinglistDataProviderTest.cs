using System.Linq;
using CargoWise.Customs.IN.MessageContracts.ExportSbGoodsRegistration;
using Enterprise.Customs.IN.Business.MessageSending.ExportSb;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Business.MessageSending.ExportSbGoodsRegistration.Testing;

[TestedType(typeof(ExportSbGoodsRegistrationCACHE05DataProvider))]
sealed class ExportSbGoodsRegistrationPackinglistDataProviderTest : ExportSbGoodsRegistrationPackinglistDataProviderAbstractClassBase
{
	public override void TestCustomHouseCode()
	{
		AssertEquals("TBA", CreateDataProvider().CustomHouseCode);
	}

	public override void TestMessageType()
	{
		AssertEquals("TBA", CreateDataProvider().MessageType);
	}

	public override void TestPackingcode()
	{
		AssertEquals("TBA", CreateDataProvider().Packingcode);
	}

	public override void TestPackingnumberFrom()
	{
		AssertEquals("TBA", CreateDataProvider().PackingnumberFrom);
	}

	public override void TestPackingNumberTo()
	{
		AssertEquals("TBA", CreateDataProvider().PackingNumberTo);
	}

	public override void TestSbDate()
	{
		AssertEquals("TBA", CreateDataProvider().SbDate);
	}

	public override void TestSbNo()
	{
		AssertEquals("TBA", CreateDataProvider().SbNo);
	}

	protected override PackinglistDataProviderAbstractClass CreateDataProvider()
	{
		return ExportSbGoodsRegistrationCACHE05DataProvider.CreateProvider(messageSendingObject).Goodsregistration.Packinglist.First();
	}
}
