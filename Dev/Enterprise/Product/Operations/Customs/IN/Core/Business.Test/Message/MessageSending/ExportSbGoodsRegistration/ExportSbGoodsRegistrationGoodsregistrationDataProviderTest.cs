using CargoWise.Customs.IN.MessageContracts.ExportSbGoodsRegistration;
using Enterprise.Customs.IN.Business.MessageSending.ExportSb;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Business.MessageSending.ExportSbGoodsRegistration.Testing;

[TestedType(typeof(ExportSbGoodsRegistrationCACHE05DataProvider))]
sealed class ExportSbGoodsRegistrationGoodsregistrationDataProviderTest : ExportSbGoodsRegistrationGoodsregistrationDataProviderAbstractClassBase
{
	protected override GoodsregistrationDataProviderAbstractClass CreateDataProvider()
	{
		return ExportSbGoodsRegistrationCACHE05DataProvider.CreateProvider(messageSendingObject).Goodsregistration;
	}

	public override void TestMaster()
	{
		Assert("to do in future WI", true);
	}

	public override void TestInvoice()
	{
		Assert("to do in future WI", true);
	}

	public override void TestPackinglist()
	{
		Assert("to do in future WI", true);
	}

	public override void TestContainer()
	{
		Assert("to do in future WI", true);
	}

	public override void TestSeal()
	{
		Assert("to do in future WI", true);
	}

	public override void TestAr4()
	{
		Assert("to do in future WI", true);
	}

	public override void TestRotation()
	{
		Assert("to do in future WI", true);
	}

	public override void TestDocument()
	{
		Assert("to do in future WI", true);
	}

	public override void TestContainerPackage()
	{
		Assert("to do in future WI", true);
	}
}
