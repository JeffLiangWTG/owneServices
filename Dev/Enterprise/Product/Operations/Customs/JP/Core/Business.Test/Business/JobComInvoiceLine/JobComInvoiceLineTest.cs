using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Business.Testing;

[TestedType(typeof(JobComInvoiceLine))]
sealed class JobComInvoiceLineTest : Customs.Business.Testing.BaseJobComInvoiceLineAbstractTest
{
	public void TestJI_Description()
	{
		AssertEquals(40, InvoiceLine.JI_DescriptionInfo.MaxLength);
	}

	public void TestJI_DutyReductionExemptionRefundCodeCaption()
	{
		Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		AssertEquals("Duty Reduction Code", InvoiceLine.JI_DutyReductionExemptionRefundCodeInfo.HumanReadableName);

		Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		AssertEquals("Duty Refund Code", InvoiceLine.JI_DutyReductionExemptionRefundCodeInfo.HumanReadableName);
	}

	public void TestUpdateDetailsFromPivotOnPartChangeCore()
	{
		var supplier = Factory.New<OrgHeader>();
		supplier.OH_Code = "Supplier01";

		var part = Factory.New<OrgSupplierPart>();
		part.OP_PartNum = "NEWPROD1";
		part.OP_Desc = "PRODUCT1";
		part.OP_StockKeepingUnit = "BAG";
		part.OP_PartNum = "NEWPROD1";
		part.OP_Desc = "PRODUCT1";
		part.OP_Brand = "Apple";
		part.OP_Model = "Phone";
		var relatedOrganization = part.RelatedOrganisations.AddNew();
		relatedOrganization.OU_OH = supplier.PK;
		relatedOrganization.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;

		var pivot = part.PivotsForBinding.AddNew() as CusClassPartPivot;
		var lookups = pivot.Lookups;
		pivot.CI_ChildType = ClassificationTypeList.Codes.HTB;
		pivot.CI_RN_NKCountry = Core.Constants.CountryCodes.Japan;
		pivot.CI_DateStart = DateTime.Now.AddDays(-1);
		pivot.CI_DateEnd = DateTime.Now.AddDays(1);

		pivot.CI_Description = "TEST DESC";
		pivot.CI_RN_NKCountryOfOrigin = "JP";
		pivot.CI_PrimaryPreference = "1234567890";
		pivot.CI_SecondaryPreference = "1234567";
		pivot.CI_PrimaryPreference = "1";
		pivot.CI_TradeControlOrderAppendix = "1";
		pivot.CI_FEFTAArticle48 = lookups.FEFTAArticle48List[0].Code;
		pivot.CI_StorageType = lookups.StorageTypeList[0].Code;
		pivot.CI_AdvanceRulingOnClassification = "A";
		pivot.CI_AdvanceRulingOnOrigin = "B";
		pivot.CI_DutyReductionExemptionRefundCode = "C";
		pivot.CI_DomesticConsumptionTaxExemptionCode = "D";
		pivot.CI_DomesticConsumptionTaxExemptionIsPartial = true;
		pivot.CI_DutyReductionAmount = 618;

		Factory.Save();

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		var header = declaration.Invoices.AddNew();
		var invoiceLine = header.InvoiceLines.AddNew();
		invoiceLine.JI_PartNo = part.OP_PartNum;
		header.JZ_OH_Supplier = supplier.PK;

		CombineAssertions(() =>
		{
			AssertEquals("JI_Description", pivot.CI_Description, invoiceLine.JI_Description);
			AssertEquals("JI_CountryOfOrigin", pivot.CI_RN_NKCountryOfOrigin, invoiceLine.JI_CountryOfOrigin);
			AssertEquals("JI_PrimaryPreference", pivot.CI_PrimaryPreference, invoiceLine.JI_PrimaryPreference);
			AssertEquals("JI_SecondaryPreference", pivot.CI_SecondaryPreference, invoiceLine.JI_SecondaryPreference);
			AssertEquals("JI_TradeControlOrderAppendix", pivot.CI_TradeControlOrderAppendix, invoiceLine.JI_TradeControlOrderAppendix);
			AssertEquals("JI_FEFTAArticle48", pivot.CI_FEFTAArticle48, invoiceLine.JI_FEFTAArticle48);
			AssertEquals("JI_AdvanceRulingOnClassification", pivot.CI_AdvanceRulingOnClassification, invoiceLine.JI_AdvanceRulingOnClassification);
			AssertEquals("JI_AdvanceRulingOnOrigin", pivot.CI_AdvanceRulingOnOrigin, invoiceLine.JI_AdvanceRulingOnOrigin);
			AssertEquals("JI_DutyReductionExemptionRefundCode", pivot.CI_DutyReductionExemptionRefundCode, invoiceLine.JI_DutyReductionExemptionRefundCode);
			AssertEquals("JI_DomesticConsumptionTaxExemptionCode", pivot.CI_DomesticConsumptionTaxExemptionCode, invoiceLine.JI_DomesticConsumptionTaxExemptionCode);
			AssertEquals("JI_DomesticConsumptionTaxExemptionIsPartial", pivot.CI_DomesticConsumptionTaxExemptionIsPartial, invoiceLine.JI_DomesticConsumptionTaxExemptionIsPartial);
			AssertEquals("JI_DutyReductionAmount", pivot.CI_DutyReductionAmount, invoiceLine.JI_DutyReductionAmount);
		});
	}

	public void TestSetDafaultValues()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		var jobComInvoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();

		AssertEquals(JobComInvoiceLine.DefaultPreference, jobComInvoiceLine.JI_Calc_Preference);
	}

	public void TestDomesticConsumptionTaxExemptionType()
	{
		InvoiceLine.JI_DomesticConsumptionTaxExemptionCode = string.Empty;
		InvoiceLine.JI_DomesticConsumptionTaxExemptionIsPartial = true;
		AssertEquals(string.Empty, InvoiceLine.DomesticConsumptionTaxExemptionType);

		InvoiceLine.JI_DomesticConsumptionTaxExemptionCode = "C";
		AssertEquals("P", InvoiceLine.DomesticConsumptionTaxExemptionType);

		InvoiceLine.JI_DomesticConsumptionTaxExemptionIsPartial = false;
		AssertEquals("A", InvoiceLine.DomesticConsumptionTaxExemptionType);
	}

	public void TestJI_NACCSCode_ReadOnly()
	{
		var declaration = Factory.New<JobDeclaration>();
		var instruction = declaration.CustomsEntryInstructions.AddNew();
		var jobComInvoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		jobComInvoiceLine.JI_CEI = instruction.PK;
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		instruction.CEI_Style = JPImportDeclarationTypeList.Codes.Y;

		Assert(jobComInvoiceLine.JI_NACCSCodeInfo.ReadOnly);

		instruction.CEI_Style = JPImportDeclarationTypeList.Codes.B;
		Assert(!jobComInvoiceLine.JI_NACCSCodeInfo.ReadOnly);

		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		instruction.CEI_Style = "Y";
		Assert(!jobComInvoiceLine.JI_NACCSCodeInfo.ReadOnly);
	}

	public void TestSetNACCSCodeWhenJI_TariffIsUpdated()
	{
		var declaration = Factory.New<JobDeclaration>();
		var jobComInvoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();

		jobComInvoiceLine.JI_Tariff = "220890001";
		AssertEquals("3", jobComInvoiceLine.JI_NACCSCode);
	}

	public void TestJI_StorageTypeVisible()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		var jobComInvoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		var instruction = declaration.CustomsEntryInstructions.AddNew();
		jobComInvoiceLine.JI_CEI = instruction.PK;

		declaration.JE_TransportMode = TransportTypeList.Codes.Air;
		foreach (var declarationType in new JPImportDeclarationTypeList().GetAllCodes())
		{
			instruction.CEI_Style = declarationType;
			switch (declarationType)
			{
				case JPImportDeclarationTypeList.Codes.S:
				case JPImportDeclarationTypeList.Codes.M:
				case JPImportDeclarationTypeList.Codes.A:
				case JPImportDeclarationTypeList.Codes.G:
					AssertEquals($"Visibilty for Air, type {declarationType}", true, jobComInvoiceLine.JI_StorageTypeVisible);
					break;
				default:
					AssertEquals($"Visibilty for Air, type {declarationType}", false, jobComInvoiceLine.JI_StorageTypeVisible);
					break;
			}
		}
		declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
		foreach (var declarationType in new JPImportDeclarationTypeList().GetAllCodes())
		{
			instruction.CEI_Style = declarationType;
			switch (declarationType)
			{
				case JPImportDeclarationTypeList.Codes.C:
				case JPImportDeclarationTypeList.Codes.F:
				case JPImportDeclarationTypeList.Codes.S:
				case JPImportDeclarationTypeList.Codes.M:
				case JPImportDeclarationTypeList.Codes.A:
				case JPImportDeclarationTypeList.Codes.G:
				case JPImportDeclarationTypeList.Codes.K:
				case JPImportDeclarationTypeList.Codes.D:
				case JPImportDeclarationTypeList.Codes.U:
				case JPImportDeclarationTypeList.Codes.L:
				case JPImportDeclarationTypeList.Codes.B:
				case JPImportDeclarationTypeList.Codes.E:
				case JPImportDeclarationTypeList.Codes.R:
					AssertEquals($"Visibilty for Sea, type {declarationType}", true, jobComInvoiceLine.JI_StorageTypeVisible);
					break;
				default:
					AssertEquals($"Visibilty for Sea, type {declarationType}", false, jobComInvoiceLine.JI_StorageTypeVisible);
					break;
			}
		}
	}

	public void TestResetJI_StorageType()
	{
		var declaration = Factory.New<JobDeclaration>();
		var jobComInvoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		var storageTypePropertyInfo = jobComInvoiceLine.JI_StorageTypeInfo;
		var instruction = declaration.CustomsEntryInstructions.AddNew();
		jobComInvoiceLine.JI_CEI = instruction.PK;

		CombineAssertions(() =>
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			foreach (var declarationType in new JPImportDeclarationTypeList().GetAllCodes())
			{
				jobComInvoiceLine.JI_StorageType = "1";
				instruction.CEI_Style = declarationType;
				switch (declarationType)
				{
					case JPImportDeclarationTypeList.Codes.S:
					case JPImportDeclarationTypeList.Codes.M:
					case JPImportDeclarationTypeList.Codes.A:
					case JPImportDeclarationTypeList.Codes.G:
						AssertEquals($"CEI_Style: {instruction.CEI_Style}. Transport Mode: {declaration.JE_TransportMode}.", "1", jobComInvoiceLine.JI_StorageType);
						Assert($"JI_StorageType should be not read only. CEI_Style: {instruction.CEI_Style}. Transport Mode: {declaration.JE_TransportMode}.", !storageTypePropertyInfo.ReadOnly);
						break;
					default:
						AssertEquals($"CEI_Style: {instruction.CEI_Style}. Transport Mode: {declaration.JE_TransportMode}.", string.Empty, jobComInvoiceLine.JI_StorageType);
						Assert($"JI_StorageType should be read only. CEI_Style: {instruction.CEI_Style}. Transport Mode: {declaration.JE_TransportMode}.", storageTypePropertyInfo.ReadOnly);
						break;
				}
			}

			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			foreach (var declarationType in new JPImportDeclarationTypeList().GetAllCodes())
			{
				jobComInvoiceLine.JI_StorageType = "1";
				instruction.CEI_Style = declarationType;
				switch (declarationType)
				{
					case JPImportDeclarationTypeList.Codes.C:
					case JPImportDeclarationTypeList.Codes.F:
					case JPImportDeclarationTypeList.Codes.S:
					case JPImportDeclarationTypeList.Codes.M:
					case JPImportDeclarationTypeList.Codes.A:
					case JPImportDeclarationTypeList.Codes.G:
					case JPImportDeclarationTypeList.Codes.K:
					case JPImportDeclarationTypeList.Codes.D:
					case JPImportDeclarationTypeList.Codes.U:
					case JPImportDeclarationTypeList.Codes.L:
					case JPImportDeclarationTypeList.Codes.B:
					case JPImportDeclarationTypeList.Codes.E:
					case JPImportDeclarationTypeList.Codes.R:
						AssertEquals($"CEI_Style: {instruction.CEI_Style}. Transport Mode: {declaration.JE_TransportMode}.", "1", jobComInvoiceLine.JI_StorageType);
						Assert($"JI_StorageType should be not read only. CEI_Style: {instruction.CEI_Style}. Transport Mode: {declaration.JE_TransportMode}.", !storageTypePropertyInfo.ReadOnly);
						break;
					default:
						AssertEquals($"CEI_Style: {instruction.CEI_Style}. Transport Mode: {declaration.JE_TransportMode}.", string.Empty, jobComInvoiceLine.JI_StorageType);
						Assert($"JI_StorageType should be read only. CEI_Style: {instruction.CEI_Style}. Transport Mode: {declaration.JE_TransportMode}.", storageTypePropertyInfo.ReadOnly);
						break;
				}
			}

			jobComInvoiceLine.JI_StorageType = "1";
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals("Storage Type should be reset when not IMP.", string.Empty, jobComInvoiceLine.JI_StorageType);
			Assert($"JI_StorageType should be read only. CEI_Style: {instruction.CEI_Style}. Transport Mode: {declaration.JE_TransportMode}.", storageTypePropertyInfo.ReadOnly);
		});
	}

	public void TestGetTariffDescription()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateNomenclatureGroup(Core.Constants.CountryCodes.Japan, "1234", ZDateTime.Today.AddDays(-10), ZDateTime.Today.AddDays(10), "DESC GROUP 1", "1234");
		helper.CreateNomenclatureGroup(Core.Constants.CountryCodes.Japan, "123456", ZDateTime.Today.AddDays(-10), ZDateTime.Today.AddDays(10), "DESC GROUP 2", "123456");
		var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Japan, Universal.Constants.TariffTypes.Export);
		tariffType.ZZI_ZZ9_NKNomenclatureGroupType = "JP";
		helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Japan, tariffType.PK, "123456789", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1), "test description 1");
		Factory.Save();

		AssertEquals("TariffDescription should be empty", ZString.Empty, InvoiceLine.TariffDescription);

		InvoiceLine.JI_Tariff = "123";
		AssertEquals("", InvoiceLine.TariffDescription);

		InvoiceLine.JI_Tariff = "123456789";
		AssertEquals("test description 1", InvoiceLine.TariffDescription);

		InvoiceLine.JI_Tariff = "1234";
		AssertEquals("DESC GROUP 1", InvoiceLine.TariffDescription);

		InvoiceLine.JI_Tariff = "123456";
		AssertEquals("DESC GROUP 2", InvoiceLine.TariffDescription);
	}

	public void TestUniversalTariffType()
	{
		Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		AssertEquals(Universal.Constants.TariffTypes.Export, InvoiceLine.UniversalTariffType);

		Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		AssertEquals(Universal.Constants.TariffTypes.Import, InvoiceLine.UniversalTariffType);
	}

	public void TestCustomsCountryCode()
	{
		AssertEquals("CustomsCountryCodeCore should be JP", Core.Constants.CountryCodes.Japan, InvoiceLine.CustomsCountryCode);
	}

	public void TestJI_ConcessionOrder()
	{
		CombineAssertions(() =>
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertExceptionThrown<MaxLengthExceededException>("Cannot set to something over the length", () => { InvoiceLine.JI_ConcessionOrder = "123456"; });
			ErrorReporter.Clear();
			InvoiceLine.JI_ConcessionOrder = "12345";
			AssertEquals("Can set to the full length", "12345", InvoiceLine.JI_ConcessionOrder);
		});
	}

	public void TestIInvoiceLinePartDetailsMembers()
	{
		using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
		{
			IInvoiceLinePartDetails partDetails = Factory.New<JobComInvoiceLine>();
			AssertEquals(Core.Constants.CountryCodes.Japan, partDetails.CustomsCountryCode);
			AssertEquals(typeof(OrgSupplierPart), partDetails.TypeOfPartUsed);
		}
	}

	public void TestAdvanceRulingOnClassification()
	{
		AssertEquals(9, InvoiceLine.JI_AdvanceRulingOnClassificationInfo.MaxLength);
	}

	public void TestAdvanceRulingOnOrigin()
	{
		AssertEquals(7, InvoiceLine.JI_AdvanceRulingOnOriginInfo.MaxLength);
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

		var jPCompany = Factory.New<GlbCompany>();
		jPCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Japan;
		var jPBranch = jPCompany.Branches.AddNew();
		jPBranch.GB_RL_NKHomePort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, Core.Constants.CountryCodes.Japan)).RL_Code;

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_OH_Importer = importer.PK;
		declaration.JE_GB = jPBranch.PK;
		var invoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
		invoiceLine.JI_PartNo = "TestTEST";
		AssertEquals("Product type gets changed depending on who is requesting", typeof(OrgSupplierPart), invoiceLine.Part.GetType());
		Factory.Save();

		GlbCompany.CurrentCompany.SetCountry("AU");
		var factory3 = new BusinessObjectFactory();
		var declarationLoaded = factory3.Load<JobDeclaration>(declaration.PK);
		AssertEquals("product type still the type", typeof(OrgSupplierPart), declarationLoaded.InvoiceLines[0].Part.GetType());
	}

	public void TestTypeDecider()
	{
		Assert("Update Customs.Business.BaseJobComInvoiceLine to include a decider for this class", Factory.New(typeof(BaseJobComInvoiceLine)).GetType() == GetExpectedBusinessObjectType());
	}

	public override void TestWipeNKTaxType()
	{
		var declaration = Factory.New<JobDeclaration>();
		var invoiceHeader = declaration.Invoices.AddNew();
		var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
		Assert(!invoiceLine.ShouldWipeNKTaxType);
	}

	public override void TestJI_FormattedTariff()
	{
		InvoiceLine.JI_Tariff = "123456789";
		AssertEquals("JI_FormattedTariff", "1234.56.789", InvoiceLine.JI_FormattedTariff);
		InvoiceLine.JI_FormattedTariff = "9876.54.321";
		AssertEquals("JI_FormattedTariff", "987654321", InvoiceLine.JI_Tariff);
	}

	public void TestJI_DutyRateFormula()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "IMP";
		var invoiceHeader = declaration.Invoices.AddNew();
		var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
		var tarriffType = helper.CreateNewOrGetExistingTariffType("JP", Universal.Constants.TariffTypes.Import);
		var rateType = helper.CreateNewOrGetExistingRateType("JP", "DTY", "Duty");
		var rateCode = helper.LoadOrCreateNewCusRateCode(Factory, "DTY", rateType.PK);
		var preference = helper.CreatePreferenceForCountry("GEN", "General", "JP");
		var tradeGroup = helper.LoadOrCreateTradeGroup("JP", "JP", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date);
		helper.AddCountry(tradeGroup, "JP", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date);
		var tariff = helper.LoadOrCreateNewTariff("JP", tarriffType.PK, "1101", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
		var rate = helper.CreateRate(tariff, rateCode.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "0.15 * VFD", preference.PK, "15%", "JP");
		helper.CreateCusApplicability(rate, tradeGroup, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date);
		Factory.Save();

		AssertEquals("{None Selected}", invoiceLine.JI_DutyRateFormula);

		invoiceLine.JI_Tariff = "1101";
		invoiceLine.JI_CountryOfOrigin = "JP";
		invoiceLine.JI_PrimaryPreference = "GEN";

		var applicableRates = invoiceLine.UniversalTariff.GetApplicableRates(invoiceLine.DutyRateSelectionCriteria);
		AssertEquals(rate.PK, applicableRates.FirstOrDefault().PK);
		AssertEquals("15%", invoiceLine.JI_DutyRateFormula);
	}

	public void TestEntryInstuctionDescription()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		AssertEquals(ZString.Empty, invoiceLine.EntryInstructionDescription);

		var instruction = declaration.CustomsEntryInstructions.AddNew();
		invoiceLine.JI_CEI = instruction.PK;
		instruction.CEI_GoodsDescription = "test goods desc";
		AssertEquals("test goods desc", invoiceLine.EntryInstructionDescription);

		declaration.JE_TransportMode = TransportTypeList.Codes.Air;
		instruction.CEI_Description = "test desc";
		AssertEquals("test desc", invoiceLine.EntryInstructionDescription);
	}

	public void TestExportControlNumber()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
		var instruction = declaration.CustomsEntryInstructions.AddNew();
		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		invoiceLine.JI_CEI = instruction.PK;

		AssertEquals(ZString.Empty, invoiceLine.ExportControlNumber);

		instruction.ExportControlNumber = "123";
		AssertEquals("123", invoiceLine.ExportControlNumber);
	}

	public void TestJI_CustomsQuantity()
	{
		AssertHasCustomAttribute<DecimalPlacesAttribute>(InvoiceLine.GetType(), "JI_CustomsQuantity", includesInherit: true, attribute => attribute.DecimalPlaces == 2);
		AssertHasCustomAttribute<DecimalPrecisionAttribute>(InvoiceLine.GetType(), "JI_CustomsQuantity", includesInherit: true, attribute => attribute.DecimalPrecision == 9);
	}

	public void TestJI_CustomsSecondQuantity()
	{
		AssertHasCustomAttribute<DecimalPlacesAttribute>(InvoiceLine.GetType(), "JI_CustomsSecondQuantity", includesInherit: true, attribute => attribute.DecimalPlaces == 2);
		AssertHasCustomAttribute<DecimalPrecisionAttribute>(InvoiceLine.GetType(), "JI_CustomsSecondQuantity", includesInherit: true, attribute => attribute.DecimalPrecision == 9);
	}

	public void TestTradeControlOrderAppendixDescription()
	{
		var startDate = ZDateTime.Today.AddDays(-1);
		var endDate = ZDateTime.Today.AddDays(1);

		var universalReferenceTestHelper = new UniversalReferenceTestDataHelper(Factory);
		universalReferenceTestHelper.CreateNewOrGetExistingCusCodeType(
					Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.JapanExportTradeControlOrdinanceAppendix, "Japan Export Trade Control Ordinance Appendix");

		var refCusCodeList1 = universalReferenceTestHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Japan,
			Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.JapanExportTradeControlOrdinanceAppendix, "10418", startDate, endDate);
		refCusCodeList1.ZZD_Description = "別表第１*4－(18)";
		Factory.Save();
		InvoiceLine.JI_TradeControlOrderAppendix = "10418";
		AssertEquals("別表第１*4－(18)", InvoiceLine.TradeControlOrderAppendixDescription);
	}

	public void TestJI_LinePriceDecimalPlaces()
	{
		var invoice = Factory.New<JobComInvoiceHeader>();
		var invoiceLine = invoice.JobComInvoiceLines.AddNew();
		var info = invoiceLine.JI_LinePriceInfo;
		invoice.JZ_RX_NKInvoice_Currency = "TWD";
		AssertHasDecimalPlacesAttribute(info, 2);

		invoice.JZ_RX_NKInvoice_Currency = "JPY";
		AssertHasDecimalPlacesAttribute(info, 0);
	}

	public void TestGetTariffAttributesByKey()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Japan, Universal.Constants.TariffTypes.Export);
		tariffType.ZZI_ZZ9_NKNomenclatureGroupType = "JP";
		var tariff = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Japan, tariffType.PK, "123456789", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1), "test description 1");
		helper.CreateNewOrGetExistingTariffAttribute(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.JapanOtherLaws, "AN", tariff);
		helper.CreateNewOrGetExistingTariffAttribute(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.JapanOtherLaws, "BN", tariff);
		helper.CreateNewOrGetExistingTariffAttribute(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.JapanSpecialCargoCode, "CN", tariff);
		Factory.Save();

		InvoiceLine.JI_Tariff = "123456789";
		AssertContainsExactElementsInAnyOrder(["AN", "BN"], InvoiceLine.GetTariffAttributesByKey(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.JapanOtherLaws));
		AssertContainsExactElementsInAnyOrder(["CN"], InvoiceLine.GetTariffAttributesByKey(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.JapanSpecialCargoCode));
	}

	#region Implementation

	new JobDeclaration Declaration => (JobDeclaration)base.Declaration;

	new JobComInvoiceLine InvoiceLine => (JobComInvoiceLine)base.InvoiceLine;

	protected override void DoMerge(BaseJobDeclaration declaration)
	{
		SetupDataEligibleForMerging(declaration);
		base.DoMerge(declaration);
	}

	void SetupDataEligibleForMerging(BaseJobDeclaration declaration)
	{
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
	}

	#endregion

	protected override Type ExpectedTypeOfApportionedCharges => typeof(JobComInvApportionedChargeCollection<InvoiceLineApportionCharge>);

	protected override Type ExpectedTypeOfCharges => typeof(JobComInvChargeCollection<InvoiceLineCharge>);
}
