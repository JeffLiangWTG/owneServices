using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class EventVisibility : RegistryBusinessObjectTemplate, ICanDelete
	{
		#region Schema

		public class Schema : RegistryBusinessObject.Schema
		{
			public const string EventCode = "EventCode";
			public const string EventDescription = "EventDescription";
		}

		#endregion

		#region Constructors

		public EventVisibility()
			: base()
		{
		}

		public EventVisibility(ZString eventCode)
			: this()
		{
			this.eventCode = eventCode;
		}

		#endregion

		#region Properties

		[BusinessObjectTestExclude]
		public EventVisibilityCollection Parent
		{
			get { return parent; }
			set
			{
				parent = value;
				if (!IsValidationSuspended)
				{
					ValidateEventType();
				}
			}
		}

		EventVisibilityCollection parent;

		#region EventCode

		[List("EventCodeList")]
		[MaxLength(3)]
		public ZString EventCode
		{
			get { return eventCode; }
			set
			{
				SetNonPersistentPropertyValue(EventCodeInfo, ref eventCode, value);
				if (!IsValidationSuspended)
				{
					ValidateEventType();
				}
			}
		}
		ZString eventCode;

		public ZPropertyInfo EventCodeInfo
		{
			get { return GetZPropertyInfo(Schema.EventCode); }
		}

		public CodeDescriptionPairList EventCodeList
		{
			get { return eventCodeList ?? (eventCodeList = EventTypeListProvider.CreateMilestoneEventTypeList(Constants.Workflow.MilestoneType, string.Empty, false, CurrentFactory)); }
		}

		CodeDescriptionPairList eventCodeList;

		#endregion

		#region EventDescription

		public ZString EventDescription
		{
			get { return (!string.IsNullOrEmpty(EventCode) && Events.All.Contains(EventCode)) ? Events.All[EventCode].Description : string.Empty; }
		}

		public ZPropertyInfo EventDescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.EventDescription); }
		}

		#endregion

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateEventType();
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new EventVisibility(EventCode);
		}

		#endregion

		#region Xml Serialisation

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.EventCode, EventCode);
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			eventCode = new ZString(reader.ReadElementString(Schema.EventCode));
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
			EventCodeInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(EventCodeInfo);
			if (!EventCode.IsEmpty && !EventCodeList.ContainsCode(EventCode))
			{
				EventCodeInfo.AddWarning(ResString.GetMultilingualString("cb515f8f-e3c2-4e89-9d31-69288a0a01cb", "Unknown event code"));
			}
			if (Parent != null)
			{
				var eventTypeCount = 0;
				foreach (EventVisibility eventInfo in Parent)
				{
					if (eventInfo != this && eventInfo.EventCode == EventCode)
					{
						eventTypeCount++;
					}
				}
				if (eventTypeCount > 0)
				{
					EventCodeInfo.AddError(ResString.GetMultilingualString("5875e17b-5519-4ad7-8497-e244ced13d66", "Event code must be unique"));
				}
			}
		}

		#endregion
	}
}
