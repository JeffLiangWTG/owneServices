using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(CFIAPGAHeader))]
	sealed class CFIAPGAHeaderProgramRequirementProviderTest : InvoiceLinePGAProgramRequirementProviderTest<CFIAPGAHeader>
	{
		protected override ZString AgencyCode
		{
			get { return PGACodes.Codes.CFIA; }
		}
	}
}
