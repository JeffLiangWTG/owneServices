using System;
using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

sealed class CusEntryHeaderAmendmentTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentException>(() => new CusEntryHeaderAmendmentWrapper(null));
	}

	public void TestSetEntryAsAmending()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "IMP";
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();

		entryHeader.CH_EntryStatus = "REG";
		entryHeader.CH_Status = "ACO";

		entryHeader.SetAsAmending();

		CombineAssertions(() =>
		{
			AssertEquals(nameof(entryHeader.CH_EntryStatus), "AMG", entryHeader.CH_EntryStatus);
			AssertEquals(nameof(entryHeader.CH_Status), "", entryHeader.CH_Status);

			var hasLog = entryHeader
				.Logs.HasLogWith(x => x.SL_SE_NKEvent == "CSO" && x.SL_Reference.Contains("Entry set to AMG, original status: [REG | ACO]"));

			Assert("Log line related to AMG must be present", hasLog);
		});
	}

	public void TestSetEntryAsAmending_WithTotalEntryLinesAndMRN()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "IMP";
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryHeader.CH_EntryStatus = "REG";

		CombineAssertions(() =>
		{
			AssertEquals("MovementReferenceNumber is empty", ZString.Empty, entryHeader.MovementReferenceNumber);
			AssertEquals("ZG_SentEntryLinesCount is 0", 0, entryHeader.ZG_SentEntryLinesCount);
			AssertEquals("CH_AddInfo is empty", ZString.Empty, entryHeader.CH_AddInfo);

			entryHeader.SetAsAmending("24ITQYG08AAB1956J4", 12345);
			Factory.Save();

			AssertEquals("MovementReferenceNumber is added", "24ITQYG08AAB1956J4", entryHeader.MovementReferenceNumber);
			AssertEquals("ZG_SentEntryLinesCount is added", 12345, entryHeader.ZG_SentEntryLinesCount);
			AssertEquals("CH_AddInfo is updated", "SentEntryLinesCount=12345", entryHeader.CH_AddInfo);
			Assert("A log entry related to AMG must be found", entryHeader.Logs.HasLogWith(x => x.SL_SE_NKEvent == "CSO" && x.SL_Reference.Contains("Entry set to AMG, original status: [REG | ]")));
		});
	}

	public void TestIsInAmendableStatusWhenDontHaveMrn()
	{
		using (TemporarilySetIsUCC6Configuration(true))
		{
			AssertIsAmendableStatus(entryHeader, ITEntryStatusList.Codes.Registered, "ACO", expectedResult: false, "When Entry has no MRN");
		}
	}

	public void TestIsInAmendableStatusWhenDontHaveMrnAndStatusIsEmpty()
	{
		using (TemporarilySetIsUCC6Configuration(true))
		{
			AssertIsAmendableStatus(entryHeader, ITEntryStatusList.Codes.Registered, string.Empty, expectedResult: true);
		}
	}

	public void TestIsInAmendableStatusWhenDontHaveMrnAndStatusIsEmptyAndIsPluggedIntoShipment()
	{
		using (TemporarilySetIsUCC6Configuration(true))
		{
			var shipment = Factory.New<ForwardingShipment>();
			entryHeader.Declaration.JE_JS = shipment.PK;
			AssertIsAmendableStatus(entryHeader, ITEntryStatusList.Codes.Registered, string.Empty, expectedResult: false);
		}
	}

	public void TestIsInAmendableStatus_ForExpUcc6()
	{
		declaration.JE_MessageType = "EXP";
		entryHeader.MovementReferenceNumberSetter("23ITMRN0001");

		using (TemporarilySetIsUCC6Configuration(true))
		{
			CombineAssertions(() =>
			{
				foreach (var (entryStatus, status, expectedResult) in GetIsAmendableTestDataEntries())
				{
					AssertIsAmendableStatus(entryHeader, entryStatus, status, expectedResult);
				}

				AssertIsAmendableStatus(entryHeader, ITEntryStatusList.Codes.Exit, "ACO", expectedResult: true);
			});
		}
	}

	public void TestIsInAmendableStatus_ForImport()
	{
		declaration.JE_MessageType = "IMP";
		entryHeader.MovementReferenceNumberSetter("23ITMRN0001");

		using (TemporarilySetIsUCC6Configuration(true))
		{
			CombineAssertions(() =>
			{
				foreach (var (entryStatus, status, expectedResult) in GetIsAmendableTestDataEntries())
				{
					AssertIsAmendableStatus(entryHeader, entryStatus, status, expectedResult);
				}
			});
		}
	}

	public void TestIsInAmendableStatus_ForExpNonUcc6()
	{
		declaration.JE_MessageType = "EXP";
		entryHeader.MovementReferenceNumberSetter("23ITMRN0001");

		CombineAssertions(() =>
		{
			AssertIsAmendableStatus(entryHeader, ITEntryStatusList.Codes.Registered, "ACO", expectedResult: false);
			AssertIsAmendableStatus(entryHeader, ITEntryStatusList.Codes.Exit, "ACO", expectedResult: false);
		});
	}

	public void TestIsInAmendableStatus_ForI2Declaration()
	{
		entryHeader.MovementReferenceNumberSetter("23ITMRN0001");

		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_Style = "I2";
		entryHeader.CH_CEI_Instruction = entryInstruction.PK;
		entryHeader.CH_EntryStatus = ITEntryStatusList.Codes.ImportCleared;

		AssertEquals("When Declaration Type is I2, IsInAmendableStatus", false, entryHeader.IsInAmendableStatus);
	}

	public void TestIsInAmendableStatus_WhenEntryHasNotDeclaration()
	{
		var orphanEntry = Factory.New<CusEntryHeader>();
		orphanEntry.MovementReferenceNumberSetter("23ITMRN0001");
		AssertIsAmendableStatus(orphanEntry, ITEntryStatusList.Codes.Registered, "ACO", expectedResult: false, "When Entry has not parent Declaration");
	}

	public void TestIsInAmendingStatus()
	{
		CombineAssertions(() =>
		{
			AssertEquals("By default, EntryHeader is not in amendable status", false, entryHeader.IsInAmendingStatus);

			entryHeader.CH_EntryStatus = "AMG";
			AssertEquals("When CH_EntryStatus is AMG, IsInAmendingStatus is true", true, entryHeader.IsInAmendingStatus);

			entryHeader.CH_EntryStatus = "AWO";
			AssertEquals("When CH_EntryStatus is AWO, IsInAmendingStatus is false", false, entryHeader.IsInAmendingStatus);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();

		declaration = Factory.New<JobDeclaration>();
		entryHeader = declaration.CustomsEntryHeaders.AddNew();
	}

	JobDeclaration declaration;
	CusEntryHeader entryHeader;

	IDisposable TemporarilySetIsUCC6Configuration(bool isUcc6)
	{
		return ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, isUcc6);
	}

	void AssertIsAmendableStatus(CusEntryHeader entryHeader, string entryStatus, string status, bool expectedResult, string assertionMessage = null)
	{
		entryHeader.CH_EntryStatus = entryStatus;
		entryHeader.CH_Status = status;

		var assertionMessageSuffix = $"When CH_EntryStatus: {entryStatus} and CH_Status: {status}, IsInAmendableStatus";
		assertionMessage = string.IsNullOrEmpty(assertionMessage)
			? assertionMessageSuffix
			: $"{assertionMessage}, {assertionMessageSuffix}";

		AssertEquals(assertionMessage, expectedResult, entryHeader.IsInAmendableStatus);
	}

	IEnumerable<(ZString entryStatus, ZString status, bool expectedResult)> GetIsAmendableTestDataEntries()
	{
		yield return ("", "", false);
		yield return (ITEntryStatusList.Codes.Registered, "ACO", true);
		yield return (ITEntryStatusList.Codes.Amending, "", false);
		yield return (ITEntryStatusList.Codes.Amended, "", true);
		yield return (ITEntryStatusList.Codes.Canceled, "", false);
		yield return (ITEntryStatusList.Codes.Canceling, "FFT", true);
		yield return (ITEntryStatusList.Codes.Canceling, "ACO", false);
		yield return (ITEntryStatusList.Codes.Canceling, "ERO", true);
		yield return (ITEntryStatusList.Codes.Canceling, "AWO", false);
	}
}
