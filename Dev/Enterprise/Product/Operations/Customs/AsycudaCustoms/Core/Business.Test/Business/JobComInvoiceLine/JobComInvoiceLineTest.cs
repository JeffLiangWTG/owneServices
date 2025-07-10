using System;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.AsycudaCustoms.Business.Testing
{
	[TestedType(typeof(JobComInvoiceLine))]
	class JobComInvoiceLineTest : Customs.Business.Testing.BaseJobComInvoiceLineAbstractTest
	{
		public void TestCustomsCountryCode()
		{
			AssertEquals("CustomsCountryCodeCore should be current login country", GlbCompany.CurrentCompany.GC_RN_NKCountryCode, InvoiceLine.CustomsCountryCode);
		}

		public void TestEffectiveAssessmentDate()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var invoice = declaration.Invoices.AddNew();
			var invoiceDate = new ZDateTime(2021, 2, 1);
			invoice.JZ_ValuationDateOverride = invoiceDate;
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;

			CombineAssertions(() =>
			{
				var instructionDate = new ZDateTime(2021, 10, 13);
				entryInstruction.CEI_DateForDuty = instructionDate;
				AssertEquals("CEI_DateForDuty", instructionDate, invoiceLine.EffectiveAssessmentDate);

				entryInstruction.CEI_DateForDuty = ZDateTime.Empty;
				AssertEquals("Fall back Today when CEI_DateForDuty not valid", invoiceDate, invoiceLine.EffectiveAssessmentDate);

				var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
				AssertEquals("Fall back Today when no linked entryInstruction", invoiceDate, invoiceLine2.EffectiveAssessmentDate);
			});
		}

		public void TestVehicleVIN_Caption()
		{
			AssertEquals("VIN", DataBoundResourceStrings.GetDataForProperty(Factory.New<JobComInvoiceLine>().VehicleVINInfo).Caption);
		}

		public void TestIInvoiceLinePartDetailsMembers()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				IInvoiceLinePartDetails partDetails = Factory.New<JobComInvoiceLine>();
				AssertEquals(typeof(OrgSupplierPart), partDetails.TypeOfPartUsed);
			}
		}

		public void TestVehicleVIN()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			AssertEquals("", invoiceLine.VehicleVIN);
			invoiceLine.VehicleVIN = "VIN123";
			AssertEquals("VIN123", invoiceLine.VehicleVIN);
			AssertEquals("VIN123", invoiceLine.FirstVehicle.CVH_VehicleIdentificationNumber);
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
			var bwCompany = Factory.New<GlbCompany>();
			bwCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Botswana;
			var bwBranch = bwCompany.Branches.AddNew();
			bwBranch.GB_RL_NKHomePort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, Core.Constants.CountryCodes.Botswana)).RL_Code;
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_GB = bwBranch.PK;
			var invoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			invoiceLine.JI_PartNo = "TestTEST";
			AssertEquals("Product type gets changed depending on who is requesting", typeof(OrgSupplierPart), invoiceLine.Part.GetType());
			Factory.Save();
			GlbCompany.CurrentCompany.SetCountry("AU");
			var factory3 = new BusinessObjectFactory();
			var declarationLoaded = factory3.Load<JobDeclaration>(declaration.PK);
			AssertEquals("product type still the type", typeof(OrgSupplierPart), declarationLoaded.InvoiceLines[0].Part.GetType());
		}

		public void TestDefaultUniversalTariffProperties()
		{
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			AssertEquals("UseUniversalTariff", true, typeof(JobComInvoiceLine).GetProperty("UseUniversalTariffCore", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(invoiceLine));
		}

		public void TestUniversalTariffType()
		{
			var company = Factory.New<GlbCompany>();
			company.GC_Code = "CBD";
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Bangladesh;
			var branch = company.Branches.AddNew();
			branch.GB_Code = "BBD";
			Declaration.JE_GB = branch.PK;
			var invoice = Declaration.Invoices.AddNew();
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			invoiceLine.JI_JZ = invoice.PK;
			AssertEquals(Universal.Constants.TariffTypes.HarmonizedSystem, invoiceLine.UniversalTariffType);
			var company2 = Factory.New<GlbCompany>();
			company2.GC_Code = "CBW";
			company2.GC_RN_NKCountryCode = Enterprise.Core.Constants.CountryCodes.Botswana;
			var branch2 = company2.Branches.AddNew();
			branch2.GB_Code = "BBW";
			Declaration.JE_GB = branch2.PK;
			invoice.JZ_GB = branch2.PK;
			AssertEquals(Constants.CusTariffCode.Schedule1Part1, invoiceLine.UniversalTariffType);
		}

		public new void TestDefaultDataGroupingCode()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var parentDataGrouping1 = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.CommonDataGrouping, "ZZ");
			helper.CreateNewOrGetExistingDataGrouping("NA", "Namibia", parentDataGrouping1);
			var parentDataGrouping2 = helper.CreateNewOrGetExistingDataGrouping("WCO", "World Trade Organization (WCO)");
			helper.CreateNewOrGetExistingDataGrouping("BD", "Bangladesh", parentDataGrouping2);
			Factory.Save();
			var company = Factory.New<GlbCompany>();
			company.GC_Code = "CBD";
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Bangladesh;
			var branch = company.Branches.AddNew();
			branch.GB_Code = "BBD";
			Declaration.JE_GB = branch.PK;
			var invoice = Declaration.Invoices.AddNew();
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			invoiceLine.JI_JZ = invoice.PK;
			AssertEquals(Core.Constants.CountryCodes.Bangladesh, invoiceLine.GetDefaultDataGroupingCode());
			AssertEquals(Core.Constants.Customs.Universal.RefDataGrouping.Codes.WorldCustomsOrganisationWCO, RefDataGrouping.GetParentDataGrouping(Factory, invoiceLine.GetDefaultDataGroupingCode()).ZZZ_DataGrouping);
			var company2 = Factory.New<GlbCompany>();
			company2.GC_Code = "CBW";
			company2.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Namibia;
			var branch2 = company2.Branches.AddNew();
			branch2.GB_Code = "BBW";
			Declaration.JE_GB = branch2.PK;
			AssertEquals(Core.Constants.CountryCodes.Namibia, invoiceLine.GetDefaultDataGroupingCode());
			AssertEquals(Core.Constants.Customs.Universal.RefDataGrouping.Codes.CommonDataGrouping, RefDataGrouping.GetParentDataGrouping(Factory, invoiceLine.GetDefaultDataGroupingCode()).ZZZ_DataGrouping);
		}

		public void TestTypeDecider()
		{
			Assert("Update Customs.Business.BaseJobComInvoiceLine to include a decider for this class", Factory.New(typeof(BaseJobComInvoiceLine)).GetType() == GetExpectedBusinessObjectType());
		}

		public void TestSetDefaultTaxOrFeeCode_ManualTariff()
		{
			var startDate = ZDateTime.Today.AddDays(-2);
			var endDate = ZDateTime.Today.AddDays(2);
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = UniversalReferenceTestDataHelper.CreateNewOrGetExistingRefCusTariffType(Factory, Core.Constants.CountryCodes.Botswana, Constants.CusTariffCode.Schedule1Part1, ensureDataGroupingExists: false);
			helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Botswana, tariffType.PK, "4321", startDate, endDate, taxOrFeeCode: "ZZR", ensureDataGroupingExists: false, isSystem: false);

			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "4321";
			AssertEquals("JI_ZZF_NKTaxType comes from UniversalTariff.ZZ1_ZZF_NKTaxOrFeeCode", "ZZR", invoiceLine.JI_ZZF_NKTaxType);
		}

		public void TestSetDefaultTaxOrFeeCode__ZZTariffWithoutVATApplicability()
		{
			var startDate = ZDateTime.Today.AddDays(-2);
			var endDate = ZDateTime.Today.AddDays(2);
			var countryCode = GlbCompany.CurrentCompany.Country.Code;

			var tariffDataGrouping = TariffDataGrouping;
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var parentDataGrouping = helper.CreateNewOrGetExistingDataGrouping(tariffDataGrouping, "parent DataGrouping");
			helper.CreateNewOrGetExistingDataGrouping(countryCode, "Current country", parentDataGrouping);

			var tariffTypePK = helper.CreateNewOrGetExistingTariffType(tariffDataGrouping, UniversalTariffTypeForDefaultTaxOrFeeCode).PK;
			Factory.Save();
			var tariff = helper.CreateTariff(tariffDataGrouping, tariffTypePK, "99999999", startDate, endDate, taxOrFeeCode: "ZZT");

			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = tariff.ZZ1_TariffCode;

			AssertEquals("JI_ZZF_NKTaxType comes from UniversalTariff.ZZ1_ZZF_NKTaxOrFeeCode", "ZZT", invoiceLine.JI_ZZF_NKTaxType);
		}

		public void TestSetDefaultTaxOrFeeCode_ZZTariffWithTwoVATApplicability()
		{
			var startDate = ZDateTime.Today.AddDays(-2);
			var endDate = ZDateTime.Today.AddDays(2);
			var countryCode = GlbCompany.CurrentCompany.Country.Code;

			var tariffDataGrouping = TariffDataGrouping;
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var parentDataGrouping = helper.CreateNewOrGetExistingDataGrouping(tariffDataGrouping, "parent DataGrouping");
			helper.CreateNewOrGetExistingDataGrouping(countryCode, "Current country", parentDataGrouping);

			var tariffTypePK = helper.CreateNewOrGetExistingTariffType(tariffDataGrouping, UniversalTariffTypeForDefaultTaxOrFeeCode).PK;
			var taxOrFee = helper.CreateTaxOrFee("ZZ1", 0.02m, countryCode);
			taxOrFee.ZZF_ZX0_NKTaxOrFeeType = "VAT";
			var taxOrFee2 = helper.CreateTaxOrFee("ZZ2", 0.04m, countryCode);
			taxOrFee2.ZZF_ZX0_NKTaxOrFeeType = "VAT";

			Factory.Save();
			var tariff = helper.CreateTariff(tariffDataGrouping, tariffTypePK, "99999999", startDate, endDate);

			helper.CreateNewOrGetExistingVATApplicability(tariff, countryCode, "ZZ1", "AdditionalCode1", startDate, endDate);
			helper.CreateNewOrGetExistingVATApplicability(tariff, countryCode, "ZZ2", "AdditionalCode2", startDate, endDate);

			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = tariff.ZZ1_TariffCode;

			AssertEquals("JI_ZZF_NKTaxType comes from VATApplicability of UniversalTariff", "ZZ2", invoiceLine.JI_ZZF_NKTaxType);
		}

		public void TestJI_TariffMaxLength()
		{
			var line = Factory.New<JobComInvoiceLine>();
			AssertEquals("Max length of JI_Tariff is 35", 35, line.JI_TariffInfo.MaxLength);
		}

		public override void TestJI_FormattedTariff()
		{
			ZString tariff = "12345678";
			InvoiceLine.JI_Tariff = tariff;
			AssertEquals("JI_FormattedTariff", "1234.56.78", InvoiceLine.JI_FormattedTariff);
		}

		public void TestCusSupportingInfo()
		{
			var supportingDocumentsProvider = (ISupportingDocumentsProvider)Factory.NewWithValidTestData<JobDeclaration>().Invoices.AddNew().InvoiceLines.AddNew();
			var supportingDocuments = supportingDocumentsProvider.SupportingDocuments;
			var supportingDocument1 = supportingDocuments.AddNew();
			Assert("CSI_Type=SUP & CSI_ParentID=invoceLine.PK", supportingDocument1.CSI_Type == "SUP" && supportingDocument1.CSI_ParentID == supportingDocumentsProvider.PK);
			supportingDocument1.CSI_Code = "CD1";
			supportingDocument1.CSI_ReferenceNumber = "REF001";
			supportingDocument1.CSI_AdditionalDescription = "Additional Description";
			Factory.Save();
			var reloadedObject = new BusinessObjectFactory().LoadTop1<CusSupportingInfo>(new ZQuery(CusSupportingInfoSchema.PK, supportingDocument1.PK));
			AssertEquals("CSI_Type", "SUP", reloadedObject.CSI_Type);
			AssertEquals("CSI_ParentID", supportingDocumentsProvider.PK, reloadedObject.CSI_ParentID);
			AssertEquals("CSI_ReferenceNumber", "REF001", reloadedObject.CSI_ReferenceNumber);
			AssertEquals("CSI_AdditionalDescription", "Additional Description", reloadedObject.CSI_AdditionalDescription);
		}

		public void TestCustomsUnitDefaultingStrategy()
		{
			SetupCustomsUnitDefaultingStrategyTestData();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;

			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			var strategy = typeof(BaseJobComInvoiceLine).GetProperty("CustomsUnitDefaultingStrategy", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(invoiceLine);
			invoiceLine.JI_Tariff = "1234";

			CombineAssertions(() =>
			{
				AssertType<UniversalRateCustomsUnitDefaultingStrategy<JobComInvoiceLine>>("Should apply UniversalRateCustomsUnitDefaultingStrategy", strategy);
				AssertNotNull(((UniversalRateCustomsUnitDefaultingStrategy<JobComInvoiceLine>)strategy).IsConvertibleFrom);
				AssertEquals("Should default UQ1", "KGX", invoiceLine.JI_CustomsUnitQty);
				AssertEquals("Should default UQ2", "LTX", invoiceLine.JI_CustomsSecondUnitQty);
				AssertEquals("Should default UQ3", "HLT", invoiceLine.JI_CustomsThirdUnitQty);
			});
		}

		void SetupCustomsUnitDefaultingStrategyTestData()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(
				Core.Constants.CountryCodes.Botswana,
				Constants.CusTariffCode.Schedule1Part1
			);
			Factory.Save();
			var tariff = helper.LoadOrCreateNewTariff(
				Core.Constants.CountryCodes.Botswana,
				tariffType.PK,
				"1234",
				ZDateTime.Now.AddYears(-1),
				ZDateTime.Now.AddYears(1)
			);
			helper.CreateTariffUOM(tariff, Universal.Constants.UnitOfMeasureTypes.StatisticalUOMType, "KGX");
			helper.CreateTariffUOM(tariff, Universal.Constants.UnitOfMeasureTypes.AdditionalUOMType, "LTX");
			helper.CreateTariffUOM(tariff, Universal.Constants.UnitOfMeasureTypes.CustomsUOM3Type, "HLT");
		}

		protected override ZString TariffDataGrouping => Core.Constants.Customs.Universal.RefDataGrouping.Codes.CommonDataGrouping;
		protected override ZString UniversalTariffTypeForDefaultTaxOrFeeCode => Constants.CusTariffCode.Schedule1Part1;

		protected new JobDeclaration Declaration => (JobDeclaration)base.Declaration;

		protected new JobComInvoiceHeader InvoiceHeader => (JobComInvoiceHeader)base.InvoiceHeader;

		protected new JobComInvoiceLine InvoiceLine => (JobComInvoiceLine)base.InvoiceLine;

		protected override Type ExpectedTypeOfApportionedCharges => typeof(JobComInvApportionedChargeCollection<InvoiceLineApportionCharge>);

		protected override Type ExpectedTypeOfCharges => typeof(JobComInvChargeCollection<InvoiceLineCharge>);
	}

	class InvoiceLinePartClassificationTariffDescriptionSyncroniserTest : Customs.Business.Testing.InvoiceLinePartClassificationTariffDescriptionSyncroniserAbstractTest
	{
		public override void TestDescriptionOnTarrifWhenMerged()
		{
			Assert("For AsycudaCustoms, invoiceLine.JI_Description is used as merge key, see GetKeyForLine in EntryCreationStrategy", true);
		}

		protected override ZString TariffCode => "00000000";

		protected override ZString TariffCode2 => "00000000";

		protected override ZString TariffDescription => TariffDescriptionCore;
		internal const string TariffDescriptionCore = "";

		protected override ZString TariffDescription2 => TariffDescriptionCore2;
		internal const string TariffDescriptionCore2 = "";

		protected override Type DeclarationTypeForTest => typeof(JobDeclaration);
	}
}
