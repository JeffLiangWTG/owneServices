using System.Diagnostics.CodeAnalysis;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Xml;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class CalculateDeliveryDueDateOptions : RegistryBusinessObjectTemplate
	{
		public static class Schema
		{
			public const string IsActive = "IsActive";
		}

		[SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		public CalculateDeliveryDueDateOptions(CalculateDeliveryDueDateTransportModeCollection transportModes)
		{
			TransportModes = transportModes;
		}

		public CalculateDeliveryDueDateOptions()
		{
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

		#region TransportModes

		[BusinessObjectTestExclude]
		public CalculateDeliveryDueDateTransportModeCollection TransportModes
		{
			get
			{
				return transportModes ?? (transportModes = new CalculateDeliveryDueDateTransportModeCollection());
			}
			set
			{
				transportModes = value;
				RegisterEditableChildObject(transportModes);
			}
		}
		CalculateDeliveryDueDateTransportModeCollection transportModes;

		#endregion

		public ZBool IsTransportModeActive(ZString transportMode)
		{
			if (IsActive && !transportMode.IsEmpty)
			{
				foreach (CalculateDeliveryDueDateTransportMode item in TransportModes)
				{
					if (item.Enabled && item.Code.EqualsIgnoringCase(transportMode))
					{
						return true;
					}
				}
			}
			
			return false;
		}

		public ZBool Inactive
		{
			get { return !IsActive; }
			set
			{
				IsActive = !value;
			}
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			var options = new CalculateDeliveryDueDateOptions();
			options.IsActive = IsActive;
			options.TransportModes = (CalculateDeliveryDueDateTransportModeCollection)TransportModes.Clone(fallbackLevel, factory);

			return options;
		}

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(Schema.IsActive, IsActive.ToString());
			TransportModeCollectionSerialiser.Serialize(writer, TransportModes);
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			IsActive = reader.ReadElementStringAsZBool(Schema.IsActive);
			TransportModes = (CalculateDeliveryDueDateTransportModeCollection)TransportModeCollectionSerialiser.Deserialize(reader);
		}

		public override int GetHashCode()
		{
			return IsActive.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			return this == (obj as CalculateDeliveryDueDateOptions);
		}

		public static bool operator ==(CalculateDeliveryDueDateOptions x, CalculateDeliveryDueDateOptions y)
		{
			return ((object)x == null && (object)y == null) || ((object)x != null && (object)y != null && x.isActive == y.isActive && TransportModesEqual(x.TransportModes , y.TransportModes, x.IsActive));
		}

		static bool TransportModesEqual(CalculateDeliveryDueDateTransportModeCollection xTransportModes, CalculateDeliveryDueDateTransportModeCollection yTransportModes, ZBool isActive)
		{
			if ((xTransportModes == null && yTransportModes == null) || !isActive)
			{
				return true;
			}

			for (int i = 0; i < xTransportModes.Count; i++)
			{
				if (xTransportModes[i].Enabled != yTransportModes[i].Enabled || xTransportModes[i].Code != yTransportModes[i].Code)
				{
					return false;
				}
			}

			return true; 
		}

		public static bool operator !=(CalculateDeliveryDueDateOptions x, CalculateDeliveryDueDateOptions y)
		{
			return !(x == y);
		}

		ZXmlSerializer TransportModeCollectionSerialiser
		{
			get { return fCollectionSerialiser ?? (fCollectionSerialiser = ZXmlSerializer.New(typeof(CalculateDeliveryDueDateTransportModeCollection))); }
		}

		ZXmlSerializer fCollectionSerialiser;
	}
}
