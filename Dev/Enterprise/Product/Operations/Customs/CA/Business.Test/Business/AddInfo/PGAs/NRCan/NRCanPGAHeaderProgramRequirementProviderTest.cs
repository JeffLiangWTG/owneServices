using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(NRCanPGAHeader))]
	sealed class NRCanPGAHeaderProgramRequirementProviderTest : InvoiceLinePGAProgramRequirementProviderTest<NRCanPGAHeader>
	{
		protected override ZString AgencyCode
		{
			get { return PGACodes.Codes.NRCan; }
		}
	}
}
