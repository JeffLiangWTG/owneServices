using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.EDI.Billing.ODPL
{
	public class OdplModuleUsage : NonPersistentBusinessObject, IObsoleteValidation
	{
		public OdplModuleUsage(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		#region Properties

		public ZString ModuleCode { get; set; }
		public ZString ModuleName { get; set; }

		public ZString FeeType { get; set; }
		public ZDecimal UnitPrice { get; set; }
		public ClientLicencePriceItem PriceItem { get; private set; }

		public ZInt StaffCount { get; set; }
		public ZInt Order { get; set; }
		public ZInt PurchasedStaffCount { get; set; }

		public ZShort LicenceUnits { get; set; }

		public ZBool UseRegisteredUserAsFeeTypeIfCoreRelatedUsage { get; set; }

		#endregion

		#region Calculated Properties

		public ZInt UnitCount
		{
			get
			{
				return Math.Max(0, MixedUnitCount - PurchasedStaffCount);
			}
		}

		// Mixed amount is the total, unadjusted usage amount including both purchased users and on-demand users.
		public ZInt MixedUnitCount
		{
			get
			{
				ZInt result = Math.Max(StaffCount, PurchasedStaffCount);
				if (BillingConstants.FeeType.IsPerLicence(FeeType) || BillingConstants.FeeType.IsPerDatabaseInstance(FeeType))
				{
					result = result > 0 ? 1 : 0;
				}
				return result;
			}
		}

		public ZDecimal Amount
		{
			get { return Utilities.Round(UnitCount * UnitPrice, BillingConstants.RoundingDecimals); }
		}

		public ZDecimal LicenceUnitsAmount
		{
			get { return UnitCount * LicenceUnits; }
		}

		public decimal MixedAmountAsMoney
		{
			get { return Utilities.Round(MixedUnitCount * UnitPrice, BillingConstants.RoundingDecimals); }
		}

		// Mixed amount is the total, unadjusted usage amount including both purchased users and on-demand users.
		public decimal MixedAmountAsLicenceUnits
		{
			get { return MixedUnitCount * LicenceUnits; }
		}

		public ZString ModuleInternalName
		{
			get { return LicenceModuleList.Instance.GetDescriptionFromCode(ModuleCode); }
		}

		public ZString FeeTypeDescription
		{
			get
			{
				if (UseRegisteredUserAsFeeTypeIfCoreRelatedUsage && (ModuleCode == BillingConstants.CoreModuleCode || FeeType == BillingConstants.FeeType.CoreUsers))
				{
					return BillingConstants.FeeTypeDescriptions.RegisteredUser;
				}
				else
				{
					return !FeeBasisText.IsEmpty ? FeeBasisText : (ZString)FeeTypes.GetDescriptionFromCode(FeeType);
				}
			}
		}

		public ZBool ShowOnSummary
		{
			get { return UnitPrice != 0 && (StaffCount != 0 || PurchasedStaffCount != 0); }
		}

		public ZBool IsProductionModule
		{
			get { return ModuleCode != BillingConstants.NonProductionDatabase.CoreModuleCode; }
		}

		public ZString FeeBasisText { get; private set; }

		#endregion

		#region Populate From PriceItem

		public void PopulateFromPriceItem(ClientLicencePriceItem priceItem)
		{
			FeeType = priceItem.L7_FeeType;
			UnitPrice = priceItem.L7_Price;
			Order = priceItem.L7_Order;
			ModuleName = priceItem.L7_DescriptionLocalized;
			LicenceUnits = (short)priceItem.L7_LicenceUnits;
			PriceItem = priceItem;
			FeeBasisText = priceItem.L7_ChargeBasisMultilingual;
		}

		#endregion

		#region Lookups

		public ReadOnlyCodeDescriptionPairList FeeTypes
		{
			get { return feeTypes ?? (feeTypes = BillingConstants.GetCachedFeeTypeList(Factory)); }
		}
		ReadOnlyCodeDescriptionPairList feeTypes;

		#endregion
	}
}

