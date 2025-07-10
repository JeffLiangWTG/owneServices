using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.Customs.FR.MessageDefinitions.DeltaIE.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business;
using static Enterprise.Customs.FR.Business.UniversalReferenceConstants;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using JobDeclaration = Enterprise.Customs.FR.Business.Declaration.JobDeclaration;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.DeltaIE.Testing
{
	class GoodsShipmentWrapperTest : Customs.Business.Testing.DataProviderTestCase<GoodsShipmentWrapper>
	{
		protected override GoodsShipmentWrapper GetProvider()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eun = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(GlbCompany.CurrentCompany.Country.Code, GlbCompany.CurrentCompany.Country.RN_Desc, eun);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalInformation, "Additional Information");
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("Direction", "Desc.", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalInformation, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("Level", "Desc.", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalInformation, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			var cusCode1 = helper.CreateCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalInformation, "INF1", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			cusCode1.Attributes.AddNew("Direction", "IMPORT");
			cusCode1.Attributes.AddNew("Direction", "EXPORT");
			cusCode1.Attributes.AddNew("Level", "HEADER");

			var cusCode2 = helper.CreateCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalInformation, "INF2", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			cusCode2.Attributes.AddNew("Direction", "IMPORT");
			cusCode2.Attributes.AddNew("Direction", "EXPORT");
			cusCode2.Attributes.AddNew("Level", "HEADER");

			Factory.Save();

			var buyer = Factory.New<OrgHeader>();
			buyer.FillWithValidTestData();
			buyer.OH_FullName = "Buyer";
			buyer.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Seychelles;

			var supplier = Factory.New<OrgHeader>();
			supplier.FillWithValidTestData();
			supplier.OH_FullName = "Supplier";
			supplier.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.France;

			var seller = Factory.New<OrgHeader>();
			seller.FillWithValidTestData();
			seller.OH_FullName = "Seller";
			seller.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Mauritius;

			var consignee = Factory.New<OrgHeader>();
			consignee.FillWithValidTestData();
			consignee.OH_FullName = "Consignee";
			consignee.MainAddress.City = "City";
			consignee.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Macedonia;
			consignee.MainAddress.Postcode = "MACD1234";

			var importer = Factory.New<OrgHeader>();
			importer.OH_Code = "IMPORTER";

			var cusAccount = Factory.New<OrgCusAccount>();
			cusAccount.CZ_OH = importer.PK;
			cusAccount.CZ_Code = "DEC";
			cusAccount.CZ_Account = "DEC111";
			cusAccount.CZ_RN_NKCountryCode = Core.Constants.CountryCodes.France;
			cusAccount.CZ_Type = OrgCusAccountDeltaIETypeList.Codes.DCN;

			var authorisationHeader = Factory.New<CusAuthorisationHeader>();
			authorisationHeader.FillWithValidTestData();
			authorisationHeader.CPH_OH_PermitHolder = supplier.PK;
			authorisationHeader.CPH_OA_AppliesTo = supplier.MainAddress.PK;
			authorisationHeader.CPH_Number = "AUTH1";
			authorisationHeader.CPH_Type = Customs.Business.CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1;

			var rule = authorisationHeader.CusAuthorisationRules.AddNew();
			rule.CPR_RuleCode = "USE";
			rule.CPR_ValueFrom = "OTH";

			var rule2 = authorisationHeader.CusAuthorisationRules.AddNew();
			rule2.CPR_RuleCode = "CAN";
			rule2.CPR_ValueFrom = "ATH";

			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_GoodsDestination = Core.Constants.CountryCodes.UnitedKingdom;
			declaration.JE_OH_Buyer = buyer.PK;
			declaration.JE_GoodsOrigin = Core.Constants.CountryCodes.UnitedStates;
			declaration.JE_MessageType = StatementEntryTypeImpExpList.Codes.Import;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_CustomsProfile = "DEC111";

			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_SubStyle = "V";

			var informationAdditionalInfo1 = declaration.AdditionalInfos.AddNew();
			informationAdditionalInfo1.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
			informationAdditionalInfo1.CSI_Code = "INF1";
			informationAdditionalInfo1.CSI_Description = "INFDecription1";

			var informationAdditionalInfo2 = declaration.AdditionalInfos.AddNew();
			informationAdditionalInfo2.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
			informationAdditionalInfo2.CSI_Code = "INF1";
			informationAdditionalInfo2.CSI_Description = "INFDecription1";

			var informationAdditionalInfo3 = instruction.AdditionalInfos.AddNew();
			informationAdditionalInfo3.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
			informationAdditionalInfo3.CSI_Code = "INF1";
			informationAdditionalInfo3.CSI_Description = "INFDecription1";

			var informationAdditionalInfo4 = instruction.AdditionalInfos.AddNew();
			informationAdditionalInfo4.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
			informationAdditionalInfo4.CSI_Code = "INF3";
			informationAdditionalInfo4.CSI_Description = "INFDecription1";

			var referenceAdditionalInfo1 = declaration.AdditionalInfos.AddNew();
			referenceAdditionalInfo1.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
			referenceAdditionalInfo1.CSI_Code = "CD1";
			referenceAdditionalInfo1.CSI_ReferenceNumber = "REF1";

			var referenceAdditionalInfo2 = declaration.AdditionalInfos.AddNew();
			referenceAdditionalInfo2.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
			referenceAdditionalInfo2.CSI_Code = "CD1";
			referenceAdditionalInfo2.CSI_ReferenceNumber = "REF1";

			var referenceAdditionalInfo3 = declaration.AdditionalInfos.AddNew();
			referenceAdditionalInfo3.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
			referenceAdditionalInfo3.CSI_Code = "CD2";
			referenceAdditionalInfo3.CSI_ReferenceNumber = "REF1";

			var referenceAdditionalInfo5 = declaration.AdditionalInfos.AddNew();
			referenceAdditionalInfo5.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
			referenceAdditionalInfo5.CSI_Code = "CD2";
			referenceAdditionalInfo5.CSI_ReferenceNumber = "REF2";

			var previousDocument1 = declaration.PreviousDocuments.AddNew();
			previousDocument1.CSI_Code = "380";
			previousDocument1.CSI_DateOfIssue = new ZDateTime(2023, 01, 01);
			previousDocument1.CSI_ReferenceNumber = "PRE1";

			var previousDocument2 = declaration.PreviousDocuments.AddNew();
			previousDocument2.CSI_Code = "380";
			previousDocument2.CSI_DateOfIssue = new ZDateTime(2023, 01, 01);
			previousDocument2.CSI_ReferenceNumber = "PRE1";

			var previousDocument3 = declaration.PreviousDocuments.AddNew();
			previousDocument3.CSI_Code = "270";
			previousDocument3.CSI_DateOfIssue = new ZDateTime(2023, 01, 02);
			previousDocument3.CSI_ReferenceNumber = "PRE1";

			var previousDocument4 = declaration.PreviousDocuments.AddNew();
			previousDocument4.CSI_Code = "270";
			previousDocument4.CSI_DateOfIssue = new ZDateTime(2023, 01, 01);
			previousDocument4.CSI_ReferenceNumber = "PRE2";

			var previousDocument5 = instruction.PreviousDocuments.AddNew();
			previousDocument5.CSI_Code = "380";
			previousDocument5.CSI_DateOfIssue = new ZDateTime(2023, 01, 01);
			previousDocument5.CSI_ReferenceNumber = "PRE1";

			var previousDocument6 = instruction.PreviousDocuments.AddNew();
			previousDocument6.CSI_Code = "380";
			previousDocument6.CSI_DateOfIssue = new ZDateTime(2023, 01, 01);
			previousDocument6.CSI_ReferenceNumber = "INS1";

			var supportingDocument1 = declaration.SupportingDocuments.AddNew();
			supportingDocument1.CSI_Code = "N380";
			supportingDocument1.CSI_DateOfExpiry = new ZDateTime(2023, 01, 01);
			supportingDocument1.CSI_ReferenceNumber = "SUP1";

			var supportingDocument2 = declaration.SupportingDocuments.AddNew();
			supportingDocument2.CSI_Code = "N380";
			supportingDocument2.CSI_DateOfExpiry = new ZDateTime(2023, 01, 01);
			supportingDocument2.CSI_ReferenceNumber = "SUP1";

			var supportingDocument3 = declaration.SupportingDocuments.AddNew();
			supportingDocument3.CSI_Code = "N380";
			supportingDocument3.CSI_DateOfExpiry = new ZDateTime(2023, 01, 02);
			supportingDocument3.CSI_ReferenceNumber = "SUP1";

			var supportingDocument4 = declaration.SupportingDocuments.AddNew();
			supportingDocument4.CSI_Code = "N380";
			supportingDocument4.CSI_DateOfExpiry = new ZDateTime(2023, 01, 01);
			supportingDocument4.CSI_ReferenceNumber = "SUP2";

			var supportingDocument5 = instruction.SupportingDocuments.AddNew();
			supportingDocument5.CSI_Code = "N270";
			supportingDocument5.CSI_DateOfExpiry = new ZDateTime(2023, 01, 01);
			supportingDocument5.CSI_ReferenceNumber = "SUP1";

			var supportingDocument6 = instruction.SupportingDocuments.AddNew();
			supportingDocument6.CSI_Code = "N270";
			supportingDocument6.CSI_DateOfExpiry = new ZDateTime(2023, 01, 01);
			supportingDocument6.CSI_ReferenceNumber = "INS2";

			var supportingDocument7 = declaration.SupportingDocuments.AddNew();
			supportingDocument7.CSI_Code = "N270";
			supportingDocument7.CSI_DateOfExpiry = new ZDateTime(2023, 01, 01);
			supportingDocument7.CSI_ReferenceNumber = "SUP1";

			instruction.CEI_OA_Warehouse2 = supplier.MainAddress.PK;
			instruction.CEI_DateForDuty = new ZDateTime(2022, 10, 30, 14, 41, 57);

			var fiscalreference1 = instruction.FiscalReferences.AddNew();
			fiscalreference1.CFR_Code = "AUT";
			fiscalreference1.CFR_Reference = "FISCAL_REFERENCE";

			var fiscalreference2 = instruction.FiscalReferences.AddNew();
			fiscalreference2.CFR_Code = "AUT";
			fiscalreference2.CFR_Reference = "FISCAL_REFERENCE";

			var fiscalreference3 = instruction.FiscalReferences.AddNew();
			fiscalreference3.CFR_Code = "CUS";
			fiscalreference3.CFR_Reference = "FISCAL_REFERENCE";

			var fiscalreference4 = instruction.FiscalReferences.AddNew();
			fiscalreference4.CFR_Code = "CUS";
			fiscalreference4.CFR_Reference = "FISCAL_REFERENCE2";

			var cusSupplyChainActorReferences1 = instruction.CusSupplyChainActorReferences.AddNew();
			cusSupplyChainActorReferences1.CFR_Code = "ABC";
			cusSupplyChainActorReferences1.CFR_Reference = "SCREF1";
			var cusSupplyChainActorReferences2 = instruction.CusSupplyChainActorReferences.AddNew();
			cusSupplyChainActorReferences2.CFR_Code = "ABC";
			cusSupplyChainActorReferences2.CFR_Reference = "SCREF1";
			var cusSupplyChainActorReferences3 = instruction.CusSupplyChainActorReferences.AddNew();
			cusSupplyChainActorReferences3.CFR_Code = "ABC";
			cusSupplyChainActorReferences3.CFR_Reference = "SCREF2";
			var cusSupplyChainActorReferences4 = instruction.CusSupplyChainActorReferences.AddNew();
			cusSupplyChainActorReferences4.CFR_Code = "DEF";
			cusSupplyChainActorReferences4.CFR_Reference = "SCREF2";

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RN_NKCountryOfExport = Core.Constants.CountryCodes.France;
			instruction.ZG_TransNature = "A";
			invoice.JZ_InvoiceDisplaySequence = 1;
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			invoice.JZ_UCR = "UCRReference";
			invoice.JZ_OA_SellerAddress = seller.MainAddress.PK;
			invoice.JZ_OA_SupplierAddress = supplier.MainAddress.PK;
			invoice.JZ_OA_BuyerAddress = buyer.MainAddress.PK;
			invoice.JZ_OA_ConsigneeAddress = consignee.MainAddress.PK;
			invoice.ZG_AgreedPlaceCode = "3";

			var informationAdditionalInfo5 = invoice.AdditionalInfos.AddNew();
			informationAdditionalInfo5.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
			informationAdditionalInfo5.CSI_Code = "INF2";
			informationAdditionalInfo5.CSI_Description = "INFDescription2";

			var refAdditionalInfo = invoice.AdditionalInfos.AddNew();
			refAdditionalInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
			refAdditionalInfo.CSI_Code = "9008";
			refAdditionalInfo.CSI_ReferenceNumber = "9008";

			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			invoiceLine1.JI_CEI = instruction.PK;
			invoiceLine1.JI_LinePrice = 12m;

			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_CEI = instruction.PK;
			invoiceLine2.JI_LinePrice = 14m;

			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_CEI_Instruction = instruction.PK;

			var entryLine1 = entry.MergedLines.AddNew();
			invoiceLine1.JI_CL = entryLine1.PK;

			var entryLine2 = entry.MergedLines.AddNew();
			invoiceLine2.JI_CL = entryLine2.PK;

			var charge1 = entry.Charges.AddNew();
			charge1.C1_ChargeAmount = 1m;
			charge1.C1_ChargeType = "P835";

			var charge2 = entry.Charges.AddNew();
			charge2.C1_ChargeAmount = 10m;
			charge2.C1_ChargeType = "K615";

			return GoodsShipmentWrapper.New(entry);
		}

		public void TestAdditionalFiscalReference()
		{
			AssertType<Collection<IAdditionalFiscalReference>>("AdditionalFiscalReference type", Provider.AdditionalFiscalReference);
			AssertContainsExactElementsInAnyOrder("There should be 3 distinct (filtered on role and number) elements in AdditionalFiscalReference, matching entry instruction fiscal references.", new string[] { "AUT|FISCAL_REFERENCE", "CUS|FISCAL_REFERENCE", "CUS|FISCAL_REFERENCE2" }, Provider.AdditionalFiscalReference.Select(x => x.Role + "|" + x.VATIdentificationNumber));

			var wrapper = GoodsShipmentWrapper.New(Factory.New<Declaration.CusEntryHeader>());
			AssertEquals("There should be 0 AdditionalFiscalReference.", 0, wrapper.AdditionalFiscalReference.Count);
		}

		public void TestAdditionalInformation()
		{
			AssertType<Collection<IAdditionalInformation>>("AdditionalInformation type", Provider.AdditionalInformation);
			AssertContainsExactElementsInAnyOrder("There should be 3 distinct (filtered on code) elements in AdditionalInformation, matching entry additional infos of type INF. ", new string[] { UniversalReferenceConstants.RefCusCodeList.AdditionalInformationCodes.FretCargo, "INF1", "INF2", "INF3" }, Provider.AdditionalInformation.Select(x => x.Code));

			var wrapper = GoodsShipmentWrapper.New(Factory.New<Declaration.CusEntryHeader>());
			AssertEquals("There should be 0 AdditionalInformation.", 0, wrapper.AdditionalInformation.Count);
		}

		public void TestAdditionalInformationExcludesG6090AndG6100WhenVatDeferTypeIs2()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			declaration.ZG_VATDeferType = VATProcedureList.Codes._2;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_JE = declaration.PK;
			entry.CH_CEI_Instruction = entryInstruction.PK;

			var ai1 = entryInstruction.AdditionalInfos.AddNew();
			ai1.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
			ai1.CSI_Code = RefCusCodeList.AdditionalInformationCodes.AI2WithVisaExemption;

			declaration.ZG_VATDeferType = VATProcedureList.Codes.S;
			var wrapper = GoodsShipmentWrapper.New(entry);
			var forbiddenCodes = wrapper.AdditionalInformation.Select(x => x.Code).ToList().Where(code => code == RefCusCodeList.AdditionalInformationCodes.AI2WithVisaExemption).ToList();
			AssertEquals("Additional info should not exclude G6090 when DeferType != 2", 1, forbiddenCodes.Count);

			declaration.ZG_VATDeferType = VATProcedureList.Codes._2;
			wrapper = GoodsShipmentWrapper.New(entry);
			forbiddenCodes = wrapper.AdditionalInformation.Select(x => x.Code).ToList().Where(code => code == RefCusCodeList.AdditionalInformationCodes.AI2WithVisaExemption).ToList();
			AssertEquals("Additional info should exclude G6090 when declaration is DeltaIE Import and DeferType = 2", 0, forbiddenCodes.Count);
			entryInstruction.AdditionalInfos.RemoveAndDelete(ai1);

			var ai2 = entryInstruction.AdditionalInfos.AddNew();
			ai2.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
			ai2.CSI_Code = RefCusCodeList.AdditionalInformationCodes.AI2WithoutVisaExemption;
			wrapper = GoodsShipmentWrapper.New(entry);
			forbiddenCodes = wrapper.AdditionalInformation.Select(x => x.Code).ToList().Where(code => code == RefCusCodeList.AdditionalInformationCodes.AI2WithoutVisaExemption).ToList();
			AssertEquals("Additional info should exclude G6100 when declaration is DeltaIE Import and DeferType = 2", 0, forbiddenCodes.Count);

			var ai3 = entryInstruction.AdditionalInfos.AddNew();
			ai3.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
			ai3.CSI_Code = RefCusCodeList.AdditionalInformationCodes.FallbackProcedure;
			wrapper = GoodsShipmentWrapper.New(entry);
			var unForbiddenCodes = wrapper.AdditionalInformation.Select(x => x.Code).ToList().Where(code => code == RefCusCodeList.AdditionalInformationCodes.FallbackProcedure).ToList();
			AssertEquals("Additional info should only exclude G6090/G6100 when declaration is DeltaIE Import and DeferType = 2", 1, unForbiddenCodes.Count);
		}

		public void TestAdditionalReference()
		{
			AssertType<Collection<IAdditionalReference>>("AdditionalReference type", Provider.AdditionalReference);
			AssertContainsExactElementsInAnyOrder("There should be 5 distinct (filtered on type and reference number) elements in AdditionalReference, matching entry additional infos of type REF. ", new string[] { "CD1|REF1", "CD2|REF1", "CD2|REF2", "9008|9008", "1DEC|DEC111" }, Provider.AdditionalReference.Select(x => x.Type + "|" + x.ReferenceNumber));

			var wrapper = GoodsShipmentWrapper.New(Factory.NewWithValidTestData<Declaration.CusEntryHeader>());
			AssertEquals("There should be 0 AdditionalReference.", 0, wrapper.AdditionalReference.Count);
		}

		public void TestAdditionalSupplyChainActor()
		{
			AssertType<Collection<IAdditionalSupplyChainActor>>("AdditionalSupplyChainActor type", Provider.AdditionalSupplyChainActor);
			AssertContainsExactElementsInAnyOrder("There should be 3 distinct (fitered on role and reference number) elements in AdditionalSupplyChainActor, matching entry instruction.", new string[] { "ABC|SCREF1", "ABC|SCREF2", "DEF|SCREF2" }, Provider.AdditionalSupplyChainActor.Select(x => x.Role + "|" + x.IdentificationNumber));

			var wrapper = GoodsShipmentWrapper.New(Factory.New<Declaration.CusEntryHeader>());
			AssertEquals("There should be 0 AdditionalSupplyChainActor.", 0, wrapper.AdditionalSupplyChainActor.Count);
		}

		public void TestAdditionsAndDeductions()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();

			var invoiceHeader1 = declaration.Invoices.AddNew();
			invoiceHeader1.JZ_RX_NKInvoice_Currency = "USD";
			var invoiceLine1 = invoiceHeader1.InvoiceLines.AddNew();
			var entryLine1 = entryHeader.MergedLines.AddNew();
			invoiceLine1.JI_CL = entryLine1.PK;

			var invoiceHeader2 = declaration.Invoices.AddNew();
			invoiceHeader2.JZ_RX_NKInvoice_Currency = "USD";
			var invoiceLine2 = invoiceHeader2.InvoiceLines.AddNew();
			var entryLine2 = entryHeader.MergedLines.AddNew();
			invoiceLine2.JI_CL = entryLine2.PK;

			var invoiceHeader3 = declaration.Invoices.AddNew();
			invoiceHeader3.JZ_RX_NKInvoice_Currency = "USD";
			var invoiceLine3 = invoiceHeader3.InvoiceLines.AddNew();
			var entryLine3 = entryHeader.MergedLines.AddNew();
			invoiceLine3.JI_CL = entryLine3.PK;

			var charge = invoiceLine1.Charges.AddNew();
			charge.J7_ChargeType = "OFT";
			charge.J7_Amount = 1m;
			charge.J7_IsDutiable = true;
			charge.J7_IsIncludedInITOT = false;
			charge.J7_RX_NKCurrency = "USD";

			var charge2 = invoiceLine2.Charges.AddNew();
			charge2.J7_ChargeType = "OFT";
			charge2.J7_Amount = 2m;
			charge2.J7_IsDutiable = true;
			charge2.J7_IsIncludedInITOT = false;
			charge2.J7_RX_NKCurrency = "USD";

			var transportCharge1 = invoiceLine3.Charges.AddNew();
			transportCharge1.J7_ChargeType = "OFT";
			transportCharge1.J7_Amount = 4m;
			transportCharge1.J7_IsDutiable = true;
			transportCharge1.J7_IsIncludedInITOT = false;
			transportCharge1.J7_RX_NKCurrency = "USD";

			var transportApportionedCharge1 = invoiceLine3.ApportionedCharges.AddNew();
			transportApportionedCharge1.J7_ChargeType = "CNE";
			transportApportionedCharge1.J7_Amount = 128m;
			transportApportionedCharge1.J7_IsDutiable = true;
			transportApportionedCharge1.J7_IsIncludedInITOT = false;
			transportApportionedCharge1.J7_RX_NKCurrency = "USD";

			invoiceLine1.JI_ValuationCode = Enterprise.MasterFiles.Business.Customs.EU.ValuationMethodList.Codes._1;
			invoiceHeader1.JZ_IncoTerm = Core.Constants.IncoTerms.FreeAlongsideShip;

			invoiceLine2.JI_ValuationCode = Enterprise.MasterFiles.Business.Customs.EU.ValuationMethodList.Codes._1;
			invoiceHeader2.JZ_IncoTerm = Core.Constants.IncoTerms.DeliveredAtFrontier;

			invoiceLine3.JI_ValuationCode = Enterprise.MasterFiles.Business.Customs.EU.ValuationMethodList.Codes._1;
			invoiceHeader3.JZ_IncoTerm = Core.Constants.IncoTerms.FreeAlongsideShip;

			AssertEquals("AK = Sum of all dutiable transport charges non included in invoice", 1m, entryLine1.CusEntryLineCalculatedFees.CL_CalcAK.Amount);
			AssertEquals("AK = Sum of all dutiable transport charges non included in invoice", 2m, entryLine2.CusEntryLineCalculatedFees.CL_CalcAK.Amount);
			AssertEquals("AK = Sum of all dutiable transport charges non included in invoice", 132m, entryLine3.CusEntryLineCalculatedFees.CL_CalcAK.Amount);

			Assert(entryLine1.CusEntryLineCalculatedFees.IsNat_146Applicable);
			Assert(!entryLine2.CusEntryLineCalculatedFees.IsNat_146Applicable);
			Assert(entryLine3.CusEntryLineCalculatedFees.IsNat_146Applicable);

			var provider = GoodsShipmentWrapper.New(entryHeader);
			AssertEquals("When entryLine satisfies rule Nat_146, transport charge AK will be calculated", 1, provider.AdditionsAndDeductions.Count);
			AssertEquals("When entryLine satisfies rule Nat_146, transport charge AK will be calculated", "AK", provider.AdditionsAndDeductions.First().Code);
			AssertEquals("The sum of all the AK of the entryLine that satisfy Nat_146", 133.0, provider.AdditionsAndDeductions.First().Amount);

			invoiceHeader1.JZ_IncoTerm = Core.Constants.IncoTerms.DeliveredAtFrontier;
			invoiceHeader3.JZ_IncoTerm = Core.Constants.IncoTerms.DeliveredAtFrontier;

			Assert(!entryLine1.CusEntryLineCalculatedFees.IsNat_146Applicable);
			Assert(!entryLine2.CusEntryLineCalculatedFees.IsNat_146Applicable);
			Assert(!entryLine3.CusEntryLineCalculatedFees.IsNat_146Applicable);

			provider = GoodsShipmentWrapper.New(entryHeader);
			AssertEquals("None of the entrylines satisfy Nat_146", 0, provider.AdditionsAndDeductions.Count);
		}

		public void TestBuyer()
		{
			AssertType<BuyerWrapper>("Buyer type", Provider.Buyer);
			AssertEquals("Values for buyer should be taken from declaration JE_OH_Buyer", "Buyer", Provider.Buyer.Name);
		}

		public void TestConsignment()
		{
			AssertType<ConsignmentWrapper>("Consignment type", Provider.Consignment);
		}

		public void TestCountryOfDispatch()
		{
			AssertType<CountryOfDispatchWrapper>("CountryOfDispatch type", Provider.CountryOfDispatch);
			AssertEquals("CountryOfDispatch should be equal to JE_GoodsOrigin", Core.Constants.CountryCodes.UnitedStates, Provider.CountryOfDispatch.CountryOfDispatch);
		}

		public void TestDateOfAcceptance()
		{
			AssertEquals("DateOfAcceptance should equal instruction.CEI_DateForDuty", "2022-10-30T14:41:57", Provider.DateOfAcceptance);
		}

		public void TestDeliveryTerms()
		{
			AssertType<DeliveryTermsWrapper>("DeliveryTerms type", Provider.DeliveryTerms);
			var truc = "d� �tre il �t�";
			byte[] tempBytes;
			tempBytes = System.Text.Encoding.GetEncoding("ISO-8859-8").GetBytes(truc);
			string asciiStr = System.Text.Encoding.UTF8.GetString(tempBytes);
			AssertEquals("DeliveryTerms should be taken value from invoice or declaration incoterm", "3", Provider.DeliveryTerms.UNLOCODE);
		}

		public void TestDestination()
		{
			AssertType<DestinationWrapper>("Destination type", Provider.Destination);
			AssertEquals("Destination CountryOfDestination should be taken value from declaration JE_GoodsDestination", Core.Constants.CountryCodes.UnitedKingdom, Provider.Destination.CountryOfDestination);
		}

		public void TestExchangeRate()
		{
			TestExchangeRateWhenSingleCurrency();
			TestExchangeRateWhenMultipleCurrencies();
		}

		void TestExchangeRateWhenSingleCurrency()
		{
			var declaration = CreateDeclarationWithSingleCurrency();
			var entryHeader = declaration.CustomsEntryHeaders[0];
			var provider = GoodsShipmentWrapper.New(entryHeader);

			Assert("Prerequisite", !entryHeader.IsMultiInvoiceCurrency);
			AssertEquals("When all entry invoices share same curency, then exchange rate should equal first invoice exchange rate ", 1.39d, provider.ExchangeRate);
		}

		void TestExchangeRateWhenMultipleCurrencies()
		{
			var declaration = CreateDeclarationWithMultipleCurrencies();
			var entryHeader = declaration.CustomsEntryHeaders[0];
			var provider = GoodsShipmentWrapper.New(entryHeader);

			Assert("Prerequisite", entryHeader.IsMultiInvoiceCurrency);
			AssertEquals("Exchange rate should be 1 when entry is multi currency.", 1d, provider.ExchangeRate);
		}

		public void TestExporter()
		{
			AssertType<ExporterWrapper>("Exporter type", Provider.Exporter);
			AssertEquals("Value for Exporter shoud be taken from any invoice Supplier when able.", "Supplier", Provider.Exporter.Name);

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();

			var supplier = Factory.New<OrgHeader>();
			supplier.FillWithValidTestData();
			supplier.OH_FullName = "Declaration Supplier";
			supplier.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.France;
			declaration.SupplierDocumentaryAddress.OrganisationPK = supplier.PK;

			var invoice1 = declaration.Invoices.AddNew();
			invoice1.FillWithValidTestData();
			invoice1.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			var invoiceLine1 = invoice1.InvoiceLines.AddNew();
			invoiceLine1.JI_LinePrice = 10d;

			var merger = new Declaration.LineMerger(declaration);
			merger.DoMerge();

			var entry = declaration.CustomsEntryHeaders[0];
			var goodsShipmentWrapper = GoodsShipmentWrapper.New(entry);

			AssertType<ExporterWrapper>("Exporter type", goodsShipmentWrapper.Exporter);
			AssertEquals("Value for Exporter shoud be taken from Supplier in Declaration when invoice Supplier is not available.", "Declaration Supplier", goodsShipmentWrapper.Exporter.Name);
		}

		public void TestConsignee()
		{
			var consigneeAddress = DeltaIEMessageWrapperTestHelper.GetAddressMock("City", "MK", "MACD1234", "#1, ");
			DeltaIEMessageWrapperTestHelper.AssertConsignee(Provider.Consignee, consigneeAddress, "Consignee", "");
			AssertType<ConsigneeWrapper>("Consignee Type", Provider.Consignee);
		}

		public void TestGoodsShipmentItem()
		{
			AssertType<Collection<IGoodsShipmentItem>>("GoodsShipmentItem type", Provider.GoodsShipmentItem);
			AssertEquals("GoodsShipmentItem values should be taken value from entry lines", "UCRReference", Provider.GoodsShipmentItem.ElementAt(0).ReferenceNumberUCR);

			var wrapper = GoodsShipmentWrapper.New(Factory.New<Declaration.CusEntryHeader>());
			AssertEquals("There should be 0 GoodsShipmentItem.", 0, wrapper.GoodsShipmentItem.Count);
		}

		public void TestInvoiceCurrency()
		{
			TestCurrencyWhenSingleCurrency();
			TestCurrencyWhenMultipleCurrencies();
		}

		void TestCurrencyWhenSingleCurrency()
		{
			var declaration = CreateDeclarationWithSingleCurrency();
			var entryHeader = declaration.CustomsEntryHeaders[0];
			var provider = GoodsShipmentWrapper.New(entryHeader);

			Assert("Prerequisite", !entryHeader.IsMultiInvoiceCurrency);
			AssertEquals("When all entry invoices share same currency, then currency should equal first invoice currency.", Core.Constants.CurrencyCodes.UnitedStates, provider.InvoiceCurrency);
		}

		void TestCurrencyWhenMultipleCurrencies()
		{
			var declaration = CreateDeclarationWithMultipleCurrencies();
			var entryHeader = declaration.CustomsEntryHeaders[0];
			var provider = GoodsShipmentWrapper.New(entryHeader);

			Assert("Prerequisite", entryHeader.IsMultiInvoiceCurrency);
			AssertEquals("Currency should be EUR when entry is multi currency.", Core.Constants.CurrencyCodes.France, provider.InvoiceCurrency);
		}

		public void TestNatureOfTransaction()
		{
			AssertEquals("NatureOfTransaction should equal any instruction ZG_TransNature", "A", Provider.NatureOfTransaction);
		}

		public void TestPreviousDocument()
		{
			AssertType<Collection<IGoodsShipmentPreviousDocument>>("PreviousDocument type", Provider.PreviousDocument);
			AssertContainsExactElementsInAnyOrder("There should be 4 distinct (fitered on type, reference) elements in PreviousDocument, matching all entry supporting documents.", new string[] { "380|PRE1", "270|PRE1", "270|PRE2", "380|INS1" }, Provider.PreviousDocument.Select(x => x.Type + "|" + x.ReferenceNumber));

			var wrapper = GoodsShipmentWrapper.New(Factory.New<Declaration.CusEntryHeader>());
			AssertEquals("There should be 0 PreviousDocument.", 0, wrapper.PreviousDocument.Count);
		}

		public void TestSeller()
		{
			AssertType<SellerWrapper>("Seller type", Provider.Seller);
			AssertEquals("Seller shoud reflect first invoice Seller", "Seller", Provider.Seller.Name);
		}

		public void TestSequenceNumber()
		{
			AssertEquals("SequenceNumber should always be 1.", "1", Provider.SequenceNumber);
		}

		public void TestSupportingDocument()
		{
			AssertType<Collection<ISupportingDocument>>("SupportingDocument type", Provider.SupportingDocument);
			AssertContainsExactElementsInAnyOrder("There should be 4 distinct (fitered on type, validity and reference number) elements in SupportingDocument, matching all entry supporting documents.", new string[] { "N380|SUP1|2023-01-01", "N380|SUP2|2023-01-01", "N270|SUP1|2023-01-01", "N270|INS2|2023-01-01" }, Provider.SupportingDocument.Select(x => x.Type + "|" + x.ReferenceNumber + "|" + x.DateOfValidity));

			var wrapper = GoodsShipmentWrapper.New(Factory.New<Declaration.CusEntryHeader>());
			AssertEquals("There should be 0 SupportingDocument.", 0, wrapper.SupportingDocument.Count);
		}

		public void TestTotalAmountInvoiced()
		{
			TestTotalAmountInvoicedWhenSingleCurrency();
			TestTotalAmountInvoicedWhenMultipleCurrencies();
		}

		void TestTotalAmountInvoicedWhenSingleCurrency()
		{
			var declaration = CreateDeclarationWithSingleCurrency();
			var entryHeader = declaration.CustomsEntryHeaders[0];
			var provider = GoodsShipmentWrapper.New(entryHeader);

			Assert("Prerequisite", !entryHeader.IsMultiInvoiceCurrency);
			AssertEquals("When all entry invoices share same currency, amount should match sum of invoices amount in their common currency.", 30d, provider.TotalAmountInvoiced);
		}

		void TestTotalAmountInvoicedWhenMultipleCurrencies()
		{
			var declaration = CreateDeclarationWithMultipleCurrencies();
			var entryHeader = declaration.CustomsEntryHeaders[0];
			var provider = GoodsShipmentWrapper.New(entryHeader);

			Assert("Prerequisite", entryHeader.IsMultiInvoiceCurrency);
			AssertEquals("Amount should be converted in EUR when entry is multi currency.", 37.04d, provider.TotalAmountInvoiced);
		}

		public void TestWarehouse()
		{
			AssertType<WarehouseWrapper>("Warehouse type", Provider.Warehouse);
			AssertEquals("Values for warehouse should be taken from authorisation whose holder is entry instruction Warehouse2", "AUTH1", Provider.Warehouse.Identifier);
		}

		public void TestAdditionalDeclarationType()
		{
			AssertEquals("AdditionalDeclarationType should be captured from EntryInstruction.CEI_SubStyle", "V", Provider.AdditionalDeclarationType);
		}

		public void TestBillOfDischarge()
		{
			AssertNull("Mapping is not decided yed", Provider.BillOfDischarge);
		}

		public void TestDetailsOfPlannedActivities()
		{
			AssertNull("Mapping is not decided yed", Provider.DetailsOfPlannedActivities);
		}

		public void TestFirstPlaceOfUseOrProcessing()
		{
			AssertNull("Mapping is not decided yed", Provider.FirstPlaceOfUseOrProcessing);
		}

		public void TestIdentificationOfGoods()
		{
			AssertNull("Mapping is not decided yed", Provider.IdentificationOfGoods);
		}

		public void TestPeriodForDischarge()
		{
			AssertNull("Mapping is not decided yed", Provider.PeriodForDischarge);
		}

		public void TestProcessedProducts()
		{
			AssertNull("Mapping is not decided yed", Provider.ProcessedProducts);
		}

		JobDeclaration CreateDeclarationWithSingleCurrency()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();

			var invoice1 = declaration.Invoices.AddNew();
			invoice1.FillWithValidTestData();
			invoice1.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			var invoiceLine1 = invoice1.InvoiceLines.AddNew();
			invoiceLine1.JI_LinePrice = 10d;

			var invoice2 = declaration.Invoices.AddNew();
			invoice2.FillWithValidTestData();
			invoice2.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			var invoiceLine2 = invoice2.InvoiceLines.AddNew();
			invoiceLine2.JI_LinePrice = 20d;

			Factory.Save();

			var merger = new Declaration.LineMerger(declaration);
			merger.DoMerge();
			return declaration;
		}

		JobDeclaration CreateDeclarationWithMultipleCurrencies()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();

			var invoice1 = declaration.Invoices.AddNew();
			invoice1.FillWithValidTestData();
			invoice1.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			var invoiceLine1 = invoice1.InvoiceLines.AddNew();
			invoiceLine1.JI_LinePrice = 10d;

			var invoice2 = declaration.Invoices.AddNew();
			invoice2.FillWithValidTestData();
			invoice2.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Guatemala;
			var invoiceLine2 = invoice2.InvoiceLines.AddNew();
			invoiceLine2.JI_LinePrice = 20d;

			Factory.Save();

			var merger = new Declaration.LineMerger(declaration);
			merger.DoMerge();
			return declaration;
		}
	}
}
