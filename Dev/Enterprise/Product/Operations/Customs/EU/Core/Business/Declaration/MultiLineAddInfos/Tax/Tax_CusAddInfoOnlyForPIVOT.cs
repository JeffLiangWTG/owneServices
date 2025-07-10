using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Customs.EU.Business.MasterFiles;

namespace Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos
{
	//NB 2017-10-26 DJC. This AddInfo class is now only used on the Product's Pivot. 
	[CusAddInfoType(CusAddInfoTypeAttribute.Codes.GBTax)]
	public class Tax_CusAddInfoOnlyForPIVOT : AutoEUAddInfoTax, IEuTax
	{
		public Tax_CusAddInfoOnlyForPIVOT(ZPropertyInfo addInfoProperty)
			: base(addInfoProperty.BizObj.Factory)
		{
			SetupParentAndEventsAndLoadValues(addInfoProperty);
		}

		public CusClassPartPivot Pivot => ((CusAddInfo)Parent)?.Parent as CusClassPartPivot;

		public ZString CountryCode
		{
			get
			{
				var pivot = Pivot;
				return pivot?.CI_RN_NKCountry ?? ZString.Empty;
			}
		}

		[List(nameof(Lookups) + "." + nameof(EUAddInfoTaxLookups.MOPList))]
		public override ZString G4_MethodOfPayment
		{
			get { return base.G4_MethodOfPayment; }
			set { base.G4_MethodOfPayment = value; }
		}

		[List(nameof(Lookups) + "." + nameof(EUAddInfoTaxLookups.RateDutyList))]
		public override ZString G4_RateDuty
		{
			get { return base.G4_RateDuty; }
			set
			{
				base.G4_RateDuty = value;
				if (IsVatGst && RateDemandsNoPayment)
				{
					G4_MethodOfPayment = ZString.Empty;
				}
			}
		}

		[List(nameof(Lookups) + "." + nameof(EUAddInfoTaxLookups.TypeList))]
		public override ZString G4_Type
		{
			get { return base.G4_Type; }
			set
			{
				base.G4_Type = value;
				if (IsVatGst && RateDemandsNoPayment)
				{
					G4_MethodOfPayment = ZString.Empty;
				}
			}
		}

		bool IsVatGst
		{
			get { return G4_Type == UniversalReferenceConstants.RefCusRateCodes.Vat; }
		}

		bool RateDemandsNoPayment
		{
			get { return G4_RateDuty == TaxRateVATDutyListImport.Codes.VATTheGoodsAreExemptFromVAT || G4_RateDuty == TaxRateVATDutyListImport.Codes.VATTheGoodsAreZeroRated; }
		}

		public override ZString KeyToDeterimeUniqueness
		{
			get { return G4_Type; }
		}

		ZBool IEuTax.IsCopying { get => IsCopyingExposed; }
		ZString IEuTax.G4_Amount { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
		ZDecimal IEuTax.G4_BaseAmount { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
		ZDecimal IEuTax.G4_BaseQuantity { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
		ZString IEuTax.G4_BaseQuantityUQ { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }

		ZDecimal IEuTax.G4_CalculatedPercentage { get => throw new NotSupportedException(); }

		ZPropertyInfo IEuTax.G4_AmountInfo => throw new NotSupportedException();
		ZPropertyInfo IEuTax.G4_BaseAmountInfo => throw new NotSupportedException();
	}
}
