using System.Linq;
using CargoWise.Customs.IN.MessageContracts.ExportSbGoodsRegistration;
using Enterprise.Customs.IN.Business.MessageSending.ExportSb;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Business.MessageSending.ExportSbGoodsRegistration.Testing;

[TestedType(typeof(ExportSbGoodsRegistrationCACHE05DataProvider))]
sealed class ExportSbGoodsRegistrationContainerPackageDataProviderTest : ExportSbGoodsRegistrationContainerPackageDataProviderAbstractClassBase
{
	public override void TestCustomHouseCode()
	{
		AssertEquals("TBA", CreateDataProvider().CustomHouseCode);
	}

	public override void TestEqmntId()
	{
		AssertEquals("TBA", CreateDataProvider().EqmntId);
	}

	public override void TestEqmntLoc()
	{
		AssertEquals("TBA", CreateDataProvider().EqmntLoc);
	}

	public override void TestEqmntSerialNo()
	{
		AssertEquals("TBA", CreateDataProvider().EqmntSerialNo);
	}

	public override void TestEqmntType()
	{
		AssertEquals("TBA", CreateDataProvider().EqmntType);
	}

	public override void TestMessageType()
	{
		AssertEquals("TBA", CreateDataProvider().MessageType);
	}

	public override void TestPckgCount()
	{
		AssertEquals("TBA", CreateDataProvider().PckgCount);
	}

	public override void TestPckgFrom()
	{
		AssertEquals("TBA", CreateDataProvider().PckgFrom);
	}

	public override void TestPckgTo()
	{
		AssertEquals("TBA", CreateDataProvider().PckgTo);
	}

	public override void TestPckgUqc()
	{
		AssertEquals("TBA", CreateDataProvider().PckgUqc);
	}

	public override void TestSbDate()
	{
		AssertEquals("TBA", CreateDataProvider().SbDate);
	}

	public override void TestSbNo()
	{
		AssertEquals("TBA", CreateDataProvider().SbNo);
	}

	protected override ContainerPackageDataProviderAbstractClass CreateDataProvider()
	{
		return ExportSbGoodsRegistrationCACHE05DataProvider.CreateProvider(messageSendingObject).Goodsregistration.ContainerPackage.First();
	}
}
