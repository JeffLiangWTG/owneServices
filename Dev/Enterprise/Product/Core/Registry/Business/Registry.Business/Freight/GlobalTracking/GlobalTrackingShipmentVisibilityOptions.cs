using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Xml;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class GlobalTrackingShipmentVisibilityOptions : RegistryBusinessObjectTemplate
	{
		public static class Schema
		{
			public const string IsActive = "IsActive";
			public const string IsDefaultContainerAutomation = "IsDefaultContainerAutomation";
		}

		#region IsActive

		public ZBool IsActive
		{
			get
			{
				return isActive;
			}
			set
			{
				if (isActive != value)
				{
					SetNonPersistentPropertyValue<ZBool>(IsActiveInfo, ref isActive, value);
					if (isActive == false)
					{
						IsDefaultContainerAutomation = false;
					}
				}
			}
		}
		ZBool isActive;

		public ZPropertyInfo IsActiveInfo
		{
			get { return GetZPropertyInfo(Schema.IsActive); }
		}

		#endregion

		public ZBool Active
		{
			get { return IsActive; }
			set
			{
				IsActive = value;
			}
		}

		public ZBool InActive
		{
			get { return !IsActive; }
			set
			{
				IsActive = !value;
			}
		}

		#region IsDefaultContainerAutomation

		public ZBool IsDefaultContainerAutomation
		{
			get
			{
				return isDefaultContainerAutomation;
			}
			set
			{
				if (isDefaultContainerAutomation != value)
				{
					SetNonPersistentPropertyValue<ZBool>(IsDefaultContainerAutomationInfo, ref isDefaultContainerAutomation, value);
				}

				if (!value)
				{
					ServiceEhubIDs.RemoveAll();
					ServiceEhubIDs.AddRange(new GlobalTrackingShipmentVisibilityServiceEhubIDList().GetDefaultGlobalTrackingShipmentVisibilityServiceEhubIDs());
				}
			}
		}
		ZBool isDefaultContainerAutomation;

		public ZPropertyInfo IsDefaultContainerAutomationInfo
		{
			get { return GetZPropertyInfo(Schema.IsDefaultContainerAutomation); }
		}

		protected bool IsDefaultContainerAutomation_ReadOnly => !IsActive;

		#endregion

		#region ServiceEhubIDs

		[BusinessObjectTestExclude]
		public GlobalTrackingShipmentVisibilityServiceEhubIDCollection ServiceEhubIDs
		{
			get
			{
				return serviceEhubIDs ?? (serviceEhubIDs = new GlobalTrackingShipmentVisibilityServiceEhubIDCollection());
			}
			set
			{
				serviceEhubIDs = value;
				serviceEhubIDs.Parent = this;
				RegisterEditableChildObject(serviceEhubIDs);
			}
		}

		GlobalTrackingShipmentVisibilityServiceEhubIDCollection serviceEhubIDs;

		#endregion

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			var options = new GlobalTrackingShipmentVisibilityOptions();

			options.IsActive = IsActive;
			options.IsDefaultContainerAutomation = IsDefaultContainerAutomation;
			options.ServiceEhubIDs = (GlobalTrackingShipmentVisibilityServiceEhubIDCollection)ServiceEhubIDs.Clone(fallbackLevel, factory);

			return options;
		}

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(Schema.IsActive, IsActive.ToString());
			writer.WriteElementString(Schema.IsDefaultContainerAutomation, IsDefaultContainerAutomation.ToString());
			GlobalTrackingShipmentVisibilityServiceEhubIDCollectionSerialiser.Serialize(writer, ServiceEhubIDs);
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			IsActive = reader.ReadElementStringAsZBool(Schema.IsActive);
			IsDefaultContainerAutomation = reader.ReadElementStringAsZBool(Schema.IsDefaultContainerAutomation);
			ServiceEhubIDs = (GlobalTrackingShipmentVisibilityServiceEhubIDCollection)GlobalTrackingShipmentVisibilityServiceEhubIDCollectionSerialiser.Deserialize(reader);
		}

		public override int GetHashCode()
		{
			var hash = 0;
			foreach (GlobalTrackingShipmentVisibilityServiceEhubID item in ServiceEhubIDs)
			{
				hash = hash ^ item.EhubID.GetHashCode();
			}

			return hash ^ IsActive.GetHashCode() ^ IsDefaultContainerAutomation.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			return this == (obj as GlobalTrackingShipmentVisibilityOptions);
		}

		public static bool operator ==(GlobalTrackingShipmentVisibilityOptions x, GlobalTrackingShipmentVisibilityOptions y)
		{
			return ((object)x == null && (object)y == null) || ((object)x != null && (object)y != null && x.IsActive == y.IsActive && x.IsDefaultContainerAutomation == y.IsDefaultContainerAutomation && GlobalTrackingShipmentVisibilityServiceEhubIDsEqual(x.ServiceEhubIDs, y.ServiceEhubIDs, x.IsActive));
		}

		static bool GlobalTrackingShipmentVisibilityServiceEhubIDsEqual(GlobalTrackingShipmentVisibilityServiceEhubIDCollection xServiceEhubIDs, GlobalTrackingShipmentVisibilityServiceEhubIDCollection yServiceEhubIDs, ZBool isActive)
		{
			if ((xServiceEhubIDs == null && yServiceEhubIDs == null) || !isActive)
			{
				return true;
			}

			for (int i = 0; i < xServiceEhubIDs.Count; i++)
			{
				if (xServiceEhubIDs[i].EhubID != yServiceEhubIDs[i].EhubID || xServiceEhubIDs[i].Service != yServiceEhubIDs[i].Service)
				{
					return false;
				}
			}

			return true;
		}

		public static bool operator !=(GlobalTrackingShipmentVisibilityOptions x, GlobalTrackingShipmentVisibilityOptions y)
		{
			return !(x == y);
		}

		ZXmlSerializer GlobalTrackingShipmentVisibilityServiceEhubIDCollectionSerialiser => fCollectionSerialiser ?? (fCollectionSerialiser = ZXmlSerializer.New(typeof(GlobalTrackingShipmentVisibilityServiceEhubIDCollection)));

		ZXmlSerializer fCollectionSerialiser;
	}
}
