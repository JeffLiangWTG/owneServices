using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Services.OperationalActions.Support.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IncidentManager.Module.Testing
{
	[TestedType(typeof(InvestigationItemActionSupporter))]
	public class InvestigationItemActionSupporterTest : OperationalActionSupporterTest<InvestigationItemActionSupporter>
	{
		protected override ModuleIdentifier ModuleID
		{
			get { return Modules.ClientModuleRegistration.InvestigationItem; }
		}

		public void TestSingularElementNoun()
		{
			AssertEquals(InvestigationItem.SingularName, Supporter.SingularElementNoun);
		}

		public void TestPluralElementNoun()
		{
			AssertEquals("Investigation Items", Supporter.PluralElementNoun);
		}

		public override bool ShouldSupportDocuments => false;
	}
}
