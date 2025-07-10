using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.DocumentWrappers.Customs.EU
{
	public class DocSADHLineTax : DocBaseWrapper
	{
		public static DocSADHLineTax New(IDocSADHLineTaxBoxSupporter taxSupporter, BusinessObjectFactory factory)
		{
			return taxSupporter == null ? null : new DocSADHLineTax(taxSupporter, factory);
		}

		protected DocSADHLineTax(IDocSADHLineTaxBoxSupporter taxSupporter, BusinessObjectFactory factory) : base(taxSupporter, factory)
		{
			this.taxSupporter = Argument.NotNull(taxSupporter, nameof(taxSupporter));
		}

		protected readonly IDocSADHLineTaxBoxSupporter taxSupporter;

		public ZString G4_Type => G4_TypeCore;

		protected virtual ZString G4_TypeCore => taxSupporter.Type;

		public ZString Box47b => taxSupporter.TaxBase;

		public ZString Box47c1 => taxSupporter.Rate;

		public ZString G4_RateDuty => taxSupporter.RateDuty;

		public ZString G4_RateOverride => G4_RateOverrideCore;

		protected virtual ZString G4_RateOverrideCore => taxSupporter.RateOverride;

		public ZString G4_Amount_InDeclarationCurrency => taxSupporter.AmountInDeclarationCurrency;

		public virtual ZString TaxType => G4_Type;
		public virtual ZString TaxMethodOfPayment => G4_MethodOfPayment;

		public ZString G4_MethodOfPayment => G4_MethodOfPaymentCore;

		protected virtual ZString G4_MethodOfPaymentCore => taxSupporter.MethodOfPayment;
	}
}
