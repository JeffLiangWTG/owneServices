using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public sealed class EBookingCarrierConfiguration : RegistryBusinessObjectTemplate
	{
		protected override void SetCustomDefaultValuesCore()
		{
			base.SetCustomDefaultValuesCore();
			LastUpdatedTime = ZDateTime.Empty;
			LastResponse = ZString.Empty;
		}
		#region Schema

		public static class Schema
		{
			public const string LastResponse = "LastResponse";
			public const string LastUpdatedTime = "LastUpdatedTime";
		}
		#endregion
		public EBookingCarrierConfiguration()
			: base()
		{
		}
		public EBookingCarrierConfiguration(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		public EBookingCarrierConfiguration(FallbackLevel fallbackLevel)
			: base(fallbackLevel)
		{
		}
		#region Clone
		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new EBookingCarrierConfiguration
			{
				LastResponse = LastResponse,
				LastUpdatedTime = LastUpdatedTime
			};
		}
		#endregion

		#region Properties

		#region LastResponse
		public ZString LastResponse
		{
			get => lastResponse;
			set
			{
				if (SetNonPersistentPropertyValue(LastResponseInfo, ref lastResponse, value))
				{
					ValidateLastResponse();
				}
			}
		}
		public ZPropertyInfo LastResponseInfo => GetZPropertyInfo(Schema.LastResponse);
		ZString lastResponse;

		public void ValidateLastResponse()
		{
			LastResponseInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(LastResponseInfo);
			JsonValidation.ValidateJson(LastResponseInfo);
		}
		#endregion

		#region LastUpdatedTime
		public ZDateTime LastUpdatedTime
		{
			get => lastUpdatedTime;
			set
			{
				if (SetNonPersistentPropertyValue(LastUpdatedTimeInfo, ref lastUpdatedTime, value))
				{
					ValidateLastUpdatedTime();
				}
			}
		}
		public ZPropertyInfo LastUpdatedTimeInfo => GetZPropertyInfo(Schema.LastUpdatedTime);
		ZDateTime lastUpdatedTime;

		public void ValidateLastUpdatedTime()
		{
			LastUpdatedTimeInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(LastUpdatedTimeInfo);
		}
		#endregion

		#endregion

		#region Xml Serialization
		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.LastResponse, LastResponse);
			writer.WriteElementString(Schema.LastUpdatedTime, LastUpdatedTime.ToISO8601String());
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			LastResponse = reader.ReadElementString(Schema.LastResponse);
			if (!ZDateTime.TryParseISO8601Date(reader.ReadElementString(Schema.LastUpdatedTime), out lastUpdatedTime))
			{
				lastUpdatedTime = ZDateTime.Empty;
			}
		}
		#endregion
	}
}
