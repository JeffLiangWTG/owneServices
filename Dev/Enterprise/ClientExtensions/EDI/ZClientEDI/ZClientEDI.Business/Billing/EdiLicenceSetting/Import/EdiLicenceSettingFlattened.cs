using CargoWise.EntityFramework;
using CargoWise.Types;
using Res = ZClientEDI.Business.Res;

namespace Enterprise.Client.EDI.Billing.Business
{
	public class EdiLicenceSettingFlattened : AutoEdiLicenceSettingFlattened
	{
		public EdiLicenceSettingFlattened()
		{
			ApplyDiscounts = true;
			LicenceUnits = -1;
		}

		public bool IsDiscountPercentProvided { get; private set; }
		public bool IsDiscountActiveProvided { get; private set; }
		public bool IsPriceProvided { get; private set; }
		public bool IsBWPurchasedLicencesProvided { get; private set; }

		public override ZString DiscountPercentAsText
		{
			set
			{
				base.DiscountPercentAsText = value;

				if (!DiscountPercentAsText.IsEmpty)
				{
					ZDecimal decimalValue;
					if (ZDecimal.TryParse(DiscountPercentAsText, out decimalValue))
					{
						IsDiscountPercentProvided = true;
						DiscountPercent = decimalValue;
					}
					else
					{
						DiscountPercentInfo.AddError(Res.GetString("78b687c7-26c4-496e-bf95-92e013ae7a6e", "{0} is not a valid number", DiscountPercentAsText));
					}
				}
			}
		}

		public override ZString PriceAsText
		{
			set
			{
				base.PriceAsText = value;

				if (!PriceAsText.IsEmpty)
				{
					ZDecimal decimalValue;
					if (ZDecimal.TryParse(PriceAsText, out decimalValue))
					{
						IsPriceProvided = true;
						Price = decimalValue;
					}
					else
					{
						PriceInfo.AddError(Res.GetString("78b687c7-26c4-496e-bf95-92e013ae7a6e", "{0} is not a valid number", PriceAsText));
					}
				}
			}
		}

		public override ZString BWPurchasedLicencesAsText
		{
			set
			{
				base.BWPurchasedLicencesAsText = value;

				if (!BWPurchasedLicencesAsText.IsEmpty)
				{
					ZDecimal decimalValue;
					if (ZDecimal.TryParse(BWPurchasedLicencesAsText, out decimalValue))
					{
						IsBWPurchasedLicencesProvided = true;
						BWPurchasedLicences = decimalValue;
					}
					else
					{
						BWPurchasedLicencesInfo.AddError(Res.GetString("78b687c7-26c4-496e-bf95-92e013ae7a6e", "{0} is not a valid number", BWPurchasedLicencesAsText));
					}
				}
			}
		}

		public override ZString DiscountActiveAsText
		{
			set
			{
				base.DiscountActiveAsText = value;

				if (!DiscountActiveAsText.IsEmpty)
				{
					ZBool boolValue;
					if (ZBool.TryParse(DiscountActiveAsText, out boolValue))
					{
						IsDiscountActiveProvided = true;
						DiscountActive = boolValue;
					}
					else
					{
						DiscountActiveInfo.AddError(Res.GetString("2b2b6efd-704b-46cb-beba-e956a6b0e8d7", "{0} is not a valid boolean value", DiscountActiveAsText));
					}
				}
			}
		}

		#region Validation

		public override void ValidateValidFrom()
		{
			ValidFromInfo.ClearAllNotifications();
			TypeValidation.CheckValidZDateTimeWithoutRange(ValidFromInfo);
		}

		public override void ValidateValidTo()
		{
			ValidToInfo.ClearAllNotifications();
			TypeValidation.CheckValidZDateTimeWithoutRange(ValidToInfo);
		}

		#endregion
	}
}


