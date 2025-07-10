using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(GACPGAHeader))]
	sealed class GACPGAHeaderProgramRequirementProviderTest : InvoiceLinePGAProgramRequirementProviderTest<GACPGAHeader>
	{
		protected override ZString AgencyCode
		{
			get { return PGACodes.Codes.GAC; }
		}
	}
}
