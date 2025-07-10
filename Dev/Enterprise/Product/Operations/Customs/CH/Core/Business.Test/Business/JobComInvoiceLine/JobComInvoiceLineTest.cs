using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.Business.Testing;
using Enterprise.Customs.Common.CH;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants.Customs.Universal.RefCusConditionTypes;
using static Enterprise.Customs.CH.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(JobComInvoiceLine))]
sealed class JobComInvoiceLineTest : Customs.Business.Testing.BaseJobComInvoiceLineAbstractTest
{
	[ExpectNoExceptions]
	public void TestAllAddInfoColumnsAreInModelView()
	{
		ModelViewTestHelper.AssertAllAddInfoColumnsAreInModelView(Factory.New<JobComInvoiceLine>(), "CHJobComInvoiceLine");
	}

	public void TestCustomsCountryCode()
	{
		AssertEquals("CustomsCountryCodeCore should be CH", Core.Constants.CountryCodes.Switzerland, InvoiceLine.CustomsCountryCode);
	}

	public void TestIInvoiceLinePartDetailsMembers()
	{
		using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
		{
			IInvoiceLinePartDetails partDetails = Factory.New<JobComInvoiceLine>();
			AssertEquals(Core.Constants.CountryCodes.Switzerland, partDetails.CustomsCountryCode);
			AssertEquals(typeof(OrgSupplierPart), partDetails.TypeOfPartUsed);
		}
	}

	public void TestPartType()
	{
		var factory2 = new BusinessObjectFactory();
		var importer = factory2.New<OrgHeader>();
		importer.FillWithValidTestData();
		var product = (MasterFiles.Business.OrgSupplierPart)factory2.New<Integration.Customs.AU.IOrgSupplierPart>();
		product.OP_PartNum = "TestTEST";
		product.RelatedOrganisations.AddOrganisationIfNotExist(importer.PK, OrgPartRelation.RelationshipTypes.Owner);
		factory2.Save();

		var customsTemplate_Company = Factory.New<GlbCompany>();
		customsTemplate_Company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Switzerland;
		var customsTemplate_Branch = customsTemplate_Company.Branches.AddNew();
		customsTemplate_Branch.GB_RL_NKHomePort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, Core.Constants.CountryCodes.Switzerland)).RL_Code;

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_OH_Importer = importer.PK;
		declaration.JE_GB = customsTemplate_Branch.PK;
		var invoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
		invoiceLine.JI_PartNo = "TestTEST";
		AssertType<OrgSupplierPart>("Product type gets changed depending on who is requesting", invoiceLine.Part);
		Factory.Save();

		GlbCompany.CurrentCompany.SetCountry("AU");
		var factory3 = new BusinessObjectFactory();
		var declarationLoaded = factory3.Load<JobDeclaration>(declaration.PK);
		AssertType<OrgSupplierPart>("product type still the type", declarationLoaded.InvoiceLines[0].Part);
	}

	public void TestTypeDecider()
	{
		AssertType<JobComInvoiceLine>("JobComInvoiceLine Type", Factory.New(typeof(BaseJobComInvoiceLine)));
	}

	public void TestJI_CustomsUnitQtyMaxLength()
	{
		AssertEquals(4, InvoiceLine.JI_CustomsUnitQtyInfo.MaxLength);
	}

	public void TestJI_TariffMaxLength()
	{
		AssertEquals(14, InvoiceLine.JI_TariffInfo.MaxLength);
	}

	public void TestJI_FormattedTariffMaxLength()
	{
		AssertEquals(17, InvoiceLine.JI_FormattedTariffInfo.MaxLength);
	}

	public void TestSupportingDocuments()
	{
		AssertType<SupportingDocumentCollection>(InvoiceLine.SupportingDocuments);
	}

	public void TestPreviousDocuments()
	{
		AssertType<PreviousDocumentCollection>(InvoiceLine.PreviousDocuments);
	}

	public override void TestJI_FormattedTariff()
	{
		CombineAssertions(() =>
		{
			InvoiceLine.JI_Tariff = "12345678";
			AssertEquals($"JI_Tariff={InvoiceLine.JI_Tariff}", "1234.5678", InvoiceLine.JI_FormattedTariff);
			InvoiceLine.JI_Tariff = "12345678123";
			AssertEquals($"JI_Tariff={InvoiceLine.JI_Tariff}", "1234.5678 123", InvoiceLine.JI_FormattedTariff);

			InvoiceLine.JI_FormattedTariff = "1234.5678";
			AssertEquals($"JI_FormattedTariff={InvoiceLine.JI_FormattedTariff}", "12345678", InvoiceLine.JI_Tariff);
			InvoiceLine.JI_FormattedTariff = "1234.5678 123";
			AssertEquals($"JI_FormattedTariff={InvoiceLine.JI_FormattedTariff}", "12345678123", InvoiceLine.JI_Tariff);

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			InvoiceLine.JI_Tariff = "12345678123456";
			AssertEquals($"JI_Tariff={InvoiceLine.JI_Tariff}", "1234.5678 123 456", InvoiceLine.JI_FormattedTariff);

			InvoiceLine.JI_FormattedTariff = "1234.5678 123 456";
			AssertEquals($"JI_FormattedTariff={InvoiceLine.JI_FormattedTariff}", "12345678123456", InvoiceLine.JI_Tariff);
		});
	}

	public void TestJI_FormattedTariff_ExtendDefault() => CombineAssertions(() =>
	{
		var testHelper = new RefCusTariffTestHelper(Factory);
		var tariff1 = testHelper.CreateImportTariff("00001111000000");
		var tariff2 = testHelper.CreateImportTariff("00002222000000");
		var tariff2WithFavorCode = testHelper.CreateImportTariff("00002222999000");
		var tariff3 = testHelper.CreateImportTariff("00003333000000");
		var tariff3WithControlCode = testHelper.CreateImportTariff("00002222000999");
		var tariff4OnlyWithFavorCode = testHelper.CreateImportTariff("00004444999000");
		var tariff5OnlyWithControlCode = testHelper.CreateImportTariff("00005555000999");
		var tariff1Export1 = testHelper.CreateExportTariff("00001111000");
		var tariff1Export2 = testHelper.CreateExportTariff("00001111000000");

		Declaration.JE_MessageType = CHJobMessageTypeList.Codes.Import;
		AssertTariff("00001111", "0000.1111 000 000");
		AssertTariff("00002222", "0000.2222 000 000");
		AssertTariff("00002222999", "0000.2222 999");
		AssertTariff("00002222999000", "0000.2222 999 000");
		AssertTariff("00003333", "0000.3333 000 000");
		AssertTariff("00003333 000", "0000.3333 000");
		AssertTariff("00003333 000 999", "0000.3333 000 999");
		AssertTariff("00004444", "0000.4444");
		AssertTariff("00005555", "0000.5555");
		AssertTariff("000011110", "0000.1111 0");
		AssertTariff("0000111", "0000.111");
		AssertTariff("0000.1111", "0000.1111 000 000");

		Declaration.JE_MessageType = CHJobMessageTypeList.Codes.Export;
		AssertTariff("00001111", "0000.1111");

		void AssertTariff(string enteredTariff, string expectedFormattedTariff, [CallerLineNumber] int line = 0)
		{
			InvoiceLine.JI_FormattedTariff = enteredTariff;
			var assertionMessage = $"[{line}] enteredTariff={enteredTariff}";
			AssertEquals($"{assertionMessage} JI_FormattedTariff", expectedFormattedTariff, InvoiceLine.JI_FormattedTariff);
			AssertEquals($"{assertionMessage} JI_Tariff", expectedFormattedTariff.KeepNumericCharacters(), InvoiceLine.JI_Tariff);
		}
	});

	public void TestCustomsFavourCode()
	{
		CombineAssertions(() =>
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			InvoiceLine.JI_Tariff = "85078000042911";
			AssertEquals($"JI_Tariff={InvoiceLine.JI_Tariff}", "42", InvoiceLine.CustomsFavourCode);
			InvoiceLine.JI_Tariff = "85078000000911";
			AssertEquals($"JI_Tariff={InvoiceLine.JI_Tariff}", ZString.Empty, InvoiceLine.CustomsFavourCode);

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;

			InvoiceLine.JI_Tariff = "85078000911";
			AssertEquals($"JI_Tariff={InvoiceLine.JI_Tariff}", ZString.Empty, InvoiceLine.CustomsFavourCode);
		});
	}

	public void TestStatisticalCode()
	{
		CombineAssertions(() =>
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			InvoiceLine.JI_Tariff = "85078000911042";
			AssertEquals($"JI_Tariff={InvoiceLine.JI_Tariff}", "42", InvoiceLine.StatisticalCode);

			InvoiceLine.JI_Tariff = "85078000911242";
			AssertEquals($"JI_Tariff={InvoiceLine.JI_Tariff}", "242", InvoiceLine.StatisticalCode);

			InvoiceLine.JI_Tariff = "85078000911000";
			AssertEquals($"JI_Tariff={InvoiceLine.JI_Tariff}", ZString.Empty, InvoiceLine.StatisticalCode);

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;

			InvoiceLine.JI_Tariff = "85078000000911";
			AssertEquals($"JI_Tariff={InvoiceLine.JI_Tariff}", ZString.Empty, InvoiceLine.StatisticalCode);
		});
	}

	public void TestTariffWithoutCustomsFavourCode()
	{
		CombineAssertions(() =>
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			InvoiceLine.JI_Tariff = "12345678000123";
			AssertEquals($"JI_Tariff={InvoiceLine.JI_Tariff}", "12345678123", InvoiceLine.TariffWithoutCustomsFavourCode);
			InvoiceLine.JI_Tariff = "123";
			AssertEquals($"JI_Tariff={InvoiceLine.JI_Tariff}", "123", InvoiceLine.TariffWithoutCustomsFavourCode);

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;

			InvoiceLine.JI_Tariff = "12345678000123";
			AssertEquals($"JI_Tariff={InvoiceLine.JI_Tariff}", ZString.Empty, InvoiceLine.TariffWithoutCustomsFavourCode);
		});
	}

	public void TestTariffCodeGroup()
	{
		InvoiceLine.JI_Tariff = "1234567890";

		CombineAssertions(() =>
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			AssertEquals($"JI_Tariff={InvoiceLine.JI_Tariff}", "1234", InvoiceLine.TariffCodeGroup);

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals($"JI_Tariff={InvoiceLine.JI_Tariff}", ZString.Empty, InvoiceLine.TariffCodeGroup);
		});
	}

	public void TestTariffNumber()
	{
		InvoiceLine.Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		InvoiceLine.JI_Tariff = "1234567890";
		AssertEquals($"JI_Tariff={InvoiceLine.JI_Tariff}", "12345678", InvoiceLine.TariffNumber);
	}

	public void TestIsStatisticalCodeOther() => CombineAssertions(() =>
	{
		Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		InvoiceLine.JI_Tariff = UniversalReferenceConstants.TariffNumbers.WaterPipeTobaccoSpecifiedInSubheading + "000" + UniversalReferenceConstants.TariffStatisticalCodes.StatisticalCodeOther;
		AssertEquals($"StatisticalCode={InvoiceLine.StatisticalCode}", true, InvoiceLine.IsStatisticalCodeOther);

		InvoiceLine.JI_Tariff = UniversalReferenceConstants.TariffNumbers.WaterPipeTobaccoSpecifiedInSubheading + "000" + UniversalReferenceConstants.TariffStatisticalCodes.TobaccoPrivateGoodsNoMoreOf10kgOr1000CHF;
		AssertEquals($"StatisticalCode={InvoiceLine.StatisticalCode}", false, InvoiceLine.IsStatisticalCodeOther);
	});

	public void TestJI_PrimaryPreference() => CombineAssertions(() =>
	{
		AssertEquals($"{nameof(InvoiceLine.JI_PrimaryPreference)} caption", "Preference Code", DataBoundResourceStrings.GetDataForProperty(InvoiceLine.JI_PrimaryPreferenceInfo).Caption);
		AssertEquals($"{nameof(InvoiceLine.JI_PrimaryPreference)} caption", "Pref. Code", DataBoundResourceStrings.GetDataForProperty(InvoiceLine.JI_PrimaryPreferenceInfo).MediumCaption);
	});

	public void TestJI_Procedure() => CombineAssertions(() =>
	{
		AssertEquals($"{nameof(InvoiceLine.JI_Procedure)} caption", "Procedure", DataBoundResourceStrings.GetDataForProperty(InvoiceLine.JI_ProcedureInfo).Caption);
		AssertEquals($"{nameof(InvoiceLine.JI_Procedure)} short caption", "CPC", DataBoundResourceStrings.GetDataForProperty(InvoiceLine.JI_ProcedureInfo).ShortCaption);
	});

	public void TestTariffDescription_Import() => AssertTariffDescription(JobMessageTypeList.Codes.Import);

	public void TestTariffDescription_Export() => AssertTariffDescription(JobMessageTypeList.Codes.Export);

	void AssertTariffDescription(string messageType) => CombineAssertions(() =>
	{
		var nomenclatureGroupType = "CN";
		var helper = new UniversalReferenceTestDataHelper(Factory);
		var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Switzerland, messageType);
		tariffType.ZZI_ZZ9_NKNomenclatureGroupType = nomenclatureGroupType;
		Factory.Save();

		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Switzerland, "Current Country Description");
		var tariffView = helper.CreateTariff(Core.Constants.CountryCodes.Switzerland, tariffType.PK, "0102030000", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, description: "ZZ1_Description", compositeKey: "01.01..05.1.1.10.10");

		var group1 = helper.CreateNomenclatureGroup(Core.Constants.CountryCodes.Switzerland, "01", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "Description Group 1", compositeKey: "01.01", nomenclatureGroupType: nomenclatureGroupType);
		var group2 = helper.CreateNomenclatureGroup(Core.Constants.CountryCodes.Switzerland, "0102", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "Description Group 2", compositeKey: "01.01..05", nomenclatureGroupType: nomenclatureGroupType);
		var group3 = helper.CreateNomenclatureGroup(Core.Constants.CountryCodes.Switzerland, "010203", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "Description Group 3", compositeKey: "01.01..05.1.1", nomenclatureGroupType: nomenclatureGroupType);

		CreateNomenclatureLanguage(SwissCustomsLanguageList.Codes.German);
		CreateNomenclatureLanguage(SwissCustomsLanguageList.Codes.Italian);
		CreateNomenclatureLanguage("EN");

		Factory.Save();

		Declaration.JE_MessageType = messageType;

		GlbStaff.CurrentUser.GS_WorkingLanguage = SwissCustomsLanguageList.Codes.Italian;
		Declaration.JE_DeclarationLanguage = SwissCustomsLanguageList.Codes.German;
		InvoiceLine.JI_Tariff = "0102030000";
		AssertEquals($"JE_DeclarationLanguage = '{Declaration.JE_DeclarationLanguage}'", "DE - RefCusNomenclatureGroup 1 ZX8_Description DE - RefCusNomenclatureGroup 2 ZX8_Description DE - RefCusNomenclatureGroup 3 ZX8_Description DE - ZX7_Description", InvoiceLine.TariffDescription);
		AssertEquals("If empty, JI_Description is set to TariffDescription", InvoiceLine.TariffDescription.ToUpper(), InvoiceLine.JI_Description);

		Declaration.JE_DeclarationLanguage = ZString.Empty;
		AssertEquals($"GS_WorkingLanguage = '{GlbStaff.CurrentUser.GS_WorkingLanguage}'", "IT - RefCusNomenclatureGroup 1 ZX8_Description IT - RefCusNomenclatureGroup 2 ZX8_Description IT - RefCusNomenclatureGroup 3 ZX8_Description IT - ZX7_Description", InvoiceLine.TariffDescription);
		AssertNotEquals("Once set, JI_Description didn't change", InvoiceLine.TariffDescription.ToUpper(), InvoiceLine.JI_Description);

		Declaration.JE_DeclarationLanguage = "XX";
		AssertEquals("Unknown JE_DeclarationLanguage", "EN - RefCusNomenclatureGroup 1 ZX8_Description EN - RefCusNomenclatureGroup 2 ZX8_Description EN - RefCusNomenclatureGroup 3 ZX8_Description EN - ZX7_Description", InvoiceLine.TariffDescription);

		void CreateNomenclatureLanguage(string languageCode)
		{
			helper.CreateOrGetLanguage(languageCode, $"{languageCode} - ZX6_Description");
			Factory.Save();
			helper.LoadOrCreateNewCusRefTariffLanguageView(Factory, tariffView.PK, languageCode, $"{languageCode} - ZX7_Description");
			helper.CreateNomenclatureGroupLanguage(group1.PK, languageCode, $"{languageCode} - RefCusNomenclatureGroup 1 ZX8_Description");
			helper.CreateNomenclatureGroupLanguage(group2.PK, languageCode, $"{languageCode} - RefCusNomenclatureGroup 2 ZX8_Description");
			helper.CreateNomenclatureGroupLanguage(group3.PK, languageCode, $"{languageCode} - RefCusNomenclatureGroup 3 ZX8_Description");
		}
	});

	public void TestJI_Procedure_ReadOnly() => CombineAssertions(() =>
	{
		Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		Assert(!InvoiceLine.JI_ProcedureInfo.ReadOnly);
		Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		Assert(InvoiceLine.JI_ProcedureInfo.ReadOnly);
	});

	public void TestSpecialMentions() => CombineAssertions(() =>
	{
		SpecialMentionsTestHelper.TestSpecialMentions(InvoiceLine.SpecialMentionsInfo);
	});

	public void TestCountOfSpecialMentionsLines() => CombineAssertions(() =>
	{
		InvoiceLine.SpecialMentions = "Line1\nLine2\n";
		AssertEquals(2, InvoiceLine.CountOfSpecialMentionsLines);
		InvoiceLine.SpecialMentions = "Line1\nLine2\nLine3\n";
		AssertEquals(3, InvoiceLine.CountOfSpecialMentionsLines);
	});

	public void TestUniveralTariffType() => CombineAssertions(() =>
	{
		Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		AssertEquals($"MessageType={Declaration.JE_MessageType}", UniversalReferenceConstants.TariffTypes.ImportTariff, InvoiceLine.UniversalTariffType);

		Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		AssertEquals($"MessageType={Declaration.JE_MessageType}", UniversalReferenceConstants.TariffTypes.ExportTariff, InvoiceLine.UniversalTariffType);
	});

	public void TestNetDuty_Caption()
	{
		AssertEquals("Net Duty", DataBoundResourceStrings.GetDataForProperty(InvoiceLine.NetDutyInfo).Caption);
	}

	public void TestNetDuty_Import_Unticked()
	{
		Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		AssertEquals($"JI_WeightIncludingInnerPackage={InvoiceLine.JI_WeightIncludingInnerPackage} -> Unticked", false, InvoiceLine.NetDuty);
	}

	public void TestNetDuty_Import_Ticked() => CombineAssertions(() =>
	{
		Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		InvoiceLine.JI_WeightIncludingInnerPackageUQ = Core.Constants.Weight.Kilograms;
		InvoiceLine.JI_WeightIncludingInnerPackage = 100;
		Factory.Save();
		var newFactory = new BusinessObjectFactory();
		var invoiceLine = newFactory.Load<JobComInvoiceLine>(InvoiceLine.PK);
		AssertNotNull(invoiceLine);
		AssertEquals($"JI_WeightIncludingInnerPackage={invoiceLine.JI_WeightIncludingInnerPackage} -> Ticked", true, invoiceLine.NetDuty);
	});

	public void TestJI_WeightIncludingInnerPackage() => CombineAssertions(() =>
	{
		AssertEquals("Customs Net Weight", DataBoundResourceStrings.GetDataForProperty(InvoiceLine.JI_WeightIncludingInnerPackageInfo).Caption);
		TypeValidation.CheckValidDecimal(InvoiceLine.JI_WeightIncludingInnerPackageInfo, 9, 3);
	});

	public void TestJI_WeightIncludingInnerPackage_Import_ReadOnly()
	{
		AssertNetDutyReadOnlyStatus(InvoiceLine.JI_WeightIncludingInnerPackageInfo, JobMessageTypeList.Codes.Import, false);
	}

	public void TestJI_WeightIncludingInnerPackageUQ_Import_ReadOnly()
	{
		AssertNetDutyReadOnlyStatus(InvoiceLine.JI_WeightIncludingInnerPackageUQInfo, JobMessageTypeList.Codes.Import, false);
	}

	public void TestJI_WeightIncludingInnerPackageUQ_Import_Default()
	{
		Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		InvoiceLine.NetDuty = true;
		AssertEquals(Core.Constants.Weight.Kilograms, InvoiceLine.JI_WeightIncludingInnerPackageUQ);
	}

	public void TestJI_TareSupplementPercentage_Caption()
	{
		AssertEquals("Tare supplement", DataBoundResourceStrings.GetDataForProperty(InvoiceLine.JI_TareSupplementPercentageInfo).Caption);
	}

	public void TestJI_TareSupplementPercentage_Import_ReadOnly()
	{
		AssertNetDutyReadOnlyStatus(InvoiceLine.JI_TareSupplementPercentageInfo, JobMessageTypeList.Codes.Import, false);
	}

	public void TestJI_TareSupplementPercentage_Import() => CombineAssertions(() =>
	{
		var helper = new RefCusTariffTestHelper(Factory);
		helper.CreateImportTariff("1000");
		helper.CreateImportTariffWithAttribute("1001", TariffAttributes.TareSupplement, "3");
		helper.CreateImportTariffWithAttribute("1002", TariffAttributes.TareSupplement, "5", "8", "6", "X");

		Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		InvoiceLine.NetDuty = true;

		AssertTareSupplement(ZString.Empty, 10, ZBool.True, "No tariff");
		AssertTareSupplement("1000", 10, ZBool.True, "No tareSupplement attribute");
		AssertTareSupplement("1001", 3, ZBool.False, "One tareSupplement attribute");
		AssertTareSupplement("1002", 8, ZBool.True, "More than one tareSupplementAttribute (take highest value)");

		void AssertTareSupplement(ZString tariffCode, ZDecimal expectedTareSupplementPercentage, ZBool expectedTareSupplementConfirmation, string assertionInfo)
		{
			InvoiceLine.JI_Tariff = tariffCode;
			AssertEquals($"JI_TareSupplementPercentage - {assertionInfo}", expectedTareSupplementPercentage, InvoiceLine.JI_TareSupplementPercentage);
			AssertEquals($"JI_TareSupplementConfirmation - {assertionInfo}", expectedTareSupplementConfirmation, InvoiceLine.JI_TareSupplementConfirmation);
		}
	});

	public void TestJI_TareSupplementConfirmation_Caption()
	{
		AssertEquals("Override", DataBoundResourceStrings.GetDataForProperty(InvoiceLine.JI_TareSupplementConfirmationInfo).Caption);
	}

	public void TestJI_TareSupplementConfirmation_Import_ReadOnly()
	{
		AssertNetDutyReadOnlyStatus(InvoiceLine.JI_TareSupplementConfirmationInfo, JobMessageTypeList.Codes.Import, false);
	}

	public void TestJI_TareSupplementConfirmationTicked_Import()
	{
		Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		InvoiceLine.NetDuty = true;
		InvoiceLine.JI_TareSupplementConfirmation = true;
		InvoiceLine.NetDuty = false;
		AssertEquals($"JI_TareSupplementConfirmation: NetDuty={InvoiceLine.NetDuty} -> Unticked", false, InvoiceLine.JI_TareSupplementConfirmation);
	}

	public void TestJI_VATCodeConfirmation()
	{
		AssertEquals($"{nameof(InvoiceLine.JI_VATCodeConfirmation)} caption", "Confirmation", DataBoundResourceStrings.GetDataForProperty(InvoiceLine.JI_VATCodeConfirmationInfo).Caption);
	}

	public void TestJI_VATValueConfirmation()
	{
		AssertEquals($"{nameof(InvoiceLine.JI_VATValueConfirmation)} caption", "VAT Value", DataBoundResourceStrings.GetDataForProperty(InvoiceLine.JI_VATValueConfirmationInfo).Caption);
	}

	public void TestJI_PermitObligation()
	{
		AssertEquals($"{nameof(InvoiceLine.JI_PermitObligation)} caption", "Permit Obligation Code", DataBoundResourceStrings.GetDataForProperty(InvoiceLine.JI_PermitObligationInfo).Caption);
	}

	public void TestJI_NonCustomsLawObligation()
	{
		AssertEquals($"{nameof(InvoiceLine.JI_NonCustomsLawObligation)} caption", "NCL Obligation Code", DataBoundResourceStrings.GetDataForProperty(InvoiceLine.JI_NonCustomsLawObligationInfo).Caption);
	}

	public void TestJI_StorageType()
	{
		AssertEquals($"{nameof(InvoiceLine.JI_StorageType)} caption", "Storage Code", DataBoundResourceStrings.GetDataForProperty(InvoiceLine.JI_StorageTypeInfo).Caption);
	}

	public void TestJI_GrossMassConfirmation()
	{
		AssertEquals($"{nameof(InvoiceLine.JI_GrossMassConfirmation)} caption", "Gross Mass", DataBoundResourceStrings.GetDataForProperty(InvoiceLine.JI_GrossMassConfirmationInfo).Caption);
	}

	public void TestJI_NetMassConfirmation()
	{
		AssertEquals($"{nameof(InvoiceLine.JI_NetMassConfirmation)} caption", "Net Mass", DataBoundResourceStrings.GetDataForProperty(InvoiceLine.JI_NetMassConfirmationInfo).Caption);
	}

	public void TestJI_AdditionalUnitConfirmation()
	{
		AssertEquals($"{nameof(InvoiceLine.JI_AdditionalUnitConfirmation)} caption", "Additional Quantity", DataBoundResourceStrings.GetDataForProperty(InvoiceLine.JI_AdditionalUnitConfirmationInfo).Caption);
	}

	public void TestJI_StatisticalValueConfirmation()
	{
		AssertEquals($"{nameof(InvoiceLine.JI_StatisticalValueConfirmation)} caption", "Statistical Value", DataBoundResourceStrings.GetDataForProperty(InvoiceLine.JI_StatisticalValueConfirmationInfo).Caption);
	}

	public void TestCalculatedGrossMass_Caption()
	{
		AssertEquals("Calculated Gross mass", DataBoundResourceStrings.GetDataForProperty(InvoiceLine.CalculatedGrossMassInfo).Caption);
	}

	public void TestJI_GoodsReturned_Caption()
	{
		AssertEquals($"{nameof(InvoiceLine.JI_GoodsReturned)} caption", "Returned Goods", DataBoundResourceStrings.GetDataForProperty(InvoiceLine.JI_GoodsReturnedInfo).Caption);
	}

	public void TestJI_GoodsReturned_Default()
	{
		AssertEquals($"{nameof(InvoiceLine.JI_GoodsReturned)} Default", false, InvoiceLine.JI_GoodsReturned);
	}

	public void TestCalculatedGrossMass_Import_ReadOnly()
	{
		AssertNetDutyReadOnlyStatus(InvoiceLine.CalculatedGrossMassInfo, JobMessageTypeList.Codes.Import, true);
	}

	public void TestEffectiveAssessmentDate() => CombineAssertions(() =>
	{
		Declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Export;
		AssertEquals("EXP Declaration without Valuation Date: today", ZDateTime.Today, InvoiceLine.EffectiveAssessmentDate);

		Declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
		AssertEquals("IMP Declaration without Valuation Date: today", ZDateTime.Today, InvoiceLine.EffectiveAssessmentDate);

		Declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Export;
		var valuationDate = new ZDate(2024, 12, 31);
		Declaration.JE_ValuationDate = valuationDate;
		AssertEquals("EXP Declaration with Valuation Date: 31.12.2024", valuationDate, InvoiceLine.EffectiveAssessmentDate);

		Declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
		AssertEquals("IMP Declaration with Valuation Date: 31.12.2024", valuationDate, InvoiceLine.EffectiveAssessmentDate);

		var acceptanceDate = new ZDate(2025, 01, 01);
		var entryInstruction = Declaration.CustomsEntryInstructions.AddNew();
		InvoiceLine.JI_CEI = entryInstruction.PK;
		entryInstruction.CEI_DateForDuty = acceptanceDate;
		AssertEquals("IMP Declaration with Acceptance Date: 01.01.2025", acceptanceDate, InvoiceLine.EffectiveAssessmentDate);

		Declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Export;
		AssertEquals("EXP Declaration with Valuation Date: 31.12.2024 / existing IMP Acceptance Date", valuationDate, InvoiceLine.EffectiveAssessmentDate);
	});

	public void TestRateFormulaDescription()
	{
		var helper = new RefCusTariffTestHelper(Factory);
		var tariff = helper.CreateImportTariffWithUOMs(RefCusTariffTestHelper.ImportTariffBeverages, new[] { SwissCustomsConstants.MeasurementUnits.GrossWeightUOM, SwissCustomsConstants.MeasurementUnits.NetWeightUOM });
		helper.AddRate(tariff, PrimaryPreferenceCodes.PreferentialTariff, "2.06 * [KGMG]", endDate: new ZDateTime(2023, 12, 31));
		helper.AddRate(tariff, PrimaryPreferenceCodes.PreferentialTariff, "3.06 * [KGMG]", startDate: new ZDateTime(2024, 01, 01));
		Factory.Save();

		Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		InvoiceLine.JI_CountryOfOrigin = RefCusTariffTestHelper.Country;
		InvoiceLine.JI_PrimaryPreference = UniversalReferenceConstants.PrimaryPreferenceCodes.PreferentialTariff;
		var entryInstruction = Declaration.CustomsEntryInstructions.AddNew();
		InvoiceLine.JI_CEI = entryInstruction.PK;

		CombineAssertions(() =>
		{
			InvoiceLine.JI_Tariff = tariff.ZZ1_TariffCode;
			AssertEquals("Rate Formula Description 01.01.2024", "3.06 CHF per KGMG", InvoiceLine.RateFormulaDescription);

			entryInstruction.CEI_DateForDuty = new ZDate(2023, 12, 31);
			AssertEquals("Rate Formula Description 31.12.2023", "2.06 CHF per KGMG", InvoiceLine.RateFormulaDescription);
		});
	}

	public void TestCalculatedGrossMassCalculation_Import()
	{
		Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		CombineAssertions("CalculatedGrossMass", () =>
		{
			InvoiceLine.JI_WeightIncludingInnerPackageUQ = Core.Constants.Weight.Kilograms;
			AssertEquals("Initial value", 0m, InvoiceLine.CalculatedGrossMass);
			InvoiceLine.JI_WeightIncludingInnerPackage = 10.0;
			InvoiceLine.JI_TareSupplementPercentage = 0;
			AssertEquals($"JI_WeightIncludingInnerPackage={InvoiceLine.JI_WeightIncludingInnerPackage}", 10.0m, InvoiceLine.CalculatedGrossMass);
			InvoiceLine.JI_TareSupplementPercentage = 0.1;
			AssertEquals($"JI_WeightIncludingInnerPackage={InvoiceLine.JI_WeightIncludingInnerPackage} and JI_TareSupplementPercentage={InvoiceLine.JI_TareSupplementPercentage}", 10.1m, InvoiceLine.CalculatedGrossMass);
			InvoiceLine.JI_TareSupplementPercentage = 1.0;
			AssertEquals($"JI_WeightIncludingInnerPackage={InvoiceLine.JI_WeightIncludingInnerPackage} and JI_TareSupplementPercentage={InvoiceLine.JI_TareSupplementPercentage}", 10.1m, InvoiceLine.CalculatedGrossMass);
			InvoiceLine.JI_WeightIncludingInnerPackageUQ = Core.Constants.Weight.Tonnes;
			AssertEquals($"JI_WeightIncludingInnerPackage={InvoiceLine.JI_WeightIncludingInnerPackage} and JI_TareSupplementPercentage={InvoiceLine.JI_TareSupplementPercentage}", 10100.0m, InvoiceLine.CalculatedGrossMass);
			InvoiceLine.JI_WeightIncludingInnerPackageUQ = Core.Constants.Weight.Grams;
			AssertEquals($"JI_WeightIncludingInnerPackage={InvoiceLine.JI_WeightIncludingInnerPackage} and JI_TareSupplementPercentage={InvoiceLine.JI_TareSupplementPercentage}", 0.1m, InvoiceLine.CalculatedGrossMass);
		});
	}

	protected override ZString UniversalTariffTypeForDefaultTaxOrFeeCode => UniversalReferenceConstants.TariffTypes.ImportTariff;

	public void TestTobaccoCollection()
	{
		var line = Factory.New<JobDeclaration>().Invoices.AddNew().InvoiceLines.AddNew();

		CombineAssertions("TobaccoCollection", () =>
		{
			AssertNotNull("JobComInvoiceLine.Tobaccos must not be null", line.Tobaccos);
			AssertType<TobaccoCollection>("Mismatching Type", line.Tobaccos);
			AssertEquals("Wrong count", 0, line.Tobaccos.Count);
			line.Tobaccos.AddNew();
			AssertEquals("Wrong count", 1, line.Tobaccos.Count);
		});
	}

	public void TestNonCustomsLawsCollection() => CombineAssertions(() =>
	{
		var line = Factory.New<JobDeclaration>().Invoices.AddNew().InvoiceLines.AddNew();

		AssertNotNull("JobComInvoiceLine.NonCustomsLaws must not be null", line.NonCustomsLaws);
		AssertType<NonCustomsLawCollection>("Mismatching Type", line.NonCustomsLaws);
		AssertEquals("Wrong count", 0, line.NonCustomsLaws.Count);
		line.NonCustomsLaws.AddNew();
		AssertEquals("Wrong count", 1, line.NonCustomsLaws.Count);
	});

	public void TestRestrictions() => CombineAssertions(() =>
	{
		AssertType<Restriction>(InvoiceLine.Restrictions.AddNew());
		AssertEquals("SequenceNumberGenerator works", 2, InvoiceLine.Restrictions.AddNew().CSI_LineNo);
	});

	public void TestInAndOutwardProcessingCollection()
	{
		CombineAssertions(() =>
		{
			AssertType<InAndOutwardProcessingCollection>(InvoiceLine.InAndOutwardProcessings);
			AssertType<InAndOutwardProcessing>(InvoiceLine.InAndOutwardProcessings.AddNew());
		});
	}

	public void TestInAndOutwardProcessing()
	{
		CombineAssertions(() =>
		{
			AssertNotNull(InvoiceLine.InAndOutwardProcessing);
			AssertEquals(false, InvoiceLine.InAndOutwardProcessing.HasChanges);
			AssertEquals(false, InvoiceLine.InAndOutwardProcessing.IsSavedByFactory);
			AssertEquals(ZString.Empty, InvoiceLine.InAndOutwardProcessingDirection);
			AssertEquals(ZString.Empty, InvoiceLine.InAndOutwardProcessingRefinementType);
			AssertEquals(ZString.Empty, InvoiceLine.InAndOutwardProcessingProcessType);
			AssertEquals(ZString.Empty, InvoiceLine.InAndOutwardProcessingBillingType);
			AssertEquals(ZString.Empty, InvoiceLine.InAndOutwardProcessingRepairReason);
			AssertEquals(false, InvoiceLine.InAndOutwardProcessingRepair);
		});

		var inAndOutwardProcessing = InvoiceLine.InAndOutwardProcessing;
		inAndOutwardProcessing.CSI_SubType = "0";
		inAndOutwardProcessing.CSI_Code = "1";
		inAndOutwardProcessing.CSI_Procedure = "2";
		inAndOutwardProcessing.CSI_IssuerType = "3";
		inAndOutwardProcessing.Repair = true;
		inAndOutwardProcessing.CSI_Description = "This is a Description";

		CombineAssertions(() =>
		{
			AssertEquals(inAndOutwardProcessing.CSI_SubType, InvoiceLine.InAndOutwardProcessingDirection);
			AssertEquals(inAndOutwardProcessing.CSI_Code, InvoiceLine.InAndOutwardProcessingRefinementType);
			AssertEquals(inAndOutwardProcessing.CSI_Procedure, InvoiceLine.InAndOutwardProcessingProcessType);
			AssertEquals(inAndOutwardProcessing.CSI_IssuerType, InvoiceLine.InAndOutwardProcessingBillingType);
			AssertEquals(inAndOutwardProcessing.Repair, InvoiceLine.InAndOutwardProcessingRepair);
			AssertEquals(InAndOutwardProcessingStatusCodes.RepairTrue, InvoiceLine.InAndOutwardProcessing.CSI_Status);
			AssertEquals(inAndOutwardProcessing.CSI_Description, InvoiceLine.InAndOutwardProcessingRepairReason);
		});
	}

	public void TestInAndOutwardProcessing_CacheReloadedAfterSaveIfEmpty()
	{
		CombineAssertions(() =>
		{
			InvoiceLine.InAndOutwardProcessingRepairReason = "1";
			Factory.Save();
			InvoiceLine.InAndOutwardProcessingRepairReason = ZString.Empty;
			Factory.Save();
			InvoiceLine.InAndOutwardProcessingRepairReason = "2";
			Factory.Save();
			var savedInvoiceLine = new BusinessObjectFactory().Load<JobComInvoiceLine>(InvoiceLine.PK);
			AssertEquals("No additional IOP row should have been created", 1, savedInvoiceLine.InAndOutwardProcessings.Count);
		});
	}

	public void TestInAndOutwardProcessing_DeletedRowNoReused()
	{
		InvoiceLine.InAndOutwardProcessingDirection = "1";
		Factory.Save();
		var savedInvoiceLine = new BusinessObjectFactory().Load<JobComInvoiceLine>(InvoiceLine.PK);
		var savedInAndOutwardProcessingPK = savedInvoiceLine.InAndOutwardProcessing.PK;
		savedInvoiceLine.InAndOutwardProcessing.Delete();
		AssertNotEquals($"Should not provide cached but deleted value", savedInAndOutwardProcessingPK, savedInvoiceLine.InAndOutwardProcessing.PK);
	}

	public void TestInAndOutwardProcessingDirectionCaption() => AssertEquals("Direction", DataBoundResourceStrings.GetDataForProperty(InvoiceLine.InAndOutwardProcessingDirectionInfo).Caption);

	public void TestInAndOutwardProcessingRefinementTypeCaption() => AssertEquals("Refinement Type", DataBoundResourceStrings.GetDataForProperty(InvoiceLine.InAndOutwardProcessingRefinementTypeInfo).Caption);

	public void TestInAndOutwardProcessingProcessTypeCaption() => AssertEquals("Process Type", DataBoundResourceStrings.GetDataForProperty(InvoiceLine.InAndOutwardProcessingProcessTypeInfo).Caption);

	public void TestInAndOutwardProcessingBillingTypeCaption() => AssertEquals("Billing Type", DataBoundResourceStrings.GetDataForProperty(InvoiceLine.InAndOutwardProcessingBillingTypeInfo).Caption);

	public void TestInAndOutwardProcessingRepairCaption() => AssertEquals("Repair", DataBoundResourceStrings.GetDataForProperty(InvoiceLine.InAndOutwardProcessingRepairInfo).Caption);

	public void TestInAndOutwardProcessingRepairReasonCaption() => AssertEquals("Repair Reason", DataBoundResourceStrings.GetDataForProperty(InvoiceLine.InAndOutwardProcessingRepairReasonInfo).Caption);

	public void TestNotifyCustomsOfficesCollection()
	{
		CombineAssertions(() =>
		{
			AssertType<NotifyCustomsOfficeCollection>(InvoiceLine.NotifyCustomsOffices);
			AssertType<NotifyCustomsOffice>(InvoiceLine.NotifyCustomsOffices.AddNew());
		});
	}

	public void TestJI_Calc_StatisticalValue()
	{
		CombineAssertions(() =>
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			InvoiceHeader.JZ_RX_NKInvoice_Currency = "CHF";
			InvoiceLine.JI_LinePrice = 50m;
			var charge1 = InvoiceLine.Charges.AddNew();
			charge1.J7_ChargeType = "ADD";
			charge1.J7_RX_NKCurrency = Declaration.LocalCurrencyCode;
			charge1.J7_Amount = 10m;
			charge1.J7_IsStatisticalValueApplicable = true;
			var charge2 = InvoiceLine.Charges.AddNew();
			charge2.J7_ChargeType = "ADD";
			charge2.J7_RX_NKCurrency = Declaration.LocalCurrencyCode;
			charge2.J7_Amount = 20m;
			charge2.J7_IsStatisticalValueApplicable = false;
			AssertEquals(60m, InvoiceLine.JI_Calc_StatisticalValue);
		});
	}

	public void TestJI_CustomsValue()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Readonly", true, InvoiceLine.JI_CustomsValueInfo.ReadOnly);
			AssertEquals("Caption", "Customs Value", InvoiceLine.JI_CustomsValueInfo.Description);
		});
	}

	public void TestAdditionalInformation()
	{
		var invoiceLine = Factory.New<JobComInvoiceLine>();
		AssertNotNull("AdditionalInformations should never be null", invoiceLine.AdditionalInformations);
	}

	public void TestJI_Weight()
	{
		AssertEquals("Caption", "Gross Weight", InvoiceLine.JI_WeightInfo.Description);
	}

	public void TestJI_WeightUQ()
	{
		AssertEquals("Default", Core.Constants.Weight.Kilograms, InvoiceLine.JI_WeightUQ);
	}

	public void TestJI_NetWeight()
	{
		AssertEquals("Caption", "Net Weight", InvoiceLine.JI_NetWeightInfo.Description);
	}

	public void TestJI_NetWeightUQ()
	{
		AssertEquals("Default", Core.Constants.Weight.Kilograms, InvoiceLine.JI_NetWeightUQ);
	}

	public void TestJI_CustomsQuantity()
	{
		Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		CombineAssertions(() =>
		{
			AssertEquals("Caption", "Gross Weight", InvoiceLine.JI_CustomsQuantityInfo.Description);
			AssertEquals("ReadOnly", true, InvoiceLine.JI_CustomsQuantityInfo.ReadOnly);

			InvoiceLine.JI_WeightUQ = Core.Constants.Weight.Grams;
			InvoiceLine.JI_Weight = 12000m;
			AssertEquals("On JI_Weight change", 12m, InvoiceLine.JI_CustomsQuantity);
			InvoiceLine.JI_WeightUQ = Core.Constants.Weight.Kilograms;
			AssertEquals("On JI_WeightUQ change", 12000m, InvoiceLine.JI_CustomsQuantity);
		});
	}

	public void TestJI_CustomsQuantityAfterUpdatingTariff()
	{
		var tariffTestHelper = new RefCusTariffTestHelper(Factory);
		const string conditionComment = "Condition WeightCheck2: [KGM]/[NAR] <= 3.5";
		var tariffWithWGTC2 = tariffTestHelper.CreateImportTariffWithConditionClass("61101100000000", UniversalReferenceConstants.CusConditionType.WeightCheck2, "[KGM]/[NAR] <= 3.5", conditionComment);

		Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		InvoiceLine.JI_WeightUQ = Core.Constants.Weight.Grams;
		InvoiceLine.JI_Weight = 12000m;

		CombineAssertions(() =>
		{
			AssertEquals("value before tariff changed", 12m, InvoiceLine.JI_CustomsQuantity);

			InvoiceLine.JI_Tariff = tariffWithWGTC2.ZZ1_TariffCode;

			AssertEquals("value after tariff changed", 12m, InvoiceLine.JI_CustomsQuantity);
		});
	}

	public override void TestCustomsQuantityIsReadonlyWhenUnitQtyEmpty()
	{
		AssertEquals("CustomsQuantity is always ReadOnly", true, InvoiceLine.JI_CustomsQuantityInfo.ReadOnly);
	}

	public override void TestMakeCustomsQuantityReadOnly()
	{
		AssertEquals("CustomsQuantity is always ReadOnly", true, InvoiceLine.JI_CustomsQuantityInfo.ReadOnly);
	}

	public void TestJI_CustomsUnitQty()
	{
		new RefCusTariffTestHelper(Factory).CreateImportTariffWithUOMs(RefCusTariffTestHelper.ImportTariffBycycle, "KGMG", "KGM", "NAR");
		Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		InvoiceLine.JI_Tariff = RefCusTariffTestHelper.ImportTariffBycycle;
		CombineAssertions(() =>
		{
			AssertEquals("Default", "KGMG", InvoiceLine.JI_CustomsUnitQty);
			AssertEquals("ReadOnly", true, InvoiceLine.JI_CustomsUnitQtyInfo.ReadOnly);
		});
	}

	public void TestJI_CustomsSecondQuantity()
	{
		Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		CombineAssertions(() =>
		{
			AssertEquals("Caption", "Net Weight", InvoiceLine.JI_CustomsSecondQuantityInfo.Description);
			AssertEquals("ReadOnly", true, InvoiceLine.JI_CustomsSecondQuantityInfo.ReadOnly);

			InvoiceLine.JI_NetWeightUQ = Core.Constants.Weight.Grams;
			InvoiceLine.JI_NetWeight = 12000;
			AssertEquals("On JI_NetWeight change", 12m, InvoiceLine.JI_CustomsSecondQuantity);
			InvoiceLine.JI_NetWeightUQ = Core.Constants.Weight.Kilograms;
			AssertEquals("On JI_NetWeightUQ change", 12000m, InvoiceLine.JI_CustomsSecondQuantity);
		});
	}

	public void TestJI_CustomsSecondQuantityAfterUpdatingTariff()
	{
		var tariffTestHelper = new RefCusTariffTestHelper(Factory);
		const string conditionComment = "Condition WeightCheck2: [KGM]/[NAR] <= 3.5";
		var tariffWithWGTC2 = tariffTestHelper.CreateImportTariffWithConditionClass("61101100000000", UniversalReferenceConstants.CusConditionType.WeightCheck2, "[KGM]/[NAR] <= 3.5", conditionComment);

		Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		InvoiceLine.JI_NetWeightUQ = Core.Constants.Weight.Grams;
		InvoiceLine.JI_NetWeight = 12000m;

		CombineAssertions(() =>
		{
			AssertEquals("value before tariff changed", 12m, InvoiceLine.JI_CustomsSecondQuantity);

			InvoiceLine.JI_Tariff = tariffWithWGTC2.ZZ1_TariffCode;

			AssertEquals("value after tariff changed", 12m, InvoiceLine.JI_CustomsSecondQuantity);
		});
	}

	public void TestJI_CustomsSecondUnitQty()
	{
		new RefCusTariffTestHelper(Factory).CreateImportTariffWithUOMs(RefCusTariffTestHelper.ImportTariffBycycle, "KGMG", "KGM", "NAR");

		Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		InvoiceLine.JI_Tariff = RefCusTariffTestHelper.ImportTariffBycycle;
		CombineAssertions(() =>
		{
			AssertEquals("Default", "KGM", InvoiceLine.JI_CustomsSecondUnitQty);
			AssertEquals("ReadOnly", true, InvoiceLine.JI_CustomsSecondUnitQtyInfo.ReadOnly);
		});
	}

	public void TestJI_CustomsThirdQuantity()
	{
		AssertEquals("Caption", "Additional Quantity", InvoiceLine.JI_CustomsThirdQuantityInfo.Description);
	}

	public void TestJI_CustomsThirdUnitQty_Import() => AssertJI_CustomsThirdUnitQty(JobMessageTypeList.Codes.Import, RefCusTariffTestHelper.ImportTariffBycycle, RefCusTariffTestHelper.ImportTariffCodeGoose);

	public void TestJI_CustomsThirdUnitQty_Export() => AssertJI_CustomsThirdUnitQty(JobMessageTypeList.Codes.Export, RefCusTariffTestHelper.ExportTariffGardenUmbrellas, RefCusTariffTestHelper.ExportTariffStraw);

	void AssertJI_CustomsThirdUnitQty(string messageType, string tariffCodeWithUOM3, string tariffCodeWithoutUOM3)
	{
		var helper = new RefCusTariffTestHelper(Factory);
		helper.CreateTariffWithUOMs(tariffCodeWithUOM3, messageType, "KGMG", "KGM", "NAR");
		helper.CreateTariffWithUOMs(tariffCodeWithoutUOM3, messageType, "KGMG", "KGM");

		Declaration.JE_MessageType = messageType;

		CombineAssertions(() =>
		{
			AssertEquals("ReadOnly", true, InvoiceLine.JI_CustomsThirdUnitQtyInfo.ReadOnly);

			InvoiceLine.JI_Tariff = tariffCodeWithUOM3;
			AssertEquals("Unit from tariff CU3", RefCusTariffTestHelper.UomCU3NAR, InvoiceLine.JI_CustomsThirdUnitQty);
			InvoiceLine.JI_Tariff = tariffCodeWithoutUOM3;
			AssertEquals("No unit from tariff CU3", ZString.Empty, InvoiceLine.JI_CustomsThirdUnitQty);
		});
	}

	public void TestJI_CustomsFourthQuantity()
	{
		Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		CombineAssertions(() =>
		{
			AssertEquals("Caption", "Sensible Goods Qty", InvoiceLine.JI_CustomsFourthQuantityInfo.Description);
			AssertEquals("ReadOnly", true, InvoiceLine.JI_CustomsFourthQuantityInfo.ReadOnly);
			InvoiceLine.JI_CustomsFourthUnitQty = "LPA";
			AssertEquals("ReadOnly", false, InvoiceLine.JI_CustomsFourthQuantityInfo.ReadOnly);
		});

		CombineAssertions("Check JI_CustomsFourthQuantity reset", () =>
		{
			InvoiceLine.JI_CustomsFourthUnitQty = "LPA";
			InvoiceLine.JI_CustomsFourthQuantity = 100;
			AssertEquals("JI_CustomsFourthUnitQty different from zero", 100M, InvoiceLine.JI_CustomsFourthQuantity);
			InvoiceLine.JI_CustomsFourthUnitQty = "";
			AssertEquals("When JI_CustomsFourthUnitQty is empty, JI_CustomsFourthQuantity should be zero", ZDecimal.Zero, InvoiceLine.JI_CustomsFourthQuantity);
		});
	}

	public void TestJI_CustomsFourthUnitQty()
	{
		Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		AssertEquals("ReadOnly", true, InvoiceLine.JI_CustomsFourthUnitQtyInfo.ReadOnly);
	}

	public void TestJI_LinePrice()
	{
		AssertEquals("Caption", "Price", InvoiceLine.JI_LinePriceInfo.Description);
	}

	public void TestJI_RX_NKLinePriceCurr()
	{
		Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		CombineAssertions(() =>
		{
			AssertEquals("ReadOnly", true, InvoiceLine.JI_RX_NKLinePriceCurrInfo.ReadOnly);

			InvoiceHeader.JZ_RX_NKInvoice_Currency = "BGF";
			AssertEquals("Same as InvoiceCurrency", "BGF", InvoiceLine.JI_RX_NKLinePriceCurr);
		});
	}

	public void TestJI_NonTradingGoods()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Caption", "Non Commercial Goods", DataBoundResourceStrings.GetDataForProperty(InvoiceLine.JI_NonTradingGoodsInfo).Caption);
			AssertEquals("ShortCaption", "Non Comm. Goods", DataBoundResourceStrings.GetDataForProperty(InvoiceLine.JI_NonTradingGoodsInfo).ShortCaption);
			AssertEquals("FullDescription", "Indicate if a certain goods is to be considered commercial or not when imported.", DataBoundResourceStrings.GetDataForProperty(InvoiceLine.JI_NonTradingGoodsInfo).FullDescription);
		});
	}

	public void TestIsIndustrialTariff()
	{
		CombineAssertions(() =>
		{
			InvoiceLine.JI_Tariff = "00000000000000";
			AssertEquals(InvoiceLine.JI_Tariff, false, InvoiceLine.IsIndustrialTariff);
			InvoiceLine.JI_Tariff = "24999999999999";
			AssertEquals(InvoiceLine.JI_Tariff, false, InvoiceLine.IsIndustrialTariff);
			InvoiceLine.JI_Tariff = "25000000000000";
			AssertEquals(InvoiceLine.JI_Tariff, true, InvoiceLine.IsIndustrialTariff);
			InvoiceLine.JI_Tariff = "99999999999999";
			AssertEquals(InvoiceLine.JI_Tariff, true, InvoiceLine.IsIndustrialTariff);
		});
	}

	public void TestIsDirectTransportationOrigin()
	{
		RefCusCodeTestHelper.CreateDirectTransportationCodeList(Factory);

		CombineAssertions(() =>
		{
			InvoiceLine.JI_CountryOfOrigin = RefCusCodeTestHelper.ValidDirectTransportationCountry;
			AssertEquals("is a direct transportation country", true, InvoiceLine.OriginIsDirectTransportationCountry);
			InvoiceLine.JI_CountryOfOrigin = RefCusCodeTestHelper.InvalidDirectTransportationCountry;
			AssertEquals("not a direct transportation country", false, InvoiceLine.OriginIsDirectTransportationCountry);
		});
	}

	public void TestNormalTariffDutyRateSelectionCriteria()
	{
		InvoiceLine.JI_PrimaryPreference = ZString.Empty;
		var selectionriteria = InvoiceLine.NormalTariffDutyRateSelectionCriteria;
		CombineAssertions(() =>
		{
			AssertType<JobComInvoiceLine.NormalTariffRateSelectionCriteria>(selectionriteria);
			AssertEquals("PrimaryPreference", UniversalReferenceConstants.PrimaryPreferenceCodes.NormalTariff, selectionriteria.PrimaryPreference);
			AssertEquals("RateType", Universal.Constants.RateTypes.Duty, selectionriteria.RateType);
			AssertEquals("RateCode", ZString.Empty, selectionriteria.RateCode);
		});
	}

	public void TestRateFormulaWithFallbackToNormalTariff()
	{
		var tariffTestHelper = new RefCusTariffTestHelper(Factory);
		var tariff_PR2_NT3 = tariffTestHelper.CreateImportTariff("10000000000000");
		tariffTestHelper.AddRate(tariff_PR2_NT3, PrimaryPreferenceCodes.PreferentialTariff, "2");
		tariffTestHelper.AddRate(tariff_PR2_NT3, PrimaryPreferenceCodes.NormalTariff, "3");
		var tariff_PR0_NT4 = tariffTestHelper.CreateImportTariff("20000000000000");
		tariffTestHelper.AddRate(tariff_PR0_NT4, PrimaryPreferenceCodes.PreferentialTariff, "0");
		tariffTestHelper.AddRate(tariff_PR0_NT4, PrimaryPreferenceCodes.NormalTariff, "4");
		var tariff_PR0 = tariffTestHelper.CreateImportTariff("30000000000000");
		tariffTestHelper.AddRate(tariff_PR0, PrimaryPreferenceCodes.PreferentialTariff, "0");
		var tariff_PR7 = tariffTestHelper.CreateImportTariff("70000000000000");
		tariffTestHelper.AddRate(tariff_PR7, PrimaryPreferenceCodes.PreferentialTariff, "7");
		var tariff_NT5 = tariffTestHelper.CreateImportTariff("40000000000000");
		tariffTestHelper.AddRate(tariff_NT5, PrimaryPreferenceCodes.NormalTariff, "5");
		var tariff_NoRate = tariffTestHelper.CreateImportTariff("50000000000000");

		Declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
		InvoiceLine.JI_CountryOfOrigin = RefCusTariffTestHelper.Country;

		CombineAssertions(() =>
		{
			InvoiceLine.JI_PrimaryPreference = PrimaryPreferenceCodes.PreferentialTariff;

			InvoiceLine.JI_Tariff = tariff_PR2_NT3.ZZ1_TariffCode;
			AssertEquals(GetMessage(), "2", InvoiceLine.RateFormulaWithFallbackToNormalTariff);

			InvoiceLine.JI_Tariff = tariff_PR0_NT4.ZZ1_TariffCode;
			AssertEquals(GetMessage(), "4", InvoiceLine.RateFormulaWithFallbackToNormalTariff);

			InvoiceLine.JI_Tariff = tariff_PR0.ZZ1_TariffCode;
			AssertEquals(GetMessage("Only PR 0"), "0", InvoiceLine.RateFormulaWithFallbackToNormalTariff);

			InvoiceLine.JI_Tariff = tariff_NT5.ZZ1_TariffCode;
			AssertEquals(GetMessage(), "5", InvoiceLine.RateFormulaWithFallbackToNormalTariff);

			InvoiceLine.JI_Tariff = tariff_NoRate.ZZ1_TariffCode;
			AssertEquals(GetMessage("No rate"), ZString.Empty, InvoiceLine.RateFormulaWithFallbackToNormalTariff);

			InvoiceLine.JI_Tariff = "90909090090090";
			AssertEquals(GetMessage("Invalid tariff"), ZString.Empty, InvoiceLine.RateFormulaWithFallbackToNormalTariff);

			InvoiceLine.JI_PrimaryPreference = PrimaryPreferenceCodes.NormalTariff;
			InvoiceLine.JI_Tariff = tariff_PR2_NT3.ZZ1_TariffCode;
			AssertEquals(GetMessage(), "3", InvoiceLine.RateFormulaWithFallbackToNormalTariff);
			InvoiceLine.JI_PrimaryPreference = ZString.Empty;
			AssertEquals(GetMessage(), "3", InvoiceLine.RateFormulaWithFallbackToNormalTariff);

			InvoiceLine.JI_Tariff = tariff_PR7.ZZ1_TariffCode;
			AssertEquals(GetMessage("No rate for NT"), ZString.Empty, InvoiceLine.RateFormulaWithFallbackToNormalTariff);

			InvoiceLine.JI_PrimaryPreference = ZString.Empty;
			AssertEquals(GetMessage("No preference"), ZString.Empty, InvoiceLine.RateFormulaWithFallbackToNormalTariff);
			InvoiceLine.JI_PrimaryPreference = PrimaryPreferenceCodes.PreferentialTariff;

			InvoiceLine.JI_CountryOfOrigin = ZString.Empty;
			AssertEquals(GetMessage("No country"), ZString.Empty, InvoiceLine.RateFormulaWithFallbackToNormalTariff);
			InvoiceLine.JI_CountryOfOrigin = RefCusTariffTestHelper.Country;

			InvoiceLine.JI_Tariff = ZString.Empty;
			AssertEquals(GetMessage("No tariff"), ZString.Empty, InvoiceLine.RateFormulaWithFallbackToNormalTariff);
		});

		string GetMessage(string message = null) => $"JI_Tariff={InvoiceLine.JI_Tariff} JI_PrimaryPreference={InvoiceLine.JI_PrimaryPreference} JI_CountryOfOrigin={InvoiceLine.JI_CountryOfOrigin} {message}";
	}

	public void TestSupportingDocumentsIncludingInherited()
	{
		ISupportingDocumentParent supportingDocumentParent = InvoiceLine;

		CombineAssertions(() =>
		{
			AssertEquals("both empty", false, supportingDocumentParent.SupportingDocumentsIncludingInherited.Any());

			var lineDoc1 = InvoiceLine.SupportingDocuments.AddNew();
			var lineDoc2 = InvoiceLine.SupportingDocuments.AddNew();
			AssertEquals("only line - count", 2, supportingDocumentParent.SupportingDocumentsIncludingInherited.Count());
			AssertEquals("only line - lineDoc1", true, supportingDocumentParent.SupportingDocumentsIncludingInherited.Any(x => object.ReferenceEquals(x, lineDoc1)));
			AssertEquals("only line - lineDoc2", true, supportingDocumentParent.SupportingDocumentsIncludingInherited.Any(x => object.ReferenceEquals(x, lineDoc2)));

			var headerDoc1 = InvoiceHeader.SupportingDocuments.AddNew();
			var headerDoc2 = InvoiceHeader.SupportingDocuments.AddNew();
			AssertEquals("header and line - count", 4, supportingDocumentParent.SupportingDocumentsIncludingInherited.Count());
			AssertEquals("header and line - lineDoc1", true, supportingDocumentParent.SupportingDocumentsIncludingInherited.Any(x => object.ReferenceEquals(x, lineDoc1)));
			AssertEquals("header and line - lineDoc2", true, supportingDocumentParent.SupportingDocumentsIncludingInherited.Any(x => object.ReferenceEquals(x, lineDoc2)));
			AssertEquals("header and line - headerDoc1", true, supportingDocumentParent.SupportingDocumentsIncludingInherited.Any(x => object.ReferenceEquals(x, headerDoc1)));
			AssertEquals("header and line - headerDoc2", true, supportingDocumentParent.SupportingDocumentsIncludingInherited.Any(x => object.ReferenceEquals(x, headerDoc2)));

			InvoiceLine.SupportingDocuments.RemoveAll();
			AssertEquals("only header - count", 2, supportingDocumentParent.SupportingDocumentsIncludingInherited.Count());
			AssertEquals("only header - headerDoc1", true, supportingDocumentParent.SupportingDocumentsIncludingInherited.Any(x => object.ReferenceEquals(x, headerDoc1)));
			AssertEquals("only header - headerDoc2", true, supportingDocumentParent.SupportingDocumentsIncludingInherited.Any(x => object.ReferenceEquals(x, headerDoc2)));

			var invoiceLine2 = Factory.New<JobComInvoiceLine>();
			invoiceLine2.SupportingDocuments.AddNew();
			ISupportingDocumentParent supportingDocumentParent2 = invoiceLine2;
			AssertEquals("No InvoiceHeader", 1, supportingDocumentParent2.SupportingDocumentsIncludingInherited.Count());
		});
	}

	public void TestJI_ZZF_NKTaxType() => CombineAssertions(() =>
	{
		RefCusTaxOrFeeTestHelper.CreateRefCusTaxOrFeeList(Factory);

		var declaration = Factory.NewWithValidTestData<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();

		invoiceLine.JI_Tariff = RefCusTaxOrFeeTestHelper.TariffWithSingleFee;
		AssertEquals("When Tariff = 99999999, VAT Code should be automatically selected", UniversalReferenceConstants.TaxCodes.StandardRate, invoiceLine.JI_ZZF_NKTaxType);

		invoiceLine.JI_Tariff = ZString.Empty;
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		invoiceLine.JI_CEI = entryInstruction.PK;
		entryInstruction.CEI_DateForDuty = new ZDate(2023, 12, 31);
		invoiceLine.JI_Tariff = RefCusTaxOrFeeTestHelper.TariffWithSingleFee;
		AssertEquals("When Tariff = 99999999, VAT Code should be automatically selected (with Acceptance)", UniversalReferenceConstants.TaxCodes.ReducedRate, invoiceLine.JI_ZZF_NKTaxType);

		entryInstruction.CEI_DateForDuty = ZDate.Empty;
		invoiceLine.JI_Tariff = RefCusTaxOrFeeTestHelper.TariffWithMultipleFees;
		AssertEquals("When Tariff = 11111111, VAT Code should be selected from the user", "", invoiceLine.JI_ZZF_NKTaxType);

		invoiceLine.JI_Tariff = RefCusTaxOrFeeTestHelper.InvalidTariff;
		AssertEquals("TaxOrFeeCodeList should be empty", true, invoiceLine.Lookups.TaxOrFeeCodeList.Count > 0);
		AssertEquals("When Tariff is invalid, VAT Code should be empty", "", invoiceLine.JI_ZZF_NKTaxType);

		invoiceLine.JI_Tariff = ZString.Empty;
		AssertEquals("TaxOrFeeCodeList should be empty", true, invoiceLine.Lookups.TaxOrFeeCodeList.Count > 0);
		AssertEquals("When Tariff is empty, VAT Code should be empty", "", invoiceLine.JI_ZZF_NKTaxType);
	});

	public void TestDutyRateAdditionalCodeCaption()
	{
		AssertEquals("Caption", "Duty Rate", DataBoundResourceStrings.GetDataForProperty(InvoiceLine.DutyRateAdditionalCodeInfo).Caption);
	}

	public void TestDutyRateAdditionalCodeMaxLength()
	{
		AssertEquals(15, InvoiceLine.DutyRateAdditionalCodeInfo.MaxLength);
	}

	public void TestDutyRateAdditionalCode_Persisted()
	{
		Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		InvoiceLine.DutyRateAdditionalCode = "123";
		Factory.Save();

		var newFactory = new BusinessObjectFactory();
		var savedInvoiceLine = newFactory.Load<JobComInvoiceLine>(InvoiceLine.PK);
		AssertEquals(new ZString("123"), savedInvoiceLine.DutyRateAdditionalCode);
	}

	public void TestDutyRateDescriptionCaption()
	{
		AssertEquals("Caption", "Duty Rate", DataBoundResourceStrings.GetDataForProperty(InvoiceLine.DutyRateAdditionalCodeInfo).Caption);
	}

	public void TestDutyRateDescription_Setter()
	{
		var tariffHelper = new RefCusTariffTestHelper(Factory);
		tariffHelper.CreateImportTariffWithMultipleRates(RefCusTariffTestHelper.ImportTariffBycycle);
		RefCusCodeTestHelper.CreateAdditionalCodes(Factory);

		Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		InvoiceLine.JI_Tariff = RefCusTariffTestHelper.ImportTariffBycycle;
		InvoiceLine.JI_CountryOfOrigin = RefCusTariffTestHelper.Country;
		InvoiceLine.JI_PrimaryPreference = UniversalReferenceConstants.PrimaryPreferenceCodes.PreferentialTariff;

		CombineAssertions(() =>
		{
			var description = ZString.Empty;
			InvoiceLine.DutyRateDescription = description;
			AssertEquals($"Description={description}", ZString.Empty, InvoiceLine.DutyRateAdditionalCode);

			description = "description AC001";
			InvoiceLine.DutyRateDescription = description;
			AssertEquals($"Description={description}", "AC001", InvoiceLine.DutyRateAdditionalCode);
		});
	}

	public void TestDutyRateDescription_Getter()
	{
		var tariffHelper = new RefCusTariffTestHelper(Factory);
		tariffHelper.CreateImportTariffWithMultipleRates(RefCusTariffTestHelper.ImportTariffBycycle);
		RefCusCodeTestHelper.CreateAdditionalCodes(Factory);

		Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		InvoiceLine.JI_Tariff = RefCusTariffTestHelper.ImportTariffBycycle;
		InvoiceLine.JI_CountryOfOrigin = RefCusTariffTestHelper.Country;
		InvoiceLine.JI_PrimaryPreference = UniversalReferenceConstants.PrimaryPreferenceCodes.PreferentialTariff;

		CombineAssertions(() =>
		{
			InvoiceLine.DutyRateAdditionalCode = ZString.Empty;
			AssertEquals($"Code={InvoiceLine.DutyRateAdditionalCode}", ZString.Empty, InvoiceLine.DutyRateDescription);

			InvoiceLine.DutyRateAdditionalCode = "AC001";
			AssertEquals($"Code={InvoiceLine.DutyRateAdditionalCode}", "description AC001", InvoiceLine.DutyRateDescription);
		});
	}

	public void TestDutyRateFormulaNumber()
	{
		var tariffHelper = new RefCusTariffTestHelper(Factory);
		var multipleRateTariff = tariffHelper.CreateImportTariffWithMultipleRates(RefCusTariffTestHelper.ImportTariffBycycle,
			rateFormulas: new[] { "12 * [NAR]", "2.04 * [KGM]", "3.04 * [KGMG]", "0", "0.132 * [KGMG]", "0.2 * [KGMG]", "0.18 * [KGM]" });

		Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		InvoiceLine.JI_Tariff = RefCusTariffTestHelper.ImportTariffBycycle;
		InvoiceLine.JI_CountryOfOrigin = RefCusTariffTestHelper.Country;
		InvoiceLine.JI_PrimaryPreference = UniversalReferenceConstants.PrimaryPreferenceCodes.PreferentialTariff;

		CombineAssertions(() =>
		{
			InvoiceLine.DutyRateAdditionalCode = "AC001";
			AssertEquals("Rate 12 [NAR]", "12", InvoiceLine.DutyRateFormulaNumber);

			InvoiceLine.DutyRateAdditionalCode = "AC002";
			AssertEquals("Rate 2.04 [KGM]", "204", InvoiceLine.DutyRateFormulaNumber);

			InvoiceLine.DutyRateAdditionalCode = "AC003";
			AssertEquals("Rate 3.04 [KGMG]", "304", InvoiceLine.DutyRateFormulaNumber);

			InvoiceLine.DutyRateAdditionalCode = "AC004";
			AssertEquals("Rate 0", "0", InvoiceLine.DutyRateFormulaNumber);

			InvoiceLine.DutyRateAdditionalCode = "AC005";
			AssertEquals("Rate 0.132 [KGMG]", "13.2", InvoiceLine.DutyRateFormulaNumber);

			InvoiceLine.DutyRateAdditionalCode = "AC006";
			AssertEquals("Rate 0.2 [KGMG]", "20", InvoiceLine.DutyRateFormulaNumber);

			InvoiceLine.DutyRateAdditionalCode = "AC007";
			AssertEquals("Rate 0.18 [KGMG]", "18", InvoiceLine.DutyRateFormulaNumber);

			InvoiceLine.DutyRateAdditionalCode = ZString.Empty;
			AssertEquals("Empty DutyRateAdditionalCode", ZString.Empty, InvoiceLine.DutyRateFormulaNumber);
		});
	}

	public void TestDutyRateConfirmation_Caption() => AssertEquals("Caption", "Confirmation", DataBoundResourceStrings.GetDataForProperty(InvoiceLine.DutyRateConfirmationInfo).Caption);

	public void TestDutyRateConfirmation_Persisted()
	{
		Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		CombineAssertions(() =>
		{
			InvoiceLine.DutyRateConfirmation = ZBool.True;
			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			var savedInvoiceLine = newFactory.Load<JobComInvoiceLine>(InvoiceLine.PK);
			AssertEquals(ZBool.True, savedInvoiceLine.DutyRateConfirmation);

			InvoiceLine.DutyRateConfirmation = ZBool.False;
			Factory.Save();
			newFactory = new BusinessObjectFactory();
			savedInvoiceLine = newFactory.Load<JobComInvoiceLine>(InvoiceLine.PK);
			AssertEquals(ZBool.False, savedInvoiceLine.DutyRateConfirmation);
		});
	}

	public void TestDutyRateAdditionalCode_ReadOnly()
	{
		var tariffHelper = new RefCusTariffTestHelper(Factory);
		var singleRateTariff = tariffHelper.CreateImportTariffWithSingleRate(RefCusTariffTestHelper.ImportTariffBeverages);
		var multipleRateTariff = tariffHelper.CreateImportTariffWithMultipleRates(RefCusTariffTestHelper.ImportTariffBycycle);
		var singleRateTariffWithCode = tariffHelper.CreateImportTariffWithMultipleRates(RefCusTariffTestHelper.ImportTariffCodeDuck, rateFormulas: new[] { "1.2 * [KGM]" });

		Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		InvoiceLine.JI_CountryOfOrigin = RefCusTariffTestHelper.Country;
		InvoiceLine.JI_PrimaryPreference = UniversalReferenceConstants.PrimaryPreferenceCodes.PreferentialTariff;

		CombineAssertions(() =>
		{
			InvoiceLine.JI_Tariff = singleRateTariff.ZZ1_TariffCode;
			AssertEquals("Single rate", 0, InvoiceLine.Lookups.AdditionalCodesList.Count);
			AssertEquals("Single rate", true, InvoiceLine.DutyRateAdditionalCodeInfo.ReadOnly);

			InvoiceLine.JI_Tariff = multipleRateTariff.ZZ1_TariffCode;
			AssertEquals("Multiple rate", 2, InvoiceLine.Lookups.AdditionalCodesList.Count);
			AssertEquals("Multiple rates", false, InvoiceLine.DutyRateAdditionalCodeInfo.ReadOnly);

			InvoiceLine.JI_Tariff = singleRateTariffWithCode.ZZ1_TariffCode;
			AssertEquals("Single rate with code", 1, InvoiceLine.Lookups.AdditionalCodesList.Count);
			AssertEquals("Single rate with code", false, InvoiceLine.DutyRateAdditionalCodeInfo.ReadOnly);

			InvoiceLine.JI_PrimaryPreference = UniversalReferenceConstants.PrimaryPreferenceCodes.NormalTariff;
			AssertEquals("Triggered by preference", true, InvoiceLine.DutyRateAdditionalCodeInfo.ReadOnly);
			InvoiceLine.JI_PrimaryPreference = UniversalReferenceConstants.PrimaryPreferenceCodes.PreferentialTariff;

			InvoiceLine.JI_CountryOfOrigin = ZString.Empty;
			AssertEquals("Triggered by country of origin", true, InvoiceLine.DutyRateAdditionalCodeInfo.ReadOnly);
		});
	}

	public void TestDefaultDutyRateAdditionalCodeIfApplicable()
	{
		var tariffHelper = new RefCusTariffTestHelper(Factory);
		var singleRateTariff = tariffHelper.CreateImportTariffWithSingleRate(RefCusTariffTestHelper.ImportTariffBeverages);
		var multipleRateTariff = tariffHelper.CreateImportTariffWithMultipleRates(RefCusTariffTestHelper.ImportTariffBycycle);
		var singleRateTariffWithCode = tariffHelper.CreateImportTariffWithMultipleRates(RefCusTariffTestHelper.ImportTariffCodeDuck, rateFormulas: new[] { "1.2 * [KGM]" });

		CombineAssertions(() =>
		{
			setupWithAnyCode();
			InvoiceLine.JI_Tariff = singleRateTariff.ZZ1_TariffCode;
			AssertEquals("single rate tariff", ZString.Empty, InvoiceLine.DutyRateAdditionalCode);

			setupWithAnyCode();
			InvoiceLine.JI_Tariff = singleRateTariffWithCode.ZZ1_TariffCode;
			AssertNotEquals("single rate tariff with code", ZString.Empty, InvoiceLine.DutyRateAdditionalCode);

			setupWithAnyCode();
			InvoiceLine.JI_Tariff = multipleRateTariff.ZZ1_TariffCode;
			AssertNotEquals("multiple rates tariff with code", ZString.Empty, InvoiceLine.DutyRateAdditionalCode);

			setupWithAnyCode();
			InvoiceLine.JI_PrimaryPreference = UniversalReferenceConstants.PrimaryPreferenceCodes.NormalTariff;
			AssertEquals("single rate tariff", ZString.Empty, InvoiceLine.DutyRateAdditionalCode);

			setupWithAnyCode();
			InvoiceLine.JI_CountryOfOrigin = ZString.Empty;
			AssertEquals("preference changed", ZString.Empty, InvoiceLine.DutyRateAdditionalCode);
		});

		void setupWithAnyCode()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			InvoiceLine.JI_CountryOfOrigin = RefCusTariffTestHelper.Country;
			InvoiceLine.JI_PrimaryPreference = UniversalReferenceConstants.PrimaryPreferenceCodes.PreferentialTariff;
			InvoiceLine.JI_Tariff = multipleRateTariff.ZZ1_TariffCode;
			InvoiceLine.DutyRateAdditionalCode = InvoiceLine.Lookups.AdditionalCodesList.GetAllCodes().First();
			AssertNotEquals("Test setup correctly", ZString.Empty, InvoiceLine.DutyRateAdditionalCode);
		}
	}

	public void TestICusCodeDataTypeSupporter()
	{
		Integration.Customs.ICusCodeDataTypeSupporter supporter = InvoiceLine;
		AssertEquals(typeof(AdditionalCodeData), supporter.GetCusCodeDataTypes()[CusCodeDataTypeList.Codes.AdditionalCode]);
	}

	public void TestIsReturnedGoods()
	{
		CombineAssertions(() =>
		{
			InvoiceLine.JI_Procedure = UniversalReferenceConstants.ProcedureCodesEdec.NormalDuty;
			AssertEquals(InvoiceLine.JI_Procedure, false, InvoiceLine.IsReturnedGoods);
			InvoiceLine.JI_Procedure = UniversalReferenceConstants.ProcedureCodesEdec.ReturnedGoods;
			AssertEquals(InvoiceLine.JI_Procedure, true, InvoiceLine.IsReturnedGoods);
			InvoiceLine.JI_Procedure = UniversalReferenceConstants.ProcedureCodesEdec.ReturnedGoodsVAT;
			AssertEquals(InvoiceLine.JI_Procedure, true, InvoiceLine.IsReturnedGoods);
		});
	}

	public void TestIsWithoutDuty()
	{
		CombineAssertions(() =>
		{
			InvoiceLine.JI_Procedure = UniversalReferenceConstants.ProcedureCodesEdec.NormalDuty;
			AssertEquals(InvoiceLine.JI_Procedure, false, InvoiceLine.IsWithoutDuty);
			InvoiceLine.JI_Procedure = UniversalReferenceConstants.ProcedureCodesEdec.ReturnedGoods;
			AssertEquals(InvoiceLine.JI_Procedure, true, InvoiceLine.IsWithoutDuty);
			InvoiceLine.JI_Procedure = UniversalReferenceConstants.ProcedureCodesEdec.ReturnedGoodsVAT;
			AssertEquals(InvoiceLine.JI_Procedure, true, InvoiceLine.IsWithoutDuty);
			InvoiceLine.JI_Procedure = UniversalReferenceConstants.ProcedureCodesEdec.ExemptFromDuty;
			AssertEquals(InvoiceLine.JI_Procedure, true, InvoiceLine.IsWithoutDuty);
		});
	}

	public void TestJI_RefundReferenceNumberCaption() => AssertEquals("Caption", "GDRN", DataBoundResourceStrings.GetDataForProperty(InvoiceLine.JI_RefundReferenceNumberInfo).Caption);

	public void TestJI_RefundReferenceNumberMaxLength() => AssertEquals(18, InvoiceLine.JI_RefundReferenceNumberInfo.MaxLength);

	public void TestJI_RefundGoodsItemNumberCaption() => AssertEquals("Caption", "Goods Item Number", DataBoundResourceStrings.GetDataForProperty(InvoiceLine.JI_RefundGoodsItemNumberInfo).Caption);

	public void TestJI_RefundGoodsItemNumberMaxLength() => AssertEquals(5, InvoiceLine.JI_RefundGoodsItemNumberInfo.MaxLength);

	public void TestJI_RefundReasonCaption() => AssertEquals("Caption", "Refund Reason", DataBoundResourceStrings.GetDataForProperty(InvoiceLine.JI_RefundReasonInfo).Caption);

	public void TestJI_RefundReasonMaxLength() => AssertEquals(512, InvoiceLine.JI_RefundReasonInfo.MaxLength);

	public void TestIsReturnedGoodsWithRefundRequest()
	{
		CombineAssertions(() =>
		{
			Declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Export;
			InvoiceLine.JI_RefundType = RefundType.OtherRefunds;
			AssertEquals("EXP - otherRefunds - IsReturnedGoodsWithRefundRequest false", false, InvoiceLine.IsReturnedGoodsWithRefundRequest);

			InvoiceLine.JI_RefundType = RefundType.ReturnedGoodsWithRefundRequest;
			AssertEquals("EXP - ReturnedGoodsWithRefundRequest - IsReturnedGoodsWithRefundRequest true", true, InvoiceLine.IsReturnedGoodsWithRefundRequest);

			Declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
			AssertEquals("IMP - ReturnedGoodsWithRefundRequest - IsReturnedGoodsWithRefundRequest false", false, InvoiceLine.IsReturnedGoodsWithRefundRequest);
		});
	}

	public void TestIsSamnaunFreeZoneTraffic()
	{
		var additionalInformation = InvoiceLine.AdditionalInformations.AddNew();

		CombineAssertions(() =>
		{
			AssertEquals("No Additional Informations", false, InvoiceLine.IsSamnaunFreeZoneTraffic);

			additionalInformation.CSI_Code = UniversalReferenceConstants.AdditionalInformationTypeCodes.FreeZoneTraffic;
			additionalInformation.CSI_ReferenceNumber = UniversalReferenceConstants.FreeZoneTradeCode.Samnaun;
			AssertEquals($"{additionalInformation.CSI_Code}/{additionalInformation.CSI_ReferenceNumber}", true, InvoiceLine.IsSamnaunFreeZoneTraffic);

			additionalInformation.CSI_Code = UniversalReferenceConstants.AdditionalInformationTypeCodes.BorderZoneTraffic;
			additionalInformation.CSI_ReferenceNumber = UniversalReferenceConstants.FreeZoneTradeCode.Samnaun;
			AssertEquals($"{additionalInformation.CSI_Code}/{additionalInformation.CSI_ReferenceNumber}", false, InvoiceLine.IsSamnaunFreeZoneTraffic);

			additionalInformation.CSI_Code = UniversalReferenceConstants.AdditionalInformationTypeCodes.FreeZoneTraffic;
			additionalInformation.CSI_ReferenceNumber = UniversalReferenceConstants.FreeZoneTradeCode.Hochsavoyen;
			AssertEquals($"{additionalInformation.CSI_Code}/{additionalInformation.CSI_ReferenceNumber}", false, InvoiceLine.IsSamnaunFreeZoneTraffic);

			var additionalInformation2 = InvoiceLine.AdditionalInformations.AddNew();
			additionalInformation2.CSI_Code = UniversalReferenceConstants.AdditionalInformationTypeCodes.FreeZoneTraffic;
			additionalInformation2.CSI_ReferenceNumber = UniversalReferenceConstants.FreeZoneTradeCode.Samnaun;
			AssertEquals($"{additionalInformation.CSI_Code}/{additionalInformation.CSI_ReferenceNumber}, {additionalInformation2.CSI_Code}/{additionalInformation2.CSI_ReferenceNumber}", true, InvoiceLine.IsSamnaunFreeZoneTraffic);
		});
	}

	public void TestIsGSPCertificateRequired()
	{
		RefCusTradeGroupTestHelper.CreateTradeGroups(Factory);

		ISupportingDocumentParent supportingDocumentParent = InvoiceLine;

		CombineAssertions(() =>
		{
			InvoiceLine.JI_CountryOfOrigin = RefCusTradeGroupTestHelper.CountryInDevelopingCountriesTradeGroup;
			InvoiceLine.JI_PrimaryPreference = UniversalReferenceConstants.PrimaryPreferenceCodes.PreferentialTariff;
			AssertEquals("When preferential tariff and development country", true, supportingDocumentParent.IsGSPCertificateRequired);

			InvoiceLine.JI_CountryOfOrigin = RefCusTradeGroupTestHelper.CountryNotInDevelopingCountriesTradeGroup;
			InvoiceLine.JI_PrimaryPreference = UniversalReferenceConstants.PrimaryPreferenceCodes.PreferentialTariff;
			AssertEquals("When preferential tariff and not a development country", false, supportingDocumentParent.IsGSPCertificateRequired);

			InvoiceLine.JI_CountryOfOrigin = RefCusTradeGroupTestHelper.CountryInDevelopingCountriesTradeGroup;
			InvoiceLine.JI_PrimaryPreference = UniversalReferenceConstants.PrimaryPreferenceCodes.NormalTariff;
			AssertEquals("When not preferential tariff but a development country", false, supportingDocumentParent.IsGSPCertificateRequired);

			InvoiceLine.JI_CountryOfOrigin = RefCusTradeGroupTestHelper.CountryNotInDevelopingCountriesTradeGroup;
			InvoiceLine.JI_PrimaryPreference = UniversalReferenceConstants.PrimaryPreferenceCodes.NormalTariff;
			AssertEquals("When not preferential tariff and not a development country", false, supportingDocumentParent.IsGSPCertificateRequired);
		});
	}

	public void TestGetConditionSelectionCriterias()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_GoodsDestination = "AU";
		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		invoiceLine.JI_CountryOfOrigin = "GB";

		CombineAssertions(() =>
		{
			foreach (var messageType in new[] { SharedJobMessageTypeList.Codes.Import, SharedJobMessageTypeList.Codes.Export })
			{
				declaration.JE_MessageType = messageType;
				var expectedTradeGroupCountry = declaration.IsImport ? "GB" : "AU";

				for (var netMassConfirmation = 0; netMassConfirmation <= 1; netMassConfirmation++)
				{
					for (var additionalUnitConfirmation = 0; additionalUnitConfirmation <= 1; additionalUnitConfirmation++)
					{
						for (var statisticalValueConfirmation = 0; statisticalValueConfirmation <= 1; statisticalValueConfirmation++)
						{
							invoiceLine.JI_NetMassConfirmation = netMassConfirmation == 1;
							invoiceLine.JI_AdditionalUnitConfirmation = additionalUnitConfirmation == 1;
							invoiceLine.JI_StatisticalValueConfirmation = statisticalValueConfirmation == 1;
							var criterias = invoiceLine.ConditionSelectionCriterias;
							var confirmations =
								$"{nameof(declaration.JE_MessageType)}={declaration.JE_MessageType} {nameof(netMassConfirmation)}={netMassConfirmation} " +
								$"{nameof(additionalUnitConfirmation)}={additionalUnitConfirmation} {nameof(statisticalValueConfirmation)}={statisticalValueConfirmation}";
							AssertContainsCriteria($"WGTC1: {confirmations}", netMassConfirmation == 0, criterias, ZString.Empty, CusConditionType.WeightCheck1, expectedTradeGroupCountry);
							AssertContainsCriteria($"WGTC2: {confirmations}", additionalUnitConfirmation == 0, criterias, ZString.Empty, CusConditionType.WeightCheck2, expectedTradeGroupCountry);
							AssertContainsCriteria($"MVC: {confirmations}", statisticalValueConfirmation == 0, criterias, ZString.Empty, CusConditionType.MeanValueCheck, expectedTradeGroupCountry);
							AssertContainsCriteria("CTRL: PRM", true, criterias, ConditionClass.Control, ZString.Empty, expectedTradeGroupCountry);
							AssertEquals($"No other conditions expected: {confirmations}", 4 - netMassConfirmation - additionalUnitConfirmation - statisticalValueConfirmation, criterias.Length);
						}
					}
				}
			}
		});
	}

	void AssertContainsCriteria(string message, bool contains, IEnumerable<IZZConditionSelectionCriteria> criterias, string conditionClass, string conditionType, string tradeGroupCountry)
	{
		var criteria = criterias.FirstOrDefault(c => c.ConditionClass == conditionClass && c.ConditionType == conditionType);
		if (contains)
		{
			AssertNotNull(message, criteria);
			AssertType<JobComInvoiceLine.CHConditionSelectionCriteria>("type of ConditionSelectionCriteria", criteria);
			AssertEquals("TradeGroupCountry of ConditionSelectionCriteria", tradeGroupCountry, criteria.TradeGroupCountry);
		}
		else
		{
			AssertNull(message, criteria);
		}
	}

	public void TestEntryLineNumberField()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		declaration.JE_MergeBy = MasterFiles.Business.OrgConstants.MergeInvoiceLines.Tariff;
		declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();

		var invoice = declaration.Invoices.AddNew();

		var line1 = invoice.InvoiceLines.AddNew();
		line1.JI_Tariff = "12345678";
		line1.JI_Description = "description 1";
		var line2 = invoice.InvoiceLines.AddNew();
		line2.JI_Tariff = "87654321";
		line2.JI_Description = "description 2";
		var line3 = invoice.InvoiceLines.AddNew();
		line3.JI_Tariff = "97541357";
		line3.JI_Description = "description 3";

		CombineAssertions(() =>
		{
			AssertEquals("EntryLineNumber 1 should be empty.", line1.EntryLineNumber, ZShort.Zero);
			AssertEquals("EntryLineNumber 2 should be empty.", line2.EntryLineNumber, ZShort.Zero);
			AssertEquals("EntryLineNumber 3 should be empty.", line3.EntryLineNumber, ZShort.Zero);

			declaration.DoMerge();

			var entryHeader = declaration.CustomsEntryHeaders.FirstOrDefault();

			AssertEquals("Entry Line Number 1 should correspond to the entry line created after the merge.", line1.EntryLineNumber, entryHeader.AllEntryLines[0].CL_LineNumber);
			AssertEquals("Entry Line Number 2 should correspond to the entry line created after the merge.", line2.EntryLineNumber, entryHeader.AllEntryLines[1].CL_LineNumber);
			AssertEquals("Entry Line Number 3 should correspond to the entry line created after the merge.", line3.EntryLineNumber, entryHeader.AllEntryLines[2].CL_LineNumber);
		});
	}

	public void TestGetNewLinkPackValidationCore()
	{
		AssertType<InvoiceLinePackageValidation>(InvoiceLine.PackagesForInvoiceLinesForBindingOnly.AddNew().Validation);
	}

	public override void TestCustomsQtyCalculatedWhenInvoiceQtySet()
	{
		Assert("Test not valid since ShouldReCalculateCustomsQtyOnLineQuantityChange is false", true);
	}

	public void TestEvaluateConditionValue()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Test PRM/ no permits", false, InvoiceLine.EvaluateConditionValue(Core.Constants.Customs.Universal.RefCusConditionTypes.ConditionClass.Control, CusCodeDataTypeList.Codes.PermitItemDetails, "1"));

			var permit = InvoiceLine.Permits.AddNew();
			permit.CSI_Code = "1";
			permit.CSI_IssuerType = "1";

			AssertEquals("Test PRM/1", true, InvoiceLine.EvaluateConditionValue(Core.Constants.Customs.Universal.RefCusConditionTypes.ConditionClass.Control, CusCodeDataTypeList.Codes.PermitItemDetails, "1"));
			AssertEquals("Test PRM/2", false, InvoiceLine.EvaluateConditionValue(Core.Constants.Customs.Universal.RefCusConditionTypes.ConditionClass.Control, CusCodeDataTypeList.Codes.PermitItemDetails, "2"));
		});

		CombineAssertions(() =>
		{
			AssertEquals("Test NCL/ no Non Customs Laws", false, InvoiceLine.EvaluateConditionValue(Core.Constants.Customs.Universal.RefCusConditionTypes.ConditionClass.Control, UniversalReferenceConstants.CusConditionValueType.NonCustomsLaw, "1"));

			var nonCustomsLaw = InvoiceLine.NonCustomsLaws.AddNew();
			nonCustomsLaw.CSI_Code = "270";

			AssertEquals("Test NCL/270", true, InvoiceLine.EvaluateConditionValue(Core.Constants.Customs.Universal.RefCusConditionTypes.ConditionClass.Control, UniversalReferenceConstants.CusConditionValueType.NonCustomsLaw, "270"));
			AssertEquals("Test NCL/66", false, InvoiceLine.EvaluateConditionValue(Core.Constants.Customs.Universal.RefCusConditionTypes.ConditionClass.Control, UniversalReferenceConstants.CusConditionValueType.NonCustomsLaw, "66"));
		});

		CombineAssertions(() =>
		{
			AssertEquals("Test RST/ no restrictions", false, InvoiceLine.EvaluateConditionValue(Core.Constants.Customs.Universal.RefCusConditionTypes.ConditionClass.Control, UniversalReferenceConstants.CusConditionValueType.Restriction, "1"));

			var restriction = InvoiceLine.Restrictions.AddNew();
			restriction.CSI_Code = "399";

			AssertEquals("Test RST/399", true, InvoiceLine.EvaluateConditionValue(Core.Constants.Customs.Universal.RefCusConditionTypes.ConditionClass.Control, UniversalReferenceConstants.CusConditionValueType.Restriction, "399"));
			AssertEquals("Test RST/155", false, InvoiceLine.EvaluateConditionValue(Core.Constants.Customs.Universal.RefCusConditionTypes.ConditionClass.Control, UniversalReferenceConstants.CusConditionValueType.Restriction, "155"));
		});
	}

	public void TestHasOriginDocument()
	{
		RefCusCodeTestHelper.CreateOriginDocumentCodes(Factory);

		CombineAssertions(() =>
		{
			AssertEquals("No document", false, InvoiceLine.HasOriginDocument);

			var supportingDocument = InvoiceLine.SupportingDocuments.AddNew();
			supportingDocument.CSI_Code = RefCusCodeTestHelper.OriginDocument;
			AssertEquals("With origin document", true, InvoiceLine.HasOriginDocument);

			supportingDocument.CSI_Code = RefCusCodeTestHelper.NoOriginDocument;
			AssertEquals("With non-origin document", false, InvoiceLine.HasOriginDocument);

			var supportingDocument2 = InvoiceLine.SupportingDocuments.AddNew();
			supportingDocument2.CSI_Code = RefCusCodeTestHelper.OriginDocument;
			AssertEquals("With origin and non-origin document", true, InvoiceLine.HasOriginDocument);

			supportingDocument2.CSI_Code = RefCusCodeTestHelper.NoOriginDocument;
			var supportingDocument3 = InvoiceHeader.SupportingDocuments.AddNew();
			supportingDocument3.CSI_Code = RefCusCodeTestHelper.OriginDocument;
			AssertEquals("With origin document on header", true, InvoiceLine.HasOriginDocument);
		});
	}

	public void TestHasFederalTaxAdministrationCommitmentPermit()
	{
		CombineAssertions(() =>
		{
			AssertEquals("No Permit", false, InvoiceLine.HasFederalTaxAdministrationCommitmentPermit);

			var permit = InvoiceLine.Permits.AddNew();
			permit.CSI_Code = PermitCodes.Commitment;
			permit.CSI_IssuerType = PermitAuthorityCodes.FTA;
			AssertEquals("With Federal Tax Administration Commitment Permit", true, InvoiceLine.HasFederalTaxAdministrationCommitmentPermit);

			permit.CSI_Code = PermitCodes.SingleEPermit;
			AssertEquals("With different Permit", false, InvoiceLine.HasFederalTaxAdministrationCommitmentPermit);
		});
	}

	public void TestHasReversPermit()
	{
		CombineAssertions(() =>
		{
			AssertEquals("No Permit", false, InvoiceLine.HasReversPermit);

			var permit = InvoiceLine.Permits.AddNew();
			permit.CSI_Code = PermitCodes.ReversTobacco;
			permit.CSI_IssuerType = PermitAuthorityCodes.STB;
			AssertEquals("With Revers Tabacco Permit", true, InvoiceLine.HasReversPermit);

			permit.CSI_Code = PermitCodes.SingleEPermit;
			AssertEquals("With different Permit", false, InvoiceLine.HasReversPermit);
		});
	}

	public void TestCalcDataForConditionFormula()
	{
		AssertType<CHConditionCalcDataForInvoiceLine>(InvoiceLine.CalcDataForConditionFormula);
	}

	public void TestTariffSensibleGoodsCode()
	{
		var helper = new RefCusTariffTestHelper(Factory);
		helper.CreateExportTariff(RefCusTariffTestHelper.ExportTariffHay);
		helper.CreateExportTariffWithAttribute(RefCusTariffTestHelper.ExportTariffNonAlcoholicBeer, UniversalReferenceConstants.TariffAttributes.SensibleGoodsCode, UniversalReferenceConstants.TariffAttributes.Values._0);
		helper.CreateExportTariffWithAttribute(RefCusTariffTestHelper.ExportTariffEthylAlcohol, UniversalReferenceConstants.TariffAttributes.SensibleGoodsCode, UniversalReferenceConstants.TariffAttributes.Values._1);
		Factory.Save();

		CombineAssertions(() =>
		{
			AssertEquals("no sensible goods code", null, InvoiceLine.TariffSensibleGoodsCode);

			InvoiceLine.JI_Tariff = RefCusTariffTestHelper.ExportTariffNonAlcoholicBeer;
			AssertEquals("invoice line tariff has sensible goods code (value 0)", "0", InvoiceLine.TariffSensibleGoodsCode);

			InvoiceLine.JI_Tariff = RefCusTariffTestHelper.ExportTariffEthylAlcohol;
			AssertEquals("invoice line tariff has sensible goods code (value 1)", "1", InvoiceLine.TariffSensibleGoodsCode);
		});
	}

	public void TestIsFixedRateAndTariffHasMultipleRates()
	{
		var tariffHelper = new RefCusTariffTestHelper(Factory);
		var singleRateTariff = tariffHelper.CreateImportTariffWithSingleRate(RefCusTariffTestHelper.ImportTariffBeverages);
		var multipleRateTariff = tariffHelper.CreateImportTariffWithMultipleRates(RefCusTariffTestHelper.ImportTariffBycycle);
		var singleRateTariffWithCode = tariffHelper.CreateImportTariffWithMultipleRates(RefCusTariffTestHelper.ImportTariffCodeDuck, rateFormulas: new[] { "1.2 * [KGM]" });

		Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		InvoiceLine.JI_CountryOfOrigin = RefCusTariffTestHelper.Country;
		InvoiceLine.JI_PrimaryPreference = UniversalReferenceConstants.PrimaryPreferenceCodes.PreferentialTariff;

		CombineAssertions(() =>
		{
			InvoiceLine.JI_Tariff = singleRateTariff.ZZ1_TariffCode;
			AssertEquals("Single rate", 0, InvoiceLine.Lookups.AdditionalCodesList.Count);
			AssertEquals("Single rate", true, InvoiceLine.IsFixedRate);
			AssertEquals("Single rate", false, InvoiceLine.TariffHasMultipleRates);

			InvoiceLine.JI_Tariff = multipleRateTariff.ZZ1_TariffCode;
			AssertEquals("Multiple rates", 2, InvoiceLine.Lookups.AdditionalCodesList.Count);
			AssertEquals("Multiple rates", false, InvoiceLine.IsFixedRate);
			AssertEquals("Multiple rates", true, InvoiceLine.TariffHasMultipleRates);

			InvoiceLine.JI_Tariff = singleRateTariffWithCode.ZZ1_TariffCode;
			AssertEquals("Single rate with code", 1, InvoiceLine.Lookups.AdditionalCodesList.Count);
			AssertEquals("Single rate with code", false, InvoiceLine.IsFixedRate);
			AssertEquals("Single rate with code", false, InvoiceLine.TariffHasMultipleRates);
		});
	}

	public void TestIsCustomsRelief()
	{
		var customsReliefProcedures = new[]
		{
				ProcedureCodesEdec.RefinementTransportation, ProcedureCodesEdec.RepairTransportation,
				ProcedureCodesEdec.CustomsRelief, ProcedureCodesEdec.ReturnedGoods, ProcedureCodesEdec.ReturnedGoodsVAT
			};
		var nonCustomsReliefProcedure = ProcedureCodesEdec.NormalDuty;

		foreach (var customsReliefProcedure in customsReliefProcedures)
		{
			InvoiceLine.JI_Procedure = customsReliefProcedure;
			AssertEquals($"IsCustomsRelief should be true when procedure is {InvoiceLine.JI_Procedure}", true, InvoiceLine.IsCustomsRelief);
		}

		InvoiceLine.JI_Procedure = nonCustomsReliefProcedure;
		AssertEquals("IsCustomsRelief should be false", false, InvoiceLine.IsCustomsRelief);
	}

	public void TestIsOrdinaryProcess()
	{
		InvoiceLine.InAndOutwardProcessingProcessType = InAndOutwardProcessingProcessTypesEdec.DueProcedure;
		AssertEquals(InvoiceLine.InAndOutwardProcessingProcessType, true, InvoiceLine.IsOrdinaryProcess);
		InvoiceLine.InAndOutwardProcessingProcessType = InAndOutwardProcessingProcessTypesEdec.SpecialProcedure;
		AssertEquals(InvoiceLine.InAndOutwardProcessingProcessType, false, InvoiceLine.IsOrdinaryProcess);
	}

	public void TestAdditionalTaxes()
	{
		var preparationFactory = new BusinessObjectFactory();

		var declaration = preparationFactory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		var cusLineTariffDetail1 = preparationFactory.New<CusLineTariffDetail>();
		cusLineTariffDetail1.BZ_ParentTableCode = invoiceLine.TablePrefix;
		cusLineTariffDetail1.BZ_ParentID = invoiceLine.PK;
		cusLineTariffDetail1.BZ_Type = RateTypes.AdditionalTaxes;
		var cusLineTariffDetail2 = preparationFactory.New<CusLineTariffDetail>();
		cusLineTariffDetail2.BZ_ParentTableCode = invoiceLine.TablePrefix;
		cusLineTariffDetail2.BZ_ParentID = invoiceLine.PK;
		cusLineTariffDetail2.BZ_Type = RateTypes.AdditionalFees;

		preparationFactory.Save();

		var testInvoiceLine = Factory.Load<JobComInvoiceLine>(invoiceLine.PK);

		AssertNotNull("AdditionalTaxes must not be null", testInvoiceLine.AdditionalTaxes);
		AssertType<CusLineTariffDetailCollection>("Mismatching Type", testInvoiceLine.AdditionalTaxes);

		AssertEquals("Only one CusLineTariffDetail should be loaded", 1, testInvoiceLine.AdditionalTaxes.Count);
		AssertEquals("ADT CusLineTariffDetail should be loaded", RateTypes.AdditionalTaxes, testInvoiceLine.AdditionalTaxes[0].BZ_Type);

		testInvoiceLine.Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		Factory.Save();

		AssertEquals("AdditionalTaxes should be deleted", 0, testInvoiceLine.AdditionalTaxes.Count);
		AssertNull("AdditionalFees should be deleted", Factory.Load<CusLineTariffDetail>(cusLineTariffDetail1.PK));
		AssertNull("AdditionalTaxes should be deleted", Factory.Load<CusLineTariffDetail>(cusLineTariffDetail2.PK));
	}

	public void TestAdditionalFees()
	{
		var preparationFactory = new BusinessObjectFactory();

		var declaration = preparationFactory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();

		var cusLineTariffDetailFEE = preparationFactory.New<CusLineTariffDetail>();
		cusLineTariffDetailFEE.BZ_ParentTableCode = invoiceLine.TablePrefix;
		cusLineTariffDetailFEE.BZ_ParentID = invoiceLine.PK;
		cusLineTariffDetailFEE.BZ_Type = RateTypes.AdditionalFees;

		var cusLineTariffDetailADT = preparationFactory.New<CusLineTariffDetail>();
		cusLineTariffDetailADT.BZ_ParentTableCode = invoiceLine.TablePrefix;
		cusLineTariffDetailADT.BZ_ParentID = invoiceLine.PK;
		cusLineTariffDetailADT.BZ_Type = RateTypes.AdditionalTaxes;

		preparationFactory.Save();

		var testInvoiceLine = Factory.Load<JobComInvoiceLine>(invoiceLine.PK);

		CombineAssertions(() =>
		{
			AssertType<CusLineTariffDetailCollection>("Type of collection", testInvoiceLine.AdditionalFees);
			var addedAdditionalFee = testInvoiceLine.AdditionalFees.AddNew();
			AssertEquals("BZ_Type", UniversalReferenceConstants.RateTypes.AdditionalFees, addedAdditionalFee.BZ_Type);

			AssertEquals("FEE loaded", true, testInvoiceLine.AdditionalFees.Contains(cusLineTariffDetailFEE));
			AssertEquals("ADT not loaded", false, testInvoiceLine.AdditionalFees.Contains(cusLineTariffDetailADT));

			testInvoiceLine.Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			Factory.Save();

			AssertEquals("AdditionalFees should be deleted", 0, testInvoiceLine.AdditionalFees.Count);
			AssertNull("AdditionalFees should be deleted", Factory.Load<CusLineTariffDetail>(cusLineTariffDetailFEE.PK));
			AssertNull("AdditionalTaxes should be deleted", Factory.Load<CusLineTariffDetail>(cusLineTariffDetailADT.PK));
		});
	}

	public void TestAdditionalTaxes_Rebuild()
	{
		Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

		var additionalTaxTariffHelper = new AdditionalTaxTariffTestHelper(Factory);
		var parentTariff = additionalTaxTariffHelper.CreateParentImportTariff();
		additionalTaxTariffHelper.CreateAdditionalTaxTariffWithAdditionalCode();
		additionalTaxTariffHelper.CreateAdditionalTaxTariffWithRelationship(parentTariff);

		InvoiceLine.JI_CountryOfOrigin = ZString.Empty;
		InvoiceLine.JI_Tariff = parentTariff.ZZ1_TariffCode;
		AssertEquals("No AdditionalTaxes loaded due to JI_CountryOfOrigin is empty", 0, InvoiceLine.AdditionalTaxes.Count);

		InvoiceLine.JI_CountryOfOrigin = AdditionalTaxTariffTestHelper.DefaultTradeGroupCountry;
		AssertEquals("AdditionalTaxes rebuild on JI_CountryOfOrigin changed", 2, InvoiceLine.AdditionalTaxes.Count);

		InvoiceLine.JI_Tariff = ZString.Empty;
		AssertEquals("No AdditionalTaxes loaded due to JI_Tariff is empty", 0, InvoiceLine.AdditionalTaxes.Count);

		InvoiceLine.JI_CountryOfOrigin = AdditionalTaxTariffTestHelper.DefaultTradeGroupCountry;
		InvoiceLine.JI_Tariff = parentTariff.ZZ1_TariffCode;
		AssertEquals(2, InvoiceLine.AdditionalTaxes.Count);
		AssertEquals("AdditionalTaxes rebuild on JI_Tariff changed", 2, InvoiceLine.AdditionalTaxes.Count);
	}

	protected override Type ExpectedTypeOfApportionedCharges => typeof(JobComInvApportionedChargeCollection<InvoiceLineApportionCharge>);

	protected override Type ExpectedTypeOfCharges => typeof(JobComInvChargeCollection<InvoiceLineCharge>);

	public override void TestCustomsQtyCalculatedByNetWeightOfProductWhenInvoiceQtySet()
	{
		using (UnitConverter.TemporarySetupCachedConvertion(base.Factory))
		{
			Declaration.JE_MessageType = "IMP";
			InvoiceLine.JI_WeightUQ = Core.Constants.Weight.Grams;
			InvoiceLine.JI_Weight = 24000m;
			InvoiceLine.JI_NetWeightUQ = Core.Constants.Weight.Grams;
			InvoiceLine.JI_NetWeight = 12000m;

			Assertion.AssertEquals(24m, InvoiceLine.JI_CustomsQuantity);
			Assertion.AssertEquals(12m, InvoiceLine.JI_CustomsSecondQuantity);
		}
	}

	public void TestJI_CusNumber() => CombineAssertions(() =>
	{
		AssertEquals("Caption", "CUS-Code", DataBoundResourceStrings.GetDataForProperty(InvoiceLine.JI_CusNumberInfo).Caption);
		AssertEquals(10, InvoiceLine.JI_CusNumberInfo.MaxLength);
	});

	public void TestJI_RateOverride_Caption()
	{
		AssertEquals("Caption", "Rate Override", DataBoundResourceStrings.GetDataForProperty(InvoiceLine.JI_RateOverrideInfo).Caption);
	}

	public void TestJI_RateOverride_Type() => AssertType<ZBool>(InvoiceLine.JI_RateOverride);

	public void TestJI_RateOverride_Default()
	{
		AssertEquals($"{nameof(InvoiceLine.JI_RateOverride)} Default", false, InvoiceLine.JI_RateOverride);
	}

	public void TestJI_OverriddenRate()
	{
		AssertEquals("Caption", "Overridden Rate", DataBoundResourceStrings.GetDataForProperty(InvoiceLine.JI_OverriddenRateInfo).Caption);
	}

	public void TestOverriddenRateXML() => CombineAssertions(() =>
	{
		var tariffHelper = new RefCusTariffTestHelper(Factory);
		var tariff = tariffHelper.CreateImportTariffWithMultipleRates(RefCusTariffTestHelper.ImportTariffBeverages, rateFormulas: new[] { "1.2 * [KGM]" });

		Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		InvoiceLine.JI_CountryOfOrigin = RefCusTariffTestHelper.Country;
		InvoiceLine.JI_PrimaryPreference = UniversalReferenceConstants.PrimaryPreferenceCodes.PreferentialTariff;

		InvoiceLine.JI_RateOverride = true;

		InvoiceLine.JI_OverriddenRate = 1.5m;
		AssertEquals("Overridden Rate XML - empty uom", 1.5m, InvoiceLine.OverriddenRateXML);

		InvoiceLine.JI_Tariff = tariff.ZZ1_TariffCode;
		InvoiceLine.JI_OverriddenRate = 0.12m;
		AssertEquals("Overridden Rate XML - KGM", 12m, InvoiceLine.OverriddenRateXML);

		InvoiceLine.UniversalDutyRate.ZZ2_RateFormula = "1.2 * [KGMG]";
		InvoiceLine.JI_OverriddenRate = 0.13m;
		AssertEquals("Overridden Rate XML - KGMG", 13m, InvoiceLine.OverriddenRateXML);

		InvoiceLine.UniversalDutyRate.ZZ2_RateFormula = "1.2 * [MIL]";
		InvoiceLine.JI_OverriddenRate = 1.4m;
		AssertEquals("Overridden Rate XML - not KGM/KGMG", 1.4m, InvoiceLine.OverriddenRateXML);

		InvoiceLine.JI_RateOverride = false;
		AssertEquals("Overridden Rate XML - not overridden", 0m, InvoiceLine.OverriddenRateXML);
	});

	public void TestJI_RateOverride_Reset()
	{
		InvoiceLine.JI_RateOverride = true;
		InvoiceLine.JI_OverriddenRate = 1.5m;
		InvoiceLine.JI_RateOverride = false;

		AssertEquals("Reset Overridden Rate", 0m, InvoiceLine.JI_OverriddenRate);
	}

	public void TestInvoiceLinePackageValidationType()
	{
		var declaration = Factory.New<JobDeclaration>();
		var invoiceHeader = declaration.Invoices.AddNew();
		var package1 = declaration.Bills.AddNew().PackingGroups.AddNew().Packages.AddNew();
		var invoiceLine = Factory.New<JobComInvoiceLineForTest>();
		invoiceLine.JI_JZ = invoiceHeader.PK;

		var packing1 = invoiceLine.PackagesForInvoiceLinesForBindingOnly[0];
		AssertType<InvoiceLinePackageValidation>(invoiceLine.GetNewLinkPackValidationCore(packing1));
	}

	public void TestOnFactorySaving() => CombineAssertions(() =>
	{
		Declaration.JE_MessageType = CHJobMessageTypeList.Codes.Export;
		InvoiceLine.NotifyCustomsOffices.AddNew();
		Factory.Save();
		AssertEquals($"MessageType={Declaration.JE_MessageType}", 0, InvoiceLine.NotifyCustomsOffices.Count);

		Declaration.JE_MessageType = CHJobMessageTypeList.Codes.ExportDeclarationActivation;
		InvoiceLine.NotifyCustomsOffices.AddNew();
		Factory.Save();
		AssertEquals($"MessageType={Declaration.JE_MessageType}", 0, InvoiceLine.NotifyCustomsOffices.Count);

		Declaration.JE_MessageType = CHJobMessageTypeList.Codes.Import;
		InvoiceLine.NotifyCustomsOffices.AddNew();
		Factory.Save();
		AssertEquals($"MessageType={Declaration.JE_MessageType}", 1, InvoiceLine.NotifyCustomsOffices.Count);
	});

	public void TestJI_PermitObligation_OnFactorySaving() => CombineAssertions(() =>
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);

		const string Switzerland = Core.Constants.CountryCodes.Switzerland;
		const string tariffCode = "01012110000911";

		RefCusCodeTestHelper.CreatePermitObligationCodeList(Factory);

		Declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
		var tariffType = helper.CreateNewOrGetExistingTariffType(Switzerland, Universal.Constants.TariffTypes.Import);

		Factory.Save();

		new RefCusTariffTestHelper(Factory).CreateImportTariff(RefCusTariffTestHelper.ImportTariffBeverages);
		var tariff = helper.CreateTariff(Switzerland, tariffType.PK, tariffCode, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
		helper.CreateTariffAttribute(UniversalReferenceConstants.TariffAttributes.HasOptionalPermit, UniversalReferenceConstants.TariffAttributes.Values._1, tariff);

		Factory.Save();

		InvoiceLine.JI_Tariff = RefCusTariffTestHelper.ImportTariffBeverages;
		AssertPermitObligationWhenEmpty("Permit Count = 0, hasOptionalPermit null or 0, JI_PermitObligation should be: ", PermitObligationCodes.NoPermit);

		InvoiceLine.JI_Tariff = tariffCode;
		AssertPermitObligationWhenEmpty("Permit Count = 0, hasOptionalPermit == 1, JI_PermitObligation should be: ", PermitObligationCodes.NoPermitNeeded);

		InvoiceLine.Permits.AddNew();
		Factory.Save();
		AssertEquals("Permit Count > 0, JI_PermitObligation should be: ", PermitObligationCodes.PermitNeeded, InvoiceLine.JI_PermitObligation);

		void AssertPermitObligationWhenEmpty(string description, string expectedResult)
		{
			InvoiceLine.JI_PermitObligation = "5";
			Factory.Save();
			AssertEquals("JI_PermitObligation != null, value should not be updated", "5", InvoiceLine.JI_PermitObligation);

			InvoiceLine.JI_PermitObligation = null;
			Factory.Save();
			AssertEquals(description, expectedResult, InvoiceLine.JI_PermitObligation);
		}
	});

	public void TestJI_NonCustomsLawObligation_OnFactorySaving() => CombineAssertions(() =>
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		Declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;

		var notExcludedCountry = Core.Constants.CountryCodes.Italy;
		var excludedCountry = Core.Constants.CountryCodes.Vatican;
		var tariffTestHelper = new RefCusTariffTestHelper(Factory);
		var tariffFactory = GetTariffFactory();

		var tariffWithNoOptionalNCL = tariffFactory.Invoke(
			"10000000000000",
			Core.Constants.Customs.Universal.RefCusConditionTypes.ConditionClass.Control,
			"N123",
			CusConditionValueType.NonCustomsLaw,
			"123",
			null,
			null,
			null);
		var tariffWithHasOptionalNCL0 = tariffFactory.Invoke(
			"20000000000000",
			Core.Constants.Customs.Universal.RefCusConditionTypes.ConditionClass.Control,
			"N123",
			CusConditionValueType.NonCustomsLaw,
			"123",
			new[] { TariffAttributes.HasOptionalNCL, TariffAttributes.Values._0 },
			new[] { notExcludedCountry, excludedCountry },
			new[] { excludedCountry });
		var tariffWithHasOptionalNCL1 = tariffFactory.Invoke(
			"30000000000000",
			Core.Constants.Customs.Universal.RefCusConditionTypes.ConditionClass.Control,
			"N123",
			CusConditionValueType.NonCustomsLaw,
			"123",
			new[] { TariffAttributes.HasOptionalNCL, TariffAttributes.Values._1 },
			new[] { notExcludedCountry, excludedCountry },
			new[] { excludedCountry });

		InvoiceLine.JI_Tariff = tariffWithNoOptionalNCL.ZZ1_TariffCode;
		AssertNonCustomsLawObligation("NonCustomsLaws Count = 0, hasOptionalNCL null, JI_NonCustomsLawObligation should be: ", NonCustomsLawObligationCodes.NotPossible);

		InvoiceLine.JI_Tariff = tariffWithHasOptionalNCL0.ZZ1_TariffCode;
		AssertNonCustomsLawObligation("NonCustomsLaws Count = 0, hasOptionalNCL 0, JI_NonCustomsLawObligation should be: ", NonCustomsLawObligationCodes.NotPossible);

		InvoiceLine.JI_CountryOfOrigin = notExcludedCountry;
		InvoiceLine.JI_Tariff = tariffWithHasOptionalNCL1.ZZ1_TariffCode;
		AssertNonCustomsLawObligation("NonCustomsLaws Count = 0, hasOptionalNCL == 1, NotExcludedCountry, JI_NonCustomsLawObligation should be: ", NonCustomsLawObligationCodes.NotNeededAccordingDeclarant);

		InvoiceLine.JI_CountryOfOrigin = excludedCountry;
		AssertNonCustomsLawObligation("NonCustomsLaws Count = 0, hasOptionalNCL == 1, NotExcludedCountry, JI_NonCustomsLawObligation should be: ", NonCustomsLawObligationCodes.NotPossible);

		InvoiceLine.NonCustomsLaws.AddNew();
		Factory.Save();
		AssertEquals("NonCustomsLaws Count > 0, JI_NonCustomsLawObligation should be: ", NonCustomsLawObligationCodes.Needed, InvoiceLine.JI_NonCustomsLawObligation);

		Func<string, string, string, string, string, string[], string[], string[], TariffView>
	GetTariffFactory()
		{
			return (tariffCode, conditionClass, conditionType, valueType, conditionValue, attributes,
					countries, excludedCountries) =>
				tariffTestHelper.CreateImportTariffWithCondition(tariffCode, conditionClass, conditionType,
					valueType, conditionValue, attributes: attributes, countries: countries,
					excludedCountries: excludedCountries);
		}

		void AssertNonCustomsLawObligation(string description, string expectedResult)
		{
			InvoiceLine.JI_NonCustomsLawObligation = "5";
			Factory.Save();
			AssertEquals("JI_NonCustomsLawObligation != null, value should not be updated", "5", InvoiceLine.JI_NonCustomsLawObligation);

			InvoiceLine.JI_NonCustomsLawObligation = null;
			Factory.Save();
			AssertEquals(description, expectedResult, InvoiceLine.JI_NonCustomsLawObligation);
		}
	});

	public void TestHasAdditionalInformationA1301() => CombineAssertions(() =>
	{
		AssertEquals("false if any A1301 Additional info present", false, InvoiceLine.HasAdditionalInformationA1301);
		var addinfo = invoiceLine.AdditionalInformations.AddNew();
		addinfo.CSI_Code = UniversalReferenceConstants.AdditionalInformationTypeCodes.VocQuantityInKilograms;
		AssertEquals("false if any A1301 Additional info present", true, InvoiceLine.HasAdditionalInformationA1301);
	});

	public void TestHasAdditionalInformationA1102() => CombineAssertions(() =>
	{
		AssertEquals("Expected false if no A1102 Additional info provided", false, InvoiceLine.HasAdditionalInformationA1102);
		var addinfo = invoiceLine.AdditionalInformations.AddNew();
		addinfo.CSI_Code = UniversalReferenceConstants.AdditionalInformationTypeCodes.AlcoholOnBeerRefundLiters;
		AssertEquals("Expected true if any A1102 Additional info provided", true, InvoiceLine.HasAdditionalInformationA1102);
	});

	public void TestIsTabaccoRefundType() => CombineAssertions(() =>
	{
		InvoiceLine.JI_RefundType = UniversalReferenceConstants.RefundType.TobaccoProductsExTaxWarehouse;
		AssertEquals("true if JI_RefundType equals to 7", true, InvoiceLine.IsTobaccoRefundType);
		InvoiceLine.JI_RefundType = UniversalReferenceConstants.RefundType.TobaccoTaxRefund;
		AssertEquals("true if JI_RefundType equals to 6", true, InvoiceLine.IsTobaccoRefundType);
		InvoiceLine.JI_RefundType = UniversalReferenceConstants.RefundType.Refund;
		AssertEquals("false if JI_RefundType not equals to 6 or 7", false, InvoiceLine.IsTobaccoRefundType);
	});

	public void TestHasAnySOTAAdditionalTax() => CombineAssertions(() =>
	{
		AssertEquals("Expected false if no SOTA Additional Tax is there", false, InvoiceLine.HasAnySOTAAdditionalTax);
		var addinfo = invoiceLine.AdditionalTaxes.AddNew();
		addinfo.BZ_Tariff = UniversalReferenceConstants.AdditionalTaxesTariffs.Tariff465002;
		AssertEquals("Expected true if SOTA Additional Tax is there", true, InvoiceLine.HasAnySOTAAdditionalTax);
	});

	public void TestHasAnyPreventionAdditionalTax() => CombineAssertions(() =>
	{
		AssertEquals("Expected false if no Prevention Additional Tax is there", false, InvoiceLine.HasAnyPreventionAdditionalTax);
		var addinfo = invoiceLine.AdditionalTaxes.AddNew();
		addinfo.BZ_Tariff = UniversalReferenceConstants.AdditionalTaxesTariffs.Tariff470002;
		AssertEquals("Expected true if Prevention Additional Tax is there", true, InvoiceLine.HasAnyPreventionAdditionalTax);
	});

	public void TestHasAnyTobaccoSubGroup02or03() => CombineAssertions(() =>
	{
		AssertEquals("Expected false if no Tobacco with subtype 02 or 03 is there", false, InvoiceLine.HasAnyTobaccoSubGroup02or03);
		var tobacco = invoiceLine.Tobaccos.AddNew();
		tobacco.CSI_Code = UniversalReferenceConstants.TobaccoMainGroupCodes.CutTobacco;
		tobacco.CSI_SubType = UniversalReferenceConstants.TobaccoSubGroupCodes._02;
		AssertEquals("Expected true if Tobacco with subtype 02 is there", true, InvoiceLine.HasAnyTobaccoSubGroup02or03);
		tobacco.CSI_SubType = UniversalReferenceConstants.TobaccoSubGroupCodes._03;
		AssertEquals("Expected true if Tobacco with subtype 03 is there", true, InvoiceLine.HasAnyTobaccoSubGroup02or03);
	});

	public void TestIsOverriddenRateZero() => CombineAssertions(() =>
	{
		AssertIsOverriddenRateZero(false, ZBool.False, 0);
		AssertIsOverriddenRateZero(false, ZBool.False, 1);
		AssertIsOverriddenRateZero(true, ZBool.True, 0);
		AssertIsOverriddenRateZero(false, ZBool.True, 1);

		void AssertIsOverriddenRateZero(bool expectedValue, ZBool rateOverride, ZDecimal overriddenRate)
		{
			InvoiceLine.JI_RateOverride = rateOverride;
			InvoiceLine.JI_OverriddenRate = overriddenRate;
			AssertEquals($"JI_RateOverride={rateOverride} JI_OverriddenRate={overriddenRate}", expectedValue, InvoiceLine.IsOverriddenRateZero);
		}
	});

	public void TestJI_RefundTypeMaxLength() => AssertEquals(1, InvoiceLine.JI_RefundTypeInfo.MaxLength);

	public void TestJI_OverridenRateType() => AssertType<ZDecimal>(InvoiceLine.JI_OverriddenRate);

	public void TestSetRefundInfoPropertiesToEmpty()
	{
		Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		InvoiceLine.JI_RefundType = UniversalReferenceConstants.RefundType.ReturnedGoodsWithRefundRequest;

		InvoiceLine.JI_RefundReferenceNumber = "24CH12345678901238";
		InvoiceLine.JI_RefundGoodsItemNumber = 1;
		InvoiceLine.JI_RefundReason = "reason text";

		InvoiceLine.JI_RefundType = UniversalReferenceConstants.RefundType.Refund;

		CombineAssertions(() =>
		{
			AssertEquals("GDRN empty", ZString.Empty, InvoiceLine.JI_RefundReferenceNumber);
			AssertEquals("Goods Item Number 0", 0, InvoiceLine.JI_RefundGoodsItemNumber);
			AssertEquals("Refund Reason", ZString.Empty, InvoiceLine.JI_RefundReason);
		});
	}

	public void TestVehiclesCollection()
	{
		AssertType<CusVehicleCollection>("Type", InvoiceLine.Vehicles);
	}

	public void TestUNDGCodes() => CombineAssertions(() =>
	{
		CaptionTestHelper.AssertCaptions(InvoiceLine.UNDGCodesInfo, caption: "UN Dangerous Codes", mediumCaption: "UNDG Codes", shortCaption: "UNDG");

		var undgSubstance1 = Factory.New<UNDGSubstance>();
		undgSubstance1.DG_UNNO = "1001";
		var undgSubstance2 = Factory.New<UNDGSubstance>();
		undgSubstance2.DG_UNNO = "1002";

		AssertEquals("DgSubstance field is empty", ZString.Empty, InvoiceLine.UNDGCodes);

		invoiceLine.UNDGs.AddNew().DI_DG = undgSubstance2.PK;
		AssertEquals("DgSubstance field one substance", "1002", InvoiceLine.UNDGCodes);

		invoiceLine.UNDGs.AddNew().DI_DG = undgSubstance1.PK;
		AssertContainsExactElementsInAnyOrder("DgSubstance field two substances", new[] { "1001", "1002" }, InvoiceLine.UNDGCodes.Split(','));
	});

	public void TestUNDGCodes_RefreshBinding()
	{
		var refreshBindingCalled = false;
		InvoiceLine.UNDGCodesInfo.ValueChanged += (s, e) => refreshBindingCalled = true;

		InvoiceLine.UNDGs.AddNew();
		AssertEquals(true, refreshBindingCalled);
	}

	public void TestUNDGs_NS30162_MaxCount() => CombineAssertions(() =>
	{
		var collection = (ISupportMaxCountValidation)InvoiceLine.UNDGs;
		AssertEquals("MaxCount", 99, collection.MaxCountValidator.MaxCount);
		AssertEquals("Notification.Type", NotificationType.MessageError, collection.MaxCountValidator.Notification.Type);
		AssertEquals("Notification.Message", PassarValidationMessages.MessageCH0006, collection.MaxCountValidator.Notification.Message);
	});

	public void TestCurrencyConverter()
	{
		Declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Export;
		AssertEquals("EXP Declaration without Valuation Date: today", ZDateTime.Today, InvoiceLine.CurrencyConverter.DateForRate);
		AssertEquals("EXP CurrencyConverter RateType", ExchangeRateType.Customs, InvoiceLine.CurrencyConverter.RateType);

		Declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
		AssertEquals("IMP Declaration without Valuation Date: today", ZDateTime.Today, InvoiceLine.CurrencyConverter.DateForRate);
		AssertEquals("IMP CurrencyConverter RateType", ExchangeRateType.Customs, InvoiceLine.CurrencyConverter.RateType);

		Declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Export;
		var valuationDate = new ZDate(2024, 12, 31);
		Declaration.JE_ValuationDate = valuationDate;
		AssertEquals("EXP Declaration with Valuation Date: 31.12.2024", valuationDate, InvoiceLine.CurrencyConverter.DateForRate);

		Declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
		AssertEquals("IMP Declaration with Valuation Date: 31.12.2024", valuationDate, InvoiceLine.CurrencyConverter.DateForRate);

		var acceptanceDate = new ZDate(2025, 01, 01);
		var entryInstruction = Declaration.CustomsEntryInstructions.AddNew();
		InvoiceLine.JI_CEI = entryInstruction.PK;
		entryInstruction.CEI_DateForDuty = acceptanceDate;
		AssertEquals("IMP Declaration with Acceptance Date: 01.01.2025", acceptanceDate, InvoiceLine.CurrencyConverter.DateForRate);

		Declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Export;
		AssertEquals("EXP Declaration with Valuation Date: 31.12.2024 / existing IMP Acceptance Date", valuationDate, InvoiceLine.CurrencyConverter.DateForRate);
	}

	protected override void DoMerge(BaseJobDeclaration declaration)
	{
		SetupDataEligibleForMerging(declaration);
		base.DoMerge(declaration);
	}

	void SetupDataEligibleForMerging(BaseJobDeclaration declaration)
	{
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
	}

	protected override BaseJobDeclaration GetJobDeclaration()
	{
		return Factory.New<JobDeclaration>();
	}

	new JobDeclaration Declaration => declaration ??= Factory.New<JobDeclaration>();
	new JobComInvoiceHeader InvoiceHeader => invoiceHeader ??= Declaration.Invoices.AddNew();
	new JobComInvoiceLine InvoiceLine => invoiceLine ??= InvoiceHeader.InvoiceLines.AddNew();

	internal JobDeclaration declaration;
	internal JobComInvoiceHeader invoiceHeader;
	internal JobComInvoiceLine invoiceLine;

	void AssertNetDutyReadOnlyStatus(ZPropertyInfo property, string messageType, bool expectedValueIfNetDutyIsTicked)
	{
		Declaration.JE_MessageType = messageType;
		CombineAssertions($"{property.Name}", () =>
		{
			AssertEquals($"NetDuty={InvoiceLine.NetDuty} -> ReadOnly=true", true, property.ReadOnly);
			InvoiceLine.NetDuty = true;
			AssertEquals($"NetDuty={InvoiceLine.NetDuty} -> ReadOnly={expectedValueIfNetDutyIsTicked}", expectedValueIfNetDutyIsTicked, property.ReadOnly);
		});
	}

	sealed class JobComInvoiceLineForTest : JobComInvoiceLine
	{
		public JobComInvoiceLineForTest(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{ }

		public new Customs.Business.JobComInvoiceLineValidation GetNewValidation() => base.GetNewValidation();

		public new Customs.Business.InvoiceLinePackageValidation GetNewLinkPackValidationCore(BaseCusLinkPackage linkPackage) => base.GetNewLinkPackValidationCore(linkPackage);
	}
}
