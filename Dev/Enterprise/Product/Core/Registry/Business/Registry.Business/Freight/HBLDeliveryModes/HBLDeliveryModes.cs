using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Xml;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class HBLDeliveryModes : RegistryBusinessObjectTemplate
	{
		#region Schema

		protected abstract class Schema
		{
			public const string DefaultHBLDeliveryMode = "DefaultHBLDeliveryMode";
			public const string ShipmentPackingMode = "ShipmentPackingMode";
		}

		#endregion

		public HBLDeliveryModes()
		{
		}

		public HBLDeliveryModes(ZString shipmentPackingMode)
		{
			ShipmentPackingMode = shipmentPackingMode;
		}

		[SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		public HBLDeliveryModes(ZString shipmentPackingMode, HBLDeliveryModeCollection modes)
		{
			ShipmentPackingMode = shipmentPackingMode;
			Modes = modes;
		}

		#region Modes

		[BusinessObjectTestExclude]
		public HBLDeliveryModeCollection Modes
		{
			get
			{
				return modes ?? (modes = new HBLDeliveryModeCollection(ShipmentPackingMode));
			}
			private set
			{
				modes = value;
				modes.Parent = this;
				RegisterEditableChildObject(modes);
			}
		}
		HBLDeliveryModeCollection modes;

		#endregion

		#region Clone

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			var result = new HBLDeliveryModes(ShipmentPackingMode);
			result.Modes = (HBLDeliveryModeCollection)Modes.Clone(fallbackLevel, factory);
			return result;
		}

		#endregion

		#region XML Serialisation

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.DefaultHBLDeliveryMode, DefaultHBLDeliveryMode);
			writer.WriteElementString(Schema.ShipmentPackingMode, ShipmentPackingMode);
			HBLDeliveryModeCollectionSerialiser.Serialize(writer, Modes);
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			DefaultHBLDeliveryMode = new ZString(reader.ReadElementString(Schema.DefaultHBLDeliveryMode));
			ShipmentPackingMode = new ZString(reader.ReadElementString(Schema.ShipmentPackingMode));
			Modes = (HBLDeliveryModeCollection)HBLDeliveryModeCollectionSerialiser.Deserialize(reader);
		}

		ZXmlSerializer HBLDeliveryModeCollectionSerialiser
		{
			get { return fCollectionSerialiser ?? (fCollectionSerialiser = ZXmlSerializer.New(typeof(HBLDeliveryModeCollection))); }
		}

		ZXmlSerializer fCollectionSerialiser;

		#endregion

		#region Property

		ZString ShipmentPackingMode { get; set; }

		[List("HBLDeliveryModesList")]
		public ZString DefaultHBLDeliveryMode
		{
			get { return defaultHBLDeliveryMode; }
			set
			{
				SetNonPersistentPropertyValue(DefaultHBLDeliveryModeInfo, ref defaultHBLDeliveryMode, value);
				if (!IsValidationSuspended)
				{
					ValidateDefaultHBLDeliveryMode();
				}
			}
		}

		public ZPropertyInfo DefaultHBLDeliveryModeInfo
		{
			get { return GetZPropertyInfo(Schema.DefaultHBLDeliveryMode); }
		}
		ZString defaultHBLDeliveryMode;

		#endregion

		#region Validation

		public void ValidateDefaultHBLDeliveryMode()
		{
			DefaultHBLDeliveryModeInfo.ClearAllNotifications();

			if (!DefaultHBLDeliveryMode.IsEmpty)
			{
				var validHBLDeliveryMode = Modes.Cast<HBLDeliveryMode>().FirstOrDefault(x => x.Code == DefaultHBLDeliveryMode && x.ShowInList);
				if (validHBLDeliveryMode == null)
				{
					DefaultHBLDeliveryModeInfo.AddError(Res.GetString("3dc04893-1495-42c7-858c-1ccdf89f6935", "Default HBL Delivery Mode must be 'Show In List'."));
				}
			}
		}

		#endregion

		#region Lookups

		public CodeDescriptionPairList HBLDeliveryModesList
		{
			get
			{
				var defaultList = new HBLDeliveryModeLists(CurrentFactory).GetDefaultHBLDeliveryModeList(ShipmentPackingMode);
				var result = new CodeDescriptionPairList();

				foreach (HBLDeliveryMode item in defaultList)
				{
					result.AddPair(item.Code, item.Description);
				}

				return result;
			}
		}

		#endregion

		#region Saving

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateDefaultHBLDeliveryMode();
		}

		#endregion
	}
}
