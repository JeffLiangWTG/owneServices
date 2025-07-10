using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

[TestedType(typeof(NctsHeaderDepartureMessageSendingObjectParent))]
sealed class NctsHeaderDepartureMessageSendingObjectParentTest : NonPersistentBusinessObjectTestCase
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>(() => new NctsHeaderDepartureMessageSendingObjectParent(null));
		NctsHeader.BH_HeaderType = NctsMovementType.Codes.Arrival;
		AssertExceptionThrown<ArgumentException>("When nctsHeader is not a departure job", () => new NctsHeaderDepartureMessageSendingObject(NctsHeader));
	}

	public void TestTopLevelBusinessObject()
	{
		var sendingObjectParent = new NctsHeaderDepartureMessageSendingObjectParent(NctsHeader);
		AssertSame(NctsHeader, sendingObjectParent.TopLevelBusinessObject);
	}

	public void TestSecurityCheckpointToSendWithMessageError()
	{
		var sendingObjectParent = new NctsHeaderDepartureMessageSendingObjectParent(NctsHeader);
		AssertSame(Env.Security.EuNctsSendWithMessageErrors, sendingObjectParent.SecurityCheckpointToSendWithMessageError);
	}

	public void TestGetBizObjValidationMessageErrors() => CombineAssertions(() =>
	{
		var sendingObjectParent = new NctsHeaderDepartureMessageSendingObjectParent(NctsHeader);
		var messageSendingObject = sendingObjectParent.SendingObjectsCollection[0];
		messageSendingObject.MessageType = PassarMessageTypeList.Codes.NT015;
		AssertNotNullOrEmpty(sendingObjectParent.BizObjValidationMessageErrors);

		messageSendingObject.MessageType = PassarMessageTypeList.Codes.NT014;
		AssertEquals(ZString.Empty, sendingObjectParent.BizObjValidationMessageErrors);
	});

	public void TestGetAdditionalWarnings() => CombineAssertions(() =>
	{
		var sendingObjectParent = new NctsHeaderDepartureMessageSendingObjectParent(NctsHeader);
		var messageSendingObject = sendingObjectParent.SendingObjectsCollection[0];
		messageSendingObject.MessageType = PassarMessageTypeList.Codes.NT015;
		AssertEquals(ZString.Empty, sendingObjectParent.AdditionalWarnings);

		messageSendingObject.MessageType = PassarMessageTypeList.Codes.NT014;
		AssertEquals(ZString.Empty, sendingObjectParent.AdditionalWarnings);
	});

	protected override BusinessObject GetNewBusinessObject()
	{
		return new NctsHeaderDepartureMessageSendingObjectParent(NctsHeader);
	}

	NctsHeader NctsHeader => nctsHeader ?? (nctsHeader = CreateNctsHeader());
	NctsHeader nctsHeader;

	NctsHeader CreateNctsHeader()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
		return nctsHeader;
	}
}
