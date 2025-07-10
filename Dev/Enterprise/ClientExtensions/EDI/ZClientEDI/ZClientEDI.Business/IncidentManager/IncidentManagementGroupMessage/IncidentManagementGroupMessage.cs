using System.Collections.Generic;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using CargoWise.Workflow;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Res = ZClientEDI.Business.Res;
using ResString = ZClientEDI.Business.ResString;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public class IncidentManagementGroupMessage : AutoIncidentManagementGroupMessage
	{
		public IncidentManagementGroupMessage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			if (!IsInDatabase)
			{
				SetEventFlagByCode(EventType.NewBroadcastMessage, true);
			}
			this.IGM_IsPublishedInfo.ValueChanged += IGM_IsPublishedInfo_SetFlagAfterValueChanged;
		}

		[MaxLength(3)]
		[List(nameof(Lookups) + "." + nameof(IncidentManagementGroupMessageLookups.Types))]
		public override ZString IGM_Type
		{
			get { return base.IGM_Type; }
			set
			{
				if (base.IGM_Type != value)
				{
					base.IGM_Type = value;
				}
			}
		}

		public ZString TypeDescription => Lookups.Types.GetDescriptionFromCode(IGM_Type);

		[RelatedBusinessObject(nameof(Business.IncidentManagementGroup))]
		public override ZGuid IGM_ING_Group
		{
			get => base.IGM_ING_Group;
			set => base.IGM_ING_Group = value;
		}

		public ZString IsPublishedDescription => IGM_IsPublished ?
			Res.GetString("20C2EEDD-60B9-4FD1-BB5B-46887D21D394", "Published") : Res.GetString("D0B1D3E7-CAF8-4CDE-A8FD-2E888F87D2ED", "Draft");

		public IncidentManagementGroup IncidentManagementGroup => Factory.Load<IncidentManagementGroup>(IGM_ING_Group);

		public ZDateTime BroadcastDateLocal => IGM_BroadcastDateUtc.ToLocalBranchTime();

		public ZDateTime SystemLastEditTimeLocal => IGM_SystemLastEditTimeUtc.ToLocalBranchTime();

		public void SetIsPublished(bool isPublished)
		{
			IGM_IsPublished = isPublished;

			if (isPublished)
			{
				IGM_BroadcastDateUtc = ZDateTime.UtcNow;
			}
			else
			{
				IGM_BroadcastDateUtc = ZDateTime.Empty;
			}
		}

		#region Override

		public override void OnSaving()
		{
			base.OnSaving();
			if (!IsInDatabase && !IGM_IsPublished)
			{
				IGM_SystemLastEditTimeUtc = ZDateTime.Empty;
			}
		}

		public override void OnSaved(bool saveSucceeded)
		{
			base.OnSaved(saveSucceeded);
			if (saveSucceeded && !IsDeleted)
			{
				var needToSaveAgain = false;
				foreach (MessageEventInfo flag in MessageEventInfoList)
				{
					if (flag.ShouldPostEvent)
					{
						IncidentManagementGroup.Logs.AddNew(BroadcastMessageEventReference(flag));
						flag.ShouldPostEvent = false;
						needToSaveAgain = true;
					}
				}

				if (needToSaveAgain)
				{
					Factory.Save();
				}
			}
		}

		#endregion

		#region Broadcast Message Events

		void IGM_IsPublishedInfo_SetFlagAfterValueChanged(object sender, System.EventArgs e)
		{
			//Please see WI00562808 and CodeDraft_1.png in WI's eDoc if you have any question about this funcation
			var publishEvent = (MessageEventInfo)MessageEventInfoList[EventType.PublishBroadcastMessage];
			var revertEvent = (MessageEventInfo)MessageEventInfoList[EventType.RevertBroadcastMessage];

			publishEvent.ShouldPostEvent = this.IGM_IsPublished;
			if (this.IsInDatabase)
			{
				if (this.IGM_IsPublishedInfo.HasChanges)
				{
					revertEvent.ShouldPostEvent = !this.IGM_IsPublished;
				}
				else
				{
					revertEvent.ShouldPostEvent = IGM_IsPublished;
				}
			}
			else
			{
				revertEvent.ShouldPostEvent = false;
			}
		}

		internal void SetEventFlagByCode(string eventCode, bool value)
		{
			if (MessageEventInfoList[eventCode] is MessageEventInfo eventInfo)
			{
				eventInfo.ShouldPostEvent = value;
				return;
			}
			throw new KeyNotFoundException("Cannot find event by code");
		}

		internal bool GetEventFlagByCode(string eventCode)
		{
			if (MessageEventInfoList[eventCode] is MessageEventInfo eventInfo)
			{
				return eventInfo.ShouldPostEvent;
			}
			throw new KeyNotFoundException("Cannot find event by code");
		}

		public string GetEventDescriptionByCode(string eventCode)
		{
			return MessageEventInfoList.GetDescriptionFromCode(eventCode);
		}

		EventValue BroadcastMessageEventReference(MessageEventInfo eventInfo)
		{
			var reference = EventLogReferenceBuilder.New()
				.AddMandatory(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Action, eventInfo.Code)
				.AddMandatory(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Description, eventInfo.Description)
				.AddMandatory(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, this.IGM_Type)
				.Build();
			return new EventValue(Events.BroadcastMessage, eventTime: eventInfo.ReleaseTime, reference: reference);
		}

		#region Event Resources

		class MessageEventInfo : ICodeDescription
		{
			public MessageEventInfo(string code, string description, bool flag)
			{
				Code = code;
				Description = description;
				ShouldPostEvent = flag;
				ReleaseTime = ZDateTimeOffset.Empty;
			}

			public ZBool ShouldPostEvent
			{
				get => shouldPostEvent;
				set
				{
					shouldPostEvent = value;
					if (value)
					{
						ReleaseTime = ZDateTimeOffset.Now;
					}
				}
			}
			ZBool shouldPostEvent;

			public ZDateTimeOffset ReleaseTime { get; private set; }

			public object PK => null;

			public string Code { get; private set; }

			public string Description { get; private set; }
		}

		CodeDescriptionPairList MessageEventInfoList
		{
			get
			{
				if (messageEventInfoList == null)
				{
					messageEventInfoList = new CodeDescriptionPairList()
					{
						new MessageEventInfo(EventType.NewBroadcastMessage,     ResString.GetMultilingualString("f8f158db-78c8-47af-ba1b-f7248878450e", "New broadcast draft created"), false),
						new MessageEventInfo(EventType.PublishBroadcastMessage, ResString.GetMultilingualString("52d9f50a-0cea-4edb-be35-9ab16be79465", "Broadcast message published"), false),
						new MessageEventInfo(EventType.RevertBroadcastMessage,  ResString.GetMultilingualString("7198b009-1ef3-4068-ae6e-185c8f4af5d5", "Published broadcast reverted to draft"), false)
					};
				}
				return messageEventInfoList;
			}
		}
		CodeDescriptionPairList messageEventInfoList;

		public static class EventType
		{
			public const string NewBroadcastMessage = "NEW";
			public const string PublishBroadcastMessage = "PUBLISH";
			public const string RevertBroadcastMessage = "REVERT";
		}

		#endregion

		#endregion
	}
}
