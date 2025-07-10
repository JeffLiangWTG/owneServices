using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	[TestedType(typeof(IncidentTriageChecklistItem))]
	public class IncidentTriageChecklistItemTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return factory.NewWithValidTestData<IncidentTriageChecklistItem>();
		}

		public void TestHumanReadableName()
		{
			var triage = Factory.NewWithValidTestData<IncidentTriageChecklistItem>();
			triage.IMC_SupportDescription = "DESC:123";
			AssertEquals("Checklist Item - DESC:123", triage.HumanReadableShortcutName);
			AssertEquals("Checklist Item", triage.HumanReadableName);
		}
	}
}
