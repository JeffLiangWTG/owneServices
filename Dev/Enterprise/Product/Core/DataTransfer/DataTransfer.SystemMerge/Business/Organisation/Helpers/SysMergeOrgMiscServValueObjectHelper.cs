using System;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.SystemMerge.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using Xsd = Enterprise.DataTransfer.SystemMerge.XmlDefinition;

namespace Enterprise.DataTransfer.SystemMerge.DataAdapters
{
	public class SysMergeOrgMiscServValueObjectHelper
	{
		public SysMergeOrgMiscServValueObjectHelper(string errorContext)
		{
			this.ErrorContext = errorContext;
		}

		public readonly string ErrorContext;

		#region Import

		public void ImportFromValueObject(Xsd.SysMergeOrgMiscServ xsdOrgMiscServ, OrgHeaderForDataTransfer organisation, IValueObjectImportContext context)
		{
			OrgMiscServ miscServ = organisation.Factory.New<OrgMiscServ>();
			miscServ.OM_OH = organisation.PK;

			context.SetPropertyInfoValueIfValueNotEmpty(miscServ.OM_IMEftCustomsFromImportInfo, xsdOrgMiscServ.EftCustomsFromImport.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(miscServ.OM_IMEftQuarantineFromImportInfo, xsdOrgMiscServ.EftQuarantineFromImport.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(miscServ.OM_IMEftHoldUntilPayAuthorisedInfo, xsdOrgMiscServ.EftHoldUntilPayAuthorised.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(miscServ.OM_IMEstDaysDeliveryAirInfo, xsdOrgMiscServ.EstDaysDeliveryAir.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(miscServ.OM_IMEstDaysDeliveryFCLInfo, xsdOrgMiscServ.EstDaysDeliveryFCL.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(miscServ.OM_IMEstDaysDeliveryLCLInfo, xsdOrgMiscServ.EstDaysDeliveryLCL.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(miscServ.OM_IMMaxEFTAmountInfo, xsdOrgMiscServ.MaxEFTAmount.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(miscServ.OM_IMMinEFTAmountInfo, xsdOrgMiscServ.MinEFTAmount.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(miscServ.OM_IMEFTBankAccountInfo, xsdOrgMiscServ.EFTBankAccount);
			context.SetPropertyInfoValueIfValueNotEmpty(miscServ.OM_IMEFTBankBSBInfo, xsdOrgMiscServ.EFTBankBSB);
			context.SetPropertyInfoValueIfValueNotEmpty(miscServ.OM_IMIsGSTDeferredInfo, xsdOrgMiscServ.IsGSTDeferred.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(miscServ.OM_IMMergeCustomsInvoiceLinesByInfo, xsdOrgMiscServ.ImportMergeCustomsInvoiceLinesBy);
			context.SetPropertyInfoValueIfValueNotEmpty(miscServ.OM_IMOrderLineAttrib1Info, xsdOrgMiscServ.OrderLineAttrib1);
			context.SetPropertyInfoValueIfValueNotEmpty(miscServ.OM_IMOrderLineAttrib2Info, xsdOrgMiscServ.OrderLineAttrib2);
			context.SetPropertyInfoValueIfValueNotEmpty(miscServ.OM_IMOrderLineAttrib3Info, xsdOrgMiscServ.OrderLineAttrib3);
			context.SetPropertyInfoValueIfValueNotEmpty(miscServ.OM_IMOriginalSeaBillsInfo, xsdOrgMiscServ.OriginalSeaBills.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(miscServ.OM_IMCopySeaBillsInfo, xsdOrgMiscServ.CopySeaBills.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(miscServ.OM_IMSendImportDocsToInfo, xsdOrgMiscServ.SendImportDocsTo);
			context.SetPropertyInfoValueIfValueNotEmpty(miscServ.OM_IMSendSeaImportDocsToInfo, xsdOrgMiscServ.SendSeaImportDocsTo);
			context.SetPropertyInfoValueIfValueNotEmpty(miscServ.OM_IMImporterCategoryInfo, xsdOrgMiscServ.ImporterCategory);
			context.SetPropertyInfoValueIfValueNotEmpty(miscServ.OM_IMAirDepotFreeDaysInfo, xsdOrgMiscServ.AirDepotFreeDays.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(miscServ.OM_IMSeaDepotFreeDaysInfo, xsdOrgMiscServ.SeaDepotFreeDays.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(miscServ.OM_IMImporterOwnsPartNumbersInfo, xsdOrgMiscServ.ImporterOwnsPartNumbers.ToString());
			//  Context.SetPropertyInfoValueIfValueNotEmpty(OrgMiscServ.OM_IMOrderStatusCodePairListInfo, xsdOrgMiscServ.OrderStatusCodePairList);
			//  Context.SetPropertyInfoValueIfValueNotEmpty(OrgMiscServ.OM_IMOrderLineStatusCodePairListInfo, xsdOrgMiscServ.OrderLineStatusCodePairList);
			context.SetPropertyInfoValueIfValueNotEmpty(miscServ.OM_IMLastOrderReferenceInfo, xsdOrgMiscServ.LastOrderReference);
			context.SetPropertyInfoValueIfValueNotEmpty(miscServ.OM_IMDefaultToNewOrdersToNextOrderNumInfo, xsdOrgMiscServ.DefaultToNewOrdersToNextOrderNum.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(miscServ.OM_IMFCLEquipmentNeededInfo, xsdOrgMiscServ.ImportFCLEquipmentNeeded);
			context.SetPropertyInfoValueIfValueNotEmpty(miscServ.OM_IMLCLEquipmentNeededInfo, xsdOrgMiscServ.ImportLCLEquipmentNeeded);
			context.SetPropertyInfoValueIfValueNotEmpty(miscServ.OM_IMAirEquipmentNeededInfo, xsdOrgMiscServ.ImportAirEquipmentNeeded);
			context.SetPropertyInfoValueIfValueNotEmpty(miscServ.OM_RS_NKIMDefaultServiceLevelInfo, xsdOrgMiscServ.ImportDefaultServiceLevel);
			context.SetPropertyInfoValueIfValueNotEmpty(miscServ.OM_IMDefaultINCOTermInfo, xsdOrgMiscServ.ImportDefaultINCOTerm);
			context.SetPropertyInfoValueIfValueNotEmpty(miscServ.OM_IMAutoImpJobReferedInfo, xsdOrgMiscServ.AutoImpJobRefered.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(miscServ.OM_IMAutoPopulateOwnerRefWithOrderNumsInfo, xsdOrgMiscServ.AutoPopulateOwnerRefWithOrderNums);
			context.SetPropertyInfoValueIfValueNotEmpty(miscServ.OM_IMPartAttrib1TypeInfo, xsdOrgMiscServ.PartAttrib1Type);
			context.SetPropertyInfoValueIfValueNotEmpty(miscServ.OM_IMPartAttrib1NameInfo, xsdOrgMiscServ.PartAttrib1Name);
			context.SetPropertyInfoValueIfValueNotEmpty(miscServ.OM_IMAttrib1IsKeyInfo, xsdOrgMiscServ.Attrib1IsKey.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(miscServ.OM_IMPartAttrib2TypeInfo, xsdOrgMiscServ.PartAttrib2Type);
			context.SetPropertyInfoValueIfValueNotEmpty(miscServ.OM_IMPartAttrib2NameInfo, xsdOrgMiscServ.PartAttrib2Name);
			context.SetPropertyInfoValueIfValueNotEmpty(miscServ.OM_IMAttrib2IsKeyInfo, xsdOrgMiscServ.Attrib2IsKey.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(miscServ.OM_IMPartAttrib3TypeInfo, xsdOrgMiscServ.PartAttrib3Type);
			context.SetPropertyInfoValueIfValueNotEmpty(miscServ.OM_IMPartAttrib3NameInfo, xsdOrgMiscServ.PartAttrib3Name);
			context.SetPropertyInfoValueIfValueNotEmpty(miscServ.OM_IMAttrib3IsKeyInfo, xsdOrgMiscServ.Attrib3IsKey.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(miscServ.OM_IMSerialNumberIsKeyInfo, xsdOrgMiscServ.SerialNumberIsKey.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(miscServ.OM_IMUseExpiryDateInfo, xsdOrgMiscServ.UseExpiryDate.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(miscServ.OM_IMUsePackingDateInfo, xsdOrgMiscServ.UsePackingDate.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(miscServ.OM_IMImporterRequiresOrderNumbersOnDocsInfo, xsdOrgMiscServ.ImporterRequiresOrderNumbersOnDocs.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(miscServ.OM_IMJobRequireOrderTrackLinkInfo, xsdOrgMiscServ.ImportJobRequireOrderTrackLink.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(miscServ.OM_IMDocumentAddressPreferenceInfo, xsdOrgMiscServ.ImportDocumentAddressPreference);
			context.SetPropertyInfoValueIfValueNotEmpty(miscServ.OM_IMDefaultWarehousePickOptionInfo, xsdOrgMiscServ.DefaultWarehousePickOption);
			context.SetPropertyInfoValueIfValueNotEmpty(miscServ.OM_IMInvoiceDetailReportSortInfo, xsdOrgMiscServ.InvoiceDetailReportSort);
			context.SetPropertyInfoValueIfValueNotEmpty(miscServ.OM_IMInvoiceDetailReportSort2Info, xsdOrgMiscServ.InvoiceDetailReportSort2);
			context.SetPropertyInfoValueIfValueNotEmpty(miscServ.OM_IMInvoiceDetailReportSort3Info, xsdOrgMiscServ.InvoiceDetailReportSort3);
			context.SetPropertyInfoValueIfValueNotEmpty(miscServ.OM_LandedCostMarginPercent1Info, xsdOrgMiscServ.LandedCostMarginPercent1.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(miscServ.OM_LandedCostMarginPercent2Info, xsdOrgMiscServ.LandedCostMarginPercent2.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(miscServ.OM_LandedCostMarginPercent3Info, xsdOrgMiscServ.LandedCostMarginPercent3.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(miscServ.OM_EXJobRequireOrderTrackLinkInfo, xsdOrgMiscServ.ExportJobRequireOrderTrackLink.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(miscServ.OM_EXExporterRequiresOrderNumbersOnDocsInfo, xsdOrgMiscServ.ExporterRequiresOrderNumbersOnDocs.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(miscServ.OM_RS_NKEXDefaultServiceLevelInfo, xsdOrgMiscServ.ExportDefaultServiceLevel);
			context.SetPropertyInfoValueIfValueNotEmpty(miscServ.OM_EXGoodsDescriptionInfo, xsdOrgMiscServ.GoodsDescription);
			context.SetPropertyInfoValueIfValueNotEmpty(miscServ.OM_EXExporterCategoryInfo, xsdOrgMiscServ.ExporterCategory);
			context.SetPropertyInfoValueIfValueNotEmpty(miscServ.OM_EXFCLEquipmentNeededInfo, xsdOrgMiscServ.ExportFCLEquipmentNeeded);
			context.SetPropertyInfoValueIfValueNotEmpty(miscServ.OM_EXLCLEquipmentNeededInfo, xsdOrgMiscServ.ExportLCLEquipmentNeeded);
			context.SetPropertyInfoValueIfValueNotEmpty(miscServ.OM_EXAirEquipmentNeededInfo, xsdOrgMiscServ.ExportAirEquipmentNeeded);
			context.SetPropertyInfoValueIfValueNotEmpty(miscServ.OM_EXAllowedToPrintOriginalBLInfo, xsdOrgMiscServ.AllowedToPrintOriginalBL.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(miscServ.OM_EXDocumentAddressPreferenceInfo, xsdOrgMiscServ.ExportDocumentAddressPreference);
			context.SetPropertyInfoValueIfValueNotEmpty(miscServ.OM_EXDefaultDGContactPhoneUsedInfo, xsdOrgMiscServ.DefaultDGContactPhoneUsed);
			context.SetPropertyInfoValueIfValueNotEmpty(miscServ.OM_EXDefaultInvoicePriceFromProductLastCostInfo, xsdOrgMiscServ.DefaultInvoicePriceFromProductLastCost);
			context.SetPropertyInfoValueIfValueNotEmpty(miscServ.OM_EXHandlingInstuctionsInfo, xsdOrgMiscServ.HandlingInstructions);
			context.SetPropertyInfoValueIfValueNotEmpty(miscServ.OM_EXDefaultIncoTermInfo, xsdOrgMiscServ.ExportDefaultIncoTerm);
			context.SetPropertyInfoValueIfValueNotEmpty(miscServ.OM_EXMergeCustomsInvoiceLinesByInfo, xsdOrgMiscServ.ExportMergeCustomsInvoiceLinesBy);
			context.SetPropertyInfoValueIfValueNotEmpty(miscServ.OM_EXPreAllocPrefixInfo, xsdOrgMiscServ.PreAllocPrefix);
			context.SetPropertyInfoValueIfValueNotEmpty(miscServ.OM_FWAgentCategoryInfo, xsdOrgMiscServ.ForwarderAgentCategory);
			context.SetPropertyInfoValueIfValueNotEmpty(miscServ.OM_FWAgentBelongsToGroupInfo, xsdOrgMiscServ.ForwarderAgentBelongsToGroup.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(miscServ.OM_FWHandlesAirInfo, xsdOrgMiscServ.ForwarderHandlesAir.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(miscServ.OM_FWHandlesSeaInfo, xsdOrgMiscServ.ForwarderHandlesSea.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(miscServ.OM_FWHandlesSeaForPortOrCountryInfo, xsdOrgMiscServ.ForwarderHandlesSeaForPortOrCountry);
			context.SetPropertyInfoValueIfValueNotEmpty(miscServ.OM_FWHandlesAirForPortOrCountryInfo, xsdOrgMiscServ.ForwarderHandlesAirForPortOrCountry);
			context.SetPropertyInfoValueIfValueNotEmpty(miscServ.OM_FWRequestForCreditAllowedInfo, xsdOrgMiscServ.ForwarderRequestForCreditAllowed.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(miscServ.OM_FWBillCollectFeesOnSingleInvoiceInfo, xsdOrgMiscServ.ForwarderBillCollectFeesOnSingleInvoice.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(miscServ.OM_FWDealDirectlyWithUltimatesInfo, xsdOrgMiscServ.ForwarderDealDirectlyWithUltimates.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(miscServ.OM_FWIATACodeInfo, xsdOrgMiscServ.ForwarderIATACode);
			context.SetPropertyInfoValueIfValueNotEmpty(miscServ.OM_FWIATAAccountNumberInfo, xsdOrgMiscServ.ForwarderIATAAccountNumber);
			context.SetPropertyInfoValueIfValueNotEmpty(miscServ.OM_FWDirectAMSReporterInfo, xsdOrgMiscServ.ForwarderDirectAMSReporter.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(miscServ.OM_CRCarrierCategoryInfo, xsdOrgMiscServ.CarrierCategory);
			context.SetPropertyInfoValueIfValueNotEmpty(miscServ.OM_SVServicesCategoryInfo, xsdOrgMiscServ.ServicesCategory);
			context.SetPropertyInfoValueIfValueNotEmpty(miscServ.OM_CMSalesCategoryInfo, xsdOrgMiscServ.SalesCategory);
			context.SetPropertyInfoValueIfValueNotEmpty(miscServ.OM_CMCompetitorActivityInfo, xsdOrgMiscServ.CompetitorActivity);
			context.SetPropertyInfoValueIfValueNotEmpty(miscServ.OM_CMClientSizeInfo, xsdOrgMiscServ.ClientSize);
			context.SetPropertyInfoValueIfValueNotEmpty(miscServ.OM_CMNoOfEmployeesInfo, xsdOrgMiscServ.NoOfEmployees.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(miscServ.OM_CMGrowthOutlookInfo, xsdOrgMiscServ.GrowthOutlook);
			context.SetPropertyInfoValueIfValueNotEmpty(miscServ.OM_CMDoesExportsInfo, xsdOrgMiscServ.DoesExports.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(miscServ.OM_CMDoesImportsInfo, xsdOrgMiscServ.DoesImports.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(miscServ.OM_RH_NKCMMainImportCmdtyInfo, xsdOrgMiscServ.MainImportCmdty);
			context.SetPropertyInfoValueIfValueNotEmpty(miscServ.OM_RH_NKCMMainExportCmdtyInfo, xsdOrgMiscServ.MainExportCmdty);
			context.SetPropertyInfoValueIfValueNotEmpty(miscServ.OM_CMUseTradeLaneFiguresInfo, xsdOrgMiscServ.UseTradeLaneFigures.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(miscServ.OM_CMTotalClientRevenueInfo, xsdOrgMiscServ.TotalClientRevenue.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(miscServ.OM_CMPercentageInfo, xsdOrgMiscServ.Percentage.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(miscServ.OM_CMAcheivableClientRevenueInfo, xsdOrgMiscServ.AcheivableClientRevenue.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(miscServ.OM_CMEstimatedProfitInfo, xsdOrgMiscServ.EstimatedProfit.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(miscServ.OM_CMWarehouseRevenueInfo, xsdOrgMiscServ.WarehouseRevenue.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(miscServ.OM_CMConsultingRevenueInfo, xsdOrgMiscServ.ConsultingRevenue.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(miscServ.OM_CMOverallClientRelationInfo, xsdOrgMiscServ.OverallClientRelation.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(miscServ.OM_CMClientsDesireToRemainInfo, xsdOrgMiscServ.ClientsDesireToRemain.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(miscServ.OM_CMEaseClientCanBePoachedInfo, xsdOrgMiscServ.EaseClientCanBePoached.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(miscServ.OM_CMAmountOfElectronicIntegrationInfo, xsdOrgMiscServ.AmountOfElectronicIntegration);
			context.SetPropertyInfoValueIfValueNotEmpty(miscServ.OM_CMOverallEffectOfClientOnAirfreightCostsInfo, xsdOrgMiscServ.OverallEffectOfClientOnAirfreightCosts);
			context.SetPropertyInfoValueIfValueNotEmpty(miscServ.OM_CMOverallEffectOfClientOnLCLCostsInfo, xsdOrgMiscServ.OverallEffectOfClientOnLCLCosts);
			context.SetPropertyInfoValueIfValueNotEmpty(miscServ.OM_CMOverallEffectOfClientOnTEUCostsInfo, xsdOrgMiscServ.OverallEffectOfClientOnTEUCosts);
			context.SetPropertyInfoValueIfValueNotEmpty(miscServ.OM_CMOverallEffectOfClientOnWarehousingCostsInfo, xsdOrgMiscServ.OverallEffectOfClientOnWarehousingCosts);
			context.SetPropertyInfoValueIfValueNotEmpty(miscServ.OM_CMOverallEffectOfClientOnOtherCostsInfo, xsdOrgMiscServ.OverallEffectOfClientOnOtherCosts);
			context.SetPropertyInfoValueIfValueNotEmpty(miscServ.OM_CMAmountOfBusinessWonInfo, xsdOrgMiscServ.AmountOfBusinessWon.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(miscServ.OM_CMSalesTerritoryInfo, xsdOrgMiscServ.SalesTerritory);
			context.SetPropertyInfoValueIfValueNotEmpty(miscServ.OM_CMClientPortalHomePageInfo, xsdOrgMiscServ.ClientPortalHomePage);
			context.SetPropertyInfoValueIfValueNotEmpty(miscServ.OM_CICompetitorCategoryInfo, xsdOrgMiscServ.CompetitorCategory);
			context.SetPropertyInfoValueIfValueNotEmpty(miscServ.OM_CIEstimatedStaffThisLocationInfo, xsdOrgMiscServ.EstimatedStaffThisLocation.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(miscServ.OM_CITypeOfServiceInfo, xsdOrgMiscServ.TypeOfService);
			context.SetPropertyInfoValueIfValueNotEmpty(miscServ.OM_CIEstimatedStaffThisCountryInfo, xsdOrgMiscServ.EstimatedStaffThisCountry.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(miscServ.OM_CISellingStyleInfo, xsdOrgMiscServ.SellingStyle);
			context.SetPropertyInfoValueIfValueNotEmpty(miscServ.OM_CITurnoverInfo, xsdOrgMiscServ.Turnover.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(miscServ.OM_CIProfitInfo, xsdOrgMiscServ.Profit.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(miscServ.OM_CICapitalEmployedInfo, xsdOrgMiscServ.CapitalEmployed.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(miscServ.OM_CICompetitiveRankingInfo, xsdOrgMiscServ.CompetitiveRanking.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(miscServ.OM_CIStrengthInfo, xsdOrgMiscServ.Strength);
			context.SetPropertyInfoValueIfValueNotEmpty(miscServ.OM_CIWeaknessesInfo, xsdOrgMiscServ.Weaknesses);
			context.SetPropertyInfoValueIfValueNotEmpty(miscServ.OM_CIOpportunitiesInfo, xsdOrgMiscServ.Opportunities);
			context.SetPropertyInfoValueIfValueNotEmpty(miscServ.OM_CIThreatsInfo, xsdOrgMiscServ.Threats);
			context.SetPropertyInfoValueIfValueNotEmpty(miscServ.OM_WhsExpiryNotificationFromDefaultsInfo, xsdOrgMiscServ.WhsExpiryNotificationFromDefaults.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(miscServ.OM_WhsExpiryNotificationPeriodInfo, xsdOrgMiscServ.WhsExpiryNotificationPeriod.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(miscServ.OM_WhsDefaultExpiryNotificationPeriodInDaysInfo, xsdOrgMiscServ.WhsDefaultExpiryNotificationPeriodInDays.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(miscServ.OM_WhsClientInvoiceFormatInfo, xsdOrgMiscServ.WhsClientInvoiceFormat);
			context.SetPropertyInfoValueIfValueNotEmpty(miscServ.OM_WhsGenerateBackOrdersOnShortfallsInfo, xsdOrgMiscServ.WhsGenerateBackOrdersOnShortfalls.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(miscServ.OM_WhsOrderNumberUniquenessStrategyInfo, xsdOrgMiscServ.OrderNumberUniquenessStrategy);
			context.SetPropertyInfoValueIfValueNotEmpty(miscServ.OM_WhsOrderFulfillmentRuleInfo, xsdOrgMiscServ.WhsOrderFulfillmentRule);
			context.SetPropertyInfoValueIfValueNotEmpty(miscServ.OM_WhsDefaultWarehousePickModeInfo, xsdOrgMiscServ.WhsDefaultWarehousePickMode);
			context.SetPropertyInfoValueIfValueNotEmpty(miscServ.OM_WhsPackingSlipOrderByInfo, xsdOrgMiscServ.WhsPackingSlipOrderBy);

			if (!xsdOrgMiscServ.Airline3CharCode.IsEmpty)
			{
				var airline = RefAirline.LoadFromAirlinePrefix(miscServ.Factory, xsdOrgMiscServ.Airline3CharCode);
				if (airline != null)
				{
					miscServ.OM_RM_Airline = airline.PK;
				}
			}

			if (xsdOrgMiscServ.CustomAttrib1.Length <= 20)
			{
				context.SetPropertyInfoValueIfValueNotEmpty(miscServ.OM_CustomAttrib1Info, xsdOrgMiscServ.CustomAttrib1);
			}
			if (xsdOrgMiscServ.CustomAttrib2.Length <= 20)
			{
				context.SetPropertyInfoValueIfValueNotEmpty(miscServ.OM_CustomAttrib2Info, xsdOrgMiscServ.CustomAttrib2);
			}
			if (xsdOrgMiscServ.CustomAttrib3.Length <= 20)
			{
				context.SetPropertyInfoValueIfValueNotEmpty(miscServ.OM_CustomAttrib3Info, xsdOrgMiscServ.CustomAttrib3);
			}
			context.SetPropertyInfoValueIfValueNotEmpty(miscServ.OM_CustomDecimal1Info, xsdOrgMiscServ.CustomDecimal1.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(miscServ.OM_CustomDecimal2Info, xsdOrgMiscServ.CustomDecimal2.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(miscServ.OM_CustomDecimal3Info, xsdOrgMiscServ.CustomDecimal3.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(miscServ.OM_CustomFlag1Info, xsdOrgMiscServ.CustomFlag1.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(miscServ.OM_CustomFlag2Info, xsdOrgMiscServ.CustomFlag2.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(miscServ.OM_CustomFlag3Info, xsdOrgMiscServ.CustomFlag3.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(miscServ.OM_CustomFlag4Info, xsdOrgMiscServ.CustomFlag4.ToString());

			if (xsdOrgMiscServ.ClientCommenced != null)
			{
				context.SetPropertyInfoValueIfValueNotEmpty(miscServ.OM_CMClientCommencedInfo, (ZDateTime)xsdOrgMiscServ.ClientCommenced);
			}
			if (xsdOrgMiscServ.LastArchiveDate != null)
			{
				context.SetPropertyInfoValueIfValueNotEmpty(miscServ.OM_LastArchiveDateInfo, (ZDateTime)xsdOrgMiscServ.LastArchiveDate);
			}
			if (xsdOrgMiscServ.LastCallDate != null)
			{
				context.SetPropertyInfoValueIfValueNotEmpty(miscServ.OM_CMLastCallDateInfo, (ZDateTime)xsdOrgMiscServ.LastCallDate);
			}
			if (xsdOrgMiscServ.EstimatedDateToClose != null)
			{
				context.SetPropertyInfoValueIfValueNotEmpty(miscServ.OM_CMEstimatedDateToCloseInfo, (ZDateTime)xsdOrgMiscServ.EstimatedDateToClose);
			}
			if (xsdOrgMiscServ.CustomDate1 != null)
			{
				context.SetPropertyInfoValueIfValueNotEmpty(miscServ.OM_CustomDate1Info, (ZDateTime)xsdOrgMiscServ.CustomDate1);
			}
			if (xsdOrgMiscServ.CustomDate2 != null)
			{
				context.SetPropertyInfoValueIfValueNotEmpty(miscServ.OM_CustomDate2Info, (ZDateTime)xsdOrgMiscServ.CustomDate2);
			}
			if (xsdOrgMiscServ.CustomDate3 != null)
			{
				context.SetPropertyInfoValueIfValueNotEmpty(miscServ.OM_CustomDate3Info, (ZDateTime)xsdOrgMiscServ.CustomDate3);
			}

			if (!xsdOrgMiscServ.OC_EXDefaultDGContactPK.IsEmpty)
			{
				OrgContact eXDefaultDGContact = miscServ.Factory.Load<OrgContact>(new Guid(xsdOrgMiscServ.OC_EXDefaultDGContactPK));
				if (eXDefaultDGContact != null)
				{
					miscServ.OM_OC_EXDefaultDGContact = eXDefaultDGContact.PK;
				}
			}

			if (!xsdOrgMiscServ.RN_EXDefaultCountryOfOriginPK.IsEmpty)
			{
				RefCountry defaultCountryOfOrigin = miscServ.Factory.Load<RefCountry>(new Guid(xsdOrgMiscServ.RN_EXDefaultCountryOfOriginPK));
				if (defaultCountryOfOrigin != null)
				{
					miscServ.OM_RN_NKEXDefaultCntryOfOrigin = defaultCountryOfOrigin.RN_Code;
				}
			}
			if (!xsdOrgMiscServ.RX_EXDefaultCurrencyPK.IsEmpty)
			{
				RefCurrency eXDefaultCurrency = miscServ.Factory.Load<RefCurrency>(new Guid(xsdOrgMiscServ.RX_EXDefaultCurrencyPK));
				if (eXDefaultCurrency != null)
				{
					miscServ.OM_RX_NKEXDefCurrency = eXDefaultCurrency.RX_Code;
				}
			}
			if (!xsdOrgMiscServ.RX_FWDefaultCurrencyPK.IsEmpty)
			{
				RefCurrency fWDefaultCurrency = miscServ.Factory.Load<RefCurrency>(new Guid(xsdOrgMiscServ.RX_FWDefaultCurrencyPK));
				if (fWDefaultCurrency != null)
				{
					miscServ.OM_RX_NKFWDefCurrency = fWDefaultCurrency.RX_Code;
				}
			}
			if (!xsdOrgMiscServ.SO_WhsPackingSlipPK.IsEmpty)
			{
				miscServ.OM_SO_WhsPackingSlip = new ZGuid(xsdOrgMiscServ.SO_WhsPackingSlipPK);
			}
			if (!xsdOrgMiscServ.CM_AuthorityToLeave.IsEmpty)
			{
				miscServ.OM_CMAuthorityToLeave = xsdOrgMiscServ.CM_AuthorityToLeave;
			}

			context.SetPropertyInfoValueIfValueNotEmpty(miscServ.OM_EXOwnsProductsInfo, xsdOrgMiscServ.OM_EXOwnsProducts.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(miscServ.OM_ConsignorAuthorityToLeaveInfo, xsdOrgMiscServ.OM_ConsignorAuthorityToLeave);
			context.SetPropertyInfoValueIfValueNotEmpty(miscServ.OM_EXJobRequireOrderTrackLinkInfo, xsdOrgMiscServ.OM_EXJobRequireOrderTrackLink);
			context.SetPropertyInfoValueIfValueNotEmpty(miscServ.OM_EXValidationForUnauditedClassificationInfo, xsdOrgMiscServ.OM_EXValidationForUnauditedClassification);
			context.SetPropertyInfoValueIfValueNotEmpty(miscServ.OM_IMShowDutyOnWarehouseEntriesInfo, xsdOrgMiscServ.IMShowDutyOnWarehouseEntries.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(miscServ.OM_IMDisallowOrdersInfo, xsdOrgMiscServ.IMDisallowOrders.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(miscServ.OM_IMAllowAttachedOrderXMLUpdateInfo, xsdOrgMiscServ.IMAllowAttachedOrderXMLUpdate.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(miscServ.OM_IMOwnsProductsInfo, xsdOrgMiscServ.IMOwnsProducts.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(miscServ.OM_IMBalanceInvoicePackageInfo, xsdOrgMiscServ.IMBalanceInvoicePackage.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(miscServ.OM_ConsigneeAuthorityToLeaveInfo, xsdOrgMiscServ.ConsigneeAuthorityToLeave);
			context.SetPropertyInfoValueIfValueNotEmpty(miscServ.OM_IMValidationForUnauditedClassificationInfo, xsdOrgMiscServ.IMValidationForUnauditedClassification);
		}

		#endregion

		#region Export

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise.Globalization", "EDI007", Justification = "no need to be translated")]
		public void ExportToValueObject(OrgHeaderForDataTransfer organisation, Xsd.SysMergeOrgMiscServ xsdMiscServ, INotifications notifications)
		{
			OrgMiscServ miscServ = organisation.Factory.LoadFromUniqueKey<OrgMiscServ>(OrgMiscServSchema.OM_OH, organisation.PK);

			if (miscServ != null)
			{
				if (miscServ.Airline != null)
				{
					xsdMiscServ.Airline3CharCode = miscServ.Airline.RM_EagleAddedAirlinePrefixOrAccountingCode;
				}
				if (miscServ.OM_IMEftCustomsFromImport)
				{
					xsdMiscServ.EftCustomsFromImport = miscServ.OM_IMEftCustomsFromImport;
					xsdMiscServ.EftCustomsFromImportSpecified = true;
				}
				if (miscServ.OM_IMEftQuarantineFromImport)
				{
					xsdMiscServ.EftQuarantineFromImport = miscServ.OM_IMEftQuarantineFromImport;
					xsdMiscServ.EftQuarantineFromImportSpecified = true;
				}
				if (miscServ.OM_IMEftHoldUntilPayAuthorised)
				{
					xsdMiscServ.EftHoldUntilPayAuthorised = miscServ.OM_IMEftHoldUntilPayAuthorised;
					xsdMiscServ.EftHoldUntilPayAuthorisedSpecified = true;
				}
				if (!miscServ.OM_IMEstDaysDeliveryAir.IsEmpty)
				{
					xsdMiscServ.EstDaysDeliveryAir = miscServ.OM_IMEstDaysDeliveryAir;
					xsdMiscServ.EstDaysDeliveryAirSpecified = true;
				}
				if (!miscServ.OM_IMEstDaysDeliveryFCL.IsEmpty)
				{
					xsdMiscServ.EstDaysDeliveryFCL = miscServ.OM_IMEstDaysDeliveryFCL;
					xsdMiscServ.EstDaysDeliveryFCLSpecified = true;
				}
				if (!miscServ.OM_IMEstDaysDeliveryLCL.IsEmpty)
				{
					xsdMiscServ.EstDaysDeliveryLCL = miscServ.OM_IMEstDaysDeliveryLCL;
					xsdMiscServ.EstDaysDeliveryLCLSpecified = true;
				}
				if (!miscServ.OM_IMMaxEFTAmount.IsEmpty)
				{
					xsdMiscServ.MaxEFTAmount = miscServ.OM_IMMaxEFTAmount;
					xsdMiscServ.MaxEFTAmountSpecified = true;
				}
				if (!miscServ.OM_IMMinEFTAmount.IsEmpty)
				{
					xsdMiscServ.MinEFTAmount = miscServ.OM_IMMinEFTAmount;
					xsdMiscServ.MinEFTAmountSpecified = true;
				}
				if (!miscServ.OM_IMEFTBankAccount.IsEmpty)
				{
					xsdMiscServ.EFTBankAccount = miscServ.OM_IMEFTBankAccount;
					xsdMiscServ.EFTBankAccountSpecified = true;
				}
				if (!miscServ.OM_IMEFTBankBSB.IsEmpty)
				{
					xsdMiscServ.EFTBankBSB = miscServ.OM_IMEFTBankBSB;
					xsdMiscServ.EFTBankBSBSpecified = true;
				}
				if (miscServ.OM_IMIsGSTDeferred)
				{
					xsdMiscServ.IsGSTDeferred = miscServ.OM_IMIsGSTDeferred;
					xsdMiscServ.IsGSTDeferredSpecified = true;
				}
				if (!miscServ.OM_IMMergeCustomsInvoiceLinesBy.IsEmpty)
				{
					xsdMiscServ.ImportMergeCustomsInvoiceLinesBy = miscServ.OM_IMMergeCustomsInvoiceLinesBy;
				}

				if (!miscServ.OM_IMOrderLineAttrib1.IsEmpty)
				{
					xsdMiscServ.OrderLineAttrib1 = miscServ.OM_IMOrderLineAttrib1;
				}

				if (!miscServ.OM_IMOrderLineAttrib2.IsEmpty)
				{
					xsdMiscServ.OrderLineAttrib2 = miscServ.OM_IMOrderLineAttrib2;
				}

				if (!miscServ.OM_IMOrderLineAttrib3.IsEmpty)
				{
					xsdMiscServ.OrderLineAttrib3 = miscServ.OM_IMOrderLineAttrib3;
				}

				if (!miscServ.OM_IMOriginalSeaBills.IsEmpty)
				{
					xsdMiscServ.OriginalSeaBills = miscServ.OM_IMOriginalSeaBills;
					xsdMiscServ.OriginalSeaBillsSpecified = true;
				}
				if (!miscServ.OM_IMCopySeaBills.IsEmpty)
				{
					xsdMiscServ.CopySeaBills = miscServ.OM_IMCopySeaBills;
					xsdMiscServ.CopySeaBillsSpecified = true;
				}
				if (!miscServ.OM_IMSendImportDocsTo.IsEmpty)
				{
					xsdMiscServ.SendImportDocsTo = miscServ.OM_IMSendImportDocsTo;
				}

				if (!miscServ.OM_IMSendSeaImportDocsTo.IsEmpty)
				{
					xsdMiscServ.SendSeaImportDocsTo = miscServ.OM_IMSendSeaImportDocsTo;
				}

				if (!miscServ.OM_IMImporterCategory.IsEmpty)
				{
					xsdMiscServ.ImporterCategory = miscServ.OM_IMImporterCategory;
				}

				if (!miscServ.OM_IMAirDepotFreeDays.IsEmpty)
				{
					xsdMiscServ.AirDepotFreeDays = miscServ.OM_IMAirDepotFreeDays;
					xsdMiscServ.AirDepotFreeDaysSpecified = true;
				}
				if (!miscServ.OM_IMSeaDepotFreeDays.IsEmpty)
				{
					xsdMiscServ.SeaDepotFreeDays = miscServ.OM_IMSeaDepotFreeDays;
					xsdMiscServ.SeaDepotFreeDaysSpecified = true;
				}
				if (miscServ.OM_IMImporterOwnsPartNumbers)
				{
					xsdMiscServ.ImporterOwnsPartNumbers = miscServ.OM_IMImporterOwnsPartNumbers;
					xsdMiscServ.ImporterOwnsPartNumbersSpecified = true;
				}
				if (!miscServ.OM_IMLastOrderReference.IsEmpty)
				{
					xsdMiscServ.LastOrderReference = miscServ.OM_IMLastOrderReference;
				}

				if (miscServ.OM_IMDefaultToNewOrdersToNextOrderNum)
				{
					xsdMiscServ.DefaultToNewOrdersToNextOrderNum = miscServ.OM_IMDefaultToNewOrdersToNextOrderNum;
					xsdMiscServ.DefaultToNewOrdersToNextOrderNumSpecified = true;
				}
				if (!miscServ.OM_IMFCLEquipmentNeeded.IsEmpty)
				{
					xsdMiscServ.ImportFCLEquipmentNeeded = miscServ.OM_IMFCLEquipmentNeeded;
				}

				if (!miscServ.OM_IMLCLEquipmentNeeded.IsEmpty)
				{
					xsdMiscServ.ImportLCLEquipmentNeeded = miscServ.OM_IMLCLEquipmentNeeded;
				}

				if (!miscServ.OM_IMAirEquipmentNeeded.IsEmpty)
				{
					xsdMiscServ.ImportAirEquipmentNeeded = miscServ.OM_IMAirEquipmentNeeded;
				}

				if (!miscServ.OM_RS_NKIMDefaultServiceLevel.IsEmpty)
				{
					xsdMiscServ.ImportDefaultServiceLevel = miscServ.OM_RS_NKIMDefaultServiceLevel;
				}

				if (!miscServ.OM_IMDefaultINCOTerm.IsEmpty)
				{
					xsdMiscServ.ImportDefaultINCOTerm = miscServ.OM_IMDefaultINCOTerm;
				}

				if (miscServ.OM_IMAutoImpJobRefered)
				{
					xsdMiscServ.AutoImpJobRefered = miscServ.OM_IMAutoImpJobRefered;
					xsdMiscServ.AutoImpJobReferedSpecified = true;
				}
				if (!miscServ.OM_IMAutoPopulateOwnerRefWithOrderNums.IsEmpty)
				{
					xsdMiscServ.AutoPopulateOwnerRefWithOrderNums = miscServ.OM_IMAutoPopulateOwnerRefWithOrderNums;
				}

				if (!miscServ.OM_IMPartAttrib1Type.IsEmpty)
				{
					xsdMiscServ.PartAttrib1Type = miscServ.OM_IMPartAttrib1Type;
				}

				if (!miscServ.OM_IMPartAttrib1Name.IsEmpty)
				{
					xsdMiscServ.PartAttrib1Name = miscServ.OM_IMPartAttrib1Name;
				}

				if (miscServ.OM_IMAttrib1IsKey)
				{
					xsdMiscServ.Attrib1IsKey = miscServ.OM_IMAttrib1IsKey;
					xsdMiscServ.Attrib1IsKeySpecified = true;
				}
				if (!miscServ.OM_IMPartAttrib2Type.IsEmpty)
				{
					xsdMiscServ.PartAttrib2Type = miscServ.OM_IMPartAttrib2Type;
				}

				if (!miscServ.OM_IMPartAttrib2Name.IsEmpty)
				{
					xsdMiscServ.PartAttrib2Name = miscServ.OM_IMPartAttrib2Name;
				}

				if (miscServ.OM_IMAttrib2IsKey)
				{
					xsdMiscServ.Attrib2IsKey = miscServ.OM_IMAttrib2IsKey;
					xsdMiscServ.Attrib2IsKeySpecified = true;
				}
				if (!miscServ.OM_IMPartAttrib3Type.IsEmpty)
				{
					xsdMiscServ.PartAttrib3Type = miscServ.OM_IMPartAttrib3Type;
				}

				if (!miscServ.OM_IMPartAttrib3Name.IsEmpty)
				{
					xsdMiscServ.PartAttrib3Name = miscServ.OM_IMPartAttrib3Name;
				}

				if (miscServ.OM_IMAttrib3IsKey)
				{
					xsdMiscServ.Attrib3IsKey = miscServ.OM_IMAttrib3IsKey;
					xsdMiscServ.Attrib3IsKeySpecified = true;
				}

				if (miscServ.OM_IMSerialNumberIsKey)
				{
					xsdMiscServ.SerialNumberIsKey = miscServ.OM_IMSerialNumberIsKey;
					xsdMiscServ.SerialNumberIsKeySpecified = true;
				}

				if (miscServ.OM_IMUseExpiryDate)
				{
					xsdMiscServ.UseExpiryDate = miscServ.OM_IMUseExpiryDate;
					xsdMiscServ.UseExpiryDateSpecified = true;
				}
				if (miscServ.OM_IMUsePackingDate)
				{
					xsdMiscServ.UsePackingDate = miscServ.OM_IMUsePackingDate;
					xsdMiscServ.UsePackingDateSpecified = true;
				}
				if (miscServ.OM_IMImporterRequiresOrderNumbersOnDocs)
				{
					xsdMiscServ.ImporterRequiresOrderNumbersOnDocs = miscServ.OM_IMImporterRequiresOrderNumbersOnDocs;
					xsdMiscServ.ImporterRequiresOrderNumbersOnDocsSpecified = true;
				}
				if (miscServ.OM_IMJobRequireOrderTrackLink)
				{
					xsdMiscServ.ImportJobRequireOrderTrackLink = miscServ.OM_IMJobRequireOrderTrackLink;
					xsdMiscServ.ImportJobRequireOrderTrackLinkSpecified = true;
				}
				if (!miscServ.OM_IMDocumentAddressPreference.IsEmpty)
				{
					xsdMiscServ.ImportDocumentAddressPreference = miscServ.OM_IMDocumentAddressPreference;
				}

				if (!miscServ.OM_IMDefaultWarehousePickOption.IsEmpty)
				{
					xsdMiscServ.DefaultWarehousePickOption = miscServ.OM_IMDefaultWarehousePickOption;
				}

				if (!miscServ.OM_IMInvoiceDetailReportSort.IsEmpty)
				{
					xsdMiscServ.InvoiceDetailReportSort = miscServ.OM_IMInvoiceDetailReportSort;
				}

				if (!miscServ.OM_IMInvoiceDetailReportSort2.IsEmpty)
				{
					xsdMiscServ.InvoiceDetailReportSort2 = miscServ.OM_IMInvoiceDetailReportSort2;
				}

				if (!miscServ.OM_IMInvoiceDetailReportSort3.IsEmpty)
				{
					xsdMiscServ.InvoiceDetailReportSort3 = miscServ.OM_IMInvoiceDetailReportSort3;
				}

				if (!miscServ.OM_LandedCostMarginPercent1.IsEmpty)
				{
					xsdMiscServ.LandedCostMarginPercent1 = miscServ.OM_LandedCostMarginPercent1;
					xsdMiscServ.LandedCostMarginPercent1Specified = true;
				}
				if (!miscServ.OM_LandedCostMarginPercent2.IsEmpty)
				{
					xsdMiscServ.LandedCostMarginPercent2 = miscServ.OM_LandedCostMarginPercent2;
					xsdMiscServ.LandedCostMarginPercent2Specified = true;
				}
				if (!miscServ.OM_LandedCostMarginPercent3.IsEmpty)
				{
					xsdMiscServ.LandedCostMarginPercent3 = miscServ.OM_LandedCostMarginPercent3;
					xsdMiscServ.LandedCostMarginPercent3Specified = true;
				}
				if (!miscServ.OM_LastArchiveDate.IsEmpty)
				{
					xsdMiscServ.LastArchiveDate = miscServ.OM_LastArchiveDate.ToDateTime();
					xsdMiscServ.LastArchiveDateSpecified = true;
				}
				if (miscServ.OM_EXJobRequireOrderTrackLink)
				{
					xsdMiscServ.ExportJobRequireOrderTrackLink = miscServ.OM_EXJobRequireOrderTrackLink;
					xsdMiscServ.ExportJobRequireOrderTrackLink = true;
				}
				if (miscServ.OM_EXExporterRequiresOrderNumbersOnDocs)
				{
					xsdMiscServ.ExporterRequiresOrderNumbersOnDocs = miscServ.OM_EXExporterRequiresOrderNumbersOnDocs;
					xsdMiscServ.ExporterRequiresOrderNumbersOnDocsSpecified = true;
				}
				if (!miscServ.OM_RS_NKEXDefaultServiceLevel.IsEmpty)
				{
					xsdMiscServ.ExportDefaultServiceLevel = miscServ.OM_RS_NKEXDefaultServiceLevel;
				}

				if (!miscServ.OM_EXGoodsDescription.IsEmpty)
				{
					xsdMiscServ.GoodsDescription = miscServ.OM_EXGoodsDescription;
				}

				if (!miscServ.OM_EXExporterCategory.IsEmpty)
				{
					xsdMiscServ.ExporterCategory = miscServ.OM_EXExporterCategory;
				}

				if (!miscServ.OM_EXFCLEquipmentNeeded.IsEmpty)
				{
					xsdMiscServ.ExportFCLEquipmentNeeded = miscServ.OM_EXFCLEquipmentNeeded;
				}

				if (!miscServ.OM_EXLCLEquipmentNeeded.IsEmpty)
				{
					xsdMiscServ.ExportLCLEquipmentNeeded = miscServ.OM_EXLCLEquipmentNeeded;
				}

				if (!miscServ.OM_EXAirEquipmentNeeded.IsEmpty)
				{
					xsdMiscServ.ExportAirEquipmentNeeded = miscServ.OM_EXAirEquipmentNeeded;
				}

				if (miscServ.OM_EXAllowedToPrintOriginalBL)
				{
					xsdMiscServ.AllowedToPrintOriginalBL = miscServ.OM_EXAllowedToPrintOriginalBL;
					xsdMiscServ.AllowedToPrintOriginalBLSpecified = true;
				}
				if (!miscServ.OM_EXDocumentAddressPreference.IsEmpty)
				{
					xsdMiscServ.ExportDocumentAddressPreference = miscServ.OM_EXDocumentAddressPreference;
				}

				if (!miscServ.OM_EXDefaultDGContactPhoneUsed.IsEmpty)
				{
					xsdMiscServ.DefaultDGContactPhoneUsed = miscServ.OM_EXDefaultDGContactPhoneUsed;
				}

				if (!miscServ.OM_EXDefaultInvoicePriceFromProductLastCost.IsEmpty)
				{
					xsdMiscServ.DefaultInvoicePriceFromProductLastCost = miscServ.OM_EXDefaultInvoicePriceFromProductLastCost;
				}

				if (!miscServ.OM_EXHandlingInstuctions.IsEmpty)
				{
					xsdMiscServ.HandlingInstructions = miscServ.OM_EXHandlingInstuctions;
				}

				if (!miscServ.OM_EXDefaultIncoTerm.IsEmpty)
				{
					xsdMiscServ.ExportDefaultIncoTerm = miscServ.OM_EXDefaultIncoTerm;
				}

				if (!miscServ.OM_EXMergeCustomsInvoiceLinesBy.IsEmpty)
				{
					xsdMiscServ.ExportMergeCustomsInvoiceLinesBy = miscServ.OM_EXMergeCustomsInvoiceLinesBy;
				}

				if (!miscServ.OM_EXPreAllocPrefix.IsEmpty)
				{
					xsdMiscServ.PreAllocPrefix = miscServ.OM_EXPreAllocPrefix;
				}

				if (!miscServ.OM_FWAgentCategory.IsEmpty)
				{
					xsdMiscServ.ForwarderAgentCategory = miscServ.OM_FWAgentCategory;
				}

				if (miscServ.OM_FWAgentBelongsToGroup)
				{
					xsdMiscServ.ForwarderAgentBelongsToGroup = miscServ.OM_FWAgentBelongsToGroup;
					xsdMiscServ.ForwarderAgentBelongsToGroupSpecified = true;
				}
				if (miscServ.OM_FWHandlesAir)
				{
					xsdMiscServ.ForwarderHandlesAir = miscServ.OM_FWHandlesAir;
					xsdMiscServ.ForwarderHandlesAirSpecified = true;
				}
				if (miscServ.OM_FWHandlesSea)
				{
					xsdMiscServ.ForwarderHandlesSea = miscServ.OM_FWHandlesSea;
					xsdMiscServ.ForwarderHandlesSeaSpecified = true;
				}
				if (!miscServ.OM_FWHandlesSeaForPortOrCountry.IsEmpty)
				{
					xsdMiscServ.ForwarderHandlesSeaForPortOrCountry = miscServ.OM_FWHandlesSeaForPortOrCountry;
				}

				if (!miscServ.OM_FWHandlesAirForPortOrCountry.IsEmpty)
				{
					xsdMiscServ.ForwarderHandlesAirForPortOrCountry = miscServ.OM_FWHandlesAirForPortOrCountry;
				}

				if (miscServ.OM_FWRequestForCreditAllowed)
				{
					xsdMiscServ.ForwarderRequestForCreditAllowed = miscServ.OM_FWRequestForCreditAllowed;
					xsdMiscServ.ForwarderRequestForCreditAllowedSpecified = true;
				}
				if (miscServ.OM_FWBillCollectFeesOnSingleInvoice)
				{
					xsdMiscServ.ForwarderBillCollectFeesOnSingleInvoice = miscServ.OM_FWBillCollectFeesOnSingleInvoice;
					xsdMiscServ.ForwarderBillCollectFeesOnSingleInvoiceSpecified = true;
				}
				if (miscServ.OM_FWDealDirectlyWithUltimates)
				{
					xsdMiscServ.ForwarderDealDirectlyWithUltimates = miscServ.OM_FWDealDirectlyWithUltimates;
					xsdMiscServ.ForwarderDealDirectlyWithUltimatesSpecified = true;
				}
				if (!miscServ.OM_FWIATACode.IsEmpty)
				{
					xsdMiscServ.ForwarderIATACode = miscServ.OM_FWIATACode;
				}

				if (!miscServ.OM_FWIATAAccountNumber.IsEmpty)
				{
					xsdMiscServ.ForwarderIATAAccountNumber = miscServ.OM_FWIATAAccountNumber;
				}

				if (miscServ.OM_FWDirectAMSReporter)
				{
					xsdMiscServ.ForwarderDirectAMSReporter = miscServ.OM_FWDirectAMSReporter;
					xsdMiscServ.ForwarderDirectAMSReporterSpecified = true;
				}
				if (!miscServ.OM_CRCarrierCategory.IsEmpty)
				{
					xsdMiscServ.CarrierCategory = miscServ.OM_CRCarrierCategory;
				}

				if (!miscServ.OM_SVServicesCategory.IsEmpty)
				{
					xsdMiscServ.ServicesCategory = miscServ.OM_SVServicesCategory;
				}

				if (!miscServ.OM_CMSalesCategory.IsEmpty)
				{
					xsdMiscServ.SalesCategory = miscServ.OM_CMSalesCategory;
				}

				if (!miscServ.OM_CMCompetitorActivity.IsEmpty)
				{
					xsdMiscServ.CompetitorActivity = miscServ.OM_CMCompetitorActivity;
				}

				if (!miscServ.OM_CMLastCallDate.IsEmpty)
				{
					xsdMiscServ.LastCallDate = miscServ.OM_CMLastCallDate.ToDateTime();
					xsdMiscServ.LastCallDateSpecified = true;
				}
				if (!miscServ.OM_CMClientSize.IsEmpty)
				{
					xsdMiscServ.ClientSize = miscServ.OM_CMClientSize;
				}

				if (!miscServ.OM_CMNoOfEmployees.IsEmpty)
				{
					xsdMiscServ.NoOfEmployees = miscServ.OM_CMNoOfEmployees;
					xsdMiscServ.NoOfEmployeesSpecified = true;
				}
				if (!miscServ.OM_CMEstimatedDateToClose.IsEmpty)
				{
					xsdMiscServ.EstimatedDateToClose = miscServ.OM_CMEstimatedDateToClose.ToDateTime();
					xsdMiscServ.EstimatedDateToCloseSpecified = true;
				}
				if (!miscServ.OM_CMGrowthOutlook.IsEmpty)
				{
					xsdMiscServ.GrowthOutlook = miscServ.OM_CMGrowthOutlook;
				}

				if (!miscServ.FollowUpDate.IsEmpty)
				{
					xsdMiscServ.FollowUpDate = miscServ.FollowUpDate.ToDateTime();
					xsdMiscServ.FollowUpDateSpecified = true;
				}
				if (miscServ.OM_CMDoesExports)
				{
					xsdMiscServ.DoesExports = miscServ.OM_CMDoesExports;
					xsdMiscServ.DoesExportsSpecified = true;
				}
				if (miscServ.OM_CMDoesImports)
				{
					xsdMiscServ.DoesImports = miscServ.OM_CMDoesImports;
					xsdMiscServ.DoesImportsSpecified = true;
				}
				if (!miscServ.OM_RH_NKCMMainImportCmdty.IsEmpty)
				{
					xsdMiscServ.MainImportCmdty = miscServ.OM_RH_NKCMMainImportCmdty;
				}

				if (!miscServ.OM_RH_NKCMMainExportCmdty.IsEmpty)
				{
					xsdMiscServ.MainExportCmdty = miscServ.OM_RH_NKCMMainExportCmdty;
				}

				if (miscServ.OM_CMUseTradeLaneFigures)
				{
					xsdMiscServ.UseTradeLaneFigures = miscServ.OM_CMUseTradeLaneFigures;
					xsdMiscServ.UseTradeLaneFiguresSpecified = true;
				}
				if (!miscServ.OM_CMTotalClientRevenue.IsEmpty)
				{
					xsdMiscServ.TotalClientRevenue = miscServ.OM_CMTotalClientRevenue;
					xsdMiscServ.TotalClientRevenueSpecified = true;
				}
				if (!miscServ.OM_CMPercentage.IsEmpty)
				{
					xsdMiscServ.Percentage = miscServ.OM_CMPercentage;
					xsdMiscServ.PercentageSpecified = true;
				}
				if (!miscServ.OM_CMAcheivableClientRevenue.IsEmpty)
				{
					xsdMiscServ.AcheivableClientRevenue = miscServ.OM_CMAcheivableClientRevenue;
					xsdMiscServ.AcheivableClientRevenueSpecified = true;
				}
				if (!miscServ.OM_CMEstimatedProfit.IsEmpty)
				{
					xsdMiscServ.EstimatedProfit = miscServ.OM_CMEstimatedProfit;
					xsdMiscServ.EstimatedProfitSpecified = true;
				}
				if (!miscServ.OM_CMWarehouseRevenue.IsEmpty)
				{
					xsdMiscServ.WarehouseRevenue = miscServ.OM_CMWarehouseRevenue;
					xsdMiscServ.WarehouseRevenueSpecified = true;
				}
				if (!miscServ.OM_CMConsultingRevenue.IsEmpty)
				{
					xsdMiscServ.ConsultingRevenue = miscServ.OM_CMConsultingRevenue;
					xsdMiscServ.ConsultingRevenueSpecified = true;
				}
				if (!miscServ.OM_CMOverallClientRelation.IsEmpty)
				{
					xsdMiscServ.OverallClientRelation = miscServ.OM_CMOverallClientRelation;
					xsdMiscServ.OverallClientRelationSpecified = true;
				}
				if (!miscServ.OM_CMClientsDesireToRemain.IsEmpty)
				{
					xsdMiscServ.ClientsDesireToRemain = miscServ.OM_CMClientsDesireToRemain;
					xsdMiscServ.ClientsDesireToRemainSpecified = true;
				}
				if (!miscServ.OM_CMEaseClientCanBePoached.IsEmpty)
				{
					xsdMiscServ.EaseClientCanBePoached = miscServ.OM_CMEaseClientCanBePoached;
					xsdMiscServ.EaseClientCanBePoachedSpecified = true;
				}

				if (!miscServ.OM_CMAmountOfElectronicIntegration.IsEmpty)
				{
					xsdMiscServ.AmountOfElectronicIntegration = miscServ.OM_CMAmountOfElectronicIntegration.ToString();
				}

				if (!miscServ.OM_CMOverallEffectOfClientOnAirfreightCosts.IsEmpty)
				{
					xsdMiscServ.OverallEffectOfClientOnAirfreightCosts = miscServ.OM_CMOverallEffectOfClientOnAirfreightCosts;
				}

				if (!miscServ.OM_CMOverallEffectOfClientOnLCLCosts.IsEmpty)
				{
					xsdMiscServ.OverallEffectOfClientOnLCLCosts = miscServ.OM_CMOverallEffectOfClientOnLCLCosts;
				}

				if (!miscServ.OM_CMOverallEffectOfClientOnTEUCosts.IsEmpty)
				{
					xsdMiscServ.OverallEffectOfClientOnTEUCosts = miscServ.OM_CMOverallEffectOfClientOnTEUCosts;
				}

				if (!miscServ.OM_CMOverallEffectOfClientOnWarehousingCosts.IsEmpty)
				{
					xsdMiscServ.OverallEffectOfClientOnWarehousingCosts = miscServ.OM_CMOverallEffectOfClientOnWarehousingCosts;
				}

				if (!miscServ.OM_CMOverallEffectOfClientOnOtherCosts.IsEmpty)
				{
					xsdMiscServ.OverallEffectOfClientOnOtherCosts = miscServ.OM_CMOverallEffectOfClientOnOtherCosts;
				}

				if (!miscServ.OM_CMAmountOfBusinessWon.IsEmpty)
				{
					xsdMiscServ.AmountOfBusinessWon = miscServ.OM_CMAmountOfBusinessWon;
					xsdMiscServ.AmountOfBusinessWonSpecified = true;
				}
				if (!miscServ.OM_CMSalesTerritory.IsEmpty)
				{
					xsdMiscServ.SalesTerritory = miscServ.OM_CMSalesTerritory;
				}

				if (!miscServ.OM_CMClientCommenced.IsEmpty)
				{
					xsdMiscServ.ClientCommenced = miscServ.OM_CMClientCommenced.ToDateTime();
				}

				if (!miscServ.OM_CMClientPortalHomePage.IsEmpty)
				{
					xsdMiscServ.ClientPortalHomePage = miscServ.OM_CMClientPortalHomePage;
				}

				if (!miscServ.OM_CICompetitorCategory.IsEmpty)
				{
					xsdMiscServ.CompetitorCategory = miscServ.OM_CICompetitorCategory;
				}

				if (!miscServ.OM_CIEstimatedStaffThisLocation.IsEmpty)
				{
					xsdMiscServ.EstimatedStaffThisLocation = miscServ.OM_CIEstimatedStaffThisLocation;
					xsdMiscServ.EstimatedStaffThisLocationSpecified = true;
				}
				if (!miscServ.OM_CITypeOfService.IsEmpty)
				{
					xsdMiscServ.TypeOfService = miscServ.OM_CITypeOfService;
				}

				if (!miscServ.OM_CIEstimatedStaffThisCountry.IsEmpty)
				{
					xsdMiscServ.EstimatedStaffThisCountry = miscServ.OM_CIEstimatedStaffThisCountry;
				}

				if (!miscServ.OM_CISellingStyle.IsEmpty)
				{
					xsdMiscServ.SellingStyle = miscServ.OM_CISellingStyle;
				}

				if (!miscServ.OM_CITurnover.IsEmpty)
				{
					xsdMiscServ.Turnover = miscServ.OM_CITurnover;
					xsdMiscServ.TurnoverSpecified = true;
				}
				if (!miscServ.OM_CIProfit.IsEmpty)
				{
					xsdMiscServ.Profit = miscServ.OM_CIProfit;
					xsdMiscServ.ProfitSpecified = true;
				}
				if (!miscServ.OM_CICapitalEmployed.IsEmpty)
				{
					xsdMiscServ.CapitalEmployed = miscServ.OM_CICapitalEmployed;
					xsdMiscServ.CapitalEmployedSpecified = true;
				}
				if (!miscServ.OM_CICompetitiveRanking.IsEmpty)
				{
					xsdMiscServ.CompetitiveRanking = miscServ.OM_CICompetitiveRanking;
					xsdMiscServ.CompetitiveRankingSpecified = true;
				}
				if (!miscServ.OM_CIStrength.IsEmpty)
				{
					xsdMiscServ.Strength = miscServ.OM_CIStrength;
				}

				if (!miscServ.OM_CIWeaknesses.IsEmpty)
				{
					xsdMiscServ.Weaknesses = miscServ.OM_CIWeaknesses;
				}

				if (!miscServ.OM_CIOpportunities.IsEmpty)
				{
					xsdMiscServ.Opportunities = miscServ.OM_CIOpportunities;
				}

				if (!miscServ.OM_CIThreats.IsEmpty)
				{
					xsdMiscServ.Threats = miscServ.OM_CIThreats;
				}

				if (!miscServ.OM_WhsExpiryNotificationFromDefaults.IsEmpty)
				{
					xsdMiscServ.WhsExpiryNotificationFromDefaults = miscServ.OM_WhsExpiryNotificationFromDefaults;
					xsdMiscServ.WhsExpiryNotificationFromDefaultsSpecified = true;
				}
				if (!miscServ.OM_WhsExpiryNotificationPeriod.IsEmpty)
				{
					xsdMiscServ.WhsExpiryNotificationPeriod = miscServ.OM_WhsExpiryNotificationPeriod;
					xsdMiscServ.WhsExpiryNotificationPeriodSpecified = true;
				}
				if (!miscServ.OM_WhsDefaultExpiryNotificationPeriodInDays.IsEmpty)
				{
					xsdMiscServ.WhsDefaultExpiryNotificationPeriodInDays = miscServ.OM_WhsDefaultExpiryNotificationPeriodInDays;
					xsdMiscServ.WhsDefaultExpiryNotificationPeriodInDaysSpecified = true;
				}
				if (!miscServ.OM_WhsClientInvoiceFormat.IsEmpty)
				{
					xsdMiscServ.WhsClientInvoiceFormat = miscServ.OM_WhsClientInvoiceFormat;
				}

				if (!miscServ.OM_WhsOrderNumberUniquenessStrategy.IsEmpty)
				{
					xsdMiscServ.OrderNumberUniquenessStrategy = miscServ.OM_WhsOrderNumberUniquenessStrategy;
				}

				if (!miscServ.OM_CustomAttrib1.IsEmpty)
				{
					xsdMiscServ.CustomAttrib1 = miscServ.OM_CustomAttrib1;
				}

				if (!miscServ.OM_CustomAttrib2.IsEmpty)
				{
					xsdMiscServ.CustomAttrib2 = miscServ.OM_CustomAttrib2;
				}

				if (!miscServ.OM_CustomAttrib3.IsEmpty)
				{
					xsdMiscServ.CustomAttrib3 = miscServ.OM_CustomAttrib3;
				}

				if (!miscServ.OM_CustomDate1.IsEmpty)
				{
					xsdMiscServ.CustomDate1 = miscServ.OM_CustomDate1.ToDateTime();
					xsdMiscServ.CustomDate1Specified = true;
				}
				if (!miscServ.OM_CustomDate2.IsEmpty)
				{
					xsdMiscServ.CustomDate2 = miscServ.OM_CustomDate2.ToDateTime();
					xsdMiscServ.CustomDate2Specified = true;
				}
				if (!miscServ.OM_CustomDate3.IsEmpty)
				{
					xsdMiscServ.CustomDate3 = miscServ.OM_CustomDate3.ToDateTime();
					xsdMiscServ.CustomDate3Specified = true;
				}
				if (!miscServ.OM_CustomDecimal1.IsEmpty)
				{
					xsdMiscServ.CustomDecimal1 = miscServ.OM_CustomDecimal1;
					xsdMiscServ.CustomDecimal1Specified = true;
				}
				if (!miscServ.OM_CustomDecimal2.IsEmpty)
				{
					xsdMiscServ.CustomDecimal2 = miscServ.OM_CustomDecimal2;
					xsdMiscServ.CustomDecimal2Specified = true;
				}
				if (!miscServ.OM_CustomDecimal3.IsEmpty)
				{
					xsdMiscServ.CustomDecimal3 = miscServ.OM_CustomDecimal3;
					xsdMiscServ.CustomDecimal3Specified = true;
				}
				if (miscServ.OM_CustomFlag1)
				{
					xsdMiscServ.CustomFlag1 = miscServ.OM_CustomFlag1;
					xsdMiscServ.CustomFlag1Specified = true;
				}
				if (miscServ.OM_CustomFlag2)
				{
					xsdMiscServ.CustomFlag2 = miscServ.OM_CustomFlag2;
					xsdMiscServ.CustomFlag2Specified = true;
				}
				if (miscServ.OM_CustomFlag3)
				{
					xsdMiscServ.CustomFlag3 = miscServ.OM_CustomFlag3;
					xsdMiscServ.CustomFlag3Specified = true;
				}
				if (miscServ.OM_CustomFlag4)
				{
					xsdMiscServ.CustomFlag4 = miscServ.OM_CustomFlag4;
					xsdMiscServ.CustomFlag4Specified = true;
				}
				if (!miscServ.OM_OH.IsEmpty)
				{
					xsdMiscServ.OrgHeaderPK = miscServ.OM_OH.ToString();
				}
				if (!miscServ.OM_OC_EXDefaultDGContact.IsEmpty) { xsdMiscServ.OC_EXDefaultDGContactPK = miscServ.OM_OC_EXDefaultDGContact.ToString(); }
				if (!miscServ.OM_RN_NKEXDefaultCntryOfOrigin.IsEmpty) { xsdMiscServ.RN_EXDefaultCountryOfOriginPK = miscServ.EXDefaultCntryOfOrigin == null ? string.Empty : miscServ.EXDefaultCntryOfOrigin.PK.ToString(); }
				if (!miscServ.OM_RX_NKEXDefCurrency.IsEmpty) { xsdMiscServ.RX_EXDefaultCurrencyPK = miscServ.EXDefCurrency == null ? string.Empty : miscServ.EXDefCurrency.PK.ToString(); }
				if (!miscServ.OM_RX_NKFWDefCurrency.IsEmpty) { xsdMiscServ.RX_FWDefaultCurrencyPK = miscServ.FWDefCurrency == null ? string.Empty : miscServ.FWDefCurrency.PK.ToString(); }
				if (!miscServ.OM_SO_WhsPackingSlip.IsEmpty) { xsdMiscServ.SO_WhsPackingSlipPK = miscServ.OM_SO_WhsPackingSlip.ToString(); }
				if (miscServ.OM_WhsGenerateBackOrdersOnShortfalls) { xsdMiscServ.WhsGenerateBackOrdersOnShortfalls = xsdMiscServ.WhsGenerateBackOrdersOnShortfallsSpecified = true; }
				if (!miscServ.OM_WhsPackingSlipOrderBy.IsEmpty) { xsdMiscServ.WhsPackingSlipOrderBy = miscServ.OM_WhsPackingSlipOrderBy; }
				if (!miscServ.OM_WhsDefaultWarehousePickMode.IsEmpty) { xsdMiscServ.WhsDefaultWarehousePickMode = miscServ.OM_WhsDefaultWarehousePickMode; }
				if (!miscServ.OM_WhsOrderFulfillmentRule.IsEmpty) { xsdMiscServ.WhsOrderFulfillmentRule = miscServ.OM_WhsOrderFulfillmentRule; }
				if (!miscServ.OM_CMAuthorityToLeave.IsEmpty)
				{
					xsdMiscServ.CM_AuthorityToLeave = miscServ.OM_CMAuthorityToLeave;
					xsdMiscServ.CM_AuthorityToLeaveSpecified = true;
				}
				if (!miscServ.OM_EXOwnsProducts.IsEmpty)
				{
					xsdMiscServ.OM_EXOwnsProducts = miscServ.OM_EXOwnsProducts;
					xsdMiscServ.OM_EXOwnsProductsSpecified = true;
				}
				if (!miscServ.OM_ConsignorAuthorityToLeave.IsEmpty)
				{
					xsdMiscServ.OM_ConsignorAuthorityToLeave = miscServ.OM_ConsignorAuthorityToLeave;
					xsdMiscServ.OM_ConsignorAuthorityToLeaveSpecified = true;
				}
				if (!miscServ.OM_EXJobRequireOrderTrackLink.IsEmpty)
				{
					xsdMiscServ.OM_EXJobRequireOrderTrackLink = miscServ.OM_EXJobRequireOrderTrackLink.ToString();
					xsdMiscServ.OM_EXJobRequireOrderTrackLinkSpecified = true;
				}
				if (!miscServ.OM_EXValidationForUnauditedClassification.IsEmpty)
				{
					xsdMiscServ.OM_EXValidationForUnauditedClassification = miscServ.OM_EXValidationForUnauditedClassification;
					xsdMiscServ.OM_EXValidationForUnauditedClassificationSpecified = true;
				}
				if (!miscServ.OM_IMShowDutyOnWarehouseEntries.IsEmpty)
				{
					xsdMiscServ.IMShowDutyOnWarehouseEntries = miscServ.OM_IMShowDutyOnWarehouseEntries;
					xsdMiscServ.IMShowDutyOnWarehouseEntriesSpecified = true;
				}
				if (!miscServ.OM_IMDisallowOrders.IsEmpty)
				{
					xsdMiscServ.IMDisallowOrders = miscServ.OM_IMDisallowOrders;
					xsdMiscServ.IMDisallowOrdersSpecified = true;
				}
				if (!miscServ.OM_IMAllowAttachedOrderXMLUpdate.IsEmpty)
				{
					xsdMiscServ.IMAllowAttachedOrderXMLUpdate = miscServ.OM_IMAllowAttachedOrderXMLUpdate;
					xsdMiscServ.IMAllowAttachedOrderXMLUpdateSpecified = true;
				}
				if (!miscServ.OM_IMOwnsProducts.IsEmpty)
				{
					xsdMiscServ.IMOwnsProducts = miscServ.OM_IMOwnsProducts;
					xsdMiscServ.IMOwnsProductsSpecified = true;
				}
				if (!miscServ.OM_IMBalanceInvoicePackage.IsEmpty)
				{
					xsdMiscServ.IMBalanceInvoicePackage = miscServ.OM_IMBalanceInvoicePackage;
					xsdMiscServ.IMBalanceInvoicePackageSpecified = true;
				}
				if (!miscServ.OM_ConsigneeAuthorityToLeave.IsEmpty)
				{
					xsdMiscServ.ConsigneeAuthorityToLeave = miscServ.OM_ConsigneeAuthorityToLeave;
					xsdMiscServ.ConsigneeAuthorityToLeaveSpecified = true;
				}
				if (!miscServ.OM_IMValidationForUnauditedClassification.IsEmpty)
				{
					xsdMiscServ.IMValidationForUnauditedClassification = miscServ.OM_IMValidationForUnauditedClassification;
					xsdMiscServ.IMValidationForUnauditedClassificationSpecified = true;
				}
			}
		}

		#endregion
	}
}
