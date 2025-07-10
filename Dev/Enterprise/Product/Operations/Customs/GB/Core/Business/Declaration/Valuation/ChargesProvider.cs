using System;
using System.Collections.Generic;

using Enterprise.Customs.Common;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.GB.Business.Declaration
{
	public static class ChargesProvider
	{
		public static CustomsChargeCode AirFreight => airFreight ?? (airFreight = new CustomsChargeCode(AirFreightCode, AirFreightDesc)
		{
			IsDutiable = true,
			IsDutiableDeemedForThisCharge = false,
			IsVATible = true,
			IsVATibleDeemedForThisCharge = true,
			IsStatisticalValueApplicable = true,
			IsStatisticalValueApplicableDeemed = false,
			ParentTypes = ChargeParentTypes.GroupInvoice | ChargeParentTypes.Invoice,
		});
		[ThreadStatic]
		static CustomsChargeCode airFreight;

		public static CustomsChargeCode InternationalFreight => internationalFreight ?? (internationalFreight = new CustomsChargeCode(ChargesProvider.OverseasFreightInChiefTerminologyCode_AWB, ChargeDescriptionsForGB.OverseasFreight)
		{
			IsDutiable = true,
			IsDutiableDeemedForThisCharge = true,
			IsVATible = true,
			IsVATibleDeemedForThisCharge = true,
			IsStatisticalValueApplicable = true,
			IsStatisticalValueApplicableDeemed = true,
			ParentTypes = ChargeParentTypes.GroupInvoice | ChargeParentTypes.Invoice,
		});
		[ThreadStatic]
		static CustomsChargeCode internationalFreight;

		public static CustomsChargeCode InternationalInsurance => internationalInsurance ?? (internationalInsurance = new CustomsChargeCode(CustomsChargeTypeList.Codes.OverseasInsurance, ChargeDescriptionsForGB.Insurance)
		{
			IsDutiable = true,
			IsDutiableDeemedForThisCharge = true,
			IsVATible = true,
			IsVATibleDeemedForThisCharge = true,
			IsStatisticalValueApplicable = true,
			IsStatisticalValueApplicableDeemed = true,
			ParentTypes = ChargeParentTypes.GroupInvoice | ChargeParentTypes.Invoice,
		});
		[ThreadStatic]
		static CustomsChargeCode internationalInsurance;
		public static CustomsChargeCode AdditionCharge
		{
			get
			{
				additionCharge ??= CustomsChargeCodeProvider.AdditionCharge;
				additionCharge.IsStatisticalValueApplicable = true;
				additionCharge.IsStatisticalValueApplicableDeemed = false;
				return additionCharge;
			}
		}
		[ThreadStatic]
		static CustomsChargeCode additionCharge;

		public static CustomsChargeCode DeductionCharge
		{
			get
			{
				deductionCharge ??= CustomsChargeCodeProvider.DeductionCharge;
				deductionCharge.IsStatisticalValueApplicable = false;
				deductionCharge.IsStatisticalValueApplicableDeemed = false;
				return deductionCharge;
			}
		}
		[ThreadStatic]
		static CustomsChargeCode deductionCharge;

		public static CustomsChargeCode Discount => discount ?? (discount = new CustomsChargeCode(CustomsChargeTypeList.Codes.Discount, ChargeDescriptionsForGB.Discount)
		{
			IsDutiable = false,
			IsDutiableDeemedForThisCharge = true,
			IsVATible = false,
			IsVATibleDeemedForThisCharge = true,
			IsStatisticalValueApplicable = false,
			IsStatisticalValueApplicableDeemed = true,
			IsIncoTermNeutral = true,
			ParentTypes = ChargeParentTypes.GroupInvoice,
		});
		[ThreadStatic]
		static CustomsChargeCode discount;

		public static CustomsChargeCode VATAdjustment => vatAdjustment ?? (vatAdjustment = new CustomsChargeCode(VATAdjustmentCode, VATAdjustmentDesc)
		{
			IsDutiable = false,
			IsDutiableDeemedForThisCharge = true,
			IsVATible = true,
			IsVATibleDeemedForThisCharge = true,
			IsStatisticalValueApplicable = false,
			IsStatisticalValueApplicableDeemed = true,
			IsIncludedInITOTDeemedForThisCharge = true,
			IsIncludedInITOTIfDeemed = false,
			IsIncoTermNeutral = true,
			ParentTypes = ChargeParentTypes.GroupInvoice | ChargeParentTypes.Invoice,
		});
		[ThreadStatic]
		static CustomsChargeCode vatAdjustment;

		public const string OverseasFreightInChiefTerminologyCode_AWB = "AWB";
		public const string VATAdjustmentCode = "VAT";
		public static ResourceString VATAdjustmentDesc = ResString.GetMultilingualString("5015900F-D79C-4327-AAA0-DF7C502AA029", "VAT Adjustment (Box 68)");
		public const string AirFreightCode = "AFT";
		public static ResourceString AirFreightDesc = ResString.GetMultilingualString("35E66D1C-BFE2-4D92-BAC5-5B9D7A3E2D1A", "Air Freight (Box 62)");

		public static IEnumerable<string> ChargesValidToBeApportionedByWeight
		{
			get
			{
				yield return AirFreightCode;
				yield return CustomsChargeTypeList.Codes.OverseasFreight;
				yield return VATAdjustmentCode;
			}
		}

		public static class ChargeDescriptionsForGB
		{
			public static ResourceString OverseasFreight = ResString.GetMultilingualString("2BA40676-B4A9-473C-9DAC-1AF30495B72C", "Freight/AWB charges (non-air: Box 63; air: to add to AFT to give Box 63)");
			public static ResourceString Discount = ResString.GetMultilingualString("3E6CC3FB-64B0-4AD6-A9C8-D28F584441FC", "Discount (Box 65)");
			public static ResourceString Insurance = ResString.GetMultilingualString("AB134577-207C-4190-8BB9-2B97685ECFF1", "Insurance (Box 66)");
		}
	}
}
