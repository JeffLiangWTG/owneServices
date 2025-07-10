using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.IT.Business;
using Enterprise.Customs.IT.Business.Testing;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.IT.NCTS.Business.Testing;

sealed class NctsDepartureSafeCustomsStatusSetterTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("When movementHeader is null",
			() => new NctsDepartureSafeCustomsStatusSetter(movementHeader: null));
	}

	public void TestTrySetAcceptedBySystem()
	{
		CombineAssertions(() =>
		{
			AssertSetStatus("", "", (manager) => manager.TrySetAcceptedBySystem(), "ACS", "ACC");
			AssertSetStatus("", "MDN", (manager) => manager.TrySetAcceptedBySystem(), "ACS", "ACC");
			AssertSetStatus("ACK", "ACC", (manager) => manager.TrySetAcceptedBySystem(), "ACS", "ACC");
			AssertSetStatus("REL", "ACC", (manager) => manager.TrySetAcceptedBySystem(), "ACS", "ACC");
			movementHeader.BM_Phase = "014";
			AssertSetStatus("", "SNT", (manager) => manager.TrySetAcceptedBySystem(), "ACS", "ACC");
			AssertSetStatus("CAN", "ACC", (manager) => manager.TrySetAcceptedBySystem(), "CAN", "ACC");
		});
	}

	public void TestTrySetAcknowledged()
	{
		CombineAssertions(() =>
		{
			AssertSetStatus("", "", (manager) => manager.TrySetAcknowledged(), "ACK", "ACC");
			AssertSetStatus("", "MDN", (manager) => manager.TrySetAcknowledged(), "ACK", "ACC");
			AssertSetStatus("", "SNT", (manager) => manager.TrySetAcknowledged(), "ACK", "ACC");
			AssertSetStatus("MRN", "ACC", (manager) => manager.TrySetAcknowledged(), "MRN", "ACC");
			AssertSetStatus("REL", "ACC", (manager) => manager.TrySetAcknowledged(), "REL", "ACC");
			AssertSetStatus("CO3", "ACC", (manager) => manager.TrySetAcknowledged(), "CO3", "ACC");

			movementHeader.BM_Phase = "014";
			AssertSetStatus("", "SNT", (manager) => manager.TrySetAcknowledged(), "ACK", "ACC");
			AssertSetStatus("ACS", "ACC", (manager) => manager.TrySetAcknowledged(), "ACS", "ACC");
			AssertSetStatus("CAN", "ACC", (manager) => manager.TrySetAcknowledged(), "CAN", "ACC");

			AssertSetStatus("XYZ", "XYZ", (manager) => manager.TrySetAcknowledged(), "ACK", "ACC");
		});
	}

	public void TestTrySetCancelled()
	{
		CombineAssertions(() =>
		{
			AssertSetStatus("", "", (manager) => manager.TrySetCancelled(), "CAN", "ACC");
			AssertSetStatus("MRN", "ACC", (manager) => manager.TrySetCancelled(), "CAN", "ACC");
			AssertSetStatus("REL", "ACC", (manager) => manager.TrySetCancelled(), "CAN", "ACC");
			AssertSetStatus("CAN", "ACC", (manager) => manager.TrySetCancelled(), "CAN", "ACC");

			movementHeader.BM_Phase = "014";
			AssertSetStatus("", "SNT", (manager) => manager.TrySetCancelled(), "CAN", "ACC");
			AssertSetStatus("ACS", "ACC", (manager) => manager.TrySetCancelled(), "CAN", "ACC");

			AssertSetStatus("XYZ", "XYZ", (manager) => manager.TrySetCancelled(), "CAN", "ACC");
		});
	}

	public void TestTrySetUnderControl()
	{
		CombineAssertions(() =>
		{
			AssertSetStatus("", "", (manager) => manager.TrySetUnderControl(), "CO3", "ACC");
			AssertSetStatus("MRN", "ACC", (manager) => manager.TrySetUnderControl(), "CO3", "ACC");
			AssertSetStatus("MRN", "ERR", (manager) => manager.TrySetUnderControl(), "CO3", "ACC");
			AssertSetStatus("NRL", "ACC", (manager) => manager.TrySetUnderControl(), "CO3", "ACC");
			AssertSetStatus("REL", "ACC", (manager) => manager.TrySetUnderControl(), "REL", "ACC");

			movementHeader.BM_Phase = "014";
			AssertSetStatus("", "SNT", (manager) => manager.TrySetUnderControl(), "", "SNT");
			AssertSetStatus("ACK", "ACC", (manager) => manager.TrySetUnderControl(), "ACK", "ACC");

			AssertSetStatus("ACS", "ACC", (manager) => manager.TrySetUnderControl(), "ACS", "ACC");
			AssertSetStatus("CAN", "ACC", (manager) => manager.TrySetUnderControl(), "CAN", "ACC");

			AssertSetStatus("XYZ", "", (manager) => manager.TrySetUnderControl(), "XYZ", "");
		});
	}

	public void TestTrySetCleared()
	{
		CombineAssertions(() =>
		{
			AssertSetStatus("", "", (manager) => manager.TrySetCleared(ZDateTime.BrettsBirthday), "REL", "ACC");
			AssertSetStatus("MRN", "ACC", (manager) => manager.TrySetCleared(ZDateTime.BrettsBirthday), "REL", "ACC");
			AssertSetStatus("MRN", "ERR", (manager) => manager.TrySetCleared(ZDateTime.BrettsBirthday), "REL", "ACC");
			AssertSetStatus("NRL", "ACC", (manager) => manager.TrySetCleared(ZDateTime.BrettsBirthday), "REL", "ACC");
			AssertSetStatus("REL", "ACC", (manager) => manager.TrySetCleared(ZDateTime.BrettsBirthday), "REL", "ACC");

			movementHeader.BM_Phase = "014";
			AssertSetStatus("", "SNT", (manager) => manager.TrySetCleared(ZDateTime.BrettsBirthday), "", "SNT");
			AssertSetStatus("ACK", "ACC", (manager) => manager.TrySetCleared(ZDateTime.BrettsBirthday), "ACK", "ACC");

			AssertSetStatus("ACS", "ACC", (manager) => manager.TrySetCleared(ZDateTime.BrettsBirthday), "ACS", "ACC");
			AssertSetStatus("CAN", "ACC", (manager) => manager.TrySetCleared(ZDateTime.BrettsBirthday), "CAN", "ACC");

			AssertSetStatus("XYZ", "", (manager) => manager.TrySetCleared(ZDateTime.BrettsBirthday), "XYZ", "");
		});
	}

	public void TestTrySetErrorOriginal()
	{
		CombineAssertions(() =>
		{
			AssertSetStatus("", "", (manager) => manager.TrySetErrorOriginal(), "", "ERR");
			AssertSetStatus("", "MDS", (manager) => manager.TrySetErrorOriginal(), "", "ERR");
			AssertSetStatus("ACK", "ACC", (manager) => manager.TrySetErrorOriginal(), "", "ERR");
			AssertSetStatus("MRN", "", (manager) => manager.TrySetErrorOriginal(), "", "ERR");
			AssertSetStatus("MRN", "MDS", (manager) => manager.TrySetErrorOriginal(), "", "ERR");
		});
	}

	public void TestTrySetRegistered()
	{
		CombineAssertions(() =>
		{
			AssertSetStatus("", "", (manager) => manager.TrySetRegistered(), "MRN", "ACC");
			AssertSetStatus("", "MDN", (manager) => manager.TrySetRegistered(), "MRN", "ACC");
			AssertSetStatus("", "SNT", (manager) => manager.TrySetRegistered(), "MRN", "ACC");
			AssertSetStatus("MRN", "ACC", (manager) => manager.TrySetRegistered(), "MRN", "ACC");
			AssertSetStatus("DLR", "ACC", (manager) => manager.TrySetRegistered(), "MRN", "ACC");
			AssertSetStatus("REL", "ACC", (manager) => manager.TrySetRegistered(), "REL", "ACC");

			AssertSetStatus("XYZ", "XYZ", (manager) => manager.TrySetRegistered(), "MRN", "ACC");
		});
	}

	public void TestTrySetGoodsWrittenOffClosed()
	{
		CombineAssertions(() =>
		{
			AssertSetStatus("", "", (manager) => manager.TrySetGoodsWrittenOffClosed(), "WRO", "ACC");
			AssertSetStatus("MRN", "ACC", (manager) => manager.TrySetGoodsWrittenOffClosed(), "WRO", "ACC");
			AssertSetStatus("MRN", "ERR", (manager) => manager.TrySetGoodsWrittenOffClosed(), "WRO", "ACC");
			AssertSetStatus("NRL", "ACC", (manager) => manager.TrySetGoodsWrittenOffClosed(), "WRO", "ACC");
			AssertSetStatus("REL", "ACC", (manager) => manager.TrySetGoodsWrittenOffClosed(), "WRO", "ACC");

			movementHeader.BM_Phase = "014";
			AssertSetStatus("", "SNT", (manager) => manager.TrySetGoodsWrittenOffClosed(), "", "SNT");
			AssertSetStatus("ACK", "ACC", (manager) => manager.TrySetGoodsWrittenOffClosed(), "ACK", "ACC");

			AssertSetStatus("ACS", "ACC", (manager) => manager.TrySetGoodsWrittenOffClosed(), "ACS", "ACC");
			AssertSetStatus("CAN", "ACC", (manager) => manager.TrySetGoodsWrittenOffClosed(), "CAN", "ACC");

			AssertSetStatus("XYZ", "", (manager) => manager.TrySetGoodsWrittenOffClosed(), "XYZ", "");
		});
	}

	public void TestLookupCustomsStatusValue()
	{
		CombineAssertions(() =>
		{
			AssertEquals("When Status is Empty", -1, NctsDepartureSafeCustomsStatusSetter.LookupCustomsStatusValue(""));
			AssertEquals("When Status is MRN", 1, NctsDepartureSafeCustomsStatusSetter.LookupCustomsStatusValue("MRN"));
			AssertEquals("When Status is WRO", 6, NctsDepartureSafeCustomsStatusSetter.LookupCustomsStatusValue("WRO"));
			AssertEquals("When Status is XXX", -1, NctsDepartureSafeCustomsStatusSetter.LookupCustomsStatusValue("XXX"));
		});
	}

	public void TestGetCustomsStatusCodesForValue()
	{
		CombineAssertions(() =>
		{
			AssertArrayEqualsByElements("When Status value is -1", Array.Empty<string>(), NctsDepartureSafeCustomsStatusSetter.GetCustomsStatusCodesForValue(-1).ToArray());
			AssertArrayEqualsByElements("When Status value is 0", new[] { "ACK" }, NctsDepartureSafeCustomsStatusSetter.GetCustomsStatusCodesForValue(0).ToArray());
			AssertArrayEqualsByElements("When Status value is 5", new[] { "CAN", "AMR" }, NctsDepartureSafeCustomsStatusSetter.GetCustomsStatusCodesForValue(5).ToArray());
			AssertArrayEqualsByElements("When Status value is 9", Array.Empty<string>(), NctsDepartureSafeCustomsStatusSetter.GetCustomsStatusCodesForValue(9).ToArray());
		});
	}

	public void TestTrySetAmended()
	{
		var header = movementHeader.Header;
		header.BH_ApplicationCode = "NC5";
		movementHeader.BM_CustomsStatus = "XXX";
		movementHeader.BM_MessageStatus = "ZZZ";

		CombineAssertions("TrySetAmended() when Ncts has no messages", () =>
		{
			AssertEquals("TrySetAmended result", false, safeCustomsStatusSetter.TrySetAmended());
			AssertEquals("BM_CustomsMessage", "XXX", movementHeader.BM_CustomsStatus);
			AssertEquals("BM_MessageStatus", "ZZZ", movementHeader.BM_MessageStatus);
		});

		var sessionGuid = ZGuid.NewZGuid();
		var sentMessage = movementHeader.Messages.AddNew();
		sentMessage.EM_ApplicationCode = "ITH";
		sentMessage.EM_MessageType = "NEW";
		sentMessage.EM_Status = "SNT";

		var sentInterchange = Factory.New<EDIInterchange>();
		sentInterchange.EI_SessionGUID = sessionGuid;
		sentMessage.EM_EI = sentInterchange.PK;

		var resMessage = movementHeader.Messages.AddNew();
		resMessage.EM_ApplicationCode = "ITH";
		resMessage.EM_MessageType = "RES";
		resMessage.EM_Status = "RCV";
		resMessage.EM_MessageText = ManifestResourceHelper.ReadManifestResourceContent("Enterprise.Customs.IT.NCTS.Business.Testing.NCTS.MessageProcessors.Departure.AidaXml.TestFiles.NctsResponsePositiveWithMRN.xml");

		var resInterchange = Factory.New<EDIInterchange>();
		resInterchange.EI_SessionGUID = sessionGuid;
		resMessage.EM_EI = sentInterchange.PK;

		AssertEquals("TrySetAmended()", true, safeCustomsStatusSetter.TrySetAmended());
		AssertEquals("BM_CustomsMessage", "MRN", movementHeader.BM_CustomsStatus);
		AssertEquals("BM_CustomsMessage", "ACC", movementHeader.BM_MessageStatus);
	}

	public void TestTrySetDeposited()
	{
		AssertEquals("TrySetDeposited", false, safeCustomsStatusSetter.TrySetDeposited());
	}

	public void TestTrySetExitCompleted()
	{
		AssertEquals("TrySetExitCompleted", false, safeCustomsStatusSetter.TrySetExitCompleted());
	}

	protected override void SetUp()
	{
		base.SetUp();
		var header = Factory.NewDepartureNctsHeader();
		movementHeader = header.MovementHeader;
		safeCustomsStatusSetter = new NctsDepartureSafeCustomsStatusSetter(movementHeader);
	}

	NctsDepartureMovementHeader movementHeader;
	ICustomsStatusSetter safeCustomsStatusSetter;

	void AssertSetStatus(
		ZString originalCustomsStatus,
		ZString originalMessageStatus,
		Func<ICustomsStatusSetter, bool> setCustomsStatusAction,
		ZString expectedFinalCustomsStatus,
		ZString expectedFinalMessageStatus)
	{
		movementHeader.BM_CustomsStatus = originalCustomsStatus;
		movementHeader.BM_MessageStatus = originalMessageStatus;

		var statusHasChanged = setCustomsStatusAction(safeCustomsStatusSetter);
		var statusChangeIsExpected = originalCustomsStatus != expectedFinalCustomsStatus
			|| originalMessageStatus != expectedFinalMessageStatus;

		AssertEquals("Allow to change status?", statusChangeIsExpected, statusHasChanged);

		AssertEquals($"When original BM_CustomsStatus is '{originalCustomsStatus}', expected final is", expectedFinalCustomsStatus, movementHeader.BM_CustomsStatus);
		AssertEquals($"When original BM_MessageStatus is '{originalCustomsStatus}', expected final is", expectedFinalMessageStatus, movementHeader.BM_MessageStatus);
	}
}
