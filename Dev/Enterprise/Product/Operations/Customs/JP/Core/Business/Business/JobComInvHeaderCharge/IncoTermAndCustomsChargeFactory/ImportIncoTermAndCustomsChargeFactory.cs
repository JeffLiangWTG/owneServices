using Enterprise.Customs.Common;

namespace Enterprise.Customs.JP.Business
{
	public sealed class ImportIncoTermAndCustomsChargeFactory : BaseIncoTermAndCustomsChargeFactory
	{
		protected override ICustomsChargeCode GetExWorks() => ExWorks;

		protected override ICustomsChargeCode GetForeignInlandFreight() => ForeignInlandFreight;

		public override CustomsChargeCode GetOverseasFreight() => OverseasFreight;

		public override CustomsChargeCode GetOverseasInsurance() => OverseasInsurance;

		protected override ICustomsChargeCode GetPackingCost() => PackingCost;

		protected override ICustomsChargeCode GetLandingCharges() => LandingCharges;

		protected override ICustomsChargeCode GetAdditionCharge() => AdditionCharge;

		protected override ICustomsChargeCode GetCommission() => Commission;

		protected override ICustomsChargeCode GetDeductionCharge() => DeductionCharge;

		protected override ICustomsChargeCode GetDiscount() => Discount;

		protected override ICustomsChargeCode GetOtherCharges() => OtherCharges;

		#region Static Charge Code

		public static CustomsChargeCode ExWorks
		{
			get
			{
				var chargeCode = CustomsChargeCodeProvider.ExWorks;
				chargeCode.IsDutiable = true;
				chargeCode.IsDutiableDeemedForThisCharge = true;
				return chargeCode;
			}
		}

		public static CustomsChargeCode ForeignInlandFreight
		{
			get
			{
				var chargeCode = CustomsChargeCodeProvider.ForeignInlandFreight;
				chargeCode.IsDutiable = true;
				chargeCode.IsDutiableDeemedForThisCharge = true;
				return chargeCode;
			}
		}

		public static CustomsChargeCode OverseasFreight
		{
			get
			{
				var chargeCode = CustomsChargeCodeProvider.OverseasFreight;
				chargeCode.IsDutiable = true;
				chargeCode.IsDutiableDeemedForThisCharge = true;
				chargeCode.DistributeBy = ChargeDistributeByList.Codes.Weight;
				return chargeCode;
			}
		}

		public static CustomsChargeCode OverseasInsurance
		{
			get
			{
				var chargeCode = CustomsChargeCodeProvider.OverseasInsurance;
				chargeCode.IsDutiable = true;
				chargeCode.IsDutiableDeemedForThisCharge = true;
				return chargeCode;
			}
		}

		public static CustomsChargeCode PackingCost
		{
			get
			{
				var chargeCode = CustomsChargeCodeProvider.PackingCost;
				chargeCode.IsDutiable = true;
				chargeCode.IsDutiableDeemedForThisCharge = true;
				return chargeCode;
			}
		}

		public static CustomsChargeCode LandingCharges
		{
			get
			{
				var chargeCode = CustomsChargeCodeProvider.LandingCharges;
				chargeCode.IsDutiable = false;
				chargeCode.IsDutiableDeemedForThisCharge = true;
				return chargeCode;
			}
		}

		public static CustomsChargeCode AdditionCharge
		{
			get
			{
				var chargeCode = CustomsChargeCodeProvider.AdditionCharge;
				chargeCode.IsDutiable = true;
				chargeCode.IsDutiableDeemedForThisCharge = true;
				return chargeCode;
			}
		}

		public static CustomsChargeCode Commission
		{
			get
			{
				var chargeCode = CustomsChargeCodeProvider.Commission;
				chargeCode.IsDutiable = true;
				chargeCode.IsDutiableDeemedForThisCharge = false;
				return chargeCode;
			}
		}

		public static CustomsChargeCode DeductionCharge
		{
			get
			{
				var chargeCode = CustomsChargeCodeProvider.DeductionCharge;
				chargeCode.IsDutiable = false;
				chargeCode.IsDutiableDeemedForThisCharge = true;
				return chargeCode;
			}
		}

		public static CustomsChargeCode Discount
		{
			get
			{
				var chargeCode = CustomsChargeCodeProvider.Discount;
				chargeCode.IsDutiable = false;
				chargeCode.IsDutiableDeemedForThisCharge = false;
				return chargeCode;
			}
		}

		public static CustomsChargeCode OtherCharges
		{
			get
			{
				var chargeCode = CustomsChargeCodeProvider.OtherCharges;
				chargeCode.IsDutiable = true;
				chargeCode.IsDutiableDeemedForThisCharge = false;
				return chargeCode;
			}
		}

		#endregion
	}
}
