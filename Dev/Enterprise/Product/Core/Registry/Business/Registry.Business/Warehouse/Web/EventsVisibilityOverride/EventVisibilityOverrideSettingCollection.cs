using System.Linq;
using System.Xml.Serialization;
using CargoWise.Common;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class EventVisibilityOverrideSettingCollection : RegistryBusinessObjectCollection
	{
		public EventVisibilityOverrideSettingCollection()
		{
		}

		public EventVisibilityOverrideSettingCollection(EventVisibilityOverride eventVisibilityOverride)
		{
			EventVisibilityOverride = eventVisibilityOverride;
		}

		internal EventVisibilityOverride EventVisibilityOverride;

		public new EventVisibilityOverrideSetting this[int i]
		{
			get { return (EventVisibilityOverrideSetting)base[i]; }
		}

		public void AddDefaultEventVisibilityOverrideSettingsByCode(string workflowCode)
		{
			SuspendValidation();
			RemoveAll();

			var codesToProcess = TrackableEvents.GetOverrideSettingDefaultEventCodes(workflowCode);

			foreach (var eventCode in codesToProcess)
			{
				AddNew(eventCode);
			}

			ResumeValidation();
		}

		public new EventVisibilityOverrideSetting AddNew()
		{
			return (EventVisibilityOverrideSetting)base.AddNew();
		}

		public EventVisibilityOverrideSetting AddNew(ZString code)
		{
			var eventVisibilityOverrideSetting = AddNew();
			using (eventVisibilityOverrideSetting.GetValidationSuspender())
			{
				eventVisibilityOverrideSetting.EventCode = code;
			}
			return eventVisibilityOverrideSetting;
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new EventVisibilityOverrideSetting();
		}

		protected override bool IgnoreCaseInCodes => true;

		public EventVisibilityOverrideSettingCollection Clone()
		{
			var clone = new EventVisibilityOverrideSettingCollection(EventVisibilityOverride);

			foreach (EventVisibilityOverrideSetting setting in this)
			{
				var clonedSetting = new EventVisibilityOverrideSetting();
				clonedSetting = clonedSetting.CopyValuesToClone(setting);
				using (clonedSetting.GetValidationSuspender())
				{
					clone.Add(clonedSetting);
				}
			}

			return clone;
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new EventVisibilityOverrideSettingCollection(EventVisibilityOverride) { CurrentFallbackLevel = fallbackLevel };
		}

		public bool IsDuplicateEventCode(ZString eventCode)
		{
			return this.OfType<EventVisibilityOverrideSetting>().IsCountMoreThan(1, e => e.EventCode == eventCode);
		}
	}
}
