using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(DFOPGAHeader))]
	sealed class DFOPGAHeaderProgramRequirementProviderTest : InvoiceLinePGAProgramRequirementProviderTest<DFOPGAHeader>
	{
		protected override ZString AgencyCode
		{
			get { return PGACodes.Codes.DFO; }
		}
	}
}
