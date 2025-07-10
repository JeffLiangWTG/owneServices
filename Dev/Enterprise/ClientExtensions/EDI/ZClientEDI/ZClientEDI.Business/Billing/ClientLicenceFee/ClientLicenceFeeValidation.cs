using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.Billing.Business
{
	public class ClientLicenceFeeValidation : AutoClientLicenceFeeValidation
	{
		public ClientLicenceFeeValidation(AutoClientLicenceFee parent) : base(parent)
		{
		}

		protected new ClientLicenceFee Parent
		{
			get { return (ClientLicenceFee)base.Parent; }
		}

		protected override void CheckL8_Type()
		{
			base.CheckL8_Type();
			MandatoryValidation.CheckEntered(Parent.L8_TypeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.L8_TypeInfo);
		}

		protected override void CheckL8_Description()
		{
			base.CheckL8_Description();
			MandatoryValidation.CheckEntered(Parent.L8_DescriptionInfo);
		}

		protected override void CheckL8_Amount()
		{
			base.CheckL8_Amount();
			MandatoryValidation.CheckEntered(Parent.L8_AmountInfo);
			if (Parent.L8_Amount == 0)
			{
				Parent.L8_AmountInfo.AddError("Cannot be zero.");
			}
		}

		protected override void CheckL8_ChargeCode()
		{
			base.CheckL8_ChargeCode();
			MandatoryValidation.CheckEntered(Parent.L8_ChargeCodeInfo);
			if (!Parent.L8_ChargeCode.IsEmpty)
			{
				var validCodes = GetValidChargeCodes(Parent.Factory);

				if (!validCodes.ContainsCode(Parent.L8_ChargeCode))
				{
					string msg = "ChargeCode must be active, of Charge Type NON or REV, and with Charge Group NGC or NJR in at least one company.";
					if (Parent.L8_ChargeCodeInfo.HasChanges || !Parent.IsInDatabase)
					{
						Parent.L8_ChargeCodeInfo.AddError(msg);
					}
					else
					{
						Parent.L8_ChargeCodeInfo.AddWarning(msg);
					}
				}
			}
		}

		public static ReadOnlyCodeDescriptionPairList GetValidChargeCodes(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("ClientLicenceFee.ChargeCodes", () =>
			{
				return LoadChargeCodes(factory);
			});
		}

		protected override void CheckL8_IsDiscountable()
		{
			base.CheckL8_IsDiscountable();
			if (Parent.L8_IsDiscountable && (Parent.L8_IsDiscountableInfo.HasChanges || Parent.L8_ChargeCodeInfo.HasChanges) && Parent.DiscountChargeCode.IsEmpty)
			{
				var regItem = EDIDataRegistry.Instance.FeeBillingDiscountChargeCodes;

				Parent.L8_IsDiscountableInfo.AddError("Fee charge code " + Parent.L8_ChargeCode + " missing discount charge code in registry "
					+ string.Join(" > ", regItem.Categories) + " > " + regItem.Caption);
			}
		}

		static ReadOnlyCodeDescriptionPairList LoadChargeCodes(BusinessObjectFactory factory)
		{
			var query = new ZQuery(AccChargeCodeSchema.AC_IsActive, true);
			query.AddToFilter(AccChargeCodeSchema.AC_ChargeType, new string[]
			{
				Core.Constants.ChargeType.Revenue,
				Core.Constants.ChargeType.NonAccrual
			});
			query.AddToFilter(AccChargeCodeSchema.AC_ChargeGroup, new string[]
			{
				ChargeCodeGroupList.Codes.NonJobRelated,
				ChargeCodeGroupList.Codes.NotGrouped
			});

			var result = new CodeDescriptionPairList();
			foreach (var code in factory.Load<AccChargeCode>(query)
				.Select(x => (string)x.AC_Code)
				.Distinct()
				.OrderBy(x => x))
			{
				result.AddPair(code);
			}

			return result;
		}

		protected override void CheckL8_StartDate()
		{
			base.CheckL8_StartDate();
			if (!Parent.IsInDatabase || Parent.L8_StartDateInfo.HasChanges)
			{
				MandatoryValidation.CheckEntered(Parent.L8_StartDateInfo);
			}
			CompareValidation.CheckDateIsBeforeAnotherDate(Parent.L8_StartDateInfo, Parent.L8_EndDateInfo);
			if (!Parent.L8_StartDate.IsEmpty && Parent.L8_StartDate.IsValid && Parent.L8_StartDate.Day != 1)
			{
				Parent.L8_StartDateInfo.AddError("Fee should start on the first day of the month.");
			}
		}

		protected override void CheckL8_EndDateIsValidZDateTimeRange()
		{
			var info = Parent.L8_EndDateInfo;
			if (!Parent.IsInDatabase || info.HasChanges)
			{
				TypeValidation.CheckValidZDateTimeRange(info, new TypeValidationLimits()
				{
					FutureYearsBeforeError = 15,
					FutureYearsBeforeWarning = 7,
					PastYearsBeforeError = 10,
					PastYearsBeforeWarning = 1
				});
			}
		}

		protected override void CheckL8_EndDate()
		{
			base.CheckL8_EndDate();
			CompareValidation.CheckDateIsAfterAnotherDate(Parent.L8_EndDateInfo, Parent.L8_StartDateInfo);
			if (!Parent.L8_EndDate.IsEmpty && Parent.L8_EndDate.IsValid && Parent.L8_EndDate.Month == Parent.L8_EndDate.AddDays(1).Month)
			{
				Parent.L8_EndDateInfo.AddError("Fee should end on the last day of the month.");
			}
		}

		protected override void CheckL8_RenewalMonths()
		{
			base.CheckL8_RenewalMonths();
			MandatoryValidation.CheckEntered(Parent.L8_RenewalMonthsInfo);
			MandatoryValidation.CheckNotZero(Parent.L8_RenewalMonthsInfo);
			MandatoryValidation.CheckNotNegative(Parent.L8_RenewalMonthsInfo);
		}

		protected override void CheckL8_OH_RemitToOrg()
		{
			base.CheckL8_OH_RemitToOrg();
			if (Parent.L8_OH_RemitToOrg.IsValid)
			{
				var remitToOrg = Parent.RemitToOrg as EDIOrgHeader;
				if (remitToOrg != null && remitToOrg.LicCompany == null)
				{
					Parent.L8_OH_RemitToOrgInfo.AddError("Remit to orgnisation should have a licence company for billing.");
				}
			}
		}

		protected override void CheckL8_RX_NKCurrency()
		{
			base.CheckL8_RX_NKCurrency();
			MandatoryValidation.CheckEntered(Parent.L8_RX_NKCurrencyInfo);
			ListValidation.ErrorIfInvalidCode(Parent.L8_RX_NKCurrencyInfo);
		}

		protected override void CheckL8_SystemCode()
		{
			MandatoryValidation.CheckEntered(Parent.L8_SystemCodeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.L8_SystemCodeInfo);
		}

		protected override void CheckL8_TaxDateCode()
		{
			MandatoryValidation.CheckEntered(Parent.L8_TaxDateCodeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.L8_TaxDateCodeInfo);
		}
	}
}

