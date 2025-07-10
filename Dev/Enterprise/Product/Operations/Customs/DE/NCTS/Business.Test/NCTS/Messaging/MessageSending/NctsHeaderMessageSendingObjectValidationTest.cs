using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using NUnit.Framework;

namespace Enterprise.Customs.DE.NCTS.Business.Testing;

[TestedType(typeof(NctsHeaderMessageSendingObjectValidation))]
sealed class NctsHeaderMessageSendingObjectValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckMessageType_DESNOT()
	{
		const string validationMessage = "Current Status of Arrival Declaration only allows Message Type ‘E_DES_NOT’ (Arrival Notification Remarks).";

		nctsHeader.BH_HeaderType = NctsMovementType.Codes.Arrival;

		var nctsHeaderMessageSendingObject = new NctsHeaderMessageSendingObject(nctsHeader) { MessageType = "ABC" };
		nctsHeader.BH_MessageStatus = string.Empty;
		AssertHasMessageError(nctsHeaderMessageSendingObject.MessageTypeInfo, validationMessage);

		nctsHeaderMessageSendingObject = new NctsHeaderMessageSendingObject(nctsHeader) { MessageType = NctsMessageTypeList.Codes.DESNOT };
		nctsHeader.BH_MessageStatus = string.Empty;
		AssertNoMessageError(nctsHeaderMessageSendingObject.MessageTypeInfo, validationMessage);
	}

	public void TestCheckMessageType_DESREM()
	{
		const string validationMessage = "Current Status of Arrival Declaration only allows Message Type ‘E_DES_REM’ (Unloading Remarks).";

		nctsHeader.BH_HeaderType = NctsMovementType.Codes.Arrival;

		var nctsHeaderMessageSendingObject = new NctsHeaderMessageSendingObject(nctsHeader);
		nctsHeader.ArrivalMovementHeader.BM_CustomsStatus = NCTS5ArrivalCustomsStatusList.Codes.UnloadPermissionGranted;
		nctsHeader.ArrivalMovementHeader.BM_MessageStatus = LogicalStatusList.Codes.Accepted;
		nctsHeaderMessageSendingObject.MessageType = "ABC";
		AssertHasMessageError(nctsHeaderMessageSendingObject.MessageTypeInfo, validationMessage);

		nctsHeaderMessageSendingObject = new NctsHeaderMessageSendingObject(nctsHeader);
		nctsHeader.ArrivalMovementHeader.BM_CustomsStatus = NCTS5ArrivalCustomsStatusList.Codes.UnloadPermissionGranted;
		nctsHeader.ArrivalMovementHeader.BM_MessageStatus = LogicalStatusList.Codes.Accepted;
		nctsHeaderMessageSendingObject.MessageType = NctsMessageTypeList.Codes.DESREM;
		AssertNoMessageError(nctsHeaderMessageSendingObject.MessageTypeInfo, validationMessage);
	}

	protected override void SetUp()
	{
		base.SetUp();
		nctsHeader = Factory.New<NctsHeader>();
	}

	NctsHeader nctsHeader;
}
