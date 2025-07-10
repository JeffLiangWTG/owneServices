using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Services.OperationalActions.Support.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IncidentManager.Module.Testing
{
	[TestedType(typeof(IncidentTriageActionSupporter))]
	public class IncidentTriageActionSupporterTest : OperationalActionSupporterTest<IncidentTriageActionSupporter>
	{
		protected override ModuleIdentifier ModuleID
		{
			get { return Modules.ClientModuleRegistration.IncidentTriage; }
		}

		public void TestSingularElementNoun()
		{
			AssertEquals(IncidentTriage.SingularName, Supporter.SingularElementNoun);
		}

		public void TestPluralElementNoun()
		{
			AssertEquals("Incident Triages", Supporter.PluralElementNoun);
		}

		public override bool ShouldSupportDocuments => false;
	}
}
