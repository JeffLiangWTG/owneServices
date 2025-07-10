using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	public static class CodeDescriptionPairListProviderFactory
	{
		public static ICodeDescriptionPairListProvider GetStaticCodeDescriptionPairListProvider(string codeDescriptionPairName)
		{
			ICodeDescriptionPairListProvider result = null;
			TypeGetter codeDescriptionTypeProvider;
			if (Lookup.TryGetValue(codeDescriptionPairName.ToLower().Trim(), out codeDescriptionTypeProvider))
			{
				result = (ICodeDescriptionPairListProvider)Activator.CreateInstance(codeDescriptionTypeProvider());
			}
			return result;
		}

		delegate Type TypeGetter();

		static Dictionary<string, TypeGetter> Lookup
		{
			get
			{
				if (lookup.Value == null)
				{
					var l = new Dictionary<string, TypeGetter>();
					lookup.Value = l;
#if DEBUG
					l.Add(nameof(DependentFilterListForTestProvider).ToLower(), () => typeof(DependentFilterListForTestProvider));
#endif
					#region SuppressResourceStringsCheckRegion

					l.Add(CodeListChoiceCodeDescriptionList.Codes.Reporttypes, () => typeof(ReportTypeOfGLAccountPairProvider));
					l.Add(CodeListChoiceCodeDescriptionList.Codes.Ordermilestoneeventlist, () => typeof(OrderMilestoneEventTypesPairProvider));
					l.Add(CodeListChoiceCodeDescriptionList.Codes.Milestoneeventtypeslist, () => typeof(MilestoneEventTypesPairProvider));
					l.Add(CodeListChoiceCodeDescriptionList.Codes.Workflowtypelist, () => typeof(WorkflowTypePairProvider));
					l.Add(CodeListChoiceCodeDescriptionList.Codes.Consolacccategory, () => typeof(ConsolAccCategoryCodeDescriptionPairProvider));
					l.Add(CodeListChoiceCodeDescriptionList.Codes.Consoltype, () => typeof(ConsolTypePairListProvider));
					l.Add(CodeListChoiceCodeDescriptionList.Codes.Arcategory, () => typeof(ARCategoryCodeDescriptionPairProvider));
					l.Add(CodeListChoiceCodeDescriptionList.Codes.Apcategory, () => typeof(APCategoryCodeDescriptionPairProvider));
					l.Add(CodeListChoiceCodeDescriptionList.Codes.Language, () => typeof(LanguageCodeDescriptionPairProvider));
					l.Add(CodeListChoiceCodeDescriptionList.Codes.Compliancesubtype, () => typeof(ComplianceSubTypeCodeDescriptionPairProvider));
					l.Add(CodeListChoiceCodeDescriptionList.Codes.Arcreditratinglist, () => typeof(ARCreditRatingListCodeDescriptionPairProvider));
					l.Add(CodeListChoiceCodeDescriptionList.Codes.Glrolluplevel, () => typeof(GLRollupLevelCodeDescriptionPairProvider));
					l.Add(CodeListChoiceCodeDescriptionList.Codes.Storageclass, () => typeof(StorageClassCodeDescriptionPairProvider));
					l.Add(CodeListChoiceCodeDescriptionList.Codes.Containerquality, () => typeof(ContainerQualityCodeDescriptionPairProvider));
					l.Add(CodeListChoiceCodeDescriptionList.Codes.Gllanguage, () => typeof(GLLanguageCodeDescriptionPairProvider));
					l.Add(CodeListChoiceCodeDescriptionList.Codes.Claimstatus, () => typeof(ClaimStatusCodeDescriptionPairProvider));
					l.Add(CodeListChoiceCodeDescriptionList.Codes.Claimtype, () => typeof(ClaimTypeCodeDescriptionPairProvider));
					l.Add(CodeListChoiceCodeDescriptionList.Codes.Claimreason, () => typeof(ClaimReasonCodeDescriptionPairProvider));
					l.Add(CodeListChoiceCodeDescriptionList.Codes.Chargegroup, () => typeof(ChargeCodeGroupPairProvider));
					l.Add(CodeListChoiceCodeDescriptionList.Codes.Organisationnotetypes, () => typeof(OrganisationNoteTypesCodeDescriptionPairProvider));
					l.Add(CodeListChoiceCodeDescriptionList.Codes.Orderheaderstatus, () => typeof(OrderHeaderStatusCodeDescriptionPairProvider));
					l.Add(CodeListChoiceCodeDescriptionList.Codes.Commissionstreams, () => typeof(CommissionStreamCodeDescriptionPairProvider));
					l.Add(CodeListChoiceCodeDescriptionList.Codes.Incoterms, () => typeof(IncotermsPairProvider));
					l.Add(CodeListChoiceCodeDescriptionList.Codes.Exceptionevents, () => typeof(ExceptionEventsCodeDescriptionPairListProvider));
					l.Add(CodeListChoiceCodeDescriptionList.Codes.Onestopcarrier, () => typeof(OneStopCarrierPairProvider));
					l.Add(CodeListChoiceCodeDescriptionList.Codes.Contactgroup, () => typeof(ContactGroupCodeDescriptionPairListProvider));
					l.Add(CodeListChoiceCodeDescriptionList.Codes.Staffholidaystype, () => typeof(HolidaysTypeCodeDescriptionPairProvider));
					l.Add(CodeListChoiceCodeDescriptionList.Codes.Staffholidaysstatus, () => typeof(HolidaysStatusCodeDescriptionPairProvider));
					l.Add(CodeListChoiceCodeDescriptionList.Codes.Nzentrypaymentstatus, () => typeof(NZEntryPaymentStatusCodeDescriptionPairProvider));
					l.Add(CodeListChoiceCodeDescriptionList.Codes.Documenttype, () => typeof(DocumentTypeCodeDescriptionPairProvider));
					l.Add(CodeListChoiceCodeDescriptionList.Codes.Containercleancodes, () => typeof(ContainerCleanCodesCodeDescriptionPairProvider));
					l.Add(CodeListChoiceCodeDescriptionList.Codes.Containerdamagecodes, () => typeof(ContainerDamageCodesCodeDescriptionPairProvider));
					l.Add(CodeListChoiceCodeDescriptionList.Codes.Aragreedpaymentmethods, () => typeof(ARAgreedPaymentMethodCodeDescriptionPairProvider));
					l.Add(CodeListChoiceCodeDescriptionList.Codes.Apagreedpaymentmethods, () => typeof(APAgreedPaymentMethodCodeDescriptionPairProvider));
					l.Add(CodeListChoiceCodeDescriptionList.Codes.Containercodes, () => typeof(RefContainerCodesCodeDescriptionPairProvider));
					l.Add(CodeListChoiceCodeDescriptionList.Codes.OrgRelatedPartyType, () => typeof(OrgRelatedPartyTypesCodeDescriptionPairProvider));
					l.Add(CodeListChoiceCodeDescriptionList.Codes.OrgFreightDirection, () => typeof(OrgFreightDirectionCodeDescriptionPairProvider));
					l.Add(CodeListChoiceCodeDescriptionList.Codes.OrgTransportMode, () => typeof(OrgTransportModeCodeDescriptionPairProvider));

					l.Add(CodeListChoiceCodeDescriptionList.Codes.Declarationmessagetype, ObjectFactory.GetType<Enterprise.Integration.Customs.Shared.IDeclarationMessageTypeCodeDescriptionPairProvider>);
					l.Add(CodeListChoiceCodeDescriptionList.Codes.Declarationmessagesubtype, ObjectFactory.GetType<Enterprise.Integration.Customs.Shared.IDeclarationMessageSubTypeCodeDescriptionPairProvider>);
					l.Add(CodeListChoiceCodeDescriptionList.Codes.Declarationentrytype, ObjectFactory.GetType<Enterprise.Integration.Customs.Shared.IDeclarationEntryTypeCodeDescriptionPairProvider>);
					l.Add(CodeListChoiceCodeDescriptionList.Codes.Declarationpackmode, ObjectFactory.GetType<Enterprise.Integration.Customs.Shared.IDeclarationPackModeCodeDescriptionPairProvider>);
					l.Add(CodeListChoiceCodeDescriptionList.Codes.Declarationpaymentmethod, ObjectFactory.GetType<Enterprise.Integration.Customs.Shared.IDeclarationPaymentMethodCodeDescriptionPairProvider>);
					l.Add(CodeListChoiceCodeDescriptionList.Codes.Declarationtransportmode, ObjectFactory.GetType<Enterprise.Integration.Customs.Shared.IDeclarationTransportModeCodeDescriptionPairProvider>);
					l.Add(CodeListChoiceCodeDescriptionList.Codes.Declarationentrystatus, ObjectFactory.GetType<Enterprise.Integration.Customs.Shared.IDeclarationEntryStatusCodeDescriptionPairProvider>);
					l.Add(CodeListChoiceCodeDescriptionList.Codes.Declarationmessagestatus, ObjectFactory.GetType<Enterprise.Integration.Customs.Shared.IDeclarationMessageStatusCodeDescriptionPairProvider>);
					l.Add(CodeListChoiceCodeDescriptionList.Codes.Declarationservicelevel, ObjectFactory.GetType<Enterprise.Integration.Customs.Shared.IDeclarationServiceLevelCodeDescriptionPairProvider>);
					l.Add(CodeListChoiceCodeDescriptionList.Codes.DeMonthlyClosingDeclarationType, ObjectFactory.GetType<Enterprise.Integration.Customs.DE.IMonthlyClosingDeclarationTypeListProvider>);
					l.Add(CodeListChoiceCodeDescriptionList.Codes.Procedurecodes, ObjectFactory.GetType<Enterprise.Integration.Customs.Shared.IProcedureCodesCodeDescriptionPairProvider>);
					l.Add(CodeListChoiceCodeDescriptionList.Codes.Previousprocedurecodes, ObjectFactory.GetType<Enterprise.Integration.Customs.Shared.IPreviousProcedureCodesCodeDescriptionPairProvider>);
					l.Add(CodeListChoiceCodeDescriptionList.Codes.InbondQPMessageStatus, ObjectFactory.GetType<Enterprise.Integration.Customs.US.InBond.IInBondQPMessageStatusCodeDescriptionPairProvider>);
					l.Add(CodeListChoiceCodeDescriptionList.Codes.InbondWPMessageStatus, ObjectFactory.GetType<Enterprise.Integration.Customs.US.InBond.IInBondWPMessageStatusCodeDescriptionPairProvider>);
					l.Add(CodeListChoiceCodeDescriptionList.Codes.Uspaymenttype, ObjectFactory.GetType<Enterprise.Integration.Customs.US.IBrokerPaymentTypeCodeDescriptionPairProvider>);
					l.Add(CodeListChoiceCodeDescriptionList.Codes.Usliquidationtype, ObjectFactory.GetType<Enterprise.Integration.Customs.US.ILiquidationTypeCodeDescriptionPairProvider>);
					l.Add(CodeListChoiceCodeDescriptionList.Codes.Monthlist, ObjectFactory.GetType<Enterprise.Integration.Customs.US.IMonthListDescriptionPairProvider>);
					l.Add(CodeListChoiceCodeDescriptionList.Codes.Usreleasestatus, ObjectFactory.GetType<Enterprise.Integration.Customs.US.IReleaseStatusCodeDescriptionPairProvider>);
					l.Add(CodeListChoiceCodeDescriptionList.Codes.Usaccrecondicrepancytype, ObjectFactory.GetType<Enterprise.Integration.Customs.US.IStatementLineFilterByOptionProvider>);
					l.Add(CodeListChoiceCodeDescriptionList.Codes.Usimporterbondtype, ObjectFactory.GetType<Enterprise.Integration.Customs.US.IImporterBondTypeCodeDescriptionPairProvider>);
					l.Add(CodeListChoiceCodeDescriptionList.Codes.Useistatuslist, ObjectFactory.GetType<Enterprise.Integration.Customs.US.IEIStatusCodeDescriptionPairProvider>);
					l.Add(CodeListChoiceCodeDescriptionList.Codes.Usensstatuslist, ObjectFactory.GetType<Enterprise.Integration.Customs.US.IENSStatusCodeDescriptionPairProvider>);
					l.Add(CodeListChoiceCodeDescriptionList.Codes.Usfdastatuslist, ObjectFactory.GetType<Enterprise.Integration.Customs.US.IFDAStatusCodeDescriptionPairProvider>);
					l.Add(CodeListChoiceCodeDescriptionList.Codes.Uspaymentstatuslist, ObjectFactory.GetType<Enterprise.Integration.Customs.US.IPaymentStatusCodeDescriptionPairProvider>);
					l.Add(CodeListChoiceCodeDescriptionList.Codes.Usentrymodelist, ObjectFactory.GetType<Enterprise.Integration.Customs.US.IEntryModeCodeDescriptionPairProvider>);
					l.Add(CodeListChoiceCodeDescriptionList.Codes.Ustaxdeferredindlist, ObjectFactory.GetType<Enterprise.Integration.Customs.US.ITaxDeferredCodeDescriptionPairProvider>);
					l.Add(CodeListChoiceCodeDescriptionList.Codes.Usreconissuelist, ObjectFactory.GetType<Enterprise.Integration.Customs.US.IReconIssueCodeDescriptionPairProvider>);
					l.Add(CodeListChoiceCodeDescriptionList.Codes.Usspilist, ObjectFactory.GetType<Enterprise.Integration.Customs.US.ISPICodeDescriptionPairProvider>);
					l.Add(CodeListChoiceCodeDescriptionList.Codes.Usproductclaimcodelist, ObjectFactory.GetType<Enterprise.Integration.Customs.US.IProductClaimCodeDescriptionPairProvider>);
					l.Add(CodeListChoiceCodeDescriptionList.Codes.Usmessagemode, ObjectFactory.GetType<Enterprise.Integration.Customs.US.IJobApplicationCodeProvider>);
					l.Add(CodeListChoiceCodeDescriptionList.Codes.Yesnolist, ObjectFactory.GetType<Enterprise.Integration.Customs.IYesNoListProvider>);
					l.Add(CodeListChoiceCodeDescriptionList.Codes.Usftzadmissiontype, ObjectFactory.GetType<Enterprise.Integration.Customs.US.IFTZAdmissionTypeCodeDescriptionPairProvider>);
					l.Add(CodeListChoiceCodeDescriptionList.Codes.USFDAAgencyProgramCodes, ObjectFactory.GetType<Enterprise.Integration.Customs.US.IUSFDAAgencyProgramCodeDescriptionPairProvider>);
					l.Add(CodeListChoiceCodeDescriptionList.Codes.USFDAProcessingCodes, ObjectFactory.GetType<Enterprise.Integration.Customs.US.IUSFDAProcessingCodeDescriptionPairProvider>);
					l.Add(CodeListChoiceCodeDescriptionList.Codes.USDOTAgencyProgramCodes, ObjectFactory.GetType<Enterprise.Integration.Customs.US.IUSDOTAgencyProgramCodeDescriptionPairProvider>);
					l.Add(CodeListChoiceCodeDescriptionList.Codes.USDOTBoxNumbers, ObjectFactory.GetType<Enterprise.Integration.Customs.US.IUSDOTBoxNumberDescriptionPairProvider>);
					l.Add(CodeListChoiceCodeDescriptionList.Codes.USAPHISProgramType, ObjectFactory.GetType<Enterprise.Integration.Customs.US.IUSAPHISProgramTypeDescriptionPairProvider>);
					l.Add(CodeListChoiceCodeDescriptionList.Codes.USAPHISProcessingCode, ObjectFactory.GetType<Enterprise.Integration.Customs.US.IUSAPHISProcessingCodeDescriptionPairProvider>);
					l.Add(CodeListChoiceCodeDescriptionList.Codes.USFTZPTTMessageStatusList, ObjectFactory.GetType<Enterprise.Integration.Customs.US.IUSFTZPTTMessageStatusListProvider>);
					l.Add(CodeListChoiceCodeDescriptionList.Codes.USFTZAdmissionMessageStatusList, ObjectFactory.GetType<Enterprise.Integration.Customs.US.IUSFTZAdmissionMessageStatusListProvider>);
					l.Add(CodeListChoiceCodeDescriptionList.Codes.USFTZConcurrenceMessageStatusList, ObjectFactory.GetType<Enterprise.Integration.Customs.US.IUSFTZConcurrenceMessageStatusListProvider>);
					l.Add(CodeListChoiceCodeDescriptionList.Codes.USFTZGoodsArrivalMessageStatusList, ObjectFactory.GetType<Enterprise.Integration.Customs.US.IUSFTZGoodsArrivalMessageStatusListProvider>);
					l.Add(CodeListChoiceCodeDescriptionList.Codes.USFTZDeliveryOfGoodsMessageStatusList, ObjectFactory.GetType<Enterprise.Integration.Customs.US.IUSFTZDeliveryOfGoodsMessageStatusListProvider>);
					l.Add(CodeListChoiceCodeDescriptionList.Codes.USITARExemptionNumberList, ObjectFactory.GetType<Enterprise.Integration.Customs.US.IUSITARExemptionNumberDescriptionPairProvider>);

					l.Add(CodeListChoiceCodeDescriptionList.Codes.Isfactionreasoncode, ObjectFactory.GetType<Enterprise.Integration.Customs.US.ISF.IActionReasonCodeCodeDescriptionPairProvider>);
					l.Add(CodeListChoiceCodeDescriptionList.Codes.Isfbondactivitycode, ObjectFactory.GetType<Enterprise.Integration.Customs.US.ISF.IBondActivityCodeCodeDescriptionPairProvider>);
					l.Add(CodeListChoiceCodeDescriptionList.Codes.Isfdispositioncode, ObjectFactory.GetType<Enterprise.Integration.Customs.US.ISF.IDispositionCodeCodeDescriptionPairProvider>);
					l.Add(CodeListChoiceCodeDescriptionList.Codes.Isfbilltype, ObjectFactory.GetType<Enterprise.Integration.Customs.US.ISF.IBillTypeCodeDescriptionPairProvider>);
					l.Add(CodeListChoiceCodeDescriptionList.Codes.Isfentrytype, ObjectFactory.GetType<Enterprise.Integration.Customs.US.ISF.IEntryTypeCodeDescriptionPairProvider>);
					l.Add(CodeListChoiceCodeDescriptionList.Codes.Isfmessagestatus, ObjectFactory.GetType<Enterprise.Integration.Customs.US.ISF.IMessageStatusCodeDescriptionPairProvider>);
					l.Add(CodeListChoiceCodeDescriptionList.Codes.Isfshipmenttype, ObjectFactory.GetType<Enterprise.Integration.Customs.US.ISF.IShipmentTypeCodeDescriptionPairProvider>);
					l.Add(CodeListChoiceCodeDescriptionList.Codes.Isftransportmode, ObjectFactory.GetType<Enterprise.Integration.Customs.US.ISF.ITransportModeCodeDescriptionPairProvider>);

					l.Add(CodeListChoiceCodeDescriptionList.Codes.AsycudaDeclarationTypes, ObjectFactory.GetType<Enterprise.Integration.Customs.AsycudaCustoms.IAsycudaDeclarationTypesProvider>);

					l.Add(CodeListChoiceCodeDescriptionList.Codes.Caacrossserviceoptions, ObjectFactory.GetType<Enterprise.Integration.Customs.CA.IACROSSServiceOptionsCodeDescriptionPairProvider>);
					l.Add(CodeListChoiceCodeDescriptionList.Codes.CATariffTreatmentCodes, ObjectFactory.GetType<Enterprise.Integration.Customs.CA.ICATariffTreatmentCodeDescriptionPairProvider>);
					l.Add(CodeListChoiceCodeDescriptionList.Codes.CARemissionTypes, ObjectFactory.GetType<Enterprise.Integration.Customs.CA.ICARemissionTypeCodeDescriptionPairProvider>);
					l.Add(CodeListChoiceCodeDescriptionList.Codes.CAStateOfOrigin, ObjectFactory.GetType<Enterprise.Integration.Customs.CA.ICAStateOfOriginCodeDescriptionPairProvider>);
					l.Add(CodeListChoiceCodeDescriptionList.Codes.CAExportMessageStatusList, ObjectFactory.GetType<Enterprise.Integration.Customs.CA.ICAExportMessageStatusListProvider>);
					l.Add(CodeListChoiceCodeDescriptionList.Codes.CABondTypeList, ObjectFactory.GetType<Enterprise.Integration.Customs.CA.ICABondTypeCodeDescriptionPairProvider>);

					l.Add(CodeListChoiceCodeDescriptionList.Codes.EUDeclarationEntrySubstyle, ObjectFactory.GetType<Enterprise.Integration.Customs.EU.IEntrySubStyleDescriptionPairProvider>);
					l.Add(CodeListChoiceCodeDescriptionList.Codes.EUTemporaryStorageRegisterStatusList, ObjectFactory.GetType<Enterprise.Integration.Customs.EU.ICusTempStorageRegHeaderStatusListProvider>);
					l.Add(CodeListChoiceCodeDescriptionList.Codes.EUTemporaryStorageRegisterAppCodeList, ObjectFactory.GetType<Enterprise.Integration.Customs.EU.ICusTempStorageRegHeaderAppCodesListProvider>);
					l.Add(CodeListChoiceCodeDescriptionList.Codes.EUISTCustomsProfiles, ObjectFactory.GetType<Enterprise.Integration.Customs.EU.IISTCustomsProfileCodeDescriptionPairProvider>);
					l.Add(CodeListChoiceCodeDescriptionList.Codes.EUISTLocationOfGoods, ObjectFactory.GetType<Enterprise.Integration.Customs.EU.IISTLocationOfGoodsCodeDescriptionPairProvider>);
					l.Add(CodeListChoiceCodeDescriptionList.Codes.NCTSGuarantees, ObjectFactory.GetType<Enterprise.Integration.Customs.EU.NCTS.IGuaranteeNumbersCodeDescriptionPairProvider>);
					l.Add(CodeListChoiceCodeDescriptionList.Codes.NCTSDeclarationTypes, ObjectFactory.GetType<Enterprise.Integration.Customs.EU.NCTS.IDeclarationTypesCodeDescriptionPairProvider>);

					l.Add(CodeListChoiceCodeDescriptionList.Codes.Zafrncustomsoffices, ObjectFactory.GetType<Enterprise.Integration.Customs.ZA.IFRNCustomsOfficesProvider>);
					l.Add(CodeListChoiceCodeDescriptionList.Codes.Zacustomsoffices, ObjectFactory.GetType<Enterprise.Integration.Customs.ZA.ICustomsOfficesProvider>);
					l.Add(CodeListChoiceCodeDescriptionList.Codes.Zareleaseprintindicators, ObjectFactory.GetType<Enterprise.Integration.Customs.ZA.IReleasePrintIndicatorProvider>);
					l.Add(CodeListChoiceCodeDescriptionList.Codes.Zaprovisionalpaymenttypes, ObjectFactory.GetType<Enterprise.Integration.Customs.ZA.IProvisionalPaymentTypeProvider>);
					l.Add(CodeListChoiceCodeDescriptionList.Codes.ZaShipmentType, ObjectFactory.GetType<Enterprise.Integration.Customs.ZA.IShipmentTypeProvider>);

					l.Add(CodeListChoiceCodeDescriptionList.Codes.FrCustomsOffices, ObjectFactory.GetType<Enterprise.Integration.Customs.FR.ICustomsOfficesProvider>);
					l.Add(CodeListChoiceCodeDescriptionList.Codes.FrProcedureCodes, ObjectFactory.GetType<Enterprise.Integration.Customs.FR.IProcedureCodesCodeDescriptionPairProvider>);
					l.Add(CodeListChoiceCodeDescriptionList.Codes.FrCustomsProfiles, ObjectFactory.GetType<Enterprise.Integration.Customs.FR.ICustomsProfileCodeDescriptionPairProvider>);
					l.Add(CodeListChoiceCodeDescriptionList.Codes.IsWrittenoffd48, ObjectFactory.GetType<Enterprise.Integration.Customs.FR.IISWrittenoffD48CodeDescriptionPairProvider>);

					l.Add(CodeListChoiceCodeDescriptionList.Codes.ITEntryInstructionStyleList, ObjectFactory.GetType<Enterprise.Integration.Customs.IT.IEntryInstructionStyleListProvider>);
					l.Add(CodeListChoiceCodeDescriptionList.Codes.ItCustomsOffices, ObjectFactory.GetType<Enterprise.Integration.Customs.IT.ICustomsOfficesProvider>);
					l.Add(CodeListChoiceCodeDescriptionList.Codes.ItMethodsOfPayment, ObjectFactory.GetType<Enterprise.Integration.Customs.IT.IMethodOfPaymentListProvider>);
					l.Add(CodeListChoiceCodeDescriptionList.Codes.ItDefermentAccountNumber, ObjectFactory.GetType<Enterprise.Integration.Customs.IT.IDefermentAccountNumberProvider>);

					l.Add(CodeListChoiceCodeDescriptionList.Codes.TWDeclarationTypeList, ObjectFactory.GetType<Enterprise.Integration.Customs.TW.ITWDeclarationTypeListCodeDescriptionPairProvider>);
					l.Add(CodeListChoiceCodeDescriptionList.Codes.TWOfficeOfReceiptList, ObjectFactory.GetType<Enterprise.Integration.Customs.TW.ITWOfficeOfReceiptListDescriptionPairProvider>);

					l.Add(CodeListChoiceCodeDescriptionList.Codes.PackageDamagedReason, ObjectFactory.GetType<Enterprise.Integration.Packing.IPackageDamagedReasonsCodeDescriptionPairProvider>);

					l.Add(CodeListChoiceCodeDescriptionList.Codes.Housebilltype, () => typeof(HouseBillTypePairProvider));
					l.Add(CodeListChoiceCodeDescriptionList.Codes.Shipmenttransportmode, () => typeof(ShipmentTransportModePairProvider));
					l.Add(CodeListChoiceCodeDescriptionList.Codes.Freightcontainermode, () => typeof(FreightContainerModePairProvider));
					l.Add(CodeListChoiceCodeDescriptionList.Codes.Pickupmethod, () => typeof(WhsPickMethodCodeDescriptionPairProvider));
					l.Add(CodeListChoiceCodeDescriptionList.Codes.Locationstatus, () => typeof(WhsLocationStatusCodeDescriptionPairListProvider));
					l.Add(CodeListChoiceCodeDescriptionList.Codes.Areatype, () => typeof(WhsAreaTypeCodeDescriptionPairListProvider));
					l.Add(CodeListChoiceCodeDescriptionList.Codes.Abccategory, () => typeof(ABCCategoryCodeDescriptionPairProvider));

					l.Add(CodeListChoiceCodeDescriptionList.Codes.Periodofactivitytype, () => typeof(PeriodOfActivityTypeCodeDescriptionPairProvider));
					l.Add(CodeListChoiceCodeDescriptionList.Codes.Industryverticaltype, () => typeof(IndustryVerticalTypeCodeDescriptionPairProvider));
					l.Add(CodeListChoiceCodeDescriptionList.Codes.Certaintylikertitem, () => typeof(CertaintyLikertItemListProvider));
					l.Add(CodeListChoiceCodeDescriptionList.Codes.Salescategory, () => typeof(SalesCategoryCodeDescriptionPairProvider));
					l.Add(CodeListChoiceCodeDescriptionList.Codes.Salescalltype, () => typeof(CommunicationTypeCodeDescriptionPairListProvider));
					l.Add(CodeListChoiceCodeDescriptionList.Codes.Enquirytype, () => typeof(EnquiryTypeCodeDescriptionPairProvider));
					l.Add(CodeListChoiceCodeDescriptionList.Codes.Enquirytypeactive, () => typeof(EnquiryTypeActiveCodeDescriptionPairProvider));
					l.Add(CodeListChoiceCodeDescriptionList.Codes.Closereason, () => typeof(EnquiryCloseReasonCodeDescriptionPairProvider));
					l.Add(CodeListChoiceCodeDescriptionList.Codes.Enquiryleadinterest, () => typeof(EnquiryLeadInterestCodeDescriptionPairProvider));
					l.Add(CodeListChoiceCodeDescriptionList.Codes.Communicationstatus, () => typeof(CommunicationStatusCodeDescriptionPairListProvider));
					l.Add(CodeListChoiceCodeDescriptionList.Codes.Salesleadtype, () => typeof(SalesLeadTypeCodeDescriptionPairProvider));
					l.Add(CodeListChoiceCodeDescriptionList.Codes.Salesterritory, () => typeof(SalesTerritoryCodeDescriptionPairProvider));
					l.Add(CodeListChoiceCodeDescriptionList.Codes.Paymentmethod, () => typeof(PaymentMethodCodeDescriptionPairProvider));
					l.Add(CodeListChoiceCodeDescriptionList.Codes.Cartagedropmode, () => typeof(CartageDropModeCodeDescriptionPairProvider));
					l.Add(CodeListChoiceCodeDescriptionList.Codes.Localtransportjobtype, () => typeof(LocalTransportJobTypeCodeDescriptionPairProvider));
					l.Add(CodeListChoiceCodeDescriptionList.Codes.Category, () => typeof(CategoryCodeDescriptionPairListProvider));

					l.Add(CodeListChoiceCodeDescriptionList.Codes.Opportunitystatus, () => typeof(OpportunityStatusCodeDescriptionPairProvider));
					l.Add(CodeListChoiceCodeDescriptionList.Codes.Opportunitytype, () => typeof(OpportunityTypeCodeDescriptionPairProvider));
					l.Add(CodeListChoiceCodeDescriptionList.Codes.Opportunitystage, () => typeof(OpportunityStageCodeDescriptionPairProvider));
					l.Add(CodeListChoiceCodeDescriptionList.Codes.Opportunityoutcome, () => typeof(OpportunityOutcomeCodeDescriptionPairProvider));
					l.Add(CodeListChoiceCodeDescriptionList.Codes.Opportunityproducttype, () => typeof(OpportunityProductTypeCodeDescriptionPairProvider));
					l.Add(CodeListChoiceCodeDescriptionList.Codes.Opportunitysource, () => typeof(OpportunitySourceCodeDescriptionPairProvider));
					l.Add(CodeListChoiceCodeDescriptionList.Codes.Closeopportunityreason, () => typeof(ClosedOpportunityReasonCodeDescriptionPairProvider));
					l.Add(CodeListChoiceCodeDescriptionList.Codes.Certificatetype, ObjectFactory.GetType<Enterprise.Recruiter.Integration.ICertificateTypeCodeDescriptionPairListProvider>);

					l.Add(CodeListChoiceCodeDescriptionList.Codes.Clientsize, () => typeof(ClientSizeCodeDescriptionPairProvider));
					l.Add(CodeListChoiceCodeDescriptionList.Codes.Staffmemberassignmentroles, () => typeof(StaffAssignmentRolesCodeDescriptionPairProvider));

					l.Add(CodeListChoiceCodeDescriptionList.Codes.Freightrateclass, () => typeof(FreightRateClassCodeDescriptionPairProvider));
					l.Add(CodeListChoiceCodeDescriptionList.Codes.Airdensity, () => typeof(AirDensityCodeDescriptionPairProvider));

					l.Add(CodeListChoiceCodeDescriptionList.Codes.Quotecancellationreason, () => typeof(QuoteCancellationReasonCodeDescriptionPairProvider));

					l.Add(CodeListChoiceCodeDescriptionList.Codes.Jobheaderstatus, () => typeof(JobHeadreStatusCodeDescriptionGroupPairProvider));
					l.Add(CodeListChoiceCodeDescriptionList.Codes.Jobprofitlossreason, () => typeof(JobProfitLossReasonCodeDescriptionPairProvider));
					l.Add(CodeListChoiceCodeDescriptionList.Codes.Presentationcategory, () => typeof(GLPresentationCategoryCodeDescriptionPairProvider));
					l.Add(CodeListChoiceCodeDescriptionList.Codes.Presentationcategorygroup, () => typeof(GLPresentationCategoryGroupCodeDescriptionPairProvider));
					l.Add(CodeListChoiceCodeDescriptionList.Codes.Cashflowcategory, () => typeof(CashFlowCategoryCodeDescriptionPairProvider));
					l.Add(CodeListChoiceCodeDescriptionList.Codes.Translatelanguage, () => typeof(TranslateLanguageCodeDescriptionPairProvider));
					l.Add(CodeListChoiceCodeDescriptionList.Codes.Branchmanagementcode, () => typeof(BranchManagementCodeDescriptionPairProvider));
					l.Add(CodeListChoiceCodeDescriptionList.Codes.TaxConfigurations, () => typeof(TaxConfigurationCodeDescriptionPairListProvider));
					l.Add(CodeListChoiceCodeDescriptionList.Codes.ARTaxConfigurations, () => typeof(ARTaxConfigurationCodeDescriptionPairListProvider));
					l.Add(CodeListChoiceCodeDescriptionList.Codes.ARTaxConfigurationOrgRates, () => typeof(ARTaxConfigurationWithOrganizationRatesCodeDescriptionPairListProvider));
					l.Add(CodeListChoiceCodeDescriptionList.Codes.APTaxConfigurations, () => typeof(APTaxConfigurationCodeDescriptionPairListProvider));

					l.Add(CodeListChoiceCodeDescriptionList.Codes.Additionalreferencenumbertype, ObjectFactory.GetType<Enterprise.Integration.Customs.Shared.IAdditionalReferenceNumberTypesCodeDescriptionPairProvider>);
					l.Add(CodeListChoiceCodeDescriptionList.Codes.Whsdockettypeandsubtype, ObjectFactory.GetType<Enterprise.Warehouse.Integration.IWhsWarehouseJobTypeWithSubTypeCodeDescriptionPairProvider>);
					l.Add(CodeListChoiceCodeDescriptionList.Codes.Whsadjustmentreason, ObjectFactory.GetType<Enterprise.Warehouse.Integration.IWhsAdjustmentReasonCodeDescriptionPairProvider>);

					l.Add(CodeListChoiceCodeDescriptionList.Codes.Salesrelationtype, () => typeof(SalesRelationTypeCodeDescriptionPairListProvider));

					l.Add(CodeListChoiceCodeDescriptionList.Codes.Subaccounttype, () => typeof(SubAccountTypeCodeDescriptionPairProvider));
					l.Add(CodeListChoiceCodeDescriptionList.Codes.Goodsreceivedstatus, () => typeof(PurchaseOrderGoodsReceivedNotesCodeDescriptionPairProvider));
					l.Add(CodeListChoiceCodeDescriptionList.Codes.Organisationtype, () => typeof(OrganisationTypeCodeDescriptionPairProvider));
					l.Add(CodeListChoiceCodeDescriptionList.Codes.Parentjobtype, () => typeof(ParentJobTypeCodeDescriptionPairProvider));

					l.Add(CodeListChoiceCodeDescriptionList.Codes.LicenseTypes, () => typeof(LicenseTypeCodeDescriptionPairListProvider));
					l.Add(CodeListChoiceCodeDescriptionList.Codes.GroupCategories, () => typeof(GroupCategoryCodeDescriptionPairListProvider));

					l.Add(CodeListChoiceCodeDescriptionList.Codes.KRTransportMode, ObjectFactory.GetType<Enterprise.Integration.Customs.KR.IKRTransportTypeCodeDescriptionPairProvider>);
					l.Add(CodeListChoiceCodeDescriptionList.Codes.KRTransactionType, ObjectFactory.GetType<Enterprise.Integration.Customs.KR.IKRTransactionTypeCodeDescriptionPairProvider>);
					l.Add(CodeListChoiceCodeDescriptionList.Codes.KRExportType, ObjectFactory.GetType<Enterprise.Integration.Customs.KR.IKRExportTypeCodeDescriptionPairProvider>);
					l.Add(CodeListChoiceCodeDescriptionList.Codes.KRProcedureType, ObjectFactory.GetType<Enterprise.Integration.Customs.KR.IKRDeclarationProcedureTypeCodeDescriptionPairProvider>);
					l.Add(CodeListChoiceCodeDescriptionList.Codes.KRPaymentMethod, ObjectFactory.GetType<Enterprise.Integration.Customs.KR.IKRInvoicePaymentTermCodeDescriptionPairProvider>);
					l.Add(CodeListChoiceCodeDescriptionList.Codes.KRLocalExportMessageType, ObjectFactory.GetType<Enterprise.Integration.Customs.KR.IKRLocalExportMessageTypeCodeDescriptionPairProvider>);
					l.Add(CodeListChoiceCodeDescriptionList.Codes.KRLocalExportTransaction, ObjectFactory.GetType<Enterprise.Integration.Customs.KR.IKRLocalExportTransactionNatureCodeDescriptionPairProvider>);
					l.Add(CodeListChoiceCodeDescriptionList.Codes.KRLocalExportGoodsType, ObjectFactory.GetType<Enterprise.Integration.Customs.KR.IKRLocalExportGoodsTypeCodeDescriptionPairProvider>);
					l.Add(CodeListChoiceCodeDescriptionList.Codes.KRLocalExportDrawbackApplicantType, ObjectFactory.GetType<Enterprise.Integration.Customs.KR.IKRLocalExportDrawbackApplicantTypeCodeDescriptionPairProvider>);

					l.Add(CodeListChoiceCodeDescriptionList.Codes.Supplytype, () => typeof(SupplyTypeCodeDescriptionPairListProvider));

					#endregion

					if (ClientHookLoader.Instance.ClientHook != null && ClientHookLoader.Instance.ClientHook.DocumentEngineCodeDescriptionPairProviders != null)
					{
						foreach (KeyValuePair<string, Type> pair in ClientHookLoader.Instance.ClientHook.DocumentEngineCodeDescriptionPairProviders)
						{
							l.Add(pair.Key, () => pair.Value);
						}
					}
				}

				return lookup.Value;
			}
		}

		public static DynamicKeyedCodePairListProvider<ZGuid> GetDynamicGUIDKeyedCodeDescriptionPairList(string codeDescriptionPairName)
		{
			if (!DynamicZGuidKeyedCodePairListProviderCache.ContainsKey(codeDescriptionPairName))
			{
				AddProviderToCache(codeDescriptionPairName);
			}
			return DynamicZGuidKeyedCodePairListProviderCache[codeDescriptionPairName];
		}

		static Dictionary<string, DynamicKeyedCodePairListProvider<ZGuid>> DynamicZGuidKeyedCodePairListProviderCache
		{
			get { return dynamicZGuidKeyedCodePairListProviderCache ?? (dynamicZGuidKeyedCodePairListProviderCache = new Dictionary<string, DynamicKeyedCodePairListProvider<ZGuid>>()); }
		}
		[ThreadStatic]
		static Dictionary<string, DynamicKeyedCodePairListProvider<ZGuid>> dynamicZGuidKeyedCodePairListProviderCache;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded constant")]
		static void AddProviderToCache(string codeDescriptionPairName)
		{
			switch (codeDescriptionPairName.ToLower())
			{
				case "dynamicorderheaderstatus":
					DynamicZGuidKeyedCodePairListProviderCache.Add(codeDescriptionPairName, new DynamicOrderHeaderStatusCodeDescriptionPairProvider(new BusinessObjectFactory()));
					break;
				case "dynamicorderlinestatus":
					DynamicZGuidKeyedCodePairListProviderCache.Add(codeDescriptionPairName, new DynamicOrderLineStatusCodeDescriptionPairProvider(new BusinessObjectFactory()));
					break;
			}
		}

		static readonly Overridable<Dictionary<string, TypeGetter>> lookup = new Overridable<Dictionary<string, TypeGetter>>();
	}

	#region Test
#if DEBUG

	public class DependentFilterListForTestProvider : IDependenceCodeDescriptionPairListProvider
	{
		public ZArchitecture.Core.ReadOnlyCodeDescriptionPairList GetCodeDescriptionPairList() => new ZArchitecture.Core.CodeDescriptionPairList();

		public ZArchitecture.Core.ReadOnlyCodeDescriptionPairList GetDependenceCodeDescriptionPairList(string value)
		{
			var result = new ZArchitecture.Core.CodeDescriptionPairList();

			if (value == "I1")
			{
				result.AddPair("I1_1");
				result.AddPair("I1_2");
			}
			else if (value == "I2")
			{
				result.AddPair("I2_1");
				result.AddPair("I2_2");
			}

			return result;
		}
	}

#endif
	#endregion
}
