using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.DocumentWrappers.Customs.EU
{
	public class DocSADHLineTaxCollection : DocBaseWrapperCollection<DocSADHLineTax>
	{
		public DocSADHLineTaxCollection(IEnumerable<IDocSADHLineTaxBoxSupporter> taxBoxSupporters, BusinessObjectFactory factory)
			: base(factory)
		{
			Argument.NotNull(taxBoxSupporters, nameof(taxBoxSupporters));

			foreach (var supporter in taxBoxSupporters)
			{
				Add(DocSADHLineTax.New(supporter, factory));
			}
		}

		protected DocSADHLineTaxCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public ZString TotalAmountInDeclarationCurrency => TotalAmountInDeclarationCurrencyCore;
		protected virtual ZString TotalAmountInDeclarationCurrencyCore => this.Cast<DocSADHLineTax>().Sum(lineTax => ZDecimal.ParseSafe(lineTax.G4_Amount_InDeclarationCurrency, 0m)).ToString();

		public ZString TotalBox47MethodOfPayment => TotalBox47MethodOfPaymentCore;
		protected virtual ZString TotalBox47MethodOfPaymentCore => ZString.Empty;
	}
}
