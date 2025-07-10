using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusCalculationRuleRate : NonPersistentBusinessObject
	{
		public CusCalculationRuleRate(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public CusCalculationRuleRateCollection CusCalculationRuleRateCollection;

		#region Properties

		public bool IsFirstRate { get; set; }

		public int CurrencyDecimalPlaces => CusCalculationRuleRateCollection?.Parent.Currency?.Decimals ?? 2;

		#region Value From

		[ResourceStringData("CusCalculationRuleRate|ValueFrom", Caption = "Value From (inclusive)")]
		[ReadOnlyMember(nameof(IsFirstRate))]
		[DecimalPlaces(nameof(CurrencyDecimalPlaces))]
		public ZDecimal ValueFrom
		{
			get { return valueFrom; }
			set
			{
				SetNonPersistentPropertyValue(ValueFromInfo, ref valueFrom, value);
				CusCalculationRuleRateCollection?.Sort(nameof(ValueFrom));
				if (!IsValidationSuspended)
				{
					Validation.ValidateValueFrom();
				}
			}
		}
		ZDecimal valueFrom;

		public ZPropertyInfo ValueFromInfo
		{
			get { return GetZPropertyInfo(nameof(ValueFrom)); }
		}

		#endregion

		#region Flat Rate

		[ResourceStringData("CusCalculationRuleRate|FlatRate", Caption = "Flat Rate")]
		[ReadOnlyMember(nameof(IsUpliftSpecified))]
		[DecimalPlaces(nameof(CurrencyDecimalPlaces))]
		public ZDecimal FlatRate
		{
			get { return flatRate; }
			set
			{
				SetNonPersistentPropertyValue(FlatRateInfo, ref flatRate, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateFlatRate();
				}
			}
		}
		ZDecimal flatRate;

		protected bool IsFlatRateSpecified => !FlatRate.IsEmpty;

		public ZPropertyInfo FlatRateInfo
		{
			get { return GetZPropertyInfo(nameof(FlatRate)); }
		}

		#endregion

		#region Uplift

		[ResourceStringData("CusCalculationRuleRate|Uplift", Caption = "Uplift %")]
		[ReadOnlyMember(nameof(IsFlatRateSpecified))]
		[DecimalPlaces(5)]
		public ZDecimal Uplift
		{
			get { return uplift; }
			set
			{
				SetNonPersistentPropertyValue(UpliftInfo, ref uplift, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateUplift();
				}
			}
		}
		ZDecimal uplift;

		protected bool IsUpliftSpecified => !Uplift.IsEmpty;

		public ZPropertyInfo UpliftInfo
		{
			get { return GetZPropertyInfo(nameof(Uplift)); }
		}

		#endregion

		#endregion

		#region Validation

		public CusCalculationRuleRateValidation Validation
		{
			get { return new CusCalculationRuleRateValidation(this); }
		}

		#endregion

		protected override void RunPreSaveValidationCore()
		{
			Validation.ValidateAll();
			base.RunPreSaveValidationCore();
		}

		public override bool CanDelete => !IsFirstRate;

		public override MultilingualString ReasonForNotAbleToDelete => ResString.GetMultilingualString("CusCalculationRuleRate|ReasonForNotAbleToDelete", "Calculation Rule requires at least one Rate.");
	}
}
