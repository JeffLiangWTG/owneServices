using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business
{
	public class TaxIdAndTaxMessageMappingHelper : ITaxIdAndTaxMessageMappingHelper
	{
		public ResourceString ValidateMapping(string lineType, AccTaxRate taxId, AccInvMsg taxMessage)
		{
			return CreateValidator(GlbCompany.CurrentCompany.PK).IsValidateMapping(new[] { lineType }, taxId, taxMessage) ? null : ResString.GetMultilingualString("D581F2DC-1430-4EA3-9F46-55BDA18CD876", ErrorMsgTemplate, lineType, taxId?.AT_Code, taxMessage?.A9_Code);
		}

		public ResourceString ValidateMappingForLine(AccTransactionLines line)
		{
			if (line.TransactionHeader != null
				&& line.TransactionHeader.AH_TransactionBelongsToGroup.IsValid
				&& line.TransactionHeader.AH_IsCancelled)
			{
				return null;
			}
			else if (line.IsInDatabase && !line.AL_LineTypeInfo.OriginalValue.Equals(TransactionLineTypes.UnapprovedCost))
			{
				return null;
			}
			else if (line.HasContext(BusinessContext.SkipTaxIdAndTaxMessageMappingValidation))
			{
				return null;
			}

			return ValidateMapping(line.AL_LineType, line.TaxRate, line.VATClass);
		}

		public ResourceString ValidateMappingForTaxOverride(string[] lineTypes, AccChargeTaxOverride chargeTaxOverride)
		{
			var taxId = chargeTaxOverride.TaxRate;
			var taxMessage = chargeTaxOverride.DefaultVATClass;
			var companyPK = chargeTaxOverride.ChargeCode != null
				? chargeTaxOverride.ChargeCode.AC_GC
				: GlbCompany.CurrentCompany.PK;

			if (!CreateValidator(companyPK).IsValidateMapping(lineTypes, taxId, taxMessage))
			{
				var errorLines = string.Join(
					System.Environment.NewLine,
					(lineTypes ?? new string[] { null }).Select(x => ResString.GetMultilingualString("5650C362-9F2F-491F-850F-7360015EF820", ErrorLine, x, taxId?.AT_Code, taxMessage?.A9_Code)));
				return ResString.GetMultilingualString("863E21F0-486E-4D3C-A042-B1F55F1D7B57", ErrorMsgTemplateForTaxOverride, errorLines);
			}

			return null;
		}

#if DEBUG
		public TaxIdAndTaxMessageMappingValidator CreateValidator_ForTestOnly(ZGuid companyPK) => CreateValidator(companyPK);

#endif
		TaxIdAndTaxMessageMappingValidator CreateValidator(ZGuid companyPK)
		{
			TaxIdAndTaxMessageMappingValidator validator = null;
			var registry = AccountingMasterFilesRegistry.Instance.TaxIdAndTaxMessageCombinationRules.GetFallBackValueAtAllLevels(companyPK.ToGuid(), Guid.Empty, Guid.Empty);
			var option = registry.ValidationOption;
			var rules = registry.TaxIdAndTaxMessageCombinationRulesCollection;

			switch (option)
			{
				case AccountingMasterFilesConstants.TaxIDAndTaxMessageValidationOption.TaxID:
					validator = new TaxIdAndTaxMessageMappingLimitTaxIDsValidator(rules);
					break;
				case AccountingMasterFilesConstants.TaxIDAndTaxMessageValidationOption.TaxMessage:
					validator = new TaxIdAndTaxMessageMappingLimitTaxMessagesValidator(rules);
					break;
				default:
					throw new InvalidOperationException("Invalid TaxIdAndTaxMessageMappingValidationOptions");
			}

			return validator;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Used in ResString.GetMultilingualString() above")]
		const string ErrorMsgTemplate = @"Transaction cannot be saved/posted due to invalid Tax ID and Tax Message combination.
Line Type={0}, Tax ID={1}, Tax Message={2}";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Used in ResString.GetMultilingualString() above")]
		const string ErrorMsgTemplateForTaxOverride = @"Tax Override cannot be saved due to invalid Tax ID and Tax Message combination.
{0}";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Used in ResString.GetMultilingualString() above")]
		const string ErrorLine = @"Line Type={0}, Tax ID={1}, Tax Message={2}";
	}
}
