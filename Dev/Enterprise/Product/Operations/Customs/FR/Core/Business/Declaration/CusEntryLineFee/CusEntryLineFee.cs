using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.FR.Business.Declaration
{
	public class CusEntryLineFee : EU.Business.Declaration.CusEntryLineFee, Integration.Customs.FR.ICusEntryLineFee
	{
		public CusEntryLineFee(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Properties

		[ResourceStringData("Enterprise.Customs.FR.Business.Declaration.CusEntryLineFee|MethodOfPaymentDescription", Caption = "Method Of Payment Description", MediumCaption = "MOP Description", ShortCaption = "MOP Desc.")]
		public ZString MethodOfPaymentDescription
		{
			get
			{
				return this.Lookups.MethodOfPaymentList[CF_MethodOfPayment]?.Description ?? ZString.Empty;
			}
		}

		#region Ai2Amount
		public ZDecimal Ai2Amount
		{
			get
			{
				if (ai2AmountCached == null)
				{
					ai2AmountCached = new CachedProperty<ZDecimal>(Factory, () =>
					 {
						 var result = ZDecimal.Zero;
						 if (!NationalFeeTypeCode.IsEmpty)
						 {
							 var effictiveCodes = new RefCusTaxOrFee.Loader(EntryLine.Header.Factory).LoadTaxOrFeeFromCodeDate(Core.Constants.CountryCodes.France, NationalFeeTypeCode, EntryLine.Header.EffectiveValuationDate).Where(x => x.ZZF_ZX0_NKTaxOrFeeType == GuaranteeTypeList.Codes.AI2 || x.ZZF_ZX0_NKTaxOrFeeType == Core.Constants.Customs.CusEntryFeeTypes.VAT);
							 if (effictiveCodes.Any())
							 {
								 result = CF_ChargeAmount;
							 }
						 }
						 if (result.IsEmpty && CF_ChargeType.Equals(FeeTypeCodeConverter.EUFeeCodeForVAT))
						 {
							 result = CF_ChargeAmount;
						 }
						 return result;
					 });
				}
				return ai2AmountCached.Value;
			}
		}

		CachedProperty<ZDecimal> ai2AmountCached;

		protected override ZString AmountInDeclarationCurrencyCore() => CF_ChargeAmount.Round(0).ToString();
		protected override ZString TaxBaseCore() => CF_BaseValue.Round(0).ToString();
		#endregion

		public override bool TotalAmountReadOnly => false;

		protected override bool ShouldDeleteIfChargeAmountIsZero => false;

		#endregion

		protected override Customs.Business.CusEntryLineFeeLookups GetNewLookups() => EntryLine?.Declaration is JobDeclaration declaration ? declaration.ApplicationExtender.GetCusEntryLineFeeLookups(this) : new CusEntryLineFeeLookups(this);

		protected override Customs.Business.CusEntryLineFeeValidation GetNewValidation() => new CusEntryLineFeeValidation(this);

		public new CusEntryLineFeeLookups Lookups => (CusEntryLineFeeLookups)base.Lookups;

		protected override bool IsLookupsCachedInBase => false;

		protected override bool GetIsNationalIndirectTaxationFee() => false;

		protected override IFeeRounder GetNewChargeAmountRounder()
		{
			return new IntegerFeeRounder();
		}

		public override bool IsActionBlank =>  base.IsActionBlank || (!EntryLine.Declaration.IsUCC6 && CF_RateOverrideReasonCode == RateOverrideReasonList.Codes.Precalcule);

		protected override IUserEnteredStashSource GetUserEnteredStashSourceCore() => new CusEntryLineFeeUserEnteredStashSource(this);

		protected override ZString TaxBoxSupporterRateCore => FormatThreeDecimals(CF_Rate);

		protected ZString FormatThreeDecimals(ZDecimal number) => number.ToString(3);
	}
}
