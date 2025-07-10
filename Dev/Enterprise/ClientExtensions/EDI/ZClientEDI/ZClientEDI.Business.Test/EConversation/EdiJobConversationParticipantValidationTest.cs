using System;
using CargoWise.Definitions;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI;
using Enterprise.EConversation.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;

namespace ZClientEDI.Business.Test
{
	public class EdiJobConversationParticipantValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckEmailAddress()
		{
			using (ClientHookLoader.Instance.OverrideClientAssemblyForTest(Clients.EDI))
			{
				EDIDataRegistry.Instance.IncidentFromEmailAddress.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "test@test.com");
				var contact = Factory.NewWithValidTestData<OrgContact>();
				contact.OC_Email = "test@test.com";

				var conversation = Factory.NewWithValidTestData<JobConversation>();

				var p1 = conversation.RelatedParties.AddNewParticipant("test@test.com");
				p1.JCP_IsSubscribed = true;
				p1.Validation.ValidateAll();
				AssertHasError(p1.EmailAddressInfo, "Subscription not supported for Customer Service Email Address (Registry > WiseTech Global Client Extensions > System Email Addresses > Customer Service Email Address)");
			}
		}
	}
}
