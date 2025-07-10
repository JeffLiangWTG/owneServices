using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.EDI.Billing.Business
{
	public interface IStlDiscount
	{
		EdiPriceHeaderDiscount HeaderDiscount { get; }
		DiscountLicenceSetting Setting { get; }
		StlMonthlyUsage MonthlyUsage { get; }

		bool IsActive { get; }

		decimal Percentage { get; }

		bool HasRequiredOtherDiscounts(IEnumerable<IStlDiscount> potentialDiscountsForDatabase);

		bool RequiredByDependentDiscount { get; }
	}

	public class DiscountInfo
	{
		public void Init(EdiPriceHeaderDiscount headerDiscount, DiscountLicenceSetting setting, StlMonthlyUsage monthlyUsage, DatabaseCountryUserSet databaseCountryUsers = null, StlDiscountFinder discountFinder = null, BillingRunContext context = null)
		{
			HeaderDiscount = headerDiscount;
			Setting = setting;
			MonthlyUsage = monthlyUsage;
			DatabaseCountryUsers = databaseCountryUsers;
			DiscountFinder = discountFinder;
			Context = context;
		}

		public EdiPriceHeaderDiscount HeaderDiscount { get; private set; }
		public DiscountLicenceSetting Setting { get; private set; }
		public StlMonthlyUsage MonthlyUsage { get; private set; }
		public DatabaseCountryUserSet DatabaseCountryUsers { get; private set; }
		public StlDiscountFinder DiscountFinder { get; private set; }
		public BillingRunContext Context { get; private set; }
	}

	public abstract class StlDiscount : IStlDiscount
	{
		protected StlDiscount(DiscountInfo info)
		{
			headerDiscount = info.HeaderDiscount;
			setting = info.Setting;
			monthlyUsage = info.MonthlyUsage;
			databaseCountryUsers = info.DatabaseCountryUsers;
			DiscountFinder = info.DiscountFinder;
			context = info.Context;
		}

		readonly EdiPriceHeaderDiscount headerDiscount;
		readonly DiscountLicenceSetting setting;
		readonly StlMonthlyUsage monthlyUsage;
		readonly DatabaseCountryUserSet databaseCountryUsers;
		readonly BillingRunContext context;

		public StlDiscountFinder DiscountFinder { get; private set; }
		public virtual bool RequiredByDependentDiscount { get; }

		public EdiPriceHeaderDiscount HeaderDiscount
		{
			get { return headerDiscount; }
		}

		public DiscountLicenceSetting Setting
		{
			get { return setting; }
		}

		public StlMonthlyUsage MonthlyUsage
		{
			get { return monthlyUsage; }
		}

		public DatabaseCountryUserSet DatabaseCountryUsers
		{
			get { return databaseCountryUsers; }
		}

		public BillingRunContext Context
		{
			get { return context; }
		}

		public virtual bool IsActive
		{
			get { return true; }
		}

		public virtual bool HasRequiredOtherDiscounts(IEnumerable<IStlDiscount> potentialDiscountsForDatabase)
		{
			return true;
		}

		public virtual decimal Percentage
		{
			get { return SettingPercentageWithFallback; }
		}

		protected decimal SettingPercentageWithFallback
		{
			get
			{
				return Setting != null && Setting.LS9_Percent != 0m
					? Setting.LS9_Percent
					: HeaderDiscount.PHD_Percent;
			}
		}

		public static void BuildAvailableDiscounts(IEnumerable<IStlDiscount> potentialDiscounts, List<IStlDiscount> availableDiscounts)
		{
			availableDiscounts.Clear();
			foreach (var discount in potentialDiscounts)
			{
				if (discount.HasRequiredOtherDiscounts(potentialDiscounts)
					&& discount.IsActive)
				{
					availableDiscounts.Add(discount);
				}
			}
		}
	}

	public class PercentageStlDiscount : StlDiscount
	{
		public PercentageStlDiscount(DiscountInfo info)
			: base(info)
		{
		}
	}

	public class DevelopingCountryStlDiscount : StlDiscount
	{
		public DevelopingCountryStlDiscount(DiscountInfo info)
			: base(info)
		{
		}

		public override bool HasRequiredOtherDiscounts(IEnumerable<IStlDiscount> potentialDiscountsForDatabase)
		{
			var countryUsers = MonthlyUsage.DatabaseUsage.CountryUsers;
			var config = (CountryDiscount)HeaderDiscount.Config;
			var countryLine = config.Lines.Cast<CountryDiscountLine>().FirstOrDefault(x => x.Country == countryUsers.DevelopingRegionUserGroup.DomesticCountry);
			if (countryLine != null && countryLine.RequiresDomesticDiscount)
			{
				var other = (DomesticEntityStlDiscount)potentialDiscountsForDatabase.FirstOrDefault(x => x is DomesticEntityStlDiscount);
				return other != null && other.IsActive;
			}
			return true;
		}

		public override decimal Percentage
		{
			get
			{
				if (Setting != null && Setting.LS9_Percent != 0m)
				{
					return Setting.LS9_Percent;
				}

				var countryUsers = MonthlyUsage.DatabaseUsage.CountryUsers;
				var config = (CountryDiscount)HeaderDiscount.Config;
				var countryLine = config.Lines.Cast<CountryDiscountLine>().FirstOrDefault(x => x.Country == countryUsers.DevelopingRegionUserGroup.DomesticCountry);
				return countryLine != null ? countryLine.Percent : 0;
			}
		}

		public override bool IsActive
		{
			get
			{
				var countryUsers = MonthlyUsage.DatabaseUsage.CountryUsers;
				var config = (CountryDiscount)HeaderDiscount.Config;
				var countryLine = config.Lines.Cast<CountryDiscountLine>().FirstOrDefault(x => x.Country == countryUsers.DevelopingRegionUserGroup.DomesticCountry);
				return countryLine != null
					&& (countryLine.RequiresDomesticDiscount || countryUsers.DevelopingRegionUserGroup.ForeignCompanyCount == 0);
			}
		}

		public override bool RequiredByDependentDiscount => true;
	}

	public class DomesticEntityStlDiscount : StlDiscount
	{
		public DomesticEntityStlDiscount(DiscountInfo info)
			: base(info)
		{
		}

		public override bool HasRequiredOtherDiscounts(IEnumerable<IStlDiscount> potentialDiscountsForDatabase)
		{
			var config = (DomesticDiscount)HeaderDiscount.Config;
			if (config.RequiresDevelopingCountry)
			{
				var other = (DevelopingCountryStlDiscount)potentialDiscountsForDatabase.FirstOrDefault(x => x is DevelopingCountryStlDiscount);
				return other != null && other.IsActive;
			}
			return true;
		}

		public override bool IsActive
		{
			get
			{
				return Percentage != 0;
			}
		}

		public override decimal Percentage
		{
			get
			{
				if (!percentage.HasValue)
				{
					percentage = CalculatePercent();
				}

				return percentage.Value;
			}
		}

		decimal? percentage;
		decimal? percentageForCurrentPeriod;

		decimal PercentageForCurrentPeriod
		{
			get
			{
				if (!percentageForCurrentPeriod.HasValue)
				{
					var config = (DomesticDiscount)HeaderDiscount.Config;
					percentageForCurrentPeriod = config.CalculatePercent(MonthlyUsage.DatabaseUsage.CountryUsers);
				}
				return percentageForCurrentPeriod.Value;
			}
		}

		decimal CalculatePercent()
		{
			decimal result = PercentageForCurrentPeriod;
			if (result == 0)
			{
				var config = (DomesticDiscount)HeaderDiscount.Config;
				if (config.ExpiryMonthCount > 0 && DatabaseCountryUsers != null && MonthlyUsage.Database != null)
				{
					var dbUsers = DatabaseCountryUsers.Get(MonthlyUsage.PeriodStart.AddMonths(-1).ToDateTime(), config.ExpiryMonthCount, MonthlyUsage.Database.PK.ToGuid());
					foreach (var countryUsers in dbUsers)
					{
						if (countryUsers != null && config.CalculatePercent(countryUsers) != 0)
						{
							result = config.MultiEntityPercent;
							break;
						}
					}
				}
			}
			return result;
		}
	}

	public class PrepaymentStlDiscount : StlDiscount
	{
		public PrepaymentStlDiscount(DiscountInfo info)
			: base(info)
		{
		}

		public override bool IsActive
		{
			get
			{
				return MonthlyUsage.HasPrepaid;
			}
		}
	}

	public class VolumeStlDiscount : StlDiscount
	{
		public VolumeStlDiscount(DiscountInfo info)
			: base(info)
		{
		}

		public static decimal CalculatePercentage(decimal totalLicenceUnits, VolumeDiscount volumeDiscountConfig = null)
		{
			var scale = (volumeDiscountConfig ?? VolumeDiscount.NewFromXml(null)).Lines.OfType<VolumeDiscountLine>()
				.OrderBy(x => x.UnitCount)
				.ToArray();

			int n = scale.Length;
			int breakIndex = -1;
			for (int i = 0; i < n; ++i)
			{
				if (scale[i].UnitCount >= totalLicenceUnits)
				{
					break;
				}
				breakIndex = i;
			}

			decimal factor = 0m;

			if (breakIndex >= 0)
			{
				factor = scale[breakIndex].ScaleFactor;
			}

			// If the fee charged would be lower if the spend was at the next break then that lower fee will apply
			if (breakIndex + 1 < n)
			{
				int nextIndex = breakIndex + 1;
				decimal nextBreak = scale[nextIndex].UnitCount;
				decimal fee = totalLicenceUnits * (1 - factor);
				decimal nextFee = nextBreak * (1 - scale[nextIndex].ScaleFactor);
				if (nextFee < fee)
				{
					// The factor that would produce the next fee
					factor = 1 - (nextFee / totalLicenceUnits);
				}
			}

			return factor * 100;
		}

		public override decimal Percentage
		{
			get
			{
				if (Setting != null && Setting.LS9_Percent > 0m)
				{
					return Setting.LS9_Percent;
				}
				else
				{
					decimal totalLicenceUnits = MonthlyUsage.BuyingGroup != null
						? MonthlyUsage.BuyingGroup.TotalLicenceUnits
						: (decimal)MonthlyUsage.TotalLicenceUnits;

					return CalculatePercentage(totalLicenceUnits, (VolumeDiscount)HeaderDiscount.Config);
				}
			}
		}
	}

	public class WiseCloudStlDiscount : StlDiscount
	{
		public WiseCloudStlDiscount(DiscountInfo info)
			: base(info)
		{
		}

		public override bool IsActive
		{
			get
			{
				return MonthlyUsage.Database != null && EDIDataRegistry.Instance.DatabaseHostedLocations.Value.GetBoolFromCode(MonthlyUsage.Database.LD_HostedLocation);
			}
		}

		/*
		 The discount will not apply if the Agreed Go Live date plus the Expiry Months (minus one day) is in the past calendar month.
		 */
		public override decimal Percentage
		{
			get
			{
				var expiryMonths = ((WiseCloudDiscount)HeaderDiscount.Config)?.ExpiryMonthsFromAgreedGoLive ?? ZInt.Zero;

				if (expiryMonths > 0 && MonthlyUsage.IsSiteLive && !MonthlyUsage.SiteLiveDate.IsEmpty)
				{
					var lastDayOfDiscount = MonthlyUsage.SiteLiveDate.AddMonths(expiryMonths).AddDays(-1);
					if (lastDayOfDiscount < MonthlyUsage.PeriodStart)
					{
						return 0m;
					}
				}

				return base.Percentage;
			}
		}
	}

	public class SingleCountryStlDiscount : StlDiscount
	{
		public SingleCountryStlDiscount(DiscountInfo info)
			: base(info)
		{
		}

		public override decimal Percentage
		{
			get
			{
				if (Setting != null && Setting.LS9_Percent != 0m)
				{
					return Setting.LS9_Percent;
				}

				return base.Percentage;
			}
		}
	}

	public class OrgMembershipStlDiscount : StlDiscount
	{
		public OrgMembershipStlDiscount(DiscountInfo info)
			: base(info)
		{
		}

		public override decimal Percentage => IsActive ? HeaderDiscount.PHD_Percent : 0;
		public override bool IsActive
		{
			get
			{
				var result = false;
				if (!MonthlyUsage.InvoicedOrganisationPK.IsEmpty)
				{
					var org = MonthlyUsage.Factory.Load<EDIOrgHeader>(MonthlyUsage.InvoicedOrganisationPK);
					if (org != null)
					{
						var firstDayOfMonth = MonthlyUsage.PeriodStart.Date;
						var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

						var config = (OrgMembershipDiscount)HeaderDiscount.Config;
						var lines = config.Lines.Select(x => x.MembershipType).Distinct().ToHashSet();
						result = org.Memberships.Any(x => lines.Contains(x.EOR_MembershipType)
							&& !x.EOR_ValidFrom.IsEmpty && x.EOR_ValidFrom <= lastDayOfMonth &&
								(x.EOR_ValidTo.IsEmpty || x.EOR_ValidTo >= firstDayOfMonth));
					}
				}
				return result;
			}
		}
	}

	public class MasterOrgDevelopingCountryStlDiscount : StlDiscount
	{
		public MasterOrgDevelopingCountryStlDiscount(DiscountInfo info)
			: base(info)
		{
		}

		public override bool IsActive
		{
			get
			{
				if (!MonthlyUsage.Database.IsEnterpriseFamilyDatabase)
				{
					if (DiscountFinder?.GetCargoWiseOneDevelopingCountryDiscount(MonthlyUsage.Database.LD_OH_WebAccessOrg) != null)
					{
						return true;
					}
					else
					{
						var config = (MasterOrgDevelopingCountryDiscount)HeaderDiscount.Config;
						var countryCode = GetCountryCode();
						return !countryCode.IsEmpty && config.Lines.Where(x => x.Country == countryCode).Any();
					}
				}

				return false;
			}
		}

		public override decimal Percentage
		{
			get
			{
				if (!MonthlyUsage.Database.IsEnterpriseFamilyDatabase)
				{
					var parentDiscount = DiscountFinder?.GetCargoWiseOneDevelopingCountryDiscount(MonthlyUsage.Database.LD_OH_WebAccessOrg);
					if (parentDiscount != null)
					{
						return parentDiscount.Percentage;
					}
					else
					{
						var countryCode = GetCountryCode();
						if (!countryCode.IsEmpty)
						{
							var config = (MasterOrgDevelopingCountryDiscount)HeaderDiscount.Config;
							return config.Lines.Where(x => x.Country == countryCode).FirstOrDefault()?.Percent ?? ZDecimal.Zero;
						}
					}
				}

				return 0;
			}
		}

		ZString GetCountryCode()
		{
			var countryCode = MonthlyUsage.Factory.Load<EDIOrgHeader>(MonthlyUsage.Database.LD_OH_WebAccessOrg)?.MainAddressCollection.Select(x => x).FirstOrDefault()?.OA_RN_NKCountryCode ?? ZString.Empty;
			if (!countryCode.IsEmpty)
			{
				var countryGroup = EDIDataRegistry.Instance.BillingCountryGroups.Value.GetDescriptionFromCode(countryCode);
				if (!string.IsNullOrEmpty(countryGroup))
				{
					countryCode = countryGroup;
				}
			}
			return countryCode;
		}
	}

	public class ProductBundleStlDiscount : StlDiscount
	{
		public ProductBundleStlDiscount(DiscountInfo info)
			: base(info)
		{
		}

		public override decimal Percentage => IsActive ? HeaderDiscount.PHD_Percent : 0;
		public override bool IsActive
		{
			get
			{
				var result = false;
				if (!MonthlyUsage.Database.LD_OH_WebAccessOrg.IsEmpty && Context != null)
				{
					var settings = ((ProductBundleDiscount)HeaderDiscount.Config)?.Lines.Select(x => x.ProductCode).ToArray() ?? Enumerable.Empty<ZString>();
					if (settings.Any())
					{
						var bundle = Context.ProductBundles.GetProductBundleByOrg(MonthlyUsage.Database.LD_OH_WebAccessOrg);
						if (bundle.Intersect(settings).Any())
						{
							result = true;
						}
					}
				}
				return result;
			}
		}
	}

	public class StlDiscountCalculatorFactory
	{
		public IStlDiscount New(DiscountInfo info)
		{
			IStlDiscount result;
			switch (info.HeaderDiscount.PHD_Type)
			{
				case BillingConstants.DiscountCalculator.Percentage: result = new PercentageStlDiscount(info); break;
				case BillingConstants.DiscountCalculator.Volume: result = new VolumeStlDiscount(info); break;
				case BillingConstants.DiscountCalculator.DomesticEntity: result = new DomesticEntityStlDiscount(info); break;
				case BillingConstants.DiscountCalculator.DevelopingCountry: result = new DevelopingCountryStlDiscount(info); break;
				case BillingConstants.DiscountCalculator.Prepayment: result = new PrepaymentStlDiscount(info); break;
				case BillingConstants.DiscountCalculator.WiseCloud: result = new WiseCloudStlDiscount(info); break;
				case BillingConstants.DiscountCalculator.SingleCountry: result = new SingleCountryStlDiscount(info); break;
				case BillingConstants.DiscountCalculator.OrgMembership: result = new OrgMembershipStlDiscount(info); break;
				case BillingConstants.DiscountCalculator.MasterOrgDevelopingCountry: result = new MasterOrgDevelopingCountryStlDiscount(info); break;
				case BillingConstants.DiscountCalculator.ProductBundle: result = new ProductBundleStlDiscount(info); break;
				default:
					throw new InvalidOperationException("Unknown discount calculator type " + info.HeaderDiscount.PHD_Type);
			}
			return result;
		}
	}
}

