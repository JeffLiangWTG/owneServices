using System.Collections.Generic;
using System.Linq;
using CargoWise.BrandManager;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.DutyCalculator;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Business.CodeDescriptionPairLists;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.ZArchitecture;
using ValuationMethodList = Enterprise.Customs.GB.Business.ValuationMethodList;

namespace Enterprise.Customs.GB.CDS.Declaration
{
	public class CDSJobComInvoiceLineValidation : EU.Business.Declaration.JobComInvoiceLineValidation
	{
		public CDSJobComInvoiceLineValidation(JobComInvoiceLine invoiceLine)
			: base(invoiceLine)
		{
		}

		protected override void CheckJI_Weight()
		{
			base.CheckJI_Weight();

			if (Parent.JI_Weight.IsEmpty
				&& (Parent.CusProcedure?.HasAttribute(Universal.AttributeNames.Codes.GrossMassMandatory) ?? false))
			{
				Parent.JI_WeightInfo.AddMessageError("Gross mass may not be empty when the CPC demands it");
			}

			if (Parent != null && Parent.CusEntryLine != null)
			{
				var cusEntryLine = Parent.CusEntryLine;
				if (cusEntryLine.EffectiveGrossWeight.IsEmpty || cusEntryLine.EffectiveGrossWeight.InKilogramsSafe > 9999999999999999)
				{
					Parent.JI_WeightInfo.AddMessageError(GrossWeightValidationError);
				}
			}
		}

		public override void ValidateSupervisingOfficeDocAddress()
		{
			CDSJobDeclarationValidation.ValidateSupervisingOfficeHasSpoffCode(Parent.Declaration, Parent.SupervisingOfficeDocAddress);
		}

		protected override string GetPackagesTabName()
		{
			return Parent.Declaration.InvoiceLinePackagesPivotTabCaption;
		}

		protected override void CheckJI_ValuationCode()
		{
			base.CheckJI_ValuationCode();

			if (Parent.JI_ValuationCode == ValuationMethodList.Codes._7)
			{
				Parent.JI_ValuationCodeInfo.AddMessageError(ValuationCodeSimplifiedProcedure_ErrorMessage);
			}
		}

		protected override void CheckJI_Procedure()
		{
			base.CheckJI_Procedure();

			var info = Parent.JI_ProcedureInfo;
			MandatoryValidation.MessageErrorIfNotEntered(info);

			if (!JobComInvoiceLine.IsValidProcedureToApportion(Parent.JI_Procedure))
			{
				Parent.JI_ProcedureInfo.AddWarning(ProcedureStopApportionWaring);
			}
		}

		JobDeclaration GBDeclaration
		{
			get { return Parent.Declaration; }
		}

		protected override void CheckJI_Description()
		{
			base.CheckJI_Description();
			if (Parent.JI_Description.IsEmpty && GBDeclaration != null && !GBDeclaration.IsFSD)
			{
				Parent.JI_DescriptionInfo.AddMessageError(E00662);
			}
		}

		protected override void CheckJI_Tariff()
		{
			base.CheckJI_Tariff();

			var declaration = GBDeclaration;
			if (declaration != null && IsEntryInstructionClearanceRequest && !Parent.JI_Tariff.IsEmpty)
			{
				Parent.JI_TariffInfo.AddMessageError("Clearance request demands no commodity code");
			}
		}

		protected override void CheckAdditionalProcedureCodesAsString()
		{
			base.CheckAdditionalProcedureCodesAsString();

			if (Parent.AdditionalProcedureCodes.Cast<AdditionalProcedureCode>().Any(x => !JobComInvoiceLine.IsValidProcedureToApportion(x.CY_Code)))
			{
				Parent.AdditionalProcedureCodesAsStringInfo.AddWarning(ProcedureStopApportionWaring);
			}
		}

		protected override void CheckJI_CountryOfOrigin()
		{
			if (IsCountryOfOriginAndSupplyInvalid())
			{
				Parent?.JI_CountryOfOriginInfo.AddMessageError(CountryOfSupply_ErrorMessage);
			}

			if (Parent?.IsImport ?? false)
			{
				if (IsCountryOfOriginMissing())
				{
					Parent?.JI_CountryOfOriginInfo.AddMessageError(CountryOfOriginRequiredError);
				}

				Parent?.AddInfoValidation.ValidateZG_CountryOfSupply();
			}
		}

		bool IsCountryOfOriginMissing()
		{
			var parent = Parent;
			return parent != null && parent.IsImport && parent.JI_CountryOfOrigin.IsEmpty &&
				   (parent?.EntryInstruction?.CEI_Style.ToString() ?? string.Empty) != ImportDeclarationTypeList.Codes.DeclarationForCustomsWarehousing;
		}

		public bool IsCountryOfOriginAndSupplyInvalid()
		{
			var parent = Parent;
			return parent != null && !parent.ZG_CountryOfSupply.IsEmpty && parent.JI_CountryOfOrigin.IsEmpty;
		}

		protected override void CheckJI_NetWeight()
		{
			if (!Parent.JI_NetWeight.IsEmpty && !Parent.JI_NetWeightUQ.IsEmpty
				&& !Parent.JI_Weight.IsEmpty && !Parent.JI_WeightUQ.IsEmpty
				&& Enterprise.Core.Constants.Weight.ContainsCode(Parent.JI_WeightUQ)
				&& Enterprise.Core.Constants.Weight.ContainsCode(Parent.JI_NetWeightUQ))
			{
				var zNetWeight = new ZWeight(Parent.JI_NetWeight, Parent.JI_NetWeightUQ);
				var zGrossWeight = new ZWeight(Parent.JI_Weight, Parent.JI_WeightUQ);
				if (zGrossWeight.IsValid && zNetWeight.IsValid && zGrossWeight < zNetWeight)
				{
					Parent.JI_NetWeightInfo.AddMessageError(NetWeightIsGreaterThanGrossWeight);
				}
			}
		}

		protected override void CheckJI_CEI()
		{
			base.CheckJI_CEI();
			var parent = Parent;

			if (parent.EntryInstruction?.CEI_Style.Equals(ImportDeclarationTypeList.Codes.ReducedDataSetDeclaration) ?? false)
			{
				if (!parent.IsNorthernIrelandDomestic || parent.IsAtRisk)
				{
					parent.JI_CEIInfo.AddMessageError(GB_NIMovements);
				}
			}
		}

		protected override void CheckJI_CustomsQuantity()
		{
			base.CheckJI_CustomsQuantity();
			if (!Parent.JI_CustomsQuantity.IsEmpty && !Parent.JI_CustomsUnitQty.IsEmpty
				&& !Parent.JI_Weight.IsEmpty && !Parent.JI_WeightUQ.IsEmpty
				&& Enterprise.Core.Constants.Weight.ContainsCode(Parent.JI_WeightUQ)
				&& Enterprise.Core.Constants.Weight.ContainsCode(Parent.JI_CustomsUnitQty))
			{
				var grossMass = new ZWeight(Parent.JI_Weight, Parent.JI_WeightUQ);
				var customsMass = new ZWeight(Parent.JI_CustomsQuantity, Parent.JI_CustomsUnitQty);
				if (grossMass.IsValid && customsMass.IsValid && grossMass < customsMass)
				{
					Parent.JI_CustomsQuantityInfo.AddMessageError(CustomsQtyLessThanGrossWeight);
				}
			}
		}

		protected override void CheckJI_CustomsSecondQuantity()
		{
			base.CheckJI_CustomsSecondQuantity();
			if (!Parent.JI_CustomsQuantity.IsEmpty && !Parent.JI_CustomsUnitQty.IsEmpty
				&& !Parent.JI_CustomsSecondQuantity.IsEmpty && !Parent.JI_CustomsSecondUnitQty.IsEmpty
				&& ConvertibleUnitsOfMeasure.WeightConversionDictionary.ContainsKey(Parent.JI_CustomsUnitQty)
				&& ConvertibleUnitsOfMeasure.WeightConversionDictionary.ContainsKey(Parent.JI_CustomsSecondUnitQty))
			{
				var customsQuantityConverter = new EU.Business.Declaration.CustomsQuantityConverter(Parent, Parent.JI_CustomsQuantityInfo, Parent.JI_CustomsUnitQtyInfo);
				var customsSecondQuantityConverter = new EU.Business.Declaration.CustomsQuantityConverter(Parent, Parent.JI_CustomsSecondQuantityInfo, Parent.JI_CustomsSecondUnitQtyInfo);

				var customMass = new ZWeight(Parent.JI_CustomsQuantity, customsQuantityConverter.GetEffectiveWeightUnit(Parent.JI_CustomsUnitQty));
				var customSecondMass = new ZWeight(Parent.JI_CustomsSecondQuantity, customsSecondQuantityConverter.GetEffectiveWeightUnit(Parent.JI_CustomsSecondUnitQty));
				var customQuantityInKg = Enterprise.ZArchitecture.Core.Utilities.Round(Enterprise.Core.Constants.Weight.Convert(customMass.Amount, customMass.Unit, Enterprise.Core.Constants.Weight.Kilograms), 3);
				var customSecondQuantityInKg = Enterprise.Core.Constants.Weight.Convert(customSecondMass.Amount, customSecondMass.Unit, Enterprise.Core.Constants.Weight.Kilograms);

				if (customQuantityInKg != customSecondQuantityInKg)
				{
					Parent.JI_CustomsSecondQuantityInfo.AddMessageError(CustomsSecondQtyMismatch);
				}
			}
		}

		protected override void CheckJI_RN_NKCountryOfExport()
		{
			var declaration = Parent.Declaration;
			if (declaration != null && declaration.IsImport && !Parent.JI_RN_NKCountryOfExport.IsEmpty)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.JI_RN_NKCountryOfExportInfo);
			}
		}

		protected override bool IsTariffMandatory
		{
			get
			{
				var decTypeMeansTariffIsForbidden = IsEntryInstructionClearanceRequest;
				var cusProcedure = Parent.CusProcedure;
				var cpcSaysTariffIsMandatory = cusProcedure == null || (!cusProcedure.HasAttribute(Universal.AttributeNames.Codes.PersonalEffects) && !cusProcedure.HasAttribute(Universal.AttributeNames.Codes.CfspNcgds) && !cusProcedure.HasAttribute(Universal.AttributeNames.Codes.HSCodeOptional)); // See https://www.gov.uk/hmrc-internal-manuals/customs-freight-simplified-procedures/cfsp08100 and https://www.gov.uk/hmrc-internal-manuals/customs-freight-simplified-procedures/cfsp05150
				return !decTypeMeansTariffIsForbidden && cpcSaysTariffIsMandatory;
			}
		}

		public new JobComInvoiceLine Parent => (JobComInvoiceLine)base.Parent;

		protected override IEnumerable<ZString> PreviousDocumentsCheckExceptedDeclarationTypes
		{
			get
			{
				return new ZString[]
				{
					ExportSADDeclarationTypeList.Codes.ExitSummaryDeclaration,
					ImportSADDeclarationTypeList.Codes.ImportSupplementaryDeclaration
				};
			}
		}

		protected override void CheckJI_Tariff_NoPackage_Import(EU.Business.Declaration.JobDeclaration euDeclaration)
		{
			if (euDeclaration is JobDeclaration dec && dec.JE_DeclarationType != ImportSADDeclarationTypeList.Codes.ImportSupplementaryDeclaration
													&& dec.JE_EntrySubStyle != EntrySubStyleListImport.Codes.SdpSupplementaryDeclarationOrSdpAndOrLcpEidrFinalSupplementaryDeclaration)
			{
				base.CheckJI_Tariff_NoPackage_Import(dec);
			}
		}

		protected override void CheckJI_Tariff_NoPackage_Export(EU.Business.Declaration.JobDeclaration euDeclaration)
		{
			var dec = (JobDeclaration)euDeclaration;
			if (dec.JE_DeclarationType != ExportSADDeclarationTypeList.Codes.ExitSummaryDeclaration)
			{
				base.CheckJI_Tariff_NoPackage_Export(dec);
			}
		}

		public const string GB_NIMovements = "H8 is only used with GB-NI movements and only for not-at-risk";
		public const string NetWeightIsGreaterThanGrossWeight = "Net mass should be less than gross mass.";
		public const string CustomsQtyLessThanGrossWeight = "Customs quantity (net mass) should be less than gross mass.";
		const string ProcedureStopApportionWaring = "No group charges or invoice charges would be apportioned into invoice lines whose procedure code ends with E01 or E02";
		public const string ValuationCodeSimplifiedProcedure_ErrorMessage = "Simplified Procedure Value (SPV, code '7') is now declared using an Additional Procedure Code 'E01' in DE 1/11 and Valuation Method Code '4' should be declared in this field.";
		public const string CountryOfSupply_ErrorMessage = "When country/region of supply [UCC 5/15] is supplied, country/region of origin [UCC 5/16] must be present";
		const string CountryOfOriginRequiredError = "[UCC 5/15,16] Country/Region of Origin is required";
		public const string GrossWeightValidationError = "[UCC 6/5] Gross Weight (GWT) value for the associated entry line cannot be zero and must be less than 10000000000000000 KG";
		public const string CustomsSecondQtyMismatch = "Mass in 6/2, when converted into KGM and rounded to three decimal places, mismatches the value in 6/1.  CDS does not allow this. Please see the published CDS known-differences list, reference KD161, for advice.";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "EDI012:UnmaintainableProductName_CSharp", Justification = "Baseline issue")]
		public string ThisTariffIsNotInTheEnterpriseDatabaseMessageError = string.Format("This tariff is not in the {0} database. Please update ediTariff, export the tariff records to {0} and use ediTariff for the lookup. Please note that the data displayed by ediTariff for classification purposes and the data it exports to {0} for measure calculations have two different sources which are updated with different frequencies. Therefore there may be a mismatch between what ediTariff displays and what {0} receives, including missing measure (i.e. tax and documents) details.", BrandingFactory.Instance.ProductName);
		public const string E00662 = "Item description must be entered";

		bool IsEntryInstructionClearanceRequest => GBDeclaration?.CusEntryInstruction?.IsClearanceRequest ?? false;

		protected override INotificationType TariffNoAdditionalCodesNotificationType => CustomsDataRegistry.Instance.GetTariffAdditionalCodeNotificationType(Parent.RegistryBranchPK);

		protected override void CheckTariffCodesForMeursingDuty() { }
	}
}
