//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoBarcodeRuleComponentValidation
//
//    This class should be used for overriding validation in AutoBarcodeRuleComponentValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------
using System;
using System.Globalization;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.BarcodeParsingEngine;

namespace Enterprise.BarcodeParsing.Business
{
	public class BarcodeRuleComponentValidation : AutoBarcodeRuleComponentValidation
	{
		public BarcodeRuleComponentValidation(AutoBarcodeRuleComponent parent)
			: base(parent)
		{
		}

		new BarcodeRuleComponent Parent
		{
			get { return (BarcodeRuleComponent)base.Parent; }
		}

		// persistent

		#region CheckBRC_ApplicationID

		protected override void CheckBRC_ApplicationID()
		{
			base.CheckBRC_ApplicationID();

			CheckApplicationIDEnteredIfPartialRule();
			CheckHasValidListItemIfApplicationIdentifierIsBoundToList();
			CheckApplicationIdentifierIsNotUsedByOtherPartialRules();
		}

		void CheckApplicationIDEnteredIfPartialRule()
		{
			if (!Parent.BRC_ApplicationIDInfo.HasErrors() && Parent.BRC_ApplicationID.IsEmpty)
			{
				var rule = Parent.Rule;
				if (rule != null && rule.IsPartialRule)
				{
					Parent.BRC_ApplicationIDInfo.AddError(Res.GetString("f924e27a-c150-49d7-972c-2d2b7fe44aad", "Application ID must be entered for Partial Rules."));
				}
			}
		}

		void CheckHasValidListItemIfApplicationIdentifierIsBoundToList()
		{
			if (!Parent.BRC_ApplicationIDInfo.HasErrors() && Parent.IsApplicationIdentifierBoundToList)
			{
				ListValidation.ErrorIfInvalidCode(Parent.BRC_ApplicationIDInfo);
			}
		}

		void CheckApplicationIdentifierIsNotUsedByOtherPartialRules()
		{
			if (!Parent.BRC_ApplicationIDInfo.HasErrors())
			{
				var rule = Parent.Rule;
				if (rule != null && rule.IsPartialRule)
				{
					var ruleSet = rule.RuleSet;
					if (ruleSet != null)
					{
						foreach (var component in ruleSet.Rules.Where(r => r.IsPartialRule).SelectMany(r => r.Components).Where(r => r.PK != Parent.PK))
						{
							if (component.BRC_ApplicationID == Parent.BRC_ApplicationID)
							{
								if (component.BRC_TargetField != Parent.BRC_TargetField)
								{
									if (component.BRC_Format != Parent.BRC_Format || component.BRC_MinLength != Parent.BRC_MinLength || component.BRC_MaxLength != Parent.BRC_MaxLength)
									{
										Parent.BRC_ApplicationIDInfo.AddError(Res.GetString("A7365C32-DFFC-4176-BA7A-83D7018DB82A",
											"This Application Identifier is already in use by Partial Rule '{0}' whose formatting is not consistent with this rule.", component.Rule.BRU_Name));
									}
									else
									{
										Parent.BRC_ApplicationIDInfo.AddWarning(Res.GetString("61545429-ca45-4395-bb98-e24c594187bf",
											"This Application Identifier is already in use by Partial Rule '{0}' which Targets '{1}'.", component.Rule.BRU_Name, component.TargetFieldDescription));
									}
								}
								else
								{
									Parent.BRC_ApplicationIDInfo.AddError(Res.GetString("F6AE7AE8-813D-4CD7-AABB-DE030B087288",
										"This Application Identifier is already in use by Partial Rule '{0}' which also Targets '{1}'.", component.Rule.BRU_Name, component.TargetFieldDescription));
								}
							}
						}
					}
				}
			}
		}

		#endregion

		#region CheckBRC_Format

		protected override void CheckBRC_Format()
		{
			base.CheckBRC_Format();

			MandatoryValidation.CheckEntered(Parent.BRC_FormatInfo);
			ListValidation.ErrorIfInvalidCode(Parent.BRC_FormatInfo);
			CheckBRC_FormatIsValidForTargetField();
		}

		#region CheckBRC_FormatIsValidForTargetField

		void CheckBRC_FormatIsValidForTargetField()
		{
			if (!Parent.BRC_FormatInfo.HasErrors() && !Parent.IsIgnored())
			{
				var barcodeParsingConsumer = Parent.Factory.GetBarcodeParsingConsumerFromModuleCode(Parent.Module);
				var validFormats = barcodeParsingConsumer.GetValidFieldFormatsForTargetField(Parent.IsGS1, Parent.BRC_TargetField) ?? Enumerable.Empty<FormatType>();
				var validFormatsList = validFormats.ToList();
				if (validFormatsList.Count > 0 && !validFormatsList.Contains(((IBarcodeRuleComponent)Parent).Format))
				{
					var errorMessage = validFormatsList.Count == 1
						? Res.GetString("d78edc97-cfd5-433c-a371-fc40d8bc283b", "Format for field '{0}' should be {1}", Parent.TargetFieldDescription, validFormatsList[0])
						: Res.GetString("44aed7d6-e124-46b9-8f7d-8de79770dae8", "Format for field '{0}' should be one of: {1}", Parent.TargetFieldDescription, string.Join(", ", validFormatsList));

					Parent.BRC_FormatInfo.AddError(errorMessage);
				}
			}
		}

		#endregion

		#endregion

		#region CheckBRC_MaxLength

		protected override void CheckBRC_MaxLength()
		{
			base.CheckBRC_MaxLength();
			BarcodeRuleWithLengthValidationHelper.CheckMaxLength(Parent);
		}

		#endregion

		#region CheckBRC_MinLength

		protected override void CheckBRC_MinLength()
		{
			base.CheckBRC_MinLength();
			BarcodeRuleWithLengthValidationHelper.CheckMinLength(Parent);
		}

		#endregion

		#region CheckBRC_Sequence

		protected override void CheckBRC_Sequence()
		{
			base.CheckBRC_Sequence();
			CheckSequenceIsEnteredAndUniqueForFullRuleAndNotEnteredForPartialRule();
		}

		void CheckSequenceIsEnteredAndUniqueForFullRuleAndNotEnteredForPartialRule()
		{
			var rule = Parent.Rule;
			if (rule != null)
			{
				if (!rule.IsPartialRule)
				{
					CheckSequenceIsEnteredAndUniqueForFullRule(rule);
				}
				else if (Parent.BRC_Sequence > 0)
				{
					Parent.BRC_SequenceInfo.AddError(Res.GetString("0cabf71c-e038-4b26-adfc-38a95888ad83", "Sequence no. should not be entered for partial rules."));
				}
			}
		}

		void CheckSequenceIsEnteredAndUniqueForFullRule(BarcodeRule rule)
		{
			if (Parent.BRC_Sequence <= 0)
			{
				Parent.BRC_SequenceInfo.AddError(Res.GetString("61655278-79aa-402b-b410-90dc9d337a4d", "A full barcode rule must have its components sequenced in order starting from 1."));
			}
			else
			{
				foreach (var component in rule.Components)
				{
					if (component.PK != Parent.PK && component.BRC_Sequence == Parent.BRC_Sequence)
					{
						Parent.BRC_SequenceInfo.AddError(Res.GetString("f66d6b80-2e59-478b-8957-6b837f100fc0", "Sequence no. must be unique."));
						break;
					}
				}
			}
		}

		#endregion

		#region CheckBRC_TargetField

		protected override void CheckBRC_TargetField()
		{
			base.CheckBRC_TargetField();

			MandatoryValidation.CheckEntered(Parent.BRC_TargetFieldInfo);
			ListValidation.ErrorIfInvalidCode(Parent.BRC_TargetFieldInfo);
			CheckBRC_TargetField_AllowsIgnoredIfFullRuleAndAtLeastOneComponentNotIgnored();
			CheckBRC_TargetField_IsNotAlreadySpecifiedInThisRule();
		}

		void CheckBRC_TargetField_AllowsIgnoredIfFullRuleAndAtLeastOneComponentNotIgnored()
		{
			if (!Parent.BRC_TargetFieldInfo.HasErrors() && Parent.IsIgnored())
			{
				var rule = Parent.Rule;
				if (rule != null)
				{
					if (rule.IsPartialRule)
					{
						Parent.BRC_TargetFieldInfo.AddError(Res.GetString("9c12a648-808a-4442-8fe5-1834bd4580a3",
							"Partial Rule Components must specify a Field to Target."));
					}
					else if (rule.Components.All(c => c.IsIgnored()))
					{
						Parent.BRC_TargetFieldInfo.AddError(Res.GetString("7e0d7bd4-23f1-4656-964d-5fa79c0b5b63",
							"At least one Component must specify a Field to Target."));
					}
				}
			}
		}

		void CheckBRC_TargetField_IsNotAlreadySpecifiedInThisRule()
		{
			if (!Parent.BRC_TargetFieldInfo.HasErrors() && !Parent.IsIgnored()) // multiple 'ignore' components are allowed
			{
				var rule = Parent.Rule;
				if (rule != null && !rule.IsPartialRule) // only full rules can have more than one component
				{
					foreach (var component in rule.Components)
					{
						if (component.PK != Parent.PK && component.BRC_TargetField.EqualsIgnoringCase(Parent.BRC_TargetField))
						{
							Parent.BRC_TargetFieldInfo.AddError(Res.GetString("d91e087d-bca6-41f8-84a5-a521700bb8f0",
								"The Target Field '{0}' cannot be specified more than once per Rule.", Parent.BRC_TargetField));
							break;
						}
					}
				}
			}
		}

		#endregion

		protected override void CheckBRC_Delimiter()
		{
			base.CheckBRC_Delimiter();
			ValidateDelimiter();
		}

		// calculated

		#region ValidateDelimiter

		void ValidateDelimiter()
		{
			ValidateCalculatedProperty(Parent.DelimiterForBindingInfo);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Added this Code after failure in DAT tests through BulkAnalyzer script(WI00610195)")]
		void CheckDelimiterForBinding()
		{
			var rule = Parent.Rule;
			if (rule != null && !Parent.IsGS1 && !Parent.BRC_Delimiter.IsEmpty && ((IBarcodeRule)rule).Terminator == Convert.ToChar(Parent.BRC_Delimiter).ToString(CultureInfo.InvariantCulture))
			{
				Parent.DelimiterForBindingInfo.AddError(Res.GetString("218C9504-5690-4A63-A5A9-E3960DAFE475", "Component delimiter should not be same as rule terminator."));
			}
		}

		#endregion

		#region ValidateLengthType

		public void ValidateLengthType()
		{
			ValidateCalculatedProperty(Parent.LengthTypeForBindingInfo);
		}

		protected void CheckLengthTypeForBinding()
		{
			var rule = Parent.Rule;
			if (rule != null && rule.IsAllComponentsFixedLength && (rule.IsGS1 ? rule.BRU_Terminator != BarcodeRule.GS1Terminator : rule.BRU_Terminator != BarcodeRule.EmptyTerminatorWithQuotes))
			{
				Parent.LengthTypeForBindingInfo.AddError(Res.GetString("8933107B-34FC-41FF-ACA7-5A0D7B087B7", "Terminator should not be used when all the Components are defined as Fixed length."));
			}
		}

		#endregion

		#region ValidateHasDelimiter

		public void ValidateHasDelimiter()
		{
			ValidateCalculatedProperty(Parent.HasDelimiterInfo);
		}

		protected void CheckHasDelimiter()
		{
			var rule = Parent.Rule;
			if (rule != null && Parent.HasDelimiter)
			{
				CheckGS1CannotHaveDelimiter(rule);
				CheckOnlyOneComponentCanHaveDelimiterIfMultiComponentDelimiter(rule);
			}
		}

		void CheckGS1CannotHaveDelimiter(BarcodeRule rule)
		{
			if (rule.IsGS1)
			{
				Parent.HasDelimiterInfo.AddError(Res.GetString("51f1a54f-2349-4505-bd9c-d637ce51e385", "GS1 rule should not have a delimiter specified."));
			}
		}

		void CheckOnlyOneComponentCanHaveDelimiterIfMultiComponentDelimiter(BarcodeRule rule)
		{
			if (!Parent.HasDelimiterInfo.HasErrors()
				&& rule.BRU_IsDelimiterMultiComponent
				&& rule.Components.Any(c => c.PK != Parent.PK && c.HasDelimiter))
			{
				Parent.HasDelimiterInfo.AddError(
					Res.GetString("43e2a920-661c-4394-a437-f1cab98dd55b",
					"Only one component can have a delimiter when using a Multi-Component delimiter."));
			}
		}

		#endregion

		#region ValidateIsDelimiterMultiComponent

		public void ValidateIsDelimiterMultiComponent()
		{
			ValidateCalculatedProperty(Parent.IsDelimiterMultiComponentInfo);
		}

		protected void CheckIsDelimiterMultiComponent()
		{
			if (Parent.IsDelimiterMultiComponent)
			{
				CheckValidRuleForMultiComponentDelimiter();
				CheckMultiComponentDelimiterOnLastComponent();
			}
		}

		void CheckValidRuleForMultiComponentDelimiter()
		{
			var rule = Parent.Rule;
			if (rule != null)
			{
				if (rule.IsGS1)
				{
					Parent.IsDelimiterMultiComponentInfo.AddError(Res.GetString("95813b9a-b302-46b2-b18b-8b94fd7bc9aa",
						"GS1 Rule Components should not have Multi-Component delimiters."));
				}
				else if (rule.IsPartialRule)
				{
					Parent.IsDelimiterMultiComponentInfo.AddError(Res.GetString("c91bba0c-fc2d-43ef-beea-5f1de9509496",
						"Partial rule components should not have Multi-Component delimiters."));
				}
			}
		}

		void CheckMultiComponentDelimiterOnLastComponent()
		{
			if (!Parent.IsDelimiterMultiComponentInfo.HasErrors() && Parent.IsLastComponent)
			{
				Parent.IsDelimiterMultiComponentInfo.AddError(Res.GetString("06c41d67-b1f3-4a8f-a68c-97744c9dc586",
					"The last rule component cannot be marked with a Multi-Component delimiter."));
			}
		}

		#endregion

		// ValidateAll

		#region ValidateAll

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateHasDelimiter();
			ValidateIsDelimiterMultiComponent();
			ValidateLengthType();
		}

		#endregion
	}
}
