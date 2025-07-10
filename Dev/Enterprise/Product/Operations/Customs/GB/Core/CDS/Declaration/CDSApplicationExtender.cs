using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.MessageProcessors;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Business.CodeDescriptionPairLists;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.CDS.CDSResponse;
using Enterprise.Customs.GB.CDS.Declaration.MultiLineAddInfos;
using Enterprise.Customs.GB.Registry;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using AddInfoJobComInvoiceLineValueSetStrategy = Enterprise.Customs.GB.Business.Declaration.AddInfoJobComInvoiceLineValueSetStrategy;
using CusEntryHeader = Enterprise.Customs.GB.Business.Declaration.CusEntryHeader;
using CusEntryHeaderValidation = Enterprise.Customs.GB.Business.Declaration.CusEntryHeaderValidation;
using CusEntryInstruction = Enterprise.Customs.GB.Business.Declaration.CusEntryInstruction;
using CusEntryInstructionValidation = Enterprise.Customs.GB.Business.Declaration.CusEntryInstructionValidation;
using CusEntryLine = Enterprise.Customs.GB.Business.Declaration.CusEntryLine;
using CusEntryLineFee = Enterprise.Customs.GB.Business.Declaration.CusEntryLineFee;
using CusEntryLineFeeValidation = Enterprise.Customs.GB.Business.Declaration.CusEntryLineFeeValidation;
using CusEntryLineValidation = Enterprise.Customs.GB.Business.Declaration.CusEntryLineValidation;
using JobComInvoiceLine = Enterprise.Customs.GB.Business.Declaration.JobComInvoiceLine;
using JobComInvoiceLineLookups = Enterprise.Customs.GB.Business.Declaration.JobComInvoiceLineLookups;
using JobComInvoiceLineTax = Enterprise.Customs.GB.Business.JobComInvoiceLineTax;
using JobComInvoiceLineValueSetStrategy = Enterprise.Customs.GB.Business.JobComInvoiceLineValueSetStrategy;
using JobDeclaration = Enterprise.Customs.GB.Business.Declaration.JobDeclaration;
using JobDeclarationValidation = Enterprise.Customs.GB.Business.Declaration.JobDeclarationValidation;
using MergeManager = Enterprise.Customs.GB.Business.Declaration.MergeManager;

namespace Enterprise.Customs.GB.CDS.Declaration
{
	public class CDSApplicationExtender : ApplicationExtender
	{
		protected CDSApplicationExtender() : base()
		{
		}
		#region MOP
		protected override IEnumerable<ZString> GetDeferredMethodsOfPaymentCore(CusEntryLine entryLine) => entryLine.Factory.GetCachedValue("GB.CDS.CusEntryLine.DeferredMethodsOfPayment", () => MethodOfPaymentCodes.CDS.DeferredMethodsOfPayment);
		protected override IEnumerable<ZString> GetGuaranteeDeferredMethodsOfPaymentCore(CusEntryLine entryLine) => entryLine.Factory.GetCachedValue("GB.CDS.CusEntryLine.GuaranteeDeferred", () => MethodOfPaymentCodes.CDS.GuaranteeDeferredMethodsOfPayment);
		protected override EU.Business.Declaration.CusEntryLine.MoPLevel GetMoPDetailsLevelCore() => EU.Business.Declaration.CusEntryLine.MoPLevel.InvoiceLine;
		#endregion

		protected override CusEntryInstructionValidation GetNewCusEntryInstructionValidationCore(CusEntryInstruction cei) => new CDSCusEntryInstructionValidation(cei);

		protected override IValueSetStrategy GetNewCusEntryInstructionValueSetStrategyCore(CusEntryInstruction cei) => new CDSCusEntryInstructionValueSetStrategy(cei);

		protected override JobDeclarationValidation GetNewJobDeclarationValidationCore(JobDeclaration declaration) => new CDSJobDeclarationValidation(declaration);
		protected override EU.Business.Declaration.MultiLineAddInfos.AdditionalInfoLookups GetNewAdditionalInfoLookupsCore(Business.Declaration.MultiLineAddInfos.AdditionalInfo additionalInfo) => new CDSAdditionalInfoLookups(additionalInfo);

		protected override ZInt MaxNumberOfAdditionalProcedureCodesCore => 98;

		protected override ZString GetJobComInvoiceLineDefaultMethodOfPaymentCore(JobDeclaration declaration)
		{
			var mop = declaration?.JE_PaymentMethod ?? ZString.Empty;
			return !mop.IsEmpty && (declaration?.IsImport ?? false) ? (ZString)Constants.MethodOfPayment.DeferredPayment : ZString.Empty;
		}

		protected override void SetDefaultsForNewChildOnInvoiceLineCore(JobDeclaration declaration, JobComInvoiceLine invoiceLine)
		{
			bool isIntoNi = false;
			switch (declaration.JE_NorthernIrelandMode)
			{
				case Constants.NorthernIrelandModeCodes.NII:
					isIntoNi = true;
					invoiceLine.AddAdditionalInfoByCodeIfNotExists(GBCommonConstants.AdditonalInfoCodes.NIIMP);
					break;
				case Constants.NorthernIrelandModeCodes.NIE:
					invoiceLine.AddAdditionalInfoByCodeIfNotExists(GBCommonConstants.AdditonalInfoCodes.NIEXP);
					break;
				case Constants.NorthernIrelandModeCodes.G2N:
					isIntoNi = true;
					invoiceLine.AddAdditionalInfoByCodeIfNotExists(GBCommonConstants.AdditonalInfoCodes.NIDOM);
					break;
			}

			if (isIntoNi)
			{
				if (declaration.JE_ClaimEuSubsidy)
				{
					invoiceLine.AddAdditionalInfoByCodeIfNotExists(GBCommonConstants.AdditonalInfoCodes.NIAID);
				}

				if (!declaration.JE_NiGoodsAtRiskOfMovingToROI)
				{
					invoiceLine.AddAdditionalInfoByCodeIfNotExists(GBCommonConstants.AdditonalInfoCodes.NIREM);
				}
			}

			var statValueManualOverrideRegistry = GBCustomsDataRegistry.Instance.CDSStatisticalValueManualOverride.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty);
			invoiceLine.ZG_StatisticalValueManualOverride = declaration.IsImport && statValueManualOverrideRegistry;
		}

		protected override JobComInvoiceLineLookups GetJobComInvoiceLineLookupsCore(JobComInvoiceLine invoiceLine) => new CDSJobComInvoiceLineLookups(invoiceLine);

		protected override EU.Business.Declaration.JobComInvoiceLineValidation GetJobComInvoiceLineValidationCore(JobComInvoiceLine invoiceLine) => new CDSJobComInvoiceLineValidation(invoiceLine);

		protected override EUAddInfoValidation GetAddInfoInvoiceLineValidationCore(AddInfoJobComInvoiceLine addInfo)
			=> new CDSAddInfoJobComInvoiceLineValidation(addInfo);

		protected override Customs.Business.CusCodeDataValidation GetFECChallengeValidationCore(Customs.Business.CusCodeData fecData)
			=> new CDSFECChallengeValidation(fecData);

		protected override TaxValidationHelper GetTaxValidationHelperCore(EU.Business.Declaration.IEuTax parent, IEnumerable<EU.Business.Declaration.IEuTax> allTaxes) => new CDSTaxValidationHelper(parent, allTaxes);

		protected override void DefaultLocationCountryCore(JobDeclaration declaration)
		{
			declaration.JE_Calc_LocationOtherInformationCountry = declaration.CountryCode;
		}

		protected override DataTransferHelper GetDataTransferHelper() => new CDSDataTransferHelper();

		protected override ZString GetEntryNumberTypeCore(JobDeclaration declaration) => Common.CusEntryNumberTypes.Standard.MovementReferenceNumber;

		protected override JiProcedureHelper GetInvoiceLineHelperCore(JobComInvoiceLine jobComInvoiceLine) => new JiProcedureHelper(jobComInvoiceLine);

		protected override EU.Business.Declaration.JobDeclarationValueSetStrategy GetValueStrategyCore(JobDeclaration declaration) => new CDSJobDeclarationValueSetStrategy(declaration);

		protected override CusEntryLineValidation GetCusEntryLineValidationCore(CusEntryLine cusEntryLine) => new CDSCusEntryLineValidation(cusEntryLine);

		protected override MergeManager GetMergeManagerCore(JobDeclaration jobDec) => new CDSMergeManager(jobDec);

		protected override ZString GuaranteeBondTypeCore => GuaranteeTypeList.Codes.Guarantee;

		protected override ZString GetDefaultDataGroupingCodeCore(JobDeclaration declaration) => Core.Constants.Customs.Universal.RefDataGrouping.Codes.CustomsDeclarationService;

		protected override ZString GetDefaultDataGroupingCodeForTariffsCore(JobDeclaration declaration) => declaration.IsEuTariffToBeUsedForNorthernIreland ? Core.Constants.CountryCodes.NorthernIreland_ForUseOnlyByEuInCertainScopes
																												: Core.Constants.Customs.Universal.RefDataGrouping.Codes.CustomsDeclarationService;
		protected override ZString GetDefaultDataGroupingCodeForDutyRateCodesCore(JobDeclaration declaration) => Core.Constants.Customs.Universal.RefDataGrouping.Codes.CustomsDeclarationService;

		protected override C88CreationStrategy CreateC88CreationStrategyCore(JobDeclaration declaration) => new CDSEntryCreationStrategy(declaration);

		protected override ZString GetNewEntryLocalReferenceNumberCore(BusinessObjectFactory factory)
		{
			var lrn = EntryLocalReferenceNumber();
			while (!CusEntryHeader.IsUniqueEntryLocalReferenceNumber(factory, lrn))
			{
				lrn = EntryLocalReferenceNumber();
			}
			return lrn;

			ZString EntryLocalReferenceNumber()
			{
				var target = new CDSEntryLocalReferenceNumberGeneratorTarget();
				var generator = new NumberGenerator
				{
					Factory = factory,
					Context = new NumberGeneratorContext(),
					BaseFountain = Env.NumberFountains.GBCDSEntryLocalReferenceNumber,
					FountainGetter = Env.NumberFountains.GetGBCDSEntryLocalReferenceNumberGeneratorFountain,
					PrimaryTarget = target
				};
				generator.ValueProviders.AddRange(new StandardValueSource());
				generator.Generate();
				generator.EnforceMaxLengths();
				return target.Value.ToUpper();
			}
		}

		protected override UCCHelper GetUCCHelper() => new CDSUCCHelper();

		protected override string GetIApportionInvoiceHolderCountryContextCore(JobDeclaration declaration)
		{
			return ZString.Format("{0}CDS", declaration.CountryCode);  // key
		}

		protected override void DefaultLocationOfGoodsCore(JobDeclaration declaration)
		{
		}

		protected override ZString GetCDSChargeCodeCore(Customs.Business.BaseJobComInvHeaderCharge charge)
		{
			return charge == null ? ZString.Empty
				: (ZString)CDSCustomsChargeTypeList.MapToCDSAdditionDeductionChargeType(charge.J7_ChargeType
				, charge.J7_IsDutiable
				, charge.J7_DistributeBy
				, charge.J7_IsIncludedInITOT);
		}

		protected override JobComInvoiceLineValueSetStrategy GetJobComInvoiceLineValueSetStrategyCore(JobComInvoiceLine invoiceLine)
		{
			return new CDSJobComInvoiceLineValueSetStrategy(invoiceLine);
		}
		protected override AddInfoJobComInvoiceLineValueSetStrategy GetAddInfoJobComInvoiceLineValueSetStrategyCore(AddInfoJobComInvoiceLine addInfoJobComInvoiceLine)
		{
			return new CDSAddInfoJobComInvoiceLineValueSetStrategy(addInfoJobComInvoiceLine);
		}

		protected override CusEntryHeaderValidation GetCusEntryHeaderValidationCore(EU.Business.Declaration.CusEntryHeader entryHeader) => new CusEntryHeaderValidation(entryHeader);

		protected override EU.Business.Declaration.JobComInvoiceHeaderValidation GetJobComInvoiceHeaderValidationCore(EU.Business.Declaration.JobComInvoiceHeader invoiceHeader) => new CDSJobComInvoiceHeaderValidation(invoiceHeader);

		protected override GBAutoSendCustomsMessageProcessor GetEntryDeclarationMessageProcessorCore(JobDeclaration declaration)
		{
			return new CDSEntryDeclarationMessageProcessor(declaration);
		}

		protected override void SetDefaultValueForInvoiceLineTaxCore(JobComInvoiceLineTax tax)
		{
			base.SetDefaultValueForInvoiceLineTaxCore(tax);
			tax.JLT_MethodOfPayment = tax.InvoiceLine?.ZG_MethodOfPayment ?? ZString.Empty;
		}

		protected override CustomsOfficeRequirement GetMainOfficeCore(JobDeclaration declaration)
		{
			var localOnly = declaration.JE_NorthernIrelandMode == NIModeList.Codes.NotToOrFromNi || declaration.JE_NorthernIrelandMode == NIModeList.Codes.MovementFromGreatBritainToNi;

			return declaration.Factory.GetCachedValue($"GB.JobDeclarationCustomsOfficeRequirementHelper.MainOffice.CDS.{declaration.JE_MessageType}.{localOnly}", () =>
			{
				if (declaration.IsImport)
				{
					return new CustomsOfficeRequirement(EuOfficeCodesTypes.Codes.OfficeForCentralizedClearance, false, false, true, declaration.JE_CustomsOfficeImportCaption);
				}
				if (declaration.IsExport)
				{
					return new CustomsOfficeRequirement(EuOfficeCodesTypes.Codes.OfficeOfExit, true, localOnly, declaration.JE_CustomsOfficeExportCaption);
				}
				return new CustomsOfficeRequirement(ZString.Empty, true, false);
			});
		}

		protected override IEnumerable<CustomsOfficeRequirement> GetOtherCustomsOfficeRequirementsCore(JobDeclaration declaration)
		{
			return declaration.Factory.GetCachedValue("GB.JobDeclarationCustomsOfficeRequirementHelper.OtherRequirements.CDS." + declaration.JE_MessageType, () =>
			{
				var results = new List<CustomsOfficeRequirement>();

				if (declaration.IsExport)
				{
					results.Add(new CustomsOfficeRequirement(EuOfficeCodesTypes.Codes.OfficeForCentralizedClearance, false, false, declaration.JE_CustomsOfficeImportCaption));
				}

				return results;
			});
		}

		protected override ZBool IsCDSChargeTypeItemLevelCore(Customs.Business.BaseJobComInvHeaderCharge charge) => new CDSChargeTypeLevelCalculator(GetCDSChargeCodeCore(charge)).IsItemLevel;

		protected override BondedWarehouseMessageProcessor GetBondedWarehouseMessageProcessorCore(ZGuid messagePK, EmailDef emailReportThatHasBeenDelayed, Action<EmailDef, Enterprise.Messaging.Business.EDIMessage> sendMail, bool shouldCheckMessageStatus = false) => new CDSBondedWarehouseMessageProcessor(messagePK, emailReportThatHasBeenDelayed, sendMail);

		//TODO When we have confimation of how CDS will treat permits of type BTH this might need to be updated
		protected override IList<Customs.Business.PermitRecord> GetPermitRecordsCore(CusEntryHeader entryHeader)
		{
			var permitRecords = new Dictionary<string, Customs.Business.PermitRecord>();
			var entryHeaderPermits = entryHeader.SupportingDocuments.Where(x => x.IsCodeAPermitType).Select(x => new CusEntryPermitRecord { SupportingDocument = x, CustomsValue = x.CSI_Value, Quantity = x.CSI_Quantity, QuantityUnit = x.CSI_UnitOfQuantity });
			var entryLinePermits = entryHeader.MergedLines.Cast<CusEntryLine>().SelectMany(l => l.SupportingDocuments.Where(x => x.IsCodeAPermitType), (l, lsd) => new CusEntryPermitRecord { SupportingDocument = lsd, CustomsValue = lsd.CSI_Value, Quantity = lsd.CSI_Quantity, QuantityUnit = lsd.CSI_UnitOfQuantity });
			var permits = entryHeaderPermits.Union(entryLinePermits);

			var permitHolder = entryHeader.IsImport ? entryHeader.Declaration.Importer?.PK ?? ZGuid.Empty : entryHeader.Declaration.Exporter?.PK ?? ZGuid.Empty;

			foreach (CusEntryPermitRecord permit in permits)
			{
				var permitNumber = permit.SupportingDocument.CSI_ReferenceNumber;
				if (permitRecords.ContainsKey(permitNumber))
				{
					var permitRecord = permitRecords[permitNumber];
					permitRecord.Value += permit.CustomsValue;
					permitRecord.Quantity += GBPermitHelper.GetCustomsQuantity(permitNumber, permitRecord, permit);
				}
				else
				{
					var cusPermit = new Customs.Business.BaseCusPermitHeader.Loader(entryHeader.Factory).Load(Core.Constants.CountryCodes.UnitedKingdom, permit.SupportingDocument.CSI_ReferenceNumber, permitHolder, entryHeader.CH_EntrySubmittedDate);

					if (cusPermit != null)
					{
						var permitRecord = new Customs.Business.PermitRecord()
						{
							Value = permit.CustomsValue,
							PermitHeader = cusPermit
						};

						permitRecord.Quantity = GBPermitHelper.GetCustomsQuantity(permitNumber, permitRecord, permit);
						permitRecords.Add(permitNumber, permitRecord);
					}
				}
			}

			return permitRecords.Values.ToList();
		}

		protected override ZString GetComplementaryJobPreviousDocumentCodeTypeCore(ZString defaultValue) => PreviousDocumentCodeListCDS.Codes.DeclarationUniqueConsignmentReferenceDucr;

		protected override Customs.Business.CusEntryLineFeeValidation GetCusEntryLineFeeValidationCore(EU.Business.Declaration.AutoCusEntryLineFee entryLineFee, Func<Customs.Business.CusEntryLineFeeValidation> defaultValidationAction) => new CusEntryLineFeeValidation(entryLineFee);

		protected override ZString GetEntryHeaderStatusDescriptionCore(ZString entryStatus, ZString fallbackValue)
		{
			var description = ResponseFunction.GetDescription(entryStatus);

			if (description.IsEmpty)
			{
				description = fallbackValue;
			}
			return description;
		}

		protected override ZString GetOldEntryStatusCore(CusEntryHeader entryHeader) => ResponseFunction.GetNumericFunctionCode((ZString)entryHeader.CH_EntryStatusInfo.OriginalValue);

		protected override ZString GetEntryStatusCore(CusEntryHeader entryHeader) => ResponseFunction.GetNumericFunctionCode(entryHeader.CH_EntryStatus);

		protected override EntryLineVatCalculator GetEntryLineVatCalculatorCore(CusEntryLine entryLine, Func<CusEntryLine, EntryLineVatCalculator> baseFunctionality) => entryLine.Declaration.IsEuTariffToBeUsedForNorthernIreland ? new CDSGBNorthernIrelandAtRiskEntryLineVatCalculator(entryLine) : baseFunctionality(entryLine);

		protected override string DeferredFeeMethodOfPaymentCodeCore => "E";

		protected override Common.IApportionStrategy GetApportionStrategyCore() => new ApportionStrategy();

		protected override Customs.Business.IFeeRounder GetChargeAmountNoRounderCore() => new CDSChargeAmountNoRounder();

		protected override ZString GetDutyAmountsAsStringCore(JobComInvoiceLine invoiceLine, Func<ZString> baseFunctionality) => baseFunctionality.Invoke();

		protected override ZString GetDutyRateDescriptionCore(CusEntryLine entryLine, ZString baseValue) => baseValue;

		protected override ZDecimal GetGSTRateCore(CusEntryLine entryLine, ZDecimal baseValue) => baseValue;

		protected override void AfterUniversalCopyCore(JobDeclaration declaration) { }

		protected override ISet<ZString> GetListOfStatusCodesForAutoBillingCore(JobDeclaration declaration)
		{
			var options = DataRegistry.Business.CustomsDataRegistry.Instance.EnableAccountingIntegration.GetFallBackValueAtAllLevels(declaration?.Branch.GB_GC.ToGuid() ?? Guid.Empty, Guid.Empty, Guid.Empty);
			return options.CDSCustomsStatusCodes.Split(',').Select(x => x.Trim()).Where(x => !x.IsEmpty).Distinct().ToHashSet();
		}

		protected override string GetFeeCodeFromRateCodeCore(CusEntryLine entryLine, string rateCode) => CDSFeeCodeFromRateCodeMapper.GetMapping(entryLine, rateCode);

		protected override ZString GetDefaultMethodOfPaymentValueCore(CusEntryLineFee fee)
		{
			var result = ZString.Empty;
			var declaration = (JobDeclaration)fee.EntryLine?.Declaration;
			if (!(declaration?.CustomsEntryInstructions.SelectMany(x => x.FiscalReferences).Any() ?? ZBool.True) || !chargeTypesThatReturnEmptyMethodOfPayment.Contains(fee.CF_ChargeType))
			{
				var invoiceLine = fee.EntryLine?.RandomLine;
				result = invoiceLine?.ZG_MethodOfPayment ?? ZString.Empty;
			}

			return result;
		}

		protected override EU.Business.Declaration.FiscalRepresentativeDefaulter GetFiscalRepresentativeDefaulterCore() => new CDSFiscalRepresentativeDefaulter();

		readonly ZString[] chargeTypesThatReturnEmptyMethodOfPayment = new ZString[] { UniversalReferenceConstants.RefCusRateCodes.Vat, UniversalReferenceConstants.RefCusRateCodes.VatOnAdditionalDutiesForNorthernIreland };
	}

	public class CDSChargeAmountNoRounder : Customs.Business.IFeeRounder
	{
		public ZDecimal Round(ZDecimal chargeAmount) => Math.Truncate(100 * chargeAmount) / 100;
	}
}
