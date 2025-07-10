using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Registry;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using static Enterprise.Core.Constants.Customs.Universal;

namespace Enterprise.Customs.GB.Business.Testing
{
	class GbExtensionHelpersTest : TestCaseWithFactory
	{
		public void TestInvalidZDateTimeWhenGetCusEntryHeader()
		{
			var ediMessage = Factory.NewWithValidTestData<EDIMessage>();

			var cusEntryHeader1 = GbExtensionHelpers.GetCusEntryHeaderFromCusEntryNumberAndDate("123", ediMessage.Factory, new ZDateTime(" "));
			var cusEntryHeader2 = GbExtensionHelpers.GetCusEntryHeaderFromCusEntryNumberAndDate("123", ediMessage.Factory, ZDateTime.Empty);
			AssertEquals("Should not report error.", 0, ErrorReporter.TotalErrorCount);
		}

		[TestDate(2021, 1, 1, 12, 0, 0)]
		public void TestUpdateEntryHeaderToCleared()
		{
			aaaBranch = Factory.New<GlbBranch>();
			aaaBranch.GB_Code = "AAA";
			aaaBranch.GB_GC = GlbCompany.CurrentCompany.PK;
			Factory.Save();

			bbbBranch = Factory.New<GlbBranch>();
			bbbBranch.GB_Code = "BBB";
			bbbBranch.GB_RL_NKHomePort = "GBLHR";
			bbbBranch.GB_GC = GlbCompany.CurrentCompany.PK;
			Factory.Save();

			RunTestForUpdateEntryHeaderToCleared(true, false);
			RunTestForUpdateEntryHeaderToCleared(false, true);
			RunTestForUpdateEntryHeaderToCleared(true, true);
		}

		void RunTestForUpdateEntryHeaderToCleared(bool dev, bool des)
		{
			CusEntryHeader entryHeader;

			using (GBCustomsDataRegistry.Instance.SendDevQueryForNonInventoryImportsUponAcceptance.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, dev))
			using (GBCustomsDataRegistry.Instance.SendDesQueryForNonInventoryImportsUponAcceptance.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, des))
			using (GBCustomsDataRegistry.Instance.CcsukLocalHostMnemonic.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "Host"))
			using (GBCustomsDataRegistry.Instance.BadgeCodes.SetTemporaryValue(Guid.Empty, bbbBranch.PK.ToGuid(), Guid.Empty, new BadgeCodeSettingCollection
			{
				new BadgeCodeSetting { BadgeCode = "CH1", RL_PortCode = "GBLHR", ApplicationCode = "", Direction = "IMP", CSPCode = "CCSUK" }
			}))
			{
				using (DisposableEnvironment.ForBranch(bbbBranch.PK.ToGuid()))
				{
					var declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = MessageTypeList.Codes.Import;
					declaration.JE_CustomsProfile = "CH1";

					entryHeader = declaration.CustomsEntryHeaders.AddNew();
					entryHeader.CH_EntryStatus = "";
					Factory.Save();
				}

				GbExtensionHelpers.UpdateEntryHeaderToCleared(entryHeader, ZDateTime.Now);
				AssertEquals(EntryStatusList.Codes.Clear, entryHeader.CH_EntryStatus);
				AssertEquals(MessageStatusList.Codes.OK, entryHeader.CH_Status);
				AssertEquals(ZDateTime.Now, entryHeader.CH_EntryReleaseDate);
				AssertEquals((dev && des) ? 2 : (!dev && !des) ? 0 : 1, entryHeader.Messages.Count);
				var devMessage = entryHeader.Messages.Cast<EDIMessage>().FirstOrDefault(x => x.EM_MessageType == Interrogate_DevDucr.FunctionCodeConst);
				var desMessage = entryHeader.Messages.Cast<EDIMessage>().FirstOrDefault(x => x.EM_MessageType == Interrogate_Des.FunctionCodeConst);
				AssertMessage(Interrogate_DevDucr.FunctionCodeConst, devMessage, dev, bbbBranch.PK);
				AssertMessage(Interrogate_Des.FunctionCodeConst, desMessage, des, bbbBranch.PK);

				using (DisposableEnvironment.ForBranch(bbbBranch.PK.ToGuid()))
				{
					var declaration = Factory.New<JobDeclaration>();
					declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;

					entryHeader = declaration.CustomsEntryHeaders.AddNew();
					entryHeader.CH_EntryStatus = "";
					Factory.Save();
				}

				GbExtensionHelpers.UpdateEntryHeaderToCleared(entryHeader, ZDateTime.Now);
				AssertEquals(EntryStatusList.Codes.Clear, entryHeader.CH_EntryStatus);
				AssertEquals(MessageStatusList.Codes.OK, entryHeader.CH_Status);
				AssertEquals(ZDateTime.Now, entryHeader.CH_EntryReleaseDate);
				AssertEquals(0, entryHeader.Messages.Count);
			}
		}

		void AssertMessage(ZString type, EDIMessage message, bool expectedToBeFound, ZGuid branchPK)
		{
			if (expectedToBeFound)
			{
				AssertNotNull("Message type " + type + " should be present", message);
				AssertEquals($"Message created at clearance should be a {type} message at DUCR level - EM_MessageSubType=D", Interrogate_DevDucr.LevelsOfMessage.DeclarationLevel, message.EM_MessageSubType);
				AssertContains($"Message created at clearance should be a {type} message - check EM_MessageText", $"BGM+{type}::109++13'", message.EM_MessageText);
				AssertEquals("Message created should be assigned the same branch as the job", branchPK, message.EM_GB);
				AssertEquals(ZDateTime.Now, message.EM_HeldUntilDate);
			}
			else
			{
				AssertNull("Message type " + type + " should NOT be present", message);
			}
		}

		public void TestUpdateEntryHeaderToCleared_ForMultipleHeaders()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_ImportClearanceStatusICS = "01";
			var entryHeader2 = declaration.CustomsEntryHeaders.AddNew();

			AssertEquals("CLR", entryHeader.CH_EntryStatus);
			AssertEquals("", entryHeader2.CH_EntryStatus);
		}

		public void TestIsAnyDefenceMeasureApplicable()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(CountryCodes.UnitedKingdom, Customs.Business.UniversalReferenceConstants.CusTariffTypes.ImportTariff);
			Factory.Save();

			var expectedResults = new[]
			{
				new { ConditionType = "490", IsApplicable = false },
				new { ConditionType = "551", IsApplicable = true },
				new { ConditionType = "552", IsApplicable = true },
				new { ConditionType = "553", IsApplicable = true },
				new { ConditionType = "554", IsApplicable = true },
				new { ConditionType = "652", IsApplicable = true },
				new { ConditionType = "654", IsApplicable = true },
				new { ConditionType = "695", IsApplicable = true },
				new { ConditionType = "696", IsApplicable = true }
			};
			var tariffCode = 10000000;
			foreach (var expected in expectedResults)
			{
				var tariff = helper.CreateTariff(CountryCodes.UnitedKingdom, tariffType.PK, tariffCode++.ToString(), ZDateTime.Today, ZDateTime.Today.AddDays(1));
				var conditionType = helper.CreateOrGetExistingRefCusConditionType(CountryCodes.UnitedKingdom, RefCusConditionTypes.ConditionClass.Rate, expected.ConditionType);
				helper.CreateOrGetExistingRefCusCondition(CountryCodes.UnitedKingdom, conditionType.PK, tariff.PK, "GB Tariff", true, false, ZDateTime.Today, ZDateTime.Today.AddDays(1));
				AssertEquals(expected.IsApplicable, tariff.IsAnyDefenceMeasureApplicable());
			}
		}

		GlbBranch aaaBranch;
		GlbBranch bbbBranch;
	}
}
