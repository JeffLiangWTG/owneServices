using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using CargoWise.EntityFramework;
using Confluent.Kafka;
using Enterprise.AuditDataServices.DevTools;
using Enterprise.AuditDataServices.Subscription.Common;
using Enterprise.AuditDataServices.Subscription.Testing;
using Enterprise.Client.EDI.DevTools.Events;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.Client.EDI.DevTools.Business.Audit.Subscribers.Test
{
	[TestedType(typeof(GroupMembershipSubscriber))]
	public class GroupMembershipSubscriberTest : ActualDataChangesAuditSubscriberTest
	{
		protected override DataTable GetTestDataTable() => null;

		public override void TestCustomFilter()
		{
			var subscriber = new GroupMembershipSubscriber();
			AssertNull(subscriber.CustomFilter);
		}

		public void TestIsEligibleToBeLoaded()
		{
			var serviceTask = new DevToolsSubscriberServiceTask();
			var subscriber = typeof(GroupMembershipSubscriber);
			AssertEquals(serviceTask.AssemblyName, new AssemblyName(subscriber.Assembly.FullName).Name);
			AssertEquals(serviceTask.SubscriberNamespace, subscriber.Namespace);
		}

		public void TestSubscriberPublishesUserAddedEvent()
		{
			var factory = new BusinessObjectFactory();

			var group = MakeGroup(factory, "GHUB");
			var staff = MakeStaff(factory, "//G", "user@example.com");

			var dt = MakeChangeDataTable();
			var row = dt.NewRow();
			row[AuditFieldNames.StartLsnFieldName] = new byte[] { 0x01 };
			row[AuditFieldNames.SeqValFieldName] = new byte[] { 0x01 };
			row[AuditFieldNames.OperationFieldName] = 1; // INSERT
			row[AuditFieldNames.TranEndTimeUtc] = DateTime.UtcNow;
			row[GlbGroupLinkSchema.GK_GS.Name] = staff;
			row[GlbGroupLinkSchema.GK_GG.Name] = group;
			dt.Rows.Add(row);

			var events = CoreTestSubscriberPublishesEvent(dt);
			AssertEquals(1, events.Count);

			var evt = events[0];
			AssertEquals(EventType.UserAddedToGroup, evt.Type);
			AssertEquals("GHUB", evt.GroupName);
			AssertEquals("user@example.com", evt.UserEmailAddress);
		}

		public void TestSubscriberPublishesUserRemovedEvent()
		{
			var factory = new BusinessObjectFactory();

			var group = MakeGroup(factory, "~GH~");
			var staff = MakeStaff(factory, "//G", "first.last@example.com");

			var dt = MakeChangeDataTable();
			var row = dt.NewRow();
			row[AuditFieldNames.StartLsnFieldName] = new byte[] { 0x01 };
			row[AuditFieldNames.SeqValFieldName] = new byte[] { 0x01 };
			row[AuditFieldNames.OperationFieldName] = 1; // INSERT
			row[AuditFieldNames.TranEndTimeUtc] = DateTime.UtcNow;
			row[GlbGroupLinkSchema.GK_GS.Name] = staff;
			row[GlbGroupLinkSchema.GK_GG.Name] = group;
			dt.Rows.Add(row);
			dt.AcceptChanges();

			row.Delete();

			var events = CoreTestSubscriberPublishesEvent(dt);
			AssertEquals(1, events.Count);

			var evt = events[0];
			AssertEquals(EventType.UserRemovedFromGroup, evt.Type);
			AssertEquals("~GH~", evt.GroupName);
			AssertEquals("first.last@example.com", evt.UserEmailAddress);
		}

		List<UserEvent> CoreTestSubscriberPublishesEvent(DataTable table)
		{
			var subscriber = MakeSubscriber();

			var events = new List<UserEvent>();

			var mockProducer = new Mock<IProducer<Null, UserEvent>>(MockBehavior.Strict);
			_ = mockProducer.Setup(p => p.Flush(It.IsAny<TimeSpan>())).Returns(0);
			_ = mockProducer.Setup(p => p.Dispose());

			_ = mockProducer
				.Setup(x => x.Produce(It.IsAny<string>(), It.IsAny<Message<Null, UserEvent>>(), It.IsAny<Action<DeliveryReport<Null, UserEvent>>>()))
				.Callback((string topic, Message<Null, UserEvent> message, Action<DeliveryReport<Null, UserEvent>> deliveryHandler) =>
				{
					events.Add(message.Value);
				});

			subscriber.ProducerBuilderFunction = builder => mockProducer.Object;
			subscriber.ProcessChanges(Mock.Of<ILogger>(), table);

			return events;
		}

		GroupMembershipSubscriber MakeSubscriber() => (GroupMembershipSubscriber)NewDataChangeSubscriber();

		Guid MakeStaff(BusinessObjectFactory factory, string code, string email)
		{
			var staff = MasterFilesTestHelper.CreateStaff(factory, code, string.Empty, email);
			factory.Save();
			return staff.PK.ToGuid();
		}

		Guid MakeGroup(BusinessObjectFactory factory, string name)
		{
			var group = MasterFilesTestHelper.CreateGroup(factory, name);
			factory.Save();
			return group.PK.ToGuid();
		}

		static DataTable MakeChangeDataTable()
		{
			var changeTable = new DataTable();
			_ = changeTable.Columns.Add(AuditFieldNames.StartLsnFieldName, typeof(byte[]));
			_ = changeTable.Columns.Add(AuditFieldNames.SeqValFieldName, typeof(byte[]));
			_ = changeTable.Columns.Add(AuditFieldNames.OperationFieldName, typeof(int));
			_ = changeTable.Columns.Add(AuditFieldNames.TranEndTimeUtc, typeof(DateTime));
			_ = changeTable.Columns.Add(GlbGroupLinkSchema.GK_GS.Name, typeof(Guid));
			_ = changeTable.Columns.Add(GlbGroupLinkSchema.GK_GG.Name, typeof(Guid));
			return changeTable;
		}
	}
}
