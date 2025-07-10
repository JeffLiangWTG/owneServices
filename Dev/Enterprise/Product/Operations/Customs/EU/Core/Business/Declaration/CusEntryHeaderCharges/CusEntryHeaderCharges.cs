using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public class CusEntryHeaderCharges : Customs.Business.CusEntryHeaderCharges, Integration.Customs.EU.ICusEntryHeaderCharges, IDocSADHLineTaxBoxSupporter
	{
		public CusEntryHeaderCharges(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{ }

		protected override bool ShouldResetDataOnMergingCore => false;

		public new CusEntryHeader EntryHeader => base.EntryHeader != null ? (CusEntryHeader)base.EntryHeader : null;

		public static new readonly CusEntryHeaderChargesTypeDecider TypeDecider = new CusEntryHeaderChargesTypeDecider();

		public override ZDecimal C1_ChargeAmount { get => base.C1_ChargeAmount; set => base.C1_ChargeAmount = ChargeAmountRounder.Round(value); }

		public IFeeRounder ChargeAmountRounder => chargeAmountRounder ?? (chargeAmountRounder = GetNewChargeAmountRounder());
		IFeeRounder chargeAmountRounder;

		protected virtual IFeeRounder GetNewChargeAmountRounder() => new FeeNoRounder();

		ZString IDocSADHLineTaxBoxSupporter.Type => C1_ChargeType;

		ZString IDocSADHLineTaxBoxSupporter.TaxBase => ZString.Empty;

		ZString IDocSADHLineTaxBoxSupporter.Rate => ZString.Empty;

		ZString IDocSADHLineTaxBoxSupporter.RateDuty => ZString.Empty;

		ZString IDocSADHLineTaxBoxSupporter.RateOverride => ZString.Empty;

		ZString IDocSADHLineTaxBoxSupporter.AmountInDeclarationCurrency => AmountInDeclarationCurrencyCore();
		protected virtual ZString AmountInDeclarationCurrencyCore() => C1_ChargeAmount.ToString(2);

		ZString IDocSADHLineTaxBoxSupporter.MethodOfPayment => C1_MethodOfPayment;

		ZString IDocSADHLineTaxBoxSupporter.NationalFeeTypeCode => ZString.Empty;

		ZString IDocSADHLineTaxBoxSupporter.DeclarationMethodOfPayment => EntryHeader?.Declaration?.JE_PaymentMethod ?? ZString.Empty;
	}
}
