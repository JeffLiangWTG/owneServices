using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Licensing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.EDI.Billing.Business.Maintenance
{
	public class MaintenancePrices : IMaintenancePercentages
	{
		public MaintenancePrices(BusinessObjectFactory factory, LicenceHeader licHeader, ClientLicencePriceHeader prices, ZDateTime renewalDate, IPerDatabaseChargeDecider decider = null)
		{
			this.licHeader = licHeader;
			this.renewalDate = renewalDate;
			this.decider = decider;
			modules = new MaintenanceModuleCollection(factory);

			if (licHeader != null && prices != null)
			{
				Calculate(prices.LocalOrStandardItems);
			}
		}

		public MaintenanceModuleCollection Modules
		{
			get { return modules; }
		}
		readonly MaintenanceModuleCollection modules;

		readonly LicenceHeader licHeader;
		public LicenceHeader LicHeader { get { return licHeader; } }

		readonly IPerDatabaseChargeDecider decider;
		readonly ZDateTime renewalDate;
		public ZDecimal PriceOldSeats { get; private set; }
		public ZDecimal PriceNewSeats { get; private set; }

		public ZDecimal Maintenance
		{
			get
			{
				ZDecimal result = 0;
				if (licHeader != null && licHeader.ReadonlyBilling != null)
				{
					if (licHeader.ReadonlyBilling.L0_FixedMaintenanceAmount != 0)
					{
						result = licHeader.ReadonlyBilling.L0_FixedMaintenanceAmount;
					}
					else
					{
						ZDecimal annualAmount = PriceOldSeats * NewPercent
							+ PriceNewSeats * licHeader.Billing.L0_NextNewSeatMaintenancePercent;
						result = annualAmount * licHeader.ReadonlyBilling.L0_RenewalMonths / (12 * 100m);
						result = Utilities.Round(result, BillingConstants.RoundingDecimals);
					}
				}

				return result;
			}
		}

		public ZDecimal NewPercent
		{
			get
			{
				if (GetIsFirstRenewal())
				{
					return licHeader.Billing.L0_NextMaintenancePercent;
				}
				else
				{
					return licHeader.Billing.L0_CurrentMaintenancePercent;
				}
			}
			set
			{
				if (GetIsFirstRenewal())
				{
					licHeader.Billing.L0_NextMaintenancePercent = value;
				}
				else
				{
					licHeader.Billing.L0_CurrentMaintenancePercent = value;
				}
			}
		}

		public ZDecimal CombinedMaintenancePercent
		{
			get
			{
				ZDecimal totalPrice = PriceOldSeats + PriceNewSeats;
				return totalPrice != 0 ? Utilities.Round((Maintenance * 12 * 100m) / (licHeader.ReadonlyBilling.L0_RenewalMonths * totalPrice), BillingConstants.RoundingDecimals) : 0m;
			}
		}

		public ZDecimal SurchargeAmount
		{
			get { return Utilities.Round(licHeader.Billing.L0_Surcharge * Maintenance / 100, BillingConstants.RoundingDecimals); }
		}

		public ZDecimal FinalAmount
		{
			get { return Maintenance + SurchargeAmount; }
		}

		public ZString SurchangeDescription
		{
			get { return licHeader.Billing.L0_SurchargeDescription; }
		}

		public ZDecimal MaintenanceAtLastPercentages
		{
			get
			{
				ZDecimal result = 0;
				if (licHeader != null && licHeader.ReadonlyBilling != null)
				{
					if (licHeader.ReadonlyBilling.L0_FixedMaintenanceAmount != 0)
					{
						result = licHeader.ReadonlyBilling.L0_FixedMaintenanceAmount;
					}
					else
					{
						ZDecimal annualAmount = PriceOldSeats * licHeader.Billing.L0_LastMaintenancePercent
						   + PriceNewSeats * licHeader.Billing.L0_LastNewSeatMaintenancePercent;

						result = annualAmount * licHeader.ReadonlyBilling.L0_RenewalMonths / (12 * 100m);
						result = Utilities.Round(result, BillingConstants.RoundingDecimals);
					}
				}

				return result;
			}
		}

		public bool GetIsFirstRenewal()
		{
			return licHeader.LA_ContractRenewalIssued.IsEmpty || licHeader.LA_ContractRenewalIssued < renewalDate;
		}

		public static bool IsMaintenanceFeeType(LicenceModules module)
		{
			return module.LM_LicenceType == LicenceTypes.Codes.PUR
				|| module.LM_LicenceType == LicenceTypes.Codes.OTM
				|| module.LM_LicenceType == LicenceTypes.Codes.ODM
				|| module.LM_LicenceType == LicenceTypes.Codes.OPN;
		}

		void Calculate(ClientLicencePriceItemCollection priceItems)
		{
			bool isFirstRenewal = GetIsFirstRenewal();
			bool isProduction = licHeader.Database.LD_LicenceType == DatabaseTypes.Codes.Production;

			Dictionary<string, MaintenanceModule> map = new Dictionary<string, MaintenanceModule>();

			foreach (LicenceModules module in licHeader.Modules)
			{
				ZString moduleCode = module.LM_GroupModuleCode;

				if (IsMaintenanceFeeType(module) && module.LM_UserCount > 0)
				{
					var item = priceItems.FindByCode(moduleCode);
					if (item != null && item.L7_FeeType == BillingConstants.FeeType.Included)
					{
						moduleCode = item.L7_ParentCode;
						item = priceItems.FindByCode(moduleCode);
					}

					if (item != null &&
						item.IsMaintenanceFeeType &&
						(!BillingConstants.FeeType.IsPerDatabase(item.L7_FeeType) || decider == null || decider.IsOwner(licHeader, moduleCode)))
					{
						int oldSeats = isFirstRenewal ? module.LM_RenewalUserCount : module.LM_PartPurchasedCount;
						int newSeats = module.LM_UserCount;
						if (!isProduction ||
							BillingConstants.FeeType.IsPerLicence(item.L7_FeeType) ||
							BillingConstants.FeeType.IsPerDatabaseInstance(item.L7_FeeType))
						{
							oldSeats = oldSeats > 0 ? 1 : 0;
							newSeats = newSeats > 0 ? 1 : 0;
						}

						if (oldSeats > newSeats)
						{
							oldSeats = newSeats;
						}

						MaintenanceModule maintenanceModule;
						if (!map.TryGetValue(moduleCode, out maintenanceModule))
						{
							maintenanceModule = new MaintenanceModule(this, moduleCode, item);
							map.Add(moduleCode, maintenanceModule);
						}

						maintenanceModule.UserCount = Math.Max(newSeats, maintenanceModule.UserCount);
						maintenanceModule.OldUserCount = Math.Max(oldSeats, maintenanceModule.OldUserCount);
					}
				}
			}

			modules.AddRange(map.Values.OrderBy(s => s.Order));

			CalculateTotals();
		}

		internal void CalculateTotals()
		{
			PriceOldSeats = 0;
			PriceNewSeats = 0;

			foreach (MaintenanceModule module in modules)
			{
				PriceOldSeats += module.UnitPrice * module.OldUserCount;
				int delta = module.UserCount - module.OldUserCount;
				PriceNewSeats += module.UnitPrice * delta;
			}
		}

		public void UpdateRenewal(BusinessObjectFactory factory)
		{
			bool isFirstRenewal = GetIsFirstRenewal();

			// Calculate all values first since some calculations
			// depend on the first renewal state and this method can change the first renewal state.
			ZDecimal maintenance = Maintenance;
			ZDecimal combinedPercent = CombinedMaintenancePercent;
			ZDecimal newPercent = NewPercent;

			LicenceHeader licHeaderInUpdateFactory = factory.Load<LicenceHeader>(licHeader.PK);
			licHeaderInUpdateFactory.LA_ContractRenewalIssued = renewalDate;
			var billing = licHeaderInUpdateFactory.Billing;

			billing.L0_LastMaintenanceAmount = maintenance;
			billing.L0_LastMaintenancePercent = newPercent;
			billing.L0_LastNewSeatMaintenancePercent = billing.L0_NextNewSeatMaintenancePercent;

			if (isFirstRenewal)
			{
				billing.L0_CurrentMaintenancePercent = newPercent;
			}

			billing.L0_NextMaintenancePercent = combinedPercent;

			foreach (LicenceModules module in licHeaderInUpdateFactory.Modules)
			{
				if (IsMaintenanceFeeType(module))
				{
					if (isFirstRenewal)
					{
						module.LM_PartPurchasedCount = (ZShort)module.LM_RenewalUserCount;
					}
					module.LM_RenewalUserCount = module.LM_UserCount;
				}
			}
		}

		ZDecimal IMaintenancePercentages.OldSeatPercent
		{
			get { return NewPercent; }
		}

		ZDecimal IMaintenancePercentages.NewSeatPercent
		{
			get { return licHeader != null && licHeader.ReadonlyBilling != null ? licHeader.Billing.L0_NextNewSeatMaintenancePercent : ZDecimal.Zero; }
		}
	}
}

