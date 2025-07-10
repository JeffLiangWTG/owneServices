using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.DutyCalculator;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Business.NctsAndDeclarationIntegration;
using Enterprise.Customs.FR.Business.CusTempStorage;
using Enterprise.Customs.FR.Business.GDM;
using Enterprise.Customs.FR.Business.MessagesWrappers.Common;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Schema;
using CustomsChargeTypeList = Enterprise.Customs.Common.CustomsChargeTypeList;

namespace Enterprise.Customs.FR.Business.Declaration
{
	[UniversalCopyAddInfo(JobComInvoiceLineSchema.Constants.Prefix, EU.Business.AddInfo.Schema.Prefix)]
	public class JobComInvoiceLine : EU.Business.Declaration.JobComInvoiceLine,
		Integration.Customs.FR.IJobComInvoiceLine, IDeltaSupporter
	{
		public JobComInvoiceLine(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : EU.Business.Declaration.JobComInvoiceLine.Schema
		{
			public const string JI_TariffBypassCode = "JI_TariffBypassCode";
			public const int JI_TariffBypassCodeMaxLength = 1;
			public const string JI_TariffBypassReason = "JI_TariffBypassReason";
			public const int JI_TariffBypassReasonMaxLength = 255;
			public const string JI_Calc_InvoicedDocumentaryAmountValue = "JI_Calc_InvoicedDocumentaryAmountValue";
		}

		protected override Customs.Business.JobComInvoiceLineValidation GetNewValidation() => Declaration?.ApplicationExtender.GetJobComInvoiceLineValidation(this) ?? new DeltaGJobComInvoiceLineValidation(this);

		protected override Customs.Business.JobComInvoiceLineLookups GetNewLookups() => Declaration?.ApplicationExtender.GetJobComInvoiceLineLookups(this) ?? new JobComInvoiceLineLookups(this);

		protected override Customs.Business.InvoiceLinePackageValidation GetNewLinkPackValidationCore(BaseCusLinkPackage linkPackage)
		{
			return new InvoiceLinePackageValidation(linkPackage, this);
		}

		public new JobDeclaration Declaration => (JobDeclaration)base.Declaration;

		public new JobComInvoiceHeader InvoiceHeader => (JobComInvoiceHeader)base.InvoiceHeader;

		public new JobComInvoiceLineLookups Lookups => (JobComInvoiceLineLookups)base.Lookups;

		public new JobComInvoiceLineValidation Validation => (JobComInvoiceLineValidation)base.Validation;

		public new InvoiceLineChargeCollection<InvoiceLineCharge> Charges => (InvoiceLineChargeCollection<InvoiceLineCharge>)base.Charges;
		protected override IJobComInvChargeCollection<BaseInvoiceLineCharge> CreateNewInvoiceLineChargeCollection() => new InvoiceLineChargeCollection<InvoiceLineCharge>(this);

		protected override IDictionary<ZString, Type> GetCusSupportingInfoTypes()
		{
			var result = base.GetCusSupportingInfoTypes();
			result[Enterprise.Customs.Common.EU.CusSupportingInfoTypeList.Codes.AdditionalInfo] = typeof(AdditionalInfo);
			result[Enterprise.Customs.Common.EU.CusSupportingInfoTypeList.Codes.SupportingDocument] = typeof(SupportingDocument);
			result[Enterprise.Customs.Common.EU.CusSupportingInfoTypeList.Codes.PreviousDocument] = typeof(PreviousDocument);
			return result;
		}

		#region AdditionalInfos

		public new AdditionalInfoCollection AdditionalInfos => (AdditionalInfoCollection)base.AdditionalInfos;

		protected override EU.Business.Declaration.MultiLineAddInfos.AdditionalInfoCollection CreateNewAdditionalInfoCollection() => new AdditionalInfoCollection(this);

		#endregion

		#region SupportingDocuments

		public new SupportingDocumentCollection SupportingDocuments => (SupportingDocumentCollection)base.SupportingDocuments;

		protected override EU.Business.Declaration.MultiLineAddInfos.SupportingDocumentCollection CreateNewSupportingDocumentCollection() => new SupportingDocumentCollection(this);

		#endregion

		#region PreviousDocuments

		public new PreviousDocumentCollection PreviousDocuments => (PreviousDocumentCollection)base.PreviousDocuments;

		protected override EU.Business.Declaration.MultiLineAddInfos.PreviousDocumentCollection CreateNewPreviousDocumentCollection() => new PreviousDocumentCollection(this);

		public PreviousInbondMovement PreviousInbondMovement
		{
			get
			{
				var prevInbondMovementCodes = new[] { "21", "22", "23", "51", "53", "71", "76", "77", "78" };
				if (prevInbondMovementCodes.Any(_ => _ == JI_Calc_PreviousProcedure))
				{
					var previousInbondMovementDocument = PreviousDocuments.Cast<PreviousDocument>().FirstOrDefault(pd => pd.CSI_Code == PreviousDocumentCodeList.Codes.IM && pd.CSI_SubType == "Z");
					if (previousInbondMovementDocument != null)
					{
						var reference = previousInbondMovementDocument.CSI_ReferenceNumber;
						if (reference.Equals(previousInbondMovement?.Number))
						{
							return previousInbondMovement;
						}
						else
						{
							previousInbondMovement = null;
							var possibleEntryTypes = new[]
							{
								CusEntryHeader.Schema.FallbackEntryType, EU.Business.MessageTypeList.Codes.Import, EU.Business.MessageTypeList.Codes.Export
							};
							var query = new ZQuery(CusEntryNumSchema.CE_EntryType, possibleEntryTypes);
							query.AddToFilter(JoinCondition.And, CusEntryNumSchema.CE_RN_NKCountryCode, CustomsCountryCode);
							query.AddToFilter(JoinCondition.And, CusEntryNumSchema.CE_EntryNum, reference);
							var entryNumber = Factory.LoadTop1<CusEntryNumber>(query);
							if (entryNumber != null && entryNumber.Parent is CusEntryHeader)
							{
								previousInbondMovement = new PreviousInbondMovement(entryNumber);
								return previousInbondMovement;
							}
						}
					}
				}
				return null;
			}
		}
		PreviousInbondMovement previousInbondMovement;

		#endregion

		#region BaseVATableValue

		public ZDecimal BaseVATableValue
		{
			get
			{
				var result = ZDecimal.Zero;
				if (InvoiceHeader != null && JI_Calc_ValueForVat > ZDecimal.Zero)
				{
					result = JI_Calc_ValueForVat + JI_Calc_DutyAmountIncludingWHEstimate;
				}
				return result;
			}
		}

		#endregion

		public override ZDateTime EffectiveAssessmentDate
		{
			get
			{
				var result = EntryInstruction?.CEI_DateForDuty ?? base.EffectiveAssessmentDate;
				return result.IsValid ? result : ZDateTime.Today;
			}
		}

		public override ZGuid JI_JZ
		{
			get => base.JI_JZ;
			set
			{
				var oldValue = JI_JZ;
				base.JI_JZ = value;
				if (oldValue != JI_JZ && !IsCopying)
				{
					InvoiceHeader?.MarkAsNeedingValidation();
					Declaration?.MarkAsNeedingValidation();
					base.ResetCustomsUnitDefaultingStrategy();
				}
			}
		}

		public override ZString JI_Procedure
		{
			get => base.JI_Procedure;
			set
			{
				var oldValue = JI_Procedure;
				base.JI_Procedure = value;
				if (oldValue != JI_Procedure && !IsCopying)
				{
					EntryInstruction?.MarkAsNeedingValidation();
				}
			}
		}
		[ResourceStringData("9C444F5A-0085-4177-BABB-940642C46D12", Caption = "Preferential Country", MultipleKey = JobDeclaration.CaptionKeyUCC)]
		public override ZString ZG_CountryOfSupply { get => base.ZG_CountryOfSupply; set => base.ZG_CountryOfSupply = value; }

		protected override ZString EffectiveCountryOfOriginCore
		{
			get
			{
				var result = Declaration?.ApplicationExtender.GetEffectiveCountryOfOrigin(this) ?? ZString.Empty;
				return result.IsEmpty ? base.EffectiveCountryOfOriginCore : result;
			}
		}

		protected override EU.Business.Declaration.AddInfoJobComInvoiceLine GetNewAddInfo()
		{
			return new AddInfoJobComInvoiceLine(JI_AddInfoInfo);
		}

		public new AddInfoJobComInvoiceLineLookups AddInfoLookups => ((AddInfoJobComInvoiceLine)AddInfo).Lookups;

		public new AddInfoJobComInvoiceLineValidation AddInfoValidation => ((AddInfoJobComInvoiceLine)AddInfo).Validation;

		public new CusEntryInstruction EntryInstruction => (CusEntryInstruction)base.EntryInstruction;

		protected override void SetDefaultTaxOrFeeCode()
		{
			if (IsImport)
			{
				base.SetDefaultTaxOrFeeCode();
			}
		}

		#region Tariff Bypass
		[List(nameof(AddInfoLookups) + "." + nameof(AddInfoJobComInvoiceLineLookups.TariffBypassCodeList))]
		[UniversalCopyAddInfoPropertyMapping(EU.Business.AutoEUAddInfo.Schema.ZG_BypassCode)]
		public ZString JI_TariffBypassCode
		{
			get { return AddInfo.ZG_BypassCode; }
			set
			{
				if (JI_TariffBypassCode != value)
				{
					AddInfo.ZG_BypassCode = value;
					JI_TariffBypassCodeInfo.RefreshBinding();

					if (!JI_TariffBypassCode.IsEmpty)
					{
						if (JI_TariffBypassCode != TariffBypassCodeList.Codes.TariffBypass_E)
						{
							JI_TariffBypassReason = new TariffBypassCodeList().GetDescriptionFromCode(JI_TariffBypassCode);
						}
						else
						{
							JI_TariffBypassReason = ZString.Empty;
						}
					}
				}
			}
		}
		public ZPropertyInfo JI_TariffBypassCodeInfo => GetWrappedZPropertyInfo(Schema.JI_TariffBypassCode, x => AddInfo.ZG_BypassCodeInfo);

		[ReadOnlyMember(nameof(IsTariffByPassReasonReadOnly))]
		[UniversalCopyAddInfoPropertyMapping(EU.Business.AutoEUAddInfo.Schema.ZG_BypassReason)]
		public ZString JI_TariffBypassReason
		{
			get { return AddInfo.ZG_BypassReason; }
			set
			{
				if (JI_TariffBypassReason != value)
				{
					AddInfo.ZG_BypassReason = value;
					JI_TariffBypassReasonInfo.RefreshBinding();
				}
			}
		}
		public ZPropertyInfo JI_TariffBypassReasonInfo => GetWrappedZPropertyInfo(Schema.JI_TariffBypassReason, x => AddInfo.ZG_BypassReasonInfo);

		public bool IsTariffByPassReasonReadOnly => !JI_TariffBypassCode.Equals(FRConstants.TariffBypassCodes.ReasonEnabledCode);
		#endregion

		#region Tax Lines
		protected override EU.Business.Declaration.JobComInvoiceLineTaxCollection CreateTaxCollection()
		{
			return new JobComInvoiceLineTaxCollection(this);
		}
		public new JobComInvoiceLineTaxCollection Taxes => (JobComInvoiceLineTaxCollection)base.Taxes;
		#endregion

		#region InvoicedDocumentaryAmount
		public Money JI_Calc_InvoicedDocumentaryAmount
		{
			get
			{
				Money result = Money.Empty;
				var invoiceCurrency = LinePriceRefCurrency;

				if (CurrencyConverter != null && invoiceCurrency != null)
				{
					result = CurrencyConverter.Add(JI_LinePriceMoney, JI_Calc_ADDAmountNotInLine);
					result = CurrencyConverter.Subtract(result, JI_Calc_CUTAmountInLine);
					result = CurrencyConverter.ConvertRounded(result, invoiceCurrency);
				}
				return result;
			}
		}

		Money JI_Calc_ADDAmountNotInLine
		{
			get
			{
				return Charges.Cast<JobComInvCharge>().Concat(ApportionedCharges).Where(x => x.J7_ChargeType == CustomsChargeTypeList.Codes.AdditionCharge && !x.J7_IsIncludedInITOT).Aggregate(Money.Empty, (m, x) => CurrencyConverter.Add(m, x.Money));
			}
		}

		Money JI_Calc_CUTAmountInLine
		{
			get
			{
				return Charges.Cast<JobComInvCharge>().Concat(ApportionedCharges).Where(x => x.J7_ChargeType == FRCustomsChargeTypeList.Codes.Cut && x.J7_IsIncludedInITOT).Aggregate(Money.Empty, (m, x) => CurrencyConverter.Add(m, x.Money));
			}
		}

		[ResourceStringData("Enterprise.Customs.FR.Business.Declaration.JobComInvoiceLine|JI_Calc_InvoicedDocumentaryAmount", Caption = "Invoiced value")]
		public ZDecimal JI_Calc_InvoicedDocumentaryAmountValue => JI_Calc_InvoicedDocumentaryAmount.Amount;

		public ZPropertyInfo JI_Calc_InvoicedDocumentaryAmountValueInfo => GetZPropertyInfo(Schema.JI_Calc_InvoicedDocumentaryAmountValue);

		public ZGuid JI_Calc_InvoicedDocumentaryAmountCurrency => JI_Calc_InvoicedDocumentaryAmount.Currency.PK;

		#endregion

		public ZBool IsDeltaDStepOneSentOK => ((CusEntryLine)CusEntryLine)?.Header?.IsDeltaDStepOneSentOK ?? false;

		public ZBool IsDeltaDStepTwoSentOK => ((CusEntryLine)CusEntryLine)?.Header?.IsDeltaDStepTwoSentOK ?? false;

		protected override ZBool IsSupportEmptyPackType(BasePackage package)
		{
			var entryLine = (CusEntryLine)CusEntryLine;
			bool result = true;
			if (entryLine == null)
			{
				result = base.IsSupportEmptyPackType(package);
			}
			else if (entryLine.CL_LineNumber == 1)
			{
				if (base.IsSupportEmptyPackType(package) || entryLine.HasNonEmptyPackage)
				{
					result = true;
				}
				else
				{
					result = false;
				}
			}

			return result;
		}

		protected override BaseCusLinkPackageCollection PackagesForInvoiceLinesCore() => new InvoiceLineCusLinkPackageCollection(this);

		public ZString SelectedPackType => PackagesPivot.Cast<InvoiceLinePackagePivot>().FirstOrDefault()?.Package?.CW_PackType ?? ZString.Empty;

		public ZString DeltaMode => Declaration?.JE_DeltaMode ?? ZString.Empty;

		protected override ZString CustomsCountryCodeCore => Core.Constants.CountryCodes.France;

		protected override Type TypeOfPartUsedCore => typeof(MasterFiles.OrgSupplierPart);

		protected override EU.Business.IGuidedDecisionMakingTarget GetGuidedDecisionMakingSingleInvoiceLineTargetCore() => new GuidedDecisionMakingSingleInvoiceLineTarget(this);
		protected override EU.Business.IGuidedDecisionMakingSource GetGuidedDecisionMakingSingleInvoiceLineSourceCore() => new GuidedDecisionMakingSingleInvoiceLineSource(this);

		protected override EU.Business.IGuidedDecisionMakingSource GetGuidedDecisionMakingMultiInvoiceLinesSourceCore() => new GuidedDecisionMakingMultiInvoiceLinesSource(this);
		protected override EU.Business.IGuidedDecisionMakingTarget GetGuidedDecisionMakingMultiInvoiceLinesTargetCore(List<EU.Business.Declaration.JobComInvoiceLine> invoiceLines) => new GuidedDecisionMakingMultiInvoiceLinesTarget(invoiceLines);

		protected override EU.Business.GuidedDecisionMakingBasic GetGuidedDecisionMakingBasicCore(EU.Business.IGuidedDecisionMakingSource source) => new GuidedDecisionMakingBasic((IGuidedDecisionMakingSource)source, Factory);

		public IEnumerable<ZString> FRAdditionalCodes
		{
			get
			{
				List<ZString> result = SupplementaryCodes.Select(x => x.CY_Code).Where(y => IsFRAdditionalCode(y)).ToList();
				var vatCana = Declaration.ZG_VATCANACode;
				if (!vatCana.IsEmpty)
				{
					result.Add(vatCana);
				}
				return result.OrderBy(x => x);
			}
		}

		public ZBool RequiresVATNumberDocument => IsImport && !HasAdditionalInfoExemptOfVatSupportingDocument && !IsVATNumberExempt;

		internal ZBool RequiresEndUseN990OrC990Document => IsImport && HasEUSAuthorizationCONRuleOfValueN990OrC990 && IsIntoEndUse;

		protected ZBool HasAdditionalInfoExemptOfVatSupportingDocument
		{
			get
			{
				if (hasAdditionalInfoExemptOfVatSupportingDocumentCache == null)
				{
					hasAdditionalInfoExemptOfVatSupportingDocumentCache = new CachedProperty<ZBool>(Factory, () =>
					{
						var additionalCodesWithDCCAttr = Lookups.FRAddInRefCusCodeListWithCategoryDCCAttribute;
						if (EffectiveAdditionalInfos().Cast<EU.Business.Declaration.MultiLineAddInfos.AdditionalInfo>().Any(x => additionalCodesWithDCCAttr.Contains(x.CSI_Code)))
						{
							return true;
						}
						if (Declaration != null && Declaration.AdditionalInfos.Cast<AdditionalInfo>().Any(x => additionalCodesWithDCCAttr.Contains(x.CSI_Code)))
						{
							return true;
						}
						return false;
					});
				}
				return hasAdditionalInfoExemptOfVatSupportingDocumentCache.Value;
			}
		}
		CachedProperty<ZBool> hasAdditionalInfoExemptOfVatSupportingDocumentCache;

		ZBool IsVATNumberExempt
		{
			get
			{
				if (isVATNumberExemptCache == null)
				{
					isVATNumberExemptCache = new CachedProperty<ZBool>(Factory, () =>
					{
						return CusProcedure == null || CusProcedure.GetAttributeValue(Universal.AttributeNames.Codes.VATNumberExempt, YesNoList.Codes.No) == YesNoList.Codes.Yes;
					});
				}
				return isVATNumberExemptCache.Value;
			}
		}
		CachedProperty<ZBool> isVATNumberExemptCache;

		ZBool IsIntoEndUse
		{
			get
			{
				if (isIntoEndUseCache == null)
				{
					isIntoEndUseCache = new CachedProperty<ZBool>(Factory, () =>
					{
						return CusProcedure != null && CusProcedure.GetAttributeValue(Universal.AttributeNames.Codes.IntoEndUse, YesNoList.Codes.No) == YesNoList.Codes.Yes;
					});
				}
				return isIntoEndUseCache.Value;
			}
		}
		CachedProperty<ZBool> isIntoEndUseCache;

		ZBool HasEUSAuthorizationCONRuleOfValueN990OrC990
		{
			get
			{
				return EntryInstruction != null && (EntryInstruction.SpecificRegimeCondition == RuleCodeCONValueFromList.Codes.N990 || EntryInstruction.SpecificRegimeCondition == RuleCodeCONValueFromList.Codes.C990);
			}
		}

		internal void ManageEndUseN990OrC990Document()
		{
			var doctype = EntryInstruction?.GetAuthorisationRuleValue(CusAuthorisationRuleTypeList.Codes.CON) ?? ZString.Empty;
			var startdate = EntryInstruction?.SpecificRegimeAuthorisation?.CPH_StartDate ?? ZDate.Empty;
			var reference = EntryInstruction?.SpecificRegimeAuthorisation?.CPH_Number ?? ZString.Empty;

			var cusSupportingCollection = SupportingDocuments;
			var cusSupportingInfo = cusSupportingCollection.Cast<SupportingDocument>().FirstOrDefault(x => x.CSI_Code == doctype && x.CSI_ReferenceNumber == reference && x.CSI_DateOfIssue == startdate);

			if (RequiresEndUseN990OrC990Document && cusSupportingInfo == null)
			{
				cusSupportingInfo = cusSupportingCollection.AddNew();
				cusSupportingInfo.CSI_Code = doctype;
				cusSupportingInfo.CSI_ReferenceNumber = reference;
				cusSupportingInfo.CSI_DateOfIssue = startdate;
			}
			else if (!RequiresEndUseN990OrC990Document && cusSupportingInfo != null)
			{
				cusSupportingCollection.RemoveAndDelete(cusSupportingInfo);
			}
		}

		public bool IsPromotionalProductToDROM => JI_SupplementaryCode1 == FRConstants.SupplementaryCodes.PromotionalProductToDROMSupplementaryCode || JI_SupplementaryCode2 == FRConstants.SupplementaryCodes.PromotionalProductToDROMSupplementaryCode || SupplementaryCodes.Any(x => x.CY_Code == FRConstants.SupplementaryCodes.PromotionalProductToDROMSupplementaryCode);

		public bool HasFreeGoods => JI_SupplementaryCode1 == FRConstants.SupplementaryCodes.FreeGoodsSupplementaryCode || JI_SupplementaryCode2 == FRConstants.SupplementaryCodes.FreeGoodsSupplementaryCode || SupplementaryCodes.Any(x => x.CY_Code == FRConstants.SupplementaryCodes.FreeGoodsSupplementaryCode);

		public bool IsProductOfNegligibleValueToDROM => JI_SupplementaryCode1 == FRConstants.SupplementaryCodes.ProductOfNegligibleValueToDROMSupplementaryCode || JI_SupplementaryCode2 == FRConstants.SupplementaryCodes.ProductOfNegligibleValueToDROMSupplementaryCode || SupplementaryCodes.Any(x => x.CY_Code == FRConstants.SupplementaryCodes.ProductOfNegligibleValueToDROMSupplementaryCode);

		public IEnumerable<ZString> CEAdditionalCodes => SupplementaryCodes.Select(x => x.CY_Code).Where(y => !IsFRAdditionalCode(y) && !y.IsEmpty).OrderBy(x => x).ToArray();

		ZBool IsFRAdditionalCode(ZString supplementaryCode) => FRConstants.SupplementaryCodes.listOfFRSupplementaryCodesPrefixes.Contains(supplementaryCode.Left(1));

		protected override IVATSelectionCriteria GetVATSelectionCriteriaCore() => new FRVATSelectionCriteria(this);

		public class FRVATSelectionCriteria : EUVATSelectionCriteria<JobComInvoiceLine>
		{
			public FRVATSelectionCriteria(JobComInvoiceLine invoiceLine) : base(invoiceLine)
			{
			}

			protected override ISet<ZString> GetTradeGroups(JobComInvoiceLine invoiceLine) => GetFRSecondTradeGroups(invoiceLine);
		}

		protected override IEnumerable<IZZRateSelectionCriteria> GetNationalRateSelectionCriteriaCore()
		{
			foreach (var rateType in UniversalReferenceDataHelper.GetNationalRateTypes(Factory, Declaration?.JE_RegionOrTerritoryOfDestination ?? ZString.Empty))
			{
				yield return new FRRateSelectionCriteriaNoPrimaryPreference(this, rateType.ZZR_RateType, "");
			}
		}

		protected override ITariffAdditionalCodeSelectionCriteria GetTariffAdditionalCodeSelectionCriteriaCore()
		{
			return new TariffAdditionalCodeSelectionCriteria(IsImport ? UniversalReferenceConstants.RefCusTariffAdditionalCodeCategories.SIP : UniversalReferenceConstants.RefCusTariffAdditionalCodeCategories.SEP, EffectiveAssessmentDate, EffectiveCountryOfOrigin, DefaultDataGroupingForTariffsCore);
		}

		protected override ZDecimal GetBaseValueToApportionOnCore(CurrencyConverter currencyConverter, string distributeBy)
		{
			switch (distributeBy)
			{
				case ChargeDistributeByList.Codes.Value:
					return currencyConverter.ConvertExact(JI_Calc_InvoicedDocumentaryAmount, LocalCurrency).Amount;
				default:
					return base.GetBaseValueToApportionOnCore(currencyConverter, distributeBy);
			}
		}

		public virtual IEnumerable<RateView> NationalRates
		{
			get
			{
				if (nationalRates == null)
				{
					nationalRates = new CachedProperty<IEnumerable<RateView>>(Factory, () =>
					{
						var list = new List<RateView>();
						var cusTariff = UniversalTariff;
						if (cusTariff != null)
						{
							foreach (var criteria in NationalRateSelectionCriteria)
							{
								var rate = cusTariff.GetApplicableRate(criteria);
								if (rate != null)
								{
									list.Add(rate);
								}
							}
						}
						return list;
					});
				}
				return nationalRates.Value;
			}
		}
		CachedProperty<IEnumerable<RateView>> nationalRates;

		public ZBool HasPrecalculeRate
		{
			get
			{
				if (hasPrecalculeRate == null)
				{
					hasPrecalculeRate = new CachedProperty<ZBool>(Factory, () =>
					{
						return NationalRates.Any(x => x.ZZ2_RateFormula == UniversalReferenceConstants.RefCusRateFormula.Precalcule);
					});
				}
				return hasPrecalculeRate.Value;
			}
		}
		CachedProperty<ZBool> hasPrecalculeRate;

		public PreviousDocument PreviousISTDocument
		{
			get
			{
				if (previousISTDocumentCache == null)
				{
					previousISTDocumentCache = new CachedProperty<PreviousDocument>(Factory, () =>
					{
						var declaration = Declaration;
						var isUCC6 = declaration?.IsUCC6 ?? false;
						var triggerCode = isUCC6 ? FRConstants.PreviousDocuments.N337 : PreviousDocumentCodeList.Codes.IST;
						var doc = PreviousDocuments.Cast<PreviousDocument>().FirstOrDefault(x => x.CSI_Code == triggerCode);
						if (doc == null && InvoiceHeader != null)
						{
							doc = InvoiceHeader.PreviousDocuments.Cast<PreviousDocument>().FirstOrDefault(x => x.CSI_Code == triggerCode);
							if (doc == null && declaration != null && InvoiceHeader.IsAttachedToPersistentDeclaration)
							{
								doc = declaration.PreviousDocuments.Cast<PreviousDocument>().FirstOrDefault(x => x.CSI_Code == triggerCode);
							}
						}
						return doc;
					});
				}
				return previousISTDocumentCache.Value;
			}
		}
		CachedProperty<PreviousDocument> previousISTDocumentCache;

		public ZString PreviousISTNumber => PreviousISTDocument?.CSI_ReferenceNumber ?? ZString.Empty;

		public CusTempStorageJobHeader PreviousISTHeader
		{
			get
			{
				if (previousISTHeaderCache == null)
				{
					previousISTHeaderCache = new CachedProperty<CusTempStorageJobHeader>(Factory, () =>
					{
						return ComplementaryJobISTFinder.FindFromReferenceNumber(Factory, PreviousISTNumber, Declaration.CountryCode);
					});
				}
				return previousISTHeaderCache.Value;
			}
		}
		CachedProperty<CusTempStorageJobHeader> previousISTHeaderCache;

		public new EU.Business.ICusAuthorizationUsageCollection<CusAuthorizationUsage, JobComInvoiceLine> CusAuthorizationUsages => (EU.Business.CusAuthorizationUsageCollection<CusAuthorizationUsage, JobComInvoiceLine>)base.CusAuthorizationUsages;

		protected override EU.Business.ICusAuthorizationUsageCollection<EU.Business.CusAuthorizationUsage, EU.Business.Declaration.JobComInvoiceLine> GetCusAuthorizationUsages()
		{
			return new EU.Business.CusAuthorizationUsageCollection<CusAuthorizationUsage, JobComInvoiceLine>(this, Factory);
		}

		protected override ZString LanguageForTariffDescriptionCore() => Core.SharedConstants.Languages.French;

		public override ZString UniversalTariffType
		{
			get
			{
				var result = base.UniversalTariffType;
				if ((!Declaration?.JE_TariffType.IsEmpty) ?? false)
				{
					result = Declaration.JE_TariffType;
				}
				return result;
			}
		}

		protected override ICustomsUnitDefaultingStrategy GetCustomsUnitDefaultingStrategy()
		{
			return new UniversalRateCustomsUnitDefaultingStrategy<JobComInvoiceLine>((invoiceLine) => invoiceLine.NationalRates.Append(invoiceLine.UniversalDutyRate).Distinct(),
																										Declaration != null ? new ZPropertyInfo[] { Declaration.JE_RegionOrTerritoryOfDestinationInfo } : Enumerable.Empty<ZPropertyInfo>(),
																										RateCalcUnitOfMeasureAggregator.IsConvertableFrom);
		}

		protected override RateDirection RateSelectionCriteriaDirection => IsExport ? RateDirection.Export : RateDirection.Import;

		protected override IZZConditionSelectionCriteria[] GetConditionSelectionCriterias() => new[] { new FRConditionSelectionCriteria(this) };

		public class FRConditionSelectionCriteria : ZZConditionSelectionCriteria<JobComInvoiceLine>
		{
			public FRConditionSelectionCriteria(JobComInvoiceLine invoiceLine)
				: base(invoiceLine)
			{
			}

			protected override ISet<ZString> GetSecondTradeGroups(JobComInvoiceLine invoiceLine) => GetFRSecondTradeGroups(invoiceLine);
		}

		public class FRRateSelectionCriteriaNoPrimaryPreference : RateSelectionCriteriaNoPrimaryPreference<JobComInvoiceLine>
		{
			public FRRateSelectionCriteriaNoPrimaryPreference(JobComInvoiceLine invoiceLine, ZString rateType, ZString rateCode)
				: base(invoiceLine, rateType, rateCode)
			{
			}

			protected override ISet<ZString> GetSecondTradeGroups(JobComInvoiceLine invoiceLine) => GetFRSecondTradeGroups(invoiceLine);
		}

		protected override IZZRateSelectionCriteria GetAllApplicableRatesSelectionCriteriaCore()
		{
			return new FRRateSelectionCriteria(this, ZString.Empty, ZString.Empty);
		}

		public class FRRateSelectionCriteria : RateSelectionCriteria<JobComInvoiceLine>
		{
			public FRRateSelectionCriteria(JobComInvoiceLine invoiceLine, ZString rateType, ZString rateCode)
				: base(invoiceLine, rateType, rateCode)
			{
			}

			protected override ISet<ZString> GetSecondTradeGroups(JobComInvoiceLine invoiceLine) => GetFRSecondTradeGroups(invoiceLine);
		}

		static ISet<ZString> GetFRSecondTradeGroups(JobComInvoiceLine invoiceLine)
		{
			var regionOrTerritoryOfDestination = invoiceLine.Declaration?.JE_RegionOrTerritoryOfDestination ?? ZString.Empty;
			return !regionOrTerritoryOfDestination.IsEmpty
				? FRDomesticOverseasTerritories.GetRegionOrTerritoryOfDestinationCombined(regionOrTerritoryOfDestination).ToHashSet()
				: new HashSet<ZString>();
		}

		protected override bool JI_ZZF_NKTaxTypeReadOnly => true;

		protected override IEntryNumberFormatterForNctsAndDeclarationIntegration GetEntryNumberFormatterCore() => new NCTS.EntryNumberFormatterForNctsAndDeclarationIntegration(Factory);

		protected override void DecorateDocAddressRequirement(JobDocAddressRequirement requirement, DocAddressType addressType)
		{
			base.DecorateDocAddressRequirement(requirement, addressType);

			if (Validation is DeltaIEJobComInvoiceLineValidation validation)
			{
				switch (addressType)
				{
					case DocAddressType.BuyingParty:
						requirement.ValidateOrganisationPK += validation.ValidateBuyerDocAddress;
						break;
					case DocAddressType.SellingParty:
						requirement.ValidateOrganisationPK += validation.ValidateSellerDocAddress;
						break;
				}
			}
		}

		public override ZDecimal LinePriceForBalanceCalc
		{
			get
			{
				return base.LinePriceForBalanceCalc - CurrencyConverter.ConvertExact(JI_Calc_CUTAmountInLine, LinePriceRefCurrency).Amount;
			}
		}

		protected override bool IsWarehouseOrderEnabledCore => base.IsWarehouseOrderEnabledCore && IsImport;

		public bool IsTradingWithSpecialFiscalTerritoriesProcedure => JI_FormattedProcedure.Right(3) == "F15";

		public bool HasNegligibleValueProcedure => JI_FormattedProcedure.EndsWith(FRConstants.ThresholdsAndLimits.NegligibleValueProcedure);

		public bool HasC2CProcedure => JI_FormattedProcedure.EndsWith(FRConstants.ThresholdsAndLimits.C2CValueProcedureSuffix);

		protected override IRefCountry CountryOfOriginFallbackCore => JI_CountryOfOrigin.IsEmpty ? null : new DataTransferCountryInfo(JI_CountryOfOrigin, ((ZZRefCusCodeListCombinedCollection)Lookups.CountryOfOrigins).Cast<ZZRefCusCodeListCombined>().FirstOrDefault(x => x.ZZD_Code == JI_CountryOfOrigin)?.ZZD_Description ?? null);

		public bool HasComponentInventory => ComponentInventoryCollection.Cast<JobComInvLineComponentInventory>().Any(x => x.Inventory != null);

		[ReadOnlyMember(nameof(HasComponentInventory))]
		public override ZString JI_PreviousEntryNumber
		{
			get => base.JI_PreviousEntryNumber;
			set => base.JI_PreviousEntryNumber = value;
		}

		[ReadOnlyMember(nameof(HasComponentInventory))]
		public override ZShort JI_PreviousEntryLineNumber
		{
			get => base.JI_PreviousEntryLineNumber;
			set => base.JI_PreviousEntryLineNumber = value;
		}

		[ReadOnlyMember(nameof(HasComponentInventory))]
		public override ZDecimal JI_BondedWhsQuantity
		{
			get => base.JI_BondedWhsQuantity;
			set => base.JI_BondedWhsQuantity = value;
		}

		[ReadOnlyMember(nameof(HasComponentInventory))]
		public override ZString JI_BondedWhsUnitQty
		{
			get => base.JI_BondedWhsUnitQty;
			set => base.JI_BondedWhsUnitQty = value;
		}

		protected override ZBool IsBondedWhsQuantityVisibleCore => IsIntoOrOutOfRegimeProcedure || (CusProcedure?.IsIntoVATWarehouse() ?? false);
	}
}
