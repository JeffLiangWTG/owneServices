using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.IT.Business.Testing;
using Enterprise.MasterFiles.Business;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

[TestedType(typeof(CusEntryInstructionCollection))]
sealed class CusEntryInstructionCollectionTest : BusinessObjectCollectionTestCase
{
	public void TestSetDefaultsForNewChildDefaultZG_UseDeclarationOfIntent()
	{
		var organisationWithDOI = Factory.New<OrgHeader>();
		var doiAuthorisation = CusAuthorisationHeaderTestHelper.CreateAuthorisationHeader(Factory, type: CusAuthorizationHeaderTypeList.Codes.DeclarationOfIntent, permitHolder: organisationWithDOI.PK, "1111111", startDate: ZDate.Today.AddDays(-10), endDate: ZDate.Today.AddDays(10));
		var declaration = Factory.New<JobDeclaration>();

		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
		declaration.JE_OH_Importer = organisationWithDOI.PK;

		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		Assert("IMP and no 'USE' rule", !entryInstruction.ZG_UseDeclarationOfIntent);
		declaration.CustomsEntryInstructions.RemoveAndDelete(entryInstruction);

		var authorisationRule = doiAuthorisation.CusAuthorisationRules.AddNew();
		authorisationRule.CPR_RuleCode = ITCusAuthorisationRuleTypeList.Codes.Use;
		authorisationRule.CPR_ValueFrom = CusAuthorisationRuleUseValueList.Codes.Never;
		entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		Assert("IMP and 'USE - Never'", !entryInstruction.ZG_UseDeclarationOfIntent);
		declaration.CustomsEntryInstructions.RemoveAndDelete(entryInstruction);

		authorisationRule.CPR_ValueFrom = CusAuthorisationRuleUseValueList.Codes.Always;
		entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		Assert("IMP and 'USE - Always' rule", entryInstruction.ZG_UseDeclarationOfIntent);
		declaration.CustomsEntryInstructions.RemoveAndDelete(entryInstruction);

		declaration.JE_MessageType = "XXX";
		entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		Assert("XXX and 'USE - Always' rule'", !entryInstruction.ZG_UseDeclarationOfIntent);
		declaration.CustomsEntryInstructions.RemoveAndDelete(entryInstruction);
	}

	public void TestSetDefaultsForNewChildDefaultZG_ParticipantType()
	{
		var declaration = Factory.New<JobDeclaration>();

		CombineAssertions("JE_MessageType = EXP", () =>
		{
			declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Export;
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			AssertEquals("STD defaulted", ParticipantTypeList.Codes.StandardOneSupplierOneImporter, entryInstruction.ZG_ParticipantType);
		});

		CombineAssertions("JE_MessageType NOT IN (EXP)", () =>
		{
			declaration.JE_MessageType = "XXX";
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			AssertEquals("No default", ZString.Empty, entryInstruction.ZG_ParticipantType);
		});
	}

	public void TestResetOrDefaultParticipantType()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = ZString.Empty;
		var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
		var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();

		CombineAssertions("PRE-CONDITION", () =>
		{
			AssertEquals($"{nameof(entryInstruction1)}", ZString.Empty, entryInstruction1.ZG_ParticipantType);
			AssertEquals($"{nameof(entryInstruction2)}", ZString.Empty, entryInstruction2.ZG_ParticipantType);
		});

		CombineAssertions("Default for JE_MessageType = EXP", () =>
		{
			declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Export;
			UndoResetOrDefaultFromSettingMessageType(ZString.Empty);
			declaration.CustomsEntryInstructions.ResetOrDefaultParticipantType();
			AssertEquals($"{nameof(entryInstruction1)}", ParticipantTypeList.Codes.StandardOneSupplierOneImporter, entryInstruction1.ZG_ParticipantType);
			AssertEquals($"{nameof(entryInstruction2)}", ParticipantTypeList.Codes.StandardOneSupplierOneImporter, entryInstruction2.ZG_ParticipantType);
		});

		CombineAssertions("Reset for JE_MessageType NOT IN EXP", () =>
		{
			declaration.JE_MessageType = "ZZZ";
			UndoResetOrDefaultFromSettingMessageType(ParticipantTypeList.Codes.StandardOneSupplierOneImporter);
			declaration.CustomsEntryInstructions.ResetOrDefaultParticipantType();
			AssertEquals($"{nameof(entryInstruction1)}", ZString.Empty, entryInstruction1.ZG_ParticipantType);
			AssertEquals($"{nameof(entryInstruction2)}", ZString.Empty, entryInstruction2.ZG_ParticipantType);
		});

		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
		{
			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
			UndoResetOrDefaultFromSettingMessageType(ZString.Empty);
			declaration.CustomsEntryInstructions.ResetOrDefaultParticipantType();

			AssertEquals($"{nameof(entryInstruction1)}", ParticipantTypeList.Codes.StandardOneSupplierOneImporter, entryInstruction1.ZG_ParticipantType);
			AssertEquals($"{nameof(entryInstruction2)}", ParticipantTypeList.Codes.StandardOneSupplierOneImporter, entryInstruction2.ZG_ParticipantType);

			var newEntryInstruction = declaration.CustomsEntryInstructions.AddNew();
			AssertEquals($"{nameof(newEntryInstruction)}", ParticipantTypeList.Codes.StandardOneSupplierOneImporter, newEntryInstruction.ZG_ParticipantType);
		}

		void UndoResetOrDefaultFromSettingMessageType(ZString newValue)
		{
			entryInstruction1.ZG_ParticipantType = newValue;
			entryInstruction2.ZG_ParticipantType = newValue;
		}
	}

	public void TestUnhookEventsIsCalledOnDispose()
	{
		var hookedEvents = true;
		var testDeclaration = Factory.NewWithValidTestData<JobDeclaration>();

		var refresherMock = new Mock<IEntryInstructionDPOAuthorizationRefresher>();
		ObjectFactory.Substitute(refresherMock.Object);

		using (new CusEntryInstructionCollection(testDeclaration))
		{
			refresherMock.Setup(s => s.UnhookEvents()).Callback(() => hookedEvents = false);
			Assert("Events should still be hooked", hookedEvents);
		}

		refresherMock.Verify(r => r.UnhookEvents());
		Assert("Events should be unhooked", !hookedEvents);
	}

	protected override BusinessObjectCollection GetCollectionToTest()
	{
		var testDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
		return new CusEntryInstructionCollection(testDeclaration);
	}
}
