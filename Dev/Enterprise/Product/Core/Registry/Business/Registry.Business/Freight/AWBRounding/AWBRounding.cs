using System.ComponentModel;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class AWBRounding : ChargeableWeightRounding
	{
		#region Schema

		public class ChargeableWeightRoundingSchema : ShipmentChgWtSchema
		{
			public const string AWBType = "AWBType";
		}

		#endregion

		#region Keys

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Hard-coded constant")]
		public static class Keys
		{
			public const string AgentMaster = "Agent Master";
			public const string DirectMaster = "Direct Master";
			public const string House = "House AWB";
			public const string MasterHouse = "Master House AWB";
		}

		#endregion

		#region Clone

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new AWBRounding();
		}

		#endregion

		#region Properties

		#region AWBType

		[ReadOnly(true)]
		[MaxLength(16)]
		public ZString AWBType
		{
			get { return fAWBType; }
			set
			{
				SetNonPersistentPropertyValue<ZString>(AWBTypeInfo, ref fAWBType, value);
			}
		}

		ZString fAWBType;

		public ZPropertyInfo AWBTypeInfo
		{
			get { return GetZPropertyInfo(ChargeableWeightRoundingSchema.AWBType); }
		}

		#endregion

		#endregion

		#region Xml Serialisation

		protected override void WriteElementsCore(XmlWriter writer)
		{
			writer.WriteElementString(ChargeableWeightRoundingSchema.AWBType, AWBType);
			base.WriteElementsCore(writer);
		}

		protected override void ReadElementsCore(XmlReaderWrapper wrapper)
		{
			AWBType = wrapper.ReadElementString(ChargeableWeightRoundingSchema.AWBType);
			base.ReadElementsCore(wrapper);
		}

		#endregion
	}
}
