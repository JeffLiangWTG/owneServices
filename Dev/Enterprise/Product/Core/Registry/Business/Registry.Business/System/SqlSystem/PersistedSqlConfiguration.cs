using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class PersistedSqlConfiguration : RegistryBusinessObject
	{
		public PersistedSqlConfiguration()
		{
		}

		public ZInt ConfigurationId { get; set; }
		public ZInt ProposedValue { get; set; }

		protected override void SetCustomDefaultValuesCore()
		{
			base.SetCustomDefaultValuesCore();

			ConfigurationId = -1;
			ProposedValue = -1;
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new PersistedSqlConfiguration();
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			base.ReadElements(reader);

			ConfigurationId = ZInt.ParseSafe(reader.ReadElementString(XmlElementSchema.ConfigurationId), ZInt.Zero);
			ProposedValue = ZInt.ParseSafe(reader.ReadElementString(XmlElementSchema.ProposedValue), ZInt.Zero);
		}

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(XmlElementSchema.ConfigurationId, ConfigurationId.ToString());
			writer.WriteElementString(XmlElementSchema.ProposedValue, ProposedValue.ToString());
		}

		#region XmlElementSchema

		abstract class XmlElementSchema
		{
			public const string ConfigurationId = "ConfigurationId";
			public const string ProposedValue = "ProposedValue";
		}

		#endregion

		public override int GetHashCode()
		{
			return ConfigurationId.GetHashCode() ^ ProposedValue.GetHashCode();
		}
	}
}
