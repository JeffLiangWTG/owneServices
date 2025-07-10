//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoFeatureControlRuleValidation
//
//    This class should be used for overriding validation in AutoFeatureControlRuleValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using System;
using System.Linq;
using System.Text.Json;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;

namespace Enterprise.Client.EDI.FeatureControl.Business
{
	public class FeatureControlRuleValidation : AutoFeatureControlRuleValidation
	{
		public FeatureControlRuleValidation(AutoFeatureControlRule parent) : base(parent)
		{
		}

		public new FeatureControlRule Parent => base.Parent as FeatureControlRule;

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateFCR_FCS_FeatureSet();
		}

		protected override void CheckFCR_Description()
		{
			base.CheckFCR_Description();
			MandatoryValidation.CheckEntered(Parent.FCR_DescriptionInfo);
			if (Parent.ControlHeader.FeatureControlRules.OfType<FeatureControlRule>().Any(x => x.PK != Parent.PK && x.FCR_Description.EqualsIgnoringCase(Parent.FCR_Description)))
			{
				Parent.FCR_DescriptionInfo.AddError("The rule description must be unique.");
			}
		}

		#region Parameters

		protected override void CheckFCR_Parameters()
		{
			base.CheckFCR_Parameters();
			if (Parent.FCR_Parameters.Length > 1024 * 1024)
			{
				Parent.FCR_ParametersInfo.AddError("The amount of data in the parameters field must not exceed 1MB. Please adjust and try again.");
			}

			if (!Parent.FCR_ParametersInfo.HasErrors() && !Parent.FCR_Parameters.IsEmpty && !IsValidJsonString(Parent.FCR_Parameters, out var errorMessage))
			{
				Parent.FCR_ParametersInfo.AddError(errorMessage);
			}
		}

		protected override void CheckFCR_ParametersIsNotEmpty()
		{
		}

		static bool IsValidJsonString(string input, out string errorMessage)
		{
			errorMessage = null;
			try
			{
				_ = JsonDocument.Parse(input);
				return true;
			}
			catch (Exception ex)
			{
				errorMessage = ex.Message;
				return false;
			}
		}

		#endregion

		protected override void CheckFCR_EndDateUtc()
		{
			base.CheckFCR_EndDateUtc();
			if (Parent.FCR_EndDateUtc.IsValid && Parent.FCR_StartDateUtc.IsValid && Parent.FCR_EndDateUtc <= Parent.FCR_StartDateUtc)
			{
				Parent.FCR_EndDateUtcInfo.AddError("The end date cannot be earlier than the start date.");
			}
		}

		protected override void CheckFCR_EndDateUtcIsValidZDateTimeRange()
		{
			new DateRangeValidation().Validate(Parent.FCR_EndDateUtcInfo, false, false);
		}

		protected override void CheckFCR_StartDateUtcIsValidZDateTimeRange()
		{
			new DateRangeValidation().Validate(Parent.FCR_StartDateUtcInfo, false, false);
		}

		protected override void CheckFCR_IsActive()
		{
			base.CheckFCR_IsActive();
			CheckActiveGlobalRule(Parent.FCR_IsActiveInfo);
			CheckFeatureSetRule(Parent.FCR_IsActiveInfo);
		}

		protected override void CheckFCR_RuleType()
		{
			base.CheckFCR_RuleType();
			CheckActiveGlobalRule(Parent.FCR_RuleTypeInfo);
			CheckFeatureSetRule(Parent.FCR_RuleTypeInfo);
		}

		#region Global Rule

		protected override void CheckFCR_UseGlobalParameters()
		{
			base.CheckFCR_UseGlobalParameters();
			if (Parent.IsGlobalRule && Parent.FCR_UseGlobalParameters)
			{
				Parent.FCR_UseGlobalParametersInfo.AddError("The global rule is unable to assign this flag.");
			}

			if (!Parent.IsGlobalRule && Parent.FCR_UseGlobalParameters &&
					!Parent.ControlHeader.FeatureControlRules.OfType<FeatureControlRule>()
						.Any(x => x.IsGlobalRule))
			{
				Parent.FCR_UseGlobalParametersInfo.AddError("There is no global control rule.");
			}
		}

		void CheckActiveGlobalRule(ZPropertyInfo propertyInfo)
		{
			var rules = Parent.ControlHeader.FeatureControlRules.OfType<FeatureControlRule>();
			if (Parent.IsGlobalRule && rules.Any(x => x.PK != Parent.PK && x.IsGlobalRule))
			{
				propertyInfo.AddError("Only one global control rule is allowed.");
			}

			if (Parent.IsGlobalRule && Parent.FCR_IsActive && rules.Any(x => x.PK != Parent.PK && x.IsFeatureSetRule && x.FCR_IsActive))
			{
				propertyInfo.AddError("Active global control rule is not allowed because there are active feature set rules.");
			}

			if (rules.Any(x => x.FCR_UseGlobalParameters) && !rules.Any(x => x.IsGlobalRule))
			{
				propertyInfo.AddError("This global rule cannot be changed because its parameters are currently in use by other rules.");
			}

			if (Parent.IsGlobalRule && Parent.LicenceDatabasePivots.Count > 0)
			{
				propertyInfo.AddError("A global rule cannot have license databases attached.");
			}
		}

		#endregion

		#region Feature Set

		protected override void CheckFCR_FCS_FeatureSet()
		{
			base.CheckFCR_FCS_FeatureSet();
			if (Parent.IsFeatureSetRule && Parent.FeatureSet != null)
			{
				var rules = Parent.ControlHeader.FeatureControlRules.OfType<FeatureControlRule>();
				if (rules.Any(x => x.FCR_FCS_FeatureSet == Parent.FCR_FCS_FeatureSet && x.PK != Parent.PK))
				{
					Parent.FCR_FCS_FeatureSetInfo.AddError($"A rule with same feature code already exists in feature set {Parent.FeatureSet.FCS_ProductName}.");
				}
			}

			if (!Parent.IsFeatureSetRule && Parent.FeatureSet != null)
			{
				Parent.FCR_FCS_FeatureSetInfo.AddError("Feature set is not allowed.");
			}
			else if (Parent.IsFeatureSetRule && Parent.FeatureSet == null)
			{
				Parent.FCR_FCS_FeatureSetInfo.AddError("Feature set is required.");
			}
		}

		void CheckFeatureSetRule(ZPropertyInfo propertyInfo)
		{
			var rules = Parent.ControlHeader.FeatureControlRules.OfType<FeatureControlRule>();
			if (Parent.IsFeatureSetRule && Parent.FCR_IsActive && rules.Any(x => x.PK != Parent.PK && x.IsGlobalRule && x.FCR_IsActive))
			{
				propertyInfo.AddError("Active feature set rule is not allowed because there is active global rule.");
			}

			if (Parent.IsFeatureSetRule && Parent.LicenceDatabasePivots.Count > 0)
			{
				propertyInfo.AddError("A feature set rule cannot have license databases attached.");
			}
		}

		#endregion
	}
}
