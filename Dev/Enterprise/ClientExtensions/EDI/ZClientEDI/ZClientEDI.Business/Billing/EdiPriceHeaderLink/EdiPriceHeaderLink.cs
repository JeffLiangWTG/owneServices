using System.Data;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;

namespace Enterprise.Client.EDI.Billing.Business
{
	public class EdiPriceHeaderLink : AutoEdiPriceHeaderLink
	{
		public EdiPriceHeaderLink(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public LicenceDatabase Database
		{
			get { return Factory.Load<LicenceDatabase>(PHL_LD); }
		}

		public ClientLicencePriceHeader PriceHeader
		{
			get { return Factory.Load<ClientLicencePriceHeader>(PHL_L6); }
		}

		[RelatedBusinessObject("PriceHeader")]
		public override ZGuid PHL_L6
		{
			get
			{
				return base.PHL_L6;
			}
			set
			{
				base.PHL_L6 = value;
			}
		}

		[List("Lookups.PriceHeaderVersions")]
		public ZString PriceHeaderVersion
		{
			get
			{
				return priceHeaderVersion ?? (priceHeaderVersion = PriceHeader != null ? PriceHeader.L6_PricelistVersion : null);
			}
			set
			{
				var available = Lookups.PriceHeaderVersionMap;
				ClientLicencePriceHeader match;
				if (available.TryGetValue(value, out match))
				{
					PHL_L6 = match.PK;
				}

				priceHeaderVersion = value;
				PHL_L6Info.RefreshBinding();
			}
		}
		string priceHeaderVersion;

		public override ZDateTime PHL_ValidFrom
		{
			get
			{
				return base.PHL_ValidFrom;
			}

			set
			{
				base.PHL_ValidFrom = value;
				var db = Database;
				if (db != null)
				{
					db.InvalidateBillingModel();
				}
			}
		}

		public ZPropertyInfo PriceHeaderVersionInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(PriceHeaderVersion), x => PHL_L6Info); }
		}

		public bool IsDateRangeMatched(ZDateTime startDate, ZDateTime endDate)
		{
			return (PHL_ValidFrom.IsEmpty || startDate >= PHL_ValidFrom) && (PHL_ValidTo.IsEmpty || endDate <= PHL_ValidTo);
		}

		public bool IsDateRangeOverlap(ZDateTime startDate, ZDateTime endDate)
		{
			return LessThanOrEqual(startDate, PHL_ValidTo)
				&& LessThanOrEqual(PHL_ValidFrom, endDate);
		}

		[List("Lookups.VolumeCodes")]
		public override ZString PHL_VolumeCode
		{
			get => base.PHL_VolumeCode;
			set
			{
				base.PHL_VolumeCode = value;

				if (PHL_VolumeCode == EdiPriceHeaderLinkVolumeCodeList.Codes.HV)
				{
					PHL_VolumePercent = 50m;
				}
				else if (PHL_VolumeCode == EdiPriceHeaderLinkVolumeCodeList.Codes.LV)
				{
					PHL_VolumePercent = 150m;
				}
				else
				{
					PHL_VolumePercent = 100m;
				}
			}
		}

		[List("Lookups.CorePackCodes")]
		public override ZString PHL_CorePackCode
		{
			get => base.PHL_CorePackCode;
			set
			{
				base.PHL_CorePackCode = value;

				if (PHL_CoreUpliftPercent_ReadOnly)
				{
					PHL_CoreUpliftPercent = 0m;
				}
				else
				{
					PHL_CoreUpliftPercent = 50m;
				}
			}
		}

		public bool ShouldIncludeDiscount(ClientLicencePriceItem priceItem, ZString usageCountryCode, IStlDiscount discount)
		{
			Argument.NotNull(discount, nameof(discount));

			var shouldInclude = true;

			if (discount.HeaderDiscount.PHD_Type == BillingConstants.DiscountCalculator.Volume)
			{
				if (priceItem != null && priceItem.L7_Code == DatabaseUsage.ActiveUsersUsageCode
					&& PHL_CorePackCode == EdiPriceHeaderLinkCorePackCodeList.Codes.EX)
				{
					shouldInclude = false;
				}
			}
			else if (discount.HeaderDiscount.PHD_Type == BillingConstants.DiscountCalculator.SingleCountry)
			{
				shouldInclude = !usageCountryCode.IsEmpty &&
					usageCountryCode == ((SingleCountryDiscount)discount.HeaderDiscount.Config).Country;
			}

			return shouldInclude;
		}

		protected bool PHL_VolumePercent_ReadOnly => PHL_VolumeCode == EdiPriceHeaderLinkVolumeCodeList.Codes.STD;

		protected bool PHL_CoreUpliftPercent_ReadOnly => PHL_CorePackCode != EdiPriceHeaderLinkCorePackCodeList.Codes.UP;

		static bool LessThanOrEqual(ZDateTime d1, ZDateTime d2)
		{
			return d1.IsEmpty || d2.IsEmpty || d1 <= d2;
		}
	}
}

