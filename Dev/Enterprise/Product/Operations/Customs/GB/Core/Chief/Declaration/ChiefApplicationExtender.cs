using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.MessageProcessors;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using CusEntryHeader = Enterprise.Customs.GB.Business.Declaration.CusEntryHeader;
using CusEntryInstruction = Enterprise.Customs.GB.Business.Declaration.CusEntryInstruction;
using CusEntryInstructionValidation = Enterprise.Customs.GB.Business.Declaration.CusEntryInstructionValidation;
using CusEntryLine = Enterprise.Customs.GB.Business.Declaration.CusEntryLine;
using CusEntryLineFee = Enterprise.Customs.GB.Business.Declaration.CusEntryLineFee;
using CusEntryLineValidation = Enterprise.Customs.GB.Business.Declaration.CusEntryLineValidation;
using JobComInvoiceLine = Enterprise.Customs.GB.Business.Declaration.JobComInvoiceLine;
using JobComInvoiceLineTax = Enterprise.Customs.GB.Business.JobComInvoiceLineTax;
using JobComInvoiceLineTaxCollection = Enterprise.Customs.GB.Business.JobComInvoiceLineTaxCollection;
using JobComInvoiceLineValueSetStrategy = Enterprise.Customs.GB.Business.JobComInvoiceLineValueSetStrategy;
using JobDeclaration = Enterprise.Customs.GB.Business.Declaration.JobDeclaration;
using MergeManager = Enterprise.Customs.GB.Business.Declaration.MergeManager;

namespace Enterprise.Customs.GB.Chief.Declaration
{
	public class ChiefApplicationExtender : ApplicationExtender
	{
		#region MOP
		protected override IEnumerable<ZString> GetDeferredMethodsOfPaymentCore(CusEntryLine entryLine) => entryLine.Factory.GetCachedValue("GB.CHIEF.CusEntryLine.DeferredMethodsOfPayment", () => MethodOfPaymentCodes.CHIEF.DeferredMethodsOfPayment);
		protected override IEnumerable<ZString> GetGuaranteeDeferredMethodsOfPaymentCore(CusEntryLine entryLine) => entryLine.Factory.GetCachedValue("GB.CHIEF.CusEntryLine.GuaranteeDeferred", () => MethodOfPaymentCodes.CHIEF.GuaranteeDeferredMethodsOfPayment);
		protected override EU.Business.Declaration.CusEntryLine.MoPLevel GetMoPDetailsLevelCore() => EU.Business.Declaration.CusEntryLine.MoPLevel.InvoiceLineTaxes;
		#endregion

		protected override CusEntryInstructionValidation GetNewCusEntryInstructionValidationCore(CusEntryInstruction cei) => new CusEntryInstructionValidation(cei);

		protected override Business.Declaration.JobDeclarationValidation GetNewJobDeclarationValidationCore(JobDeclaration declaration) => new Business.Declaration.JobDeclarationValidation(declaration);

		protected override ZInt MaxNumberOfAdditionalProcedureCodesCore => ZInt.Zero;

		protected override JobComInvoiceLineValidation GetJobComInvoiceLineValidationCore(JobComInvoiceLine invoiceLine) => new JobComInvoiceLineValidation(invoiceLine);

		protected override EUAddInfoValidation GetAddInfoInvoiceLineValidationCore(AddInfoJobComInvoiceLine addInfo) => new AddInfoJobComInvoiceLineValidation(addInfo);

		protected override Customs.Business.CusCodeDataValidation GetFECChallengeValidationCore(Customs.Business.CusCodeData fecData)
			=> new FECChallengeValidation((FECChallenge)fecData);

		protected override TaxValidationHelper GetTaxValidationHelperCore(IEuTax parent, IEnumerable<IEuTax> allTaxes) => new TaxValidationHelper(parent, allTaxes);

		protected override void DefaultLocationCountryCore(JobDeclaration declaration)
		{
			declaration.JE_Calc_LocationOtherInformationCountry = ZString.Empty;
		}

		protected override DataTransferHelper GetDataTransferHelper() => new ChiefDataTransferHelper();

		protected override ZString GetEntryNumberTypeCore(JobDeclaration declaration) => declaration?.JE_MessageType ?? Customs.Business.JobMessageTypeList.Codes.Import;

		protected override JiProcedureHelper GetInvoiceLineHelperCore(JobComInvoiceLine jobComInvoiceLine) => new ChiefJiProcedureHelper(jobComInvoiceLine);

		protected override JobDeclarationValueSetStrategy GetValueStrategyCore(JobDeclaration declaration) => new ChiefJobDeclarationValueSetStrategy(declaration);

		protected override CusEntryLineValidation GetCusEntryLineValidationCore(CusEntryLine cusEntryLine) => new CusEntryLineValidation(cusEntryLine);

		protected override MergeManager GetMergeManagerCore(JobDeclaration jobDec) => new MergeManager(jobDec);

		protected override ZString GuaranteeBondTypeCore => ZString.Empty;

		protected override ZString GetDefaultDataGroupingCodeCore(JobDeclaration declaration) => declaration.CountryCode;
		protected override ZString GetDefaultDataGroupingCodeForTariffsCore(JobDeclaration declaration) => declaration.CountryCode;

		protected override ZString GetDefaultDataGroupingCodeForDutyRateCodesCore(JobDeclaration declaration) => GetDefaultDataGroupingCodeForTariffsCore(declaration);

		protected override C88CreationStrategy CreateC88CreationStrategyCore(JobDeclaration declaration) => new C88CreationStrategy(declaration);

		protected override ZString GetNewEntryLocalReferenceNumberCore(BusinessObjectFactory factory) => ZString.Empty;

		protected override UCCHelper GetUCCHelper() => new UCCHelper();

		protected override void DefaultLocationOfGoodsCore(JobDeclaration declaration) => new Box30LocationOfGoodsValueSetter(declaration).SetBox30();

		protected override ZString GetCDSChargeCodeCore(Customs.Business.BaseJobComInvHeaderCharge charge) => ZString.Empty;

		protected override JobComInvoiceLineValueSetStrategy GetJobComInvoiceLineValueSetStrategyCore(JobComInvoiceLine invoiceLine)
		{
			return new ChiefJobComInvoiceLineValueSetStrategy(invoiceLine);
		}

		protected override Business.Declaration.CusEntryHeaderValidation GetCusEntryHeaderValidationCore(EU.Business.Declaration.CusEntryHeader entryHeader) => new Business.Declaration.CusEntryHeaderValidation(entryHeader);

		protected override JobComInvoiceHeaderValidation GetJobComInvoiceHeaderValidationCore(EU.Business.Declaration.JobComInvoiceHeader invoiceHeader) => new JobComInvoiceHeaderValidation(invoiceHeader);

		protected override GBAutoSendCustomsMessageProcessor GetEntryDeclarationMessageProcessorCore(JobDeclaration declaration)
		{
			return new ChiefEntryDeclarationMessageProcessor(declaration);
		}

		protected override CustomsOfficeRequirement GetMainOfficeCore(JobDeclaration declaration)
		{
			return declaration.Factory.GetCachedValue("GB.JobDeclarationCustomsOfficeRequirementHelper.MainOffice.CHF." + declaration.JE_MessageType, () =>
			{
				if (declaration.IsImport)
				{
					return null;
				}
				if (declaration.IsExport)
				{
					return new CustomsOfficeRequirement(EuOfficeCodesTypes.Codes.OfficeOfExit, false, false, true, declaration.JE_CustomsOfficeExportCaption);
				}
				return new CustomsOfficeRequirement(ZString.Empty, true, false);
			});
		}

		protected override ZBool IsCDSChargeTypeItemLevelCore(Customs.Business.BaseJobComInvHeaderCharge charge) => false;

		protected override BondedWarehouseMessageProcessor GetBondedWarehouseMessageProcessorCore(ZGuid messagePK, EmailDef emailReportThatHasBeenDelayed, Action<EmailDef, Enterprise.Messaging.Business.EDIMessage> sendMail, bool shouldCheckMessageStatus = false) => null;

		protected override IList<Customs.Business.PermitRecord> GetPermitRecordsCore(CusEntryHeader entryHeader)
		{
			var permitRecords = new Dictionary<string, Customs.Business.PermitRecord>();
			var entryHeaderPermits = entryHeader.SupportingDocuments.Where(x => x.IsCodeAPermitType).Select(x => new CusEntryPermitRecord { SupportingDocument = x, Quantity = x.CSI_Quantity, QuantityUnit = x.CSI_UnitOfQuantity });
			var entryLinePermits = entryHeader.MergedLines.Cast<CusEntryLine>().SelectMany(l => l.SupportingDocuments.Where(x => x.IsCodeAPermitType), (l, lsd) => new CusEntryPermitRecord { SupportingDocument = lsd, Quantity = lsd.CSI_Quantity, QuantityUnit = lsd.CSI_UnitOfQuantity });
			var permits = entryHeaderPermits.Union(entryLinePermits);

			var permitHolder = entryHeader.IsImport ? entryHeader.Declaration.Importer?.PK ?? ZGuid.Empty : entryHeader.Declaration.Exporter?.PK ?? ZGuid.Empty;

			foreach (CusEntryPermitRecord permitRecord in permits)
			{
				var permitNumber = permitRecord.SupportingDocument.CSI_ReferenceNumber;
				if (permitRecords.ContainsKey(permitNumber))
				{
					var permit = permitRecords[permitNumber];
					PopulatePermitValueAndQuantity(permit, permitRecord, true);
				}
				else
				{
					var cusPermit = new Customs.Business.BaseCusPermitHeader.Loader(entryHeader.Factory).Load(Core.Constants.CountryCodes.UnitedKingdom, permitRecord.SupportingDocument.CSI_ReferenceNumber, permitHolder, entryHeader.CH_EntrySubmittedDate);

					if (cusPermit != null)
					{
						var permit = new Customs.Business.PermitRecord()
						{
							PermitHeader = cusPermit
						};

						PopulatePermitValueAndQuantity(permit, permitRecord, false);
						permitRecords.Add(permitNumber, permit);
					}
				}
			}

			return permitRecords.Values.ToList();
		}

		void PopulatePermitValueAndQuantity(Customs.Business.PermitRecord permit, CusEntryPermitRecord permitRecord, bool addValues)
		{
			if ((permit.PermitHeader?.CPH_QtyValIndicator ?? ZString.Empty) == Customs.Business.PermitQtyValIndicatorList.Codes.QTY || permit.PermitHeader.CPH_QtyValIndicator == Customs.Business.PermitQtyValIndicatorList.Codes.BTH)
			{
				if (addValues)
				{
					permit.Quantity += GBPermitHelper.GetCustomsQuantity(permit.PermitHeader.CPH_Number, permit, permitRecord);
				}
				else
				{
					permit.Quantity = GBPermitHelper.GetCustomsQuantity(permit.PermitHeader.CPH_Number, permit, permitRecord);
				}
			}
			else if (permit.PermitHeader.CPH_QtyValIndicator == Customs.Business.PermitQtyValIndicatorList.Codes.VAL)
			{
				if (addValues)
				{
					permit.Value += permitRecord.Quantity;
				}
				else
				{
					permit.Value = permitRecord.Quantity;
				}
			}
		}

		protected override ZString GetComplementaryJobPreviousDocumentCodeTypeCore(ZString defaultValue) => defaultValue;

		protected override Customs.Business.CusEntryLineFeeValidation GetCusEntryLineFeeValidationCore(AutoCusEntryLineFee entryLineFee, Func<Customs.Business.CusEntryLineFeeValidation> defaultValidationAction) => defaultValidationAction.Invoke();

		protected override ZString GetEntryHeaderStatusDescriptionCore(ZString entryStatus, ZString fallbackValue) => fallbackValue;

		protected override ZString GetOldEntryStatusCore(CusEntryHeader entryHeader)
		{
			// Using this method as Original value of the AddInfo field CH_ImportClearanceStatusICS does not work
			// This can be refactored if/when add info's are fixed
			var addInfoHash = AddInfoParser.CreateDictionaryWithAddInfoString((ZString)entryHeader.CH_AddInfoInfo.OriginalValue);
			var value = ZString.Empty;
			addInfoHash.TryGetValue(CusEntryHeader.Schema.ImportClearanceStatusICS.Substring(3), out value);

			return value;
		}

		protected override ZString GetEntryStatusCore(CusEntryHeader entryHeader) => entryHeader.CH_ImportClearanceStatusICS;

		protected override EntryLineVatCalculator GetEntryLineVatCalculatorCore(CusEntryLine entryLine, Func<CusEntryLine, EntryLineVatCalculator> baseFunctionality) => baseFunctionality(entryLine);

		protected override string DeferredFeeMethodOfPaymentCodeCore => "F";

		protected override ZString GetDutyAmountsAsStringCore(JobComInvoiceLine invoiceLine, Func<ZString> baseFunctionality) => GetDutyLinesAndFormatWithThisRunner(invoiceLine.Taxes, (JobComInvoiceLineTax tax) => tax.G4_Type + ":" + tax.JLT_Amount.ToString("#.00", CultureInfo.CurrentCulture));

		protected override ZString GetDutyRateDescriptionCore(CusEntryLine entryLine, ZString baseValue)
		{
			return GetDutyLinesAndFormatWithThisRunner(entryLine.RandomLine.Taxes, delegate (JobComInvoiceLineTax tax)
			{
				return tax.JLT_Type + ":" + tax.JLT_Calc_CalculatedPercentage.Round(2).ToStringTrimZeros(2) + "%";
			});
		}

		protected override ZDecimal GetGSTRateCore(CusEntryLine entryLine, ZDecimal baseValue)
		{
			var tax = (from JobComInvoiceLineTax t in entryLine.RandomLine.Taxes where t.JLT_Type == UniversalReferenceConstants.RefCusRateCodes.Vat || t.JLT_Type == UniversalReferenceConstants.RefCusRateCodes.VatOnAdditionalDutiesForNorthernIreland select t).FirstOrDefault();
			return tax != null ? tax.JLT_Calc_CalculatedPercentage.Round(2) : ZDecimal.Zero;
		}

		ZString GetDutyLinesAndFormatWithThisRunner(JobComInvoiceLineTaxCollection taxes, FormatOneDutyLineRunner runner)
		{
			var taxLines = new List<ZString>();
			foreach (var tax in (from JobComInvoiceLineTax t in taxes where t.JLT_Type != UniversalReferenceConstants.RefCusRateCodes.Vat select t))
			{
				taxLines.Add(runner(tax.Data));
			}
			var array = taxLines.ToArray();
			Array.Sort(array);
			return ZString.Join(System.Environment.NewLine, array);
		}

		delegate ZString FormatOneDutyLineRunner(JobComInvoiceLineTax tax);

		protected override void AfterUniversalCopyCore(JobDeclaration declaration)
		{
			var entryInstructions = declaration.CustomsEntryInstructions;
			var entryInstructionsToRemove = new List<CusEntryInstruction>();

			if (entryInstructions.Count > 1)
			{
				foreach (var entryInstruction in entryInstructions)
				{
					if (!declaration.DeclarationTypeList.ContainsCode(entryInstruction.CEI_Style))
					{
						entryInstructionsToRemove.Add(entryInstruction);
					}
				}

				entryInstructionsToRemove.ForEach(x => entryInstructions.RemoveAndDelete(x));
			}
		}

		protected override ISet<ZString> GetListOfStatusCodesForAutoBillingCore(JobDeclaration declaration)
		{
			var options = CustomsDataRegistry.Instance.EnableAccountingIntegration.GetFallBackValueAtAllLevels(declaration?.JE_GC.ToGuid() ?? Guid.Empty, Guid.Empty, Guid.Empty);
			return options.ChiefCustomsStatusCodes.Split(',').Select(x => x.Trim()).Where(x => !x.IsEmpty).Distinct().ToHashSet();
		}

		protected override ZString GetDefaultMethodOfPaymentValueCore(CusEntryLineFee fee) => ZString.Empty;

		protected override FiscalRepresentativeDefaulter GetFiscalRepresentativeDefaulterCore() => new FiscalRepresentativeDefaulter();
	}
}
