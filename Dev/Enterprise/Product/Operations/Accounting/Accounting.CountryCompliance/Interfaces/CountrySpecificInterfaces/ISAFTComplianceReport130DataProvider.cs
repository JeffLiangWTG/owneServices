namespace Enterprise.Accounting.CountryCompliance.Interfaces
{
	public interface ISAFTComplianceReport130DataProvider
	{
		string TaxAccountingBasis { get; }
		string TaxAuthority { get; }
		string TaxTableDescription { get; }
		string JournalType { get; }
	}
}
