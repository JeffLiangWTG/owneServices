using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.CustomerService.Business.Testing
{
	[TestedType(typeof(IncidentRequest))]
	sealed class IncidentRequestTest : EnterpriseBusinessObjectTestCase
	{
		public void TestSaveSetsIncidentNumber()
		{
			var incident = Factory.New<IncidentRequest>();
			AssertEquals("Precondition: empty", true, incident.INC_IncidentNumber.IsEmpty);

			incident.INC_IncidentNumber = "1";
			incident.OnSaved(false);
			AssertEquals("cleared if save failed", true, incident.INC_IncidentNumber.IsEmpty);

			Factory.Save();
			AssertEquals("filled in", false, incident.INC_IncidentNumber.IsEmpty);
		}
	}
}
