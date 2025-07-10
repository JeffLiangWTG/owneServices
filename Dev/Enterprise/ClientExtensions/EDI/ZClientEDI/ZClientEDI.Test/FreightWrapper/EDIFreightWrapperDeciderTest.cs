using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.DocumentWrappers.GenericWrappers;

namespace Enterprise.Client.EDI.Business.Test
{
	public class EDIFreightWrapperDeciderTest : TestCaseWithFactory
	{
		public void TestNewFreightWrapperOverride()
		{
			ProfessionalServicesQuote quote = Factory.NewWithValidTestData<ProfessionalServicesQuote>();
			FreightWrapper wrapper = FreightWrapper.NewFreightWrapper(quote, Factory);
			AssertEquals("wrapper should be type \"ProfessionalServicesQuote\" wrapper", typeof(FreightWrapperFromProfessionalServicesQuote), wrapper.GetType());

			SupportIncident supportIncident = Factory.NewWithValidTestData<SupportIncident>();
			wrapper = FreightWrapper.NewFreightWrapper(supportIncident, Factory);
			AssertEquals("wrapper should be type \"SupportIncident\" wrapper", typeof(FreightWrapperFromSupportIncident), wrapper.GetType());

			var incidentGroup = Factory.NewWithValidTestData<IncidentManagementGroup>();
			wrapper = FreightWrapper.NewFreightWrapper(incidentGroup, Factory);
			AssertEquals("wrapper should be type \"IncidentManagementGroup\" wrapper", typeof(FreightWrapperFromIncidentManagementGroup), wrapper.GetType());

			EDIProject project = Factory.NewWithValidTestData<EDIProject>();
			wrapper = FreightWrapper.NewFreightWrapper(project, Factory);
			AssertEquals("wrapper should be type \"Project\" wrapper", typeof(FreightWrapperFromProject), wrapper.GetType());
		}
	}
}
