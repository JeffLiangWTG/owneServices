using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

sealed class SafeCustomsStatusSetterTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("When entryHeader is null", () => new SafeCustomsStatusSetter(entryHeader: null));
	}

	public void TestTrySetDeposited()
	{
		CombineAssertions(() =>
		{
			AssertSetEntryStatus("", (manager) => manager.TrySetDeposited(), "DEP");
			AssertSetEntryStatus("DEP", (manager) => manager.TrySetDeposited(), "DEP");
			AssertSetEntryStatus("REG", (manager) => manager.TrySetDeposited(), "REG");
			AssertSetEntryStatus("UCL", (manager) => manager.TrySetDeposited(), "UCL");
			AssertSetEntryStatus("ICC", (manager) => manager.TrySetDeposited(), "ICC");
			AssertSetEntryStatus("ECC", (manager) => manager.TrySetDeposited(), "ECC");
			AssertSetEntryStatus("AMG", (manager) => manager.TrySetDeposited(), "AMG");
			AssertSetEntryStatus("AMD", (manager) => manager.TrySetDeposited(), "AMD");
			AssertSetEntryStatus("CNG", (manager) => manager.TrySetDeposited(), "CNG");
			AssertSetEntryStatus("CNC", (manager) => manager.TrySetDeposited(), "CNC");

			AssertSetEntryStatus("XYZ", (manager) => manager.TrySetDeposited(), "DEP");
		});
	}

	public void TestTrySetRegistered()
	{
		CombineAssertions(() =>
		{
			AssertSetEntryStatus("", (manager) => manager.TrySetRegistered(), "REG");
			AssertSetEntryStatus("DEP", (manager) => manager.TrySetRegistered(), "REG");
			AssertSetEntryStatus("REG", (manager) => manager.TrySetRegistered(), "REG");
			AssertSetEntryStatus("UCL", (manager) => manager.TrySetRegistered(), "UCL");
			AssertSetEntryStatus("ICC", (manager) => manager.TrySetRegistered(), "ICC");
			AssertSetEntryStatus("ECC", (manager) => manager.TrySetRegistered(), "ECC");
			AssertSetEntryStatus("AMG", (manager) => manager.TrySetRegistered(), "AMG");
			AssertSetEntryStatus("AMD", (manager) => manager.TrySetRegistered(), "AMD");
			AssertSetEntryStatus("CNG", (manager) => manager.TrySetRegistered(), "CNG");
			AssertSetEntryStatus("CNC", (manager) => manager.TrySetRegistered(), "CNC");

			AssertSetEntryStatus("XYZ", (manager) => manager.TrySetRegistered(), "REG");
		});
	}

	public void TestTrySetUnderControl()
	{
		CombineAssertions(() =>
		{
			AssertSetEntryStatus("", (manager) => manager.TrySetUnderControl(), "UCL");
			AssertSetEntryStatus("DEP", (manager) => manager.TrySetUnderControl(), "UCL");
			AssertSetEntryStatus("REG", (manager) => manager.TrySetUnderControl(), "UCL");
			AssertSetEntryStatus("UCL", (manager) => manager.TrySetUnderControl(), "UCL");
			AssertSetEntryStatus("ICC", (manager) => manager.TrySetUnderControl(), "ICC");
			AssertSetEntryStatus("ECC", (manager) => manager.TrySetUnderControl(), "ECC");
			AssertSetEntryStatus("AMG", (manager) => manager.TrySetUnderControl(), "AMG");
			AssertSetEntryStatus("AMD", (manager) => manager.TrySetUnderControl(), "AMD");
			AssertSetEntryStatus("CNG", (manager) => manager.TrySetUnderControl(), "CNG");
			AssertSetEntryStatus("CNC", (manager) => manager.TrySetUnderControl(), "CNC");

			AssertSetEntryStatus("XYZ", (manager) => manager.TrySetUnderControl(), "UCL");
		});
	}

	public void TestTrySetCleared()
	{
		CombineAssertions("When declaration is import", () =>
		{
			entryHeader.Declaration.JE_MessageType = "IMP";
			AssertSetCustomsStatusAndClearanceDate("", "", (manager) => manager.TrySetCleared(ZDateTime.BrettsBirthday), "ICC", "CLO", ZDateTime.BrettsBirthday);
			AssertSetCustomsStatusAndClearanceDate("DEP", "ACO", (manager) => manager.TrySetCleared(ZDateTime.BrettsBirthday), "ICC", "CLO", ZDateTime.BrettsBirthday);
			AssertSetCustomsStatusAndClearanceDate("REG", "ACO", (manager) => manager.TrySetCleared(ZDateTime.BrettsBirthday), "ICC", "CLO", ZDateTime.BrettsBirthday);
			AssertSetCustomsStatusAndClearanceDate("UCL", "AWO", (manager) => manager.TrySetCleared(ZDateTime.BrettsBirthday), "ICC", "CLO", ZDateTime.BrettsBirthday);
			AssertSetCustomsStatusAndClearanceDate("ICC", "CLO", (manager) => manager.TrySetCleared(ZDateTime.BrettsBirthday), "ICC", "CLO", ZDateTime.BrettsBirthday);
			AssertSetCustomsStatusAndClearanceDate("ECC", "CLO", (manager) => manager.TrySetCleared(ZDateTime.BrettsBirthday), "ECC", "CLO", ZDateTime.BrettsBirthday);
			AssertSetCustomsStatusAndClearanceDate("AMG", "ACS", (manager) => manager.TrySetCleared(ZDateTime.BrettsBirthday), "AMG", "ACS", ZDateTime.BrettsBirthday);
			AssertSetCustomsStatusAndClearanceDate("AMD", "ACO", (manager) => manager.TrySetCleared(ZDateTime.BrettsBirthday), "AMD", "ACO", ZDateTime.BrettsBirthday);
			AssertSetCustomsStatusAndClearanceDate("CNG", "", (manager) => manager.TrySetCleared(ZDateTime.BrettsBirthday), "CNG", "", ZDateTime.BrettsBirthday);
			AssertSetCustomsStatusAndClearanceDate("CNC", "AWO", (manager) => manager.TrySetCleared(ZDateTime.BrettsBirthday), "CNC", "AWO", ZDateTime.BrettsBirthday);

			AssertSetCustomsStatusAndClearanceDate("XYZ", "", (manager) => manager.TrySetCleared(ZDateTime.BrettsBirthday), "ICC", "CLO", ZDateTime.BrettsBirthday);
		});

		CombineAssertions("When declaration is export", () =>
		{
			entryHeader.Declaration.JE_MessageType = "EXP";
			AssertSetCustomsStatusAndClearanceDate("", "", (manager) => manager.TrySetCleared(ZDateTime.BrettsBirthday), "ECC", "CLO", ZDateTime.BrettsBirthday);
			AssertSetCustomsStatusAndClearanceDate("DEP", "ACO", (manager) => manager.TrySetCleared(ZDateTime.BrettsBirthday), "ECC", "CLO", ZDateTime.BrettsBirthday);
			AssertSetCustomsStatusAndClearanceDate("REG", "ACO", (manager) => manager.TrySetCleared(ZDateTime.BrettsBirthday), "ECC", "CLO", ZDateTime.BrettsBirthday);
			AssertSetCustomsStatusAndClearanceDate("UCL", "AWO", (manager) => manager.TrySetCleared(ZDateTime.BrettsBirthday), "ECC", "CLO", ZDateTime.BrettsBirthday);
			AssertSetCustomsStatusAndClearanceDate("ICC", "CLO", (manager) => manager.TrySetCleared(ZDateTime.BrettsBirthday), "ICC", "CLO", ZDateTime.BrettsBirthday);
			AssertSetCustomsStatusAndClearanceDate("ECC", "CLO", (manager) => manager.TrySetCleared(ZDateTime.BrettsBirthday), "ECC", "CLO", ZDateTime.BrettsBirthday);
			AssertSetCustomsStatusAndClearanceDate("AMG", "ACS", (manager) => manager.TrySetCleared(ZDateTime.BrettsBirthday), "AMG", "ACS", ZDateTime.BrettsBirthday);
			AssertSetCustomsStatusAndClearanceDate("AMD", "ACO", (manager) => manager.TrySetCleared(ZDateTime.BrettsBirthday), "AMD", "ACO", ZDateTime.BrettsBirthday);
			AssertSetCustomsStatusAndClearanceDate("CNG", "", (manager) => manager.TrySetCleared(ZDateTime.BrettsBirthday), "CNG", "", ZDateTime.BrettsBirthday);
			AssertSetCustomsStatusAndClearanceDate("CNC", "AWO", (manager) => manager.TrySetCleared(ZDateTime.BrettsBirthday), "CNC", "AWO", ZDateTime.BrettsBirthday);

			AssertSetCustomsStatusAndClearanceDate("XYZ", "", (manager) => manager.TrySetCleared(ZDateTime.BrettsBirthday), "ECC", "CLO", ZDateTime.BrettsBirthday);
		});
	}

	public void TestTrySetExitCompleted()
	{
		CombineAssertions(() =>
		{
			AssertSetEntryStatus("", (manager) => manager.TrySetExitCompleted(), "EXI");
			AssertSetEntryStatus("DEP", (manager) => manager.TrySetExitCompleted(), "EXI");
			AssertSetEntryStatus("REG", (manager) => manager.TrySetExitCompleted(), "EXI");
			AssertSetEntryStatus("UCL", (manager) => manager.TrySetExitCompleted(), "EXI");
			AssertSetEntryStatus("ICC", (manager) => manager.TrySetExitCompleted(), "EXI");
			AssertSetEntryStatus("ECC", (manager) => manager.TrySetExitCompleted(), "EXI");
			AssertSetEntryStatus("AMG", (manager) => manager.TrySetExitCompleted(), "AMG");
			AssertSetEntryStatus("AMD", (manager) => manager.TrySetExitCompleted(), "AMD");
			AssertSetEntryStatus("CNG", (manager) => manager.TrySetExitCompleted(), "CNG");
			AssertSetEntryStatus("CNC", (manager) => manager.TrySetExitCompleted(), "CNC");

			AssertSetEntryStatus("XYZ", (manager) => manager.TrySetExitCompleted(), "EXI");
		});
	}

	public void TestTrySetCancelled()
	{
		CombineAssertions(() =>
		{
			AssertSetEntryStatus("", (manager) => manager.TrySetCancelled(), "CNC");
			AssertSetEntryStatus("DEP", (manager) => manager.TrySetCancelled(), "CNC");
			AssertSetEntryStatus("REG", (manager) => manager.TrySetCancelled(), "CNC");
			AssertSetEntryStatus("UCL", (manager) => manager.TrySetCancelled(), "CNC");
			AssertSetEntryStatus("ICC", (manager) => manager.TrySetCancelled(), "CNC");
			AssertSetEntryStatus("ECC", (manager) => manager.TrySetCancelled(), "CNC");
			AssertSetEntryStatus("AMG", (manager) => manager.TrySetCancelled(), "CNC");
			AssertSetEntryStatus("AMD", (manager) => manager.TrySetCancelled(), "AMD");
			AssertSetEntryStatus("CNG", (manager) => manager.TrySetCancelled(), "CNC");
			AssertSetEntryStatus("CNC", (manager) => manager.TrySetCancelled(), "CNC");

			AssertSetEntryStatus("XYZ", (manager) => manager.TrySetCancelled(), "CNC");
		});
	}

	public void TestTrySetAmended()
	{
		CombineAssertions(() =>
		{
			AssertSetEntryStatus("", (manager) => manager.TrySetAmended(), "AMD");
			AssertSetEntryStatus("DEP", (manager) => manager.TrySetAmended(), "AMD");
			AssertSetEntryStatus("REG", (manager) => manager.TrySetAmended(), "AMD");
			AssertSetEntryStatus("UCL", (manager) => manager.TrySetAmended(), "AMD");
			AssertSetEntryStatus("ICC", (manager) => manager.TrySetAmended(), "AMD");
			AssertSetEntryStatus("ECC", (manager) => manager.TrySetAmended(), "AMD");
			AssertSetEntryStatus("AMG", (manager) => manager.TrySetAmended(), "AMD");
			AssertSetEntryStatus("AMD", (manager) => manager.TrySetAmended(), "AMD");
			AssertSetEntryStatus("CNG", (manager) => manager.TrySetAmended(), "AMD");
			AssertSetEntryStatus("CNC", (manager) => manager.TrySetAmended(), "CNC");

			AssertSetEntryStatus("XYZ", (manager) => manager.TrySetAmended(), "AMD");
		});
	}

	public void TestSetAsAcknowledgeWhenCustomsStatusChange()
	{
		CombineAssertions(() =>
		{
			entryHeader.Declaration.JE_MessageType = "IMP";
			AssertSetCustomsStatusAndMessageStatus("", "", m => m.TrySetRegistered(), "REG", "ACO");
			AssertSetCustomsStatusAndMessageStatus("REG", "", m => m.TrySetRegistered(), "REG", "");

			AssertSetCustomsStatusAndMessageStatus("REG", "AWO", m => m.TrySetUnderControl(), "UCL", "ACO");
			AssertSetCustomsStatusAndMessageStatus("AMG", "ACS", m => m.TrySetUnderControl(), "AMG", "ACS");
		});
	}

	public void TestTrySetErrorOriginal()
	{
		CombineAssertions(() =>
		{
			AssertSetMessageStatus("", (manager) => manager.TrySetErrorOriginal(), "ERO");
			AssertSetMessageStatus("AWO", (manager) => manager.TrySetErrorOriginal(), "ERO");
			AssertSetMessageStatus("ACO", (manager) => manager.TrySetErrorOriginal(), "ERO");
			AssertSetMessageStatus("CLO", (manager) => manager.TrySetErrorOriginal(), "ERO");
			AssertSetMessageStatus("ACS", (manager) => manager.TrySetErrorOriginal(), "ERO");
			AssertSetMessageStatus("FFT", (manager) => manager.TrySetErrorOriginal(), "FFT");
		});
	}

	public void TestTrySetAcknowledged()
	{
		CombineAssertions(() =>
		{
			AssertSetMessageStatus("", (manager) => manager.TrySetAcknowledged(), "ACO");
			AssertSetMessageStatus("AWO", (manager) => manager.TrySetAcknowledged(), "ACO");
			AssertSetMessageStatus("CLO", (manager) => manager.TrySetAcknowledged(), "CLO");
			AssertSetMessageStatus("ACS", (manager) => manager.TrySetAcknowledged(), "ACS");
			AssertSetMessageStatus("ERO", (manager) => manager.TrySetAcknowledged(), "ERO");
			AssertSetMessageStatus("FFT", (manager) => manager.TrySetAcknowledged(), "FFT");
		});
	}

	public void TestTrySetAcceptedBySystem()
	{
		CombineAssertions(() =>
		{
			AssertSetMessageStatus("", (manager) => manager.TrySetAcceptedBySystem(), "ACS");
			AssertSetMessageStatus("AWO", (manager) => manager.TrySetAcceptedBySystem(), "ACS");
			AssertSetMessageStatus("ACO", (manager) => manager.TrySetAcceptedBySystem(), "ACS");
			AssertSetMessageStatus("CLO", (manager) => manager.TrySetAcceptedBySystem(), "ACS");
			AssertSetMessageStatus("ERO", (manager) => manager.TrySetAcceptedBySystem(), "ERO");
			AssertSetMessageStatus("FFT", (manager) => manager.TrySetAcceptedBySystem(), "FFT");
		});
	}

	public void TrySetGoodsWrittenOffClosed()
	{
		AssertEquals("TrySetGoodsWrittenOffClosed", false, safeCustomsStatusSetter.TrySetGoodsWrittenOffClosed());
	}

	protected override void SetUp()
	{
		base.SetUp();
		var declaration = Factory.New<JobDeclaration>();
		entryHeader = declaration.CustomsEntryHeaders.AddNew();
		safeCustomsStatusSetter = new SafeCustomsStatusSetter(entryHeader);
	}

	CusEntryHeader entryHeader;
	ICustomsStatusSetter safeCustomsStatusSetter;

	void AssertSetEntryStatus(ZString originalStatus,
		Func<ICustomsStatusSetter, bool> setCustomsStatusAction,
		ZString expectedFinalStatus)
	{
		entryHeader.CH_EntryStatus = originalStatus;
		var result = setCustomsStatusAction(safeCustomsStatusSetter);
		AssertEquals("Allow to change status?", originalStatus != expectedFinalStatus, result);
		AssertEquals($"When original entry status is '{originalStatus}', expected final is", expectedFinalStatus, entryHeader.CH_EntryStatus);
	}

	void AssertSetCustomsStatusAndClearanceDate(ZString originalStatus,
		ZString originalMessageStatus,
		Func<ICustomsStatusSetter, bool> setCustomsStatusAction,
		ZString expectedFinalStatus,
		ZString expectedMessageStatus,
		ZDateTime expectedClearanceDate)
	{
		entryHeader.CH_EntryReleaseDate = ZDateTime.Empty;
		AssertSetCustomsStatusAndMessageStatus(originalStatus, originalMessageStatus, setCustomsStatusAction, expectedFinalStatus, expectedMessageStatus);
		AssertEquals($"When original entry status is '{originalStatus}', expected final is", expectedClearanceDate, entryHeader.CH_EntryReleaseDate);
	}

	void AssertSetCustomsStatusAndMessageStatus(ZString originalStatus,
		ZString originalMessageStatus,
		Func<ICustomsStatusSetter, bool> setCustomsStatusAction,
		ZString expectedFinalStatus,
		ZString expectedMessageStatus)
	{
		entryHeader.CH_Status = originalMessageStatus;
		AssertSetEntryStatus(originalStatus, setCustomsStatusAction, expectedFinalStatus);
		AssertEquals($"When original message status is '{originalMessageStatus}', expected final is", expectedMessageStatus, entryHeader.CH_Status);
	}

	void AssertSetMessageStatus(ZString originalStatus,
		Func<ICustomsStatusSetter, bool> setMessageStatusAction,
		ZString expectedFinalStatus)
	{
		entryHeader.CH_Status = originalStatus;
		var result = setMessageStatusAction(safeCustomsStatusSetter);
		AssertEquals("Allow to change status?", originalStatus != expectedFinalStatus, result);
		AssertEquals($"When original message status is '{originalStatus}', expected final is", expectedFinalStatus, entryHeader.CH_Status);
	}
}
