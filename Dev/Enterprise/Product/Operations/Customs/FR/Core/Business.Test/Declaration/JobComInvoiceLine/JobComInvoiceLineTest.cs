using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.FR.Business.CusTempStorage;
using Enterprise.Customs.FR.Business.GDM;
using Enterprise.Customs.FR.Business.MasterFiles;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Core;
using Moq.Protected;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using static Enterprise.Customs.FR.Business.Declaration.JobComInvoiceLine;
using CustomsChargeTypeList = Enterprise.Customs.Business.CustomsChargeTypeList;
using UCCCustomsChargeTypeList = Enterprise.Customs.EU.Business.UCCCustomsChargeTypeList;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	[TestedType(typeof(JobComInvoiceLine))]
	sealed class JobComInvoiceLineTest : EU.Business.Declaration.Testing.JobComInvoiceLineTest<JobComInvoiceLine>
	{
		protected override Type ExpectedGuidedDecisionMakingBasicType => typeof(GuidedDecisionMakingBasic);

		protected override Type GetExpectedEntryInstructionType() => typeof(CusEntryInstruction);

		public void TestIsPromotionalProductToDROM()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			Assert("There is no SupplementaryCode equal to 0090.", !invoiceLine.IsPromotionalProductToDROM);

			invoiceLine.JI_SupplementaryCode1 = FRConstants.SupplementaryCodes.PromotionalProductToDROMSupplementaryCode;
			Assert("JI_SupplementaryCode1 is equal to 0090.", invoiceLine.IsPromotionalProductToDROM);

			invoiceLine.JI_SupplementaryCode1 = ZString.Empty;
			invoiceLine.JI_SupplementaryCode2 = FRConstants.SupplementaryCodes.PromotionalProductToDROMSupplementaryCode;
			Assert("JI_SupplementaryCode2 is equal to 0090.", invoiceLine.IsPromotionalProductToDROM);

			invoiceLine.JI_SupplementaryCode2 = ZString.Empty;
			invoiceLine.AdditionalSupplementaryCodes.AddNew().CY_Code = FRConstants.SupplementaryCodes.PromotionalProductToDROMSupplementaryCode;
			Assert("There exists a SupplementaryCode equal to 0090.", invoiceLine.IsPromotionalProductToDROM);
		}

		public void TestIsProductOfNegligibleValueToDROM()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			Assert("HasNegligibleValueProcedure should be false when none of JI_SupplementaryCode1, JI_SupplementaryCode2, and CY_Code are 0089", !invoiceLine.IsProductOfNegligibleValueToDROM);

			invoiceLine.JI_SupplementaryCode1 = FRConstants.SupplementaryCodes.ProductOfNegligibleValueToDROMSupplementaryCode;
			Assert("JI_SupplementaryCode1 is equal to 0089.", invoiceLine.IsProductOfNegligibleValueToDROM);

			invoiceLine.JI_SupplementaryCode1 = ZString.Empty;
			invoiceLine.JI_SupplementaryCode2 = FRConstants.SupplementaryCodes.ProductOfNegligibleValueToDROMSupplementaryCode;
			Assert("JI_SupplementaryCode2 is equals to 0089.", invoiceLine.IsProductOfNegligibleValueToDROM);

			invoiceLine.JI_SupplementaryCode2 = ZString.Empty;
			invoiceLine.AdditionalSupplementaryCodes.AddNew().CY_Code = FRConstants.SupplementaryCodes.ProductOfNegligibleValueToDROMSupplementaryCode;
			Assert("CY_Code is equals 0089.", invoiceLine.IsProductOfNegligibleValueToDROM);
		}

		public void TestHasNegligibleValueProcedure()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();

			invoiceLine.JI_Procedure = "1122000";
			Assert("HasNegligibleValueProcedure should be false when procedure concession is not C07.", !invoiceLine.HasNegligibleValueProcedure);

			invoiceLine.JI_Procedure = "1122" + FRConstants.ThresholdsAndLimits.NegligibleValueProcedure;
			Assert("HasNegligibleValueProcedure should be true when procedure concession is C07.", invoiceLine.HasNegligibleValueProcedure);
		}

		public void TestHasC2CProcedure()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();

			invoiceLine.JI_Procedure = "1122000";
			Assert("HasC2CProcedure should be false when procedure concession is not C08.", !invoiceLine.HasC2CProcedure);

			invoiceLine.JI_Procedure = "1122" + FRConstants.ThresholdsAndLimits.C2CValueProcedureSuffix;
			Assert("HasC2CProcedure should be true when procedure concession is C08.", invoiceLine.HasC2CProcedure);
		}

		public void TestIsTradingWithSpecialFiscalTerritoriesProcedure()
		{
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			invoiceLine.JI_Procedure = "1122C07";
			AssertEquals("IsTradingWithSpecialFiscalTerritoriesProcedure should be false when procedure doesn't end with F15.", false, invoiceLine.IsTradingWithSpecialFiscalTerritoriesProcedure);

			invoiceLine.JI_Procedure = "1122F15";
			AssertEquals("IsTradingWithSpecialFiscalTerritoriesProcedure should be true when procedure ends with F15.", true, invoiceLine.IsTradingWithSpecialFiscalTerritoriesProcedure);
		}

		public void TestCPCList()
		{
			Db.Connection.ExecuteNonQuery("DELETE FROM RefDatabase_RefCusProcedure");
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateRefCusProcedure(CountryCodes.France, "", "10", "00", "000", "Export DeltaG Procedure Group 10", EU.Business.MessageTypeList.Codes.Export, group: "B1");
			helper.CreateRefCusProcedure(CountryCodes.France, "", "21", "00", "000", "Export DeltaG Procedure Group 21", EU.Business.MessageTypeList.Codes.Export, group: "B2");
			helper.CreateRefCusProcedure(Core.Constants.Customs.Universal.RefDataGrouping.Codes.DeltaIE, "", "22", "00", "000", "Export DeltaIE Procedure Group B1", EU.Business.MessageTypeList.Codes.Export, group: "B1");
			helper.CreateRefCusProcedure(Core.Constants.Customs.Universal.RefDataGrouping.Codes.DeltaIE, "", "31", "00", "000", "Export DeltaIE Procedure Group B2", EU.Business.MessageTypeList.Codes.Export, group: "B2");
			helper.CreateRefCusProcedure(CountryCodes.France, "", "01", "00", "000", "Import DeltaG Procedure Group 01", EU.Business.MessageTypeList.Codes.Import, group: "B1");
			helper.CreateRefCusProcedure(CountryCodes.France, "", "02", "00", "000", "Import DeltaG Procedure Group 02", EU.Business.MessageTypeList.Codes.Import, group: "B2");
			helper.CreateRefCusProcedure(Core.Constants.Customs.Universal.RefDataGrouping.Codes.DeltaIE, "", "07", "00", "000", "Import DeltaIE Procedure Group B1", EU.Business.MessageTypeList.Codes.Import, group: "B1");
			helper.CreateRefCusProcedure(Core.Constants.Customs.Universal.RefDataGrouping.Codes.DeltaIE, "", "40", "00", "000", "Import DeltaIE Procedure Group B2", EU.Business.MessageTypeList.Codes.Import, group: "B2");
			Factory.Save();

			CombineAssertions("List should filter on FR Data Grouping,  Message Type, Declaration Application Code and DeclarationType", () =>
			{
				AssertProcedureList(EU.Business.MessageTypeList.Codes.Export, "B1", "1000000");
				AssertProcedureList(EU.Business.MessageTypeList.Codes.Export, "B2", "2100000");
				AssertProcedureList(EU.Business.MessageTypeList.Codes.Import, "B1", "0100000");
				AssertProcedureList(EU.Business.MessageTypeList.Codes.Import, "B2", "0200000");
			});
		}

		void AssertProcedureList(string messageType, string declarationType, string expectedResult)
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = messageType;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaG;
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_Style = declarationType;
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;

			AssertContainsExactElementsInAnyOrder(new string[] { expectedResult }, invoiceLine.Lookups.CPCList.Select(x => x.ZZ6_ProcedureCode + x.ZZ6_PreviousProcedureCode + x.ZZ6_Concession).ToArray());
		}

		public void TestIsWarehouseOrderEnabled()
		{
			AssertIsWarehouseOrderEnabled("AB72", WarehouseMoveStatus.Codes.Yes, isWarehouseOrderFunctionActivated: true, JobMessageTypeList.Codes.Export, entryInstructionIsBondedWarehouse: true, expectedIsWarehouseOrderEnabled: false);
		}

		void AssertIsWarehouseOrderEnabled(string proc, string hasOutOfWarehouseProcedure, bool isWarehouseOrderFunctionActivated, string direction, bool entryInstructionIsBondedWarehouse, bool expectedIsWarehouseOrderEnabled)
		{
			var whsDataTestHelper = new WhsDataTestHelper(Factory);
			whsDataTestHelper.Importer.CompanyData.OB_IMUsedBondedWhs = false;
			whsDataTestHelper.Owner.CompanyData.OB_IMUsedBondedWhs = false;
			whsDataTestHelper.Warehouse.CompanyData.OB_IMUsedBondedWhs = entryInstructionIsBondedWarehouse;
			whsDataTestHelper.Warehouse2.CompanyData.OB_IMUsedBondedWhs = entryInstructionIsBondedWarehouse;

			var helper = new UniversalReferenceTestDataHelper(Factory);
			var currentCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			helper.CreateNewOrGetExistingDataGrouping(currentCountry);
			var procedure = helper.CreateRefCusProcedure(currentCountry, ZString.Empty, proc.Substring(0, 2), proc.Substring(2, 2), ZString.Empty, "description", direction);
			procedure.ZZ6_OutOfWarehouse = hasOutOfWarehouseProcedure;
			var declarationMock = Factory.NewMoq<JobDeclaration>();
			var declaration = declarationMock.Object;
			declarationMock.Protected()
				.Setup<bool>("IsWarehouseOrderFunctionActivatedCore")
				.Returns(isWarehouseOrderFunctionActivated);

			var instruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew() as CusEntryInstruction;
			instruction.CEI_Style = "AB";
			instruction.CEI_OH_Owner = whsDataTestHelper.Owner.PK;
			instruction.CEI_OA_Warehouse = whsDataTestHelper.Warehouse.MainAddress.PK;
			instruction.CEI_OA_Warehouse2 = whsDataTestHelper.Warehouse2.MainAddress.PK;

			declaration.JE_MessageType = direction;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CEI = instruction.PK;
			invoiceLine.JI_Procedure = procedure.ZZ6_ProcedureCode + procedure.ZZ6_PreviousProcedureCode;

			var message = $"Direction is {direction} and EntryInstruction.WarehouseIsBOndedWarehousing  is {entryInstructionIsBondedWarehouse} and HasOutOfWarehouseProcedure is {hasOutOfWarehouseProcedure} and isWarehouseOrderFunctionActivated is {isWarehouseOrderFunctionActivated} so invoiceline.IsWarehouseOrderEnabled should be equal to {expectedIsWarehouseOrderEnabled}.";
			AssertEquals(message, expectedIsWarehouseOrderEnabled, invoiceLine.IsWarehouseOrderEnabled);
		}

		public void TestGroupChargesApportionShouldUseDocumentaryAmoutAsBaseWhenDistributedByValue()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_RX_NKInvoice_Currency = Enterprise.Core.Constants.CurrencyCodes.France;
			var invoice1Charge = invoice1.Charges.AddNew();
			invoice1Charge.J7_ChargeType = CustomsChargeTypeList.Codes.AdditionCharge;
			invoice1Charge.J7_Amount = 100m;
			invoice1Charge.J7_DistributeBy = "VAL";
			var invoiceLine1 = invoice1.InvoiceLines.AddNew();
			invoiceLine1.JI_LinePrice = 100m;

			var invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_RX_NKInvoice_Currency = Enterprise.Core.Constants.CurrencyCodes.France;
			var invoice2Charge = invoice2.Charges.AddNew();
			invoice2Charge.J7_ChargeType = FRCustomsChargeTypeList.Codes.Cut;
			invoice2Charge.J7_Amount = 50m;
			invoice2Charge.J7_DistributeBy = "VAL";
			var invoiceLine2 = invoice2.InvoiceLines.AddNew();
			invoiceLine2.JI_LinePrice = 100m;

			var groupCharge = invoice1.GroupHeader.Charges.AddNew();
			groupCharge.J7_ChargeType = FRCustomsChargeTypeList.Codes.AdjustmentCharge;
			groupCharge.J7_Amount = 250m;
			groupCharge.J7_DistributeBy = "VAL";

			declaration.ResumeApportionment();

			CombineAssertions(() =>
			{
				AssertEquals("Prerequisite: InvoiceLine1 DocumentaryAmount", 200m, invoiceLine1.JI_Calc_InvoicedDocumentaryAmountValue);
				AssertEquals("Prerequisite: InvoiceLine2 DocumentaryAmount", 50m, invoiceLine2.JI_Calc_InvoicedDocumentaryAmountValue);

				AssertEquals("Group Charges is distributed by the DocumentaryAmount: Invoice1", 200m, invoice1.GroupCharges.First().J7_Amount);
				AssertEquals("Group Charges is distributed by the DocumentaryAmount: Invoice2", 50m, invoice2.GroupCharges.First().J7_Amount);
			});
		}

		public new void TestSetTariffEtcDataFromProductsPivotCore()
		{
			SetTariffEtcDataFromProductsPivotCore(JobMessageTypeList.Codes.Import, ClassificationType.IMP, ("1234567890", "FR", "CPC", "Supp1", "Supp2", "AddSupp1,AddSupp2", 3.33m, "PRC"));
			SetTariffEtcDataFromProductsPivotCore(JobMessageTypeList.Codes.Import, ClassificationType.EXP, ("", "", "", "", "", "", 0m, ""));
			SetTariffEtcDataFromProductsPivotCore(JobMessageTypeList.Codes.Import, ClassificationType.Both, ("1234567890", "FR", "", "", "", "", 0m, ""));

			SetTariffEtcDataFromProductsPivotCore(JobMessageTypeList.Codes.Export, ClassificationType.IMP, ("", "", "", "", "", "", 0m, ""));
			SetTariffEtcDataFromProductsPivotCore(JobMessageTypeList.Codes.Export, ClassificationType.EXP, ("1234567890", "FR", "CPC", "Supp1", "Supp2", "AddSupp1,AddSupp2", 3.33m, "PRC"));
			SetTariffEtcDataFromProductsPivotCore(JobMessageTypeList.Codes.Export, ClassificationType.Both, ("1234567890", "FR", "", "", "", "", 0m, ""));

			void SetTariffEtcDataFromProductsPivotCore(ZString messageType, ZString childType,
				(string expectedJI_Tariff, string expectedJI_CountryOfOrigin, string expectedJI_Procedure, string expectedJI_SupplementaryCode1, string expectedSupplementaryCode2, string expectedAdditionalSupplementaryCodes, decimal expectedJI_CustomsThirdQuantity, string expectJI_PrimaryPreference) tuple)
			{
				var product = (EU.Business.MasterFiles.OrgSupplierPart)EU.Business.MasterFiles.OrgSupplierPart.New(Factory);
				var importer = Factory.NewWithValidTestData<OrgHeader>();
				importer.OH_Code = "~~~";
				product.OP_PartNum = "ABC";
				product.RelatedOrganisations.RemoveAndDeleteAll();
				product.RelatedOrganisations.AddOrganisationIfNotExist(importer.PK, OrgPartRelation.RelationshipTypes.Owner);

				var pivot = Factory.New<CusClassPartPivot>();
				pivot.CI_OP = product.PK;
				pivot.CI_ChildType = childType;
				pivot.CI_TariffNum = "1234567890";

				pivot.CI_Supplement1 = "Supp1";
				pivot.CI_Supplement2 = "Supp2";
				var addSupp1 = pivot.AdditionalSupplementaryCodes.AddNew();
				var addSupp2 = pivot.AdditionalSupplementaryCodes.AddNew();
				addSupp1.CY_Code = "AddSupp1";
				addSupp2.CY_Code = "AddSupp2";

				pivot.CI_CPC = "CPC";

				pivot.CI_ConcessionOrder = "ConOrder";
				pivot.CI_ThirdQty = 3.33m;
				pivot.CI_RN_NKCountryOfOrigin = Core.Constants.CountryCodes.France;
				pivot.PreferenceCode = "PRC";

				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_OH_Importer = importer.PK;
				declaration.JE_MessageType = messageType;
				var invLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();

				invLine.JI_CC = ZGuid.Invalid;
				invLine.JI_PartNo = product.OP_PartNum;

				CombineAssertions(string.Join(" ", "Message Type:", messageType, ", ChildType:", childType), () =>
				{
					AssertEquals("JI_Tariff", tuple.expectedJI_Tariff, invLine.JI_Tariff);
					AssertEquals("JI_CountryOfOrigin", tuple.expectedJI_CountryOfOrigin, invLine.JI_CountryOfOrigin);
					AssertEquals("JI_Procedure", tuple.expectedJI_Procedure, invLine.JI_Procedure);
					AssertEquals("JI_SupplementaryCode1", tuple.expectedJI_SupplementaryCode1, invLine.JI_SupplementaryCode1);
					AssertEquals("SupplementaryCode2", tuple.expectedSupplementaryCode2, invLine.JI_SupplementaryCode2);
					AssertEquals("AdditionalSupplementaryCodes", tuple.expectedAdditionalSupplementaryCodes, invLine.AdditionalSupplementaryCodes.AsString);
					AssertEquals("JI_CustomsThirdQuantity", tuple.expectedJI_CustomsThirdQuantity, invLine.JI_CustomsThirdQuantity);
					AssertEquals("JI_PrimaryPreference", tuple.expectJI_PrimaryPreference, invLine.JI_PrimaryPreference);
				});
			}
		}

		public void TestUniversalTariffType()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceline = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			AssertEquals("IMP", invoiceline.UniversalTariffType);

			declaration.JE_TariffType = ZString.Empty;
			AssertEquals("EXP", invoiceline.UniversalTariffType);

			var invoiceline2 = Factory.New<JobComInvoiceHeader>().InvoiceLines.AddNew();
			AssertEquals("HSN", invoiceline2.UniversalTariffType);
		}

		public void TestSetDefaultTaxOrFeeCode_OneRelatedTaxOrFeeCode()
		{
			var tariff = CreateTariffAndFeeDetails();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = tariff.ZZ1_TariffCode;
			AssertEquals("Has only one related TaxOrFeeCode: ZZ1", "ZZ1", invoiceLine.JI_ZZF_NKTaxType);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = tariff.ZZ1_TariffCode;
			AssertEquals("Has no related TaxOrFeeCode", string.Empty, invoiceLine.JI_ZZF_NKTaxType);
		}

		TariffView CreateTariffAndFeeDetails()
		{
			var startDate = ZDateTime.Today.AddDays(-2);
			var endDate = ZDateTime.Today.AddDays(2);
			var countryCode = Core.Constants.CountryCodes.France;
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffTypePK = helper.CreateNewOrGetExistingTariffType(countryCode, UniversalTariffTypeForDefaultTaxOrFeeCode).PK;

			var rate = 0.01m * (1 + 1m);
			var taxOrFee = helper.CreateTaxOrFee("ZZ1", rate, countryCode);
			taxOrFee.ZZF_ZX0_NKTaxOrFeeType = "VAT";
			Factory.Save();

			var tariff = helper.CreateTariff(countryCode, tariffTypePK, "99999999", startDate, endDate, taxOrFeeCode: "ZZT");
			helper.CreateNewOrGetExistingVATApplicability(tariff, countryCode, "ZZ1", "AdditionalCode1", startDate, endDate);
			Factory.Save();

			return tariff;
		}

		public void TestDefaultDataGrouping()
		{
			foreach (var country in Core.Constants.CountryCodes.FranceAndOverseasDepartmentsUnderItsCustomsJurisdiction)
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(country))
				{
					var invoiceLine = Factory.New<JobDeclaration>().Invoices.AddNew().InvoiceLines.AddNew();
					CombineAssertions($"Default data groupings for {country}", () =>
					{
						AssertEquals("Default data grouping.", Core.Constants.CountryCodes.France, invoiceLine.GetDefaultDataGroupingCode());
						AssertEquals("Default tariff data grouping.", Core.Constants.CountryCodes.France, invoiceLine.GetDefaultDataGroupingCode(DefaultDataGroupingType.Tariff));
						AssertEquals("Default duties data grouping.", Core.Constants.CountryCodes.France, invoiceLine.GetDefaultDataGroupingCode(DefaultDataGroupingType.DutyRateCodes));
					});
				}
			}
		}

		public void TestZG_CountryOfSupply_CaptionKeyUCC()
		{
			CombineAssertions(() =>
			{
				var captionResourceString = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(Factory.New<JobComInvoiceLine>().ZG_CountryOfSupplyInfo, JobDeclaration.CaptionKeyUCC);
				AssertEquals("Preferential Country", captionResourceString.Caption);
			});
		}

		public void TestEffectiveCountryOfOriginCore()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;

			invoiceLine.JI_CountryOfOrigin = "DE";
			invoiceLine.ZG_CountryOfSupply = "CN";
			AssertEquals("CN", invoiceLine.EffectiveCountryOfOrigin);

			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaG;
			AssertEquals("DE", invoiceLine.EffectiveCountryOfOrigin);

			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals("DE", invoiceLine.EffectiveCountryOfOrigin);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			invoiceLine.ZG_CountryOfSupply = "";
			AssertEquals("DE", invoiceLine.EffectiveCountryOfOrigin);
		}

		public override void TestComponentPrice()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = Factory.New<JobComInvoiceLineForTest>();
			invoiceLine.JI_JZ = invoice.PK;
			invoiceLine.JI_LinePrice = 50m;
			invoiceLine.JI_ValuationMarkup = 10m;
			Assert("Prerequisite.", Declaration.Configuration.InvoiceLineConfiguration.InflateItemPriceByValuationMarkup(Declaration));
			AssertEquals("JI_ValuationMarkup should shift ComponentPrice calculation in FR because InflateItemPriceByValuationMarkup is true.", 55m, invoiceLine.ComponentPrice);
		}

		public void TestCustomsUnitDefaultingStrategy()
		{
			var (tariff, rate, _, _, _, _, rate5, rate6) = UniversalRateCustomsUnitDefaultingStrategyTestingHelper.SetupTariffData(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, "IMP");

			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = Factory.New<DummyJobComInvoiceLine_TestCustomsUnitDefaultingStrategy>();
			invoiceLine.UniversalDutyRateForReturn = rate5;
			invoiceLine.NationalRatesForReturn = new RateView[] { rate6 };

			invoiceLine.JI_JZ = invoice.PK;
			invoiceLine.JI_Tariff = "11111111";

			CombineAssertions(() =>
			{
				AssertEquals("First UQ", "CU1", invoiceLine.JI_CustomsUnitQty);
				AssertEquals("Second UQ", "CU2", invoiceLine.JI_CustomsSecondUnitQty);
				AssertEquals("Third UQ", "UM4", invoiceLine.JI_CustomsThirdUnitQty);
				AssertEquals("Fourth UQ", "UM5", invoiceLine.JI_CustomsFourthUnitQty);
			});

			invoiceLine.JI_CustomsUnitQty = "";
			invoiceLine.JI_CustomsSecondUnitQty = "";
			invoiceLine.JI_CustomsThirdUnitQty = "";
			invoiceLine.JI_CustomsFourthUnitQty = "";

			declaration.JE_RegionOrTerritoryOfDestination = "CORSE";
			CombineAssertions(() =>
			{
				AssertEquals("First UQ", "CU1", invoiceLine.JI_CustomsUnitQty);
				AssertEquals("Second UQ", "CU2", invoiceLine.JI_CustomsSecondUnitQty);
				AssertEquals("Third UQ", "UM4", invoiceLine.JI_CustomsThirdUnitQty);
				AssertEquals("Fourth UQ", "UM5", invoiceLine.JI_CustomsFourthUnitQty);
			});
		}

		public void TestJI_ZZF_NKTaxTypeReadOnly()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();

			AssertEquals("JI_ZZF_NKTaxType should be read only", true, invoiceLine.JI_ZZF_NKTaxTypeInfo.ReadOnly);
		}

		public void TestPreviousISTDocument()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaG;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();

			CombineAssertions("Test for DeltaG Declaration", () =>
			{
				var doc1 = invoice.PreviousDocuments.AddNew();
				doc1.CSI_Code = FRConstants.PreviousDocuments.N337;
				doc1.CSI_ReferenceNumber = "001";
				AssertNull("Previous IST document is null as code type is not IST", invoiceLine.PreviousISTDocument);

				var doc2 = declaration.PreviousDocuments.AddNew();
				doc2.CSI_Code = PreviousDocumentCodeList.Codes.IST;
				doc2.CSI_ReferenceNumber = "002";
				AssertSame("Previous IST document fallback to declaration if there is no previous document on invoice line and invoice header.", doc2, invoiceLine.PreviousISTDocument);

				var doc3 = invoice.PreviousDocuments.AddNew();
				doc3.CSI_Code = PreviousDocumentCodeList.Codes.IST;
				doc3.CSI_ReferenceNumber = "003";
				AssertSame("Previous IST document fallback to invoice header if there is no previous document on invoice line.", doc3, invoiceLine.PreviousISTDocument);

				var doc4 = invoiceLine.PreviousDocuments.AddNew();
				doc4.CSI_Code = PreviousDocumentCodeList.Codes.IST;
				doc4.CSI_ReferenceNumber = "004";
				AssertSame("Previous IST document on invoice line.", doc4, invoiceLine.PreviousISTDocument);
			});

			invoice.PreviousDocuments.RemoveAndDeleteAll();
			invoiceLine.PreviousDocuments.RemoveAndDeleteAll();
			declaration.PreviousDocuments.RemoveAndDeleteAll();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;

			CombineAssertions("Test for DeltaIE Declaration", () =>
			{
				var doc1 = invoice.PreviousDocuments.AddNew();
				doc1.CSI_Code = PreviousDocumentCodeList.Codes.IST;
				doc1.CSI_ReferenceNumber = "001";
				AssertNull("Previous IST document is null as code type is not N337", invoiceLine.PreviousISTDocument);

				var doc2 = declaration.PreviousDocuments.AddNew();
				doc2.CSI_Code = FRConstants.PreviousDocuments.N337;
				doc2.CSI_ReferenceNumber = "002";
				AssertSame("Previous IST document fallback to declaration if there is no previous document on invoice line and invoice header.", doc2, invoiceLine.PreviousISTDocument);

				var doc3 = invoice.PreviousDocuments.AddNew();
				doc3.CSI_Code = FRConstants.PreviousDocuments.N337;
				doc3.CSI_ReferenceNumber = "003";
				AssertSame("Previous IST document fallback to invoice header if there is no previous document on invoice line.", doc3, invoiceLine.PreviousISTDocument);

				var doc4 = invoiceLine.PreviousDocuments.AddNew();
				doc4.CSI_Code = FRConstants.PreviousDocuments.N337;
				doc4.CSI_ReferenceNumber = "004";
				AssertSame("Previous IST document on invoice line.", doc4, invoiceLine.PreviousISTDocument);
			});
		}

		public void TestPreviousISTNumber()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();

			var doc1 = invoice.PreviousDocuments.AddNew();
			doc1.CSI_Code = PreviousDocumentCodeList.Codes.IM;
			doc1.CSI_ReferenceNumber = "001";
			AssertEquals("Previous IST number is empty as code type is not IST", ZString.Empty, invoiceLine.PreviousISTNumber);

			var doc2 = invoice.PreviousDocuments.AddNew();
			doc2.CSI_Code = PreviousDocumentCodeList.Codes.IST;
			doc2.CSI_ReferenceNumber = "002";
			AssertEquals("Previous IST number fallback to invoice header if there is no one on line.", "002", invoiceLine.PreviousISTNumber);

			var doc3 = invoiceLine.PreviousDocuments.AddNew();
			doc3.CSI_Code = PreviousDocumentCodeList.Codes.IST;
			doc3.CSI_ReferenceNumber = "003";
			AssertEquals("Previous IST number on invoice line.", "003", invoiceLine.PreviousISTNumber);
		}

		public void TestPreviousISTRegister()
		{
			var ist1 = CusTempStorageJobHeader.New(Factory, FRConstants.TemporaryStorage.AppCodeIST);
			ist1.SJH_JobReference = "FRJ_IST1";

			var ist2 = CusTempStorageJobHeader.New(Factory, FRConstants.TemporaryStorage.AppCodeIST);
			ist2.SJH_JobReference = "FRJ_IST2";

			var ist3 = CusTempStorageJobHeader.New(Factory, FRConstants.TemporaryStorage.AppCodeIST);
			ist3.SJH_JobReference = "FRJ_IST3";
			var ddt = CusEntryNumber.New(ist3, CusEntryNumberTypes.France.DDT, Core.Constants.CountryCodes.France);
			ddt.CE_EntryNum = "DDT_SAMPLE";

			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();

			var doc = invoice.PreviousDocuments.AddNew();
			doc.CSI_Code = PreviousDocumentCodeList.Codes.IM;
			doc.CSI_ReferenceNumber = "FRJ_IST1";
			AssertNull("Previous IST header is null as code type is not IST", invoiceLine.PreviousISTHeader);

			doc.CSI_Code = PreviousDocumentCodeList.Codes.IST;
			doc.CSI_ReferenceNumber = "FRJ_IST2";
			AssertSame("Previous IST header is found by matching job reference.", ist2, invoiceLine.PreviousISTHeader);

			doc.CSI_ReferenceNumber = "DDT_SAMPLE";
			AssertSame("Previous IST header is found by matching DDT number.", ist3, invoiceLine.PreviousISTHeader);
		}

		public void TestConditionSelectionCriteria_SecondTradeGroups()
		{
			var invoiceHeader = Factory.New<JobComInvoiceHeader>();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();

			CombineAssertions(() =>
			{
				var conditionSelectionCriteria = invoiceLine.ConditionSelectionCriterias.First();
				AssertEquals("Empty SecondTradeGroups when invoiceLine has no attached declaration", 0, conditionSelectionCriteria.SecondTradeGroups.Count);

				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				invoiceHeader.JZ_JE = declaration.PK;
				conditionSelectionCriteria = invoiceLine.ConditionSelectionCriterias.First();
				AssertEquals("Empty SecondTradeGroups when declaration has no Region value", 0, conditionSelectionCriteria.SecondTradeGroups.Count);

				declaration.JE_RegionOrTerritoryOfDestination = FRDomesticOverseasTerritories.Codes.CORSE;
				conditionSelectionCriteria = invoiceLine.ConditionSelectionCriterias.First();
				AssertContainsExactElementsInAnyOrder("Has SecondTradeGroups when declaration has Region value", new[] { FRDomesticOverseasTerritories.Codes.CORSE, FRLocalGroups.Codes.METRO }, conditionSelectionCriteria.SecondTradeGroups);
			});
		}

		public void TestVATSelectionCriteria_SecondTradeGroups()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_ValuationDateOverride = new ZDateTime(2019, 12, 10, 0, 0, 0);
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_ZZF_NKTaxType = "tax1";

			AssertCusRateType(FRDomesticOverseasTerritories.Codes.CORSE, 2, new ZString[] { FRDomesticOverseasTerritories.Codes.CORSE, FRLocalGroups.Codes.METRO });
			AssertCusRateType(FRDomesticOverseasTerritories.Codes.CONTI, 2, new ZString[] { FRDomesticOverseasTerritories.Codes.CONTI, FRLocalGroups.Codes.METRO });
			AssertCusRateType(FRDomesticOverseasTerritories.Codes.GUADE, 3, new ZString[] { FRDomesticOverseasTerritories.Codes.GUADE, FRLocalGroups.Codes.DPDOM, FRLocalGroups.Codes.MGPRE });
			AssertCusRateType(FRDomesticOverseasTerritories.Codes.MARTI, 3, new ZString[] { FRDomesticOverseasTerritories.Codes.MARTI, FRLocalGroups.Codes.DPDOM, FRLocalGroups.Codes.MGPRE });
			AssertCusRateType(FRDomesticOverseasTerritories.Codes.GUYAN, 2, new ZString[] { FRDomesticOverseasTerritories.Codes.GUYAN, FRLocalGroups.Codes.DPDOM });
			AssertCusRateType(FRDomesticOverseasTerritories.Codes.MAYOT, 2, new ZString[] { FRDomesticOverseasTerritories.Codes.MAYOT, FRLocalGroups.Codes.DPDOM });
			AssertCusRateType(FRDomesticOverseasTerritories.Codes.REUNI, 3, new ZString[] { FRDomesticOverseasTerritories.Codes.REUNI, FRLocalGroups.Codes.DPDOM, FRLocalGroups.Codes.MGPRE });
			AssertCusRateType("OTHER", 1, new ZString[] { "OTHER" });

			void AssertCusRateType(ZString region, int countOfTradeGroup, ZString[] listOfTradeGroup)
			{
				declaration.JE_RegionOrTerritoryOfDestination = region;
				var vatSelectionCriteria = invoiceLine.VATSelectionCriteria;
				AssertEquals(countOfTradeGroup, vatSelectionCriteria.TradeGroups.Count);
				AssertContainsExactElementsInAnyOrder(listOfTradeGroup, vatSelectionCriteria.TradeGroups);
			}
		}

		public void TestTariffAdditionalCodeSelectionCriteria()
		{
			AssertTariffAdditionalCodeSelectionCriterias(JobMessageTypeList.Codes.Import);
			AssertTariffAdditionalCodeSelectionCriterias(JobMessageTypeList.Codes.Export);
		}

		void AssertTariffAdditionalCodeSelectionCriterias(ZString direction)
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = direction;
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_DateForDuty = new ZDateTime(2024, 03, 22, 0, 0, 0);
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;
			invoiceLine.JI_CountryOfOrigin = "US";

			var tariffAdditionalCodeSelectionCriterias = invoiceLine.TariffAdditionalCodeSelectionCriteria;
			AssertEquals("Category", direction == JobMessageTypeList.Codes.Import ? "SIP" : "SEP", tariffAdditionalCodeSelectionCriterias.Category);
			AssertEquals("EffectiveDate", new ZDateTime(2024, 03, 22, 0, 0, 0), tariffAdditionalCodeSelectionCriterias.EffectiveDate);
			AssertEquals("TradeGroupCountry", "US", tariffAdditionalCodeSelectionCriterias.TradeGroupCountry);
			AssertEquals("DataGrouping", GlbCompany.CurrentCompany.Country.Code, tariffAdditionalCodeSelectionCriterias.DataGrouping);
		}

		public override void TestNationalRateSelectionCriteria()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.France);
			helper.CreateCusRateType(Core.Constants.CountryCodes.France, "AMC");
			helper.CreateCusRateType(Core.Constants.CountryCodes.France, "DEV");
			helper.CreateCusRateType(Core.Constants.CountryCodes.France, "EXC");
			helper.CreateCusRateType(Core.Constants.CountryCodes.France, "RED");
			helper.CreateCusRateType(Core.Constants.CountryCodes.France, "OMR");
			helper.CreateCusRateType(Core.Constants.CountryCodes.France, "OME");

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_RegionOrTerritoryOfDestination = FRDomesticOverseasTerritories.Codes.CORSE;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_SupplementaryCode1 = "1";
			invoiceLine.JI_SupplementaryCode2 = "2";

			var nationalRateSelectionCriteria = invoiceLine.NationalRateSelectionCriteria;
			AssertNotNull("CountervailingRateSelectionCriteria", nationalRateSelectionCriteria);
			AssertEquals(4, nationalRateSelectionCriteria.Count());
			AssertContainsExactElementsInAnyOrder(new ZString[] { "AMC", "DEV", "EXC", "RED" }, nationalRateSelectionCriteria.Select(x => x.RateType));
			AssertContainsExactElementsInAnyOrder(new HashSet<ZString>(new ZString[] { FRDomesticOverseasTerritories.Codes.CORSE, FRLocalGroups.Codes.METRO }), nationalRateSelectionCriteria.FirstOrDefault().SecondTradeGroups);

			declaration.JE_RegionOrTerritoryOfDestination = FRDomesticOverseasTerritories.Codes.CONTI;
			AssertEquals(4, nationalRateSelectionCriteria.Count());
			AssertContainsExactElementsInAnyOrder(new ZString[] { "AMC", "DEV", "EXC", "RED" }, nationalRateSelectionCriteria.Select(x => x.RateType));

			declaration.JE_RegionOrTerritoryOfDestination = FRDomesticOverseasTerritories.Codes.REUNI;
			AssertEquals(6, nationalRateSelectionCriteria.Count());
			AssertContainsExactElementsInAnyOrder(new ZString[] { "AMC", "DEV", "EXC", "RED", "OME", "OMR" }, nationalRateSelectionCriteria.Select(x => x.RateType));

			declaration.JE_RegionOrTerritoryOfDestination = ZString.Empty;
			AssertEquals(0, nationalRateSelectionCriteria.FirstOrDefault().SecondTradeGroups.Count);
		}

		public void TestRateSelectionCriteria_ImportExport()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.France);
			helper.CreateCusRateType(Core.Constants.CountryCodes.France, "AMC", isExport: true);
			helper.CreateCusRateType(Core.Constants.CountryCodes.France, "DEV", isExport: false);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_SupplementaryCode1 = "1";
			invoiceLine.JI_SupplementaryCode2 = "2";

			var rateSelectionCriteria = (FRRateSelectionCriteria)invoiceLine.AllApplicableRatesSelectionCriteria;
			AssertNotNull("AllApplicableRatesSelectionCriteria", rateSelectionCriteria);
			AssertEquals(RateDirection.Import, rateSelectionCriteria.Direction);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			rateSelectionCriteria = (FRRateSelectionCriteria)invoiceLine.AllApplicableRatesSelectionCriteria;
			AssertNotNull("AllApplicableRatesSelectionCriteria", rateSelectionCriteria);
			AssertEquals(RateDirection.Export, rateSelectionCriteria.Direction);
		}

		public void TestHasPrecalculeRate()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.France);

			var testTradeGroup1 = helper.CreateTradeGroup(Enterprise.Core.Constants.CountryCodes.SouthAfrica, "STANDARD", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.AddCountry(testTradeGroup1, Enterprise.Core.Constants.CountryCodes.SouthAfrica, new ZDate(1980, 01, 01), new ZDate(2079, 06, 06));

			var rateType1 = helper.CreateCusRateType(Core.Constants.CountryCodes.France, "MSC");
			var rateCode1 = helper.CreateCusRateCode(Factory, "111", rateType1.PK);

			var rateType2 = helper.CreateCusRateType(Core.Constants.CountryCodes.France, "AMC");
			var rateCode2 = helper.CreateCusRateCode(Factory, "222", rateType2.PK);

			var tariffType = helper.CreateTariffType(Core.Constants.CountryCodes.France, Customs.Business.UniversalReferenceConstants.CusTariffTypes.ImportTariff);
			Factory.Save();

			var tariff = helper.CreateTariff(Core.Constants.CountryCodes.France, tariffType.PK, "1010101011", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var rate1 = helper.CreateRefCusRate(tariff.PK, rateCode1.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, rateFormula: UniversalReferenceConstants.RefCusRateFormula.Precalcule);
			var rate2 = helper.CreateRefCusRate(tariff.PK, rateCode2.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var testApplicability1 = helper.CreateCusApplicability(rate1, testTradeGroup1, new ZDate(1980, 01, 01), new ZDate(2079, 06, 06));
			var testApplicability2 = helper.CreateCusApplicability(rate2, testTradeGroup1, new ZDate(1980, 01, 01), new ZDate(2079, 06, 06));

			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var entryInstruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_Style = "11";
			entryInstruction.CEI_DateForDuty = new ZDateTime(2022, 03, 08);
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Enterprise.Core.Constants.CurrencyCodes.France;
			invoice.JZ_ValuationDateOverride = new ZDateTime(2022, 03, 08);
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = tariff.ZZ1_TariffCode;
			invoiceLine.JI_CEI = entryInstruction.PK;
			invoiceLine.JI_Procedure = invoiceLine.EntryInstruction.CEI_Style + "11";
			invoiceLine.JI_CountryOfOrigin = Enterprise.Core.Constants.CountryCodes.SouthAfrica;
			Factory.Save();
			Assert(invoiceLine.HasPrecalculeRate);

			rate1.ZZ2_RateFormula = "";
			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = tariff.ZZ1_TariffCode;
			Factory.Save();
			Assert(!invoiceLine2.HasPrecalculeRate);
		}

		public void TestRequiresVATNumber()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.France);
			var procedure1 = helper.CreateOrFindExistingRefCusProcedure(Core.Constants.CountryCodes.France, "", "50", "0", "   ", "", "IMP", "");
			var procedure2 = helper.CreateOrFindExistingRefCusProcedure(Core.Constants.CountryCodes.France, "", "42", "0", "   ", "", "IMP", "");
			helper.CreateRefCusProcedureAttribute(procedure2.PK, "VATNumberExempt", "Y");
			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalInformation, "additional info");
			helper.CreateCusCodeList("FR", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalInformation, "70000", "doc 70000", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
			helper.CreateCusCodeListWithAttribute("FR", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalInformation, "I9101", "doc I9101", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6), Core.Constants.Customs.Universal.RefCusCodeList.Attributes.Category, Core.Constants.Customs.Universal.RefCusCodeList.AttributeValues.DCC);
			Factory.Save();

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			InvoiceLine.JI_FormattedProcedure = "500";
			AssertEquals("Procedure 500 has no VATNumberExempt attribute, so it requires a VAT number.", true, InvoiceLine.RequiresVATNumberDocument);

			InvoiceLine.JI_FormattedProcedure = "420";
			AssertEquals("Procedure 400 has a VATNumberExempt attribute, so it requires no VAT number.", false, InvoiceLine.RequiresVATNumberDocument);

			InvoiceLine.JI_FormattedProcedure = "500";
			var specialMention = InvoiceLine.AdditionalInfos.AddNew();
			specialMention.CSI_Code = "70000";
			AssertEquals("Special mention 7000 has no attribute DCC, so it requires a VAT number.", true, InvoiceLine.RequiresVATNumberDocument);

			specialMention.CSI_Code = "I9101";
			AssertEquals("Special mention I9101 has attribute DCC, so it requires no VAT number.", false, InvoiceLine.RequiresVATNumberDocument);

			InvoiceLine.AdditionalInfos.RemoveAndDeleteAll();
			AssertEquals(true, InvoiceLine.RequiresVATNumberDocument);

			var specialMentionOnInvoiceHeader = InvoiceHeader.AdditionalInfos.AddNew();
			specialMentionOnInvoiceHeader.CSI_Code = "I9101";
			AssertEquals("Special mention on invoice header still affects.", false, InvoiceLine.RequiresVATNumberDocument);
			InvoiceHeader.AdditionalInfos.RemoveAndDeleteAll();

			var specialMentionOnDeclaration = Declaration.AdditionalInfos.AddNew();
			specialMentionOnDeclaration.CSI_Code = "I9101";
			AssertEquals("Special mention on declaration still affects.", false, InvoiceLine.RequiresVATNumberDocument);

			Declaration.AdditionalInfos.RemoveAndDeleteAll();
			AssertEquals(true, InvoiceLine.RequiresVATNumberDocument);

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals("Exports need no VAT number.", false, InvoiceLine.RequiresVATNumberDocument);
		}

		public void TestRequiresIntoEndUseN990OrC990Document()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.France);
			var procedure1 = helper.CreateOrFindExistingRefCusProcedure(Core.Constants.CountryCodes.France, "", "44", "0", "   ", "", "IMP", "");
			var procedure2 = helper.CreateOrFindExistingRefCusProcedure(Core.Constants.CountryCodes.France, "", "44", "1", "   ", "", "IMP", "");
			helper.CreateRefCusProcedureAttribute(procedure2.PK, "IntoEndUse", "Y");

			Factory.Save();

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_Style = "44";
			InvoiceLine.JI_CEI = entryInstruction.PK;
			var authorizationUsage = entryInstruction.CusAuthorizationUsages.AddNew();
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();

			var authorizationHeader = Factory.New<CusAuthorisationHeader>();
			authorizationHeader.CPH_Number = "1234";
			authorizationHeader.CPH_Type = "EUS";
			authorizationHeader.CPH_IsAdHoc = true;
			authorizationHeader.CPH_OH_PermitHolder = orgHeader.PK;
			authorizationHeader.CPH_IsActive = true;

			authorizationUsage.AGC_CPH_Authorization = authorizationHeader.PK;
			authorizationUsage.AGC_Code = "EUS";

			var rule = authorizationHeader.CusAuthorisationRules.AddNew();
			rule.CPR_RuleCode = CusAuthorisationRuleTypeList.Codes.CON;
			InvoiceLine.JI_FormattedProcedure = "441";

			rule.CPR_ValueFrom = "N990";
			AssertEquals("Procedure 441 (IntoEndUse, CON:N990) requires a document.", true, InvoiceLine.RequiresEndUseN990OrC990Document);

			rule.CPR_ValueFrom = "C990";
			AssertEquals("Procedure 441 (IntoEndUse, CON:C990) requires a document.", true, InvoiceLine.RequiresEndUseN990OrC990Document);

			CombineAssertions("Document should not be required when conditions are not met: (procedure lacks IntoEndUse, invalid CON rule value, non-CON rule, or non-EUS auth).", () =>
			{
				rule.CPR_RuleCode = CusAuthorisationRuleTypeList.Codes.CON;
				rule.CPR_ValueFrom = "N990";
				InvoiceLine.JI_FormattedProcedure = "440";
				AssertEquals("Procedure 440 (no IntoEndUse, CON:N990) should not require a document.", false, InvoiceLine.RequiresEndUseN990OrC990Document);

				rule.CPR_ValueFrom = "C990";
				InvoiceLine.JI_FormattedProcedure = "440";
				AssertEquals("Procedure 440 (no IntoEndUse, CON:C990) should not require a document.", false, InvoiceLine.RequiresEndUseN990OrC990Document);

				rule.CPR_ValueFrom = "1234";
				InvoiceLine.JI_FormattedProcedure = "441";
				AssertEquals("Procedure 441 (IntoEndUse, CON:1234 - invalid) should not require a document.", false, InvoiceLine.RequiresEndUseN990OrC990Document);

				InvoiceLine.JI_FormattedProcedure = "440";
				AssertEquals("Procedure 440 (no IntoEndUse, CON:1234 - invalid) should not require a document.", false, InvoiceLine.RequiresEndUseN990OrC990Document);

				rule.CPR_RuleCode = CusAuthorisationRuleTypeList.Codes.AUT;
				rule.CPR_ValueFrom = "C990";
				InvoiceLine.JI_FormattedProcedure = "441";
				AssertEquals("Procedure 441 (IntoEndUse, non-CON AUT:C990) should not require a document.", false, InvoiceLine.RequiresEndUseN990OrC990Document);

				InvoiceLine.JI_FormattedProcedure = "440";
				AssertEquals("Procedure 440 (no IntoEndUse, non-CON AUT:C990) should not require a document.", false, InvoiceLine.RequiresEndUseN990OrC990Document);

				rule.CPR_ValueFrom = "N990";
				InvoiceLine.JI_FormattedProcedure = "441";
				AssertEquals("Procedure 441 (IntoEndUse, non-CON AUT:N990) should not require a document.", false, InvoiceLine.RequiresEndUseN990OrC990Document);

				InvoiceLine.JI_FormattedProcedure = "440";
				AssertEquals("Procedure 440 (no IntoEndUse, non-CON AUT:N990) should not require a document.", false, InvoiceLine.RequiresEndUseN990OrC990Document);

				authorizationHeader.CPH_Type = "CW1";
				rule.CPR_RuleCode = CusAuthorisationRuleTypeList.Codes.AUT;
				rule.CPR_ValueFrom = "1234";
				InvoiceLine.JI_FormattedProcedure = "441";
				AssertEquals("Procedure 441 (IntoEndUse, CW1 auth, AUT:1234) should not require a document.", false, InvoiceLine.RequiresEndUseN990OrC990Document);

				InvoiceLine.JI_FormattedProcedure = "440";
				AssertEquals("Procedure 440 (no IntoEndUse, CW1 auth, AUT:1234) should not require a document.", false, InvoiceLine.RequiresEndUseN990OrC990Document);
			});
		}

		public void TestManageEndUseN990OrC990Document()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.France);
			var procedure1 = helper.CreateOrFindExistingRefCusProcedure(Core.Constants.CountryCodes.France, "", "44", "0", "   ", "", "IMP", "");
			var procedure2 = helper.CreateOrFindExistingRefCusProcedure(Core.Constants.CountryCodes.France, "", "44", "1", "   ", "", "IMP", "");
			helper.CreateRefCusProcedureAttribute(procedure1.PK, "IntoEndUse", "Y");

			Factory.Save();

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_Style = "44";

			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;

			var authorizationUsage = entryInstruction.CusAuthorizationUsages.AddNew();
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();

			var authorizationHeader = Factory.New<CusAuthorisationHeader>();
			authorizationHeader.CPH_Number = "1234";
			authorizationHeader.CPH_Type = "EUS";
			authorizationHeader.CPH_IsAdHoc = true;
			authorizationHeader.CPH_OH_PermitHolder = orgHeader.PK;
			authorizationHeader.CPH_IsActive = true;
			authorizationHeader.CPH_StartDate = new ZDate(2024, 08, 05);

			var rule = authorizationHeader.CusAuthorisationRules.AddNew();
			rule.CPR_RuleCode = CusAuthorisationRuleTypeList.Codes.CON;
			rule.CPR_ValueFrom = "N990";

			authorizationUsage.AGC_CPH_Authorization = authorizationHeader.PK;
			authorizationUsage.AGC_Code = "EUS";

			invoiceLine.JI_FormattedProcedure = "440";

			AssertEquals("Prerequisite: RequiresEndUseN990OrC990Document should be true when CPC has IntoEndUse attribute and a valid EUS authorization with rule CON:N990 exists.", true, invoiceLine.RequiresEndUseN990OrC990Document);

			var supDoc = invoiceLine.SupportingDocuments.Cast<SupportingDocument>().FirstOrDefault();
			AssertNotNull("Supporting document should be created", supDoc);

			CombineAssertions("Supporting document should be populated when a CPC has IntoEndUse attribute and the associated EUS authorization has a CON rule with value N990/C990", () =>
			{
				AssertEquals("Document type must be 'N990'", "N990", supDoc.CSI_Code);
				AssertEquals("Document reference should match the authorization number", "1234", supDoc.CSI_ReferenceNumber);
				AssertEquals("Document date must match the authorization start date", new ZDate(2024, 08, 05), supDoc.CSI_DateOfIssue);
			});

			invoiceLine.JI_FormattedProcedure = "441";

			AssertEquals("Prerequisite: RequiresEndUseN990OrC990Document should be false when CPC has no IntoEndUse attribute and a valid EUS authorization with rule CON:N990 exists.", false, invoiceLine.RequiresEndUseN990OrC990Document);
			supDoc = invoiceLine.SupportingDocuments.Cast<SupportingDocument>().FirstOrDefault();
			AssertNull("Supporting document should be deleted", supDoc);
		}

		public void TestEffectiveAdditionalInfosCastIsOk()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.France);
			var procedure1 = helper.CreateOrFindExistingRefCusProcedure(Core.Constants.CountryCodes.France, "", "50", "0", "   ", "", "IMP", "");
			var procedure2 = helper.CreateOrFindExistingRefCusProcedure(Core.Constants.CountryCodes.France, "", "42", "0", "   ", "", "IMP", "");
			helper.CreateRefCusProcedureAttribute(procedure2.PK, "VATNumberExempt", "Y");
			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalInformation, "additional info");
			helper.CreateCusCodeList("FR", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalInformation, "70000", "doc 70000", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
			helper.CreateCusCodeListWithAttribute("FR", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalInformation, "I9101", "doc I9101", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6), Core.Constants.Customs.Universal.RefCusCodeList.Attributes.Category, Core.Constants.Customs.Universal.RefCusCodeList.AttributeValues.DCC);
			Factory.Save();

			InvoiceLine.JI_FormattedProcedure = "500";
			var info = Factory.New<EU.Business.Declaration.MultiLineAddInfos.AdditionalInfo>();
			info.CSI_Code = "70000";
			InvoiceLine.EffectiveAdditionalInfos().Add(info);
			var specialMention = InvoiceLine.AdditionalInfos.AddNew();
			specialMention.CSI_Code = "70000";

			AssertType<List<EU.Business.Declaration.MultiLineAddInfos.AdditionalInfo>>(InvoiceLine.EffectiveAdditionalInfos());
			Assert(InvoiceLine.EffectiveAdditionalInfos().Cast<AdditionalInfo>().Any(x => true));
			AssertEquals("Special mention 7000 has no attribute DCC, so it requires a VAT number.", true, InvoiceLine.RequiresVATNumberDocument);
		}

		public void TestAdditionalCodes()
		{
			Declaration.ZG_VATCANACode = "1001";
			AssertContainsExactElementsInExactOrder("FR additional codes.", new[] { "1001" }, InvoiceLine.FRAdditionalCodes);

			var cana1 = InvoiceLine.AdditionalSupplementaryCodes.AddNew();
			cana1.CY_Code = "V911";

			var cana2 = InvoiceLine.AdditionalSupplementaryCodes.AddNew();
			cana2.CY_Code = "V910";

			var caco1 = InvoiceLine.AdditionalSupplementaryCodes.AddNew();
			caco1.CY_Code = "B002";

			var caco2 = InvoiceLine.AdditionalSupplementaryCodes.AddNew();
			caco2.CY_Code = "B001";

			AssertContainsExactElementsInExactOrder("FR additional codes.", new[] { "1001", "V910", "V911" }, InvoiceLine.FRAdditionalCodes);
			AssertContainsExactElementsInAnyOrder("CE additional codes.", new[] { "B001", "B002" }, InvoiceLine.CEAdditionalCodes);
		}

		public void TestIInvoiceLinePartDetailsMembers()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				var partDetails = Factory.New<JobComInvoiceLine>();
				AssertEquals(Core.Constants.CountryCodes.France, partDetails.CustomsCountryCode);
				AssertEquals(typeof(MasterFiles.OrgSupplierPart), partDetails.TypeOfPartUsed);
			}
		}

		public override void TestCountryOfSupplyDefaulting()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals("Pre-Req: DefaultCountryOfSupplyFromSupplier and DefaultCountryOfSupplyFromInvoiceHeader are true for this test to correctly expect a defaulting behavior", true, declaration.Configuration.InvoiceLineConfiguration.DefaultCountryOfSupplyFromSupplier(declaration));

			var invoice = declaration.Invoices.AddNew();
			OrgHeader supplier = Factory.NewWithValidTestData<OrgHeader>();
			supplier.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			declaration.SupplierDocumentaryAddress.OrganisationPK = supplier.PK;
			AssertEquals(Core.Constants.CountryCodes.Australia, invoice.DefaultCountryOfSupply);

			var invoiceLine = invoice.InvoiceLines.AddNew();
			AssertEquals(Core.Constants.CountryCodes.Australia, invoiceLine.ZG_CountryOfSupply);
		}

		public void TestGetNewValidation()
		{
			CombineAssertions(() =>
			{
				Declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaG;
				AssertType<DeltaGJobComInvoiceLineValidation>(InvoiceLine.Validation);
				Declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
				AssertType<DeltaIEJobComInvoiceLineValidation>(InvoiceLine.Validation);
			});
		}

		public void TestGetNewLookups()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaG;
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			AssertType<JobComInvoiceLineLookups>(invoiceLine.Lookups);

			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			AssertType<DeltaIEJobComInvoiceLineLookups>(invoiceLine.Lookups);

			invoiceLine = Factory.New<JobComInvoiceLine>();
			AssertType<JobComInvoiceLineLookups>(invoiceLine.Lookups);
		}

		public void TestSelectedPackType()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;

			AssertEquals(ZString.Empty, invoiceLine.SelectedPackType);

			var package1 = (EU.Business.Declaration.Package)declaration.Bills.AddNew().PackingGroups.AddNew().Packages.AddNew();
			package1.CW_PackType = "CT";
			package1.CW_PackQty = 10;

			var package2 = (EU.Business.Declaration.Package)declaration.Bills.AddNew().PackingGroups.AddNew().Packages.AddNew();
			package2.CW_PackType = "AP";
			package2.CW_PackQty = 2;

			var linkPackageCollection = invoiceLine.PackagesForInvoiceLinesForBindingOnly;
			linkPackageCollection[0].IsLinked = true;
			linkPackageCollection[1].IsLinked = false;

			AssertEquals("CT", linkPackageCollection[0].Package.CW_PackType);
			AssertEquals("AP", linkPackageCollection[1].Package.CW_PackType);

			AssertEquals("CT", invoiceLine.SelectedPackType);
		}

		public void TestEffectiveAssessmentDate()
		{
			var date1 = new ZDateTime(2019, 04, 01);
			var date2 = new ZDateTime(2019, 04, 02);
			var date3 = new ZDateTime(2019, 04, 03);

			var declarationMock = Factory.NewMoq<JobDeclaration>();
			declarationMock.Setup(m => m.DateOfValuation).Returns(date1);

			var declaration = declarationMock.Object;
			var ceiC = declaration.CustomsEntryInstructions.AddNew();
			ceiC.CEI_DateForDuty = date2;
			ceiC.CEI_SubStyle = "C";

			var ceiB = declaration.CustomsEntryInstructions.AddNew();
			ceiB.CEI_DateForDuty = date3;
			ceiB.CEI_SubStyle = "B";

			var ceiD = declaration.CustomsEntryInstructions.AddNew();
			ceiD.CEI_SubStyle = "D";

			var invoice = declarationMock.Object.Invoices.AddNew();
			var invoiceLineC = invoice.InvoiceLines.AddNew();
			invoiceLineC.JI_CEI = ceiC.PK;

			var invoiceLineB = invoice.InvoiceLines.AddNew();
			invoiceLineB.JI_CEI = ceiB.PK;

			var invoiceLineX = invoice.InvoiceLines.AddNew();

			var invoiceLineD = invoice.InvoiceLines.AddNew();
			invoiceLineD.JI_CEI = ceiD.PK;

			AssertEquals("This comes from the mock.", date1, declaration.DateOfValuation);
			AssertEquals("InvoiceHeader.EffectiveValuationDate should be the date on the first entry instruction among all the invoice lines order by sub style.", date3, invoice.EffectiveValuationDate);
			AssertEquals("InvoiceLine.EffectiveAssessmentDate should be the date on the entry instruction it belongs.", date2, invoiceLineC.EffectiveAssessmentDate);
			AssertEquals("InvoiceLine.EffectiveAssessmentDate should be the date on the entry instruction it belongs.", date3, invoiceLineB.EffectiveAssessmentDate);
			AssertEquals("InvoiceLine.EffectiveAssessmentDate should fallback to the InvoiceHeader.EffectiveValuationDate if it doesn't have a entry instruction.", date3, invoiceLineX.EffectiveAssessmentDate);
			AssertEquals("InvoiceLine.EffectiveAssessmentDate should fallback to today if its entry instruction has no date.", ZDateTime.Today, invoiceLineD.EffectiveAssessmentDate);
			declarationMock.VerifyAll();
		}

		public void TestBaseTVA()
		{
			CusRefRateCodeViewTestHelper.CreateAndSaveCusRateCodeForCountryIfNotExists(Factory, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, Universal.Constants.RateTypes.Duty, FeeTypeList.Codes.A00);

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			InvoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.EuropeanUnion;
			InvoiceLine.JI_LinePrice = 1000;
			AssertEquals("pre-condition", InvoiceLine.JI_Calc_ValueForVat, 1000m);

			var entry = Factory.New<CusEntryHeader>();
			entry.CH_JE = Declaration.PK;
			var entryLine = entry.AllEntryLines.AddNew();
			InvoiceLine.JI_CL = entryLine.PK;
			entryLine.Fees.SetAmount(EU.Business.UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts, 200);
			AssertEquals("Pre-condition", 200m, InvoiceLine.JI_Calc_DutyAmount);
			AssertEquals("BaseVATableValue should equal JI_CustomsValue plus JI_Calc_DutyAmountIncludingWHEstimate", 1200m, InvoiceLine.BaseVATableValue);

			InvoiceLine.JI_LinePrice = 0;
			AssertEquals("BaseVATableValue should not be less than zero.", ZDecimal.Zero, InvoiceLine.BaseVATableValue);
		}

		public void TestBaseTVA_Scenario2()
		{
			JobComInvoiceLine invoiceLine = InitBaseTVAInvoiceLine();

			var charge1 = invoiceLine.ApportionedCharges.AddNew();
			charge1.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
			charge1.J7_Amount = 70m;
			charge1.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.EuropeanUnion;
			charge1.J7_IsDutiable = true;
			charge1.J7_IsStatisticalValueApplicable = true;
			charge1.J7_IsGSTApplicable = true;
			charge1.J7_DistributeBy = Common.ChargeDistributeByList.Codes.Value;
			charge1.J7_IsSystem = true;

			var charge2 = invoiceLine.ApportionedCharges.AddNew();
			charge2.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
			charge2.J7_Amount = 12m;
			charge2.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.EuropeanUnion;
			charge2.J7_IsDutiable = false;
			charge2.J7_IsStatisticalValueApplicable = true;
			charge2.J7_IsGSTApplicable = true;
			charge2.J7_DistributeBy = Common.ChargeDistributeByList.Codes.Value;
			charge2.J7_IsSystem = true;

			var charge3 = invoiceLine.ApportionedCharges.AddNew();
			charge3.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
			charge3.J7_Amount = 18m;
			charge3.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.EuropeanUnion;
			charge3.J7_IsDutiable = false;
			charge3.J7_IsStatisticalValueApplicable = false;
			charge3.J7_IsGSTApplicable = true;
			charge3.J7_DistributeBy = Common.ChargeDistributeByList.Codes.Value;
			charge3.J7_IsSystem = true;

			Factory.Save();

			AssertEquals(1100m, invoiceLine.BaseVATableValue);
			AssertEquals(1100m, invoiceLine.JI_Calc_ValueForVat);
		}

		public void TestJI_Calc_InvoicedDocumentaryAmount()
		{
			var declaration = Factory.New<JobDeclaration>();

			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_InvoiceAmount = 1000m;
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 1000m;

			var charges = invoiceLine.Charges;
			var includedApplicableCharge = charges.AddNew(Common.CustomsChargeTypeList.Codes.AdditionCharge, 100m, Core.Constants.CurrencyCodes.UnitedStates);
			includedApplicableCharge.J7_IsDutiable = true;
			includedApplicableCharge.J7_IsGSTApplicable = true;
			includedApplicableCharge.J7_IsStatisticalValueApplicable = true;
			includedApplicableCharge.J7_IsNotIncludedInInvoice = true;

			var cutCharges = charges.AddNew(FRCustomsChargeTypeList.Codes.Cut, 20m, Core.Constants.CurrencyCodes.UnitedStates);
			cutCharges.J7_IsDutiable = true;
			cutCharges.J7_IsGSTApplicable = true;
			cutCharges.J7_IsStatisticalValueApplicable = true;
			cutCharges.J7_IsIncludedInITOT = true;

			var importDutiesCharge = charges.AddNew(UCCCustomsChargeTypeList.Codes.ImportDutiesOrOtherCharge, 28m, Core.Constants.CurrencyCodes.UnitedStates);
			importDutiesCharge.J7_IsDutiable = false;
			importDutiesCharge.J7_IsGSTApplicable = true;
			importDutiesCharge.J7_IsStatisticalValueApplicable = true;
			importDutiesCharge.J7_IsNotIncludedInInvoice = true;

			var apportionedcharges = invoiceLine.ApportionedCharges;
			var overseasInsuranceCharge = apportionedcharges.AddNew(Common.CustomsChargeTypeList.Codes.OverseasInsurance, 120m, Core.Constants.CurrencyCodes.UnitedStates);
			overseasInsuranceCharge.J7_IsDutiable = true;
			overseasInsuranceCharge.J7_IsGSTApplicable = true;
			overseasInsuranceCharge.J7_IsStatisticalValueApplicable = true;
			overseasInsuranceCharge.J7_IsIncludedInITOT = false;

			var overseasFreightCharge = apportionedcharges.AddNew(Common.CustomsChargeTypeList.Codes.OverseasFreight, 1000m, Core.Constants.CurrencyCodes.UnitedStates);
			overseasFreightCharge.J7_IsDutiable = true;
			overseasFreightCharge.J7_IsGSTApplicable = true;
			overseasFreightCharge.J7_IsStatisticalValueApplicable = true;
			overseasFreightCharge.J7_IsIncludedInITOT = false;

			var constructionCharge = charges.AddNew(UCCCustomsChargeTypeList.Codes.ConstructionErectionAssemblyCharge, 67m, Core.Constants.CurrencyCodes.EuropeanUnion);
			constructionCharge.J7_IsDutiable = false;
			constructionCharge.J7_IsGSTApplicable = true;
			constructionCharge.J7_IsStatisticalValueApplicable = false;
			constructionCharge.J7_IsNotIncludedInInvoice = false;

			var adjustedCharge1 = apportionedcharges.AddNew(UCCCustomsChargeTypeList.Codes.AdjustmentCharge, 25m, Core.Constants.CurrencyCodes.EuropeanUnion);
			adjustedCharge1.J7_IsDutiable = false;
			adjustedCharge1.J7_IsGSTApplicable = true;
			adjustedCharge1.J7_IsStatisticalValueApplicable = false;
			adjustedCharge1.J7_IsIncludedInITOT = false;

			var adjustedCharge2 = apportionedcharges.AddNew(UCCCustomsChargeTypeList.Codes.AdjustmentCharge, 80m, Core.Constants.CurrencyCodes.EuropeanUnion);
			adjustedCharge2.J7_IsDutiable = false;
			adjustedCharge2.J7_IsGSTApplicable = true;
			adjustedCharge2.J7_IsStatisticalValueApplicable = false;
			adjustedCharge2.J7_IsIncludedInITOT = true;

			AssertEquals(0m, invoiceLine.JI_Calc_InvoicedDocumentaryAmount.Amount);
			AssertEquals(Core.Constants.CurrencyCodes.EuropeanUnion, invoiceLine.JI_Calc_InvoicedDocumentaryAmount.Currency.Code);

			invoiceLine.InvoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.EuropeanUnion;
			AssertEquals(1057.55m, invoiceLine.JI_Calc_InvoicedDocumentaryAmount.Amount);
			AssertEquals(Core.Constants.CurrencyCodes.EuropeanUnion, invoiceLine.JI_Calc_InvoicedDocumentaryAmount.Currency.Code);

			invoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			constructionCharge.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.Australia;
			adjustedCharge1.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.Australia;

			var expectedResultInEURUsingMoneyInLocalCurrency = new Money(invoiceLine.JI_LinePriceInLocalCurrency + includedApplicableCharge.MoneyInLocalCurrency.Amount - cutCharges.MoneyInLocalCurrency.Amount, RefCurrency.LoadFromCurrencyCode(Factory, Core.Constants.CurrencyCodes.EuropeanUnion));
			var expectedResultOfUSDUsingMoneyInLocalCurrency = invoiceHeader.CurrencyConverter.ConvertExact(expectedResultInEURUsingMoneyInLocalCurrency, RefCurrency.LoadFromCurrencyCode(Factory, Core.Constants.CurrencyCodes.UnitedStates));
			AssertEquals("Using MoneyInLocalCurrency in the middle of the calculation results in precision errors", 1079.99m, expectedResultOfUSDUsingMoneyInLocalCurrency.Amount);
			AssertEquals("JI_Calc_InvoicedDocumentaryAmount should be equal to JI_LinePrice + AdditionCharge - CutCharge", 1080m, invoiceLine.JI_Calc_InvoicedDocumentaryAmount.Amount);
			AssertEquals(Core.Constants.CurrencyCodes.UnitedStates, invoiceLine.JI_Calc_InvoicedDocumentaryAmount.Currency.Code);
		}

		public void TestInvoiceLinePackageValidationType()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			var package1 = (EU.Business.Declaration.Package)declaration.Bills.AddNew().PackingGroups.AddNew().Packages.AddNew();
			var invoiceLine = Factory.New<JobComInvoiceLineForTest>();
			invoiceLine.JI_JZ = invoiceHeader.PK;

			var packing1 = invoiceLine.PackagesForInvoiceLinesForBindingOnly[0];
			AssertType<InvoiceLinePackageValidation>(invoiceLine.GetNewLinkPackValidationCore(packing1));
		}

		public void TestTariffBypassCodeSetter()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();

			invoiceLine.JI_TariffBypassCode = TariffBypassCodeList.Codes.TariffBypass_E;
			AssertEquals(ZString.Empty, invoiceLine.JI_TariffBypassReason);

			invoiceLine.JI_TariffBypassCode = TariffBypassCodeList.Codes.TariffBypass_D;
			AssertEquals(TariffBypassCodeList.Descriptions.TariffBypass_D, invoiceLine.JI_TariffBypassReason);

			invoiceLine.JI_TariffBypassCode = TariffBypassCodeList.Codes.TariffBypass_G;
			AssertEquals(TariffBypassCodeList.Descriptions.TariffBypass_G, invoiceLine.JI_TariffBypassReason);

			invoiceLine.JI_TariffBypassCode = TariffBypassCodeList.Codes.TariffBypass_I;
			AssertEquals(TariffBypassCodeList.Descriptions.TariffBypass_I, invoiceLine.JI_TariffBypassReason);
		}

		public void TestPreviousInbondMovement_Normal()
		{
			var preDoc_Dummy = InvoiceLine.PreviousDocuments.AddNew();
			AssertNull("No Previous Inbond Movement", InvoiceLine.PreviousInbondMovement);

			var preInbondMovement = InvoiceLine.PreviousDocuments.AddNew();
			var entryHeader = Declaration.CustomsEntryHeaders.AddNew();
			entryHeader.EntryNumber = "Normal";
			(preInbondMovement.CSI_Code, preInbondMovement.CSI_SubType, preInbondMovement.CSI_ReferenceNumber) = ("IM", "Z", "Normal");

			CombineAssertions(() =>
			{
				AssertNull("No Previous Inbond Movement for no procedure code", InvoiceLine.PreviousInbondMovement);
				InvoiceLine.JI_Procedure = "1021";

				Declaration.JE_DeltaMode = "G1";
				var firstPreviousInbondMovement = InvoiceLine.PreviousInbondMovement;
				AssertEquals("Number is correctly set for normal entry", "Normal", InvoiceLine.PreviousInbondMovement.Number);
				AssertEquals("Type is correctly set for normal entry", "1", InvoiceLine.PreviousInbondMovement.Type);

				Declaration.JE_DeltaMode = "G2";
				var secondPreviousInbondMovement = InvoiceLine.PreviousInbondMovement;
				AssertEquals("Type is correctly set for fallback entry", "4", InvoiceLine.PreviousInbondMovement.Type);

				AssertEquals("Previous Inbond Movement cached", firstPreviousInbondMovement, secondPreviousInbondMovement);

				foreach (var prevInbondMovementProcCode in new[] { "21", "22", "23", "51", "53", "71", "76", "77", "78" })
				{
					InvoiceLine.JI_Procedure = "10" + prevInbondMovementProcCode;
					AssertNotNull($"Previous Inbond Movement Count for procedure code {prevInbondMovementProcCode}", InvoiceLine.PreviousInbondMovement);
				}

				InvoiceLine.JI_Procedure = "1000";
				AssertNull("No Previous Inbond Movement for wrong procedure code", InvoiceLine.PreviousInbondMovement);
			});
		}

		public void TestPreviousInbondMovement_FallbackAndNotRegularised()
		{
			var preInbondMovement = InvoiceLine.PreviousDocuments.AddNew();
			var entryHeader = Declaration.CustomsEntryHeaders.AddNew();
			entryHeader.DeltaGFallbackStatus = "PPW";
			entryHeader.FRCustomsFallbackNumber = "Fallback";
			(preInbondMovement.CSI_Code, preInbondMovement.CSI_SubType, preInbondMovement.CSI_ReferenceNumber) = ("IM", "Z", "Fallback");

			CombineAssertions(() =>
			{
				AssertNull("No Previous Inbond Movement for no procedure code", InvoiceLine.PreviousInbondMovement);
				InvoiceLine.JI_Procedure = "1021";

				Declaration.JE_DeltaMode = "G1";
				AssertEquals("Number is correctly set for normal entry", "Fallback", InvoiceLine.PreviousInbondMovement.Number);
				AssertEquals("Type is correctly set for normal entry", "2", InvoiceLine.PreviousInbondMovement.Type);

				Declaration.JE_DeltaMode = "G2";
				AssertEquals("Type is correctly set for fallback entry", "5", InvoiceLine.PreviousInbondMovement.Type);

				foreach (var prevInbondMovementProcCode in new[] { "21", "22", "23", "51", "53", "71", "76", "77", "78" })
				{
					InvoiceLine.JI_Procedure = "10" + prevInbondMovementProcCode;
					AssertNotNull($"Previous Inbond Movement Count for procedure code {prevInbondMovementProcCode}", InvoiceLine.PreviousInbondMovement);
				}

				InvoiceLine.JI_Procedure = "1024";
				AssertNull("No Previous Inbond Movement for wrong procedure code", InvoiceLine.PreviousInbondMovement);
			});
		}

		public void TestPreviousInbondMovement_FallbackAndRegularised()
		{
			var preInbondMovement = InvoiceLine.PreviousDocuments.AddNew();
			var entryHeader = Declaration.CustomsEntryHeaders.AddNew();
			entryHeader.DeltaGFallbackStatus = "RGM";
			entryHeader.FRCustomsFallbackNumber = "Fallback";
			(preInbondMovement.CSI_Code, preInbondMovement.CSI_SubType, preInbondMovement.CSI_ReferenceNumber) = ("IM", "Z", "Fallback");

			CombineAssertions(() =>
			{
				AssertNull("No Previous Inbond Movement for no procedure code", InvoiceLine.PreviousInbondMovement);
				InvoiceLine.JI_Procedure = "1021";

				Declaration.JE_DeltaMode = "G1";
				AssertEquals("Number is correctly set for normal entry", "Fallback", InvoiceLine.PreviousInbondMovement.Number);
				AssertEquals("Type is correctly set for normal entry", "1", InvoiceLine.PreviousInbondMovement.Type);

				Declaration.JE_DeltaMode = "G2";
				AssertEquals("Type is correctly set for normal entry", "4", InvoiceLine.PreviousInbondMovement.Type);

				foreach (var prevInbondMovementProcCode in new[] { "21", "22", "23", "51", "53", "71", "76", "77", "78" })
				{
					InvoiceLine.JI_Procedure = "10" + prevInbondMovementProcCode;
					AssertNotNull($"Previous Inbond Movement Count for procedure code {prevInbondMovementProcCode}", InvoiceLine.PreviousInbondMovement);
				}

				InvoiceLine.JI_Procedure = "1024";
				AssertNull("No Previous Inbond Movement for wrong procedure code", InvoiceLine.PreviousInbondMovement);
			});
		}

		public void TestNoPreviousInbondMovement_NoReference()
		{
			InvoiceLine.JI_Procedure = "1021";

			var preInbondMovement_NoReference = InvoiceLine.PreviousDocuments.AddNew();
			(preInbondMovement_NoReference.CSI_Code, preInbondMovement_NoReference.CSI_SubType, preInbondMovement_NoReference.CSI_ReferenceNumber) = ("IM", "Z", "NoReference");
			CombineAssertions(() =>
			{
				Declaration.JE_DeltaMode = "G1";
				AssertNull("No Previous Inbond Movement for no matching reference", InvoiceLine.PreviousInbondMovement);
			});
		}

		public void TestIsBondedWhsQuantityVisible_InFrance()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var procedure = helper.CreateRefCusProcedure(CountryCodes.France, ZString.Empty, "53", "53", "D07", "Import Procedure Group", JobMessageTypeList.Codes.Import, intoWarehouse: false, outOfWarehouse: false);

			using (BaseJobDeclaration.SetupWHSUniversalXMLForTesting())
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.SetSupportsBondedWarehousingForTesting(true);
				var invoice = declaration.Invoices.AddNew();
				var invoiceLine = invoice.InvoiceLines.AddNew();
				invoiceLine.JI_Procedure = "5353D07";

				procedure.ZZ6_IntoWarehouse = WarehouseMoveStatus.Codes.No;
				procedure.ZZ6_OutOfWarehouse = WarehouseMoveStatus.Codes.No;
				procedure.ZZ6_IntoVATWarehouse = WarehouseMoveStatus.Codes.No;
				AssertEquals("Bonded Warehouse Quantity should not be visible when procedure is neither IntoWarehouse, OutOfWarehouse nor IntoVATWarehouse.", false, invoiceLine.IsBondedWhsQuantityVisible);

				procedure.ZZ6_IntoWarehouse = WarehouseMoveStatus.Codes.Yes;
				procedure.ZZ6_OutOfWarehouse = WarehouseMoveStatus.Codes.No;
				AssertEquals("Bonded Warehouse Quantity should be visible when procedure is IntoWarehouse.", true, invoiceLine.IsBondedWhsQuantityVisible);

				procedure.ZZ6_IntoWarehouse = WarehouseMoveStatus.Codes.No;
				procedure.ZZ6_OutOfWarehouse = WarehouseMoveStatus.Codes.Yes;
				AssertEquals("Bonded Warehouse Quantity should be visible when procedure is OutOfWarehouse.", true, invoiceLine.IsBondedWhsQuantityVisible);

				procedure.ZZ6_IntoWarehouse = WarehouseMoveStatus.Codes.No;
				procedure.ZZ6_OutOfWarehouse = WarehouseMoveStatus.Codes.No;
				procedure.ZZ6_IntoVATWarehouse = WarehouseMoveStatus.Codes.Yes;
				AssertEquals("Bonded Warehouse Quantity should be visible when procedure is IntoVATWarehouse.", true, invoiceLine.IsBondedWhsQuantityVisible);
			}
		}

		public void TestLanguageForTariffDescription()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();

			AssertEquals(Core.SharedConstants.Languages.French, invoiceLine.LanguageForTariffDescription);
		}

		public void TestCusAuthorizationUsages()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();

			AssertType<EU.Business.CusAuthorizationUsageCollection<CusAuthorizationUsage, JobComInvoiceLine>>(invoiceLine.CusAuthorizationUsages);
		}

		public void TestHasComponentInventory()
		{
			var invoiceLine = Factory.New<JobComInvoiceLineForTest>();
			AssertEquals("HasComponentInventory is false when ComponentInventoryCollection is empty", false, invoiceLine.HasComponentInventory);

			var whsInventory = Factory.New<IWhsInventoryView>();
			whsInventory.WI_AllocationKey = "WI123";
			var inventory = Factory.New<JobComInvLineComponentInventory>();
			inventory.JIV_AllocationKey = whsInventory.WI_AllocationKey;
			invoiceLine.ComponentInventoryCollection.Add(inventory);
			AssertEquals("HasComponentInventory is true when ComponentInventoryCollection is not empty", true, invoiceLine.HasComponentInventory);
		}

		public void TestJI_PreviousEntryNumberReadOnly()
		{
			var invoiceLine = Factory.New<JobComInvoiceLineForTest>();
			AssertEquals("JI_PreviousEntryNumber is writable when HasComponentInventory is false", false, invoiceLine.JI_PreviousEntryNumberInfo.ReadOnly);

			var whsInventory = Factory.New<IWhsInventoryView>();
			whsInventory.WI_AllocationKey = "WI123";
			var inventory = Factory.New<JobComInvLineComponentInventory>();
			inventory.JIV_AllocationKey = whsInventory.WI_AllocationKey;
			invoiceLine.ComponentInventoryCollection.Add(inventory);
			AssertEquals("JI_PreviousEntryNumber is readonly when HasComponentInventory is true", true, invoiceLine.JI_PreviousEntryNumberInfo.ReadOnly);
		}

		public void TestJI_PreviousEntryLineNumberReadOnly()
		{
			var invoiceLine = Factory.New<JobComInvoiceLineForTest>();
			AssertEquals("JI_PreviousEntryLineNumber is writable when HasComponentInventory is false", false, invoiceLine.JI_PreviousEntryLineNumberInfo.ReadOnly);

			var whsInventory = Factory.New<IWhsInventoryView>();
			whsInventory.WI_AllocationKey = "WI123";
			var inventory = Factory.New<JobComInvLineComponentInventory>();
			inventory.JIV_AllocationKey = whsInventory.WI_AllocationKey;
			invoiceLine.ComponentInventoryCollection.Add(inventory);
			AssertEquals("JI_PreviousEntryLineNumber is readonly when HasComponentInventory is true", true, invoiceLine.JI_PreviousEntryLineNumberInfo.ReadOnly);
		}

		public void TestJI_BondedWhsQuantityReadOnly()
		{
			var invoiceLine = Factory.New<JobComInvoiceLineForTest>();
			AssertEquals("JI_BondedWhsQuantity is writable when HasComponentInventory is false", false, invoiceLine.JI_BondedWhsQuantityInfo.ReadOnly);

			var whsInventory = Factory.New<IWhsInventoryView>();
			whsInventory.WI_AllocationKey = "WI123";
			var inventory = Factory.New<JobComInvLineComponentInventory>();
			inventory.JIV_AllocationKey = whsInventory.WI_AllocationKey;
			invoiceLine.ComponentInventoryCollection.Add(inventory);
			AssertEquals("JI_BondedWhsQuantity is readonly when HasComponentInventory is true", true, invoiceLine.JI_BondedWhsQuantityInfo.ReadOnly);
		}

		public void TestJI_BondedWhsUnitQtyReadOnly()
		{
			var invoiceLine = Factory.New<JobComInvoiceLineForTest>();
			AssertEquals("JI_BondedWhsUnitQty is writable when HasComponentInventory is false", false, invoiceLine.JI_BondedWhsUnitQtyInfo.ReadOnly);

			var whsInventory = Factory.New<IWhsInventoryView>();
			whsInventory.WI_AllocationKey = "WI123";
			var inventory = Factory.New<JobComInvLineComponentInventory>();
			inventory.JIV_AllocationKey = whsInventory.WI_AllocationKey;
			invoiceLine.ComponentInventoryCollection.Add(inventory);
			AssertEquals("JI_BondedWhsUnitQty is readonly when HasComponentInventory is true", true, invoiceLine.JI_BondedWhsUnitQtyInfo.ReadOnly);
		}

		public void TestHasFreeGoods()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			Assert("HasFreeGoods should be false when there is no SupplementaryCode equal to 0097.", !invoiceLine.HasFreeGoods);

			invoiceLine.JI_SupplementaryCode1 = FRConstants.SupplementaryCodes.FreeGoodsSupplementaryCode;
			Assert("HasFreeGoods should be true when JI_SupplementaryCode1 is equal to 0097.", invoiceLine.HasFreeGoods);

			invoiceLine.JI_SupplementaryCode1 = ZString.Empty;
			invoiceLine.JI_SupplementaryCode2 = FRConstants.SupplementaryCodes.FreeGoodsSupplementaryCode;
			Assert("HasFreeGoods should be true when JI_SupplementaryCode2 is equal to 0097.", invoiceLine.HasFreeGoods);

			invoiceLine.JI_SupplementaryCode2 = ZString.Empty;
			invoiceLine.AdditionalSupplementaryCodes.AddNew().CY_Code = FRConstants.SupplementaryCodes.FreeGoodsSupplementaryCode;
			Assert("HasFreeGoods should be true when CY_Code equal to 0097.", invoiceLine.HasFreeGoods);
		}

		public override void TestGetNewLinkPackValidation()
		{
			AssertType<InvoiceLinePackageValidation>(InvoiceLine.PackagesForInvoiceLinesForBindingOnly.AddNew().Validation);
		}

		protected override Type ExpectedTypeOfCharges => typeof(InvoiceLineChargeCollection<InvoiceLineCharge>);

		protected override Type GetExpectedPartType() => typeof(MasterFiles.OrgSupplierPart);

		protected override Type GetExpectedCusEntryLineType() => typeof(CusEntryLine);

		protected override CodeDescriptionPairList GetExpectedCustomsChargeTypeList()
		{
			var customsChargeTypeList = new UCCCustomsChargeTypeList();
			customsChargeTypeList.RemoveCode(UCCCustomsChargeTypeList.Codes.Additions71Charge);
			customsChargeTypeList.RemoveCode(UCCCustomsChargeTypeList.Codes.AirTransportCostsCharge);
			customsChargeTypeList.RemoveCode(UCCCustomsChargeTypeList.Codes.BuyingCommissionsCharge);
			customsChargeTypeList.RemoveCode(UCCCustomsChargeTypeList.Codes.Deductions71Charge);
			customsChargeTypeList.RemoveCode(UCCCustomsChargeTypeList.Codes.DiscountNotElsewhereDeclaredCharge);
			customsChargeTypeList.RemoveCode(UCCCustomsChargeTypeList.Codes.EngineeringDevelopmentArtworkCharge);
			customsChargeTypeList.RemoveCode(UCCCustomsChargeTypeList.Codes.IndirectAndOtherPaymentsCharge);
			customsChargeTypeList.RemoveCode(UCCCustomsChargeTypeList.Codes.InsuranceCostsCharge);
			customsChargeTypeList.RemoveCode(UCCCustomsChargeTypeList.Codes.InterestCharge);
			customsChargeTypeList.RemoveCode(UCCCustomsChargeTypeList.Codes.MaterialsComponentsPartsCharge);
			customsChargeTypeList.RemoveCode(UCCCustomsChargeTypeList.Codes.MaterialsConsumedCharge);
			customsChargeTypeList.RemoveCode(UCCCustomsChargeTypeList.Codes.RightToReproduceCharge);
			customsChargeTypeList.RemoveCode(UCCCustomsChargeTypeList.Codes.ToolsMiesMouldsCharge);
			customsChargeTypeList.RemoveCode(UCCCustomsChargeTypeList.Codes.TransportCostsCharge);
			customsChargeTypeList.AddPair(EU.Business.ChargeTypeList.Codes.StatisticalValue, EU.Business.ChargeTypeList.Descriptions.StatisticalValue);
			customsChargeTypeList.Sort();
			return customsChargeTypeList;
		}

		protected override void CreateEUCountry()
		{
			var refCountry = Factory.New<RefCountry>();
			refCountry.RN_Code = "EU";
			refCountry.RN_Desc = "European Union";

			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.France, "France");
			helper.CreateNewOrGetExistingCusCodeType(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_EX17, "EU Country of destination");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.France, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_EX17, "EU", "European Union", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);

			Factory.Save();
		}

		new JobDeclaration Declaration => (JobDeclaration)base.Declaration;

		new JobComInvoiceHeader InvoiceHeader => (JobComInvoiceHeader)base.InvoiceHeader;

		JobComInvoiceLine InitBaseTVAInvoiceLine()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_OH_Importer = Factory.NewWithValidTestData<OrgHeader>().PK;
			declaration.JE_OH_Supplier = Factory.NewWithValidTestData<OrgHeader>().PK;

			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_InvoiceAmount = 1000m;
			invoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.EuropeanUnion;

			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 1000m;

			return invoiceLine;
		}

		sealed class JobComInvoiceLineForTest : JobComInvoiceLine
		{
			public JobComInvoiceLineForTest(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{ }

			public new Customs.Business.JobComInvoiceLineValidation GetNewValidation() => base.GetNewValidation();

			public new Customs.Business.InvoiceLinePackageValidation GetNewLinkPackValidationCore(BaseCusLinkPackage linkPackage) => base.GetNewLinkPackValidationCore(linkPackage);

			public override bool HasOutOfInwardProcessingProcedure => true;

			public new ZDecimal ComponentPrice => base.ComponentPrice;
		}

		sealed class DummyJobComInvoiceLine_TestCustomsUnitDefaultingStrategy : JobComInvoiceLine
		{
			public DummyJobComInvoiceLine_TestCustomsUnitDefaultingStrategy(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public RateView UniversalDutyRateForReturn { get; set; }

			public IEnumerable<RateView> NationalRatesForReturn { get; set; }

			public override RateView UniversalDutyRate => UniversalDutyRateForReturn;

			public override IEnumerable<RateView> NationalRates => NationalRatesForReturn;
		}
	}
}
