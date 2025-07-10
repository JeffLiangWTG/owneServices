using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Confluent.Kafka;
using Enterprise.AuditDataServices.Subscription.Common;
using Enterprise.Client.EDI.DevTools.Events;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.DevTools.Business.Audit.Subscribers
{
	public class GroupMembershipSubscriber : ActualDataChangesAuditSubscriber
	{
		public override bool NotifyInsert => true;
		public override bool NotifyUpdate => false;
		public override bool NotifyDelete => true;
		public override string Code => "DTG";
		public override string Description => "DevTools Group Membership Subscriber";
		public override ITableSchema Table => GlbGroupLinkSchema.Instance;
		public override IEnumerable<SchemaColumn> SpecificColumns => null;
		public override Action<DataRow> CustomFilter => null;

		public override bool IsRequired()
			=> !string.IsNullOrEmpty(EDIDataRegistry.Instance.DevToolsIntegrationKafkaBootstrapServers.Value);

		public override void ProcessChanges(ILogger logger, DataTable changeTable)
		{
			var relationships = CalculateRelationshipEvents(changeTable);
			logger.Log(LogType.Information, $"Calculated {relationships.Count} events from {changeTable.Rows.Count} changed rows.");

			if (relationships.Count == 0)
			{
				return;
			}

			var events = CalculateEvents(logger, relationships);
			PublishEvents(logger, EDIDataRegistry.Instance, events);
		}

		static List<DataEvent> CalculateRelationshipEvents(DataTable changeTable)
		{
			var relationships = new List<DataEvent>();

			foreach (DataRow row in changeTable.Rows)
			{
				Guid staffKey;
				Guid groupKey;
				EventType relationshipEvent;

				switch (row.RowState)
				{
					case DataRowState.Added:
						staffKey = (Guid)row[GlbGroupLinkSchema.GK_GS.Name];
						groupKey = (Guid)row[GlbGroupLinkSchema.GK_GG.Name];
						relationshipEvent = EventType.UserAddedToGroup;
						break;

					case DataRowState.Deleted:
						relationshipEvent = EventType.UserRemovedFromGroup;
						staffKey = (Guid)row[GlbGroupLinkSchema.GK_GS.Name, DataRowVersion.Original];
						groupKey = (Guid)row[GlbGroupLinkSchema.GK_GG.Name, DataRowVersion.Original];
						relationshipEvent = EventType.UserRemovedFromGroup;
						break;

					case DataRowState.Modified:
					case DataRowState.Unchanged:
					case DataRowState.Detached:
						continue;

					default:
						throw new ArgumentException($"Unexpected state of row in table: '{row.RowState:G}'", nameof(changeTable));
				}

				relationships.Add(new (staffKey, groupKey, relationshipEvent));
			}

			return relationships;
		}

		static List<UserEvent> CalculateEvents(ILogger logger, List<DataEvent> relationships)
		{
			var factory = new BusinessObjectFactory
			{
				NameForDebugging = nameof(GroupMembershipSubscriber),
				RefreshEnabled = false,
			};

			var affectedStaff = factory.Load<GlbStaff>(new ZQuery(GlbStaffSchema.PK, relationships.Select(k => k.StaffKey).Distinct()));
			var staffMap = affectedStaff.ToDictionary(s => s.PK, s => s.GS_EmailAddress);

			logger.Log(LogType.Debug, $"Loaded {staffMap.Count} staff.");

			var affectedGroups = factory.Load<GlbGroup>(new ZQuery(GlbGroupSchema.PK, relationships.Select(k => k.GroupKey).Distinct()));
			var groupMap = affectedGroups.ToDictionary(g => g.PK, g => g.GG_Code);

			logger.Log(LogType.Debug, $"Loaded {groupMap.Count} group(s).");

			var events = new List<UserEvent>(capacity: relationships.Count);

			foreach (var r in relationships)
			{
				events.Add(new UserEvent
				{
					Type = r.Type,
					GroupName = groupMap[r.GroupKey],
					UserEmailAddress = staffMap[r.StaffKey],
				});
			}

			return events;
		}

		void PublishEvents(ILogger logger, EDIDataRegistry registry, IReadOnlyCollection<UserEvent> events)
		{
			var config = new ProducerConfig
			{
				BootstrapServers = registry.DevToolsIntegrationKafkaBootstrapServers.Value,
				SecurityProtocol = SecurityProtocol.SaslSsl,
				SaslMechanism = SaslMechanism.Plain,
				SaslUsername = registry.DevToolsIntegrationKafkaSaslUserName.Value,
				SaslPassword = registry.DevToolsIntegrationKafkaSaslPassword.Value,
			};

			var builder = new ProducerBuilder<Null, UserEvent>(config)
				.SetValueSerializer(new UserEventKafkaSerializer());

			using var producer = ProducerBuilderFunction(builder);

			foreach (var @event in events)
			{
				var message = new Message<Null, UserEvent> { Value = @event };
				producer.Produce(registry.DevToolsIntegrationKafkaTopic.Value, message);
			}

			_ = producer.Flush(timeout: TimeSpan.FromSeconds(30));

			logger.Log(LogType.Information, $"Published {events.Count} events to Kafka.");
		}

		public Func<ProducerBuilder<Null, UserEvent>, IProducer<Null, UserEvent>> ProducerBuilderFunction { get; set; } = static b => b.Build();

		class DataEvent
		{
			public DataEvent(Guid staffKey, Guid groupKey, EventType type)
			{
				StaffKey = staffKey;
				GroupKey = groupKey;
				Type = type;
			}

			public Guid StaffKey { get; }
			public Guid GroupKey { get; }
			public EventType Type { get; }
		}
	}
}
