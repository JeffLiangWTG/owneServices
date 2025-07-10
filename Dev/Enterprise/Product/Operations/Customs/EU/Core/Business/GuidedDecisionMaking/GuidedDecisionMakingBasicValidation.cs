using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.EU.Business
{
	public class GuidedDecisionMakingBasicValidation : AutoGuidedDecisionMakingBasicValidation
	{
		public GuidedDecisionMakingBasicValidation(AutoGuidedDecisionMakingBasic parent) : base(parent)
		{
			validationInternals = this;
			exportConfiguration = Parent.GetNewExportValidationConfiguration();
			importConfiguration = Parent.GetNewImportValidationConfiguration();
		}

		public new GuidedDecisionMakingBasic Parent
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (GuidedDecisionMakingBasic)base.Parent; }
		}

		protected readonly IGuidedDecisionMakingBasicValidationConfiguration exportConfiguration;
		protected readonly IGuidedDecisionMakingBasicValidationConfiguration importConfiguration;

		public void ValidateEffectiveDate()
		{
			validationInternals.Validate(Parent.EffectiveDateInfo, new RunValidationInvoker(CheckEffectiveDate));
		}

		void CheckEffectiveDate()
		{
			MandatoryValidation.CheckEntered(Parent.EffectiveDateInfo);
			TypeValidation.CheckValidZDateTimeWithoutRange(Parent.EffectiveDateInfo);
		}

		protected override void CheckTariffCode()
		{
			MandatoryValidation.CheckEntered(Parent.TariffCodeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.TariffCodeInfo);

			var (controlConditionCheck, informationConditionCheck) = (Parent?.ParentBusinessObject as JobComInvoiceLine)?.Validation?.GetCheckClassConditionsResults()
																	 ?? (string.Empty, string.Empty);
			CheckClassConditions(controlConditionCheck, informationConditionCheck);
		}

		void CheckClassConditions(ZString controlConditionCheck, ZString informationConditionCheck)
		{
			var targetInfo = Parent.TariffCodeInfo;
			if (!controlConditionCheck.IsEmpty)
			{
				targetInfo.AddNotification(CargoWise.EntityFramework.NotificationType.MessageError, controlConditionCheck);
			}

			if (!informationConditionCheck.IsEmpty)
			{
				targetInfo.AddNotification(CargoWise.EntityFramework.NotificationType.Warning, informationConditionCheck);
			}
		}

		protected override void CheckCountryOfOrigin()
		{
			if ((exportConfiguration != null && exportConfiguration.IsCountryOfOriginRequired) || (importConfiguration != null && importConfiguration.IsCountryOfOriginRequired))
			{
				MandatoryValidation.CheckEntered(Parent.CountryOfOriginInfo);
			}

			ListValidation.ErrorIfInvalidCode(Parent.CountryOfOriginInfo);
		}

		protected override void CheckCountryOfDestination()
		{
			if ((exportConfiguration != null && exportConfiguration.IsCountryOfDestinationRequired) || (importConfiguration != null && importConfiguration.IsCountryOfDestinationRequired))
			{
				MandatoryValidation.CheckEntered(Parent.CountryOfDestinationInfo);
			}

			ListValidation.ErrorIfInvalidCode(Parent.CountryOfDestinationInfo);
		}

		protected override void CheckPreference()
		{
			if (((exportConfiguration != null && exportConfiguration.IsPreferenceRequired) || (importConfiguration != null && importConfiguration.IsPreferenceRequired)) && Parent.Lookups.PreferenceList.Count > 0)
			{
				MandatoryValidation.CheckEntered(Parent.PreferenceInfo);
				ListValidation.ErrorIfInvalidCode(Parent.PreferenceInfo);
			}
		}

		protected override void CheckQuotaOrderNumber()
		{
			if (((exportConfiguration != null && exportConfiguration.IsQuotaOrderNumberRequired) || (importConfiguration != null && importConfiguration.IsQuotaOrderNumberRequired)) && Parent.Lookups.QuotaOrderNumberList.Count > 0)
			{
				MandatoryValidation.CheckEntered(Parent.QuotaOrderNumberInfo);
				ListValidation.ErrorIfInvalidCode(Parent.QuotaOrderNumberInfo);
			}
		}

		protected override void CheckCustomsFirstQuantity()
		{
			if (Parent.CustomsFirstQuantity <= 0)
			{
				Parent.CustomsFirstQuantityInfo.AddWarning(Res.GetString("F0BAFF3F-2AC5-42AB-88FD-DB47B71022ED", "Net Weight must be greater than zero."));
			}
		}

		protected override void CheckCustomsSecondQuantity()
		{
			if (!Parent.CustomsSecondQuantityRequireNoCU2)
			{
				if (Parent.CustomsSecondQuantity <= 0)
				{
					Parent.CustomsSecondQuantityInfo.AddWarning(Res.GetString("956E3499-F52E-42CC-B73B-4B98264FD7C9", "Supplementary Quantity must be greater than zero."));
				}
				else if (Parent.CustomsSecondUnitQty.IsEmpty)
				{
					Parent.CustomsSecondQuantityInfo.AddWarning(Res.GetString("FCDA603A-3481-4E23-9646-8698CFFE7774", "Supplementary Quantity must have a valid unit. Please select valid Country Of Origin to get valid unit."));
				}
			}
		}

		protected override void CheckCustomsThirdQuantity()
		{
			if (!Parent.CustomsThirdQuantityRequireNoCU3)
			{
				var info = Parent.CustomsThirdQuantityInfo;
				if (Parent.CustomsThirdQuantity < 0)
				{
					info.AddWarning(Res.GetString("90CF13EE-BC42-4580-A8A0-6B0156AAC4F5", "Third Quantity can not be less than zero."));
				}

				if (Parent.CustomsThirdQuantity > 0 && Parent.CustomsThirdUnitQty.IsEmpty)
				{
					info.AddWarning(Res.GetString("2962E2D2-1FE8-4A82-8FE5-FDF1253A1A10", "The unit must be given when Third Quantity is greater than zero."));
				}

				if (Parent.CustomsThirdQuantity == 0 && !Parent.CustomsThirdUnitQty.IsEmpty)
				{
					info.AddWarning(Res.GetString("127A2431-D604-4379-A332-E523B62567DE", "Third Quantity must be greater than zero when The unit is given."));
				}
			}
		}

		protected override void CheckMeursingResult()
		{
			base.CheckMeursingResult();
			MandatoryValidation.WarnIfNotEntered(Parent.MeursingResultInfo);
			ListValidation.WarnIfInvalidCode(Parent.MeursingResultInfo);
		}
		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		protected readonly IValidationInternals validationInternals;
	}
}
