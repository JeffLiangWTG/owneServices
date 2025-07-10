using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(ECCCPGAHeader))]
	sealed class ECCCPGAHeaderProgramRequirementProviderTest : InvoiceLinePGAProgramRequirementProviderTest<ECCCPGAHeader>
	{
		protected override ZString AgencyCode
		{
			get { return PGACodes.Codes.ECCC; }
		}
	}
}
