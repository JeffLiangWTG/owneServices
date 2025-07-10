using System.IO;
using System.Text;
using System.Xml;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Xml;

namespace Enterprise.Registry.Business
{
	public class PersistedSqlConfigurationsDataType : RegistryDataType<PersistedSqlConfigurationsCollection>
	{
		public PersistedSqlConfigurationsDataType()
			: base(RegistryDataTypes.Codes.Binary, new PersistedSqlConfigurationsCollection())
		{
		}

		protected override bool HasDefaultEditorInfoCore
		{
			get { return false; }
		}

		protected override PersistedSqlConfigurationsCollection CloneValue(PersistedSqlConfigurationsCollection value)
		{
			var cloned = new PersistedSqlConfigurationsCollection();
			foreach (var item in value)
			{
				cloned.Add(item);
			}

			return cloned;
		}

		protected override byte[] SerialiseCore(PersistedSqlConfigurationsCollection value)
		{
			var serializer = ZXmlSerializer.New(DataType);
			using (var stream = new MemoryStream())
			{
				using (var writer = new XmlTextWriter(stream, Encoding.UTF8))
				{
					serializer.Serialize(writer, value);
				}

				return stream.ToArray();
			}
		}

		protected override PersistedSqlConfigurationsCollection DeserialiseCore(byte[] value)
		{
			var result = DefaultValue;
			if (value != null && value.Length > 0)
			{
				var serializer = ZXmlSerializer.New(DataType);

				using (var stream = new MemoryStream(value))
				using (var reader = new XmlTextReader(stream))
				{
					result = (PersistedSqlConfigurationsCollection)serializer.Deserialize(reader);
				}
			}

			return result;
		}
	}
}
