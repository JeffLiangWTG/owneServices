using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.DocumentWrappers.Customs.EU;

namespace Enterprise.Customs.IT.Business;

class ITDocSADHLineTaxCollection : DocSADHLineTaxCollection
{
	public ITDocSADHLineTaxCollection(IEnumerable<IDocSADHLineTaxBoxSupporter> taxBoxSupporters, BusinessObjectFactory factory) : base(taxBoxSupporters, factory)
	{
	}

	protected override ZString TotalAmountInDeclarationCurrencyCore =>
		this.Cast<DocSADHLineTax>()
		.Where(lineTax => !ITDocSADHLineTaxHelper.ExcludeDutiesInTotals(lineTax.G4_MethodOfPayment))
		.Sum(lineTax => ZDecimal.ParseSafe(lineTax.G4_Amount_InDeclarationCurrency, 0m))
		.ToString(DecimalsFormat);

	const string DecimalsFormat = "N2";
}
