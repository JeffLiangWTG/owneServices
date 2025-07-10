using System.Xml;
using System.Xml.Serialization;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Xml;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	[CodeProperty("Code"), DescriptionProperty("Description")]
	public class EventVisibilityOverride : RegistryBusinessObject, ICategorisedRegistryBusinessObjectCollection
	{
		#region Schema

		new abstract class Schema : RegistryBusinessObject.Schema
		{
			public const string IncludeRelatedEvents = nameof(CargoWise.Definitions.EventVisibilityOverride.IncludeRelatedEvents);
		}

		#endregion

		protected override int MaxDescriptionLength => 256;

		#region Constructors

		public EventVisibilityOverride()
			: base()
		{
		}

		public EventVisibilityOverride(ZString workflowCode)
			: base()
		{
			Code = workflowCode;
		}

		#endregion

		#region Set Default Value

		protected override void SetCustomDefaultValuesCore()
		{
			base.SetCustomDefaultValuesCore();
			IncludeRelatedEvents = ZBool.True;
		}

		#endregion

		#region Properties

		[List(nameof(WorkflowCodeDescriptionPairList))]
		public override ZString Code
		{
			get { return base.Code; }
			set
			{
				if (base.Code != value || base.Code.IsDefault)
				{
					EventVisibilityOverrideSettings.AddDefaultEventVisibilityOverrideSettingsByCode(value);

					base.Code = value;
					Description = WorkflowCodeDescriptionPairList.GetMultilingualDescriptionFromCode(value);
				}
			}
		}

		protected override void ValidateCodeCore()
		{
			base.ValidateCodeCore();
			if (!Code.IsEmpty && !WorkflowCodeDescriptionPairList.ContainsCode(Code))
			{
				CodeInfo.AddWarning(ResString.GetMultilingualString("d2e41fc4-e0cb-4688-bf7f-0d7f480965ed", "Unknown workflow code."));
			}
		}

		public CodeDescriptionPairList WorkflowCodeDescriptionPairList => workflowCodeDescriptionPairList ?? (workflowCodeDescriptionPairList = (CodeDescriptionPairList)ObjectFactory.Get<IWorkflowDescriptorListWithStandaloneTaskType>());
		CodeDescriptionPairList workflowCodeDescriptionPairList;

		public ZBool IncludeRelatedEvents
		{
			get { return includeRelatedEvents; }
			set { SetNonPersistentPropertyValue(IncludeRelatedEventsInfo, ref includeRelatedEvents, value); }
		}
		ZBool includeRelatedEvents;

		public ZPropertyInfo IncludeRelatedEventsInfo => GetZPropertyInfo(nameof(IncludeRelatedEvents));

		#region EventVisibilityOverrideSettings

		[ChildEditable]
		public EventVisibilityOverrideSettingCollection EventVisibilityOverrideSettings
		{
			get
			{
				if (eventVisibilityOverrideSettings == null)
				{
					eventVisibilityOverrideSettings = new EventVisibilityOverrideSettingCollection(this);
					RegisterEditableChildObject(eventVisibilityOverrideSettings);
				}

				return eventVisibilityOverrideSettings;
			}
		}
		EventVisibilityOverrideSettingCollection eventVisibilityOverrideSettings;

		#endregion

		#endregion

		#region Clone

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new EventVisibilityOverride(Code);
		}

		protected override void CopyValuesToClone(RegistryBusinessObjectTemplate clone)
		{
			base.CopyValuesToClone(clone);
			((EventVisibilityOverride)clone).SetEventVisibilityOverrideSettings(EventVisibilityOverrideSettings.Clone());
		}

		public void SetEventVisibilityOverrideSettings(EventVisibilityOverrideSettingCollection settings)
		{
			UnRegisterEditableChildObject(EventVisibilityOverrideSettings);
			eventVisibilityOverrideSettings = settings;
			eventVisibilityOverrideSettings.EventVisibilityOverride = this;
			RegisterEditableChildObject(EventVisibilityOverrideSettings);
		}

		#endregion

		#region Xml Serialisation

		protected override void ReadMoreElements(XmlReader reader)
		{
			base.ReadMoreElements(reader);
			SetEventVisibilityOverrideSettings((EventVisibilityOverrideSettingCollection)CollectionSerialiser.Deserialize(reader));
			IncludeRelatedEvents = new ZBool(reader.ReadElementString(Schema.IncludeRelatedEvents));
		}

		protected override void WriteMoreElements(XmlWriter writer)
		{
			base.WriteMoreElements(writer);
			CollectionSerialiser.Serialize(writer, EventVisibilityOverrideSettings);
			writer.WriteElementString(Schema.IncludeRelatedEvents, IncludeRelatedEvents.ToString());
		}

		ZXmlSerializer CollectionSerialiser
		{
			get { return collectionSerialiser ?? (collectionSerialiser = ZXmlSerializer.New(typeof(EventVisibilityOverrideSettingCollection))); }
		}

		public RegistryBusinessObjectCollection InnerCollection => EventVisibilityOverrideSettings;

		ZXmlSerializer collectionSerialiser;

		#endregion
	}
}
