using System.IO;
using System.Text;
using System.Xml;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Xml;

namespace Enterprise.Client.EDI.Registry.Business
{
	public class SourceModulesRegistryItem : StronglyTypedRegistryItem<SourceModuleCollection, SourceModuleCollection>
	{
		public SourceModulesRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, SourceModuleCollection defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new SourceModulesRegistryDataType(), new SourceModulesRegistryEditorInfo(), storage, RegistryOptions.Default, defaultValue))
		{
		}
	}

	public class SourceModulesRegistryDataType : RegistryDataType<SourceModuleCollection>
	{
		public SourceModulesRegistryDataType()
			: base(RegistryDataTypes.Codes.Binary, new SourceModuleCollection())
		{
		}

		protected override SourceModuleCollection CloneValue(SourceModuleCollection value)
		{
			return value.GetCopy();
		}

		protected override bool HasDefaultEditorInfoCore
		{
			get { return false; }
		}

		#region Serialise/Deserialise

		protected override byte[] SerialiseCore(SourceModuleCollection value)
		{
			byte[] result = null;
			ZXmlSerializer serialiser = ZXmlSerializer.New(DataType);

			using (MemoryStream stream = new MemoryStream())
			using (XmlTextWriter writer = new XmlTextWriter(stream, new UnicodeEncoding(false, false)))
			{
				serialiser.Serialize(writer, value);
				writer.Flush();
				result = stream.ToArray();
			}

			return result;
		}

		protected override SourceModuleCollection DeserialiseCore(byte[] value)
		{
			var result = DefaultValue;

			if (value.Length > 0)
			{
				ZXmlSerializer serialiser = ZXmlSerializer.New(DataType);

				using (MemoryStream stream = new MemoryStream(value))
				using (XmlTextReader reader = new XmlTextReader(stream))
				{
					result = (SourceModuleCollection)serialiser.Deserialize(reader);
				}
			}
			else
			{
				result = DefaultValue;
			}

			return result;
		}

		#endregion
	}
}

