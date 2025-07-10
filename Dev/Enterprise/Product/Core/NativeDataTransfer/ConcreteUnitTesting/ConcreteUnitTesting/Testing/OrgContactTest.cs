using System.IO;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DataTransfer.Native.Business.Xsd;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DataTransfer.Native.ConcreteUnitTesting.Testing
{
	class OrgContactTest : TestCaseWithFactory
	{
		public void TestOrgContactImport_NoGender()
		{
			var encoding = new System.Text.UTF8Encoding();
			using (var stream = new MemoryStream(encoding.GetBytes(OrgHeaderWithContactNoGender)))
			{
				var manager = new ImportServiceManagerForTesting();
				manager.ImportService.Import(stream);

				string expectedLog = @"
--- Start Import Process --------------------------------------------------------------
Processed: Organization
--- Import Process Finished -----------------------------------------------------------
OrgHeader - 1 inserts, 0 updates, 0 deletes
OrgMiscServ - 1 inserts, 0 updates, 0 deletes
OrgContact - 1 inserts, 0 updates, 0 deletes
OrgAddress - 1 inserts, 0 updates, 0 deletes
OrgAddressCapability - 1 inserts, 0 updates, 0 deletes
OrgCompanyData - 1 inserts, 0 updates, 0 deletes
OrgInvoiceRollupOrGroup - 1 inserts, 0 updates, 0 deletes
				".Trim();

				AssertMultilineASCIIEquals("Log Text on Add", expectedLog, manager.GetLogs());
			}

			var org = new BusinessObjectFactory().Load<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, "WTGSYD"));
			AssertEquals(1, org.Length);
			AssertEquals(1, org[0].Contacts.Count);

			var contact = org[0].Contacts[0];
			AssertEquals("N", contact.OC_Gender);
		}

		public void TestOrgContactImport_WrongGender()
		{
			var encoding = new System.Text.UTF8Encoding();
			var xml = string.Format(OrgHeaderWithContactAndGender, "false");
			using (var stream = new MemoryStream(encoding.GetBytes(xml)))
			{
				var manager = new ImportServiceManagerForTesting();
				manager.ImportService.Import(stream);

				string expectedLog = @"
--- Start Import Process --------------------------------------------------------------
Processed: Organization
--- Import Process Finished -----------------------------------------------------------
OrgHeader - 1 inserts, 0 updates, 0 deletes
OrgMiscServ - 1 inserts, 0 updates, 0 deletes
OrgContact - 1 inserts, 0 updates, 0 deletes
OrgAddress - 1 inserts, 0 updates, 0 deletes
OrgAddressCapability - 1 inserts, 0 updates, 0 deletes
OrgCompanyData - 1 inserts, 0 updates, 0 deletes
OrgInvoiceRollupOrGroup - 1 inserts, 0 updates, 0 deletes
				".Trim();

				AssertMultilineASCIIEquals("Log Text on Add", expectedLog, manager.GetLogs());
			}

			var org = new BusinessObjectFactory().Load<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, "WTGSYD"));
			AssertEquals(1, org.Length);
			AssertEquals(1, org[0].Contacts.Count);

			var contact = org[0].Contacts[0];
			AssertEquals("N", contact.OC_Gender);
		}

		public void TestOrgContactImport_ExistingGender()
		{
			var encoding = new System.Text.UTF8Encoding();
			var xml = string.Format(OrgHeaderWithContactAndGender, "O");
			using (var stream = new MemoryStream(encoding.GetBytes(xml)))
			{
				var manager = new ImportServiceManagerForTesting();
				manager.ImportService.Import(stream);

				string expectedLog = @"
--- Start Import Process --------------------------------------------------------------
Processed: Organization
--- Import Process Finished -----------------------------------------------------------
OrgHeader - 1 inserts, 0 updates, 0 deletes
OrgMiscServ - 1 inserts, 0 updates, 0 deletes
OrgContact - 1 inserts, 0 updates, 0 deletes
OrgAddress - 1 inserts, 0 updates, 0 deletes
OrgAddressCapability - 1 inserts, 0 updates, 0 deletes
OrgCompanyData - 1 inserts, 0 updates, 0 deletes
OrgInvoiceRollupOrGroup - 1 inserts, 0 updates, 0 deletes
				".Trim();

				AssertMultilineASCIIEquals("Log Text on Add", expectedLog, manager.GetLogs());
			}

			var org = new BusinessObjectFactory().Load<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, "WTGSYD"));
			AssertEquals(1, org.Length);
			AssertEquals(1, org[0].Contacts.Count);

			var contact = org[0].Contacts[0];
			AssertEquals("O", contact.OC_Gender);
		}

		public void TestOrgContactGenderParsesToCorrectType()
		{
			var xsdGenerator = new NativeXsdGenerator();
			var definition = TestUtil.GetEntitySetDefinition("Organization");
			var contact = definition.Entities.FirstOrDefault(e => e.EntityName == "OrgContact");
			var genderDef = contact.PropertyDefinitions.FirstOrDefault(d => d.PropertyName == "Gender");
			var result = XsdDataTypeFactory.Map(genderDef.ColumnDef);

			AssertEquals("xs:string", result.Name);
		}

		public void TestOrgContactImport_UpdateExisting()
		{
			var encoding = new System.Text.UTF8Encoding();
			var xml = string.Format(OrgHeaderWithContactAndGender, "O");
			using (var stream = new MemoryStream(encoding.GetBytes(xml)))
			{
				var manager = new ImportServiceManagerForTesting();
				manager.ImportService.Import(stream);
			}

			var org = new BusinessObjectFactory().Load<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, "WTGSYD"));
			AssertEquals(1, org.Length);
			AssertEquals(1, org[0].Contacts.Count);

			var contact = org[0].Contacts[0];
			AssertEquals("O", contact.OC_Gender);

			xml = string.Format(OrgHeaderWithContactAndGender, "M");

			using (var stream = new MemoryStream(encoding.GetBytes(xml)))
			{
				var manager = new ImportServiceManagerForTesting();
				manager.ImportService.Import(stream);

				string expectedLog = @"
--- Start Import Process --------------------------------------------------------------
Processed: Organization
--- Import Process Finished -----------------------------------------------------------
OrgHeader - 0 inserts, 0 updates, 0 deletes
OrgMiscServ - 0 inserts, 0 updates, 0 deletes
OrgContact - 0 inserts, 1 updates, 0 deletes
OrgAddress - 0 inserts, 0 updates, 0 deletes
OrgAddressCapability - 0 inserts, 0 updates, 0 deletes
OrgCompanyData - 0 inserts, 0 updates, 0 deletes
OrgInvoiceRollupOrGroup - 0 inserts, 0 updates, 0 deletes
				".Trim();

				AssertMultilineASCIIEquals("Log Text on Add", expectedLog, manager.GetLogs());
			}

			org = new BusinessObjectFactory().Load<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, "WTGSYD"));
			AssertEquals(1, org.Length);
			AssertEquals(1, org[0].Contacts.Count);

			contact = org[0].Contacts[0];
			AssertEquals("M", contact.OC_Gender);
		}

		#region Constants

		const string OrgHeaderWithContactNoGender = @"
<Native xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"" version=""2.0"">
  <Header>
    <OwnerCode>EDICUS</OwnerCode>
    <EnableCodeMapping>true</EnableCodeMapping>
  </Header>
  <Body>
    <Organization version=""2.0"">
      <OrgHeader Action=""MERGE"">
        <PK>4598518d-c7c0-49be-ac2f-3759127fe758</PK>
        <Code>WTGSYD</Code>
        <IsActive>true</IsActive>
        <FullName>WTG</FullName>
        <IsConsignee>false</IsConsignee>
        <IsConsignor>true</IsConsignor>
        <IsTransportClient>false</IsTransportClient>
        <IsWarehouseClient>false</IsWarehouseClient>
        <IsForwarder>false</IsForwarder>
        <IsShippingProvider>false</IsShippingProvider>
        <IsAirWholesaler>false</IsAirWholesaler>
        <IsSeaWholesaler>false</IsSeaWholesaler>
        <IsRailProvider>false</IsRailProvider>
        <IsLineHaulProvider>false</IsLineHaulProvider>
        <IsMiscFreightServices>false</IsMiscFreightServices>
        <IsAirCTO>false</IsAirCTO>
        <IsAirLine>false</IsAirLine>
        <IsBroker>false</IsBroker>
        <IsContainerYard>false</IsContainerYard>
        <IsLocalTransport>false</IsLocalTransport>
        <IsPackDepot>false</IsPackDepot>
        <IsSeaCTO>false</IsSeaCTO>
        <IsShippingLine>false</IsShippingLine>
        <IsUnpackDepot>false</IsUnpackDepot>
        <IsRailHead>false</IsRailHead>
        <IsRoadFreightDepot>false</IsRoadFreightDepot>
        <IsShippingConsortium>false</IsShippingConsortium>
        <IsFumigationContractor>false</IsFumigationContractor>
        <IsGlobalAccount>false</IsGlobalAccount>
        <IsNationalAccount>false</IsNationalAccount>
        <IsSalesLead>false</IsSalesLead>
        <IsCompetitor>false</IsCompetitor>
        <IsTempAccount>false</IsTempAccount>
        <IsPersonalEffectsAccount>false</IsPersonalEffectsAccount>
        <IsDistributionCentre>false</IsDistributionCentre>
        <IsUserFlag1>false</IsUserFlag1>
        <IsUserFlag2>false</IsUserFlag2>
        <IsUserFlag3>false</IsUserFlag3>
        <IsUserFlag4>false</IsUserFlag4>
        <IsUserFlag5>false</IsUserFlag5>
        <IsUserFlag6>false</IsUserFlag6>
        <IsUserFlag7>false</IsUserFlag7>
        <IsUserFlag8>false</IsUserFlag8>
        <IsUserFlag9>false</IsUserFlag9>
        <IsUserFlag10>false</IsUserFlag10>
        <IsUserFlag11>false</IsUserFlag11>
        <IsUserFlag12>false</IsUserFlag12>
        <IsUserFlag13>false</IsUserFlag13>
        <IsUserFlag14>false</IsUserFlag14>
        <Language>EN</Language>
        <ScreeningStatus>NOT</ScreeningStatus>
        <SystemLastEditTimeUtc>2019-03-18T22:18:00</SystemLastEditTimeUtc>
        <SystemCreateTimeUtc>2019-03-18T22:18:00</SystemCreateTimeUtc>
        <IsUserFlag15>false</IsUserFlag15>
        <IsUserFlag16>false</IsUserFlag16>
        <IsUserFlag17>false</IsUserFlag17>
        <IsUserFlag18>false</IsUserFlag18>
        <IsUserFlag19>false</IsUserFlag19>
        <IsUserFlag20>false</IsUserFlag20>
        <IsUserFlag21>false</IsUserFlag21>
        <IsUserFlag22>false</IsUserFlag22>
        <IsUserFlag23>false</IsUserFlag23>
        <IsUserFlag24>false</IsUserFlag24>
        <IsControllingCustomer>false</IsControllingCustomer>
        <IsControllingAgent>false</IsControllingAgent>
        <Category>BUS</Category>
        <OrgMiscServ Action=""MERGE"">
          <PK>2bb553f1-8943-44f2-afb6-44de725a01bb</PK>
          <Airline3CharCode></Airline3CharCode>
          <IMEftCustomsFromImport>false</IMEftCustomsFromImport>
          <IMEftQuarantineFromImport>false</IMEftQuarantineFromImport>
          <IMEftHoldUntilPayAuthorised>false</IMEftHoldUntilPayAuthorised>
          <IMEstDaysDeliveryAir>0</IMEstDaysDeliveryAir>
          <IMEstDaysDeliveryFCL>0</IMEstDaysDeliveryFCL>
          <IMEstDaysDeliveryLCL>0</IMEstDaysDeliveryLCL>
          <IMMaxEFTAmount>0.0000</IMMaxEFTAmount>
          <IMMinEFTAmount>0.0000</IMMinEFTAmount>
          <IMEFTBankAccount></IMEFTBankAccount>
          <IMEFTBankBSB></IMEFTBankBSB>
          <IMIsGSTDeferred>false</IMIsGSTDeferred>
          <IMMergeCustomsInvoiceLinesBy>DEF</IMMergeCustomsInvoiceLinesBy>
          <IMOrderLineAttrib1></IMOrderLineAttrib1>
          <IMOrderLineAttrib2></IMOrderLineAttrib2>
          <IMOrderLineAttrib3></IMOrderLineAttrib3>
          <IMOriginalSeaBills>3</IMOriginalSeaBills>
          <IMCopySeaBills>3</IMCopySeaBills>
          <IMSendImportDocsTo>IMP</IMSendImportDocsTo>
          <IMSendSeaImportDocsTo>IMP</IMSendSeaImportDocsTo>
          <IMImporterCategory>STD</IMImporterCategory>
          <IMAirDepotFreeDays>1</IMAirDepotFreeDays>
          <IMSeaDepotFreeDays>3</IMSeaDepotFreeDays>
          <IMImporterOwnsPartNumbers>false</IMImporterOwnsPartNumbers>
          <IMOrderStatusCodePairList></IMOrderStatusCodePairList>
          <IMOrderLineStatusCodePairList></IMOrderLineStatusCodePairList>
          <IMLastOrderReference></IMLastOrderReference>
          <IMDefaultToNewOrdersToNextOrderNum>false</IMDefaultToNewOrdersToNextOrderNum>
          <IMFCLEquipmentNeeded></IMFCLEquipmentNeeded>
          <IMLCLEquipmentNeeded></IMLCLEquipmentNeeded>
          <IMAirEquipmentNeeded></IMAirEquipmentNeeded>
          <IMDefaultINCOTerm>FOB</IMDefaultINCOTerm>
          <IMAutoImpJobRefered>false</IMAutoImpJobRefered>
          <IMAutoPopulateOwnerRefWithOrderNums>DEF</IMAutoPopulateOwnerRefWithOrderNums>
          <IMPartAttrib1Type></IMPartAttrib1Type>
          <IMPartAttrib1Name></IMPartAttrib1Name>
          <IMPartAttrib1IsMandatory>false</IMPartAttrib1IsMandatory>
          <IMAttrib1IsKey>false</IMAttrib1IsKey>
          <IMPartAttrib2Type></IMPartAttrib2Type>
          <IMPartAttrib2Name></IMPartAttrib2Name>
          <IMPartAttrib2IsMandatory>false</IMPartAttrib2IsMandatory>
          <IMAttrib2IsKey>false</IMAttrib2IsKey>
          <IMPartAttrib3Type></IMPartAttrib3Type>
          <IMPartAttrib3Name></IMPartAttrib3Name>
          <IMPartAttrib3IsMandatory>false</IMPartAttrib3IsMandatory>
          <IMAttrib3IsKey>false</IMAttrib3IsKey>
          <IMUseExpiryDate>false</IMUseExpiryDate>
          <IMUsePackingDate>false</IMUsePackingDate>
          <IMImporterRequiresOrderNumbersOnDocs>false</IMImporterRequiresOrderNumbersOnDocs>
          <IMJobRequireOrderTrackLink>false</IMJobRequireOrderTrackLink>
          <IMDocumentAddressPreference></IMDocumentAddressPreference>
          <IMDefaultWarehousePickOption>AUT</IMDefaultWarehousePickOption>
          <IMInvoiceDetailReportSort>DEF</IMInvoiceDetailReportSort>
          <IMInvoiceDetailReportSort2>DEF</IMInvoiceDetailReportSort2>
          <IMInvoiceDetailReportSort3>DEF</IMInvoiceDetailReportSort3>
          <IMShowDutyOnWarehouseEntries>false</IMShowDutyOnWarehouseEntries>
          <LandedCostMarginPercent1>0.00</LandedCostMarginPercent1>
          <LandedCostMarginPercent2>0.00</LandedCostMarginPercent2>
          <LandedCostMarginPercent3>0.00</LandedCostMarginPercent3>
          <LastArchiveDate></LastArchiveDate>
          <EXJobRequireOrderTrackLink>false</EXJobRequireOrderTrackLink>
          <EXExporterRequiresOrderNumbersOnDocs>false</EXExporterRequiresOrderNumbersOnDocs>
          <EXGoodsDescription></EXGoodsDescription>
          <EXExporterCategory>STD</EXExporterCategory>
          <EXFCLEquipmentNeeded></EXFCLEquipmentNeeded>
          <EXLCLEquipmentNeeded></EXLCLEquipmentNeeded>
          <EXAirEquipmentNeeded></EXAirEquipmentNeeded>
          <EXAllowedToPrintOriginalBL>false</EXAllowedToPrintOriginalBL>
          <EXDocumentAddressPreference></EXDocumentAddressPreference>
          <EXDefaultDGContactPhoneUsed></EXDefaultDGContactPhoneUsed>
          <EXDefaultInvoicePriceFromProductLastCost></EXDefaultInvoicePriceFromProductLastCost>
          <EXHandlingInstuctions></EXHandlingInstuctions>
          <EXDefaultIncoTerm>FOB</EXDefaultIncoTerm>
          <EXMergeCustomsInvoiceLinesBy>NON</EXMergeCustomsInvoiceLinesBy>
          <EXPreAllocPrefix></EXPreAllocPrefix>
          <FWAgentCategory>STD</FWAgentCategory>
          <FWAgentBelongsToGroup>false</FWAgentBelongsToGroup>
          <FWHandlesAir>false</FWHandlesAir>
          <FWHandlesSea>false</FWHandlesSea>
          <FWHandlesSeaForPortOrCountry></FWHandlesSeaForPortOrCountry>
          <FWHandlesAirForPortOrCountry></FWHandlesAirForPortOrCountry>
          <FWRequestForCreditAllowed>false</FWRequestForCreditAllowed>
          <FWBillCollectFeesOnSingleInvoice>true</FWBillCollectFeesOnSingleInvoice>
          <FWDealDirectlyWithUltimates>false</FWDealDirectlyWithUltimates>
          <FWIATACode></FWIATACode>
          <FWIATAAccountNumber></FWIATAAccountNumber>
          <FWDirectAMSReporter>false</FWDirectAMSReporter>
          <CRCarrierCategory></CRCarrierCategory>
          <SVServicesCategory></SVServicesCategory>
          <CMSalesCategory></CMSalesCategory>
          <CMCompetitorActivity>FRT</CMCompetitorActivity>
          <CMLastCallDate></CMLastCallDate>
          <CMLastUnactionedCallDate></CMLastUnactionedCallDate>
          <CMClientSize></CMClientSize>
          <CMNoOfEmployees>0</CMNoOfEmployees>
          <CMEstimatedDateToClose></CMEstimatedDateToClose>
          <CMGrowthOutlook></CMGrowthOutlook>
          <CMFollowUpDate></CMFollowUpDate>
          <CMDoesExports>false</CMDoesExports>
          <CMDoesImports>false</CMDoesImports>
          <CMUseTradeLaneFigures>true</CMUseTradeLaneFigures>
          <CMTotalClientRevenue>0.0000</CMTotalClientRevenue>
          <CMPercentage>0.000</CMPercentage>
          <CMAcheivableClientRevenue>0.0000</CMAcheivableClientRevenue>
          <CMEstimatedProfit>0.0000</CMEstimatedProfit>
          <CMWarehouseRevenue>0.0000</CMWarehouseRevenue>
          <CMConsultingRevenue>0.0000</CMConsultingRevenue>
          <CMOverallClientRelation>0</CMOverallClientRelation>
          <CMClientsDesireToRemain>0</CMClientsDesireToRemain>
          <CMEaseClientCanBePoached>0</CMEaseClientCanBePoached>
          <CMAmountOfElectronicIntegration>0</CMAmountOfElectronicIntegration>
          <CMOverallEffectOfClientOnAirfreightCosts></CMOverallEffectOfClientOnAirfreightCosts>
          <CMOverallEffectOfClientOnLCLCosts></CMOverallEffectOfClientOnLCLCosts>
          <CMOverallEffectOfClientOnTEUCosts></CMOverallEffectOfClientOnTEUCosts>
          <CMOverallEffectOfClientOnWarehousingCosts></CMOverallEffectOfClientOnWarehousingCosts>
          <CMOverallEffectOfClientOnOtherCosts></CMOverallEffectOfClientOnOtherCosts>
          <CMAmountOfBusinessWon>0</CMAmountOfBusinessWon>
          <CMSalesTerritory></CMSalesTerritory>
          <CMIsHouseAccount>false</CMIsHouseAccount>
          <CMCommission></CMCommission>
          <CMClientCommenced></CMClientCommenced>
          <CMClientPortalHomePage></CMClientPortalHomePage>
          <CICompetitorCategory></CICompetitorCategory>
          <CMDistanceCalculationProvider>DEF</CMDistanceCalculationProvider>
          <CMDistanceCalculationVersion></CMDistanceCalculationVersion>
          <CMDistanceCalculationMethod></CMDistanceCalculationMethod>
          <CMPaidUpCapital>0.0000</CMPaidUpCapital>
          <CMEstablishedDate></CMEstablishedDate>
          <CIEstimatedStaffThisLocation>0</CIEstimatedStaffThisLocation>
          <CITypeOfService></CITypeOfService>
          <CIEstimatedStaffThisCountry>0</CIEstimatedStaffThisCountry>
          <CISellingStyle></CISellingStyle>
          <CITurnover>0.0000</CITurnover>
          <CIProfit>0.0000</CIProfit>
          <CICapitalEmployed>0.0000</CICapitalEmployed>
          <CICompetitiveRanking>0</CICompetitiveRanking>
          <CIStrength></CIStrength>
          <CIWeaknesses></CIWeaknesses>
          <CIOpportunities></CIOpportunities>
          <CIThreats></CIThreats>
          <WhsOverrideSystemPickingRules>false</WhsOverrideSystemPickingRules>
          <WhsPickFromDefaultFIFO>0</WhsPickFromDefaultFIFO>
          <WhsPickFromFullPallets>0</WhsPickFromFullPallets>
          <WhsPickFromConsolidatedFullPallets>0</WhsPickFromConsolidatedFullPallets>
          <WhsPickFromPickFaces>0</WhsPickFromPickFaces>
          <WhsPickFromPalletOverflow>0</WhsPickFromPalletOverflow>
          <WhsPickFromBrokenPallets>0</WhsPickFromBrokenPallets>
          <WhsPickFromFIFOBulkOnly>0</WhsPickFromFIFOBulkOnly>
          <WhsPickSortArrivalDate>0</WhsPickSortArrivalDate>
          <WhsPickSortExpiryDate>0</WhsPickSortExpiryDate>
          <WhsPickSortPackingDate>0</WhsPickSortPackingDate>
          <WhsPickSortPickFace>0</WhsPickSortPickFace>
          <WhsPickSortLocationRow>0</WhsPickSortLocationRow>
          <WhsPickSortLocationColumn>0</WhsPickSortLocationColumn>
          <WhsPickSortLocationLevel>0</WhsPickSortLocationLevel>
          <WhsExpiryNotificationFromDefaults>true</WhsExpiryNotificationFromDefaults>
          <WhsExpiryNotificationPeriod>0</WhsExpiryNotificationPeriod>
          <WhsClientInvoiceFormat></WhsClientInvoiceFormat>
          <WhsOrderNumberUniquenessStrategy></WhsOrderNumberUniquenessStrategy>
          <WhsOverrideSystemPutawayRules>false</WhsOverrideSystemPutawayRules>
          <WhsPutawayToLocation>0</WhsPutawayToLocation>
          <WhsPutawayToPickFace>0</WhsPutawayToPickFace>
          <WhsPutawayToProductArea>0</WhsPutawayToProductArea>
          <WhsPutawayToClientArea>0</WhsPutawayToClientArea>
          <WhsPutawaySortLocationColumn>0</WhsPutawaySortLocationColumn>
          <WhsPutawaySortLocationLevel>0</WhsPutawaySortLocationLevel>
          <WhsPutawaySortLocationRow>0</WhsPutawaySortLocationRow>
          <WhsPutawaySameProductTogether>false</WhsPutawaySameProductTogether>
          <WhsEDIChangeMessageCancelOrderLinesNotIncludedInMessage>true</WhsEDIChangeMessageCancelOrderLinesNotIncludedInMessage>
          <WhsGenerateBackOrdersOnShortfalls>false</WhsGenerateBackOrdersOnShortfalls>
          <WhsAutoPckEdiOrders>false</WhsAutoPckEdiOrders>
          <WhsOrderFulfillmentRule>NON</WhsOrderFulfillmentRule>
          <WhsPackingSlipOrderBy>DEF</WhsPackingSlipOrderBy>
          <WhsDefaultWarehousePickMode>ASP</WhsDefaultWarehousePickMode>
          <WhsDefaultWarehouseRollUp>false</WhsDefaultWarehouseRollUp>
          <WhsTransportPayer>DEF</WhsTransportPayer>
          <MinimumShelfLifeAccepted>0</MinimumShelfLifeAccepted>
          <WhsABCAnalysisEnabled>false</WhsABCAnalysisEnabled>
          <WhsABCAnalysisMethod>DEF</WhsABCAnalysisMethod>
          <WhsABCAnalysisPeriod>DEF</WhsABCAnalysisPeriod>
          <IsScanPackQtyAllowed>false</IsScanPackQtyAllowed>
          <IsLabelPrintedOnClosePackage>true</IsLabelPrintedOnClosePackage>
          <CustomAttrib1></CustomAttrib1>
          <CustomAttrib2></CustomAttrib2>
          <CustomAttrib3></CustomAttrib3>
          <CustomDate1></CustomDate1>
          <CustomDate2></CustomDate2>
          <CustomDate3></CustomDate3>
          <CustomDecimal1>0.000</CustomDecimal1>
          <CustomDecimal2>0.000</CustomDecimal2>
          <CustomDecimal3>0.000</CustomDecimal3>
          <CustomFlag1>false</CustomFlag1>
          <CustomFlag2>false</CustomFlag2>
          <CustomFlag3>false</CustomFlag3>
          <CustomFlag4>false</CustomFlag4>
          <IMOwnsProducts>false</IMOwnsProducts>
          <EXOwnsProducts>false</EXOwnsProducts>
          <CRVoyageRecyclingPeriodInMonths>-1</CRVoyageRecyclingPeriodInMonths>
          <IsAutoPackAllowed>true</IsAutoPackAllowed>
          <WhsOrderDefaultPickPriority>0</WhsOrderDefaultPickPriority>
          <ConsigneeAuthorityToLeave>DEF</ConsigneeAuthorityToLeave>
          <ConsignorAuthorityToLeave>DEF</ConsignorAuthorityToLeave>
          <IMAllowOrders>true</IMAllowOrders>
          <CMPeriodOfActivity></CMPeriodOfActivity>
          <CMIndustryVertical></CMIndustryVertical>
          <WhsIsRecalculateOrderPricing>false</WhsIsRecalculateOrderPricing>
          <CMAuthorityToLeave>DEF</CMAuthorityToLeave>
          <TBAllowMixedAccountNumbersOnManifest>false</TBAllowMixedAccountNumbersOnManifest>
          <IMAllowAttachedOrderXMLUpdate>false</IMAllowAttachedOrderXMLUpdate>
          <EXValidationForUnauditedClassification></EXValidationForUnauditedClassification>
          <IMValidationForUnauditedClassification></IMValidationForUnauditedClassification>
          <IMBalanceInvoicePackage>false</IMBalanceInvoicePackage>
          <WhsPackageToleranceEnabled>false</WhsPackageToleranceEnabled>
          <WhsPackageWeightTolerancePercent>0.0</WhsPackageWeightTolerancePercent>
          <WhsPickFromHighPriorityLocations>0</WhsPickFromHighPriorityLocations>
          <WhsEnforceScanOfProductsWhenPackingTote>false</WhsEnforceScanOfProductsWhenPackingTote>
          <ARGlobalCreditApproved>false</ARGlobalCreditApproved>
          <ARGlobalCreditLimit>0.0000</ARGlobalCreditLimit>
          <ARGlobalOnCreditHold>false</ARGlobalOnCreditHold>
          <IMDefaultServiceLevel TableName=""RefServiceLevel"" />
          <EXDefaultServiceLevel TableName=""RefServiceLevel"" />
          <EXDefCurrency TableName=""RefCurrency"" />
          <EXDefaultCntryOfOrigin TableName=""RefCountry"">
            <Code>AU</Code>
            <PK>e4eb6e97-aa78-4c46-bf86-35b7fbb5fb0e</PK>
          </EXDefaultCntryOfOrigin>
          <EXDefaultDGContact TableName=""OrgContact"" />
          <FWDefCurrency TableName=""RefCurrency"">
            <Code>AUD</Code>
            <PK>53feb23f-a7b0-4e0d-af5d-f6f6e5417399</PK>
          </FWDefCurrency>
          <CMMainImportCmdty TableName=""RefCommodityCode"" />
          <CMMainExportCmdty TableName=""RefCommodityCode"" />
          <WhsDefaultWarehouse TableName=""GlbBranch"" />
          <WhsPackingSlip TableName=""StmTemplate"" />
          <CMPreferredPaymentCompany TableName=""GlbCompany"" />
          <CartonGroup TableName=""WhsCartonGroup"" />
          <OrgSecurityGroup TableName=""GlbGroup"" />
          <ARGlobalCreditGroup TableName=""OrgHeader"" />
          <ARGlobalCreditCurrency TableName=""RefCurrency"" />
        </OrgMiscServ>
        <OrgContactCollection>
          <OrgContact Action=""MERGE"">
            <PK>06da560a-82f4-4ec2-b476-9bc8bf18d5e9</PK>
            <IsActive>true</IsActive>
            <ContactName>Anton</ContactName>
            <Salutation></Salutation>
            <Language>EN</Language>
            <NotifyMode>EML</NotifyMode>
            <Title></Title>            
            <JobCategory>EMU</JobCategory>
            <Phone></Phone>
            <PhoneExtension></PhoneExtension>
            <Fax></Fax>
            <Mobile></Mobile>
            <HomePhone></HomePhone>
            <Pager></Pager>
            <OtherPhone></OtherPhone>
            <ProfilePhoto></ProfilePhoto>
            <WebAccessEnabled>false</WebAccessEnabled>
            <WebContractSignedDate></WebContractSignedDate>
            <Birthday></Birthday>
            <YearJoinedIndustry></YearJoinedIndustry>
            <YearJoinedCompany></YearJoinedCompany>
            <ContactSource></ContactSource>
            <DetailsVerified></DetailsVerified>
            <Email>anton.gorlin@wisetechglobal.com</Email>
            <AttachmentType>PDF</AttachmentType>
            <PersonalInfo></PersonalInfo>
            <PasswordHash></PasswordHash>
            <PasswordHashIterations>0</PasswordHashIterations>
            <PasswordSalt></PasswordSalt>
            <Nationality TableName=""RefCountry"">
              <Code></Code>
            </Nationality>
            <OrgAddress />
            <AddressOverride TableName=""OrgHeader"" />
          </OrgContact>
        </OrgContactCollection>
        <OrgAddressCollection>
          <OrgAddress Action=""MERGE"">
            <PK>d85b7891-69b8-47fd-9dda-90980a6ed66b</PK>
            <IsActive>true</IsActive>
            <Code>72 O'RIORDAN ST</Code>
            <CompanyNameOverride></CompanyNameOverride>
            <Address1>72 O'RIORDAN ST</Address1>
            <Address2></Address2>
            <State>NSW</State>
            <PostCode>2015</PostCode>
            <Phone></Phone>
            <Fax></Fax>
            <Mobile></Mobile>
            <PickupFromTimeOnly></PickupFromTimeOnly>
            <PickupToTimeOnly></PickupToTimeOnly>
            <DeliverFromTimeOnly></DeliverFromTimeOnly>
            <DeliverToTimeOnly></DeliverToTimeOnly>
            <DoNotAttendFrom></DoNotAttendFrom>
            <DoNotAttendTo></DoNotAttendTo>
            <DockLeveler>false</DockLeveler>
            <ForkLift>false</ForkLift>
            <PalletJack>false</PalletJack>
            <ContainerHandling></ContainerHandling>
            <AccessPoint></AccessPoint>
            <LabourRequired></LabourRequired>
            <CommunicationRequired></CommunicationRequired>
            <Dock_Height></Dock_Height>
            <FCLEquipmentNeeded>WUP</FCLEquipmentNeeded>
            <LCLEquipmentNeeded>PSL</LCLEquipmentNeeded>
            <AIREquipmentNeeded>PSL</AIREquipmentNeeded>
            <Email></Email>
            <UseCumulativeFreeWaitingTime>false</UseCumulativeFreeWaitingTime>
            <DeliveryRoute></DeliveryRoute>
            <DeliveryRouteSequence>0</DeliveryRouteSequence>
            <AddressMap></AddressMap>
            <AuthorityToLeave>DEF</AuthorityToLeave>
            <OtherWarehouseFacilities></OtherWarehouseFacilities>
            <LoadingUnloadingConstraints></LoadingUnloadingConstraints>
            <City>ALEXANDRIA</City>
            <GroupNumber>0</GroupNumber>
            <AdditionalAddressInformation></AdditionalAddressInformation>
            <VerifiesContainerGrossWeight>false</VerifiesContainerGrossWeight>
            <Language>EN</Language>
            <SystemCreateTimeUtc>2019-03-18T22:18:00</SystemCreateTimeUtc>
            <SystemLastEditTimeUtc>2019-03-18T22:18:00</SystemLastEditTimeUtc>
            <OrgAddressCapabilityCollection>
              <OrgAddressCapability Action=""MERGE"">
                <PK>126fb8ed-9fa1-4988-8792-bf87b953fa7c</PK>
                <AddressType>OFC</AddressType>
                <IsMainAddress>true</IsMainAddress>
              </OrgAddressCapability>
            </OrgAddressCapabilityCollection>
            <RelatedPortCode TableName=""RefUNLOCO"">
              <Code>AUSYD</Code>
              <PK>ad87872e-96b8-4c9a-bc0b-6a4088247a64</PK>
            </RelatedPortCode>
            <CountryCode TableName=""RefCountry"">
              <Code>AU</Code>
              <PK>e4eb6e97-aa78-4c46-bf86-35b7fbb5fb0e</PK>
            </CountryCode>
          </OrgAddress>
        </OrgAddressCollection>
        <OrgCompanyDataCollection>
          <OrgCompanyData Action=""MERGE"">
            <PK>e1db7069-5703-4a5d-8c25-082d4d3e5db0</PK>
            <IsDebtor>false</IsDebtor>
            <IsCreditor>false</IsCreditor>
            <APCategory></APCategory>
            <APExternalCreditorCode></APExternalCreditorCode>
            <APCreditLimit>0.0000</APCreditLimit>
            <APPayInvoiceAfterPostingDefault>false</APPayInvoiceAfterPostingDefault>
            <APPaymentTermDays>0</APPaymentTermDays>
            <APPaymentTerms>COD</APPaymentTerms>
            <APCreditAgreedPaymentMethod></APCreditAgreedPaymentMethod>
            <APWHTApplicable>false</APWHTApplicable>
            <APAirlineAccountNumber></APAirlineAccountNumber>
            <APQualityAssured>false</APQualityAssured>
            <APQualityAssuredCheckedDate></APQualityAssuredCheckedDate>
            <APCostsSelfBilled>false</APCostsSelfBilled>
            <APPrintContractorForm>false</APPrintContractorForm>
            <APVATConfig>DEF</APVATConfig>
            <ARCategory></ARCategory>
            <ARExternalDebtorCode></ARExternalDebtorCode>
            <ARQualityAssured>false</ARQualityAssured>
            <ARQualityAssuredCheckedDate></ARQualityAssuredCheckedDate>
            <ARAutoUpdateRates>true</ARAutoUpdateRates>
            <ARCombinedStatementInvoice>false</ARCombinedStatementInvoice>
            <ARConsolidatedAccountingCategory></ARConsolidatedAccountingCategory>
            <ARForeignCurrStatement>false</ARForeignCurrStatement>
            <ARBuyersConsolInvoicingStyle>DEF</ARBuyersConsolInvoicingStyle>
            <ARCreditLimit>0.0000</ARCreditLimit>
            <ARTemporaryCreditLimitIncrease>0.0000</ARTemporaryCreditLimitIncrease>
            <ARTemporaryCreditLimitIncreaseExpiry></ARTemporaryCreditLimitIncreaseExpiry>
            <ARCreditRating></ARCreditRating>
            <ARCreditAgreedPaymentMethod></ARCreditAgreedPaymentMethod>
            <ARSeaInvoiceTermDays>COD</ARSeaInvoiceTermDays>
            <ARSeaInvoiceTerms>0</ARSeaInvoiceTerms>
            <ARAirInvoiceTermDays>COD</ARAirInvoiceTermDays>
            <ARAirInvoiceTerms>0</ARAirInvoiceTerms>
            <ARInvoiceTerms>COD</ARInvoiceTerms>
            <ARInvoiceTermDays>0</ARInvoiceTermDays>
            <ARTreatDisbursementsAsStandardValue>0.0000</ARTreatDisbursementsAsStandardValue>
            <ARDisbursementInvoiceTerms>COD</ARDisbursementInvoiceTerms>
            <ARDisbursementInvoiceTermDays>0</ARDisbursementInvoiceTermDays>
            <ARDontShowTaxOnDocs>false</ARDontShowTaxOnDocs>
            <ARSellersConsolInvoicingStyle>DEF</ARSellersConsolInvoicingStyle>
            <AROnCreditHold>false</AROnCreditHold>
            <ARCreditApproved>true</ARCreditApproved>
            <ARUseSettlementGroupCreditLimit>false</ARUseSettlementGroupCreditLimit>
            <ARAccountAndCreditReviewDue></ARAccountAndCreditReviewDue>
            <ARAllowMultiCurrencyPayment>false</ARAllowMultiCurrencyPayment>
            <ARPreviousChequeDrawer></ARPreviousChequeDrawer>
            <ARPreviousChequeDrawerBank></ARPreviousChequeDrawerBank>
            <ARPreviousChequeDrawerBankBranch></ARPreviousChequeDrawerBankBranch>
            <ARReceiptInvoiceAfterPostingDefault>false</ARReceiptInvoiceAfterPostingDefault>
            <AREftCustomsPaymentMethod></AREftCustomsPaymentMethod>
            <ARWHTApplicable>false</ARWHTApplicable>
            <ARIncludeInwardsWhsConsolidatedInvoice>true</ARIncludeInwardsWhsConsolidatedInvoice>
            <ARIncludeOutwardsWhsConsolidatedInvoice>true</ARIncludeOutwardsWhsConsolidatedInvoice>
            <ARWhsStorageCalcMethod>MAX</ARWhsStorageCalcMethod>
            <CRIsShipsAgencyPrincipal>false</CRIsShipsAgencyPrincipal>
            <ARWarehouseRatingPeriod>DEF</ARWarehouseRatingPeriod>
            <ARVATConfig>DEF</ARVATConfig>
            <RateSecurityGroup></RateSecurityGroup>
            <EXBillAgentChargesDirect>false</EXBillAgentChargesDirect>
            <IMBillAgentChargesDirect>false</IMBillAgentChargesDirect>
            <IMUsedBondedWhs>false</IMUsedBondedWhs>
            <WhsClientFreeStorageDays>0</WhsClientFreeStorageDays>
            <WhsOverrideFreeStorage>false</WhsOverrideFreeStorage>
            <WhsIncludeReleaseChargesOnShipment>true</WhsIncludeReleaseChargesOnShipment>
            <WhsChargeStorageInAdvance>true</WhsChargeStorageInAdvance>
            <ARCustomerSelfBillsRevenue>false</ARCustomerSelfBillsRevenue>
            <ARVATSplitPaymentApplicable>false</ARVATSplitPaymentApplicable>
            <ARClientNumber></ARClientNumber>
            <APCreateVATComplianceDocumentOnPosting>NON</APCreateVATComplianceDocumentOnPosting>
            <IMProductValueDefaultOptions></IMProductValueDefaultOptions>
            <OrgInvoiceRollupOrGroupCollection>
              <OrgInvoiceRollupOrGroup Action=""MERGE"">
                <PK>852d0fad-5583-4b22-b696-db308bdc1121</PK>
                <JobType>ALL</JobType>
                <TransportMode>ALL</TransportMode>
                <ServiceDirection>ALL</ServiceDirection>
                <GroupOrSubTotal>DEF</GroupOrSubTotal>
                <GroupOrSubtotalStyle>DEF</GroupOrSubtotalStyle>
                <InvoiceLineDisplayOption>DEF</InvoiceLineDisplayOption>
                <InvoicePostingStyle>DEF</InvoicePostingStyle>
                <InvoicePostingCurrency TableName=""RefCurrency"" />
              </OrgInvoiceRollupOrGroup>
            </OrgInvoiceRollupOrGroupCollection>
            <APDefltCurrency TableName=""RefCurrency"">
              <Code>AUD</Code>
              <PK>53feb23f-a7b0-4e0d-af5d-f6f6e5417399</PK>
            </APDefltCurrency>
            <APCreditorGroup TableName=""OrgCreditorGroup"" />
            <APDefaultBankAccount TableName=""AccBankAccount"" />
            <APDefaultChargeCode TableName=""AccChargeCode"" />
            <ARPayToAccount TableName=""AccBankAccount"" />
            <ARDDefltCurrency TableName=""RefCurrency"">
              <Code>AUD</Code>
              <PK>53feb23f-a7b0-4e0d-af5d-f6f6e5417399</PK>
            </ARDDefltCurrency>
            <ARDebtorGroup TableName=""OrgDebtorGroup"" />
            <ControllingBranch TableName=""GlbBranch"">
              <Code>SYD</Code>
              <PK>fdd429d2-648c-4895-8f9f-06e90ded2be5</PK>
            </ControllingBranch>
            <GlbCompany>
              <Code>EDI</Code>
              <PK>878d7aca-ffc3-49fc-9710-969ca0c0f2ac</PK>
            </GlbCompany>
          </OrgCompanyData>
        </OrgCompanyDataCollection>
        <ClosestPort TableName=""RefUNLOCO"">
          <Code>AUSYD</Code>
          <PK>ad87872e-96b8-4c9a-bc0b-6a4088247a64</PK>
        </ClosestPort>
      </OrgHeader>
    </Organization>
  </Body>
</Native>";

		const string OrgHeaderWithContactAndGender = @"
<Native xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"" version=""2.0"">
  <Header>
    <OwnerCode>EDICUS</OwnerCode>
    <EnableCodeMapping>true</EnableCodeMapping>
  </Header>
  <Body>
    <Organization version=""2.0"">
      <OrgHeader Action=""MERGE"">
        <PK>4598518d-c7c0-49be-ac2f-3759127fe758</PK>
        <Code>WTGSYD</Code>
        <IsActive>true</IsActive>
        <FullName>WTG</FullName>
        <IsConsignee>false</IsConsignee>
        <IsConsignor>true</IsConsignor>
        <IsTransportClient>false</IsTransportClient>
        <IsWarehouseClient>false</IsWarehouseClient>
        <IsForwarder>false</IsForwarder>
        <IsShippingProvider>false</IsShippingProvider>
        <IsAirWholesaler>false</IsAirWholesaler>
        <IsSeaWholesaler>false</IsSeaWholesaler>
        <IsRailProvider>false</IsRailProvider>
        <IsLineHaulProvider>false</IsLineHaulProvider>
        <IsMiscFreightServices>false</IsMiscFreightServices>
        <IsAirCTO>false</IsAirCTO>
        <IsAirLine>false</IsAirLine>
        <IsBroker>false</IsBroker>
        <IsContainerYard>false</IsContainerYard>
        <IsLocalTransport>false</IsLocalTransport>
        <IsPackDepot>false</IsPackDepot>
        <IsSeaCTO>false</IsSeaCTO>
        <IsShippingLine>false</IsShippingLine>
        <IsUnpackDepot>false</IsUnpackDepot>
        <IsRailHead>false</IsRailHead>
        <IsRoadFreightDepot>false</IsRoadFreightDepot>
        <IsShippingConsortium>false</IsShippingConsortium>
        <IsFumigationContractor>false</IsFumigationContractor>
        <IsGlobalAccount>false</IsGlobalAccount>
        <IsNationalAccount>false</IsNationalAccount>
        <IsSalesLead>false</IsSalesLead>
        <IsCompetitor>false</IsCompetitor>
        <IsTempAccount>false</IsTempAccount>
        <IsPersonalEffectsAccount>false</IsPersonalEffectsAccount>
        <IsDistributionCentre>false</IsDistributionCentre>
        <IsUserFlag1>false</IsUserFlag1>
        <IsUserFlag2>false</IsUserFlag2>
        <IsUserFlag3>false</IsUserFlag3>
        <IsUserFlag4>false</IsUserFlag4>
        <IsUserFlag5>false</IsUserFlag5>
        <IsUserFlag6>false</IsUserFlag6>
        <IsUserFlag7>false</IsUserFlag7>
        <IsUserFlag8>false</IsUserFlag8>
        <IsUserFlag9>false</IsUserFlag9>
        <IsUserFlag10>false</IsUserFlag10>
        <IsUserFlag11>false</IsUserFlag11>
        <IsUserFlag12>false</IsUserFlag12>
        <IsUserFlag13>false</IsUserFlag13>
        <IsUserFlag14>false</IsUserFlag14>
        <Language>EN</Language>
        <ScreeningStatus>NOT</ScreeningStatus>
        <SystemLastEditTimeUtc>2019-03-18T22:18:00</SystemLastEditTimeUtc>
        <SystemCreateTimeUtc>2019-03-18T22:18:00</SystemCreateTimeUtc>
        <IsUserFlag15>false</IsUserFlag15>
        <IsUserFlag16>false</IsUserFlag16>
        <IsUserFlag17>false</IsUserFlag17>
        <IsUserFlag18>false</IsUserFlag18>
        <IsUserFlag19>false</IsUserFlag19>
        <IsUserFlag20>false</IsUserFlag20>
        <IsUserFlag21>false</IsUserFlag21>
        <IsUserFlag22>false</IsUserFlag22>
        <IsUserFlag23>false</IsUserFlag23>
        <IsUserFlag24>false</IsUserFlag24>
        <IsControllingCustomer>false</IsControllingCustomer>
        <IsControllingAgent>false</IsControllingAgent>
        <Category>BUS</Category>
        <OrgMiscServ Action=""MERGE"">
          <PK>2bb553f1-8943-44f2-afb6-44de725a01bb</PK>
          <Airline3CharCode></Airline3CharCode>
          <IMEftCustomsFromImport>false</IMEftCustomsFromImport>
          <IMEftQuarantineFromImport>false</IMEftQuarantineFromImport>
          <IMEftHoldUntilPayAuthorised>false</IMEftHoldUntilPayAuthorised>
          <IMEstDaysDeliveryAir>0</IMEstDaysDeliveryAir>
          <IMEstDaysDeliveryFCL>0</IMEstDaysDeliveryFCL>
          <IMEstDaysDeliveryLCL>0</IMEstDaysDeliveryLCL>
          <IMMaxEFTAmount>0.0000</IMMaxEFTAmount>
          <IMMinEFTAmount>0.0000</IMMinEFTAmount>
          <IMEFTBankAccount></IMEFTBankAccount>
          <IMEFTBankBSB></IMEFTBankBSB>
          <IMIsGSTDeferred>false</IMIsGSTDeferred>
          <IMMergeCustomsInvoiceLinesBy>DEF</IMMergeCustomsInvoiceLinesBy>
          <IMOrderLineAttrib1></IMOrderLineAttrib1>
          <IMOrderLineAttrib2></IMOrderLineAttrib2>
          <IMOrderLineAttrib3></IMOrderLineAttrib3>
          <IMOriginalSeaBills>3</IMOriginalSeaBills>
          <IMCopySeaBills>3</IMCopySeaBills>
          <IMSendImportDocsTo>IMP</IMSendImportDocsTo>
          <IMSendSeaImportDocsTo>IMP</IMSendSeaImportDocsTo>
          <IMImporterCategory>STD</IMImporterCategory>
          <IMAirDepotFreeDays>1</IMAirDepotFreeDays>
          <IMSeaDepotFreeDays>3</IMSeaDepotFreeDays>
          <IMImporterOwnsPartNumbers>false</IMImporterOwnsPartNumbers>
          <IMOrderStatusCodePairList></IMOrderStatusCodePairList>
          <IMOrderLineStatusCodePairList></IMOrderLineStatusCodePairList>
          <IMLastOrderReference></IMLastOrderReference>
          <IMDefaultToNewOrdersToNextOrderNum>false</IMDefaultToNewOrdersToNextOrderNum>
          <IMFCLEquipmentNeeded></IMFCLEquipmentNeeded>
          <IMLCLEquipmentNeeded></IMLCLEquipmentNeeded>
          <IMAirEquipmentNeeded></IMAirEquipmentNeeded>
          <IMDefaultINCOTerm>FOB</IMDefaultINCOTerm>
          <IMAutoImpJobRefered>false</IMAutoImpJobRefered>
          <IMAutoPopulateOwnerRefWithOrderNums>DEF</IMAutoPopulateOwnerRefWithOrderNums>
          <IMPartAttrib1Type></IMPartAttrib1Type>
          <IMPartAttrib1Name></IMPartAttrib1Name>
          <IMPartAttrib1IsMandatory>false</IMPartAttrib1IsMandatory>
          <IMAttrib1IsKey>false</IMAttrib1IsKey>
          <IMPartAttrib2Type></IMPartAttrib2Type>
          <IMPartAttrib2Name></IMPartAttrib2Name>
          <IMPartAttrib2IsMandatory>false</IMPartAttrib2IsMandatory>
          <IMAttrib2IsKey>false</IMAttrib2IsKey>
          <IMPartAttrib3Type></IMPartAttrib3Type>
          <IMPartAttrib3Name></IMPartAttrib3Name>
          <IMPartAttrib3IsMandatory>false</IMPartAttrib3IsMandatory>
          <IMAttrib3IsKey>false</IMAttrib3IsKey>
          <IMUseExpiryDate>false</IMUseExpiryDate>
          <IMUsePackingDate>false</IMUsePackingDate>
          <IMImporterRequiresOrderNumbersOnDocs>false</IMImporterRequiresOrderNumbersOnDocs>
          <IMJobRequireOrderTrackLink>false</IMJobRequireOrderTrackLink>
          <IMDocumentAddressPreference></IMDocumentAddressPreference>
          <IMDefaultWarehousePickOption>AUT</IMDefaultWarehousePickOption>
          <IMInvoiceDetailReportSort>DEF</IMInvoiceDetailReportSort>
          <IMInvoiceDetailReportSort2>DEF</IMInvoiceDetailReportSort2>
          <IMInvoiceDetailReportSort3>DEF</IMInvoiceDetailReportSort3>
          <IMShowDutyOnWarehouseEntries>false</IMShowDutyOnWarehouseEntries>
          <LandedCostMarginPercent1>0.00</LandedCostMarginPercent1>
          <LandedCostMarginPercent2>0.00</LandedCostMarginPercent2>
          <LandedCostMarginPercent3>0.00</LandedCostMarginPercent3>
          <LastArchiveDate></LastArchiveDate>
          <EXJobRequireOrderTrackLink>false</EXJobRequireOrderTrackLink>
          <EXExporterRequiresOrderNumbersOnDocs>false</EXExporterRequiresOrderNumbersOnDocs>
          <EXGoodsDescription></EXGoodsDescription>
          <EXExporterCategory>STD</EXExporterCategory>
          <EXFCLEquipmentNeeded></EXFCLEquipmentNeeded>
          <EXLCLEquipmentNeeded></EXLCLEquipmentNeeded>
          <EXAirEquipmentNeeded></EXAirEquipmentNeeded>
          <EXAllowedToPrintOriginalBL>false</EXAllowedToPrintOriginalBL>
          <EXDocumentAddressPreference></EXDocumentAddressPreference>
          <EXDefaultDGContactPhoneUsed></EXDefaultDGContactPhoneUsed>
          <EXDefaultInvoicePriceFromProductLastCost></EXDefaultInvoicePriceFromProductLastCost>
          <EXHandlingInstuctions></EXHandlingInstuctions>
          <EXDefaultIncoTerm>FOB</EXDefaultIncoTerm>
          <EXMergeCustomsInvoiceLinesBy>NON</EXMergeCustomsInvoiceLinesBy>
          <EXPreAllocPrefix></EXPreAllocPrefix>
          <FWAgentCategory>STD</FWAgentCategory>
          <FWAgentBelongsToGroup>false</FWAgentBelongsToGroup>
          <FWHandlesAir>false</FWHandlesAir>
          <FWHandlesSea>false</FWHandlesSea>
          <FWHandlesSeaForPortOrCountry></FWHandlesSeaForPortOrCountry>
          <FWHandlesAirForPortOrCountry></FWHandlesAirForPortOrCountry>
          <FWRequestForCreditAllowed>false</FWRequestForCreditAllowed>
          <FWBillCollectFeesOnSingleInvoice>true</FWBillCollectFeesOnSingleInvoice>
          <FWDealDirectlyWithUltimates>false</FWDealDirectlyWithUltimates>
          <FWIATACode></FWIATACode>
          <FWIATAAccountNumber></FWIATAAccountNumber>
          <FWDirectAMSReporter>false</FWDirectAMSReporter>
          <CRCarrierCategory></CRCarrierCategory>
          <SVServicesCategory></SVServicesCategory>
          <CMSalesCategory></CMSalesCategory>
          <CMCompetitorActivity>FRT</CMCompetitorActivity>
          <CMLastCallDate></CMLastCallDate>
          <CMLastUnactionedCallDate></CMLastUnactionedCallDate>
          <CMClientSize></CMClientSize>
          <CMNoOfEmployees>0</CMNoOfEmployees>
          <CMEstimatedDateToClose></CMEstimatedDateToClose>
          <CMGrowthOutlook></CMGrowthOutlook>
          <CMFollowUpDate></CMFollowUpDate>
          <CMDoesExports>false</CMDoesExports>
          <CMDoesImports>false</CMDoesImports>
          <CMUseTradeLaneFigures>true</CMUseTradeLaneFigures>
          <CMTotalClientRevenue>0.0000</CMTotalClientRevenue>
          <CMPercentage>0.000</CMPercentage>
          <CMAcheivableClientRevenue>0.0000</CMAcheivableClientRevenue>
          <CMEstimatedProfit>0.0000</CMEstimatedProfit>
          <CMWarehouseRevenue>0.0000</CMWarehouseRevenue>
          <CMConsultingRevenue>0.0000</CMConsultingRevenue>
          <CMOverallClientRelation>0</CMOverallClientRelation>
          <CMClientsDesireToRemain>0</CMClientsDesireToRemain>
          <CMEaseClientCanBePoached>0</CMEaseClientCanBePoached>
          <CMAmountOfElectronicIntegration>0</CMAmountOfElectronicIntegration>
          <CMOverallEffectOfClientOnAirfreightCosts></CMOverallEffectOfClientOnAirfreightCosts>
          <CMOverallEffectOfClientOnLCLCosts></CMOverallEffectOfClientOnLCLCosts>
          <CMOverallEffectOfClientOnTEUCosts></CMOverallEffectOfClientOnTEUCosts>
          <CMOverallEffectOfClientOnWarehousingCosts></CMOverallEffectOfClientOnWarehousingCosts>
          <CMOverallEffectOfClientOnOtherCosts></CMOverallEffectOfClientOnOtherCosts>
          <CMAmountOfBusinessWon>0</CMAmountOfBusinessWon>
          <CMSalesTerritory></CMSalesTerritory>
          <CMIsHouseAccount>false</CMIsHouseAccount>
          <CMCommission></CMCommission>
          <CMClientCommenced></CMClientCommenced>
          <CMClientPortalHomePage></CMClientPortalHomePage>
          <CICompetitorCategory></CICompetitorCategory>
          <CMDistanceCalculationProvider>DEF</CMDistanceCalculationProvider>
          <CMDistanceCalculationVersion></CMDistanceCalculationVersion>
          <CMDistanceCalculationMethod></CMDistanceCalculationMethod>
          <CMPaidUpCapital>0.0000</CMPaidUpCapital>
          <CMEstablishedDate></CMEstablishedDate>
          <CIEstimatedStaffThisLocation>0</CIEstimatedStaffThisLocation>
          <CITypeOfService></CITypeOfService>
          <CIEstimatedStaffThisCountry>0</CIEstimatedStaffThisCountry>
          <CISellingStyle></CISellingStyle>
          <CITurnover>0.0000</CITurnover>
          <CIProfit>0.0000</CIProfit>
          <CICapitalEmployed>0.0000</CICapitalEmployed>
          <CICompetitiveRanking>0</CICompetitiveRanking>
          <CIStrength></CIStrength>
          <CIWeaknesses></CIWeaknesses>
          <CIOpportunities></CIOpportunities>
          <CIThreats></CIThreats>
          <WhsOverrideSystemPickingRules>false</WhsOverrideSystemPickingRules>
          <WhsPickFromDefaultFIFO>0</WhsPickFromDefaultFIFO>
          <WhsPickFromFullPallets>0</WhsPickFromFullPallets>
          <WhsPickFromConsolidatedFullPallets>0</WhsPickFromConsolidatedFullPallets>
          <WhsPickFromPickFaces>0</WhsPickFromPickFaces>
          <WhsPickFromPalletOverflow>0</WhsPickFromPalletOverflow>
          <WhsPickFromBrokenPallets>0</WhsPickFromBrokenPallets>
          <WhsPickFromFIFOBulkOnly>0</WhsPickFromFIFOBulkOnly>
          <WhsPickSortArrivalDate>0</WhsPickSortArrivalDate>
          <WhsPickSortExpiryDate>0</WhsPickSortExpiryDate>
          <WhsPickSortPackingDate>0</WhsPickSortPackingDate>
          <WhsPickSortPickFace>0</WhsPickSortPickFace>
          <WhsPickSortLocationRow>0</WhsPickSortLocationRow>
          <WhsPickSortLocationColumn>0</WhsPickSortLocationColumn>
          <WhsPickSortLocationLevel>0</WhsPickSortLocationLevel>
          <WhsExpiryNotificationFromDefaults>true</WhsExpiryNotificationFromDefaults>
          <WhsExpiryNotificationPeriod>0</WhsExpiryNotificationPeriod>
          <WhsClientInvoiceFormat></WhsClientInvoiceFormat>
          <WhsOrderNumberUniquenessStrategy></WhsOrderNumberUniquenessStrategy>
          <WhsOverrideSystemPutawayRules>false</WhsOverrideSystemPutawayRules>
          <WhsPutawayToLocation>0</WhsPutawayToLocation>
          <WhsPutawayToPickFace>0</WhsPutawayToPickFace>
          <WhsPutawayToProductArea>0</WhsPutawayToProductArea>
          <WhsPutawayToClientArea>0</WhsPutawayToClientArea>
          <WhsPutawaySortLocationColumn>0</WhsPutawaySortLocationColumn>
          <WhsPutawaySortLocationLevel>0</WhsPutawaySortLocationLevel>
          <WhsPutawaySortLocationRow>0</WhsPutawaySortLocationRow>
          <WhsPutawaySameProductTogether>false</WhsPutawaySameProductTogether>
          <WhsEDIChangeMessageCancelOrderLinesNotIncludedInMessage>true</WhsEDIChangeMessageCancelOrderLinesNotIncludedInMessage>
          <WhsGenerateBackOrdersOnShortfalls>false</WhsGenerateBackOrdersOnShortfalls>
          <WhsAutoPckEdiOrders>false</WhsAutoPckEdiOrders>
          <WhsOrderFulfillmentRule>NON</WhsOrderFulfillmentRule>
          <WhsPackingSlipOrderBy>DEF</WhsPackingSlipOrderBy>
          <WhsDefaultWarehousePickMode>ASP</WhsDefaultWarehousePickMode>
          <WhsDefaultWarehouseRollUp>false</WhsDefaultWarehouseRollUp>
          <WhsTransportPayer>DEF</WhsTransportPayer>
          <MinimumShelfLifeAccepted>0</MinimumShelfLifeAccepted>
          <WhsABCAnalysisEnabled>false</WhsABCAnalysisEnabled>
          <WhsABCAnalysisMethod>DEF</WhsABCAnalysisMethod>
          <WhsABCAnalysisPeriod>DEF</WhsABCAnalysisPeriod>
          <IsScanPackQtyAllowed>false</IsScanPackQtyAllowed>
          <IsLabelPrintedOnClosePackage>true</IsLabelPrintedOnClosePackage>
          <CustomAttrib1></CustomAttrib1>
          <CustomAttrib2></CustomAttrib2>
          <CustomAttrib3></CustomAttrib3>
          <CustomDate1></CustomDate1>
          <CustomDate2></CustomDate2>
          <CustomDate3></CustomDate3>
          <CustomDecimal1>0.000</CustomDecimal1>
          <CustomDecimal2>0.000</CustomDecimal2>
          <CustomDecimal3>0.000</CustomDecimal3>
          <CustomFlag1>false</CustomFlag1>
          <CustomFlag2>false</CustomFlag2>
          <CustomFlag3>false</CustomFlag3>
          <CustomFlag4>false</CustomFlag4>
          <IMOwnsProducts>false</IMOwnsProducts>
          <EXOwnsProducts>false</EXOwnsProducts>
          <CRVoyageRecyclingPeriodInMonths>-1</CRVoyageRecyclingPeriodInMonths>
          <IsAutoPackAllowed>true</IsAutoPackAllowed>
          <WhsOrderDefaultPickPriority>0</WhsOrderDefaultPickPriority>
          <ConsigneeAuthorityToLeave>DEF</ConsigneeAuthorityToLeave>
          <ConsignorAuthorityToLeave>DEF</ConsignorAuthorityToLeave>
          <IMAllowOrders>true</IMAllowOrders>
          <CMPeriodOfActivity></CMPeriodOfActivity>
          <CMIndustryVertical></CMIndustryVertical>
          <WhsIsRecalculateOrderPricing>false</WhsIsRecalculateOrderPricing>
          <CMAuthorityToLeave>DEF</CMAuthorityToLeave>
          <TBAllowMixedAccountNumbersOnManifest>false</TBAllowMixedAccountNumbersOnManifest>
          <IMAllowAttachedOrderXMLUpdate>false</IMAllowAttachedOrderXMLUpdate>
          <EXValidationForUnauditedClassification></EXValidationForUnauditedClassification>
          <IMValidationForUnauditedClassification></IMValidationForUnauditedClassification>
          <IMBalanceInvoicePackage>false</IMBalanceInvoicePackage>
          <WhsPackageToleranceEnabled>false</WhsPackageToleranceEnabled>
          <WhsPackageWeightTolerancePercent>0.0</WhsPackageWeightTolerancePercent>
          <WhsPickFromHighPriorityLocations>0</WhsPickFromHighPriorityLocations>
          <WhsEnforceScanOfProductsWhenPackingTote>false</WhsEnforceScanOfProductsWhenPackingTote>
          <ARGlobalCreditApproved>false</ARGlobalCreditApproved>
          <ARGlobalCreditLimit>0.0000</ARGlobalCreditLimit>
          <ARGlobalOnCreditHold>false</ARGlobalOnCreditHold>
          <IMDefaultServiceLevel TableName=""RefServiceLevel"" />
          <EXDefaultServiceLevel TableName=""RefServiceLevel"" />
          <EXDefCurrency TableName=""RefCurrency"" />
          <EXDefaultCntryOfOrigin TableName=""RefCountry"">
            <Code>AU</Code>
            <PK>e4eb6e97-aa78-4c46-bf86-35b7fbb5fb0e</PK>
          </EXDefaultCntryOfOrigin>
          <EXDefaultDGContact TableName=""OrgContact"" />
          <FWDefCurrency TableName=""RefCurrency"">
            <Code>AUD</Code>
            <PK>53feb23f-a7b0-4e0d-af5d-f6f6e5417399</PK>
          </FWDefCurrency>
          <CMMainImportCmdty TableName=""RefCommodityCode"" />
          <CMMainExportCmdty TableName=""RefCommodityCode"" />
          <WhsDefaultWarehouse TableName=""GlbBranch"" />
          <WhsPackingSlip TableName=""StmTemplate"" />
          <CMPreferredPaymentCompany TableName=""GlbCompany"" />
          <CartonGroup TableName=""WhsCartonGroup"" />
          <OrgSecurityGroup TableName=""GlbGroup"" />
          <ARGlobalCreditGroup TableName=""OrgHeader"" />
          <ARGlobalCreditCurrency TableName=""RefCurrency"" />
        </OrgMiscServ>
        <OrgContactCollection>
          <OrgContact Action=""MERGE"">
            <PK>06da560a-82f4-4ec2-b476-9bc8bf18d5e9</PK>
            <IsActive>true</IsActive>
            <ContactName>Anton</ContactName>
            <Salutation></Salutation>
            <Language>EN</Language>
            <NotifyMode>EML</NotifyMode>
            <Title></Title>
            <Gender>{0}</Gender>
            <JobCategory>EMU</JobCategory>
            <Phone></Phone>
            <PhoneExtension></PhoneExtension>
            <Fax></Fax>
            <Mobile></Mobile>
            <HomePhone></HomePhone>
            <Pager></Pager>
            <OtherPhone></OtherPhone>
            <ProfilePhoto></ProfilePhoto>
            <WebAccessEnabled>false</WebAccessEnabled>
            <WebContractSignedDate></WebContractSignedDate>
            <Birthday></Birthday>
            <YearJoinedIndustry></YearJoinedIndustry>
            <YearJoinedCompany></YearJoinedCompany>
            <ContactSource></ContactSource>
            <DetailsVerified></DetailsVerified>
            <Email>anton.gorlin@wisetechglobal.com</Email>
            <AttachmentType>PDF</AttachmentType>
            <PersonalInfo></PersonalInfo>
            <PasswordHash></PasswordHash>
            <PasswordHashIterations>0</PasswordHashIterations>
            <PasswordSalt></PasswordSalt>
            <Nationality TableName=""RefCountry"">
              <Code></Code>
            </Nationality>
            <OrgAddress />
            <AddressOverride TableName=""OrgHeader"" />
          </OrgContact>
        </OrgContactCollection>
        <OrgAddressCollection>
          <OrgAddress Action=""MERGE"">
            <PK>d85b7891-69b8-47fd-9dda-90980a6ed66b</PK>
            <IsActive>true</IsActive>
            <Code>72 O'RIORDAN ST</Code>
            <CompanyNameOverride></CompanyNameOverride>
            <Address1>72 O'RIORDAN ST</Address1>
            <Address2></Address2>
            <State>NSW</State>
            <PostCode>2015</PostCode>
            <Phone></Phone>
            <Fax></Fax>
            <Mobile></Mobile>
            <PickupFromTimeOnly></PickupFromTimeOnly>
            <PickupToTimeOnly></PickupToTimeOnly>
            <DeliverFromTimeOnly></DeliverFromTimeOnly>
            <DeliverToTimeOnly></DeliverToTimeOnly>
            <DoNotAttendFrom></DoNotAttendFrom>
            <DoNotAttendTo></DoNotAttendTo>
            <DockLeveler>false</DockLeveler>
            <ForkLift>false</ForkLift>
            <PalletJack>false</PalletJack>
            <ContainerHandling></ContainerHandling>
            <AccessPoint></AccessPoint>
            <LabourRequired></LabourRequired>
            <CommunicationRequired></CommunicationRequired>
            <Dock_Height></Dock_Height>
            <FCLEquipmentNeeded>WUP</FCLEquipmentNeeded>
            <LCLEquipmentNeeded>PSL</LCLEquipmentNeeded>
            <AIREquipmentNeeded>PSL</AIREquipmentNeeded>
            <Email></Email>
            <UseCumulativeFreeWaitingTime>false</UseCumulativeFreeWaitingTime>
            <DeliveryRoute></DeliveryRoute>
            <DeliveryRouteSequence>0</DeliveryRouteSequence>
            <AddressMap></AddressMap>
            <AuthorityToLeave>DEF</AuthorityToLeave>
            <OtherWarehouseFacilities></OtherWarehouseFacilities>
            <LoadingUnloadingConstraints></LoadingUnloadingConstraints>
            <City>ALEXANDRIA</City>
            <GroupNumber>0</GroupNumber>
            <AdditionalAddressInformation></AdditionalAddressInformation>
            <VerifiesContainerGrossWeight>false</VerifiesContainerGrossWeight>
            <Language>EN</Language>
            <SystemCreateTimeUtc>2019-03-18T22:18:00</SystemCreateTimeUtc>
            <SystemLastEditTimeUtc>2019-03-18T22:18:00</SystemLastEditTimeUtc>
            <OrgAddressCapabilityCollection>
              <OrgAddressCapability Action=""MERGE"">
                <PK>126fb8ed-9fa1-4988-8792-bf87b953fa7c</PK>
                <AddressType>OFC</AddressType>
                <IsMainAddress>true</IsMainAddress>
              </OrgAddressCapability>
            </OrgAddressCapabilityCollection>
            <RelatedPortCode TableName=""RefUNLOCO"">
              <Code>AUSYD</Code>
              <PK>ad87872e-96b8-4c9a-bc0b-6a4088247a64</PK>
            </RelatedPortCode>
            <CountryCode TableName=""RefCountry"">
              <Code>AU</Code>
              <PK>e4eb6e97-aa78-4c46-bf86-35b7fbb5fb0e</PK>
            </CountryCode>
          </OrgAddress>
        </OrgAddressCollection>
        <OrgCompanyDataCollection>
          <OrgCompanyData Action=""MERGE"">
            <PK>e1db7069-5703-4a5d-8c25-082d4d3e5db0</PK>
            <IsDebtor>false</IsDebtor>
            <IsCreditor>false</IsCreditor>
            <APCategory></APCategory>
            <APExternalCreditorCode></APExternalCreditorCode>
            <APCreditLimit>0.0000</APCreditLimit>
            <APPayInvoiceAfterPostingDefault>false</APPayInvoiceAfterPostingDefault>
            <APPaymentTermDays>0</APPaymentTermDays>
            <APPaymentTerms>COD</APPaymentTerms>
            <APCreditAgreedPaymentMethod></APCreditAgreedPaymentMethod>
            <APWHTApplicable>false</APWHTApplicable>
            <APAirlineAccountNumber></APAirlineAccountNumber>
            <APQualityAssured>false</APQualityAssured>
            <APQualityAssuredCheckedDate></APQualityAssuredCheckedDate>
            <APCostsSelfBilled>false</APCostsSelfBilled>
            <APPrintContractorForm>false</APPrintContractorForm>
            <APVATConfig>DEF</APVATConfig>
            <ARCategory></ARCategory>
            <ARExternalDebtorCode></ARExternalDebtorCode>
            <ARQualityAssured>false</ARQualityAssured>
            <ARQualityAssuredCheckedDate></ARQualityAssuredCheckedDate>
            <ARAutoUpdateRates>true</ARAutoUpdateRates>
            <ARCombinedStatementInvoice>false</ARCombinedStatementInvoice>
            <ARConsolidatedAccountingCategory></ARConsolidatedAccountingCategory>
            <ARForeignCurrStatement>false</ARForeignCurrStatement>
            <ARBuyersConsolInvoicingStyle>DEF</ARBuyersConsolInvoicingStyle>
            <ARCreditLimit>0.0000</ARCreditLimit>
            <ARTemporaryCreditLimitIncrease>0.0000</ARTemporaryCreditLimitIncrease>
            <ARTemporaryCreditLimitIncreaseExpiry></ARTemporaryCreditLimitIncreaseExpiry>
            <ARCreditRating></ARCreditRating>
            <ARCreditAgreedPaymentMethod></ARCreditAgreedPaymentMethod>
            <ARSeaInvoiceTermDays>COD</ARSeaInvoiceTermDays>
            <ARSeaInvoiceTerms>0</ARSeaInvoiceTerms>
            <ARAirInvoiceTermDays>COD</ARAirInvoiceTermDays>
            <ARAirInvoiceTerms>0</ARAirInvoiceTerms>
            <ARInvoiceTerms>COD</ARInvoiceTerms>
            <ARInvoiceTermDays>0</ARInvoiceTermDays>
            <ARTreatDisbursementsAsStandardValue>0.0000</ARTreatDisbursementsAsStandardValue>
            <ARDisbursementInvoiceTerms>COD</ARDisbursementInvoiceTerms>
            <ARDisbursementInvoiceTermDays>0</ARDisbursementInvoiceTermDays>
            <ARDontShowTaxOnDocs>false</ARDontShowTaxOnDocs>
            <ARSellersConsolInvoicingStyle>DEF</ARSellersConsolInvoicingStyle>
            <AROnCreditHold>false</AROnCreditHold>
            <ARCreditApproved>true</ARCreditApproved>
            <ARUseSettlementGroupCreditLimit>false</ARUseSettlementGroupCreditLimit>
            <ARAccountAndCreditReviewDue></ARAccountAndCreditReviewDue>
            <ARAllowMultiCurrencyPayment>false</ARAllowMultiCurrencyPayment>
            <ARPreviousChequeDrawer></ARPreviousChequeDrawer>
            <ARPreviousChequeDrawerBank></ARPreviousChequeDrawerBank>
            <ARPreviousChequeDrawerBankBranch></ARPreviousChequeDrawerBankBranch>
            <ARReceiptInvoiceAfterPostingDefault>false</ARReceiptInvoiceAfterPostingDefault>
            <AREftCustomsPaymentMethod></AREftCustomsPaymentMethod>
            <ARWHTApplicable>false</ARWHTApplicable>
            <ARIncludeInwardsWhsConsolidatedInvoice>true</ARIncludeInwardsWhsConsolidatedInvoice>
            <ARIncludeOutwardsWhsConsolidatedInvoice>true</ARIncludeOutwardsWhsConsolidatedInvoice>
            <ARWhsStorageCalcMethod>MAX</ARWhsStorageCalcMethod>
            <CRIsShipsAgencyPrincipal>false</CRIsShipsAgencyPrincipal>
            <ARWarehouseRatingPeriod>DEF</ARWarehouseRatingPeriod>
            <ARVATConfig>DEF</ARVATConfig>
            <RateSecurityGroup></RateSecurityGroup>
            <EXBillAgentChargesDirect>false</EXBillAgentChargesDirect>
            <IMBillAgentChargesDirect>false</IMBillAgentChargesDirect>
            <IMUsedBondedWhs>false</IMUsedBondedWhs>
            <WhsClientFreeStorageDays>0</WhsClientFreeStorageDays>
            <WhsOverrideFreeStorage>false</WhsOverrideFreeStorage>
            <WhsIncludeReleaseChargesOnShipment>true</WhsIncludeReleaseChargesOnShipment>
            <WhsChargeStorageInAdvance>true</WhsChargeStorageInAdvance>
            <ARCustomerSelfBillsRevenue>false</ARCustomerSelfBillsRevenue>
            <ARVATSplitPaymentApplicable>false</ARVATSplitPaymentApplicable>
            <ARClientNumber></ARClientNumber>
            <APCreateVATComplianceDocumentOnPosting>NON</APCreateVATComplianceDocumentOnPosting>
            <IMProductValueDefaultOptions></IMProductValueDefaultOptions>
            <OrgInvoiceRollupOrGroupCollection>
              <OrgInvoiceRollupOrGroup Action=""MERGE"">
                <PK>852d0fad-5583-4b22-b696-db308bdc1121</PK>
                <JobType>ALL</JobType>
                <TransportMode>ALL</TransportMode>
                <ServiceDirection>ALL</ServiceDirection>
                <GroupOrSubTotal>DEF</GroupOrSubTotal>
                <GroupOrSubtotalStyle>DEF</GroupOrSubtotalStyle>
                <InvoiceLineDisplayOption>DEF</InvoiceLineDisplayOption>
                <InvoicePostingStyle>DEF</InvoicePostingStyle>
                <InvoicePostingCurrency TableName=""RefCurrency"" />
              </OrgInvoiceRollupOrGroup>
            </OrgInvoiceRollupOrGroupCollection>
            <APDefltCurrency TableName=""RefCurrency"">
              <Code>AUD</Code>
              <PK>53feb23f-a7b0-4e0d-af5d-f6f6e5417399</PK>
            </APDefltCurrency>
            <APCreditorGroup TableName=""OrgCreditorGroup"" />
            <APDefaultBankAccount TableName=""AccBankAccount"" />
            <APDefaultChargeCode TableName=""AccChargeCode"" />
            <ARPayToAccount TableName=""AccBankAccount"" />
            <ARDDefltCurrency TableName=""RefCurrency"">
              <Code>AUD</Code>
              <PK>53feb23f-a7b0-4e0d-af5d-f6f6e5417399</PK>
            </ARDDefltCurrency>
            <ARDebtorGroup TableName=""OrgDebtorGroup"" />
            <ControllingBranch TableName=""GlbBranch"">
              <Code>SYD</Code>
              <PK>fdd429d2-648c-4895-8f9f-06e90ded2be5</PK>
            </ControllingBranch>
            <GlbCompany>
              <Code>EDI</Code>
              <PK>878d7aca-ffc3-49fc-9710-969ca0c0f2ac</PK>
            </GlbCompany>
          </OrgCompanyData>
        </OrgCompanyDataCollection>
        <ClosestPort TableName=""RefUNLOCO"">
          <Code>AUSYD</Code>
          <PK>ad87872e-96b8-4c9a-bc0b-6a4088247a64</PK>
        </ClosestPort>
      </OrgHeader>
    </Organization>
  </Body>
</Native>";

		#endregion
	}
}
