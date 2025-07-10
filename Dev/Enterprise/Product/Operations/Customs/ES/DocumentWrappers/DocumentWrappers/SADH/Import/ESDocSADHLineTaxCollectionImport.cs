using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.DocumentWrappers.Customs.EU;

namespace Enterprise.Customs.ES.DocumentWrappers.SADH
{
	public class ESDocSADHLineTaxCollectionImport : DocSADHLineTaxCollection
	{
		public ESDocSADHLineTaxCollectionImport(IEnumerable<IESDocSADHLineTaxBoxSupporter> taxBoxSupporters, BusinessObjectFactory factory)
			: base(factory)
		{
			Argument.NotNull(taxBoxSupporters, nameof(taxBoxSupporters));

			foreach (var supporter in taxBoxSupporters)
			{
				Add(ESDocSADHLineTaxImport.New(supporter, factory));
			}
		}

		protected ESDocSADHLineTaxCollectionImport(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		const string DecimalsFormat = "N2";
		const string DeferredMoPCode = "D";

		protected override ZString TotalAmountInDeclarationCurrencyCore => this.Cast<DocSADHLineTax>().
			Where(lineTax => lineTax.G4_MethodOfPayment != DeferredMoPCode).
			Sum(lineTax => ZDecimal.ParseSafe(lineTax.G4_Amount_InDeclarationCurrency, 0m)).ToString(DecimalsFormat);

		protected override ZString TotalBox47MethodOfPaymentCore
		{
			get
			{
				var sadLineTaxes = this.Cast<DocSADHLineTax>();
				var nonDeferredMopTax = sadLineTaxes.FirstOrDefault(lineTax => lineTax.G4_MethodOfPayment != DeferredMoPCode);
				return nonDeferredMopTax?.G4_MethodOfPayment ?? ZString.Empty;
			}
		}
	}
}
