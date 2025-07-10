using Enterprise.CustomerService.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.EventManagement;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	[TestedType(typeof(EdiIncidentRequest))]
	internal class EdiIncidentRequestTest : EnterpriseBusinessObjectTestCase
	{
		public void TestTypeDecider()
		{
			AssertType<EdiIncidentRequest>(Factory.New<IncidentRequest>());
		}

		public void TestHumanReadableNames()
		{
			var request = Factory.New<EdiIncidentRequest>();
			AssertEquals("IncidentRequest", request.HumanReadableName);
			AssertEquals("IncidentRequest", request.HumanReadableShortcutName);
		}

		public void TestDocManagerInfo()
		{
			var request = Factory.New<EdiIncidentRequest>();
			AssertType<IncidentDocManagerInfo>(request.DocManagerInfo);
		}

		public void TestRelatedSupportIncident()
		{
			var savedRequest = Factory.New<EdiIncidentRequest>();
			var savedIncident = Factory.NewWithValidTestData<SupportIncident>();
			Factory.Save();

			var unsavedRequest = Factory.New<EdiIncidentRequest>();
			var unsavedIncident = Factory.New<SupportIncident>();

			AssertNull(savedRequest.RelatedSupportIncident);
			AssertEquals(savedIncident, savedIncident.Request.RelatedSupportIncident);
			AssertNull(unsavedRequest.RelatedSupportIncident);
			AssertEquals(unsavedIncident, unsavedIncident.Request.RelatedSupportIncident);
		}

		public void TestProcessHanleringInfo()
		{
			var request = Factory.New<EdiIncidentRequest>();
			var info = ((IProcessHandlingInfoProvider)request).ProcessHandlingInfo;
			AssertType<EdiIncidentRequestProcessHandlingInfo>(info);
		}

		public void TestGlobalSearchBusinessObjectProvider()
		{
			var request = Factory.New<EdiIncidentRequest>();
			AssertEquals("EdiIncidentRequest is IGlobalSearchBusinessObjectProvider", true, request is IGlobalSearchBusinessObjectProvider);
			AssertEquals(request.RelatedSupportIncident, ((IGlobalSearchBusinessObjectProvider)request).BusinessObjectForController);
		}
	}
}
