using Enterprise.Services.OperationalActions.Support.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Client.EDI.MasterFiles.Module.Testing
{
	[TestedType(typeof(EdiOrganisationActionSupporter))]
	internal sealed class EdiOrganisationActionSupporterTest : OperationalActionSupporterTest<EdiOrganisationActionSupporter>
	{
		protected override ModuleIdentifier ModuleID
		{
			get
			{
				return ModuleIDs.Organisation;
			}
		}
	}
}
