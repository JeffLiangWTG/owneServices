using Enterprise.Accounting.CountryCompliance.Interfaces;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.CountryCompliance.Implementation.Norway
{
	class NorwaySAFTComplianceReport130DataProvider : ISAFTComplianceReport130DataProvider
	{
		public string TaxAccountingBasis => "A";
		public string TaxAuthority => (NoResString)"Skatteetaten";
		public string TaxTableDescription => (NoResString)"Merverdiavgift";
		public string JournalType => "A";
	}
}
