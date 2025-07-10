using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(PHACPGAHeader))]
	sealed class PHACPGAHeaderProgramRequirementProviderTest : InvoiceLinePGAProgramRequirementProviderTest<PHACPGAHeader>
	{
		protected override ZString AgencyCode => PGACodes.Codes.PHAC;
	}
}
