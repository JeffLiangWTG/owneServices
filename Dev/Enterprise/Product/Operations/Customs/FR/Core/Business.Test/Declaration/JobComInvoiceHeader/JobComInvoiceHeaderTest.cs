using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	[TestedType(typeof(JobComInvoiceHeader))]
	class JobComInvoiceHeaderTest : EU.Business.Declaration.Testing.JobComInvoiceHeaderTest<JobDeclaration, JobComInvoiceHeader, JobComInvoiceLine>
	{
		public override void TestCFRCalculationWithFreightAdjustedFlag()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_ExportDate = new ZDateTime(2004, 10, 30);
			var invoice = dec.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();

			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.CostAndFreight;
			invoice.JZ_InvoiceAmount = 10500m;
			invoice.JZ_RX_NKInvoice_Currency = dec.LocalCurrencyCode;

			var oFT = invoice.Charges.AddNew();
			oFT.J7_ChargeType = OverseasFreightCode;
			oFT.J7_Amount = 500m;
			oFT.J7_RX_NKCurrency = dec.LocalCurrencyCode;
			oFT.J7_IsDutiable = true;
			oFT.J7_IsIncludedInITOT = false;

			var adjustedOFT = invoice.Charges.AddNew();
			adjustedOFT.J7_ChargeType = OverseasFreightCode;
			adjustedOFT.J7_Amount = 300m;
			adjustedOFT.J7_RX_NKCurrency = dec.LocalCurrencyCode;
			adjustedOFT.J7_AdjustedCharge = true;
			adjustedOFT.J7_IsIncludedInITOT = false;

			AssertEquals("InvoiceLineTotal should only take charges without adjusted on", 10000m, invoice.InvoiceLineTotal);
			AssertEquals("FOB", 10500m, invoice.JZ_Calc_FOBAmount);
			AssertEquals("CIF calculation should exclude non vatible charges.", 9700m, invoice.JZ_Calc_CIFAmount);

			oFT.J7_IsIncludedInITOT = true;
			adjustedOFT.J7_IsIncludedInITOT = true;

			AssertEquals("InvoiceLineTotal should only take charges without adjusted on", 10500m, invoice.InvoiceLineTotal);
			AssertEquals("FOB", 10500m, invoice.JZ_Calc_FOBAmount);
			AssertEquals("CIF calculation should exclude non vatible adjusted charges.", 10200m, invoice.JZ_Calc_CIFAmount);
		}

		public void TestDefaultDataGrouping()
		{
			foreach (var country in Core.Constants.CountryCodes.FranceAndOverseasDepartmentsUnderItsCustomsJurisdiction)
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(country))
				{
					var invoice = Factory.New<JobDeclaration>().Invoices.AddNew();
					CombineAssertions($"Default data groupings for {country}", () =>
					{
						AssertEquals("Default data grouping.", Core.Constants.CountryCodes.France, invoice.GetDefaultDataGroupingCode());
						AssertEquals("Default tariff data grouping.", Core.Constants.CountryCodes.France, invoice.GetDefaultDataGroupingCode(DefaultDataGroupingType.Tariff));
						AssertEquals("Default duties data grouping.", Core.Constants.CountryCodes.France, invoice.GetDefaultDataGroupingCode(DefaultDataGroupingType.DutyRateCodes));
					});
				}
			}
		}

		public void TestJZ_OH_BuyerChanged()
		{
			var (invoice, link) = PrepareDataForPopulateCharges();
			AssertEquals(1, invoice.GroupHeader.Charges.Count);

			invoice.JZ_OH_Buyer = ZGuid.Empty;
			AssertEquals(0, invoice.GroupHeader.Charges.Count);

			invoice.JZ_OH_Buyer = link.OL_OH_Buyer;
			AssertEquals(1, invoice.GroupHeader.Charges.Count);
		}

		public void TestJZ_OH_SupplierChanged()
		{
			var (invoice, link) = PrepareDataForPopulateCharges();
			AssertEquals(1, invoice.GroupHeader.Charges.Count);

			invoice.JZ_OH_Supplier = ZGuid.Empty;
			AssertEquals(0, invoice.GroupHeader.Charges.Count);

			invoice.JZ_OH_Supplier = link.OL_OH_Supplier;
			AssertEquals(1, invoice.GroupHeader.Charges.Count);
		}

		public void TestJZ_InvoiceAmountChanged()
		{
			var (invoice, link) = PrepareDataForPopulateCharges();
			AssertEquals(1, invoice.GroupHeader.Charges.Count);
			var charge = invoice.GroupHeader.Charges.Cast<GroupInvoiceCharge>().FirstOrDefault();
			AssertEquals(10m, charge.J7_Amount);

			invoice.JZ_InvoiceAmount = 200m;
			AssertEquals(20m, charge.J7_Amount);
		}

		public void TestJZ_RX_NKInvoice_CurrencyChanged()
		{
			var (invoice, link) = PrepareDataForPopulateCharges();
			AssertEquals(1, invoice.GroupHeader.Charges.Count);
			var charge = invoice.GroupHeader.Charges.Cast<GroupInvoiceCharge>().FirstOrDefault();
			AssertEquals(Core.Constants.CurrencyCodes.UnitedStates, charge.J7_RX_NKCurrency);

			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.EuropeanUnion;
			AssertEquals(Core.Constants.CurrencyCodes.EuropeanUnion, charge.J7_RX_NKCurrency);
		}

		(JobComInvoiceHeader, OrgSupplierBuyerLink) PrepareDataForPopulateCharges()
		{
			var link = Factory.NewWithValidTestData<OrgSupplierBuyerLink>();
			link.OL_RN_NKImporterCountry = Core.Constants.CountryCodes.Australia;
			link.OL_InsuranceUplift = 10m;
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_GB = GlbBranch.CurrentBranch.PK;
			declaration.JE_RL_NKFinalDestination = Core.Constants.CountryCodes.Australia;
			var groupInvoiceHeader = declaration.TopGroupInvoice;

			var invoice1 = groupInvoiceHeader.AllJobComInvoiceHeaders.AddNew() as JobComInvoiceHeader;
			invoice1.JZ_OH_Supplier = link.OL_OH_Supplier;
			invoice1.JZ_OH_Buyer = link.OL_OH_Buyer;
			invoice1.JZ_InvoiceAmount = 100m;
			invoice1.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			return (invoice1, link);
		}

		public override void TestDefaultCountryOfSupply()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
			AssertEquals("Pre-Req: DefaultCountryOfSupplyFromSupplier and DefaultCountryOfSupplyFromInvoiceHeader are true for this test to correctly expect a defaulting behavior", true, declaration.Configuration.InvoiceLineConfiguration.DefaultCountryOfSupplyFromSupplier(declaration));

			var invoice = declaration.Invoices.AddNew();
			AssertEquals("DefaultCountryOfSupply should return an empty string when both declaration and invoice header show no supplier.", ZString.Empty, invoice.DefaultCountryOfSupply);

			OrgHeader supplier = Factory.NewWithValidTestData<OrgHeader>();
			supplier.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			declaration.SupplierDocumentaryAddress.OrganisationPK = supplier.PK;
			AssertEquals("DefaultCountryOfSupply should not return an empty string when its declaration has its supplier country defined, because DefaultCountryOfSupplyFromSupplier is activated in FR solution", Core.Constants.CountryCodes.Australia, invoice.DefaultCountryOfSupply);

			declaration.SupplierDocumentaryAddress.OrganisationPK = ZGuid.Empty;
			invoice.JZ_OA_SupplierAddress = supplier.MainAddress.PK;
			AssertEquals("DefaultCountryOfSupply should not return an empty string when the invoice has its supplier country defined, because DefaultCountryOfSupplyFromInvoiceHeader is activated in EU solution", Core.Constants.CountryCodes.Australia, invoice.DefaultCountryOfSupply);
		}

		public void TestGetStandaloneIncoTermAndChargeFactoryCountryContext()
		{
			CombineAssertions(() =>
			{
				var declaration = Factory.New<JobDeclaration>();
				var invoiceHeader = Factory.New<InvoiceHeaderForTesting>();
				invoiceHeader.JZ_JE = declaration.PK;
				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
				AssertEquals("Import", "FR", invoiceHeader.GetStandaloneIncoTermAndChargeFactoryCountryContext());

				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
				AssertEquals("Export", "FREXP", invoiceHeader.GetStandaloneIncoTermAndChargeFactoryCountryContext());
			});
		}

		public override string GetLocalCurrencyCode() => Core.Constants.CurrencyCodes.France;

		public void TestEffectiveValuationDate_ShouldReturnCEI_DateForDutyFromTheEntryInstruction()
		{
			var date = new ZDateTime(2019, 04, 01);

			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_DateForDuty = date;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;

			AssertEquals(date, invoice.EffectiveValuationDate);
		}

		public void TestEffectiveValuationDate_ShouldReturnTodayIfNoEntryInstructionFound()
		{
			var date = new ZDateTime(2019, 04, 01);
			var today = ZDateTime.Today;

			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_DateForDuty = date;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();

			AssertEquals(today, invoice.EffectiveValuationDate);
		}

		public void TestEffectiveValuationDate_ShouldReturnCEI_DateForDutyFromTheFirstEntryInstruction_OrderByCEI_SubStyle()
		{
			var date1 = new ZDateTime(2019, 04, 01);
			var date2 = new ZDateTime(2019, 04, 02);

			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction1.CEI_SubStyle = "B";
			entryInstruction1.CEI_DateForDuty = date1;

			var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction2.CEI_SubStyle = "A";
			entryInstruction2.CEI_DateForDuty = date2;

			var invoice = declaration.Invoices.AddNew();

			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			invoiceLine1.JI_CEI = entryInstruction2.PK;

			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_CEI = entryInstruction1.PK;

			var invoiceLine3 = invoice.InvoiceLines.AddNew();

			AssertEquals(date2, invoice.EffectiveValuationDate);
		}

		public void TestIncotermPlaceResetWhenIncotermChanges()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_IncoTermPlace = "BERGERAC";
			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			AssertEquals("JZ_IncoTermPlace should be empty when JZ_IncoTerm changes", ZString.Empty, invoice.JZ_IncoTermPlace);
		}

		public void TestIncotermPlaceCodeRecalculationWhenIncotermChanges()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_IncoTermPlace = "BERGERAC";
			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			AssertEquals("JZ_IncoTermPlaceCode should be reclaclulated when JZ_IncoTerm changes", "3", invoice.ZG_AgreedPlaceCode);
		}

		public void TestValuationMethodeDefaultValue()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			AssertEquals("1", invoice.ZG_ValuationMethod);
		}

		public void TestValuationMethodeValueInJZ_AddInfoIsputInTheInvoiceLine()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			AssertEquals("1", invoice.ZG_ValuationMethod);
			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			AssertEquals("1", invoiceLine1.JI_ValuationCode);
			invoice.ZG_ValuationMethod = "2";
			AssertEquals("2", invoiceLine1.JI_ValuationCode);

			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			AssertEquals("2", invoiceLine2.JI_ValuationCode);

			invoiceLine1.JI_ValuationCode = "1";

			invoice.ZG_ValuationMethod = "3";
			AssertEquals("3", invoiceLine2.JI_ValuationCode);
			AssertEquals("1", invoiceLine1.JI_ValuationCode);
		}

		public void TestPopulateCharges()
		{
			var link = Factory.NewWithValidTestData<OrgSupplierBuyerLink>();
			link.OL_RN_NKImporterCountry = GlbBranch.CurrentBranch.Country.Code;
			link.OL_BuyingCommissionPercentage = 1.22m;
			link.OL_RoyaltyPercentage = 2.33m;
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Supplier = link.OL_OH_Supplier;
			declaration.JE_OH_Buyer = link.OL_OH_Buyer;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_GB = GlbBranch.CurrentBranch.PK;
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.Charges.RemoveAndDeleteAll();

			invoiceHeader.JZ_OH_Buyer = ZGuid.Empty;
			invoiceHeader.JZ_OH_Buyer = link.OL_OH_Buyer;
			AssertEquals("Should have called PopulateInvoiceCharges for invoices changing JZ_OH_Buyer.", true, invoiceHeader.Charges.Count > 0);

			invoiceHeader.Charges.RemoveAndDeleteAll();
			invoiceHeader.JZ_OH_Supplier = ZGuid.Empty;
			invoiceHeader.JZ_OH_Supplier = link.OL_OH_Supplier;
			AssertEquals("Should have called PopulateInvoiceCharges for invoices changing JZ_OH_Supplier.", true, invoiceHeader.Charges.Count > 0);
		}

		public void TestPopulateCharges_NoPopulateForStandAloneInvoice()
		{
			var link = Factory.NewWithValidTestData<OrgSupplierBuyerLink>();
			link.OL_RN_NKImporterCountry = GlbBranch.CurrentBranch.Country.Code;
			link.OL_BuyingCommissionPercentage = 1.22m;
			link.OL_RoyaltyPercentage = 2.33m;

			var invoiceHeader = Factory.New<JobComInvoiceHeader>();
			invoiceHeader.JZ_OH_Supplier = link.OL_OH_Supplier;
			invoiceHeader.JZ_OH_Buyer = link.OL_OH_Buyer;
			AssertEquals("Should not populate charges for standalone InvoiceHeader.", false, invoiceHeader.Charges.Count > 0);
		}

		public void TestChargesType()
		{
			var invoiceHeader = Factory.New<JobComInvoiceHeader>();
			AssertType<InvoiceChargeCollection<InvoiceCharge>>("Should have created a new Charges property of Enterprise.Customs.FR.Business.Declaration.InvoiceChargeCollection", invoiceHeader.Charges);
		}

		protected override CodeDescriptionPairList GetExpectedCustomsChargeTypeList()
		{
			var customsChargeTypeList = new UCCCustomsChargeTypeList();
			customsChargeTypeList.RemoveCode(UCCCustomsChargeTypeList.Codes.Additions71Charge);
			customsChargeTypeList.RemoveCode(UCCCustomsChargeTypeList.Codes.AirTransportCostsCharge);
			customsChargeTypeList.RemoveCode(UCCCustomsChargeTypeList.Codes.Deductions71Charge);
			customsChargeTypeList.RemoveCode(UCCCustomsChargeTypeList.Codes.DiscountNotElsewhereDeclaredCharge);
			customsChargeTypeList.RemoveCode(UCCCustomsChargeTypeList.Codes.IndirectAndOtherPaymentsCharge);
			customsChargeTypeList.RemoveCode(UCCCustomsChargeTypeList.Codes.InsuranceCostsCharge);
			customsChargeTypeList.RemoveCode(UCCCustomsChargeTypeList.Codes.RightToReproduceCharge);
			customsChargeTypeList.RemoveCode(UCCCustomsChargeTypeList.Codes.TransportCostsCharge);
			customsChargeTypeList.RemoveCode(UCCCustomsChargeTypeList.Codes.ToolsMiesMouldsCharge);
			customsChargeTypeList.AddPair(FRCustomsChargeTypeList.Codes.ExclusiveFreightInsideEU, FRCustomsChargeTypeList.Descriptions.ExclusiveFreightInsideEU);
			customsChargeTypeList.AddPair(FRCustomsChargeTypeList.Codes.ExclusiveFreightToFrenchDestination, FRCustomsChargeTypeList.Descriptions.ExclusiveFreightToFrenchDestination);
			customsChargeTypeList.AddPair(FRCustomsChargeTypeList.Codes.ExclusiveInsuranceInsideEU, FRCustomsChargeTypeList.Descriptions.ExclusiveInsuranceInsideEU);
			customsChargeTypeList.AddPair(FRCustomsChargeTypeList.Codes.ExclusiveInsuranceToFrenchDestination, FRCustomsChargeTypeList.Descriptions.ExclusiveInsuranceToFrenchDestination);
			customsChargeTypeList.AddPair(FRCustomsChargeTypeList.Codes.InclusiveFreightFromFrenchBorder, FRCustomsChargeTypeList.Descriptions.InclusiveFreightFromFrenchBorder);
			customsChargeTypeList.AddPair(FRCustomsChargeTypeList.Codes.InclusiveFreightInsideEU, FRCustomsChargeTypeList.Descriptions.InclusiveFreightInsideEU);
			customsChargeTypeList.AddPair(FRCustomsChargeTypeList.Codes.InclusiveInsuranceFromFrenchBorder, FRCustomsChargeTypeList.Descriptions.InclusiveInsuranceFromFrenchBorder);
			customsChargeTypeList.AddPair(FRCustomsChargeTypeList.Codes.InclusiveInsuranceInsideEU, FRCustomsChargeTypeList.Descriptions.InclusiveInsuranceInsideEU);
			customsChargeTypeList.AddPair(FRCustomsChargeTypeList.Codes.Cut, FRCustomsChargeTypeList.Descriptions.Cut);
			customsChargeTypeList.Sort();
			return customsChargeTypeList;
		}

		public void TestSupplierOrgPKDefaulting()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = Factory.New<InvoiceHeaderForTesting>();
			var supplier = Factory.NewWithValidTestData<OrgHeader>();
			invoiceHeader.JZ_OH_Supplier = supplier.PK;

			invoiceHeader.JZ_JE = declaration.PK;
			AssertEquals(invoiceHeader.JZ_OH_Supplier, invoiceHeader.SupplierOrgPK);
			AssertEquals(invoiceHeader.SupplierOrgPK, invoiceHeader.JZ_OA_SupplierAddress_ZAddress.OrgPK);
		}

		public void TestGetNewValidation()
		{
			CombineAssertions(() =>
			{
				declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaG;
				AssertType<DeltaGJobComInvoiceHeaderValidation>(invoiceHeader.Validation);
				declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
				AssertType<DeltaIEJobComInvoiceHeaderValidation>(invoiceHeader.Validation);
			});
		}

		public void TestGetNewLookups()
		{
			CombineAssertions(() =>
			{
				declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaG;
				AssertType<DeltaGJobComInvoiceHeaderLookups>(invoiceHeader.Lookups);
				declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
				AssertType<DeltaIEJobComInvoiceHeaderLookups>(invoiceHeader.Lookups);
			});
		}

		public void TestIsProvisonalAmountAuthorised()
		{
			var invoiceHeader = Factory.NewWithValidTestData<JobComInvoiceHeader>();
			AssertEquals("IsProvisonalAmountAuthorised is false when no relevant data exist", false, invoiceHeader.IsProvisonalAmountAuthorised);

			CombineAssertions("Test InvoiceHeader.AdditionalInfos", () =>
			{
				var item = invoiceHeader.AdditionalInfos.AddNew();
				item.CSI_Code = FRConstants.DocumentCodes.ProvisonalAmountAuthorisedDocumentCode;
				AssertEquals("IsProvisonalAmountAuthorised is true when CSI_Code is 1AVP", true, invoiceHeader.IsProvisonalAmountAuthorised);

				item.CSI_Code = "ABC";
				AssertEquals("IsProvisonalAmountAuthorised is false when CSI_Code is not 1AVP", false, invoiceHeader.IsProvisonalAmountAuthorised);

				invoiceHeader.AdditionalInfos.RemoveAndDelete(item);
			});

			CombineAssertions("Test Declaration.AdditionalInfos", () =>
			{
				var item = invoiceHeader.JobDeclaration.AdditionalInfos.AddNew();
				item.CSI_Code = FRConstants.DocumentCodes.ProvisonalAmountAuthorisedDocumentCode;
				AssertEquals("IsProvisonalAmountAuthorised is true when CSI_Code is 1AVP", true, invoiceHeader.IsProvisonalAmountAuthorised);

				item.CSI_Code = "ABC";
				AssertEquals("IsProvisonalAmountAuthorised is false when CSI_Code is not 1AVP", false, invoiceHeader.IsProvisonalAmountAuthorised);

				invoiceHeader.JobDeclaration.AdditionalInfos.RemoveAndDelete(item);
			});

			CombineAssertions("Test InvoiceLine.AdditionalInfos", () =>
			{
				var line = invoiceHeader.InvoiceLines.AddNew();
				var item = line.AdditionalInfos.AddNew();
				item.CSI_Code = FRConstants.DocumentCodes.ProvisonalAmountAuthorisedDocumentCode;
				AssertEquals("IsProvisonalAmountAuthorised is true when CSI_Code is 1AVP", true, invoiceHeader.IsProvisonalAmountAuthorised);

				item.CSI_Code = "ABC";
				AssertEquals("IsProvisonalAmountAuthorised is false when CSI_Code is not 1AVP", false, invoiceHeader.IsProvisonalAmountAuthorised);

				invoiceHeader.JobDeclaration.InvoiceLines.RemoveAndDelete(line);
			});

			CombineAssertions("Test InvoiceHeader.SupportingDocuments", () =>
			{
				var item = invoiceHeader.SupportingDocuments.AddNew();
				item.CSI_Code = FRConstants.DocumentCodes.ProvisonalAmountAuthorisedDocumentCode;
				AssertEquals("IsProvisonalAmountAuthorised is true when CSI_Code is 1AVP", true, invoiceHeader.IsProvisonalAmountAuthorised);

				item.CSI_Code = "ABC";
				AssertEquals("IsProvisonalAmountAuthorised is false when CSI_Code is not 1AVP", false, invoiceHeader.IsProvisonalAmountAuthorised);

				invoiceHeader.SupportingDocuments.RemoveAndDelete(item);
			});

			CombineAssertions("Test Declaration.SupportingDocuments", () =>
			{
				invoiceHeader = Factory.NewWithValidTestData<JobComInvoiceHeader>();
				var item = invoiceHeader.JobDeclaration.SupportingDocuments.AddNew();
				item.CSI_Code = FRConstants.DocumentCodes.ProvisonalAmountAuthorisedDocumentCode;
				AssertEquals("IsProvisonalAmountAuthorised is true when CSI_Code is 1AVP", true, invoiceHeader.IsProvisonalAmountAuthorised);

				item.CSI_Code = "ABC";
				AssertEquals("IsProvisonalAmountAuthorised is false when CSI_Code is not 1AVP", false, invoiceHeader.IsProvisonalAmountAuthorised);

				invoiceHeader.JobDeclaration.SupportingDocuments.RemoveAndDelete(item);
			});

			CombineAssertions("Test InvoiceLine.SupportingDocuments", () =>
			{
				invoiceHeader = Factory.NewWithValidTestData<JobComInvoiceHeader>();
				var line = invoiceHeader.InvoiceLines.AddNew();
				var item = line.SupportingDocuments.AddNew();
				item.CSI_Code = FRConstants.DocumentCodes.ProvisonalAmountAuthorisedDocumentCode;
				AssertEquals("IsProvisonalAmountAuthorised is true when CSI_Code is 1AVP", true, invoiceHeader.IsProvisonalAmountAuthorised);

				item.CSI_Code = "ABC";
				AssertEquals("IsProvisonalAmountAuthorised is false when CSI_Code is not 1AVP", false, invoiceHeader.IsProvisonalAmountAuthorised);
			});

			CombineAssertions("Test multiple items", () =>
			{
				var invoiceHeader = Factory.NewWithValidTestData<JobComInvoiceHeader>();

				var item1 = invoiceHeader.AdditionalInfos.AddNew();
				var item2 = invoiceHeader.JobDeclaration.AdditionalInfos.AddNew();
				var item3 = invoiceHeader.InvoiceLines.AddNew().AdditionalInfos.AddNew();
				var item4 = invoiceHeader.SupportingDocuments.AddNew();

				item1.CSI_Code = FRConstants.DocumentCodes.ProvisonalAmountAuthorisedDocumentCode;
				item2.CSI_Code = FRConstants.DocumentCodes.ProvisonalAmountAuthorisedDocumentCode;
				item3.CSI_Code = FRConstants.DocumentCodes.ProvisonalAmountAuthorisedDocumentCode;
				item4.CSI_Code = FRConstants.DocumentCodes.ProvisonalAmountAuthorisedDocumentCode;
				AssertEquals("IsProvisonalAmountAuthorised is true when all relevant items have CSI_Code = 1AVP", true, invoiceHeader.IsProvisonalAmountAuthorised);

				item1.CSI_Code = FRConstants.DocumentCodes.ProvisonalAmountAuthorisedDocumentCode;
				item2.CSI_Code = "ABC";
				item3.CSI_Code = "XYZ";
				item4.CSI_Code = "123";
				AssertEquals("IsProvisonalAmountAuthorised is true when at least one relevant item has CSI_Code = 1AVP", true, invoiceHeader.IsProvisonalAmountAuthorised);

				item1.CSI_Code = ZString.Empty;
				AssertEquals("IsProvisonalAmountAuthorised is false when no relevant item has CSI_Code = 1AVP", false, invoiceHeader.IsProvisonalAmountAuthorised);
			});
		}

		public void TestHasFreeGoods()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			Assert("HasFreeGoods should be false when there is no invoiceLine.", !invoice.HasFreeGoods);

			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			Assert("Prerequisite: HasFreeGoods should be false as invoiceLine1 has no SupplementaryCode equal to 0097.", !invoiceLine1.HasFreeGoods);
			Assert("Prerequisite: HasFreeGoods should be false as invoiceLine2 has no SupplementaryCode equal to 0097.", !invoiceLine2.HasFreeGoods);
			Assert("HasFreeGoods should be false if no invoiceLine has SupplementaryCode equal to 0097.", !invoice.HasFreeGoods);

			invoiceLine2.JI_SupplementaryCode1 = FRConstants.SupplementaryCodes.FreeGoodsSupplementaryCode;
			Assert("Prerequisite: HasFreeGoods should be false as invoiceLine1 has no SupplementaryCode equal to 0097.", !invoiceLine1.HasFreeGoods);
			Assert("Prerequisite: HasFreeGoods should be true as invoiceLine2 has SupplementaryCode equal to 0097.", invoiceLine2.HasFreeGoods);
			Assert("HasFreeGoods should be true if any associated invoiceLine has a SupplementaryCode equal to 0097.", invoice.HasFreeGoods);
		}

		protected override BaseJobDeclaration GetNewDeclaration() => Factory.New<JobDeclaration>();

		protected override void AssertDefaultDocumentWhenApplicationCodeChanges(EU.Business.Declaration.JobDeclaration declaration, EU.Business.Declaration.JobComInvoiceHeader invoice, string messageType, string defaultInvoiceDocumentCode)
		{
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			invoice.JZ_InvoiceNumber = "6";
			AssertEquals($"{messageType}, In France, we add {defaultInvoiceDocumentCode} document without considering the application code.", 1, invoice.SupportingDocuments.Count);
		}

		protected override Type ExpectedTypeOfCharges => typeof(InvoiceChargeCollection<InvoiceCharge>);

		protected override IEnumerable<Func<BaseJobDeclaration, JobComInvoiceHeader, BaseJobComInvoiceLine, (ZPropertyInfo, IZType)>> GetJZ_Calc_LinesEnteredRelatedProperties()
		{
			return base.GetJZ_Calc_LinesEnteredRelatedProperties();
		}

		class InvoiceHeaderForTesting : JobComInvoiceHeader
		{
			public InvoiceHeaderForTesting(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public new string GetStandaloneIncoTermAndChargeFactoryCountryContext() => base.GetStandaloneIncoTermAndChargeFactoryCountryContext();
		}
	}
}
