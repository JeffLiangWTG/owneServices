using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.GB.Registry
{
	[XmlSerializerAssembly("Enterprise.Customs.GB.Registry.XmlSerializers")]
	public class CcsukNonstandardPimaSetting : RegistryBusinessObjectTemplate
	{
		#region schema and constructors
		public abstract class Schema
		{
			public const string AirportAndShed = "AirportAndShed";
			public const string MessageType = "MessageType";
			public const string TypeBPima = "TypeBPima";
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new CcsukNonstandardPimaSetting(fallbackLevel, factory);
		}

		public CcsukNonstandardPimaSetting()
		{
		}

		public CcsukNonstandardPimaSetting(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public CcsukNonstandardPimaSetting(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		public CcsukNonstandardPimaSetting(string airportAndShed, string messageType, string typeBPima, BusinessObjectFactory factory)
			: this(factory)
		{
			AirportAndShed = airportAndShed;
			MessageType = messageType;
			TypeBPima = typeBPima;
		}
		#endregion

		#region AirportAndShed
		[MaxLength(6)]
		public ZString AirportAndShed
		{
			get { return fAirportAndShed; }
			set
			{
				SetNonPersistentPropertyValue(AirportAndShedInfo, ref fAirportAndShed, value.ToUpper());
				if (!IsValidationSuspended)
				{
					ValidateAirportAndShed();
				}
			}
		}
		ZString fAirportAndShed;

		public ZPropertyInfo AirportAndShedInfo
		{
			get { return GetZPropertyInfo(Schema.AirportAndShed); }
		}

		public void ValidateAirportAndShed()
		{
			AirportAndShedInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(AirportAndShedInfo);
		}

		#endregion

		#region MessageType
		[MaxLength(3)]
		[List(nameof(NonstandardPimaMessageTypesList))]
		public ZString MessageType
		{
			get { return fMessageType; }
			set
			{
				SetNonPersistentPropertyValue(MessageTypeInfo, ref fMessageType, value.ToUpper());
				if (!IsValidationSuspended)
				{
					ValidateMessageType();
				}
			}
		}
		ZString fMessageType;

		public ZPropertyInfo MessageTypeInfo
		{
			get { return GetZPropertyInfo(Schema.MessageType); }
		}

		public void ValidateMessageType()
		{
			MessageTypeInfo.ClearAllNotifications();
			ListValidation.ErrorIfInvalidCode(MessageTypeInfo);
			MandatoryValidation.CheckEntered(MessageTypeInfo);
		}

		#endregion

		#region TypeBPima
		[MaxLength(20)]
		public ZString TypeBPima
		{
			get { return fTypeBPima; }
			set
			{
				SetNonPersistentPropertyValue(TypeBPimaInfo, ref fTypeBPima, value.ToUpper());
				if (!IsValidationSuspended)
				{
					ValidateTypeBPima();
				}
			}
		}
		ZString fTypeBPima;

		public ZPropertyInfo TypeBPimaInfo
		{
			get { return GetZPropertyInfo(Schema.TypeBPima); }
		}

		public void ValidateTypeBPima()
		{
			TypeBPimaInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(TypeBPimaInfo);
		}
		#endregion

		#region XML Reading and Writing
		protected sealed override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.AirportAndShed, AirportAndShed.ToUpperInvariant());
			writer.WriteElementString(Schema.MessageType, MessageType.ToUpperInvariant());
			writer.WriteElementString(Schema.TypeBPima, TypeBPima.ToUpperInvariant());
		}

		protected sealed override void ReadElements(XmlReaderWrapper reader)
		{
			AirportAndShed = reader.ReadElementString(Schema.AirportAndShed);
			MessageType = reader.ReadElementString(Schema.MessageType);
			TypeBPima = reader.ReadElementString(Schema.TypeBPima);
		}
		#endregion

		public NonstandardPimaMessageTypeList NonstandardPimaMessageTypesList
		{
			get { return CurrentFactory.GetCachedValue<NonstandardPimaMessageTypeList>(); }
		}
	}

	public class NonstandardPimaMessageTypeList : CodeDescriptionPairList
	{
		public static class Codes
		{
			public const string FRD = "FRD";
			public const string FRN = "FRN";
			public const string BTH = "BTH";
		}

		public static class Descriptions
		{
			public const string FRD = "FRD - split request";
			public const string FRN = "FRN - renomination request";
			public const string BTH = "Both FRD and FRN message types";
		}

		public NonstandardPimaMessageTypeList()
		{
			AddPair(Codes.FRD, Descriptions.FRD);
			AddPair(Codes.FRN, Descriptions.FRN);
			AddPair(Codes.BTH, Descriptions.BTH);
		}
	}
}
