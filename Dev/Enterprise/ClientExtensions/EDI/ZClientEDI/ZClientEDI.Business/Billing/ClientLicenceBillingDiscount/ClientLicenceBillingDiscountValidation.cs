using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Client.EDI.Billing.Business
{
	public class ClientLicenceBillingDiscountValidation : AutoClientLicenceBillingDiscountValidation
	{
		public ClientLicenceBillingDiscountValidation(AutoClientLicenceBillingDiscount parent) : base(parent)
		{
		}

		#region Parent

		public new ClientLicenceBillingDiscount Parent
		{
			get { return (ClientLicenceBillingDiscount)base.Parent; }
		}

		#endregion

		protected override void CheckL5_SystemCode()
		{
			base.CheckL5_SystemCode();
			MandatoryValidation.CheckEntered(Parent.L5_SystemCodeInfo);
			if (!Parent.L5_SystemCodeInfo.HasChanges && Parent.IsInDatabase)
			{
				ListValidation.WarnIfInvalidCode(Parent.L5_SystemCodeInfo);
			}
			else
			{
				ListValidation.ErrorIfInvalidCode(Parent.L5_SystemCodeInfo);

				if (!Parent.L5_SystemCodeInfo.HasErrors() && Parent.IsCapped && Parent.L5_SystemCode != BillingConstants.BillingSystem.ODM)
				{
					Parent.L5_SystemCodeInfo.AddError("Only ODM system supports capped discount.");
				}

				if ((Parent.L5_SystemCode == BillingConstants.BillingSystem.AirlineMessaging
					|| Parent.L5_SystemCode == BillingConstants.BillingSystem.NZCustoms
					|| Parent.L5_SystemCode == BillingConstants.BillingSystem.PortMessaging
					|| Parent.L5_SystemCode == BillingConstants.BillingSystem.ShippingPortMessaging
					|| Parent.L5_SystemCode == BillingConstants.BillingSystem.ClientMapping)
					&& Parent.IsCommitment)
				{
					var message = "Commitment discounts are not supported on this system.";
					if (Parent.IsInDatabase)
					{
						Parent.L5_SystemCodeInfo.AddWarning(message);
					}
					else
					{
						Parent.L5_SystemCodeInfo.AddError(message);
					}
				}

				ValidateBreakUnits();
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		protected override void CheckL5_Type()
		{
			base.CheckL5_Type();
			MandatoryValidation.CheckEntered(Parent.L5_TypeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.L5_TypeInfo);

			if (!Parent.L5_TypeInfo.HasErrors() && Parent.Parent != null)
			{
				if (Parent.IsCommitment)
				{
					foreach (ClientLicenceBillingDiscount billingDiscount in Parent.Parent.BillingDiscounts)
					{
						if (billingDiscount != Parent &&
							billingDiscount.L5_SystemCode == Parent.L5_SystemCode &&
							billingDiscount.L5_DiscountCode == Parent.L5_DiscountCode)
						{
							if (billingDiscount.IsCommitment && HaveOverlappingDates(Parent, billingDiscount))
							{
								Parent.L5_TypeInfo.AddError("You can't have commitment discounts with overlapping dates.");
								return;
							}
							else if (billingDiscount.IsPrepayment && HaveOverlappingDates(Parent, billingDiscount))
							{
								Parent.L5_TypeInfo.AddError("You can't have commitment and prepayment discounts at the same time.");
								return;
							}
						}
					}
				}
				else if (Parent.IsPrepayment)
				{
					foreach (ClientLicenceBillingDiscount billingDiscount in Parent.Parent.BillingDiscounts)
					{
						if (billingDiscount != Parent &&
							billingDiscount.L5_SystemCode == Parent.L5_SystemCode &&
							billingDiscount.L5_DiscountCode == Parent.L5_DiscountCode)
						{
							if (billingDiscount.IsPrepayment && HaveOverlappingDates(Parent, billingDiscount))
							{
								Parent.L5_TypeInfo.AddError("You can't have prepayment discounts with overlapping dates.");
								return;
							}
							else if (billingDiscount.IsCommitment && HaveOverlappingDates(Parent, billingDiscount))
							{
								Parent.L5_TypeInfo.AddError("You can't have commitment and prepayment discounts at the same time.");
								return;
							}
						}
					}
				}
				else if (Parent.IsCapped)
				{
					if (!Parent.L5_DiscountCode.IsEmpty)
					{
						Parent.L5_TypeInfo.AddError("Standard discounts cannot be commitment discounts.");
					}
				}
				else if (Parent.IsWiseCloud)
				{
					if (Parent.L5_SystemCode != BillingConstants.BillingSystem.ODM)
					{
						Parent.L5_TypeInfo.AddError("WiseCloud discounts are applicable to ODM only.");
					}
				}

				ValidateBreakUnits();
			}
		}

		protected override void CheckL5_ModuleCode()
		{
			base.CheckL5_ModuleCode();
			if (Parent.IsModuleSpecific)
			{
				MandatoryValidation.CheckEntered(Parent.L5_ModuleCodeInfo);
				if (!Parent.IsInDatabase || Parent.L5_ModuleCodeInfo.HasChanges)
				{
					ListValidation.ErrorIfInvalidCode(Parent.L5_ModuleCodeInfo);
				}
				else
				{
					ListValidation.WarnIfInvalidCode(Parent.L5_ModuleCodeInfo);
				}
			}
		}

		protected override void CheckL5_BreakAmount()
		{
			base.CheckL5_BreakAmount();
			if (!Parent.L5_BreakAmount_ReadOnly && !Parent.IsCapped)
			{
				MandatoryValidation.CheckEntered(Parent.L5_BreakAmountInfo);
			}
			CompareValidation.CheckNumberNotNegative(Parent.L5_BreakAmountInfo);

			if (!Parent.L5_BreakAmountInfo.HasErrors() && Parent.IsVolume && Parent.Parent != null)
			{
				foreach (ClientLicenceBillingDiscount billingDiscount in Parent.Parent.BillingDiscounts)
				{
					if (billingDiscount.L5_Type == Parent.L5_Type
						&& billingDiscount != Parent
						&& billingDiscount.L5_BreakAmount == Parent.L5_BreakAmount
						&& billingDiscount.L5_SystemCode == Parent.L5_SystemCode
						&& billingDiscount.L5_DiscountCode == Parent.L5_DiscountCode
						&& HaveOverlappingDates(Parent, billingDiscount))
					{
						Parent.L5_BreakAmountInfo.AddError("You can't have two similar volume break amounts.");
						break;
					}
				}
			}
		}

		protected override void CheckL5_BreakUnits()
		{
			base.CheckL5_BreakUnits();
			MandatoryValidation.CheckEntered(Parent.L5_BreakUnitsInfo);
			ListValidation.ErrorIfInvalidCode(Parent.L5_BreakUnitsInfo);
			if ((Parent.L5_BreakUnits == BillingConstants.DiscountBreakUnit.LicenceUnits)
				&& (Parent.L5_Type != BillingConstants.DiscountType.Volume)
				&& (Parent.L5_Type != BillingConstants.DiscountType.Commitment))
			{
				Parent.L5_BreakUnitsInfo.AddError("Break units of type licence-units is only applicable for volume discount or commitment discount");
			}

			ValidateBreakUnits();
		}

		protected override void CheckL5_Units()
		{
			base.CheckL5_Units();
			if (!Parent.L5_Units_ReadOnly)
			{
				if (Parent.L5_Type == BillingConstants.DiscountType.MinimumFee
					&& Parent.L5_SystemCode == BillingConstants.BillingSystem.ODM)
				{
					CompareValidation.CheckNumberNotNegative(Parent.L5_UnitsInfo);
				}
				else
				{
					MandatoryValidation.CheckEntered(Parent.L5_UnitsInfo);
					CompareValidation.CheckNumberGreaterThanZero(Parent.L5_UnitsInfo);

					if (!Parent.L5_UnitsInfo.HasErrors() && Parent.IsIncrementalVolume && Parent.Parent != null)
					{
						foreach (ClientLicenceBillingDiscount billingDiscount in Parent.Parent.BillingDiscounts)
						{
							if (billingDiscount.L5_Type == Parent.L5_Type
								&& billingDiscount != Parent
								&& billingDiscount.L5_Units == Parent.L5_Units
								&& billingDiscount.L5_SystemCode == Parent.L5_SystemCode
								&& billingDiscount.L5_DiscountCode == Parent.L5_DiscountCode
								&& HaveOverlappingDates(Parent, billingDiscount))
							{
								Parent.L5_UnitsInfo.AddError("You can't have two equal incremental volume units.");
								break;
							}
						}
					}
				}
			}
		}

		protected override void CheckL5_Discount()
		{
			base.CheckL5_Discount();
			if (!Parent.IsMinimumFee)
			{
				if (!Parent.IsCommitment)
				{
					MandatoryValidation.CheckEntered(Parent.L5_DiscountInfo);

					if (Parent.IsSurcharge)
					{
						CompareValidation.CheckLessThanOrEqualTo(Parent.L5_DiscountInfo, 0m);
					}
					else
					{
						CompareValidation.CheckNumberGreaterThanZero(Parent.L5_DiscountInfo);
					}
				}

				CompareValidation.CheckLessThanOrEqualTo(Parent.L5_DiscountInfo, 100m);
			}
		}

		protected override void CheckL5_Description()
		{
			base.CheckL5_Description();
			if (Parent.IsSpecial || Parent.IsSurcharge)
			{
				MandatoryValidation.CheckEntered(Parent.L5_DescriptionInfo);
			}
		}

		protected override void CheckL5_StartDate()
		{
			base.CheckL5_StartDate();
			CompareValidation.CheckDateIsBeforeAnotherDate(Parent.L5_StartDateInfo, Parent.L5_EndDateInfo);
			if (!Parent.L5_StartDate.IsEmpty && Parent.L5_StartDate.IsValid && Parent.L5_StartDate.Day != 1)
			{
				Parent.L5_StartDateInfo.AddError("Discount should start on the first day of the month.");
			}
		}

		protected override void CheckL5_StartDateIsValidZDateTimeRange()
		{
			TypeValidation.CheckValidZDateTimeRange(Parent.L5_StartDateInfo, new TypeValidationLimits()
			{
				FutureYearsBeforeError = 10,
				FutureYearsBeforeWarning = 5,
				PastYearsBeforeError = 50,
				PastYearsBeforeWarning = 15
			});
		}

		protected override void CheckL5_EndDate()
		{
			base.CheckL5_EndDate();
			CompareValidation.CheckDateIsAfterAnotherDate(Parent.L5_EndDateInfo, Parent.L5_StartDateInfo);
			if (!Parent.L5_EndDate.IsEmpty && Parent.L5_EndDate.IsValid && Parent.L5_EndDate.Month == Parent.L5_EndDate.AddDays(1).Month)
			{
				Parent.L5_EndDateInfo.AddError("Discount should end on the last day of the month.");
			}
		}

		protected override void CheckL5_EndDateIsValidZDateTimeRange()
		{
			TypeValidation.CheckValidZDateTimeRange(Parent.L5_EndDateInfo, new TypeValidationLimits()
			{
				FutureYearsBeforeError = 50,
				FutureYearsBeforeWarning = 15,
				PastYearsBeforeError = 50,
				PastYearsBeforeWarning = 15
			});
		}

		protected override void CheckL5_Duration()
		{
			if (Parent.L5_Duration > 0 && !Parent.L5_DiscountCode.IsEmpty)
			{
				Parent.L5_DurationInfo.AddError("Standard discounts cannot have a duration.");
			}
		}

		protected override void CheckL5_SubCode()
		{
			base.CheckL5_SubCode();
			ListValidation.ErrorIfInvalidCode(Parent.L5_SubCodeInfo);
			if (Parent.L5_SystemCode == BillingConstants.BillingSystem.ABMCustoms && Parent.IsCommitment)
			{
				MandatoryValidation.CheckEntered(Parent.L5_SubCodeInfo);
			}
		}

		#region Implementation

		public bool HaveOverlappingDates(ClientLicenceBillingDiscount discount1, ClientLicenceBillingDiscount discount2)
		{
			return HaveOverlap(discount1.L5_StartDate, discount1.L5_EndDate, discount2.L5_StartDate, discount2.L5_EndDate);
		}

		static bool HaveOverlap(ZDateTime start1, ZDateTime end1, ZDateTime start2, ZDateTime end2)
		{
			return (start2.IsEmpty || end1.IsEmpty || start2 <= end1)
				&& (start1.IsEmpty || end2.IsEmpty || start1 <= end2);
		}

		void ValidateBreakUnits()
		{
			if (!Parent.L5_BreakAmountInfo.HasErrors() && Parent.IsVolume && Parent.Parent != null)
			{
				foreach (ClientLicenceBillingDiscount billingDiscount in Parent.Parent.BillingDiscounts)
				{
					if (billingDiscount.L5_SystemCode == Parent.L5_SystemCode
						&& billingDiscount.L5_Type == BillingConstants.DiscountType.Volume
						&& billingDiscount.L5_Type == Parent.L5_Type
						&& billingDiscount.L5_BreakUnits != Parent.L5_BreakUnits
						&& billingDiscount != Parent
						&& HaveOverlappingDates(Parent, billingDiscount))
					{
						Parent.L5_BreakUnitsInfo.AddError("Volume break units must be the same for discount with the same volume type, system and overlapping date.");
						break;
					}
				}
			}
		}

		#endregion
	}
}

