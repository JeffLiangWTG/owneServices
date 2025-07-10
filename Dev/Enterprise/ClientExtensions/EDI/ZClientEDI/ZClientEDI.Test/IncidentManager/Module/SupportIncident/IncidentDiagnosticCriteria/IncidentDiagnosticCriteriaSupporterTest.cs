using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Services.OperationalActions.Support.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IncidentManager.Module.Testing
{
	[TestedType(typeof(IncidentDiagnosticCriteriaActionSupporter))]
	public class IncidentDiagnosticCriteriaActionSupporterTest : OperationalActionSupporterTest<IncidentDiagnosticCriteriaActionSupporter>
	{
		protected override ModuleIdentifier ModuleID
		{
			get { return Modules.ClientModuleRegistration.IncidentDiagnosticCriteria; }
		}

		public void TestSingularElementNoun()
		{
			AssertEquals(IncidentDiagnosticCriteria.SingularName, Supporter.SingularElementNoun);
		}

		public void TestPluralElementNoun()
		{
			AssertEquals("Diagnostic Criteria", Supporter.PluralElementNoun);
		}

		public override bool ShouldSupportDocuments => false;
	}
}
