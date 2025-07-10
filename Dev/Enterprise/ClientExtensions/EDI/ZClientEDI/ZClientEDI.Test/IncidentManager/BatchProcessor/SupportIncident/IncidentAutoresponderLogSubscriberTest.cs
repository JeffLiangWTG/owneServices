using System;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI.IncidentManager.Test;
using Enterprise.EConversation.Business;
using NUnit.Framework;

namespace ZClientEDI.Test.IncidentManager.BatchProcessor.SupportIncident
{
	public class IncidentAutoresponderLogSubscriberTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		[UseSnapshotProtection(skipTransaction: true)]
		public void TestLoadTop1JobConversationParticipantSingleItemSuccess()
		{
			// Arrange
			var logSubscriber = new IncidentAutoresponderLogSubscriberForTest();
			var participant = Factory.NewWithValidTestData<JobConversationParticipant>();
			var id = participant.JCP_ParticipantID;
			Factory.Save();
			// Act
			var fetchedParticipant = logSubscriber.FetchJobConversationParticipantExposed(Factory, id);
			// Assert
			AssertEquals(participant.JCP_JCC_Conversation, fetchedParticipant.JCP_JCC_Conversation);
		}

		[ExpectNoExceptions]
		[UseSnapshotProtection(skipTransaction: true)]
		public void TestLoadTop1JobConversationParticipantSingleItemNotFound()
		{
			// Arrange
			var logSubscriber = new IncidentAutoresponderLogSubscriberForTest();
			var participant = Factory.NewWithValidTestData<JobConversationParticipant>();
			var id = participant.JCP_ParticipantID;
			Factory.Save();
			// Act
			var fetchedParticipant = logSubscriber.FetchJobConversationParticipantExposed(Factory, ZGuid.NewZGuid());
			// Assert
			AssertNull(fetchedParticipant);
		}

		[ExpectException(typeof(ArgumentNullException))]
		[UseSnapshotProtection(skipTransaction: true)]
		public void TestLoadTop1JobConversationParticipantNullFactory()
		{
			// Arrange
			var logSubscriber = new IncidentAutoresponderLogSubscriberForTest();
			var participant = Factory.NewWithValidTestData<JobConversationParticipant>();
			var id = participant.JCP_ParticipantID;
			Factory.Save();
			//Act
			// Assert
			logSubscriber.FetchJobConversationParticipantExposed(null, id);
		}

		[ExpectException(typeof(ArgumentNullException))]
		[UseSnapshotProtection(skipTransaction: true)]
		public void TestLoadTop1JobConversationParticipantReturnNull()
		{
			//Arrange
			var logSubscriber = new IncidentAutoresponderLogSubscriberForTest();
			var participant = Factory.NewWithValidTestData<JobConversationParticipant>();
			var id = participant.JCP_ParticipantID;
			Factory.Save();
			//Act
			//Assert
			logSubscriber.FetchJobConversationParticipantExposed(null, ZGuid.NewZGuid());
		}

		[ExpectException(typeof(ArgumentException))]
		[UseSnapshotProtection(skipTransaction: true)]
		public void TestLoadTop1JobConversationParticipantReturnNullForEmptyId()
		{
			//Arrange
			var logSubscriber = new IncidentAutoresponderLogSubscriberForTest();
			var participant = Factory.NewWithValidTestData<JobConversationParticipant>();
			var id = participant.JCP_ParticipantID;
			Factory.Save();
			//Act
			//Assert
			logSubscriber.FetchJobConversationParticipantExposed(Factory, ZGuid.Empty);
		}

		[ExpectNoExceptions]
		[UseSnapshotProtection(skipTransaction: true)]
		public void TestLoadTop1JobConversationParticipantSingleAmongManyItemSuccess()
		{
			//Arrange
			var logSubscriber = new IncidentAutoresponderLogSubscriberForTest();
			var participant = Factory.NewWithValidTestData<JobConversationParticipant>();
			var id = participant.JCP_ParticipantID;
			for (var i = 0; i < 10; i++)
			{
				Factory.NewWithValidTestData<JobConversationParticipant>();
			}
			Factory.Save();
			//Act
			var fetchedParticipant = logSubscriber.FetchJobConversationParticipantExposed(Factory, id);
			//Assert
			AssertEquals(participant.JCP_JCC_Conversation, fetchedParticipant.JCP_JCC_Conversation);
		}
	}
}
