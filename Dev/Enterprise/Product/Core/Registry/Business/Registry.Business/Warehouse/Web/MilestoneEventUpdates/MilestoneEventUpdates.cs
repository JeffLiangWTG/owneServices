using System.Collections.Generic;
using System.Xml;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public abstract class MilestoneEventUpdates : RegistryBusinessObjectTemplate, ICanDelete
	{
		#region Schema

		public class Schema : RegistryBusinessObject.Schema
		{
			public const string WorkflowType = "WorkflowType";
			public const string EventType = "EventType";
		}

		#endregion

		#region Constructors

		public MilestoneEventUpdates()
			: base()
		{
		}

		public MilestoneEventUpdates(ZString eventType)
			: this()
		{
			this.eventType = eventType;
		}

		#endregion

		#region Properties

		public List<string> WebPartyTypes
		{
			get { return new List<string>(InternalData.Keys); }
		}

		[BusinessObjectTestExclude]
		public MilestoneEventUpdatesCollection Parent
		{
			get { return parent; }
			set
			{
				parent = value;
				if (!IsValidationSuspended && value != null)
				{
					ValidateEventType();
				}
			}
		}

		MilestoneEventUpdatesCollection parent;

		public ZString WorkflowType
		{
			get { return GetWorkflowType(); }
		}

		protected virtual ZString GetWorkflowType()
		{
			return ZString.Empty;
		}

		[List("EventTypeList")]
		[MaxLength(3)]
		public ZString EventType
		{
			get { return eventType; }
			set
			{
				SetNonPersistentPropertyValue(EventTypeInfo, ref eventType, value);
				if (!IsValidationSuspended)
				{
					ValidateEventType();
				}
			}
		}
		ZString eventType;

		public ZPropertyInfo EventTypeInfo
		{
			get { return GetZPropertyInfo(Schema.EventType); }
		}

		public CodeDescriptionPairList EventTypeList => Parent?.EventTypeList ?? new CodeDescriptionPairList();

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateEventType();
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return GetNewMilestoneEventUpdatesCopy();
		}

		protected abstract MilestoneEventUpdates GetNewMilestoneEventUpdatesCopy();

		#endregion

		#region Xml Serialisation

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.EventType, EventType);
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			eventType = new ZString(reader.ReadElementString(Schema.EventType));
		}

		#endregion

		#region ICanDelete Members

		bool ICanDelete.CanDelete
		{
			get { return true; }
		}

		MultilingualString ICanDelete.ReasonForNotAbleToDelete
		{
			get { return ResString.GetMultilingualString("bc2bd5a0-b96a-4456-8316-da8af68a91e0", "This is a system defined value and cannot be deleted."); }
		}

		#endregion

		#region Validation

		public void ValidateEventType()
		{
			EventTypeInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(EventTypeInfo);

			if (Parent != null)
			{
				if (!EventType.IsEmpty && !EventTypeList.ContainsCode(EventType))
				{
					EventTypeInfo.AddWarning(Res.GetString("cb515f8f-e3c2-4e89-9d31-69288a0a01cb", "Unknown event code"));
				}

				var eventTypeCount = 0;
				foreach (MilestoneEventUpdates eventUpdate in Parent)
				{
					if (eventUpdate != this && eventUpdate.EventType == EventType)
					{
						eventTypeCount++;
					}
				}
				if (eventTypeCount > 0)
				{
					EventTypeInfo.AddError(Res.GetString("5875e17b-5519-4ad7-8497-e244ced13d66", "Event code must be unique"));
				}
			}
		}

		#endregion

		#region Implementation

		public void SetValue(string webPartyType, bool value)
		{
			if (InternalData.ContainsKey(webPartyType))
			{
				InternalData[webPartyType] = value;
			}
			else
			{
				InternalData.Add(webPartyType, value);
			}
		}

		public bool GetValue(string webPartyType)
		{
			bool result = false;
			if (InternalData.ContainsKey(webPartyType))
			{
				result = InternalData[webPartyType];
			}
			return result;
		}

		protected Dictionary<string, bool> InternalData
		{
			get { return internalData ?? (internalData = new Dictionary<string, bool>()); }
		}

		Dictionary<string, bool> internalData;

		#endregion
	}
}
