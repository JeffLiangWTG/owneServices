using CargoWise.Types;

namespace Enterprise.DocumentWrappers.Customs.EU.ImportDeclarationDocument
{
	public interface IIDDDutiesAndTax
	{
		ZString NationalTaxType { get; }
		ZString TaxType { get; }
		ZString Amount { get; }
		ZString TaxRate { get; }
		ZString TaxAmount { get; }
		ZString PayableTaxAmount { get; }
		ZString Status { get; }
	}
}
