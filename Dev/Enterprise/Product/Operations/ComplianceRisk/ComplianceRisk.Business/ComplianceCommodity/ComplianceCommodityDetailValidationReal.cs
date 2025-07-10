using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ComplianceRisk.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ComplianceRisk.Business
{
	public class ComplianceCommodityDetailValidationReal : ComplianceCommodityDetailValidation
	{
		public ComplianceCommodityDetailValidationReal(ComplianceCommodityDetail parent) : base(parent)
		{
		}

		#region Parent

		public new ComplianceCommodityDetail Parent
		{
			get { return base.Parent; }
		}

		#endregion

		protected override void CheckCCD_Description()
		{
			RemoveDuplicateRowErrorIfNeeded();

			if (!Parent.ReadOnly && !Parent.CCD_DescriptionInfo.ReadOnly)
			{
				base.CheckCCD_Description();
				CheckUniqueOfHsCodeAndDescriptionAndOriginIfNeeded();
			}
		}

		protected override void CheckCCD_HarmonizedCode()
		{
			RemoveDuplicateRowErrorIfNeeded();

			if (!Parent.ReadOnly && !Parent.CCD_HarmonizedCodeInfo.ReadOnly)
			{
				base.CheckCCD_HarmonizedCode();
				MandatoryValidation.CheckEntered(Parent.CCD_HarmonizedCodeInfo);
				ValidateHarmonizedCode();
				CheckUniqueOfHsCodeAndDescriptionAndOriginIfNeeded();
			}
		}

		public override void CheckCommodityStatus()
		{
			if (Parent.HasRowWarnings)
			{
				Parent.RemoveRowWarning(GetMessages.RiskCheckInProgressMessage);
				Parent.RemoveRowWarning(GetMessages.ReviewComplianceAlertMessage);
				Parent.RemoveRowWarning(GetMessages.UnsupportedCountryMessage, true);
				Parent.RemoveRowWarning(GetMessages.UnsupportedHarmonizedMessage);
				Parent.RemoveRowWarning(GetMessages.HarmonizedMatchedNotCompleteMessage, true);
				Parent.RemoveRowWarning(GetMessages.ComplianceRuleAdministratorMessage);
			}

			if (Parent.BorderWiseCheckInProgress)
			{
				Parent.AddRowWarning(GetMessages.RiskCheckInProgressMessage);
			}

			if (Parent.AssessmentInitialized)
			{
				if (!Parent.IsValidHsCode)
				{
					Parent.AddRowWarning(GetMessages.UnsupportedHarmonizedMessage);
				}

				if (Parent.HasUnsupportedCountries)
				{
					Parent.AddRowWarning(GetMessages.UnsupportedCountryMessage + string.Join(", ", Parent.Countries
						.Where(u => !u.Supported)
						.Select(u => Parent.Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, u.CountryCode)?.Description)));
				}

				if (Parent.HasMatchedHsCode && Parent.MatchedHsCode != Parent.CCD_HarmonizedCode )
				{
					Parent.AddRowWarning(GetMessages.HarmonizedMatchedNotCompleteMessage + Parent.MatchedHsCode);
				}

				if (Parent.CCD_SpecificCondition)
				{
					Parent.AddRowWarning(GetMessages.ReviewComplianceAlertMessage);
				}

				if (Parent.BlockedByComplianceRule && Parent.CCD_RiskStatus.HasCommodityRiskFactor())
				{
					Parent.AddRowWarning(GetMessages.ComplianceRuleAdministratorMessage);
				}
			}
		}

		protected override void CheckCCD_RN_NKOrigin()
		{
			RemoveDuplicateRowErrorIfNeeded();

			if (!Parent.ReadOnly && !Parent.CCD_RN_NKOriginInfo.ReadOnly)
			{
				base.CheckCCD_RN_NKOrigin();
				ListValidation.ErrorIfInvalidCode(Parent.CCD_RN_NKOriginInfo, Parent.Lookups.Origins);
				CheckUniqueOfHsCodeAndDescriptionAndOriginIfNeeded();
			}
		}

		string NotAllowDuplicateRecordsMessagePrefix => Res.GetString("F41465C8-B65E-4C9A-8F15-3313D14A76F4", "Harmonized Code, Goods Description and Origin of Goods must be unique.");

		void RemoveDuplicateRowErrorIfNeeded()
		{
			if (!RunningValidateAll)
			{
				Parent.RemoveRowError(NotAllowDuplicateRecordsMessagePrefix);
			}
		}

		void CheckUniqueOfHsCodeAndDescriptionAndOriginIfNeeded()
		{
			// For performance consideration, only check when not running validate all
			if (!RunningValidateAll && !Parent.HasRowErrors)
			{
				if (Parent.ComplianceRiskStatus?.CommodityDetailCollection != null)
				{
					var commodities = ComplianceCheckRequestModelBuilder.GetApplicableCommodityDetails(Parent.ComplianceRiskStatus.CommodityDetailCollection.Cast<ComplianceCommodityDetail>());
					if (commodities.Any(u =>
					u.PK != Parent.PK
					&& u.CCD_HarmonizedCode.EqualsIgnoringCase(Parent.CCD_HarmonizedCode)
					&& u.CCD_CountryOrGrouping == Parent.CCD_CountryOrGrouping
					&& u.CCD_RN_NKOrigin.EqualsIgnoringCase(Parent.CCD_RN_NKOrigin)
					&& u.CCD_Description.EqualsIgnoringCase(Parent.CCD_Description)))
					{
						Parent.AddRowError(NotAllowDuplicateRecordsMessagePrefix);
					}
				}
			}
		}

		void ValidateHarmonizedCode()
		{
			if (!Parent.CCD_HarmonizedCode.IsEmpty && !Regex.IsMatch(Parent.CCD_HarmonizedCode, "^[0-9]+$"))
			{
				var notification = Res.GetString("AF2E4AAD-4C59-4B52-B7AC-AA99B9B21C9E", "Invalid Harmonized Code. Only numeric characters are allowed.");

				if (Parent.IsInDatabase && !Parent.CCD_HarmonizedCodeInfo.HasChanges)
				{
					Parent.CCD_HarmonizedCodeInfo.AddWarning(notification);
				}
				else
				{
					Parent.CCD_HarmonizedCodeInfo.AddError(notification);
				}
			}
		}

		public static class GetMessages
		{
			public static ZString HarmonizedMatchedNotCompleteMessage => Res.GetString("8D3F6DF3-5580-404E-8E0B-328EC81C934E", "This Harmonized Code is not recognized. The Compliance Check will be performed on: ");
			public static ZString UnsupportedHarmonizedMessage => Res.GetString("A86FF110-4397-4C5A-9FF5-1F6FCC78A3AA", "Unable to provide Compliance Alerts because the Harmonized Code is not supported.");
			public static ZString RiskCheckInProgressMessage => Res.GetString("223790BC-7104-47E1-8B7C-8DCEFDAD3288", "Risk Check in progress");
			public static ZString ReviewComplianceAlertMessage => Res.GetString("90E51181-7E44-43EA-BA7A-7C8FCE2AA42C", "Review Compliance Alerts.");
			public static ZString ComplianceRuleAdministratorMessage => Res.GetString("FEAA5E1B-D15F-41F6-AE64-E7522DA9011F", "Compliance Rule set by administrator applies to this commodity.");
			public static ZString UnsupportedCountryMessage => Res.GetString("61B46114-6C74-4AC8-800B-A775E79CBCCC", "Unable to provide Compliance Alerts for the following countries/regions: ");
		}
	}
}
