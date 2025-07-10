using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.ConsolRevenue
{
	public class ConsolRevenueLookups
	{
		public ConsolRevenueLookups(ConsolRevenue parent)
		{
			this.Parent = parent;
		}

		readonly ConsolRevenue Parent;

		public CodeDescriptionPairList ApportionmentMethodList
		{
			get
			{
				var methods = new CodeDescriptionPairList(OLookUpEditType.AllocationMethod);

				var excludedMethods = Parent.Consol?.CostSupporter?.ExcludedApportionmentMethods;
				if (excludedMethods != null)
				{
					excludedMethods.ForEach(method => methods.RemoveCode(method));
				}

				return methods;
			}
		}

		#region Currencies

		public virtual RefCurrencyCollection Currencies
		{
			get { return new RefCurrencyCollection(Parent.Factory); }
		}

		#endregion

		AccChargeCodeCollection fChargeCodes;
		public AccChargeCodeCollection ChargeCodes
		{
			get
			{
				if (fChargeCodes == null)
				{
					fChargeCodes = new AccChargeCodeCollection(Parent.Factory, ChargeCodeCollectionFilter, GlbCompany.CurrentCompany.PK.ToGuid());
					fChargeCodes.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Dept Filter", "Property", ChargeCodeDepartmentFilterList));
				}
				return fChargeCodes;
			}
		}

		protected ZQuery ChargeCodeCollectionFilter
		{
			get
			{
				string[] allowedTypes = {
					Core.Constants.ChargeType.Margin,
					Core.Constants.ChargeType.Disbursement,
					Core.Constants.ChargeType.ManualJobAccrual,
					Core.Constants.ChargeType.Revenue
				};

				return new ZQuery(AccChargeCodeSchema.AC_ChargeType, allowedTypes);
			}
		}

		ZString ChargeCodeDepartmentFilterList
		{
			get
			{
				ZString result = "";

				foreach (JobCharge charge in Parent.SplitCharges)
				{
					if (charge.Department != null && !result.Contains(charge.Department.GE_Code))
					{
						result += (charge.Department.GE_Code + ", ");
					}
				}
				result += "ALL";
				return result;
			}
		}

		public CodeDescriptionPairList SupplyTypes
		{
			get
			{
				return AccountingMasterFilesRegistry.Instance.SupplyTypeClassificationCodesList.Value.GetActiveCodeDescriptionPairList();
			}
		}
	}
}

