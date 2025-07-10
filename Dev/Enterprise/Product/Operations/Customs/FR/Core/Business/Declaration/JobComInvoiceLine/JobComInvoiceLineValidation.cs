using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Extensions;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.FR.Messaging.MessageBuilders;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Customs.FR.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.FR.Business.Declaration
{
	public class JobComInvoiceLineValidation : EU.Business.Declaration.JobComInvoiceLineValidation
	{
		public JobComInvoiceLineValidation(JobComInvoiceLine parent)
			: base(parent)
		{
		}

		public new JobComInvoiceLine Parent => (JobComInvoiceLine)base.Parent;

		protected override void CheckTariffNoAdditionalCodes()
		{
			var parent = Parent;
			var taxOrFeeDetailEntities = parent.Lookups.TaxOrFeeDetailEntities;
			if (parent.JI_TaxOrFeeDetailEntity == null || !parent.JI_TaxOrFeeDetailEntity.AdditionalCode.IsEmpty || parent.Lookups.AdditionalCodesList.GetAllCodes().Any(supplementaryCode => !taxOrFeeDetailEntities.Any(vatCode => vatCode == supplementaryCode)))
			{
				base.CheckTariffNoAdditionalCodes();
			}
		}

		protected override INotificationType TariffNoAdditionalCodesNotificationType => CargoWise.EntityFramework.NotificationType.MessageError;

		protected override void CheckJI_CustomsSecondQuantity()
		{
			base.CheckJI_CustomsSecondQuantity();

			var invoiceLine = Parent;
			if (!invoiceLine.JI_CustomsSecondQuantity.IsEmpty && IsRuleNAT_235ActiveAndHasTariffBypassAdditionalInformation())
			{
				invoiceLine.JI_CustomsSecondQuantityInfo.AddMessageError(errorMessage_NAT_235);
			}
		}

		protected override void CheckJI_CustomsThirdQuantity()
		{
			base.CheckJI_CustomsThirdQuantity();

			var invoiceLine = Parent;
			if (!invoiceLine.JI_CustomsThirdQuantity.IsEmpty && invoiceLine.JI_CustomsSecondQuantity.IsEmpty)
			{
				invoiceLine.JI_CustomsThirdQuantityInfo.AddMessageError(Res.GetString("19A4DE62-8908-4F56-B394-0E9B8986F6AD", "A second QTY must be entered before entering a third QTY value."));
			}

			if (!invoiceLine.JI_CustomsThirdQuantity.IsEmpty && IsRuleNAT_235ActiveAndHasTariffBypassAdditionalInformation())
			{
				invoiceLine.JI_CustomsThirdQuantityInfo.AddMessageError(errorMessage_NAT_235);
			}
		}

		static string errorMessage_NAT_235 => Res.GetString("D1B750C6-E788-4DA4-8479-B1647DCE17DA", "[NAT_235] Add. Information starting with 'K' does not allow supplementary quantities.");

		bool IsRuleNAT_235ActiveAndHasTariffBypassAdditionalInformation()
		{
			var invoiceLine = Parent;
			return invoiceLine.Validation.ValidationDecider is IImportInvoiceLineValidationDecider { IsRuleNAT_235Active: true }
				&& (invoiceLine.AdditionalInfos.Cast<AdditionalInfo>().Any(x => x.CSI_Code.StartsWith("K"))
				|| (invoiceLine.InvoiceHeader?.AdditionalInfos.Cast<AdditionalInfo>().Any(x => x.CSI_Code.StartsWith("K")) ?? false)
				|| (invoiceLine.EntryInstruction?.AdditionalInfos.Cast<AdditionalInfo>().Any(x => x.CSI_Code.StartsWith("K")) ?? false)
				|| (invoiceLine.Declaration?.AdditionalInfos.Cast<AdditionalInfo>().Any(x => x.CSI_Code.StartsWith("K")) ?? false));
		}

		protected override void CheckJI_Tariff_NoPackage()
		{
		}

		protected override void CheckJI_CEI()
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.JI_CEIInfo);
		}

		protected override void CheckJI_Weight()
		{
			base.CheckJI_Weight();
			if (Parent.JI_Weight <= 0)
			{
				Parent.JI_WeightInfo.AddMessageError(MessageBuilderHelper.MessageGrossWeightGreaterThanZero);
			}
		}

		protected override void CheckJI_Description()
		{
			base.CheckJI_Description();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.JI_DescriptionInfo);
		}

		protected override void CheckJI_PrimaryPreference()
		{
			base.CheckJI_PrimaryPreference();

			if (Parent.Declaration.JE_MessageType == Customs.Business.JobMessageTypeList.Codes.Import)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.JI_PrimaryPreferenceInfo);
			}
		}

		protected override void CheckJI_CountryOfOrigin()
		{
			base.CheckJI_CountryOfOrigin();
			CheckRuleC0710_N01();
			CheckRuleNAT_105();
			CheckRuleNAT_254();
		}

		void CheckRuleC0710_N01()
		{
			var parent = Parent;
			var entryInstruction = parent.EntryInstruction;
			var subTypeToConsider = new ZString[] { EntrySubStyleList.Codes.NormalDeclaration,
				EntrySubStyleList.Codes.SimplifiedDeclaration,
				EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeA,
				EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeC,
				EntrySubStyleList.Codes.SupplementaryRecapitulativeDeclarationOfSimplifiedDeclarationsCoveredByCAndF,
				EntrySubStyleList.Codes.SupplementaryRecapitulativeDeclarationUnderTheProcedureCoveredUnderArticle182OfTheCode,
				EntrySubStyleList.Codes.SupplementaryDeclarationForCodeCOrCodeF,
				EntrySubStyleList.Codes.SupplementaryDeclarationPeriodic };

			if (parent.Validation.ValidationDecider is IImportInvoiceLineValidationDecider { IsRuleC0710_N01Active: true }
				&& parent.Declaration.JE_CustomsOffice.StartsWith(Core.Constants.CountryCodes.France)
				&& entryInstruction.CEI_Procedure != Core.Constants.Customs.Universal.RefCusProcedure.Codes.StorageDeclaration
				&& subTypeToConsider.Contains(entryInstruction.CEI_SubStyle)
				&& parent.JI_CountryOfOrigin.IsEmpty)
			{
				parent.JI_CountryOfOriginInfo.AddMessageError(Res.GetString("596004DA-E20D-4E3B-BE81-D8293ACCDC64", "[C0710_N01] The Country of Origin is mandatory."));
			}
		}

		void CheckRuleNAT_105()
		{
			var parent = Parent;
			if (parent.Validation.ValidationDecider is IImportInvoiceLineValidationDecider { IsRuleNAT_105Active: true }
				&& parent.Declaration.JE_EntryStyle == EntryStyleListImport.Codes.ImportNormal
				&& parent.Factory.IsMemberOfEU(parent.JI_CountryOfOrigin))
			{
				parent.JI_CountryOfOriginInfo.AddMessageError(Res.GetString("1D2F2AD5-7686-409B-BE2E-D325565F236E", "[NAT_105] Country of Origin can't be an EU one if declaration type is 'IM'."));
			}
		}

		void CheckRuleNAT_254()
		{
			var parent = Parent;
			var countryCodeToConsider = new ZString[] { Core.Constants.CountryCodes.Turkey, Core.Constants.CountryCodes.Andorra, Core.Constants.CountryCodes.SanMarino };
			var preference = parent.JI_PrimaryPreference;
			var countryOfOrigin = parent.JI_CountryOfOrigin;
			if (parent.Validation.ValidationDecider is IImportInvoiceLineValidationDecider { IsRuleNAT_254Active: true }
				&& (preference == Core.Constants.Customs.Universal.RefCusPreference.Codes._400 || preference == Core.Constants.Customs.Universal.RefCusPreference.Codes._420)
				&& !countryCodeToConsider.Contains(countryOfOrigin))
			{
				parent.JI_CountryOfOriginInfo.AddMessageError(Res.GetString("E9CF42F5-AD81-436C-9379-22DC36245C4C", "[NAT_254] Country of Origin must either be TR, AD or SM for Pref. Code '400' and '420'."));
			}
		}

		protected override void CheckJI_ValuationCode()
		{
			base.CheckJI_ValuationCode();
			if (Parent.IsImport && !Parent.Declaration.IsUCC6)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.JI_ValuationCodeInfo);
			}
		}

		protected override void CheckJI_CustomsThirdUnitQty()
		{
			base.CheckJI_CustomsThirdUnitQty();
			if (Parent.JI_CustomsThirdQuantity != 0 && Parent.JI_CustomsThirdUnitQty == "")
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.JI_CustomsThirdUnitQtyInfo);
			}
		}

		protected override void CheckJI_Tariff()
		{
			base.CheckJI_Tariff();
			if (Parent.Declaration != null)
			{
				if (Parent.JI_FormattedTariff.Trim().Length != 13 && Parent.Declaration.IsImport)
				{
					Parent.JI_FormattedTariffInfo.AddMessageError(Res.GetString("0230ed4e-5a69-4465-99ee-35c7a61b74f0", "Tariff should contain 10 numbers"));
				}
				if (Parent.JI_FormattedTariff.Trim().Length < 10 && Parent.Declaration.IsExport)
				{
					Parent.JI_FormattedTariffInfo.AddMessageError(Res.GetString("dcaf5bbb-4d45-4a41-b941-d37de8dc3c6e", "Tariff should contain at least 8 numbers"));
				}
			}
		}

		protected override void CheckJI_CustomsQuantity()
		{
			base.CheckJI_CustomsQuantity();
			if (Parent.JI_CustomsQuantity <= 0)
			{
				Parent.JI_CustomsQuantityInfo.AddMessageError(MessageBuilderHelper.MessageCustomsQuantityGreaterThanZero);
			}
			if (Parent.JI_Weight != 0)
			{
				if (Parent.JI_CustomsQuantity > Parent.GrossWeightInKG)
				{
					Parent.JI_CustomsQuantityInfo.AddMessageError(Res.GetString("59A416BA-6E67-45DA-B830-20759951B4D2", "Customs Quantity[38] can not be greater than GWT[35]"));
				}
			}
		}

		protected override void CheckJI_Procedure()
		{
			base.CheckJI_Procedure();
			CheckRuleNat_030(Parent.JI_ProcedureInfo);
			CheckRuleC0834_N02(Parent.JI_ProcedureInfo);
			CheckCPCAgainstEntryInstruction();

			if (Parent.CusProcedure != null && Parent.EntryInstruction != null && !Parent.CusProcedure.Groups.Contains(Parent.EntryInstruction.CEI_Style))
			{
				Parent.JI_ProcedureInfo.AddMessageError(Res.GetString("959EED78-B2E2-4A2D-8373-37114DC9209E", "The procedure you have selected is not matching the entry instruction"));
			}
		}

		protected void CheckCPCAgainstEntryInstruction()
		{
			var invoiceLine = Parent;
			if (invoiceLine.Validation.ValidationDecider is IImportInvoiceLineValidationDecider { ShouldValidateCPCAgainstEntryInstruction: true } && invoiceLine.EntryInstruction != null)
			{
				if(!invoiceLine.JI_Procedure.IsEmpty && (invoiceLine.JI_Procedure.Length < invoiceLine.EntryInstruction.CEI_Procedure.Length || !invoiceLine.JI_Procedure.StartsWith(invoiceLine.EntryInstruction.CEI_Procedure)))
				{
					invoiceLine.JI_ProcedureInfo.AddMessageError(Res.GetString("C4F1E0A2-3D7B-4F5A-8C9E-6D3B2F1A0D5C", $"Invoice line Requested Procedure {invoiceLine.JI_Procedure} does not match the Entry Instruction Requested Procedure {invoiceLine.EntryInstruction.CEI_Procedure}"));
				}
			}
		}

		void CheckRuleNat_030(ZPropertyInfo propertyInfo)
		{
			var invoiceLine = Parent;
			if (ValidationDecider is IImportInvoiceLineValidationDecider { IsRuleNAT_030Active: true }
				&& invoiceLine.JI_Calc_Concession == RefCusProcedure.Concession.F48
				&& !invoiceLine.FiscalReferences.Cast<CusFiscalReference>().Any(x => x.CFR_Code == FiscalReferenceCodeList.Codes.FR5_Vendor)
				&& !(invoiceLine.EntryInstruction?.FiscalReferences.Cast<CusFiscalReference>().Any(x => x.CFR_Code == FiscalReferenceCodeList.Codes.FR5_Vendor) ?? false))
			{
				propertyInfo.AddMessageError(Res.GetString("C89CCAF6-5537-4BE3-94CF-D28A19C940F1", "[NAT_030] Fiscal reference FR5 must be provided when using concession F48."));
			}
		}

		void CheckRuleC0834_N02(ZPropertyInfo propertyInfo)
		{
			var invoiceLine = Parent;
			if (invoiceLine.Validation.ValidationDecider is IImportInvoiceLineValidationDecider validationDecider
				&& validationDecider.IsRuleC0834_N02Active
				&& invoiceLine.JI_Calc_Concession == RefCusProcedure.Concession._1DP)
			{
				if (!invoiceLine.FiscalReferences.Cast<CusFiscalReference>().Any(x => IsSuitableFor1DPConcession(x.CFR_Code))
					&& !(invoiceLine.EntryInstruction?.FiscalReferences.Cast<CusFiscalReference>().Any(x => IsSuitableFor1DPConcession(x.CFR_Code)) ?? false))
				{
					propertyInfo.AddMessageError(Res.GetString("4AE692E9-3E1B-492D-981B-33CFA8F3F4AA", "[C0834_N02] For Concession 1DP, a fiscal reference of any type but FR5 must be served."));
				}
			}
		}

		bool IsSuitableFor1DPConcession(string fiscalReferenceCode) => fiscalReferenceCode == FiscalReferenceCodeList.Codes.FR1_Importer
																	|| fiscalReferenceCode == FiscalReferenceCodeList.Codes.FR2_Customer
																	|| fiscalReferenceCode == FiscalReferenceCodeList.Codes.FR3_TaxRepresentative
																	|| fiscalReferenceCode == FiscalReferenceCodeList.Codes.FR4_HolderOfDeferredPaymentAuth;

		protected override void CheckJI_LinePrice()
		{
			base.CheckJI_LinePrice();

			var invoiceLine = Parent;
			if (invoiceLine.Validation.ValidationDecider is IImportInvoiceLineValidationDecider validationDecider
				&& validationDecider.IsRuleNAT_240Active
				&& invoiceLine.HasFreeGoods
				&& invoiceLine.JI_LinePrice > 0)
			{
				invoiceLine.JI_LinePriceInfo.AddMessageError(Res.GetString("e876380f-4050-4381-ad7a-af350b41ba39", "[NAT_240] Invoice Line price must be equal to 0 EUR When Additional Code 0097 (free goods) is selected."));
			}
		}
	}
}
