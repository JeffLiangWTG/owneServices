using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class ContainerPenaltyFreeDaysOptions : RegistryBusinessObjectTemplate
	{
		public static class Schema
		{
			public const string FreeDays = "FreeDays";
			public const string UnlimitedFreeDays = "UnlimitedFreeDays";
		}

		#region Properties

		#region FreeDays

		public ZInt FreeDays
		{
			get { return freeDays; }
			set
			{
				SetNonPersistentPropertyValue<ZInt>(FreeDaysInfo, ref freeDays, value);
			}
		}
		ZInt freeDays;

		public ZPropertyInfo FreeDaysInfo
		{
			get { return GetZPropertyInfo(Schema.FreeDays); }
		}

		protected bool FreeDays_ReadOnly => UnlimitedFreeDays;

		#endregion

		#region UnlimitedFreeDays

		public ZBool UnlimitedFreeDays
		{
			get { return unlimitedFreeDays; }
			set
			{
				if (unlimitedFreeDays != value)
				{
					SetNonPersistentPropertyValue<ZBool>(UnlimitedFreeDaysInfo, ref unlimitedFreeDays, value);
				}
			}
		}
		ZBool unlimitedFreeDays;

		public ZPropertyInfo UnlimitedFreeDaysInfo
		{
			get { return GetZPropertyInfo(Schema.UnlimitedFreeDays); }
		}

		#endregion

		public ZInt? ValidFreeDays
		{
			get
			{
				if (UnlimitedFreeDays)
				{
					return null;
				}

				return FreeDays;
			}
		}

		#endregion

		#region Value
		public ZInt? Value => ValidFreeDays;
		#endregion

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			var options = new ContainerPenaltyFreeDaysOptions();

			options.FreeDays = FreeDays;
			options.UnlimitedFreeDays = UnlimitedFreeDays;
			return options;
		}

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(Schema.FreeDays, FreeDays.ToString());
			writer.WriteElementString(Schema.UnlimitedFreeDays, UnlimitedFreeDays.ToString());
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			FreeDays = reader.ReadElementStringAsZInt(Schema.FreeDays);
			UnlimitedFreeDays = reader.ReadElementStringAsZBool(Schema.UnlimitedFreeDays);
		}

		public override int GetHashCode()
		{
			return FreeDays.GetHashCode() ^ UnlimitedFreeDays.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			return this == (obj as ContainerPenaltyFreeDaysOptions);
		}

		public static bool operator ==(ContainerPenaltyFreeDaysOptions x, ContainerPenaltyFreeDaysOptions y)
		{
			return ((object)x == null && (object)y == null) || ((object)x != null && (object)y != null && x.FreeDays == y.FreeDays && x.UnlimitedFreeDays == y.UnlimitedFreeDays);
		}

		public static bool operator !=(ContainerPenaltyFreeDaysOptions x, ContainerPenaltyFreeDaysOptions y)
		{
			return !(x == y);
		}
	}
}
