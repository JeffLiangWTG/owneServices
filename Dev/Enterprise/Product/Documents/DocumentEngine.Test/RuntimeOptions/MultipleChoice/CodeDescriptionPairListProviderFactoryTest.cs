using CargoWise.Application;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.RuntimeOptions.Testing
{
	sealed class CodeDescriptionPairListProviderFactoryTest : TestCase
	{
		public void TestFactory()
		{
			AssertEquals(typeof(DependentFilterListForTestProvider), CodeDescriptionPairListProviderFactory.GetStaticCodeDescriptionPairListProvider("dependentfilterlistfortestprovider").GetType());
			AssertEquals(typeof(ABCCategoryCodeDescriptionPairProvider), CodeDescriptionPairListProviderFactory.GetStaticCodeDescriptionPairListProvider("abccategory").GetType());
			AssertEquals(typeof(MilestoneEventTypesPairProvider), CodeDescriptionPairListProviderFactory.GetStaticCodeDescriptionPairListProvider("milestoneeventtypeslist").GetType());
			AssertEquals(typeof(WorkflowTypePairProvider), CodeDescriptionPairListProviderFactory.GetStaticCodeDescriptionPairListProvider("workflowtypelist").GetType());
			AssertEquals(typeof(ConsolAccCategoryCodeDescriptionPairProvider), CodeDescriptionPairListProviderFactory.GetStaticCodeDescriptionPairListProvider("consolacccategory").GetType());
			AssertEquals(typeof(ConsolTypePairListProvider), CodeDescriptionPairListProviderFactory.GetStaticCodeDescriptionPairListProvider("consoltype").GetType());
			AssertEquals(typeof(ARCategoryCodeDescriptionPairProvider), CodeDescriptionPairListProviderFactory.GetStaticCodeDescriptionPairListProvider("arcategory").GetType());
			AssertEquals(typeof(APCategoryCodeDescriptionPairProvider), CodeDescriptionPairListProviderFactory.GetStaticCodeDescriptionPairListProvider("apcategory").GetType());
			AssertEquals(typeof(LanguageCodeDescriptionPairProvider), CodeDescriptionPairListProviderFactory.GetStaticCodeDescriptionPairListProvider("language").GetType());
			AssertEquals(typeof(TranslateLanguageCodeDescriptionPairProvider), CodeDescriptionPairListProviderFactory.GetStaticCodeDescriptionPairListProvider("translatelanguage").GetType());
			AssertEquals(typeof(BranchManagementCodeDescriptionPairProvider), CodeDescriptionPairListProviderFactory.GetStaticCodeDescriptionPairListProvider("branchmanagementcode").GetType());
			AssertEquals(typeof(TaxConfigurationCodeDescriptionPairListProvider), CodeDescriptionPairListProviderFactory.GetStaticCodeDescriptionPairListProvider("taxconfigurations").GetType());
			AssertEquals(typeof(ARTaxConfigurationCodeDescriptionPairListProvider), CodeDescriptionPairListProviderFactory.GetStaticCodeDescriptionPairListProvider("artaxconfigurations").GetType());
			AssertEquals(typeof(APTaxConfigurationCodeDescriptionPairListProvider), CodeDescriptionPairListProviderFactory.GetStaticCodeDescriptionPairListProvider("aptaxconfigurations").GetType());
			AssertEquals(typeof(ComplianceSubTypeCodeDescriptionPairProvider), CodeDescriptionPairListProviderFactory.GetStaticCodeDescriptionPairListProvider("compliancesubtype").GetType());
			AssertEquals(typeof(ARCreditRatingListCodeDescriptionPairProvider), CodeDescriptionPairListProviderFactory.GetStaticCodeDescriptionPairListProvider("arcreditratinglist").GetType());
			AssertEquals(typeof(GLRollupLevelCodeDescriptionPairProvider), CodeDescriptionPairListProviderFactory.GetStaticCodeDescriptionPairListProvider("glrolluplevel").GetType());
			AssertEquals(typeof(StorageClassCodeDescriptionPairProvider), CodeDescriptionPairListProviderFactory.GetStaticCodeDescriptionPairListProvider("storageclass").GetType());
			AssertEquals(typeof(ContainerQualityCodeDescriptionPairProvider), CodeDescriptionPairListProviderFactory.GetStaticCodeDescriptionPairListProvider("containerquality").GetType());
			AssertEquals(typeof(GLLanguageCodeDescriptionPairProvider), CodeDescriptionPairListProviderFactory.GetStaticCodeDescriptionPairListProvider("gllanguage").GetType());
			AssertEquals(typeof(ClaimTypeCodeDescriptionPairProvider), CodeDescriptionPairListProviderFactory.GetStaticCodeDescriptionPairListProvider("claimtype").GetType());
			AssertEquals(typeof(ClaimStatusCodeDescriptionPairProvider), CodeDescriptionPairListProviderFactory.GetStaticCodeDescriptionPairListProvider("claimstatus").GetType());
			AssertEquals(typeof(ClaimReasonCodeDescriptionPairProvider), CodeDescriptionPairListProviderFactory.GetStaticCodeDescriptionPairListProvider("claimreason").GetType());
			AssertEquals(typeof(ChargeCodeGroupPairProvider), CodeDescriptionPairListProviderFactory.GetStaticCodeDescriptionPairListProvider("chargegroup").GetType());
			AssertEquals(typeof(OrganisationNoteTypesCodeDescriptionPairProvider), CodeDescriptionPairListProviderFactory.GetStaticCodeDescriptionPairListProvider("organisationnotetypes").GetType());
			AssertEquals(typeof(OrderHeaderStatusCodeDescriptionPairProvider), CodeDescriptionPairListProviderFactory.GetStaticCodeDescriptionPairListProvider("orderheaderstatus").GetType());
			AssertEquals(typeof(CommissionStreamCodeDescriptionPairProvider), CodeDescriptionPairListProviderFactory.GetStaticCodeDescriptionPairListProvider("commissionstreams").GetType());
			AssertEquals(typeof(IncotermsPairProvider), CodeDescriptionPairListProviderFactory.GetStaticCodeDescriptionPairListProvider("incoterms").GetType());
			AssertEquals(typeof(OneStopCarrierPairProvider), CodeDescriptionPairListProviderFactory.GetStaticCodeDescriptionPairListProvider("onestopcarrier").GetType());
			AssertEquals(typeof(HolidaysTypeCodeDescriptionPairProvider), CodeDescriptionPairListProviderFactory.GetStaticCodeDescriptionPairListProvider("staffholidaystype").GetType());
			AssertEquals(typeof(HolidaysStatusCodeDescriptionPairProvider), CodeDescriptionPairListProviderFactory.GetStaticCodeDescriptionPairListProvider("staffholidaysstatus").GetType());
			AssertEquals(typeof(NZEntryPaymentStatusCodeDescriptionPairProvider), CodeDescriptionPairListProviderFactory.GetStaticCodeDescriptionPairListProvider("nzentrypaymentstatus").GetType());
			AssertEquals(ObjectFactory.GetType<Enterprise.Integration.Customs.Shared.IDeclarationMessageTypeCodeDescriptionPairProvider>(), CodeDescriptionPairListProviderFactory.GetStaticCodeDescriptionPairListProvider("declarationmessagetype").GetType());
			AssertEquals(ObjectFactory.GetType<Enterprise.Integration.Customs.Shared.IDeclarationMessageSubTypeCodeDescriptionPairProvider>(), CodeDescriptionPairListProviderFactory.GetStaticCodeDescriptionPairListProvider("declarationmessagesubtype").GetType());
			AssertEquals(ObjectFactory.GetType<Enterprise.Integration.Customs.Shared.IDeclarationEntryTypeCodeDescriptionPairProvider>(), CodeDescriptionPairListProviderFactory.GetStaticCodeDescriptionPairListProvider("declarationentrytype").GetType());
			AssertEquals(ObjectFactory.GetType<Enterprise.Integration.Customs.Shared.IDeclarationPackModeCodeDescriptionPairProvider>(), CodeDescriptionPairListProviderFactory.GetStaticCodeDescriptionPairListProvider("declarationpackmode").GetType());
			AssertEquals(ObjectFactory.GetType<Enterprise.Integration.Customs.Shared.IDeclarationPaymentMethodCodeDescriptionPairProvider>(), CodeDescriptionPairListProviderFactory.GetStaticCodeDescriptionPairListProvider("declarationpaymentmethod").GetType());
			AssertEquals(ObjectFactory.GetType<Enterprise.Integration.Customs.Shared.IDeclarationTransportModeCodeDescriptionPairProvider>(), CodeDescriptionPairListProviderFactory.GetStaticCodeDescriptionPairListProvider("declarationtransportmode").GetType());
			AssertEquals(ObjectFactory.GetType<Enterprise.Integration.Customs.DE.IMonthlyClosingDeclarationTypeListProvider>(), CodeDescriptionPairListProviderFactory.GetStaticCodeDescriptionPairListProvider("demonthlyclosingdeclarationtype").GetType());
			AssertEquals(ObjectFactory.GetType<Enterprise.Integration.Customs.Shared.IDeclarationEntryStatusCodeDescriptionPairProvider>(), CodeDescriptionPairListProviderFactory.GetStaticCodeDescriptionPairListProvider("declarationentrystatus").GetType());
			AssertEquals(ObjectFactory.GetType<Enterprise.Integration.Customs.Shared.IDeclarationMessageStatusCodeDescriptionPairProvider>(), CodeDescriptionPairListProviderFactory.GetStaticCodeDescriptionPairListProvider("declarationmessagestatus").GetType());
			AssertEquals(ObjectFactory.GetType<Enterprise.Integration.Customs.Shared.IDeclarationServiceLevelCodeDescriptionPairProvider>(), CodeDescriptionPairListProviderFactory.GetStaticCodeDescriptionPairListProvider("declarationservicelevel").GetType());
			AssertEquals(ObjectFactory.GetType<Enterprise.Integration.Customs.Shared.IProcedureCodesCodeDescriptionPairProvider>(), CodeDescriptionPairListProviderFactory.GetStaticCodeDescriptionPairListProvider("procedurecodes").GetType());
			AssertEquals(ObjectFactory.GetType<Enterprise.Integration.Customs.Shared.IPreviousProcedureCodesCodeDescriptionPairProvider>(), CodeDescriptionPairListProviderFactory.GetStaticCodeDescriptionPairListProvider("previousprocedurecodes").GetType());
			AssertEquals(typeof(HouseBillTypePairProvider), CodeDescriptionPairListProviderFactory.GetStaticCodeDescriptionPairListProvider("housebilltype").GetType());
			AssertEquals(typeof(ShipmentTransportModePairProvider), CodeDescriptionPairListProviderFactory.GetStaticCodeDescriptionPairListProvider("shipmenttransportmode").GetType());
			AssertEquals(typeof(FreightContainerModePairProvider), CodeDescriptionPairListProviderFactory.GetStaticCodeDescriptionPairListProvider("freightcontainermode").GetType());

			AssertEquals(typeof(IndustryVerticalTypeCodeDescriptionPairProvider), CodeDescriptionPairListProviderFactory.GetStaticCodeDescriptionPairListProvider("industryverticaltype").GetType());
			AssertEquals(typeof(PeriodOfActivityTypeCodeDescriptionPairProvider), CodeDescriptionPairListProviderFactory.GetStaticCodeDescriptionPairListProvider("periodofactivitytype").GetType());
			AssertEquals(typeof(CertaintyLikertItemListProvider), CodeDescriptionPairListProviderFactory.GetStaticCodeDescriptionPairListProvider("certaintylikertitem").GetType());
			AssertEquals(typeof(SalesCategoryCodeDescriptionPairProvider), CodeDescriptionPairListProviderFactory.GetStaticCodeDescriptionPairListProvider("salescategory").GetType());
			AssertEquals(typeof(CommunicationTypeCodeDescriptionPairListProvider), CodeDescriptionPairListProviderFactory.GetStaticCodeDescriptionPairListProvider("salescalltype").GetType());
			AssertEquals(typeof(EnquiryTypeCodeDescriptionPairProvider), CodeDescriptionPairListProviderFactory.GetStaticCodeDescriptionPairListProvider("enquirytype").GetType());
			AssertEquals(typeof(EnquiryTypeActiveCodeDescriptionPairProvider), CodeDescriptionPairListProviderFactory.GetStaticCodeDescriptionPairListProvider("enquirytypeactive").GetType());
			AssertEquals(typeof(EnquiryCloseReasonCodeDescriptionPairProvider), CodeDescriptionPairListProviderFactory.GetStaticCodeDescriptionPairListProvider("closereason").GetType());
			AssertEquals(typeof(EnquiryLeadInterestCodeDescriptionPairProvider), CodeDescriptionPairListProviderFactory.GetStaticCodeDescriptionPairListProvider("enquiryleadinterest").GetType());
			AssertEquals(typeof(CommunicationStatusCodeDescriptionPairListProvider), CodeDescriptionPairListProviderFactory.GetStaticCodeDescriptionPairListProvider("communicationstatus").GetType());
			AssertEquals(typeof(SalesLeadTypeCodeDescriptionPairProvider), CodeDescriptionPairListProviderFactory.GetStaticCodeDescriptionPairListProvider("salesleadtype").GetType());
			AssertEquals(typeof(SalesTerritoryCodeDescriptionPairProvider), CodeDescriptionPairListProviderFactory.GetStaticCodeDescriptionPairListProvider("salesterritory").GetType());
			AssertEquals(typeof(PaymentMethodCodeDescriptionPairProvider), CodeDescriptionPairListProviderFactory.GetStaticCodeDescriptionPairListProvider("paymentmethod").GetType());
			AssertEquals(typeof(LocalTransportJobTypeCodeDescriptionPairProvider), CodeDescriptionPairListProviderFactory.GetStaticCodeDescriptionPairListProvider("localtransportjobtype").GetType());
			AssertEquals(typeof(CartageDropModeCodeDescriptionPairProvider), CodeDescriptionPairListProviderFactory.GetStaticCodeDescriptionPairListProvider("cartagedropmode").GetType());
			AssertEquals(typeof(CategoryCodeDescriptionPairListProvider), CodeDescriptionPairListProviderFactory.GetStaticCodeDescriptionPairListProvider("category").GetType());

			AssertEquals(typeof(FreightRateClassCodeDescriptionPairProvider), CodeDescriptionPairListProviderFactory.GetStaticCodeDescriptionPairListProvider("freightrateclass").GetType());
			AssertEquals(typeof(AirDensityCodeDescriptionPairProvider), CodeDescriptionPairListProviderFactory.GetStaticCodeDescriptionPairListProvider("airdensity").GetType());
			AssertEquals(typeof(QuoteCancellationReasonCodeDescriptionPairProvider), CodeDescriptionPairListProviderFactory.GetStaticCodeDescriptionPairListProvider("quotecancellationreason").GetType());

			AssertNull(CodeDescriptionPairListProviderFactory.GetStaticCodeDescriptionPairListProvider("anythingelse"));

			AssertEquals(typeof(WhsPickMethodCodeDescriptionPairProvider), CodeDescriptionPairListProviderFactory.GetStaticCodeDescriptionPairListProvider("pickupmethod").GetType());
			AssertEquals(typeof(WhsLocationStatusCodeDescriptionPairListProvider), CodeDescriptionPairListProviderFactory.GetStaticCodeDescriptionPairListProvider("locationstatus").GetType());
			AssertEquals(typeof(WhsAreaTypeCodeDescriptionPairListProvider), CodeDescriptionPairListProviderFactory.GetStaticCodeDescriptionPairListProvider("areatype").GetType());

			AssertEquals(typeof(OpportunityStatusCodeDescriptionPairProvider), CodeDescriptionPairListProviderFactory.GetStaticCodeDescriptionPairListProvider("opportunitystatus").GetType());
			AssertEquals(typeof(OpportunityTypeCodeDescriptionPairProvider), CodeDescriptionPairListProviderFactory.GetStaticCodeDescriptionPairListProvider("opportunitytype").GetType());
			AssertEquals(typeof(OpportunityStageCodeDescriptionPairProvider), CodeDescriptionPairListProviderFactory.GetStaticCodeDescriptionPairListProvider("opportunitystage").GetType());
			AssertEquals(typeof(OpportunityOutcomeCodeDescriptionPairProvider), CodeDescriptionPairListProviderFactory.GetStaticCodeDescriptionPairListProvider("opportunityoutcome").GetType());
			AssertEquals(typeof(OpportunityProductTypeCodeDescriptionPairProvider), CodeDescriptionPairListProviderFactory.GetStaticCodeDescriptionPairListProvider("opportunityproducttype").GetType());
			AssertEquals(typeof(ClientSizeCodeDescriptionPairProvider), CodeDescriptionPairListProviderFactory.GetStaticCodeDescriptionPairListProvider("clientsize").GetType());
			AssertEquals(typeof(StaffAssignmentRolesCodeDescriptionPairProvider), CodeDescriptionPairListProviderFactory.GetStaticCodeDescriptionPairListProvider("staffmemberassignmentroles").GetType());
			AssertEquals(typeof(JobProfitLossReasonCodeDescriptionPairProvider), CodeDescriptionPairListProviderFactory.GetStaticCodeDescriptionPairListProvider("jobprofitlossreason").GetType());
			AssertEquals(typeof(CashFlowCategoryCodeDescriptionPairProvider), CodeDescriptionPairListProviderFactory.GetStaticCodeDescriptionPairListProvider("cashflowcategory").GetType());
			AssertEquals(ObjectFactory.GetType<Enterprise.Recruiter.Integration.ICertificateTypeCodeDescriptionPairListProvider>(), CodeDescriptionPairListProviderFactory.GetStaticCodeDescriptionPairListProvider("certificatetype").GetType());
			AssertEquals(ObjectFactory.GetType<Enterprise.Integration.Customs.US.IBrokerPaymentTypeCodeDescriptionPairProvider>(), CodeDescriptionPairListProviderFactory.GetStaticCodeDescriptionPairListProvider("uspaymenttype").GetType());
			AssertEquals(ObjectFactory.GetType<Enterprise.Integration.Customs.US.ISPICodeDescriptionPairProvider>(), CodeDescriptionPairListProviderFactory.GetStaticCodeDescriptionPairListProvider("usspilist").GetType());
			AssertEquals(ObjectFactory.GetType<Enterprise.Integration.Customs.US.IProductClaimCodeDescriptionPairProvider>(), CodeDescriptionPairListProviderFactory.GetStaticCodeDescriptionPairListProvider("usproductclaimcodelist").GetType());
			AssertEquals(ObjectFactory.GetType<Enterprise.Integration.Customs.US.ILiquidationTypeCodeDescriptionPairProvider>(), CodeDescriptionPairListProviderFactory.GetStaticCodeDescriptionPairListProvider("usliquidationtype").GetType());
			AssertEquals(ObjectFactory.GetType<Enterprise.Integration.Customs.US.IMonthListDescriptionPairProvider>(), CodeDescriptionPairListProviderFactory.GetStaticCodeDescriptionPairListProvider("monthlist").GetType());
			AssertEquals(ObjectFactory.GetType<Enterprise.Integration.Customs.US.IReleaseStatusCodeDescriptionPairProvider>(), CodeDescriptionPairListProviderFactory.GetStaticCodeDescriptionPairListProvider("usreleasestatus").GetType());
			AssertEquals(ObjectFactory.GetType<Enterprise.Integration.Customs.US.IImporterBondTypeCodeDescriptionPairProvider>(), CodeDescriptionPairListProviderFactory.GetStaticCodeDescriptionPairListProvider("usimporterbondtype").GetType());
			AssertEquals(ObjectFactory.GetType<Enterprise.Integration.Customs.US.IEIStatusCodeDescriptionPairProvider>(), CodeDescriptionPairListProviderFactory.GetStaticCodeDescriptionPairListProvider("usEIStatusList").GetType());
			AssertEquals(ObjectFactory.GetType<Enterprise.Integration.Customs.US.IENSStatusCodeDescriptionPairProvider>(), CodeDescriptionPairListProviderFactory.GetStaticCodeDescriptionPairListProvider("usENSStatusList").GetType());
			AssertEquals(ObjectFactory.GetType<Enterprise.Integration.Customs.US.IFDAStatusCodeDescriptionPairProvider>(), CodeDescriptionPairListProviderFactory.GetStaticCodeDescriptionPairListProvider("usFDAStatusList").GetType());
			AssertEquals(ObjectFactory.GetType<Enterprise.Integration.Customs.US.IPaymentStatusCodeDescriptionPairProvider>(), CodeDescriptionPairListProviderFactory.GetStaticCodeDescriptionPairListProvider("usPaymentStatusList").GetType());
			AssertEquals(ObjectFactory.GetType<Enterprise.Integration.Customs.US.IEntryModeCodeDescriptionPairProvider>(), CodeDescriptionPairListProviderFactory.GetStaticCodeDescriptionPairListProvider("usEntryModeList").GetType());
			AssertEquals(ObjectFactory.GetType<Enterprise.Integration.Customs.US.ITaxDeferredCodeDescriptionPairProvider>(), CodeDescriptionPairListProviderFactory.GetStaticCodeDescriptionPairListProvider("usTaxDeferredIndList").GetType());
			AssertEquals(ObjectFactory.GetType<Enterprise.Integration.Customs.US.IReconIssueCodeDescriptionPairProvider>(), CodeDescriptionPairListProviderFactory.GetStaticCodeDescriptionPairListProvider("usReconIssueList").GetType());
			AssertEquals(ObjectFactory.GetType<Enterprise.Integration.Customs.US.ISF.IActionReasonCodeCodeDescriptionPairProvider>(), CodeDescriptionPairListProviderFactory.GetStaticCodeDescriptionPairListProvider("isfactionreasoncode").GetType());
			AssertEquals(ObjectFactory.GetType<Enterprise.Integration.Customs.US.ISF.IBillTypeCodeDescriptionPairProvider>(), CodeDescriptionPairListProviderFactory.GetStaticCodeDescriptionPairListProvider("isfbilltype").GetType());
			AssertEquals(ObjectFactory.GetType<Enterprise.Integration.Customs.US.ISF.IBondActivityCodeCodeDescriptionPairProvider>(), CodeDescriptionPairListProviderFactory.GetStaticCodeDescriptionPairListProvider("isfbondactivitycode").GetType());
			AssertEquals(ObjectFactory.GetType<Enterprise.Integration.Customs.US.ISF.IDispositionCodeCodeDescriptionPairProvider>(), CodeDescriptionPairListProviderFactory.GetStaticCodeDescriptionPairListProvider("isfdispositioncode").GetType());
			AssertEquals(ObjectFactory.GetType<Enterprise.Integration.Customs.US.ISF.IEntryTypeCodeDescriptionPairProvider>(), CodeDescriptionPairListProviderFactory.GetStaticCodeDescriptionPairListProvider("isfentrytype").GetType());
			AssertEquals(ObjectFactory.GetType<Enterprise.Integration.Customs.US.ISF.IMessageStatusCodeDescriptionPairProvider>(), CodeDescriptionPairListProviderFactory.GetStaticCodeDescriptionPairListProvider("isfmessagestatus").GetType());
			AssertEquals(ObjectFactory.GetType<Enterprise.Integration.Customs.US.ISF.IShipmentTypeCodeDescriptionPairProvider>(), CodeDescriptionPairListProviderFactory.GetStaticCodeDescriptionPairListProvider("isfshipmenttype").GetType());
			AssertEquals(ObjectFactory.GetType<Enterprise.Integration.Customs.Shared.IAdditionalReferenceNumberTypesCodeDescriptionPairProvider>(), CodeDescriptionPairListProviderFactory.GetStaticCodeDescriptionPairListProvider("additionalreferencenumbertype").GetType());
			AssertEquals(ObjectFactory.GetType<Enterprise.Integration.Customs.CA.IACROSSServiceOptionsCodeDescriptionPairProvider>(), CodeDescriptionPairListProviderFactory.GetStaticCodeDescriptionPairListProvider("caacrossserviceoptions").GetType());
			AssertEquals(ObjectFactory.GetType<Enterprise.Integration.Customs.CA.ICATariffTreatmentCodeDescriptionPairProvider>(), CodeDescriptionPairListProviderFactory.GetStaticCodeDescriptionPairListProvider("catarifftreatmentcodes").GetType());
			AssertEquals(ObjectFactory.GetType<Enterprise.Integration.Customs.CA.ICARemissionTypeCodeDescriptionPairProvider>(), CodeDescriptionPairListProviderFactory.GetStaticCodeDescriptionPairListProvider("caremissiontypes").GetType());
			AssertEquals(ObjectFactory.GetType<Enterprise.Integration.Customs.CA.ICABondTypeCodeDescriptionPairProvider>(), CodeDescriptionPairListProviderFactory.GetStaticCodeDescriptionPairListProvider("cabondtypelist").GetType());
			AssertEquals(ObjectFactory.GetType<Enterprise.Integration.Customs.CA.ICAStateOfOriginCodeDescriptionPairProvider>(), CodeDescriptionPairListProviderFactory.GetStaticCodeDescriptionPairListProvider("castateoforigin").GetType());
			AssertEquals(ObjectFactory.GetType<Enterprise.Integration.Customs.ZA.IFRNCustomsOfficesProvider>(), CodeDescriptionPairListProviderFactory.GetStaticCodeDescriptionPairListProvider("zafrncustomsoffices").GetType());
			AssertEquals(ObjectFactory.GetType<Enterprise.Integration.Customs.ZA.ICustomsOfficesProvider>(), CodeDescriptionPairListProviderFactory.GetStaticCodeDescriptionPairListProvider("zacustomsoffices").GetType());
			AssertEquals(ObjectFactory.GetType<Enterprise.Integration.Customs.ZA.IReleasePrintIndicatorProvider>(), CodeDescriptionPairListProviderFactory.GetStaticCodeDescriptionPairListProvider("zareleaseprintindicators").GetType());
			AssertEquals(ObjectFactory.GetType<Enterprise.Integration.Customs.ZA.IProvisionalPaymentTypeProvider>(), CodeDescriptionPairListProviderFactory.GetStaticCodeDescriptionPairListProvider("zaprovisionalpaymenttypes").GetType());
			AssertEquals(ObjectFactory.GetType<Enterprise.Integration.Customs.ZA.IShipmentTypeProvider>(), CodeDescriptionPairListProviderFactory.GetStaticCodeDescriptionPairListProvider("zashipmenttype").GetType());
			AssertEquals(ObjectFactory.GetType<Enterprise.Integration.Customs.EU.IEntrySubStyleDescriptionPairProvider>(), CodeDescriptionPairListProviderFactory.GetStaticCodeDescriptionPairListProvider("eudeclarationentrysubstyle").GetType());
			AssertEquals(ObjectFactory.GetType<Enterprise.Integration.Customs.EU.ICusTempStorageRegHeaderStatusListProvider>(), CodeDescriptionPairListProviderFactory.GetStaticCodeDescriptionPairListProvider("eutemporarystorageregisterstatuslist").GetType());
			AssertEquals(ObjectFactory.GetType<Enterprise.Integration.Customs.EU.ICusTempStorageRegHeaderAppCodesListProvider>(), CodeDescriptionPairListProviderFactory.GetStaticCodeDescriptionPairListProvider("eutemporarystorageregisterappcodelist").GetType());
			AssertEquals(ObjectFactory.GetType<Enterprise.Integration.Customs.EU.IISTCustomsProfileCodeDescriptionPairProvider>(), CodeDescriptionPairListProviderFactory.GetStaticCodeDescriptionPairListProvider("euistcustomsprofiles").GetType());
			AssertEquals(ObjectFactory.GetType<Enterprise.Integration.Customs.EU.IISTLocationOfGoodsCodeDescriptionPairProvider>(), CodeDescriptionPairListProviderFactory.GetStaticCodeDescriptionPairListProvider("euistlocationofgoods").GetType());
			AssertEquals(ObjectFactory.GetType<Enterprise.Integration.Customs.EU.NCTS.IGuaranteeNumbersCodeDescriptionPairProvider>(), CodeDescriptionPairListProviderFactory.GetStaticCodeDescriptionPairListProvider("nctsguarantees").GetType());
			AssertEquals(ObjectFactory.GetType<Enterprise.Integration.Customs.EU.NCTS.IDeclarationTypesCodeDescriptionPairProvider>(), CodeDescriptionPairListProviderFactory.GetStaticCodeDescriptionPairListProvider("nctsdeclarationtypes").GetType());
			AssertEquals(ObjectFactory.GetType<Enterprise.Integration.Customs.FR.ICustomsOfficesProvider>(), CodeDescriptionPairListProviderFactory.GetStaticCodeDescriptionPairListProvider("frcustomsoffices").GetType());
			AssertEquals(ObjectFactory.GetType<Enterprise.Integration.Customs.FR.IProcedureCodesCodeDescriptionPairProvider>(), CodeDescriptionPairListProviderFactory.GetStaticCodeDescriptionPairListProvider("frprocedurecodes").GetType());
			AssertEquals(ObjectFactory.GetType<Enterprise.Integration.Customs.FR.ICustomsProfileCodeDescriptionPairProvider>(), CodeDescriptionPairListProviderFactory.GetStaticCodeDescriptionPairListProvider("frcustomsprofiles").GetType());
			AssertEquals(ObjectFactory.GetType<Enterprise.Integration.Customs.FR.IISWrittenoffD48CodeDescriptionPairProvider>(), CodeDescriptionPairListProviderFactory.GetStaticCodeDescriptionPairListProvider("iswrittenoffd48").GetType());
			AssertEquals(ObjectFactory.GetType<Enterprise.Integration.Customs.IT.IEntryInstructionStyleListProvider>(), CodeDescriptionPairListProviderFactory.GetStaticCodeDescriptionPairListProvider("itentryinstructionstylelist").GetType());

			AssertEquals(ObjectFactory.GetType<Enterprise.Integration.Customs.IT.ICustomsOfficesProvider>(), CodeDescriptionPairListProviderFactory.GetStaticCodeDescriptionPairListProvider("itcustomsoffices").GetType());
			AssertEquals(ObjectFactory.GetType<Enterprise.Integration.Customs.IT.IMethodOfPaymentListProvider>(), CodeDescriptionPairListProviderFactory.GetStaticCodeDescriptionPairListProvider("itmethodsofpayment").GetType());
			AssertEquals(ObjectFactory.GetType<Enterprise.Integration.Customs.IT.IDefermentAccountNumberProvider>(), CodeDescriptionPairListProviderFactory.GetStaticCodeDescriptionPairListProvider("itdefermentaccountnumber").GetType());

			AssertEquals(ObjectFactory.GetType<Enterprise.Integration.Customs.TW.ITWDeclarationTypeListCodeDescriptionPairProvider>(), CodeDescriptionPairListProviderFactory.GetStaticCodeDescriptionPairListProvider("twdeclarationtypeList").GetType());
			AssertEquals(ObjectFactory.GetType<Enterprise.Integration.Customs.TW.ITWOfficeOfReceiptListDescriptionPairProvider>(), CodeDescriptionPairListProviderFactory.GetStaticCodeDescriptionPairListProvider("twofficeofreceiptlist").GetType());

			AssertEquals(ObjectFactory.GetType<Enterprise.Integration.Customs.US.IJobApplicationCodeProvider>(), CodeDescriptionPairListProviderFactory.GetStaticCodeDescriptionPairListProvider("usmessagemode").GetType());
			AssertEquals(ObjectFactory.GetType<Enterprise.Integration.Customs.US.IUSFDAAgencyProgramCodeDescriptionPairProvider>(), CodeDescriptionPairListProviderFactory.GetStaticCodeDescriptionPairListProvider("usfdaagencyprogramcode").GetType());
			AssertEquals(ObjectFactory.GetType<Enterprise.Integration.Customs.US.IUSFDAProcessingCodeDescriptionPairProvider>(), CodeDescriptionPairListProviderFactory.GetStaticCodeDescriptionPairListProvider("usfdaprocessingcodes").GetType());
			AssertEquals(ObjectFactory.GetType<Enterprise.Integration.Customs.US.IUSAPHISProgramTypeDescriptionPairProvider>(), CodeDescriptionPairListProviderFactory.GetStaticCodeDescriptionPairListProvider("usaphisprogramtype").GetType());
			AssertEquals(ObjectFactory.GetType<Enterprise.Integration.Customs.US.IUSAPHISProcessingCodeDescriptionPairProvider>(), CodeDescriptionPairListProviderFactory.GetStaticCodeDescriptionPairListProvider("usaphisprocessingcode").GetType());
			AssertEquals(ObjectFactory.GetType<Enterprise.Integration.Customs.US.IUSFTZPTTMessageStatusListProvider>(), CodeDescriptionPairListProviderFactory.GetStaticCodeDescriptionPairListProvider("usftzpttmessagestatuslist").GetType());
			AssertEquals(ObjectFactory.GetType<Enterprise.Integration.Customs.US.IUSFTZAdmissionMessageStatusListProvider>(), CodeDescriptionPairListProviderFactory.GetStaticCodeDescriptionPairListProvider("usftzadmissionmessagestatuslist").GetType());
			AssertEquals(ObjectFactory.GetType<Enterprise.Integration.Customs.US.IUSFTZConcurrenceMessageStatusListProvider>(), CodeDescriptionPairListProviderFactory.GetStaticCodeDescriptionPairListProvider("usftzconcurrencemessagestatuslist").GetType());
			AssertEquals(ObjectFactory.GetType<Enterprise.Integration.Customs.US.IUSFTZGoodsArrivalMessageStatusListProvider>(), CodeDescriptionPairListProviderFactory.GetStaticCodeDescriptionPairListProvider("usftzgoodsarrivalmessagestatuslist").GetType());
			AssertEquals(ObjectFactory.GetType<Enterprise.Integration.Customs.US.IUSFTZDeliveryOfGoodsMessageStatusListProvider>(), CodeDescriptionPairListProviderFactory.GetStaticCodeDescriptionPairListProvider("usftzdeliveryofgoodsmessagestatuslist").GetType());
			AssertEquals(ObjectFactory.GetType<Enterprise.Integration.Customs.US.IUSITARExemptionNumberDescriptionPairProvider>(), CodeDescriptionPairListProviderFactory.GetStaticCodeDescriptionPairListProvider("usitarexemptionnumberlist").GetType());

			AssertEquals(ObjectFactory.GetType<Enterprise.Warehouse.Integration.IWhsWarehouseJobTypeWithSubTypeCodeDescriptionPairProvider>(), CodeDescriptionPairListProviderFactory.GetStaticCodeDescriptionPairListProvider("whsdockettypeandsubtype").GetType());
			AssertEquals(ObjectFactory.GetType<Enterprise.Warehouse.Integration.IWhsAdjustmentReasonCodeDescriptionPairProvider>(), CodeDescriptionPairListProviderFactory.GetStaticCodeDescriptionPairListProvider("whsadjustmentreason").GetType());
			AssertEquals(typeof(SalesRelationTypeCodeDescriptionPairListProvider), CodeDescriptionPairListProviderFactory.GetStaticCodeDescriptionPairListProvider("salesrelationtype").GetType());

			AssertEquals(ObjectFactory.GetType<Enterprise.Integration.Packing.IPackageDamagedReasonsCodeDescriptionPairProvider>(), CodeDescriptionPairListProviderFactory.GetStaticCodeDescriptionPairListProvider("packagedamagedreason").GetType());

			AssertEquals(typeof(PurchaseOrderGoodsReceivedNotesCodeDescriptionPairProvider), CodeDescriptionPairListProviderFactory.GetStaticCodeDescriptionPairListProvider("goodsreceivedstatus").GetType());
			AssertEquals(typeof(SubAccountTypeCodeDescriptionPairProvider), CodeDescriptionPairListProviderFactory.GetStaticCodeDescriptionPairListProvider("subaccounttype").GetType());
			AssertEquals(typeof(OrganisationTypeCodeDescriptionPairProvider), CodeDescriptionPairListProviderFactory.GetStaticCodeDescriptionPairListProvider("organisationtype").GetType());
			AssertEquals(typeof(ParentJobTypeCodeDescriptionPairProvider), CodeDescriptionPairListProviderFactory.GetStaticCodeDescriptionPairListProvider("parentjobtype").GetType());
			AssertEquals(typeof(ARAgreedPaymentMethodCodeDescriptionPairProvider), CodeDescriptionPairListProviderFactory.GetStaticCodeDescriptionPairListProvider("aragreedpaymentmethods").GetType());
			AssertEquals(typeof(APAgreedPaymentMethodCodeDescriptionPairProvider), CodeDescriptionPairListProviderFactory.GetStaticCodeDescriptionPairListProvider("apagreedpaymentmethods").GetType());

			AssertEquals(typeof(LicenseTypeCodeDescriptionPairListProvider), CodeDescriptionPairListProviderFactory.GetStaticCodeDescriptionPairListProvider("licensetypes").GetType());
			AssertEquals(typeof(GLPresentationCategoryGroupCodeDescriptionPairProvider), CodeDescriptionPairListProviderFactory.GetStaticCodeDescriptionPairListProvider("presentationcategorygroup").GetType());

			AssertEquals(ObjectFactory.GetType<Enterprise.Integration.Customs.KR.IKRTransportTypeCodeDescriptionPairProvider>(), CodeDescriptionPairListProviderFactory.GetStaticCodeDescriptionPairListProvider("kr_transportmode").GetType());
			AssertEquals(ObjectFactory.GetType<Enterprise.Integration.Customs.KR.IKRTransactionTypeCodeDescriptionPairProvider>(), CodeDescriptionPairListProviderFactory.GetStaticCodeDescriptionPairListProvider("kr_transactiontype").GetType());
			AssertEquals(ObjectFactory.GetType<Enterprise.Integration.Customs.KR.IKRExportTypeCodeDescriptionPairProvider>(), CodeDescriptionPairListProviderFactory.GetStaticCodeDescriptionPairListProvider("kr_exporttype").GetType());
			AssertEquals(ObjectFactory.GetType<Enterprise.Integration.Customs.KR.IKRDeclarationProcedureTypeCodeDescriptionPairProvider>(), CodeDescriptionPairListProviderFactory.GetStaticCodeDescriptionPairListProvider("kr_proceduretype").GetType());
			AssertEquals(ObjectFactory.GetType<Enterprise.Integration.Customs.KR.IKRInvoicePaymentTermCodeDescriptionPairProvider>(), CodeDescriptionPairListProviderFactory.GetStaticCodeDescriptionPairListProvider("kr_paymentmethod").GetType());
			AssertEquals(ObjectFactory.GetType<Enterprise.Integration.Customs.KR.IKRLocalExportMessageTypeCodeDescriptionPairProvider>(), CodeDescriptionPairListProviderFactory.GetStaticCodeDescriptionPairListProvider("kr_localexportmessagetype").GetType());
			AssertEquals(ObjectFactory.GetType<Enterprise.Integration.Customs.KR.IKRLocalExportTransactionNatureCodeDescriptionPairProvider>(), CodeDescriptionPairListProviderFactory.GetStaticCodeDescriptionPairListProvider("kr_localexporttransaction").GetType());
			AssertEquals(ObjectFactory.GetType<Enterprise.Integration.Customs.KR.IKRLocalExportGoodsTypeCodeDescriptionPairProvider>(), CodeDescriptionPairListProviderFactory.GetStaticCodeDescriptionPairListProvider("kr_localexporttype").GetType());
			AssertEquals(ObjectFactory.GetType<Enterprise.Integration.Customs.KR.IKRLocalExportDrawbackApplicantTypeCodeDescriptionPairProvider>(), CodeDescriptionPairListProviderFactory.GetStaticCodeDescriptionPairListProvider("kr_localexportdrawbakapplicanttype").GetType());
		}

		public void TestFactoryReturnsCorrectTypeForSupplyTypeProvider()
		{
			AssertEquals(typeof(SupplyTypeCodeDescriptionPairListProvider), CodeDescriptionPairListProviderFactory.GetStaticCodeDescriptionPairListProvider("supplytype").GetType());
		}

		public void TestGUIDKeyedFactory()
		{
			AssertEquals(typeof(DynamicOrderHeaderStatusCodeDescriptionPairProvider), CodeDescriptionPairListProviderFactory.GetDynamicGUIDKeyedCodeDescriptionPairList("dynamicorderheaderstatus").GetType());
			AssertEquals(typeof(DynamicOrderLineStatusCodeDescriptionPairProvider), CodeDescriptionPairListProviderFactory.GetDynamicGUIDKeyedCodeDescriptionPairList("dynamicorderlinestatus").GetType());
		}
	}
}
