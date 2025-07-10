using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	[TestedType(typeof(IncidentManagementGroupMessage))]
	public class IncidentManagementGroupMessageTest : EnterpriseBusinessObjectTestCase
	{
		public void TestGroup()
		{
			var message = Factory.NewWithValidTestData<IncidentManagementGroupMessage>();
			var group = Factory.NewWithValidTestData<IncidentManagementGroup>();
			message.IGM_ING_Group = group.PK;
			Factory.Save();

			AssertEquals(group, message.IncidentManagementGroup);
		}

		[TestDate(2023, 9, 1, 12, 0, 0)]
		public void TestSystemLastEditTimeShouldBeEmptyWhenGroupCreated()
		{
			var message = GetNewMessage();
			Factory.Save();
			AssertEquals(ZDateTime.Empty, message.IGM_SystemLastEditTimeUtc);

			message.IGM_IsPublished = false;
			message.IGM_Type = IncidentManagementGroupMessageTypePairList.Codes.AutoReply;
			message.IGM_Message = "Test for update time";
			Factory.Save();
			AssertEquals("Should update time", new ZDateTime(2023, 9, 1, 12, 0, 0), message.IGM_SystemLastEditTimeUtc);
		}

		public void TestSetEventFlagByCode()
		{
			var message = GetNewMessage();
			var group = message.IncidentManagementGroup;

			AssertPublishEventFlag(message, string.Empty, false);
			message.SetEventFlagByCode(IncidentManagementGroupMessage.EventType.PublishBroadcastMessage, true);
			AssertPublishEventFlag(message, string.Empty, true);

			AssertExceptionThrown(typeof(KeyNotFoundException), () =>
			{
				message.SetEventFlagByCode("NTZ", true);
			});
		}

		public void TestGetEventFlagByCode()
		{
			var message = GetNewMessage();
			var group = message.IncidentManagementGroup;

			AssertEquals(false, message.GetEventFlagByCode(IncidentManagementGroupMessage.EventType.PublishBroadcastMessage));
			message.SetEventFlagByCode(IncidentManagementGroupMessage.EventType.PublishBroadcastMessage, true);
			AssertEquals(true, message.GetEventFlagByCode(IncidentManagementGroupMessage.EventType.PublishBroadcastMessage));

			AssertExceptionThrown(typeof(KeyNotFoundException), () =>
			{
				message.GetEventFlagByCode("NTZ");
			});
		}

		public void TestCreateEventLogByFlag()
		{
			var message = GetNewMessage();
			var group = message.IncidentManagementGroup;

			message.SetEventFlagByCode(IncidentManagementGroupMessage.EventType.NewBroadcastMessage, true);
			message.SetEventFlagByCode(IncidentManagementGroupMessage.EventType.RevertBroadcastMessage, true);
			message.SetEventFlagByCode(IncidentManagementGroupMessage.EventType.PublishBroadcastMessage, true);

			Factory.Save();
			Assert(!message.GetEventFlagByCode(IncidentManagementGroupMessage.EventType.NewBroadcastMessage));
			Assert(!message.GetEventFlagByCode(IncidentManagementGroupMessage.EventType.RevertBroadcastMessage));
			Assert(!message.GetEventFlagByCode(IncidentManagementGroupMessage.EventType.PublishBroadcastMessage));

			Assert(!group.Logs.HasChanges);
			group = new BusinessObjectFactory().Load<IncidentManagementGroup>(group.PK);
			var messages = group.IncidentManagementGroupMessages;

			var bcmEventQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.BroadcastMessage.Code);
			var bcmEvents = group.Logs.Find(bcmEventQuery);

			var publishEvent = bcmEvents.Where(x => x.SL_Reference.Contains(message.GetEventDescriptionByCode(IncidentManagementGroupMessage.EventType.PublishBroadcastMessage)));
			var revertEvent = bcmEvents.Where(x => x.SL_Reference.Contains(message.GetEventDescriptionByCode(IncidentManagementGroupMessage.EventType.RevertBroadcastMessage)));
			var addNewEvent = bcmEvents.Where(x => x.SL_Reference.Contains(message.GetEventDescriptionByCode(IncidentManagementGroupMessage.EventType.NewBroadcastMessage)));

			AssertEquals(1, publishEvent.Count());
			AssertEquals(1, revertEvent.Count());
			AssertEquals(1, addNewEvent.Count());

			Assert(ZDateTimeOffset.Now >= publishEvent.First().SL_EventTimeOffset);
			Assert(ZDateTimeOffset.Now >= revertEvent.First().SL_EventTimeOffset);
			Assert(ZDateTimeOffset.Now >= addNewEvent.First().SL_EventTimeOffset);
		}

		public void TestEventLogChangeBetweenPublishAndRevert()
		{
			var message = GetNewMessage();
			var group = message.IncidentManagementGroup;

			Assert(!message.IsInDatabase);
			Assert(!message.IGM_IsPublished);

			AssertPublishEventFlag(message, string.Empty,false);
			AssertRevertEventFlag(message, string.Empty, false);

			message.IGM_IsPublished = true;
			AssertPublishEventFlag(message, "(1)PUBLISH should be created if IsPublish is true", true);
			AssertRevertEventFlag(message, "(1)REVERT should not be created because the original value of IsPublish is false", false);

			message.IGM_IsPublished = false;
			AssertPublishEventFlag(message, "(2)PUBLISH should not be created if IsPublish is false", false);
			AssertRevertEventFlag(message, "(2)REVERT should not be created. Although IsPublish is from true to false, its original value is false", false);
			Factory.Save();

			message.IGM_IsPublished = false;
			AssertPublishEventFlag(message, "(3)PUBLISH should not be created, because IsPublish doesn't have changes", false);
			AssertRevertEventFlag(message, "(3)REVERT should not be created, because IsPublish doesn't have changes", false);

			message.IGM_IsPublished = true;
			AssertPublishEventFlag(message, "(4)PUBLISH should be created, because IsPublish is from false to true", true);
			AssertRevertEventFlag(message, "(4)REVERT should not be created, because IsPublish is from false to true and this message isn't in the database", false);

			message.IGM_IsPublished = false;
			AssertPublishEventFlag(message, string.Empty, false);
			AssertRevertEventFlag(message, string.Empty, false);

			message.IGM_IsPublished = true;
			AssertPublishEventFlag(message, string.Empty, true);
			AssertRevertEventFlag(message, string.Empty, false);
			Factory.Save();

			message.IGM_IsPublished = true;
			AssertPublishEventFlag(message, "(5)PUBLISH should not be created, because IsPublish doesn't have changes", false);
			AssertRevertEventFlag(message, "(5)REVERT should not be created, because IsPublish doesn't have changes", false);

			message.IGM_IsPublished = false;
			AssertPublishEventFlag(message, "(6)PUBLISH should not be created, because IsPublish is false", false);
			AssertRevertEventFlag(message, "(6)REVERT should be created, because IsPublish is from true to false and the value in database is true", true);

			message.IGM_IsPublished = true;
			AssertPublishEventFlag(message, "(7)PUBLISH should be created, because IsPublish recover to true that means this message will be republished", true);
			AssertRevertEventFlag(message, "(7)REVERT should be created", true);

			message.IGM_IsPublished = false;
			AssertPublishEventFlag(message, string.Empty, false);
			AssertRevertEventFlag(message, string.Empty, true);
			Factory.Save();

			message.IGM_IsPublished = false;
			AssertPublishEventFlag(message, "(8)PUBLISH should not be created, because IsPublish doesn't have changes", false);
			AssertRevertEventFlag(message, "(8)REVERT should not be created, because IsPublish doesn't have changes", false);

			message.IGM_IsPublished = true;
			AssertPublishEventFlag(message, "(9)PUBLISH should be created, because IsPublish is from false to true", true);
			AssertRevertEventFlag(message, "(9)REVERT should not be created", false);

			message.IGM_IsPublished = false;
			AssertPublishEventFlag(message, "(10)PUBLISH should not be created, because IsPublish recovered to false", false);
			AssertRevertEventFlag(message, "(10)REVERT should not be created", false);
		}

		void AssertPublishEventFlag(IncidentManagementGroupMessage message, string assertMessage ,bool expected)
		{
			AssertEquals(assertMessage, expected, message.GetEventFlagByCode(IncidentManagementGroupMessage.EventType.PublishBroadcastMessage));
		}

		void AssertRevertEventFlag(IncidentManagementGroupMessage message, string assertMessage, bool expected)
		{
			AssertEquals(assertMessage, expected, message.GetEventFlagByCode(IncidentManagementGroupMessage.EventType.RevertBroadcastMessage));
		}

		IncidentManagementGroup GetNewGroup() => Factory.NewWithValidTestData<IncidentManagementGroup>();
		IncidentManagementGroupMessage GetNewMessage()
		{
			var message = Factory.NewWithValidTestData<IncidentManagementGroupMessage>();
			var group = GetNewGroup();
			message.IGM_ING_Group = group.PK;

			return message;
		}
	}
}
