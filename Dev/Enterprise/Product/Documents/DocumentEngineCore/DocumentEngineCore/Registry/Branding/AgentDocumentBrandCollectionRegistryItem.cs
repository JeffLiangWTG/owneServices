using System.IO;
using System.Linq;
using System.Xml;

using CargoWise.Common;

using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Xml;

namespace Enterprise.DocumentEngineCore.Registry
{
	public class AgentDocumentBrandCollectionRegistryItem : ClientAndAgentBrandingRegistryItem
	{
		public AgentDocumentBrandCollectionRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage)
			: base(name, category, caption, hint, new AgentDocumentBrandRegistryDataType(), storage)
		{
		}

		public AgentDocumentBrandCollectionRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options)
			: base(name, category, caption, hint, new AgentDocumentBrandRegistryDataType(), storage, options)
		{
		}

		#region class AgentDocumentBrandRegistryDataType

		[RegistryEditor("Enterprise.DocumentEngineCore.GUI.Registry.DocumentBrandingRegistryItemEditor, Enterprise.DocumentEngineCore.GUI")]
		internal class AgentDocumentBrandRegistryDataType : ClientAndAgentBrandingRegistryDataType<AgentDocumentBrandCollection>
		{
			public AgentDocumentBrandRegistryDataType() { }

			protected override byte[] SerialiseCore(AgentDocumentBrandCollection value)
			{
				var estimatedSize = value.Cast<AgentDocumentBrand>()
					.Select(item => item.Image?.Size)
					.Sum(size => size?.Width * size?.Height) ?? 0;

				using (GCWrapper.MemoryFailPoint(GCWrapper.BytesToMegabytes(estimatedSize)))
				{
					var serialiser = ZXmlSerializer.New(DataType);

					using (var temp = TempFile.New())
					using (var stream = File.Create(temp.Filename))
					{
						serialiser.Serialize(stream, value);
						stream.Flush();
						stream.Position = 0;

						return stream.ReadFully((int)stream.Length);
					}
				}
			}

			protected override AgentDocumentBrandCollection DeserialiseCore(byte[] value)
			{
				AgentDocumentBrandCollection result = null;
				if (value.Length > 0)
				{
					using (var memoryFailPoint = GCWrapper.MemoryFailPoint(GCWrapper.BytesToMegabytes(value.Length)))
					{
						var serialiser = ZXmlSerializer.New(DataType);
						using (var temp = TempFile.New())
						using (var stream = File.Create(temp.Filename))
						{
							stream.Write(value, 0, value.Length);
							stream.Flush();
							stream.Position = 0;
							value = null;
							using (var reader = new XmlTextReader(stream))
							{
								result = (AgentDocumentBrandCollection)serialiser.Deserialize(reader);
							}
						}
					}
				}
				else
				{
					result = DefaultValue;
				}

				return result;
			}
		}

		#endregion
	}
}
