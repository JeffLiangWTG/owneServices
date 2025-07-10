using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.Universal;
using NUnit.Framework;
using RefDataGroupingCodes = Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes;

namespace Enterprise.Customs.IT.Business.Testing;

[TestedType(typeof(DeclarationConfiguration))]
sealed class DeclarationConfigurationTest : DeclarationConfigurationAbstractTest<DeclarationConfiguration, InstructionConfiguration, InvoiceHeaderConfiguration, InvoiceLineConfiguration, EU.Business.EntryHeaderConfiguration, EntryLineConfiguration>
{
	public void TestUseEucdmSupportingDocumentGoodsShipment()
	{
		AssertEquals(false, configuration.UseEucdmSupportingDocumentGoodsShipment);
	}

	public override void TestUCCAdditionalInfosSupport()
	{
		CombineAssertions(() =>
		{
			declaration.JE_MessageType = "IMP";
			AssertEquals(false, configuration.UCCAdditionalInfosSupport(declaration));
			declaration.JE_MessageType = "EXP";
			AssertEquals(true, configuration.UCCAdditionalInfosSupport(declaration));
		});
	}

	public override void TestMiscAdditionalInfosSupport()
	{
		AssertEquals(false, configuration.MiscAdditionalInfosSupport(declaration));
	}

	public override void TestMiscSupportingDocumentsSupport()
	{
		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
		AssertEquals("When Declaration Type IMP, Do not show MiscSupportingDocuments tab", false, configuration.MiscSupportingDocumentsSupport(declaration));

		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
		AssertEquals("When Declaration Type EXP and MessageVersion is not XML, Show MiscSupportingDocuments tab", true, configuration.MiscSupportingDocumentsSupport(declaration));

		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
		declaration.MessageVersion = MessageVersionList.Codes.XML;
		AssertEquals("When Declaration Type EXP and MessageVersion is XML, Do not show MiscSupportingDocuments tab", false, configuration.MiscSupportingDocumentsSupport(declaration));
	}

	public override void TestMiscPreviousDocumentsSupport()
	{
		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
		AssertEquals("When Declaration Type IMP, Do not show MiscPreviousDocumentsSupport tab", false, configuration.MiscPreviousDocumentsSupport(declaration));

		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
		AssertEquals("When Declaration Type EXP and MessageVersion is not XML, Show MiscPreviousDocumentsSupport tab", true, configuration.MiscPreviousDocumentsSupport(declaration));

		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
		declaration.MessageVersion = MessageVersionList.Codes.XML;
		AssertEquals("When Declaration Type EXP and MessageVersion is XML, Do not show MiscPreviousDocumentsSupport tab", false, configuration.MiscPreviousDocumentsSupport(declaration));
	}

	public override void TestMiscGuaranteesSupport()
	{
		AssertEquals(false, configuration.MiscGuaranteesSupport(declaration));
	}

	public override void TestUseUniversalFeeCalculation()
	{
		AssertEquals(true, configuration.UseUniversalFeeCalculation(declaration));
	}

	public override void TestDV1DetailsSupport()
	{
		AssertEquals(false, configuration.DV1DetailsSupport(declaration));
	}

	public override void TestLockNumberOfEntryLinesForRegisteredEntry()
	{
		AssertEquals(false, configuration.LockNumberOfEntryLinesForRegisteredEntry);
	}

	public override void TestIsUCC5()
	{
		AssertEquals(false, configuration.IsUCC5(Factory.New<DummyBusinessObject>()));
	}

	public override void TestIsUCC6()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Disabled when business object is not JobDeclaration", false, configuration.IsUCC6(Factory.New<DummyBusinessObject>()));
			AssertEquals("Pre-Cond: Message Version", MessageVersionList.Codes.TXT, declaration.MessageVersion);
			AssertIsUCC6(EUJobMessageTypeList.Codes.Export, false);
			AssertIsUCC6(EUJobMessageTypeList.Codes.Import, true);
			AssertIsUCC6(EUJobMessageTypeList.Codes.MiscellaneousCustoms, false);
		});
	}

	public void TestIsUCC6ForEXP_MessageVersionChange()
	{
		declaration.JE_MessageType = "EXP";
		CombineAssertions("For EXP", () =>
		{
			declaration.MessageVersion = "TXT";
			AssertEquals("When MessageVersion=TXT, IsUCC6", false, configuration.IsUCC6(declaration));

			declaration.MessageVersion = "XML";
			AssertEquals("When MessageVersion=TXT, IsUCC6", true, configuration.IsUCC6(declaration));

			declaration.MessageVersion = "ABC";
			AssertEquals("When MessageVersion=Unknown, IsUCC6", false, configuration.IsUCC6(declaration));

			declaration.MessageVersion = "";
			AssertEquals("When MessageVersion is empty, IsUCC6", false, configuration.IsUCC6(declaration));
		});
	}

	public void TestIsTransitionPeriodAES30_MessageVersionChange()
	{
		using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.AESTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, false))
		{
			declaration.JE_MessageType = "EXP";
			declaration.MessageVersion = "XML";
			AssertEquals("When is EXP XML, but Out of Transition Period", false, configuration.IsTransitionPeriodAES30(declaration));

			declaration.JE_MessageType = "EXP";
			declaration.MessageVersion = "TXT";
			AssertEquals("When is EXP TXT, but Out of Transition Period", false, configuration.IsTransitionPeriodAES30(declaration));
		}

		using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.AESTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, true))
		{
			CombineAssertions("When AES Transition Period is valid", () =>
			{
				declaration.JE_MessageType = "EXP";
				declaration.MessageVersion = "XML";
				AssertEquals("When declaration is EXP XML", true, configuration.IsTransitionPeriodAES30(declaration));

				declaration.JE_MessageType = "EXP";
				declaration.MessageVersion = "TXT";
				AssertEquals("When is EXP TXT", false, configuration.IsTransitionPeriodAES30(declaration));

				declaration.JE_MessageType = "IMP";
				AssertEquals("When declaration is not EXP", false, configuration.IsTransitionPeriodAES30(declaration));
			});
		}
	}

	public void TestIsImportMessageVersionUCC6()
	{
		CombineAssertions($"When {nameof(declaration.JE_MessageType)} is Import", () =>
		{
			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
			AssertEquals("When IMUC6 functionality not defined", true, configuration.IsImportMessageVersionUCC6(declaration));

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(
				Constants.FunctionalityTypes.ImportMessageVersionUCC6,
				Core.Constants.CountryCodes.Italy,
				ZDate.Today,
				true))
			{
				AssertEquals(
					"When IMUC6 functionality is defined and is valid, IsImportMessageVersionUCC6",
					true,
					configuration.IsImportMessageVersionUCC6(declaration));
			}

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(
				Constants.FunctionalityTypes.ImportMessageVersionUCC6,
				Core.Constants.CountryCodes.Italy, ZDate.Today.AddDays(-7), true))
			{
				AssertEquals(
					"When IMUC6 functionality is defined but not valid, IsImportMessageVersionUCC6",
					false,
					configuration.IsImportMessageVersionUCC6(declaration));
			}

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(
				Constants.FunctionalityTypes.ImportMessageVersionUCC6,
				Core.Constants.CountryCodes.Italy,
				ZDate.Today.AddDays(7),
				true))
			{
				AssertEquals(
					"When IMUC6 functionality is defined but not valid, IsImportMessageVersionUCC6",
					false,
					configuration.IsImportMessageVersionUCC6(declaration));
			}
		});

		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;

		using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(
			Constants.FunctionalityTypes.ImportMessageVersionUCC6,
			Core.Constants.CountryCodes.Italy,
			ZDate.Today,
			true))
		{
			AssertEquals(
				$"When {nameof(declaration.JE_MessageType)} is Export and IMUC6 functionality is defined and is valid, IsImportMessageVersionUCC6",
				false,
				configuration.IsImportMessageVersionUCC6(declaration));
		}
	}

	public override void TestIsIncoTermOnDeclarationRequiredToBeSameAsIncoTermOnInvoice()
	{
		AssertEquals(false, configuration.IsIncoTermOnDeclarationRequiredToBeSameAsIncoTermOnInvoice);
	}

	public void TestGetValidationDecider()
	{
		CombineAssertions(() =>
		{
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
			{
				declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
				AssertNull("UCC6 EXP", configuration.GetValidationDecider(declaration));
				declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
				AssertType<UCC6ImportDeclarationValidationDecider>("UCC6 IMP", configuration.GetValidationDecider(declaration));
			}

			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
			{
				declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
				AssertNull("Not UCC6 EXP", configuration.GetValidationDecider(declaration));
				declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
				AssertNull("Not UCC6 IMP", configuration.GetValidationDecider(declaration));
			}
		});
	}

	public void TestGetPackageValidationDecider()
	{
		CombineAssertions(() =>
		{
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
			{
				declaration.JE_MessageType = "EXP";
				AssertNull("UCC6 EXP", configuration.GetPackageValidationDecider(declaration));
				declaration.JE_MessageType = "IMP";
				AssertType<UCC6ImportPackageValidationDecider>("UCC6 IMP", configuration.GetPackageValidationDecider(declaration));
			}

			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
			{
				declaration.JE_MessageType = "EXP";
				AssertNull("Not UCC6 EXP", configuration.GetPackageValidationDecider(declaration));
				declaration.JE_MessageType = "IMP";
				AssertNull("Not UCC6 IMP", configuration.GetPackageValidationDecider(declaration));
			}
		});
	}

	void AssertIsUCC6(string decType, bool expectEnabled)
	{
		declaration.JE_MessageType = decType;
		AssertEquals($"Is UCC6 enabled for {decType}?", expectEnabled, configuration.IsUCC6(declaration));
	}

	public override void TestShouldCheckLegalByDeclarantType()
	{
		AssertEquals(false, configuration.ShouldCheckLegalByDeclarantType);
	}

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
		declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
	}

	JobDeclaration declaration;
}
