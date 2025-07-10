using CargoWise.EntityFramework.Testing;
using Enterprise.EConversation.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.EConversation.Testing
{
	[TestedType(typeof(JobConversationParticipantCollection))]
	sealed class JobConversationParticipantCollectionTest : ActiveBusinessObjectCollectionTestCase<JobConversationParticipantCollection>
	{
		public void TestNewItemsAreGivenTheProvidedTableCode()
		{
			var convo = Factory.NewWithValidTestData<JobConversation>();

			var participants = new JobConversationParticipantCollection(Factory, convo, "OC");
			AssertEquals("OC", participants.AddNew().JCP_ParticipantTableCode);

			participants = new JobConversationParticipantCollection(Factory, convo, "GS");
			AssertEquals("GS", participants.AddNew().JCP_ParticipantTableCode);
		}

		public void TestAddNewParticipant()
		{
			var nonSystemStaff = Factory.NewWithValidTestData<GlbStaff>();
			var parent = Factory.NewWithValidTestData<DummyConversationProvider>();
			Factory.Save();

			var webStaff = Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, User.WebUserCode);
			var serviceStaff = Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, User.ServiceUserCode);

			var convo = Factory.NewWithValidTestData<JobConversation>();
			var participants = convo.Participants;

			AssertEquals("support user is not subscribed", false, participants.AddNewParticipant((IConversationParticipant)Env.CurrentUser).JCP_IsSubscribed);
			AssertEquals("web user is not subscribed", false, participants.AddNewParticipant(webStaff).JCP_IsSubscribed);
			AssertEquals("service user is not subscribed", false, participants.AddNewParticipant(serviceStaff).JCP_IsSubscribed);
			AssertEquals("non-system user is subscribed", true, participants.AddNewParticipant(nonSystemStaff).JCP_IsSubscribed);
		}

		protected override JobConversationParticipantCollection GetCollectionToTest()
		{
			var conversation = Factory.NewWithValidTestData<JobConversation>();

			return new JobConversationParticipantCollection(Factory, conversation);
		}
	}
}
