using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.Customs.FR.MessageDefinitions.DeltaIE.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business;
using static Enterprise.Customs.FR.Business.UniversalReferenceConstants;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using JobDeclaration = Enterprise.Customs.FR.Business.Declaration.JobDeclaration;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.DeltaIE.Testing
{
	class GoodsShipmentItemWrapperTest : Customs.Business.Testing.DataProviderTestCase<GoodsShipmentItemWrapper>
	{
		protected override GoodsShipmentItemWrapper GetProvider()
		{
			var date1 = ZDateTime.MinSmallDateTimeValue;
			var date2 = ZDateTime.MaxSmallDateTimeValue;

			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tradeGroup = helper.CreateTradeGroup(Env.CurrentCompany.Country.Code, "TEST1", date1, date2);
			helper.AddCountry(tradeGroup, Core.Constants.CountryCodes.Australia, date1.Date, date2.Date);
			var hsnTariffType = helper.CreateNewOrGetExistingTariffType(Env.CurrentCompany.Country.Code, Customs.Business.UniversalReferenceConstants.CusTariffTypes.ImportTariff);
			Factory.Save();
			var rateType = helper.CreateNewOrGetExistingRateType(Env.CurrentCompany.Country.Code, Universal.Constants.RateTypes.Duty, "Duty");
			helper.LoadOrCreateNewCusRateCode(Factory, "DTA", rateType.PK);
			Factory.Save();
			helper.CreateTariff(Env.CurrentCompany.Country.Code, hsnTariffType.PK, "123456789", date1, date2, "dummy Description 0");
			Factory.Save();

			var procedure = helper.CreateOrFindExistingRefCusProcedure(Core.Constants.CountryCodes.France, "IM", "10", "71", "F61", "", "IMP", "10P");
			procedure.ZZ6_IntoWarehouse = "Y";
			procedure.ZZ6_OutOfWarehouse = "N";
			Factory.Save();

			var buyerDocAddress = Factory.NewWithValidTestData<JobDocAddress>();
			var buyer = Factory.NewWithValidTestData<OrgHeader>();
			buyerDocAddress.E2_AddressType = DocAddressTypes.Codes.BuyingParty;
			buyerDocAddress.OrganisationPK = buyer.PK;
			buyerDocAddress.E2_ParentTableCode = "JI";
			buyer.OH_FullName = "buyer";
			buyer.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Seychelles;
			buyer.MainAddress.City = "buyerCity";
			buyer.MainAddress.Postcode = "BUYPC11";

			var supplier = Factory.New<OrgHeader>();
			supplier.FillWithValidTestData();
			supplier.OH_FullName = "Supplier";
			supplier.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Mauritius;

			var sellerDocAddress = Factory.NewWithValidTestData<JobDocAddress>();
			var seller = Factory.NewWithValidTestData<OrgHeader>();
			sellerDocAddress.E2_AddressType = DocAddressTypes.Codes.SellingParty;
			sellerDocAddress.OrganisationPK = seller.PK;
			sellerDocAddress.E2_ParentTableCode = "JI";
			seller.OH_FullName = "seller";
			seller.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Fiji;
			seller.MainAddress.City = "sellerCity";
			seller.MainAddress.Postcode = "SELPC11";

			var exporter = Factory.New<OrgHeader>();
			exporter.FillWithValidTestData();
			exporter.OH_FullName = "exporter";
			exporter.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Kenya;
			exporter.MainAddress.City = "exporterCity";
			exporter.MainAddress.Postcode = "EXPPC11";

			var permit = Factory.New<CusPermitHeader>();
			permit.FillWithValidTestData();
			permit.CPH_OH_PermitHolder = supplier.PK;
			permit.CPH_Number = "PermitNumber";

			var rule = permit.CusPermitRules.AddNew();
			rule.CPR_RuleCode = "USE";
			rule.CPR_ValueFrom = "OTH";

			var rule2 = permit.CusPermitRules.AddNew();
			rule2.CPR_RuleCode = "CAN";
			rule2.CPR_ValueFrom = "ATH";

			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_OA_Warehouse2 = supplier.MainAddress.PK;

			declaration.JE_MessageType = Customs.Common.Shared.SharedJobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;

			var decPreviousDocument1 = declaration.PreviousDocuments.AddNew();
			decPreviousDocument1.CSI_Code = "380";
			decPreviousDocument1.CSI_DateOfIssue = new ZDateTime(2023, 01, 01);
			decPreviousDocument1.CSI_ReferenceNumber = "DECPRE1";

			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_CEI_Instruction = instruction.PK;
			var entryLine = entry.MergedLines.AddNew();
			entryLine.CL_LineNumber = 1;
			entryLine.CL_StatisticalValue = 14;
			entryLine.EffectiveDescription = "Goods Description";

			instruction.CusSupplyChainActorReferences.AddNew().FillWithValidTestData();
			instruction.CEI_DateForDuty = new ZDateTime(2022, 10, 30, 14, 41, 57);
			instruction.CEI_SubStyle = "V";

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_UCR = "UCR_Reference";
			instruction.ZG_TransNature = "11";

			var invPreviousDocument1 = invoice.PreviousDocuments.AddNew();
			invPreviousDocument1.CSI_Code = "380";
			invPreviousDocument1.CSI_DateOfIssue = new ZDateTime(2023, 01, 01);
			invPreviousDocument1.CSI_ReferenceNumber = "INVPRE1";

			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			invoiceLine1.JI_CEI = instruction.PK;
			invoiceLine1.JI_CL = entryLine.PK;
			invoiceLine1.ZG_CountryOfDispatch = Core.Constants.CountryCodes.France;

			var invPreviousDocument2 = invoiceLine1.InvoiceHeader.PreviousDocuments.AddNew();
			invPreviousDocument2.CSI_Code = "380";
			invPreviousDocument2.CSI_DateOfIssue = new ZDateTime(2023, 01, 01);
			invPreviousDocument2.CSI_ReferenceNumber = "INVPRE2";

			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_CEI = instruction.PK;
			invoiceLine2.JI_CL = entryLine.PK;
			invoiceLine2.ZG_CountryOfDispatch = Core.Constants.CountryCodes.France;

			invoice.JZ_RX_NKInvoice_Currency = "EUR";
			invoiceLine1.JI_ValuationCode = "A";
			invoiceLine1.JI_Description = "DESCRIPTION";
			invoiceLine1.ZG_CountryOfDestination = Core.Constants.CountryCodes.UnitedKingdom;
			invoice.JZ_InvoiceDisplaySequence = 1;
			invoice.JZ_InvoiceAmount = 12m;
			invoice.ZG_AgreedPlaceCode = "3";
			invoiceLine1.JI_Tariff = "123456789";
			invoiceLine1.JI_Procedure = procedure.ZZ6_ProcedureCode + procedure.ZZ6_PreviousProcedureCode + procedure.ZZ6_Concession;
			invoiceLine1.JI_OH_Supplier = supplier.PK;
			invoiceLine1.JI_OA_ExporterAddress = seller.MainAddress.PK;
			invoiceLine1.JI_OA_Seller = seller.MainAddress.PK;
			invoiceLine1.JI_OA_ConsigneeAddress = buyer.MainAddress.PK;
			invoiceLine1.JI_PrimaryPreference = "100";

			var authorisationUsage1 = invoiceLine1.CusAuthorizationUsages.AddNew();
			authorisationUsage1.AGC_OH_Owner = buyer.PK;
			authorisationUsage1.EffectiveReferenceNumber = "AUTH1";
			authorisationUsage1.AGC_Code = "AC1";

			var authorisationUsage2 = invoiceLine1.CusAuthorizationUsages.AddNew();
			authorisationUsage2.AGC_OH_Owner = buyer.PK;
			authorisationUsage2.EffectiveReferenceNumber = "AUTH2";
			authorisationUsage2.AGC_Code = "AC1";

			var authorisationUsage3 = invoiceLine1.CusAuthorizationUsages.AddNew();
			authorisationUsage3.AGC_OH_Owner = buyer.PK;
			authorisationUsage3.EffectiveReferenceNumber = "AUTH2";
			authorisationUsage3.AGC_Code = "AC2";

			var fiscalreference1 = invoiceLine1.FiscalReferences.AddNew();
			fiscalreference1.CFR_Code = "AUT";
			fiscalreference1.CFR_Reference = "FISCAL_REFERENCE";

			var fiscalreference2 = invoiceLine1.FiscalReferences.AddNew();
			fiscalreference2.CFR_Code = "AUT";
			fiscalreference2.CFR_Reference = "FISCAL_REFERENCE";
			var fiscalreference3 = invoiceLine1.FiscalReferences.AddNew();
			fiscalreference3.CFR_Code = "CUS";
			fiscalreference3.CFR_Reference = "FISCAL_REFERENCE";

			var fiscalreference4 = invoiceLine1.FiscalReferences.AddNew();
			fiscalreference4.CFR_Code = "CUS";
			fiscalreference4.CFR_Reference = "FISCAL_REFERENCE2";

			var previousDocument1 = invoiceLine1.PreviousDocuments.AddNew();
			previousDocument1.CSI_Code = "380";
			previousDocument1.CSI_DateOfIssue = new ZDateTime(2023, 01, 01);
			previousDocument1.CSI_ReferenceNumber = "PRE1";

			var previousDocument2 = invoiceLine1.PreviousDocuments.AddNew();
			previousDocument2.CSI_Code = "380";
			previousDocument2.CSI_DateOfIssue = new ZDateTime(2023, 01, 01);
			previousDocument2.CSI_ReferenceNumber = "PRE1";

			var previousDocument3 = invoiceLine1.PreviousDocuments.AddNew();
			previousDocument3.CSI_Code = "270";
			previousDocument3.CSI_DateOfIssue = new ZDateTime(2023, 01, 02);
			previousDocument3.CSI_ReferenceNumber = "PRE1";

			var previousDocument4 = invoiceLine1.PreviousDocuments.AddNew();
			previousDocument4.CSI_Code = "270";
			previousDocument4.CSI_DateOfIssue = new ZDateTime(2023, 01, 01);
			previousDocument4.CSI_ReferenceNumber = "PRE2";

			var supportingDocument1 = invoiceLine1.SupportingDocuments.AddNew();
			supportingDocument1.CSI_Code = "N380";
			supportingDocument1.CSI_DateOfExpiry = new ZDateTime(2023, 01, 01);
			supportingDocument1.CSI_ReferenceNumber = "SUP1";

			var supportingDocument2 = invoiceLine1.SupportingDocuments.AddNew();
			supportingDocument2.CSI_Code = "N380";
			supportingDocument2.CSI_DateOfExpiry = new ZDateTime(2023, 01, 01);
			supportingDocument2.CSI_ReferenceNumber = "SUP1";

			var supportingDocument3 = invoiceLine1.SupportingDocuments.AddNew();
			supportingDocument3.CSI_Code = "N380";
			supportingDocument3.CSI_DateOfExpiry = new ZDateTime(2023, 01, 02);
			supportingDocument3.CSI_ReferenceNumber = "SUP1";

			var supportingDocument4 = invoiceLine1.SupportingDocuments.AddNew();
			supportingDocument4.CSI_Code = "N380";
			supportingDocument4.CSI_DateOfExpiry = new ZDateTime(2023, 01, 01);
			supportingDocument4.CSI_ReferenceNumber = "SUP2";

			var supportingDocument5 = invoiceLine1.SupportingDocuments.AddNew();
			supportingDocument5.CSI_Code = "N270";
			supportingDocument5.CSI_DateOfExpiry = new ZDateTime(2023, 01, 01);
			supportingDocument5.CSI_ReferenceNumber = "SUP1";

			var referenceAdditionalInfo1 = invoiceLine1.AdditionalInfos.AddNew();
			referenceAdditionalInfo1.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
			referenceAdditionalInfo1.CSI_Code = "CD1";
			referenceAdditionalInfo1.CSI_ReferenceNumber = "REF1";

			var referenceAdditionalInfo2 = invoiceLine1.AdditionalInfos.AddNew();
			referenceAdditionalInfo2.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
			referenceAdditionalInfo2.CSI_Code = "CD1";
			referenceAdditionalInfo2.CSI_ReferenceNumber = "REF1";

			var referenceAdditionalInfo3 = invoiceLine1.AdditionalInfos.AddNew();
			referenceAdditionalInfo3.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
			referenceAdditionalInfo3.CSI_Code = "CD2";
			referenceAdditionalInfo3.CSI_ReferenceNumber = "REF1";

			var referenceAdditionalInfo4 = invoiceLine1.AdditionalInfos.AddNew();
			referenceAdditionalInfo4.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
			referenceAdditionalInfo4.CSI_Code = "CD2";
			referenceAdditionalInfo4.CSI_ReferenceNumber = "REF2";

			var informationAdditionalInfo1 = invoiceLine1.AdditionalInfos.AddNew();
			informationAdditionalInfo1.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
			informationAdditionalInfo1.CSI_Code = "INF1";

			var informationAdditionalInfo2 = invoiceLine1.AdditionalInfos.AddNew();
			informationAdditionalInfo2.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
			informationAdditionalInfo2.CSI_Code = "INF1";

			var informationAdditionalInfo3 = invoiceLine1.AdditionalInfos.AddNew();
			informationAdditionalInfo3.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
			informationAdditionalInfo3.CSI_Code = "INF2";

			var transportAdditionalInfo1 = invoiceLine1.AdditionalInfos.AddNew();
			transportAdditionalInfo1.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
			transportAdditionalInfo1.CSI_Code = "CD1";
			transportAdditionalInfo1.CSI_ReferenceNumber = "TRA1";

			var transportAdditionalInfo2 = invoiceLine1.AdditionalInfos.AddNew();
			transportAdditionalInfo2.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
			transportAdditionalInfo2.CSI_Code = "CD1";
			transportAdditionalInfo2.CSI_ReferenceNumber = "TRA1";

			var transportAdditionalInfo3 = invoiceLine1.AdditionalInfos.AddNew();
			transportAdditionalInfo3.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
			transportAdditionalInfo3.CSI_Code = "CD2";
			transportAdditionalInfo3.CSI_ReferenceNumber = "TRA1";

			var transportAdditionalInfo4 = invoiceLine1.AdditionalInfos.AddNew();
			transportAdditionalInfo4.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
			transportAdditionalInfo4.CSI_Code = "CD2";
			transportAdditionalInfo4.CSI_ReferenceNumber = "TRA2";

			var cusSupplyChainActorReference1 = invoiceLine1.CusSupplyChainActorReferences.AddNew();
			cusSupplyChainActorReference1.CFR_Reference = "actref1";
			cusSupplyChainActorReference1.CFR_Code = "CRF";
			cusSupplyChainActorReference1.CFR_Type = "SCA";

			var cusSupplyChainActorReference2 = invoiceLine1.CusSupplyChainActorReferences.AddNew();
			cusSupplyChainActorReference2.CFR_Reference = "actref2";
			cusSupplyChainActorReference2.CFR_Code = "KKK";
			cusSupplyChainActorReference2.CFR_Type = "SCA";

			var charge = invoiceLine1.Charges.AddNew();
			charge.J7_Amount = 18m;
			invoiceLine1.Charges.AddNew().FillWithValidTestData();

			invoiceLine1.JI_OH_Supplier = supplier.PK;
			invoiceLine1.JI_OA_ExporterAddress = exporter.MainAddress.PK;

			buyerDocAddress.E2_ParentID = invoiceLine1.PK;
			sellerDocAddress.E2_ParentID = invoiceLine1.PK;

			var package1 = (EU.Business.Declaration.Package)declaration.Bills.AddNew().PackingGroups.AddNew().Packages.AddNew();
			package1.CW_PackQty = 10;
			package1.CW_PackType = "PK";
			package1.CW_MarksAndNos = "MARKS";

			var packing1 = invoiceLine1.PackagesForInvoiceLinesForBindingOnly[0];
			packing1.IsLinked = true;
			packing1.PackQty = 10;

			var package2 = (EU.Business.Declaration.Package)declaration.Bills.AddNew().PackingGroups.AddNew().Packages.AddNew();
			package2.CW_PackQty = 20;
			package2.CW_PackType = "CT";
			package2.CW_MarksAndNos = "MARKS2";

			var packing2 = invoiceLine1.PackagesForInvoiceLinesForBindingOnly[1];
			packing2.IsLinked = true;
			packing2.PackQty = 5;

			var packing3 = invoiceLine2.PackagesForInvoiceLinesForBindingOnly[1];
			packing3.IsLinked = true;
			packing3.PackQty = 8;

			return GoodsShipmentItemWrapper.New(entryLine);
		}

		public void TestPrimaryPreference()
		{
			AssertEquals("StatisticalValue should be equals InvoiceLine.JI_PrimaryPreference", "100", Provider.PrimaryPreference);
		}

		public void TestAdditionalFiscalReference()
		{
			AssertType<Collection<IAdditionalFiscalReference>>("AdditionalFiscalReference type", Provider.AdditionalFiscalReference);
			AssertContainsExactElementsInAnyOrder("There should be 3 distinct (filtered on role and number) elements in AdditionalFiscalReference, matching all invoice lines fiscal references.", new string[] { "AUT|FISCAL_REFERENCE", "CUS|FISCAL_REFERENCE", "CUS|FISCAL_REFERENCE2" }, Provider.AdditionalFiscalReference.Select(x => x.Role + "|" + x.VATIdentificationNumber));

			var wrapper = GoodsShipmentItemWrapper.New(Factory.New<Declaration.CusEntryLine>());
			AssertEquals("There should be 0 AdditionalFiscalReference.", 0, wrapper.AdditionalFiscalReference.Count);
		}

		public void TestAdditionalInformation()
		{
			AssertType<Collection<IAdditionalInformation>>("AdditionalInformation type", Provider.AdditionalInformation);
			AssertContainsExactElementsInAnyOrder("There should be 2 distinct (fitered on code) elements in AdditionalInformation, matching all invoice lines additional infos of type INF. ", new string[] { "INF1", "INF2" }, Provider.AdditionalInformation.Select(x => x.Code));
		}

		public void TestAdditionalInformationIncludesG6090AndG6100FromHeaderWhenVatDeferTypeIs2()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			declaration.ZG_VATDeferType = VATProcedureList.Codes._2;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_JE = declaration.PK;
			entry.CH_CEI_Instruction = entryInstruction.PK;
			var entryLine = entry.MergedLines.AddNew();

			var ai1 = entryInstruction.AdditionalInfos.AddNew();
			ai1.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
			ai1.CSI_Code = RefCusCodeList.AdditionalInformationCodes.AI2WithVisaExemption;

			declaration.ZG_VATDeferType = VATProcedureList.Codes.S;
			var wrapper = GoodsShipmentItemWrapper.New(entryLine);
			AssertEquals("Additional info should not include G6090 from header when DeferType != 2", 0, wrapper.AdditionalInformation.Count);
			declaration.ZG_VATDeferType = VATProcedureList.Codes._2;

			wrapper = GoodsShipmentItemWrapper.New(entryLine);
			AssertContainsExactElementsInAnyOrder("Additional info should include G6090 from header when declaration is DeltaIE Import and DeferType = 2", new[] { "G6090" }, wrapper.AdditionalInformation.Select(x => x.Code));
			entryInstruction.AdditionalInfos.RemoveAndDelete(ai1);

			var ai2 = entryInstruction.AdditionalInfos.AddNew();
			ai2.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
			ai2.CSI_Code = RefCusCodeList.AdditionalInformationCodes.AI2WithoutVisaExemption;
			wrapper = GoodsShipmentItemWrapper.New(entryLine);
			AssertContainsExactElementsInAnyOrder("Additional info should include G6100 from header when declaration is DeltaIE Import and DeferType = 2", new[] { "G6100" }, wrapper.AdditionalInformation.Select(x => x.Code));

			var ai3 = entryInstruction.AdditionalInfos.AddNew();
			ai3.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
			ai3.CSI_Code = RefCusCodeList.AdditionalInformationCodes.AI2WithVisaExemption;
			wrapper = GoodsShipmentItemWrapper.New(entryLine);
			AssertContainsExactElementsInAnyOrder("Additional info should include both G6090 and G6100 from header when declaration is DeltaIE Import and DeferType = 2", new[] { "G6090", "G6100" }, wrapper.AdditionalInformation.Select(x => x.Code));

			var ai4 = entryInstruction.AdditionalInfos.AddNew();
			ai4.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
			ai4.CSI_Code = RefCusCodeList.AdditionalInformationCodes.FallbackProcedure;
			wrapper = GoodsShipmentItemWrapper.New(entryLine);
			AssertContainsExactElementsInAnyOrder("Should include only G6090 and G6100 from header when declaration is DeltaIE Import and DeferType = 2", new[] { "G6090", "G6100" }, wrapper.AdditionalInformation.Select(x => x.Code));
		}

		public void TestAdditionalReference()
		{
			AssertType<Collection<IAdditionalReference>>("AdditionalReference type", Provider.AdditionalReference);
			AssertContainsExactElementsInAnyOrder("There should be 3 distinct (fitered on type and reference number) elements in AdditionalReference, matching all invoice lines additional infos of type REF. ", new string[] { "CD1|REF1", "CD2|REF1", "CD2|REF2" }, Provider.AdditionalReference.Select(x => x.Type + "|" + x.ReferenceNumber));

			var wrapper = GoodsShipmentItemWrapper.New(Factory.New<Declaration.CusEntryLine>());
			AssertEquals("There should be 0 AdditionalReference.", 0, wrapper.AdditionalReference.Count);
		}

		public void TestAdditionalSupplyChainActor()
		{
			DeltaIEMessageWrapperTestHelper.AssertAdditionalSupplyChainActor(Provider.AdditionalSupplyChainActor, 2, new[] { "actref1", "actref2" }, new[] { "CRF", "KKK" }, new[] { "", "" });
		}

		public void TestAuthorisation()
		{
			AssertType<Collection<IAuthorisation>>("Authorisation type", Provider.Authorisation);
			AssertContainsExactElementsInAnyOrder("There should be 3 distinct Authorisations (filtered on code, number and owner) as there are 3 CusAuthorizationUsages against the invoiceLine.CusAuthorizationUsages.", new string[] { "AC1|AUTH1", "AC1|AUTH2", "AC2|AUTH2" }, Provider.Authorisation.Select(x => x.Type + "|" + x.ReferenceNumber));

			var wrapper = GoodsShipmentItemWrapper.New(Factory.New<Declaration.CusEntryLine>());
			AssertEquals("There should be 0 Authorisation.", 0, wrapper.Authorisation.Count);
		}

		public void TestBuyer()
		{
			var expectedAddress = DeltaIEMessageWrapperTestHelper.GetAddressMock("buyerCity", "SC", "BUYPC11", "#1, ");
			DeltaIEMessageWrapperTestHelper.AssertBuyer(Provider.Buyer, expectedAddress, "buyer", "");
		}

		public void TestCommodity()
		{
			AssertType<CommodityWrapper>("Commodity type", Provider.Commodity);
			AssertEquals("DescriptionOfGoods should be equal to CL_Description", "Goods Description", Provider.Commodity.DescriptionOfGoods);
		}

		public void TestCountryOfDispatch()
		{
			AssertType<CountryOfDispatchWrapper>("CountryOfDispatch type", Provider.CountryOfDispatch);
			AssertEquals("CountryOfDispatch should be equal to JE_GoodsOrigin", Core.Constants.CountryCodes.France, Provider.CountryOfDispatch.CountryOfDispatch);
		}

		public void TestCustomsValuation()
		{
			AssertType<CustomsValuationWrapper>("CustomsValuation type", Provider.CustomsValuation);
			AssertEquals("ValuationMethod should be equal to JI_ValuationCode", "A", Provider.CustomsValuation.ValuationMethod);
		}

		public void TestDateOfAcceptance()
		{
			AssertEquals("DateOfAcceptance should be equals instruction.CEI_DateForDuty", "2022-10-30T14:41:57", Provider.DateOfAcceptance);
		}

		public void TestDeclarationGoodsItemNumber()
		{
			AssertEquals("DeclarationGoodsItemNumber should be equal to CusEntryLine CL_LineNumber", "1", Provider.DeclarationGoodsItemNumber);
		}

		public void TestDestination()
		{
			AssertType<GoodsShipmentItemDestinationWrapper>("Destination type", Provider.Destination);
			AssertEquals("Destination CountryOfDestination should be taken value from declaration ZG_CountryOfDestination", Core.Constants.CountryCodes.UnitedKingdom, Provider.Destination.CountryOfDestination);
		}

		public void TestExporter()
		{
			var expectedAddress = DeltaIEMessageWrapperTestHelper.GetAddressMock("exporterCity", "KE", "EXPPC11", "#1, ");
			DeltaIEMessageWrapperTestHelper.AssertExporter(Provider.Exporter, expectedAddress, "exporter", "");
		}

		public void TestConsignee()
		{
			var consigneeAddress = DeltaIEMessageWrapperTestHelper.GetAddressMock("buyerCity", "SC", "BUYPC11", "#1, ");
			DeltaIEMessageWrapperTestHelper.AssertConsignee(Provider.Consignee, consigneeAddress, "buyer", "");
		}

		public void TestOrigin()
		{
			AssertType<OriginWrapper>("Origin type", Provider.Origin);
		}

		public void TestNatureOfTransaction()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.ZG_TransNature = "34";
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_CEI_Instruction = instruction.PK;
			var entryLine = entry.MergedLines.AddNew();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.ZG_TransNature = "12";
			invoiceLine.JI_CEI = instruction.PK;
			invoiceLine.JI_CL = entryLine.PK;

			var wrapper = GoodsShipmentItemWrapper.New(entryLine);
			AssertEquals("NatureOfTransaction should be equal to InvoiceLine.ZG_TransNature", "12", wrapper.NatureOfTransaction);

			invoiceLine.ZG_TransNature = "";
			wrapper = GoodsShipmentItemWrapper.New(entryLine);
			AssertEquals("NatureOfTransaction should be equal to InvoiceLine.ZG_TransNature", string.Empty, wrapper.NatureOfTransaction);
		}

		public void TestPreviousDocument()
		{
			AssertType<Collection<IGoodsShipmentItemPreviousDocument>>("PreviousDocument type", Provider.PreviousDocument);
			AssertContainsExactElementsInAnyOrder("There should be 3 distinct (filtered on type, reference) elements in PreviousDocument, matching all invoice lines supporting documents.", new string[] { "380|PRE1", "270|PRE1", "270|PRE2" }, Provider.PreviousDocument.Select(x => x.Type + "|" + x.ReferenceNumber));

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_RL_NKOrigin = Core.Constants.CountryCodes.France;
			var instruction = declaration.CustomsEntryInstructions.AddNew();

			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_CEI_Instruction = instruction.PK;
			var entryLine = entry.MergedLines.AddNew();

			var wrapper = GoodsShipmentItemWrapper.New(entryLine);
			AssertEquals("There should be 0 PreviousDocument.", 0, wrapper.PreviousDocument.Count);
		}

		public void TestPackaging()
		{
			AssertType<Collection<IPackaging>>("Packaging type", Provider.Packaging);
			AssertContainsExactElementsInAnyOrder("Packaging should reflect all packages of all invoice lines, grouped by link package.", new string[] { "PK|10|MARKS", "CT|13|MARKS2" }, Provider.Packaging.Select(x => x.TypeOfPackages + "|" + x.NumberOfPackages + "|" + x.ShippingMarks));
		}

		public void TestSeller()
		{
			var expectedAddress = DeltaIEMessageWrapperTestHelper.GetAddressMock("sellerCity", "FJ", "SELPC11", "#1, ");
			DeltaIEMessageWrapperTestHelper.AssertSeller(Provider.Seller, expectedAddress, "seller", "");
		}

		public void TestSequenceNumber()
		{
			AssertEquals("SequenceNumber should be equal to CusEntryLine CL_LineNumber", "1", Provider.SequenceNumber);
		}

		public void TestSupportingDocument()
		{
			AssertType<Collection<ISupportingDocument>>("SupportingDocument type", Provider.SupportingDocument);
			AssertContainsExactElementsInAnyOrder("There should be 4 distinct (fitered on type, validity and reference number) elements in SupportingDocument, matching all invoice lines supporting documents.", new string[] { "N380|SUP1|2023-01-01", "N380|SUP2|2023-01-01", "N380|SUP1|2023-01-02", "N270|SUP1|2023-01-01" }, Provider.SupportingDocument.Select(x => x.Type + "|" + x.ReferenceNumber + "|" + x.DateOfValidity));

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_RL_NKOrigin = Core.Constants.CountryCodes.France;
			var instruction = declaration.CustomsEntryInstructions.AddNew();

			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_CEI_Instruction = instruction.PK;
			var entryLine = entry.MergedLines.AddNew();

			var wrapper = GoodsShipmentItemWrapper.New(entryLine);
			AssertEquals("There should be 0 SupportingDocument.", 0, wrapper.SupportingDocument.Count);
		}

		public void TestStatisticalValue()
		{
			AssertEquals("StatisticalValue should be equals entryline CL_StatisticalValue", 14d, Provider.StatisticalValue);
		}

		public void TestReferenceNumberUCR()
		{
			AssertEquals("ReferenceNumberUCR should be equals InvoiceHeader.JZ_UCR", "UCR_Reference", Provider.ReferenceNumberUCR);
		}

		public void TestProcedure()
		{
			AssertType<DeltaIEProcedureWrapper>("Procedure type", Provider.Procedure);
			AssertEquals("PreviousProcedure should be equal to JI_procedure 3st to 4nd char.", "71", Provider.Procedure.PreviousProcedure);
		}

		public void TestTransportDocument()
		{
			AssertType<Collection<ITransportDocument>>("TransportDocument type", Provider.TransportDocument);
			AssertContainsExactElementsInAnyOrder("There should be 3 distinct (fitered on type and reference number) elements in TransportDocument, matching all invoice lines additional infos of type TRA. ", new string[] { "CD1|TRA1", "CD2|TRA1", "CD2|TRA2" }, Provider.TransportDocument.Select(x => x.Type + "|" + x.ReferenceNumber));

			var wrapper = GoodsShipmentItemWrapper.New(Factory.New<Declaration.CusEntryLine>());
			AssertEquals("There should be 0 TransportDocument.", 0, wrapper.TransportDocument.Count);
		}

		public void TestValuationAdjustment()
		{
			AssertType<ValuationAdjustmentWrapper>("ValuationAdjustment type", Provider.ValuationAdjustment);
		}

		public void TestAdditionalDeclarationType()
		{
			AssertEquals("AdditionalDeclarationType should be captured from EntryInstruction.CEI_SubStyle", "V", Provider.AdditionalDeclarationType);
		}

		public void TestDescriptionOfGoods()
		{
			AssertEquals("Goods Description", Provider.DescriptionOfGoods);
		}
	}
}
