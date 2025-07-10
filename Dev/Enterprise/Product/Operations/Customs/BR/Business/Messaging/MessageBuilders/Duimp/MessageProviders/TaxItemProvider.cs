using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.BR.MessageContracts.Duimp.Outgoing;

namespace Enterprise.Customs.BR.Business.Duimp
{
	public class TaxItemProvider : ITaxItem
	{
		TaxItemProvider(DuimpTaxRegime taxRegime)
		{
			this.taxRegime = Argument.NotNull(taxRegime, nameof(taxRegime));
			invoiceLine = taxRegime.Parent;
		}
		readonly DuimpTaxRegime taxRegime;
		readonly JobComInvoiceLine invoiceLine;

		public static TaxItemProvider New(DuimpTaxRegime taxRegime) => taxRegime == null ? null : new TaxItemProvider(taxRegime);

		public string TaxCode => BRRefCusMapper.MapCW1RateTypeToCustomsCode(taxRegime.Factory, taxRegime.CSI_SubType);

		public int TaxRegimeCode => int.TryParse(taxRegime.CSI_Code.KeepNumericCharacters(), out var code) ? code : 0;

		public int LegalBasisCode => int.TryParse(taxRegime.CSI_Procedure.KeepNumericCharacters(), out var code) ? code : 0;

		public IEnumerable<IAttributeItem> Attributes => fAttributes ??= invoiceLine.TaxRegimeAttributes
			.Where(x => taxRegime.Profiles.Any(profile => profile.QuestionCode == x.CY_Code))
			.SelectMany(GetAttributesRecursively).WhereNotNull().ToArray();
		IAttributeItem[] fAttributes;

		IEnumerable<IAttributeItem> GetAttributesRecursively(AttributeCusCodeData attr) =>
			new[] { AttributeProvider.New(attr) }.Union(attr.ChildAttributes?.SelectMany(GetAttributesRecursively));
	}
}
