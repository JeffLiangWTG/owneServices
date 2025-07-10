using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Client.EDI.Billing.Business
{
	public class BulkAddDiscount : NonPersistentBusinessObject
	{
		public BulkAddDiscount() : base()
		{
		}

		public void PopulateClientLicenceBillingDiscount(ClientLicenceBillingDiscount billingDiscount)
		{
			billingDiscount.L5_SystemCode = SystemCode;
			billingDiscount.L5_SubCode = SubCode;
			billingDiscount.L5_Type = DiscountType;
			billingDiscount.L5_ModuleCode = ModuleCode;
			billingDiscount.L5_BreakAmount = BreakAmount;
			billingDiscount.L5_Units = Units;
			billingDiscount.L5_BreakUnits = BreakUnits;
			billingDiscount.L5_Discount = Discount;
			billingDiscount.L5_StartDate = StartDate;
			billingDiscount.L5_EndDate = EndDate;
			billingDiscount.L5_Duration = Duration;
			billingDiscount.L5_Description = Description;
			billingDiscount.L5_Comment = Comment;
		}

		#region Schema

		public static class Schema
		{
			public const string SystemCode = "SystemCode";
			public const string SubCode = "SubCode";
			public const string DiscountType = "DiscountType";
			public const string ModuleCode = "ModuleCode";
			public const string BreakAmount = "BreakAmount";
			public const string Units = "Units";
			public const string BreakUnits = "BreakUnits";
			public const string Discount = "Discount";
			public const string StartDate = "StartDate";
			public const string EndDate = "EndDate";
			public const string Duration = "Duration";
			public const string Description = "Description";
			public const string Comment = "Comment";
		}

		#endregion

		#region Properties

		#region System Code

		[List("Lookups.SystemCodes")]
		[MaxLength(ClientLicenceBillingDiscount.Schema.L5_SystemCodeMaxLength)]
		public ZString SystemCode
		{
			get { return systemCode; }
			set { SetNonPersistentPropertyValue(SystemCodeInfo, ref systemCode, value); }
		}
		ZString systemCode;

		public ZPropertyInfo SystemCodeInfo
		{
			get { return GetZPropertyInfo(Schema.SystemCode); }
		}

		#endregion

		#region Sub Code

		[List("Lookups.SubCodes")]
		[MaxLength(ClientLicenceBillingDiscount.Schema.L5_SubCodeMaxLength)]
		public ZString SubCode
		{
			get { return subCode; }
			set { SetNonPersistentPropertyValue(SubCodeInfo, ref subCode, value); }
		}
		ZString subCode;

		public ZPropertyInfo SubCodeInfo
		{
			get { return GetZPropertyInfo(Schema.SubCode); }
		}

		public bool SubCode_ReadOnly
		{
			get { return !ClientLicenceBillingDiscountLookups.GetSystemCodesThatHaveSubCodes().Contains(SystemCode); }
		}

		#endregion

		#region Discount Type

		[List("Lookups.DiscountTypes")]
		[MaxLength(ClientLicenceBillingDiscount.Schema.L5_TypeMaxLength)]
		public ZString DiscountType
		{
			get { return discountType; }
			set { SetNonPersistentPropertyValue(DiscountTypeInfo, ref discountType, value); }
		}
		ZString discountType;

		public ZPropertyInfo DiscountTypeInfo
		{
			get { return GetZPropertyInfo(Schema.DiscountType); }
		}

		#endregion

		#region Module Code

		[List("Lookups.ModuleCodes")]
		[MaxLength(ClientLicenceBillingDiscount.Schema.L5_ModuleCodeMaxLength)]
		public ZString ModuleCode
		{
			get { return moduleCode; }
			set { SetNonPersistentPropertyValue(ModuleCodeInfo, ref moduleCode, value); }
		}
		ZString moduleCode;

		public ZPropertyInfo ModuleCodeInfo
		{
			get { return GetZPropertyInfo(Schema.ModuleCode); }
		}

		public bool ModuleCode_ReadOnly
		{
			get { return DiscountType != BillingConstants.DiscountType.ModuleSpecific; }
		}

		#endregion

		#region Break Amount

		public ZDecimal BreakAmount
		{
			get { return breakAmount; }
			set { SetNonPersistentPropertyValue(BreakAmountInfo, ref breakAmount, value); }
		}
		ZDecimal breakAmount;

		public ZPropertyInfo BreakAmountInfo
		{
			get { return GetZPropertyInfo(Schema.BreakAmount); }
		}

		public bool BreakAmount_ReadOnly
		{
			get
			{
				return DiscountType == BillingConstants.DiscountType.ModuleSpecific
				  || DiscountType == BillingConstants.DiscountType.Special
				  || DiscountType == BillingConstants.DiscountType.IncrementalVolume
				  || (DiscountType == BillingConstants.DiscountType.Commitment && BillingConstants.IsTransactional(SystemCode));
			}
		}

		#endregion

		#region Units

		public ZInt Units
		{
			get { return units; }
			set { SetNonPersistentPropertyValue(UnitsInfo, ref units, value); }
		}
		ZInt units;

		public ZPropertyInfo UnitsInfo
		{
			get { return GetZPropertyInfo(Schema.Units); }
		}

		public bool Units_ReadOnly
		{
			get
			{
				return DiscountType != BillingConstants.DiscountType.MinimumFee
				  && DiscountType != BillingConstants.DiscountType.IncrementalVolume
				  && !(DiscountType == BillingConstants.DiscountType.Commitment && BillingConstants.IsTransactional(SystemCode));
			}
		}

		#endregion

		#region Break Units

		[List("Lookups.BreakUnits")]
		[MaxLength(ClientLicenceBillingDiscount.Schema.L5_BreakUnitsMaxLength)]
		public ZString BreakUnits
		{
			get { return breakUnits; }
			set { SetNonPersistentPropertyValue(BreakUnitsInfo, ref breakUnits, value); }
		}
		ZString breakUnits;

		public ZPropertyInfo BreakUnitsInfo
		{
			get { return GetZPropertyInfo(Schema.BreakUnits); }
		}

		#endregion

		#region Discount

		public ZDecimal Discount
		{
			get { return discount; }
			set { SetNonPersistentPropertyValue(DiscountInfo, ref discount, value); }
		}
		ZDecimal discount;

		public ZPropertyInfo DiscountInfo
		{
			get { return GetZPropertyInfo(Schema.Discount); }
		}

		public bool Discount_ReadOnly
		{
			get { return DiscountType == BillingConstants.DiscountType.MinimumFee; }
		}

		#endregion

		#region Start Date

		public ZDateTime StartDate
		{
			get { return startDate; }
			set { SetNonPersistentPropertyValue(StartDateInfo, ref startDate, value); }
		}
		ZDateTime startDate;

		public ZPropertyInfo StartDateInfo
		{
			get { return GetZPropertyInfo(Schema.StartDate); }
		}

		#endregion

		#region End Date

		public ZDateTime EndDate
		{
			get { return endDate; }
			set { SetNonPersistentPropertyValue(EndDateInfo, ref endDate, value); }
		}
		ZDateTime endDate;

		public ZPropertyInfo EndDateInfo
		{
			get { return GetZPropertyInfo(Schema.EndDate); }
		}

		#endregion

		#region Duration

		public ZShort Duration
		{
			get { return duration; }
			set { SetNonPersistentPropertyValue(DurationInfo, ref duration, value); }
		}
		ZShort duration;

		public ZPropertyInfo DurationInfo
		{
			get { return GetZPropertyInfo(Schema.Duration); }
		}

		#endregion

		#region Description

		public ZString Description
		{
			get { return description; }
			set { SetNonPersistentPropertyValue(DescriptionInfo, ref description, value); }
		}
		ZString description;

		public ZPropertyInfo DescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.Description); }
		}

		#endregion

		#region Comment

		public ZString Comment
		{
			get { return comment; }
			set { SetNonPersistentPropertyValue(CommentInfo, ref comment, value); }
		}
		ZString comment;

		public ZPropertyInfo CommentInfo
		{
			get { return GetZPropertyInfo(Schema.Comment); }
		}

		#endregion

		#endregion

		#region Lookups

		public BulkAddDiscountLookups Lookups
		{
			get { return lookups ?? (lookups = new BulkAddDiscountLookups(this, new BusinessObjectFactory())); }
		}
		BulkAddDiscountLookups lookups;

		#endregion

	}
}

