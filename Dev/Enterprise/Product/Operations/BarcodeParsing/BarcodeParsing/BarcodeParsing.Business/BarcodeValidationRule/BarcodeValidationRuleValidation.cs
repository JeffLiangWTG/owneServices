using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.BarcodeParsingEngine;

namespace Enterprise.BarcodeParsing.Business
{
	public class BarcodeValidationRuleValidation : AutoBarcodeValidationRuleValidation
	{
		public BarcodeValidationRuleValidation(AutoBarcodeValidationRule parent) : base(parent)
		{
		}

		new BarcodeValidationRule Parent => (BarcodeValidationRule)base.Parent;

		#region CheckBVR_Prefix

		protected override void CheckBVR_Prefix()
		{
			base.CheckBVR_Prefix();
			if (!Parent.BVR_PrefixInfo.HasErrors() && Parent.BVR_Prefix.Length > 0 && Parent.IsDateFormat)
			{
				Parent.BVR_PrefixInfo.AddError(Res.GetString("17d1d592-8c4d-4dda-ba0b-2da651344a18", "Date format should not have prefix"));
			}
		}

		#endregion

		#region CheckBVR_MinLength

		protected override void CheckBVR_MinLength()
		{
			base.CheckBVR_MinLength();
			BarcodeRuleWithLengthValidationHelper.CheckMinLength(Parent);
		}

		#endregion

		#region CheckBVR_MaxLength

		protected override void CheckBVR_MaxLength()
		{
			base.CheckBVR_MaxLength();
			BarcodeRuleWithLengthValidationHelper.CheckMaxLength(Parent);
		}

		#endregion

		#region CheckBVR_Format

		protected override void CheckBVR_Format()
		{
			base.CheckBVR_Format();

			MandatoryValidation.CheckEntered(Parent.BVR_FormatInfo);
			ListValidation.ErrorIfInvalidCode(Parent.BVR_FormatInfo);
			CheckBVR_FormatIsValidForTargetField();
		}

		void CheckBVR_FormatIsValidForTargetField()
		{
			if (!Parent.BVR_FormatInfo.HasErrors())
			{
				var barcodeParsingConsumer = Parent.Factory.GetBarcodeParsingConsumerFromModuleCode(Parent.Module);
				var validFormats = barcodeParsingConsumer.GetValidFieldFormatsForTargetField(false, Parent.BVR_TargetField) ?? Enumerable.Empty<FormatType>();
				var validFormatsList = validFormats.ToList();
				if (validFormatsList.Count > 0 && !validFormatsList.Contains(Parent.Format))
				{
					var errorMessage = validFormatsList.Count == 1
						? Res.GetString("d78edc97-cfd5-433c-a371-fc40d8bc283b", "Format for field '{0}' should be {1}", Parent.TargetFieldDescription, validFormatsList[0])
						: Res.GetString("44aed7d6-e124-46b9-8f7d-8de79770dae8", "Format for field '{0}' should be one of: {1}", Parent.TargetFieldDescription, string.Join(", ", validFormatsList));

					Parent.BVR_FormatInfo.AddError(errorMessage);
				}
			}
		}

		#endregion

		#region CheckBVR_TargetField

		protected override void CheckBVR_TargetField()
		{
			base.CheckBVR_TargetField();

			MandatoryValidation.CheckEntered(Parent.BVR_TargetFieldInfo);
			ListValidation.ErrorIfInvalidCode(Parent.BVR_TargetFieldInfo);
			CheckBVR_TargetField_IsNotAlreadySpecifiedInThisRule();
			CheckBVR_TargetFieldIsValidForModule();
		}

		void CheckBVR_TargetField_IsNotAlreadySpecifiedInThisRule()
		{
			if (!Parent.BVR_TargetFieldInfo.HasErrors())
			{
				var ruleExistsForFields = Parent.RuleSet.ValidationRules.Any(r => r.PK != Parent.PK && r.BVR_TargetField.EqualsIgnoringCase(Parent.BVR_TargetField));
				if (ruleExistsForFields)
				{
					Parent.BVR_TargetFieldInfo.AddError(Res.GetString("5BE1561B-4F3C-4847-AD33-DD96E91E3378",
						"The Target Field '{0}' cannot be specified more than once per Rule Set.", Parent.BVR_TargetField));
				}
			}
		}

		void CheckBVR_TargetFieldIsValidForModule()
		{
			if (!Parent.BVR_TargetFieldInfo.HasErrors())
			{
				var barcodeParsingConsumer = Parent.Factory.GetBarcodeParsingConsumerFromModuleCode(Parent.Module);
				if (barcodeParsingConsumer is IBarcodeValidationRulesConsumer barcodeValidationRulesConsumer)
				{
					var errorMessage = barcodeValidationRulesConsumer.ValidateTargetFieldForValidationRules(Parent.BVR_TargetField);
					if (!string.IsNullOrWhiteSpace(errorMessage))
					{
						Parent.BVR_TargetFieldInfo.AddError(errorMessage);
					}
				}
			}
		}

		#endregion
	}
}
