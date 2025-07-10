using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.IL.Business
{
	[XmlSerializerAssembly("Enterprise.Customs.IL.Business.XmlSerializers")]
	public class DCAService : RegistryBusinessObjectTemplate
	{
		public DCAService()
		{
		}

		public DCAService(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		#region Schema

		public static class Schema
		{
			public const string Name = "Name";
			public const string NextRunDateTime = "NextRunDateTime";
		}
		#endregion

		public ZString Name
		{
			get => name;
			set
			{
				CheckMaximumLength(NameInfo, value);
				SetNonPersistentPropertyValue(NameInfo, ref name, value);
			}
		}
		ZString name;

		public ZPropertyInfo NameInfo => GetZPropertyInfo(Schema.Name);

		public ZDateTime NextRunDateTime
		{
			get
			{
				return nextRunDateTime;
			}
			set
			{
				SetNonPersistentPropertyValue(NextRunDateTimeInfo, ref nextRunDateTime, value);
			}
		}
		ZDateTime nextRunDateTime;

		public ZPropertyInfo NextRunDateTimeInfo => GetZPropertyInfo(Schema.NextRunDateTime);

		#region Xml Serialisation

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.Name, Name);
			ZString zString = (NextRunDateTime.IsEmpty ? "" : ((string)NextRunDateTime.SqlFormat));
			writer.WriteElementString(Schema.NextRunDateTime, zString);
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			Name = reader.ReadElementString(Schema.Name);
			ZString dateTime = reader.ReadElementString(Schema.NextRunDateTime);
			nextRunDateTime = (dateTime.IsEmpty ? ZDateTime.Empty : ZDateTime.FromSqlFormat(dateTime));
		}

		#endregion

		#region Override

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory) => new DCAService(fallbackLevel, factory);

		#endregion
	}
}
