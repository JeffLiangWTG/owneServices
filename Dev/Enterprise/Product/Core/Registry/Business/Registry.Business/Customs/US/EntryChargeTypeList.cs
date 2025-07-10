using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Registry.Business.Customs.US
{
	public class EntryChargeTypeList : Enterprise.Registry.Business.Customs.EntryChargeTypeList
	{
		#region SuppressResourceStringsCheckRegion

		public EntryChargeTypeList()
		{
			LoadData(new BusinessObjectFactory());
		}

		public EntryChargeTypeList(BusinessObjectFactory factory)
		{
			LoadData(factory);
		}

		void LoadData(BusinessObjectFactory factory)
		{
			var cusRateTypeList = ObjectFactory.Get<Integration.Customs.Shared.Universal.IRefCusCodeListTypesListProvider>().GetList(factory, Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AccountingClassFeeCode, ZDateTime.Now);
			foreach (CodeDescriptionPair cusRateType in cusRateTypeList)
			{
				var code = cusRateType.Code;
				if (!ExcludedFeeCodes.Contains(code))
				{
					AddIfNotExists(code, cusRateType.Description, true, ZString.Empty);
				}
			}
			AddIfNotExists(Core.Constants.USCustoms.FeeCodes.AntidumpingDuty, "Antidumping Duty", true, "");
			AddIfNotExists(Core.Constants.USCustoms.FeeCodes.CountervailingDuty, "Countervailing Duty", true, "");
			AddIfNotExists(Core.Constants.USCustoms.FeeCodes.Duty, "Duty", true, "");
			AddIfNotExists(Core.Constants.USCustoms.FeeCodes.ExciseTaxPayable, "Excise Tax Payable", true, "");
			AddIfNotExists(Core.Constants.USCustoms.FeeCodes.ExciseTaxDeferred, "Excise Tax Deferred", true, "");
			AddIfNotExists(Core.Constants.USCustoms.FeeCodes.ReconciliationInterest, "Interest Amount For Reconciliation Summary", true, "");
		}

		static ZString[] ExcludedFeeCodes => new ZString[]
		{
			Core.Constants.USCustoms.FeeCodes.DistilledSpirits, Core.Constants.USCustoms.FeeCodes.Wines,
			Core.Constants.USCustoms.FeeCodes.Tobacco, Core.Constants.USCustoms.FeeCodes.OtherExcise, Core.Constants.USCustoms.FeeCodes.OtherAgencies
		};

		public static bool IsFeeType(string chargeType)
		{
			return chargeType == Core.Constants.USCustoms.FeeCodes.Beef ||
				chargeType == Core.Constants.USCustoms.FeeCodes.Blueberry ||
				chargeType == Core.Constants.USCustoms.FeeCodes.Cotton ||
				chargeType == Core.Constants.USCustoms.FeeCodes.DutiableMail ||
				chargeType == Core.Constants.USCustoms.FeeCodes.Avocado ||
				chargeType == Core.Constants.USCustoms.FeeCodes.Honey ||
				chargeType == Core.Constants.USCustoms.FeeCodes.MerchandiseInformal ||
				chargeType == Core.Constants.USCustoms.FeeCodes.FreshLimes ||
				chargeType == Core.Constants.USCustoms.FeeCodes.Mango ||
				chargeType == Core.Constants.USCustoms.FeeCodes.Sorghum ||
				chargeType == Core.Constants.USCustoms.FeeCodes.DairyFee ||
				chargeType == Core.Constants.USCustoms.FeeCodes.MerchandiseSurcharge ||
				chargeType == Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing ||
				chargeType == Core.Constants.USCustoms.FeeCodes.Mushroom ||
				chargeType == Core.Constants.USCustoms.FeeCodes.Raspberry ||
				chargeType == Core.Constants.USCustoms.FeeCodes.Pork ||
				chargeType == Core.Constants.USCustoms.FeeCodes.Potato ||
				chargeType == Core.Constants.USCustoms.FeeCodes.SoftwoodLumber ||
				chargeType == Core.Constants.USCustoms.FeeCodes.Sugar ||
				chargeType == Core.Constants.USCustoms.FeeCodes.Watermelon ||
				chargeType == Core.Constants.USCustoms.FeeCodes.HMF ||
				chargeType == Core.Constants.USCustoms.FeeCodes.Coffee ||
				chargeType == Core.Constants.USCustoms.FeeCodes.Pecan ||
				chargeType == Core.Constants.USCustoms.FeeCodes.ChristmasTree;
		}

		public static string[] GetFeeCodes()
		{
			List<string> result = new List<string>();

			foreach (CodeDescriptionPair pair in new EntryChargeTypeList())
			{
				if (IsFeeType(pair.Code))
				{
					result.Add(pair.Code);
				}
			}

			return result.ToArray();
		}

		public override string DutyCode => Core.Constants.USCustoms.FeeCodes.Duty;

		public override string TaxCode => "";

		public static bool IsOtherRevenueAmountChargeCode(string chargeType)
		{
			return chargeType == Core.Constants.USCustoms.FeeCodes.Coffee;
		}
	}

	#endregion
}
