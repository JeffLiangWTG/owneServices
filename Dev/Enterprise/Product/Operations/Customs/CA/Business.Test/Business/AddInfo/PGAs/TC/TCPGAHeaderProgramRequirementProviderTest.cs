using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(TCPGAHeader))]
	sealed class TCPGAHeaderProgramRequirementProviderTest : InvoiceLinePGAProgramRequirementProviderTest<TCPGAHeader>
	{
		protected override ZString AgencyCode
		{
			get { return PGACodes.Codes.TC; }
		}
	}
}
