using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(HCPGAHeader))]
	sealed class HCPGAHeaderProgramRequirementProviderTest : InvoiceLinePGAProgramRequirementProviderTest<HCPGAHeader>
	{
		protected override ZString AgencyCode
		{
			get { return PGACodes.Codes.HC; }
		}
	}
}
