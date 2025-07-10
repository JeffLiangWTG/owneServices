using System.Linq;
using CargoWise.Customs.IN.MessageContracts.ExportSbGoodsRegistration;
using Enterprise.Customs.IN.Business.MessageSending.ExportSb;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Business.MessageSending.ExportSbGoodsRegistration.Testing;

[TestedType(typeof(ExportSbGoodsRegistrationCACHE05DataProvider))]
sealed class ExportSbGoodsRegistrationContainerDataProviderTest : ExportSbGoodsRegistrationContainerDataProviderAbstractClassBase
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

	public override void TestContainerNumber()
	{
		AssertEquals("TBA", CreateDataProvider().ContainerNumber);
	}

	public override void TestContainerSize()
	{
		AssertEquals("TBA", CreateDataProvider().ContainerSize);
	}

	public override void TestExciseSealNo()
	{
		AssertEquals("TBA", CreateDataProvider().ExciseSealNo);
	}

	public override void TestSealDate()
	{
		AssertEquals("TBA", CreateDataProvider().SealDate);
	}

	public override void TestSealTypeIndicator()
	{
		AssertEquals("TBA", CreateDataProvider().SealTypeIndicator);
	}

	public override void TestSealDeviceId()
	{
		AssertEquals("TBA", CreateDataProvider().SealDeviceId);
	}

	public override void TestMovementDocumentId()
	{
		AssertEquals("TBA", CreateDataProvider().MovementDocumentId);
	}

	public override void TestMovementDocumentNo()
	{
		AssertEquals("TBA", CreateDataProvider().MovementDocumentNo);
	}

	public override void TestEqmntType()
	{
		AssertEquals("TBA", CreateDataProvider().EqmntType);
	}

	public override void TestEqmntQty()
	{
		AssertEquals("TBA", CreateDataProvider().EqmntQty);
	}

	public override void TestEqmntQtyCode()
	{
		AssertEquals("TBA", CreateDataProvider().EqmntQtyCode);
	}

	public override void TestEqmntSerialNo()
	{
		AssertEquals("TBA", CreateDataProvider().EqmntSerialNo);
	}

	protected override ContainerDataProviderAbstractClass CreateDataProvider()
	{
		return ExportSbGoodsRegistrationCACHE05DataProvider.CreateProvider(messageSendingObject).Goodsregistration.Container.First();
	}
}
