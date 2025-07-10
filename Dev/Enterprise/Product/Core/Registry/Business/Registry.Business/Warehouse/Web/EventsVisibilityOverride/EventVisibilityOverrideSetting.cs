using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Registry.Business.Web;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class EventVisibilityOverrideSetting : RegistryBusinessObject, ICanDelete
	{
		#region Schema

		protected new abstract class Schema : RegistryBusinessObject.Schema
		{
			public const string EventCode = nameof(CargoWise.Definitions.EventVisibilityOverrideSetting.EventCode);
			public const string IsActive = nameof(CargoWise.Definitions.EventVisibilityOverrideSetting.IsActive);
			public const string EventDetail = nameof(CargoWise.Definitions.EventVisibilityOverrideSetting.EventDetail);
			public const string QuickView = nameof(CargoWise.Definitions.EventVisibilityOverrideSetting.QuickView);
			public const string EventDescriptionOverride = nameof(CargoWise.Definitions.EventVisibilityOverrideSetting.EventDescriptionOverride);
			public const string DuplicateEventsHandlingMethod = nameof(CargoWise.Definitions.EventVisibilityOverrideSetting.DuplicateEventsHandlingMethod);
			public const string IncludeEstimates = nameof(CargoWise.Definitions.EventVisibilityOverrideSetting.IncludeEstimates);
		}

		#endregion

		#region Constructors

		public EventVisibilityOverrideSetting()
		{
		}

		public EventVisibilityOverrideSetting(FallbackLevel fallbackLevel)
			: base(fallbackLevel)
		{
		}

		#endregion

		#region Set Default Value

		protected override void SetCustomDefaultValuesCore()
		{
			base.SetCustomDefaultValuesCore();
			IsActive = ZBool.True;
			EventDetail = true;
			QuickView = false;
			IncludeEstimates = false;
			DuplicateEventsHandlingMethod = DuplicateEventHandlingMethodList.Codes.ShowAll;
		}

		#endregion

		#region Properties

		#region EventCode

		[List(nameof(EventCodeList))]
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
				Code = value;
			}
		}
		ZString eventCode;

		public ZPropertyInfo EventCodeInfo
		{
			get { return GetZPropertyInfo(Schema.EventCode); }
		}

		public void ValidateEventType()
		{
			EventCodeInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(EventCodeInfo);
			if (!EventCode.IsEmpty && !EventCodeList.ContainsCode(EventCode))
			{
				EventCodeInfo.AddWarning(ResString.GetMultilingualString("cb515f8f-e3c2-4e89-9d31-69288a0a01cb", "Unknown event code"));
			}
			if (ParentCollection != null && ParentCollection.IsDuplicateEventCode(EventCode))
			{
					EventCodeInfo.AddError(ResString.GetMultilingualString("5875e17b-5519-4ad7-8497-e244ced13d66", "Event code must be unique"));
			}
		}

		EventVisibilityOverrideSettingCollection ParentCollection => (EventVisibilityOverrideSettingCollection)ParentCollections.FirstOrDefault();

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateEventType();
		}

		public CodeDescriptionPairList EventCodeList
		{
			get { return eventCodeList ?? (eventCodeList = EventTypeListProvider.CreateMilestoneEventTypeList(Constants.Workflow.MilestoneType, string.Empty, false, CurrentFactory)); }
		}

		CodeDescriptionPairList eventCodeList;

		#endregion

		#region IsSystem

		public ZBool IsSystem =>
			ParentCollection != null
			&& ParentCollection.EventVisibilityOverride.Code == CargoWise.Definitions.TrackableEvents.ForwardingShipmentWorkflowDescriptorCode
			&& CargoWise.Definitions.TrackableEvents.DefaultCodesForSHP.Contains(eventCode.ToString());

		public ZPropertyInfo IsSystemInfo => GetZPropertyInfo(nameof(IsSystem));

		#endregion

		#region IsActive

		public ZBool IsActive
		{
			get { return isActive; }
			set { SetNonPersistentPropertyValue(IsActiveInfo, ref isActive, value); }
		}
		ZBool isActive;

		public ZPropertyInfo IsActiveInfo => GetZPropertyInfo(nameof(IsActive));

		protected bool IsActive_ReadOnly => IsSystem;

		#endregion

		#region EventDescriptionOverride

		public ZString EventDescriptionOverride
		{
			get { return !eventDescriptionOverride.IsEmpty ? eventDescriptionOverride : GetEventDescriptionFromCode(EventCode); }
			set { SetNonPersistentPropertyValue(EventDescriptionOverrideInfo, ref eventDescriptionOverride, value); }
		}
		ZString eventDescriptionOverride;

		public ZString GetEventDescriptionFromCode(ZString code)
		{
			return (!string.IsNullOrEmpty(code) && Events.All.Contains(code)) ? (ZString)Events.All[code].Description : new ZString();
		}

		public ZPropertyInfo EventDescriptionOverrideInfo => GetZPropertyInfo(nameof(EventDescriptionOverride));

		protected bool EventDescriptionOverride_ReadOnly => !IsActive;

		#endregion

		#region EventDetail

		public ZBool EventDetail
		{
			get { return eventDetail; }
			set { SetNonPersistentPropertyValue(EventDetailInfo, ref eventDetail, value); }
		}
		ZBool eventDetail;

		public ZPropertyInfo EventDetailInfo => GetZPropertyInfo(nameof(EventDetail));

		protected bool EventDetail_ReadOnly => IsSystem;

		#endregion

		#region QuickView

		public ZBool QuickView
		{
			get { return quickView; }
			set { SetNonPersistentPropertyValue(QuickViewInfo, ref quickView, value); }
		}
		ZBool quickView;

		public ZPropertyInfo QuickViewInfo => GetZPropertyInfo(nameof(QuickView));

		#endregion

		#region DuplicateEvents

		[List(nameof(DuplicateEventsHandlingMethodList))]
		[ResourceStringData("EventVisibilityOverrideSetting.DuplicateEventsHandlingMethod", Caption = "Duplicate Events Handling Method", ShortCaption = "Duplicate Events", FullDescription = "How to handle Duplicate Events")]
		public ZString DuplicateEventsHandlingMethod
		{
			get { return duplicateEventsHandlingMethod; }
			set { SetNonPersistentPropertyValue(DuplicateEventsHandlingMethodInfo, ref duplicateEventsHandlingMethod, value); }
		}
		ZString duplicateEventsHandlingMethod;

		public ZPropertyInfo DuplicateEventsHandlingMethodInfo => GetZPropertyInfo(nameof(DuplicateEventsHandlingMethod));

		public DuplicateEventHandlingMethodList DuplicateEventsHandlingMethodList => new DuplicateEventHandlingMethodList();

		#endregion

		#region IncludeEstimates

		public ZBool IncludeEstimates
		{
			get { return includeEstimates; }
			set { SetNonPersistentPropertyValue(IncludeEstimatesInfo, ref includeEstimates, value); }
		}
		ZBool includeEstimates;

		public ZPropertyInfo IncludeEstimatesInfo => GetZPropertyInfo(nameof(IncludeEstimates));

		#endregion

		#endregion

		#region Clone

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new EventVisibilityOverrideSetting();
		}

		public EventVisibilityOverrideSetting CopyValuesToClone(EventVisibilityOverrideSetting setting)
		{
			var clone = new EventVisibilityOverrideSetting();
			using (clone.GetValidationSuspender())
			{
				clone.EventCode = setting.EventCode;
				clone.Description = setting.Description;
				clone.IsActive = setting.IsActive;
				clone.EventDetail = setting.EventDetail;
				clone.QuickView = setting.QuickView;
				clone.EventDescriptionOverride = setting.EventDescriptionOverride;
				clone.DuplicateEventsHandlingMethod = setting.DuplicateEventsHandlingMethod;
				clone.IncludeEstimates = setting.IncludeEstimates;
			}

			return clone;
		}

		#endregion

		#region Xml Serialisation

		protected override void ReadMoreElements(XmlReader reader)
		{
			base.ReadMoreElements(reader);
			while (SkipInvalidElements(reader))
			{
				switch (reader.Name)
				{
					case Schema.EventCode:
					EventCode = new ZString(reader.ReadElementString(Schema.EventCode));
					break;
					case Schema.Description:
					Description = (NoResString)reader.ReadElementString(Schema.Description);
					break;
					case Schema.IsActive:
					IsActive = new ZBool(reader.ReadElementString(Schema.IsActive));
					break;
					case Schema.EventDetail:
					EventDetail = new ZBool(reader.ReadElementString(Schema.EventDetail));
					break;
					case Schema.QuickView:
					QuickView = new ZBool(reader.ReadElementString(Schema.QuickView));
					break;
					case Schema.EventDescriptionOverride:
					EventDescriptionOverride = new ZString(reader.ReadElementString(Schema.EventDescriptionOverride));
					break;
					case Schema.DuplicateEventsHandlingMethod:
					DuplicateEventsHandlingMethod = new ZString(reader.ReadElementString(Schema.DuplicateEventsHandlingMethod));
					break;
					case Schema.IncludeEstimates:
					IncludeEstimates = new ZBool(reader.ReadElementString(Schema.IncludeEstimates));
					break;
					default:
					SkipCurrentElementAsItIsNotInTheSchemaAnymore(reader);
					break;
				}
			}

			void SkipCurrentElementAsItIsNotInTheSchemaAnymore(XmlReader reader)
			{
				_ = reader.ReadElementContentAsObject();
			}

			bool SkipInvalidElements(XmlReader reader)
			{
				while (reader.Name.IsNullOrEmpty())
				{
					reader.Read();
				}

				return reader.NodeType == XmlNodeType.Element;
			}
		}

		protected override void WriteMoreElements(XmlWriter writer)
		{
			base.WriteMoreElements(writer);
			writer.WriteElementString(Schema.EventCode, EventCode.ToString());
			writer.WriteElementString(Schema.Description, Description.ToString());
			writer.WriteElementString(Schema.IsActive, IsActive.ToString());
			writer.WriteElementString(Schema.EventDetail, EventDetail.ToString());
			writer.WriteElementString(Schema.QuickView, QuickView.ToString());
			writer.WriteElementString(Schema.EventDescriptionOverride, EventDescriptionOverride.ToString());
			writer.WriteElementString(Schema.DuplicateEventsHandlingMethod, DuplicateEventsHandlingMethod.ToString());
			writer.WriteElementString(Schema.IncludeEstimates, IncludeEstimates.ToString());
		}

		#endregion
	}
}
