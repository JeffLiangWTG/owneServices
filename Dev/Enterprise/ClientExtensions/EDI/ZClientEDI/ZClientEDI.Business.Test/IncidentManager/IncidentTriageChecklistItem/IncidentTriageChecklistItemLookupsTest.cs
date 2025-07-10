using System.Linq;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	public class IncidentTriageChecklistItemLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestIMC_Category_ShouldMatchLookups()
		{
			var checklistItem1 = Factory.NewWithValidTestData<IncidentTriageChecklistItem>();
			var checklistItem2 = Factory.NewWithValidTestData<IncidentTriageChecklistItem>();
			var checklistItem1Categorys = checklistItem1.Lookups.Categorys.ToArray();
			AssertArrayEqualsByElements(checklistItem1Categorys, checklistItem2.Lookups.Categorys.ToArray());
			AssertEquals(true, checklistItem1Categorys.Any(x => x.Code.Equals("CAC") && x.Description.Equals("Corrective Action - Client")));
			AssertEquals(true, checklistItem1Categorys.Any(x => x.Code.Equals("CAS") && x.Description.Equals("Corrective Action - Support")));
			AssertEquals(true, checklistItem1Categorys.Any(x => x.Code.Equals("DID") && x.Description.Equals("Diagnostic Document Request")));
			AssertEquals(true, checklistItem1Categorys.Any(x => x.Code.Equals("EXP") && x.Description.Equals("Explanation")));
			AssertEquals(true, checklistItem1Categorys.Any(x => x.Code.Equals("SVR") && x.Description.Equals("Service Request Parameter")));
		}
	}
}
