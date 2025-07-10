using System.ComponentModel;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class DefaultMinimumStayAndTravelTime : RegistryBusinessObjectTemplate
	{
		#region Schema

		protected abstract class Schema
		{
			public const string TransportMode = "TransportMode";
			public const string StayTime = "StayTime";
			public const string TravelTime = "TravelTime";
		}

		#endregion

		#region TransportMode

		[ReadOnly(true)]
		public ZString TransportMode
		{
			get { return fTransportMode; }
			set
			{
				SetNonPersistentPropertyValue(TransportModeInfo, ref fTransportMode, value);
			}
		}

		ZString fTransportMode;

		public ZPropertyInfo TransportModeInfo
		{
			get { return GetZPropertyInfo(Schema.TransportMode); }
		}

		#endregion

		#region StayTime

		public ZInt StayTime
		{
			get { return fStayTime; }
			set
			{
				SetNonPersistentPropertyValue(StayTimeInfo, ref fStayTime, value);
				if (!IsValidationSuspended)
				{
					ValidateStayTime();
				}
			}
		}

		ZInt fStayTime;

		public ZPropertyInfo StayTimeInfo
		{
			get { return GetZPropertyInfo(Schema.StayTime); }
		}

		#endregion

		#region TravelTime

		public ZInt TravelTime
		{
			get { return fTravelTime; }
			set
			{
				SetNonPersistentPropertyValue(TravelTimeInfo, ref fTravelTime, value);
				if (!IsValidationSuspended)
				{
					ValidateTravelTime();
				}
			}
		}

		ZInt fTravelTime;

		public ZPropertyInfo TravelTimeInfo
		{
			get { return GetZPropertyInfo(Schema.TravelTime); }
		}

		#endregion

		#region Validation

		public void ValidateStayTime()
		{
			StayTimeInfo.ClearAllNotifications();
			MandatoryValidation.CheckNotNegative(StayTimeInfo);
		}

		public void ValidateTravelTime()
		{
			TravelTimeInfo.ClearAllNotifications();
			MandatoryValidation.CheckNotNegative(TravelTimeInfo);
		}

		#endregion

		#region Overrides

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new DefaultMinimumStayAndTravelTime();
		}

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(Schema.TransportMode, TransportMode);
			writer.WriteElementString(Schema.StayTime, StayTime.ToString());
			writer.WriteElementString(Schema.TravelTime, TravelTime.ToString());
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			TransportMode = reader.ReadElementString(Schema.TransportMode);
			StayTime = new ZInt(reader.ReadElementString(Schema.StayTime));
			TravelTime = new ZInt(reader.ReadElementString(Schema.TravelTime));
		}

		#endregion

	}
}
