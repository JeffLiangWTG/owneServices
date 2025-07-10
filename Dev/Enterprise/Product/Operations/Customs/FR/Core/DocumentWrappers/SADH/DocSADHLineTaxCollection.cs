using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.FR.DocumentWrappers.SADH;

public class DocSADHLineTaxCollection : Enterprise.DocumentWrappers.Customs.EU.DocSADHLineTaxCollection
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

	protected override ZString TotalAmountInDeclarationCurrencyCore => this.Cast<DocSADHLineTax>().Where(lineTax => lineTax.G4_MethodOfPayment == "1" || lineTax.G4_MethodOfPayment == "2").Sum(lineTax => ZDecimal.ParseSafe(lineTax.G4_Amount_InDeclarationCurrency, 0m)).ToString();

	protected DocSADHLineTaxCollection(BusinessObjectFactory factory)
		: base(factory)
	{
	}
}
