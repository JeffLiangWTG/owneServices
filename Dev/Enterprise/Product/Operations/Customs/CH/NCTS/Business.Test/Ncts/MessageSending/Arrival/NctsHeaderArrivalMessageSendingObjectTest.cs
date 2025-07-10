using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.CH.Business.Testing;
using Enterprise.Customs.EU.NCTS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

[TestedType(typeof(NctsHeaderArrivalMessageSendingObject))]
sealed class NctsHeaderArrivalMessageSendingObjectTest : NctsHeaderCommonMessageSendingObjectTest
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentNullException>("When nctsHeader is null", () => new NctsHeaderArrivalMessageSendingObject(null));
			NctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
			AssertExceptionThrown<ArgumentException>("When nctsHeader is not a arrival job", () => new NctsHeaderArrivalMessageSendingObject(NctsHeader));
		});
	}

	public void TestLookups()
	{
		CombineAssertions(() =>
		{
			AssertType<NctsHeaderArrivalMessageSendingObjectLookups>(SendingObject.Lookups);
			AssertSame("cached", SendingObject.Lookups, SendingObject.Lookups);
		});
	}

	public void TestGetNewValidation()
	{
		CombineAssertions(() =>
		{
			AssertType<NctsHeaderCommonMessageSendingObjectValidation>(SendingObject.Validation);
		});
	}

	public void TestCaptions()
	{
		CombineAssertions(() =>
		{
			CaptionTestHelper.AssertCaptions(SendingObject.LRNInfo, caption: "Registration Number (LRN)");
			CaptionTestHelper.AssertCaptions(SendingObject.MessageTypeInfo, caption: "Message Type");
		});
	}

	public void TestSchema()
	{
		CombineAssertions(() =>
		{
			AssertEquals("MaxLength", 22, SendingObject.LRNInfo.MaxLength);
			AssertEquals("MaxLength", 35, SendingObject.MRNInfo.MaxLength);
			AssertEquals("MaxLength", 5, SendingObject.MessageTypeInfo.MaxLength);
		});
	}

	public void TestRegistrationNumber()
	{
		NctsHeader.ArrivalMovementHeader.BM_PaperlessInbondNum = "22CH123456789012N0";
		AssertEquals("22CH123456789012N0", SendingObject.LRN);
	}

	public void TestMessageType_Default()
	{
		var sendingObject = GetNewBusinessObject() as NctsHeaderArrivalMessageSendingObject;
		var firstElement = sendingObject.Lookups.MessageTypeList[0];
		AssertEquals("MessageType default", firstElement.Code, sendingObject.MessageType);
	}

	public void TestShouldSendDefaultValue() => CombineAssertions(() =>
	{
		const string lrn = "22CH123456789012N0";

		var masterHeader = CreateNctsHeader();
		masterHeader.ArrivalMovementHeader.BM_PaperlessInbondNum = lrn;
		masterHeader.ArrivalMovementHeader.BM_CustomsStatus = NCTS5ArrivalCustomsStatusList.Codes.UnloadPermissionGranted;
		var mrn = masterHeader.ArrivalMovementHeader.MovementReferenceNumbers.AddNew();
		mrn.CSI_ReferenceNumber = "123456789CH";
		mrn.CSI_Status = YesNoList.Codes.Yes;

		var childHeader = CreateNctsHeader();
		childHeader.ArrivalMovementHeader.BM_PaperlessInbondNum = lrn;
		childHeader.ArrivalMovementHeader.MultipleMRNIndicator = false;
		childHeader.ArrivalMovementHeader.BM_CustomsStatus = NCTS5ArrivalCustomsStatusList.Codes.UnloadPermissionGranted;
		childHeader.ArrivalMovementHeader.BM_NoChangesToReport = true;
		childHeader.MovementReferenceNumberSetter("123456789CH");

		masterHeader.ArrivalMovementHeader.RelatedArrivalMovements.AddPivotFor(childHeader.ArrivalMovementHeader);

		AssertShouldSendForMultiMrn(YesNoList.Codes.Yes, NCTS5ArrivalCustomsStatusList.Codes.UnloadPermissionGranted, ZString.Empty, true, true, false);

		AssertShouldSendForMultiMrn(ZString.Empty, NCTS5ArrivalCustomsStatusList.Codes.UnloadPermissionGranted, ZString.Empty, true, true, false);

		AssertShouldSendForMultiMrn(YesNoList.Codes.No, NCTS5ArrivalCustomsStatusList.Codes.UnloadPermissionGranted, ZString.Empty, false, false, true);

		AssertShouldSendForMultiMrn(YesNoList.Codes.Yes, NCTS5ArrivalCustomsStatusList.Codes.UnloadRemarks, CHLogicalStatusList.Codes.Invalid, true, true, false);

		AssertShouldSendForMultiMrn(YesNoList.Codes.Yes, ZString.Empty, CHLogicalStatusList.Codes.Invalid, true, false, true);

		sendingObject = new NctsHeaderArrivalMessageSendingObject(NctsHeader);
		AssertEquals("Default value when is not Multi MRN it should be: ", true, sendingObject.ShouldSend);
		AssertEquals("ReadOnly", true, sendingObject.ShouldSendInfo.ReadOnly);

		void AssertShouldSendForMultiMrn(string mrnSealsState, string customsStatus, string messageStatus, bool noChangesToReport, bool expectedShouldSend, bool expectedReadOnly)
		{
			mrn.CSI_Status = mrnSealsState;
			childHeader.ArrivalMovementHeader.BM_CustomsStatus = customsStatus;
			childHeader.ArrivalMovementHeader.BM_MessageStatus = messageStatus;
			childHeader.ArrivalMovementHeader.BM_NoChangesToReport = noChangesToReport;
			var sendingObject = new NctsHeaderArrivalMessageSendingObject(childHeader, true);
			AssertEquals($"ShouldSend - CSI_Status:{mrnSealsState}, CustomsStatus:{customsStatus}, MessageStatus:{messageStatus}, NoChangesToReport:{noChangesToReport}", expectedShouldSend, sendingObject.ShouldSend);
			AssertEquals($"ReadOnly - CSI_Status:{mrnSealsState}, CustomsStatus:{customsStatus}, MessageStatus:{messageStatus}, NoChangesToReport:{noChangesToReport}", expectedReadOnly, sendingObject.ShouldSendInfo.ReadOnly);
		}
	});

	public void TestMessageSubTypeForEDIMessageNT007() => AssertMessageSubTypeForEDIMessage(PassarMessageTypeList.Codes.NT007, MessageSubTypeCodeList.Codes.PassarArrivalNotification, SendingObject);

	public void TestToMessageStringForNT007() => AssertTestToMessageString(PassarMessageTypeList.Codes.NT007, SendingObject);

	public void TestMessageSubTypeForEDIMessageNT044() => AssertMessageSubTypeForEDIMessage(PassarMessageTypeList.Codes.NT044, MessageSubTypeCodeList.Codes.PassarInventoryResult, SendingObject);

	public void TestToMessageStringForNT044() => AssertTestToMessageString(PassarMessageTypeList.Codes.NT044, SendingObject);

	protected override BusinessObject GetNewBusinessObject()
	{
		return new NctsHeaderArrivalMessageSendingObject(NctsHeader);
	}

	NctsHeader NctsHeader => nctsHeader ?? (nctsHeader = CreateNctsHeader());
	NctsHeader nctsHeader;

	NctsHeader CreateNctsHeader()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.BH_HeaderType = NctsMovementType.Codes.Arrival;
		return nctsHeader;
	}

	NctsHeaderArrivalMessageSendingObject SendingObject => sendingObject ?? (sendingObject = new NctsHeaderArrivalMessageSendingObject(NctsHeader));
	NctsHeaderArrivalMessageSendingObject sendingObject;
}
