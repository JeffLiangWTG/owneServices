//CW1135:Do Not Use Country Specific Business Rule Analyzer

using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace AnalyzersRunner.FunctionalTestingTarget.CargoWise.Analyzers
{
	class CW1135
	{
		public void Method()
		{
			if (((IGlbCompany)GlbCompany.CurrentCompany).GC_RN_NKCountryCode == "AA")
			{
			}
		}
	}
}
