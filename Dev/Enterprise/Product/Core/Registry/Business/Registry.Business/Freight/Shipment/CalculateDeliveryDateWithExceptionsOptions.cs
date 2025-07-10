using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class CalculateDeliveryDateWithExceptionsOptions : RegistryBusinessObjectTemplate
	{
#pragma warning disable CW1050 // Use System.TimeSpan Type For A Duration
		public const int MaximumDurationHoursUpperBound = 24;
#pragma warning restore CW1050 // Use System.TimeSpan Type For A Duration

		public static class Schema
		{
			public const string MaximumDurationHours = "MaximumDurationHours";
			public const string UnlimitedDuration = "UnlimitedDuration";
		}

		#region MaximumDurationHours

		public ZInt MaximumDurationHours
		{
			get { return maximumDurationHours; }
			set
			{
				SetNonPersistentPropertyValue<ZInt>(MaximumDurationHoursInfo, ref maximumDurationHours, value);
			}
		}
		ZInt maximumDurationHours;

		public ZPropertyInfo MaximumDurationHoursInfo
		{
			get { return GetZPropertyInfo(Schema.MaximumDurationHours); }
		}

		protected bool MaximumDurationHours_ReadOnly => UnlimitedDuration;

		#endregion

		#region UnlimitedDurationHours

		public ZBool UnlimitedDuration
		{
			get { return unlimitedDuration; }
			set
			{
				if (unlimitedDuration != value)
				{
					SetNonPersistentPropertyValue<ZBool>(UnlimitedDurationHoursInfo, ref unlimitedDuration, value);
					MaximumDurationHours = unlimitedDuration ? 0 : MaximumDurationHoursUpperBound;
				}
			}
		}
		ZBool unlimitedDuration;

		public ZPropertyInfo UnlimitedDurationHoursInfo
		{
			get { return GetZPropertyInfo(Schema.UnlimitedDuration); }
		}

		#endregion

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			var options = new CalculateDeliveryDateWithExceptionsOptions();

			options.MaximumDurationHours = MaximumDurationHours;
			options.UnlimitedDuration = UnlimitedDuration;
			return options;
		}

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(Schema.MaximumDurationHours, MaximumDurationHours.ToString());
			writer.WriteElementString(Schema.UnlimitedDuration, UnlimitedDuration.ToString());
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			MaximumDurationHours = reader.ReadElementStringAsZInt(Schema.MaximumDurationHours);
			UnlimitedDuration = reader.ReadElementStringAsZBool(Schema.UnlimitedDuration);
		}

		public override int GetHashCode()
		{
			return MaximumDurationHours.GetHashCode() ^ UnlimitedDuration.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			return this == (obj as CalculateDeliveryDateWithExceptionsOptions);
		}

		public static bool operator ==(CalculateDeliveryDateWithExceptionsOptions x, CalculateDeliveryDateWithExceptionsOptions y)
		{
			return ((object)x == null && (object)y == null) || ((object)x != null && (object)y != null && x.MaximumDurationHours == y.MaximumDurationHours && x.UnlimitedDuration == y.UnlimitedDuration);
		}

		public static bool operator !=(CalculateDeliveryDateWithExceptionsOptions x, CalculateDeliveryDateWithExceptionsOptions y)
		{
			return !(x == y);
		}
	}
}
