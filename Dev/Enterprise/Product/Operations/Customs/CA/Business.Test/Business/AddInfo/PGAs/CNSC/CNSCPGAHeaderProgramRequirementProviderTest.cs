using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(CNSCPGAHeader))]
	sealed class CNSCPGAHeaderProgramRequirementProviderTest : InvoiceLinePGAProgramRequirementProviderTest<CNSCPGAHeader>
	{
		protected override ZString AgencyCode
		{
			get { return PGACodes.Codes.CNSC; }
		}
	}
}
