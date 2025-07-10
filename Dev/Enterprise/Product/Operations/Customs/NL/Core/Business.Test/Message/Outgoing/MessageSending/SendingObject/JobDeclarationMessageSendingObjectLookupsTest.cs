using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.NL.Business.Declaration;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using static Enterprise.Customs.NL.Business.Common.NLConstants;

namespace Enterprise.Customs.NL.Business.Testing;

sealed class JobDeclarationMessageSendingObjectLookupsTest : BusinessObjectLookupsTestCase
{
	public void TestGetImportMessageTypeList()
	{
		var lookups = new JobDeclarationMessageSendingObjectLookups(testItem);
		var result = lookups.GetImportMessageTypeList();
		AssertType<ImportSendMessageTypes>(result);
		AssertEquals("AMD, CAN, DEC, FBK, PRE, SUP, CRI", result.CodesAsString);
	}

	public void TestGetExportMessageTypeList()
	{
		var lookups = new JobDeclarationMessageSendingObjectLookups(testItem);
		var result = lookups.GetExportMessageTypeList();
		AssertType<ExportSendMessageTypes>(result);
		AssertEquals("DEC", result.CodesAsString);
	}

	public void TestGetExportMessageTypeList_FallbackProcedure()
	{
		testItem.Declaration.JE_MessageStatus = "EXP";
		var lookups = new JobDeclarationMessageSendingObjectLookups(testItem);
		var entryHeader = testItem.Header;
		entryHeader.DMSFallbackIsActive = true;
		AssertEquals("FBK", lookups.GetExportMessageTypeList().CodesAsString);
	}

	public void TestGetExportMessageTypeList_FallbackProcedure_MessageHasBeenSentBeforeFallback()
	{
		var fallbackConfig = new FallbackConfiguration();
		fallbackConfig.Start = new ZDateTime(2024, 10, 25, 10, 38, 00);
		var companyPK = entry.RegistryCompanyPK;
		NLCustomsRegistry.Instance.FallbackConfiguration_DMSExport.SetValue(companyPK, Guid.Empty, Guid.Empty, fallbackConfig);

		var message = Factory.New<NLEDIMessage>();
		message.EM_ReceiveTransmit = "TRX";
		message.EM_SystemCreateTimeUtc = new ZDateTime(2024, 10, 25, 07, 38, 00);
		entry.Messages.Add(message);

		CombineAssertions(() =>
		{
			AssertEquals("MessageHasBeenSentBeforeFallback is true", true, entry.MessageHasBeenSentBeforeFallback);

			testItem.Declaration.JE_MessageStatus = "EXP";
			var lookups = new JobDeclarationMessageSendingObjectLookups(testItem);
			var entryHeader = testItem.Header;
			entryHeader.DMSFallbackIsActive = true;
			AssertEquals("FBK is not avaliable when MessageHasBeenSentBeforeFallback is true", string.Empty, lookups.GetExportMessageTypeList().CodesAsString);
		});
	}

	public void TestGetExportMessageTypeList_AMD()
	{
		testItem.Declaration.JE_MessageStatus = "AMD";
		var validCombinations = new List<(ZString entryHeaderStatus, ZString phaseStatus)>
		{
			(ZString.Empty, CustomsEntryPhaseStatusList.Codes._513),
			(StatusNew.Accepted, CustomsEntryPhaseStatusList.Codes._513),
			(StatusNew.Error, CustomsEntryPhaseStatusList.Codes._513),
			(StatusNew.Invalid, CustomsEntryPhaseStatusList.Codes._513)
		};
		var lookups = new JobDeclarationMessageSendingObjectLookups(testItem);
		var entryHeader = testItem.Header;
		var entryHeaderStatusses = typeof(StatusNew).GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy).Where(fi => fi.IsLiteral && !fi.IsInitOnly).Select(f => f.GetValue(null)).Cast<string>().ToList();
		entryHeaderStatusses.Add(string.Empty);
		foreach (var entryHeaderStatus in entryHeaderStatusses)
		{
			var entryStatuses = typeof(EntryStatusNew).GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy).Where(fi => fi.IsLiteral && !fi.IsInitOnly).Select(f => f.GetValue(null)).Cast<string>().ToList();
			entryStatuses.Add(string.Empty);
			foreach (var entryStatus in entryStatuses)
			{
				var phaseStatusses = new CustomsEntryPhaseStatusList().GetAllCodes().ToList();
				phaseStatusses.Add(string.Empty);
				foreach (var phaseStatus in phaseStatusses)
				{
					entryHeader.CH_Status = entryHeaderStatus;
					entryHeader.CH_EntryStatus = entryStatus;
					entryHeader.CH_PhaseStatus = phaseStatus;
					var shouldContainAMD = validCombinations.Any(c => c.entryHeaderStatus == entryHeaderStatus && c.phaseStatus == phaseStatus);
					AssertEquals($"MessageStatus '{entryHeaderStatus}', EntryStatus '{entryStatus}', PhaseStatus '{phaseStatus}'", shouldContainAMD, lookups.GetExportMessageTypeList().ContainsCode("AMD"));
				}
			}
		}
	}

	public void TestGetExportMessageTypeList_CRE() => CombineAssertions(() =>
	{
		testItem.Declaration.JE_MessageStatus = "EXP";
		var lookups = new JobDeclarationMessageSendingObjectLookups(testItem);
		var entryHeader = testItem.Header;
		var combinations = new List<(string entryHeaderStatus, string phaseStatus, bool shouldContainCRE)>
		{
			("", "CRE", true),
			("ERR", "CRE", true),
			("INV", "CRE", true),
			("REM", "CRE", false),
			("INV", "515", false),
		};
		foreach (var combination in combinations)
		{
			entryHeader.CH_Status = combination.entryHeaderStatus;
			entryHeader.CH_PhaseStatus = combination.phaseStatus;
			AssertEquals($"MessageStatus '{combination.entryHeaderStatus}', PhaseStatus '{combination.phaseStatus}'", combination.shouldContainCRE, lookups.GetExportMessageTypeList().ContainsCode("CRE"));
		}
	});

	public void TestGetExportMessageTypeList_SUP() => CombineAssertions(() =>
	{
		testItem.Declaration.JE_MessageStatus = "EXP";
		var lookups = new JobDeclarationMessageSendingObjectLookups(testItem);
		var entryHeader = testItem.Header;
		var combinations = new List<(string entryInstructionSubStyle, string entryHeaderStatus, string phaseStatus, bool shouldContainSUP)>
		{
			("X", "", "SUP", true),
			("X", "ERR", "SUP", true),
			("X", "INV", "SUP", true),
			("Y", "", "SUP", true),
			("Y", "ERR", "SUP", true),
			("Y", "INV", "SUP", true),
			("Z", "", "SUP", false),
			("X", "REM", "SUP", false),
			("Y", "INV", "515", false),
		};
		foreach (var combination in combinations)
		{
			entryHeader.EntryInstruction.CEI_SubStyle = combination.entryInstructionSubStyle;
			entryHeader.CH_Status = combination.entryHeaderStatus;
			entryHeader.CH_PhaseStatus = combination.phaseStatus;
			AssertEquals($"SubStyle '{combination.entryInstructionSubStyle}', MessageStatus '{combination.entryHeaderStatus}', PhaseStatus '{combination.phaseStatus}'", combination.shouldContainSUP, lookups.GetExportMessageTypeList().ContainsCode("SUP"));
		}
	});

	public void TestGetExportMessageTypeList_CAN() => CombineAssertions(() =>
	{
		testItem.Declaration.JE_MessageStatus = "EXP";
		var validCombinations = new List<(string entryHeaderStatus, string entryStatus, string phaseStatus)>
		{
			(StatusNew.Accepted, EntryStatusNew.MRNAllocated, CustomsEntryPhaseStatusList.Codes._515),
			(StatusNew.Accepted, EntryStatusNew.PreLodged, CustomsEntryPhaseStatusList.Codes.REG),
			(StatusNew.SentToCustoms, EntryStatusNew.PreLodged, CustomsEntryPhaseStatusList.Codes._511),
			(StatusNew.Error, EntryStatusNew.PreLodged, CustomsEntryPhaseStatusList.Codes._511),
			(StatusNew.Invalid, EntryStatusNew.PreLodged, CustomsEntryPhaseStatusList.Codes._511),
			(StatusNew.Accepted, EntryStatusNew.RequestForInformation, CustomsEntryPhaseStatusList.Codes._515),
			(StatusNew.ReminderReceived, EntryStatusNew.RequestForInformation, CustomsEntryPhaseStatusList.Codes._515),
			(StatusNew.Accepted, EntryStatusNew.ProvisionalRelease, CustomsEntryPhaseStatusList.Codes._515),
			(StatusNew.Accepted, EntryStatusNew.Released, CustomsEntryPhaseStatusList.Codes._515),
			(StatusNew.ReminderReceived, EntryStatusNew.ProvisionalRelease, CustomsEntryPhaseStatusList.Codes._515),
			(StatusNew.Accepted, EntryStatusNew.Amended, CustomsEntryPhaseStatusList.Codes._515),
			(StatusNew.ReminderReceived, EntryStatusNew.Released, CustomsEntryPhaseStatusList.Codes._515),
			(StatusNew.Error, null, CustomsEntryPhaseStatusList.Codes._514),
			(StatusNew.Invalid, null, CustomsEntryPhaseStatusList.Codes._514),
			(StatusNew.Accepted, null, CustomsEntryPhaseStatusList.Codes._514),
			(StatusNew.Accepted, EntryStatusNew.Received, CustomsEntryPhaseStatusList.Codes.CRE),
			(StatusNew.Accepted, EntryStatusNew.Amended, CustomsEntryPhaseStatusList.Codes._515),
			(StatusNew.Accepted, EntryStatusNew.Received, CustomsEntryPhaseStatusList.Codes.SUP),
			(StatusNew.Accepted, EntryStatusNew.Received, CustomsEntryPhaseStatusList.Codes._583),
			(StatusNew.Accepted, null, CustomsEntryPhaseStatusList.Codes._513),
		};
		var lookups = new JobDeclarationMessageSendingObjectLookups(testItem);
		var entryHeader = testItem.Header;
		var entryHeaderStatusses = typeof(StatusNew).GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy).Where(fi => fi.IsLiteral && !fi.IsInitOnly).Select(f => f.GetValue(null)).Cast<string>().ToList();
		entryHeaderStatusses.Add(string.Empty);
		foreach (var entryHeaderStatus in entryHeaderStatusses)
		{
			var entryStatusses = typeof(EntryStatusNew).GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy).Where(fi => fi.IsLiteral && !fi.IsInitOnly).Select(f => f.GetValue(null)).Cast<string>().ToList();
			entryStatusses.Add(string.Empty);
			foreach (var entryStatus in entryStatusses)
			{
				var phaseStatusses = new CustomsEntryPhaseStatusList().GetAllCodes().ToList();
				phaseStatusses.Add(string.Empty);
				foreach (var phaseStatus in phaseStatusses)
				{
					entryHeader.CH_Status = entryHeaderStatus;
					entryHeader.CH_EntryStatus = entryStatus;
					entryHeader.CH_PhaseStatus = phaseStatus;
					var shouldContainCAN = validCombinations.Any(c => c.entryHeaderStatus == entryHeaderStatus && (c.entryStatus == null || c.entryStatus == entryStatus) && c.phaseStatus == phaseStatus);
					AssertEquals($"MessageStatus '{entryHeaderStatus}', EntryStatus '{entryStatus}', PhaseStatus '{phaseStatus}'", shouldContainCAN, lookups.GetExportMessageTypeList().ContainsCode("CAN"));
				}
			}
		}
	});

	public void TestGetExportMessageTypeList_EXT() => CombineAssertions(() =>
	{
		testItem.Declaration.JE_MessageStatus = "EXP";
		var validCombinations = new List<(ZString entryHeaderStatus, ZString entryStatus, ZString phaseStatus)>
		{
			(StatusNew.Accepted, EntryStatusNew.ProvisionalRelease, CustomsEntryPhaseStatusList.Codes._515),
			(StatusNew.Accepted, EntryStatusNew.Released, CustomsEntryPhaseStatusList.Codes._515),
			(StatusNew.ReminderReceived, EntryStatusNew.ProvisionalRelease, CustomsEntryPhaseStatusList.Codes._515),
			(StatusNew.Accepted, EntryStatusNew.RequestForInformation, CustomsEntryPhaseStatusList.Codes._515),
			(StatusNew.ReminderReceived, EntryStatusNew.RequestForInformation, CustomsEntryPhaseStatusList.Codes._515),
			(StatusNew.Accepted, EntryStatusNew.Amended, CustomsEntryPhaseStatusList.Codes._515),
			(StatusNew.ReminderReceived, EntryStatusNew.Released, CustomsEntryPhaseStatusList.Codes._515),
			(StatusNew.Error, null, CustomsEntryPhaseStatusList.Codes._583),
			(StatusNew.Accepted, null, CustomsEntryPhaseStatusList.Codes._513),
			(StatusNew.Invalid, null, CustomsEntryPhaseStatusList.Codes._583),
			(StatusNew.Accepted, null, CustomsEntryPhaseStatusList.Codes._514),
			(StatusNew.Accepted, EntryStatusNew.Received, CustomsEntryPhaseStatusList.Codes.CRE),
			(StatusNew.Accepted, EntryStatusNew.Received, CustomsEntryPhaseStatusList.Codes.SUP),
		};
		var lookups = new JobDeclarationMessageSendingObjectLookups(testItem);
		var entryHeader = testItem.Header;
		var entryHeaderStatusses = typeof(StatusNew).GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy).Where(fi => fi.IsLiteral && !fi.IsInitOnly).Select(f => f.GetValue(null)).Cast<string>().ToList();
		entryHeaderStatusses.Add(string.Empty);
		foreach (var entryHeaderStatus in entryHeaderStatusses)
		{
			var entryStatusses = typeof(EntryStatusNew).GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy).Where(fi => fi.IsLiteral && !fi.IsInitOnly).Select(f => f.GetValue(null)).Cast<string>().ToList();
			entryStatusses.Add(string.Empty);
			foreach (var entryStatus in entryStatusses)
			{
				var phaseStatusses = new CustomsEntryPhaseStatusList().GetAllCodes().ToList();
				phaseStatusses.Add(string.Empty);
				foreach (var phaseStatus in phaseStatusses)
				{
					entryHeader.CH_Status = entryHeaderStatus;
					entryHeader.CH_EntryStatus = entryStatus;
					entryHeader.CH_PhaseStatus = phaseStatus;
					var shouldContainEXT = validCombinations.Any(c => c.entryHeaderStatus == entryHeaderStatus && (c.entryStatus == ZString.Empty || c.entryStatus == entryStatus) && c.phaseStatus == phaseStatus);
					AssertEquals($"MessageStatus '{entryHeaderStatus}', EntryStatus '{entryStatus}', PhaseStatus '{phaseStatus}'", shouldContainEXT, lookups.GetExportMessageTypeList().ContainsCode("EXT"));
				}
			}
		}
	});

	public void TestGetExportMessageTypeList_DEC() => CombineAssertions(() =>
	{
		testItem.Declaration.JE_MessageStatus = "EXP";
		var validCombinations = new List<(ZString entryHeaderStatus, ZString entryStatus, ZString phaseStatus)>
		{
			(ZString.Empty, ZString.Empty, ZString.Empty),
			(ZString.Empty, ZString.Empty, CustomsEntryPhaseStatusList.Codes._515),
			(StatusNew.Invalid, EntryStatusNew.DeclarationRejected, CustomsEntryPhaseStatusList.Codes._515),
			(StatusNew.Error, ZString.Empty, CustomsEntryPhaseStatusList.Codes._515),
			(StatusNew.Invalid, ZString.Empty, CustomsEntryPhaseStatusList.Codes._515),
		};
		var lookups = new JobDeclarationMessageSendingObjectLookups(testItem);
		var entryHeader = testItem.Header;
		var entryHeaderStatusses = typeof(StatusNew).GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy).Where(fi => fi.IsLiteral && !fi.IsInitOnly).Select(f => f.GetValue(null)).Cast<string>().ToList();
		entryHeaderStatusses.Add(string.Empty);
		foreach (var entryHeaderStatus in entryHeaderStatusses)
		{
			var entryStatusses = typeof(EntryStatusNew).GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy).Where(fi => fi.IsLiteral && !fi.IsInitOnly).Select(f => f.GetValue(null)).Cast<string>().ToList();
			entryStatusses.Add(string.Empty);
			foreach (var entryStatus in entryStatusses)
			{
				var phaseStatusses = new CustomsEntryPhaseStatusList().GetAllCodes().ToList();
				phaseStatusses.Add(string.Empty);
				foreach (var phaseStatus in phaseStatusses)
				{
					entryHeader.CH_Status = entryHeaderStatus;
					entryHeader.CH_EntryStatus = entryStatus;
					entryHeader.CH_PhaseStatus = phaseStatus;
					var shouldContainDEC = validCombinations.Any(c => c.entryHeaderStatus == entryHeaderStatus && c.entryStatus == entryStatus && c.phaseStatus == phaseStatus);
					AssertEquals($"MessageStatus '{entryHeaderStatus}', EntryStatus '{entryStatus}', PhaseStatus '{phaseStatus}'", shouldContainDEC, lookups.GetExportMessageTypeList().ContainsCode("DEC"));
				}
			}
		}
	});

	public void TestExitCustomsOfficeList()
	{
		var lookups = new JobDeclarationMessageSendingObjectLookups(testItem);

		var tomorrow = ZDateTime.Today.AddDays(1);
		var helper = new UniversalReferenceTestDataHelper(Factory);
		var eun = helper.CreateNewOrGetExistingDataGrouping(EconomicGroupList.Codes.EuropeanUnion, "European Union");
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Netherlands, "The Netherlands", eun);
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Italy, "Italy", eun);
		helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office");
		helper.CreateCusCodeType(RefCusCodeListAttributeTypes.Codes.ROLE, "ROLE");
		helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Netherlands, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "NL001234", "DUTCH OFFICE1", ZDateTime.BrettsBirthday, tomorrow, RefCusCodeListAttributeTypes.Codes.ROLE, EuOfficeCodesTypes.Codes.CompetentAuthorityOfEnquiry);
		helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Netherlands, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "NL005678", "DUTCH OFFICE2", ZDateTime.BrettsBirthday, tomorrow, RefCusCodeListAttributeTypes.Codes.ROLE, EuOfficeCodesTypes.Codes.OfficeOfExitInland);
		helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Netherlands, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "NL004321", "DUTCH OFFICE3", ZDateTime.BrettsBirthday, tomorrow, RefCusCodeListAttributeTypes.Codes.ROLE, EuOfficeCodesTypes.Codes.OfficeOfExit);

		helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Italy, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "IT004323", "Italy OFFICE1", ZDateTime.BrettsBirthday, tomorrow, RefCusCodeListAttributeTypes.Codes.ROLE, EuOfficeCodesTypes.Codes.CompetentAuthorityOfEnquiry);
		helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Italy, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "IT004324", "Italy OFFICE2", ZDateTime.BrettsBirthday, tomorrow, RefCusCodeListAttributeTypes.Codes.ROLE, EuOfficeCodesTypes.Codes.OfficeOfExitInland);
		helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Italy, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "IT004325", "Italy OFFICE3", ZDateTime.BrettsBirthday, tomorrow, RefCusCodeListAttributeTypes.Codes.ROLE, EuOfficeCodesTypes.Codes.OfficeOfExit);
		Factory.Save();

		var coll = lookups.ExitCustomsOfficeList;
		coll.Load();
		AssertType<CustomsOfficeCodeCollection>(lookups.ExitCustomsOfficeList);
		AssertContainsExactElementsInAnyOrder(new[] { "NL004321", "NL005678" }, coll.Select(x => x.ZZD_Code));
	}

	public void TestExitTypeList()
	{
		var lookups = new JobDeclarationMessageSendingObjectLookups(testItem);
		var exitTypeList = lookups.ExitTypeList;
		AssertContains("1, 2, 3, 4", exitTypeList.CodesAsString);
	}

	public void TestSecurityList()
	{
		var lookups = new JobDeclarationMessageSendingObjectLookups(testItem);
		var securityTypeList = lookups.SecurityTypeList;
		AssertType<ExportSecurityTypeList>(lookups.SecurityTypeList);
		AssertContains("0, 2", securityTypeList.CodesAsString);
	}

	public void TestGetExportMessageTypeList_PRE() => CombineAssertions(() =>
	{
		testItem.Declaration.JE_MessageStatus = "EXP";
		var lookups = new JobDeclarationMessageSendingObjectLookups(testItem);
		var entryHeader = testItem.Header;

		var combinations = new List<(string entryInstructionSubStyle, string messageStatus, string entryHeaderStatus, string phaseStatus, bool shouldContainsPRE)>
		{
			(EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeA,StatusNew.Accepted, EntryStatusNew.PreLodged, CustomsEntryPhaseStatusList.Codes.REG, true),
			(EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeB, StatusNew.Accepted, EntryStatusNew.PhysicalInspection, CustomsEntryPhaseStatusList.Codes.REG, true),
			(EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeC, StatusNew.Accepted, EntryStatusNew.DocumentsControl, CustomsEntryPhaseStatusList.Codes.REG, true),
			(EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeA, StatusNew.Error, EntryStatusNew.PreLodged, CustomsEntryPhaseStatusList.Codes._511, true),
			(EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeA , StatusNew.Invalid , EntryStatusNew.PreLodged , CustomsEntryPhaseStatusList.Codes._511 , true),
			(EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeB, StatusNew.Accepted, ZString.Empty, CustomsEntryPhaseStatusList.Codes._513, true),
			(EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeC , StatusNew.Accepted , ZString.Empty , CustomsEntryPhaseStatusList.Codes._514 , true),
			(EntrySubStyleList.Codes.NormalDeclaration , StatusNew.Accepted , ZString.Empty , CustomsEntryPhaseStatusList.Codes._514 , false),
			(EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeA , StatusNew.Invalid , ZString.Empty , CustomsEntryPhaseStatusList.Codes.REG , false),
			(EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeA , StatusNew.Accepted , ZString.Empty , CustomsEntryPhaseStatusList.Codes._511 , false)
		};

		foreach (var combination in combinations)
		{
			entryHeader.EntryInstruction.CEI_SubStyle = combination.entryInstructionSubStyle;
			entryHeader.CH_Status = combination.messageStatus;
			entryHeader.CH_PhaseStatus = combination.phaseStatus;
			entryHeader.CH_EntryStatus = combination.entryHeaderStatus;

			AssertEquals($"SubStyle '{combination.entryInstructionSubStyle}', MessageStatus '{combination.messageStatus}', EntryStatus '{combination.entryHeaderStatus}', PhaseStatus '{combination.phaseStatus}'", combination.shouldContainsPRE, lookups.GetExportMessageTypeList().ContainsCode(ExportSendMessageTypes.Codes.PRE));
		}
	});

	#region Setup
	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
		instruction = (Declaration.CusEntryInstruction)declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
		instruction.CEI_JE = declaration.PK;
		entry = declaration.CustomsEntryHeaders.AddNew();
		entry.CH_CEI_Instruction = instruction.PK;
		messageSendingObjectParent = new JobDeclarationMessageSendingObjectParent(declaration);
		testItem = messageSendingObjectParent.SendingObjectsCollection[0];
	}
	JobDeclaration declaration;
	Declaration.CusEntryInstruction instruction;
	Declaration.CusEntryHeader entry;
	JobDeclarationMessageSendingObjectParent messageSendingObjectParent;
	JobDeclarationMessageSendingObject testItem;
	#endregion
}
